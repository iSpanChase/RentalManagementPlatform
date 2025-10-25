using System.IO;
using System.Threading.Tasks;

namespace RentalManagementPlatformWebAPI.Services.Interfaces
{
    public interface IMinioService
    {
        Task<string> UploadFileAsync(Stream stream, string fileName);
        Task<string> GetFileUrlAsync(string objectName);
    }
}