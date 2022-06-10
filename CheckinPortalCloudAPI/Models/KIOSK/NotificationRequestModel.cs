using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CheckinPortalCloudAPI.Models.KIOSK
{
    public class NotificationRequestModel
    {
        public string ReservationNameID { get; set; }
        public string NotificationType { get; set; }
        public string NotificationMessage { get; set; }
        public int UserID { get; set; }
        public string UserName { get; set; }
        public bool isActionTaken { get; set; }
        public int? NotificationID { get; set; }
        public string DeviceIdentifier { get; set; }
    }
}