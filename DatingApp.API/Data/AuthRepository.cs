using DatingApp.API.Models;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.API.Data;

public class AuthRepository(DataContext context) : IAuthRepository
{
    public async Task<User?> Login(string username, string password)
    {
        var user = await context.User.FirstOrDefaultAsync(x => x.Username == username);
        if (user == null || !PasswordHasher.Verify(password, user.PasswordHash, user.PasswordSalt))
            return null;
        return user;
    }

    public async Task<User> Register(User user, string password)
    {
        (user.PasswordHash, user.PasswordSalt) = PasswordHasher.Hash(password);
        context.User.Add(user);
        await context.SaveChangesAsync();
        return user;
    }

    public Task<bool> UserExists(string username) => context.User.AnyAsync(x => x.Username == username);
}
