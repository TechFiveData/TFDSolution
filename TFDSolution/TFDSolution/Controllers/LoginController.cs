using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using TFDSolution.App_Start;
using TFDSolution.Business;
using TFDSolution.Business.Interface;
using TFDSolution.Common;
using TFDSolution.Models;
using TFDSolution.Transport;
using TFDSolution.Transport.Master;

namespace TFDSolution.Controllers
{
    [LinceseAuthenticationFilter]
    public class LoginController : Controller
    {
        // GET: Login
        public readonly IUserBusines iUserBusiness;
        public LoginController()
        {
            iUserBusiness = new UserBusines();
        }
        [AllowAnonymous]
        public ActionResult Index()
        {
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetNoStore();
            Response.Cache.SetExpires(System.DateTime.UtcNow.AddMinutes(-1));
            if ((Session != null && Session["LoginUserInfo"] != null))
            {
                return RedirectToAction("Index", "Home");
            }
            if (TempData["RegistrationMesage"] != null)
            {
                ViewBag.RegistrationMsg = TempData["RegistrationMesage"];
            }
            LoginModel login = new LoginModel();
            return View(login);
        }

        [HttpPost]
        public ActionResult Index(LoginModel login)
        {
            if (ModelState.IsValid)
            {
                string EncryptPassword = AesOperation.EncryptString(login.Password);
                login.ErrorMessage = string.Empty;
                LoginUserInfo loginUser = iUserBusiness.UserLogin(new Transport.Master.UserMast()
                {
                    UserName = login.UserName,
                    Password = EncryptPassword
                });

                if (loginUser.IsSuccess.HasValue && loginUser.IsSuccess.Value)
                {
                    Session["LoginUserInfo"] = loginUser;
                    loginUser.IPAddress = login.IPAddress;
                    SessionPersister.LoginedUser = loginUser;
                    // Optionally, you can use FormsAuthentication to create a cookie
                    FormsAuthentication.SetAuthCookie(loginUser.UserName, false);
                    if (TFDSolution.Common.SessionPersister.LoginedUser != null)
                    {
                        var dashboards = TFDSolution.Common.SessionPersister.LoginedUser?.Dashboards;
                        if (dashboards != null && dashboards.Count > 0)
                        {
                            DashboardModel dashObj = dashboards.FirstOrDefault();
                            if (loginUser.DefaultDashboardId > 0 && dashboards.Where(x => x.DashboardId == loginUser.DefaultDashboardId).FirstOrDefault() != null)
                            {
                                dashObj = dashboards.Where(x => x.DashboardId == loginUser.DefaultDashboardId).FirstOrDefault();
                                if (dashObj != null && dashObj.IsSuccess.HasValue && dashObj.IsSuccess.Value)
                                {
                                    CommonBusiness.SaveAuditLog(new Transport.Common.AuditLog()
                                    {
                                        Action = "Login",
                                        CompanyId = SessionPersister.LoginedUser.CompanyInfo.CompanyId,
                                        FinancialYearId = SessionPersister.LoginedUser.CompanyInfo.DefaultFiancialId,
                                        IPAddress = SessionPersister.LoginedUser.IPAddress,
                                        PageName = "Dashboard",
                                        RecordId = string.Empty,
                                        Remark = "User Login and Redirect to Dashboard",
                                        UserId = SessionPersister.LoginedUser.UserId.Value
                                    });
                                }
                                return RedirectToAction("Index", "Dashboard", new { @dasbharodName = dashObj.DashboardName });
                            }
                            CommonBusiness.SaveAuditLog(new Transport.Common.AuditLog()
                            {
                                Action = "Login",
                                CompanyId = SessionPersister.LoginedUser.CompanyInfo.CompanyId,
                                FinancialYearId = SessionPersister.LoginedUser.CompanyInfo.DefaultFiancialId,
                                IPAddress = SessionPersister.LoginedUser.IPAddress,
                                PageName = "Calender",
                                RecordId = string.Empty,
                                Remark = "User Login and Redirect to Calender",
                                UserId = SessionPersister.LoginedUser.UserId.Value
                            });
                            return RedirectToAction("Calender", "Home");
                        }
                    }
                    // Redirect to the home page or dashboard
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    login.ErrorMessage = loginUser.Response;
                    ModelState.AddModelError("", loginUser.Response);
                }
            }
            return View(login);
        }
        [AllowAnonymous]
        public ActionResult Logout()
        {
            iUserBusiness.ReleaseUserLogin(Convert.ToString(SessionPersister.LoginedUser.UserId));
            // Clear the session
            CommonBusiness.SaveAuditLog(new Transport.Common.AuditLog()
            {
                Action = "LogOut",
                CompanyId = SessionPersister.LoginedUser.CompanyInfo.CompanyId,
                FinancialYearId = SessionPersister.LoginedUser.CompanyInfo.DefaultFiancialId,
                IPAddress = SessionPersister.LoginedUser.IPAddress,
                PageName = "Index",
                RecordId = string.Empty,
                Remark = "User LogOut and Redirect to LogIn Page",
                UserId = SessionPersister.LoginedUser.UserId.Value
            });
            Session.Clear();
            SessionPersister.LoginedUser = null;
            // Optionally, if using Forms Authentication
            FormsAuthentication.SignOut();
            
            return RedirectToAction("Index");
        }

        #region Forgot Password
        public ActionResult forgotPassword()
        {
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetNoStore();
            Response.Cache.SetExpires(System.DateTime.UtcNow.AddMinutes(-1));
            ForgotModel model = new ForgotModel();
            model.UserName = string.Empty; // p
            model.EmailAddress = string.Empty; // p
            return View(model);
        }
        [HttpPost]
        public ActionResult forgotPassword(ForgotModel model)
        {
            CommonBusiness.SaveAuditLog(new Transport.Common.AuditLog()
            {
                Action = "Open",
                CompanyId = SessionPersister.LoginedUser.CompanyInfo.CompanyId,
                FinancialYearId = SessionPersister.LoginedUser.CompanyInfo.DefaultFiancialId,
                IPAddress = SessionPersister.LoginedUser.IPAddress,
                PageName = "ForgotPassword",
                RecordId = string.Empty,
                Remark = "User open Forgot Password",
                UserId = SessionPersister.LoginedUser.UserId.Value
            });
            //string sessionId = iUserBusiness.ForgotPassword(model.UserName, model.EmailAddress);
            return RedirectToAction("updatemypassword");
        }
        public ActionResult updatemypassword(string sessionId)
        {
            return View();
        }
        #endregion

    }
}