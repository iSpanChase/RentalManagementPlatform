using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentalManagementPlatformWebAPI.DTOs;
using RentalManagementPlatformWebAPI.Services;

namespace RentalManagementPlatformWebAPI.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class UsersController : ControllerBase
	{
		private readonly IUserService _svc;
		public UsersController(IUserService svc) { _svc = svc; }

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

		[HttpGet("me")]
		[Authorize]
		public async Task<ActionResult<UserProfileDto>> Me()
			=> Ok(await _svc.GetProfileAsync(User));

		[HttpPut("me")]
		[Authorize]
		public async Task<ActionResult<UserProfileDto>> Update([FromBody] UpdateProfileDto dto)
		{
			if (dto.BirthDate.Date > DateTime.UtcNow.Date)
				return BadRequest(new { message = "生日不可晚於今天" });

			var updated = await _svc.UpdateProfileAsync(User, dto);
			return Ok(updated); // 你服務目前回傳 UserProfileDto，就維持 200 OK
		}

		[HttpPost("me/avatar")]
		[Authorize]
		public async Task<ActionResult<string>> UploadAvatar(IFormFile file)
			=> Ok(await _svc.UploadAvatarAsync(User, file));
	}
}
