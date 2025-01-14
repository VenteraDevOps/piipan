# shellcheck disable=SC2034

### Constants
# It's helpful to tag all piipan-related resources
PROJECT_TAG=piipan
RESOURCE_TAGS="{ \"Project\": \"${PROJECT_TAG}\" }"

NOTIFICATIONS_API_SYSTEM_TAG="NotificationsApi"
STATES_API_SYSTEM_TAG="StatesApi"
MATCH_RES_API_SYSTEM_TAG="MatchResApi"

# Tag filters for system types; descriptions are in iac.md
PER_STATE_ETL_TAG="SysType=PerStateEtl"
PER_STATE_STORAGE_TAG="SysType=PerStateStorage"
ORCHESTRATOR_API_TAG="SysType=OrchestratorApi"
MATCH_RES_API_TAG="SysType=${MATCH_RES_API_SYSTEM_TAG}"
STATES_API_TAG="SysType=${STATES_API_SYSTEM_TAG}"
DASHBOARD_APP_TAG="SysType=DashboardApp"
QUERY_APP_TAG="SysType=QueryApp"
DUP_PART_API_TAG="SysType=DupPartApi"
NOTIFICATIONS_API_TAG="SysType=${NOTIFICATIONS_API_SYSTEM_TAG}"


# The default Azure subscription
SUBSCRIPTION_ID=$(az account show --query id -o tsv)

# Name of App Service Plan, used by both query tool and dashboard
APP_SERVICE_PLAN=plan-apps2-$ENV

# App Service Plan used by function apps with VNet integration
APP_SERVICE_PLAN_FUNC_NAME=plan-apps1-$ENV
APP_SERVICE_PLAN_FUNC_SKU=P1V2
APP_SERVICE_PLAN_FUNC_KIND=functionapp

# Name of environment variable used to pass database connection strings
# to app or function code
DB_CONN_STR_KEY=DatabaseConnectionString

# Name of environment variable used to pass blob storage account connection
# strings to app or function code
BLOB_CONN_STR_KEY=BlobStorageConnectionString

# Name of environment variable used to pass Azure Services connection strings
# to app or function code (required to fetch managed identity tokens)
AZ_SERV_STR_KEY=AzureServicesAuthConnectionString

# Name of the environment variable used to indicate the active Azure cloud
# so that application code can use the appropriate, cloud-specific domain
CLOUD_NAME_STR_KEY=CloudName

# Azure Key Vault naming is kebab-case
# Azure Function environment variable naming is PascalCase
# Thus, creating constants for both to avoid naming conflicts

# Column Encryption Key - Azure Function environment variable
COLUMN_ENCRYPT_KEY=ColumnEncryptionKey
# Column Encryption Key - Azure Key Vault secret
COLUMN_ENCRYPT_KEY_KV=column-encryption-key
# Payload Encryption Key - Azure Function environment variable
UPLOAD_ENCRYPT_KEY=UploadPayloadKey
# Payload Encryption Key - Azure Key Vault secret
UPLOAD_ENCRYPT_KEY_KV=upload-payload-key
# Payload Encryption Key SHA - Azure Function environment variable
UPLOAD_ENCRYPT_KEY_SHA=UploadPayloadKeySHA
# Payload Encryption Key SHA - Azure Key Vault secret
UPLOAD_ENCRYPT_KEY_SHA_KV=upload-payload-key-sha

# Name of the environment variable used to indicate the current state. This is
# used in the bulk upload Azure Function.
STATE_STR_KEY=State

# Name of environment variable used to pass event grid connection
# strings to app or function code
EVENTGRID_CONN_STR_ENDPOINT=EventGridEndPoint
EVENTGRID_CONN_STR_KEY=EventGridKeyString

# Event Grid for Search Metrics
EVENTGRID_CONN_METRICS_SEARCH_STR_ENDPOINT=EventGridMetricSearchEndPoint
EVENTGRID_CONN_METRICS_SEARCH_STR_KEY=EventGridMetricSearchKeyString

