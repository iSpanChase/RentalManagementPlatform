using RentalManagementPlatformWebAPI.DTOs;
using System.Security.Claims;

namespace RentalManagementPlatformWebAPI.Services
{
    /// <summary>
    /// 使用者相關服務介面：
    /// - 包含註冊、取得/更新個人資料、上傳頭像
    /// - 透過 Email / UserId / Username 查詢使用者
    /// - 支援第三方登入相關流程
    /// </summary>
    public interface IUserService
    {
        /// <summary>
        /// 一般註冊，建立本地帳號並視情況指派對應角色
        /// </summary>
        Task<UserProfileDto> RegisterAsync(RegistrationRequestDto dto);

        /// <summary>
        /// 依目前登入者的 Claims 取得個人資料
        /// </summary>
        Task<UserProfileDto> GetProfileAsync(ClaimsPrincipal principal);

        /// <summary>
        /// 依目前登入者的 Claims 更新個人資料
        /// </summary>
        Task<UserProfileDto> UpdateProfileAsync(ClaimsPrincipal principal, UpdateProfileDto dto);

        /// <summary>
        /// 上傳目前登入者的大頭照並回傳圖片 URL
        /// </summary>
        Task<string> UploadAvatarAsync(ClaimsPrincipal principal, IFormFile file);

        /// <summary>
        /// 透過 Email 查詢 UserId（重設密碼 / 驗證信流程常用）
        /// </summary>
        Task<int?> GetUserIdByEmailAsync(string email);

        /// <summary>
        /// 透過 UserId 取得 Profile（FAQ 或後台查詢使用）
        /// </summary>
        Task<UserProfileDto?> GetProfileByIdAsync(int userId);

        /// <summary>
        /// 透過 Username 取得 Profile（FAQ 用途）
        /// </summary>
        Task<UserProfileDto?> FAQGetProfileByUsername(string username);

        /// <summary>
        /// 依第三方登入資訊（Google / LINE 等）找到或建立使用者
        /// </summary>
        Task<UserProfileDto> FindOrCreateFromExternalAsync(ExternalProfileDto dto);
    }
}
