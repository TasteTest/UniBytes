using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using backend.Services.Interfaces;
using System.Diagnostics.CodeAnalysis;

namespace backend.Services.Wrappers;

/// <summary>
/// Wrapper around BlobContainerClient for testability
/// </summary>
[ExcludeFromCodeCoverage]
public class BlobContainerClientWrapper : IBlobContainerClientWrapper
{
    private readonly BlobContainerClient? _containerClient;
    private readonly ILogger<BlobContainerClientWrapper> _logger;

    public bool IsConfigured { get; }

    public BlobContainerClientWrapper(IConfiguration configuration, ILogger<BlobContainerClientWrapper> logger)
    {
        _logger = logger;
        
        var containerName = configuration["Azure:BlobStorage:ContainerName"]
                            ?? configuration["Azure:BlobStorage:Container"]
                            ?? configuration["AzureStorage:ContainerName"]
                            ?? configuration["BLOB_CONTAINER_NAME"]
                            ?? "menu-images";

        var accountName = configuration["Azure:BlobStorage:AccountName"]
                          ?? configuration["STORAGE_ACCOUNT_NAME"];

        // Try connection string first (for local dev with Account Key)
        var connectionString = configuration["Azure:BlobStorage:ConnectionString"]
                               ?? configuration["AzureStorage:ConnectionString"]
                               ?? configuration["AZURE_STORAGE_CONNECTION_STRING"];

        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            try
            {
                _logger.LogInformation("Attempting to configure blob storage with connection string. Container: {ContainerName}", containerName);
                var blobServiceClient = new BlobServiceClient(connectionString);
                _containerClient = blobServiceClient.GetBlobContainerClient(containerName);
                // Try to create container if it doesn't exist
                try
                {
                    _containerClient.CreateIfNotExists(PublicAccessType.Blob);
                    _logger.LogInformation("Container created or already exists");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Could not create container, it might already exist");
                }
                IsConfigured = true;
                _logger.LogInformation("Azure Blob Storage configured with connection string for container: {ContainerName}", containerName);
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to use connection string");
            }
        }
        else
        {
            _logger.LogWarning("No connection string found in configuration");
        }

        // Try SAS token
        var sasToken = configuration["Azure:BlobStorage:SasToken"]
                       ?? configuration["AZURE_STORAGE_SAS_TOKEN"];

        if (!string.IsNullOrWhiteSpace(sasToken) && !string.IsNullOrWhiteSpace(accountName))
        {
            try
            {
                sasToken = sasToken.TrimStart('?');
                var containerUri = new Uri($"https://{accountName}.blob.core.windows.net/{containerName}?{sasToken}");
                _containerClient = new BlobContainerClient(containerUri);
                IsConfigured = true;
                _logger.LogInformation("Azure Blob Storage configured with SAS token for container: {ContainerName}", containerName);
                return;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to use SAS token");
            }
        }

        // Fall back to Managed Identity / DefaultAzureCredential
        if (string.IsNullOrEmpty(accountName))
        {
            _logger.LogWarning("Azure Storage not configured. Image uploads will fail.");
            IsConfigured = false;
            return;
        }

        try
        {
            var blobServiceUri = new Uri($"https://{accountName}.blob.core.windows.net");
            var credential = new DefaultAzureCredential();
            var blobServiceClient = new BlobServiceClient(blobServiceUri, credential);
            _containerClient = blobServiceClient.GetBlobContainerClient(containerName);
            
            try
            {
                _containerClient.CreateIfNotExists(PublicAccessType.None);
            }
            catch (Azure.RequestFailedException ex) when (ex.ErrorCode == "PublicAccessNotPermitted")
            {
                _logger.LogInformation(ex, "Container exists or public access not permitted (this is expected)");
            }
            
            IsConfigured = true;
            _logger.LogInformation("Azure Blob Storage configured with Managed Identity for account: {AccountName}", accountName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to configure Azure Blob client using Managed Identity");
            IsConfigured = false;
        }
    }

    public async Task UploadAsync(string blobName, Stream content, BlobHttpHeaders? headers = null, CancellationToken cancellationToken = default)
    {
        if (_containerClient == null)
            throw new InvalidOperationException("Blob container client is not configured");
            
        var blobClient = _containerClient.GetBlobClient(blobName);
        
        // Try the simplest upload first - just content and headers, no options
        await blobClient.UploadAsync(content, headers, cancellationToken: cancellationToken);
    }

    public async Task<bool> DeleteIfExistsAsync(string blobName, CancellationToken cancellationToken = default)
    {
        if (_containerClient == null)
            throw new InvalidOperationException("Blob container client is not configured");
            
        var blobClient = _containerClient.GetBlobClient(blobName);
        var response = await blobClient.DeleteIfExistsAsync(cancellationToken: cancellationToken);
        return response.Value;
    }

    public Uri GetBlobUri(string blobName)
    {
        if (_containerClient == null)
            throw new InvalidOperationException("Blob container client is not configured");
            
        var blobClient = _containerClient.GetBlobClient(blobName);
        return blobClient.Uri;
    }
}
