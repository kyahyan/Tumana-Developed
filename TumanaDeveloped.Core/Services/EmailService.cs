using Umbraco.Core.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using TumanaDeveloped.Core.interfaces;
using TumanaDeveloped.Core.ViewModel;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Services;
using Umbraco.Web;
using System.Web;

namespace TumanaDeveloped.Core.Services
{
    public class EmailService : IEmailService
    {
        private UmbracoHelper _umbraco;
        private IContentService _contentService;
        private ILogger _logger;
        public EmailService(UmbracoHelper umbraco, IContentService contentService, ILogger iLogger)
        {
            _umbraco = umbraco;
            _contentService = contentService;
            _logger = iLogger;
        }
       
        public void SendContactNotificationToAdmin(ContactFormViewModel vm)
        {

            var emailTemplate = GetEmailTEmplate("New Contact Form Notification");

            if(emailTemplate == null)
            {
                throw new Exception("Template not found");
            }

            var subject = emailTemplate.Value<string>("emailTemplateSubjectLine");
            var htmlContent = emailTemplate.Value<string>("emailTemplateHtmlContent");
            var textContent = emailTemplate.Value<string>("emailTemplateTextContent");


            MailMerge("name", vm.Name, ref htmlContent, ref textContent);
            MailMerge("email", vm.Email, ref htmlContent, ref textContent);
            MailMerge("comment", vm.Comment, ref htmlContent, ref textContent);


            var siteSettings = _umbraco.ContentAtRoot().DescendantsOrSelfOfType("siteSettings").FirstOrDefault();
            if (siteSettings == null)
            {
                throw new Exception("There are no site Settings");
            }
            var toAddresses = siteSettings.Value<string>("emailSettingsAdminAccounts");

            if (string.IsNullOrEmpty(toAddresses))
            {
                throw new Exception("There needs to be a to address in site settings");
            }

            SendMail(toAddresses, subject, htmlContent, textContent);

        }

        private void SendMail(string toAddresses, string subject, string htmlContent, string textContent)
        {
            var siteSettings = _umbraco.ContentAtRoot().DescendantsOrSelfOfType("siteSettings").FirstOrDefault();
            if (siteSettings == null)
            {
                throw new Exception("There are no site Settings");
            }
            //debug mode

            var fromAddress = siteSettings.Value<string>("emailSettingsFromAddress");

            if (string.IsNullOrEmpty(fromAddress))
            {
                throw new Exception("There needs to be a from address in site settings");
            }

            var debugMode = siteSettings.Value<bool>("testMode");
            var testEmailAccounts = siteSettings.Value<string>("emailTestAccounts");

            var recipients = toAddresses;

            if (debugMode)
            {
                recipients = testEmailAccounts;
                subject += "(TEST MODE) - " + toAddresses;
            }

            var emails = _umbraco.ContentAtRoot().DescendantsOrSelfOfType("emails").FirstOrDefault();
            var newEmail = _contentService.Create(toAddresses, emails.Id, "Email");
            newEmail.SetValue("emailSubject", subject);
            newEmail.SetValue("emailToAddress", recipients);
            newEmail.SetValue("emailHtmlContent", htmlContent);
            newEmail.SetValue("emailTextContent", textContent);
            newEmail.SetValue("emailSent", false);
            _contentService.SaveAndPublish(newEmail);


            var mimeType = new System.Net.Mime.ContentType("text/html");
            var alternativeView = AlternateView.CreateAlternateViewFromString(htmlContent, mimeType);


            var smtpMessage = new MailMessage();
            smtpMessage.AlternateViews.Add(alternativeView);

            foreach (var recipient in recipients.Split(','))
            {
                if (!string.IsNullOrEmpty(recipient))
                {
                    smtpMessage.To.Add(recipient);
                }
            }

            smtpMessage.From = new MailAddress(fromAddress);
            smtpMessage.Subject = subject;
            smtpMessage.Body = textContent;


            using (var smtp = new SmtpClient())
            {
                try
                {
                    smtp.Send(smtpMessage);
                    newEmail.SetValue("emailSent", true);
                    newEmail.SetValue("emailSentDate", DateTime.Now);
                    _contentService.SaveAndPublish(newEmail);

                }
                catch (Exception exc)
                {
                    _logger.Error<EmailService>("Problem sending the email", exc);
                    throw exc;
                }
            }

        }

        public void SendVerifyEmailAddressNotification(string membersEmail, string verificationToken)
        {

            var emailTemplate = GetEmailTEmplate("Verify Email");

            if (emailTemplate == null)
            {
                throw new Exception("Template not found");
            }

            var subject = emailTemplate.Value<string>("emailTemplateSubjectLine");
            var htmlContent = emailTemplate.Value<string>("emailTemplateHtmlContent");
            var textContent = emailTemplate.Value<string>("emailTemplateTextContent");

            var url = HttpContext.Current.Request.Url.AbsoluteUri.Replace(HttpContext.Current.Request.Url.AbsolutePath, string.Empty);
            url += $"/verify?token={verificationToken}";
            MailMerge("verify-url", url, ref htmlContent, ref textContent);


            //Send Email
            SendMail(membersEmail, subject, htmlContent, textContent);


        }

        private void MailMerge(string token, string value, ref string htmlContent, ref string textContent)
        {
            htmlContent = htmlContent.Replace($"##{token}##", value);
            textContent = textContent.Replace($"##{token}##", value);
        }

        private IPublishedContent GetEmailTEmplate(string templateName)
        {
            var template = _umbraco.ContentAtRoot()
                .DescendantsOrSelfOfType("emailTemplate")
                .Where(w => w.Name == templateName)
                .FirstOrDefault();

            return template;

        }

        public void SendResetPasswordNotification(string membersEmail, string resetToken)
        {

            var emailTemplate = GetEmailTEmplate("Reset Password");

            if (emailTemplate == null)
            {
                throw new Exception("Template not found");
            }

            var subject = emailTemplate.Value<string>("emailTemplateSubjectLine");
            var htmlContent = emailTemplate.Value<string>("emailTemplateHtmlContent");
            var textContent = emailTemplate.Value<string>("emailTemplateTextContent");

            var url = HttpContext.Current.Request.Url.AbsoluteUri.Replace(HttpContext.Current.Request.Url.AbsolutePath, string.Empty);
            url += $"/reset-password?token={resetToken}";


            MailMerge("reset-url", url, ref htmlContent, ref textContent);

            SendMail(membersEmail, subject, htmlContent, textContent);

        }

        public void SendPasswordChangedNotification(string membersEmail)
        {

            var emailTemplate = GetEmailTEmplate("Password Changed");

            if (emailTemplate == null)
            {
                throw new Exception("Template not found");
            }

            var subject = emailTemplate.Value<string>("emailTemplateSubjectLine");
            var htmlContent = emailTemplate.Value<string>("emailTemplateHtmlContent");
            var textContent = emailTemplate.Value<string>("emailTemplateTextContent");

            SendMail(membersEmail, subject, htmlContent, textContent);


        }
    }
}
