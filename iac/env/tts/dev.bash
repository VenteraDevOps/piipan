# shellcheck disable=SC2034 

# Deployment environment for resource identifiers
ENV=$(basename "${BASH_SOURCE%.*}")

# Default location
LOCATION=eastus

# Default resource group
RESOURCE_GROUP=rg-core-$ENV

# Resource group for matching API
MATCH_RESOURCE_GROUP=rg-match-$ENV

# Prefix for resource identifiers
PREFIX=vennac

# Either AzureCloud or AzureUSGovernment
CLOUD_NAME=AzureCloud

# used to create API Management resources
APIM_EMAIL=noreply@ventera.test

# OIDC configuration - all apps
IDP_OIDC_IP_RANGES=""

# OIDC configuration - Dashboard app
DASHBOARD_APP_IDP_OIDC_CONFIG_URI="https://venteranac.b2clogin.com/venteranac.onmicrosoft.com/v2.0/.well-known/openid-configuration?p=B2C_1_SI"
DASHBOARD_APP_IDP_OIDC_SCOPES='("openid","email","profile")'
DASHBOARD_APP_IDP_CLIENT_ID=cdfcf55a-5ac5-4f7d-ba9d-46217a41f9aa

# OIDC configuration - Query Tool app
QUERY_TOOL_APP_IDP_OIDC_CONFIG_URI="https://venteranac.b2clogin.com/venteranac.onmicrosoft.com/v2.0/.well-known/openid-configuration?p=B2C_1_SI"
QUERY_TOOL_APP_IDP_OIDC_SCOPES='("openid","email","profile")'
QUERY_TOOL_APP_IDP_CLIENT_ID=9c2d426c-0748-42fd-a902-07b261759020

# SIEM tool app registration name
SIEM_RECEIVER=$PREFIX-siem-tool-$ENV

# Azure Storage SKU for per-state storage accounts and storage accounts backing function apps
STORAGE_SKU="Standard_ZRS" # Standard Zone Redundant Storage
# STORAGE_SKU="Standard_LRS" # Standard Locally Redundant Storage (Use this when ZRS is not available in the region)
