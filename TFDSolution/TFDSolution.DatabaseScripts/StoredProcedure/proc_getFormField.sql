DROP PROCEDURE IF EXISTS [dbo].[proc_getFormField]
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Amit Gorvadiya
-- Create date: 20-03-2025
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE proc_getFormField
	-- Add the parameters for the stored procedure here
	@FieldId AS VARCHAR(50)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here
	SELECT CONVERT(varchar(36), m_FormField.FieldId) AS FieldId
		,CONVERT(varchar(36), m_FormField.FormId) AS FormId
		,CONVERT(varchar(36), m_FormField.FormTabId) AS FormTabId
		,CONVERT(varchar(36), m_FormField.FieldTypeId) AS FieldTypeId
		,CONVERT(varchar(36), m_FormField.FormSectionId) AS FormSectionId
		,m_FormField.IsActive,m_FormField.FieldSize
		,m_FormField.FieldName,m_FormField.FieldCaption,m_FormField.FieldLength,
		m_FormField.IsRequired,m_FormField.CreatedOn,m_FormField.UpdatedOn
		,m_FormField.ParameterId
		,m_FormField.IsDisable
		,m_FormField.SortOrder
		,m_FormField.DDLSourceName,m_FormField.DDLSourceType,m_FormField.DDLTextField,m_FormField.DDLValueField,m_FormField.IsReadOnly
		,m_FormField.IsVisible,m_FormField.IsVisibleInList,m_FormField.FieldPlaceHolder,m_FormField.FieldHelpText			
		,m_FormFieldTypes.FieldType
		,m_FormFieldTypes.FieldTypeClass

		,m_FormTab.TabName
		,m_FormSection.SectionName
		,m_FormMast.SQLTableName
		
		FROM m_FormField WITH(NOLOCK) 
		INNER JOIN m_FormFieldTypes WITH(NOLOCK) ON m_FormFieldTypes.FieldTypeId = m_FormField.FieldTypeId
		INNER JOIN m_FormTab WITH(NOLOCK) ON m_FormTab.FormTabId = m_FormField.FormTabId
		INNER JOIN m_FormSection WITH(NOLOCK) ON m_FormField.FormSectionId = m_FormSection.FormSectionId
		INNER JOIN m_FormMast WITH(NOLOCK) ON m_FormField.FormId = m_FormMast.FormId
		WHERE m_FormField.FieldId = @FieldId
		Order by m_FormField.SortOrder
END

GO
