DROP PROCEDURE IF EXISTS [dbo].[proc_GetSPResultSchema]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Lavina Shaktawat
-- Create date: 18/03/2025
-- Description:	<Description,,>
-- =============================================
-- EXEC proc_GetSPResultSchema @SPName ='proc_getParty'
CREATE PROCEDURE proc_GetSPResultSchema
    @SPName NVARCHAR(128)
AS
BEGIN
    SET NOCOUNT ON;
	
	SELECT name AS ColumnName, system_type_name AS DataType, is_nullable AS IsNullable 
	FROM sys.dm_exec_describe_first_result_set(@SPName, NULL, 0)
END
