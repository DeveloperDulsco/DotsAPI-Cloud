using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CheckinPortalCloudAPI.Models.KIOSK
{
	public class KiokManualAuthorizationModel
	{
		public string ReservationNameID { get; set; }
		public bool? IsAdultCountAmmended { get; set; }

		public bool? IsECTAmmended { get; set; }
		public bool? IsManualyFacialAuthrised { get; set; }
		public string ManualyFacialAuthorisedUserName { get; set; }
		
		public bool? IsManuallyRoomAssigned { get; set; }
		public string ManuallyRoomAssignedUsername { get; set; }
		public bool? IsKeyEncodedFailled { get; set; }
		public bool? IsPrintFailled { get; set; }
		public bool? IsChekedinEmailSend { get; set; }
		public bool? IsDataSendToEVA { get; set; }
		public DateTime? EVATransactionDateTime { get; set; }
		public DateTime? CreatedDateTime { get; set; }
		public string Process { get; set; }
		public bool? IscheckedOutFailed { get; set; }
		public bool? IsCheckedinFailled { get; set; }
		
	}
}