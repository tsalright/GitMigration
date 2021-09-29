using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;

namespace GitMigration.Clients.SourceRepo
{
    public class AzureDevOpsClient : ISourceRepoClient
    {
        private readonly ILogger<AzureDevOpsClient> _logger;
        private readonly IConfiguration _config;
        private string _pat;

        private VssConnection _connection;

        public AzureDevOpsClient(ILogger<AzureDevOpsClient> logger, IConfiguration config)
        {
            _logger = logger;
            _config = config;
        }

        public async Task<IEnumerable<Models.SourceRepo>> GetRepositories(string parentName)
        {
            _logger.LogInformation("Getting repositories");
            try
            {
                using var gitClient = _connection.GetClient<GitHttpClient>();

                var repos = await gitClient.GetRepositoriesAsync(parentName);
                List<Models.SourceRepo> sourceRepos = new List<Models.SourceRepo>();
                repos.ForEach(repo => sourceRepos.Add(new Models.SourceRepo
                {
                    DefaultBranch = repo.DefaultBranch,
                    Name = repo.Name,
                    WebUrl = repo.WebUrl
                }));
                return sourceRepos;
            }
            catch(Exception e)
            {
                _logger.LogError(e, "Failed to get ADO Repositories");
                throw;
            }
        }

        public void SetPatAndOrg(string pat, string orgName)
        {
            _pat = pat ?? _config["ADO:Pat"];
            var org = orgName ?? _config["ADO:Org"];
            
            var credentials = new VssBasicCredential(string.Empty, _pat);
            _connection = new VssConnection(new Uri($"https://dev.azure.com/{org}"), credentials);
        }
    }
}