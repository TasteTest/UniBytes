using Azure.Storage.Blobs.Models;
using backend.Common;
using backend.Services;
using backend.Services.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Formats.Jpeg;

namespace Backend.Tests.Services;

public class AzureBlobStorageServiceTests
{
    private readonly Mock<IBlobContainerClientWrapper> _mockContainerWrapper;
    private readonly Mock<ILogger<AzureBlobStorageService>> _mockLogger;
    private readonly AzureBlobStorageService _blobStorageService;

    public AzureBlobStorageServiceTests()
    {
        _mockContainerWrapper = new Mock<IBlobContainerClientWrapper>();
        _mockLogger = new Mock<ILogger<AzureBlobStorageService>>();
        
        // Default to configured state
        _mockContainerWrapper.Setup(x => x.IsConfigured).Returns(true);

        _blobStorageService = new AzureBlobStorageService(
            _mockContainerWrapper.Object,
            _mockLogger.Object);
    }

    // Helper method to create a minimal valid JPEG image for testing
    private static MemoryStream CreateTestImage(int width = 100, int height = 100)
    {
        using var image = new Image<Rgba32>(width, height);
        var stream = new MemoryStream();
        image.Save(stream, new JpegEncoder());
        stream.Position = 0;
        return stream;
    }

    #region UploadImageAsync Tests

    [Fact]
    public async Task UploadImageAsync_NotConfigured_ReturnsFailure()
    {
        // Arrange
        var mockWrapper = new Mock<IBlobContainerClientWrapper>();
        mockWrapper.Setup(x => x.IsConfigured).Returns(false);
        
        var service = new AzureBlobStorageService(mockWrapper.Object, _mockLogger.Object);
        using var stream = new MemoryStream();

        // Act
        var result = await service.UploadImageAsync(stream, "test.jpg");

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Azure Storage is not configured");
    }

