using System.Collections.Immutable;
using JetBrains.Annotations;

namespace SleepingBearSystems.ToolsSamples.ImplementFactStoreSqlite;

/// <summary>
///     User record.
/// </summary>
/// <param name="Id">The user's ID.</param>
/// <param name="Name">The user's name.</param>
/// <param name="Password">The user's password.</param>
[UsedImplicitly]
public record User(Guid Id, string Name, string Password);

/// <summary>
///     User created fact.
/// </summary>
/// <param name="Id">The user's ID.</param>
/// <param name="Name">The user's name.</param>
/// <param name="Password">The user's password.</param>
public record UserCreatedFact(Guid Id, string Name, string Password) : IFact;

/// <summary>
///     User name changed fact.
/// </summary>
/// <param name="Id">The user's ID.</param>
/// <param name="Name">The user's name.</param>
public record UserNameChangedFact(Guid Id, string Name) : IFact;

/// <summary>
///     User password changed fact.
/// </summary>
/// <param name="Id">The user's ID.</param>
/// <param name="Password">The user's password.</param>
public record UserPasswordChangedFact(Guid Id, string Password) : IFact;

/// <summary>
///     User deleted fact.
/// </summary>
/// <param name="Id">The user's ID.</param>
public record UserDeletedFact(Guid Id) : IFact;

/// <summary>
///     User repository.
/// </summary>
public sealed class UserRepository
{
    private readonly FactStore _factStore;

    /// <summary>
    ///     Constructor.
    /// </summary>
    /// <param name="factStore">The fact store service.</param>
    /// <exception cref="ArgumentNullException">Thrown if the fact store service is null.</exception>
    public UserRepository(FactStore factStore)
    {
        this._factStore = factStore ?? throw new ArgumentNullException(nameof(factStore));
    }

    /// <summary>
    ///     Get the users from the fact store.
    /// </summary>
    /// <returns>A collection of users.</returns>
    public ImmutableList<User> GetUsers()
    {
        var users = this._factStore.GetFacts("users");
        return users
            .Aggregate(
                ImmutableDictionary<Guid, User>.Empty,
                (localUsers, fact) => fact switch
                {
                    UserCreatedFact created => localUsers.Add(created.Id,
                        new User(created.Id, created.Name, created.Password)),
                    UserNameChangedFact nameChanged => localUsers.SetItem(nameChanged.Id,
                        localUsers[nameChanged.Id] with { Name = nameChanged.Name }),
                    UserPasswordChangedFact passwordChanged => localUsers.SetItem(passwordChanged.Id,
                        localUsers[passwordChanged.Id] with { Password = passwordChanged.Password }),
                    UserDeletedFact userDeleted => localUsers.Remove(userDeleted.Id),
                    _ => localUsers
                })
            .Values
            .ToImmutableList();
    }
}
