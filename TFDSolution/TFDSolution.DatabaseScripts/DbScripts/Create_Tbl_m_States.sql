
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'm_States' AND type = 'U')
BEGIN
	CREATE TABLE m_States (
		State_id INT PRIMARY KEY,
		State_name VARCHAR(100) NOT NULL,
		Country_id INT,
		Capital_city VARCHAR(100),
		Population INT,
		Area_km2 INT,
		Region VARCHAR(50),
		FOREIGN KEY (Country_id) REFERENCES m_Countries(Country_id)
	);

	-- Assuming country_id for India is 1 in the 'countries' table
	INSERT INTO m_States (State_id, State_name, Country_id, Capital_city, Population, Area_km2, Region)
	VALUES
	(1, 'Andhra Pradesh', 1, 'Amaravati', 49577103, 162968, 'South'),
	(2, 'Arunachal Pradesh', 1, 'Itanagar', 1382611, 83743, 'North East'),
	(3, 'Assam', 1, 'Dispur', 31205576, 78438, 'North East'),
	(4, 'Bihar', 1, 'Patna', 104099452, 94163, 'East'),
	(5, 'Chhattisgarh', 1, 'Raipur', 25545198, 135191, 'Central'),
	(6, 'Goa', 1, 'Panaji', 1458545, 3702, 'West'),
	(7, 'Gujarat', 1, 'Gandhinagar', 60439692, 196024, 'West'),
	(8, 'Haryana', 1, 'Chandigarh', 25353081, 44212, 'North'),
	(9, 'Himachal Pradesh', 1, 'Shimla', 6864602, 55673, 'North'),
	(10, 'Jharkhand', 1, 'Ranchi', 32988134, 79714, 'East'),
	(11, 'Karnataka', 1, 'Bengaluru', 61095297, 191791, 'South'),
	(12, 'Kerala', 1, 'Thiruvananthapuram', 33406061, 38863, 'South'),
	(13, 'Madhya Pradesh', 1, 'Bhopal', 72626809, 308350, 'Central'),
	(14, 'Maharashtra', 1, 'Mumbai', 112374333, 307713, 'West'),
	(15, 'Manipur', 1, 'Imphal', 2570390, 22327, 'North East'),
	(16, 'Meghalaya', 1, 'Shillong', 2966889, 22429, 'North East'),
	(17, 'Mizoram', 1, 'Aizawl', 1097206, 21081, 'North East'),
	(18, 'Nagaland', 1, 'Kohima', 1980602, 16579, 'North East'),
	(19, 'Odisha', 1, 'Bhubaneswar', 46487116, 155707, 'East'),
	(20, 'Punjab', 1, 'Chandigarh', 27704236, 50362, 'North'),
	(21, 'Rajasthan', 1, 'Jaipur', 68548437, 342239, 'West'),
	(22, 'Sikkim', 1, 'Gangtok', 610577, 7096, 'North East'),
	(23, 'Tamil Nadu', 1, 'Chennai', 72138958, 130058, 'South'),
	(24, 'Telangana', 1, 'Hyderabad', 35003674, 112077, 'South'),
	(25, 'Tripura', 1, 'Agartala', 3671032, 10486, 'North East'),
	(26, 'Uttar Pradesh', 1, 'Lucknow', 199812341, 243286, 'North'),
	(27, 'Uttarakhand', 1, 'Dehradun', 10086292, 53483, 'North'),
	(28, 'West Bengal', 1, 'Kolkata', 91276115, 88752, 'East');
END