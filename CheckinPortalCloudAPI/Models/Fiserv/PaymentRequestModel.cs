using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CheckinPortalCloudAPI.Models.Fiserv
{
    public class PaymentRequestModel
    {
        public string merchantAccount { get; set; }
        public string ApiKey { get; set; }
        public object RequestObject { get; set; }
        public string RequestIdentifier { get; set; }
    }
    public class PaymentRequest
    {
        public string merchantAccount { get; set; } //API SECRET
       // public string Secret { get; set; }
        public string ApiKey { get; set; }
        public object RequestObject { get; set; }
        public string RequestIdentifier { get; set; } //requestype
    }

    public class CaptureRequest
    {
        public string OrginalPSPRefernce { get; set; } //orderid//hostid//ipgtransacrionid
        public decimal? Amount { get; set; } //total
        public string MerchantReference { get; set; } //merchnattransactionid
        public string Currency { get; set; } //currency
        //public string MerchantReference { get; set; }
    }
 
 

}