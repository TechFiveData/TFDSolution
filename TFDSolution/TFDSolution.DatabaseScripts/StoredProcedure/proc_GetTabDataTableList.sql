DROP PROCEDURE IF EXISTS  [dbo].[proc_GetTabDataTableList] 
GO
    
-- =============================================    
-- Author:  Lavina Shaktawat    
-- Create date: 26/03/2025    
-- Description: <Description,,>    
-- =============================================    

/*DECLARE @ErrorMessage VARCHAR(200);    
EXEC dbo.proc_GetTabDataTableList     
  @TabId = 'c5154f64-2846-4299-a5a7-1e97654d193d'   ,  
	 @ParentId= 51,
  @ErrorMsg =@ErrorMessage out  
--PRINT 'Stored Procedure Execution Status: ' + @ErrorMessage; */   

CREATE PROCEDURE [dbo].[proc_GetTabDataTableList]       
    @TabId NVARCHAR(50),    
	@ParentId NVARCHAR(25),
 @ErrorMsg Varchar (200) OUTPUT    
AS    
BEGIN    
    
	DECLARE @FormId  AS NVARCHAR(50),    
		@SQLTableName  AS NVARCHAR(50),    
		@Columns AS NVARCHAR(MAX)='',    
		@SQL NVARCHAR(MAX);    
	DECLARE @LookupColumns NVARCHAR(MAX) = '';
	DECLARE @CompanyId AS NVARCHAR(50)    


	DECLARE @TempData TABLE ( RowNum INT,   
		FieldName VARCHAR(MAX),    
		DDLSourceType VARCHAR(100),    
		DDLSourceName VARCHAR(100),    
		DDLTextField VARCHAR(100),    
		DDLValueField VARCHAR(100)  ,
		SortOrder INT
		);
	DECLARE @TempData2 TABLE (  RowNum INT,    
		FieldName VARCHAR(MAX),    
		DDLSourceType VARCHAR(100),    
		DDLSourceName VARCHAR(100),    
		DDLTextField VARCHAR(100),    
		DDLValueField VARCHAR(100)  ,
		SortOrder INT
		);

	SELECT  @SQLTableName = tabSQLTableName  FROM m_FormTab WHERE FormTabId = @TabId 
	
	INSERT INTO @TempData (RowNum,
		FieldName,DDLSourceType,DDLSourceName,DDLTextField,DDLValueField,SortOrder)
		SELECT ROW_NUMBER() OVER (ORDER BY COLUMN_NAME desc) AS RowNum, 
		COLUMN_NAME,NULL,NULL,NULL,NULL ,0 FROM INFORMATION_SCHEMA.COLUMNS    
		WHERE TABLE_NAME = @SQLTableName AND COLUMN_NAME IN ('ID')      
    UNION 
		SELECT ROW_NUMBER() OVER (ORDER BY Field.SortOrder ) as RowNum, 
		FieldName,Field.DDLSourceType,Field.DDLSourceName,Field.DDLTextField,Field.DDLValueField,Field.SortOrder
		FROM m_FormField  AS Field WITH (NOLOCK)      
		INNER JOIN m_FormTab AS tab WITH (NOLOCK)   ON Field.FormTabId = tab.FormTabId     
		WHERE Field.FormTabId = @TabId AND ISNULL(Field.IsVisibleInList ,0) =1  
	UNION
		SELECT ROW_NUMBER() OVER (ORDER BY COLUMN_NAME ASC) AS RowNum, 
		COLUMN_NAME,NULL,NULL,NULL,NULL ,1000 FROM INFORMATION_SCHEMA.COLUMNS    
		WHERE TABLE_NAME = @SQLTableName AND 
		COLUMN_NAME IN ('CreatedBy', 'CreatedOn','CompanyId','FiancialYearId','Status', 'UpdatedBy', 'UpdatedOn' )      
   
	INSERT INTO @TempData2 
	SELECT  ROW_NUMBER() OVER (ORDER BY SortOrder,RowNum  ASC ) Rownum, 
	FieldName,DDLSourceType, DDLSourceName, DDLTextField,
	DDLValueField ,SortOrder FROM @TempData

	SELECT @Columns = STRING_AGG(FieldName, ', ') FROM @TempData2  

	-- Build Lookup Column Replacement for Fields with Foreign Key References
	SELECT @LookupColumns = STRING_AGG(
    '(SELECT ' + DDLSourceName + '.' + DDLTextField + ' FROM ' + DDLSourceName + 
	' WHERE ' + DDLSourceName + '.' + DDLValueField + ' = ' + @SQLTableName + 
	'.' + FieldName + ') AS ' + FieldName
	, ', ') 
	FROM @TempData2 WHERE DDLSourceType IS NOT NULL;

	-- If there are lookup columns, replace them in the query
	IF LEN(ISNULL(@LookupColumns, '')) > 0
	BEGIN
		DECLARE @OriginalColumns NVARCHAR(MAX);
		DECLARE @FieldName NVARCHAR(100);
		DECLARE @LookupColumn NVARCHAR(500);
    
		-- Create a cursor to loop through lookup fields
		DECLARE LookupCursor CURSOR FOR
		SELECT FieldName, 
			   '(SELECT ' + DDLSourceName + '.' + DDLTextField + 
			   ' FROM ' + DDLSourceName + 
			   ' WHERE ' + DDLSourceName + '.' + CONVERT(VARCHAR(50), DDLValueField) + 
			   ' = ' + @SQLTableName + '.' + FieldName + ') AS ' + FieldName
		FROM @TempData2 WHERE DDLSourceType IS NOT NULL;

		-- Open the cursor
		OPEN LookupCursor;
    
		-- Fetch the first row
		FETCH NEXT FROM LookupCursor INTO @FieldName, @LookupColumn;
    
		-- Loop through all lookup columns and replace them in @Columns
		WHILE @@FETCH_STATUS = 0
		BEGIN
			SET @Columns = REPLACE(@Columns, @FieldName, @LookupColumn);
			FETCH NEXT FROM LookupCursor INTO @FieldName, @LookupColumn;
		END;

		-- Close and deallocate cursor
		CLOSE LookupCursor;
		DEALLOCATE LookupCursor;
	END;

	SET @SQL = 'SELECT ' + @Columns + ' FROM ' + QUOTENAME(@SQLTableName) +
				' WHERE ParentId = '+ CONVERT(VARCHAR(20), @ParentId) ;

	--select @SQL as finalQuery
	PRINT @SQL; -- Debugging    
	SET @ErrorMsg = 'Success'    
	EXEC sp_executesql @SQL;    
        
END