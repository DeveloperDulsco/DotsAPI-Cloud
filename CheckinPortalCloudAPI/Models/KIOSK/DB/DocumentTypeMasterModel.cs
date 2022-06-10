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
}