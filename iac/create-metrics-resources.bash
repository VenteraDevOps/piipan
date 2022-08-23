#!/usr/bin/env bash
#
# Provisions and configures the infrastructure components for all Piipan Metrics subsystems.
# Assumes an Azure user with the Global Administrator role has signed in with the Azure CLI.
# Assumes Piipan base resource groups, resources have been created in the same environment
# (for example, state-specific blob topics).
# Must be run from a trusted network.
#
# azure-env is the name of the deployment environment (e.g., "tts/dev").
# See iac/env for available environments.
#
# usage: create-metrics-resources.bash <azure-env>

# shellcheck source=./tools/common.bash
source "$(dirname "$0")"/../tools/common.bash || exit

set_constants () {
  DB_SERVER_NAME=$PREFIX-psql-core-$ENV
  DB_NAME=metrics

  # Metrics Collection Info
  COLLECT_APP_FILEPATH=Piipan.Metrics.Func.Collect
  COLLECT_STORAGE_NAME=${PREFIX}st${METRICS_COLLECT_APP_ID}${ENV}
  COLLECT_NEW_BULKUPLOAD_FUNC=CreateBulkUploadMetrics
  COLLECT_UPDATED_BULKUPLOAD_FUNC=UpdateBulkUploadMetricsStatus
  COLLECT_NEW_SEARCH_METRICS_FUNC=CreateSearchMetrics
  COLLECT_CREATE_UPDATE_MATCH_METRICS_FUNC=PublishMatchMetrics

  # Metrics API Info
  API_APP_FILEPATH=Piipan.Metrics.Func.Api
  API_APP_STORAGE_NAME=${PREFIX}st${METRICS_API_APP_ID}${ENV}

  # VNet, which should always exist when running this script
  VNET_ID=$(\
    az network vnet show \
      -n "${VNET_NAME}" \
      -g "${RESOURCE_GROUP}" \
      --query id \
      -o tsv)

  set_defaults
}

