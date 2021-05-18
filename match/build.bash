#!/bin/bash
#
# Builds project with optional testing and app deployment
# Relies on a solutions file (sln) in the subsystem root directory
# See build-common.bash for usage details

# shellcheck source=./tools/common.bash
source "$(dirname "$0")"/../tools/common.bash || exit
# shellcheck source=./tools/build-common.bash
source "$(dirname "$0")"/../tools/build-common.bash || exit

run_deploy () {
  azure_env=$1
  # shellcheck source=./iac/env/tts/dev.bash
  source "$(dirname "$0")"/../iac/env/"${azure_env}".bash
  # shellcheck source=./iac/iac-common.bash
  source "$(dirname "$0")"/../iac/iac-common.bash
  verify_cloud

  match_function_apps=($(get_resources $PER_STATE_MATCH_API_TAG "$RESOURCE_GROUP"))

  for app in "${match_function_apps[@]}"
  do
    echo "Publish ${app} to Azure Environment ${azure_env}"
    pushd ./src/Piipan.Match.State
      func azure functionapp publish "$app" --dotnet
    popd
  done

  orch_function_apps=($(get_resources $ORCHESTRATOR_API_TAG "$MATCH_RESOURCE_GROUP"))

  for app in "${orch_function_apps[@]}"
  do
    echo "Publish ${app} to Azure Environment ${azure_env}"
    pushd ./src/Piipan.Match.Orchestrator
      func azure functionapp publish "$app" --dotnet
    popd
  done
}

main "$@"