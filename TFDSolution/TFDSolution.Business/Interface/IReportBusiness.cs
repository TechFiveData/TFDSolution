using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TFDSolution.Transport;
using TFDSolution.Transport.Master;

namespace TFDSolution.Business.Interface
{
    public interface IReportBusiness
    {
        List<Rep_StockSummaryModel> getItemStockSummary(DateTime fromDate, DateTime ToDate);
        List<ReportTemplateModel> getReportTamplates(string UserId);
        ResponseModel SaveReportTamplate(ReportTemplateModel mast);
        ReportData executeMISReport(ReportFilterRequest model);
        ReportTemplateModel getReportTamplateById(int ReportId);
        DataSet executeMISReportExport(ReportFilterRequest model);
        ReportData executeMISReportColumns(ReportFilterRequest model);
        Task<ReportData> ExecuteMISReportAsync(ReportFilterRequest model, string connectionstring);
        Task<LedgerReportResult> RunLedgerReport(LedgerReportRequest model);
        Task<TrailBalanceReportResult> RunTrialBalanceReport(LedgerReportRequest model);
        Task<ProfitLossReportResult> RunProfitLossReport(LedgerReportRequest model);
        Task<BalanceSheetReportResult> RunBalanceSheetReport(LedgerReportRequest model);
        Task SaveTrailBalanceReportHistoryAsync(LedgerReportRequest model, TrailBalanceReportResult response);
    }
}
