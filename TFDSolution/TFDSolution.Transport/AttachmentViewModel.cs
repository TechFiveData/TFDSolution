using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace TFDSolution.Transport
{
       public class AttachmentViewModel
    {
        public Guid AttachmentId { get; set; }
        public Guid ItemId { get; set; }
        public string AttachedFileName { get; set; }
        public string AttachedFilePath { get; set; }
        public decimal AttachmentSize { get; set; }
        public int AttachmentType { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        public string UpdatedBy { get; set; }
        public DateTime? UpdatedOn { get; set; }

        [Required]
        public HttpPostedFileBase File { get; set; } // For file upload
    }
}
