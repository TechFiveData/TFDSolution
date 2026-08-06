DROP PROCEDURE IF EXISTS [dbo].[proc_AddField]
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

--exec proc_AddField 'm_Paramcurrency', 'Complex','Textbox','100'

-- =============================================
-- Author:		Amit Gorvadiya
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE proc_AddField
	-- Add the parameters for the stored procedure here
	@SQLTableName AS VARCHAR(50),
	@FieldName AS VARCHAR(50),
	@FieldType AS VARCHAR(50),
	@FieldLength AS VARCHAR(5) = NULL
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	DECLARE @createtableCommandString AS NVARCHAR(MAX) = ''

    IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = @SQLTableName AND type = 'U')
	BEGIN
		SET @createtableCommandString  = 'CREATE TABLE [dbo].' + @SQLTableName + '
	    ([Id] [int] IDENTITY(1,1) NOT NULL
		CONSTRAINT [PK_' + @SQLTableName + '] PRIMARY KEY CLUSTERED ([Id] ASC)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]) ON [PRIMARY]';		
		EXECUTE sp_executesql @createtableCommandString
	END

	IF EXISTS (SELECT 1 FROM sys.tables WHERE name = @SQLTableName AND type = 'U')
	BEGIN		
		IF @FieldType = 'TextBox' OR @FieldType = 'MultiText'
		BEGIN

			SET @createtableCommandString  = 'ALTER TABLE ' + @SQLTableName + ' ADD ' + @FieldName + ' VARCHAR('+ @FieldLength +') NULL'
			EXECUTE sp_executesql @createtableCommandString
		END
		ELSE IF @FieldType = 'RadioButton' OR @FieldType = 'CheckBox'
		BEGIN
			
			SET @createtableCommandString  = 'ALTER TABLE ' + @SQLTableName + ' ADD ' + @FieldName + ' BIT NULL'
			EXECUTE sp_executesql @createtableCommandString
		END
		ELSE IF @FieldType = 'Selection' OR @FieldType = 'Dropdown' OR @FieldType = 'Label'
		BEGIN
			
			SET @createtableCommandString  = 'ALTER TABLE ' + @SQLTableName + ' ADD ' + @FieldName + ' NVARCHAR(200) NULL'
			EXECUTE sp_executesql @createtableCommandString
		END
		ELSE IF @FieldType = 'DateTimeField' OR @FieldType = 'DateTimeField'
		BEGIN
			
			SET @createtableCommandString  = 'ALTER TABLE ' + @SQLTableName + ' ADD ' + @FieldName + ' datetime NULL'
			EXECUTE sp_executesql @createtableCommandString
		END
	END
END
GO
