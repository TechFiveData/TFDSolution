CREATE TABLE SqlTemplate(
    TemplateId INT IDENTITY(1,1) PRIMARY KEY,
    TemplateName VARCHAR(50) NOT NULL
);

--DROP TABLE SqlTemplateDetails
CREATE TABLE SqlTemplateDetails (
    TemplateDetailId INT IDENTITY(1,1) PRIMARY KEY,
    TemplateId INT NOT NULL,
    FieldName VARCHAR(255) NOT NULL,
    FieldLength INT  NULL,
    FieldDataType VARCHAR(50) NOT NULL,
    AllowNullable BIT NOT NULL Default(1),
	IdentityType BIT NULL
    FOREIGN KEY (TemplateId) REFERENCES SqlTemplate(TemplateId)
);

/*------------ Insert  in  sqlTemplate -------------*/
INSERT INTO sqlTemplate (TemplateName) 
VALUES	('Default Template'), 
		('Tranaction Template'), 
		('Master Template'),
		('Parameter Template');

/*------------ Insert  in  SqlTemplateDetails -------------*/
INSERT INTO SqlTemplateDetails
(TemplateId, FieldName, FieldDataType, FieldLength, AllowNullable,IdentityType) 
VALUES ( 1,'Id','INT', NULL,0 ,1 ),
( 1,'UserId','UNIQUEIDENTIFIER',NULL,0 ,0),
( 1,'CompanyId','UNIQUEIDENTIFIER',NULL,1 ,0),
( 1,'CreatedBy','UNIQUEIDENTIFIER',NULL,0 ,0),
( 1,'CreatedOn','DATETIME',NULL,0 ,0),
( 1,'UpdatedBy','UNIQUEIDENTIFIER',NULL,1 ,0),
( 1,'UpdatedOn','DATETIME',NULL,1 ,0)

INSERT INTO SqlTemplateDetails
(TemplateId, FieldName, FieldDataType, FieldLength, AllowNullable,IdentityType) 
VALUES ( 2,'Id','INT', NULL,0 ,1 ),
( 2,'UserId','UNIQUEIDENTIFIER',NULL,0 ,0),
( 2,'CreatedBy','UNIQUEIDENTIFIER',NULL,0 ,0),
( 2,'CreatedOn','DATETIME',NULL,0 ,0),
( 2,'UpdatedBy','UNIQUEIDENTIFIER',NULL,1 ,0),
( 2,'UpdatedOn','DATETIME',NULL,1 ,0)

INSERT INTO SqlTemplateDetails
(TemplateId, FieldName, FieldDataType, FieldLength, AllowNullable,IdentityType) 
VALUES ( 3,'Id','INT', NULL,0 ,1 ),
( 3,'UserId','UNIQUEIDENTIFIER',NULL,0 ,0),
( 3,'CreatedBy','UNIQUEIDENTIFIER',NULL,0 ,0),
( 3,'CreatedOn','DATETIME',NULL,0 ,0)

INSERT INTO SqlTemplateDetails
(TemplateId, FieldName, FieldDataType, FieldLength, AllowNullable,IdentityType) 
VALUES ( 4,'Id','INT', NULL,0 ,1 ),
( 4,'UserId','UNIQUEIDENTIFIER',NULL,0 ,0)