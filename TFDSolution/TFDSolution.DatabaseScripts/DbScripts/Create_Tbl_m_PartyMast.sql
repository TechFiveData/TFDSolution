IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'm_PartyMast' AND type = 'U')
BEGIN
	CREATE TABLE m_PartyMast (
    PartyId UNIQUEIDENTIFIER PRIMARY KEY ,
    PartyCode VARCHAR(20) UNIQUE NOT NULL,
    PartyType INT NOT NULL,  -- Selection
    PartyName VARCHAR(50) NOT NULL,
    AlternateName VARCHAR(50) NULL,
    PartyGroup INT NOT NULL,  -- Selection
    Currency INT NOT NULL,  -- Selection
    Telephone1 VARCHAR(20) NULL,
    Telephone2 VARCHAR(20) NULL,
    Mobile VARCHAR(20) NOT NULL,
    Email VARCHAR(50) NOT NULL,
    WebSite VARCHAR(255) NULL,
    Fax VARCHAR(20) NULL,
    ContactPerson VARCHAR(50) NULL,
    Industry INT NULL,  -- Selection
    PartyStatus INT NOT NULL,  -- Check Box (1 for Active, 0 for Inactive)
    SalesPerson INT NULL,  -- Selection
    Remarks VARCHAR(500) NULL,

    BillToName VARCHAR(50) NOT NULL,
    BillToAddress1 VARCHAR(255) NOT NULL,
    BillToAddress2 VARCHAR(255) NULL,
    BillToCity INT  NULL,  -- Selection
    BillToState INT  NULL,  -- Selection
    BillToCountry INT  NULL,  -- Selection
    BillToPincode VARCHAR(10)  NULL,
    BillToContactNo VARCHAR(20) NULL,
    BillToContactPerson VARCHAR(50) NULL,
    BillToEmail VARCHAR(50) NULL,

    ShipToName VARCHAR(50)  NULL,
    ShipToAddress1 VARCHAR(255)  NULL,
    ShipToAddress2 VARCHAR(255) NULL,
    ShipToCity INT  NULL,  -- Selection
    ShipToState INT  NULL,
    ShipToCountry INT  NULL,
    ShipToPincode VARCHAR(10)  NULL,
    ShipToContactNo VARCHAR(20) NULL,
    ShipToContactPerson VARCHAR(50) NULL,
    ShipToEmail VARCHAR(50) NULL,

    GSTNo VARCHAR(20) NULL,
    GSTType INT NULL,  -- Selection
    PANNo VARCHAR(10) NULL,

    CreatedBy UNIQUEIDENTIFIER NOT NULL,
	CreatedOn DATETIME NOT NULL,
	UpdatedBy UNIQUEIDENTIFIER  NULL,
	UpdatedOn DATETIME  NULL
	);
END
GO


