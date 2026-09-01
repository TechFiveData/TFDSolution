using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TFDSolution.Transport.Master;

namespace TFDSolution.Transport
{
    public class ReportTemplateModel
    {
        public int SrNo { get; set; }
        public int ReportId { get; set; }
        public string ReportTitle { get; set; }
        public string ReportName { get; set; }
        public bool IsActive { get; set; }
        public string AssignedUsers { get; set; }
        public string SQLQueryName { get; set; }
        public DateTime CreatedOn { get; set; }
        public Nullable<DateTime> UpdatedOn { get; set; }
        public string Action { get; set; }
        public List<ReportTemplateHeader> Header { get; set; }
        public List<ReportTemplateProcedureMapping> ProcedureMappings { get; set; }
    }

    public class ReportTemplateHeader
    {
        public int SrNo { get; set; }
        public int Id { get; set; }
        public string FieldName { get; set; }
        public string FieldCaption { get; set; }
        public string FieldType { get; set; }
        public string FieldDefaultValue { get; set; }
        public string DropdwonTextField { get; set; }
        public string DropdwonValueField { get; set; }
        public string SourceType { get; set; }
    }

    public class ReportTemplateProcedureMapping
    {
        public int SrNo { get; set; }
        public string ParameterName { get; set; }
        public string ParamDataType { get; set; }
        public string ParamValueName { get; set; }
        public string DefaultValue { get; set; }
    }

    public class ReportSessionCache
    {
        public ReportFilterRequest Request { get; set; }
        public ReportData Result { get; set; }
        public List<Dictionary<string, object>> Data { get; set; }   // FULL or FILTERED
        public int TotalRecords { get; set; }
        public List<ColumnDef> Columns { get; set; }
        public Dictionary<string, string> ColumnSearches { get; set; }
    }
    public class ReportFilterRequest : BaseRequest
    {
        public int ReportId { get; set; }
        public string ReportTitle { get; set; }
        public string ReportName { get; set; }
        public string CompanyName { get; set; }
        public List<PageFieldData> FieldData { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string SearchTerm { get; set; }
    }

    public class ReportData : ResponseModel
    {
        public List<Dictionary<string, object>> Data { get; set; }
        public List<ColumnDef> columns { get; set; }
        public List<string> secondColumns { get; set; }
        public int TotalRecords { get; set; }
    }

    public class ColumnDef
    {
        public string data { get; set; }
        public string title { get; set; }
    }

    public class LedgerReportRequest
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int AccountId { get; set; }
        public string AccountName { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public string CompanyId { get; set; }
        public string CompanyName { get; set; }
        public int FinacialYearId { get; set; }
        public string UserId { get; set; }
        public string ReportType { get; set; }
    }

    public class LedgerReportResult : ResponseModel
    {
        public List<LedgerReportData> Data { get; set; }
        public int TotalRecords { get; set; }
    }

    public class LedgerReportSessionCache
    {
        public LedgerReportRequest Request { get; set; }
        public List<LedgerReportData> Data { get; set; }
        public int TotalRecords { get; set; }
        public Dictionary<string, string> ColumnSearches { get; set; }
    }
    public class LedgerReportData
    {
        public int RowNo { get; set; }
        public int Id { get; set; }
        public string CompanyId { get; set; }
        public int FinancialYearId { get; set; }
        public string FormId { get; set; }
        public Nullable<DateTime> CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public int ParentId { get; set; }
        public int DetailId { get; set; }
        public int Account_Id { get; set; }
        public string AccountName { get; set; }
        public string URNNo { get; set; }
        public string DocNo { get; set; }
        public Nullable<DateTime> DocDate { get; set; }
        public decimal Debit_Amount { get; set; }
        public decimal Credit_Amount { get; set; }
        public decimal Balance { get; set; }
        public int CurrencyId { get; set; }
        public decimal ExchangeRate { get; set; }
        public string Remarks { get; set; }
        public string FormTitle { get; set; }
        public string FormName { get; set; }
        public string Currency { get; set; }
        public string UserName { get; set; }
    }

    public class TrailBalanceReportData
    {
        public int Level { get; set; }
        public int Account_Id { get; set; }
        public string Particular { get; set; }
        public decimal Op_Debit { get; set; }
        public decimal Op_Credit { get; set; }
        public decimal Tr_Debit { get; set; }
        public decimal Tr_Credit { get; set; }
        public decimal Cl_Debit { get; set; }
        public decimal Cl_Credit { get; set; }
        public string RowType { get; set; }
        public string SortPath { get; set; }
        public int SortOrder { get; set; }
    }

    public class TrailBalanceReportResult : ResponseModel
    {
        public List<TrailBalanceReportData> Data { get; set; }
        public int TotalRecords { get; set; }
    }
    public class TrailBalanceReportSessionCache
    {
        public LedgerReportRequest Request { get; set; }
        public List<TrailBalanceReportData> Data { get; set; }
        public int TotalRecords { get; set; }
        public Dictionary<string, string> ColumnSearches { get; set; }
    }

    public class ProfitLossReportResult : ResponseModel
    {
        public List<ProfitLossReportData> Data { get; set; }
        public Decimal TotalIncome { get; set; }
        public Decimal TotalExpense { get; set; }
        public Decimal NetProfit { get; set; }
        public int TotalRecords { get; set; }
    }
    public class ProfitLossReportData
    {
        public int Level { get; set; }
        public string Particular { get; set; }
        public string AccountName { get; set; }
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
        public decimal Amount { get; set; }
        public string RowType { get; set; }
        public string SortPath { get; set; }
        public int SortOrder { get; set; }
        public string CompanyId { get; set; }
        public int FinancialYearId { get; set; }
        public string FormId { get; set; }
        public Nullable<DateTime> CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public int ParentId { get; set; }
        public int DetailId { get; set; }
        public int Account_Id { get; set; }
        public string FormTitle { get; set; }
        public string FormName { get; set; }
        public string Currency { get; set; }
        public string UserName { get; set; }

    }
    public class ProfitLossReportSessionCache
    {
        public LedgerReportRequest Request { get; set; }
        public List<ProfitLossReportData> Data { get; set; }
        public int TotalRecords { get; set; }
        public Dictionary<string, string> ColumnSearches { get; set; }
    }

    public class BalanceSheetReportResult : ResponseModel
    {
        public List<BalanceSheetReportData> Data { get; set; }
        public Decimal TotalAsset { get; set; }
        public Decimal TotalLiability { get; set; }
        public Decimal Difference { get; set; }
        public int TotalRecords { get; set; }
    }
    public class BalanceSheetReportData
    {
        public int Level { get; set; }
        public string Particular { get; set; }
        public string AccountName { get; set; }
        public decimal Debit { get; set; }
        public decimal Credit { get; set; }
        public decimal Balance { get; set; }
        public string BalanceType { get; set; }
        public string RowType { get; set; }
        public string SortPath { get; set; }
        public int SortOrder { get; set; }
        public string CompanyId { get; set; }
        public int FinancialYearId { get; set; }
        public string FormId { get; set; }
        public Nullable<DateTime> CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public int ParentId { get; set; }
        public int DetailId { get; set; }
        public int Account_Id { get; set; }
        public string FormTitle { get; set; }
        public string FormName { get; set; }
        public string Currency { get; set; }
        public string UserName { get; set; }
    }
    public class BalanceSheetReportSessionCache
    {
        public LedgerReportRequest Request { get; set; }
        public List<BalanceSheetReportData> Data { get; set; }
        public int TotalRecords { get; set; }
        public Dictionary<string, string> ColumnSearches { get; set; }
    }
}
