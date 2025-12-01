using backend.Common;
using backend.DTOs.Response;

namespace backend.Services.Interfaces;

/// <summary>
/// Client for communicating with backend-user service
/// </summary>
public interface IUserServiceClient
{
    Task<Result<UserInfoResponse>> GetUserInfoAsync(string accessToken, string userEmail, CancellationToken cancellationToken = default);
}

