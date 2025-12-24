####################################################
### This will disable Entra-only auth for all Sql Servers
### belonging to this resource group. Purpose is to
### side-step a known issue on ARM, where a Sql Server
### with Entra-only auth enabled cannot be re-deployed 
### by Bicep because Entra-only auth was enabled in a previous
### pipeline run and the bicep file doesn't reflect that.
### Error (and workaround):
### https://github.com/Azure/bicep-types-az/issues/1436#issuecomment-1478124310
####################################################
param ($resourceGroupName)

####################################################
### Disable Entra-only auth for all SQL Servers in this Resource Group
####################################################
Write-Host "##[warning]--- Disable Entra-only auth for all SQL Servers in this Resource Group - START ---"
$sqlServers = (az sql server list -g $resourceGroupName --query '[].name' --output tsv)
foreach ($sqlServer in $sqlServers) {
    az sql server ad-only-auth disable --resource-group $resourceGroupName --name $sqlServer
    Write-Host "##[section]`tExisting Sql Server's $($sqlServer) Entra-only auth is disabled."
}
Write-Host "##[warning]--- Disable Entra-only auth for all SQL Servers in this Resource Group - END ---" -ForegroundColor Yellow
