BEGIN;

CREATE TABLE IF NOT EXISTS participant_uploads(
    id serial PRIMARY KEY,
    state VARCHAR(50) NOT NULL,
    uploaded_at timestamptz NOT NULL,
    completed_at  timestamptz,
    status text NOT NULL,
    upload_identifier text,
    participants_uploaded bigint,
    error_message text
);

COMMENT ON TABLE participant_uploads IS 'Participant bulk upload event record';

CREATE TABLE IF NOT EXISTS participant_searchs(
    id serial PRIMARY KEY,
    state VARCHAR(50) NOT NULL,
    search_reason VARCHAR(100) NOT NULL,
    search_from VARCHAR(50) NULL,
    match_creation VARCHAR(50) NULL,
    match_count int,
    searched_at timestamptz NOT NULL
);

COMMENT ON TABLE participant_searchs IS 'Participant search event record';
COMMIT;
