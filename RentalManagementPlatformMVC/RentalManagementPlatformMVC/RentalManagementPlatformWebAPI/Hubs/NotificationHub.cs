using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace RentalManagementPlatformWebAPI.Hubs
{
    /// <summary>
    /// 用於即時通知的 SignalR Hub。
    /// </summary>
    public class NotificationHub : Hub
    {
        /// <summary>
        /// 允許客戶端加入其 userId 專屬的群組，確保通知只發送給相關的用戶。
        /// </summary>
        /// <param name="userId">要訂閱的用戶 ID。</param>
        public async Task JoinUserGroup(string userId)
        {
            // 根據房東的 ID 建立一個群組名稱。
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
        }
    }
}