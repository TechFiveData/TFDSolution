DROP PROCEDURE IF EXISTS proc_GetUserMastList
GO

-- =============================================
-- Author:		Lavina Shaktawat
-- Create date: 05/04/2025 
-- Description:	<Get User Master if userid supplied else all list>
-- =============================================
CREATE PROCEDURE proc_GetUserMastList
    @UserId VARCHAR(50) = null
AS
BEGIN
    SET NOCOUNT ON;
	BEGIN TRY	
		SELECT UserId			
				,FirstName		
				,LastName		
				,EmailId			
				,UserName		
				,Password		
				,IsActive		
				,CreatedOn		
				,UpdatedOn		
				,LastLoginDate	
				,PasswordExpiredOn
		FROM m_UserMast 
		WHERE UserId = (CASE WHEN ISNULL(@UserId,'') = '' THEN UserId ELSE @UserId END)
		
	END TRY
    BEGIN CATCH
        DECLARE @ErrMsg NVARCHAR(4000), @ErrSeverity INT, @ErrState INT;
        SELECT @ErrMsg = ERROR_MESSAGE(), @ErrSeverity = ERROR_SEVERITY(), @ErrState = ERROR_STATE();
        RAISERROR(@ErrMsg, @ErrSeverity, @ErrState);
    END CATCH
END
GO