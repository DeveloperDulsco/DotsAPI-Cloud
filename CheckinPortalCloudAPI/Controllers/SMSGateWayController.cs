using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Twilio;
using Twilio.Rest.Api.V2010.Account;

namespace CheckinPortalCloudAPI.Controllers
{
    public class SMSGateWayController : ApiController
    {
        [HttpGet]
        [ActionName("SendSMS")]
        public bool SendSMS()
        {
            string accountSid = "ACde07f387790767811356431b47d4922a";//Environment.GetEnvironmentVariable("TWILIO_ACCOUNT_SID");
            string authToken = "f0ed3a8702a668bf4928c72d700c350d";//Environment.GetEnvironmentVariable("TWILIO_AUTH_TOKEN");

            TwilioClient.Init(accountSid, authToken);

            var message = MessageResource.Create(
                body: "Join Earth's mightiest heroes. Like Kevin Bacon.",
                from: new Twilio.Types.PhoneNumber("SBSTest"),
                to: new Twilio.Types.PhoneNumber("+971556259878")
            );

            return true;
        }
    }
}
