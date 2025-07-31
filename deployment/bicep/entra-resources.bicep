// EntraExternalId-specific Azure resources
// Creates B2C tenant configuration and OAuth 2.0 application settings

@description('Primary location for all resources')
param location string

@description('Resource token to make resource names unique')
param resourceToken string

@description('Tags to apply to all resources')
param tags object = {}

@description('Entra External ID tenant domain')
param entraExternalIdTenant string

@description('Entra External ID application client ID')
param entraExternalIdClientId string

@description('Key Vault name for storing additional EntraExternalId secrets')
param keyVaultName string

// Reference to existing Key Vault
resource keyVault 'Microsoft.KeyVault/vaults@2023-07-01' existing = {
  name: keyVaultName
}

// Store EntraExternalId OIDC configuration endpoints as secrets for easy reference
resource entraAuthorityUrl 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  parent: keyVault
  name: 'entra-authority-url'
  properties: {
    value: 'https://${entraExternalIdTenant}.b2clogin.com/${entraExternalIdTenant}.onmicrosoft.com/v2.0'
    contentType: 'Entra External ID OAuth 2.0 authority URL'
  }
}

resource entraWellKnownEndpoint 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  parent: keyVault
  name: 'entra-well-known-endpoint'
  properties: {
    value: 'https://${entraExternalIdTenant}.b2clogin.com/${entraExternalIdTenant}.onmicrosoft.com/v2.0/.well-known/openid_configuration'
    contentType: 'Entra External ID OIDC well-known configuration endpoint'
  }
}

resource entraJwksUri 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  parent: keyVault
  name: 'entra-jwks-uri'
  properties: {
    value: 'https://${entraExternalIdTenant}.b2clogin.com/${entraExternalIdTenant}.onmicrosoft.com/discovery/v2.0/keys'
    contentType: 'Entra External ID JWKS endpoint for token validation'
  }
}

resource entraTokenEndpoint 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  parent: keyVault
  name: 'entra-token-endpoint'
  properties: {
    value: 'https://${entraExternalIdTenant}.b2clogin.com/${entraExternalIdTenant}.onmicrosoft.com/oauth2/v2.0/token'
    contentType: 'Entra External ID OAuth 2.0 token endpoint'
  }
}

resource entraAuthorizationEndpoint 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  parent: keyVault
  name: 'entra-authorization-endpoint'
  properties: {
    value: 'https://${entraExternalIdTenant}.b2clogin.com/${entraExternalIdTenant}.onmicrosoft.com/oauth2/v2.0/authorize'
    contentType: 'Entra External ID OAuth 2.0 authorization endpoint'
  }
}

// Store recommended OAuth 2.0 scopes for Fabrikam API
resource entraRecommendedScopes 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  parent: keyVault
  name: 'entra-recommended-scopes'
  properties: {
    value: 'openid profile email offline_access https://fabrikamapi/read https://fabrikamapi/write'
    contentType: 'Recommended OAuth 2.0 scopes for Fabrikam API access'
  }
}

// Store redirect URIs template for easy configuration
resource entraRedirectUris 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  parent: keyVault
  name: 'entra-redirect-uris'
  properties: {
    value: 'https://app-${resourceToken}.azurewebsites.net/signin-oidc,https://app-${resourceToken}.azurewebsites.net/auth/callback'
    contentType: 'OAuth 2.0 redirect URIs for this deployment'
  }
}

// Output the OAuth 2.0 endpoints for easy access
output ENTRA_AUTHORITY_URL string = 'https://${entraExternalIdTenant}.b2clogin.com/${entraExternalIdTenant}.onmicrosoft.com/v2.0'
output ENTRA_WELL_KNOWN_ENDPOINT string = 'https://${entraExternalIdTenant}.b2clogin.com/${entraExternalIdTenant}.onmicrosoft.com/v2.0/.well-known/openid_configuration'
output ENTRA_JWKS_URI string = 'https://${entraExternalIdTenant}.b2clogin.com/${entraExternalIdTenant}.onmicrosoft.com/discovery/v2.0/keys'
output ENTRA_TOKEN_ENDPOINT string = 'https://${entraExternalIdTenant}.b2clogin.com/${entraExternalIdTenant}.onmicrosoft.com/oauth2/v2.0/token'
output ENTRA_AUTHORIZATION_ENDPOINT string = 'https://${entraExternalIdTenant}.b2clogin.com/${entraExternalIdTenant}.onmicrosoft.com/oauth2/v2.0/authorize'
output ENTRA_CLIENT_ID string = entraExternalIdClientId
output ENTRA_TENANT_DOMAIN string = entraExternalIdTenant
output ENTRA_REDIRECT_URIS string = 'https://app-${resourceToken}.azurewebsites.net/signin-oidc,https://app-${resourceToken}.azurewebsites.net/auth/callback'
