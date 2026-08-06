
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'm_FormMast' AND type = 'U')
BEGIN

CREATE TABLE [dbo].[m_FormMast](
	[FormId] [uniqueidentifier] NOT NULL,
	[FormName] [nvarchar](50) NOT NULL,
	[FormTitle] [nvarchar](50) NOT NULL,
	[FormDescription] [nvarchar](500) NULL,
	[IsActive] [bit] NOT NULL,
	[ParentFormId] [uniqueidentifier] NULL,
	[SQLTableName] [varchar](30) NULL,
	[Prefix] [varchar](50) NULL,
	[CompanyId] [uniqueidentifier] NULL,
	[SqlTemplateId] [int] NULL,
 CONSTRAINT [PK_m_FormMast] PRIMARY KEY CLUSTERED 
(
	[FormId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
END
GO


UPDATE m_FormMast SET Prefix='m_Mast' WHERE FormName='Master' 
UPDATE m_FormMast SET Prefix='t_Sales' WHERE FormName='Sales' 
UPDATE m_FormMast SET Prefix='t_Purchase' WHERE FormName='Purchase' 
UPDATE m_FormMast SET Prefix='t_Param' WHERE FormName='Parameters' 
GO

---17/06/2025
Alter Table m_FormMast Add SortOrder INT NULL
