using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using backend.Common;

namespace backend.Services;

public class AzureBlobStorageService : IBlobStorageService
{
    private readonly BlobContainerClient? _containerClient;
    private readonly ILogger<AzureBlobStorageService> _logger;
    private readonly bool _isConfigured;

    public AzureBlobStorageService(IConfiguration configuration, ILogger<AzureBlobStorageService> logger)
    {
        _logger = logger;
        var connectionString = configuration["AzureStorage:ConnectionString"];
        
        if (string.IsNullOrEmpty(connectionString))
        {
            _logger.LogWarning("Azure Storage connection string not configured. Image uploads will fail.");
            _isConfigured = false;
            return;
        }
        
        var containerName = configuration["AzureStorage:ContainerName"] ?? "menu-images";
        
        var blobServiceClient = new BlobServiceClient(connectionString);
        _containerClient = blobServiceClient.GetBlobContainerClient(containerName);
        _containerClient.CreateIfNotExists(PublicAccessType.Blob);
        _isConfigured = true;
    }

    public async Task<Result<string>> UploadImageAsync(Stream imageStream, string fileName, CancellationToken ct = default)
    {
        if (!_isConfigured || _containerClient == null)
        {
            return Result<string>.Failure("Azure Storage is not configured");
        }
        
        try
        {
            var blobName = $"{Guid.NewGuid()}_{fileName}";
            var blobClient = _containerClient.GetBlobClient(blobName);
            
            await blobClient.UploadAsync(imageStream, new BlobHttpHeaders { ContentType = "image/jpeg" }, cancellationToken: ct);
            
            return Result<string>.Success(blobClient.Uri.ToString());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading image to blob storage");
            return Result<string>.Failure("Failed to upload image");
        }
    }

    public async Task<Result> DeleteImageAsync(string blobUrl, CancellationToken ct = default)
    {
        if (!_isConfigured || _containerClient == null)
        {
            return Result.Failure("Azure Storage is not configured");
        }
        
        try
        {
            var uri = new Uri(blobUrl);
            var blobName = uri.Segments[^1];
            var blobClient = _containerClient.GetBlobClient(blobName);
            await blobClient.DeleteIfExistsAsync(cancellationToken: ct);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting image from blob storage");
            return Result.Failure("Failed to delete image");
        }
    }
}