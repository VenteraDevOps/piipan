#!/usr/bin/env bash
#
# Provisions and configures the infrastructure components for all Piipan
# subsystems. Assumes an Azure user with the Global Administrator role
# has signed in with the Azure CLI. Must be run from a trusted network.
# See install-extensions.bash for prerequisite Azure CLI extensions.
#
# azure-env is the name of the deployment environment (e.g., "tts/dev").
# See iac/env for available environments.
#
# usage: create-resources.bash <azure-env>

# shellcheck source=./tools/common.bash
source "$(dirname "$0")"/../tools/common.bash || exit

set_constants () {
  # Name of secret used to store the PostgreSQL server admin password
  PG_SECRET_NAME=particpants-records-admin

  # Name of administrator login for PostgreSQL server
  PG_SUPERUSER=postgres

  # Name of Azure Active Directory admin for PostgreSQL server
  PG_AAD_ADMIN=piipan-admins

  # Name of PostgreSQL server
  PG_SERVER_NAME=$PREFIX-psql-participants-$ENV

  # Orchestrator Function app and its blob storage
  ORCHESTRATOR_FUNC_APP_NAME=$PREFIX-func-orchestrator-$ENV
  ORCHESTRATOR_FUNC_APP_STORAGE_NAME=${PREFIX}storchestrator${ENV}

  PRIVATE_DNS_ZONE=$(private_dns_zone)

  # Query rool cWAF custom rule
  WAF_CUSTOM_RULE_NAME=rateLimitRuleByRequestMethod
}

# Generate the storage account connection string for the corresponding
# blob storage account.
# XXX Uses the secondary access key (aka `key2`) for internal access, reserving
#     the primary access key (aka `key1`) for state access. Improve by replacing
#     with share access signatures (SAS URLs) via managed identities at runtime.
blob_connection_string () {
  resource_group=$1
  name=$2

  az storage account show-connection-string \
    --key secondary \
    --resource-group "$resource_group" \
    --name "$name" \
    --query connectionString \
    -o tsv
}

# From a managed identity name, generate the value for
# AzureServicesAuthConnectionString
az_connection_string () {
  resource_group=$1
  identity=$2

  client_id=$(\
    az identity show \
      --resource-group "$resource_group" \
      --name "$identity" \
      --query clientId \
      --output tsv)

    echo "RunAs=App;AppId=${client_id}"
}

