using System.Threading.Tasks;

namespace GitMigration.Clients.TargetRepo
{
    public interface ITargetRepoClient
    {
        Task<Models.TargetRepo> MigrateRepository(Models.SourceRepo sourceRepo, string teamName);
        void SetPatAndOrg(string pat, string orgName);
    }
}