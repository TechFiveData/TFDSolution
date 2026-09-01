using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.UI.WebControls;
using TFDSolution.Business;
using TFDSolution.Transport.Master;

namespace TFDSolution.Models
{
    public class PageModel
    {
        //public FormMast PageData { get; set; }
        public FormMast_Data Design { get; set; }
        public List<DocumentModel> DocNumberSetting { get; set; }
        public List<FormPendingModel> PendingData { get; set; }
        public List<ReportMast> Reports { get; set; }
        public string PageAction { get; set; }
        public int PendingId { get; set; }
        public string ItemSrNos { get; set; }
    }

    public class PageListData
    {      
        //public IEnumerable<IDictionary<string, object>> Data { get; set; }
        public IEnumerable<IDictionary<string, object>> Data { get; set; }
        public List<string> Columns { get; set; }
    }
}   