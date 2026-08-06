DROP PROCEDURE IF EXISTS [dbo].[proc_getCompany]
GO
/****** Object:  StoredProcedure [dbo].[proc_getCompanyList]    Script Date: 3/2/2025 6:26:05 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Amit Gorvadiya
-- Create date: 02/03/2025
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[proc_getCompany]
	-- Add the parameters for the stored procedure here
	@UId AS VARCHAR(50)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	DECLARE @companyId AS UNIQUEIDENTIFIER;

	SET @companyId = CONVERT(UNIQUEIDENTIFIER, @UId)

    -- Insert statements for procedure here
	SELECT CompanyId,CompanyCode,CompanyName,Phone,Mobile
      ,Email,WebStite,Fax,RegisterAddress1
      ,RegisterAddress2,RegiterCityId
      ,RegiterStateId,RegisterCountryId
      ,RegisterPincode,FactoryAddress1
      ,FactoryAddress2,FactoryCityId
      ,FactoryStateId,FactoryCountryId,FactoryPincode
      ,GSTNo,PANNo,CINNo,ECCNo,IECCode,m_CompanyMast.CreatedOn,m_CompanyMast.UpdatedOn,
	  (m_UserMast.FirstName +' ' + m_UserMast.LastName) as CreatedBy,
	  (ISNULL(u.FirstName,'') +' ' + ISNULL(u.LastName,'')) as UpdatedBy
	FROM [dbo].m_CompanyMast WITH(NOLOCK)
	INNER JOIN m_UserMast WITH(NOLOCK) ON m_UserMast.UserId = m_CompanyMast.CreatedBy
	LEFT JOIN m_UserMast u WITH(NOLOCK) ON u.UserId = m_CompanyMast.UpdatedBy
	WHERE m_CompanyMast.CompanyId =  @companyId

END
