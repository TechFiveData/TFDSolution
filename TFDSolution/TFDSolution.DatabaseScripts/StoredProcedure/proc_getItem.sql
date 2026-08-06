DROP PROCEDURE IF EXISTS [dbo].[proc_getItem]
GO
/** Object:  StoredProcedure [dbo].[proc_getCompanyList]    Script Date: 3/2/2025 6:26:05 PM **/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Lavina Shaktawat
-- Create date: 03/03/2025
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[proc_getItem]
	-- Add the parameters for the stored procedure here
	@UId AS VARCHAR(50)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	DECLARE @ItemId AS UNIQUEIDENTIFIER;

	SET @ItemId = CONVERT(UNIQUEIDENTIFIER, @UId)

  	SELECT ItemId, ItemCode, ItemDescription, AlternateDescription, 
		ItemGroup, ItemUOM, ItemCategory, ItemType, PackingType, 
		ItemStatus, PurchaseUOM, ItemPerPurchase, PurchaseTaxPercentage, 
		PurchaseHSNCode, SalesUOM, ItemPerSales, SalesTaxPercentage, 
		SalesHSNCode, ItemWeight, MinimumQty, MaximumQty, ReorderQty, 
		ValuationMethod,  (m_UserMast.FirstName +' ' + m_UserMast.LastName) as CreatedBy,m_ItemMast.CreatedOn,
	(ISNULL(u.FirstName,'') +' ' + ISNULL(u.LastName,'')) as UpdatedBy ,m_ItemMast.UpdatedOn
	FROM dbo.m_ItemMast WITH(NOLOCK)
	INNER JOIN m_UserMast WITH(NOLOCK) ON m_UserMast.UserId = m_ItemMast.CreatedBy
	LEFT JOIN m_UserMast u WITH(NOLOCK) ON u.UserId = m_ItemMast.UpdatedBy
	WHERE m_ItemMast.ItemId = @ItemId

END
GO