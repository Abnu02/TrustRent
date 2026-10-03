using TrustRent.Application.Abstractions.Storage;
using Microsoft.AspNetCore.Http;


namespace TrustRent.Infrastructure.Services;

public class LocalFileStorageService : IFileStorageService
{
    private readonly string _webRootPath;
    private readonly IHttpContextAccessor _httpContextAccessor;

    private static readonly string[] AllowedExtensions =
    {
        ".jpg",
        ".jpeg",
        ".png",
        ".webp"
    };

    private const long MaxFileSize =
        5 * 1024 * 1024;

    public LocalFileStorageService(
        IHttpContextAccessor httpContextAccessor)
    {
        _webRootPath = Path.Combine(AppContext.BaseDirectory, "wwwroot");
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<string> SavePropertyImageAsync(
        Stream fileStream,
        string fileName,
        string contentType,
        CancellationToken cancellationToken)
    {
        var extension =
            Path.GetExtension(fileName)
                .ToLowerInvariant();

        if (!AllowedExtensions.Contains(extension))
            throw new InvalidOperationException(
                "Only JPG, JPEG, PNG and WEBP images are allowed.");

        if (fileStream.Length > MaxFileSize)
            throw new InvalidOperationException(
                "Image size cannot exceed 5 MB.");

        var uploadFolder =
            Path.Combine(
                _webRootPath,
                "uploads",
                "properties");

        Directory.CreateDirectory(uploadFolder);

        var storedName =
            $"{Guid.NewGuid():N}{extension}";

        var physicalPath =
            Path.Combine(
                uploadFolder,
                storedName);

        await using var output =
            new FileStream(
                physicalPath,
                FileMode.Create);

        await fileStream.CopyToAsync(
            output,
            cancellationToken);

        var request =
            _httpContextAccessor.HttpContext?.Request;

        var baseUrl =
            $"{request?.Scheme}://{request?.Host}";

        return $"{baseUrl}/uploads/properties/{storedName}";
    }

    public Task DeleteAsync(
        string? relativePath,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
            return Task.CompletedTask;

        var fileName =
            Path.GetFileName(
                new Uri(relativePath).LocalPath);

        var path =
            Path.Combine(
                _webRootPath,
                "uploads",
                "properties",
                fileName);

        if (File.Exists(path))
            File.Delete(path);

        return Task.CompletedTask;
    }
}