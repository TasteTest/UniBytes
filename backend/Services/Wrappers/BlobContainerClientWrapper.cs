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
                IsConfigured = true;
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
            IsConfigured = false;
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
