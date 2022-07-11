# ENVIRONMENT CONFIGURATION SCRIPT

main () {

    echo "COPYING... ./env/tts/dev/insert-state-info.sql ../../../match/dml/insert-state-info.sql"
    cp -f ./env/tts/dev/insert-state-info.sql ../match/dml/insert-state-info.sql

}

main "$@"
