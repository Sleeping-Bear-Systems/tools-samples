using System.IO;
using Autofac;
using SleepingBearSystems.Tools.Persistence.Sqlite;

namespace SleepingBearSystems.ToolsSamples.SqliteDatabase;

internal static class Program
{
    public static int Main(string[] args)
    {
        try
        {
            var path = Path.Combine(Path.GetTempPath(), "sbs_sqlite.db");
            using var container = new ContainerBuilder()
                .RegisterSqliteDatabase("sbs_sqlite")
                .Build();

            using var scope = container.BeginLifetimeScope();

            return 0;
        }
        catch (Exception e)
        {
            return 1;
        }
    }
}
