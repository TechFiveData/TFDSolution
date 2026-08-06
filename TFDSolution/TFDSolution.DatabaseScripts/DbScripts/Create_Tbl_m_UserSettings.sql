IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'm_UserSettings' AND type = 'U')
BEGIN
CREATE TABLE [dbo].[m_UserSettings](
	[UserSettingId] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [uniqueidentifier] NOT NULL,
	[CompanyId] [uniqueidentifier] NOT NULL,
	[DefaultFiancialYearId] [int] NOT NULL,
 CONSTRAINT [PK_m_UserSettings] PRIMARY KEY CLUSTERED 
(
	[UserSettingId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
END


