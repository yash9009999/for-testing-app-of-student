using api.Models;

namespace api.Services.Required;

public interface IUserRepository
{
    User? GetById(long id);

    User? GetByExternalUserId(string externalUserId);

    User Create(User user);

    /// <summary>SSD: bool return makes “silent no-op” updates observable at the service layer.</summary>
    bool Update(User user);
}
