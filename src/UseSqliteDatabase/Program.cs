using System.Data.SQLite;
using System.Globalization;
using System.Reflection;
using Serilog;
using SleepingBearSystems.Tools.Common;
using SleepingBearSystems.Tools.Persistence;
using SleepingBearSystems.Tools.Persistence.Sqlite;

namespace SleepingBearSystems.ToolsSamples.UseSqliteDatabase;

internal static class Program
{
    public static int Main()
    {
        var logger = default(Serilog.Core.Logger);
        try
        {
            // create logger
            logger = new LoggerConfiguration()
                .WriteTo.Console(formatProvider: CultureInfo.InvariantCulture)
                .CreateLogger();

            logger.Information("Use SqliteDatabase...");
            ManuallyCreateSqliteDatabase(logger);

            logger.Information("Use TemporaryDatabaseGuard...");
            UseTemporaryDatabaseGuard(logger);

            logger.Information("Exiting...");

            return 0;
        }
        catch (Exception ex)
        {
            ex.FailFastIfCritical("SleepingBearSystems.ToolsSamples.UseSqliteDatabase");
            logger?.Error(ex, "An error occurred");
            return 1;
        }
    }

    /// <summary>
    ///     Creates an SQLite database, populates it with data, and then reads the data.
    /// </summary>
    private static void ManuallyCreateSqliteDatabase(Serilog.Core.Logger logger)
    {
        // create a database
        var path = Path.Combine(Path.GetTempPath(), $"sbs_sqlite_{Guid.NewGuid():N}.db");
        var builder = new SQLiteConnectionStringBuilder
        {
            DataSource = path,
            Version = 3,
            FailIfMissing = false
        };
        var connectionString = builder.ToString();
        var databaseInfo = DatabaseInfo.FromConnectionString(connectionString);
        logger.Information("Connection String: {ConnectionString}", connectionString);
        databaseInfo.CreateDatabase();

        // open a connection
        using (var connectionGuard = SqlConnectionGuard.Create(databaseInfo))
        {
            UseDatabase(logger, connectionGuard);
        }

        File.Delete(path);
    }

    /// <summary>
    ///     Creates an SQLite database, populates it with data, and then reads the data.  Uses the
    ///     <see cref="TemporaryDatabaseGuard" /> class to create a temporary SQLite database.
    /// </summary>
    private static void UseTemporaryDatabaseGuard(ILogger logger)
    {
        using var guard = TemporaryDatabaseGuard.Create();

        // open a connection
        using var connectionGuard = SqlConnectionGuard.Create(guard.DatabaseInfo);

        UseDatabase(logger, connectionGuard);
    }

    /// <summary>
    ///     Writes and reads information from the database using a <see cref="SqlConnectionGuard" /> instance.
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="connectionGuard"></param>
    private static void UseDatabase(ILogger logger, SqlConnectionGuard connectionGuard)
    {
        // populate database
        {
            logger.Information("Populating Database...");
            var assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var sqlPath = Path.Combine(assemblyPath!, "PopulateDatabase.sql");
            var commandText = File.ReadAllText(sqlPath);
            using var command = connectionGuard.CreateCommand(commandText, TimeSpan.FromSeconds(300));
            command.ExecuteNonQuery();
        }

        // read data
        {
            logger.Information("Reading Data...");
            using var command = connectionGuard.CreateCommand(
                "SELECT id, name, password FROM sbs_user_data",
                TimeSpan.FromSeconds(300));
            var users = command.ReadRecords(reader => new UserData(
                reader.GetInt32(0),
                reader.GetString(1),
                reader.GetString(2)));
            foreach (var user in users)
                logger.Information(
                    "Id: {UserId} Name: {UserName} Password: {UserPassword}",
                    user.Id,
                    user.Name,
                    user.Password);
        }
    }

    private sealed record UserData(int Id, string Name, string Password);
}
