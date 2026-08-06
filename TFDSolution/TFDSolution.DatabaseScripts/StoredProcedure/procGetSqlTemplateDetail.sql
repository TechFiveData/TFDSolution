DROP PROCEDURE IF EXISTS [dbo].[procGetSqlTemplateDetail]  
GO

-- =============================================
-- Author:		Lavina Shaktawat
-- Create date: 29/03/2025 
-- Description:	<Get Field List of Selected Sql Template>
-- =============================================
--exec [procGetSqlTemplateDetail] 1
CREATE PROCEDURE [dbo].[procGetSqlTemplateDetail]   
 @TemplateId INT
AS
BEGIN
	SELECT TemplateDetailId, TemplateId, FieldName ,FieldDataType
	FieldLength,AllowNullable,IdentityType
	FROM SqlTemplateDetails 
	WHERE TemplateId = CASE WHEN @TemplateId =0 THEN TemplateId ELSE @TemplateId END
END
GO