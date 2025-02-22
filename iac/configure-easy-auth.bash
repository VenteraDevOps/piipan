#!/usr/bin/env bash
# Configures Easy Auth for internal API communication within Piipan.
# Requires specific privileges on Azure Active Directory that the
# subscription Global Administrator role has, but a subscription
# Contributor does not.
#
# azure-env is the name of the deployment environment (e.g., "tts/dev").
# See iac/env for available environments.
#
# usage: configure-easy-auth.bash <azure-env>

# shellcheck source=./tools/common.bash
source "$(dirname "$0")"/../tools/common.bash || exit

# App Service Authentication is done at the Azure tenant level
TENANT_ID=$(az account show --query homeTenantId -o tsv)

# Generate the necessary JSON object for assigning an app role to
# a service principal or managed identity
app_role_assignment () {
  principalId=$1
  resourceId=$2
  appRoleId=$3

  echo "\
  {
    \"principalId\": \"${principalId}\",
    \"resourceId\": \"${resourceId}\",
    \"appRoleId\": \"${appRoleId}\"
  }"
}

# Generate the necessary JSON object for adding an app role
# to an Active Directory app registration
app_role_manifest () {
  role=$1

  # shellcheck disable=SC2089
  json="\
  [{
    \"allowedMemberTypes\": [
      \"User\",
      \"Application\"
    ],
    \"description\": \"Grants application access\",
    \"displayName\": \"Authorized client\",
    \"isEnabled\": true,
    \"origin\": \"Application\",
    \"value\": \"${role}\"
  }]"
  echo "$json"
}

# Generate the necessary JSON object for updating a service principal
# to require an appRoleAssignment.
# https://docs.microsoft.com/en-us/graph/api/resources/approleassignment?view=graph-rest-1.0
app_role_required () {
  echo "\
  {
    \"appRoleAssignmentRequired\": true
  }"
}

# Generate the application ID URI for a given app, using the default format
# https://docs.microsoft.com/en-us/azure/active-directory/develop/security-best-practices-for-app-registration#appid-uri-configuration
app_id_uri () {
  app=$1

  app_aad_client=$(\
    az ad app list \
      --display-name "${app}" \
      --filter "displayName eq '${app}'" \
      --query "[0].appId" \
      --output tsv)

  echo "api://${app_aad_client}"
}

# Update an Active Directory app registration with an application
# role for a given application.
configure_aad_app_reg () {
  app=$1
  role=$2
  group=$3

  object_id=$(\
    az ad app list \
      --display-name "${app}" \
      --filter "displayName eq '${app}'" \
      --query "[0].id" \
      --output tsv)

  # First configure the app registration with the application role
  # Running `az ad app update` with the `--app-roles` parameter will throw
  # an error if the app already exists and the app role is enabled.
  exists=$(\
    az ad app list \
    --display-name "${app}" \
    --filter "displayName eq '${app}'" \
    --query "[0].appRoles[?value == '${role}'].value" \
    --output tsv)
  if [ -z "$exists" ]; then
    app_role=$(app_role_manifest "$role")
    az ad app update \
      --id "$object_id" \
      --display-name "$app" \
      --app-roles "${app_role}"
  fi

  # Set the application ID URI and limit app to the current tenant
  app_uri=$(app_id_uri "$app")
  az ad app update \
    --id "$object_id" \
    --sign-in-audience "AzureADMyOrg" \
    --identifier-uris "$app_uri"

  echo "$object_id"
}

# Create a service principal associated with a given AAD
# application registration
create_aad_app_sp () {
  app=$1
  aad_app_id=$2
  filter="displayName eq '${app}' and servicePrincipalType eq 'Application'"

  # `az ad sp create` throws error if service principal exits
  sp=$(\
    az ad sp list \
    --display-name "$app" \
    --filter "${filter}" \
    --query "[0].id" \
    --output tsv)
  if [ -z "$sp" ]; then
    sp=$(\
      az ad sp create \
        --id "$aad_app_id" \
        --query id \
        --output tsv)
  fi

  echo "$sp"
}

