using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Flurl.Http;
using GitMigration.Clients.LocalRepo;
using GitMigration.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Octokit;
using Octokit.Internal;

namespace GitMigration.Clients.TargetRepo
{
    public class GitHubClient : ITargetRepoClient
    {
        private readonly ILogger<GitHubClient> _logger;
        private readonly IConfiguration _config;
        private readonly ILocalRepoClient _localRepoClient;
        private Octokit.GitHubClient _client;
        private string _pat;
        private string _orgName;
        
        public GitHubClient(ILogger<GitHubClient> logger, IConfiguration config, ILocalRepoClient localRepoClient)
        {
            _logger = logger;
            _config = config;
            _localRepoClient = localRepoClient;
        }

        public async Task<Models.TargetRepo> MigrateRepository(Models.SourceRepo sourceRepo, string teamName)
        {
            // Add Logging stuff here and Target Repo
            _logger.LogInformation($"Started migrating {sourceRepo.Name}");
            var gitHubRepo = await CreateRepository(sourceRepo.Name, teamName);
            var targetRepo = new Models.TargetRepo
            {
                TeamName = teamName,
                HtmlUrl = gitHubRepo.HtmlUrl
            };
            targetRepo.IsBranchRenameCompleted = _localRepoClient.RenameBranch(sourceRepo.LocalRepoPath, sourceRepo.DefaultBranch);
            targetRepo.IsOriginUpdated = _localRepoClient.ChangeOriginTarget(sourceRepo.LocalRepoPath, gitHubRepo.CloneUrl);
            targetRepo.BranchesMigrated = _localRepoClient.PushRepoBranches(sourceRepo.LocalRepoPath, _pat, new List<string>{"master", "origin/master"});
            targetRepo.TagsMigrated = _localRepoClient.PushRepoTags(sourceRepo.LocalRepoPath, _pat);
            targetRepo.IsPermissionsSet = await UpdatePermissionsOnTeamRepos(teamName, sourceRepo.Name);
            targetRepo.IsMasterBranchRemoved = await RemoveMasterFromGitHub(sourceRepo.Name);
            return targetRepo;
        }

        public void SetPatAndOrg(string pat, string orgName)
        {
            _orgName = orgName ?? _config["GitHub:Org"];
            _pat = pat ?? _config["GitHub:Pat"];
            var credentials = new InMemoryCredentialStore(new Octokit.Credentials(_pat));
            _client = new Octokit.GitHubClient(new ProductHeaderValue("GitMigration"), credentials);
        }

        private async Task<Octokit.Repository> CreateRepository(string repoName, string teamName)
        {
            _logger.LogInformation($"Creating repo in GitHub");
            try
            {
                var exitingRepo = await _client.Repository.Get(_orgName, repoName);
                if (exitingRepo != null)
                {
                    return exitingRepo;
                }
            }
            catch (Octokit.NotFoundException)
            {
                _logger.LogInformation("Repo Not Found, proceeding to create");
            }

            try
            {
                var teamId = await GetTeamId(teamName);
                var newRepo = new NewRepository(repoName)
                {
                    Private = true,
                    TeamId = teamId
                };
                return await _client.Repository.Create(_orgName, newRepo);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to create repository in GitHub");
                throw;
            }
        }

        private async Task<int> GetTeamId(string teamName)
        {
            _logger.LogInformation($"Getting teamId for {teamName}");
            try
            {
                var teams = await _client.Organization.Team.GetAll(_orgName);
                return teams.First(t => t.Name == teamName).Id;
            }
            catch (Exception e)
            {
                _logger.LogCritical(e, "Failed to retrieve TeamId");
                throw;
            }
        }

        private async Task<bool> UpdatePermissionsOnTeamRepos(string teamName, string repoName)
        {
            _logger.LogInformation($"Setting {teamName} as admin on {repoName}");
            try
            {
                var results =
                    await $"https://api.github.com/orgs/{_orgName}/teams/{teamName.ToLower().Replace(" ", "-")}/repos/{_orgName}/{repoName}"
                        .WithHeader("User-Agent", "GitMigration")
                        .WithHeader("Authorization", $"token {_pat}")
                        .WithHeader("Accept", "application/vnd.github.v3+json")
                        .PutJsonAsync(new PermissionRequest {permission = "admin"});
                return results.ResponseMessage.IsSuccessStatusCode;
            }
            catch (Exception e)
            {
                _logger.LogCritical(e, "Failed to update team's permissions to admin");
                return false;
            }
        }

        private async Task<bool> RemoveMasterFromGitHub(string repoName)
        {
            try
            {
                _logger.LogInformation("Cleaning up master branch from github repo");
                var result = await $"https://api.github.com/repos/{_orgName}/{repoName}/git/refs/heads/master"
                    .WithHeader("User-Agent", "GitMigration")
                    .WithHeader("Authorization", $"token {_pat}")
                    .WithHeader("Accept", "application/vnd.github.v3+json")
                    .DeleteAsync();

                if (result.ResponseMessage.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Removed master branch");
                    return true;
                }

                _logger.LogInformation("Master branch failed to remove");
                return false;
            }
            catch (Exception)
            {
                _logger.LogInformation("Master branch doesn't exist");
                return false;
            }
        }
    }
}