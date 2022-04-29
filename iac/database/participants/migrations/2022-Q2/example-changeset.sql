ALTER TABLE IF EXISTS piipan.uploads
    ADD COLUMN IF NOT EXISTS error_message character varying COLLATE pg_catalog."default";