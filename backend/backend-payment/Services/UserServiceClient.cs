using System.Text.Json;
using backend_payment.Common;
using backend_payment.DTOs.Response;
using backend_payment.Services.Interfaces;

namespace backend_payment.Services;

/// <summary>
/// Client for communicating with backend-user service
/// </summary>
public class UserServiceClient : IUserServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<UserServiceClient> _logger;

    public UserServiceClient(HttpClient httpClient, ILogger<UserServiceClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<Result<UserInfoResponse>> GetUserInfoAsync(
        string accessToken, 
        string userEmail, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "/api/auth/me");
            request.Headers.Add("Authorization", $"Bearer {accessToken}");
            request.Headers.Add("X-User-Email", userEmail);

            var response = await _httpClient.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogWarning("Failed to get user info from backend-user: {StatusCode} - {Content}", 
                    response.StatusCode, errorContent);
                return Result<UserInfoResponse>.Failure("Failed to authenticate user");
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var userInfo = JsonSerializer.Deserialize<UserInfoResponse>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (userInfo == null)
            {
                return Result<UserInfoResponse>.Failure("Invalid user info response");
            }

            return Result<UserInfoResponse>.Success(userInfo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling backend-user service");
            return Result<UserInfoResponse>.Failure($"Error communicating with user service: {ex.Message}");
        }
    }
}

