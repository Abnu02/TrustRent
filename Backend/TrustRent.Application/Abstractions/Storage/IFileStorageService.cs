namespace TrustRent.Application.Abstractions.Storage;

public interface IFileStorageService
{
    Task<string> SavePropertyImageAsync(
        Stream fileStream,
        string fileName,
        string contentType,
        CancellationToken cancellationToken);

    Task DeleteAsync(
        string? relativePath,
        CancellationToken cancellationToken);
}