# Event Grid for Match Metrics
EVENTGRID_CONN_METRICS_MATCH_STR_ENDPOINT=EventGridMetricMatchEndPoint
EVENTGRID_CONN_METRICS_MATCH_STR_KEY=EventGridMetricMatchKeyString

# Event Grid for Match Metrics
EVENTGRID_CONN_NOTIFICATION_STR_ENDPOINT=EventGridNotificationEndPoint
EVENTGRID_CONN_NOTIFICATION_STR_KEY=EventGridNotificationKeyString

# In the States.csv file, the state is enabled if they have the ENABLED text in column 3. Disabled if they have DISABLED text in column 3.
# Defaults to disabled, so any text other than ENABLED is disabled.
STATE_ENABLED_KEY=ENABLED
STATE_DISABLED_KEY=DISABLED
# Name of environment variable used to pass event grid connection
# strings to app or function code
EVENTGRID_CONN_STR_ENDPOINT=EventGridEndPoint
EVENTGRID_CONN_STR_KEY=EventGridKeyString

# For connection strings, our established placeholder values
PASSWORD_PLACEHOLDER='{password}'
DATABASE_PLACEHOLDER='{database}'

# Virtual Network and Subnets
VNET_NAME=vnet-core-$ENV
DB_SUBNET_NAME=snet-participants-$ENV # Subnet that participants database private endpoint uses
DB_2_SUBNET_NAME=snet-core-$ENV # Subnet that core database private endpoint uses
FUNC_SUBNET_NAME=snet-apps1-$ENV # Subnet function apps use
FUNC_NSG_NAME=nsg-apps1-$ENV # Network security groups for function apps subnet
WEBAPP_SUBNET_NAME=snet-apps2-$ENV # Subnet web apps use
WEBAPP_NSG_NAME=nsg-apps2-$ENV # Network security groups for web apps subnet
PRIVATE_ENDPOINT_NAME=pe-participants-$ENV
CORE_DB_PRIVATE_ENDPOINT_NAME=pe-core-$ENV

# Metrics Resources
METRICS_COLLECT_APP_ID=metricscol
METRICS_COLLECT_APP_NAME=${PREFIX}-func-${METRICS_COLLECT_APP_ID}-${ENV}
METRICS_API_APP_ID=metricsapi
METRICS_API_APP_NAME=$PREFIX-func-$METRICS_API_APP_ID-$ENV
METRICS_API_FUNCTION_NAME=GetParticipantUploads
METRICS_API_FUNCTION_NAME_LASTUPLOAD=GetLastUpload
METRICS_API_FUNCTION_NAME_STATISTICS=GetUploadStatistics

# Core Database Resources
CORE_DB_SERVER_NAME=$PREFIX-psql-core-$ENV
COLLAB_DB_NAME=collaboration

# Log Analytics Workspace
LOG_ANALYTICS_WORKSPACE_NAME=${PREFIX}-log-analytics-workspace-${ENV}

# Diagnostic Settings
DIAGNOSTIC_SETTINGS_NAME="stream-logs-to-event-hub"
DIAGNOSTIC_SETTINGS_EVGT_DATA_PLANE="DataPlaneRequests"
DIAGNOSTIC_SETTINGS_EVGT_DELIVERY_FAIL="DeliveryFailures"
DIAGNOSTIC_SETTINGS_EVGT_PUBLISH_FAIL="PublishFailures"
DIAGNOSTIC_SETTINGS_FUNC="FunctionAppLogs"
DIAGNOSTIC_SETTINGS_WORKSPACE="Audit"

# Event Hub
EVENT_HUB_NAMESPACE=$PREFIX-evh-monitoring-$ENV
EVENT_HUB_NAME=logs

# Name of Key Vault
VAULT_NAME=$PREFIX-kv-core-$ENV

