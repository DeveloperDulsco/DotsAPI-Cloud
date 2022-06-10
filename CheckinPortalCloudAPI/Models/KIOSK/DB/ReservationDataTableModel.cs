using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CheckinPortalCloudAPI.Models.KIOSK.DB
{
    public class ReservationDataTableModel
    {
		public string ReservationNameID { get; set; }
		public string ReservationNumber { get; set; }
		public DateTime? ArrivalDate { get; set; }
		public DateTime? DepartureDate { get; set; }
		public int Adultcount { get; set; }
		public int Childcount { get; set; }
		public string MembershipNo { get; set; }
		public string MembershipType { get; set; }
		public bool? IsDepositAvailable { get; set; }
		public bool IsCardDetailPresent { get; set; }
		public bool IsSaavyPaid { get; set; }
		public string FlightNo { get; set; }
		public DateTime ETA { get; set; }
		public string ReservationSource { get; set; }
		public string RoomType { get; set; }
		public string RoomTypeDescription { get; set; }
		public decimal AverageRoomRate { get; set; }
		public decimal TotalTax { get; set; }
		public decimal TotalAmount { get; set; }
		public decimal PaidAmount { get; set; }
		public decimal BalanceAmount { get; set; }
		public bool IsMemberShipEnrolled { get; set; }
		public string RoomNumber { get; set; }
		public string StatusDescription { get; set; }
		public bool? IsBreakFastAvailable { get; set; }
		public bool? EmailSent { get; set; }
		public bool? ShareFlag { get; set; }
		public string CSRNumber { get; set; }
		public string ShareID { get; set; }
		public string ExternalRefNumber { get; set; }
		public string ReservationStatus { get; set; }
		
	}


}