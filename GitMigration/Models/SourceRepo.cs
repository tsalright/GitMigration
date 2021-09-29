namespace GitMigration.Models
{
    public class SourceRepo
    {
        public string Name { get; set; }
        public string WebUrl { get; set; }
        public string DefaultBranch { get; set; }
        public string LocalRepoPath { get; set; }

        // Logging Properties
        public bool IsCloned => !string.IsNullOrWhiteSpace(LocalRepoPath);
         
    }
}