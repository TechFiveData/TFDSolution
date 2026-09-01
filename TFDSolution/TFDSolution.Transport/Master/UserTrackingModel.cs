using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFDSolution.Transport.Master
{
    public class UserTrackingModel
    {
        public string RecordType { get; set; }
        public string RecordId { get; set; }
        public string RecordNo { get; set; }

        public List<UserTrackingDetailModel> TrackingDetails { get; set; }

        public UserTrackingModel()
        {
            TrackingDetails = new List<UserTrackingDetailModel>();
        }
    }


    public class UserTrackingDetailModel
    {
        public long Id { get; set; }

        /// <summary>
        /// Unique action type
        /// CREATE
        /// CREATED_FROM
        /// STATUS_CHANGE
        /// DELETE
        /// UPDATE
        /// ITEM_ADD
        /// ITEM_UPDATE
        /// AMEND
        /// REPLICATION
        /// ITEM_DELETE
        /// </summary>
        public string ActionType { get; set; }

        /// <summary>
        /// Display name of action
        /// Example: Record Created, Status Changed
        /// </summary>
        public string ActionName { get; set; }

        /// <summary>
        /// User who performed the action
        /// </summary>
        public string UserId { get; set; }

        public string UserName { get; set; }

        /// <summary>
        /// Date and time when activity happened
        /// </summary>
        public DateTime ActionDate { get; set; }

        /// <summary>
        /// Optional source record
        /// Example: Inquiry / Indent / Quotation
        /// For Created From / Replication / Amendment:
        /// </summary>
        public string SourceRecordType { get; set; }

        public string SourceRecordId { get; set; }

        public string SourceRecordNo { get; set; }

        /// <summary>
        /// Status before change
        /// For Status Changed:
        /// </summary>
        public string OldStatus { get; set; }

        /// <summary>
        /// Status after change
        /// </summary>
        public string NewStatus { get; set; }

        /// <summary>
        /// Item information
        /// For Item Added / Updated / Deleted:
        /// </summary>
        public string ItemId { get; set; }

        public string ItemCode { get; set; }

        public string ItemName { get; set; }

        /// <summary>
        /// Field which was changed
        /// Example: Quantity, Rate, Delivery Date
        /// For Record Updated, the important properties are:
        /// </summary>
        public string FieldName { get; set; }

        /// <summary>
        /// Previous field value
        /// </summary>
        public string OldValue { get; set; }

        /// <summary>
        /// New field value
        /// </summary>
        public string NewValue { get; set; }

        /// <summary>
        /// Related/amended/replicated record number
        /// </summary>
        public string RelatedRecordNo { get; set; }

        public string RelatedRecordId { get; set; }

        /// <summary>
        /// Optional short description
        /// </summary>
        public string Description { get; set; }
    }
}
