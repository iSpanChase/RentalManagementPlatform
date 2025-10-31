using RentalManagementPlatformWebAPI.Models;

namespace RentalManagementPlatformWebAPI.Services
{
    public interface IImageUrlResolver
    {
        string ResolveImageUrl(RoomPhoto photo);
    }
}