# Assign an application role to a service principal (generally in
# the form of a managed identity)
assign_app_role () {
  echo "Assigning application role: $3"
  resource_id=$1
  principal_id=$2
  role=$3
  role_id=$(\
    az ad sp show \
    --id "$resource_id" \
    --query "appRoles[?value == '${role}'].id" \
    --output tsv)

  domain=$(graph_host_suffix)

  # Any client that attemps authentication must be assigned a role
  update_json=$(app_role_required)
  az rest \
  --method PATCH \
  --uri "https://graph${domain}/v1.0/servicePrincipals/${resource_id}" \
  --headers 'Content-Type=application/json' \
  --body "${update_json}"

  # Similar to `az ad app create`, `az rest` will throw error when assigning
  # an app role to an identity that already has the role.
  exists=$(\
    az rest \
    --method GET \
    --uri "https://graph${domain}/v1.0/servicePrincipals/${resource_id}/appRoleAssignedTo" \
    --query "value[?principalId == '${principal_id}'].appRoleId" \
    --output tsv)

  if [ -z "$exists" ]; then
    role_json=$(app_role_assignment "$principal_id" "$resource_id" "$role_id")
    echo "$role_json"
    az rest \
    --method POST \
    --uri "https://graph${domain}/v1.0/servicePrincipals/${resource_id}/appRoleAssignedTo" \
    --headers 'Content-Type=application/json' \
    --body "$role_json"
  fi
}

# Activate App Service authentication (Easy Auth) for an app
# service or function app, and require app role assignment.
# Assumes Active Directory application and associated service
# principal already exist for the app
enable_easy_auth () {
  app=$1
  group=$2

  app_aad_client=$(\
    az ad app list \
      --display-name "${app}" \
      --filter "displayName eq '${app}'" \
      --query "[0].id" \
      --output tsv)

  app_uri=$(app_id_uri "$app")

  aad_endpoint=$(\
    az cloud show \
      --query endpoints.activeDirectory \
      --output tsv)

  echo "Configuring Easy Auth settings for ${app}"
  az webapp auth update \
    --resource-group "$group" \
    --name "$app" \
    --aad-allowed-token-audiences "$app_uri" \
    --aad-client-id "$app_aad_client" \
    --aad-token-issuer-url "${aad_endpoint}/${TENANT_ID}/" \
    --enabled true \
    --action LoginWithAzureActiveDirectory
}

# Configures App Service Authentication (aka Easy Auth) for an API provider
# (a Function App) and an API client (either a Function App or App Service):
#    - Registers an Azure Active Directory (AAD) app with an application role
#      for the API provider.
#    - Create a service principal (SP) for the app registation.
#    - Add the application role to the client identity.
#    - Configure and enable App Service Authentiction (i.e., Easy Auth)
#      for the API provider.
#    - Enable requirement that authentication tokens are only issued to client
#      applications that are assigned an app role.
#
# <func> is the name of the API provider Function App
# <group> is the resource group <func> belongs to
# <role> is the Piipan role name
# <client_identity> is the principal id of the client Function App/App service
configure_easy_auth_pair () {
  local func=$1
  local group=$2
  local role=$3
  local client_identity=$4

  local func_app_reg_id
  func_app_reg_id=$(configure_aad_app_reg "$func" "$role" "$group")

  # Wait a bit to prevent "service principal being created must in the local tenant" error
  sleep 60
  local func_app_sp
  func_app_sp=$(create_aad_app_sp "$func" "$func_app_reg_id")

  # Activate App Service Authentication for the Function App API
  # Wait a bit so we can find the SP in AAD when we search for it
  sleep 60
  enable_easy_auth "$func" "$group"

  # Give the client component access to the Function App API
  # Wait a bit to prevent ResourceNotFoundError
  sleep 60
  assign_app_role "$func_app_sp" "$client_identity" "$role"
}

