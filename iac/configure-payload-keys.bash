#!/usr/bin/env bash
#
# Sets the Payload encryption key values in Azure Key Vault (as secrets) for encrypting
# and decrypting bulk upload files. These keys will allow Azure API Management
# to encrypt bulk upload files and allow Azure functions to decrypt them
# when processing.
#
# Assumes an Azure user with the Global Administrator role has signed in
# with the Azure CLI, the infrastructure, established by create-resources.bash
# is in place, particularly, the Azure Key Vault instance.
#
# usage: configure-payload-keys.bash <azure-env>

# shellcheck source=./tools/common.bash
source "$(dirname "$0")"/../tools/common.bash || exit
# shellcheck source=./tools/build-common.bash
source "$(dirname "$0")"/../tools/build-common.bash || exit

main () {
    local azure_env=$1
    # shellcheck source=./iac/env/tts/dev.bash
    source "$(dirname "$0")"/../iac/env/"${azure_env}".bash
    # shellcheck source=./iac/iac-common.bash
    source "$(dirname "$0")"/../iac/iac-common.bash

    echo "Beginning Payload Key Generation"
    payloadKey=$(openssl enc -aes-256-cbc -k secret -P -md sha512 -pbkdf2 | sed -n 's/^key=\(.*\)/\1/p')
    BASE64UPLOADKEY=$(echo "${payloadKey}" | xxd -r -p | base64)
    shaOfPayloadKey=$(echo -n "${payloadKey}" | xxd -r -p | sha256sum)
    BASE64UPLOADSHAKEY=$(echo "${shaOfPayloadKey}" | xxd -r -p | base64)

    echo "Setting Payload Encryption Key in vault"
    az deployment group create \
      --name "${VAULT_NAME}-${UPLOAD_ENCRYPT_KEY_KV}" \
      --resource-group "${RESOURCE_GROUP}" \
      --template-file ./arm-templates/key-vault-secret.json \
      --parameters \
        keyVaultName="${VAULT_NAME}" \
        resourceTags="${RESOURCE_TAGS}" \
        secretName="${UPLOAD_ENCRYPT_KEY_KV}" \
        secretValue="${BASE64UPLOADKEY}"

    echo "Setting SHA of Payload Encryption Key in vault"
    az deployment group create \
      --name "${VAULT_NAME}-${UPLOAD_ENCRYPT_KEY_SHA_KV}" \
      --resource-group "${RESOURCE_GROUP}" \
      --template-file ./arm-templates/key-vault-secret.json \
      --parameters \
        keyVaultName="${VAULT_NAME}" \
        resourceTags="${RESOURCE_TAGS}" \
        secretName="${UPLOAD_ENCRYPT_KEY_SHA_KV}" \
        secretValue="${BASE64UPLOADSHAKEY}"
}

main "$@"
