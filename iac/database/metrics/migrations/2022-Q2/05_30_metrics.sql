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