main () {
  # Load agency/subscription/deployment-specific settings
  azure_env=$1
  source "$(dirname "$0")"/env/"${azure_env}".bash
  # shellcheck source=./iac/iac-common.bash
  source "$(dirname "$0")"/iac-common.bash

  verify_cloud
  set_constants

  # Create Metrics Collect Function Storage Account
  echo "Creating Storage Account: ${COLLECT_STORAGE_NAME}"
  az deployment group create \
    --name "${COLLECT_STORAGE_NAME}" \
    --resource-group "${RESOURCE_GROUP}" \
    --template-file ./arm-templates/function-storage.json \
    --parameters \
      storageAccountName="${COLLECT_STORAGE_NAME}" \
      resourceTags="${RESOURCE_TAGS}" \
      location="${LOCATION}" \
      diagnosticSettingName="${DIAGNOSTIC_SETTINGS_NAME}" \
      eventHubAuthorizationRuleId="${EH_RULE_ID}" \
      eventHubName="${EVENT_HUB_NAME}" \
      sku="${STORAGE_SKU}" \
      subnet="${FUNC_SUBNET_NAME}" \
      vnet="${VNET_ID}" \
      workspaceId="${LOG_ANALYTICS_WORKSPACE_ID}"

  # Create Metrics Collect Function App in Azure
  echo "Creating function: ${METRICS_COLLECT_APP_NAME}"
  az functionapp create \
    --resource-group "${RESOURCE_GROUP}" \
    --tags "Project=${PROJECT_TAG}" \
    --plan "${APP_SERVICE_PLAN_FUNC_NAME}" \
    --runtime "dotnet" \
    --functions-version 4 \
    --name "${METRICS_COLLECT_APP_NAME}" \
    --storage-account "${COLLECT_STORAGE_NAME}" \
    --assign-identity "[system]" \
    --disable-app-insights true

  # Integrate function app into Virtual Network
  echo "Integrating ${METRICS_COLLECT_APP_NAME} into virtual network"
  az functionapp vnet-integration add \
    --name "${METRICS_COLLECT_APP_NAME}" \
    --resource-group "${RESOURCE_GROUP}" \
    --subnet "${FUNC_SUBNET_NAME}" \
    --vnet "${VNET_NAME}"

  # Allow only incoming traffic from Event Grid
  # Only set rule if it does not exist, to avoid error
  exists=$(\
    az functionapp config access-restriction show \
      -n "$METRICS_COLLECT_APP_NAME" \
      -g "$RESOURCE_GROUP" \
      --query "ipSecurityRestrictions[?ip_address == 'AzureEventGrid'].ip_address" \
      -o tsv)
  if [ -z "$exists" ]; then
    az functionapp config access-restriction add \
      -n "$METRICS_COLLECT_APP_NAME" \
      -g "$RESOURCE_GROUP" \
      --priority 100 \
      --service-tag AzureEventGrid
  fi

  echo "Configure: ${METRICS_COLLECT_APP_NAME}"
  az functionapp config set \
    --name "${METRICS_COLLECT_APP_NAME}" \
    --resource-group "${RESOURCE_GROUP}" \
    --always-on true \
    --ftps-state "Disabled" \
    --min-tls-version "1.2" \
    --vnet-route-all-enabled true

  echo "Removing public access for ${COLLECT_STORAGE_NAME}"
  az storage account update \
    --name "${COLLECT_STORAGE_NAME}" \
    --resource-group "${RESOURCE_GROUP}" \
    --default-action "Deny"

  # Configure log streaming for function app
  metrics_collect_function_id=$(\
    az functionapp show \
      -n "$METRICS_COLLECT_APP_NAME" \
      -g "$RESOURCE_GROUP" \
      -o tsv \
      --query id)
  update_diagnostic_settings "${metrics_collect_function_id}" "${DIAGNOSTIC_SETTINGS_FUNC}"

  echo "Creating Application Insight for function: ${METRICS_COLLECT_APP_NAME}"
  appInsightConnectionString=$(\
    az monitor app-insights component create \
      --app "${METRICS_COLLECT_APP_NAME}" \
      --location "${LOCATION}" \
      --resource-group "${RESOURCE_GROUP}" \
      --tags "Project=${PROJECT_TAG}" \
      --application-type "${APPINSIGHTS_KIND}" \
      --kind "${APPINSIGHTS_KIND}" \
      --workspace "${LOG_ANALYTICS_WORKSPACE_ID}" \
      --query "${APPINSIGHTS_CONNECTION_STRING}" \
      --output tsv)

  echo "Integrating Application Insight with function: ${METRICS_COLLECT_APP_NAME}"
  az monitor app-insights component connect-function \
    --app "${METRICS_COLLECT_APP_NAME}" \
    --function "${METRICS_COLLECT_APP_NAME}" \
    --resource-group "${RESOURCE_GROUP}"

  echo "Configure appsettings for: ${METRICS_COLLECT_APP_NAME}"
  db_conn_str=$(pg_connection_string "$DB_SERVER_NAME" "$DB_NAME" "${METRICS_COLLECT_APP_NAME//-/_}")
  eventgrid_endpoint=$(eventgrid_endpoint "$RESOURCE_GROUP" "$UPDATE_BU_METRICS_TOPIC_NAME")
  eventgrid_key_str=$(eventgrid_key_string "$RESOURCE_GROUP" "$UPDATE_BU_METRICS_TOPIC_NAME")
  eventgrid_search_endpoint=$(eventgrid_endpoint "$RESOURCE_GROUP" "${CREATE_SEARCH_METRICS_TOPIC_NAME}")
  eventgrid_search_key_string=$(eventgrid_key_string "$RESOURCE_GROUP" "${CREATE_SEARCH_METRICS_TOPIC_NAME}")
  eventgrid_match_metrics_endpoint=$(eventgrid_endpoint "$RESOURCE_GROUP" "${CREATE_MATCH_METRICS_TOPIC_NAME}")
  eventgrid_match_metrics_key_string=$(eventgrid_key_string "$RESOURCE_GROUP" "${CREATE_MATCH_METRICS_TOPIC_NAME}")

  az functionapp config appsettings set \
    --resource-group "$RESOURCE_GROUP" \
    --name "$METRICS_COLLECT_APP_NAME" \
    --settings \
      ${APPLICATIONINSIGHTS_CONNECTION_STRING}="${appInsightConnectionString}" \
      $DB_CONN_STR_KEY="$db_conn_str" \
      $CLOUD_NAME_STR_KEY="$CLOUD_NAME" \
      $EVENTGRID_CONN_STR_ENDPOINT="$eventgrid_endpoint" \
      $EVENTGRID_CONN_STR_KEY="$eventgrid_key_str" \
      $EVENTGRID_CONN_METRICS_SEARCH_STR_ENDPOINT="${eventgrid_search_endpoint}" \
      $EVENTGRID_CONN_METRICS_SEARCH_STR_KEY="${eventgrid_search_key_string}" \
      $EVENTGRID_CONN_METRICS_MATCH_STR_ENDPOINT="${eventgrid_match_metrics_endpoint}" \
      $EVENTGRID_CONN_METRICS_MATCH_STR_KEY="${eventgrid_match_metrics_key_string}" \
    --output none

  # Waiting before publishing the app, since publishing immediately after creation returns an App Not Found error
  # Waiting was the best solution I could find. More info in these GH issues:
  # https://github.com/Azure/azure-functions-core-tools/issues/1616
  # https://github.com/Azure/azure-functions-core-tools/issues/1766
  echo "Waiting to publish function app"
  sleep 60

  # publish the function app
  try_run "func azure functionapp publish ${METRICS_COLLECT_APP_NAME} --dotnet" 7 "../metrics/src/Piipan.Metrics/$COLLECT_APP_FILEPATH"

  # Create a Subscription to upload events that get routed to Function
  az eventgrid event-subscription create \
    --source-resource-id "${DEFAULT_PROVIDERS}/Microsoft.EventGrid/topics/${UPDATE_BU_METRICS_TOPIC_NAME}" \
    --name "$UPDATE_BU_METRICS_CUSTOM_TOPIC" \
    --endpoint "${DEFAULT_PROVIDERS}/Microsoft.Web/sites/${METRICS_COLLECT_APP_NAME}/functions/${COLLECT_UPDATED_BULKUPLOAD_FUNC}" \
    --endpoint-type azurefunction

  # Create a Subscription to upload events that get routed to Function
  az eventgrid event-subscription create \
    --source-resource-id "${DEFAULT_PROVIDERS}/Microsoft.EventGrid/topics/${CREATE_SEARCH_METRICS_TOPIC_NAME}" \
    --name "${CREATE_SEARCH_METRICS_TOPIC_NAME}" \
    --endpoint "${DEFAULT_PROVIDERS}/Microsoft.Web/sites/${METRICS_COLLECT_APP_NAME}/functions/${COLLECT_NEW_SEARCH_METRICS_FUNC}" \
    --endpoint-type azurefunction

  # Create a Subscription to upload events that get routed to Function
  az eventgrid event-subscription create \
    --source-resource-id "${DEFAULT_PROVIDERS}/Microsoft.EventGrid/topics/${CREATE_MATCH_METRICS_TOPIC_NAME}" \
    --name "${CREATE_MATCH_METRICS_TOPIC_NAME}" \
    --endpoint "${DEFAULT_PROVIDERS}/Microsoft.Web/sites/${METRICS_COLLECT_APP_NAME}/functions/${COLLECT_CREATE_UPDATE_MATCH_METRICS_FUNC}" \
    --endpoint-type azurefunction

  while IFS=$',\t\r\n' read -r abbr name; do

    echo "Subscribing to ${name} blob events"
    abbr=$(echo "$abbr" | tr '[:upper:]' '[:lower:]')
    sub_name=evgs-${abbr}metricsupload-${ENV}
    topic_name=$(state_event_grid_topic_name "$abbr" "$ENV")

    az eventgrid system-topic event-subscription create \
      --name "${sub_name}" \
      --resource-group "${RESOURCE_GROUP}" \
      --system-topic-name "${topic_name}" \
      --endpoint "${DEFAULT_PROVIDERS}/Microsoft.Web/sites/${METRICS_COLLECT_APP_NAME}/functions/${COLLECT_NEW_BULKUPLOAD_FUNC}" \
      --endpoint-type azurefunction \
      --included-event-types Microsoft.Storage.BlobCreated \
      --subject-begins-with /blobServices/default/containers/upload/blobs/

    func_app=$PREFIX-func-${abbr}etl-$ENV

    az functionapp config appsettings set \
      --resource-group "$RESOURCE_GROUP" \
      --name "$func_app" \
      --settings \
        $EVENTGRID_CONN_STR_ENDPOINT="$eventgrid_endpoint" \
        $EVENTGRID_CONN_STR_KEY="$eventgrid_key_str" \
      --output none

  done < env/"${azure_env}"/states.csv

  # Create Metrics API Function Storage Account
  echo "Creating storage account: ${API_APP_STORAGE_NAME}"
  az deployment group create \
    --name "${API_APP_STORAGE_NAME}" \
    --resource-group "${RESOURCE_GROUP}" \
    --template-file ./arm-templates/function-storage.json \
    --parameters \
      storageAccountName="${API_APP_STORAGE_NAME}" \
      resourceTags="${RESOURCE_TAGS}" \
      location="${LOCATION}" \
      diagnosticSettingName="${DIAGNOSTIC_SETTINGS_NAME}" \
      eventHubAuthorizationRuleId="${EH_RULE_ID}" \
      eventHubName="${EVENT_HUB_NAME}" \
      sku="${STORAGE_SKU}" \
      subnet="${FUNC_SUBNET_NAME}" \
      vnet="${VNET_ID}" \
      workspaceId="${LOG_ANALYTICS_WORKSPACE_ID}"

  # Create Metrics API Function App in Azure
  echo "Creating function app: ${METRICS_API_APP_NAME}"
  az functionapp create \
    --resource-group "${RESOURCE_GROUP}" \
    --plan "${APP_SERVICE_PLAN_FUNC_NAME}" \
    --tags "Project=${PROJECT_TAG}" \
    --runtime "dotnet" \
    --functions-version 4 \
    --name "${METRICS_API_APP_NAME}" \
    --storage-account "${API_APP_STORAGE_NAME}" \
    --assign-identity "[system]" \
    --disable-app-insights true

  # Create an Active Directory app registration associated with the app.
  az ad app create \
    --display-name "${METRICS_API_APP_NAME}" \
    --sign-in-audience "AzureADMyOrg"

  # Integrate function app into Virtual Network
  echo "Integrating $METRICS_API_APP_NAME into virtual network"
  az functionapp vnet-integration add \
    --name "$METRICS_API_APP_NAME" \
    --resource-group "$RESOURCE_GROUP" \
    --subnet "$FUNC_SUBNET_NAME" \
    --vnet "$VNET_NAME"

  echo "Configure: ${METRICS_API_APP_NAME}"
  az functionapp config set \
    --name "${METRICS_API_APP_NAME}" \
    --resource-group "${RESOURCE_GROUP}" \
    --always-on true \
    --ftps-state "Disabled" \
    --min-tls-version "1.2" \
    --vnet-route-all-enabled true

  echo "Removing public access for ${API_APP_STORAGE_NAME}"
  az storage account update \
    --name "${API_APP_STORAGE_NAME}" \
    --resource-group "${RESOURCE_GROUP}" \
    --default-action "Deny"

  echo "Creating Application Insight for function: ${METRICS_API_APP_NAME}"
  appInsightConnectionString=$(\
  az monitor app-insights component create \
    --app "${METRICS_API_APP_NAME}" \
    --location "${LOCATION}" \
    --resource-group "${RESOURCE_GROUP}" \
    --tags "Project=${PROJECT_TAG}" \
    --application-type "${APPINSIGHTS_KIND}" \
    --kind "${APPINSIGHTS_KIND}" \
    --workspace "${LOG_ANALYTICS_WORKSPACE_ID}" \
    --query "${APPINSIGHTS_CONNECTION_STRING}" \
    --output tsv)

  echo "Integrating Application Insight with function: ${METRICS_API_APP_NAME}"
  az monitor app-insights component connect-function \
    --app "${METRICS_API_APP_NAME}" \
    --function "${METRICS_API_APP_NAME}" \
    --resource-group "${RESOURCE_GROUP}"

  echo "Configure appsettings for: ${METRICS_API_APP_NAME}"
  db_conn_str=$(pg_connection_string "$DB_SERVER_NAME" "$DB_NAME" "${METRICS_API_APP_NAME//-/_}")
  az functionapp config appsettings set \
      --resource-group "$RESOURCE_GROUP" \
      --name "$METRICS_API_APP_NAME" \
      --settings \
        ${APPLICATIONINSIGHTS_CONNECTION_STRING}="${appInsightConnectionString}" \
        $DB_CONN_STR_KEY="$db_conn_str" \
        $CLOUD_NAME_STR_KEY="$CLOUD_NAME" \
      --output none

  # Configure log streaming for function app
  metrics_api_function_id=$(\
    az functionapp show \
      -n "$METRICS_API_APP_NAME" \
      -g "$RESOURCE_GROUP" \
      -o tsv \
      --query id)
  update_diagnostic_settings "${metrics_api_function_id}" "${DIAGNOSTIC_SETTINGS_FUNC}"

  # publish metrics function app
  try_run "func azure functionapp publish ${METRICS_API_APP_NAME} --dotnet" 7 "../metrics/src/Piipan.Metrics/$API_APP_FILEPATH"

  ## Dashboard
  echo "Create Front Door and WAF policy for dashboard app"
  suffix=$(web_app_host_suffix)
  dashboard_host=${DASHBOARD_APP_NAME}${suffix}
  ./add-front-door-to-app.bash \
    "$azure_env" \
    "$RESOURCE_GROUP" \
    "$DASHBOARD_FRONTDOOR_NAME" \
    "$DASHBOARD_WAF_NAME" \
    "$dashboard_host"

  front_door_id=$(\
  az network front-door show \
    --name "$DASHBOARD_FRONTDOOR_NAME" \
    --resource-group "$RESOURCE_GROUP" \
    --query frontdoorId \
    --output tsv)
  echo "Front Door ID: ${front_door_id}"

  front_door_uri="https://$DASHBOARD_FRONTDOOR_NAME"$(front_door_host_suffix)

  metrics_api_hostname=$(\
    az functionapp show \
    -n "$METRICS_API_APP_NAME" \
    -g "$RESOURCE_GROUP" \
    --query "defaultHostName" \
    --output tsv)
  metrics_api_uri="https://${metrics_api_hostname}/api/"
  metrics_api_app_id=$(\
    az ad app list \
      --display-name "${METRICS_API_APP_NAME}" \
      --filter "displayName eq '${METRICS_API_APP_NAME}'" \
      --query "[0].appId" \
      --output tsv)

  # Create App Service resources for dashboard app
  echo "Creating App Service resources for dashboard app"
  az deployment group create \
    --name "$DASHBOARD_APP_NAME" \
    --resource-group "$RESOURCE_GROUP" \
    --template-file ./arm-templates/dashboard-app.json \
    --parameters \
      location="$LOCATION" \
      resourceTags="$RESOURCE_TAGS" \
      appName="$DASHBOARD_APP_NAME" \
      servicePlan="$APP_SERVICE_PLAN" \
      metricsApiUri="$metrics_api_uri" \
      metricsApiAppId="$metrics_api_app_id" \
      idpOidcConfigUri="$DASHBOARD_APP_IDP_OIDC_CONFIG_URI" \
      idpOidcScopes="$DASHBOARD_APP_IDP_OIDC_SCOPES" \
      idpClientId="$DASHBOARD_APP_IDP_CLIENT_ID" \
      aspNetCoreEnvironment="$PREFIX" \
      frontDoorId="$front_door_id" \
      frontDoorUri="$front_door_uri" \
      diagnosticSettingName="${DIAGNOSTIC_SETTINGS_NAME}" \
      eventHubAuthorizationRuleId="${EH_RULE_ID}" \
      eventHubName="${EVENT_HUB_NAME}" \
      workspaceId="${LOG_ANALYTICS_WORKSPACE_ID}"

  echo "Integrating ${DASHBOARD_APP_NAME} into virtual network"
  az webapp vnet-integration add \
    --name "$DASHBOARD_APP_NAME" \
    --resource-group "$RESOURCE_GROUP" \
    --subnet "$WEBAPP_SUBNET_NAME" \
    --vnet "$VNET_ID"

  create_oidc_secret "$DASHBOARD_APP_NAME"

  script_completed
}
main "$@"
