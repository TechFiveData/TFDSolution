DROP PROCEDURE IF EXISTS  dbo.Proc_SaveFieldProcInParamMapping
GO
-- =============================================
-- Author:		Lavina Shaktawat
-- Create date: 04/05/2025
-- Description:	<To Save input param mapping for Form Field of SP source type >
-- =============================================
CREATE PROCEDURE dbo.Proc_SaveFieldProcInParamMapping
   @FieldParams dbo.UDT_FieldProcInParamMapping READONLY
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        -- 1. Update existing records based on FieldParamId
        UPDATE T
        SET 
            T.FieldId = S.FieldId,
            T.ParamName = S.ParamName,
            T.MapFieldId = S.MapFieldId,
            T.DefaultValue = S.DefaultValue,
            T.UpdatedOn = GETDATE()
        FROM dbo.m_FieldProcInParamMapping T
        INNER JOIN @FieldParams S
            ON T.FieldParamId = S.FieldParamId
        WHERE S.FieldParamId IS NOT NULL;

        -- 2. Insert new records only if ParamName + FieldId does not exist already
        INSERT INTO dbo.m_FieldProcInParamMapping (
            FieldId, ParamName, MapFieldId, DefaultValue, CreatedOn
        )
        SELECT 
            S.FieldId,
            S.ParamName,
            S.MapFieldId,
            S.DefaultValue,
            GETDATE()
        FROM @FieldParams S
        WHERE S.FieldParamId IS NULL
          AND NOT EXISTS (
              SELECT 1 
              FROM dbo.m_FieldProcInParamMapping M
              WHERE M.FieldId = S.FieldId AND M.ParamName = S.ParamName
          );
    END TRY
    BEGIN CATCH
        DECLARE @ErrMsg NVARCHAR(4000), @ErrSeverity INT, @ErrState INT;
        SELECT 
            @ErrMsg = ERROR_MESSAGE(), 
            @ErrSeverity = ERROR_SEVERITY(), 
            @ErrState = ERROR_STATE();

        RAISERROR('Error in Proc_SaveFieldProcInParamMapping: %s', @ErrSeverity, @ErrState, @ErrMsg);
    END CATCH
END;