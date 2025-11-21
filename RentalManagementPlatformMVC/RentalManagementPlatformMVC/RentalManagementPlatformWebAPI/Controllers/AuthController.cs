using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using RentalManagementPlatformWebAPI.DTOs;
using RentalManagementPlatformWebAPI.Models;
using RentalManagementPlatformWebAPI.Repositories;
using RentalManagementPlatformWebAPI.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace RentalManagementPlatformWebAPI.Controllers
{
    /// <summary>
    /// 認證/授權相關 API 控制器：
    /// - 帳號密碼登入 / Google 登入 / Token Refresh / 登出
    /// - 註冊 / 忘記密碼 / 重設密碼
    /// - Email 驗證（寄發驗證信 + 驗證連結）
    /// - 除錯相關的 JWT / SMTP 設定檢查 API
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _auth;
        private readonly IUserService _userService;
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly IConfiguration _cfg;
        private readonly ILogger<AuthController> _logger;
        private readonly string _frontendBaseUrl;
        private readonly IEmailSender _emailSender;
        private readonly IHostEnvironment _env;
        private readonly IRoleRepository _roles;
        private readonly IPermissionRepository _perms;
        private readonly IEmailVerificationService _emailVerify;

        /// <summary>
        /// 透過 DI 注入登入服務/使用者服務、Repository、密碼雜湊器、寄信服務
        /// 以及設定檔、Logger、環境資訊等。
        /// </summary>
        public AuthController(
            IAuthService auth,
            IUserService userService,
            IUserRepository userRepository,
            IPasswordHasher<User> passwordHasher,
            IConfiguration cfg,
            ILogger<AuthController> logger,
            IEmailSender emailSender,
            IHostEnvironment env,
            IRoleRepository roles,
            IPermissionRepository perms,
            IEmailVerificationService emailVerify)
        {
            _auth = auth;
            _userService = userService;
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _cfg = cfg;
            _logger = logger;
            _emailSender = emailSender;
            _env = env;
            _roles = roles;
            _perms = perms;
            _emailVerify = emailVerify;
            // 前端的根網址（用於重設密碼連結），appsettings 裡可設 Frontend:BaseUrl
            _frontendBaseUrl = _cfg["Frontend:BaseUrl"] ?? "http://localhost:5173";
        }

        /// <summary>
        /// 取得目前登入使用者的角色與權限清單：
        /// - 透過 Claims 中的 NameIdentifier/sub 找到 userId。
        /// - 再到資料庫讀取角色碼與權限碼。
        /// </summary>
        [HttpGet("me/abilities")]
        [Authorize]
        public async Task<IActionResult> GetMyAbilities()
        {
            var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub")?.Value;
            if (!int.TryParse(sub, out var userId)) return Unauthorized();

            var roles = await _roles.GetCodesByUserIdAsync(userId);
            var perms = await _perms.GetCodesByUserIdAsync(userId);
            return Ok(new { roles, perms });
        }

        /// <summary>
        /// 帳號密碼登入：
        /// - 呼叫 IAuthService.LoginAsync，內部會處理驗證與產生 JWT + RefreshToken。
        /// - 針對不同例外型別回傳不同 HTTP Status（403/401/500）。
        /// </summary>
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginRequestDto dto)
        {
            try
            {
                var res = await _auth.LoginAsync(dto);
                return Ok(res);
            }
            catch (PendingOperatorException ex)
            {
                // ★ Operator 待審核：403
                return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                // ★ 帳號或密碼錯誤 / 第三方登入限制：401
                return Unauthorized(new { message = ex.Message });
            }
            catch (InvalidOperationException ex) // 多半來自 JwtTokenService 設定檢查
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { type = ex.GetType().Name, message = ex.Message });
            }
            catch (SqlException ex) // RefreshToken 寫入 DB 失敗/連線問題
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { type = ex.GetType().Name, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login failed for {Email}", dto?.Email);
                return Problem("登入發生錯誤"); // 500 with generic message
            }
        }

        /// <summary>
        /// Google 第三方登入：
        /// - 前端傳入 Google idToken。
        /// - 呼叫 IAuthService.GoogleLoginAsync 完成驗證/建立或綁定使用者並簽發 JWT。
        /// </summary>
        [HttpPost("google")]
        [AllowAnonymous]
        public async Task<ActionResult<LoginResponseDto>> Google([FromBody] GoogleLoginDto dto)
            => Ok(await _auth.GoogleLoginAsync(dto.IdToken));

        /// <summary>
        /// 使用 RefreshToken 換新的 AccessToken / RefreshToken：
        /// - 由 IAuthService.RefreshAsync 處理 Token 驗證與 rotation。
        /// </summary>
        [HttpPost("refresh")]
        [AllowAnonymous]
        public async Task<ActionResult<LoginResponseDto>> Refresh([FromBody] TokenRefreshDto dto)
            => Ok(await _auth.RefreshAsync(dto.RefreshToken));

        /// <summary>
        /// 登出：撤銷目前使用者所有有效 RefreshToken（等於登出所有裝置）。
        /// </summary>
        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            var sub = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                   ?? User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value;

            if (!int.TryParse(sub, out var userId)) return NoContent();

            await _auth.RevokeAllAsync(userId);
            return NoContent();
        }

        /// <summary>
        /// 註冊 API：
        /// 1. 先檢查生日不可大於今天（後端再擋一次）。
        /// 2. 呼叫 IUserService.RegisterAsync 建立使用者與角色關係。
        /// 3. 註冊成功後，若 Email 尚未驗證，觸發寄送驗證信。
        /// 4. 若寄信失敗，記錄 log 但不影響註冊成功回應。
        /// </summary>
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<ActionResult<UserProfileDto>> Register([FromBody] RegistrationRequestDto dto)
        {
            // ✅ 後端擋：生日不可晚於今天（用 UTC Date 比較，避免時區誤差）
            if (dto.BirthDate.Date > DateTime.UtcNow.Date)
                return BadRequest(new { message = "生日不可晚於今天" });

            try
            {
                var result = await _userService.RegisterAsync(dto);

                // 註冊成功後嘗試寄驗證信（失敗不影響回傳）
                try
                {
                    var user = await _userRepository.GetByEmailAsync(dto.Email);
                    if (user != null && !user.Isverified)
                        await _emailVerify.CreateAndSendAsync(user.UserId, user.Email!, HttpContext.RequestAborted, BuildApiBase(Request));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Send verification mail failed after register. Email={Email}", dto.Email);
                }
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                // 例如 Email 已被使用、密碼不符合規則等
                return BadRequest(new { message = ex.Message });
            }
        }

        // ===== 忘記密碼：產生短效 JWT，寄送（這裡先以 log 代替寄信） =====

        /// <summary>
        /// 忘記密碼流程起點：
        /// 1. 前端送出 Email。
        /// 2. 若帳號存在，產生一顆短效「重設密碼專用 JWT」。
        /// 3. 組合前端 reset-password 頁面的連結，附上 token & email 當 QueryString。
        /// 4. 寄送 Email，內含重設密碼連結。
        /// ※ 若 Email 不存在，仍回 200，避免洩漏帳號是否存在。
        /// </summary>
        [HttpPost("forgot-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto req)
        {
            if (string.IsNullOrWhiteSpace(req.Email)) return Ok();

            var user = await _userRepository.GetByEmailAsync(req.Email);
            if (user == null) return Ok(); // 不洩漏帳號存在與否

            var token = CreatePasswordResetToken(user);
            var resetUrl =
                $"{_frontendBaseUrl.TrimEnd('/')}/reset-password?token={Uri.EscapeDataString(token)}&email={Uri.EscapeDataString(user.Email!)}";

            var subject = "【AirNest】重設您的密碼";
            var html = $@"
      <p>您好，{System.Net.WebUtility.HtmlEncode(user.Name ?? user.Username ?? user.Email)}：</p>
      <p>請點擊以下連結以重設您的密碼（15 分鐘內有效）：</p>
      <p><a href=""{resetUrl}"" target=""_blank"">{resetUrl}</a></p>
      <p>若您未曾提出申請，請忽略此信。</p>
      <hr/><p>AirNest 支援團隊</p>";

            try
            {
                await _emailSender.SendAsync(user.Email!, subject, html);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Send reset email failed. To={Email}", user.Email);

                // 開發環境：回傳實際錯誤訊息，方便你立刻定位
                if (_env.IsDevelopment())
                    return StatusCode(500, new { message = $"寄信失敗：{ex.GetType().Name}: {ex.Message}" });

                // 正式環境：只回通用訊息
                return StatusCode(500, new { message = "寄信失敗，請稍後再試或聯絡管理員。" });
            }
        }

        // ===== Email 驗證：重寄驗證信 =====

        /// <summary>
        /// 重送驗證信用的簡易 DTO。
        /// </summary>
        public record ResendVerificationDto(string Email);

        /// <summary>
        /// 建立後端 API 的 Email 驗證 URL base（給 EmailVerificationService 組連結用）。
        /// </summary>
        string BuildApiBase(HttpRequest req) => $"{req.Scheme}://{req.Host}/api/Auth/verify-email";

        /// <summary>
        /// 重寄 Email 驗證信：
        /// 1. 依 Email 找到使用者。
        /// 2. 若不存在或已驗證，直接回 200（避免洩漏資訊）。
        /// 3. 呼叫 EmailVerificationService 建立驗證紀錄並寄信。
        /// </summary>
        [HttpPost("resend-verification")]
        [AllowAnonymous]
        public async Task<IActionResult> ResendVerification([FromBody] ResendVerificationDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto?.Email)) return Ok();
            var user = await _userRepository.GetByEmailAsync(dto.Email.Trim());
            if (user == null || user.Isverified) return Ok();

            try
            {
                await _emailVerify.CreateAndSendAsync(
                    user.UserId, user.Email!, HttpContext.RequestAborted, BuildApiBase(Request));
                return Ok(new { sent = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Resend verification failed. Email={Email}", dto.Email);
                if (_env.IsDevelopment())
                    return StatusCode(500, new { message = ex.Message, type = ex.GetType().Name });
                return StatusCode(500, new { message = "寄信失敗，請稍後再試或聯絡管理員。" });
            }
        }

        // ===== Email 驗證：點擊信中連結完成驗證 =====

        /// <summary>
        /// 信中驗證連結的目標 API：
        /// 1. 驗證 email + token 是否有效（EmailVerificationService）。
        /// 2. 若成功，將使用者設為已驗證。
        /// 3. 最後 Redirect 回前端的成功頁面（SuccessUrl）。
        /// </summary>
        [HttpGet("verify-email")]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyEmail([FromQuery] string email, [FromQuery] string token)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(token))
                return BadRequest(new { message = "參數不完整" });

            var ok = await _emailVerify.VerifyAsync(email.Trim(), token.Trim(), HttpContext.RequestAborted);
            if (!ok) return BadRequest(new { message = "連結無效或已過期" });

            // 你也可以改為 Redirect 到前端成功頁：
            var successUrl = (_cfg["EmailVerification:SuccessUrl"] ?? $"{_frontendBaseUrl}/verify-email/success").TrimEnd('/');
            return Redirect(successUrl);
            //return Ok(new { message = "Email 驗證成功" });
        }

        // ===== 重設密碼：驗證 token，通過後更新密碼 =====

        /// <summary>
        /// 將前端帶來的 token 字串「正規化」成純 JWT：
        /// - 支援以下格式：
        ///   1) 純 JWT
        ///   2) 整條網址（含 token=xxx&...），會從 QueryString 解析出 token
        ///   3) 字串中包含 "token=xxx&..." 的情況
        /// - 並去除多餘引號與 & 後面的參數。
        /// </summary>
        private static string NormalizeToken(string? input)
        {
            var raw = input?.Trim() ?? string.Empty;
            if (string.IsNullOrEmpty(raw)) return raw;

            // 若是整條 URL，解析出 QueryString 裡的 token
            if (raw.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    var uri = new Uri(raw);
                    var q = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(uri.Query);
                    if (q.TryGetValue("token", out var v)) raw = v.ToString();
                }
                catch { }
            }

            // 處理字串中包含 "token=xxx" 的情況
            var eq = raw.IndexOf("token=", StringComparison.OrdinalIgnoreCase);
            if (eq >= 0) raw = raw[(eq + "token=".Length)..];

            // 若後面還有 & 其他參數，只取 token 這段
            var amp = raw.IndexOf('&');
            if (amp > 0) raw = raw[..amp];

            // 去掉多餘的引號
            return raw.Trim().Trim('"', '\'');
        }

        /// <summary>
        /// 驗證重設密碼用 JWT 的工具方法：
        /// - 會驗證 Issuer/Audience/Signature/Lifetime。
        /// - 檢查 "type" claim 是否為 "password_reset"。
        /// - 回傳：
        ///   - ok：是否驗證成功
        ///   - subRaw：原始 sub 字串（可能是 userId / email / username）
        ///   - userId：若 sub 是 int，就解析成 userId
        ///   - email：token 中的 email claim
        ///   - reason：失敗原因（給前端顯示用）
        /// </summary>
        // ✅ 更強韌：sub 不是數字時，不立刻回錯；會把 sub / email 都帶回給呼叫端
        private (bool ok, string? subRaw, int? userId, string? email, string? reason)
            TryParsePasswordResetToken(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_cfg["Authentication:Jwt:Key"]!);

            var parameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = _cfg["Authentication:Jwt:Issuer"],
                ValidateAudience = true,
                ValidAudience = _cfg["Authentication:Jwt:Audience"],
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(2)
            };

            try
            {
                var principal = handler.ValidateToken(token, parameters, out var securityToken);

                if (securityToken is not JwtSecurityToken jwt ||
                    !jwt.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                    return (false, null, null, null, "Token 演算法不正確（僅接受 HS256）");

                var type = principal.FindFirst("type")?.Value;
                if (!string.Equals(type, "password_reset", StringComparison.Ordinal))
                    return (false, null, null, null, "Token 類型錯誤");

                var sub = principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;   // 可能是 userId / email / username / Guid
                var mail = principal.FindFirst(JwtRegisteredClaimNames.Email)?.Value; // 可能為空

                int? uid = null;
                if (int.TryParse(sub, out var parsed)) uid = parsed;

                return (true, sub, uid, mail, null);
            }
            catch (SecurityTokenInvalidSignatureException) { return (false, null, null, null, "簽章驗證失敗（Key 不一致）"); }
            catch (SecurityTokenInvalidIssuerException) { return (false, null, null, null, "Issuer 不正確"); }
            catch (SecurityTokenInvalidAudienceException) { return (false, null, null, null, "Audience 不正確"); }
            catch (SecurityTokenExpiredException) { return (false, null, null, null, "重設連結已失效，請重新操作「忘記密碼」"); }
            catch { return (false, null, null, null, "無法驗證重設連結"); }
        }

        /// <summary>
        /// 重設密碼 API：
        /// 1. 檢查參數完整性與密碼強度。
        /// 2. NormalizeToken → 解析出純 JWT。
        /// 3. TryParsePasswordResetToken 驗證 token 與解析 sub/email。
        /// 4. 依序嘗試以下方式找到 User：
        ///    (1) userId (sub 為數字) →
        ///    (2) token 的 email →
        ///    (3) sub 當 email / username →
        ///    (4) request 裡的 Email。
        /// 5. 若找到使用者，更新 PasswordHash + UpdatedAt。
        /// </summary>
        [HttpPost("reset-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto req)
        {
            if (string.IsNullOrWhiteSpace(req.Token) || string.IsNullOrWhiteSpace(req.NewPassword))
                return BadRequest(new { message = "參數不完整" });

            // ✅ 密碼強度檢查（重設密碼也必須符合）
            if (!System.Text.RegularExpressions.Regex.IsMatch(req.NewPassword, "^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d).{8,}$"))
                return BadRequest(new { message = "密碼需至少 8 碼，且包含英文大小寫與數字" });

            var raw = NormalizeToken(req.Token);

            var (ok, subRaw, uid, emailFromToken, reason) = TryParsePasswordResetToken(raw);
            if (!ok) return BadRequest(new { message = reason ?? "無法驗證重設連結" });

            User? user = null;

            // 1) 先用 userId
            if (uid.HasValue && uid.Value > 0)
                user = await _userRepository.GetByIdAsync(uid.Value);

            // 2) 再用 token 的 email
            if (user == null && !string.IsNullOrWhiteSpace(emailFromToken))
                user = await _userRepository.GetByEmailAsync(emailFromToken);

            // 3) 再用 sub 當 email/username 嘗試
            if (user == null && !string.IsNullOrWhiteSpace(subRaw))
            {
                if (subRaw.Contains("@"))
                    user = await _userRepository.GetByEmailAsync(subRaw);
                if (user == null)
                    user = await _userRepository.GetByUsernameAsync(subRaw);
            }

            // ✅ 4) 最後保底：用請求裡的 email（你這次的 body 已經有傳）
            if (user == null && !string.IsNullOrWhiteSpace(req.Email))
                user = await _userRepository.GetByEmailAsync(req.Email.Trim());

            if (user == null)
                return BadRequest(new { message = $"找不到使用者（ID={uid?.ToString() ?? "N/A"}, Email={(emailFromToken ?? req.Email) ?? "N/A"}, Sub={subRaw ?? "N/A"}）" });

            // 更新新密碼
            user.PasswordHash = _passwordHasher.HashPassword(user, req.NewPassword);
            user.UpdatedAt = DateTime.UtcNow;
            await _userRepository.SaveChangesAsync();
            return Ok();
        }


        // ===== 私有小工具 =====

        /// <summary>
        /// 建立「重設密碼專用」JWT：
        /// - claims:
        ///   - sub  : 一律為 userId（字串）
        ///   - email: 使用者 Email
        ///   - type : "password_reset"
        /// - 有效時間由 PasswordReset:TokenMinutes 設定（預設 15 分鐘）。
        /// </summary>
        private string CreatePasswordResetToken(User user)
        {
            var issuer = _cfg["Authentication:Jwt:Issuer"];
            var audience = _cfg["Authentication:Jwt:Audience"];
            var signingKey = _cfg["Authentication:Jwt:Key"];

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[] {
        new Claim(JwtRegisteredClaimNames.Sub,  user.UserId.ToString()),            // ✅ 一律放 userId
        new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),       // ✅ 再放 email 當後援
        new Claim("type", "password_reset")
    };

            var minutes = int.TryParse(_cfg["PasswordReset:TokenMinutes"], out var m) ? m : 15;

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(minutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }


        /// <summary>
        /// SMTP 設定除錯用 API：
        /// - 回傳目前載入的 SMTP 設定（不會回傳實際密碼，只告知是否有設定密碼）。
        /// - 方便你在前端或 Postman 檢查看目前後端使用的設定是否正確。
        /// </summary>
        [HttpGet("debug/smtp")]
        [AllowAnonymous]
        public IActionResult DebugSmtp([FromServices] Microsoft.Extensions.Options.IOptions<SmtpOptions> opt)
        {
            var o = opt.Value;
            return Ok(new
            {
                o.Host,
                o.Port,
                o.UseSsl,
                o.FromEmail,
                o.FromName,
                User = o.User,
                HasPassword = !string.IsNullOrEmpty(o.Password)
            });
        }

        /// <summary>
        /// JWT 設定除錯用 API：
        /// - 回傳目前載入的 Issuer/Audience 以及 Key 的長度與頭尾幾碼。
        /// - 可用來檢查實際執行環境有沒有吃到正確的設定。
        /// </summary>
        // A. 回傳目前後端在用的 JWT 驗證設定（不回 Key 的全文，僅回長度與前/後 4 碼）
        [HttpGet("debug/jwt-config")]
        [AllowAnonymous]
        public IActionResult DebugJwtConfig()
        {
            string key = _cfg["Authentication:Jwt:Key"] ?? "";
            return Ok(new
            {
                cfgIssuer = _cfg["Authentication:Jwt:Issuer"],
                cfgAudience = _cfg["Authentication:Jwt:Audience"],
                keyLen = key.Length,
                keyHead = key.Length >= 4 ? key[..4] : key,
                keyTail = key.Length >= 4 ? key[^4..] : key
            });
        }

        /// <summary>
        /// Debug 用 DTO：只帶一個 Token。
        /// </summary>
        public record TokenOnlyDto(string Token);

        /// <summary>
        /// 重設密碼 token 除錯 API：
        /// - 不做驗證，只是 ReadJwtToken 把 payload 抽出來給你比對 Issuer/Audience/type/sub/exp。
        /// - 方便你用 Postman 或前端檢查「產出的 token 與設定檔是否一致」。
        /// </summary>
        [HttpPost("debug/validate-reset")]
        [AllowAnonymous]
        public IActionResult DebugValidateReset([FromBody] TokenOnlyDto dto)
        {
            var token = dto?.Token ?? "";
            if (string.IsNullOrWhiteSpace(token))
                return Ok(new { ok = false, reason = "no token" });

            var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);

            var payloadIssuer = jwt.Issuer;
            var payloadAudience = jwt.Audiences?.FirstOrDefault();
            var cfgIssuer = _cfg["Authentication:Jwt:Issuer"];
            var cfgAudience = _cfg["Authentication:Jwt:Audience"];

            return Ok(new
            {
                ok = string.Equals(payloadIssuer, cfgIssuer, StringComparison.Ordinal)
                   && string.Equals(payloadAudience, cfgAudience, StringComparison.Ordinal),
                payloadIssuer,
                cfgIssuer,
                issuerMatch = string.Equals(payloadIssuer, cfgIssuer, StringComparison.Ordinal),
                payloadAudience,
                cfgAudience,
                audienceMatch = string.Equals(payloadAudience, cfgAudience, StringComparison.Ordinal),
                type = jwt.Claims.FirstOrDefault(c => c.Type == "type")?.Value,
                sub = jwt.Claims.FirstOrDefault(c => c.Type == System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value,
                exp = jwt.Claims.FirstOrDefault(c => c.Type == System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Exp)?.Value
            });
        }
    }
}
