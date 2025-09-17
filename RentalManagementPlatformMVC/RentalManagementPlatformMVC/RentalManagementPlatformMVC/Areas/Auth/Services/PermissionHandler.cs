using DocumentFormat.OpenXml.InkML;
using Microsoft.AspNetCore.Authorization;
using static RentalManagementPlatformMVC.Areas.Auth.Services.PermissionRequirement;

namespace RentalManagementPlatformMVC.Areas.Auth.Services
{
	/// <summary>
	/// 驗證使用者 claims 是否符合指定 PermissionRequirement
	/// </summary>
	public sealed class PermissionHandler : AuthorizationHandler<PermissionRequirement>
	{
		private static readonly StringComparer Cmp = StringComparer.OrdinalIgnoreCase;

		// 可選：定義超級角色快速通關（例如 Admin）
		private const string SuperRole = "ADMIN";

		protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
		{
			// 超管角色直接放行
			if (context.User.IsInRole(SuperRole))
			{
				context.Succeed(requirement);
				return Task.CompletedTask;
			}

			// 收集使用者所有 "permission" claims
			var userPerms = context.User
								   .FindAll(c => Cmp.Equals(c.Type, "permission"))
								   .Select(c => c.Value)
								   .ToHashSet(Cmp);

			bool ok = requirement.MatchMode == PermissionMatchMode.All
				? requirement.Codes.All(code => userPerms.Contains(code))
				: requirement.Codes.Any(code => userPerms.Contains(code));

			if (ok)
				context.Succeed(requirement);

			return Task.CompletedTask;
		}
	}
}
