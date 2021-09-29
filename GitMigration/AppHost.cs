using System;
using GitMigration.Models;
using GitMigration.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Octokit;

namespace GitMigration
{
    public interface IAppHost
    {
        void Run();
    }
    public class AppHost : IAppHost
    {
        private readonly IMigrationService _migrationService;
        private readonly ILogger<AppHost> _logger;
        private readonly IConfiguration _configuration;

        public AppHost(IMigrationService migrationService, ILogger<AppHost> logger, IConfiguration configuration)
        {
            _migrationService = migrationService;
            _logger = logger;
            _configuration = configuration;
        }
        public void Run()
        {
            _logger.LogInformation("Starting Migration Run");
            var request = new MigrationRequest
            {
                AdoProjectName = _configuration["ADO:ProjectName"],
                GitHubTeamName = _configuration["GitHub:TeamName"],
                ReposToMigrate = _configuration["ADO:ReposToMigrate"].Split(',')
            };
            _migrationService.Start(request, _configuration["ADO:Pat"], _configuration["GitHub:Pat"]).GetAwaiter().GetResult();
            _logger.LogInformation("Finished Migration Run");
        }
    }
}