    [Fact]
    public async Task UploadImageAsync_ValidStream_ReturnsSuccessWithUrl()
    {
        // Arrange
        using var stream = CreateTestImage();
        var fileName = "test.jpg";
        var expectedUri = new Uri("https://teststorage.blob.core.windows.net/container/guid_test.jpg");

        _mockContainerWrapper.Setup(x => x.UploadAsync(
            It.IsAny<string>(), 
            It.IsAny<Stream>(), 
            It.IsAny<BlobHttpHeaders>(), 
            It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockContainerWrapper.Setup(x => x.GetBlobUri(It.IsAny<string>()))
            .Returns(expectedUri);

        // Act
        var result = await _blobStorageService.UploadImageAsync(stream, fileName);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().Be(expectedUri.ToString());

        _mockContainerWrapper.Verify(x => x.UploadAsync(
            It.Is<string>(s => s.EndsWith("_test.jpg")),
            It.IsAny<Stream>(),
            It.Is<BlobHttpHeaders>(h => h.ContentType == "image/jpeg"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UploadImageAsync_ExceptionThrown_ReturnsFailure()
    {
        // Arrange
        using var stream = new MemoryStream(new byte[] { 1, 2, 3 });
        var fileName = "test.jpg";

        _mockContainerWrapper.Setup(x => x.UploadAsync(
            It.IsAny<string>(), 
            It.IsAny<Stream>(), 
            It.IsAny<BlobHttpHeaders>(), 
            It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Upload failed"));

        // Act
        var result = await _blobStorageService.UploadImageAsync(stream, fileName);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Failed to upload image");
    }

    [Fact]
    public async Task UploadImageAsync_GeneratesUniqueBlobName()
    {
        // Arrange
        using var stream = CreateTestImage();
        var fileName = "myimage.png";
        string? capturedBlobName = null;

        _mockContainerWrapper.Setup(x => x.UploadAsync(
            It.IsAny<string>(), 
            It.IsAny<Stream>(), 
            It.IsAny<BlobHttpHeaders>(), 
            It.IsAny<CancellationToken>()))
            .Callback<string, Stream, BlobHttpHeaders, CancellationToken>((name, _, _, _) => capturedBlobName = name)
            .Returns(Task.CompletedTask);

        _mockContainerWrapper.Setup(x => x.GetBlobUri(It.IsAny<string>()))
            .Returns(new Uri("https://test.blob.core.windows.net/container/blob"));

        // Act
        await _blobStorageService.UploadImageAsync(stream, fileName);

        // Assert
        capturedBlobName.Should().NotBeNull();
        capturedBlobName.Should().EndWith("_myimage.png");
        capturedBlobName.Should().MatchRegex(@"^[a-f0-9\-]{36}_myimage\.png$");
    }

    #endregion

    #region DeleteImageAsync Tests

    [Fact]
    public async Task DeleteImageAsync_NotConfigured_ReturnsFailure()
    {
        // Arrange
        var mockWrapper = new Mock<IBlobContainerClientWrapper>();
        mockWrapper.Setup(x => x.IsConfigured).Returns(false);
        
        var service = new AzureBlobStorageService(mockWrapper.Object, _mockLogger.Object);
        var blobUrl = "https://teststorage.blob.core.windows.net/container/test.jpg";

        // Act
        var result = await service.DeleteImageAsync(blobUrl);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Azure Storage is not configured");
    }

    [Fact]
    public async Task DeleteImageAsync_ValidUrl_ReturnsSuccess()
    {
        // Arrange
        var blobUrl = "https://teststorage.blob.core.windows.net/container/test-image.jpg";

        _mockContainerWrapper.Setup(x => x.DeleteIfExistsAsync(
            It.IsAny<string>(), 
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _blobStorageService.DeleteImageAsync(blobUrl);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();

        _mockContainerWrapper.Verify(x => x.DeleteIfExistsAsync(
            "test-image.jpg",
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteImageAsync_BlobDoesNotExist_StillReturnsSuccess()
    {
        // Arrange
        var blobUrl = "https://teststorage.blob.core.windows.net/container/nonexistent.jpg";

        _mockContainerWrapper.Setup(x => x.DeleteIfExistsAsync(
            It.IsAny<string>(), 
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(false); // Blob didn't exist

        // Act
        var result = await _blobStorageService.DeleteImageAsync(blobUrl);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteImageAsync_ExceptionThrown_ReturnsFailure()
    {
        // Arrange
        var blobUrl = "https://teststorage.blob.core.windows.net/container/test.jpg";

        _mockContainerWrapper.Setup(x => x.DeleteIfExistsAsync(
            It.IsAny<string>(), 
            It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Delete failed"));

        // Act
        var result = await _blobStorageService.DeleteImageAsync(blobUrl);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Failed to delete image");
    }

    [Fact]
    public async Task DeleteImageAsync_ExtractsBlobNameFromUrl()
    {
        // Arrange
        var blobUrl = "https://myaccount.blob.core.windows.net/mycontainer/subfolder/myblob.jpg";
        string? capturedBlobName = null;

        _mockContainerWrapper.Setup(x => x.DeleteIfExistsAsync(
            It.IsAny<string>(), 
            It.IsAny<CancellationToken>()))
            .Callback<string, CancellationToken>((name, _) => capturedBlobName = name)
            .ReturnsAsync(true);

        // Act
        await _blobStorageService.DeleteImageAsync(blobUrl);

        // Assert
        capturedBlobName.Should().Be("myblob.jpg");
    }

    [Fact]
    public async Task DeleteImageAsync_InvalidUrl_ReturnsFailure()
    {
        // Arrange
        var invalidUrl = "not-a-valid-url";

        // Act
        var result = await _blobStorageService.DeleteImageAsync(invalidUrl);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Failed to delete image");
    }

    #endregion

    #region CancellationToken Tests

    [Fact]
    public async Task UploadImageAsync_PassesCancellationToken()
    {
        // Arrange
        using var stream = CreateTestImage();
        using var cts = new CancellationTokenSource();
        var token = cts.Token;

        _mockContainerWrapper.Setup(x => x.UploadAsync(
            It.IsAny<string>(), 
            It.IsAny<Stream>(), 
            It.IsAny<BlobHttpHeaders>(), 
            token))
            .Returns(Task.CompletedTask);

        _mockContainerWrapper.Setup(x => x.GetBlobUri(It.IsAny<string>()))
            .Returns(new Uri("https://test.blob.core.windows.net/container/blob"));

        // Act
        await _blobStorageService.UploadImageAsync(stream, "test.jpg", token);

        // Assert
        _mockContainerWrapper.Verify(x => x.UploadAsync(
            It.IsAny<string>(),
            It.IsAny<Stream>(),
            It.IsAny<BlobHttpHeaders>(),
            token), Times.Once);
    }

    [Fact]
    public async Task DeleteImageAsync_PassesCancellationToken()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        var token = cts.Token;
        var blobUrl = "https://teststorage.blob.core.windows.net/container/test.jpg";

        _mockContainerWrapper.Setup(x => x.DeleteIfExistsAsync(It.IsAny<string>(), token))
            .ReturnsAsync(true);

        // Act
        await _blobStorageService.DeleteImageAsync(blobUrl, token);

        // Assert
        _mockContainerWrapper.Verify(x => x.DeleteIfExistsAsync(
            It.IsAny<string>(), token), Times.Once);
    }

    #endregion
}
