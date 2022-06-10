using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CheckinPortalCloudAPI.Models.KIOSK
{
    public class ReservationRequestModel
    {
        public string ReferenceNumber { get; set; }
        public DateTime? ArrivalDate { get; set; }
    }
}