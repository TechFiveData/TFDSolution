
CREATE OR ALTER PROCEDURE proc_GetRoles
    @RoleId INT = NULL
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        SELECT 
            RoleId,
            RoleName,
            Description,
            CreatedOn,
            Status
        FROM m_RoleMast WITH (NOLOCK)
        WHERE (@RoleId IS NULL OR RoleId = @RoleId)
       -- AND Status =1;
    END TRY
    BEGIN CATCH
        DECLARE @ErrorMessage NVARCHAR(4000), 
                @ErrorSeverity INT, 
                @ErrorState INT;

        SELECT 
            @ErrorMessage = ERROR_MESSAGE(),
            @ErrorSeverity = ERROR_SEVERITY(),
            @ErrorState = ERROR_STATE();

        RAISERROR (@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END;