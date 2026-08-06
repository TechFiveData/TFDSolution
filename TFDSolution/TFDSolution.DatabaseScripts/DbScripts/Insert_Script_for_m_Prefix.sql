----Item Code
IF NOT EXISTS (SELECT 1 FROM m_Prefix WHERE TableName = 'm_ItemMast' AND FieldName = 'ItemCode')
BEGIN
    INSERT INTO m_Prefix (TableName,	FieldName,	Prefix)
	VALUES('m_ItemMast','ItemCode','I' )
END
GO
----For Company Code
IF NOT EXISTS (SELECT 1 FROM m_Prefix WHERE TableName = 'm_CompanyMast' AND FieldName = 'CompanyCode')
BEGIN
    INSERT INTO m_Prefix (TableName,	FieldName,	Prefix)
	VALUES('m_CompanyMast','CompanyCode','C' )
END
GO
---For Party Code
IF NOT EXISTS (SELECT 1 FROM m_Prefix WHERE TableName = 'm_PartyMast' AND FieldName = 'PartyCode')
BEGIN
    INSERT INTO m_Prefix (TableName,	FieldName,	Prefix)
	VALUES('m_PartyMast','PartyCode','P' )
END
GO

