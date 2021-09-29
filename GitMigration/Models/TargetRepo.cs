namespace GitMigration.Models
{
    public class TargetRepo
    {
        public string TeamName { get; set; }
        public string HtmlUrl { get; set; }
        public string DefaultBranch { get; set; }
        public bool IsSettingsFileCommitted { get; set; }
        public bool IsBranchRenameCompleted { get; set; }
        public bool IsOriginUpdated { get; set; }
        public int BranchesMigrated { get; set; }
        public int TagsMigrated { get; set; }
        public bool IsPermissionsSet { get; set; }
        public bool IsMasterBranchRemoved { get; set; }
    }
}