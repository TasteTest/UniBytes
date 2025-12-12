using Azure.Storage.Blobs.Models;

namespace backend.Services.Interfaces;

/// <summary>
/// Wrapper interface for Azure Blob Container operations to enable testability
/// </summary>
public interface IBlobContainerClientWrapper
{
    /// <summary>
    /// Upload a blob to the container
    /// </summary>
    Task UploadAsync(string blobName, Stream content, BlobHttpHeaders? headers = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Delete a blob if it exists
    /// </summary>
    Task<bool> DeleteIfExistsAsync(string blobName, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get the URI for a blob
    /// </summary>
    Uri GetBlobUri(string blobName);
    
    /// <summary>
    /// Whether the wrapper is properly configured
    /// </summary>
    bool IsConfigured { get; }
}
