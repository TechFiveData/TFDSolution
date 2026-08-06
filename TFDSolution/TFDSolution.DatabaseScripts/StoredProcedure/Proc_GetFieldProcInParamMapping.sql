DROP PROCEDURE IF EXISTS  dbo.Proc_GetFieldProcInParamMapping
GO
-- =============================================
-- Author:		Lavina Shaktawat
-- Create date: 04/05/2025
-- Description:	<To Get input param mapping for Form Field of SP source type >
-- =============================================
-- exec Proc_GetFieldProcInParamMapping @FieldId='3A607491-09BA-4EDC-8619-0989A6BFC554'
CREATE PROCEDURE dbo.Proc_GetFieldProcInParamMapping
    @FieldId UNIQUEIDENTIFIER 
AS
BEGIN
    SET NOCOUNT ON;
	
    BEGIN TRY
        SELECT         
            FieldId,
            ParamName,
            (CASE  WHEN MapFieldId IS NOT NULL THEN 
                    (SELECT FieldName 
                     FROM m_FormField WITH (NOLOCK) 
                     WHERE FieldId = MapFieldId)
                ELSE DefaultValue
            END )AS ParamMapValue
			,(CASE WHEN MapFieldId IS NOT NULL THEN convert(varchar, MapFieldId) ELSE DefaultValue END )	MapFieldId
			 
        FROM dbo.m_FieldProcInParamMapping
        WHERE FieldId = @FieldId;
    END TRY
    BEGIN CATCH
        DECLARE @ErrMsg NVARCHAR(4000), @ErrSeverity INT, @ErrState INT;
        SELECT 
            @ErrMsg = ERROR_MESSAGE(), 
            @ErrSeverity = ERROR_SEVERITY(), 
            @ErrState = ERROR_STATE();

        RAISERROR('Error in Get_FieldProcInParamMapping: %s', @ErrSeverity, @ErrState, @ErrMsg);
    END CATCH
END;