DROP PROCEDURE IF EXISTS proc_deleteUserSettings
GO


-- =============================================
-- Author:		Lavina Shaktawat
-- Create date: 05/04/2025 
-- Description:	<Delete User company setting based on company id and userid>
-- =============================================
CREATE PROCEDURE proc_deleteUserSettings
    @UserId UNIQUEIDENTIFIER ,
	@CompanyId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
	DECLARE @MessageText AS VARCHAR(500) = ''
	DECLARE @IsSuccess AS BIT = 0
	DECLARE @NewId int = 0;

	SELECT @NewId = UserSettingId FROM m_UserSettings WITH (NOLOCK) WHERE UserId = @UserId and companyID =@CompanyId 

	BEGIN TRY
		IF @NewId>0
        BEGIN
			Delete From m_UserSettings
			WHERE UserSettingId = @NewId   
		
			SET @IsSuccess = 1
			SET @MessageText = 'Record Deleted successfully.'
			
			SELECT @IsSuccess as IsSuccess, @MessageText as Response, 'Delete' as Action, @NewId as PrimaryId 
		END
        ELSE
        BEGIN
           	SET @IsSuccess = 0
			SET @MessageText = 'Record Not Found.'
			SELECT @IsSuccess as IsSuccess, @MessageText as Response, 'Delete' as Action
        END
	END TRY
    BEGIN CATCH
		SET @IsSuccess = 0
		SET @MessageText = ERROR_MESSAGE()
		SELECT @IsSuccess as IsSuccess, @MessageText as Response, 'Error' as Action
    END CATCH
END
GO