using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CheckinPortalCloudAPI.Models.KIOSK.DB
{
    public class ProfileDataTableModel
    {
		public string ProfileID { get; set; }
		public string ReservationNameID { get; set; }
		public string FirstName { get; set; }
		public string MiddleName { get; set; }
		public string LastName { get; set; }
		public string Email { get; set; }
		public string Phone { get; set; }
		public DateTime? BirthDate { get; set; }
		public string Nationality { get; set; }
		public string UpdatedEmail { get; set; }
		public string UpdatedPhone { get; set; }

		public string AddressLine1 { get; set; }
		public string AddressLine2 { get; set; }
		public string City { get; set; }
		public string StateCode { get; set; }
		public string PostalCode { get; set; }
		public string CountryCode { get; set; }

		public string UpdatedAddressLine1 { get; set; }
		public string UpdatedAddressLine2 { get; set; }
		public string UpdatedCity { get; set; }
		public string UpdatedStateCode { get; set; }
		public string UpdatedPostalCode { get; set; }
		public string UpdatedCountryCode { get; set; }
		public string CloudProfileDetailID { get; set; }
		public string Gender { get; set; }
	}
}