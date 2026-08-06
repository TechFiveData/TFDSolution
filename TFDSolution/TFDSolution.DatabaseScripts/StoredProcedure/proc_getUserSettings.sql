

DROP PROCEDURE IF EXISTS proc_getUserSettings
GO

-- =============================================
-- Author:		Lavina Shaktawat
-- Create date: 05/04/2025 
-- Description:	<Get user setting details by userid >
-- =============================================
CREATE PROCEDURE proc_getUserSettings
    @UserId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
	BEGIN TRY
		IF EXISTS (SELECT 1 FROM m_UserSettings WITH (NOLOCK) WHERE UserId = @UserId)
        BEGIN
			SELECT 	UserSettingId, UserId, CompanyId,DefaultFiancialYearId		
			FROM m_UserSettings WITH (NOLOCK)
			WHERE UserId = (CASE WHEN ISNULL(@UserId,'') = '' THEN UserId ELSE @UserId END)
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