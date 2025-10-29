using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
namespace RentalManagementPlatformWebAPI.Auth
{
	public class PermissionPolicyProvider : DefaultAuthorizationPolicyProvider
	{
		public PermissionPolicyProvider(IOptions<AuthorizationOptions> options) : base(options) { }

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
