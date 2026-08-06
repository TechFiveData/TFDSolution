
DROP PROCEDURE IF EXISTS [dbo].[proc_GetColumnsByTable]
GO 
-- =============================================
-- Author:		Lavina Shaktawat
-- Create date: 16/03/2025
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[proc_GetColumnsByTable]
    @TableName NVARCHAR(200)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT COLUMN_NAME 
    FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = @TableName;
END;