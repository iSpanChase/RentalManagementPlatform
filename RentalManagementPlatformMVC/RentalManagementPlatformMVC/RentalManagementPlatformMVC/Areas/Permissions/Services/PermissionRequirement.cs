using Microsoft.AspNetCore.Authorization;

namespace RentalManagementPlatformMVC.Areas.Permissions.Services
{
	public class PermissionRequirement : IAuthorizationRequirement
	{
		public string Code { get; }
		public PermissionRequirement(string code) => Code = code;
	}
}
