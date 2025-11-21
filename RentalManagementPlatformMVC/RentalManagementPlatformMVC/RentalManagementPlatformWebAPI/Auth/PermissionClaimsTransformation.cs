using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Claims;
using RentalManagementPlatformWebAPI.Repositories;

namespace RentalManagementPlatformWebAPI.Auth
{
    /// <summary>
    /// 在每次成功驗證後，動態補上「角色 Role」與「權限 perm」的 Claims：
    /// - 依照目前登入使用者的 userId，到資料庫查出角色與權限
    /// - 使用 MemoryCache 快取 5 分鐘，減少每個 request 都 hitting DB 的負擔
    /// - 若已經有 Role / perm claim，就不再重複堆疊
    /// </summary>
    public class PermissionClaimsTransformation : IClaimsTransformation
    {
        private readonly IRoleRepository _roles;
        private readonly IPermissionRepository _perms;
        private readonly IMemoryCache _cache;
        private readonly ILogger<PermissionClaimsTransformation> _logger;

        /// <summary>
        /// 透過 DI 注入角色、權限 Repository，快取以及 logger
        /// </summary>
        public PermissionClaimsTransformation(
            IRoleRepository roles,
            IPermissionRepository perms,
            IMemoryCache cache,
            ILogger<PermissionClaimsTransformation> logger)
        {
            _roles = roles; _perms = perms; _cache = cache; _logger = logger;
        }

        /// <summary>
        /// 在 ClaimsPrincipal 建立後、授權前呼叫：
        /// 1. 確認身分已驗證、且為 ClaimsIdentity
        /// 2. 若已經有 Role 與 perm claim，就直接返回，避免重複新增
        /// 3. 從 principal 中解析 userId（Sub / user_id / uid 等多種可能）
        /// 4. 用 userId 當 key 查快取：
        ///    - 沒快取 → 向 DB 查角色碼 / 權限碼，存入快取 5 分鐘
        /// 5. 把角色與權限加入 ClaimsIdentity
        /// 6. 若過程出錯，記 log 但不會讓整個 pipeline 變 500
        /// </summary>
        public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
        {
            // 若不是 ClaimsIdentity 或尚未驗證，則直接略過
            if (principal.Identity is not ClaimsIdentity id || !id.IsAuthenticated) return principal;

            // 避免重複堆疊角色 / 權限 Claims（例如多次呼叫 TransformAsync）
            if (id.HasClaim(c => c.Type == ClaimTypes.Role) && id.HasClaim(c => c.Type == "perm"))
                return principal;

            // ★ 這行改成用 principal 來取，而不是 id
            //   為了盡可能支援多種 token 格式的 userId claim
            var sub =
                principal.FindFirstValue(ClaimTypes.NameIdentifier) ??
                principal.FindFirst("sub")?.Value ??
                principal.FindFirst("user_id")?.Value ??  // 你專案若有用這個
                principal.FindFirst("uid")?.Value;        // 其他可能別名

            if (!int.TryParse(sub, out var userId)) return principal;

            try
            {
                var cacheKey = $"auth:claims:{userId}";
                // 嘗試由快取取得 (roles, perms)
                if (!_cache.TryGetValue(cacheKey, out (List<string> roles, List<string> perms) rp))
                {
                    var roles = await _roles.GetCodesByUserIdAsync(userId);
                    var perms = await _perms.GetCodesByUserIdAsync(userId);
                    rp = (roles ?? new(), perms ?? new());

                    // 缓存 5 分鐘，減少 DB 查詢頻率
                    _cache.Set(cacheKey, rp, TimeSpan.FromMinutes(5));
                }

                // 將角色碼轉成 Role claims
                foreach (var r in rp.roles.Distinct())
                    id.AddClaim(new Claim(ClaimTypes.Role, r));

                // 將權限碼轉成自訂 "perm" claims
                foreach (var p in rp.perms.Distinct())
                    id.AddClaim(new Claim("perm", p));
            }
            catch (Exception ex)
            {
                // 出錯時只記 log，不影響整體登入流程（避免拋 500）
                _logger.LogError(ex, "Failed to enrich claims for user {UserId}", userId);
                return principal; // 不讓它炸成 500
            }

            return principal;
        }
    }
}
