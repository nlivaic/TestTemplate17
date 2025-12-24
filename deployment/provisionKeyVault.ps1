param ($environment, $projectName, $resourceGroupName)

$rgId = az group show --resource-group $resourceGroupName --query id --output tsv
####################################################
### Create Key Vault 
####################################################
Write-Host "##[warning]------ Create Key Vault START ------"
$tenantId = (az account list --query "[?isDefault].tenantId | [0]" --output tsv)
$jsonResultAll = az deployment group create --resource-group $resourceGroupName --template-file ./deployment/iacKeyVault.bicep --parameters environment=$environment projectName=$projectName tenantId=$tenantId | ConvertFrom-Json
$keyVaultName = $jsonResultAll.properties.outputs.keyVaultName.value
Write-Host "##vso[task.setvariable variable=keyVaultName;isoutput=true]$keyVaultName"
Write-Host "##[warning]------ Create Key Vault END ------"

####################################################
### Assign Key Vault Roles to AzureConnection
####################################################
Write-Host "##[warning]------ Assign Key Vault Roles to AzureConnection START ------"
$servicePrincipalId = az ad sp list --filter "appId eq '$env:servicePrincipalId'" --query '[].id' --output tsv
$keyVaultId = az keyvault show --name $keyVaultName --query id --output tsv
# "Key Vault Secrets Officer" role, scoped to this key vault
az role assignment create --assignee $servicePrincipalId --role "b86a8fe4-44ce-4948-aee5-eccb2c155cd7" --scope $keyVaultId
Write-Host "##[warning]------ Assign Key Vault Roles to AzureConnection END ------"

$vaultUri = az keyvault show --name $keyVaultName --query 'properties.vaultUri'
Write-Host "##vso[task.setvariable variable=vaultUri;isoutput=true]$vaultUri"
