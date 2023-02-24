using System.Collections.Immutable;
using JetBrains.Annotations;

namespace SleepingBearSystems.ToolsSamples.ImplementFactStorePostgres;

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
        var dictionary = new Dictionary<Guid, User>();
        var facts = this._factStore.GetFacts("users");
        foreach (var fact in facts)
            switch (fact)
            {
                case UserCreatedFact created:
                    dictionary.Add(created.Id, new User(created.Id, created.Name, created.Password));
                    break;
                case UserNameChangedFact nameChanged:
                    dictionary[nameChanged.Id] = dictionary[nameChanged.Id] with
                    {
                        Name = nameChanged.Name
                    };
                    break;
                case UserPasswordChangedFact passwordChanged:
                    dictionary[passwordChanged.Id] = dictionary[passwordChanged.Id] with
                    {
                        Password = passwordChanged.Password
                    };
                    break;
                case UserDeletedFact userDeleted:
                    dictionary.Remove(userDeleted.Id);
                    break;
            }

        return dictionary.Values.ToImmutableList();
    }
}
