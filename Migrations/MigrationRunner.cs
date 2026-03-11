using DbUp;
using System.Reflection;

namespace NavetraERP.Migrations;

public static class MigrationRunner
{
    public static void Run(string connectionString)
    {
        EnsureDatabase.For.SqlDatabase(connectionString);

        var upgrader = DeployChanges.To
            .SqlDatabase(connectionString)
            .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly())
            .WithTransactionPerScript()
            .WithVariablesDisabled()
            .LogToConsole()
            .Build();

        var result = upgrader.PerformUpgrade();

        if (!result.Successful)
        {
            throw new Exception($"Adatbázis hiba: {result.Error}");
        }
    }
}
