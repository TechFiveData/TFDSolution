
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE OR ALTER   PROCEDURE [dbo].[Proc_UpdateFollowUpDateTime]
	@FollowUpId INT ,
	@FollowUpDate DateTime	,
    @CreatedBy VARCHAR(50)
AS
/*
Created By: Lavina Shaktawat
Create Date: 26/7/2025
Purpose: To Update Follow DateTime
*/
BEGIN
    SET NOCOUNT ON;
	DECLARE @MessageText AS VARCHAR(500) = ''
	DECLARE @IsSuccess AS BIT = 0
	DECLARE @NewEventId AS BIGINT=0
	BEGIN TRY
		IF (@FollowUpId>0)
		BEGIN
			SET @NewEventId = @FollowUpId

			UPDATE t_FollowUp
			SET 
			FollowUpDate = @FollowUpDate,
			UpdatedBy =  @CreatedBy,
			UpdatedOn = GETDATE()
			WHERE FollowUpId = @FollowUpId;

			Update t_FollowUp 
			SET NextFollowUpDate = @FollowUpDate 
			WHERE NextFollowUpID = @FollowUpId


			SET @IsSuccess = 1
			SET @MessageText = 'Record save successfully.'

			SELECT @IsSuccess as IsSuccess, @MessageText as Response, 'Save' as Action, @FollowUpId as Id 
			return;

		END
		SELECT 0 as IsSuccess, 'Invalid Followup' as Response, 'Save' as Action, @FollowUpId as Id 
		

	END TRY
    BEGIN CATCH
		SET @IsSuccess = 0
		SET @MessageText = ERROR_MESSAGE();
		SELECT @IsSuccess as IsSuccess, @MessageText as Response, 'Error' as Action, @NewEventId as PrimaryId 
	END CATCH
END
