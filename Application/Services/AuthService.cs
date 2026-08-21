using CRN_Technical_Assessment.Application.DTOs;
using CRN_Technical_Assessment.Application.Interfaces;
using CRN_Technical_Assessment.Domain.Exceptions;
using CRN_Technical_Assessment.Infrastructure.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CRN_Technical_Assessment.Application.Services;

public class AuthService : IAuthService
{
    private readonly ApplicationDbContext _context;
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        ApplicationDbContext context,
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtTokenService,
        IConfiguration configuration,
        ILogger<AuthService> logger)
    {
        _context = context;
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto dto, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Login attempt for user '{Username}'", dto.Username);

        var user = await _userRepository.GetByUsernameAsync(dto.Username, cancellationToken);

        if (user is null || !_passwordHasher.Verify(dto.Password, user.PasswordHash))
        {
            _logger.LogWarning("Failed login attempt for user '{Username}'", dto.Username);
            throw new UnauthorizedException("Invalid username or password.");
        }

        var accessToken = _jwtTokenService.GenerateAccessToken(user);
        var refreshToken = _jwtTokenService.GenerateRefreshToken();
        var refreshTokenHash = _jwtTokenService.HashRefreshToken(refreshToken);

        var refreshExpiryDays = int.Parse(_configuration["Jwt:RefreshTokenExpirationDays"] ?? "7");

        user.RefreshTokenHash = refreshTokenHash;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(refreshExpiryDays);

        _userRepository.Update(user);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("User '{Username}' logged in successfully", user.Username);

        var expirationMinutes = int.Parse(_configuration["Jwt:AccessTokenExpirationMinutes"] ?? "60");

        return new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            AccessTokenExpiry = DateTime.UtcNow.AddMinutes(expirationMinutes),
            Username = user.Username,
            Role = user.Role
        };
    }

    public async Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenRequestDto dto, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Refresh token attempt for user '{Username}'", dto.Username);

        var user = await _userRepository.GetByUsernameAsync(dto.Username, cancellationToken)
            ?? throw new UnauthorizedException("Invalid token.");

        if (user.RefreshTokenHash is null || user.RefreshTokenExpiry is null)
            throw new UnauthorizedException("No active refresh token.");

        if (DateTime.UtcNow > user.RefreshTokenExpiry)
            throw new UnauthorizedException("Refresh token has expired.");

        var incomingHash = _jwtTokenService.HashRefreshToken(dto.RefreshToken);
        if (incomingHash != user.RefreshTokenHash)
        {
            _logger.LogWarning("Invalid refresh token supplied for user '{Username}'", dto.Username);
            throw new UnauthorizedException("Invalid refresh token.");
        }

        var newAccessToken = _jwtTokenService.GenerateAccessToken(user);
        var newRefreshToken = _jwtTokenService.GenerateRefreshToken();
        var newRefreshTokenHash = _jwtTokenService.HashRefreshToken(newRefreshToken);

        var refreshExpiryDays = int.Parse(_configuration["Jwt:RefreshTokenExpirationDays"] ?? "7");

        user.RefreshTokenHash = newRefreshTokenHash;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(refreshExpiryDays);

        _userRepository.Update(user);
        await _context.SaveChangesAsync(cancellationToken);

        var expirationMinutes = int.Parse(_configuration["Jwt:AccessTokenExpirationMinutes"] ?? "60");

        return new AuthResponseDto
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken,
            AccessTokenExpiry = DateTime.UtcNow.AddMinutes(expirationMinutes),
            Username = user.Username,
            Role = user.Role
        };
    }

    public async Task RevokeTokenAsync(string username, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Revoking refresh token for user '{Username}'", username);

        var user = await _userRepository.GetByUsernameAsync(username, cancellationToken)
            ?? throw new NotFoundException("User", username);

        user.RefreshTokenHash = null;
        user.RefreshTokenExpiry = null;

        _userRepository.Update(user);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Refresh token revoked for user '{Username}'", username);
    }
}
