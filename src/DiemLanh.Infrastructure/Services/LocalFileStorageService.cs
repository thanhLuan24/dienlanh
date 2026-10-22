using DiemLanh.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace DiemLanh.Infrastructure.Services;

public class LocalFileStorageService : IFileStorageService
{
    private readonly ILogger<LocalFileStorageService> _logger;
    private readonly string _webRootPath;

    private static readonly HashSet<string> _allowedImageTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg", "image/jpg", "image/png", "image/webp", "image/gif"
    };

    public LocalFileStorageService(ILogger<LocalFileStorageService> logger, string webRootPath)
    {
        _logger = logger;
        _webRootPath = webRootPath;
    }

    public async Task<string> SaveFileAsync(Stream fileStream, string fileName, string folder)
    {
        var uploadsRoot = Path.Combine(_webRootPath, "uploads");
        var folderPath = Path.Combine(uploadsRoot, folder);
        Directory.CreateDirectory(folderPath);

        var ext = Path.GetExtension(fileName);
        var uniqueName = $"{Guid.NewGuid():N}{ext}";
        var filePath = Path.Combine(folderPath, uniqueName);

        await using var output = File.Create(filePath);
        await fileStream.CopyToAsync(output);

        _logger.LogInformation("File saved: {FilePath}", filePath);
        return $"/uploads/{folder}/{uniqueName}";
    }

    public Task DeleteFileAsync(string filePath)
    {
        if (string.IsNullOrEmpty(filePath)) return Task.CompletedTask;

        var physicalPath = Path.Combine(_webRootPath, filePath.TrimStart('/'));
        if (File.Exists(physicalPath))
        {
            File.Delete(physicalPath);
            _logger.LogInformation("File deleted: {FilePath}", physicalPath);
        }
        return Task.CompletedTask;
    }

    public bool IsValidImageType(string contentType)
        => _allowedImageTypes.Contains(contentType);

    public bool IsValidFileSize(long fileSizeBytes, long maxSizeMb = 5)
        => fileSizeBytes <= maxSizeMb * 1024 * 1024;

    public string GetPublicUrl(string filePath) => filePath;
}
