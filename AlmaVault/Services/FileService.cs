namespace AlmaVault.Services
{
    public class FileService : IFileService
    {
        private readonly IWebHostEnvironment _environment;

        public FileService(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public async Task<(string RelativePath, string OriginalFileName)> UploadPdfAsync(IFormFile file, string subFolder)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("No file provided.");
            }

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (extension != ".pdf")
            {
                throw new InvalidOperationException("Only PDF files are allowed.");
            }

            // Path targeting wwwroot/{subFolder}
            var uploadsFolder = Path.Combine(_environment.WebRootPath, subFolder);
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var originalFileName = Path.GetFileName(file.FileName);
            var uniqueFileName = $"{Guid.NewGuid()}_{originalFileName}";
            var fullPath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            var relativePath = $"/{subFolder.Replace('\\', '/')}/{uniqueFileName}";
            return (relativePath, originalFileName);
        }
    }
}