# Query Tool App Info
QUERY_TOOL_APP_NAME=$PREFIX-app-querytool-$ENV
QUERY_TOOL_FRONTDOOR_NAME=$PREFIX-fd-querytool-$ENV
QUERY_TOOL_WAF_NAME=wafquerytool${ENV}

# Dashboard App Info
DASHBOARD_APP_NAME=$PREFIX-app-dashboard-$ENV
DASHBOARD_FRONTDOOR_NAME=$PREFIX-fd-dashboard-$ENV
DASHBOARD_WAF_NAME=wafdashboard${ENV}

# Orchestrator Function app and its blob storage
ORCHESTRATOR_FUNC_APP_NAME=$PREFIX-func-orchestrator-$ENV
ORCHESTRATOR_FUNC_APP_STORAGE_NAME=${PREFIX}storchestrator${ENV}

# Match Resolution Function App Info
MATCH_RES_FUNC_APP_NAME=$PREFIX-func-matchres-$ENV
MATCH_RES_FUNC_APP_STORAGE_NAME=${PREFIX}stmatchres${ENV}

# States Function App Info
STATES_FUNC_APP_NAME=$PREFIX-func-states-$ENV
STATES_FUNC_APP_STORAGE_NAME=${PREFIX}ststates${ENV}

# Notifications Function App Info
NOTIFICATIONS_FUNC_APP_NAME=$PREFIX-func-notifications-$ENV
NOTIFICATIONS_FUNC_APP_STORAGE_NAME=${PREFIX}stnotifications${ENV}

# Names of apps authenticated by OIDC
OIDC_APPS=("$QUERY_TOOL_APP_NAME" "$DASHBOARD_APP_NAME")

# Search Metrics Event Grid
CREATE_SEARCH_METRICS_TOPIC_NAME=evgt-create-search-metrics-${ENV}

# Update Metrics Event Grid
UPDATE_BU_METRICS_TOPIC_NAME=evgt-update-bu-metrics-${ENV}
UPDATE_BU_METRICS_CUSTOM_TOPIC=evgs-update-bu-metrics-${ENV}

# Match Metric Event grid
CREATE_MATCH_METRICS_TOPIC_NAME=evgt-publish-match-metrics-$ENV

# Application Insights
APPINSIGHTS_CONNECTION_STRING="connectionString"
APPINSIGHTS_KIND="web"
APPINSIGHTS_INSTRUMENTATIONKEY="APPINSIGHTS_INSTRUMENTATIONKEY"
APPLICATIONINSIGHTS_CONNECTION_STRING="APPLICATIONINSIGHTS_CONNECTION_STRING"

#Search Metric Event grid
CREATE_NOTIFICATIONS_TOPIC_NAME=evgt-create-notifications-$ENV
### END Constants

### Functions
# Return the object ID for the currently logged in account.
# Supports both users and service principals.
current_user_objid () {
  type=$(az account show --query user.type --output tsv)

  if [ "${type}" = "servicePrincipal" ]; then
    app_id=$(az account show --query user.name --output tsv)
    CURRENT_USER_OBJID=$(az ad sp show --id "${app_id}" --query id --output tsv)
  else
    CURRENT_USER_OBJID=$(az ad signed-in-user show --query id --output tsv)
  fi
}
current_user_objid

# Create a very long, (mostly) random password. Ensures all Azure character
# class requirements are met by tacking on a non-random, tailored suffix.
random_password () {
  head /dev/urandom | LC_ALL=C tr -dc "A-Za-z0-9" | head -c 64 ; echo -n 'aA1!'
}

