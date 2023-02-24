using System.Collections.Immutable;
using System.Globalization;
using Serilog;
using SleepingBearSystems.Tools.Persistence;
using SleepingBearSystems.Tools.Persistence.Postgres;

namespace SleepingBearSystems.ToolsSamples.ImplementFactStorePostgres;

internal static class Program
{
    public static int Main()
    {
        ILogger? logger = default;
        try
        {
            // create logger
            logger = new LoggerConfiguration()
                .WriteTo.Console(formatProvider: CultureInfo.InvariantCulture)
                .CreateLogger();

            var databaseUpgradeTasks = ImmutableList<DatabaseUpgradeTask>
                .Empty
                .Add(DatabaseUpgradeTask.FromEmbeddedResource(
                    new DatabaseVersion("fact_store", 1, new Guid("A94E91FC-2784-4E8D-89A7-FAC474E36C79")),
                    typeof(FactStore),
                    "Task001_AddFactsTable.sql"));

            using var guard = TemporaryDatabaseGuard
                .FromEnvironmentVariable(
                    logger,
                    "SBS_TEST_SERVER_POSTGRES")
                .UpgradeDatabase(databaseUpgradeTasks);

            logger.Information("Create fact store...");
            var factStore = new FactStore(guard.Database);

            logger.Information("Register fact types...");
            factStore.RegisterFact<UserCreatedFact>();
            factStore.RegisterFact<UserNameChangedFact>();
            factStore.RegisterFact<UserPasswordChangedFact>();
            factStore.RegisterFact<UserDeletedFact>();

            logger.Information("Create users...");
            var userId1 = new Guid("8631C28F-0696-4601-940C-78B8D3261BE6");
            var userId2 = new Guid("D657007E-AD50-4AE7-8572-0CDB62ECAA1F");
            var userId3 = new Guid("C8E93D87-DF3F-45A2-8533-D522CC8EC341");
            factStore.AppendFacts(
                "users",
                new IFact[]
                {
                    new UserCreatedFact(userId1, "john_grey", "password123"),
                    new UserCreatedFact(userId2, "jane_green", "password234"),
                    new UserCreatedFact(userId3, "tom_black", "password345")
                });

            var repository = new UserRepository(factStore);
            LogUsers(logger, repository);

            logger.Information("Update users...");
            factStore.AppendFacts(
                "users",
                new IFact[]
                {
                    new UserNameChangedFact(userId2, "jane_blue"),
                    new UserPasswordChangedFact(userId2, "123password"),
                    new UserDeletedFact(userId1)
                });
            LogUsers(logger, repository);

            logger.Information("Exiting...");

            return 0;
        }
        catch (Exception ex)
        {
            logger?.Error(ex, "An error occurred");
            return 1;
        }
    }

    private static void LogUsers(ILogger logger, UserRepository repository)
    {
        var users = repository.GetUsers();
        logger.Information("Count: {Count}", users.Count);
        foreach (var user in users) logger.Information("  {User}", user);
    }
}
