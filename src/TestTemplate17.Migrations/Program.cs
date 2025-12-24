using System;
using System.IO;
using DbUp;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace TestTemplate17.Migrations;

public class Program
{
    public static int Main(string[] args)
    {
        var connectionString = string.Empty;
        var dbUser = string.Empty;
        var dbPassword = string.Empty;
        var scriptsPath = string.Empty;
        var sqlUsersGroupName = string.Empty;

        var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
            ?? "Production";
        Console.WriteLine($"Environment: {env}.");
        var builder = new ConfigurationBuilder()
            .AddJsonFile($"appsettings.json", true, true)
            .AddJsonFile($"appsettings.{env}.json", true, true)
            .AddEnvironmentVariables();

        var config = builder.Build();
        InitializeParameters();
        var connectionStringBuilderTestTemplate17 = new SqlConnectionStringBuilder(connectionString);
        if (env.Equals("Development"))
        {
            connectionStringBuilderTestTemplate17.UserID = dbUser;
            connectionStringBuilderTestTemplate17.Password = dbPassword;
        }
        else
        {
            connectionStringBuilderTestTemplate17.UserID = dbUser;
            connectionStringBuilderTestTemplate17.Password = dbPassword;
            connectionStringBuilderTestTemplate17.Authentication = SqlAuthenticationMethod.ActiveDirectoryPassword;
        }
        var upgraderTestTemplate17 =
            DeployChanges.To
                .SqlDatabase(connectionStringBuilderTestTemplate17.ConnectionString)
                .WithVariable("SqlUsersGroupNameVariable", sqlUsersGroupName)    // This is necessary to perform template variable replacement in the scripts.
                .WithScriptsFromFileSystem(
                    !string.IsNullOrWhiteSpace(scriptsPath)
                            ? Path.Combine(scriptsPath, "TestTemplate17Scripts")
                        : Path.Combine(Environment.CurrentDirectory, "TestTemplate17Scripts"))
                .LogToConsole()
                .Build();
        Console.WriteLine($"Now upgrading TestTemplate17.");
        if (env == "Development")
        {
            upgraderTestTemplate17.MarkAsExecuted("0000_AzureSqlContainedUser.sql");
        }
        var resultTestTemplate17 = upgraderTestTemplate17.PerformUpgrade();

        if (!resultTestTemplate17.Successful)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"TestTemplate17 upgrade error: {resultTestTemplate17.Error}");
            Console.ResetColor();
            return -1;
        }

        // Uncomment the below sections if you also have an Identity Server project in the solution.
        /*
        var connectionStringTestTemplate17Identity = string.IsNullOrWhiteSpace(args.FirstOrDefault())
            ? config["ConnectionStrings:TestTemplate17IdentityDb"]
            : args.FirstOrDefault();

        var upgraderTestTemplate17Identity =
            DeployChanges.To
                .SqlDatabase(connectionStringTestTemplate17Identity)
                .WithScriptsFromFileSystem(
                    scriptsPath != null
                        ? Path.Combine(scriptsPath, "TestTemplate17IdentityScripts")
                        : Path.Combine(Environment.CurrentDirectory, "TestTemplate17IdentityScripts"))
                .LogToConsole()
                .Build();
        Console.WriteLine($"Now upgrading TestTemplate17 Identity.");
        if (env != "Development")
        {
            upgraderTestTemplate17Identity.MarkAsExecuted("0004_InitialData.sql");
            Console.WriteLine($"Skipping 0004_InitialData.sql since we are not in Development environment.");
            upgraderTestTemplate17Identity.MarkAsExecuted("0005_Initial_Configuration_Data.sql");
            Console.WriteLine($"Skipping 0005_Initial_Configuration_Data.sql since we are not in Development environment.");
        }
        var resultTestTemplate17Identity = upgraderTestTemplate17Identity.PerformUpgrade();

        if (!resultTestTemplate17Identity.Successful)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"TestTemplate17 Identity upgrade error: {resultTestTemplate17Identity.Error}");
            Console.ResetColor();
            return -1;
        }
        */

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Success!");
        Console.ResetColor();
        return 0;

        void InitializeParameters()
        {
            // Local database, populated from .env file.
            if (args.Length == 0)
            {
                connectionString = config["TestTemplate17Db_Migrations_Connection"];
                dbUser = config["DbUser"];
                dbPassword = config["DbPassword"];
            }

            // Deployed database
            else if (args.Length == 5)
            {
                connectionString = args[0];
                dbUser = args[1];
                dbPassword = args[2];
                scriptsPath = args[3];
                sqlUsersGroupName = args[4];
            }
        }
    }
}
