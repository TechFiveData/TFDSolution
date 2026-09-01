using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TFDSolution.App_Start;

namespace TFDSolution.Controllers
{
    [LinceseAuthenticationFilter]
    public class BaseController : Controller
    {
        // GET: Base
        public BaseController()
        {
        }
    }
}