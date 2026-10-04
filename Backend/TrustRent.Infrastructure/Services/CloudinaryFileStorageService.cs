using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using TrustRent.Application.Abstractions.Storage;

namespace TrustRent.Infrastructure.Services;

public sealed class CloudinaryFileStorageService : IFileStorageService
{
    private static readonly HashSet<string> AllowedExtensions =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg",
            ".jpeg",
            ".png",
            ".webp"
        };

    private const long MaxFileSize = 5 * 1024 * 1024;

    private readonly HttpClient _httpClient;
    private readonly string _cloudName;
    private readonly string _apiKey;
    private readonly string _apiSecret;
    private readonly string _folder;

    public CloudinaryFileStorageService(
        HttpClient httpClient,
        IConfiguration configuration)
    {
        _httpClient = httpClient;
        _cloudName = configuration["Cloudinary:CloudName"]
            ?? throw new InvalidOperationException(
                "Cloudinary cloud name is missing.");
        _apiKey = configuration["Cloudinary:ApiKey"]
            ?? throw new InvalidOperationException(
                "Cloudinary API key is missing.");
        _apiSecret = configuration["Cloudinary:ApiSecret"]
            ?? throw new InvalidOperationException(
                "Cloudinary API secret is missing.");
        _folder = configuration["Cloudinary:Folder"] ??
            "trustrent/properties";
    }

    public async Task<string> SavePropertyImageAsync(
        Stream fileStream,
        string fileName,
        string contentType,
        CancellationToken cancellationToken)
    {
        var extension = Path.GetExtension(fileName);
        if (!AllowedExtensions.Contains(extension))
        {
            throw new InvalidOperationException(
                "Only JPG, JPEG, PNG and WEBP images are allowed.");
        }

        if (fileStream.Length > MaxFileSize)
        {
            throw new InvalidOperationException(
                "Image size cannot exceed 5 MB.");
        }

        if (!contentType.StartsWith(
                "image/",
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "The uploaded file must be an image.");
        }

        var publicId =
            $"{_folder}/{Guid.NewGuid():N}";
        var timestamp = DateTimeOffset.UtcNow
            .ToUnixTimeSeconds()
            .ToString(CultureInfo.InvariantCulture);

        var parameters = new SortedDictionary<string, string>(
            StringComparer.Ordinal)
        {
            ["public_id"] = publicId,
            ["timestamp"] = timestamp
        };

        using var content = new MultipartFormDataContent();
        content.Add(new StringContent(_apiKey), "api_key");
        content.Add(new StringContent(publicId), "public_id");
        content.Add(new StringContent(timestamp), "timestamp");
        content.Add(new StringContent(Sign(parameters)), "signature");

        var imageContent = new StreamContent(fileStream);
        imageContent.Headers.ContentType =
            new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
        content.Add(imageContent, "file", Path.GetFileName(fileName));

        using var response = await _httpClient.PostAsync(
            GetCloudinaryEndpoint("image/upload"),
            content,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        await using var responseStream =
            await response.Content.ReadAsStreamAsync(cancellationToken);
        using var json = await JsonDocument.ParseAsync(
            responseStream,
            cancellationToken: cancellationToken);

        if (!json.RootElement.TryGetProperty("secure_url", out var imageUrl) ||
            string.IsNullOrWhiteSpace(imageUrl.GetString()))
        {
            throw new InvalidOperationException(
                "Cloudinary upload response did not contain an image URL.");
        }

        return imageUrl.GetString()!;
    }

    public async Task DeleteAsync(
        string? relativePath,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(relativePath) ||
            !Uri.TryCreate(relativePath, UriKind.Absolute, out var imageUri) ||
            !string.Equals(
                imageUri.Host,
                "res.cloudinary.com",
                StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var publicId = GetPublicId(imageUri);
        if (string.IsNullOrWhiteSpace(publicId))
            return;

        var timestamp = DateTimeOffset.UtcNow
            .ToUnixTimeSeconds()
            .ToString(CultureInfo.InvariantCulture);
        var parameters = new SortedDictionary<string, string>(
            StringComparer.Ordinal)
        {
            ["public_id"] = publicId,
            ["timestamp"] = timestamp
        };

        using var content = new FormUrlEncodedContent(
        [
            new KeyValuePair<string, string>("api_key", _apiKey),
            new KeyValuePair<string, string>("public_id", publicId),
            new KeyValuePair<string, string>("timestamp", timestamp),
            new KeyValuePair<string, string>("signature", Sign(parameters))
        ]);

        using var response = await _httpClient.PostAsync(
            GetCloudinaryEndpoint("image/destroy"),
            content,
            cancellationToken);

        response.EnsureSuccessStatusCode();
    }

    private string Sign(IEnumerable<KeyValuePair<string, string>> parameters)
    {
        var signedValues = string.Join(
            "&",
            parameters.Select(parameter =>
                $"{parameter.Key}={parameter.Value}"));
        var bytes = Encoding.UTF8.GetBytes(signedValues + _apiSecret);
        return Convert.ToHexString(SHA1.HashData(bytes)).ToLowerInvariant();
    }

    private Uri GetCloudinaryEndpoint(string action)
    {
        return new Uri(
            $"https://api.cloudinary.com/v1_1/{Uri.EscapeDataString(_cloudName)}/{action}");
    }

    private string GetPublicId(Uri imageUri)
    {
        var segments = imageUri.AbsolutePath
            .Split('/', StringSplitOptions.RemoveEmptyEntries);
        var uploadIndex = Array.IndexOf(segments, "upload");

        if (uploadIndex < 0 || uploadIndex + 1 >= segments.Length)
            return string.Empty;

        var assetSegments = segments
            .Skip(uploadIndex + 1)
            .ToArray();

        if (assetSegments.Length > 0 &&
            assetSegments[0].Length > 1 &&
            assetSegments[0][0] == 'v' &&
            assetSegments[0][1..].All(char.IsDigit))
        {
            assetSegments = assetSegments[1..];
        }

        if (assetSegments.Length == 0)
            return string.Empty;

        var lastSegment = assetSegments[^1];
        var extensionIndex = lastSegment.LastIndexOf('.');
        if (extensionIndex > 0)
            assetSegments[^1] = lastSegment[..extensionIndex];

        return string.Join("/", assetSegments);
    }
}
