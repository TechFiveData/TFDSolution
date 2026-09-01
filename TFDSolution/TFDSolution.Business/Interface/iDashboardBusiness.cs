using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TFDSolution.Transport;

namespace TFDSolution.Business.Interface
{
    public interface iDashboardBusiness
    {
        List<DashboardModel> GetDashboard(string UserId, string CompanyId);
        DashboardModel GetDashboardData(string dashboardName, string CompanyId, int financialYear);
        ChartDataResult GetChartData(string period, string sqlScript);
    }
}
