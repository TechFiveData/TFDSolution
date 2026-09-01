using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TFDSolution.Transport.Master;

namespace TFDSolution.Transport
{
    public class EmailRequest
    {
        public string ToEmail { get; set; }
        public string CCEmail { get; set; }
        public string BCCEmail { get; set; }
        public string Subject { get; set; }
        public string MessageBody { get; set; }
        public int ReportId { get; set; }
        public string ReportFileName { get; set; }
        public int ParentId { get; set; }
        public string FullFilePath { get; set; }
        public string UserId { get; set; }
        public string CompanyName { get; set; }
        public string CompanyId { get; set; }
        public string URNNo { get; set; }
        public string FormId { get; set; }
        public List<PageFieldData> FieldData { get; set; }
        public string UserName { get; set; }
        public string BaseURL { get; set; }
    }
}
