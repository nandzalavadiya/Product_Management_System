using CRN_Technical_Assessment.Domain.Entities;

namespace CRN_Technical_Assessment.Application.Interfaces;

/// <summary>
/// Repository interface for User data access (authentication).
/// </summary>
public interface IUserRepository
{
    Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);
    Task<bool> ExistsByUsernameAsync(string username, CancellationToken cancellationToken = default);
    Task AddAsync(User user, CancellationToken cancellationToken = default);
    void Update(User user);
}
