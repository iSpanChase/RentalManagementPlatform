using RentalManagementPlatformWebAPI.DTOs;

namespace RentalManagementPlatformWebAPI.Services
{
    /// <summary>
    /// 一組 JWT Token（Access Token + Refresh Token）的回傳資料模型
    /// </summary>
    public record JwtPair(string AccessToken, DateTime ExpiresAt, string RefreshToken);

    /// <summary>
    /// 負責產生 JWT Token 的服務介面：
    /// - Create：同時產生 AccessToken + RefreshToken
    /// - IssueTokenAsync：只發一顆 AccessToken（不處理 refresh）
    /// </summary>
    public interface IJwtTokenService
    {
        /// <summary>
        /// 產生一組 JwtPair：
        /// - 會把 userId / email / fullName / roles / permissions 寫進 JWT claims
        /// - 回傳 AccessToken + 到期時間 + RefreshToken（隨機字串）
        /// </summary>
        JwtPair Create(int userId, string email, string fullName, IEnumerable<string> roles, IEnumerable<string> permissions);

        /// <summary>
        /// 只針對 UserProfileDto 發出一顆 AccessToken：
        /// - 目前內部會呼叫 Create(...)，但 roles / permissions 傳入空集合
        /// </summary>
        Task<string> IssueTokenAsync(UserProfileDto user);
    }
}
