IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'm_UserDetails' AND type = 'U')
BEGIN
	CREATE TABLE m_UserDetails (
    UserId			UNIQUEIDENTIFIER ,
    Address1		VARCHAR(255) NOT NULL,
    Address2		VARCHAR(255) NULL,
    City			INT  NULL,  -- Selection
    StateId			INT  NULL,  -- Selection
    CountryId		INT  NULL,  -- Selection
	ZipCode			VARCHAR(10)  NULL,
    Pincode			VARCHAR(10)  NULL,
    ContactNo		VARCHAR(20) NULL,
    ContactPerson	VARCHAR(50) NULL,
    ContactEmail	VARCHAR(50) NULL
	);
END
GO
IF NOT EXISTS (
	SELECT 1
	FROM sys.foreign_keys 
	WHERE name = 'FK_m_UserDetails_m_usermast'
)
BEGIN
	ALTER TABLE m_UserDetails
	ADD CONSTRAINT FK_m_UserDetails_m_usermast
	FOREIGN KEY (UserId) REFERENCES m_usermast(UserId) ON DELETE CASCADE;
END

Alter Table m_UserDetails Add MobileNumber VARCHAR(15)