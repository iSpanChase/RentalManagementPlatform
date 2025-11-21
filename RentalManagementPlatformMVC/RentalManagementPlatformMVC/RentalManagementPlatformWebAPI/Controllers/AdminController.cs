using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using RentalManagementPlatformWebAPI.Repositories;

namespace RentalManagementPlatformWebAPI.Controllers
{
    /// <summary>
    /// 後台系統管理相關 API：
    /// - 目前提供系統管理員審核「待審核的系統管理員(Operator)」申請。
    /// - 受權限控管，需具備 Admin.ApproveOperator 權限才能操作。
    /// </summary>
    [ApiController]
    [Route("api/admin")]
    public class AdminController : ControllerBase
    {
        private readonly IUserRepository _users;
        private readonly IRoleRepository _roles;
        private readonly IMemoryCache _cache;

        /// <summary>
        /// 注入 User/Role Repository 與快取，用來查詢使用者、指派角色以及清除 claims 快取。
        /// </summary>
        public AdminController(IUserRepository users, IRoleRepository roles, IMemoryCache cache)
        { _users = users; _roles = roles; _cache = cache; }

        /// <summary>
        /// 取得所有「申請成為系統管理員(Operator)，但尚未審核通過」的使用者清單。
        /// - 需具備 Admin.ApproveOperator 權限。
        /// - 只回傳必要欄位給前端顯示。
        /// </summary>
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

        /// <summary>
        /// 審核通過某位使用者成為系統管理員(Operator)：
        /// 1. 檢查使用者是否存在。
        /// 2. 檢查該使用者是否真的有「待審核 Operator」申請。
        /// 3. 找出 OPERATOR 角色並指派給此使用者。
        /// 4. 將 IsOperatorPending 設為 false。
        /// 5. 清除該使用者的 claims 快取，讓新角色立即生效。
        /// </summary>
        // 只有真‧管理員能核准
        [Authorize(Policy = "Admin.ApproveOperator")]
        [HttpPost("users/{userId:int}/approve-operator")]
        public async Task<IActionResult> ApproveOperator([FromRoute] int userId)
        {
            var user = await _users.GetByIdAsync(userId);
            if (user is null) return NotFound();

            // 若沒有在「待審核中」，就不能重複核准
            if (user.IsOperatorPending != true)
                return BadRequest("此使用者沒有待審核的 Operator 申請。");

            var op = await _roles.GetByCodeAsync("OPERATOR");
            if (op is null) return Problem("找不到角色：OPERATOR");

            // 指派 OPERATOR 角色給此使用者
            await _roles.AssignUserAsync(op.RoleId, userId);

            // 將「待審核」旗標關閉
            user.IsOperatorPending = false;
            await _users.SaveChangesAsync();

            // 清掉此使用者的 claims 快取，讓角色權限立刻更新
            _cache.Remove($"auth:claims:{userId}");
            return NoContent();
        }
    }
}
