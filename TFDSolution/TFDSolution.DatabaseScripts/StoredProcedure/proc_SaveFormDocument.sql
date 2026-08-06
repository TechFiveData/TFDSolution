DROP PROCEDURE IF EXISTS [dbo].[proc_SaveFormDocument]
GO
-- =============================================
-- Author:		Lavina Shaktawat
-- Create date: 01/04/2025
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[proc_SaveFormDocument]
    @DocId INT = NULL,  -- Nullable for insert/update logic
    @DocName NVARCHAR(50),
    @DocAlias NVARCHAR(50),
    @StartNo NVARCHAR(50),
	@FormId NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
	DECLARE @MessageText AS VARCHAR(500) = ''
	DECLARE @IsSuccess AS BIT = 0
	DECLARE @PrimaryId AS INT 
	BEGIN TRY
		-- If DocId is provided, update the existing record; otherwise, insert a new record
		IF EXISTS (SELECT 1 FROM m_FormDocNoSetting WHERE DocNoSettingId = @DocId)
		BEGIN
			UPDATE m_FormDocNoSetting
			SET 
				DocName = @DocName,
				DocAlias = @DocAlias,
				StartNumber = @StartNo,
				FormId = CAST (@FormId AS uniqueidentifier)
			WHERE DocNoSettingId = @DocId;
			SET @PrimaryId = @DocId;
			SELECT @IsSuccess as IsSuccess, @MessageText as Response, 'Save' as Action, @PrimaryId as PrimaryId 

		END
		ELSE
		BEGIN
			INSERT INTO m_FormDocNoSetting (DocName, DocAlias, StartNumber,FormId)
			VALUES (@DocName, @DocAlias, @StartNo, @FormId);
			SET @PrimaryId = SCOPE_IDENTITY();  -- Get recently inserted ID
			SET @IsSuccess = 1;
			SET @MessageText = 'Record save successfully';
			SELECT @IsSuccess as IsSuccess, @MessageText as Response, 'Save' as Action, @PrimaryId as PrimaryId 

		END
	END TRY
    BEGIN CATCH
		SET @IsSuccess = 0
		SET @MessageText = ERROR_MESSAGE();
		SELECT @IsSuccess as IsSuccess, @MessageText as Response, 'Error' as Action, @PrimaryId as PrimaryId 
    END CATCH
END;