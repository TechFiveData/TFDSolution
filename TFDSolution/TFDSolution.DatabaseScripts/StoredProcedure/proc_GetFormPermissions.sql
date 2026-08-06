
/****** Object:  StoredProcedure [dbo].[proc_GetRolePermissionFormList]    Script Date: 4/6/2025 10:07:46 pm ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/* =============================================  
 Author:  Lavina Shaktawat  
 Create date:   06/JUN/2025  
 Description: To fetch forms permisssion based on Form ID and roleid  

 EXEC proc_GetFormPermissions 0, 'FB255902-133D-443B-9D94-07897C26D599'
 =============================================*/  
CREATE OR ALTER PROCEDURE [dbo].[proc_GetFormPermissions]
    @RoleId INT,
	@FormId VARCHAR(50) 
AS
BEGIN
    SET NOCOUNT ON;
	BEGIN TRY
		SELECT   
        F.FormId,
		F.FormName,
		F.ParentFormId,
		RP.RolePermissionID,
        CAST(ISNULL(RP.FullAccess, 0) AS bit) AS FullAccess,
		CAST(ISNULL(RP.CanAdd, 0) AS bit) AS CanAdd,
		CAST(ISNULL(RP.CanEdit, 0) AS bit) AS CanEdit,
		CAST(ISNULL(RP.CanDelete, 0) AS bit) AS CanDelete,
		CAST(ISNULL(RP.CanPrint, 0) AS bit) AS CanPrint,
		CAST(ISNULL(RP.CanApprove, 0) AS bit) AS CanApprove,
		CAST(CASE 
			WHEN ISNULL(RP.FullAccess, 0) = 0 
			 AND ISNULL(RP.CanAdd, 0) = 0 
			 AND ISNULL(RP.CanEdit, 0) = 0 
			 AND ISNULL(RP.CanDelete, 0) = 0 
			 AND ISNULL(RP.CanPrint, 0) = 0 
			 AND ISNULL(RP.CanApprove, 0) = 0 
			THEN 1 ELSE 0 
		END AS bit) AS NoAccess
		FROM m_FormMast F WITH(NOLOCK)     
		LEFT JOIN  m_RolePermission RP WITH(NOLOCK)  ON F.FormId = RP.FormID  AND RP.RoleID = @RoleId   
		WHERE F.FormId = @FormId;  

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