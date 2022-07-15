# ENVIRONMENT CONFIGURATION SCRIPT

# COPY OR INSERT ENVIRONMENT FILE TO A PROVIDE NEW_LOCATION
safe_copy () {
    local ENV_FILE
    local NEW_LOCATION
    
    ENV_FILE=$1
    NEW_LOCATION=$2
    
    if [ -f "$ENV_FILE" ]; then
      echo "COPYING... $ENV_FILE to $NEW_LOCATION"
      # shellcheck source=./iac/env/tts/dev/config.bash
      cp -f ./env/tts/dev/insert-state-info.sql ../match/dml/insert-state-info.sql
    else 
      echo "ERROR COPYING... $ENV_FILE to $NEW_LOCATION"
    fi
}

main () {

  ENV_NAME=$1

  safe_copy "$ENV_NAME/insert-state-info.sql" ../match/dml/insert-state-info.sql

}

main "$@"
