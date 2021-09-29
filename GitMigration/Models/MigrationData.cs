using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.Services.Common;

namespace GitMigration.Models
{
    public class MigrationData
    {
        public string SourceOrgName { get; set; }
        public string SourceTeamName { get; set; }
        public string TargetOrgName { get; set; }
        public string TargetTeamTeam { get; set; }
        public IEnumerable<string> RepositoriesToInclude { get; set; }
        public IEnumerable<SourceRepo> SourceRepos { get; set; }
        public List<TargetRepo> TargetRepos { get; set; }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Org Name: {SourceOrgName} => {TargetOrgName}");
            sb.AppendLine($"Team Name: {SourceTeamName} => {TargetTeamTeam}\n");
            sb.AppendLine($"Repositories to Migrate Include ({RepositoriesToInclude.Count()}");
            RepositoriesToInclude.ForEach(repo => sb.AppendLine($"\t{repo}"));
            sb.AppendLine($"\nRepositories found in Source Org/Team ({SourceRepos.Count()})");
            SourceRepos.Where(r => !RepositoriesToInclude.Any() || RepositoriesToInclude.Contains(r.Name))
                .ForEach(repo =>
                {
                    sb.AppendLine($"{repo.Name} {repo.DefaultBranch}");
                    sb.AppendLine($"\tLocal Storage: {repo.LocalRepoPath}");
                    sb.AppendLine($"\tWeb Url: {repo.WebUrl}");
                    sb.AppendLine($"\tIs Cloned: {repo.IsCloned}");
                });
            sb.AppendLine($"\nRepositories migrated to Target Org/Team ({TargetRepos.Count()})");
            TargetRepos.ForEach(repo =>
            {
                sb.AppendLine("***********************************************");
                sb.AppendLine($"\t{repo.HtmlUrl}");
                sb.AppendLine($"\t{repo.TeamName}");
                sb.AppendLine($"\tIs Settings File Committed: {repo.IsSettingsFileCommitted}");
                sb.AppendLine($"\tIs Branch Rename Complete: {repo.IsBranchRenameCompleted}");
                sb.AppendLine($"\tIs Origin Updated: {repo.IsOriginUpdated}");
                sb.AppendLine($"\t# of Branches Migrated: {repo.BranchesMigrated}");
                sb.AppendLine($"\t# of Tags Migrated: {repo.TagsMigrated}");
                sb.AppendLine($"\tAre Permissions Set: {repo.IsPermissionsSet}");
                sb.AppendLine($"\tIs Master Branch Removed: {repo.IsMasterBranchRemoved}");
            });
            return sb.ToString();
        }
    }
}

/*
 * *******************
 * Migration starting
 * *******************
 *
 * Org Name: {SourceOrgName} => {TargetOrgName}
 * Team Name: {SourceTeamName} => {TargetTeamName}
 *
 * Repositories to Migrate Include (Count of RepositoriesToInclude):
 *   List of RepositoriesToInclude
 *
 * Repositories found in Source Org/Team (Count of SourceRepos):
 *   List of SourceRepos following this template >> $"{SourceRepo.Name} {SourceRepo.DefaultBranch} ({SourceRepo.WebUrl})"
 *
 * 
*/