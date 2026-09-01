using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;

namespace TFDSolution.Transport.Master
{

    public class FormMast : Permissions
    {
        public System.Guid FormId { get; set; }
        public string FormName { get; set; }

        public bool IsHierachyForm { get; set; }
        public string FormTitle { get; set; }
        public string FormDescription { get; set; }
        public bool IsActive { get; set; }
        public Nullable<System.Guid> ParentFormId { get; set; }
        public string ParentFormName { get; set; }
        public string ParentFormTitle { get; set; }
        public string PFormId { get; set; }
        public string SQLTableName { get; set; }
        public System.Guid UserId { get; set; }
        public System.Guid CompanyId { get; set; }
        public string Prefix { get; set; }
        public string FormAlias { get; set; }
        public Nullable<int> SqlTemplateId { get; set; }
        public int SortOrder { get; set; }
        public List<FormMast> ChildForm { get; set; }

        public List<FormField> FormFields { get; set; }

        public List<FormSection> FormSection { get; set; }
        public List<FormFieldType> FormFieldTypes { get; set; }

    }
    public class FormSystemModel
    {
        public Guid FormId { get; set; }
        public string FormName { get; set; }
        public string FormTitle { get; set; }
        public string FormDescription { get; set; }
        public bool IsActive { get; set; }
        public Guid? ParentFormId { get; set; }
        public int SortOrder { get; set; }
        public bool IsHierachyForm { get; set; }
        public string SQLTableName { get; set; }
        public string SqlTemplateId { get; set; }
    }
    public class FormFieldType
    {
        public Guid FieldTypeId { get; set; }

        [Required]
        [StringLength(50)]
        public string FieldType { get; set; }

        [StringLength(100)]
        public string FieldTypeClass { get; set; }

        [StringLength(30)]
        public string FieldTypeName { get; set; }

        public int? SortOrder { get; set; }

        public bool? IsFieldTypeElement { get; set; }
    }
    public class FormFieldOption
    {

    }

    public class FieldDependencySQL
    {
        public string FieldName { get; set; }
        public string SQLQuery { get; set; }
        public string DDLTextField { get; set; }
        public string DDLValueField { get; set; }

    }
    public class GridField
    {
        public string FieldTitle { get; set; }
        public string FieldName { get; set; }
        public List<PageFieldData> FieldValue { get; set; }
    }
    public class FormField
    {
        public string FieldId { get; set; }
        public string FormId { get; set; }
        public string FormTabId { get; set; }
        public string FieldTypeId { get; set; }
        public bool IsActive { get; set; }
        public string FieldName { get; set; }
        public string FieldCaption { get; set; }
        public string FieldFormula { get; set; }
        public int FieldLength { get; set; }
        // public int SortOrder { get; set; }
        public string FormSectionId { get; set; }
        public bool IsRequired { get; set; }
        public System.DateTime CreatedOn { get; set; }
        public Nullable<System.DateTime> UpdatedOn { get; set; }
        public Nullable<System.Guid> ParameterId { get; set; }
        public string FieldType { get; set; }
        public string TabName { get; set; }
        public string SectionName { get; set; }
        public System.Guid UserId { get; set; }
        public System.Guid CompanyId { get; set; }
        public string DDLTextField { get; set; }
        public string DDLValueField { get; set; }
        public string DDLSourceType { get; set; }
        public string DDLSourceName { get; set; }
        public Nullable<bool> IsReadOnly { get; set; }
        public Nullable<bool> IsSummary { get; set; }
        public Nullable<bool> IsVisible { get; set; }
        public Nullable<bool> IsVisibleInList { get; set; }
        public Nullable<bool> IsDependencyField { get; set; }
        public Nullable<bool> IsMultiSelection { get; set; }
        public Nullable<bool> AllowMultiDocument { get; set; }
        public string SQLTableName { get; set; }
        public string FieldSize { get; set; }
        public int FieldDecimal { get; set; }
        public string FieldPlaceHolder { get; set; }
        public string FieldHelpText { get; set; }
        public int? SortOrder { get; set; }
        public Nullable<bool> IsDisable { get; set; }
        public Nullable<bool> IsUnique { get; set; }
        public string FieldValue { get; set; }
        public string FieldRemarks { get; set; }
        public int TemplateId { get; set; }
        //AllowMultiDocument
        public List<FieldSPParameter> Parameters { get; set; }
        public int InfoTabCount { get; set; }
        public Nullable<bool> IsTimeField { get; set; }
        public string FieldFormat { get; set; }
        public bool IsLanguage { get; set; }
    }

