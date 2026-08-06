SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Lavina Shaktawat
-- Create date: 05/06/2025
-- Description:	< Get Table name by Form Name>
-- =============================================

CREATE OR ALTER PROCEDURE proc_GetTablesByFormName
    @FormName NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        SELECT tbl.TABLE_NAME
        FROM m_FormMast
        INNER JOIN m_FormTab ON m_FormTab.FormId = m_FormMast.FormId
        INNER JOIN INFORMATION_SCHEMA.TABLES tbl ON tbl.TABLE_NAME = m_FormTab.TabSQLTableName
        WHERE 
            m_FormTab.TabSQLTableName IS NOT NULL
            AND m_FormMast.FormName = @FormName;
    END TRY
    BEGIN CATCH
        -- You can log error details to a custom error log table if needed
        DECLARE @ErrorMessage NVARCHAR(4000), @ErrorSeverity INT, @ErrorState INT;
        SELECT 
            @ErrorMessage = ERROR_MESSAGE(),
            @ErrorSeverity = ERROR_SEVERITY(),
            @ErrorState = ERROR_STATE();

        RAISERROR (@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END