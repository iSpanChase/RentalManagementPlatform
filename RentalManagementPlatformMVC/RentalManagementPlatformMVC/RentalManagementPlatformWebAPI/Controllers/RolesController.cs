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
		private readonly IUserService _users; // ★ 新增

		public RolesController(IRoleService svc, IUserService users) // ★ 新增注入
		{
			_svc = svc;
			_users = users;
		}

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

		// ★ 新增：用 Email 指派角色
		public sealed class AssignUserByEmailDto { public string Email { get; set; } = ""; }

		[HttpPost("{roleId:int}/users/by-email")]
		[Authorize(Policy = "Roles.Assign")]
		public async Task<IActionResult> AssignByEmail([FromRoute] int roleId, [FromBody] AssignUserByEmailDto dto)
		{
			var uid = await _users.GetUserIdByEmailAsync(dto.Email);
			if (uid is null) return NotFound("User not found.");
			await _svc.AssignUserAsync(roleId, uid.Value);
			return NoContent();
		}

		// ★（可選）用 Email 收回角色
		[HttpDelete("{roleId:int}/users/by-email")]
		[Authorize(Policy = "Roles.Assign")]
		public async Task<IActionResult> RevokeByEmail([FromRoute] int roleId, [FromBody] AssignUserByEmailDto dto)
		{
			var uid = await _users.GetUserIdByEmailAsync(dto.Email);
			if (uid is null) return NotFound("User not found.");
			await _svc.RevokeUserAsync(roleId, uid.Value);
			return NoContent();
		}
	}
}
