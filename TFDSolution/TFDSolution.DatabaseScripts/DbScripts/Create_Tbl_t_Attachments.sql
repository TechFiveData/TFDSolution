IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 't_Attachments' AND type = 'U')
BEGIN
    CREATE TABLE t_Attachments(
    AttachmentId UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY, -- GUID as the primary key    
    AttachedFileName VARCHAR(100) NOT NULL, -- Manual entry
    AttachedFilePath NVARCHAR(1000) NOT NULL, -- Manual entry
    AttachmentSize DECIMAL(18,2) NOT NULL, -- Selection (Stored as Integer)
	AttachmentType INT NOT NULL, -- Like Item,Bill etc
   	CreatedBy UNIQUEIDENTIFIER NOT NULL,
	CreatedOn DATETIME NOT NULL,
	UpdatedBy UNIQUEIDENTIFIER  NULL,
	UpdatedOn DATETIME  NULL
	)
END
GO