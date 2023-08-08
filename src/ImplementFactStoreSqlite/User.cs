using System;
using System.Collections.Immutable;
using System.Linq;
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
        return this._factStore.GetFacts("users")
            .Aggregate(
                ImmutableDictionary<Guid, User>.Empty,
                (users, fact) => fact switch
                {
                    UserCreatedFact created => users.Add(created.Id,
                        new User(created.Id, created.Name, created.Password)),
                    UserNameChangedFact nameChanged => users.SetItem(nameChanged.Id,
                        users[nameChanged.Id] with { Name = nameChanged.Name }),
                    UserPasswordChangedFact passwordChanged => users.SetItem(passwordChanged.Id,
                        users[passwordChanged.Id] with { Password = passwordChanged.Password }),
                    UserDeletedFact userDeleted => users.Remove(userDeleted.Id),
                    _ => users
                })
            .Values
            .ToImmutableList();
    }
}
