using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GitMigration.Clients.LocalRepo;
using GitMigration.Clients.SourceRepo;
using GitMigration.Clients.TargetRepo;
using GitMigration.Models;
using Microsoft.Extensions.Logging;

namespace GitMigration.Services
{
    public class MigrationService : IMigrationService
    {
        private readonly ISourceRepoClient _sourceRepoClient;
        private readonly ILocalRepoClient _localRepoClient;
        private readonly ITargetRepoClient _targetRepoClient;
        private readonly ILogger<MigrationService> _logger;

        public MigrationService(ISourceRepoClient sourceRepoClient, ITargetRepoClient targetRepoClient, ILogger<MigrationService> logger, ILocalRepoClient localRepoClient)
        {
            _sourceRepoClient = sourceRepoClient;
            _targetRepoClient = targetRepoClient;
            _logger = logger;
            _localRepoClient = localRepoClient;
        }
        
        public async Task Start(MigrationRequest request, string adoPat, string gitHubPat)
        {
            var summaryData = InitializeMigrationData(request);
            _logger.LogInformation("Setting up auth for clients");
            _sourceRepoClient.SetPatAndOrg(adoPat, request.AdoOrgName);
            _targetRepoClient.SetPatAndOrg(gitHubPat, request.GitHubOrgName);
            
            summaryData.SourceRepos = await _sourceRepoClient.GetRepositories(request.AdoProjectName);
            foreach (var sourceRepo in summaryData.SourceRepos.Where(r => !request.ReposToMigrate.Any() || request.ReposToMigrate.Contains(r.Name)))
            { 
                _logger.LogInformation($"{sourceRepo.Name}\n\tWebUrl: {sourceRepo.WebUrl}");
                sourceRepo.LocalRepoPath = _localRepoClient.PullRepo(sourceRepo, adoPat);
                summaryData.TargetRepos.Add(await _targetRepoClient.MigrateRepository(sourceRepo, request.GitHubTeamName));
                _localRepoClient.CleanUpLocalClone(sourceRepo.Name);
            }
            _logger.LogInformation(summaryData.ToString());
        }

        private MigrationData InitializeMigrationData(MigrationRequest request)
        {
            return new MigrationData
            {
                SourceOrgName = request.AdoOrgName,
                SourceTeamName = request.AdoProjectName,
                TargetOrgName = request.GitHubOrgName,
                TargetTeamTeam = request.GitHubTeamName,
                RepositoriesToInclude = request.ReposToMigrate,
                TargetRepos = new List<TargetRepo>()
            };
        }
    }
}