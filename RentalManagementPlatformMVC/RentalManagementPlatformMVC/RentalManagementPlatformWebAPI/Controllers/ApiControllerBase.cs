
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace RentalManagementPlatformWebAPI.Controllers
{
    public abstract class ApiControllerBase : ControllerBase
    {
        /// <summary>
        /// Gets the user ID of the currently authenticated user from the token claims.
        /// </summary>
        /// <exception cref="InvalidOperationException">Thrown if the user ID claim is not found in the token, which should not happen for authorized requests.</exception>
        protected int CurrentUserId
        {
            get
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (int.TryParse(userIdClaim, out int userId))
                {
                    return userId;
                }
                // This should not happen if the endpoint is protected with [Authorize]
                throw new InvalidOperationException("User ID not found or invalid in token.");
            }
        }
    }
}
