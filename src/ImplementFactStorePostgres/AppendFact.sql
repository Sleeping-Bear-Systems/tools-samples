-- AppendFact.sql

INSERT INTO sbs_facts
	(stream_id, fact_type, fact_data)
VALUES (@streamId, @factType, @factData);
