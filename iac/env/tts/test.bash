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

# OIDC configuration - Global
IDP_OIDC_IP_RANGES=""

# OIDC configuration - Dashboard app
DASHBOARD_APP_IDP_OIDC_CONFIG_URI="https://ttsb2cdev.b2clogin.com/ttsb2cdev.onmicrosoft.com/v2.0/.well-known/openid-configuration?p=B2C_1_si"
DASHBOARD_APP_IDP_OIDC_SCOPES='("openid","email","profile")'
DASHBOARD_APP_IDP_CLIENT_ID=d7281a84-817d-4a76-8005-b29f67594340

# OIDC configuration - Query Tool app
QUERY_TOOL_APP_IDP_OIDC_CONFIG_URI="https://ttsb2cdev.b2clogin.com/ttsb2cdev.onmicrosoft.com/v2.0/.well-known/openid-configuration?p=B2C_1_si"
QUERY_TOOL_APP_IDP_OIDC_SCOPES='("openid","email","profile")'
QUERY_TOOL_APP_IDP_CLIENT_ID=71286b1e-5f5a-4757-ab5f-714802f33277

# SIEM tool app registration name
SIEM_RECEIVER=$PREFIX-siem-tool-$ENV

# Azure Storage SKU for per-state storage accounts and storage accounts backing function apps
STORAGE_SKU="Standard_ZRS"

# Azure API Management (APIM) SKU
APIM_SKU="Developer"
