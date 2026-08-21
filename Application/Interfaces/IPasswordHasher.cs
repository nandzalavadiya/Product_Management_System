using CRN_Technical_Assessment.Domain.Entities;

namespace CRN_Technical_Assessment.Application.Interfaces;

/// <summary>
/// Service interface for password hashing and verification.
/// </summary>
public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string hash);
}
