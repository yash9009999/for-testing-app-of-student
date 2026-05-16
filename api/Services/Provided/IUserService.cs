using api.Dtos;

namespace api.Services.Provided;

public interface IUserService
{
    /// <summary>SSD: never returns null after auto-provision — throws <see cref="ArgumentException"/> for invalid external ids.</summary>
    UserDTO GetUserById(string externalUserId);

    /// <summary>SSD: profile-only fields — immutable identity columns cannot be overwritten via this contract.</summary>
    UserDTO UpdateUser(string externalUserId, UserProfileUpdateRequest request);
}
