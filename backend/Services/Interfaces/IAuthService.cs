using backend.Common;
using backend.DTOs.Request;
using backend.DTOs.Response;

namespace backend.Services.Interfaces;

/// <summary>
/// Authentication service interface
/// </summary>
public interface IAuthService
{
    Task<Result<AuthResponse>> AuthenticateWithGoogleAsync(GoogleAuthRequest request, CancellationToken cancellationToken = default);
}

