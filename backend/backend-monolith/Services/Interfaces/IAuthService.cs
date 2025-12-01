using backend_monolith.Common;
using backend_monolith.DTOs.Request;
using backend_monolith.DTOs.Response;

namespace backend_monolith.Services.Interfaces;

/// <summary>
/// Authentication service interface
/// </summary>
public interface IAuthService
{
    Task<Result<AuthResponse>> AuthenticateWithGoogleAsync(GoogleAuthRequest request, CancellationToken cancellationToken = default);
}

