using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFDSolution.Transport.Common
{
    public class FollowUpEventModel
    {
        public int ParentFollowUpId { get; set; } // Optional: For linking follow-ups
        public int FollowUpId { get; set; }
        public string Title { get; set; }
        public int FollowUpCount { get; set; }
        public string Url { get; set; }
        // public string SelectAgainst { get; set; } // Optional: Entity like Lead, Client, etc.
        public int? CoAgent { get; set; }
        public DateTime FollowUpDate { get; set; }
        public string Interaction { get; set; }
        public string Response { get; set; }
        public Guid CreatedBy { get; set; }
        public int? Status { get; set; }
        public string AssignedTo { get; set; }
        public int? ContactPerson_Id { get; set; }
        public string FormId { get; set; }
        public int Form_ParentId { get; set; }
        public string Form_URNNo { get; set; }
        public string ContactPerson { get; set; }
        public DateTime? NextFollowUpDate { get; set; }
        public int? NextFollowUpID { get; set; } = null;
        public int EventPriority { get; set; } = 1; // Default to Low priority
        public int? ModeOfCommunication { get; set; }  // Default to Personally Meet
        public string ContactNumber { get; set; } // Optional: Contact number for follow-up
        public int AccountId { get; set; }
    }

}
