namespace RentalManagementPlatformWebAPI.DTOs
{
    /// <summary>
    /// 登入相關請求/回應 DTO：
    /// - LoginRequestDto       : 一般帳密登入用
    /// - GoogleLoginDto        : Google 第三方登入用
    /// - TokenRefreshDto       : 使用 RefreshToken 換新 AccessToken 用
    /// - LoginResponseDto      : 登入/刷新成功後，回傳給前端的資訊
    /// </summary>
    public record LoginRequestDto(string Email, string Password);

    /// <summary>
    /// 前端傳來的 Google 登入資訊：
    /// - IdToken 為 Google Sign-In 取得的 id_token
    /// - 後端會用 IGoogleTokenVerifier 驗證
    /// </summary>
    public record GoogleLoginDto(string IdToken);

    /// <summary>
    /// 前端以 RefreshToken 交換新 AccessToken 時使用的 DTO
    /// </summary>
    public record TokenRefreshDto(string RefreshToken);

    /// <summary>
    /// 登入 / 刷新 Token 成功後回傳的資料：
    /// - AccessToken  : JWT 字串，前端放在 Authorization: Bearer ...
    /// - ExpiresAt    : AccessToken 到期時間（UTC）
    /// - RefreshToken : 用來換新 AccessToken 的長期憑證
    /// - Profile      : 登入使用者的基本資料
    /// - Roles        : 使用者擁有的角色代碼清單（ADMIN / TENANT ...）
    /// - Permissions  : 使用者擁有的權限代碼清單（Admin.ApproveOperator 等）
    /// </summary>
    public class LoginResponseDto
    {
        /// <summary>
        /// JWT Access Token 字串，前端呼叫 API 時要帶在 Header
        /// </summary>
        public string AccessToken { get; set; } = null!;

        /// <summary>
        /// AccessToken 的到期時間（UTC）
        /// </summary>
        public DateTime ExpiresAt { get; set; }

        /// <summary>
        /// 用於刷新 AccessToken 的 RefreshToken（非 JWT，純隨機字串）
        /// </summary>
        public string RefreshToken { get; set; } = null!;

        /// <summary>
        /// 登入使用者的基本資料（UserProfileDto）
        /// </summary>
        public UserProfileDto Profile { get; set; } = null!;

        /// <summary>
        /// 使用者擁有的角色代碼清單（例如：ADMIN、TENANT、HOST...）
        /// </summary>
        public IEnumerable<string> Roles { get; set; } = Array.Empty<string>();

        /// <summary>
        /// 使用者擁有的權限代碼清單（例如：Admin.ApproveOperator）
        /// </summary>
        public IEnumerable<string> Permissions { get; set; } = Array.Empty<string>();
    }
}
