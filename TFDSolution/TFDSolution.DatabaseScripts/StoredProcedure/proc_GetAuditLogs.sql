DROP PROCEDURE IF EXISTS proc_GetAuditLogs
GO
-- =============================================
-- Author:		Lavina Shaktawat
-- Create date: 06/04/2025 
-- Description:	<Get Audit log details >
-- =============================================
-- exec proc_GetAuditLogs @StartDate='2024-04-01', @EndDate ='2024-04-06' */
CREATE PROCEDURE proc_GetAuditLogs
    @StartDate DATETIME ,
    @EndDate DATETIME,
	@LogId INT = NULL,
    @CompanyId VARCHAR(50) = NULL,
    @UserId VARCHAR(50)  = NULL
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        SELECT 
            AuditLogId ,
            LogDateTime,
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
        FROM AuditLogs
        WHERE 
		CAST(LogDateTime AS DATE) BETWEEN CAST(@StartDate AS DATE) AND CAST(@EndDate AS DATE)
        AND AuditLogId = (CASE WHEN ISNULL(@LogId,0)=0 THEN AuditLogId Else @LogId END)
		AND CompanyId = (CASE WHEN ISNULL(@CompanyId,'')='' THEN CompanyId Else @CompanyId END)
		AND UserId = (CASE WHEN ISNULL(@UserId,'')='' THEN UserId Else @UserId END)
        ORDER BY LogDateTime DESC;
    END TRY
    BEGIN CATCH
        -- Return the error
        DECLARE @ErrorMessage NVARCHAR(4000), @ErrorSeverity INT, @ErrorState INT;
        SELECT 
            @ErrorMessage = ERROR_MESSAGE(),
            @ErrorSeverity = ERROR_SEVERITY(),
            @ErrorState = ERROR_STATE();

        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END
GO
