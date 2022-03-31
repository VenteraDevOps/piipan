# shellcheck disable=SC2034

# Deployment environment for resource identifiers
ENV=$(basename "${BASH_SOURCE%.*}")

# Default location
LOCATION=westus2

# Default resource group
RESOURCE_GROUP=rg-core-$ENV

# Resource group for matching API
MATCH_RESOURCE_GROUP=rg-match-$ENV

# Prefix for resource identifiers
PREFIX=tts

# Either AzureCloud or AzureUSGovernment
CLOUD_NAME=AzureCloud

# used to create API Management resources
APIM_EMAIL=noreply@tts.test

# OIDC configuration - all apps
IDP_OIDC_IP_RANGES=""

# OIDC configuration - Dashboard app
DASHBOARD_APP_IDP_OIDC_CONFIG_URI="https://ttsb2cdev.b2clogin.com/ttsb2cdev.onmicrosoft.com/v2.0/.well-known/openid-configuration?p=B2C_1_si"
DASHBOARD_APP_IDP_OIDC_SCOPES='("openid","email","profile")'
DASHBOARD_APP_IDP_CLIENT_ID=e7e769ad-e9bc-4c5f-8c3e-ebaf6cf9cacb

# OIDC configuration - Query Tool app
QUERY_TOOL_APP_IDP_OIDC_CONFIG_URI="https://ttsb2cdev.b2clogin.com/ttsb2cdev.onmicrosoft.com/v2.0/.well-known/openid-configuration?p=B2C_1_si"
QUERY_TOOL_APP_IDP_OIDC_SCOPES='("openid","email","profile")'
QUERY_TOOL_APP_IDP_CLIENT_ID=a8e3c164-77a9-45fd-9950-cc9862aa774a

# SIEM tool app registration name
SIEM_RECEIVER=$PREFIX-siem-tool-$ENV

# Azure Storage SKU for per-state storage accounts and storage accounts backing function apps
STORAGE_SKU="Standard_ZRS" # Standard Zone Redundant Storage
# STORAGE_SKU="Standard_LRS" # Standard Locally Redundant Storage (Use this when ZRS is not available in the region)
