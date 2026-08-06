--DROP TABLE IF EXISTS m_Parameters

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'm_Parameters' AND type = 'U')
BEGIN
    CREATE TABLE m_Parameters(
    EntryType VARCHAR(50) NOT NULL,
	[Value] INT NOT NULL,
	[Description] VARCHAR(100) NOT NULL,	
	EntryTypeDesc VARCHAR(100)  NULL,
	EntryCode VARCHAR(50)  NULL   
	)
END
GO
