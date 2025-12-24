param ($keyVaultName, $dbConnectionString, $messageBrokerConnectionString, $authAuthority, $authAudience, $authValidIssuer)

# Will expose method NewPassword
. $PSScriptRoot\secretGenerator.ps1

################################################################
### Set Key Vault secrets to provided values.
################################################################
$sqlSaPasswordSecretName = "SqlSaPassword"
$sqlAdminPasswordSecretName = "SqlAdminPassword"
$dbConnectionName = 'TestTemplate17DbConnection'
$messageBrokerName = 'MessageBroker'
$applicationInsightsConnectionName = 'ApplicationInsightsConnectionString'
$authoritySecretName = "AuthAuthority"
$audienceSecretName = "AuthAudience"
$validIssuerSecretName = "AuthValidIssuer"

# We have to check whether all the relevant secrets are in there.
# If not, generate those secrets and store in Key Vault.
$query = "contains([].id, 'https://$($keyVaultName).vault.azure.net/secrets/$($sqlSaPasswordSecretName)')"
$exists = az keyvault secret list --vault-name $keyVaultName --query $query
if ($exists -eq "false") {
	az keyvault secret set --vault-name $keyVaultName --name $sqlSaPasswordSecretName --value $(NewPassword) --output none
	Write-Host "##[section]Set secret $sqlSaPasswordSecretName"
}

$query = "contains([].id, 'https://$($keyVaultName).vault.azure.net/secrets/$($sqlAdminPasswordSecretName)')"
$exists = az keyvault secret list --vault-name $keyVaultName --query $query
if ($exists -eq "false") {
	az keyvault secret set --vault-name $keyVaultName --name $sqlAdminPasswordSecretName --value $(NewPassword) --output none
	Write-Host "##[section]Set secret $sqlAdminPasswordSecretName"
}

if ($authAuthority -ne $null) {
	$query = "contains([].id, 'https://$($keyVaultName).vault.azure.net/secrets/$($authoritySecretName)')"
	$exists = az keyvault secret list --vault-name $keyVaultName --query $query
	if ($exists -eq "false") {
		az keyvault secret set --vault-name $keyVaultName --name $authoritySecretName --value "$($authAuthority)" --output none
		Write-Host "##[section]Set secret $authoritySecretName"
	}
}

if ($authAudience -ne $null) {
	$query = "contains([].id, 'https://$($keyVaultName).vault.azure.net/secrets/$($audienceSecretName)')"
	$exists = az keyvault secret list --vault-name $keyVaultName --query $query
	if ($exists -eq "false") {
		az keyvault secret set --vault-name $keyVaultName --name $audienceSecretName --value "$($authAudience)" --output none
		Write-Host "##[section]Set secret $audienceSecretName"
	}
}

if ($authValidIssuer -ne $null) {
	$query = "contains([].id, 'https://$($keyVaultName).vault.azure.net/secrets/$($validIssuerSecretName)')"
	$exists = az keyvault secret list --vault-name $keyVaultName --query $query
	if ($exists -eq "false") {
		az keyvault secret set --vault-name $keyVaultName --name $validIssuerSecretName --value "$($authValidIssuer)" --output none
		Write-Host "##[section]Set secret $validIssuerSecretName"
	}
}

################################################################
### Set connection strings.
################################################################
if ($dbConnectionString -ne $null) {
	$query = "contains([].id, 'https://$($keyVaultName).vault.azure.net/secrets/$($dbConnectionName)')"
	$exists = az keyvault secret list --vault-name $keyVaultName --query $query
	if ($exists -eq "false") {
		az keyvault secret set --vault-name $keyVaultName --name $dbConnectionName --value "$($dbConnectionString)" --output none
		Write-Host "##[section]Set secret $dbConnectionName"
	}
}

if ($messageBrokerConnectionString -ne $null) {
	$query = "contains([].id, 'https://$($keyVaultName).vault.azure.net/secrets/$($messageBrokerName)')"
	$exists = az keyvault secret list --vault-name $keyVaultName --query $query
	if ($exists -eq "false") {
		az keyvault secret set --vault-name $keyVaultName --name $messageBrokerName --value "$($messageBrokerConnectionString)" --output none
		Write-Host "##[section]Set secret $messageBrokerName"
	}
}