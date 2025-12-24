param ($location, $environment, $projectName)

# Resource group
Write-Host "##[warning]------ Create Resource Group START ------"
$jsonResultRg = az deployment sub create --location $location --template-file ./deployment/resource-group.bicep --parameters environment=$environment projectName=$projectName location=$location | ConvertFrom-Json
$resourceGroupName = $jsonResultRg.properties.outputs.resourceGroupName.value
Write-Host "##[section]Created resource group named: $($resourceGroupName)"
Write-Host "##vso[task.setvariable variable=resourceGroupName;isoutput=true]$resourceGroupName"
Write-Host "##[warning]------ Create Resource Group END ------"
