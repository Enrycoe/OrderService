using Microsoft.EntityFrameworkCore;
using OrderService.Domain.Abstractions;
using OrderService.Domain.Entities;
using OrderService.Infrastructure.Context;

namespace OrderService.Infrastructure.Repositories;

public class UserRepository(AppDbContext context) : IUserRepository
{
    public async Task<User?> GetByUsernameAsync(string username, CancellationToken ct)
    {
        var user = await context.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Username == username,
                                                                          ct);
        return user;
    }

    public async Task AddAsync(User user, CancellationToken ct)
    {
        await context.Users.AddAsync(user, ct);
        await context.SaveChangesAsync(ct);
    }
}
