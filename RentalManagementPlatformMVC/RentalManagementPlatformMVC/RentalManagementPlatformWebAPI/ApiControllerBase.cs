using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace RentalManagementPlatformWebAPI
{
    public abstract class ApiControllerBase : ControllerBase
    {
        protected int CurrentUserId
        {
            get
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                
                if (int.TryParse(userIdClaim, out int userId))
                {
                    return userId;
                }
                // 如果找不到或無法轉換，拋出例外
                throw new InvalidOperationException("User ID not found in token.");
            }
        }
    }
}
