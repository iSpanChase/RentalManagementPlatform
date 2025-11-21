using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace RentalManagementPlatformWebAPI.Auth
{
    /// <summary>
    /// 自訂授權 Policy Provider：
    /// - 讓 [Authorize(Policy = "X")] 自動對應成「需要 perm=X 的權限」
    /// - 好處：不需要在 Startup/Program 逐一手動 AddPolicy("X", ...)
    /// </summary>
    public class PermissionPolicyProvider : DefaultAuthorizationPolicyProvider
    {
        /// <summary>
        /// 透過 DI 將 AuthorizationOptions 傳給基底建構子
        /// </summary>
        public PermissionPolicyProvider(IOptions<AuthorizationOptions> options) : base(options) { }

        /// <summary>
        /// 依照 policyName 動態建立授權規則：
        /// - 任何 [Authorize(Policy = "XXX")]：
        ///   等價於「需要已驗證 + 具有一個 'perm' claim，且值為 'XXX'」
        /// - policyName 直接對應到 perm claim 的值
        ///   例如：Policy = "Admin.ApproveOperator" → 需要 perm="Admin.ApproveOperator"
        /// </summary>
        public override Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
        {
            // 任何 [Authorize(Policy="X")] 都等價於需要 perm=X 的 claim
            var policy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .RequireClaim("perm", policyName)
                .Build();
            return Task.FromResult<AuthorizationPolicy?>(policy);
        }
    }
}
