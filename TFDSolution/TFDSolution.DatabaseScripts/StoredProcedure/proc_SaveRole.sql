DROP PROCEDURE IF EXISTS proc_SaveRole
GO
CREATE PROCEDURE proc_SaveRole
    @RoleId INT = NULL,              -- If NULL or 0, INSERT; otherwise, UPDATE
    @RoleName NVARCHAR(100),
    @Description NVARCHAR(255),
    @CreatedOn DATETIME = NULL  ,     -- Optional for INSERT; ignored in UPDATE
	@Status BIT
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        IF ISNULL(@RoleId, 0) = 0 and NOT EXISTS(SELECT 1 FROM m_RoleMast WITH (NOLOCK) WHERE RoleName =@RoleName)
        BEGIN
            -- Insert new role
            INSERT INTO m_RoleMast (RoleName, Description, CreatedOn,Staus  )
            VALUES (@RoleName, @Description, ISNULL(@CreatedOn, GETDATE()),@Status);

            -- Optionally return the new RoleId
            SELECT SCOPE_IDENTITY() AS NewRoleId;
        END
        ELSE
        BEGIN
            -- Update existing role
            UPDATE m_RoleMast
            SET 
                RoleName = @RoleName,
                Description = @Description,
				Staus = ISNULL(@Status,Staus)
            WHERE RoleId = @RoleId;

            SELECT @RoleId AS UpdatedRoleId;
        END
    END TRY
    BEGIN CATCH
        DECLARE @ErrorMessage NVARCHAR(4000),
                @ErrorSeverity INT,
                @ErrorState INT;

        SELECT 
            @ErrorMessage = ERROR_MESSAGE(),
            @ErrorSeverity = ERROR_SEVERITY(),
            @ErrorState = ERROR_STATE();

        RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
    END CATCH
END;
