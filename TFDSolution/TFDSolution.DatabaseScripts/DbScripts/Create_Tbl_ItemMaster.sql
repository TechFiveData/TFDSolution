
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'm_ItemMast' AND type = 'U')
BEGIN
    CREATE TABLE m_ItemMast(
    ItemId UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY, -- GUID as the primary key
    ItemCode VARCHAR(20) NOT NULL UNIQUE,  -- Ensuring uniqueness for ItemCode
    ItemDescription VARCHAR(255) NOT NULL, -- Manual entry
    AlternateDescription VARCHAR(255) NULL, -- Manual entry
    ItemGroup INT NOT NULL, -- Selection (Stored as Integer)
    ItemUOM INT NOT NULL, -- Selection (Stored as Integer)
    ItemCategory INT NOT NULL, -- Selection (Stored as Integer)
    ItemType INT NOT NULL, -- Selection (Stored as Integer)
    PackingType INT NOT NULL, -- Selection (Stored as Integer)
    ItemStatus BIT NOT NULL, -- Check box (1 for Active, 0 for Inactive)
    PurchaseUOM INT  NULL, -- Selection (Stored as Integer)
    ItemPerPurchase INT  NULL, -- Manual entry, assuming integer value
    PurchaseTaxPercentage DECIMAL(5,2) NULL, -- Selection
    PurchaseHSNCode INT NULL, -- Selection (Stored as Integer)
    SalesUOM INT  NULL, -- Selection (Stored as Integer)
    ItemPerSales INT  NULL, -- Manual entry, assuming integer value
    SalesTaxPercentage DECIMAL(5,2) NULL, -- Selection
    SalesHSNCode INT NULL, -- Selection (Stored as Integer)
	ItemWeight DECIMAL(18,2) NULL,
	MinimumQty  DECIMAL(18,2) NULL,
	MaximumQty  DECIMAL(18,2) NULL,
	ReorderQty  DECIMAL(18,2) NULL,
	ValuationMethod INT,
	CreatedBy UNIQUEIDENTIFIER NOT NULL,
	CreatedOn DATETIME NOT NULL,
	UpdatedBy UNIQUEIDENTIFIER  NULL,
	UpdatedOn DATETIME  NULL
	)
END
GO







