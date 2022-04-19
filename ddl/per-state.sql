BEGIN;

-- Creates participant records tables
-- Access controls for tables are defined in per-state-controls.sql

SET search_path=piipan,public;

CREATE TABLE IF NOT EXISTS uploads(
	id serial PRIMARY KEY,
	created_at timestamptz NOT NULL,
	publisher text NOT NULL,
	upload_identifier text NOT NULL
);

COMMENT ON TABLE uploads IS 'Bulk PII upload events';
COMMENT ON COLUMN uploads.created_at IS 'Date/time the records were uploaded in bulk';
COMMENT ON COLUMN uploads.publisher IS 'User or service account that performed the upload';
COMMENT ON COLUMN uploads.upload_identifier IS 'Unique ID for uploads for status inquiries';

CREATE TABLE IF NOT EXISTS participants(
	id serial PRIMARY KEY,
	lds_hash text NOT NULL
		CONSTRAINT hex_check CHECK (lds_hash ~ '^[0-9a-f]{128}$'),
	upload_id integer REFERENCES uploads (id),
    	case_id text NOT NULL,
    	participant_id text NOT NULL,
	participant_closing_date   date,
    	recent_benefit_issuance_dates daterange[],
    	vulnerable_individual boolean
);

COMMENT ON TABLE participants IS 'Program participant';
COMMENT ON COLUMN participants.lds_hash IS 'Participant''s deidentified data as hex value';
COMMENT ON COLUMN participants.case_id IS 'Participant''s state-specific case identifier';
COMMENT ON COLUMN participants.participant_id IS 'Participant''s state-specific identifier';
COMMENT ON COLUMN participants.participant_closing_date   IS 'Date when the Participant''s case will close. This will be the last date the participate is eligible to receive benefits.';
COMMENT ON COLUMN participants.recent_benefit_issuance_dates IS 'Participant''s recent benefit months issuances date ranges.';
COMMENT ON COLUMN participants.vulnerable_individual IS 'Participant''s vulnerability status';

CREATE INDEX IF NOT EXISTS participants_lds_hash_idx ON participants (lds_hash, upload_id);
CREATE UNIQUE INDEX IF NOT EXISTS participants_uniq_ids_idx ON participants (case_id, participant_id, upload_id);

COMMIT;
