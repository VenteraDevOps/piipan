#!/usr/bin/env bash
#
# Configures Microsoft Defender and assigns the CIS Microsoft Azure
# Foundations Benchmark 1.3.0 to the subscription.
# Assumes an Azure user with the Global Administrator role has signed in with the Azure CLI.
#
# usage: configure-defender-and-policy.bash <azure-env>

# shellcheck source=./tools/common.bash
source "$(dirname "$0")"/../tools/common.bash || exit

set_constants () {
  # https://docs.microsoft.com/en-us/azure/governance/policy/samples/cis-azure-1-3-0
  # https://docs.microsoft.com/en-us/azure/governance/policy/samples/gov-cis-azure-1-3-0
  # While the policy set definition is the same, it is enforced differently
  # based on commerical or government Azure cloud
  # The policy "name" is the UUID of the set-definition
  CIS_POLICY_SET_DEFINITION_NAME="612b5213-9160-4969-8578-1518bd2a000c"
}

main () {
  # Load agency/subscription/deployment-specific settings
  azure_env=$1
  source "$(dirname "$0")"/env/"${azure_env}".bash
  # shellcheck source=./iac/iac-common.bash
  source "$(dirname "$0")"/iac-common.bash
  verify_cloud

  set_constants

  echo "Configure Microsoft Defender for Cloud"
  az deployment sub create \
    --name "defender-${LOCATION}" \
    --location "${LOCATION}" \
    --template-file ./arm-templates/defender.json \
    --parameters cloudName="${CLOUD_NAME}"

  echo "Assigning ${CIS_POLICY_SET_DEFINITION_NAME} to Subscription ${SUBSCRIPTION_ID}"
  az policy assignment create \
    --name "CIS Microsoft Azure Foundations Benchmark v1.3.0 - ${ENV}" \
    --location "${LOCATION}" \
    --policy-set-definition "${CIS_POLICY_SET_DEFINITION_NAME}" \
    --identity-scope "/subscriptions/${SUBSCRIPTION_ID}" \
    --mi-system-assigned

  script_completed
}

main "$@"
