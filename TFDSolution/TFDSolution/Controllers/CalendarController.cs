using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TFDSolution.App_Start;
using System.Web.Mvc;
using TFDSolution.Business;
using TFDSolution.Business.Interface;
using TFDSolution.Transport.Common;
using TFDSolution.Transport;
using System.Web.Services.Configuration;
using System.Reflection;
using TFDSolution.Common;
using TFDSolution.Common.Enums;
using System.Globalization;


namespace TFDSolution.Controllers
{
    [CustomAuthenticationFilter]
    public class CalendarController : Controller
    {
        private readonly ITransactionBusiness transactionBusiness;
        public readonly Business.Interface.IMasterBusiness master;
        public CalendarController()
        {
            transactionBusiness = new TransactionBusiness();
            master = new MasterBusiness();
        }

        #region "Common Methods"
        public JsonResult GetAssignedToUsers()
        {
            string companyId = Convert.ToString(SessionPersister.LoginedUser.CompanyInfo.CompanyId);
            var list = master.getSelection("UserList", companyId);

            return Json(list, JsonRequestBehavior.AllowGet);
        }
        public JsonResult GetCoAgents()
        {
            string companyId = Convert.ToString(SessionPersister.LoginedUser.CompanyInfo.CompanyId);
            var list = master.getSelection("CoAgentList", companyId);

            return Json(list, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetEventPriorities()
        {
            var list = Enum.GetValues(typeof(TFDSolution.Common.Enums.Enums.EventPriority))
                .Cast<TFDSolution.Common.Enums.Enums.EventPriority>()
                .Select(e => new clsSelection
                {
                    Text = e.ToString(),
                    Value = ((int)e).ToString()
                })
                .ToList();

            return Json(list, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetFollowupStatusList()
        {
            var list = Enum.GetValues(typeof(TFDSolution.Common.Enums.Enums.FollowupStatus))
                .Cast<TFDSolution.Common.Enums.Enums.FollowupStatus>()
                .Select(e => new clsSelection
                {
                    Text = e.ToString(),
                    Value = ((int)e).ToString()
                })
                .ToList();

            return Json(list, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetCommunicationModeList()
        {
            var list = Enum.GetValues(typeof(TFDSolution.Common.Enums.Enums.CommunicationModes))
                .Cast<TFDSolution.Common.Enums.Enums.CommunicationModes>()
                .Select(e => new clsSelection
                {
                    Text = CommonHelper.GetEnumDisplayName(e),  // ✅ Get display name
                    Value = ((int)e).ToString()
                })
                .ToList();

            return Json(list, JsonRequestBehavior.AllowGet);
        }
        #endregion "Common Methods"

        #region "Event Methods"
        [HttpPost]
        public JsonResult SaveEvent(FollowUpEventModel evt)
        {
            ResponseModel response = transactionBusiness.SaveEvent(evt);
            return Json(response, JsonRequestBehavior.AllowGet);
        }
        public JsonResult SaveFollowUpDate(int followUpId, DateTime followUpDate)
        {
            try
            {
                ResponseModel response = new ResponseModel();
                if (followUpId <= 0)
                {
                    response.IsSuccess = false;
                    response.Response = "Invalid FollowUp ID.";
                    return Json(response, JsonRequestBehavior.AllowGet);
                }
                if (followUpDate == default(DateTime))
                {
                    response.IsSuccess = false;
                    response.Response = "Invalid FollowUp Date.";
                    return Json(response, JsonRequestBehavior.AllowGet);
                }
                response = transactionBusiness.SaveFollowUpDateTime(followUpId, followUpDate, Convert.ToString(SessionPersister.LoginedUser.UserId.Value));
                return Json(response, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        public JsonResult SaveNextFollowUpEvent(FollowUpEventModel evt)
        {
            evt.CreatedBy = SessionPersister.LoginedUser.UserId.Value;
            ResponseModel response = transactionBusiness.SaveNextFollowUpEvent(evt);
            return Json(response, JsonRequestBehavior.AllowGet);
        }
        public JsonResult SaveTRNFollowUp(FollowUpEventModel evt)
        {
            evt.CreatedBy = SessionPersister.LoginedUser.UserId.Value;
            ResponseModel response = transactionBusiness.SaveTRNFollowUp(evt);
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        public JsonResult GetEvents()
        {
            var events = transactionBusiness.GetAllEvents().Select(e => new
            {

                title = e.FollowUpCount > 0 ? e.Title + " (" + Convert.ToString(e.FollowUpCount) + ")" : e.Title,
                start = e.FollowUpDate.ToString("yyyy-MM-ddTHH:mm:ss"),
                url = e.Url,
                extendedProps = new
                {
                    popuptitle = e.Title,
                    followUpId = e.FollowUpId,
                    followUpDate = e.FollowUpDate.ToString("dd/MM/yyyy HH:mm tt", CultureInfo.InvariantCulture),
                    followupCount = e.FollowUpCount,
                    nextFollowUpID = e.NextFollowUpID,
                    priority = e.EventPriority,
                    assignedTo = e.AssignedTo, // e.g. Lead/Client/Task
                    interaction = e.Interaction,
                    outcome = e.Response,
                    coAgent = e.CoAgent,
                    status = e.Status,
                    urn = e.Form_URNNo,
                    nextFollowUpDate = e.NextFollowUpDate?.ToString("dd/MM/yyyy hh:mm tt", CultureInfo.InvariantCulture) ?? null,
                    contactPerson = e.ContactPerson,
                    modeOfCommunication = e.ModeOfCommunication,
                    contactNumber = e.ContactNumber
                }
            });

            return Json(events, JsonRequestBehavior.AllowGet);

        }
        public JsonResult GetFollowUpById(int id)
        {
            var record = transactionBusiness.GetAllEvents(id).FirstOrDefault();
            if (record == null)
                return Json(null, JsonRequestBehavior.AllowGet);

            return Json(new
            {
                followUpId = record.FollowUpId,
                assignedTo = record.AssignedTo,
                followUpDate = record.FollowUpDate.ToString("dd/MM/yyyy HH:mm tt", CultureInfo.InvariantCulture),
                priority = record.EventPriority,
                interaction = record.Interaction,
                outcome = record.Response,
                coAgent = record.CoAgent,
                urn = record.Form_URNNo,
                status = record.Status,
                nextFollowUpDate = record.NextFollowUpDate?.ToString("dd/MM/yyyy HH:mm tt", CultureInfo.InvariantCulture) ?? null,
                title = record.Title,
                contactPerson = record.ContactPerson,
                modeOfCommunication = record.ModeOfCommunication,
                contactNumber = record.ContactNumber
            }, JsonRequestBehavior.AllowGet);
        }

        //Soft Delete FollowUp
        public JsonResult DeleteFollowUp(int FollowUpId)
        {
            try
            {
                ResponseModel response = new ResponseModel();
                if (FollowUpId <= 0)
                {
                    response.IsSuccess = false;
                    response.Response = "Invalid FollowUp ID.";
                    return Json(response, JsonRequestBehavior.AllowGet);
                }

                response = transactionBusiness.DeleteFollowUp(FollowUpId, Convert.ToString(SessionPersister.LoginedUser.UserId.Value));
                return Json(response, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        #endregion "Event Methods"
    }
}