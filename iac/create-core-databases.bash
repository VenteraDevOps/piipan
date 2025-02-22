#!/usr/bin/env bash

# shellcheck source=./tools/common.bash
source "$(dirname "$0")"/../tools/common.bash || exit

set_constants () {
  DB_ADMIN_NAME=piipanadmin
  SUPERUSER=$DB_ADMIN_NAME
  PG_SECRET_NAME=core-pg-admin
  METRICS_DB_NAME=metrics

  # Name of Azure Active Directory admin for PostgreSQL server
  PG_AAD_ADMIN=piipan-admins-${ENV}

  PRIVATE_DNS_ZONE=$(private_dns_zone)

  set_defaults
}

main () {
  # Load agency/subscription/deployment-specific settings
  azure_env=$1
  # shellcheck source=./iac/env/tts/dev.bash
  source "$(dirname "$0")"/env/"${azure_env}".bash
  # shellcheck source=./iac/iac-common.bash
  source "$(dirname "$0")"/iac-common.bash
  # shellcheck source=./iac/db-common.bash
  source "$(dirname "$0")"/db-common.bash
  verify_cloud

  set_constants

  # Set PG Secret in key vault
  # By default, Azure CLI will print the password set in Key Vault; instead
  # just extract and print the secret id from the JSON response.
  echo "Setting key in vault"
  PG_SECRET=$(random_password)
  export PG_SECRET
   printenv PG_SECRET | tr -d '\n' | az keyvault secret set \
     --vault-name "$VAULT_NAME" \
     --name "$PG_SECRET_NAME" \
     --file /dev/stdin \
     --query id
     #--value "$PG_SECRET"


  echo "Creating core database server"
  az deployment group create \
    --name core-db \
    --resource-group "$RESOURCE_GROUP" \
    --template-file ./arm-templates/database-core.json \
    --parameters \
      administratorLogin=$DB_ADMIN_NAME \
      serverName="$CORE_DB_SERVER_NAME" \
      secretName="$PG_SECRET_NAME" \
      vaultName="$VAULT_NAME" \
      vnetName="$VNET_NAME" \
      subnetName="$DB_2_SUBNET_NAME" \
      privateEndpointName="$CORE_DB_PRIVATE_ENDPOINT_NAME" \
      privateDnsZoneName="$PRIVATE_DNS_ZONE" \
      resourceTags="$RESOURCE_TAGS" \
      diagnosticSettingName="${DIAGNOSTIC_SETTINGS_NAME}" \
      eventHubAuthorizationRuleId="${EH_RULE_ID}" \
      eventHubName="${EVENT_HUB_NAME}" \
      workspaceId="${LOG_ANALYTICS_WORKSPACE_ID}"

  db_set_env "$RESOURCE_GROUP" "$CORE_DB_SERVER_NAME" "$DB_ADMIN_NAME" "$PG_SECRET"

  echo "Creating $METRICS_DB_NAME database and applying DDL"
  db_init "$METRICS_DB_NAME" "$SUPERUSER"
  db_apply_ddl "$METRICS_DB_NAME" ../metrics/ddl/metrics.sql

  echo "Creating $COLLAB_DB_NAME database and applying DDL"
  db_init "$COLLAB_DB_NAME" "$SUPERUSER"
  db_apply_ddl "$COLLAB_DB_NAME" ../match/ddl/match-record.sql
  db_apply_ddl "$COLLAB_DB_NAME" ../match/ddl/state-info.sql

  echo "Inserting state info data"
  db_apply_ddl "$COLLAB_DB_NAME" ../match/dml/insert-state-info.sql

  db_config_aad "$RESOURCE_GROUP" "$CORE_DB_SERVER_NAME" "$PG_AAD_ADMIN"
  db_use_aad "$CORE_DB_SERVER_NAME" "$PG_AAD_ADMIN"

  echo "Configuring $METRICS_DB_NAME access for $METRICS_API_APP_NAME"
  db_create_managed_role "$METRICS_DB_NAME" "$METRICS_API_APP_NAME" "$RESOURCE_GROUP"
  db_config_managed_role "$METRICS_DB_NAME" "$METRICS_API_APP_NAME"
  db_grant_read "$METRICS_DB_NAME" "$METRICS_API_APP_NAME"

  echo "Configuring $METRICS_DB_NAME access for $METRICS_COLLECT_APP_NAME"
  db_create_managed_role "$METRICS_DB_NAME" "$METRICS_COLLECT_APP_NAME" "$RESOURCE_GROUP"
  db_config_managed_role "$METRICS_DB_NAME" "$METRICS_COLLECT_APP_NAME"
  db_grant_readwrite "$METRICS_DB_NAME" "$METRICS_COLLECT_APP_NAME"


  echo "Configuring $COLLAB_DB_NAME access for ${ORCHESTRATOR_FUNC_APP_NAME}"
  db_create_managed_role "$COLLAB_DB_NAME" "${ORCHESTRATOR_FUNC_APP_NAME}" "$MATCH_RESOURCE_GROUP"
  db_config_managed_role "$COLLAB_DB_NAME" "${ORCHESTRATOR_FUNC_APP_NAME}"
  db_grant_readwrite "$COLLAB_DB_NAME" "${ORCHESTRATOR_FUNC_APP_NAME}"

  echo "Configuring $COLLAB_DB_NAME access for ${MATCH_RES_FUNC_APP_NAME}"
  db_create_managed_role "$COLLAB_DB_NAME" "${MATCH_RES_FUNC_APP_NAME}" "$MATCH_RESOURCE_GROUP"
  db_config_managed_role "$COLLAB_DB_NAME" "${MATCH_RES_FUNC_APP_NAME}"
  db_grant_readwrite "$COLLAB_DB_NAME" "${MATCH_RES_FUNC_APP_NAME}"

  echo "Configuring $COLLAB_DB_NAME access for ${STATES_FUNC_APP_NAME}"
  db_create_managed_role "$COLLAB_DB_NAME" "${STATES_FUNC_APP_NAME}" "$RESOURCE_GROUP"
  db_config_managed_role "$COLLAB_DB_NAME" "${STATES_FUNC_APP_NAME}"
  db_grant_readwrite "$COLLAB_DB_NAME" "${STATES_FUNC_APP_NAME}"

  db_leave_aad "$PG_AAD_ADMIN"

  echo "Secure database connection"
  ./remove-external-network.bash \
    "$azure_env" \
    "$RESOURCE_GROUP" \
    "$CORE_DB_SERVER_NAME"

  script_completed
}

main "$@"
