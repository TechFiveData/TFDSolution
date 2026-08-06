DROP PROCEDURE IF EXISTS [dbo].[m_getFormFields]

GO
/****** Object:  StoredProcedure [dbo].[m_getFormFields]    Script Date: 3/18/2025 10:00:11 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[m_getFormFields]
	-- Add the parameters for the stored procedure here
	@FormId as varchar(37)  = NULL,
	@SectionId as varchar(40)  = NULL,
	@TabId as varchar(40)  = NULL
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	DECLARE @guid uniqueidentifier 
	DECLARE @secid uniqueidentifier 
	DECLARE @tagbid uniqueidentifier 

	IF(@FormId IS NOT NULL AND @FormId <> '')
	BEGIN
		SET @guid = CONVERT(uniqueidentifier, @FormId)
	END
	IF(@SectionId IS NOT NULL AND @SectionId <> '')
	BEGIN
		SET @secid = CONVERT(uniqueidentifier, @SectionId)
	END
	IF(@TabId IS NOT NULL AND @TabId <> '')
	BEGIN
		SET @tagbid = CONVERT(uniqueidentifier, @TabId)
	END
	IF(@SectionId IS NULL OR @SectionId = '')
	BEGIN
		SELECT m_FormField.FieldId,m_FormField.FormId,m_FormField.FormTabId,m_FormField.FieldTypeId,m_FormField.IsActive,
		m_FormField.FieldName,m_FormField.FieldCaption,m_FormField.FieldLength
		,m_FormField.SortOrder,
		m_FormField.FormSectionId,m_FormField.IsRequired,m_FormField.CreatedOn,m_FormField.UpdatedOn,m_FormField.ParameterId
		,m_FormFieldTypes.FieldType
		,m_FormTab.TabName
		,m_FormSection.SectionName		
		FROM m_FormField WITH(NOLOCK) 
		INNER JOIN m_FormFieldTypes WITH(NOLOCK) ON m_FormFieldTypes.FieldTypeId = m_FormField.FieldTypeId
		INNER JOIN m_FormTab WITH(NOLOCK) ON m_FormTab.FormTabId = m_FormField.FormTabId
		INNER JOIN m_FormSection WITH(NOLOCK) ON m_FormField.FormSectionId = m_FormSection.FormSectionId
		WHERE m_FormField.FormId = @FormId
		order  by m_FormField.SortOrder
	END
	ELSE
	BEGIN
	
		SELECT m_FormField.FieldId,m_FormField.FormId,m_FormField.FormTabId,m_FormField.FieldTypeId,m_FormField.IsActive,
		m_FormField.FieldName,m_FormField.FieldCaption,m_FormField.FieldLength
		, m_FormField.SortOrder,
		m_FormField.FormSectionId,m_FormField.IsRequired,m_FormField.CreatedOn,m_FormField.UpdatedOn,m_FormField.ParameterId
		,m_FormFieldTypes.FieldType
		,m_FormTab.TabName
		,m_FormSection.SectionName
		FROM m_FormField WITH(NOLOCK) 
		INNER JOIN m_FormFieldTypes WITH(NOLOCK) ON m_FormFieldTypes.FieldTypeId = m_FormField.FieldTypeId
		INNER JOIN m_FormTab WITH(NOLOCK) ON m_FormTab.FormTabId = m_FormField.FormTabId
		INNER JOIN m_FormSection WITH(NOLOCK) ON m_FormField.FormSectionId = m_FormSection.FormSectionId
		WHERE m_FormField.FormId = @FormId and m_FormField.FormSectionId = @secid and m_FormField.FormTabId = @tagbid
		order  by m_FormField.SortOrder
	END
END
