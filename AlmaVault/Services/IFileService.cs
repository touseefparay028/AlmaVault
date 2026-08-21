
namespace AlmaVault.Services
{
    public interface IFileService
    {
        Task<(string RelativePath, string OriginalFileName)> UploadPdfAsync(IFormFile file, string subFolder);
    }
}