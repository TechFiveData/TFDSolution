using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Security;

namespace TFDSolution.Transport.Master
{
    public class UserMast
    {
        public System.Guid UserId { get; set; }
        [Required]
        [MaxLength(50, ErrorMessage = "Length cannot exceed 50 Character.")]
        public string FirstName { get; set; }
        [Required]
        [MaxLength(50, ErrorMessage = "Length cannot exceed 50 Character.")]
        public string LastName { get; set; }
        [Required]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = "Invalid email format.")]
        [MaxLength(50, ErrorMessage = "Length cannot exceed 50 Character.")]
        public string EmailId { get; set; }
        [Required]
        [MaxLength(50, ErrorMessage = "Length cannot exceed 50 Character.")]
        //[RegularExpression(@"^[A-Za-z._]{2,50}$", ErrorMessage = "Enter valid User Name.")]
        public string UserName { get; set; }
        public string Password { get; set; }
        [Required]
        public bool IsActive { get; set; }

        public  int  Status{ get; set; } = 0; 
        public System.DateTime CreatedOn { get; set; }
        public Nullable<System.DateTime> UpdatedOn { get; set; }
        public Nullable<System.DateTime> LastLoginDate { get; set; }
        public Nullable<System.DateTime> PasswordExpiredOn { get; set; }
        [Required]
        public int UserRole { get; set; }
        public string RoleName { get; set; }
        public int SrNo { get; set; }
        public bool? IsLoggedIn { get; set; }
        public string SessionId { get; set; }

    }

    public class UserDetail : UserMast
    {
        [Required]
        [RegularExpression(@"^[6-9]\d{9}$", ErrorMessage = "Invalid mobile number format.")]
        [MaxLength(10, ErrorMessage = "Length cannot exceed 10 digits.")]
        public string Mobile { get; set; }

        public int TemplateId { get; set; }
      
        [MaxLength(200, ErrorMessage = "Length cannot exceed 200 Character.")]
        public string Address1 { get; set; }
        [MaxLength(200, ErrorMessage = "Length cannot exceed 200 Character.")]
        public string Address2 { get; set; }
        public int? City { get; set; }
        public int? State { get; set; }
        public int? Country { get; set; }
        public string ZipCode { get; set; }
        [RegularExpression(@"^[1-9][0-9]{5}$", ErrorMessage = "Invalid PIN Code.")]
        [MaxLength(6, ErrorMessage = "Length cannot exceed 6 digits.")]
        public string Pincode { get; set; }
        [RegularExpression(@"^[6-9]\d{9}$", ErrorMessage = "Invalid mobile number format.")]
        [MaxLength(10, ErrorMessage = "Length cannot exceed 10 digits.")]
        public string ContactNo { get; set; }
        [MaxLength(50, ErrorMessage = "Length cannot exceed 50 Character.")]
        
        public string ContactPerson { get; set; }
        
        [MaxLength(50, ErrorMessage = "Length cannot exceed 50 Character.")]
        public string ContactEmail { get; set; }
        public List<UserCompany> UserCompanies { get; set; } // Updated to use UserCompany
        public string FromEmail { get; set; }
        public string FromPassword { get; set; }
        public string DisplayName { get; set; }
        public string SmtpServer { get; set; }
        public string SmtpPort { get; set; }
        public bool EnableSSL { get; set; }
        //public int FinancialYearId { get; set; }
        public bool? IsLoggedIn { get; set; }
        public string SessionId { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmPassword { get; set; }

        public int? DashboardId { get; set; }
    }

    public class LoginUserInfo
    {
        public Nullable<System.Guid> UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string EmailId { get; set; }
        public string IPAddress { get; set; }
        public string UserName { get; set; }
        public Nullable<bool> IsActive { get; set; }
        public System.DateTime CreatedOn { get; set; }
        public Nullable<System.DateTime> UpdatedOn { get; set; }
        public Nullable<System.DateTime> LastLoginDate { get; set; }
        public Nullable<System.DateTime> PasswordExpiredOn { get; set; }
        public string Response { get; set; }
        public Nullable<bool> IsSuccess { get; set; }
        public UserCompany CompanyInfo { get; set; }
        public List<FormMast> Forms { get; set; }
        public int RoleID  {get;set;}
        public List<DashboardModel> Dashboards { get; set; }
        public bool? IsLoggedIn { get; set; }
        public string SessionId { get; set; }
        public int DefaultDashboardId { get; set; }
    }

    public class UserCompany
    {
        public System.Guid CompanyId { get; set; }
        public int DefaultFiancialId { get; set; }
        public string CompanyCode { get; set; }
        public string CompanyName { get; set; }
        public string FinancialAlias { get; set; }
        public System.DateTime FinancialStartDate { get; set; }
        public System.DateTime FinancialEndDate { get; set; }
    }

    public class UserPasswordModel
    {
        public string UserId { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmPassword { get; set; }
        public string Password { get; set; }
    }

    public class UserNotificationModel
    {
        public string Title { get; set; }
        public string Message { get; set; }
        public string Type { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string LinkUrl { get; set; }
        public bool IsSystemGenerated { get; set; }
        public string URNNo { get; set; }
        public string DocNo { get; set; }
        public string FormId { get; set; }
        public string FormTitle { get; set; }

        public int UserNotificationId { get; set; }
        public int NotificationId { get; set; }
        public string UserId { get; set; }
        public Nullable<bool> IsRead { get; set; }
        public Nullable<DateTime> ReadDate { get; set; }
        public Nullable<DateTime> AssignedDate { get; set; }
        public int UnReadCount { get; set; }
        public int Status { get; set; }
        public int Id { get; set; }
    }
    public class AuditLogUserNameData
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
    }
    public class AuditLogUserNameList
    {
        public List<AuditLogUserNameData> Data { get; set; }
        public int RoleId { get; set; }
    }
    public class UserAuditLogList
    {
        public int SrNo { get; set; }
        public string UserName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string EmailId { get; set; }
        public Nullable<System.DateTime> LastLoginDate { get; set; }
        public Nullable<System.DateTime> LogDateTime { get; set; }
        public string PageName { get; set; }
        public string Action { get; set; }
        public string IPAddress { get; set; }
        public string Remark { get; set; }
        public string URNNo { get; set; }
        public string PageURL { get; set; }
    }

}
