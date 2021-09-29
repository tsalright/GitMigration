using System.Collections.Generic;

namespace GitMigration.Clients.LocalRepo
{
    public interface ILocalRepoClient
    {
        bool AddSettingsFile(string localRepoPath);
        bool RenameBranch(string localRepoPath, string sourceBranchName = "master");
        bool ChangeOriginTarget(string localRepoPath, string targetOriginUrl);
        int PushRepoBranches(string localRepoPath, string pat, IEnumerable<string> branchNamesToExclude);
        int PushRepoTags(string localRepoPath, string pat);
        string PullRepo(Models.SourceRepo repo, string pat);
        void CleanUpLocalClone(string repoName);
    }
}