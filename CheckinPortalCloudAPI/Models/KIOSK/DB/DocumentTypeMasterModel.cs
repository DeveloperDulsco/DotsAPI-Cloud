using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CheckinPortalCloudAPI.Models.KIOSK.DB
{
    public class DocumentTypeMasterModel
    {
        public string DocumentCode { get; set; }
        public string DocumentType { get; set; }
        public string OperaDocumentCode { get; set; }
        public string DocumentType3CharCode { get; set; }
        public string IssueCountry3CharCode { get; set; }
    }

    public class CountryCodeMasterModel
    {
        public string Country_Full_name { get; set; }
        public string Country_3Char_code { get; set; }
        public string Country_2Char_code { get; set; }
    }

    public class PaymentTypeMasterModel
    {
        public string OperaPaymentTypeCode { get; set; }
        public string VendorPaymentTypeCode { get; set; }
    }
}