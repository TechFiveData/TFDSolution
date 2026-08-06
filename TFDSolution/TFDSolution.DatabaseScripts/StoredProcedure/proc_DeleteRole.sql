DROP PROCEDURE IF EXISTS proc_DeleteRole
GO
CREATE PROCEDURE proc_DeleteRole
    @RoleId INT
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @IsSuccess AS BIT = 0
    DECLARE @MessageText AS VARCHAR(500) = ''
	Begin Transaction;
    BEGIN TRY
	
        -- Optional: Check for existing permission mappings before delete
        IF EXISTS (SELECT 1 FROM m_RolePermission WHERE RoleID = @RoleId)
        BEGIN
           Delete From m_RolePermission WHERE RoleID = @RoleId
        END

		Update m_UserMast SET RoleId=NULL WHERE RoleId = @RoleId

        DELETE FROM m_RoleMast
        WHERE RoleId = @RoleId;
        -- Check if the delete was successful
        SET @IsSuccess = 1
        SET @MessageText = 'Record Deleted successfully.'
		COmmit;
        SELECT @IsSuccess as IsSuccess, @MessageText as Response, 'Delete' as Action
    END TRY
    BEGIN CATCH
		RollBack;
        SET @IsSuccess = 0
        DECLARE @ErrorMessage NVARCHAR(500)
        SET  @ErrorMessage = ERROR_MESSAGE()              
        SELECT @IsSuccess as IsSuccess, @ErrorMessage as Response, 'Error' as Action
       
    END CATCH
END;
