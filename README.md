# GitMigration
DotNetCore Web API for migrating from ADO to GitHub. Use this to migrate your repositories from Azure DevOps to GitHub.
It will migrate all branches at this point and rename the default branch (usually `master`) to `main` before pushing the repo up to GitHub. 


The `ReposToMigrate` array can be empty or contain a list of specific repos you want to focus on. If it is empty, it will process all repositories in the Azure DevOps Project.

The local **appsettings.json** contains settings for the default `GitHubOrgName`, `GitHubPat`, `AdoProjectName`, and `AdoPat`.
It also contains the settings for the git commit `Signature` you want to use for commiting and pushing the **settings.yml** file. 