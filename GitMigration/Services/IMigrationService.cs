using System.Threading.Tasks;
using GitMigration.Models;

namespace GitMigration.Services
{
    public interface IMigrationService
    {
        Task Start(MigrationRequest request, string adoPat, string gitHubPat);
    }
}