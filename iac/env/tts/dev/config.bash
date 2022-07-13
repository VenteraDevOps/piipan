# ENVIRONMENT CONFIGURATION SCRIPT


safe_copy () {
    local OLD_FILE
    local NEW_FILE

    OLD_FILE=$1
    NEW_FILE=$2

    if [ -f "$OLD_FILE" ] && [ -f "$NEW_FILE" ]; then
      echo "COPYING... $OLD_FILE to $NEW_FILE"
      # shellcheck source=./iac/env/tts/dev/config.bash
      cp -f ./env/tts/dev/insert-state-info.sql ../match/dml/insert-state-info.sql
    else 
      echo "ERROR COPYING... $OLD_FILE to $NEW_FILE"
    fi

}

main () {

    safe_copy ./env/tts/dev/insert-state-info.sql ../match/dml/insert-state-info.sql
}

main "$@"
