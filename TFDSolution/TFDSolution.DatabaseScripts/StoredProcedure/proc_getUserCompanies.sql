DROP PROCEDURE IF EXISTS proc_getUserCompanies
GO

-- =============================================
-- Author:		Lavina Shaktawat
-- Create date: 05/04/2025 
-- Description:	<Get Company by userid>
-- =============================================
--Exec proc_getUserCompanies 'D9C916C6-95C7-48F1-8C5D-97D6C72B8042'
CREATE PROCEDURE proc_getUserCompanies
    @UserId UNIQUEIDENTIFIER 
AS
BEGIN
    SET NOCOUNT ON;
	DECLARE @UserTbl TABLE
	(UserId uniqueidentifier, CompanyId UNIQUEIDENTIFIER, CompanyName VARCHAR(50), FiancialYearId INT, StartDate DATETIME, EndDate DATETIME)
	BEGIN TRY
		INSERT INTO @UserTbl(UserId,CompanyId,CompanyName,FiancialYearId,StartDate,EndDate)
		SELECT U.UserId, C.CompanyId, C.CompanyName,F.FiancialYearId,F.StartDate,F.EndDate
		From m_UserMast U WITH (NOLOCK)
		INNER JOIN m_UserSettings S WITH (NOLOCK) ON U.UserId = S.UserId
		INNER JOIN m_CompanyMast C WITH (NOLOCK) ON S.CompanyId = C.CompanyId
		INNER JOIN m_CompanyFinancials F WITH (NOLOCK) ON F.CompanyId = C.CompanyID 
			AND F.IsActive=1
		WHERE U.UserId = @UserId
			   		
		IF EXISTS (SELECT 1 FROM @UserTbl)
        BEGIN
			Select Distinct UserId ,CompanyId ,CompanyName
			From @UserTbl

			Select Distinct CompanyId ,FiancialYearId,StartDate ,EndDate
			From @UserTbl
	
		END
        ELSE
        BEGIN
            RAISERROR('User Not Exists', 16, 1);
        END
	END TRY
    BEGIN CATCH
        DECLARE @ErrMsg NVARCHAR(4000), @ErrSeverity INT, @ErrState INT;
        SELECT @ErrMsg = ERROR_MESSAGE(), @ErrSeverity = ERROR_SEVERITY(), @ErrState = ERROR_STATE();
        RAISERROR(@ErrMsg, @ErrSeverity, @ErrState);
    END CATCH
END
GO