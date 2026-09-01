using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Web;
using TFDSolution.Business;
using TFDSolution.Transport.Master;
using System.IO;
using TFDSolution.Transport;
using TFDSolution.Transport.Common;
using System.Reflection.Emit;
using System.Web.UI.WebControls;
namespace TFDSolution.Common
{
    public static class MailHelper
    {
        public static (bool success, string message) SendEmail(string userId, string toEmail, string ccEmail, string bccEmail, string subject, string body, HttpFileCollectionBase files = null)
        {
            try
            {
                // Fetch UserDetail using userId
                var masterBusiness = new MasterBusiness();
                UserDetail userDetail = masterBusiness.GetUser(userId);
                if (userDetail == null || string.IsNullOrEmpty(userDetail.FromEmail)
                    || string.IsNullOrEmpty(userDetail.FromPassword)
                    || string.IsNullOrEmpty(userDetail.SmtpPort)
                    || string.IsNullOrEmpty(userDetail.SmtpServer))
                {
                    return (false, "Email Configuration Not Valid.");
                }
                // Read Email.html template from Content folder
                string templatePath = HttpContext.Current.Server.MapPath("~/Content/Email.html");
                if (!File.Exists(templatePath))
                {
                    return (false, "Email template not found.");
                }
                string htmlTemplate = File.ReadAllText(templatePath);

                // Replace placeholders in the template
                string finalBody = htmlTemplate
                    .Replace("{{Name}}", "Saurabh")
                    .Replace("{{Message}}", string.IsNullOrWhiteSpace(body) ? "Your Stock Summary is ready." : body)
                    .Replace("{{OrderNo}}", "12345")
                    .Replace("{{Date}}", DateTime.Now.ToString("dd-MMM-yyyy"))
                    .Replace("{{Total}}", "₹ 25,000")
                    .Replace("{{ActionUrl}}", "https://yourapp.com/stock-summary")
                    .Replace("{{UnsubscribeUrl}}", "https://yourapp.com/unsubscribe")
                    .Replace("{{ViewInBrowserUrl}}", "https://yourapp.com/viewmail");

                using (var msg = new MailMessage())
                {
                    msg.From = new MailAddress(userDetail.FromEmail, userDetail.DisplayName);

                    // Validate ToEmail
                    if (string.IsNullOrWhiteSpace(toEmail))
                    {
                        return (false, "To email is required.");
                    }
                    msg.To.Add(toEmail);

                    // Add CC emails
                    if (!string.IsNullOrWhiteSpace(ccEmail))
                    {
                        foreach (var cc in ccEmail.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries))
                        {
                            msg.CC.Add(cc.Trim());
                        }
                    }

                    // Add BCC emails
                    if (!string.IsNullOrWhiteSpace(bccEmail))
                    {
                        foreach (var bcc in bccEmail.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries))
                        {
                            msg.Bcc.Add(bcc.Trim());
                        }
                    }

                    msg.Subject = string.IsNullOrWhiteSpace(subject) ? "Stock Summary" : subject;
                    msg.Body = finalBody;
                    msg.IsBodyHtml = true;

                    // Add attachments if provided
                    if (files != null && files.Count > 0)
                    {
                        for (int i = 0; i < files.Count; i++)
                        {
                            var uploadedFile = files[i];
                            if (uploadedFile != null && uploadedFile.ContentLength > 0)
                            {
                                var attachment = new Attachment(uploadedFile.InputStream, uploadedFile.FileName);
                                msg.Attachments.Add(attachment);
                            }
                        }
                    }
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                    using (var smtp = new SmtpClient
                    {
                        Host = userDetail.SmtpServer,
                        Port = Convert.ToInt32(userDetail.SmtpPort),
                        EnableSsl = userDetail.EnableSSL,
                        UseDefaultCredentials = false,
                        Credentials = new NetworkCredential(userDetail.FromEmail, userDetail.FromPassword)
                    })
                    {
                        smtp.Send(msg);
                    }
                }
                return (true, "Email sent successfully.");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public static ResponseModel SendEmail(EmailRequest request)
        {
            ResponseModel response = new ResponseModel();
            response.IsSuccess = false;
            EmailLog log = new EmailLog();
            log.BCCEmail = request.BCCEmail;
            log.CCEmail= request.CCEmail;
            log.CompanyId = request.CompanyId;
            log.UserId = request.UserId;
            log.FormId = request.FormId;
            log.ToEmail = request.ToEmail;
            log.IsSent = false;
            log.RecordId = request.ParentId;
            log.Subject = request.Subject;
            log.UserId = request.UserId;            
            try
            {                
                // Fetch UserDetail using userId
                var masterBusiness = new MasterBusiness();
                UserDetail userDetail = masterBusiness.GetUser(request.UserId);
                if (userDetail == null || string.IsNullOrEmpty(userDetail.FromEmail)
                    || string.IsNullOrEmpty(userDetail.FromPassword)
                    || string.IsNullOrEmpty(userDetail.SmtpPort)
                    || string.IsNullOrEmpty(userDetail.SmtpServer))
                {
                    response.IsSuccess = true;
                    response.Response = "Email Configuration Not Valid.";
                    return response;
                }
                // Read Email.html template from Content folder
                string templatePath = HttpContext.Current.Server.MapPath("~/Content/EmailTemplate/DefaultEmail.html");
                if (!File.Exists(templatePath))
                {
                    response.IsSuccess = true;
                    response.Response = "Email template not found.";
                    return response;
                }
                if (!string.IsNullOrWhiteSpace(request.Subject))
                {
                    string subjectHtml = Convert.ToString(request.Subject);
                    string bodyHtml = Convert.ToString(request.MessageBody);
                    subjectHtml = subjectHtml.Replace("{UserName}", request.UserName);
                    bodyHtml = bodyHtml.Replace("{UserName}", request.UserName);
                    if (request != null && request.FieldData != null && request.FieldData.Count > 0)
                    {
                        foreach (var item in request.FieldData)
                        {
                            if (subjectHtml.Contains("{" + item.FieldName + "}"))
                            {
                                subjectHtml = subjectHtml.Replace("{" + item.FieldName + "}", item.FieldValue);
                            }
                            if (bodyHtml.Contains("{" + item.FieldName + "}"))
                            {
                                bodyHtml = bodyHtml.Replace("{" + item.FieldName + "}", item.FieldValue);
                            }
                        }
                    }
                    request.Subject = subjectHtml;
                    request.MessageBody = bodyHtml;
                }
                string htmlTemplate = File.ReadAllText(templatePath);
                // Replace placeholders in the template
                string finalBody = htmlTemplate
                    .Replace("{Name}", "Saurabh")
                    .Replace("{Message}", string.IsNullOrWhiteSpace(request.MessageBody) ? "This email is system generated. Please don't reply" : request.MessageBody)
                    .Replace("{URNNo}", request.URNNo)
                    .Replace("{Year}", DateTime.Now.ToString("yyyy"))
                    .Replace("{ReportTemplateLine}", (!string.IsNullOrEmpty(request.FullFilePath)) ? "<p>Please find the attached file.</p>" : "")
                    .Replace("{CompanyName}", request.CompanyName)
                    .Replace("{ActionUrl}", "https://yourapp.com/stock-summary")
                    .Replace("{UnsubscribeUrl}", "https://yourapp.com/unsubscribe")
                    .Replace("{ViewInBrowserUrl}", "https://yourapp.com/viewmail");

                using (var msg = new MailMessage())
                {
                    msg.From = new MailAddress(userDetail.FromEmail, userDetail.DisplayName);
                    // Validate ToEmail
                    if (string.IsNullOrWhiteSpace(request.ToEmail))
                    {
                        response.IsSuccess = true;
                        response.Response = "To email is required.";
                        return response;
                    }
                    msg.To.Add(request.ToEmail);
                    // Add CC emails
                    if (!string.IsNullOrWhiteSpace(request.CCEmail))
                    {
                        foreach (var cc in request.CCEmail.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries))
                        {
                            msg.CC.Add(cc.Trim());
                        }
                    }
                    // Add BCC emails
                    if (!string.IsNullOrWhiteSpace(request.BCCEmail))
                    {
                        foreach (var bcc in request.BCCEmail.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries))
                        {
                            msg.Bcc.Add(bcc.Trim());
                        }
                    }

                    msg.Subject = string.IsNullOrWhiteSpace(request.Subject) ? "Notification" : request.Subject;
                    msg.Body = finalBody;
                    msg.IsBodyHtml = true;
                    // Add attachments if provided
                    if (!string.IsNullOrEmpty(request.FullFilePath))
                    {
                        string physicalPath = HttpContext.Current.Server.MapPath(request.FullFilePath);
                        if (File.Exists(physicalPath))
                        {
                            Attachment attachment = new Attachment(physicalPath);
                            msg.Attachments.Add(attachment);
                            log.AttachedFileName = request.ReportFileName;
                        }
                    }
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                    using (var smtp = new SmtpClient
                    {
                        Host = userDetail.SmtpServer,
                        Port = Convert.ToInt32(userDetail.SmtpPort),
                        EnableSsl = userDetail.EnableSSL,
                        UseDefaultCredentials = false,
                        Credentials = new NetworkCredential(userDetail.FromEmail, userDetail.FromPassword)
                    })
                    {
                        smtp.Send(msg);
                    }
                }
                
