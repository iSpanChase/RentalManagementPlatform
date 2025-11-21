using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentalManagementPlatformWebAPI.DTOs;
using RentalManagementPlatformWebAPI.Services;

namespace RentalManagementPlatformWebAPI.Controllers
{
    /// <summary>
    /// 角色（Role）管理相關 API：
    /// - 查詢所有角色清單
    /// - 對使用者指派 / 收回角色（用 userId 或 email）
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class RolesController : ControllerBase
    {
        private readonly IRoleService _svc;
        private readonly IUserService _users; // ★ 新增

        /// <summary>
        /// 注入角色服務與使用者服務：
        /// - IRoleService：處理角色指派/收回
        /// - IUserService：提供依 Email 查 userId 等功能
        /// </summary>
        public RolesController(IRoleService svc, IUserService users) // ★ 新增注入
        {
            _svc = svc;
            _users = users;
        }

        /// <summary>
        /// 取得系統中所有角色清單：
        /// - 需具備 Roles.View 權限
        /// - 回傳 RoleDto（Id、顯示名稱、角色代碼）
        /// </summary>
        [HttpGet]
        [Authorize(Policy = "Roles.View")]
        public async Task<List<RoleDto>> Get() => await _svc.GetAllAsync();

        /// <summary>
        /// 直接用 userId 指派角色給使用者：
        /// - 需具備 Roles.Assign 權限
        /// - 適合在後台管理頁中，已經直接有 userId 的情況
        /// </summary>
        [HttpPost("{roleId:int}/users/{userId:int}")]
        [Authorize(Policy = "Roles.Assign")]
        public async Task<IActionResult> Assign([FromRoute] int roleId, [FromRoute] int userId)
        {
            await _svc.AssignUserAsync(roleId, userId);
            return NoContent();
        }

        /// <summary>
        /// 直接用 userId 收回該使用者的指定角色：
        /// - 需具備 Roles.Assign 權限
        /// </summary>
        [HttpDelete("{roleId:int}/users/{userId:int}")]
        [Authorize(Policy = "Roles.Assign")]
        public async Task<IActionResult> Revoke([FromRoute] int roleId, [FromRoute] int userId)
        {
            await _svc.RevokeUserAsync(roleId, userId);
            return NoContent();
        }

        /// <summary>
        /// 用 Email 指派角色給使用者時的 Request DTO：
        /// - 只需一個 Email 欄位
        /// </summary>
        public sealed class AssignUserByEmailDto { public string Email { get; set; } = ""; }

        /// <summary>
        /// 透過 Email 指派角色給使用者：
        /// 1. 用 IUserService.GetUserIdByEmailAsync 找出 userId
        /// 2. 若找不到使用者則回傳 404
        /// 3. 找到後呼叫 IRoleService.AssignUserAsync 指派角色
        /// </summary>
        [HttpPost("{roleId:int}/users/by-email")]
        [Authorize(Policy = "Roles.Assign")]
        public async Task<IActionResult> AssignByEmail([FromRoute] int roleId, [FromBody] AssignUserByEmailDto dto)
        {
            var uid = await _users.GetUserIdByEmailAsync(dto.Email);
            if (uid is null) return NotFound("User not found.");
            await _svc.AssignUserAsync(roleId, uid.Value);
            return NoContent();
        }

        /// <summary>
        /// 透過 Email 收回指定角色（可選功能）：
        /// - 流程與 AssignByEmail 類似，只是改成 RevokeUserAsync
        /// </summary>
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
