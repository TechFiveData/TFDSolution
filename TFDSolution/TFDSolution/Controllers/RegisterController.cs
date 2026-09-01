using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TFDSolution.App_Start;
using TFDSolution.Business;
using TFDSolution.Business.Interface;
using TFDSolution.Common;
using TFDSolution.Models;
using TFDSolution.Transport;

namespace TFDSolution.Controllers
{
    [LinceseAuthenticationFilter]
    public class RegisterController : Controller
    {
        // GET: Register
        public ActionResult Index()
        {
            if ((Session != null && Session["LoginUserInfo"] != null))
            {
                return RedirectToAction("Index", "Home");
            }
            RegisterModel registerModel = new RegisterModel();
            return View(registerModel);
        }

        [HttpPost]
        [AllowAnonymous]
        public ActionResult Index(RegisterModel registerModel)
        {
            if (ModelState.IsValid)
            {
                IUserBusines iUser = new UserBusines();
                int ActiveUsers = iUser.GetActiveUsers();
                LicenseModel model = ValidationKey.IsValid();
                if (model.MaximumUser <= ActiveUsers)
                {
                    //ViewBag.RegistrationMsg = "You've exceeded the user limits as per your current plan.";
                    ModelState.AddModelError("", "You've exceeded the user limits as per your current plan.");

                }
                else
                {
                    string EncryptPassword = AesOperation.EncryptString(registerModel.Password);
                    ResponseModel reult = iUser.SetUserData(new Transport.Master.UserMast()
                    {
                        EmailId = registerModel.EmailId,
                        FirstName = registerModel.FirstName,
                        LastName = registerModel.LastName,
                        IsActive = false,   
                        Password = EncryptPassword,
                        UserId = Guid.Empty,
                        UserName = registerModel.UserName,
                    });
                    //if (reult != null && reult.IsSuccess.HasValue && reult.IsSuccess.Value)
                    //{
                    //    CommonBusiness.SaveAuditLog(new Transport.Common.AuditLog()
                    //    {
                    //        Action = "Register",
                    //        CompanyId = SessionPersister.LoginedUser.CompanyInfo.CompanyId,
                    //        FinancialYearId = SessionPersister.LoginedUser.CompanyInfo.DefaultFiancialId,
                    //        IPAddress = SessionPersister.LoginedUser.IPAddress,
                    //        PageName = "Login",
                    //        RecordId = string.Empty,
                    //        Remark = "User registration has been successfully completed and redirect to loginin page.",
                    //        UserId = SessionPersister.LoginedUser.UserId.Value
                    //    });
                    //}
                    if (reult != null && (reult.IsSuccess.HasValue && reult.IsSuccess.Value))
                    {
                        TempData["RegistrationMesage"] = "Your registration has been successfully completed.";
                        return RedirectToAction("Index", "Login");
                    }
                    else if (reult != null)
                    {
                        ModelState.AddModelError("", reult.Response);
                    }
                    else
                    {
                        ModelState.AddModelError("", "Registration failed. Please try again..");
                    }
                }
            }
            return View(registerModel);
        }
    }
}