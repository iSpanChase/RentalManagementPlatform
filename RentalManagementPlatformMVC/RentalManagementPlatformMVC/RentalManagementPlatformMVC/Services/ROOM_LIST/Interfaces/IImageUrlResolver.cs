using RentalManagementPlatformMVC.Models;

namespace RentalManagementPlatformMVC.Services
{
    public interface IImageUrlResolver
    {
        string ResolveImageUrl(RoomPhoto photo);
    }
}
