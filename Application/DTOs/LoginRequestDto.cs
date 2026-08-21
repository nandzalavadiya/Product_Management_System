namespace CRN_Technical_Assessment.Application.DTOs;

/// <summary>DTO for login request.</summary>
public class LoginRequestDto
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
