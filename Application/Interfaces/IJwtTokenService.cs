using CRN_Technical_Assessment.Domain.Entities;

namespace CRN_Technical_Assessment.Application.Interfaces;

public interface IJwtTokenService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
    string HashRefreshToken(string refreshToken);
}
