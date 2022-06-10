using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Twilio.AspNet.Common;
using Twilio.AspNet.Mvc;
using Twilio.TwiML;

namespace CheckinPortalCloudAPI.Controllers
{
    public class TwilioNotificationController : TwilioController
    {
        public TwiMLResult Index(SmsRequest incomingMessage)
        {
            var messagingResponse = new MessagingResponse();
            messagingResponse.Message("The copy cat says: " +
                                      incomingMessage.Body);
            System.IO.File.AppendAllText("test.txt",messagingResponse.ToString());
            return TwiML(messagingResponse);
        }
    }
}
