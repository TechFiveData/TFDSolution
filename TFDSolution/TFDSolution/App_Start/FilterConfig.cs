using System.Web;
using System.Web.Mvc;
using TFDSolution.App_Start.Filters;

namespace TFDSolution
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            filters.Add(new AuditLogAttribute()); //Global logging filter
        }
    }
}
