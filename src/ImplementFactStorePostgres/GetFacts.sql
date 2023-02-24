-- GetFacts.sql

SELECT fact_type, fact_data
FROM sbs_facts
WHERE stream_id = @streamId
ORDER BY id asc;
