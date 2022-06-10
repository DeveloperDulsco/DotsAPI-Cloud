using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CheckinPortalCloudAPI.Models.KIOSK
{
    public class PackageMasterModel
    {
        public string PackageCode { get; set; }
        public string PackageAmount { get; set; }
        public bool? IsBreakFast { get; set; }
    }
}