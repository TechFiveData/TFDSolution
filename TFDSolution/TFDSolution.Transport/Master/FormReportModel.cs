using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFDSolution.Transport.Master
{
    public class FormReportModel
    {
        public string FormId { get; set; }
        public List<ReportMast> Reports { get; set; }
    }
}
