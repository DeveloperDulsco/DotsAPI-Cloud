using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CheckinPortalCloudAPI.Models.KIOSK
{
    public class AuditRequestModel
    {
        public string PageName { get; set; }
        public string UserName { get; set; }
        public string AuditMessage { get; set; }
        public string GroupIdentifier { get; set; }
        public string GeneralIdentifier { get; set; }
        public string DeviceIdentifier { get; set; }
        public List<AuditJsonObject> jsonObjects { get; set; }
    }

    public class AuditJsonObject
    {
        public string OldValue { get; set; }
        public string NewValue { get; set; }
        public string FieldName { get; set; }
    }
}