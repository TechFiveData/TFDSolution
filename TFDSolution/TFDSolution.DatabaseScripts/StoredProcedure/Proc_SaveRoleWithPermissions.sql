DROP PROCEDURE IF EXISTS  dbo.Proc_SaveRoleWithPermissions
GO
-- =============================================
-- Author:		Lavina Shaktawat
-- Create date: 24/05/2025
-- Description:	<To Save Role permissions>
-- =============================================


CREATE PROCEDURE [dbo].[Proc_SaveRoleWithPermissions]
    @RoleId INT OUTPUT,
    @RoleName NVARCHAR(50),
    @Description NVARCHAR(1000),
    @Status BIT,
    @UserId UNIQUEIDENTIFIER,
    @PermissionTable dbo.UDT_RolePermissionType READONLY
AS
BEGIN
    SET NOCOUNT ON;
	DECLARE @MessageText AS VARCHAR(500) = ''
	DECLARE @IsSuccess AS BIT = 0
	DECLARE @Action AS VARCHAR(10) = ''
    BEGIN TRY

        BEGIN TRANSACTION;

        -- Insert or Update Role
        IF EXISTS (SELECT 1 FROM m_RoleMast WHERE RoleId = @RoleId)
        BEGIN
            UPDATE m_RoleMast
            SET RoleName = @RoleName,
                Description = @Description,
                Status = @Status
            WHERE RoleId = @RoleId;
        END
        ELSE
        BEGIN
            INSERT INTO m_RoleMast (RoleName, Description, CreatedOn, Status)
            VALUES (@RoleName, @Description, GETDATE(), ISNULL(@Status,1));

            SET @RoleId = SCOPE_IDENTITY();
        END

        -----------------------------
        -- Insert or Update Permissions
        -----------------------------
        MERGE m_RolePermission AS target
        USING @PermissionTable AS source
        ON target.RoleID = @RoleId AND target.FormId = source.FormId

        WHEN MATCHED THEN
            UPDATE SET
                target.FullAccess = source.FullAccess,
                target.CanAdd = source.CanAdd,
                target.CanEdit = source.CanEdit,
                target.CanDelete = source.CanDelete,
                target.CanPrint = source.CanPrint,
                target.CanApprove = source.CanApprove,
                target.UpdatedBy = @UserId,
                target.UpdatedOn = GETDATE()

        WHEN NOT MATCHED BY TARGET THEN
            INSERT (
                RoleID, FormId, 
                FullAccess, CanAdd, CanEdit, CanDelete, CanPrint, CanApprove,
                CreatedBy, CreatedOn
            )
            VALUES (
                @RoleId, source.FormId, 
                source.FullAccess, source.CanAdd, source.CanEdit, source.CanDelete, source.CanPrint, source.CanApprove,
                @UserId, GETDATE()
            );

        -------------------------------------
        -- Delete obsolete permissions
        -------------------------------------
        DELETE FROM m_RolePermission
        WHERE RoleID = @RoleId
          AND FormId NOT IN (SELECT FormId FROM @PermissionTable);
        COMMIT;

		SET @IsSuccess = 1
		SET @MessageText = 'Record save successfully.'
		SET @Action = 'Save'
    END TRY
    BEGIN CATCH
        ROLLBACK;
		SET @IsSuccess = 0
		SET @MessageText = ERROR_MESSAGE();
		SET @Action  = 'Error' 
    END CATCH
	SELECT @IsSuccess as IsSuccess, @MessageText as Response, 'Error' as Action, NULL as PrimaryId 
END