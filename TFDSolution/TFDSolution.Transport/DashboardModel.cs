using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFDSolution.Transport
{
    public class DashboardModel : ResponseModel
    {
        public int DashboardId { get; set; }
        public string DashboardTitle { get; set; }
        public string DashboardName { get; set; }
        public int SortOrder { get; set; }
        public List<Dashboard_Widgets> Settings { get; set; }
    }
    public class Dashboard_Widgets
    {
        public string Title { get; set; }
        public string WidgetType { get; set; }
        public string BackgroundColor { get; set; }
        public string FontColor { get; set; }
        public string SQLDataScript { get; set; }
        public bool IsActive { get; set; }
        public int SortOrder { get; set; }
        public string RedirectURL { get; set; }
        public string DisplaySize { get; set; }
        public string ChartType { get; set; }
        public IEnumerable<IDictionary<string, object>> Data { get; set; }
        public DashboardResultData ResultData { get; set; }
    }

    public class DashboardResultData
    {
        public IEnumerable<IDictionary<string, object>> Data { get; set; }
        public List<string> SummaryColumn { get; set; }
    }
}
