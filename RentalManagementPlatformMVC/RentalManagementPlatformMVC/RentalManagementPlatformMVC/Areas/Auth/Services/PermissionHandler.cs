using DocumentFormat.OpenXml.InkML;
using Microsoft.AspNetCore.Authorization;

namespace RentalManagementPlatformMVC.Areas.Auth.Services
{
	public sealed class PermissionHandler : AuthorizationHandler<PermissionRequirement>
	{
		protected override Task HandleRequirementAsync(
			AuthorizationHandlerContext context, PermissionRequirement requirement)
		{
			// 快速通關（可選）：若你有超管角色（例：ADMIN）
			if (context.User.IsInRole("ADMIN"))
			{
				context.Succeed(requirement);
				return Task.CompletedTask;
			}

			// 一般權限：檢查 "permission" claim 是否包含指定的 perm_code
			if (context.User.HasClaim("permission", requirement.Code))
				context.Succeed(requirement);

			return Task.CompletedTask;
		}
	}
}
