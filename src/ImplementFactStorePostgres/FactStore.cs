using System.Data;
using System.Text;
using Newtonsoft.Json;
using SleepingBearSystems.Tools.Infrastructure;
using SleepingBearSystems.Tools.Persistence;
using SleepingBearSystems.Tools.Persistence.Postgres;

namespace SleepingBearSystems.ToolsSamples.ImplementFactStorePostgres;

/// <summary>
///     Class for persisting facts in a database.
/// </summary>
public sealed class FactStore
{
    private static readonly Lazy<string> AppendEventSql = new(() =>
        typeof(FactStore)
            .GetStringEmbeddedResource("AppendFact.sql")
            .MatchOrThrow(() => throw new InvalidOperationException()));

    private static readonly Lazy<string> GetEventsSql = new(() =>
        typeof(FactStore)
            .GetStringEmbeddedResource("GetFacts.sql")
            .MatchOrThrow(() => throw new InvalidOperationException()));

    private readonly DatabaseInfo _databaseInfo;

    private readonly Dictionary<string, Type> _factRegistry = new();

    /// <summary>
    ///     Constructor.
    /// </summary>
    public FactStore(DatabaseInfo databaseInfo)
    {
        this._databaseInfo = databaseInfo ?? throw new ArgumentNullException(nameof(databaseInfo));
    }

    /// <summary>
    ///     Registers a fact type for deserialization.
    /// </summary>
    /// <exception cref="ArgumentNullException">Thrown if the type is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown if the fact type is already registered.</exception>
    public void RegisterFact<TFact>() where TFact : IFact
    {
        var type = typeof(TFact);
        if (this._factRegistry.TryGetValue(type.Name, out _))
        {
            throw new InvalidOperationException($"Fact is already registered: '{type.Name}'");
        }

        this._factRegistry.Add(type.Name, type);
    }

    /// <summary>
    ///     Persists a collection of facts to the database.
    /// </summary>
    /// <param name="streamId">The stream ID.</param>
    /// <param name="facts">The collection of facts.</param>
    /// <param name="cancellationToken">The cancellation toke. (optional)</param>
    /// <exception cref="ArgumentNullException">Thrown if the stream ID isn't null or empty.</exception>
    public async Task AppendFactsAsync(string streamId, IEnumerable<IFact>? facts,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(streamId))
        {
            throw new ArgumentNullException(nameof(streamId));
        }

        var validFacts = (facts ?? Enumerable.Empty<IFact>()).ToList();
        if (validFacts.Count == 0)
        {
            return;
        }

        var guard = await SqlConnectionGuard.CreateAsync(this._databaseInfo, cancellationToken);
        await using var command = guard.CreateCommand(AppendEventSql.Value);
        foreach (var fact in validFacts)
        {
            var type = fact.GetType().Name;
            var data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(fact));
            command.Parameters.Clear();
            command
                .AddParameter("@streamId", streamId, DbType.String)
                .AddParameter("@factType", type, DbType.String)
                .AddParameter("@factData", data, DbType.Binary);
            await command.ExecuteNonQueryAsync(cancellationToken);
        }
    }

    /// <summary>
    ///     Gets all the facts for a particular stream.
    /// </summary>
    /// <param name="streamId">The stream ID.</param>
    /// <param name="cancellationToken">The cancellation token. (optional)</param>
    /// <returns>A collection of fact instances.</returns>
    /// <exception cref="ArgumentNullException">Thrown if the stream ID isn't null or empty.</exception>
    /// <exception cref="InvalidOperationException">
    ///     Thrown if the fact isn't registered
    ///     or the fact can't be deserialized.
    /// </exception>
    public async Task<IEnumerable<IFact>> GetFactsAsync(string streamId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(streamId))
        {
            throw new ArgumentNullException(nameof(streamId));
        }

        await using var guard = await SqlConnectionGuard.CreateAsync(this._databaseInfo, cancellationToken);
        await using var command = guard.CreateCommand(GetEventsSql.Value);
        command.AddParameter("@streamId", streamId, DbType.String);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        var events = new List<IFact>();
        while (await reader.ReadAsync(cancellationToken))
        {
            var factType = reader.GetString(0);
            if (!this._factRegistry.TryGetValue(factType, out var type))
            {
                throw new InvalidOperationException($"Unknown type: '{factType}'");
            }

            var data = (byte[])reader[1];
            var json = Encoding.UTF8.GetString(data);
            var fact = JsonConvert.DeserializeObject(json, type) as IFact ??
                       throw new InvalidOperationException($"Unable to deserialize fact: '{factType}'");
            events.Add(fact);
        }

        return events;
    }
}
