using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RentalManagementPlatformWebAPI.DTOs;
using RentalManagementPlatformWebAPI.Models;
using RentalManagementPlatformWebAPI.Repositories;

namespace RentalManagementPlatformWebAPI.Services
{
    /// <summary>
    /// 自訂例外：用來表示「系統管理員（Operator）身分仍在審核中」
    /// 主要在一般登入流程中丟出，提醒前端顯示專門訊息
    /// </summary>
    public sealed class PendingOperatorException : Exception
    {
        public PendingOperatorException(string message) : base(message) { }
    }

    /// <summary>
    /// 負責「認證與授權」相關的服務：
    /// - 一般帳密登入
    /// - Google 第三方登入
    /// - JWT access token / refresh token 發行與刷新
    /// - RefreshToken 作廢（登出全部裝置）
    /// - 查詢使用者角色與權限代碼
    /// </summary>
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _users;
        private readonly IRoleRepository _roles;
        private readonly IPermissionRepository _perms;
        private readonly IRefreshTokenRepository _refreshRepo;
        private readonly IJwtTokenService _jwt;
        private readonly IGoogleTokenVerifier _google;
        private readonly RentalManagementPlatformSqlContext _db;
        private readonly Microsoft.AspNetCore.Identity.IPasswordHasher<User> _hasher;
        private readonly IConfiguration _cfg;

        /// <summary>
        /// 透過 DI 注入各種 Repository、JWT 服務、Google 驗證器、DbContext、密碼雜湊器與設定檔
        /// </summary>
        public AuthService(
            IUserRepository users,
            IRoleRepository roles,
            IPermissionRepository perms,
            IRefreshTokenRepository refreshRepo,
            IJwtTokenService jwt,
            IGoogleTokenVerifier google,
            RentalManagementPlatformSqlContext db,
            Microsoft.AspNetCore.Identity.IPasswordHasher<User> hasher,
            IConfiguration cfg)
        {
            _users = users; _roles = roles; _perms = perms; _refreshRepo = refreshRepo;
            _jwt = jwt; _google = google; _db = db; _hasher = hasher; _cfg = cfg;
        }

        /// <summary>
        /// 一般帳號密碼登入流程：
        /// 1. 依 Email 找到使用者，若無則回傳「帳號或密碼錯誤」
        /// 2. 若 PasswordHash 為空 → 表示為第三方登入帳號，禁止用密碼登入
        /// 3. 使用 PasswordHasher 驗證密碼
        /// 4. 若使用者 IsOperatorPending == true：
        ///    - 若已被指派 OPERATOR 角色 → 自動清除待審核旗標
        ///    - 否則丟出 PendingOperatorException，告知「系統管理員身分尚未審核通過」
        /// 5. 更新 LastLoginAt
        /// 6. 建立 JWT AccessToken + RefreshToken（內含角色與權限）
        /// 7. 將 RefreshToken 存入資料庫（含有效期限）
        /// 8. 回傳 LoginResponseDto 給前端
        /// </summary>
        public async Task<LoginResponseDto> LoginAsync(LoginRequestDto dto)
        {
            var user = await _users.GetByEmailAsync(dto.Email) ?? throw new UnauthorizedAccessException("帳號或密碼錯誤");
            if (string.IsNullOrEmpty(user.PasswordHash))
                throw new UnauthorizedAccessException("此帳號使用第三方登入，請改用 Google 登入");

            var vr = _hasher.VerifyHashedPassword(user, user.PasswordHash, dto.Password);
            if (vr == PasswordVerificationResult.Failed)
                throw new UnauthorizedAccessException("帳號或密碼錯誤");

            // 取得使用者目前的角色碼與權限碼（用於 JWT claims 與前端顯示）
            var roleCodes = await _roles.GetCodesByUserIdAsync(user.UserId) ?? new List<string>();
            var permCodes = await _perms.GetCodesByUserIdAsync(user.UserId) ?? new List<string>();

            // ★ 新增：Operator 待審核機制
            if (user.IsOperatorPending == true)
            {
                if (roleCodes.Any(rc => string.Equals(rc, "OPERATOR", StringComparison.OrdinalIgnoreCase)))
                {
                    // 情境：後台已經核可此帳號為 OPERATOR
                    // → 清除待審核旗標，讓他可以正常登入
                    user.IsOperatorPending = false;
                    await _users.SaveChangesAsync();
                }
                else
                {
                    // 尚未核可 → 丟自訂例外，前端可顯示「系統管理員身分尚未審核通過」
                    throw new PendingOperatorException("此帳號的系統管理員身分尚未審核通過");
                }
            }

            // 更新最後登入時間
            user.LastLoginAt = DateTime.UtcNow;
            await _users.SaveChangesAsync();

            // 建立 JWT token（內含 userId, email, displayName, 角色與權限）
            var pair = _jwt.Create(
                user.UserId,
                user.Email ?? "",
                user.Name ?? user.Username ?? user.Email ?? "",
                roleCodes,
                permCodes
            );

            // 取得 RefreshToken 有效天數，若設定檔沒有就預設 7 天
            var days = int.TryParse(_cfg["Authentication:Jwt:RefreshTokenDays"], out var d) ? d : 7;

            // 將 RefreshToken 寫入資料庫（可支援多裝置同時登入）
            await _refreshRepo.AddAsync(new RefreshToken
            {
                UserId = user.UserId,
                Token = pair.RefreshToken,
                ExpiresAt = DateTime.UtcNow.AddDays(days),
                CreatedAt = DateTime.UtcNow,
                Revoked = false
            });

            return new LoginResponseDto
            {
                AccessToken = pair.AccessToken,
                ExpiresAt = pair.ExpiresAt,
                RefreshToken = pair.RefreshToken,
                Profile = MapProfile(user),
                Roles = roleCodes,
                Permissions = permCodes
            };
        }

