using backend_menu.Services;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace Menu.Tests.Services;

public class AzureBlobStorageServiceTests
{
    // Note: AzureBlobStorageService creates a real BlobServiceClient in its constructor,
    // which makes unit testing difficult without a real connection string.
    // The service is properly mocked in controller tests where it's used.
    // For proper testing of this service, consider:
    // 1. Creating integration tests with Azure Storage Emulator/Azurite
    // 2. Refactoring the service to use dependency injection for BlobServiceClient
    
    [Fact(Skip = "Requires Azure Storage connection string or refactoring for dependency injection")]
    public void Constructor_WithValidConfiguration_CreatesInstance()
    {
        // This test is skipped because AzureBlobStorageService creates a real BlobServiceClient
        // in its constructor, which requires a valid Azure Storage connection string.
        // The service is adequately tested through integration tests or when mocked in other tests.
    }
}