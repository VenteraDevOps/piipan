#!/usr/bin/env bash
#
# Basic integration test tool for the Metrics API. Sends a curl request to the
# API's GetParticipantUploads endpoint and writes result to stdout.
# Requires the caller is on an allowed Network.
#
# azure-env is the name of the deployment environment (e.g., "tts/dev").
# See iac/env for available environments.
#
# usage: test-metricsapi.bash <azure-env>

# shellcheck source=./tools/common.bash
source "$(dirname "$0")"/../../tools/common.bash || exit

main () {
  azure_env=$1
  # shellcheck source=./iac/env/tts/dev.bash
  source "$(dirname "$0")"/../../iac/env/"${azure_env}".bash
  # shellcheck source=./iac/iac-common.bash
  source "$(dirname "$0")"/../../iac/iac-common.bash
  verify_cloud

  domain=$(web_app_host_suffix)
  token=$(az account get-access-token --resource "https://${METRICS_API_APP_NAME}${domain}" --query accessToken -o tsv)

  # grab url for metrics api
  function_uri=$(az functionapp function show \
    --name "$METRICS_API_APP_NAME" \
    --resource-group "$RESOURCE_GROUP" \
    --function-name $METRICS_API_FUNCTION_NAME \
    --query invokeUrlTemplate \
    --output tsv)

  echo "Submitting request to ${function_uri}"
  curl -X GET -i \
    -H "Content-Type: application/json" \
    -H "Authorization: Bearer ${token}" \
    "${function_uri}"

  printf "\n"

  # grab url for metrics api
  function_uri_lastupload=$(az functionapp function show \
    --name "$METRICS_API_APP_NAME" \
    --resource-group "$RESOURCE_GROUP" \
    --function-name $METRICS_API_FUNCTION_NAME_LASTUPLOAD \
    --query invokeUrlTemplate \
    --output tsv)

  echo "Submitting request to ${function_uri_lastupload}"
  curl -X GET -i \
    -H "Content-Type: application/json" \
    -H "Authorization: Bearer ${token}" \
    "${function_uri_lastupload}"

  printf "\n"

  script_completed
}

main "$@"
