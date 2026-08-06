DROP PROCEDURE IF EXISTS [dbo].[proc_getParty]
GO
-- =============================================
-- Author:		Lavina Shaktawat
-- Create date: 08/03/2025
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[proc_getParty]
	@UId AS VARCHAR(50) 
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
            PartyId, PartyCode, PartyType, PartyName, AlternateName, PartyGroup, Currency, 
    Telephone1, Telephone2, Mobile, Email, WebSite, Fax, ContactPerson, Industry, PartyStatus, 
    SalesPerson, Remarks, 
    BillToName, BillToAddress1, BillToAddress2, BillToCity, BillToState, 
    BillToCountry, BillToPincode, BillToContactNo, BillToContactPerson, BillToEmail, 
    ShipToName, ShipToAddress1, ShipToAddress2, ShipToCity, ShipToState, 
    ShipToCountry, ShipToPincode, ShipToContactNo, ShipToContactPerson, ShipToEmail, 
    GSTNo, GSTType, PANNo ,
	m_PartyMast.CreatedOn,m_PartyMast.UpdatedOn,
	(m_UserMast.FirstName +' ' + m_UserMast.LastName) as CreatedBy,
	(ISNULL(u.FirstName,'') +' ' + ISNULL(u.LastName,'')) as UpdatedBy
	FROM [dbo].m_PartyMast WITH(NOLOCK)
	INNER JOIN m_UserMast WITH(NOLOCK) ON m_UserMast.UserId = m_PartyMast.CreatedBy
	LEFT JOIN m_UserMast u WITH(NOLOCK) ON u.UserId = m_PartyMast.UpdatedBy
	WHERE m_PartyMast.PartyId =  @UId
END
GO
