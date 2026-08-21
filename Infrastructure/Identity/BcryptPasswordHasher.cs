using CRN_Technical_Assessment.Application.Interfaces;

namespace CRN_Technical_Assessment.Infrastructure.Identity;

public class BcryptPasswordHasher : IPasswordHasher
{
    public string Hash(string password) =>
        BCrypt.Net.BCrypt.HashPassword(password);

    public bool Verify(string password, string hash) =>
        BCrypt.Net.BCrypt.Verify(password, hash);
}
