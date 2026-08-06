Drop type if exists UDT_SortOrderData
GO

CREATE TYPE dbo.UDT_SortOrderData AS TABLE
(
    FormId UNIQUEIDENTIFIER,
    SortOrder INT	
);