using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RentalManagementPlatformWebAPI.Models;
using System;
using System.Security.Cryptography;
using System.Text;
using static RentalManagementPlatformWebAPI.Services.IEmailVerificationService;

namespace RentalManagementPlatformWebAPI.Services
{
	public class EmailVerificationService : IEmailVerificationService
	{
		private readonly RentalManagementPlatformSqlContext _db;
		private readonly IEmailSender _sender;
		private readonly IConfiguration _cfg;
		private readonly EmailVerificationOptions _opt;

		public EmailVerificationService(
			RentalManagementPlatformSqlContext db,
			IEmailSender sender,
			IConfiguration cfg,
			IOptions<EmailVerificationOptions> opt)
		{
			_db = db;
			_sender = sender;
			_cfg = cfg;
			_opt = opt.Value;
		}

		private static string NewToken()
		{
			var bytes = RandomNumberGenerator.GetBytes(32);          // 256-bit
			return Convert.ToBase64String(bytes)                      // base64 -> urlsafe
				   .TrimEnd('=').Replace('+', '-').Replace('/', '_');
		}

		private static string Hash(string raw)
		{
			using var sha = SHA256.Create();
			var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(raw));
			return BitConverter.ToString(hash).Replace("-", "").ToUpperInvariant();
		}

		public async Task CreateAndSendAsync(int userId, string email, CancellationToken ct, string? baseVerifyUrl = null)
		{
			// 1) 作廢舊未使用 token（避免多碼同時有效）
			var old = await _db.EmailVerifications
				.Where(x => x.UserId == userId && !x.IsUsed)
				.ToListAsync(ct);
			foreach (var x in old) x.IsUsed = true;

			// 2) 產新碼並寫入 DB（僅存雜湊）
			var raw = NewToken();
			var entity = new EmailVerification
			{
				UserId = userId,
				TokenHash = Hash(raw),
				ExpiresAt = DateTime.UtcNow.AddMinutes(_opt.ExpireMinutes),
				IsUsed = false,
				CreatedAt = DateTime.UtcNow
			};
			_db.EmailVerifications.Add(entity);
			await _db.SaveChangesAsync(ct);

			// 3) 寄出 Email（連結由前端頁處理）
			var baseUrl = (baseVerifyUrl ?? _opt.BaseVerifyUrl).TrimEnd('/');
			var link = $"{baseUrl}?email={Uri.EscapeDataString(email)}&token={Uri.EscapeDataString(raw)}";

			var html = $@"<p>您好，</p>
                  <p>請於 {_opt.ExpireMinutes} 分鐘內點擊以下連結完成電子郵件驗證：</p>
                  <p><a href=""{link}"" target=""_blank"">{link}</a></p>";

			await _sender.SendAsync(email, "請完成電子郵件驗證", html);
		}

		public async Task<bool> VerifyAsync(string email, string token, CancellationToken ct)
		{
			var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email, ct);
			if (user == null) return false;

			var hash = Hash(token ?? "");
			var rec = await _db.EmailVerifications
				.Where(x => x.UserId == user.UserId && !x.IsUsed)
				.OrderByDescending(x => x.CreatedAt)
				.FirstOrDefaultAsync(ct);

			if (rec == null) return false;
			if (!string.Equals(rec.TokenHash, hash, StringComparison.Ordinal)) return false;
			if (rec.ExpiresAt < DateTime.UtcNow) return false;

			// 標記使用 & 將使用者設為已驗證
			rec.IsUsed = true;
			user.Isverified = true;
			await _db.SaveChangesAsync(ct);
			return true;
		}

	}
}
