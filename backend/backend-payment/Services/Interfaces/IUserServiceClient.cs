using backend_payment.Common;
using backend_payment.DTOs.Response;

namespace backend_payment.Services.Interfaces;

/// <summary>
/// Client for communicating with backend-user service
/// </summary>
public interface IUserServiceClient
{
    Task<Result<UserInfoResponse>> GetUserInfoAsync(string accessToken, string userEmail, CancellationToken cancellationToken = default);
}

