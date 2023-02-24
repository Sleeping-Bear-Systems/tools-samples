-- Task001_AddFactsTable.sql

CREATE TABLE sbs_facts
(
	id        SERIAL      NOT NULL PRIMARY KEY,
	stream_id VARCHAR(64) NOT NULL,
	fact_type VARCHAR(32) NOT NULL,
	fact_data BYTEA       NOT NULL
);
