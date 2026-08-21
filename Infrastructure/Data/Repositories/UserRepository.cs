using CRN_Technical_Assessment.Application.Interfaces;
using CRN_Technical_Assessment.Domain.Entities;
using CRN_Technical_Assessment.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CRN_Technical_Assessment.Infrastructure.Data.Repositories;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;

    public UserRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default) =>
        await _context.Users
            .FirstOrDefaultAsync(u => u.Username == username, cancellationToken);

    public async Task<bool> ExistsByUsernameAsync(string username, CancellationToken cancellationToken = default) =>
        await _context.Users.AnyAsync(u => u.Username == username, cancellationToken);

    public async Task AddAsync(User user, CancellationToken cancellationToken = default) =>
        await _context.Users.AddAsync(user, cancellationToken);

    public void Update(User user) =>
        _context.Users.Update(user);
}
