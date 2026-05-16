using AuthServer.Models;

namespace AuthServer.Services;

public interface IUserService
{
    AuthUser? GetByUsername(string username);
    AuthUser? GetByEmail(string email);
    AuthUser? GetById(string id);
    AuthUser Create(string username, string email, string fullName, string password);
    bool VerifyPassword(AuthUser user, string password);
}
