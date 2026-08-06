DROP PROCEDURE IF EXISTS [dbo].[procDeleteHeaderFieldData]


SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Lavina Shaktawat
-- Create date: 28/03/2025
-- Description:	<Description,,>
-- =============================================
/*

EXEC dbo.procDeleteHeaderFieldData 
    @UserID = '6D07CF92-1B5F-42DF-83BA-0ADF83B914D2',  
    @PageName = 'colour', 
    @PrimaryId = 5
*/

CREATE PROCEDURE [dbo].[procDeleteHeaderFieldData]
    @UserID UNIQUEIDENTIFIER,    
    @PageName NVARCHAR(100),
	@PrimaryId BIGINT      
AS
BEGIN 
	BEGIN TRY
		DECLARE @MessageText AS VARCHAR(500) = ''
		DECLARE @IsSuccess AS BIT = 0
		DECLARE @SQLTableName NVARCHAR(50) 
		DECLARE @SQL NVARCHAR(MAX)

		SELECT @SQLTableName = SQLTableName FROM m_FormMast Where FormName = @PageName 

		IF ISNULL(@SQLTableName, '') = '' OR
			NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = @SQLTableName AND TABLE_SCHEMA = 'dbo')
		BEGIN
			SET @MessageText = 'Table Not Exists';
			SELECT @IsSuccess AS IsSuccess, @MessageText AS Response, 'Error' AS Action, @PrimaryId AS PrimaryId;
			RETURN;
		END
			
		 -- Construct Dynamic SQL for DELETE
        SET @SQL = 'DELETE FROM ' + QUOTENAME(@SQLTableName) + ' WHERE Id = @PrimaryId'

        -- Execute the Dynamic SQL
        EXEC sp_executesql @SQL, N'@PrimaryId BIGINT', @PrimaryId;

        -- Success Response
        SET @IsSuccess = 1;
        SET @MessageText = 'Record Deleted Successfully';
        SELECT @IsSuccess AS IsSuccess, @MessageText AS Response, 'Delete' AS Action, @PrimaryId AS PrimaryId;

	END TRY
	BEGIN CATCH
		SET @IsSuccess = 0
		SET @MessageText = ERROR_MESSAGE();
		SELECT @IsSuccess as IsSuccess, @MessageText as Response, 'Error' as Action, @PrimaryId as PrimaryId 
	END CATCH
END;