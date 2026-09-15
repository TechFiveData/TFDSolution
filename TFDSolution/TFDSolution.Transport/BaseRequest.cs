using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFDSolution.Transport
{
    public class BaseRequest
    {
        public string CompanyId { get; set; }
        public string UserId { get; set; }
        public int FinacialYearId { get; set; }
    }
    public class QueryParamModel
    {
        public int PendingId { get; set; }
        public List<int> DetailIds { get; set; }
        public string DetailId { get; set; }
        public DateTime ExpiryDate { get; set; }
    }
    public class SubmitQuoteRequest: BaseRequest
    {
        public string ErrorMessage { get; set; }
        public string FormId { get; set; }
        public string CompanyName { get; set; }
    }

    public class PendingSelectedRequest
    {
        public string FieldValues { get; set; }
        public string FormTabId { get; set; }
        public string TableName { get; set; }
    }
}
