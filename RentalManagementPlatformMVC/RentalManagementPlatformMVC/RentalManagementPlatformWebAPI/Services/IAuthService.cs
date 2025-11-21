using RentalManagementPlatformWebAPI.DTOs;

namespace RentalManagementPlatformWebAPI.Services
{
    /// <summary>
    /// 認證相關服務介面：
    /// - 一般帳密登入
    /// - Google 第三方登入
    /// - 使用 RefreshToken 刷新 access token
    /// - 作廢使用者所有 RefreshToken（全部登出）
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// 一般帳密登入，成功時回傳 JWT token 與使用者資訊
        /// </summary>
        Task<LoginResponseDto> LoginAsync(LoginRequestDto dto);

        /// <summary>
        /// Google 第三方登入，使用 Google idToken 驗證並登入/建立帳號
        /// </summary>
        Task<LoginResponseDto> GoogleLoginAsync(string idToken);

        /// <summary>
        /// 使用 RefreshToken 取得新的 AccessToken / RefreshToken 配對
        /// </summary>
        Task<LoginResponseDto> RefreshAsync(string refreshToken);

        /// <summary>
        /// 作廢特定使用者的所有有效 RefreshToken（例如：全部裝置登出）
        /// </summary>
        Task RevokeAllAsync(int userId);
    }
}
