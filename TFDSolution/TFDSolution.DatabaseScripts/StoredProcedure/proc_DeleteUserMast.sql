DROP PROCEDURE IF EXISTS proc_DeleteUserMast
GO

-- =============================================
-- Author:		Lavina Shaktawat
-- Create date: 05/04/2025 
-- Description:	<Delete User company setting based on company id and userid>
-- =============================================
CREATE PROCEDURE proc_DeleteUserMast
    @UserId UNIQUEIDENTIFIER 
AS
BEGIN
    SET NOCOUNT ON;
	DECLARE @MessageText AS VARCHAR(500) = ''
	DECLARE @IsSuccess AS BIT = 0

	BEGIN TRY
		IF EXISTS (SELECT 1 FROM m_UserMast WITH (NOLOCK) WHERE UserId = @UserId )
        BEGIN
			UPDATE m_UserMast SET IsActive =0 WHERE UserId = @UserId
			SET @IsSuccess = 1
			SET @MessageText = 'Record Deleted successfully.'
		END
        ELSE
        BEGIN
			SET @MessageText = 'Record Not Found.'
        END
		SELECT @IsSuccess as IsSuccess, @MessageText as Response, 'Delete' as Action, @UserId as PrimaryId 
	END TRY
    BEGIN CATCH
		SET @IsSuccess = 0
		SET @MessageText = ERROR_MESSAGE()
		SELECT @IsSuccess as IsSuccess, @MessageText as Response, 'Error' as Action
    END CATCH
END
GO