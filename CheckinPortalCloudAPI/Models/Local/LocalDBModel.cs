using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CheckinPortalCloudAPI.Models.Local.DB
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
	}

	public class BIData
    {
		public List<BISummaryArrivals> BISummaryArrivals { get; set; }
		public List<BINationalityWiseSummaryArrivals> BINationalityWiseSummaryArrivals { get; set; }
    }

	public class ReservationAdditionalDetails
	{
		public string ResID { get; set; }
		public string FieldName { get; set; }
		public string FIeldValue { get; set; }
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
		public int PrecheckoutSuccess {get;set;}
		public int RoomUpgrade { get; set; }
		public int PackageUpgrade { get; set; }

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

	public partial class PaymentDetails
	{
		public List<PushPaymentHeaderModel> paymentHeaders { get; set; }
		public List<PaymentAdditionalInfo> paymentAdditionalInfos { get; set; }
		public List<PaymentHistory> paymentHistories { get; set; }
	}

	public class PaymentHeader
	{
		public string TransactionID { get; set; }
		public string ReservationNumber { get; set; }
		public string ReservationNameID { get; set; }
		public string MaskedCardNumber { get; set; }
		//public string adjustAuthorisationData { get; set; }
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
	}

	public class PaymentTransactionDetails
	{
		public string PaymentID { get; set; }
		public string TransactionID { get; set; }
		public string ReservationNumber { get; set; }
		public string ReservationNameID { get; set; }
		public string MaskedCardNumber { get; set; }
		//public string adjustAuthorisationData { get; set; }
		public string ExpiryDate { get; set; }
		public string FundingSource { get; set; }
		public string Amount { get; set; }
		public string Currency { get; set; }
		public string RecurringIdentifier { get; set; }
		public string AuthorisationCode { get; set; }
		public string pspReferenceNumber { get; set; }
		public string ParentPspRefereceNumber { get; set; }
		public string ResultCode { get; set; }
		public string ResponseMessage { get; set; }
		public bool? IsActive { get; set; }
		public string TransactionType { get; set; }
		public string DisplayTransactionType { get; set; }		
		public string StatusType { get; set; }
		public DateTime CreatedDateTime { get; set; }
		public string CardType { get; set; }
		public string OperaPaymentTypeCode { get; set; }
		public string NotificationStatus { get; set; }
		public string AdjustAuthorisationData { get; set; }
	}

	public class PushPaymentHeaderModel
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
	}

	public class PaymentAdditionalInfo
	{
		public string TransactionID { get; set; }
		public string KeyHeader { get; set; }
		public string KeyValue { get; set; }
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
		public string CloudProfileDetailID { get; set; }
		

	}

	public class ReservationDocumentsDataTableModel
	{
		public string ReservationNameID { get; set; }
		public byte[] Document { get; set; }
		public string DocumentType { get; set; }
		

	}

	public partial class FeedBackModel
	{
		public string ReservationNameID { get; set; }
		public Nullable<int> QuestionID { get; set; }
		public string Answer { get; set; }
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

	public partial class ReservationPolicyModel
	{
		public string ReservationNameID { get; set; }
		public string RequestType { get; set; }
		public bool ReqStatus { get; set; }
		public int UserID { get; set; }
		public bool IsRoomUpsell { get; set; }
		public string PackageCode { get; set; }
		public string PackageDescription { get; set; }

	}

	public partial class UserDetails
	{
		public int UserID { get; set; }
		public string UserName { get; set; }
		public string DisplayName { get; set; }
		public string QrCode { get; set; }
		public string RoleName { get; set; }
		public string Result { get; set; }
		public string Message { get; set; }
	}


	public partial class ProfileDocumentDetailstbModel
    {
		public string ProfileDocID { get; set; }
		public string ProfileID { get; set; }
		public string ProfileDetailID { get; set; }

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

	public class CountryState
	{
		public string CountryName { get; set; }
		public string CountryCode { get; set; }
		public string StateCode { get; set; }
		public string StateName { get; set; }
	}

	public class ReservationListTypeModel
	{
		public string ReservationNameID { get; set; }
		public bool? IsPushedToCloud { get; set; }
		public string StatusDescription { get; set; }
		public bool? IsFetchedFromCloud { get; set; }
		public bool? EmailSent { get; set; }
	}

	public class ReservationTrackStatus
	{
		public string ReservationNumber { get; set; }
		public string ReservationNameID { get; set; }
		public string ProcessType { get; set; }
		public string ProcessStatus { get; set; }
		public bool? EmailSent { get; set; }
		public int? ID { get; set; }
	}

	public class DataClearResponseDataTableModel
	{
		public string ResultMessage { get; set; }
	}
	public class ReservationCompareStatus
	{
		public string ReservationNameID { get; set; }
		public DateTime? ArrivalDate { get; set; }
		public DateTime? DepartureDate { get; set; }
		public string Adultcount { get; set; }
		public string Childcount { get; set; }
		public string RoomType { get; set; }
		public string Email { get; set; }
		public string Phone { get; set; }
		public bool IsBreakFastAvailable { get; set; }
	}

	public class ReservationDueoutAmountCompare
	{
		public string ReservationDetailID { get; set; }
		public decimal? BalanceAmount { get; set; }
		public decimal? TotalAmount { get; set; }

	}
	public class RequestReservationDetail
	{
		public string ReservationNameID { get; set; }
	}
}
