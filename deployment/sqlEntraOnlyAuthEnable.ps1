####################################################
### This will enable Entra-only auth for all Sql Servers
### belonging to this resource group. These were previously
### disabled in sqlEntraOnlyAuthDisable.ps1 script.
### It is imperative this task runs always, even
### if the pipeline run is cancelled or fails.
### This task should not fail otherwise your SQL
### Servers will not be usable :)
####################################################
param ($resourceGroupName)

####################################################
### AAD only auth
####################################################
Write-Host "##[warning]--- Enable Entra-only auth for all SQL Servers in this Resource Group - START ---"
$sqlServers = (az sql server list -g $resourceGroupName --query '[].name' --output tsv)
foreach ($sqlServer in $sqlservers) {
    $sqlServerAdmin = az sql server ad-admin list --resource-group $resourceGroupName --server-name $sqlServer --query '[].login' --output tsv
    If ($sqlServerAdmin -ne $null) {
        az sql server ad-only-auth enable --resource-group $resourceGroupName --name $sqlServer
        Write-Host "##[section]`tExisting Sql Server's $($sqlServer) Entra-only auth is enabled."
    }
}
Write-Host "##[warning]--- Enable Entra-only auth for all SQL Servers in this Resource Group - END ---" -ForegroundColor Yellow
