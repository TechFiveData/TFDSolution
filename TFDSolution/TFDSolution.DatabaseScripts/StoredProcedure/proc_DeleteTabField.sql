DROP PROCEDURE IF EXISTS  [dbo].[proc_DeleteTabField]
GO


-- =============================================
-- Author:		Lavina Shaktawat
-- Create date: 20/03/2025
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[proc_DeleteTabField]
    @PrimaryId NVARCHAR(50),
    @SourceTable NVARCHAR(50)
AS
BEGIN
	SET NOCOUNT ON;
	DECLARE @FormId  AS NVARCHAR(50)
	DECLARE @MessageText AS VARCHAR(500) = ''
	DECLARE @IsSuccess AS BIT = 0

	print @PrimaryId

    DECLARE @SQLTableName NVARCHAR(128)
    DECLARE @SQL NVARCHAR(MAX)
	DECLARE @RowCount INT
	
	  -- Data check karna
    SET @SQL = 'SELECT @FormId = FormId FROM ' + QUOTENAME(@SourceTable) + ' WHERE ' 
	SET @SQL = @SQL + (SELECT CASE @SourceTable WHEN 'm_FormField' THEN ' FieldId = '''+ @PrimaryId+'''' WHEN 'm_FormTab' THEN ' FormTabId ='''+@PrimaryId+'''' END)
	print @SQL
	EXEC sp_executesql @SQL, N'@FormId  NVARCHAR(50) OUTPUT', @FormId OUTPUT
		print @FormId
	IF (LEN(@FormId)>0 )
	BEGIN
		SELECT @SQLTableName = SQLTableName FROM m_FormMast WHERE FormId = @FormId
	END
	print @SQLTableName

	IF (LEN(@SQLTableName)>0)
	BEGIN
		IF ( @SourceTable = 'm_FormField')
		BEGIN	 print 'in tab'
			DECLARE @FieldName VARCHAR(30)
			SELECT @FieldName = FieldName FROM m_FormField WHERE FieldId = @PrimaryId
			SET @SQL = N'SELECT @RowCount = COUNT(*) FROM ' + QUOTENAME(@SQLTableName) + ' WHERE ISNULL('+ @FieldName +','''') != ''''' ;
			print @SQL
			EXEC sp_executesql @SQL, N'@RowCount INT OUTPUT', @RowCount OUTPUT;
			IF @RowCount > 0
			BEGIN
				SET @MessageText ='Failed! Records Exists'				
			END
			ELSE
			BEGIN
				DELETE FROM m_FormField where FieldId = @PrimaryId
				SET @MessageText = 'Field Deleted Successfully'
				SET @IsSuccess =1
			END
		END
		ELSE IF ( @SourceTable = 'm_FormTab')
		BEGIN
	
			SET @SQL = N'SELECT @RowCount = COUNT(*) FROM ' + QUOTENAME(@SQLTableName);

			EXEC sp_executesql @SQL, N'@RowCount INT OUTPUT', @RowCount OUTPUT;
			IF @RowCount > 0
			BEGIN
				SET @MessageText ='Failed! Records Exists'		
			END
			ELSE
			BEGIN
				DELETE FROM m_FormTab where FormTabId = @PrimaryId
				SET @MessageText = 'Tab Deleted Successfully'
				SET @IsSuccess =1
			END
		END 
	END
	ELSE
	BEGIN
		SET @MessageText ='Table Not Exist'		
	END

	SELECT @IsSuccess as IsSuccess, @MessageText as Response, 'Delete' as Action
END