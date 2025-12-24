param ($usersGroupName, $resourceGroupName, $appServiceWebName)

####################################################
### Create User Group
####################################################
Write-Host "##[warning]--- Create and Populate User Group - START ---"
$usersGroupId = (az ad group list --filter "displayName eq '$usersGroupName'" --query '[].id' --output tsv)
If ($usersGroupId -eq $null) {
    $usersGroupId = (az ad group create --display-name $usersGroupName --mail-nickname $usersGroupName --query id --output tsv)
    Write-Host "##[section]Created Entra group '$usersGroupName' with group Id: $usersGroupId"
}
Write-Host "##[warning]--- Create and Populate User Group - END ---"

####################################################
### Assign roles to User Group
####################################################
Write-Host "##[warning]--- Assign roles to User Group - START ---"
$rgId = az group show --resource-group $resourceGroupName --query id --output tsv
# Scoped to resource group
# Key Vault Secrets Officer role
az role assignment create --assignee $usersGroupId --role "b86a8fe4-44ce-4948-aee5-eccb2c155cd7" --scope $rgId
Write-Host "##[section]Added Entra group '$usersGroupId' to role 'Key Vault Secrets Officer'"
Write-Host "##[warning]--- Assign roles to User Group - END ---"

####################################################
### Create Managed Identity for Web API
####################################################
Write-Host "##[warning]--- Create Managed Identity for Web API - START ---"
# Enable managed identity on app
$managedIdentityId = (az webapp identity show --resource-group $resourceGroupName --name $appServiceWebName --query principalId --output tsv)
If ($managedIdentityId -eq $null) {
    $managedIdentityId = (az webapp identity assign --resource-group $resourceGroupName --name $appServiceWebName  --query principalId --output tsv)
    Write-Host "##[section]Created system-assigned managed identity for '$appServiceWebName' with Id: $managedIdentityId"
}
# Add Managed Identity to users group
$isInGroup = (az ad group member check --group $usersGroupId --member-id $managedIdentityId  --query value --output tsv)
if ($isInGroup -eq 'false') {
    az ad group member add --group $usersGroupId --member-id $managedIdentityId
    Write-Host "##[section]Added Entra Managed Identity '$appServiceWebName' with id '$managedIdentityId' to group: $usersGroupId"
}
Write-Host "##[warning]--- Create Managed Identity for Web API - END ---"
