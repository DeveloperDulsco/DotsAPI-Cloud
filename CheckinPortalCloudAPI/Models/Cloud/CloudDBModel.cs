using Adyen.Service.Resource.BinLookup;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CheckinPortalCloudAPI.Models.Cloud.DB

{
	public class OperaReservationDataTableModel
	{
		public string ReservationNameID { get; set; }
		public string ReservationNumber { get; set; }
		public DateTime? ArrivalDate { get; set; }
		public DateTime? DepartureDate { get; set; }
		public int Adultcount { get; set; }
		public int Childcount { get; set; }
		public string MembershipNo { get; set; }
		public string MembershipType { get; set; }
		public bool IsDepositAvailable { get; set; }
		public bool IsCardDetailPresent { get; set; }

		public bool IsSaavyPaid { get; set; }

		public DateTime ETA { get; set; }

		public string RoomType { get; set; }
		public string RoomTypeDescription { get; set; }
		public bool EcomPaymentStatus { get; set; }
		public bool IsCheckOutFlag { get; set; }
		public decimal AverageRoomRate { get; set; }
		public decimal TotalTax { get; set; }
		public decimal TotalAmount { get; set; }
		public decimal PaidAmount { get; set; }
		public decimal BalanceAmount { get; set; }
		public string StatusDescription { get; set; }
		public string ReservationSource { get; set; }

		public bool? IsBreakFastAvailable { get; set; }

	}

	public class BIData
	{
		public List<BISummaryArrivals> BISummaryArrivals { get; set; }
		public List<BINationalityWiseSummaryArrivals> BINationalityWiseSummaryArrivals { get; set; }
	}

	public class BISummaryArrivals
	{
		public DateTime ReportDate { get; set; }
		public int Arrivals { get; set; }
		public int PrecheckinEmailSent { get; set; }
		public int PreCheckInSuccess { get; set; }
		public int PrecheckinFailed { get; set; }
		public int Departure { get; set; }
		public int PrecheckoutEmailsent { get; set; }
		public int PrecheckoutSuccess { get; set; }
		public int RoomUpgrade { get; set; }
		public int PackageUpgrade { get; set; }

	}

	public class ReservationAdditionalDetails
	{
		public string ResID { get; set; }
		public string FieldName { get; set; }
		public string FIeldValue { get; set; }
	}

	public class BINationalityWiseSummaryArrivals
	{
		public DateTime ReportDate { get; set; }
		public string Nationality { get; set; }
		public string ReservationNumber { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public string MiddleName { get; set; }
		public string Email { get; set; }
		public string Phone { get; set; }
		public string Gender { get; set; }
		public int Arrivals { get; set; }
		public int PreCheckInSuccess { get; set; }
		public int RoomUpgrade { get; set; }

	}

	public class CountryState
	{
		public string CountryName { get; set; }
		public string CountryCode { get; set; }
		public string StateCode { get; set; }
		public string StateName { get; set; }
	}

	public class OperaDueOutReservationDataTableModel
	{
		public string ReservationNameID { get; set; }
		public string ReservationNumber { get; set; }
		public DateTime? ArrivalDate { get; set; }
		public DateTime? DepartureDate { get; set; }
		public int Adultcount { get; set; }
		public int Childcount { get; set; }
		public string MembershipNo { get; set; }
		public string MembershipType { get; set; }
		public bool IsDepositAvailable { get; set; }
		public bool IsCardDetailPresent { get; set; }

		public bool IsSaavyPaid { get; set; }

		public DateTime ETA { get; set; }

		public string RoomType { get; set; }
		public string RoomTypeDescription { get; set; }

		public decimal AverageRoomRate { get; set; }
		public decimal TotalTax { get; set; }
		public decimal TotalAmount { get; set; }

	}

	public class ReservationDocumentsDataTableModel
	{
		public string ReservationNameID { get; set; }
		public string DocumentType { get; set; }
		public byte[] Document { get; set; }

	}

	public class DataClearResponseDataTableModel
	{
		public string ResultMessage { get; set; }
	}
	public class ProfileDetailsDataTableModel
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

		public string AddressLine1 { get; set; }
		public string AddressLine2 { get; set; }
		public string City { get; set; }
		public string StateCode { get; set; }
		public string PostalCode { get; set; }
		public string CountryCode { get; set; }
		public string Gender { get; set; }

	}
	public class ProfileDocumentDetailsModel
	{
		public string ReservationNameID { get; set; }
		public string ProfileID { get; set; }
		public string DocumentTypeCode { get; set; }
		public string DocumentNumber { get; set; }
		public DateTime? ExpiryDate { get; set; }
		public DateTime? IssueDate { get; set; }
		public string IssueCountry { get; set; }
		public byte[] DocumentImage1 { get; set; }
		public byte[] DocumentImage2 { get; set; }
		public byte[] DocumentImage3 { get; set; }
		public byte[] FaceImage { get; set; }


	}

	public class ReservationListTypeModel
	{
		public string ReservationNameID { get; set; }
		public bool IsPushedToLocal { get; set; }
		public string StatusDescription { get; set; }
	}

	public partial class ProfileDocuments
	{
		public string ReservationNameID { get; set; }
		public string ProfileID { get; set; }
		public string DocumentNumber { get; set; }
		public Nullable<System.DateTime> ExpiryDate { get; set; }
		public Nullable<System.DateTime> IssueDate { get; set; }
		public byte[] DocumentImage1 { get; set; }
		public byte[] DocumentImage2 { get; set; }
		public byte[] DocumentImage3 { get; set; }
		public byte[] FaceImage { get; set; }
		public string CloudProfileDetailID { get; set; }
		public string DocumentTypeCode { get; set; }
		public string IssueCountry { get; set; }

	}

	public partial class PaymentDetails
	{
		public List<PaymentHeader> paymentHeaders { get; set; }
		public List<PaymentAdditionalInfo> paymentAdditionalInfos { get; set; }
		public List<PaymentHistory> paymentHistories { get; set; }
	}

	public class PaymentHeader
	{
		public string TransactionID { get; set; }
		public string ReservationNumber { get; set; }
		public string ReservationNameID { get; set; }
		public string MaskedCardNumber { get; set; }
		public string ExpiryDate { get; set; }
		public string FundingSource { get; set; }
		public string Amount { get; set; }
		public string Currency { get; set; }
		public string RecurringIdentifier { get; set; }
		public string AuthorisationCode { get; set; }
		public string pspReferenceNumber { get; set; }
		public string ParentPspRefereceNumber { get; set; }
		public string TransactionType { get; set; }
		public string ResultCode { get; set; }
		public string ResponseMessage { get; set; }
		public bool? IsActive { get; set; }
		public string StatusType { get; set; }
		public string CardType { get; set; }
		public string OperaPaymentTypeCode { get; set; }

	}

	public class PaymentAdditionalInfo
	{
		public string TransactionID { get; set; }
		public string KeyHeader { get; set; }
		public string KeyValue { get; set; }
	}

	public class PaymentNotification
	{
		public string EventCode { get; set; }
		public DateTime EventDate { get; set; }
		public string MerchantAccountCode { get; set; }
		public string MerchantReference { get; set; }
		public string OrginalReference { get; set; }
		public string PaymentMethod { get; set; }
		public string PspReference { get; set; }
		public decimal Amount { get; set; }
		public string Currency { get; set; }
		public string Reason { get; set; }
		public bool TransactionStatus { get; set; }
		public string NotificationJSON { get; set; }

	}

	public class TbNotificationListType
	{
		public string PspReferenceNumber { get; set; }

	}



	public class PaymentHistory
	{
		public string TransactionID { get; set; }
		public string ReservationNameID { get; set; }
		public string ReservationNumber { get; set; }
		public string PData { get; set; }
		public string MDData { get; set; }
		public string PaRes { get; set; }
		public string PSPReference { get; set; }
		public string ResultCode { get; set; }
		public string RefusalReason { get; set; }
		public string TransactionType { get; set; }
	}

	public partial class UpsellPackageModel
	{
		public string ReservationNameID { get; set; }
		public string PackageCode { get; set; }
		public string PackageName { get; set; }
		public string PackageDesc { get; set; }
		public string PackageAmount { get; set; }
		public bool? IsRoomUpsell { get; set; }
	}

	public partial class PackageMasterModel
	{
		public int? PackageID { get; set; }
		public string PackageCode { get; set; }
		public string PackageName { get; set; }
		public decimal? PackageAmount { get; set; }
		public string PackageDesc { get; set; }
		public string PackageImage { get; set; }
		public List<RoomTypeCode> RoomTypeCode { get; set; }
		public bool? isActive { get; set; }
		public bool? isRoomUpsell { get; set; }
		public string Units { get; set; }
	}

	public partial class PackageMasterDataTableModel
	{
		public int UniquePackageFlag { get; set; }
		public int PackageID { get; set; }
		public string PackageCode { get; set; }
		public string PackageName { get; set; }
		public decimal PackageAmount { get; set; }
		public string PackageDesc { get; set; }
		public byte[] PackageImage { get; set; }
		public bool isActive { get; set; }
		public bool IsRoomUpsell { get; set; }
		public int RoomTypeID { get; set; }
		public string RoomTypeCode { get; set; }
		public string RoomTypeDescription { get; set; }
		public string Units { get; set; }

	}



	public class RoomTypeCode
	{
		public string RoomCode { get; set; }
	}

	public partial class CloudFetchCheckoutReservationModel
	{
		public string ReservationNameID { get; set; }
		public string ReservationNumber { get; set; }
		public Nullable<System.DateTime> ArrivalDate { get; set; }
		public Nullable<System.DateTime> DepartureDate { get; set; }
		public Nullable<int> Adultcount { get; set; }
		public Nullable<int> Childcount { get; set; }
		public string MembershipNo { get; set; }
		public string MembershipType { get; set; }
		public Nullable<bool> IsDepositAvailable { get; set; }
		public Nullable<bool> IsCardDetailPresent { get; set; }

		public Nullable<bool> IsSaavyPaid { get; set; }

		public Nullable<bool> IsPreCheckedInPMS { get; set; }
		public string FlightNo { get; set; }
		public Nullable<System.DateTime> ETA { get; set; }

		public bool? IsMemberShipEnrolled { get; set; }
		public byte[] GuestSignature { get; set; }
		public string FolioEmail { get; set; }
	}

	public class ReservationStatusInCloud
	{
		public string MessageCode { get; set; }
		public string MessageDescription { get; set; }
		public bool? MessageStatus { get; set; }
	}

	public partial class CloudFetchReservationPolicyModel
	{
		public string ReservationNameID { get; set; }
		public string ReservationNumber { get; set; }
		public string PolicyDescription { get; set; }
		public bool? PolicyValue { get; set; }
	}

	public class CloudFetchReservationModel
	{
		public string ReservationNameID { get; set; }
		public string ReservationNumber { get; set; }
		public Nullable<System.DateTime> ArrivalDate { get; set; }
		public Nullable<System.DateTime> DepartureDate { get; set; }
		public Nullable<int> Adultcount { get; set; }
		public Nullable<int> Childcount { get; set; }
		public string MembershipNo { get; set; }
		public string MembershipType { get; set; }
		public Nullable<bool> IsDepositAvailable { get; set; }
		public Nullable<bool> IsCardDetailPresent { get; set; }

		public Nullable<bool> IsSaavyPaid { get; set; }

		public Nullable<bool> IsPreCheckedInPMS { get; set; }
		public string FlightNo { get; set; }
		public Nullable<System.DateTime> ETA { get; set; }
		public string ProfileID { get; set; }
		public string FirstName { get; set; }
		public string MiddleName { get; set; }
		public string LastName { get; set; }
		public string Email { get; set; }
		public string Phone { get; set; }
		public Nullable<System.DateTime> BirthDate { get; set; }
		public string AddressLine1 { get; set; }
		public string AddressLine2 { get; set; }
		public string City { get; set; }
		public string PostalCode { get; set; }
		public string NationalityCode { get; set; }
		public string StateCode { get; set; }
		public string CountryCode { get; set; }
		public bool? IsMemberShipEnrolled { get; set; }
		public string CloudProfileDetailID { get; set; }
		public decimal? AverageRoomRate { get; set; }
		public decimal? TotalTax { get; set; }
		public decimal? TotalAmount { get; set; }
		public byte[] GuestSignature { get; set; }
		public string RoomType { get; set; }
		public string RoomTypeDescription { get; set; }
		public string StatusDescription { get; set; }
		public string ReservationSource { get; set; }
		public string Gender { get; set; }

	}

	public partial class FeedBackModel
	{
		public string ReservationNameID { get; set; }
		public Nullable<int> QuestionID { get; set; }
		public string Answer { get; set; }
	}

	public partial class RoomTypeMaster
	{
		public int RoomTypeID { get; set; }
		public string RoomTypeCode { get; set; }
		public string RoomTypeDescription { get; set; }
	}
	public partial class PolicyMaster
	{
		public bool Ismandatory { get; set; }
		public string PolicyType { get; set; }
		public string PolicyDescription { get; set; }
	}
	public class CloudReservationModel
	{
		public int ReservationDetailID { get; set; }
		public string ReservationNameID { get; set; }
		public string ReservationNumber { get; set; }
		public Nullable<System.DateTime> ArrivalDate { get; set; }
		public Nullable<System.DateTime> DepartureDate { get; set; }
		public Nullable<int> Adultcount { get; set; }
		public Nullable<int> Childcount { get; set; }
		public string MembershipNo { get; set; }
		public string MembershipType { get; set; }
		public Nullable<bool> IsDepositAvailable { get; set; }
		public Nullable<bool> IsCardDetailPresent { get; set; }
		public Nullable<bool> IsSaavyPaid { get; set; }
		public Nullable<bool> IsPreCheckedInPMS { get; set; }

		public Nullable<bool> IsPrecheckOutPMS { get; set; }
		public Nullable<bool> IsEcomchekinPaymentStaus { get; set; }
		public Nullable<bool> IsEcomchekOUtPaymentStaus { get; set; }
		public Nullable<bool> IsUploadComplete { get; set; }
		public string FlightNo { get; set; }
		public Nullable<System.DateTime> ETA { get; set; }
		public bool? IsBreakFastAvailable { get; set; }
	}
	public class CountryMaster
	{
		public int CountryMasterID { get; set; }
		public string Country_Full_name { get; set; }
		public string Country_3Char_code { get; set; }
		public string Country_2Char_code { get; set; }
	}
	public class StateMaster
	{
		public int StateMasterID { get; set; }
		public string Statename { get; set; }
		public string StateCode { get; set; }
		public int? CountryMasterID { get; set; }
	}
	public class ReservationStatusRequestModel
	{
		public int ReservationID { get; set; }
		public string ReservationNameID { get; set; }
		public string Type { get; set; }
	}
}