                response.IsSuccess = true;
                response.Response = "Email sent successfully.";
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                if (ex.InnerException != null)
                    response.Response = "Error Sending Email : " + ex.InnerException.Message;
                else
                    response.Response = "Error Sending Email : " + ex.Message;                
            }
            log.IsSent = response.IsSuccess.Value;
            log.ExceptionMessage = response.Response;
            CommonBusiness.SaveEmailLog(log);
            return response;
        }
        public static ResponseModel SendEmailDefault(EmailRequest request)
        {
            ResponseModel response = new ResponseModel();
            response.IsSuccess = false;
            EmailLog log = new EmailLog();
            log.BCCEmail = request.BCCEmail;
            log.CCEmail = request.CCEmail;
            log.CompanyId = request.CompanyId;
            log.UserId = request.UserId;
            log.FormId = request.FormId;
            log.ToEmail = request.ToEmail;
            log.IsSent = false;
            log.RecordId = request.ParentId;
            log.Subject = request.Subject;
            log.UserId = request.UserId;
            try
            {
                // Fetch UserDetail using userId
                var masterBusiness = new MasterBusiness();
                UserDetail userDetail = masterBusiness.GetUser(request.UserId);
                if (userDetail == null || string.IsNullOrEmpty(userDetail.FromEmail)
                    || string.IsNullOrEmpty(userDetail.FromPassword)
                    || string.IsNullOrEmpty(userDetail.SmtpPort)
                    || string.IsNullOrEmpty(userDetail.SmtpServer))
                {
                    response.IsSuccess = true;
                    response.Response = "Email Configuration Not Valid.";
                    return response;
                }
                // Read Email.html template from Content folder
                //string templatePath = HttpContext.Current.Server.MapPath("~/Content/EmailTemplate/DefaultEmail.html");
                //if (!File.Exists(templatePath))
                //{
                //    response.IsSuccess = true;
                //    response.Response = "Email template not found.";
                //    return response;
                //}
                if (!string.IsNullOrWhiteSpace(request.Subject))
                {
                    string subjectHtml = Convert.ToString(request.Subject);
                    string bodyHtml = Convert.ToString(request.MessageBody);
                    subjectHtml = subjectHtml.Replace("{UserName}", request.UserName);
                    bodyHtml = bodyHtml.Replace("{UserName}", request.UserName);
                    if (request != null && request.FieldData != null && request.FieldData.Count > 0)
                    {
                        foreach (var item in request.FieldData)
                        {
                            if (subjectHtml.Contains("{" + item.FieldName + "}"))
                            {
                                subjectHtml = subjectHtml.Replace("{" + item.FieldName + "}", item.FieldValue);
                            }
                            if (bodyHtml.Contains("{" + item.FieldName + "}"))
                            {
                                bodyHtml = bodyHtml.Replace("{" + item.FieldName + "}", item.FieldValue);
                            }
                        }
                    }
                    request.Subject = subjectHtml;
                    request.MessageBody = bodyHtml;
                }
                //string htmlTemplate = File.ReadAllText(templatePath);
                // Replace placeholders in the template
                string finalBody = request.MessageBody;
                //.Replace("{Name}", "Saurabh")
                //.Replace("{Message}", string.IsNullOrWhiteSpace(request.MessageBody) ? "This email is system generated. Please don't reply" : request.MessageBody)
                //.Replace("{URNNo}", request.URNNo)
                //.Replace("{Year}", DateTime.Now.ToString("yyyy"))
                //.Replace("{ReportTemplateLine}", (!string.IsNullOrEmpty(request.FullFilePath)) ? "<p>Please find the attached file.</p>" : "")
                //.Replace("{CompanyName}", request.CompanyName)
                //.Replace("{ActionUrl}", "https://yourapp.com/stock-summary")
                //.Replace("{UnsubscribeUrl}", "https://yourapp.com/unsubscribe")
                //.Replace("{ViewInBrowserUrl}", "https://yourapp.com/viewmail");

                using (var msg = new MailMessage())
                {
                    msg.From = new MailAddress(userDetail.FromEmail, request.CompanyName);
                    // Validate ToEmail
                    if (string.IsNullOrWhiteSpace(request.ToEmail))
                    {
                        response.IsSuccess = true;
                        response.Response = "To email is required.";
                        return response;
                    }
                    msg.To.Add(request.ToEmail);
                    // Add CC emails
                    if (!string.IsNullOrWhiteSpace(request.CCEmail))
                    {
                        foreach (var cc in request.CCEmail.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries))
                        {
                            msg.CC.Add(cc.Trim());
                        }
                    }
                    // Add BCC emails
                    if (!string.IsNullOrWhiteSpace(request.BCCEmail))
                    {
                        foreach (var bcc in request.BCCEmail.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries))
                        {
                            msg.Bcc.Add(bcc.Trim());
                        }
                    }

                    msg.Subject = string.IsNullOrWhiteSpace(request.Subject) ? "Notification" : request.Subject;
                    msg.Body = finalBody;
                    msg.IsBodyHtml = true;
                    // Add attachments if provided
                    if (File.Exists(request.FullFilePath))
                    {
                        Attachment attachment = new Attachment(request.FullFilePath);
                        msg.Attachments.Add(attachment);
                        log.AttachedFileName = request.ReportFileName;
                    }
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                    using (var smtp = new SmtpClient
                    {
                        Host = userDetail.SmtpServer,
                        Port = Convert.ToInt32(userDetail.SmtpPort),
                        EnableSsl = userDetail.EnableSSL,
                        UseDefaultCredentials = false,
                        Credentials = new NetworkCredential(userDetail.FromEmail, userDetail.FromPassword)
                    })
                    {
                        smtp.Send(msg);
                    }
                }

                response.IsSuccess = true;
                response.Response = "Email sent successfully.";
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                if (ex.InnerException != null)
                    response.Response = "Error Sending Email : " + ex.InnerException.Message;
                else
                    response.Response = "Error Sending Email : " + ex.Message;
            }
            log.IsSent = response.IsSuccess.Value;
            log.ExceptionMessage = response.Response;
            CommonBusiness.SaveEmailLog(log);
            return response;
        }
    }
}