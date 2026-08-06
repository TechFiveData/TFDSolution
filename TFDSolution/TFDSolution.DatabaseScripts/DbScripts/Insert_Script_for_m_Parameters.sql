
/*----------------Insert Item Master Dropdown Entries ----------------------*/
----Item Type----
IF NOT EXISTS (SELECT 1 FROM m_Parameters WHERE EntryType = 'ItemType' AND [Description] = 'Purchase')
BEGIN
    INSERT INTO m_Parameters (EntryType, [Value], [Description],EntryTypeDesc,EntryCode  )
    VALUES ('ItemType', 1, 'Purchase', 'Entry for Item Type Dropdown', NULL );
END
GO
IF NOT EXISTS (SELECT 1 FROM m_Parameters WHERE EntryType = 'ItemType' AND [Description] = 'Sales')
BEGIN
    INSERT INTO m_Parameters (EntryType, [Value], [Description],EntryTypeDesc,EntryCode  )
    VALUES ('ItemType',2, 'Sales', 'Entry for Item Type Dropdown', NULL );
END
GO
----Item Group----
IF NOT EXISTS (SELECT 1 FROM m_Parameters WHERE EntryType = 'ItemGroup' AND [Description] = 'Finished Goods')
BEGIN
    INSERT INTO m_Parameters (EntryType, [Value], [Description],EntryTypeDesc,EntryCode  )
    VALUES ('ItemGroup',1, 'Finished Goods', 'Entry for Item Group Dropdown', NULL );
END
GO
-----Item UOM---
IF NOT EXISTS (SELECT 1 FROM m_Parameters WHERE EntryType = 'ItemUOM' AND [Description] = 'NOS')
BEGIN
    INSERT INTO m_Parameters (EntryType, [Value], [Description],EntryTypeDesc,EntryCode  )
    VALUES ('ItemUOM',1, 'NOS', 'Entry for Item UOM Dropdown', NULL );
END
GO
-----Item Category---
IF NOT EXISTS (SELECT 1 FROM m_Parameters WHERE EntryType = 'ItemCategory' AND [Description] = 'Item')
BEGIN
    INSERT INTO m_Parameters (EntryType, [Value], [Description],EntryTypeDesc,EntryCode  )
    VALUES ('ItemCategory',1,'Item', 'Entry for Item Category Dropdown', NULL );
END
GO
IF NOT EXISTS (SELECT 1 FROM m_Parameters WHERE EntryType = 'ItemCategory' AND [Description] = 'Service')
BEGIN
    INSERT INTO m_Parameters (EntryType, [Value], [Description],EntryTypeDesc,EntryCode  )
    VALUES ('ItemCategory',2,'Service', 'Entry for Item Category Dropdown', NULL );
END
GO

-----Packing Type---
IF NOT EXISTS (SELECT 1 FROM m_Parameters WHERE EntryType = 'PackingType' AND [Description] = 'None')
BEGIN
    INSERT INTO m_Parameters (EntryType, [Value], [Description],EntryTypeDesc,EntryCode  )
    VALUES ('PackingType',1,'None', 'Entry for Item Packing Type Dropdown', NULL );
END
GO
IF NOT EXISTS (SELECT 1 FROM m_Parameters WHERE EntryType = 'PackingType' AND [Description] = 'BatchSerialNo')
BEGIN
    INSERT INTO m_Parameters (EntryType, [Value], [Description],EntryTypeDesc,EntryCode  )
    VALUES ('PackingType',2,'BatchSerialNo', 'Entry for Item Packing Type Dropdown', NULL );
END
GO

-----Valuation Method---
IF NOT EXISTS (SELECT 1 FROM m_Parameters WHERE EntryType = 'ValuationMethod' AND [Description] = 'FIFO')
BEGIN
    INSERT INTO m_Parameters (EntryType, [Value], [Description],EntryTypeDesc,EntryCode  )
    VALUES ('ValuationMethod',1,'FIFO', 'Entry for Item Valuation Method Dropdown', NULL );
END
GO
IF NOT EXISTS (SELECT 1 FROM m_Parameters WHERE EntryType = 'ValuationMethod' AND [Description] = 'Moving Average')
BEGIN
    INSERT INTO m_Parameters (EntryType, [Value], [Description],EntryTypeDesc,EntryCode  )
    VALUES ('ValuationMethod',2,'Moving Average', 'Entry for Item Valuation Method Dropdown', NULL );
END
GO
IF NOT EXISTS (SELECT 1 FROM m_Parameters WHERE EntryType = 'ValuationMethod' AND [Description] = 'Batch')
BEGIN
    INSERT INTO m_Parameters (EntryType, [Value], [Description],EntryTypeDesc,EntryCode  )
    VALUES ('ValuationMethod',3,'Batch', 'Entry for Item Valuation Method Dropdown', NULL );
END
GO
/*----------------Insert Party Master Dropdown Entries ----------------------*/
------Party Type----
IF NOT EXISTS (SELECT 1 FROM m_Parameters WHERE EntryType = 'PartyType' AND [Description] = 'Customer')
BEGIN
    INSERT INTO m_Parameters (EntryType, [Value], [Description],EntryTypeDesc,EntryCode  )
    VALUES ('PartyType',1,'Customer', 'Entry for Party Type Dropdown', NULL );
END
GO
IF NOT EXISTS (SELECT 1 FROM m_Parameters WHERE EntryType = 'PartyType' AND [Description] = 'Supplier')
BEGIN
    INSERT INTO m_Parameters (EntryType, [Value], [Description],EntryTypeDesc,EntryCode  )
    VALUES ('PartyType',2,'Supplier', 'Entry for Party Type Dropdown', NULL );
END
GO

------Party Group----
IF NOT EXISTS (SELECT 1 FROM m_Parameters WHERE EntryType = 'PartyGroup' AND [Description] = 'Sundary Creditors')
BEGIN
    INSERT INTO m_Parameters (EntryType, [Value], [Description],EntryTypeDesc,EntryCode  )
    VALUES ('PartyGroup',1,'Sundary Creditors', 'Entry for Party Group Dropdown', NULL );
END
GO

------ GST TYPE----
IF NOT EXISTS (SELECT 1 FROM m_Parameters WHERE EntryType = 'GSTType' AND [Description] = 'Below')
BEGIN
    INSERT INTO m_Parameters (EntryType, [Value], [Description],EntryTypeDesc,EntryCode  )
    VALUES ('GSTType',1,'Below', 'Entry for GST Type Dropdown', NULL );
END
GO

------ Currency----
IF NOT EXISTS (SELECT 1 FROM m_Parameters WHERE EntryType = 'Currency' AND [Description] = 'INR')
BEGIN
    INSERT INTO m_Parameters (EntryType, [Value], [Description],EntryTypeDesc,EntryCode  )
    VALUES ('Currency',1,'INR', 'Entry for Currency Dropdown', NULL );
END
GO
IF NOT EXISTS (SELECT 1 FROM m_Parameters WHERE EntryType = 'Currency' AND [Description] = 'USD')
BEGIN
    INSERT INTO m_Parameters (EntryType, [Value], [Description],EntryTypeDesc,EntryCode  )
    VALUES ('Currency',2,'USD', 'Entry for Currency Dropdown', NULL );
END
GO