using Azure.Identity;
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
        // Try connection string first (legacy / env AZURE_STORAGE_CONNECTION_STRING)
        var connectionString = configuration["Azure:BlobStorage:ConnectionString"]
                               ?? configuration["AzureStorage:ConnectionString"]
                               ?? configuration["AZURE_STORAGE_CONNECTION_STRING"];

        var containerName = configuration["Azure:BlobStorage:ContainerName"]
                            ?? configuration["Azure:BlobStorage:Container"]
                            ?? configuration["AzureStorage:ContainerName"]
                            ?? configuration["BLOB_CONTAINER_NAME"]
                            ?? "menu-images";

        // Only use connection string if it's not empty and contains valid format
        if (!string.IsNullOrWhiteSpace(connectionString) && connectionString.Contains("AccountName="))
        {
            try
            {
                var blobServiceClient = new BlobServiceClient(connectionString);
                _containerClient = blobServiceClient.GetBlobContainerClient(containerName);
                _containerClient.CreateIfNotExists(PublicAccessType.None);
                _isConfigured = true;
                _logger.LogInformation("Azure Blob Storage configured with connection string");
                return;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to use connection string, falling back to Managed Identity");
            }
        }

        // Fall back to Managed Identity / DefaultAzureCredential
        var accountName = configuration["Azure:BlobStorage:AccountName"]
                          ?? configuration["STORAGE_ACCOUNT_NAME"];

        if (string.IsNullOrEmpty(accountName))
        {
            _logger.LogWarning("Azure Storage not configured (no connection string or account name). Image uploads will fail.");
            _isConfigured = false;
            return;
        }

        try
        {
            var blobServiceUri = new Uri($"https://{accountName}.blob.core.windows.net");
            var credential = new DefaultAzureCredential();
            var blobServiceClient = new BlobServiceClient(blobServiceUri, credential);
            _containerClient = blobServiceClient.GetBlobContainerClient(containerName);
            
            // Try to create container with private access (no public access)
            try
            {
                _containerClient.CreateIfNotExists(PublicAccessType.None);
            }
            catch (Azure.RequestFailedException ex) when (ex.ErrorCode == "PublicAccessNotPermitted")
            {
                // Container might already exist, or public access is disabled - that's fine
                _logger.LogInformation("Container exists or public access not permitted (this is expected)");
            }
            
            _isConfigured = true;
            _logger.LogInformation("Azure Blob Storage configured with Managed Identity for account: {AccountName}", accountName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to configure Azure Blob client using Managed Identity");
            _isConfigured = false;
        }
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