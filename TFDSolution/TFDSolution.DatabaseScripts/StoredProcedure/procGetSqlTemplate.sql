DROP PROCEDURE IF EXISTS [dbo].[procGetSqlTemplates]  
GO
-- =============================================
-- Author:		Lavina Shaktawat
-- Create date: 29/03/2025 
-- Description:	<Get List of Sql Templates>
-- =============================================
--exec [procGetSqlTemplates] 
CREATE PROCEDURE [dbo].[procGetSqlTemplates]   
AS
BEGIN
	SELECT TemplateId, TemplateName FROM SQLTemplate
END
GO