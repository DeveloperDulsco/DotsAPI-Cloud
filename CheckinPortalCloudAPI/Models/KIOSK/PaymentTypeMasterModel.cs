using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CheckinPortalCloudAPI.Models.KIOSK
{
    public class PaymentTypeMasterModel
    {
        public int ID { get; set; }
        public string OperaPaymentTypeCode { get; set; }
        public string VendorPaymentTypeCode { get; set; }
    }
}