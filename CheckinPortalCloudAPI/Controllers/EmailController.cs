using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace CheckinPortalCloudAPI.Controllers
{
    public class EmailController : ApiController
    {
        [HttpPost]
        [ActionName("SendEmail")]
        public async Task<Models.Email.EmailResponse> SendEmail(Models.Email.EmailRequest emailRequest)
        {
            try
            {
                //if(!string.IsNullOrEmpty(emailRequest.AttachmentFileName) && string.IsNullOrEmpty(emailRequest.AttchmentBase64))
                if (!string.IsNullOrEmpty(emailRequest.AttachmentFileName) && emailRequest.EmailType.Equals(Models.Email.EmailType.Precheckedin))
                {
                    if(System.IO.Directory.Exists(System.Web.HttpContext.Current.Request.MapPath("~\\Resources\\EmailAttachments")) )
                    {
                        string[] filenames = System.IO.Directory.GetFiles(System.Web.HttpContext.Current.Request.MapPath("~\\Resources\\EmailAttachments"), "*.pdf");
                        if (filenames.Length >= 1)
                        {
                            byte[] fileBytes = System.IO.File.ReadAllBytes(filenames[0]);
                            System.IO.FileInfo fileInfo = new System.IO.FileInfo(filenames[0]);
                            emailRequest.AttchmentBase64 = Convert.ToBase64String(fileBytes);
                            ServiceLib.Email.EmailLib emailLib1 = new ServiceLib.Email.EmailLib();
                            Models.Email.EmailResponse emailResponse1 = await emailLib1.SendEmail(emailRequest.ToEmail, emailRequest.FromEmail, emailRequest.EmailType, emailRequest.GuestName, emailRequest.Subject, ConfigurationManager.AppSettings["SMTPUsername"], ConfigurationManager.AppSettings["SMTPPassword"], ConfigurationManager.AppSettings["SMTPHOST"], Int32.Parse(ConfigurationManager.AppSettings["PORT"]), bool.Parse(ConfigurationManager.AppSettings["SslEnabled"]), emailRequest.confirmationNumber, emailRequest.displayFromEmail, emailRequest.AttchmentBase64, emailRequest.ItemName, emailRequest.TotalAmount,
                                                                                                emailRequest.ArrivalDate, emailRequest.DepartureDate, emailRequest.ReservationNumber, ConfigurationManager.AppSettings["SMTPDefaultCredentials"],fileInfo.Name);
                            return emailResponse1;
                        }
                    }
                }
                //System.IO.File.AppendAllText(System.Web.HttpContext.Current.Request.MapPath("~\\Resources\\EmailRequest.txt"), "Request : " + JsonConvert.SerializeObject(emailRequest));
                ServiceLib.Email.EmailLib emailLib = new ServiceLib.Email.EmailLib();
                Models.Email.EmailResponse emailResponse = await emailLib.SendEmail(emailRequest.ToEmail, emailRequest.FromEmail, emailRequest.EmailType, emailRequest.GuestName, emailRequest.Subject, ConfigurationManager.AppSettings["SMTPUsername"], ConfigurationManager.AppSettings["SMTPPassword"], ConfigurationManager.AppSettings["SMTPHOST"], Int32.Parse(ConfigurationManager.AppSettings["PORT"]), bool.Parse(ConfigurationManager.AppSettings["SslEnabled"]),emailRequest.confirmationNumber,emailRequest.displayFromEmail,emailRequest.AttchmentBase64,emailRequest.ItemName,emailRequest.TotalAmount, 
                                                                                    emailRequest.ArrivalDate,emailRequest.DepartureDate,emailRequest.ReservationNumber, ConfigurationManager.AppSettings["SMTPDefaultCredentials"]);
                //System.IO.File.AppendAllText(System.Web.HttpContext.Current.Request.MapPath("~\\Resources\\EmailRequest.txt"), "Responset : " + JsonConvert.SerializeObject(emailResponse));
                return emailResponse;
            }
            catch (Exception ex)
            {
                return new Models.Email.EmailResponse()
                {
                    result = false,
                    responseMessage = ex.Message,
                    statusCode = -1
                };
            }

        }
    }
}
