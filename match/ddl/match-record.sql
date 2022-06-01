BEGIN;

DO $$ BEGIN
    CREATE TYPE hash_type AS ENUM ('ldshash');
EXCEPTION
    WHEN duplicate_object THEN
        RAISE NOTICE 'hash_type ENUM already exists, skipping';
END $$;

CREATE TABLE IF NOT EXISTS matches(
    id serial PRIMARY KEY,
    match_id text UNIQUE NOT NULL,
    created_at timestamptz NOT NULL,
    initiator text NOT NULL,
    states text[2] NOT NULL,
    hash text NOT NULL,
    hash_type hash_type NOT NULL default 'ldshash',
    input jsonb,
    data jsonb NOT NULL
);

COMMENT ON TABLE matches IS 'Match records';
COMMENT ON COLUMN matches.match_id IS 'Match record''s human-readable unique identifier.';
COMMENT ON COLUMN matches.created_at IS 'Match record''s creation date/time.';
COMMENT ON COLUMN matches.initiator IS 'Match record''s initiating entity.';
COMMENT ON COLUMN matches.states IS 'State/territory pair involved in match.';
COMMENT ON COLUMN matches.hash IS 'Value of hash used to identify match.';
COMMENT ON COLUMN matches.hash_type IS 'Type of hash used to identify match.';
COMMENT ON COLUMN matches.input IS 'Incoming data from real-time match request.';
COMMENT ON COLUMN matches.data IS 'Response data from match request.';

CREATE TABLE IF NOT EXISTS match_res_events(
    id serial PRIMARY KEY,
    match_id text NOT NULL REFERENCES matches (match_id),
    inserted_at timestamptz NOT NULL DEFAULT (NOW() AT TIME ZONE 'utc'),
    actor text NOT NULL,
    actor_state text,
    delta jsonb NOT NULL
);

CREATE INDEX IF NOT EXISTS index_match_id_on_match_res_events on match_res_events (match_id);

COMMENT ON TABLE match_res_events IS 'Match resolution events';
COMMENT ON COLUMN match_res_events.match_id IS 'References match ID of original match';
COMMENT ON COLUMN match_res_events.actor IS 'the person or automated system performing this change';
COMMENT ON COLUMN match_res_events.actor_state IS 'indicates if the actor is associated with a state involved in the match';
COMMENT ON COLUMN match_res_events.delta IS 'json object representing data changes submitted by states, as well as stateful domain data like match status';

CREATE TABLE IF NOT EXISTS state_info(
    id text UNIQUE PRIMARY KEY,
    state text UNIQUE NOT NULL,
    state_abbreviation text NOT NULL,
    email text NOT NULL,
    phone text,
	region text NOT NULL
);

COMMENT ON COLUMN state_info.id IS 'Unique number for each state. Will be alphabetical - Alabama = 1, Alaska = 2 etc';
COMMENT ON COLUMN state_info.phone IS 'The phone number contact';
COMMENT ON COLUMN state_info.email IS 'The email to contact that state';
COMMENT ON COLUMN state_info.state IS 'State/territory full name';
COMMENT ON COLUMN state_info.state_abbreviation IS 'State/territorys two letter abbreviation';
COMMENT ON COLUMN state_info.region IS 'The region the specified state belongs to';

INSERT INTO state_info(id, state, state_abbreviation, email, phone)
select * from (select '15' as id, 'Iowa' as state, 'IA' as state_abbreviation, 'IA-test@usda.gov' as email, '1234567890' as phone, 'MWRO' AS region) as temp
WHERE NOT EXISTS 
(select id from state_info where id = '15') limit 1;

INSERT INTO state_info(id, state, state_abbreviation, email, phone)
select * from (select '18' as id, 'Louisiana' as state, 'LA' as state_abbreviation, 'LA-test@usda.gov' as email, '1234567890' as phone, 'SWRO' AS region) as temp
WHERE NOT EXISTS 
(select id from state_info where id = '18') limit 1;

INSERT INTO state_info(id, state, state_abbreviation, email, phone)
select * from (select '21' as id, 'Massachusetts' as state, 'MA' as state_abbreviation, 'MA-test@usda.gov' as email, '1234567890' as phone, 'NERO' AS region) as temp
WHERE NOT EXISTS 
(select id from state_info where id = '21') limit 1;

INSERT INTO state_info(id, state, state_abbreviation, email, phone)
select * from (select '26' as id, 'Montana' as state, 'MT' as state_abbreviation, 'MT-test@usda.gov' as email, '1234567890' as phone, 'MPRO' AS region) as temp
WHERE NOT EXISTS 
(select id from state_info where id = '26') limit 1;

COMMIT;
