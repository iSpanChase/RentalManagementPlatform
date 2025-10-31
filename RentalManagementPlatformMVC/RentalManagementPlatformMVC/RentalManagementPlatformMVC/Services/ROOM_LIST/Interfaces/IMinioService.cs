namespace RentalManagementPlatformMVC.Services
{
    public interface IMinioService
    {
        Task<string> UploadFileAsync(Stream stream, string fileName);
        Task<string> GetFileUrlAsync(string objectName);
    }
}
