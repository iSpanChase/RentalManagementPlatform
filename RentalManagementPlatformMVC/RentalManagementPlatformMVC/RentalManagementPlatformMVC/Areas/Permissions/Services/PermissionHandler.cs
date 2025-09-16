using Microsoft.AspNetCore.Authorization;

namespace RentalManagementPlatformMVC.Areas.Permissions.Services
{
	public class PermissionHandler : AuthorizationHandler<PermissionRequirement>
	{
		protected override Task HandleRequirementAsync(AuthorizationHandlerContext ctx, PermissionRequirement req)
		{
			var ok = ctx.User.HasClaim("permission", req.Code);
			if (ok) ctx.Succeed(req);
			return Task.CompletedTask;
		}
	}
}
