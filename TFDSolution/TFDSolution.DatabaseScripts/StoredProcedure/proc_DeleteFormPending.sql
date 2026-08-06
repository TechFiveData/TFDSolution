DROP PROCEDURE IF EXISTS [dbo].[proc_DeleteFormPending]
GO
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Lavina Shaktawat
-- Create date: 09/04/2025
-- Description:	<Delete Form Pending Details based on FormPendingId >
-- =============================================
CREATE PROCEDURE [dbo].[proc_DeleteFormPending]
    @FormPendingId INT 
AS
BEGIN
    SET NOCOUNT ON;
	DECLARE @Action NVARCHAR(10), 
	@Response NVARCHAR(200),
	@IsSuccess BIT=0;

    BEGIN TRY
		
		DELETE FROM m_FormPending WHERE FormPendingId = @FormPendingId;

		SET @Action = 'Delete';
        SET @Response = 'Record deleted successfully.';
        SET @IsSuccess = 1;
		SELECT @Action AS Action, @FormPendingId AS PrimaryId, @IsSuccess AS IsSuccess, @Response AS Response		
    END TRY
    BEGIN CATCH
         SET @Action = 'Error';
        SET @Response = ERROR_MESSAGE();
        SET @IsSuccess = 0;
		SELECT @Action AS Action,  @IsSuccess AS IsSuccess, @Response AS Response		
    END CATCH
END;