using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFDSolution.Transport.Master
{
    public class FormTabData : ResponseModel
    {
        public IEnumerable<IDictionary<string, object>> Data { get; set; }
        public List<string> Columns { get; set; }
        public List<PageGridData> PageData { get; set; }
        public List<SummaryItem> Summary { get; set; }
        public string IsNextButtonStoreProcedure { get; set; }
    }
    public class SummaryItem
    {
        public string Label { get; set; }
        public object Value { get; set; }
    }
    public class FormData : ResponseModel
    {
        public IEnumerable<IDictionary<string, object>> Data { get; set; }
        public List<PageGridData> PageData { get; set; }
        public List<string> Columns { get; set; }
        public int TotalRecords { get; set; }
    }

    public class PageGridData
    {
        public string ColumnName { get; set; }
        public string ColumnTitle { get; set; }
        public object ColumnData { get; set; }
        public string ColumnType { get; set; }
        public int ColumnLength { get; set; }
        public bool IsRequired { get; set; }
        public bool IsSummary { get; set; }

        public string FieldId { get; set; }

        public int ItemAdvanceId { get; set; }

        public string FieldTypeId { get; set; }

        public bool IsActive { get; set; }

        public string FieldName { get; set; }

        public string FieldCaption { get; set; }

        public string FieldFormula { get; set; }

        public int FieldLength { get; set; }

        public string FieldType { get; set; }

        public string TabName { get; set; }

        public string SectionName { get; set; }

        public Guid UserId { get; set; }

        public Guid CompanyId { get; set; }

        public string DDLTextField { get; set; }

        public string DDLValueField { get; set; }

        public string DDLSourceType { get; set; }

        public string DDLSourceName { get; set; }

        public bool? IsReadOnly { get; set; }

        public bool? IsVisible { get; set; }

        public bool? IsVisibleInList { get; set; }

        public bool? IsDependencyField { get; set; }

        public bool? IsMultiSelection { get; set; }

        public bool? AllowMultiDocument { get; set; }

        public string SQLTableName { get; set; }

        public string FieldSize { get; set; }

        public int FieldDecimal { get; set; }

        public string FieldPlaceHolder { get; set; }

        public string FieldHelpText { get; set; }

        public int? SortOrder { get; set; }

        public bool? IsDisable { get; set; }

        public bool? IsUnique { get; set; }

        public string FieldValue { get; set; }

        public string FieldRemarks { get; set; }

        public int TemplateId { get; set; }

        public int InfoTabCount { get; set; }
    }

    public class PageTabModel
    {
        public List<FormField> FormFields { get; set; }
        public FormTab Design { get; set; }
        public int Id { get; set; }
        public string PageAction { get; set; }
        public string FormTabId { get; set; }
        public int PrevId { get; set; }
        public int NextId { get; set; }
        public IEnumerable<IDictionary<string, object>> Data { get; set; }

        public string From_Source { get; set; }
        public string From_URNNO { get; set; }
        public int From_Item_SRNO { get; set; }
        public int DetailId { get; set; }
        public string FormId { get; set; }
        public int ParentId { get; set; }
        public string FieldId { get; set; }
        public int IsDefault { get; set; }
        public string CompanyId { get; set; }
        public string UserId { get; set; }
        public int FinancialYearId { get; set; }
    }
    public class BomProcessModel
    {
        public int Id { get; set; }
        public string FormId { get; set; }
        public int ParentId { get; set; }
        public bool QCEnable { get; set; }
        public int ProcessId { get; set; }
        public int LocationId { get; set; }
        public Guid CreatedBy { get; set; }
        public string ProcessName { get; set; }
        public string LocationName { get; set; }
    }

    public class TabRecordSortOrderModel
    {
        public int Id { get; set; }
        public int SortOrder { get; set; }
    }
}
