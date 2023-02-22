-- PopulateDatabase.sql

CREATE TABLE sbs_user_data
(
	id       INTEGER     NOT NULL PRIMARY KEY,
	name     VARCHAR(32) NOT NULL,
	password VARCHAR(32) NOT NULL
);

INSERT INTO sbs_user_data
	(id, name, password)
VALUES (1, 'john_indigo', 'password_123'),
	   (2, 'jane_grey', 'password_234'),
	   (3, 'laura_white', 'password_345'),
	   (4, 'chris_black', 'password_456'),
	   (5, 'tina_green', 'password_567'),
	   (6, 'roger_blue', 'password_678'),
	   (7, 'edna_red', 'password_789')
