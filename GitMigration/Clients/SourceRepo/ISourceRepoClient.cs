using System.Collections.Generic;
using System.Threading.Tasks;

namespace GitMigration.Clients.SourceRepo
{
    public interface ISourceRepoClient
    {
        Task<IEnumerable<Models.SourceRepo>> GetRepositories(string parentName);
        void SetPatAndOrg(string pat, string orgName);
    }
}