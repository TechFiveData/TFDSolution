DROP PROCEDURE IF EXISTS proc_setUserSettings
GO
--Exec proc_getUserCompanies 'D9C916C6-95C7-48F1-8C5D-97D6C72B8042'

-- =============================================
-- Author:		Lavina Shaktawat
-- Create date: 05/04/2025 
-- Description:	<Set User company setting>
-- =============================================
CREATE PROCEDURE proc_setUserSettings
    @UserId UNIQUEIDENTIFIER ,
	@CompanyId UNIQUEIDENTIFIER,
	@DefaultYearID INT = NULL
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
			UPDATE m_UserSettings 
			SET DefaultFiancialYearId  = @DefaultYearID
			WHERE UserSettingId = @NewId ;

			SET @IsSuccess = 1
			SET @MessageText = 'Record save successfully.'
			
			SELECT @IsSuccess as IsSuccess, @MessageText as Response, 'Save' as Action, @NewId as PrimaryId 
		END
        ELSE
        BEGIN
            INSERT INTO m_UserSettings(UserId, CompanyId,DefaultFiancialYearId)
			VALUES (@UserId,@CompanyId,@DefaultYearID)
			set @NewId= scope_identity();
			SET @IsSuccess = 1
			SET @MessageText = 'Record save successfully.'
			SELECT @IsSuccess as IsSuccess, @MessageText as Response, 'Save' as Action, @NewId as PrimaryId 
        END
	END TRY
    BEGIN CATCH
		SET @IsSuccess = 0
		SET @MessageText = ERROR_MESSAGE()
		SELECT @IsSuccess as IsSuccess, @MessageText as Response, 'Error' as Action
    END CATCH
END
GO