main () {
  # Load agency/subscription/deployment-specific settings
  azure_env=$1
  # shellcheck source=./iac/env/tts/dev.bash
  source "$(dirname "$0")"/env/"${azure_env}".bash
  # shellcheck source=./iac/iac-common.bash
  source "$(dirname "$0")"/iac-common.bash
  verify_cloud

  set_constants

  ./create-resource-groups.bash "$azure_env"

  # Virtual network is used to secure connections between
  # participant records database and all apps that communicate with it.
  # Apps will be integrated with VNet as they're created.
  # echo "Creating Virtual Network and Subnets"
  # az deployment group create \
  #   --name "$VNET_NAME" \
  #   --resource-group "$RESOURCE_GROUP" \
  #     --template-file ./arm-templates/virtual-network.json \
  #     --parameters \
  #       location="$LOCATION" \
  #       resourceTags="$RESOURCE_TAGS" \
  #       vnetName="$VNET_NAME" \
  #       peParticipantsSubnetName="$DB_SUBNET_NAME" \
  #       peCoreSubnetName="$DB_2_SUBNET_NAME" \
  #       funcAppServicePlanSubnetName="$FUNC_SUBNET_NAME" \
  #       funcAppServicePlanNsgName="$FUNC_NSG_NAME" \
  #       webAppServicePlanSubnetName="$WEBAPP_SUBNET_NAME" \
  #       webAppServicePlanNsgName="$WEBAPP_NSG_NAME" \
  #       idpOidcIpRanges="$IDP_OIDC_IP_RANGES"

  # Many CLI commands use a URI to identify nested resources; pre-compute the URI's prefix
  # for our default resource group
  DEFAULT_PROVIDERS=/subscriptions/${SUBSCRIPTION_ID}/resourceGroups/${RESOURCE_GROUP}/providers

  # Several CLI commands use the vnet resource ID
  # Resource ID required when vnet is in a separate resource group
  VNET_ID=$(\
    az network vnet show \
      -n "$VNET_NAME" \
      -g "$RESOURCE_GROUP" \
      --query id \
      -o tsv)

  # Create an Event Hub namespace and hub where resource logs will be streamed,
  # as well as an application registration that can be used to read logs
  siem_app_id=$(\
    az ad sp list \
      --display-name "$SIEM_RECEIVER" \
      --filter "displayname eq '$SIEM_RECEIVER'" \
      --query "[0].objectId" \
      --output tsv)
  # Avoid resetting password by only creating app registration if it does not exist
  if [ -z "$siem_app_id" ]; then
    siem_app_id=$(\
      az ad sp create-for-rbac \
        --name "$SIEM_RECEIVER" \
        --role Reader \
        --query objectId \
        --output tsv)

    # Wait bit to avoid "InvalidPrincipalId" on app registration use below
    sleep 60
  fi

  # Create event hub and assign role to app registration
  # az deployment group create \
  #   --name monitoring \
  #   --resource-group "$RESOURCE_GROUP" \
  #   --template-file  ./arm-templates/event-hub-monitoring.json \
  #   --parameters \
  #     resourceTags="$RESOURCE_TAGS" \
  #     location="$LOCATION" \
  #     env="$ENV" \
  #     prefix="$PREFIX" \
  #     receiverId="$siem_app_id"

  # # Create a key vault which will store credentials for use in other templates
  # az deployment group create \
  #   --name "$VAULT_NAME" \
  #   --resource-group "$RESOURCE_GROUP" \
  #   --template-file ./arm-templates/key-vault.json \
  #   --parameters \
  #     name="$VAULT_NAME" \
  #     location="$LOCATION" \
  #     objectId="$CURRENT_USER_OBJID" \
  #     resourceTags="$RESOURCE_TAGS" \
  #     eventHubName="$EVENT_HUB_NAME"


  # For each participating state, create a separate storage account.
  # Each account has a blob storage container named `upload`.
  while IFS=, read -r abbr name ; do
      abbr=$(echo "$abbr" | tr '[:upper:]' '[:lower:]')
      func_stor_name=${PREFIX}st${abbr}upload${ENV}
      echo "Creating storage for $name ($func_stor_name)"
      az deployment group create \
      --name "$func_stor_name" \
      --resource-group "$RESOURCE_GROUP" \
      --template-file ./arm-templates/blob-storage.json \
      --parameters \
        storageAccountName="$func_stor_name" \
        resourceTags="$RESOURCE_TAGS" \
        location="$LOCATION" \
        vnet="$VNET_ID" \
        subnet="$FUNC_SUBNET_NAME" \
          sku="$STORAGE_SKU" \
        eventHubNamespace="$EVENT_HUB_NAME"
  done < states.csv




 



  # This is a subscription-level resource provider
  # az provider register --wait --namespace Microsoft.EventGrid


  # Function apps need an Event Hub authorization rule ID for log streaming
  eh_rule_id=$(\
    az eventhubs namespace authorization-rule list \
      --resource-group "$RESOURCE_GROUP" \
      --namespace-name "$EVENT_HUB_NAME" \
      --query "[?name == 'RootManageSharedAccessKey'].id" \
      -o tsv)

  state_abbrs=""
  while IFS=, read -r abbr name ; do
    abbr=$(echo "$abbr" | tr '[:upper:]' '[:lower:]')
    state_abbrs+=",${abbr}"
  done < states.csv
  state_abbrs=${state_abbrs:1}

  # Create orchestrator-level Function app using ARM template and
  # deploy project code using functions core tools. Networking
  # restrictions for the function app and storage account are added
  # in a separate step to avoid deployment and publishing issues.
  # db_conn_str=$(pg_connection_string "$PG_SERVER_NAME" "$DATABASE_PLACEHOLDER" "$ORCHESTRATOR_FUNC_APP_NAME")
  # collab_db_conn_str=$(pg_connection_string "$CORE_DB_SERVER_NAME" "$COLLAB_DB_NAME" "$ORCHESTRATOR_FUNC_APP_NAME")
  # az deployment group create \
  #   --name orch-api \
  #   --resource-group "$MATCH_RESOURCE_GROUP" \
  #   --template-file  ./arm-templates/function-orch-match.json \
  #   --parameters \
  #     resourceTags="$RESOURCE_TAGS" \
  #     location="$LOCATION" \
  #     functionAppName="$ORCHESTRATOR_FUNC_APP_NAME" \
  #     appServicePlanName="$APP_SERVICE_PLAN_FUNC_NAME" \
  #     storageAccountName="$ORCHESTRATOR_FUNC_APP_STORAGE_NAME" \
  #     databaseConnectionString="$db_conn_str" \
  #     collabDatabaseConnectionString="$collab_db_conn_str" \
  #     cloudName="$CLOUD_NAME" \
  #     states="$state_abbrs" \
  #     coreResourceGroup="$RESOURCE_GROUP" \
  #     eventHubName="$EVENT_HUB_NAME"

  # Publish function app
  # try_run "func azure functionapp publish ${ORCHESTRATOR_FUNC_APP_NAME} --dotnet" 7 "../match/src/Piipan.Match/Piipan.Match.Func.Api"

  # echo "Allowing $VNET_NAME to access $ORCHESTRATOR_FUNC_APP_STORAGE_NAME"
  # # Subnet ID is needed when vnet and storage are in different resource groups
  # func_subnet_id=$(\
  #   az network vnet subnet show \
  #     --vnet-name "$VNET_NAME" \
  #     --name "$FUNC_SUBNET_NAME" \
  #     --resource-group "$RESOURCE_GROUP" \
  #     --query id \
  #     -o tsv)
  # az storage account network-rule add \
  #   --account-name "$ORCHESTRATOR_FUNC_APP_STORAGE_NAME" \
  #   --resource-group "$MATCH_RESOURCE_GROUP" \
  #   --subnet "$func_subnet_id"
  # az storage account update \
  #   --name "$ORCHESTRATOR_FUNC_APP_STORAGE_NAME" \
  #   --resource-group "$MATCH_RESOURCE_GROUP" \
  #   --default-action Deny

  # echo "Integrating ${ORCHESTRATOR_FUNC_APP_NAME} into virtual network"
  # az functionapp vnet-integration add \
  #   --name "$ORCHESTRATOR_FUNC_APP_NAME" \
  #   --resource-group "$MATCH_RESOURCE_GROUP" \
  #   --subnet "$FUNC_SUBNET_NAME" \
  #   --vnet "$VNET_ID"
  # az functionapp config appsettings set \
  #   --name "$ORCHESTRATOR_FUNC_APP_NAME" \
  #   --resource-group "$MATCH_RESOURCE_GROUP" \
  #   --settings \
  #     WEBSITE_CONTENTOVERVNET=1 \
  #     WEBSITE_VNET_ROUTE_ALL=1

  # Create an Active Directory app registration associated with the app.
  # Used by subsequent resources to configure auth
  # az ad app create \
  #   --display-name "$ORCHESTRATOR_FUNC_APP_NAME" \
  #   --available-to-other-tenants false

  # ./config-managed-role.bash "$ORCHESTRATOR_FUNC_APP_NAME" "$MATCH_RESOURCE_GROUP" "${PG_AAD_ADMIN}@${PG_SERVER_NAME}"

  # # Create Match Resolution API Function App
  # echo "Create Match Resolution API Function App"
  # collab_db_conn_str=$(pg_connection_string "$CORE_DB_SERVER_NAME" "$COLLAB_DB_NAME" "$MATCH_RES_FUNC_APP_NAME")
  # az deployment group create \
  #   --name match-res-api \
  #   --resource-group "$MATCH_RESOURCE_GROUP" \
  #   --template-file  ./arm-templates/function-match-res.json \
  #   --parameters \
  #     resourceTags="$RESOURCE_TAGS" \
  #     location="$LOCATION" \
  #     functionAppName="$MATCH_RES_FUNC_APP_NAME" \
  #     appServicePlanName="$APP_SERVICE_PLAN_FUNC_NAME" \
  #     storageAccountName="$MATCH_RES_FUNC_APP_STORAGE_NAME" \
  #     collabDatabaseConnectionString="$collab_db_conn_str" \
  #     cloudName="$CLOUD_NAME" \
  #     states="$state_abbrs" \
  #     coreResourceGroup="$RESOURCE_GROUP" \
  #     eventHubName="$EVENT_HUB_NAME"

  # echo "Publish Match Resolution API Function App"
  # try_run "func azure functionapp publish ${MATCH_RES_FUNC_APP_NAME} --dotnet" 7 "../match/src/Piipan.Match/Piipan.Match.Func.ResolutionApi"

  # echo "Integrating ${MATCH_RES_FUNC_APP_NAME} into virtual network"
  # az functionapp vnet-integration add \
  #   --name "$MATCH_RES_FUNC_APP_NAME" \
  #   --resource-group "$MATCH_RESOURCE_GROUP" \
  #   --subnet "$FUNC_SUBNET_NAME" \
  #   --vnet "$VNET_ID"
  # az functionapp config appsettings set \
  #   --name "$MATCH_RES_FUNC_APP_NAME" \
  #   --resource-group "$MATCH_RESOURCE_GROUP" \
  #   --settings \
  #     WEBSITE_CONTENTOVERVNET=1 \
  #     WEBSITE_VNET_ROUTE_ALL=1

  # if [ "$exists" = "true" ]; then
  #   echo "Leaving $CURRENT_USER_OBJID as a member of $PG_AAD_ADMIN"
  # else
  #   # Revoke temporary assignment of current user as a PostgreSQL AD admin
  #   az ad group member remove \
  #     --group "$PG_AAD_ADMIN" \
  #     --member-id "$CURRENT_USER_OBJID"
  # fi

  # Create per-state Function apps and assign corresponding managed identity for
  # access to the per-state blob-storage and database, set up system topics and
  # event subscription to bulk upload (blob creation) events
  # while IFS=, read -r abbr name ; do
  # echo "Creating function app for $name ($abbr)"
  # done < states.csv

  while IFS=, read -r abbr name ; do
    echo "Creating function app for $name ($abbr)"
    abbr=$(echo "$abbr" | tr '[:upper:]' '[:lower:]')

    # Per-state Function App
    func_app=$PREFIX-func-${abbr}etl-$ENV

    # Storage account for the Function app for its own use;
    func_stor=${PREFIX}st${abbr}etl${ENV}

    # Managed identity to access database
    identity=$(state_managed_id_name "$abbr" "$ENV")

    # Per-state database
    db_name=${abbr}

    # Actual Function, under the Function App, that receives an event
    # and does the work, name derived from classname in `etl` directory
    func_name=BulkUpload

    # Per-state storage account for bulk upload;
    # matches name passed to blob-storage.json
    stor_name=${PREFIX}st${abbr}upload${ENV}

    # System topic for per-state upload (create blob) events
    # same as topic name in create-metrics-resources.bash
    topic_name=evgt-${abbr}upload-$ENV
    topic_name=$(state_event_grid_topic_name "$abbr" "$ENV")

    # Subscription to upload events that get routed to Function
    sub_name=evgs-${abbr}upload-$ENV

    # Every Function app needs a storage account for its own internal use;
    # e.g., bindings state, keys, function code. Keep this separate from
    # the storage account used to upload data for better isolation.
    az deployment group create \
      --name "$func_stor" \
      --resource-group "$RESOURCE_GROUP" \
      --template-file ./arm-templates/function-storage.json \
      --parameters \
        uniqueStorageName="$func_stor" \
        resourceTags="$RESOURCE_TAGS" \
        location="$LOCATION" \
        vnet="$VNET_ID" \
        subnet="$FUNC_SUBNET_NAME" \
        sku="$STORAGE_SKU"

    # Even though the OS *should* be abstracted away at the Function level, Azure
    # portal has oddities/limitations when using Linux -- lets just get it
    # working with Windows as underlying OS
    az functionapp create \
      --resource-group "$RESOURCE_GROUP" \
      --plan "$APP_SERVICE_PLAN_FUNC_NAME" \
      --tags Project="$PROJECT_TAG" "$PER_STATE_ETL_TAG" \
      --runtime dotnet \
      --functions-version 3 \
      --os-type Windows \
      --name "$func_app" \
      --storage-account "$func_stor"

    # # Integrate function app into Virtual Network
    echo "Integrating ${func_app} into virtual network"
    az functionapp vnet-integration add \
      --name "$func_app" \
      --resource-group "$RESOURCE_GROUP" \
      --subnet "$FUNC_SUBNET_NAME" \
      --vnet "$VNET_NAME"
    az webapp config set \
      --name "$func_app" \
      --resource-group "$RESOURCE_GROUP" \
      --vnet-route-all-enabled true

    # Stream logs to Event Hub
    func_id=$(\
      az functionapp show \
        -n "$func_app" \
        -g "$RESOURCE_GROUP" \
        -o tsv \
        --query id)
    az monitor diagnostic-settings create \
      --name "stream-logs-to-event-hub" \
      --resource "$func_id" \
      --event-hub "logs" \
      --event-hub-rule "$eh_rule_id" \
      --logs '[
        {
          "category": "FunctionAppLogs",
          "enabled": true
        }
      ]'

    #  Assumes if any identity is set, it is the one we are specifying below
    exists=$(az functionapp identity show \
      --resource-group "$RESOURCE_GROUP" \
      --name "$func_app")

    if [ -z "$exists" ]; then
      # Conditionally execute otherwise we will get an error if it is already
      # assigned this managed identity
      az functionapp identity assign \
        --resource-group "$RESOURCE_GROUP" \
        --name "$func_app" \
        --identities "${DEFAULT_PROVIDERS}/Microsoft.ManagedIdentity/userAssignedIdentities/${identity}"
    fi

    # db_conn_str=$(pg_connection_string "$PG_SERVER_NAME" "$db_name" "$identity")
    blob_conn_str=$(blob_connection_string "$RESOURCE_GROUP" "$stor_name")
    az_serv_str=$(az_connection_string "$RESOURCE_GROUP" "$identity")

    az functionapp config appsettings set \
      --resource-group "$RESOURCE_GROUP" \
      --name "$func_app" \
      --settings \
        $AZ_SERV_STR_KEY="$az_serv_str" \
        $DB_CONN_STR_KEY="Server=tts-psql-participants-dev.postgres.database.azure.com;Database=ea;Port=5432;User Id=id_eaadmin_dev@tts-psql-participants-dev;Password={password};Ssl Mode=VerifyFull;;Tcp Keepalive=true;Tcp Keepalive Time=30000;Command Timeout=300;" \
        $BLOB_CONN_STR_KEY="$blob_conn_str" \
        $CLOUD_NAME_STR_KEY="$CLOUD_NAME" \
      --output none

              # $DB_CONN_STR_KEY="$db_conn_str" \
              # 



    event_grid_system_topic_id=$(\
      az eventgrid system-topic create \
        --location "$LOCATION" \
        --name "$topic_name" \
        --topic-type Microsoft.Storage.storageAccounts \
        --resource-group "$RESOURCE_GROUP" \
        --source "${DEFAULT_PROVIDERS}/Microsoft.Storage/storageAccounts/${stor_name}" \
        -o tsv \
        --query id)


    echo "try_run func azure functionapp publish ${func_app} --dotnet 7 ../etl/src/Piipan.Etl/Piipan.Etl.Func.BulkUpload"
    # # Create Function endpoint before setting up event subscription
    try_run "func azure functionapp publish ${func_app} --dotnet" 7 "../etl/src/Piipan.Etl/Piipan.Etl.Func.BulkUpload"

    az eventgrid system-topic event-subscription create \
      --name "$sub_name" \
      --resource-group "$RESOURCE_GROUP" \
      --system-topic-name "$topic_name" \
      --endpoint "${DEFAULT_PROVIDERS}/Microsoft.Web/sites/${func_app}/functions/${func_name}" \
      --endpoint-type azurefunction \
      --included-event-types Microsoft.Storage.BlobCreated \
      --subject-begins-with /blobServices/default/containers/upload/blobs/

    # Stream event topic logs to Event Hub
    az monitor diagnostic-settings create \
      --name "stream-logs-to-event-hub" \
      --resource "$event_grid_system_topic_id" \
      --event-hub "logs" \
      --event-hub-rule "$eh_rule_id" \
      --logs '[
        {
          "category": "DeliveryFailures",
          "enabled": true
        }
      ]'
  done < states.csv

 
  script_completed
}

main "$@"
