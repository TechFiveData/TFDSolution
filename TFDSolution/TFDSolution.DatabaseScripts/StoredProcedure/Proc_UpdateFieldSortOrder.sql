CREATE OR ALTER PROCEDURE Proc_UpdateFieldSortOrder
    @SortOrderTable dbo.UDT_SortOrderData READONLY
AS
-- =============================================
-- Author:		Lavina Shaktawat
-- Create date: 19/06/2025 
-- Description:	<Update Sorting Order of Fields listed in input tabletype parameter>
-- =============================================
BEGIN
    SET NOCOUNT ON;
	DECLARE @MessageText AS VARCHAR(500) = ''
	DECLARE @IsSuccess AS BIT = 0

    BEGIN TRY
        BEGIN TRANSACTION;
	
        -- Update SortOrder for each FieldID from the input table
        UPDATE f
        SET f.SortOrder = s.SortOrder
		FROM m_FormField f
        INNER JOIN @SortOrderTable s ON f.FieldId = s.FormId;

        COMMIT TRANSACTION;

		SET @IsSuccess = 1
		SET @MessageText = 'Record Updated successfully.'
		SELECT @IsSuccess as IsSuccess, @MessageText as Response, 'Update' as Action,Null as PrimaryId 
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

		SET @IsSuccess = 0
		SET @MessageText = ERROR_MESSAGE()
		SELECT @IsSuccess as IsSuccess, @MessageText as Response, 'Error' as Action
    END CATCH
END
GO

