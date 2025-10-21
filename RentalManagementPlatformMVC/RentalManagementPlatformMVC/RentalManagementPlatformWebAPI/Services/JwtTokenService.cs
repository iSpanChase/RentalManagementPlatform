using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace RentalManagementPlatformWebAPI.Services
{
	public class JwtTokenService : IJwtTokenService
	{
		private readonly IConfiguration _cfg;
		public JwtTokenService(IConfiguration cfg) { _cfg = cfg; }

		public JwtPair Create(int userId, string email, string fullName, IEnumerable<string> roles, IEnumerable<string> permissions)
		{
			var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_cfg["Jwt:Key"]!));
			var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

			var claims = new List<Claim>
		{
			new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
			new Claim(ClaimTypes.Email, email),
			new Claim(ClaimTypes.Name, fullName)
		};
			claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));
			claims.AddRange(permissions.Select(p => new Claim("perm", p)));

			var expires = DateTime.UtcNow.AddMinutes(int.Parse(_cfg["Jwt:AccessTokenMinutes"] ?? "30"));

			var token = new JwtSecurityToken(
				issuer: _cfg["Jwt:Issuer"],
				audience: _cfg["Jwt:Audience"],
				claims: claims,
				expires: expires,
				signingCredentials: creds
			);

			var access = new JwtSecurityTokenHandler().WriteToken(token);
			var refresh = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
			return new JwtPair(access, expires, refresh);
		}
	}
}
