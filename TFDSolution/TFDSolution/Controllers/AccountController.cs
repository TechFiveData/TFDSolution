using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using TFDSolution.App_Start;
using TFDSolution.Business;
using TFDSolution.Business.Interface;
using TFDSolution.Common;
using TFDSolution.Transport.Master;

namespace TFDSolution.Controllers
{
    [CustomAuthenticationFilter]
    public class AccountController : Controller
    {
        public readonly IUserBusines userBusines;
        public readonly IMasterBusiness master;
        public AccountController()
        {
            userBusines = new UserBusines();
            master = new MasterBusiness();  
        }
        // GET: Account
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Profile()
        {
            UserMast mast = userBusines.GetUserInfo(SessionPersister.LoginedUser.UserId.Value);
            return View(mast);
        }

        public ActionResult MyNofications()
        {
            return View();
        }
        public async Task<JsonResult> getMyNofications()
        {
            string userId = SessionPersister.LoginedUser.UserId.Value.ToString();
            // Await the task
            List<UserNotificationModel> userNotifications = await master.getUserNotification(userId, 0);
            return Json(userNotifications, JsonRequestBehavior.AllowGet);
        }
        public async Task<JsonResult> GetNotificationDetail(int notificationId)
        {
            string userId = SessionPersister.LoginedUser.UserId.Value.ToString();
            UserNotificationModel response = await master.GetNotificationDetail(notificationId, userId);
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SystemUpdates()
        {
            ERPUpdateModel model = new ERPUpdateModel();
            return View(model);
        }
    }
}