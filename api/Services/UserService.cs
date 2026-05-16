using System.Text.RegularExpressions;
using api.Dtos;
using api.Models;
using api.Services.Required;
using api.Services.Provided;

namespace api.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IDateTimeProvider _dateTimeProvider;

    public UserService(IUserRepository userRepository, IDateTimeProvider dateTimeProvider)
    {
        _userRepository = userRepository;
        _dateTimeProvider = dateTimeProvider;
    }

    public UserDTO GetUserById(string externalUserId)
    {
        if (string.IsNullOrWhiteSpace(externalUserId) || externalUserId.Length > 128)
            throw new ArgumentException("Invalid external user id.", nameof(externalUserId));

        var user = _userRepository.GetByExternalUserId(externalUserId);

        if (user == null)
            user = AutoCreateUser(externalUserId);

        return MapToDto(user);
    }

    public UserDTO UpdateUser(string externalUserId, UserProfileUpdateRequest request)
    {
        if (string.IsNullOrWhiteSpace(externalUserId))
            throw new ArgumentException("Invalid external user id.", nameof(externalUserId));

        var user = _userRepository.GetByExternalUserId(externalUserId)
            ?? throw new KeyNotFoundException();

        user.Username = request.Username;
        user.Email = request.Email;
        user.FullName = request.FullName;
        user.FavouriteFlavour = request.FavouriteFlavour;

        if (!_userRepository.Update(user))
            throw new InvalidOperationException("User profile was not updated.");

        return MapToDto(user);
    }

    /// <summary>SSD: synthetic email local-part is RFC-ish safe — strips characters that would produce an invalid address from raw SSO subjects.</summary>
    private User AutoCreateUser(string externalUserId)
    {
        var safeLocal = Regex.Replace(externalUserId, @"[^a-zA-Z0-9._-]", "_");
        if (string.IsNullOrEmpty(safeLocal))
            safeLocal = "user";

        var newUser = new User
        {
            ExternalUserId = externalUserId,
            Username = $"user_{safeLocal[..Math.Min(32, safeLocal.Length)]}",
            Email = $"{safeLocal[..Math.Min(64, safeLocal.Length)]}@scoops2go.local",
            FullName = "Scoops2Go User",
            FavouriteFlavour = string.Empty,
            CreatedAt = _dateTimeProvider.UtcNow.UtcDateTime,
            Orders = new List<Order>()
        };

        return _userRepository.Create(newUser);
    }

    private static UserDTO MapToDto(User user)
    {
        return new UserDTO(
            user.Id,
            user.Username,
            user.Email,
            user.FullName,
            user.FavouriteFlavour,
            user.CreatedAt
        );
    }
}
