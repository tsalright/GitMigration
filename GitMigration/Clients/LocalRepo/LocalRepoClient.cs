using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LibGit2Sharp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.Services.Common;

namespace GitMigration.Clients.LocalRepo
{
    public class LocalRepoClient : ILocalRepoClient
    {
        private readonly ILogger<LocalRepoClient> _logger;
        private readonly IConfiguration _config;
        
        public LocalRepoClient(ILogger<LocalRepoClient> logger, IConfiguration config)
        {
            _logger = logger;
            _config = config;
        }

        public bool AddSettingsFile(string localRepoPath)
        {
            _logger.LogInformation("Adding settings.yml to default branch");
            try
            {
                using var localRepo = new Repository(localRepoPath);
                var settingsDirectory = $"{localRepo.Info.WorkingDirectory}.github";
                if (!Directory.Exists(settingsDirectory))
                {
                    Directory.CreateDirectory(settingsDirectory);    
                }
                File.Copy("./settings.yml", $"{settingsDirectory}/settings.yml", true);
                Commands.Stage(localRepo, $"{settingsDirectory}/settings.yml");
                var signature = new Signature(_config["Signature:Name"], _config["Signature:Email"], DateTimeOffset.Now);
                localRepo.Commit("Add settings for GitHub", signature, signature);
                return true;
            }
            catch (Exception e)
            {
                _logger.LogCritical(e, "Failed to add settings.yml to default branch");
                return false;
            }
        }

        public bool RenameBranch(string localRepoPath, string sourceBranchName = "master")
        {
            _logger.LogInformation($"Renaming {sourceBranchName} to {_config["GitHub:DefaultBranchName"]}");
            using var localRepo = new Repository(localRepoPath);

            try
            {
                localRepo.Branches.Rename(sourceBranchName, _config["GitHub:DefaultBranchName"]);
                return true;
            }
            catch (NameConflictException nce)
            {
                _logger.LogInformation("Target Branch Name Already Exists");
                return true;
            }
            catch (Exception e)
            {
                _logger.LogCritical(e, $"Failed to rename default branch to {_config["GitHub:DefaultBranchName"]}");
                return false;
            }
        }

        public bool ChangeOriginTarget(string localRepoPath, string targetOriginUrl)
        {
            _logger.LogInformation($"Retargeting repo to {targetOriginUrl}");
            try
            {
                using var localRepo = new Repository(localRepoPath);
                localRepo.Network.Remotes.Update("origin", r => r.Url = targetOriginUrl);
                var remote = localRepo.Network.Remotes["origin"];
                    
                localRepo.Branches
                    .ForEach(branch => localRepo.Branches
                        .Update(branch, 
                            delegate(BranchUpdater updater)
                            {
                                updater.Remote = remote.Name;
                                updater.UpstreamBranch = branch.CanonicalName;
                            }));
                return true;
            }
            catch (Exception e)
            {
                _logger.LogCritical(e, "Failed to retarget repo to GitHub");
                return false;
            }
        }

        public int PushRepoBranches(string localRepoPath, string pat, IEnumerable<string> branchNamesToExclude)
        {
            _logger.LogInformation("Pushing repo branches");
            try
            {
                branchNamesToExclude ??= new List<string>();
                
                using var localRepo = new Repository(localRepoPath);
                var options = new PushOptions
                {
                    CredentialsProvider = (url, user, cred) =>
                        new UsernamePasswordCredentials
                        {
                            Username = pat,
                            Password = string.Empty
                        }
                };
                var reposToPush = localRepo.Branches
                    .Where(b =>
                    {
                        var namesToExclude = branchNamesToExclude.ToList();
                        return !namesToExclude.Contains(b.FriendlyName.ToLower());
                    }).ToList();
                localRepo.Network
                    .Push(reposToPush, options);
                return reposToPush.Count();
            }
            catch(NonFastForwardException e)
            {
                _logger.LogWarning(e, "Branch pushed in previous run. Skipping.");
                return 0;
            }
            catch (Exception e)
            {
                _logger.LogCritical(e, "Failed to push repo branches to GitHub");
                return -1;
            }
        }

        public int PushRepoTags(string localRepoPath, string pat)
        {
            _logger.LogInformation("Pushing repo tags");
            try
            {
                using var localRepo = new Repository(localRepoPath);
                var options = new PushOptions
                {
                    CredentialsProvider = (url, user, cred) =>
                        new UsernamePasswordCredentials
                        {
                            Username = pat,
                            Password = string.Empty
                        }
                };
                foreach (var tag in localRepo.Tags)
                {
                    localRepo.Network.Push(localRepo.Network.Remotes["origin"], tag.CanonicalName, options);
                }

                return localRepo.Tags.Count();
            }
            catch (Exception e)
            {
                _logger.LogCritical(e, "Failed to push repo tags to GitHub");
                return -1;
            }
        }

        public string PullRepo(Models.SourceRepo repo, string pat)
        {
            _logger.LogInformation("Cloning repo for migration");
            try
            {
                var tempPath = $"/tmp/{repo.Name}";
                if (Directory.Exists(tempPath))
                {
                    Directory.Delete(tempPath, true);    
                }
            
                var cloneOptions = new CloneOptions
                {
                    CredentialsProvider = (url, user, creds) =>
                        new UsernamePasswordCredentials
                        {
                            Username = pat,
                            Password = string.Empty
                        }
                };
                return Repository.Clone(repo.WebUrl, tempPath, cloneOptions);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Failed to clone {repo.Name} repository from ADO");
                throw;
            }
        }

        public void CleanUpLocalClone(string repoName)
        {
            var tempPath = $"/tmp/{repoName}";
            if (Directory.Exists(tempPath))
            {
                Directory.Delete(tempPath, true);  
                _logger.LogInformation($"Cleaned up {tempPath}");
            }
        }
    }
}