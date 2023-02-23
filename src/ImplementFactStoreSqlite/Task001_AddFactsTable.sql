-- Task001_AddFactsTable.sql

CREATE TABLE sbs_facts
(
	id        INTEGER     NOT NULL PRIMARY KEY AUTOINCREMENT,
	stream_id VARCHAR(64) NOT NULL,
	fact_type VARCHAR(32) NOT NULL,
	fact_data BLOB        NOT NULL
);
