using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using RentalManagementPlatformWebAPI.Repositories;

namespace RentalManagementPlatformWebAPI.Controllers
{
	[ApiController]
	[Route("api/admin")]
	public class AdminController : ControllerBase
	{
		private readonly IUserRepository _users;
		private readonly IRoleRepository _roles;
		private readonly IMemoryCache _cache;

		public AdminController(IUserRepository users, IRoleRepository roles, IMemoryCache cache)
		{ _users = users; _roles = roles; _cache = cache; }

		[Authorize(Policy = "Admin.ApproveOperator")]
		[HttpGet("operators/pending")]
		public async Task<ActionResult<IEnumerable<object>>> GetPendingOperators()
		{
			var users = await _users.Query()                 // 你的 UserRepository 若沒有 Query() 就用 DbContext
				.Where(u => u.IsOperatorPending == true)
				.Select(u => new {
					u.UserId,
					u.Email,
					u.Name,
					u.Username,
					u.CreatedAt
				})
				.OrderByDescending(u => u.CreatedAt)
				.ToListAsync();

			return Ok(users);
		}

		// 只有真‧管理員能核准
		[Authorize(Policy = "Admin.ApproveOperator")]
		[HttpPost("users/{userId:int}/approve-operator")]
		public async Task<IActionResult> ApproveOperator([FromRoute] int userId)
		{
			var user = await _users.GetByIdAsync(userId);
			if (user is null) return NotFound();

			if (user.IsOperatorPending != true)
				return BadRequest("此使用者沒有待審核的 Operator 申請。");

			var op = await _roles.GetByCodeAsync("OPERATOR");
			if (op is null) return Problem("找不到角色：OPERATOR");

			await _roles.AssignUserAsync(op.RoleId, userId);
			user.IsOperatorPending = false;
			await _users.SaveChangesAsync();
			_cache.Remove($"auth:claims:{userId}");
			return NoContent();
		}
	}
}
