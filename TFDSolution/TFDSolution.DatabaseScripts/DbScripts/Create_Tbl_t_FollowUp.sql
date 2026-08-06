DROP TABLE IF EXISTS t_FollowUp
GO

CREATE TABLE t_FollowUp (
    FollowUpId INT IDENTITY(1,1) PRIMARY KEY,	
    FollowUpDate DATETIME NOT NULL, 
    Interaction NVARCHAR(MAX) NULL,
    Response NVARCHAR(MAX) NULL,
    CreatedBy UNIQUEIDENTIFIER NOT NULL,
	CreatedOn DATETIME NOT NULL,
	UpdatedBy UNIQUEIDENTIFIER  NULL,
	UpdatedOn DATETIME  NULL,
    Status INT NOT NULL,
    AssignedTo VARCHAR(50) NOT NULL,
    Form_URNNo NVARCHAR(50) NULL,
    ContactPerson_Id INT NULL,
    ContactPerson NVARCHAR(100) NULL,
    NextFollowUpID INT NULL,
    NextFollowUpDate DATETIME NULL,
    CoAgent INT NULL,
    EventPriority INT NOT NULL DEFAULT 1,
    FormId NVARCHAR(100) NOT NULL,
    Form_ParentId INT NULL
);
GO


INSERT [dbo].[t_FollowUp] ([FollowUpId], [FollowUpDate], [Interaction], [Response], [CreatedBy], [CreatedOn], [UpdatedBy], [UpdatedOn], [Status], [AssignedTo], [Form_URNNo], [ContactPerson_Id], [ContactPerson], [NextFollowUpID], [NextFollowUpDate], [CoAgent], [EventPriority], [FormId], [Form_ParentId]) VALUES (2, CAST(N'2025-07-23T10:40:28.950' AS DateTime), N'Test', N'Test', N'850f1eb9-d6ad-483e-941c-29efc4fcded2', CAST(N'2025-07-23T10:40:28.950' AS DateTime), NULL, NULL, 1, N'850F1EB9-D6AD-483E-941C-29EFC4FCDED2', N'DMX1001', 1, N'jorge', NULL, NULL, 1, 1, N'A184EA18-DA25-4C5D-8AA4-1082CA029CD0', 1)
GO