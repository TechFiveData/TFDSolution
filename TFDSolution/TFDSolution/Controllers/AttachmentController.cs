using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TFDSolution.Transport;
using TFDSolution.Transport.Master;

public class AttachmentController : Controller
{
    private static List<AttachmentViewModel> attachments = new List<AttachmentViewModel>();

    [HttpPost]
    public ActionResult UploadAttachment(AttachmentViewModel model)
    {
        if (model.File == null || model.File.ContentLength == 0)
        {
            ModelState.AddModelError("File", "Please select a file to upload.");
            return PartialView("_AttachmentUpload", model);
        }

        string uploadsFolder = Server.MapPath("~/uploads");
        if (!Directory.Exists(uploadsFolder))
        {
            Directory.CreateDirectory(uploadsFolder);
        }

        string uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(model.File.FileName)}";
        string filePath = Path.Combine(uploadsFolder, uniqueFileName);

        model.File.SaveAs(filePath); // Save the file

        // Add to in-memory list (Replace this with DB Save logic)
        var newAttachment = new AttachmentViewModel
        {
            AttachmentId = Guid.NewGuid(),
            ItemId = model.ItemId,
            AttachedFileName = model.File.FileName,
            AttachedFilePath = "/uploads/" + uniqueFileName,
            AttachmentSize = Math.Round((decimal)model.File.ContentLength / 1024, 2), // KB
            CreatedOn = DateTime.UtcNow,
            CreatedBy = User.Identity.Name ?? "System"
        };

        attachments.Add(newAttachment);

        return PartialView("_AttachmentUpload", new AttachmentViewModel());
    }

    [HttpGet]
    public ActionResult GetAttachments(Guid itemId)
    {
        var filteredAttachments = attachments.Where(a => a.ItemId == itemId).ToList();
        return PartialView("_AttachmentList", filteredAttachments);
    }
}
