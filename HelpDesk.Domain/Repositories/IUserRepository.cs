using HelpDesk.Domain.Entities;

namespace HelpDesk.Domain.Repositories;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken);
    Task<User> CreateAsync(User user, CancellationToken cancellationToken);
    Task UpdatePasswordHashAsync(Guid userId, string passwordHash, DateTimeOffset updatedAt, CancellationToken cancellationToken);
}
