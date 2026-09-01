using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Web.UI.WebControls;
using TFDSolution.App_Start;
using TFDSolution.Business;
using TFDSolution.Business.Interface;
using TFDSolution.Common;
using TFDSolution.Models;
using TFDSolution.Transport;
using TFDSolution.Transport.Common;
using TFDSolution.Transport.Master;

namespace TFDSolution.Controllers
{
    [CustomAuthenticationFilter]
    public class MasterController : BaseController
    {
        public readonly Business.Interface.IMasterBusiness master;
        public readonly Business.Interface.IUserBusines userBusiness;

        public MasterController()
        {
            master = new MasterBusiness();
            userBusiness = new UserBusines();
        }
        // GET: Master
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult getSelection(string _type, string _whareClause, string _orderType, string _entryType)
        {
            string companyId = Convert.ToString(SessionPersister.LoginedUser.CompanyInfo.CompanyId);
            List<clsSelection> clsSelections = master.getSelection(_type, companyId, _whareClause, _orderType, _entryType);
            return Json(clsSelections, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Company()
        {
            return View();
        }

        public JsonResult getCompanyList()
        {
            List<CompanyMast> company = master.GetCompanies();
            return Json(company, JsonRequestBehavior.AllowGet);
        }

        public ActionResult AddCompany(CompanyMast modal)
        {
            modal.UserId = SessionPersister.LoginedUser.UserId.Value;
            ResponseModel response = master.AddCompany(modal);
            return Json(response, JsonRequestBehavior.AllowGet);
        }
        public ActionResult CompanyAction(string uId, string act)
        {

            CompanyMast company = new CompanyMast();
            if (!string.IsNullOrEmpty(uId) && uId == "-1")
            {
                uId = string.Empty;
            }
            if (string.IsNullOrEmpty(act) && !string.IsNullOrEmpty(uId))
            {
                act = "E";
            }
            else if (string.IsNullOrEmpty(act) && string.IsNullOrEmpty(uId))
            {
                act = "A";
            }
            if (string.IsNullOrEmpty(act) || (!string.IsNullOrEmpty(act) && (act.ToUpper() != "E" && act.ToUpper() != "A")))
            {
                act = "V";
            }
            if (!string.IsNullOrEmpty(uId))
            {
                company = master.GetCompany(uId);
            }
            string companyId = Convert.ToString(SessionPersister.LoginedUser.CompanyInfo.CompanyId);
            company.PageAction = act.ToUpper();
            ViewBag.SectionList = master.getSelection("FormSection", companyId);
            ViewBag.CityList = master.getSelection("City", companyId);
            ViewBag.StateList = master.getSelection("State", companyId);
            ViewBag.CountryList = master.getSelection("Country", companyId);
            ViewBag.CurrencyList = master.getSelection("Currency", companyId);
            if (TempData["ErrorMessages"] != null)
            {
                ViewBag.ErrorMessages = TempData["ErrorMessages"];
            }
            if (TempData["SuccessMessage"] != null)
            {
                ViewBag.SuccessMessage = TempData["SuccessMessage"];
            }
            return View(company);
        }
        [HttpPost]
        public ActionResult CompanyAction(CompanyMast modal)
        {
            string errorMessage = "";
            ResponseModel response = new ResponseModel();
            if (!string.IsNullOrEmpty(modal.CompanyName) && !string.IsNullOrEmpty(modal.Phone) && !string.IsNullOrEmpty(modal.Mobile))
            {
                modal.UserId = SessionPersister.LoginedUser.UserId.Value;
                response = master.SaveCompany(modal);
                if (response.IsSuccess.HasValue && !response.IsSuccess.Value)
                {
                    errorMessage = response.Response;
                }
                else if (response.IsSuccess.HasValue && response.IsSuccess.Value)
                {
                    TempData["SuccessMessage"] = response.Response;
                    return RedirectToAction("CompanyAction", new { @uId = modal.CompanyId, act = "V" });
                    //ViewBag.SuccessMessage = response.Response;
                }
                else if (response.IsSuccess.HasValue == false)
                {
                    errorMessage = "Record is failed to save.";
                }
            }
            else
            {
                errorMessage = "Please supplier value into required fields";

            }
            TempData["ErrorMessages"] = errorMessage;
            return RedirectToAction("CompanyAction", new { @uId = modal.CompanyId, act = "E" });
        }

        public ActionResult getFinancialYears(string uId)
        {
            List<CompanyFinancial> company = master.GetCompanyFinancial(uId);
            return Json(company, JsonRequestBehavior.AllowGet);
        }
        public ActionResult SaveCompanyFinancial(CompanyFinancial modal)
        {
            modal.UserId = SessionPersister.LoginedUser.UserId.Value;
            ResponseModel response = master.SaveCompanyFinancial(modal);
            return Json(response, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult DeleteCompany(string uId)
        {
            ResponseModel response = master.DeleteCompany(Guid.Parse(uId));
            return Json(response, JsonRequestBehavior.AllowGet);

        }
        public ActionResult Item()
        {
            return View();
        }
        public JsonResult getItemList()
        {
            List<ItemMast> item = master.GetItemList();
            return Json(item, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ItemAction(string uId)
        {
            string companyId = Convert.ToString(SessionPersister.LoginedUser.CompanyInfo.CompanyId);
            ItemMast itemMst = new Transport.Master.ItemMast();
            ViewBag.ItemGroup = master.getSelection("Parameter", companyId, "", "", "ItemGroup");
            ViewBag.ItemUOM = master.getSelection("Parameter", companyId, "", "", "ItemUOM");
            ViewBag.ItemCategory = master.getSelection("Parameter", companyId, "", "", "ItemCategory");
            ViewBag.ItemType = master.getSelection("Parameter", companyId, "", "", "ItemType");
            ViewBag.PackingType = master.getSelection("Parameter", companyId, "", "", "PackingType");
            ViewBag.ValuationMethod = master.getSelection("Parameter", companyId, "", "", "ValuationMethod");
            ViewBag.HSNCode = master.getSelection("Parameter", companyId, "", "", "HSNCode");
            ViewBag.TaxPercentage = master.getSelection("Parameter", companyId, "", "", "TaxPercentage");
            if (!string.IsNullOrEmpty(uId))
            {
                itemMst = master.GetItem(uId);
                ViewBag.ShowDetail = true; // Set a flag to make the div visible
            }
            else
            {
                itemMst.ItemCode = CommonBusiness.getNextCode("m_ItemMast");
            }
            return View(itemMst);
        }

        [HttpPost]
        public ActionResult ItemAction(ItemMast modal)
        {
            ResponseModel response = new ResponseModel();
            if (ModelState.IsValid)
            {
                modal.UserId = SessionPersister.LoginedUser.UserId.Value;
                response = master.SaveItem(modal);
                if (response.IsSuccess.HasValue && !response.IsSuccess.Value)
                {
                    ViewBag.ErrorMessages = response.Response;
                }
                else if (response.IsSuccess.HasValue && response.IsSuccess.Value)
                {
                    ViewBag.SuccessMessage = response.Response;
                    ViewBag.ShowDetail = true; // Set a flag to make the div visible
                    if (response.PrimaryId != null && response.PrimaryId != Guid.Empty)
                    {
                        modal.ItemId = response.PrimaryId.Value;
                        //return RedirectToAction("ItemAction", new { uId = modal.ItemId }); // Redirecting with new ID
                    }
                }
                else if (response.IsSuccess.HasValue == false)
                {
                    ViewBag.ErrorMessages = "Record is failed to save.";
                }
            }
            else
            {
                ViewBag.ErrorMessages = "Please supplier value into required fields";
            }
            string companyId = Convert.ToString(SessionPersister.LoginedUser.CompanyInfo.CompanyId);
            ViewBag.ItemGroup = master.getSelection("Parameter", companyId, "", "", "ItemGroup");
            ViewBag.ItemUOM = master.getSelection("Parameter", companyId, "", "", "ItemUOM");
            ViewBag.ItemCategory = master.getSelection("Parameter", companyId, "", "", "ItemCategory");
            ViewBag.ItemType = master.getSelection("Parameter", companyId, "", "", "ItemType");
            ViewBag.PackingType = master.getSelection("Parameter", companyId, "", "", "PackingType");
            ViewBag.ValuationMethod = master.getSelection("Para meter", companyId, "", "", "ValuationMethod");
            ViewBag.HSNCode = master.getSelection("Para meter", companyId, "", "", "HSNCode");
            ViewBag.TaxPercentage = master.getSelection("Para meter", companyId, "", "", "TaxPercentage");
            return View(modal);
        }
        [HttpPost]
        public ActionResult DeleteItem(string uId)
        {
            ResponseModel response = master.DeleteItem(Guid.Parse(uId));
            return Json(response, JsonRequestBehavior.AllowGet);

        }
        #region "Party"
        public ActionResult PartyMaster()
        {
            return View();
        }
        public JsonResult getPartyList()
        {
            List<PartyMst> item = master.GetPartyList();
            return Json(item, JsonRequestBehavior.AllowGet);
        }
        public ActionResult PartyAction(string uId)
        {
            string companyId = Convert.ToString(SessionPersister.LoginedUser.CompanyInfo.CompanyId);
            PartyMst partyMst = new PartyMst();
            ViewBag.PartyType = master.getSelection("Parameter", companyId, "", "", "PartyType");
            ViewBag.PartyGroup = master.getSelection("Parameter", companyId, "", "", "PartyGroup");
            ViewBag.Industry = master.getSelection("Parameter", companyId, "", "", "PartyType");
            ViewBag.SalesPerson = master.getSelection("Parameter", companyId, "", "", "PartyType");
            ViewBag.GSTType = master.getSelection("Parameter", companyId, "", "", "GSTType");
            ViewBag.CityList = master.getSelection("City", companyId);
            ViewBag.StateList = master.getSelection("State", companyId);
            ViewBag.CountryList = master.getSelection("Country", companyId);
            ViewBag.Currency = master.getSelection("Currency", companyId);

            if (!string.IsNullOrEmpty(uId))
            {
                partyMst = master.GetParty(uId);
                ViewBag.ShowDetail = true; // Set a flag to make the div visible
            }
            else
            {
                partyMst.PartyCode = CommonBusiness.getNextCode("m_PartyMast");
            }

            return View(partyMst);
        }
        [HttpPost]
        public ActionResult PartyAction(PartyMst modal)
        {
            string companyId = Convert.ToString(SessionPersister.LoginedUser.CompanyInfo.CompanyId);
            ResponseModel response = new ResponseModel();
            if (ModelState.IsValid)
            {
                modal.UserId = SessionPersister.LoginedUser.UserId.Value;
                response = master.SaveParty(modal);
                if (response.IsSuccess.HasValue && !response.IsSuccess.Value)
                {
                    ViewBag.ErrorMessages = response.Response;
                }
                else if (response.IsSuccess.HasValue && response.IsSuccess.Value)
                {
                    ViewBag.SuccessMessage = response.Response;
                    ViewBag.ShowDetail = true; // Set a flag to make the div visible
                    if (response.PrimaryId != null && response.PrimaryId != Guid.Empty)
                    {
                        modal.PartyId = response.PrimaryId.Value;
                        //return RedirectToAction("ItemAction", new { uId = modal.ItemId }); // Redirecting with new ID
                    }
                }
                else if (response.IsSuccess.HasValue == false)
                {
                    ViewBag.ErrorMessages = "Record is failed to save.";
                }
            }
            else
            {
                ViewBag.ErrorMessages = "Please supplier value into required fields";
            }
            ViewBag.PartyType = master.getSelection("Parameter", companyId, "", "", "PartyType");
            ViewBag.PartyGroup = master.getSelection("Parameter", companyId, "", "", "PartyGroup");
            ViewBag.Industry = master.getSelection("Parameter", companyId, "", "", "PartyType");
            ViewBag.SalesPerson = master.getSelection("Parameter", companyId, "", "", "PartyType");
            ViewBag.GSTType = master.getSelection("Parameter", companyId, "", "", "GSTType");
            ViewBag.CityList = master.getSelection("City", companyId);
            ViewBag.StateList = master.getSelection("State", companyId);
            ViewBag.CountryList = master.getSelection("Country", companyId);
            ViewBag.Currency = master.getSelection("Currency", companyId);
            return View(modal);
        }

        [HttpPost]
        public ActionResult DeleteParty(string uId)
        {
            ResponseModel response = master.DeleteParty(Guid.Parse(uId));
            return Json(response, JsonRequestBehavior.AllowGet);

        }
        #endregion "Party"

        #region "User"
        public ActionResult UserMaster()
        {
            IUserBusines iUser = new UserBusines();
            int ActiveUsers = iUser.GetActiveUsers();
            TFDSolution.Models.LicenseModel model = ValidationKey.IsValid();
            if (model.MaximumUser <= ActiveUsers)
            {
                ViewBag.UserLimitsMessage = "You've exceeded the user limits as per your current plan.";
            }
            return View();
        }
        public JsonResult getUserList()
        {
            List<UserMast> item = master.GetUserList();
            return Json(item, JsonRequestBehavior.AllowGet);
        }
        public ActionResult UserAction(string uId)
        {
            UserDetail userDetail = new UserDetail();
            try
            {
                // Fetch user details using the GetUser method
                if (!string.IsNullOrEmpty(uId))
                {
                    userDetail = master.GetUser(uId);
                    if (userDetail != null)
                    {
                        ViewBag.ShowDetail = true; // Set a flag to make the div visible
                    }
                    else
                    {
                        ViewBag.ErrorMessages = "User not found.";
                    }
                }
                else
                {
                    IUserBusines iUser = new UserBusines();
                    int ActiveUsers = iUser.GetActiveUsers();
                    TFDSolution.Models.LicenseModel licenseModel = ValidationKey.IsValid();
                    if (licenseModel.MaximumUser <= ActiveUsers)
                    {
                        ViewBag.ErrorMessages = "You've exceeded the user limits as per your current plan.";
                    }
                    //  ViewBag.ErrorMessages = "Invalid User ID.";
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
                // Log the exception (if logging is implemented)
                ViewBag.ErrorMessages = "An error occurred while fetching user details.";
            }
            string companyId = Convert.ToString(SessionPersister.LoginedUser.CompanyInfo.CompanyId);
            // Populate dropdowns or other data required for the view
            ViewBag.CompanyList = master.getSelection("Company", companyId);
            //ViewBag.FinancialYearList = master.getSelection("FinancialYear");
            ViewBag.UserRoleList = master.getSelection("UserRole", companyId);
            ViewBag.CityList = master.getSelection("City", companyId);
            ViewBag.StateList = master.getSelection("State", companyId);
            ViewBag.CountryList = master.getSelection("Country", companyId);
            ViewBag.DashboardIdList = master.getSelection("Dashboard", companyId, Convert.ToString(SessionPersister.LoginedUser.UserId.Value));
            //// Pre-select companies if editing
            // if (userDetail.SelectedCompanyIds != null)
            // {
            //     ViewBag.SelectedCompanies = userDetail.SelectedCompanyIds;
            // }
            if (userDetail != null && !string.IsNullOrEmpty(userDetail.Password))
            {
                userDetail.Password = AesOperation.DecryptString(userDetail.Password);
            }
            return View(userDetail);
        }
        [HttpPost]
        public ActionResult UserAction(UserDetail model)
        {
            string companyId = Convert.ToString(SessionPersister.LoginedUser.CompanyInfo.CompanyId);
            IUserBusines iUser = new UserBusines();
            int ActiveUsers = iUser.GetActiveUsers();
            TFDSolution.Models.LicenseModel licenseModel = ValidationKey.IsValid();
            if (licenseModel.MaximumUser <= ActiveUsers)
            {
                ViewBag.ErrorMessages = "You've exceeded the user limits as per your current plan.";
            }
            else
            {
                ResponseModel response = new ResponseModel();
                if (ModelState.IsValid)
                {
                    //model.UserId = SessionPersister.LoginedUser.UserId.Value;
                    // Save the selected companies (model.SelectedCompanyIds)
                    response = master.SaveUserWithCompanies(model);

                    if (response.IsSuccess.HasValue && response.IsSuccess.Value)
                    {
                        ViewBag.SuccessMessage = response.Response;
                    }
                    else
                    {
                        ViewBag.ErrorMessages = response.Response ?? "Record failed to save.";
                    }
                }
                else
                {
                    ViewBag.ErrorMessages = "Please provide values for required fields.";
                }
            }
            // Populate dropdowns or other data required for the view
            ViewBag.CompanyList = master.getSelection("Company", companyId);
            // ViewBag.FinancialYearList = master.getSelection("FinancialYear");
            ViewBag.UserRoleList = master.getSelection("UserRole", companyId);
            ViewBag.CityList = master.getSelection("City", companyId);
            ViewBag.StateList = master.getSelection("State", companyId);
            ViewBag.CountryList = master.getSelection("Country", companyId);
            ViewBag.DashboardIdList = master.getSelection("Dashboard", companyId, Convert.ToString(SessionPersister.LoginedUser.UserId.Value));
            return View(model);

        }

        public ActionResult UserUpdatePassword(UserPasswordModel model)
        {
            string EncryptPassword = AesOperation.EncryptString(model.NewPassword);
            model.Password = EncryptPassword;
            ResponseModel response = userBusiness.UpdateUserPassword(model);
            return Json(response, JsonRequestBehavior.AllowGet);
        }
        public ActionResult ReleaseUserLogin(string uId)
        {
            ResponseModel response = userBusiness.ReleaseUserLogin(uId);
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult DeleteUser(string uId)
        {
            ResponseModel response = master.DeleteUser(Guid.Parse(uId));
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult UploadProfilePic(HttpPostedFileBase ProfilePic, string username)
        {
            if (ProfilePic == null || string.IsNullOrEmpty(username))
                return Json(new { success = false, message = "No file or username." });

            if (ProfilePic.ContentLength > 5 * 1024 * 1024)
                return Json(new { success = false, message = "File size exceeds 5MB." });

            var ext = System.IO.Path.GetExtension(ProfilePic.FileName).ToLower();
            if (ext != ".jpg" && ext != ".jpeg" && ext != ".png" && ext != ".gif")
                return Json(new { success = false, message = "Only image files allowed." });

            var dir = Server.MapPath("~/Images/Profile/");
            if (!System.IO.Directory.Exists(dir))
                System.IO.Directory.CreateDirectory(dir);

            var filePath = System.IO.Path.Combine(dir, username + ".png");
            ProfilePic.SaveAs(filePath);

            return Json(new { success = true });
        }

        public FileResult GetProfilePic(string username)
        {
            var filePath = Server.MapPath("~/Images/Profile/" + username + ".png");
            if (System.IO.File.Exists(filePath))
                return File(filePath, "image/png");
            else
                return File(Server.MapPath("~/Images/NoImage.png"), "image/png");
        }
        #endregion "User"

        #region "Role"
        public ActionResult RoleMaster()
        {
            return View();
        }
        public JsonResult getRoleList()
        {
            List<RoleMast> item = master.GetRoleList();
            return Json(item, JsonRequestBehavior.AllowGet);
        }

        public ActionResult RolePermissionAction(string RoleId)
        {
            int _RoleId = string.IsNullOrEmpty(RoleId) ? 0 : Convert.ToInt32(RoleId);
            RoleDetail roleDetail = new RoleDetail();
            try
            {
                // Fetch user details using the GetUser method
                //if (_RoleId > 0)
                //{
                roleDetail = master.GetRolePermissionDetails(_RoleId);
                if (roleDetail != null)
                {
                    ViewBag.ShowDetail = true; // Set a flag to make the div visible
                }
                else
                {
                    ViewBag.ErrorMessages = "Role not found.";
                }
                //}
                //else
                //{
                //    // ViewBag.ErrorMessages = "Invalid Role ID.";
                //}
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
                // Log the exception (if logging is implemented)
                ViewBag.ErrorMessages = "An error occurred while fetching Role details.";
            }

            //ViewBag.RolePermission = roleDetail;
            return View(roleDetail);
        }

        [HttpPost]
        public ActionResult RolePermissionAction(RoleDetail model)
        {
            ResponseModel response = new ResponseModel();

            model.UserId = SessionPersister.LoginedUser.UserId.Value;

            // Save the selected companies (model.SelectedCompanyIds)
            response = master.SaveRolePermissionsRoleWise(model);

            if (response.IsSuccess.HasValue && response.IsSuccess.Value)
            {
                ViewBag.SuccessMessage = response.Response;
            }
            else
            {
                ViewBag.ErrorMessages = response.Response ?? "Record failed to save.";
            }

            return View(model);
        }
        [HttpPost]
        public ActionResult DeleteRole(string RoleId)
        {
            ResponseModel response = master.DeleteRole(Convert.ToInt32(RoleId));
            return Json(response, JsonRequestBehavior.AllowGet);

        }
        #endregion "Role"

        #region "Form Flow"        
        public ActionResult FormFlowSetting()
        {
            FormMast model = new FormMast();

            try
            {
                var forms = master.getForms(null);
                if (forms != null && forms.Any())
                {
                    // Get only parent forms (ParentFormId == null)
                    model.ChildForm = forms.Where(f => f.ParentFormId == null).ToList();
                    foreach (var parent in model.ChildForm)
                    {
                        parent.ChildForm = forms.Where(f => f.ParentFormId == parent.FormId).ToList();
                    }
                    ViewBag.ShowDetail = true; // Set a flag to make the div visible
                }
                else
                {
                    ViewBag.ErrorMessages = "Forms not found.";
                }

            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
                // Log the exception (if logging is implemented)
                ViewBag.ErrorMessages = "An error occurred while fetching Forms details.";
            }

            //ViewBag.RolePermission = roleDetail;
            return View(model);
        }

        [HttpPost]
        public ActionResult UpdateSortOrder(SortOrderUpdateRequest updatedRows)
        {
            ResponseModel response = new ResponseModel();
            // Call your business layer to update the sort order in DB
            var business = new MasterBusiness();
            if (updatedRows == null || updatedRows.Rows == null || !updatedRows.Rows.Any())
            {
                return Json(new { success = false, message = "No rows to update." });
            }
            response = master.UpdateFormSortOrder(updatedRows.Rows);
            if (response.IsSuccess.HasValue && response.IsSuccess.Value)
            {
                ViewBag.SuccessMessage = response.Response;
                return Json(new { success = response.IsSuccess.Value });
            }
            else
            {
                ViewBag.ErrorMessages = response.Response ?? "Record failed to save.";
                return Json(new { success = false });
            }
        }

        public ActionResult UpdateFormCaption(FormDataModel updateForm)
        {
            ResponseModel response = master.SaveFormCaption(updateForm);
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        #endregion "Form Flow"

        #region Bank Details 
        public ActionResult getBankDetails(string uId)
        {
            List<CompanyBank> company = master.GetBankDetails(uId);
            return Json(company, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult SaveBankDetails(CompanyBank modal)
        {
            modal.CreatedBy = SessionPersister.LoginedUser.UserId.Value;
            modal.UpdatedBy = SessionPersister.LoginedUser.UserId.Value;
            ResponseModel response = master.SavebankDetails(modal);
            return Json(response, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public ActionResult getBankDetailsForEdit(Guid BankId)
        {
            CompanyBank response = master.getBankDetailsForEdit(BankId);
            return Json(response, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public ActionResult DeleteCompanyBankDetails(Guid BankId)
        {
            ResponseModel response = master.DeleteCompanyBankDetails(BankId);
            return Json(response, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Unit Details 
        public ActionResult getUnitDetails(string uId)
        {
            List<CompanyUnit> company = master.GetUnitDetails(uId);
            return Json(company, JsonRequestBehavior.AllowGet);
        }
        public ActionResult SaveUnitDetails(CompanyUnit modal)
        {
            modal.CreatedBy = SessionPersister.LoginedUser.UserId.Value;
            modal.UpdatedBy = SessionPersister.LoginedUser.UserId.Value;
            ResponseModel response = master.SaveUnitDetails(modal);
            return Json(response, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public ActionResult getUnitDetailsForEdit(Guid CompanyUnitId)
        {
            CompanyUnit response = master.getUnitDetailsForEdit(CompanyUnitId);
            return Json(response, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public ActionResult DeleteCompanyUnitDetails(Guid CompanyUnitId)
        {
            ResponseModel response = master.DeleteCompanyUnitDetails(CompanyUnitId);
            return Json(response, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region User Notification
        public async Task<JsonResult> getUserNotifications()
        {
            string userId = SessionPersister.LoginedUser.UserId.Value.ToString();

            // Await the task
            List<UserNotificationModel> userNotifications = await master.getUserNotification(userId, 5);

            return Json(userNotifications, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region User Name
        public async Task<JsonResult> getUsersForAuditLog()
        
        {
            AuditLogUserNameList response = new AuditLogUserNameList();
            string userId = SessionPersister.LoginedUser.UserId.Value.ToString();
            int roleId = SessionPersister.LoginedUser.RoleID;

            // Await the task
            response.Data = await master.getUsersForAuditLog(userId, roleId);
            response.RoleId = SessionPersister.LoginedUser.RoleID;
            return Json(response, JsonRequestBehavior.AllowGet);
        }
        public async Task<JsonResult> getUsersAuditLogList(string userId, string fromDate, string toDate)
        {
            string companyId = SessionPersister.LoginedUser.CompanyInfo.CompanyId.ToString();
            // Await the task
            List<UserAuditLogList> userNotifications = await master.getUsersAuditLogList(userId, fromDate, toDate, companyId);

            return Json(userNotifications, JsonRequestBehavior.AllowGet);
        }
        #endregion


    }

}