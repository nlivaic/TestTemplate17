param environment string
param projectName string
param location string = resourceGroup().location
param tenantId string

// Object containing a mapping for location / region code
var regionCodes = {
  westeurope: 'we'
}   
// remove space and make sure all lower case
var sanitizedLocation = toLower(replace(location, ' ', ''))
// get the region code
var regionCode = regionCodes[sanitizedLocation]
// naming convention
var kv = 'kv'

var baseName = '${regionCode}${environment}${projectName}'

var keyvault_name = toLower('${baseName}${kv}1')

resource keyvault 'Microsoft.KeyVault/vaults@2023-07-01' = {
  name: keyvault_name
  location: location
  properties: {
    sku: {
      family: 'A'
      name: 'standard'
    }
    tenantId: tenantId
    accessPolicies: []
    enabledForDeployment: false
    enabledForDiskEncryption: false
    enabledForTemplateDeployment: false
    enableSoftDelete: true
    softDeleteRetentionInDays: 90
    enableRbacAuthorization: true
    vaultUri: 'https://${keyvault_name}.vault.azure.net/'
    provisioningState: 'Succeeded'
    publicNetworkAccess: 'Enabled'
  }
}

output keyVaultName string = keyvault_name
