using QRCoder;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.Web;

namespace CheckinPortalCloudAPI.ServiceLib.Email
{
    public class EmailLib
    {
        public async Task<Models.Email.EmailResponse> SendEmail(string ToEmail,string FromEmail,Models.Email.EmailType emailType,string GuestName,string Subject,string Username,string Password,string Host,int port,bool enableSsl,string Confirmation_no,string displayFromEmail,string base64Attchement,string ItemName,string TotalAmount,
                                                                 string ArrivalDate,string DepartureDate,string ReservationNumber, string SMTPDefaultCredentials)
        {
            try
            {
                if (string.IsNullOrEmpty(Confirmation_no))
                    Confirmation_no = DateTime.Now.ToString("ddMMyyyyHHmmss");
                if (string.IsNullOrEmpty(FromEmail))
                    return new Models.Email.EmailResponse()
                    {
                        result = false,
                        responseMessage = "From email can not be blank"
                    };
                ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
                {
                    return true;
                };
                using (MailMessage message = createMailMessage(emailType, GuestName, Confirmation_no, Subject, FromEmail, displayFromEmail,base64Attchement,ItemName,TotalAmount,ArrivalDate,DepartureDate,ReservationNumber))
                {
                    


                    //System.IO.File.WriteAllText(System.Web.HttpContext.Current.Request.MapPath("~\\Resources\\log.txt"), "Message created");
                    if (ToEmail != null)
                    {
                        //foreach (string toEmail in ToEmail)
                        message.To.Add(new MailAddress(ToEmail));
                    }
                    else
                    {
                        return new Models.Email.EmailResponse()
                        {
                            result = false,
                            responseMessage = "To email address is missing"
                        };
                    }
                    if (string.IsNullOrEmpty(FromEmail))
                    {
                        return new Models.Email.EmailResponse()
                        {
                            result = false,
                            responseMessage = "from email address is missing"
                        };
                    }
                    using (var smtp = new SmtpClient())
                    {
                        if (!string.IsNullOrEmpty(base64Attchement))
                        {
                            using (MemoryStream stream = new MemoryStream(Convert.FromBase64String(base64Attchement)))
                            {
                                Attachment att = new Attachment(stream, "attachment.pdf");//System.Web.HttpContext.Current.Request.MapPath("~\\Resources\\Images\\Temp\\" + Confirmation_no + "_folio.pdf"));
                                att.ContentDisposition.Inline = true;
                                message.Attachments.Add(att);

                                smtp.UseDefaultCredentials = string.IsNullOrEmpty(SMTPDefaultCredentials) ? false : SMTPDefaultCredentials.ToUpper().Equals("TRUE") ? true : false;
                                var credentials = new NetworkCredential
                                {
                                    UserName = Username,
                                    Password = Password
                                };

                                smtp.Credentials = credentials;
                                smtp.Host = Host;
                                smtp.Port = port;

                                smtp.EnableSsl = enableSsl;
                                await smtp.SendMailAsync(message);
                                message.Dispose();
                                smtp.Dispose();
                                return new Models.Email.EmailResponse()
                                {
                                    result = true,

                                };
                            }
                        }
                        smtp.UseDefaultCredentials = string.IsNullOrEmpty(SMTPDefaultCredentials) ? false : SMTPDefaultCredentials.ToUpper().Equals("TRUE") ? true : false;
                        var credential = new NetworkCredential
                        {
                            UserName = Username,
                            Password = Password
                        };
                        smtp.UseDefaultCredentials = false;
                        smtp.Credentials = credential;
                        smtp.Host = Host;
                        smtp.Port = port;
                        smtp.EnableSsl = enableSsl;
                        smtp.Timeout = 10000;
                        smtp.DeliveryMethod = SmtpDeliveryMethod.Network;


                        await smtp.SendMailAsync(message);
                        message.Dispose();
                        smtp.Dispose();
                        return new Models.Email.EmailResponse()
                        {
                            result = true,
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<Models.Email.EmailResponse> SendEmail(string ToEmail, string FromEmail, Models.Email.EmailType emailType, string GuestName, string Subject, string Username, string Password, string Host, int port, bool enableSsl, string Confirmation_no, string displayFromEmail, string base64Attchement, string ItemName, string TotalAmount,
                                                                 string ArrivalDate, string DepartureDate, string ReservationNumber, string SMTPDefaultCredentials,string attachmentname)
        {
            try
            {
                if (string.IsNullOrEmpty(Confirmation_no))
                    Confirmation_no = DateTime.Now.ToString("ddMMyyyyHHmmss");
                if (string.IsNullOrEmpty(FromEmail))
                    return new Models.Email.EmailResponse()
                    {
                        result = false,
                        responseMessage = "From email can not be blank"
                    };
                ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
                {
                    return true;
                };
                using (MailMessage message = createMailMessage(emailType, GuestName, Confirmation_no, Subject, FromEmail, displayFromEmail, base64Attchement, ItemName, TotalAmount, ArrivalDate, DepartureDate, ReservationNumber))
                {



                    //System.IO.File.WriteAllText(System.Web.HttpContext.Current.Request.MapPath("~\\Resources\\log.txt"), "Message created");
                    if (ToEmail != null)
                    {
                        //foreach (string toEmail in ToEmail)
                        message.To.Add(new MailAddress(ToEmail));
                    }
                    else
                    {
                        return new Models.Email.EmailResponse()
                        {
                            result = false,
                            responseMessage = "To email address is missing"
                        };
                    }
                    if (string.IsNullOrEmpty(FromEmail))
                    {
                        return new Models.Email.EmailResponse()
                        {
                            result = false,
                            responseMessage = "from email address is missing"
                        };
                    }
                    using (var smtp = new SmtpClient())
                    {
                        if (!string.IsNullOrEmpty(base64Attchement))
                        {
                            using (MemoryStream stream = new MemoryStream(Convert.FromBase64String(base64Attchement)))
                            {
                                Attachment att = new Attachment(stream, attachmentname);//System.Web.HttpContext.Current.Request.MapPath("~\\Resources\\Images\\Temp\\" + Confirmation_no + "_folio.pdf"));
                                att.ContentDisposition.Inline = true;
                                message.Attachments.Add(att);

                                smtp.UseDefaultCredentials = string.IsNullOrEmpty(SMTPDefaultCredentials) ? false : SMTPDefaultCredentials.ToUpper().Equals("TRUE") ? true : false;
                                var credentials = new NetworkCredential
                                {
                                    UserName = Username,
                                    Password = Password
                                };

                                smtp.Credentials = credentials;
                                smtp.Host = Host;
                                smtp.Port = port;

                                smtp.EnableSsl = enableSsl;
                                await smtp.SendMailAsync(message);
                                message.Dispose();
                                smtp.Dispose();
                                return new Models.Email.EmailResponse()
                                {
                                    result = true,

                                };
                            }
                        }
                        smtp.UseDefaultCredentials = string.IsNullOrEmpty(SMTPDefaultCredentials) ? false : SMTPDefaultCredentials.ToUpper().Equals("TRUE") ? true : false;
                        var credential = new NetworkCredential
                        {
                            UserName = Username,
                            Password = Password
                        };
                        smtp.UseDefaultCredentials = false;
                        smtp.Credentials = credential;
                        smtp.Host = Host;
                        smtp.Port = port;
                        smtp.EnableSsl = enableSsl;
                        smtp.Timeout = 10000;
                        smtp.DeliveryMethod = SmtpDeliveryMethod.Network;


                        await smtp.SendMailAsync(message);
                        message.Dispose();
                        smtp.Dispose();
                        return new Models.Email.EmailResponse()
                        {
                            result = true,
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        public MailMessage createMailMessage(Models.Email.EmailType emailType,string GuestName,string Confirmation_no,string Subject,string FromEmail,string displayFromEmail,string FileAttachement,string ItemName,string TotalAmount,
                                             string ArrivalDate, string DepartureDate, string ReservationNumber)
        {
            try
            {
                if (emailType == Models.Email.EmailType.Precheckedin)
                {
                    MailMessage message = new MailMessage();
                    string header_content_id = Guid.NewGuid().ToString();
                    string buton_content_id = Guid.NewGuid().ToString();
                    string header_image_path = System.Web.HttpContext.Current.Request.MapPath("~\\Resources\\Images\\checkin\\hotel-logo.png");
                    string button_image_path = System.Web.HttpContext.Current.Request.MapPath("~\\Resources\\Images\\checkin\\proceed.png");
                    string htmlBody = System.IO.File.ReadAllText(System.Web.HttpContext.Current.Request.MapPath("~\\Resources\\HTML\\welcome.html"));
                    htmlBody = htmlBody.Replace("$$HEADER_IMAGE$$", header_content_id);
                    htmlBody = htmlBody.Replace("$$BUTTON_IMAGE$$", buton_content_id);
                    htmlBody = htmlBody.Replace("$$GUEST_NAME$$", GuestName);
                    htmlBody = htmlBody.Replace("$$CONFIRMATION_NO$$", Confirmation_no);
                    if(!string.IsNullOrEmpty(ArrivalDate))
                        htmlBody = htmlBody.Replace("$$ARRIVAL_DATE$$", ArrivalDate);
                    if (!string.IsNullOrEmpty(DepartureDate))
                        htmlBody = htmlBody.Replace("$$DEPARTURE_DATE$$", DepartureDate);                    
                    if (!string.IsNullOrEmpty(ReservationNumber))
                        htmlBody = htmlBody.Replace("$$RESERVATION_NUMBER$$", ReservationNumber);
                    AlternateView avHtml = AlternateView.CreateAlternateViewFromString(htmlBody, null, MediaTypeNames.Text.Html);
                    LinkedResource inline = new LinkedResource(header_image_path, MediaTypeNames.Image.Jpeg);
                    inline.ContentId = header_content_id;
                    avHtml.LinkedResources.Add(inline);
                    inline = new LinkedResource(button_image_path, MediaTypeNames.Image.Jpeg);
                    inline.ContentId = buton_content_id;
                    avHtml.LinkedResources.Add(inline);
                    message.Subject = Subject;
                    message.IsBodyHtml = true;
                    message.AlternateViews.Add(avHtml);
                    message.From = new MailAddress(FromEmail, displayFromEmail);
                    return message;
                }
                else if (emailType == Models.Email.EmailType.CheckinSlip)
                {
                    MailMessage message = new MailMessage();
                    string header_content_id = Guid.NewGuid().ToString();
                    string buton_content_id = Guid.NewGuid().ToString();
                    string header_image_path = System.Web.HttpContext.Current.Request.MapPath("~\\Resources\\Images\\checkin\\hotel-logo.png");
                    string button_image_path = System.Web.HttpContext.Current.Request.MapPath("~\\Resources\\Images\\checkin\\proceed.png");
                    string htmlBody = System.IO.File.ReadAllText(System.Web.HttpContext.Current.Request.MapPath("~\\Resources\\HTML\\CheckinSlip.html"));
                    htmlBody = htmlBody.Replace("$$HEADER_IMAGE$$", header_content_id);
                    htmlBody = htmlBody.Replace("$$BUTTON_IMAGE$$", buton_content_id);
                    htmlBody = htmlBody.Replace("$$GUEST_NAME$$", GuestName);
                    htmlBody = htmlBody.Replace("$$CONFIRMATION_NO$$", Confirmation_no);
                    if (!string.IsNullOrEmpty(ArrivalDate))
                        htmlBody = htmlBody.Replace("$$ARRIVAL_DATE$$", ArrivalDate);
                    if (!string.IsNullOrEmpty(DepartureDate))
                        htmlBody = htmlBody.Replace("$$DEPARTURE_DATE$$", DepartureDate);
                    if (!string.IsNullOrEmpty(ReservationNumber))
                        htmlBody = htmlBody.Replace("$$RESERVATION_NUMBER$$", ReservationNumber);
                    AlternateView avHtml = AlternateView.CreateAlternateViewFromString(htmlBody, null, MediaTypeNames.Text.Html);
                    LinkedResource inline = new LinkedResource(header_image_path, MediaTypeNames.Image.Jpeg);
                    inline.ContentId = header_content_id;
                    avHtml.LinkedResources.Add(inline);
                    inline = new LinkedResource(button_image_path, MediaTypeNames.Image.Jpeg);
                    inline.ContentId = buton_content_id;
                    avHtml.LinkedResources.Add(inline);
                    message.Subject = Subject;
                    message.IsBodyHtml = true;
                    message.AlternateViews.Add(avHtml);
                    message.From = new MailAddress(FromEmail, displayFromEmail);
                    return message;
                }
                else if (emailType == Models.Email.EmailType.PreCheckedout)
                {
                    MailMessage message = new MailMessage();
                    string header_content_id = Guid.NewGuid().ToString();
                    string buton_content_id = Guid.NewGuid().ToString();
                    string header_image_path = System.Web.HttpContext.Current.Request.MapPath("~\\Resources\\Images\\checkout\\hotel-logo.png");
                    string button_image_path = System.Web.HttpContext.Current.Request.MapPath("~\\Resources\\Images\\checkout\\chkout.png");
                    string htmlBody = System.IO.File.ReadAllText(System.Web.HttpContext.Current.Request.MapPath("~\\Resources\\HTML\\check-out.html"));
                    htmlBody = htmlBody.Replace("$$HEADER_IMAGE$$", header_content_id);
                    htmlBody = htmlBody.Replace("$$BUTTON_IMAGE$$", buton_content_id);
                    htmlBody = htmlBody.Replace("$$GUEST_NAME$$", GuestName);
                    htmlBody = htmlBody.Replace("$$CONFIRMATION_NO$$", Confirmation_no);
                    if (!string.IsNullOrEmpty(ArrivalDate))
                        htmlBody = htmlBody.Replace("$$ARRIVAL_DATE$$", ArrivalDate);
                    if (!string.IsNullOrEmpty(DepartureDate))
                        htmlBody = htmlBody.Replace("$$DEPARTURE_DATE$$", DepartureDate);
                    if (!string.IsNullOrEmpty(ReservationNumber))
                        htmlBody = htmlBody.Replace("$$RESERVATION_NUMBER$$", ReservationNumber);
                    AlternateView avHtml = AlternateView.CreateAlternateViewFromString(htmlBody, null, MediaTypeNames.Text.Html);
                    LinkedResource inline = new LinkedResource(header_image_path, MediaTypeNames.Image.Jpeg);
                    inline.ContentId = header_content_id;
                    avHtml.LinkedResources.Add(inline);
                    inline = new LinkedResource(button_image_path, MediaTypeNames.Image.Jpeg);
                    inline.ContentId = buton_content_id;
                    avHtml.LinkedResources.Add(inline);
                    message.AlternateViews.Add(avHtml);
                    message.Subject = Subject;
                    message.IsBodyHtml = true;
                    message.From = new MailAddress(FromEmail, displayFromEmail);
                    return message;
                }
                else if (emailType == Models.Email.EmailType.GuestFolio)
                {
                    System.IO.File.WriteAllText(System.Web.HttpContext.Current.Request.MapPath("~\\Resources\\log.txt"), "It is Guest folio");
                    MailMessage message = new MailMessage();
                    //if (!string.IsNullOrEmpty(FileAttachement))
                    //{
                    //    using (MemoryStream stream = new MemoryStream(Convert.FromBase64String(FileAttachement)))
                    //    {
                    //        Attachment att = new Attachment(stream, System.Web.HttpContext.Current.Request.MapPath("~\\Resources\\Images\\Temp\\" + Confirmation_no + "_folio.pdf"));
                    //        att.ContentDisposition.Inline = true;
                    //        message.Attachments.Add(att);
                    //    }
                    //}
                    string header_content_id = Guid.NewGuid().ToString();
                    string buton_content_id = Guid.NewGuid().ToString();
                    string header_image_path = System.Web.HttpContext.Current.Request.MapPath("~\\Resources\\Images\\checkout\\hotel-logo.png");
                    string htmlBody = System.IO.File.ReadAllText(System.Web.HttpContext.Current.Request.MapPath("~\\Resources\\HTML\\GuestFolio.html"));
                    htmlBody = htmlBody.Replace("$$HEADER_IMAGE$$", header_content_id);
                    htmlBody = htmlBody.Replace("$$BUTTON_IMAGE$$", "");
                    htmlBody = htmlBody.Replace("$$GUEST_NAME$$", GuestName);
                    htmlBody = htmlBody.Replace("$$CONFIRMATION_NO$$", "");
                    if (!string.IsNullOrEmpty(ArrivalDate))
                        htmlBody = htmlBody.Replace("$$ARRIVAL_DATE$$", ArrivalDate);
                    if (!string.IsNullOrEmpty(DepartureDate))
                        htmlBody = htmlBody.Replace("$$DEPARTURE_DATE$$", DepartureDate);
                    if (!string.IsNullOrEmpty(ReservationNumber))
                        htmlBody = htmlBody.Replace("$$RESERVATION_NUMBER$$", ReservationNumber);
                    AlternateView avHtml = AlternateView.CreateAlternateViewFromString(htmlBody, null, MediaTypeNames.Text.Html);
                    LinkedResource inline = new LinkedResource(header_image_path, MediaTypeNames.Image.Jpeg);
                    inline.ContentId = header_content_id;
                    avHtml.LinkedResources.Add(inline);
                    message.AlternateViews.Add(avHtml);
                    message.Subject = Subject;
                    message.IsBodyHtml = true;
                    message.From = new MailAddress(FromEmail, displayFromEmail);
                    return message;
                }
                else if (emailType == Models.Email.EmailType.AcceptRequest)
                {
                    MailMessage message = new MailMessage();
                    string header_content_id = Guid.NewGuid().ToString();
                    string buton_content_id = Guid.NewGuid().ToString();
                    string header_image_path = System.Web.HttpContext.Current.Request.MapPath("~\\Resources\\Images\\checkout\\hotel-logo.png");
                    string htmlBody = System.IO.File.ReadAllText(System.Web.HttpContext.Current.Request.MapPath("~\\Resources\\HTML\\AcceptRequest.html"));
                    htmlBody = htmlBody.Replace("$$HEADER_IMAGE$$", header_content_id);
                    htmlBody = htmlBody.Replace("$$BUTTON_IMAGE$$", "");
                    htmlBody = htmlBody.Replace("$$GUEST_NAME$$", GuestName);
                    htmlBody = htmlBody.Replace("$$CONFIRMATION_NO$$", "");
                    htmlBody = htmlBody.Replace("$$ItemName$$", ItemName);
                    htmlBody = htmlBody.Replace("$$total$$", TotalAmount);
                    if (!string.IsNullOrEmpty(ArrivalDate))
                        htmlBody = htmlBody.Replace("$$ARRIVAL_DATE$$", ArrivalDate);
                    if (!string.IsNullOrEmpty(DepartureDate))
                        htmlBody = htmlBody.Replace("$$DEPARTURE_DATE$$", DepartureDate);
                    if (!string.IsNullOrEmpty(ReservationNumber))
                        htmlBody = htmlBody.Replace("$$RESERVATION_NUMBER$$", ReservationNumber);
                    AlternateView avHtml = AlternateView.CreateAlternateViewFromString(htmlBody, null, MediaTypeNames.Text.Html);
                    LinkedResource inline = new LinkedResource(header_image_path, MediaTypeNames.Image.Jpeg);
                    inline.ContentId = header_content_id;
                    avHtml.LinkedResources.Add(inline);
                    message.AlternateViews.Add(avHtml);
                    message.Subject = Subject;
                    message.IsBodyHtml = true;
                    message.From = new MailAddress(FromEmail, displayFromEmail);
                    return message;
                }
                else if (emailType == Models.Email.EmailType.RejectRequest)
                {
                    MailMessage message = new MailMessage();
                    string header_content_id = Guid.NewGuid().ToString();
                    string buton_content_id = Guid.NewGuid().ToString();
                    string header_image_path = System.Web.HttpContext.Current.Request.MapPath("~\\Resources\\Images\\checkout\\hotel-logo.png");
                    string htmlBody = System.IO.File.ReadAllText(System.Web.HttpContext.Current.Request.MapPath("~\\Resources\\HTML\\RejectRequest.html"));
                    htmlBody = htmlBody.Replace("$$HEADER_IMAGE$$", header_content_id);
                    htmlBody = htmlBody.Replace("$$BUTTON_IMAGE$$", "");
                    htmlBody = htmlBody.Replace("$$GUEST_NAME$$", GuestName);
                    htmlBody = htmlBody.Replace("$$CONFIRMATION_NO$$", "");
                    htmlBody = htmlBody.Replace("$$ItemName$$", ItemName);
                    htmlBody = htmlBody.Replace("$$total$$", TotalAmount);
                    if (!string.IsNullOrEmpty(ArrivalDate))
                        htmlBody = htmlBody.Replace("$$ARRIVAL_DATE$$", ArrivalDate);
                    if (!string.IsNullOrEmpty(DepartureDate))
                        htmlBody = htmlBody.Replace("$$DEPARTURE_DATE$$", DepartureDate);
                    if (!string.IsNullOrEmpty(ReservationNumber))
                        htmlBody = htmlBody.Replace("$$RESERVATION_NUMBER$$", ReservationNumber);
                    AlternateView avHtml = AlternateView.CreateAlternateViewFromString(htmlBody, null, MediaTypeNames.Text.Html);
                    LinkedResource inline = new LinkedResource(header_image_path, MediaTypeNames.Image.Jpeg);
                    inline.ContentId = header_content_id;
                    avHtml.LinkedResources.Add(inline);
                    message.AlternateViews.Add(avHtml);
                    message.Subject = Subject;
                    message.IsBodyHtml = true;
                    message.From = new MailAddress(FromEmail, displayFromEmail);
                    return message;
                }
                else if (emailType == Models.Email.EmailType.ServiceError)
                {
                    MailMessage message = new MailMessage();
                    string header_content_id = Guid.NewGuid().ToString();
                    string buton_content_id = Guid.NewGuid().ToString();
                    string header_image_path = System.Web.HttpContext.Current.Request.MapPath("~\\Resources\\Images\\ServiceError\\hotel-logo.png");
                    string htmlBody = System.IO.File.ReadAllText(System.Web.HttpContext.Current.Request.MapPath("~\\Resources\\HTML\\ServiceError.html"));
                    htmlBody = htmlBody.Replace("$$HEADER_IMAGE$$", header_content_id);
                    htmlBody = htmlBody.Replace("$$GUEST_NAME$$", GuestName);
                    htmlBody = htmlBody.Replace("$$ERROR_MESSAGE$$", "");
                    htmlBody = htmlBody.Replace("$$DATE_DETAILS$$", DateTime.Now.ToString("dd/MM/yyyy"));


                    AlternateView avHtml = AlternateView.CreateAlternateViewFromString(htmlBody, null, MediaTypeNames.Text.Html);
                    LinkedResource inline = new LinkedResource(header_image_path, MediaTypeNames.Image.Jpeg);
                    inline.ContentId = header_content_id;
                    avHtml.LinkedResources.Add(inline);
                    message.AlternateViews.Add(avHtml);
                    message.Subject = Subject;
                    message.IsBodyHtml = true;
                    message.From = new MailAddress(FromEmail, displayFromEmail);
                    return message;
                }
                if (emailType == Models.Email.EmailType.PayByLink)
                {
                    MailMessage message = new MailMessage();
                    string header_content_id = Guid.NewGuid().ToString();
                    string buton_content_id = Guid.NewGuid().ToString();
                    string header_image_path = System.Web.HttpContext.Current.Request.MapPath("~\\Resources\\Images\\PaybyLink\\hotel-logo.png");
                    string button_image_path = System.Web.HttpContext.Current.Request.MapPath("~\\Resources\\Images\\PaybyLink\\proceed.png");
                    string htmlBody = System.IO.File.ReadAllText(System.Web.HttpContext.Current.Request.MapPath("~\\Resources\\HTML\\PaybyLink.html"));
                    htmlBody = htmlBody.Replace("$$HEADER_IMAGE$$", header_content_id);
                    htmlBody = htmlBody.Replace("$$BUTTON_IMAGE$$", buton_content_id);
                    htmlBody = htmlBody.Replace("$$GUEST_NAME$$", GuestName);
                    htmlBody = htmlBody.Replace("$$CONFIRMATION_NO$$", Confirmation_no);
                                AlternateView avHtml = AlternateView.CreateAlternateViewFromString(htmlBody, null, MediaTypeNames.Text.Html);
                    LinkedResource inline = new LinkedResource(header_image_path, MediaTypeNames.Image.Jpeg);
                    inline.ContentId = header_content_id;
                    avHtml.LinkedResources.Add(inline);
                    inline = new LinkedResource(button_image_path, MediaTypeNames.Image.Jpeg);
                    inline.ContentId = buton_content_id;
                    avHtml.LinkedResources.Add(inline);
                    message.Subject = Subject;
                    message.IsBodyHtml = true;
                    message.AlternateViews.Add(avHtml);
                    message.From = new MailAddress(FromEmail, displayFromEmail);
                    return message;
                }
                else
                {
                    MailMessage message = new MailMessage();
                    byte[] bytes = Convert.FromBase64String(getQRCodeImage(Confirmation_no));
                    Image image;
                    int x = 0;
                    
                    {
                        if (File.Exists(System.Web.HttpContext.Current.Request.MapPath("~\\Resources\\Images\\Temp\\" + Confirmation_no + "-QrCode.jpeg")))
                            File.Delete(System.Web.HttpContext.Current.Request.MapPath("~\\Resources\\Images\\Temp\\" + Confirmation_no + "-QrCode.jpeg"));
                    }
                   
                    using (MemoryStream ms = new MemoryStream(bytes))
                    {
                        image = Image.FromStream(ms);
                        image.Save(System.Web.HttpContext.Current.Request.MapPath("~\\Resources\\Images\\Temp\\" + Confirmation_no + "-QrCode.jpeg"), ImageFormat.Jpeg);
                    }
                    
                    string header_content_id = Guid.NewGuid().ToString();
                    string qrcode_content_id = Guid.NewGuid().ToString();
                    string header_image_path = System.Web.HttpContext.Current.Request.MapPath("~\\Resources\\Images\\checkout\\hotel-logo.png");
                    string htmlBody = System.IO.File.ReadAllText(System.Web.HttpContext.Current.Request.MapPath("~\\Resources\\HTML\\ConfirmationEmail.html"));
                    htmlBody = htmlBody.Replace("$$HEADER_IMAGE$$", header_content_id);
                    htmlBody = htmlBody.Replace("$$QRCODE_IMAGE$$", qrcode_content_id);
                    htmlBody = htmlBody.Replace("$$CONFIRMATION_NO$$", Confirmation_no);
                    htmlBody = htmlBody.Replace("$$GUEST_NAME$$", GuestName);
                    if (!string.IsNullOrEmpty(ArrivalDate))
                        htmlBody = htmlBody.Replace("$$ARRIVAL_DATE$$", ArrivalDate);
                    if (!string.IsNullOrEmpty(DepartureDate))
                        htmlBody = htmlBody.Replace("$$DEPARTURE_DATE$$", DepartureDate);
                    if (!string.IsNullOrEmpty(ReservationNumber))
                        htmlBody = htmlBody.Replace("$$RESERVATION_NUMBER$$", ReservationNumber);
                    AlternateView avHtml = AlternateView.CreateAlternateViewFromString(htmlBody, null, MediaTypeNames.Text.Html);
                    LinkedResource inline = new LinkedResource(header_image_path, MediaTypeNames.Image.Jpeg);
                    inline.ContentId = header_content_id;
                    avHtml.LinkedResources.Add(inline);
                    inline = new LinkedResource(System.Web.HttpContext.Current.Request.MapPath("~\\Resources\\Images\\Temp\\"+Confirmation_no + "-QrCode.jpeg"), MediaTypeNames.Image.Jpeg);
                    inline.ContentId = qrcode_content_id;
                    avHtml.LinkedResources.Add(inline);
                    message.AlternateViews.Add(avHtml);
                    message.Subject = Subject;
                    message.IsBodyHtml = true;
                    message.From = new MailAddress(FromEmail, displayFromEmail);
                    return message;
                }
            }
            catch(Exception ex)
            {
                System.IO.File.WriteAllText(System.Web.HttpContext.Current.Request.MapPath("~\\Resources\\log.txt"),ex.ToString());
                throw ex;
            }
        }

        public string getQRCodeImage(string confirmationNo)
        {
            try
            {
                QRCodeGenerator qrGenerator = new QRCodeGenerator();
                QRCodeData qrCodeData = qrGenerator.CreateQrCode(confirmationNo, QRCodeGenerator.ECCLevel.Q);
                QRCode qrCode = new QRCode(qrCodeData);
                Bitmap qrCodeImage = qrCode.GetGraphic(20);
                System.IO.MemoryStream ms = new MemoryStream();
                qrCodeImage.Save(ms, ImageFormat.Jpeg);
                byte[] byteImage = ms.ToArray();
                var QRCodeBase64 = Convert.ToBase64String(byteImage);
                return QRCodeBase64;
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }
    }
}