-- =============================================
-- Author:		Lavina Shaktawat
-- Create date: 05/04/2025 
-- Description:	<Set Active status of User>
-- =============================================

CREATE PROCEDURE proc_SetUserStatus
    @UserId UNIQUEIDENTIFIER,
    @Status BIT
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM m_usermast WHERE UserId = @UserId)
    BEGIN
        UPDATE m_UserMast
        SET IsActive = @Status
        WHERE UserId = @UserId;
    END
    ELSE
    BEGIN
        RAISERROR('User Not Exists', 16, 1);
    END
END
GO