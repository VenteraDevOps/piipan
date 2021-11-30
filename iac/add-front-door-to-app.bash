#!/usr/bin/env bash
# This script creates an Azure Front Door with a WAF policy
# and applies it to a single web app.
#
# Arguments:
#   env (eg: tts/dev)
#   resource_group (eg: rg-core-dev)
#   front_door_name (eg: tts-fd-dashboard-dev)
#   waf_name (eg: waf-dashboard-dev)
#   app_host_name (eg: my-dashboard.azurewebsites.net)
#
# Usage:
# ./iac/add-front-door-to-app.bash tts/dev rg-core-dev dashboard my-dashboard-123.azurewebsites.net

# shellcheck source=./tools/common.bash
source "$(dirname "$0")"/../tools/common.bash || exit

main () {
  azure_env=$1
  source "$(dirname "$0")"/env/"${azure_env}".bash
  source "$(dirname "$0")"/iac-common.bash
  verify_cloud

  resource_group=$2
  front_door_name=$3
  waf_name=$4
  app_address=$5

  suffix=$(front_door_host_suffix)
  az deployment group create \
    --name "$front_door_name" \
    --resource-group "$resource_group" \
    --template-file ./arm-templates/front-door-app-service.json \
    --parameters \
      appAddress="$app_address" \
      frontDoorHostName="${front_door_name}${suffix}" \
      frontDoorName="$front_door_name" \
      resourceGroupName="$resource_group" \
      resourceTags="$RESOURCE_TAGS" \
      wafPolicyName="$waf_name" \
      prefix="$PREFIX" \
      env="$ENV"

  script_completed
}
main "$@"
