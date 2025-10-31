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
			=> Ok(await _svc.RegisterAsync(dto));

		[HttpGet("me")]
		[Authorize]
		public async Task<ActionResult<UserProfileDto>> Me()
			=> Ok(await _svc.GetProfileAsync(User));

		[HttpPut("me")]
		[Authorize]
		public async Task<ActionResult<UserProfileDto>> Update([FromBody] UpdateProfileDto dto)
			=> Ok(await _svc.UpdateProfileAsync(User, dto));

		[HttpPost("me/avatar")]
		[Authorize]
		public async Task<ActionResult<string>> UploadAvatar(IFormFile file)
			=> Ok(await _svc.UploadAvatarAsync(User, file));
	}
}
