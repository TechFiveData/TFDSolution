
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'm_Countries' AND type = 'U')
BEGIN
CREATE TABLE m_Countries (
    Country_id INT PRIMARY KEY,
    Country_name VARCHAR(100) NOT NULL,
    Capital_city VARCHAR(100),
    Population INT,
    Area_km2 INT,
    Region VARCHAR(50),
    Currency VARCHAR(50),
    Official_language VARCHAR(100)
);

	INSERT INTO m_Countries (Country_id, Country_name, Capital_city, Population, Area_km2, Region, Currency, Official_language)
	VALUES
	(1, 'India', 'Dehli', 331002651, 9833517, 'Asia', 'INR', 'Hindi'),
	(2, 'Canada', 'Ottawa', 37742154, 9984670, 'North America', 'CAD', 'English, French'),
	(3, 'Germany', 'Berlin', 83783942, 357022, 'Europe', 'EUR', 'German'),
	(4, 'Japan', 'Tokyo', 126856000, 377975, 'Asia', 'JPY', 'Japanese'),
	(5, 'Australia', 'Canberra', 25499884, 7692024, 'Oceania', 'AUD', 'English');
END