        /// <summary>
        /// Google 第三方登入流程：
        /// 1. 用 IGoogleTokenVerifier 驗證 idToken（包含 aud 檢查）
        /// 2. 先用 Provider + ProviderSubject 找既有使用者
        /// 3. 若找不到：
        ///    a. 用 email 找本地帳號，若存在 → 綁定 Provider/Subject
        ///    b. 若仍找不到 → 建立新使用者並預設給 TENANT 角色
        /// 4. 更新 LastLoginAt
        /// 5. 取得角色 / 權限代碼
        /// 6. 發行 JWT AccessToken + RefreshToken 並存入資料庫
        /// 7. 回傳 LoginResponseDto
        /// </summary>
        public async Task<LoginResponseDto> GoogleLoginAsync(string idToken)
        {
            var aud = _cfg["Authentication:Jwt:Audience"]; // 可傳 null 使用自動驗證
            var gp = await _google.VerifyAsync(idToken, aud) ?? throw new UnauthorizedAccessException("Google token 驗證失敗");

            // 先用 Provider + Subject 找是否已有綁定帳號
            var user = await _users.GetByProviderAsync("Google", gp.Sub);
            if (user is null)
            {
                // 若 email 已存在本地帳號，視需求：可阻擋或合併
                user = await _users.GetByEmailAsync(gp.Email);
                if (user is null)
                {
                    // 完全新使用者 → 建立一個本地帳號並綁定 Google
                    user = new User
                    {
                        Email = gp.Email,
                        Name = gp.Name ?? gp.Email,
                        Username = MakeUsernameFromEmail(gp.Email),
                        Provider = "Google",
                        ProviderSubject = gp.Sub,
                        ProfileImageurl = gp.Picture ?? "",
                        LastLoginAt = DateTime.UtcNow,
                        Isverified = true
                    };
                    await _users.AddAsync(user);

                    // 預設給 TENANT 角色（若角色存在）
                    var tenant = await _roles.GetByCodeAsync("TENANT");
                    if (tenant != null) await _roles.AssignUserAsync(tenant.RoleId, user.UserId);
                }
                else
                {
                    // 情境：本地已存在同 email 的帳號
                    // → 將此帳號與 Google 帳號綁定（Provider + Subject）
                    user.Provider = "Google";
                    user.ProviderSubject = gp.Sub;
                    user.LastLoginAt = DateTime.UtcNow;
                    await _users.SaveChangesAsync();
                }
            }
            else
            {
                // 已經是綁定 Google 的帳號 → 更新最後登入時間即可
                user.LastLoginAt = DateTime.UtcNow;
                await _users.SaveChangesAsync();
            }

            // 取得角色與權限代碼
            var (roles, perms) = await GetRoleAndPermCodesAsync(user.UserId);

            // 建立 JWT token（包含角色與權限）
            var pair = _jwt.Create(user.UserId, user.Email, user.Name ?? "", roles, perms);

            var days = int.TryParse(_cfg["Authentication:Jwt:RefreshTokenDays"], out var d) ? d : 7;
            await _refreshRepo.AddAsync(new RefreshToken
            {
                UserId = user.UserId,
                Token = pair.RefreshToken,
                ExpiresAt = DateTime.UtcNow.AddDays(days),
                CreatedAt = DateTime.UtcNow,
                Revoked = false
            });

            return new LoginResponseDto
            {
                AccessToken = pair.AccessToken,
                ExpiresAt = pair.ExpiresAt,
                RefreshToken = pair.RefreshToken,
                Profile = MapProfile(user),
                Roles = roles,
                Permissions = perms
            };
        }

