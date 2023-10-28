using System.Collections.Immutable;
using System.Globalization;
using Serilog;
using SleepingBearSystems.Tools.Common;
using SleepingBearSystems.Tools.Infrastructure;
using SleepingBearSystems.Tools.Persistence;
using SleepingBearSystems.Tools.Persistence.Postgres;

namespace SleepingBearSystems.ToolsSamples.ImplementFactStorePostgres;

internal static class Program
{
    public static async Task<int> Main()
    {
        ILogger? logger = default;
        try
        {
            // create logger
            logger = new LoggerConfiguration()
                .WriteTo.Console(formatProvider: CultureInfo.InvariantCulture)
                .CreateLogger();

            var sql = typeof(FactStore)
                .GetStringEmbeddedResource("Task001_AddFactsTable.sql")
                .MatchOrThrow(() => throw new InvalidOperationException());
            var databaseUpgradeTasks = ImmutableList<DatabaseUpgradeTask>
                .Empty
                .Add(DatabaseUpgradeTask.Create("1.0.0:A94E91FC27844E8D89A7FAC474E36C79", sql));

            await using var guard = await TemporaryDatabaseGuard.CreateAsync();
            await guard.DatabaseInfo.UpgradeDatabaseAsync(databaseUpgradeTasks);

            logger.Information("Create fact store...");
            var factStore = new FactStore(guard.DatabaseInfo);

            logger.Information("Register fact types...");
            factStore.RegisterFact<UserCreatedFact>();
            factStore.RegisterFact<UserNameChangedFact>();
            factStore.RegisterFact<UserPasswordChangedFact>();
            factStore.RegisterFact<UserDeletedFact>();

            logger.Information("Create users...");
            var userId1 = new Guid("8631C28F-0696-4601-940C-78B8D3261BE6");
            var userId2 = new Guid("D657007E-AD50-4AE7-8572-0CDB62ECAA1F");
            var userId3 = new Guid("C8E93D87-DF3F-45A2-8533-D522CC8EC341");
            await factStore.AppendFactsAsync(
                "users",
                new IFact[]
                {
                    new UserCreatedFact(userId1, "john_grey", "password123"),
                    new UserCreatedFact(userId2, "jane_green", "password234"),
                    new UserCreatedFact(userId3, "tom_black", "password345")
                });

            var repository = new UserRepository(factStore);
            await LogUsersAsync(logger, repository);

            logger.Information("Update users...");
            var facts = new IFact[]
            {
                new UserNameChangedFact(userId2, "jane_blue"),
                new UserPasswordChangedFact(userId2, "123password"),
                new UserDeletedFact(userId1)
            };
            foreach (var fact in facts) logger.Information("fact: {Fact}", fact);
            await factStore.AppendFactsAsync("users", facts);
            await LogUsersAsync(logger, repository);

            logger.Information("Exiting...");

            return 0;
        }
        catch (Exception ex)
        {
            ex.FailFastIfCritical("SleepingBearSystems.ToolsSamples.ImplementFactStorePostgres.Program");
            logger?.Error(ex, "An error occurred");
            return 1;
        }
    }

    private static async Task LogUsersAsync(ILogger logger, UserRepository repository)
    {
        var users = await repository.GetUsersAsync();
        logger.Information("Count: {Count}", users.Count);
        foreach (var user in users) logger.Information("  {User}", user);
    }
}
