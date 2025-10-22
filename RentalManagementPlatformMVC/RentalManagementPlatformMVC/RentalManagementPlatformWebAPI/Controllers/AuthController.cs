using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentalManagementPlatformWebAPI.DTOs;
using RentalManagementPlatformWebAPI.Services;

namespace RentalManagementPlatformWebAPI.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class AuthController : ControllerBase
	{
		private readonly IAuthService _auth;
		public AuthController(IAuthService auth) { _auth = auth; }

		[HttpPost("login")]
		[AllowAnonymous]
		public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginRequestDto dto)
			=> Ok(await _auth.LoginAsync(dto));

		[HttpPost("google")]
		[AllowAnonymous]
		public async Task<ActionResult<LoginResponseDto>> Google([FromBody] GoogleLoginDto dto)
			=> Ok(await _auth.GoogleLoginAsync(dto.IdToken));

		[HttpPost("refresh")]
		[AllowAnonymous]
		public async Task<ActionResult<LoginResponseDto>> Refresh([FromBody] TokenRefreshDto dto)
			=> Ok(await _auth.RefreshAsync(dto.RefreshToken));

		[HttpPost("logout")]
		[Authorize]
		public async Task<IActionResult> Logout()
		{
			var sub = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
				   ?? User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value;

			if (!int.TryParse(sub, out var userId)) return NoContent();

			await _auth.RevokeAllAsync(userId);
			return NoContent();
		}
	}
}
