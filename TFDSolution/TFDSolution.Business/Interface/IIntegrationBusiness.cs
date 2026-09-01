using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TFDSolution.Transport.Integration;

namespace TFDSolution.Business.Interface
{
    public interface IIntegrationBusiness
    {
        EWayBillRequest GetEWayBillRequestData(string formId, int parentId, string companyId, string userId, int financialYearId, string storedProcedure);
        void InsertIntegrationLogs(IntegrationAPILog model);
        IntegrationConfiguration GetIntegrationConfiguration(string companyId);
        EInvoiceRequest GetEInvoiceRequestData(string formId, int parentId, string companyId, string userId, int financialYearId, string storedProcedure);
        bool UpdateEWayBillData(string sqlTableName, int parentID, string EWayBillNumber, string EWayBillDate, string EWayValidUpto, string EWayCreatedBy);
        bool UpdateEWayBillCancel(string sqlTableName, int parentID, string EWayBillCancelDate, string EWayCreatedBy);
        bool UpdateEInvoiceData(string sqlTableName, int parentID, string eInvoiceAckNo, string eInvoiceAckDate, string eInvoiceIRN, string eInvoiceStatus, string eInvoiceCreatedBy);

        bool UpdateEInvoiceCancel(string sqlTableName, int parentID, string EInvoiceCancelDate, string EInvoiceCreatedBy);
    }
}
