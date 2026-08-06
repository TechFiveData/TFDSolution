DROP PROCEDURE IF EXISTS [dbo].[proc_GetAllStoredProcedures]
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
-- EXEC proc_GetAllStoredProcedures
CREATE PROCEDURE proc_GetAllStoredProcedures
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        p.name AS ProcedureName
       /* ,s.name AS SchemaName,
        p.create_date AS CreatedDate,
        p.modify_date AS ModifiedDate*/
    FROM sys.procedures p
    INNER JOIN sys.schemas s ON p.schema_id = s.schema_id
    ORDER BY p.name;
END;