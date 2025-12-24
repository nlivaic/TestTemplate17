param ($environment, $projectName, $resourceGroupName, $sqlAdminUsername, $sqlAdminPassword, $vaultUri)

Write-Host "##[warning]------ Create Resources START ------"
$jsonResultAll = az deployment group create --resource-group $resourceGroupName --template-file ./deployment/iac.bicep --parameters environment=$environment projectName=$projectName sql_admin_username=$sqlAdminUsername sql_admin_password=$sqlAdminPassword vault_uri=$vaultUri| ConvertFrom-Json
$appServiceWebName = $jsonResultAll.properties.outputs.appServiceWebName.value
$sqlServerName = $jsonResultAll.properties.outputs.sqlServerName.value

Write-Host "##vso[task.setvariable variable=appServiceWebName;isoutput=true]$appServiceWebName"
Write-Host "##vso[task.setvariable variable=sqlServerName;isoutput=true]$sqlServerName"
$dbConnection = $jsonResultAll.properties.outputs.dbConnection.value
$messageBrokerConnectionString = $jsonResultAll.properties.outputs.messageBrokerConnectionString.value
Write-Host "##vso[task.setvariable variable=dbConnection;isoutput=true]$dbConnection"
Write-Host "##vso[task.setvariable variable=messageBrokerConnectionString;isoutput=true]$messageBrokerConnectionString"
Write-Host "##[warning]------ Create Resources END ------"
