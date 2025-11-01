using backend_user.Common;
using backend_user.DTOs.Request;
using backend_user.DTOs.Response;

namespace backend_user.Services.Interfaces;

/// <summary>
/// Authentication service interface
/// </summary>
public interface IAuthService
{
    Task<Result<AuthResponse>> AuthenticateWithGoogleAsync(GoogleAuthRequest request, CancellationToken cancellationToken = default);
}