# Generate the ADO.NET connection string for corresponding database. Password
# will be set to PASSWORD_PLACEHOLDER.
pg_connection_string () {
  server=$1
  db=$2
  user=$3
  user=${user//-/_}

  base=$(az postgres show-connection-string \
    --server-name "$server" \
    --database-name "$db" \
    --admin-user "$user" \
    --admin-password "$PASSWORD_PLACEHOLDER" \
    --query connectionStrings.\"ado.net\" \
    -o tsv)

  # See:
  # https://github.com/Azure/azure-cli-extensions/issues/3143
  # https://docs.microsoft.com/en-us/azure/azure-government/compare-azure-government-global-azure
  if [ "$CLOUD_NAME" = "AzureUSGovernment" ]; then
    base=${base/.postgres.database.azure.com/.postgres.database.usgovcloudapi.net}
  fi

  echo "${base}Ssl Mode=VerifyFull;"
}

# Verify that the expected Azure environment is the active cloud
verify_cloud () {
  local cn
  cn=$(az cloud show --query name -o tsv)

  if [ "$CLOUD_NAME" != "$cn" ]; then
    echo "error: '$cn' is the active cloud, expecting '$CLOUD_NAME'" 1>&2
    return 1
  fi
}

# Verify that the FILE exist
verify_file () {

  local FILEPATH
  local FILENAME
  local FILE

  FILEPATH=$1
  FILENAME=$2
  FILE=$FILEPATH$FILENAME

  if [ -f "$FILE" ]; then
      echo "$FILENAME exists."
  else
      echo "$FILENAME does not exist. A $FILENAME file is required to be present at $FILEPATH"
      exit 64
  fi
}

# Run setup environment configuration script
setup_enviroment () {

  local envc
  # local FILENAME
  # local FILE

  ENV_CONFIG=$1
  # FILENAME=$2
  FILE=$ENV_CONFIG/config.bash

  if [ -f "$FILE" ]; then
      echo "Setting up environment configuration $FILE."
      # shellcheck source=./iac/env/tts/dev/config.bash
      source "$FILE" "$ENV_CONFIG"
  else
      echo "No environment configuration found at $FILE."
  fi
}

# Return a space-delimited string of resource names for the resources
# that match the provided SysType tag and are in the specified resource group.
# If no matching resources are found, a non-zero error is returned.
get_resources () {
  local sys_type=$1
  local group=$2

  local res
  res=$(\
    az resource list \
      --tag "$sys_type" \
      --query "[? resourceGroup == '${group}' ].name" \
      -o tsv)

  local as_array=("$res")
  if [[ ${#as_array[@]} -eq 0 ]]; then
    echo "error: no resources found with $sys_type in $group" 1>&2
    return 1
  fi

  echo "$res"
}

# hard-coded switches between commerical and government Azure environments
web_app_host_suffix () {
  if [ "$CLOUD_NAME" = "AzureUSGovernment" ]; then
    echo ".azurewebsites.us"
  else
    echo ".azurewebsites.net"
  fi
}

front_door_host_suffix () {
  if [ "$CLOUD_NAME" = "AzureUSGovernment" ]; then
    echo ".azurefd.us"
  else
    echo ".azurefd.net"
  fi
}

graph_host_suffix () {
  if [ "$CLOUD_NAME" = "AzureUSGovernment" ]; then
    echo ".microsoft.us"
  else
    echo ".microsoft.com"
  fi
}

apim_host_suffix () {
  if [ "$CLOUD_NAME" = "AzureUSGovernment" ]; then
    echo ".azure-api.us"
  else
    echo ".azure-api.net"
  fi
}

resource_manager_host_suffix () {
  if [ "$CLOUD_NAME" = "AzureUSGovernment" ]; then
    echo ".usgovcloudapi.net"
  else
    echo ".azure.com"
  fi
}

state_managed_id_name () {
  abbr=$1
  env=$2

  echo "id-${abbr}admin-${env}"
}

state_event_grid_topic_name () {
  abbr=$1
  env=$2

  echo "evgt-${abbr}upload-${env}"
}

private_dns_zone () {
  base=privatelink.postgres.database.azure.com

  if [ "$CLOUD_NAME" = "AzureUSGovernment" ]; then
    base=${base/.postgres.database.azure.com/.postgres.database.usgovcloudapi.net}
  fi

  echo $base
}

# Azure CLI 2.37 upgraded azure-mgmt-msi version to 2021-09-30-preview. This API
# version will not work with AzureUSGovernment, and must be downgraded.
# https://github.com/Azure/azure-cli/pull/22284
# https://github.com/Azure/azure-cli/issues/22661
# https://github.com/Azure/azure-cli/issues/22735
configure_azure_profile () {
  local cn
  cn=$(\
    az cloud show \
      --query "name" \
      --output tsv)

  if [ "${cn}" = "AzureUSGovernment" ]; then
    profile=$(\
      az cloud show \
        --name "${cn}" \
        --query "profile" \
        --output tsv)

    if [ "${profile}" = "latest" ]; then
      az cloud set -n "${cn}" --profile "2020-09-01-hybrid"
    else
      az cloud set -n "${cn}" --profile "latest"
    fi
  fi
}

# try_run()
#
# The function help with the robusness of the IaC code.
# In ocassions the original when run a command it can fail, because any kind of error.
# The wrapper function will try run the command to a max_tries of times.
#
# mycommand - command to be run
# max_tries - max number of try, default value 3
# directory - path where tje mycommand should be run
#
# usage:   try_run <mycommand> <max_tries> <directory>
#
try_run () {
  mycommand=$1
  max_tries="${2:-3}"
  directory="${3:-"./"}"


  ERR=0 # or some non zero error number you want
  mycommand+=" || ERR=1"

  pushd "$directory" || exit
    for (( i=1; i<=max_tries; i++ ))
      do
        ERR=0
        echo "Running: ${mycommand}"
        eval "$mycommand"

        if [ $ERR -eq 0 ];then
          (( i = max_tries + 1))
        else
          echo "Waiting to retry..."
          sleep $(( i * 30 ))
        fi

      done
    if [ $ERR -eq 1 ];then
      echo "Too many non-sucessful tries to run: ${mycommand}"
      exit $ERR
    fi
  popd || exit

}

_get_oidc_secret_name () {
  local app_name=$1
  echo "${app_name}-oidc-secret"
}

# Given an App Service instance name, establish a placeholder secret
# for OIDC in the core key vault, using a random value. See get_oidc_secret
# for how this secret is used.
# If the secret already exists, no action will be taken.
create_oidc_secret () {
  local app_name=$1

  local secret_name
  secret_name=$(_get_oidc_secret_name "$app_name")

  local secret_id
  secret_id=$(\
    az keyvault secret list \
      --vault-name "$VAULT_NAME" \
      --query "[?name == '${secret_name}'].id" \
      --output tsv)

  if [ -z "$secret_id" ]; then
    echo "creating $secret_name"

    local value
    value=$(random_password)
    set_oidc_secret "$app_name" "$value"
  else
    echo "$secret_name already exists, no action taken"
  fi
}

# Given an App Service instance name and a secret value, set the
# corresponding secret in the core key vault. See get_oidc_secret
# for how this secret is used.
set_oidc_secret () {
  local app_name=$1
  local value=$2

  local secret_name
  secret_name=$(_get_oidc_secret_name "$app_name")

  # use builtin and /dev/stdin so as to not expose secret in process listing
  printf '%s' "$value" | az keyvault secret set \
    --vault-name "$VAULT_NAME" \
    --name "$secret_name" \
    --file /dev/stdin \
    --query id > /dev/null
    #--value "$value"

}

# Given an App Service instance name, output the secret established for OIDC,
# fetching it from the core key vault.
#
# This value is the client secret used when authenticating the OIDC Relying
# Party (i.e., the web app)  to the configured OIDC Identity Provider (IdP)
# under the Authorization Code Flow.
get_oidc_secret () {
  local app_name=$1

  local secret_name
  secret_name=$(_get_oidc_secret_name "$app_name")

  az keyvault secret show \
    --vault-name "$VAULT_NAME" \
    --name "$secret_name" \
    --query value \
    --output tsv
}

# Generate the eventgrid key string for the corresponding
eventgrid_endpoint () {
  group=$1
  name=$2

  az eventgrid topic show \
  --name "$name" \
  -g "$group" \
  --query "endpoint" \
  -o tsv
}

# Generate the eventgrid key string for the corresponding
eventgrid_key_string () {
  group=$1
  name=$2

  az eventgrid topic key list \
    --name "$name" \
    --resource-group "$group" \
    --query "key2" \
    -o tsv
}

# Many CLI commands use a URI to identify nested resources; pre-compute the URIs
set_defaults () {
    # Default Providers
    DEFAULT_PROVIDERS="/subscriptions/${SUBSCRIPTION_ID}/resourceGroups/${RESOURCE_GROUP}/providers"

    # Log Analytics Workspace Id
    LOG_ANALYTICS_WORKSPACE_ID="${DEFAULT_PROVIDERS}/microsoft.operationalinsights/workspaces/${LOG_ANALYTICS_WORKSPACE_NAME}"

    # Function apps need an Event Hub authorization rule Id for log streaming
    EH_RULE_ID="${DEFAULT_PROVIDERS}/Microsoft.EventHub/namespaces/${EVENT_HUB_NAMESPACE}/authorizationRules/RootManageSharedAccessKey"
}

# Update Diagnostic Settings for a resource and type
# This is a temporary method to handle diagnostic updates, until all
# Azure resource creation is coverted to ARM and the AZ CLI bug is fixed.
# AZ CLI has a bug, so this must be done through REST API
# https://github.com/Azure/azure-cli/issues/22572
update_diagnostic_settings () {
  local resource=$1
  local type=$2

  local logs='"category": "'"${type}"'"'
  local json='{
    "properties": {
      "eventHubAuthorizationRuleId": "'"${EH_RULE_ID}"'",
      "eventHubName": "'"${EVENT_HUB_NAME}"'",
      "workspaceId": "'"${LOG_ANALYTICS_WORKSPACE_ID}"'",
      "logs": [
        {
          '"${logs}"',
          "enabled": true
        }
      ]
    }
  }'
  submit_diagnostic_settings "${resource}" "${json}"
}

# Add Diagnostic Settings for an Event Grid Topic (not System Topic)
# For Event Grid System Topic, use update_diagnostic_settings()
# This is a temporary method to handle diagnostic updates, until all
# Azure resource creation is coverted to ARM and the AZ CLI bug is fixed.
# AZ CLI has a bug, so this must be done through REST API
# https://github.com/Azure/azure-cli/issues/22572
update_event_grid_topic_diagnostic_settings () {
  local resource=$1

  local json='{
    "properties": {
      "eventHubAuthorizationRuleId": "'"${EH_RULE_ID}"'",
      "eventHubName": "'"${EVENT_HUB_NAME}"'",
      "workspaceId": "'"${LOG_ANALYTICS_WORKSPACE_ID}"'",
      "logs": [
        {
          "category": "'"${DIAGNOSTIC_SETTINGS_EVGT_DATA_PLANE}"'",
          "enabled": true
        },
        {
          "category": "'"${DIAGNOSTIC_SETTINGS_EVGT_DELIVERY_FAIL}"'",
          "enabled": true
        },
        {
          "category": "'"${DIAGNOSTIC_SETTINGS_EVGT_PUBLISH_FAIL}"'",
          "enabled": true
        }
      ]
    }
  }'
  submit_diagnostic_settings "${resource}" "${json}"
}

# Submit the Diagnostic Setting request
submit_diagnostic_settings () {
  local resource=$1
  local content=$2

  mgmt_domain=$(resource_manager_host_suffix)
  az rest \
    --method PUT \
    --uri "https://management${mgmt_domain}/${resource}/providers/Microsoft.Insights/diagnosticSettings/${DIAGNOSTIC_SETTINGS_NAME}?api-version=2021-05-01-preview" \
    --headers 'Content-Type=application/json' \
    --body "${content}"
}
### END Functions

