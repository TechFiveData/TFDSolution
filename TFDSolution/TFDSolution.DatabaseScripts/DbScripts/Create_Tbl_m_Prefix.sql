IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'm_Prefix' AND type = 'U')
BEGIN
	CREATE TABLE [dbo].[m_Prefix](
	[TableName] [varchar](30) NOT NULL,
	[FieldName] [varchar](50) NOT NULL,
	[Prefix] [varchar](3) NOT NULL
) 
END
GO