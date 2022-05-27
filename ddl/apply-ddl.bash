#!/usr/bin/env bash
#
# Creates participant records tables and their access controls for
# each configured state. PGHOST, PGUSER, PGPASSWORD must be set.
#
# usage: apply-ddl.bash

# shellcheck source=./iac/iac-common.bash
source "$(dirname "$0")"/../iac/iac-common.bash || exit

set -e

# PGUSER and PGPASSWORD should correspond to the out-of-the-box,
# non-AD "superuser" administrtor login
set -u
: "$PGHOST"
: "$PGUSER"
: "$PGPASSWORD"
: "$ENV"

# Azure user connection string will be of the form:
# administatorLogin@serverName
SUPERUSER=${PGUSER%@*}

export PGOPTIONS='--client-min-messages=warning'
PSQL_OPTS=(-v ON_ERROR_STOP=1 -X -q)

apply_ddl () {
  db=$1
  owner=$2
  admin=$3
  reader="readonly"

  psql "${PSQL_OPTS[@]}" -d "$db" \
    -f ./per-state.sql

  psql "${PSQL_OPTS[@]}" -d "$db" \
    -v owner="$owner" \
    -v admin="$admin" \
    -v reader="$reader" \
    -v superuser="$SUPERUSER" \
    -f ./per-state-controls.sql
}

main () {
  azure_env=$1

  while IFS=, read -r abbr _; do
    db=$(echo "$abbr" | tr '[:upper:]' '[:lower:]')
    owner=$db
    admin=$(state_managed_id_name "$db" "$ENV")
    admin=${admin//-/_}

    echo "Applying DDL to database $db..."
    
    # --username=$PGUSER --password=$PGPASSWORD --url=$PGHOST
    liquibase --changeLogFile=../iac/database/participants/master-changelog.xml \
    --username=$PGUSER \
    --password=$PGPASSWORD \
    --url=jdbc:postgresql://$PGHOST:5432/$db \
    --liquibase-schema-name=piipan \
    --headless=true update -Downer=$owner -Dadmin=$admin -Dreader=readonly -Dsuperuser=$SUPERUSER
    #liquibase --changeLogFile=../iac/database/participants/master-changelog.xml --username=postgres@venwgf-psql-participants-bgf --password=1Sg2qIOvFawLOYJqieYPyL9BkUA9GQb9pAwNmeYqRAUbospNJTg1CaU2ZewJoThZaA1! --url=jdbc:postgresql://venwgf-psql-participants-bgf.postgres.database.azure.com:5432/ea --headless=true update
    #liquibase --changeLogFile=../iac/database/participants/database-changelog.xml update
    #apply_ddl "$db" "$owner" "$admin"

  done < ../iac/env/"${azure_env}"/states.csv
}

main "$@"
