using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentalManagementPlatformWebAPI.DTOs;
using RentalManagementPlatformWebAPI.Services;

namespace RentalManagementPlatformWebAPI.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class PermissionsController : ControllerBase
	{
		private readonly IPermissionService _svc;
		public PermissionsController(IPermissionService svc) { _svc = svc; }

		[HttpGet]
		[Authorize(Policy = "Permissions.View")]
		public async Task<List<PermissionDto>> Get() => await _svc.GetAllAsync();

		[Authorize(Policy = "Permissions.View")]
		[HttpGet("roles/{roleId:int}")]
		public async Task<ActionResult<List<int>>> GetRolePermissionIds([FromRoute] int roleId)
		{
			var ids = await _svc.GetIdsByRoleAsync(roleId);
			return Ok(ids);
		}

		[HttpPost("roles/{roleId:int}")]
		[Authorize(Policy = "Permissions.ManagePermissions")]
		public async Task<IActionResult> Assign([FromRoute] int roleId, [FromBody] AssignPermissionDto dto)
		{
			await _svc.AssignAsync(roleId, dto.PermissionIds);
			return NoContent();
		}

		[HttpDelete("roles/{roleId:int}")]
		[Authorize(Policy = "Permissions.ManagePermissions")]
		public async Task<IActionResult> Remove([FromRoute] int roleId, [FromBody] AssignPermissionDto dto)
		{
			await _svc.RemoveAsync(roleId, dto.PermissionIds);
			return NoContent();
		}
	}
}
