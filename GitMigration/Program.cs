using System;
using System.IO;
using GitMigration.Clients.LocalRepo;
using GitMigration.Clients.SourceRepo;
using GitMigration.Clients.TargetRepo;
using GitMigration.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GitMigration
{
    class Program
    {
        static void Main(string[] args)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .AddCommandLine(args)
                .Build();
            
            var serviceProvider = new ServiceCollection()
                .AddSingleton<IConfiguration>(config)
                .AddLogging(configure => configure.AddConsole())
                .AddSingleton<IAppHost, AppHost>()
                .AddSingleton<ISourceRepoClient, AzureDevOpsClient>()
                .AddSingleton<ILocalRepoClient, LocalRepoClient>()
                .AddSingleton<ITargetRepoClient, GitHubClient>()
                .AddSingleton<IMigrationService, MigrationService>()
                .BuildServiceProvider();
            
            
            var appHost = serviceProvider.GetService<IAppHost>();
            appHost?.Run();
        }
    }
}