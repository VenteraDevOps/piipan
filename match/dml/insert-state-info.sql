BEGIN;

INSERT INTO state_info(id, state, state_abbreviation, email, phone, region)
select * from (select '15' as id, 'Iowa' as state, 'IA' as state_abbreviation, 'IA-test@usda.gov' as email, '1234567890' as phone, 'MWRO' AS region) as temp
WHERE NOT EXISTS 
(select id from state_info where id = '15') limit 1;

INSERT INTO state_info(id, state, state_abbreviation, email, phone, region)
select * from (select '18' as id, 'Louisiana' as state, 'LA' as state_abbreviation, 'LA-test@usda.gov' as email, '1234567890' as phone, 'SWRO' AS region) as temp
WHERE NOT EXISTS 
(select id from state_info where id = '18') limit 1;

INSERT INTO state_info(id, state, state_abbreviation, email, phone, region)
select * from (select '21' as id, 'Massachusetts' as state, 'MA' as state_abbreviation, 'MA-test@usda.gov' as email, '1234567890' as phone, 'NERO' AS region) as temp
WHERE NOT EXISTS 
(select id from state_info where id = '21') limit 1;

INSERT INTO state_info(id, state, state_abbreviation, email, phone, region)
select * from (select '26' as id, 'Montana' as state, 'MT' as state_abbreviation, 'MT-test@usda.gov' as email, '1234567890' as phone, 'MPRO' AS region) as temp
WHERE NOT EXISTS 
(select id from state_info where id = '26') limit 1;

INSERT INTO state_info(id, state, state_abbreviation, email, phone, region)
select * from (select '101' as id, 'Echo Alpha' as state, 'EA' as state_abbreviation, 'EA-test@usda.gov' as email, '1234567890' as phone, 'EA-MPRO' AS region) as temp
WHERE NOT EXISTS 
(select id from state_info where id = '101') limit 1;

INSERT INTO state_info(id, state, state_abbreviation, email, phone, region)
select * from (select '102' as id, 'Echo Bravo' as state, 'EB' as state_abbreviation, 'EB-test@usda.gov' as email, '1234567890' as phone, 'EB-MPRO' AS region) as temp
WHERE NOT EXISTS 
(select id from state_info where id = '102') limit 1;

INSERT INTO state_info(id, state, state_abbreviation, email, phone, region)
select * from (select '103' as id, 'Echo Charlie' as state, 'EC' as state_abbreviation, 'EC-test@usda.gov' as email, '1234567890' as phone, 'EC-MPRO' AS region) as temp
WHERE NOT EXISTS 
(select id from state_info where id = '103') limit 1;

COMMIT;