main () {
  # Load agency/subscription/deployment-specific settings
  azure_env=$1
  source "$(dirname "$0")"/env/"${azure_env}".bash
  source "$(dirname "$0")"/iac-common.bash
  verify_cloud

  # Name of application roles authorized to call match APIs
  ORCH_API_APP_ROLE='OrchestratorApi.Query'
  METRICS_API_APP_ROLE='Metrics.Read'
  MATCH_RESOLUTION_API_APP_ROLE='MatchResolution.Query'
  STATES_API_APP_ROLE='States.Query'
  ETL_API_APP_ROLE='Etl.Query'

  orch_name=$(get_resources "$ORCHESTRATOR_API_TAG" "$MATCH_RESOURCE_GROUP")

  match_resolution_name=$(get_resources "$MATCH_RES_API_TAG" "$MATCH_RESOURCE_GROUP")

  states_name=$(get_resources "$STATES_API_TAG" "$RESOURCE_GROUP")

  query_tool_name=$(get_resources "$QUERY_APP_TAG" "$RESOURCE_GROUP")

  dp_api_name=$(get_resources "$DUP_PART_API_TAG" "$MATCH_RESOURCE_GROUP")

  dashboard_name=$(get_resources "$DASHBOARD_APP_TAG" "$RESOURCE_GROUP")

  etl_function_names=($(get_resources "${PER_STATE_ETL_TAG}" "${RESOURCE_GROUP}"))

  metrics_api_name=$METRICS_API_APP_NAME

  query_tool_identity=$(\
    az webapp identity show \
      --name "$query_tool_name" \
      --resource-group "$RESOURCE_GROUP" \
      --query principalId \
      --output tsv)

  apim_identity=$(\
    az apim show \
      --name "$dp_api_name" \
      --resource-group "$MATCH_RESOURCE_GROUP" \
      --query identity.principalId \
      --output tsv)

  dashboard_identity=$(\
    az webapp identity show \
      --name "$dashboard_name" \
      --resource-group "$RESOURCE_GROUP" \
      --query principalId \
      --output tsv)

  echo "Configure Easy Auth for OrchestratorApi and QueryApp"
  configure_easy_auth_pair \
    "$orch_name" "$MATCH_RESOURCE_GROUP" \
    "$ORCH_API_APP_ROLE" \
    "$query_tool_identity"

  echo "Configure Easy Auth for OrchestratorApi and APIM DupPartApi"
  configure_easy_auth_pair \
    "$orch_name" "$MATCH_RESOURCE_GROUP" \
    "$ORCH_API_APP_ROLE" \
    "$apim_identity"

  echo "Configure Easy Auth for Dashboard and MetricsApi"
  configure_easy_auth_pair \
    "$metrics_api_name" "$RESOURCE_GROUP" \
    "$METRICS_API_APP_ROLE" \
    "$dashboard_identity"

  echo "Configure Easy Auth for MatchResolutionApi and QueryApp"
  configure_easy_auth_pair \
    "$match_resolution_name" "$MATCH_RESOURCE_GROUP" \
    "$MATCH_RESOLUTION_API_APP_ROLE" \
    "$query_tool_identity"

  echo "Configure Easy Auth for StatesApi and QueryApp"
  configure_easy_auth_pair \
    "$states_name" "$RESOURCE_GROUP" \
    "$STATES_API_APP_ROLE" \
    "$query_tool_identity"

  echo "Loop through every State ETL Function and enable Easy Auth between the function and (1) Dashboard AND (2) APIM"
  for etl_app_name in "${etl_function_names[@]}"
    do
      etl_app_name="${etl_app_name/$'\r'/}"
      configure_easy_auth_pair \
        "${etl_app_name}" "${RESOURCE_GROUP}" \
        "${ETL_API_APP_ROLE}" \
        "${apim_identity}"

      etl_app_name="${etl_app_name/$'\r'/}"
      configure_easy_auth_pair \
        "${etl_app_name}" "${RESOURCE_GROUP}" \
        "${ETL_API_APP_ROLE}" \
        "${dashboard_identity}"
    done

  script_completed
}

main "$@"