    public class FieldSPParameter
    {
        public int ParameterId { get; set; }
        public string ParameterName { get; set; }
        public string ParamDefaultValue { get; set; }
        public string ParameterValueFieldName { get; set; }
        public string ParameterFromTable { get; set; }
    }

    public class ItemSrNoResult
    {
        public int ItemSrNo { get; set; }
        public int RowIndex { get; set; }
    }

    public class SelectionRequest
    {
        public string SearchTerm { get; set; }
        public string FieldValue { get; set; }
        public List<PageFieldData> FieldData { get; set; }
        public List<PageFieldData> HeaderFieldData { get; set; }
        public string UserId { get; set; }
        public string CompanyId { get; set; }
        public int FiancialYearId { get; set; }
        public string PageType { get; set; }
        public string FormId { get; set; }
        public string TabId { get; set; }
        public string SectionId { get; set; }
        public string FieldId { get; set; }
    }
    public class DynamicSubmitPageModel
    {
        public string CompanyId { get; set; }
        public int FiancialYearId { get; set; }
        public string FormId { get; set; }
        public string UserId { get; set; }
        public int DetailId { get; set; }
        public int ParentId { get; set; }
    }
    public class SubmitFormModel
    {
        public string PageTablename { get; set; }
        public int SqlTemplateId { get; set; }
        public int Id { get; set; }
        public int ParentId { get; set; }
        public List<PageFieldData> FieldData { get; set; }

        public List<PageFieldData> HeaderFieldData { get; set; }
        public Nullable<System.Guid> UserId { get; set; }
        public Nullable<System.Guid> CompanyId { get; set; }
        public int FiancialYearId { get; set; }
        public string PageType { get; set; }
        public string PageAction { get; set; }
        public string FormId { get; set; }
        public string TabId { get; set; }
        public string SectionId { get; set; }
        public int DocId { get; set; }
        public int CurrencyId { get; set; }
        public string FromFieldName { get; set; }
        public string FieldId { get; set; }
        public bool AllowMultipleDocument { get; set; }
        public string MultipleDocFieldName { get; set; }
        public string URNNo { get; set; }
        public int ProcessId { get; set; }

        public int PendingId { get; set; }
        public string ItemSrNos { get; set; }
    }
    public class ItemOtherCharges
    {
        public int OtherChargesDetailId { get; set; }
        public string TaxChargeName { get; set; }
        public decimal Percentage { get; set; }
        public decimal Amount { get; set; }
    }
    public class PageOtherCharges
    {
        public int DetailId { get; set; }
        public Guid UserId { get; set; }
        public int ParentId { get; set; }
        public string FormId { get; set; }
        public List<ItemOtherCharges> FieldData { get; set; }
    }
    public class PageFieldData
    {
        public string FieldName { get; set; }
        public bool IsRequired { get; set; }
        public string FieldValue { get; set; }
        public string FieldText { get; set; }
        public string FieldType { get; set; }
        public string MessageTemplate { get; set; }
        public string TabTitle { get; set; }
        public int SrNo { get; set; }
    }

    public class ItemOtherChargeData
    {
        public string TaxChargeName { get; set; }
        public int OtherChargesDetailId { get; set; }
        public int ParentId { get; set; }
        public string FormId { get; set; }
        public string UserId { get; set; }
        public decimal Amount { get; set; }
    }

