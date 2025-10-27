using System.Threading.Tasks;

namespace RentalManagementPlatformWebAPI.Services.Interfaces
{
    public interface IFileUrlResolver
    {
        Task<string> GetUrlAsync(string entityType, int entityId, string photoType);
    }
}