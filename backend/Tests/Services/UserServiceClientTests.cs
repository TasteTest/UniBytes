using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using backend.Common;
using backend.DTOs.Response;
using backend.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Xunit;

namespace Backend.Tests.Services;

public class UserServiceClientTests
{
    private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
    private readonly HttpClient _httpClient;
    private readonly Mock<ILogger<UserServiceClient>> _mockLogger;
    private readonly UserServiceClient _userServiceClient;

    public UserServiceClientTests()
    {
        _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_mockHttpMessageHandler.Object)
        {
            BaseAddress = new Uri("http://localhost:5000")
        };
        _mockLogger = new Mock<ILogger<UserServiceClient>>();

        _userServiceClient = new UserServiceClient(_httpClient, _mockLogger.Object);
    }

    [Fact]
    public async Task GetUserInfoAsync_ValidRequest_ReturnsSuccess()
    {
        // Arrange
        var accessToken = "valid_token";
        var userEmail = "test@example.com";
        var userId = Guid.NewGuid();
        var userInfo = new UserInfoResponse
        {
            Id = userId.ToString(),
            Email = userEmail,
            FirstName = "John",
            LastName = "Doe"
        };

        var responseContent = JsonSerializer.Serialize(userInfo);
        var httpResponseMessage = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(responseContent, Encoding.UTF8, "application/json")
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Get &&
                    req.RequestUri!.ToString().Contains("/api/auth/me") &&
                    req.Headers.Authorization!.Scheme == "Bearer" &&
                    req.Headers.Authorization.Parameter == accessToken),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponseMessage);

        // Act
        var result = await _userServiceClient.GetUserInfoAsync(accessToken, userEmail);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Email.Should().Be(userEmail);
        result.Data.FirstName.Should().Be("John");
    }

    [Fact]
    public async Task GetUserInfoAsync_UnsuccessfulResponse_ReturnsFailure()
    {
        // Arrange
        var accessToken = "invalid_token";
        var userEmail = "test@example.com";

        var httpResponseMessage = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.Unauthorized,
            Content = new StringContent("Unauthorized", Encoding.UTF8, "application/json")
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponseMessage);

        // Act
        var result = await _userServiceClient.GetUserInfoAsync(accessToken, userEmail);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Failed to authenticate user");
    }

    [Fact]
    public async Task GetUserInfoAsync_InvalidJsonResponse_ReturnsFailure()
    {
        // Arrange
        var accessToken = "valid_token";
        var userEmail = "test@example.com";

        var httpResponseMessage = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent("invalid json", Encoding.UTF8, "application/json")
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponseMessage);

        // Act
        var result = await _userServiceClient.GetUserInfoAsync(accessToken, userEmail);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Error communicating with user service");
    }

    [Fact]
    public async Task GetUserInfoAsync_NullResponse_ReturnsFailure()
    {
        // Arrange
        var accessToken = "valid_token";
        var userEmail = "test@example.com";

        var httpResponseMessage = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent("null", Encoding.UTF8, "application/json")
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(httpResponseMessage);

        // Act
        var result = await _userServiceClient.GetUserInfoAsync(accessToken, userEmail);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Invalid user info response");
    }

    [Fact]
    public async Task GetUserInfoAsync_HttpException_ReturnsFailure()
    {
        // Arrange
        var accessToken = "valid_token";
        var userEmail = "test@example.com";

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Network error"));

        // Act
        var result = await _userServiceClient.GetUserInfoAsync(accessToken, userEmail);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Error communicating with user service");
        result.Error.Should().Contain("Network error");
    }

    [Fact]
    public async Task GetUserInfoAsync_SetsCorrectHeaders()
    {
        // Arrange
        var accessToken = "test_token";
        var userEmail = "user@example.com";
        HttpRequestMessage? capturedRequest = null;

        var httpResponseMessage = new HttpResponseMessage
        {
            StatusCode = HttpStatusCode.OK,
            Content = new StringContent(JsonSerializer.Serialize(new UserInfoResponse
            {
                Id = Guid.NewGuid().ToString(),
                Email = userEmail
            }), Encoding.UTF8, "application/json")
        };

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .Callback<HttpRequestMessage, CancellationToken>((req, ct) => capturedRequest = req)
            .ReturnsAsync(httpResponseMessage);

        // Act
        await _userServiceClient.GetUserInfoAsync(accessToken, userEmail);

        // Assert
        capturedRequest.Should().NotBeNull();
        capturedRequest!.Headers.Authorization.Should().NotBeNull();
        capturedRequest.Headers.Authorization!.Scheme.Should().Be("Bearer");
        capturedRequest.Headers.Authorization.Parameter.Should().Be(accessToken);
        capturedRequest.Headers.GetValues("X-User-Email").First().Should().Be(userEmail);
    }

    [Fact]
    public async Task GetUserInfoAsync_TaskCancelled_ReturnsFailure()
    {
        // Arrange
        var accessToken = "valid_token";
        var userEmail = "test@example.com";

        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new TaskCanceledException("Request timeout"));

        // Act
        var result = await _userServiceClient.GetUserInfoAsync(accessToken, userEmail);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("Error communicating with user service");
    }
}

