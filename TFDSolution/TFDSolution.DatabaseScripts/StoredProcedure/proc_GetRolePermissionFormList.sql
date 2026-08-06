DROP PROCEDURE IF EXISTS proc_GetRolePermissionFormList
GO
-- proc_GetRolePermissionFormList 4
CREATE PROCEDURE proc_GetRolePermissionFormList
    @RoleId INT
AS
BEGIN
    SET NOCOUNT ON;
	BEGIN TRY
			SELECT RoleId,RoleName,Description,CreatedOn,status 
			FROM m_RoleMast 
			WHERE RoleId = @RoleId  

			SELECT F.FormId, F.FormName, F.ParentFormId, PF.FormName AS ParentFormName, 
			RP.RolepermissionId, RP.RoleId,			
			RP.FullAccess, RP.CanAdd, RP.CanEdit,  RP.CanDelete, RP.CanPrint, RP.CanApprove, 
			RP.Createdby, RP.CreatedOn, RP.UpdatedBy, RP.UpdatedOn
			FROM 
	


	END TRY
    BEGIN CATCH
        -- Return error information
        DECLARE @ErrorMessage NVARCHAR(4000);
        DECLARE @ErrorSeverity INT;
        DECLARE @ErrorState INT;

        SELECT 
            @ErrorMessage = ERROR_MESSAGE(),
            @ErrorSeverity = ERROR_SEVERITY(),
            @ErrorState = ERROR_STATE();

        RAISERROR (@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END;