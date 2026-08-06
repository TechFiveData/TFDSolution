DROP TYPE IF EXISTS  [dbo].[UDT_RolePermissionType]

CREATE TYPE [dbo].[UDT_RolePermissionType] AS TABLE
(
    RolePermissionID INT,
    RoleID INT,
    FormId UNIQUEIDENTIFIER,
    FullAccess BIT,
    CanAdd BIT,
    CanEdit BIT,
    CanDelete BIT,
    CanPrint BIT,
    CanApprove BIT
);