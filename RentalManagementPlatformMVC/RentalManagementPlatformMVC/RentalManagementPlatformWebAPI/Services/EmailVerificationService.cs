using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RentalManagementPlatformWebAPI.Models;
using System;
using System.Security.Cryptography;
using System.Text;
using static RentalManagementPlatformWebAPI.Services.IEmailVerificationService;

namespace RentalManagementPlatformWebAPI.Services
{
    /// <summary>
    /// 負責「Email 驗證」相關流程：
    /// - 產生一次性驗證 Token（原始碼只寄給使用者，DB 只存 Hash）
    /// - 寄送驗證信（內含前端的驗證連結）
    /// - 驗證 Token 是否正確 / 未過期，並更新使用者 Isverified 狀態
    /// </summary>
    public class EmailVerificationService : IEmailVerificationService
    {
        private readonly RentalManagementPlatformSqlContext _db;
        private readonly IEmailSender _sender;
        private readonly IConfiguration _cfg;
        private readonly EmailVerificationOptions _opt;

        /// <summary>
        /// 透過 DI 注入：
        /// - DbContext：讀寫 Users / EmailVerifications
        /// - IEmailSender：實際發送 Email 的實作
        /// - IConfiguration：若有其他設定可使用
        /// - IOptions&lt;EmailVerificationOptions&gt;：讀取驗證相關設定（例如過期時間、驗證 URL）
        /// </summary>
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

        /// <summary>
        /// 產生一組隨機 Token（原始碼）：
        /// - 使用 256-bit 隨機數
        /// - 先做 Base64，再轉成 URL-Safe 格式（移除 =、+ → -、/ → _）
        ///   → 方便當成 QueryString 帶在驗證連結中
        /// </summary>
        private static string NewToken()
        {
            var bytes = RandomNumberGenerator.GetBytes(32);          // 256-bit
            return Convert.ToBase64String(bytes)                      // base64 -> urlsafe
                   .TrimEnd('=').Replace('+', '-').Replace('/', '_');
        }

        /// <summary>
        /// 將原始 Token 做 SHA256 雜湊：
        /// - DB 裡只存 Hash，不存原始 Token，降低外洩風險
        /// - Hash 轉為大寫的十六進位字串（方便比對）
        /// </summary>
        private static string Hash(string raw)
        {
            using var sha = SHA256.Create();
            var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(raw));
            return BitConverter.ToString(hash).Replace("-", "").ToUpperInvariant();
        }

        /// <summary>
        /// 建立新的 Email 驗證紀錄並寄出驗證信：
        /// 步驟：
        /// 1. 作廢此 userId 之前所有未使用的驗證碼（避免同時存在多組有效碼）
        /// 2. 產生新的 Token 原始碼，將 Hash 存進 EmailVerifications 資料表
        /// 3. 組出前端驗證頁的 URL（含 email、token），透過 IEmailSender 寄出
        /// </summary>
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
            // baseVerifyUrl 可由呼叫端覆寫，否則使用設定檔內的 BaseVerifyUrl
            var baseUrl = (baseVerifyUrl ?? _opt.BaseVerifyUrl).TrimEnd('/');
            var link = $"{baseUrl}?email={Uri.EscapeDataString(email)}&token={Uri.EscapeDataString(raw)}";

            var html = $@"<p>您好，</p>
                  <p>請於 {_opt.ExpireMinutes} 分鐘內點擊以下連結完成電子郵件驗證：</p>
                  <p><a href=""{link}"" target=""_blank"">{link}</a></p>";

            await _sender.SendAsync(email, "請完成電子郵件驗證", html);
        }

        /// <summary>
        /// 驗證 Email + Token 是否有效：
        /// 1. 先找對應的 User
        /// 2. 取得此使用者最新一筆「尚未使用」的驗證紀錄
        /// 3. 比對 TokenHash 是否相符、是否尚未過期
        /// 4. 若成功：
        ///    - 將該紀錄標記為已使用 (IsUsed = true)
        ///    - 將 User.Isverified 設為 true
        /// </summary>
        public async Task<bool> VerifyAsync(string email, string token, CancellationToken ct)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email, ct);
            if (user == null) return false;

            // 將使用者帶來的 token 做 Hash 來比對 DB 中的 TokenHash
            var hash = Hash(token ?? "");
            var rec = await _db.EmailVerifications
                .Where(x => x.UserId == user.UserId && !x.IsUsed)
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync(ct);

            // 找不到紀錄、Hash 不符或已過期 → 驗證失敗
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
