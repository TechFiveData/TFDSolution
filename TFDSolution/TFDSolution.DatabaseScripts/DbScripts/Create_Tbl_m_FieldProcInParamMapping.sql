CREATE TABLE dbo.m_FieldProcInParamMapping(
   FieldParamId INT IDENTITY(1,1) PRIMARY KEY,
   FieldId uniqueidentifier,
   ParamName NVARCHAR(100) NOT NULL,
   MapFieldId uniqueIdentifier NULL,
   DefaultValue NVARCHAR(200) NULL,
   CreatedOn DATETIME NOT NULL DEFAULT GETDATE(),
   UpdatedOn DATETIME NULL 
);

