using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFDSolution.Transport
{
    public class FormInquiryData
    {
            public string FormPendingId { get; set; }
            public int ParentId { get; set; }
            public string Ids { get; set; }  // comma separated item ids
    }
}
