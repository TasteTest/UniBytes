using Azure.Storage.Blobs.Models;
using backend.Common;
using backend.Services.Interfaces;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;

namespace backend.Services;

public class AzureBlobStorageService : IBlobStorageService
{
    private readonly IBlobContainerClientWrapper _containerWrapper;
    private readonly ILogger<AzureBlobStorageService> _logger;
    private const int ThumbnailWidth = 400;
    private const int JpegQuality = 85;

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
            
            // Generate thumbnail
            using var image = await Image.LoadAsync(imageStream, ct);
            
            // Resize to thumbnail width while maintaining aspect ratio
            image.Mutate(x => x.Resize(new ResizeOptions
            {
                Size = new Size(ThumbnailWidth, 0), // Height will be calculated automatically
                Mode = ResizeMode.Max
            }));
            
            // Upload thumbnail to blob storage
            using var thumbnailStream = new MemoryStream();
            await image.SaveAsync(thumbnailStream, new JpegEncoder { Quality = JpegQuality }, ct);
            thumbnailStream.Position = 0;
            
            await _containerWrapper.UploadAsync(blobName, thumbnailStream, new BlobHttpHeaders { ContentType = "image/jpeg" }, ct);
            var uri = _containerWrapper.GetBlobUri(blobName);
            
            _logger.LogInformation("Uploaded thumbnail for {FileName} with size {Width}x{Height}", 
                fileName, image.Width, image.Height);
            
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