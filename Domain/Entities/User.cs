namespace CRN_Technical_Assessment.Domain.Entities;

public class User
{
    public int Id { get; set; }

    public string Username { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;

    public string? RefreshTokenHash { get; set; }

    public DateTime? RefreshTokenExpiry { get; set; }

    public DateTime CreatedOn { get; set; }
}
