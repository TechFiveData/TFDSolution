using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFDSolution.Transport.Master
{
    public class DocumentModel
    {
        public int DocNoSettingId { get; set; }
        public string DocName { get; set; }
        public string DocAlias { get; set; }
        public string StartNumber { get; set; }
        public string FormId { get; set; }
        public bool IsDefault { get; set; }
    }

    public class FormDocumentNo
    {
        public int DocNoSettingId { get; set; }
        public string DocNo { get; set; }
    }
}
