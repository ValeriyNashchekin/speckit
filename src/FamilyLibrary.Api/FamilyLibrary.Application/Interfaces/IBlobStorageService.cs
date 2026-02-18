namespace FamilyLibrary.Application.Interfaces;

/// <summary>
/// Interface for blob storage operations.
/// </summary>
public interface IBlobStorageService
{
    Task<string> UploadAsync(string container, string blobName, Stream content, CancellationToken cancellationToken = default);
    Task<Stream> DownloadAsync(string container, string blobName, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(string container, string blobName, CancellationToken cancellationToken = default);
    Task DeleteAsync(string container, string blobName, CancellationToken cancellationToken = default);
    Task<string> GetSasUrlAsync(string container, string blobName, TimeSpan expiration, CancellationToken cancellationToken = default);
}
