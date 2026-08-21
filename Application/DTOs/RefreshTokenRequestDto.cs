namespace CRN_Technical_Assessment.Application.DTOs;

/// <summary>DTO for requesting a token refresh.</summary>
public class RefreshTokenRequestDto
{
    public string Username { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
}
