using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFDSolution.Transport
{
    public class EmailTemplateModel
    {
        public int TemplateId { get; set; }                // Unique template ID
        public string TemplateName { get; set; }           // Friendly name
        public string Subject { get; set; }                // Subject line
        public string Body { get; set; }                   // Email body (HTML/Text)
        public string CC { get; set; }                     // Comma-separated CC emails
        public string BCC { get; set; }                    // Comma-separated BCC emails
        public string FromEmail { get; set; }              // Default sender
        public string ReplyTo { get; set; }                // Reply-to address
        public bool IsHtml { get; set; }                   // Whether body is HTML
        public bool IsActive { get; set; }                 // Active status
        public string CreatedBy { get; set; }              // Audit field
        public DateTime CreatedOn { get; set; }            // Created datetime
        public string UpdatedBy { get; set; }              // Last updated by
        public DateTime? UpdatedOn { get; set; }           // Last updated datetime
        public string FormIds { get; set; }                // Related Form IDs (comma-separated or JSON)
        public string Signature { get; set; }                   // Email body (HTML/Text)
        public int SrNo { get; set; }
        public string UserId { get; set; }                   // Email body (HTML/Text)
        public string AccountEmails { get; set; }
    }
}
