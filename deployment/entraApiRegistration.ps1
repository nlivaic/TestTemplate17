##################################
### Create Azure App Registration
##################################
Param( [string]$projectName )

$tenantId = (az account list --query "[?isDefault].tenantId | [0]" --output tsv)
$displayNameApi = "$($projectName)Api"

Write-Host "##[warning]--- Create Azure App Registration - START ---"
Write-Host "##[section]displayNameApi: $displayNameApi"
$appRegistration = az ad app create `
    --display-name $displayNameApi `
    --sign-in-audience AzureADMyOrg `
    --required-resource-accesses "deployment/manifest.json"

$appRegistrationResult = ($appRegistration | ConvertFrom-Json)
$appRegistrationResultAppId = $appRegistrationResult.appId
$azAppOID = (az ad app show --id $appRegistrationResultAppId  | ConvertFrom-JSON).id

Write-Host "##[section]Created API $displayNameApi with appId: $appRegistrationResultAppId"
Write-Host "##[warning]--- Create Azure App Registration - END ---"

##################################
### Expose API
##################################
Write-Host "##[warning]--- Expose API - START ---"
az ad app update --id $appRegistrationResultAppId --identifier-uris api://$appRegistrationResultAppId
Write-Host "##[section]API $displayNameApi exposed"
Write-Host "##[warning]--- Expose API - END ---"

##################################
###  Add scopes (oauth2Permissions)
##################################
Write-Host "##[warning]--- Add scopes - START ---"
# 0. Setup some basic stuff to work with scopes.
$headerJson = @{
    'Content-Type' = 'application/json'
} | ConvertTo-Json -d 3 -Compress 
# $headerJson = $headerJson.replace('"', '\"')
$graphURL="https://graph.microsoft.com/v1.0/applications/$azAppOID"

Write-Host ""
Write-Host "##[section]Disabling existing scopes..."
# 1. Read existing scopes.
$oauth2PermissionScopesApiOld = $appRegistrationResult.api
# 2. Disable scopes, because we want to provision from a list.
foreach ($scope in $oauth2PermissionScopesApiOld.oauth2PermissionScopes) {
    Write-Host "##[section]`tExisting scope $($scope.value) disabled."
    $scope.isEnabled = 'false'
}
$bodyOauth2PermissionScopesApiOld = @{
    api = $appRegistrationResult.api
}
$bodyOauth2PermissionScopesApiOldJsonEscaped = ($bodyOauth2PermissionScopesApiOld|ConvertTo-Json -d 4 -Compress)
$bodyOauth2PermissionScopesApiOldJsonEscaped | Out-File -FilePath .\deployment\oauth2PermissionScopesOld.json
az rest --method PATCH --uri $graphurl --headers $headerJson --body '@deployment/oauth2PermissionScopesOld.json'
Remove-Item .\deployment\oauth2PermissionScopesOld.json
Write-Host "##[section]Existing scopes disabled successfully."

# 3. Add new scopes from file deployment/oauth2PermissionScopes.json.
Write-Host ""
Write-Host "##[section]Creating scopes..."
az rest --method PATCH --uri $graphurl --headers $headerJson --body '@deployment/oauth2PermissionScopes.json'
# 4. Re-enable previously disabled scopes.
if ($? -eq $false) {
    Write-Error "Error creating scopes."
    Write-Error "Re-enabling original scopes."
    foreach ($scope in $oauth2PermissionScopesApiOld.oauth2PermissionScopes) {
        Write-Host "##[error]`tExisting scope $($scope.value) re-enabled."
        $scope.isEnabled = 'true'
    }
    $bodyOauth2PermissionScopesApiOldJsonEscaped = ($bodyOauth2PermissionScopesApiOld|ConvertTo-Json -d 4 -Compress)
    $bodyOauth2PermissionScopesApiOldJsonEscaped | Out-File -FilePath .\deployment\oauth2PermissionScopesOld.json
    az rest --method PATCH --uri $graphurl --headers $headerJson --body '@deployment/oauth2PermissionScopesOld.json'
    Remove-Item .\deployment\oauth2PermissionScopesOld.json
    Write-Host "##[error]--- Add scopes - END (Error) ---"
    Return
}
# 5. If all went well, print confirmation message and list of new scopes.
$appRegistration = az ad app show --id $appRegistrationResultAppId
$appRegistrationResult = ($appRegistration | ConvertFrom-Json)
$oauth2PermissionScopesApi = $appRegistrationResult.api
foreach ($scope in $oauth2PermissionScopesApi.oauth2PermissionScopes) {
    Write-Host "##[section]`tScope created: $($scope.value)"
}
Write-Host "##[section]Scopes created successfully."
Write-Host "##[warning]--- Add scopes - END ---"

Write-Host ""
Write-Host "##[section]Registered App details:"
Write-Host "##[section]$($appRegistration)"

##################################
###  Service Principal Lock
##################################
Write-Host "##[warning]--- Service Principal Lock - START ---"
az rest --method PATCH --uri $graphurl --headers $headerJson --body '@deployment/servicePrincipalLockConfiguration.json'
Write-Host "##[section]Service Principal Locked."
Write-Host "##[warning]--- Service Principal Lock - END ---"

##################################
###  Create a Service Principal for the API App Registration
##################################
Write-Host "##[warning]--- Create a ServicePrincipal - START ---"

$createdSp = az ad sp show --id $appRegistrationResultAppId
if ($? -eq $false) {
    $createdSp = az ad sp create --id $appRegistrationResultAppId
    Write-Host "##[section]Created Service Principal for API App registration"
} else {
    Write-Host "##[section]Service Principal already exists, skipped creation."
}
Write-Host "##[section]Service principal details:"
Write-Host "##[section]$($createdSp)""
Write-Host "##[warning]--- Create a ServicePrincipal - END ---"

##################################
###  Return configuration
##################################
Write-Host "##vso[task.setvariable variable=tenantId;isoutput=true]$tenantId"
Write-Host "##vso[task.setvariable variable=authAuthority;isoutput=true]https://login.microsoftonline.com/$tenantId/v2.0"
Write-Host "##vso[task.setvariable variable=authAudience;isoutput=true]api://$appRegistrationResultAppId"
Write-Host "##vso[task.setvariable variable=authValidIssuer;isoutput=true]https://sts.windows.net/$tenantId/"
