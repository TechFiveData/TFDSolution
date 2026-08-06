DROP PROCEDURE IF EXISTS [dbo].[proc_GetFormStructure]
GO

/* =============================================
 Author:		Lavina Shaktawat
 Create date:   17/03/2025
 Description:	<Description>
 =============================================*/
 -- EXEC  proc_GetFormStructure 'currency'
CREATE PROCEDURE proc_GetFormStructure
    @FormName VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

	DECLARE @FormId Uniqueidentifier

	SELECT @FormId = FormId FROM m_FormMast WHERE FormName = @FormName

    -- Get form details
    SELECT 
        m_FormMast.FormId, 
        m_FormMast.FormName, 
        m_FormMast.FormTitle, 
        m_FormMast.FormDescription, 
        m_FormMast.IsActive, 
        m_FormMast.SQLTableName,
		parent.FormId as ParentFormId,
		parent.FormName as ParentFormName,
		parent.FormTitle as ParentFormTitle
    FROM m_FormMast  
	LEFT JOIN m_FormMast parent on parent.FormId = m_FormMast.ParentFormId
    WHERE m_FormMast.FormId = @FormId;
	
   -- Get sections of the form
    SELECT 
        s.FormSectionId, 
        s.SectionName, 
        t.FormTabId, 
        t.TabName, 
        t.IsActive AS TabIsActive,
        t.CreatedOn AS TabCreatedOn,
        t.UpdatedOn AS TabUpdatedOn,
		ISNULL(t.SortOrder,0) AS SortOrder
    FROM m_FormSection s
    LEFT JOIN m_FormTab t ON s.FormSectionId = t.FormSectionId
    WHERE t.FormId = @FormId;

    -- Get fields for each section and tab
    SELECT 
        ff.FieldId, 
        ff.FormId, 
        ff.FormTabId, 
        ff.FormSectionId, 
        ff.FieldTypeId, 
        ff.FieldName, 
        ff.FieldCaption, 
        ff.FieldLength, 
        ff.IsActive, 
        ff.IsRequired, 
        ff.CreatedOn, 
        ff.UpdatedOn, 
        ff.ParameterId, 
        ff.DDLSourceType,
		ff.DDLSourceName,
		ff.DDLTextField,
		ff.DDLValueField,
		ff.IsReadOnly,
		ff.IsVisible,	
		ff.IsVisibleInList,
		ff.FieldSize,
		ff.FieldPlaceHolder,
		ff.FieldHelpText,
		ISNULL(ff.SortOrder,0) AS SortOrder,
		m_FormFieldTypes.FieldType,
		ff.IsDisable 
    FROM m_FormField ff
	INNER JOIN m_FormFieldTypes ON m_FormFieldTypes.FieldTypeId = ff.FieldTypeId
    WHERE ff.FormId = @FormId
	ORDER BY ff.SortOrder asc
END;