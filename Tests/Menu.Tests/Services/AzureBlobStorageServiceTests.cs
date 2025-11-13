using backend_menu.Services;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace Menu.Tests.Services;

public class AzureBlobStorageServiceTests
{
    private readonly BlobServiceClient _mockBlobServiceClient;
    private readonly BlobContainerClient _mockContainerClient;
    private readonly BlobClient _mockBlobClient;
    private readonly IConfiguration _mockConfiguration;
    private readonly ILogger<AzureBlobStorageService> _mockLogger;
    private readonly AzureBlobStorageService _service;

    public AzureBlobStorageServiceTests()
    {
        _mockBlobServiceClient = Substitute.For<BlobServiceClient>();
        _mockContainerClient = Substitute.For<BlobContainerClient>();
        _mockBlobClient = Substitute.For<BlobClient>();
        _mockConfiguration = Substitute.For<IConfiguration>();
        _mockLogger = Substitute.For<ILogger<AzureBlobStorageService>>();

        _mockConfiguration["AzureBlobStorage:ContainerName"].Returns("menu-images");
        _mockConfiguration["AzureBlobStorage:BaseUrl"].Returns("https://test.blob.core.windows.net");

        _mockBlobServiceClient.GetBlobContainerClient(Arg.Any<string>())
            .Returns(_mockContainerClient);
        _mockContainerClient.GetBlobClient(Arg.Any<string>())
            .Returns(_mockBlobClient);

        _service = new AzureBlobStorageService(_mockConfiguration, _mockLogger);
    }

    [Fact]
    public async Task UploadImageAsync_WithValidStream_ReturnsSuccessWithUrl()
    {
        // Arrange
        var stream = new MemoryStream([1, 2, 3, 4, 5]);
        var fileName = "test-image.jpg";
        var expectedUrl = $"https://test.blob.core.windows.net/menu-images/{fileName}";

        var mockResponse = Substitute.For<Response<BlobContentInfo>>();
        _mockBlobClient.UploadAsync(
            Arg.Any<Stream>(),
            Arg.Any<BlobHttpHeaders>(),
            cancellationToken: Arg.Any<CancellationToken>())
            .Returns(mockResponse);

        _mockBlobClient.Uri.Returns(new Uri(expectedUrl));

        // Act
        var result = await _service.UploadImageAsync(stream, fileName, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(expectedUrl);
    }

    [Fact]
    public async Task UploadImageAsync_WhenUploadFails_ReturnsFailure()
    {
        // Arrange
        var stream = new MemoryStream([1, 2, 3]);
        var fileName = "fail.jpg";

        _mockBlobClient.UploadAsync(
            Arg.Any<Stream>(),
            Arg.Any<BlobHttpHeaders>(),
            cancellationToken: Arg.Any<CancellationToken>())
            .Throws(new RequestFailedException("Upload failed"));

        // Act
        var result = await _service.UploadImageAsync(stream, fileName, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Upload failed");
    }

    [Fact]
    public async Task DeleteImageAsync_WithValidUrl_ReturnsSuccess()
    {
        // Arrange
        var imageUrl = "https://test.blob.core.windows.net/menu-images/test-image.jpg";
        
        var mockResponse = Substitute.For<Response>();
        _mockBlobClient.DeleteIfExistsAsync(cancellationToken: Arg.Any<CancellationToken>())
            .Returns(Response.FromValue(true, mockResponse));

        // Act
        var result = await _service.DeleteImageAsync(imageUrl, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _mockBlobClient.Received(1).DeleteIfExistsAsync(cancellationToken: Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteImageAsync_WhenDeletionFails_ReturnsFailure()
    {
        // Arrange
        var imageUrl = "https://test.blob.core.windows.net/menu-images/test.jpg";

        _mockBlobClient.DeleteIfExistsAsync(cancellationToken: Arg.Any<CancellationToken>())
            .Throws(new RequestFailedException("Delete failed"));

        // Act
        var result = await _service.DeleteImageAsync(imageUrl, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Delete failed");
    }
}