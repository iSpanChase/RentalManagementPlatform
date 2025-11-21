using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentalManagementPlatformWebAPI.DTOs;
using RentalManagementPlatformWebAPI.Services;

namespace RentalManagementPlatformWebAPI.Controllers
{
    /// <summary>
    /// 權限（Permission）管理相關 API：
    /// - 查詢系統所有權限
    /// - 查詢某角色擁有的權限 Id
    /// - 對角色指派 / 移除權限
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class PermissionsController : ControllerBase
    {
        private readonly IPermissionService _svc;
        public PermissionsController(IPermissionService svc) { _svc = svc; }

        /// <summary>
        /// 取得系統中所有權限清單：
        /// - 需具備 Permissions.View 權限
        /// - 回傳 PermissionDto 清單，給前端顯示用
        /// </summary>
        [HttpGet]
        [Authorize(Policy = "Permissions.View")]
        public async Task<List<PermissionDto>> Get() => await _svc.GetAllAsync();

        /// <summary>
        /// 依角色 Id 取得該角色目前擁有的所有權限 Id：
        /// - 用於前端角色編輯頁，預設勾選已擁有的權限
        /// </summary>
        [Authorize(Policy = "Permissions.View")]
        [HttpGet("roles/{roleId:int}")]
        public async Task<ActionResult<List<int>>> GetRolePermissionIds([FromRoute] int roleId)
        {
            var ids = await _svc.GetIdsByRoleAsync(roleId);
            return Ok(ids);
        }

        /// <summary>
        /// 將一組權限 Id 指派給指定角色：
        /// - 需具備 Permissions.ManagePermissions 權限
        /// - 由 IPermissionService 處理實際指派邏輯與快取清除
        /// </summary>
        [HttpPost("roles/{roleId:int}")]
        [Authorize(Policy = "Permissions.ManagePermissions")]
        public async Task<IActionResult> Assign([FromRoute] int roleId, [FromBody] AssignPermissionDto dto)
        {
            await _svc.AssignAsync(roleId, dto.PermissionIds);
            return NoContent();
        }

        /// <summary>
        /// 從指定角色移除一組權限 Id：
        /// - 需具備 Permissions.ManagePermissions 權限
        /// - 由 IPermissionService 負責刪除關聯與清除 claims 快取
        /// </summary>
        [HttpDelete("roles/{roleId:int}")]
        [Authorize(Policy = "Permissions.ManagePermissions")]
        public async Task<IActionResult> Remove([FromRoute] int roleId, [FromBody] AssignPermissionDto dto)
        {
            await _svc.RemoveAsync(roleId, dto.PermissionIds);
            return NoContent();
        }
    }
}