        /// <summary>
        /// 使用 RefreshToken 取得新的 AccessToken（Token 交換/刷新）：
        /// 1. 查詢 refreshToken 是否存在
        /// 2. 檢查是否已被撤銷或過期
        /// 3. 取得對應 User 資料
        /// 4. 重新計算角色 / 權限代碼（避免角色變更後 token 還是舊的）
        /// 5. 建立新的 JWT AccessToken + RefreshToken
        /// 6. 將舊的 RefreshToken 設為 Revoked = true
        /// 7. 新的 RefreshToken 寫入資料庫（token rotation）
        /// </summary>
        public async Task<LoginResponseDto> RefreshAsync(string refreshToken)
        {
            var rt = await _refreshRepo.GetAsync(refreshToken) ?? throw new UnauthorizedAccessException("Refresh token 不存在");
            if (rt.Revoked || rt.ExpiresAt <= DateTime.UtcNow) throw new UnauthorizedAccessException("Refresh token 已失效");

            var user = await _users.GetByIdAsync(rt.UserId) ?? throw new UnauthorizedAccessException("使用者不存在");

            // 重新讀取最新的角色與權限，確保授權資訊是即時的
            var (roles, perms) = await GetRoleAndPermCodesAsync(user.UserId);

            // 建立新的 Access + Refresh pair
            var pair = _jwt.Create(user.UserId, user.Email, user.Name ?? "", roles, perms);

            // 可選安全作法：舊 refresh 標記為 revoked（防止重放攻擊）
            rt.Revoked = true;

            // 新增一筆新的 refresh token（token rotation 慣用模式）
            await _refreshRepo.AddAsync(new RefreshToken
            {
                UserId = user.UserId,
                Token = pair.RefreshToken,
                ExpiresAt = DateTime.UtcNow.AddDays(int.TryParse(_cfg["Authentication:Jwt:RefreshTokenDays"], out var d) ? d : 7),
                CreatedAt = DateTime.UtcNow,
                Revoked = false
            });
            await _refreshRepo.SaveChangesAsync();

            return new LoginResponseDto
            {
                AccessToken = pair.AccessToken,
                ExpiresAt = pair.ExpiresAt,
                RefreshToken = pair.RefreshToken,
                Profile = MapProfile(user),
                Roles = roles,
                Permissions = perms
            };
        }

        /// <summary>
        /// 將指定 userId 的所有「仍有效的」RefreshToken 全部標記為 revoked：
        /// - 通常用於「全部裝置登出」或後台強制使用者登出
        /// </summary>
        public async Task RevokeAllAsync(int userId)
        {
            var list = await _refreshRepo.GetActiveByUserAsync(userId, DateTime.UtcNow);
            foreach (var t in list) t.Revoked = true;
            await _refreshRepo.SaveChangesAsync();
        }

        /// <summary>
        /// 讀取使用者目前的「角色代碼」與「權限代碼」：
        /// - 角色代碼：從 UserRoles → Role.RoleCode
        /// - 權限代碼：從 UserRoles → Role.RolePermissions → Permission.PermCode
        /// - 結果皆去除重複（Distinct）
        /// 此方法在登入、Google 登入與 Refresh 時都會使用
        /// </summary>
        private async Task<(List<string> roles, List<string> perms)> GetRoleAndPermCodesAsync(int userId)
        {
            // 角色代碼
            var roleCodes = await _db.UserRoles
                .Where(ur => ur.UserId == userId)
                .Select(ur => ur.Role.RoleCode)
                .Distinct()
                .ToListAsync();

            // 權限代碼
            var permCodes = await _db.UserRoles
                .Where(ur => ur.UserId == userId)
                .SelectMany(ur => ur.Role.RolePermissions.Select(rp => rp.Permission.PermCode))
                .Distinct()
                .ToListAsync();

            return (roleCodes, permCodes);
        }

        /// <summary>
        /// 將 User 實體轉換成 LoginResponse 中使用的 UserProfileDto：
        /// - 僅帶出登入後前端需要的基本資料
        /// </summary>
        private static UserProfileDto MapProfile(User u) => new()
        {
            UserId = u.UserId,
            Email = u.Email,
            Name = u.Name ?? "",
            Username = u.Username ?? "",
            Phone = u.Phone ?? "",
            ProfileImageUrl = u.ProfileImageurl ?? ""
        };

        /// <summary>
        /// 由 email 產生一個不易重複的 Username：
        /// - 取 email 的 @ 前段
        /// - 再加上一段 Guid 字串（只取前 6 碼）避免重複
        /// - 主要用在 Google 第三方登入建立新帳號時
        /// </summary>
        private static string MakeUsernameFromEmail(string email)
        {
            var name = email.Split('@')[0];
            return $"{name}_{Guid.NewGuid().ToString("N")[..6]}";
        }
    }
}
