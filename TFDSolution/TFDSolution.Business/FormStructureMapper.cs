using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TFDSolution.Transport.Master;

namespace TFDSolution.Business
{

    public static class FormStructureMapper
    {
        public static FormMast_Data MapToFormStructure(DataSet ds)
        {
            if (ds == null || ds.Tables.Count < 3)
                return null;

            // Table 1: FormMast
            DataTable formTable = ds.Tables[0];
            // Table 2: Form Sections with Tabs
            DataTable sectionTabTable = ds.Tables[1];
            // Table 3: Fields
            DataTable fieldsTable = ds.Tables[2];

            // Map FormMast
            FormMast_Data form = formTable.AsEnumerable().Select(row => new FormMast_Data
            {
                FormId = row.Field<Guid>("FormId"),
                ParentFormId = row.Field<Guid>("ParentFormId"),
                FormName = row.Field<string>("FormName"),
                FormTitle = row.Field<string>("FormTitle"),
                FormDescription = row.Field<string>("FormDescription"),
                IsActive = row.Field<bool>("IsActive"),
                SQLTableName = row.Field<string>("SQLTableName"),
                ParentFormName = row.Field<string>("ParentFormName"),
                ParentFormTitle = row.Field<string>("ParentFormTitle"),
                SqlTemplateId = row.Field<int>("SqlTemplateId"),
                Sections = new List<FormSection>()
            }).FirstOrDefault();

            if (form == null)
                return null;

            // Map Sections and Tabs
            var sections = sectionTabTable.AsEnumerable()
                .GroupBy(row => new { FormSectionId = row.Field<Guid>("FormSectionId"), SectionName = row.Field<string>("SectionName") })
                .Select(sectionGroup => new FormSection
                {
                    FormSectionId = sectionGroup.Key.FormSectionId.ToString(),
                    SectionName = sectionGroup.Key.SectionName,
                    Tabs = sectionGroup.Select(tabRow => new FormTab
                    {
                        FormTabId = tabRow.Field<Guid>("FormTabId").ToString(),
                        TabName = tabRow.Field<string>("TabName"),
                        TabTitle = tabRow.Field<string>("TabTitle"),
                        IsActive = tabRow.Field<bool>("TabIsActive"),
                        CreatedOn = tabRow.Field<DateTime>("TabCreatedOn"),
                        UpdatedOn = tabRow.Field<DateTime?>("TabUpdatedOn"),
                        TabSQLTableName = tabRow.Field<string>("TabSQLTableName"),
                        SortOrder = tabRow.Field<int>("SortOrder"),
                        NextTabDataScript = tabRow.Field<string>("NextTabDataScript"),
                        Fields = new List<FormField>()
                    }).OrderBy(x => x.SortOrder).ToList()
                }).ToList();

            // Map Fields and Attach to Tabs
            foreach (var fieldRow in fieldsTable.AsEnumerable())
            {
                var field = new FormField
                {
                    FieldId = fieldRow.Field<Guid>("FieldId").ToString(),
                    FormId = fieldRow.Field<Guid>("FormId").ToString(),
                    FormTabId = fieldRow.Field<Guid>("FormTabId").ToString(),
                    FormSectionId = fieldRow.Field<Guid>("FormSectionId").ToString(),
                    FieldTypeId = fieldRow.Field<Guid>("FieldTypeId").ToString(),
                    FieldName = fieldRow.Field<string>("FieldName"),
                    FieldCaption = fieldRow.Field<string>("FieldCaption"),
                    FieldLength = fieldRow.Field<int>("FieldLength"),
                    IsActive = fieldRow.Field<bool>("IsActive"),
                    IsRequired = fieldRow.Field<bool>("IsRequired"),
                    CreatedOn = fieldRow.Field<DateTime>("CreatedOn"),
                    UpdatedOn = fieldRow.Field<DateTime?>("UpdatedOn"),
                    ParameterId = fieldRow.Field<Guid?>("ParameterId"),
                    DDLSourceType = fieldRow.Field<string>("DDLSourceType"),
                    DDLSourceName = fieldRow.Field<string>("DDLSourceName"),
                    DDLTextField = fieldRow.Field<string>("DDLTextField"),
                    DDLValueField = fieldRow.Field<string>("DDLValueField"),
                    IsReadOnly = fieldRow.Field<bool?>("IsReadOnly"),
                    IsVisible = fieldRow.Field<bool?>("IsVisible"),
                    IsVisibleInList = fieldRow.Field<bool?>("IsVisibleInList"),
                    FieldSize = fieldRow.Field<string>("FieldSize"),
                    FieldPlaceHolder = fieldRow.Field<string>("FieldPlaceHolder"),
                    FieldHelpText = fieldRow.Field<string>("FieldHelpText"),
                    FieldType = fieldRow.Field<string>("FieldType"),
                    FieldFormula = fieldRow.Field<string>("FieldFormula"),
                    SortOrder = fieldRow.Field<int?>("SortOrder"),
                    IsDisable = fieldRow.Field<bool?>("IsDisable"),
                    IsMultiSelection = fieldRow.Field<bool?>("IsMultiSelection"),
                    AllowMultiDocument = fieldRow.Field<bool?>("AllowMultiDocument"),
                    IsDependencyField = fieldRow.Field<bool?>("IsDependencyField"),
                    FieldDecimal = fieldRow.Field<int>("FieldDecimal"),
                    IsTimeField = fieldRow.Field<bool?>("IsTimeField"),
                    FieldFormat = fieldRow.Field<string>("FieldFormat"),
                };

                // Attach field to the correct tab in the correct section
                var matchingTab = sections
                    .SelectMany(s => s.Tabs)
                    .FirstOrDefault(t => t.FormTabId == field.FormTabId.ToString());

                matchingTab?.Fields.Add(field);
            }

            // Assign mapped sections to the form
            form.Sections = sections;

            return form;
        }
    }
}
