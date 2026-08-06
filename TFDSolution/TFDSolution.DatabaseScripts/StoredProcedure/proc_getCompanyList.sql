DROP PROCEDURE IF EXISTS [dbo].[proc_getCompanyList]
-- ================================================
-- Template generated from Template Explorer using:
-- Create Procedure (New Menu).SQL
--
-- Use the Specify Values for Template Parameters 
-- command (Ctrl-Shift-M) to fill in the parameter 
-- values below.
--
-- This block of comments will not be included in
-- the definition of the procedure.
-- ================================================
/****** Object:  StoredProcedure [dbo].[proc_getCompanyList]    Script Date: 3/7/2025 9:22:40 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Amit Gorvadiya
-- Create date: 02/03/2025
-- Description:	<Description,,>
-- =============================================
ALTER PROCEDURE [dbo].[proc_getCompanyList]
	-- Add the parameters for the stored procedure here
	
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

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
	ORDER BY CompanyCode DESC

END
