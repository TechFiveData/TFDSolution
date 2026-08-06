DROP PROCEDURE IF EXISTS [dbo].[proc_SaveFormPending]
GO
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Lavina Shaktawat
-- Create date: 09/04/2025
-- Description:	<Save Form Pending Details based on FormPendingId >
-- =============================================
CREATE PROCEDURE [dbo].[proc_SaveFormPending]
    @FormPendingId INT = 0,
    @FormId UNIQUEIDENTIFIER,
    @FormTabName NVARCHAR(50),
    @SourceName NVARCHAR(100),
    @SourceRequest NVARCHAR(500),
    @SortOrder INT,
    @UserId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
	DECLARE @Action NVARCHAR(10), 
	@Response NVARCHAR(200),
	@IsSuccess BIT=0;

    BEGIN TRY
        IF ISNULL(@FormPendingId,0)=0
        BEGIN
            -- INSERT
            INSERT INTO [dbo].[m_FormPending]
            (
                FormId, FormTabName, SourceName, SourceRequest,
                SortOrder,  CreatedBy,CreatedOn
            )
            VALUES
            (
                @FormId, @FormTabName, @SourceName, @SourceRequest,
                @SortOrder, @UserId, GETDATE()
            );
			SET @FormPendingId = SCOPE_IDENTITY();
        END
        ELSE
        BEGIN
            -- UPDATE
            UPDATE [dbo].[m_FormPending]
            SET 
                FormId = @FormId,
                FormTabName = @FormTabName,
                SourceName = @SourceName,
                SourceRequest = @SourceRequest,
                SortOrder = @SortOrder
            WHERE FormPendingId = @FormPendingId;
        END
		SET @Action = 'Save';
        SET @Response = 'Record saved successfully.';
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