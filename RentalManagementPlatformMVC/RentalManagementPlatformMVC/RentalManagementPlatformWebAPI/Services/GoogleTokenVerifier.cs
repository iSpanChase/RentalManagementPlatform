using Google.Apis.Auth;

namespace RentalManagementPlatformWebAPI.Services
{
    /// <summary>
    /// 使用 Google 官方套件驗證 Google ID Token：
    /// - 驗證簽章 / 有效期限
    /// - （選擇性）驗證 Audience 是否符合預期的 ClientId
    /// - 驗證成功後，回傳精簡過的 GoogleProfile
    /// </summary>
    public class GoogleTokenVerifier : IGoogleTokenVerifier
    {
        /// <summary>
        /// 驗證來自前端的 Google idToken：
        /// - expectedAudience 不為 null 時，代表要檢查 aud 是否等於你設定的 clientId
        /// - 驗證成功 → 轉成自訂的 GoogleProfile 回傳
        /// - 驗證失敗（例外） → 回傳 null，由呼叫端判斷
        /// </summary>
        public async Task<GoogleProfile?> VerifyAsync(string idToken, string? expectedAudience = null)
        {
            var settings = new GoogleJsonWebSignature.ValidationSettings
            {
                // 若 expectedAudience 為 null，Audience 也設為 null → 使用 Google 預設驗證方式
                Audience = expectedAudience is null ? null : new[] { expectedAudience }
            };

            try
            {
                var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);
                return new GoogleProfile(
                    Sub: payload.Subject,
                    Email: payload.Email,
                    Name: payload.Name,
                    Picture: payload.Picture,
                    EmailVerified: payload.EmailVerified
                );
            }
            catch
            {
                // 任一驗證失敗（例如無效 token、簽章錯誤、aud 不符）皆回傳 null
                return null;
            }
        }
    }
}
