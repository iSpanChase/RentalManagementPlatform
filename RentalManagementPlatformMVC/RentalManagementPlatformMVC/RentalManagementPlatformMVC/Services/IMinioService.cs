namespace RentalManagementPlatformMVC.Services
{
    public interface IMinioService
    {
        Task<string> UploadFileAsync(Stream stream, string fileName, string contentType);
        Task<string> GetFileUrlAsync(string objectName);
    }
}
