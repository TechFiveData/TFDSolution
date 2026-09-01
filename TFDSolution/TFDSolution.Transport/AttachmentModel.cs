using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TFDSolution.Transport
{
    public class PageAttachmentViewModel
    {
        public List<PageAttachmentModel> Attachments { get; set; }
        public PageAttachmentRequest Request { get; set; }
    }
    public class PageAttachmentRequest
    {
        public string PageAction { get; set; }
        public int RecordStatus { get; set; }
        public string FormId { get; set; }
        public int ParentId { get; set; }
        public string FileName { get; set; }
        public string FileType { get; set; }
        public string FileSize { get; set; }
        public string FilePath { get; set; }
        public string URNNo { get; set; }
        public string UploadedBy { get; set; }
        public int DetailId { get; set; }
        public string FieldId { get; set; }
        public int ItemSrNo { get; set; }
    }
    public class PageAttachmentModel
    {
        public int SrNo { get; set; }
        public int Id { get; set; }
        public string FormId { get; set; }
        public string URNNo { get; set; }
        public int ParentId { get; set; }
        public string FileName { get; set; }
        public string FileType { get; set; }
        public string FileSize { get; set; }
        public string FilePath { get; set; }
        public string UploadDate { get; set; }
        public string UploadedBy { get; set; }
        public int DetailId { get; set; }
        public int ItemSrNo { get; set; }
        public string FieldId { get; set; }
    }
}
