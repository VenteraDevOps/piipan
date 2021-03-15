### Constants
# It's helpful to tag all piipan-related resources
PROJECT_TAG=piipan
RESOURCE_TAGS="{ \"Project\": \"${PROJECT_TAG}\" }"

# Identity object ID for the Azure environment account
CURRENT_USER_OBJID=`az ad signed-in-user show --query objectId --output tsv`

# The default Azure subscription
SUBSCRIPTION_ID=`az account show --query id -o tsv`

# Name of App Service Plan, used by both query tool and dashboard
APP_SERVICE_PLAN=piipan-app-plan

# Name of environment variable used to pass database connection strings
# to app or function code
DB_CONN_STR_KEY=DatabaseConnectionString

# Name of environment variable used to pass blob storage account connection
# strings to app or function code
BLOB_CONN_STR_KEY=BlobStorageConnectionString

# Name of environment variable used to pass Azure Services connection strings
# to app or function code (required to fetch managed identity tokens)
AZ_SERV_STR_KEY=AzureServicesAuthConnectionString

# For connection strings, our established placeholder value
PASSWORD_PLACEHOLDER='{password}'
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

  base=`az postgres show-connection-string \
    --server-name $server \
    --database-name $db \
    --admin-user $user \
    --admin-password "$PASSWORD_PLACEHOLDER" \
    --query connectionStrings.\"ado.net\" \
    -o tsv`

  # See:
  # https://github.com/Azure/azure-cli-extensions/issues/3143
  # https://docs.microsoft.com/en-us/azure/azure-government/compare-azure-government-global-azure
  if [ "$CLOUD_NAME" = "AzureUSGovernment" ]; then
    base=${base/.postgres.database.azure.com/.postgres.database.usgovcloudapi.net}
  fi

  echo "${base}Ssl Mode=Require;"
}

# Verify that the expected Azure environment is the active cloud
verify_cloud () {
  local cn=$(az cloud show --query name -o tsv)

  if [ "$CLOUD_NAME" != "$cn" ]; then
    echo "error: '$cn' is the active cloud, expecting '$CLOUD_NAME'"
    return 1
  fi
  return 0
}

# hard-coded switches between commerical and government Azure environments
web_app_host_suffix () {
  if [ "$CLOUD_NAME" = "AzureUSGovernment" ]; then
    return ".azurewebsites.us"
  else
    return ".azurewebsites.net"
  fi
}

front_door_host_suffix () {
  if [ "$CLOUD_NAME" = "AzureUSGovernment" ]; then
    # can't find this in the list of azure gov domains so I'm guessing here
    # https://docs.microsoft.com/en-us/azure/azure-government/compare-azure-government-global-azure
    return ".azurefd.us"
  else
    return ".azurefd.net"
  fi
}
### END Functions
