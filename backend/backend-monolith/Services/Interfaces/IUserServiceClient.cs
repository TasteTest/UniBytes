using backend_monolith.Common;
using backend_monolith.DTOs.Response;

namespace backend_monolith.Services.Interfaces;

/// <summary>
/// Client for communicating with backend-user service
/// </summary>
public interface IUserServiceClient
{
    Task<Result<UserInfoResponse>> GetUserInfoAsync(string accessToken, string userEmail, CancellationToken cancellationToken = default);
}

