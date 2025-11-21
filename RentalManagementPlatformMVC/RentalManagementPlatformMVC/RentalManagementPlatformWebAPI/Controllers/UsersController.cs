using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentalManagementPlatformWebAPI.DTOs;
using RentalManagementPlatformWebAPI.Services;

namespace RentalManagementPlatformWebAPI.Controllers
{
    /// <summary>
    /// 使用者相關 API：
    /// - 註冊（簡化版）
    /// - 取得 / 更新「目前登入者」的個人資料
    /// - 依 Id 或 Username 查詢使用者（限管理員 / 系統管理員）
    /// - 上傳個人頭像
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _svc;
        public UsersController(IUserService svc) { _svc = svc; }

        /// <summary>
        /// 使用者註冊（UsersController 版本，與 AuthController 的 Register 稍有不同）：
        /// - 後端再檢查一次生日不可晚於今天（UTC Date）
        /// - 呼叫 IUserService.RegisterAsync 建立使用者
        /// - 這裡不處理寄驗證信，只回傳建立後的 UserProfileDto
        /// </summary>
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<ActionResult<UserProfileDto>> Register([FromBody] RegistrationRequestDto dto)
        {
            // 用 UTC 的 Date 比較，避免時區誤差
            if (dto.BirthDate.Date > DateTime.UtcNow.Date)
                return BadRequest(new { message = "生日不可晚於今天" });

            var created = await _svc.RegisterAsync(dto);
            return Ok(created);
        }

        /// <summary>
        /// 取得「目前登入者」的個人資料：
        /// - 需登入（[Authorize]）
        /// - 由 IUserService.GetProfileAsync 透過 Claims 解析 userId 再查 DB
        /// </summary>
        [HttpGet("me")]
        [Authorize]
        public async Task<ActionResult<UserProfileDto>> Me()
            => Ok(await _svc.GetProfileAsync(User));

        /// <summary>
        /// 依 userId 查任意使用者的 Profile：
        /// - 限 ADMIN / OPERATOR 角色使用（例如客服 / 後台查詢）
        /// </summary>
        // ★ 新增：查任意使用者
        [HttpGet("{id:int}")]
        [Authorize(Roles = "ADMIN,OPERATOR")] // 依你們實際客服/管理員角色調整
        public async Task<ActionResult<UserProfileDto>> GetById([FromRoute] int id)
        {
            var dto = await _svc.GetProfileByIdAsync(id);
            if (dto == null) return NotFound();
            return Ok(dto);
        }

        /// <summary>
        /// 依 Username 查詢使用者 Profile：
        /// - 限 ADMIN / OPERATOR 可以使用
        /// - 適合在後台或 FAQ 系統中「用帳號名稱查人」
        /// </summary>
        // UsersController
        [HttpGet("by-username/{username}")]
        [Authorize(Roles = "ADMIN,OPERATOR")] // 或你的客服 Policy
        public async Task<ActionResult<UserProfileDto>> GetByUsername(string username)
        {
            var dto = await _svc.FAQGetProfileByUsername(username);
            if (dto == null) return NotFound();
            return Ok(dto);
        }

        /// <summary>
        /// 更新「目前登入者」的個人資料：
        /// - 再次檢查生日不可晚於今天
        /// - 呼叫 IUserService.UpdateProfileAsync 進行更新
        /// - 回傳更新後的 UserProfileDto
        /// </summary>
        [HttpPut("me")]
        [Authorize]
        public async Task<ActionResult<UserProfileDto>> Update([FromBody] UpdateProfileDto dto)
        {
            if (dto.BirthDate.Date > DateTime.UtcNow.Date)
                return BadRequest(new { message = "生日不可晚於今天" });

            var updated = await _svc.UpdateProfileAsync(User, dto);
            return Ok(updated); // 你服務目前回傳 UserProfileDto，就維持 200 OK
        }

        /// <summary>
        /// 上傳「目前登入者」的大頭照：
        /// - 由 IUserService.UploadAvatarAsync 負責寫檔與更新 ProfileImageUrl
        /// - 回傳圖片 URL 字串
        /// </summary>
        [HttpPost("me/avatar")]
        [Authorize]
        public async Task<ActionResult<string>> UploadAvatar(IFormFile file)
            => Ok(await _svc.UploadAvatarAsync(User, file));
    }
}
