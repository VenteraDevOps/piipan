BEGIN;

CREATE TABLE IF NOT EXISTS participant_uploads(
    id serial PRIMARY KEY,
    state VARCHAR(50) NOT NULL,
    uploaded_at timestamptz NOT NULL
);

COMMENT ON TABLE participant_uploads IS 'Participant bulk upload event record';

COMMIT;
