Drop type if exists UDT_UserCompanySetting
GO

CREATE TYPE dbo.UDT_UserCompanySetting AS TABLE
(
    CompanyId UNIQUEIDENTIFIER,
    UserId UNIQUEIDENTIFIER,
	FinancialYearId INT
);