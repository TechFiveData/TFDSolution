
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'm_City' AND type = 'U')
BEGIN

	CREATE TABLE m_City (
		City_id INT PRIMARY KEY,
		City_name VARCHAR(100) NOT NULL,
		State_id INT,
		Country_id INT,
		Region VARCHAR(50),
		FOREIGN KEY (State_id) REFERENCES m_States(State_id),
		FOREIGN KEY (Country_id) REFERENCES m_Countries(Country_id)
	);

	-- Assuming country_id for India is 1 in the 'countries' table
	INSERT INTO m_City (City_id, City_name, State_id, Country_id, Region)
	VALUES
	(1, 'Mumbai', 14, 1, 'West'),
	(2, 'Delhi', 26, 1, 'North'),
	(3, 'Bengaluru', 11,1,'South'),
	(4, 'Chennai', 23, 1,'South'),
	(5, 'Kolkata', 28, 1, 'East'),
	(6, 'Hyderabad', 24, 1, 'South'),
	(7, 'Ahmedabad', 7, 1, 'West'),
	(8, 'Pune', 7, 1, 'West'),
	(9, 'Surat', 7, 1, 'West'),
	(10, 'Jaipur', 21, 1, 'West'),
	(11, 'Lucknow', 26, 1, 'North'),
	(12, 'Chandigarh', 8, 1, 'North'),
	(13, 'Patna', 4, 1, 'East'),
	(14, 'Bhopal', 13, 1, 'Central'),
	(15, 'Kochi', 23, 1, 'South'),
	(16, 'Indore', 13, 1,'Central'),
	(17, 'Agra', 26, 1, 'North'),
	(18, 'Vadodara', 7, 1, 'West'),
	(19, 'Nagpur', 13, 1, 'Central'),
	(20, 'Visakhapatnam', 1, 1, 'South'),
	(21, 'Ludhiana', 20, 1, 'North'),
	(22, 'Nashik', 7, 1, 'West'),
	(23, 'Kanpur', 26, 1, 'North'),
	(24, 'Vijayawada', 1, 1, 'South'),
	(25, 'Jamshedpur', 10, 1, 'East');
END