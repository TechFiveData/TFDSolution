IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'm_ItemWarehouse' AND type = 'U')
BEGIN
    CREATE TABLE m_ItemWarehouse(
    WarehouseId UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY, -- GUID as the primary key
    LocationCode VARCHAR(20) NOT NULL UNIQUE,  -- Ensuring uniqueness for ItemCode
    LocationName VARCHAR(100) NOT NULL, -- Manual entry
    CurrentStock DECIMAL(18,2) NOT NULL, -- Manual entry
    StockValue DECIMAL(18,2) NOT NULL, -- Selection (Stored as Integer)
   	CreatedBy UNIQUEIDENTIFIER NOT NULL,
	CreatedOn DATETIME NOT NULL,
	UpdatedBy UNIQUEIDENTIFIER  NULL,
	UpdatedOn DATETIME  NULL
	)
END
GO