    public class PageCharges
    {
        public string TaxChargeName { get; set; }
        public string Effect { get; set; }

        public decimal Amount { get; set; }
        public int ChangeValue { get; set; }
        public int OtherChargesDetailId { get; set; }
    }
    public class PageTotal
    {
        public string Name { get; set; }
        public decimal Amount { get; set; }
    }
    public class PageTotalData
    {
        public string PageAction { get; set; }
        public List<PageTotal> Totals { get; set; }
        public List<PageCharges> OtherChargeDetail { get; set; }
    }

    public class FormMast_Data : Permissions
    {
        public System.Guid FormId { get; set; }
        public string FormName { get; set; }
        public string FormTitle { get; set; }
        public string FormDescription { get; set; }
        public bool IsActive { get; set; }
        public Nullable<System.Guid> ParentFormId { get; set; }
        public string ParentFormName { get; set; }
        public string ParentFormTitle { get; set; }
        public string PFormId { get; set; }
        public string SQLTableName { get; set; }
        public System.Guid UserId { get; set; }
        public System.Guid CompanyId { get; set; }
        public int FinancialYearId { get; set; }
        public string Prefix { get; set; }
        public int SqlTemplateId { get; set; }
        public List<FormSection> Sections { get; set; } = new List<FormSection>();

        public IEnumerable<IDictionary<string, object>> Data { get; set; }
    }

    public class FormSection
    {
        public string FormSectionId { get; set; }
        public string SectionName { get; set; }
        public List<FormTab> Tabs { get; set; } = new List<FormTab>();
    }

    public class FormTab
    {
        public string FormTabId { get; set; }
        public string TabName { get; set; }
        public string TabTitle { get; set; }
        public string FormId { get; set; }
        public string SectionId { get; set; }
        public bool IsActive { get; set; }
        public string TabSQLTableName { get; set; }
        //public int SortOrder { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public int? SortOrder { get; set; }
        public List<FormField> Fields { get; set; } = new List<FormField>();
        public string NextTabDataScript { get; set; }
        public string SectionName { get; set; }
        public bool EnableInlineForm { get; set; }
    }
    public class FormSectionTabDto
    {
        public Guid FormSectionId { get; set; }
        public string SectionName { get; set; }
        public Guid FormTabId { get; set; }
        public string TabName { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? UpdatedOn { get; set; }
    }

    public class DeleteFormSetting
    {
        public string PrimaryId { get; set; }
        public string Source { get; set; }
    }

    public class FormSettings
    {
        public int SrNo { get; set; }
        public string KeyCaption { get; set; }
        public string KeyName { get; set; }
        public string FormId { get; set; }
        public string KeyValue { get; set; }
        public string FormTabId { get; set; }
    }

    public class FormStockSettings
    {
        public string TabName { get; set; }
        public string FormTabId { get; set; }
        public string FormId { get; set; }
        public string StoredProcedure { get; set; }
        public string StockEffect { get; set; }
    }

    public class ItemStockModel
    {
        public string CompanyId { get; set; }
        public int Id { get; set; }
        public int DetailId { get; set; }
        public string Userid { get; set; }
        public int Status { get; set; }
        public string FormId { get; set; }
        public string TabId { get; set; }
        public string PageAction { get; set; }
    }

    public class FormDataModel
    {
        public string FormId { get; set; }
        public string FormTitle { get; set; }
        public string FormName { get; set; }
        public string CompanyId { get; set; }
        public string CompanyName { get; set; }
        public string UserId { get; set; }
        public int FinacialYearId { get; set; }
        public string TabId { get; set; }
        public string TabName { get; set; }
        public int ParentId { get; set; }
        public int ProcessId { get; set; }
        public List<PageFieldData> FieldData { get; set; }

        public string ExportType { get; set; }
    }

    public class DependencyModel
    {
        public List<PageFieldData> PageFieldData { get; set; }
        public List<PageFieldData> DependencyFields { get; set; }
    }
    public class SqlQueryModel
    {
        public string URNNo { get; set; }
        public string Query { get; set; }
    }

