
DROP PROCEDURE IF EXISTS [dbo].[proc_GetTablesByPrefix]
GO 
-- =============================================
-- Author:		Lavina Shaktawat
-- Create date: 16/03/2025
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[proc_GetTablesByPrefix]
    @Prefix NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TABLE_NAME 
    FROM INFORMATION_SCHEMA.TABLES 
    WHERE TABLE_NAME LIKE @Prefix + '%';
END;