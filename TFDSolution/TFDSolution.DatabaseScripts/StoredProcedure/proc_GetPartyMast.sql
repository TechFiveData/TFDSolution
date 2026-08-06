CREATE PROCEDURE proc_GetPartyMast
(
    @PartyId UNIQUEIDENTIFIER = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        PartyId,
        PartyCode,
        PartyType,
        PartyName,
        AlternateName,
        PartyGroup,
        Currency,
        Telephone1,
        Telephone2,
        Mobile,
        Email,
        WebSite,
        Fax,
        ContactPerson,
        Industry,
        PartyStatus,
        SalesPerson,
        Remarks,

        BillToName,
        BillToAddress1,
        BillToAddress2,
        BillToCity,
        BillToState,
        BillToCountry,
        BillToPincode,
        BillToContactNo,
        BillToContactPerson,
        BillToEmail,

        ShipToName,
        ShipToAddress1,
        ShipToAddress2,
        ShipToCity,
        ShipToState,
        ShipToCountry,
        ShipToPincode,
        ShipToContactNo,
        ShipToContactPerson,
        ShipToEmail,

        GSTNo,
        GSTType,
        PANNo,
        CreatedBy,
        CreatedOn,
        UpdatedBy,
        UpdatedOn
    FROM m_PartyMast
    WHERE 
	PartyId = (CASE WHEN  ISNULL(@PartyId,'00000000-0000-0000-0000-000000000000') = '00000000-0000-0000-0000-000000000000'
				THEN PartyId ELSE @PartyId END)
END;