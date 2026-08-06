
DROP TYPE [dbo].[Udt_FieldsData]
GO

CREATE TYPE [dbo].[Udt_FieldsData] AS TABLE(
	[FieldName] [nvarchar](128) NULL,
	[FieldValue] [nvarchar](max) NULL,
	[DataType] [nvarchar](50) NULL
)
GO


