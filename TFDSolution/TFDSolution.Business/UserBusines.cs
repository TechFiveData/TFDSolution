using Microsoft.SqlServer.Server;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using TFDSolution.Business.Interface;
using TFDSolution.Data;
using TFDSolution.Transport;
using TFDSolution.Transport.Master;

namespace TFDSolution.Business
{
    public class UserBusines : IUserBusines
    {
        public bool UserNameExist(string username)
        {
            try
            {
                using (var content = new TFDSolutionEntities())
                {
                    return (content.m_UserMast.Where(x => x.UserName == username).FirstOrDefault() != null);
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
            }
            return false;
        }
        public int GetActiveUsers()
        {
            try
            {
                using (var content = new TFDSolutionEntities())
                {
                    if (content.m_UserMast.Where(x => x.IsActive == true).FirstOrDefault() != null)
                    {
                        return content.m_UserMast.Where(x => x.IsActive == true).ToList().Count();
                    }
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
            }
            return 0;
        }
        public ResponseModel SetUserData(UserMast user)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                using (var content = new TFDSolutionEntities())
                {
                    m_setUser_Result result = content.m_setUser
                        ("add", user.UserId, user.FirstName,
                        user.LastName, user.EmailId, user.UserName, user.Password, user.IsActive).FirstOrDefault();
                    if (result != null)
                    {
                        response.Action = result.Action;
                        response.IsSuccess = result.IsSuccess;
                        response.PrimaryId = result.PrimaryId;
                        response.Response = result.Response;
                    }
                }
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Response = "Exception: " + ex.Message;
            }
            return response;
        }
        public ResponseModel UpdateUserPassword(UserPasswordModel model)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                using (var content = new TFDSolutionEntities())
                {
                    content.Database.ExecuteSqlCommand(
                        "UPDATE m_UserMast SET Password = @password, PasswordExpiredOn = DATEADD(M,3,GETDATE()) WHERE UserId = @UserId",
                        new SqlParameter("@password", model.Password),
                        new SqlParameter("@UserId", model.UserId)
                    );
                    response.Response = "User password has been updated successfully.";
                    response.IsSuccess = true;
                }
            }
            catch (Exception)
            {
                response.Response = "Error in update user password.";
                response.IsSuccess = false;
            }
            return response;
        }

        public ResponseModel ReleaseUserLogin(string userId)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                using (var content = new TFDSolutionEntities())
                {
                    var sessionId = HttpContext.Current.Session.SessionID;
                    content.Database.ExecuteSqlCommand(
                        "UPDATE m_UserMast SET SessionId = NULL, IsLoggedIn = 0 WHERE UserId = @UserId",
                        new SqlParameter("@UserId", userId)
                    );
                    response.Response = "User session released successfully.";
                    response.IsSuccess = true;
                }
            }
            catch (Exception)
            {
                response.Response = "Error in releasing user session.";
                response.IsSuccess = false;
            }
            return response;
        }
        public LoginUserInfo UserLogin(UserMast user)
        {
            LoginUserInfo response = new LoginUserInfo();

            try
            {
                using (var content = new TFDSolutionEntities())
                {
                    //auth_getLogin_Result result = content.auth_getLogin(user.UserName, user.Password).FirstOrDefault();

                    var parameters = new List<SqlParameter>
                    {
                        new SqlParameter("@UserName", user.UserName ),
                        new SqlParameter("@Password", user.Password)
                    };
                    response = content.Database.SqlQuery<LoginUserInfo>("EXEC proc_Auth_GetLogin @UserName, @Password", parameters.ToArray()).FirstOrDefault();

                    if (response != null && response.UserId != Guid.Empty)
                    {
                        var sessionId = HttpContext.Current.Session.SessionID;
                        content.Database.ExecuteSqlCommand(
                            "UPDATE m_UserMast SET SessionId = @SessionId, IsLoggedIn = 1 WHERE UserId = @UserId",
                            new SqlParameter("@SessionId", sessionId),
                            new SqlParameter("@UserId", response.UserId)
                        );

                        IMasterBusiness iMasterBusiness = new MasterBusiness();
                        response.Forms = iMasterBusiness.getUserForms(response.UserId.Value, response.RoleID);
                        m_UserSettings userSettings = content.m_UserSettings.Where(x => x.UserId == response.UserId).FirstOrDefault();
                        if (userSettings != null)
                        {
                            m_CompanyFinancials financials = content.m_CompanyFinancials.Where(x => x.FiancialYearId == userSettings.DefaultFiancialYearId).FirstOrDefault();
                            m_CompanyMast companyMast = content.m_CompanyMast.Where(x => x.CompanyId == userSettings.CompanyId).FirstOrDefault();
                            response.CompanyInfo = new UserCompany();
                            response.CompanyInfo.DefaultFiancialId = userSettings.DefaultFiancialYearId;
                            response.CompanyInfo.CompanyId = userSettings.CompanyId;
                            if (companyMast != null)
                            {
                                response.CompanyInfo.CompanyName = companyMast.CompanyName;
                                response.CompanyInfo.CompanyCode = companyMast.CompanyCode;
                            }
                            if (financials != null)
                            {
                                response.CompanyInfo.FinancialAlias = financials.Alias;
                                response.CompanyInfo.FinancialStartDate = financials.StartDate;
                                response.CompanyInfo.FinancialEndDate = financials.EndDate;
                            }
                        }
                        if (response.CompanyInfo != null)
                        {
                            iDashboardBusiness iDashboardBusiness = new DashboardBusiness();
                            response.Dashboards = iDashboardBusiness.GetDashboard(Convert.ToString(response.UserId.Value), Convert.ToString(response.CompanyInfo.CompanyId));
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Response = "Exception: " + ex.Message;
            }
            return response;
        }

        public UserMast GetUserInfo(Guid UserId)
        {
            UserMast response = new UserMast();
            try
            {
                using (var content = new TFDSolutionEntities())
                {
                    m_UserMast result = content.m_UserMast.Where(x => x.UserId == UserId).FirstOrDefault();
                    if (result != null)
                    {
                        response.LastName = result.LastName;
                        response.FirstName = result.FirstName;
                        response.UserId = result.UserId;
                        response.EmailId = result.EmailId;
                        response.UserName = result.UserName;
                        response.IsActive = result.IsActive;
                        response.CreatedOn = result.CreatedOn;
                        response.UpdatedOn = result.UpdatedOn;
                        response.LastLoginDate = result.LastLoginDate;
                        response.PasswordExpiredOn = result.PasswordExpiredOn;
                    }
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);                
            }
            return response;
        }

        public UserMast ForgotPassword(string UserName, string EmailAddress)
        {
            UserMast response = new UserMast();
            try
            {
                using (var content = new TFDSolutionEntities())
                {
                    m_UserMast result = content.m_UserMast.Where(x => x.UserName == UserName && x.EmailId == EmailAddress).FirstOrDefault();
                    if (result != null)
                    {
                        response.LastName = result.LastName;
                        response.FirstName = result.FirstName;
                        response.UserId = result.UserId;
                        response.EmailId = result.EmailId;
                        response.UserName = result.UserName;
                        response.IsActive = result.IsActive;
                        response.CreatedOn = result.CreatedOn;
                        response.UpdatedOn = result.UpdatedOn;
                        response.LastLoginDate = result.LastLoginDate;
                        response.PasswordExpiredOn = result.PasswordExpiredOn;
                    }
                }
            }
            catch (Exception ex)
            {
                CommonBusiness.LogEx(ex);
                //response.IsSuccess = false;
                //response.Response = "Exception: " + ex.Message;
            }
            return response;
        }
    }
}