    public class ExportTemplateRequest
    {
        public string TabId { get; set; }
        public string TabName { get; set; }
        public string TabTitle { get; set; }
        public string FileName { get; set; }
        public string FormId { get; set; }
        public string URNNo { get; set; }
        public int ProcessId { get; set; }
        public string ExportType { get; set; }
        public List<PageFieldData> HeaderFieldData { get; set; }
    }

    public class FormStatusDataView
    {
        public string UserName { get; set; }
        public bool IsApproved { get; set; }
        public int PriorityNo { get; set; }
        public int StatusId { get; set; }
        public string StatusName { get; set; }
        public string StatusDateTime { get; set; }
    }

    public class FormItemAdvanceSettings
    {
        public int ItemAdvanceId { get; set; }
        public string GridName { get; set; }
        public string GridTitle { get; set; }
        public string Description { get; set; }
        public string SQLTableName { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public int? SortOrder { get; set; }
        public List<FormItemAdvanceField> Fields { get; set; } = new List<FormItemAdvanceField>();
        public bool EnableInlineForm { get; set; }
        public string GetDataScript { get; set; }
        public string SetDataScript { get; set; }
        public string ValidDataScript { get; set; }
        public string UserId { get; set; }
        public string CompanyId { get; set; }
    }

    public class FormItemAdvanceField
    {
        public string FieldId { get; set; }
        public int ItemAdvanceId { get; set; }
        public string FieldTypeId { get; set; }
        public bool IsActive { get; set; }
        public string FieldName { get; set; }
        public string FieldCaption { get; set; }
        public string FieldFormula { get; set; }
        public int FieldLength { get; set; }
        // public int SortOrder { get; set; }
        public bool IsRequired { get; set; }
        public System.DateTime CreatedOn { get; set; }
        public Nullable<System.DateTime> UpdatedOn { get; set; }
        public string FieldType { get; set; }
        public string TabName { get; set; }
        public string SectionName { get; set; }
        public System.Guid UserId { get; set; }
        public System.Guid CompanyId { get; set; }
        public string DDLTextField { get; set; }
        public string DDLValueField { get; set; }
        public string DDLSourceType { get; set; }
        public string DDLSourceName { get; set; }
        public Nullable<bool> IsReadOnly { get; set; }
        public Nullable<bool> IsSummary { get; set; }
        public Nullable<bool> IsVisible { get; set; }
        public Nullable<bool> IsVisibleInList { get; set; }
        public Nullable<bool> IsDependencyField { get; set; }
        public Nullable<bool> IsMultiSelection { get; set; }
        public Nullable<bool> AllowMultiDocument { get; set; }
        public string SQLTableName { get; set; }
        public string FieldSize { get; set; }
        public int FieldDecimal { get; set; }
        public string FieldPlaceHolder { get; set; }
        public string FieldHelpText { get; set; }
        public int? SortOrder { get; set; }
        public Nullable<bool> IsDisable { get; set; }
        public Nullable<bool> IsUnique { get; set; }
        public string FieldValue { get; set; }
        public string FieldRemarks { get; set; }
        public int TemplateId { get; set; }
        public int InfoTabCount { get; set; }
        public Nullable<bool> IsTimeField { get; set; }
        public string FieldFormat { get; set; }
    }

    public class FormCommentModel
    {
        public string FormId { get; set; }
        public string UserId { get; set; }
        public string CommentText { get; set; }
        public int ParentId { get; set; }
        public int DetailId { get; set; }
        public int CommentId { get; set; }
        public string CreatedBy { get; set; }
        public string ShortName { get; set; }
        public string CommentedOn { get; set; }
        public string UserName { get; set; }
        public bool IsYou { get; set; }
        public bool IsLoggedIn { get; set; }
    }

    public class RFQDataModel
    {
        public string URNno { get; set; }
        public string DocDate { get; set; }
        public CompanyMast CompanyMast { get; set; }
    }
}