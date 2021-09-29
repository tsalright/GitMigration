using System.Collections.Generic;

namespace GitMigration.Models
{
    public class MigrationRequest
    {
        public string AdoOrgName { get; set; }
        public string GitHubOrgName { get; set; }
        public string AdoProjectName { get; set; }
        public string GitHubTeamName { get; set; }
        public IEnumerable<string> ReposToMigrate { get; set; }
    }
}