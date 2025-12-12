using Azure.Storage.Blobs.Models;
using backend.Common;
using backend.Services.Interfaces;

namespace backend.Services;

public class AzureBlobStorageService : IBlobStorageService
{
    private readonly IBlobContainerClientWrapper _containerWrapper;
    private readonly ILogger<AzureBlobStorageService> _logger;

    public AzureBlobStorageService(IBlobContainerClientWrapper containerWrapper, ILogger<AzureBlobStorageService> logger)
    {
        _containerWrapper = containerWrapper;
        _logger = logger;
    }

    public async Task<Result<string>> UploadImageAsync(Stream imageStream, string fileName, CancellationToken ct = default)
    {
        if (!_containerWrapper.IsConfigured)
        {
            return Result<string>.Failure("Azure Storage is not configured");
        }
        
        try
        {
            var blobName = $"{Guid.NewGuid()}_{fileName}";
            await _containerWrapper.UploadAsync(blobName, imageStream, new BlobHttpHeaders { ContentType = "image/jpeg" }, ct);
            var uri = _containerWrapper.GetBlobUri(blobName);
            
            return Result<string>.Success(uri.ToString());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading image to blob storage");
            return Result<string>.Failure("Failed to upload image");
        }
    }

    public async Task<Result> DeleteImageAsync(string blobUrl, CancellationToken ct = default)
    {
        if (!_containerWrapper.IsConfigured)
        {
            return Result.Failure("Azure Storage is not configured");
        }
        
        try
        {
            var uri = new Uri(blobUrl);
            var blobName = uri.Segments[^1];
            await _containerWrapper.DeleteIfExistsAsync(blobName, ct);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting image from blob storage");
            return Result.Failure("Failed to delete image");
        }
    }
}