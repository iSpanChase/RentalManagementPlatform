using Microsoft.IdentityModel.Tokens;
using RentalManagementPlatformWebAPI.DTOs;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace RentalManagementPlatformWebAPI.Services
{
    /// <summary>
    /// JWT 產生實作：
    /// - 從 appsettings 讀取 JWT 設定（Key/Issuer/Audience/AccessTokenMinutes）
    /// - 將使用者資訊、角色、權限寫入 JWT claims 中
    /// - 產生 AccessToken（JWT 字串）與 RefreshToken（隨機字串）
    /// </summary>
    public class JwtTokenService : IJwtTokenService
    {
        private readonly IConfiguration _cfg;
        private readonly IRoleService _roles;
        private readonly IPermissionService _perms;

        /// <summary>
        /// 透過 DI 注入設定與角色/權限服務
        /// （目前 Create 已接收 roles/permissions 參數，_roles/_perms 可在未來擴充時使用）
        /// </summary>
        public JwtTokenService(IConfiguration cfg, IRoleService roles, IPermissionService perms)
        {
            _cfg = cfg;
            _roles = roles;
            _perms = perms;
        }

        /// <summary>
        /// 建立一組 JwtPair（AccessToken + RefreshToken）：
        /// 1. 從設定讀取 Key/Issuer/Audience/AccessTokenMinutes
        /// 2. 檢查 Key 是否存在且長度足夠（HS256 建議至少 32 bytes）
        /// 3. 建立 claims：
        ///    - Sub：userId
        ///    - Email：使用者 email
        ///    - Name：顯示名稱
        ///    - Role：多個角色（重複去除）
        ///    - "perm"：多個權限代碼（重複去除）
        /// 4. 產製 JWT AccessToken
        /// 5. 產生 64 bytes 隨機 RefreshToken（Base64 字串）
        /// </summary>
        public JwtPair Create(int userId, string email, string fullName, IEnumerable<string> roles, IEnumerable<string> permissions)
        {
            roles ??= Enumerable.Empty<string>();         // ★ null-safe，避免呼叫端傳 null
            permissions ??= Enumerable.Empty<string>();   // ★ null-safe

            var keyRaw = _cfg["Authentication:Jwt:Key"];
            if (string.IsNullOrWhiteSpace(keyRaw))
                throw new InvalidOperationException("JWT Key 未設定（Authentication:Jwt:Key）。");
            if (Encoding.UTF8.GetByteCount(keyRaw) < 32)
                throw new InvalidOperationException("JWT Key 長度不足（HS256 建議至少 32 bytes）。");

            var issuer = _cfg["Authentication:Jwt:Issuer"];
            var audience = _cfg["Authentication:Jwt:Audience"];
            if (string.IsNullOrWhiteSpace(issuer) || string.IsNullOrWhiteSpace(audience))
                throw new InvalidOperationException("Authentication:Jwt:Issuer 或 Authentication:Jwt:Audience 未設定。");

            // 建立對稱加密金鑰與簽章憑證
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyRaw));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // === 基本 claims ===
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
                new Claim(ClaimTypes.Email, email ?? string.Empty),
                new Claim(ClaimTypes.Name,  fullName ?? string.Empty),
            };

            // === 角色 claims（Role）===
            foreach (var r in roles.Distinct())
                claims.Add(new Claim(ClaimTypes.Role, r));

            // === 權限 claims（自訂 "perm" 類型）===
            foreach (var p in permissions.Distinct())
                claims.Add(new Claim("perm", p));

            // Token 有效時間（分鐘），預設 30 分
            var minutes = int.TryParse(_cfg["Authentication:Jwt:AccessTokenMinutes"], out var m) ? m : 30;
            var expires = DateTime.UtcNow.AddMinutes(minutes);

            // 建立 JWT Token 物件
            var token = new JwtSecurityToken(
                issuer: _cfg["Authentication:Jwt:Issuer"],
                audience: _cfg["Authentication:Jwt:Audience"],
                claims: claims,
                expires: expires,
                signingCredentials: creds
            );

            // 寫成字串（給前端使用）
            var access = new JwtSecurityTokenHandler().WriteToken(token);

            // RefreshToken：單純使用加密安全的隨機字串，不是 JWT
            var refresh = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

            return new JwtPair(access, expires, refresh);
        }

        /// <summary>
        /// 只針對 UserProfileDto 發出一顆 AccessToken：
        /// - 內部呼叫 Create(...)，但 roles/permissions 傳入空集合
        /// - 適用於「單純需要 access token」而暫時不考慮角色/權限的情境
        /// </summary>
        public Task<string> IssueTokenAsync(UserProfileDto user)
        {
            // 依你的 UserProfileDto 欄位對應：UserId / Email / Name / Username
            // 目前沒有可查「使用者角色/權限」的服務介面方法 → 先帶空集合
            var pair = Create(
                user.UserId,
                user.Email ?? string.Empty,
                string.IsNullOrWhiteSpace(user.Name) ? user.Username : user.Name,
                Enumerable.Empty<string>(),
                Enumerable.Empty<string>()
            );
            return Task.FromResult(pair.AccessToken);
        }
    }
}
