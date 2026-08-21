using CRN_Technical_Assessment.Application.DTOs;

namespace CRN_Technical_Assessment.Application.Interfaces;

/// <summary>
/// Service interface for authentication operations.
/// </summary>
public interface IAuthService
{
    Task<AuthResponseDto> LoginAsync(LoginRequestDto dto, CancellationToken cancellationToken = default);
    Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenRequestDto dto, CancellationToken cancellationToken = default);
    Task RevokeTokenAsync(string username, CancellationToken cancellationToken = default);
}
