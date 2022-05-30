# shellcheck disable=SC2034

### Constants
# It's helpful to tag all piipan-related resources
PROJECT_TAG=piipan
RESOURCE_TAGS="{ \"Project\": \"${PROJECT_TAG}\" }"

# Tag filters for system types; descriptions are in iac.md
PER_STATE_ETL_TAG="SysType=PerStateEtl"
PER_STATE_STORAGE_TAG="SysType=PerStateStorage"
ORCHESTRATOR_API_TAG="SysType=OrchestratorApi"
MATCH_RES_API_TAG="SysType=MatchResApi"
DASHBOARD_APP_TAG="SysType=DashboardApp"
QUERY_APP_TAG="SysType=QueryApp"
DUP_PART_API_TAG="SysType=DupPartApi"

# Identity object ID for the Azure environment account
CURRENT_USER_OBJID=$(az ad signed-in-user show --query objectId --output tsv)

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

# Name of the environment variable used to indicate the current state. This is
# used in the bulk upload Azure Function.
STATE_STR_KEY=State

# In the States.csv file, the state is enabled if they have the ENABLED text in column 3. Disabled if they have DISABLED text in column 3.
# Defaults to disabled, so any text other than ENABLED is disabled.
STATE_ENABLED_KEY=ENABLED
STATE_DISABLED_KEY=DISABLED

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

# Core Database Resources
CORE_DB_SERVER_NAME=$PREFIX-psql-core-$ENV
COLLAB_DB_NAME=collaboration

# Event Hub
EVENT_HUB_NAME=$PREFIX-evh-monitoring-$ENV

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

# Match Resolution Function App Info
MATCH_RES_FUNC_APP_NAME=$PREFIX-func-matchres-$ENV
MATCH_RES_FUNC_APP_STORAGE_NAME=${PREFIX}stmatchres${ENV}

# Names of apps authenticated by OIDC
OIDC_APPS=("$QUERY_TOOL_APP_NAME" "$DASHBOARD_APP_NAME")

### END Constants

### Functions
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

### END Functions
