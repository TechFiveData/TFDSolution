DROP PROCEDURE IF EXISTS proc_InsertAuditLog
GO

-- =============================================
-- Author:		Lavina Shaktawat
-- Create date: 06/04/2025 
-- Description:	<Save Audit log details >
-- =============================================

CREATE PROCEDURE proc_InsertAuditLog
    @CompanyId UNIQUEIDENTIFIER,
    @UserId UNIQUEIDENTIFIER,
    @FinancialYearId INT = NULL,
    @PageName NVARCHAR(200),
    @Action NVARCHAR(100),
    @RecordId NVARCHAR(100) = NULL,
    @FieldName NVARCHAR(100) = NULL,
    @FieldOldValue NVARCHAR(MAX) = NULL,
    @FieldNewValue NVARCHAR(MAX) = NULL,
    @IPAddress NVARCHAR(45) = NULL,
    @Remark NVARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;

      BEGIN TRY
        INSERT INTO AuditLogs
        (
            CompanyId,
            UserId,
            FinancialYearId,
            PageName,
            Action,
            RecordId,
            FieldName,
            FieldOldValue,
            FieldNewValue,
            IPAddress,
            Remark
        )
        VALUES
        (
            @CompanyId,
            @UserId,
            @FinancialYearId,
            @PageName,
            @Action,
            @RecordId,
            @FieldName,
            @FieldOldValue,
            @FieldNewValue,
            @IPAddress,
            @Remark
        );
    END TRY
    BEGIN CATCH
        -- Log error or re-raise it
        DECLARE @ErrorMessage NVARCHAR(4000), @ErrorSeverity INT, @ErrorState INT;
        SELECT 
            @ErrorMessage = ERROR_MESSAGE(),
            @ErrorSeverity = ERROR_SEVERITY(),
            @ErrorState = ERROR_STATE();

        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END
GO