using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TFDSolution.Transport;
using TFDSolution.Transport.Master;

namespace TFDSolution.Business.Interface
{
    public interface IConfigurationBusiness
    {
        ResponseModel UpdateOtherChargesStatus(OtherChargesModel request);
        ResponseModel ReplicationOtherCharge(int chargesId, string UserId);
        List<OtherChargesModel> GetSqlTemplateDetail(string companyId);
        ResponseModel SaveOtherCharges(OtherChargesModel model);
        ResponseModel DeleteOtherCharge(int chargesId);
        OtherChargesModel GetOtherChargeDetails(int chargesId);
        OtherChargesModel GetItemOtherCharges(int chargesId, int ParentId, int DetailId, string FormId);
        Task<FormData> GetDynamicMultipleApproval(MultipleApprovalRequest request);
        ResponseModel UpdateRecordStatus(MultipleApprovalRequest request);

        #region Email Templates
        List<EmailTemplateModel> getEmailTamplates();
        ResponseModel SaveEmailTamplate(EmailTemplateModel model);
        EmailTemplateModel getEmailTamplateById(int templateId);
        EmailTemplateModel getEmailTamplateByForm(string formId, int AccountId, int ParentId);
        #endregion

        List<DashboardDetailSettingModel> GetDashboardDetailSettings(int dashboardId);
        List<DashboardSettingModel> getDashboardSettings();
        ResponseModel SaveDashboardSetting(DashboardSettingModel model);
        ResponseModel DeleteDashboardDetailSetting(int Id);
        ResponseModel SaveDashboardDetailSetting(DashboardDetailSettingModel model);
        DashboardSettingModel getDashboardSettingById(int dashboardId);
        List<FormModel> getFormUserApproval(FormModel request);
        List<FormUserApprovalModel> getFormUserApprovalSettings(string formId);
        ResponseModel DeleteDashboardSetting(int Id);
        DashboardDetailSettingModel GetDashboardDetail(int Id);
        ResponseModel SaveUserApprovalSettings(UserApprovalSettingModel model);
    }
}
