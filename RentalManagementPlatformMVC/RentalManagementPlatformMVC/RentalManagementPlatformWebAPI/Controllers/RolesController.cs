using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentalManagementPlatformWebAPI.DTOs;
using RentalManagementPlatformWebAPI.Services;

namespace RentalManagementPlatformWebAPI.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class RolesController : ControllerBase
	{
		private readonly IRoleService _svc;
		public RolesController(IRoleService svc) { _svc = svc; }

		[HttpGet]
		[Authorize(Policy = "Roles.View")]
		public async Task<List<RoleDto>> Get() => await _svc.GetAllAsync();

		[HttpPost("{roleId:int}/users/{userId:int}")]
		[Authorize(Policy = "Roles.Assign")]
		public async Task<IActionResult> Assign([FromRoute] int roleId, [FromRoute] int userId)
		{
			await _svc.AssignUserAsync(roleId, userId);
			return NoContent();
		}

		[HttpDelete("{roleId:int}/users/{userId:int}")]
		[Authorize(Policy = "Roles.Assign")]
		public async Task<IActionResult> Revoke([FromRoute] int roleId, [FromRoute] int userId)
		{
			await _svc.RevokeUserAsync(roleId, userId);
			return NoContent();
		}
	}
}
