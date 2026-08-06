DROP PROCEDURE IF EXISTS [dbo].[proc_GetFormPendingList]
GO
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Lavina Shaktawat
-- Create date: 09/04/2025
-- Description:	<Get Details based on FormPendingId >
-- =============================================
CREATE PROCEDURE [dbo].[proc_GetFormPendingList]
    @FormPendingId INT =0
AS
BEGIN
    SET NOCOUNT ON;
	
    BEGIN TRY
            -- Return a specific record if FormPendingId supply
            SELECT 
                FP.FormPendingId,
                FP.FormId,
				FM.FormName,
				FM.ParentFormId,
				FMP.FormName As ParentFormName,
                FP.FormTabName,
                FP.SourceName,
                FP.SourceRequest,
                FP.SortOrder,
                FP.CreatedOn,
                FP.CreatedBy
            FROM [dbo].[m_FormPending] FP WITH(NOLOCK)
			INNER JOIN m_FormMast FM  WITH(NOLOCK) ON FP.FormId = FM.FormId
			INNER JOIN m_FormMast FMP WITH(NOLOCK) ON FM.ParentFormId = FMP.FormId
            WHERE FormPendingId = (CASE WHEN ISNULL( @FormPendingId,0) = 0 THEN FormPendingId 
									ELSE @FormPendingId END);
        
    END TRY
    BEGIN CATCH
        DECLARE @ErrMsg NVARCHAR(4000), @ErrSeverity INT;
        SELECT @ErrMsg = ERROR_MESSAGE(), @ErrSeverity = ERROR_SEVERITY();
        RAISERROR(@ErrMsg, @ErrSeverity, 1);
    END CATCH
END;