//using Adyen.Service.Resource.BinLookup;
using Adyen.Service.Resource.BinLookup;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CheckinPortalCloudAPI.Models.Cloud
{
    public class CloudRequestModel
    {
        public object RequestObject { get; set; }
    }


    //public class FetchReservationRequest
    //{
    //    public string ReservationNumber { get; set; }
    //    public string ReservationNameID { get; set; }
    //    public bool? isForceFetch { get; set; }
    //    public ServiceParameters ServiceParameters { get; set; }
    //}


    public class CloudResponseModel
    {
        public object responseData { get; set; }
        public bool result { get; set; }
        public string responseMessage { get; set; }
        public int statusCode { get; set; }

    }

    public class FetchReservationRequest
    {
        public string ReservationNumber { get; set; }
        public string ReservationNameID { get; set; }
        public bool? isForceFetch { get; set; }
    }

    

    public class ReservationDocument
    {
        public string DocumentType { get; set; }
        public string DocumentBase64 { get; set; }
    }

    public class OperaReservation
    {
        public string ConfirmationNumber { get; set; } //No Need to process
        public string ReservationNumber { get; set; } //Added
        public string ReservationNameID { get; set; }
        public DateTime? ArrivalDate { get; set; } //Added
        public DateTime? DepartureDate { get; set; } //Added        
        public DateTime? CreatedDateTime { get; set; } //Added
        public int? Adults { get; set; } //Addedd
        public int? Child { get; set; } //Addedd        
        public string ReservationStatus { get; set; } //New one
        public string ComputedReservationStatus { get; set; } //New one
        public string LegNumber { get; set; }
        public string ChainCode { get; set; }
        public DateTime? ExpectedDepartureTime { get; set; }
        public DateTime? ExpectedArrivalTime { get; set; }
        public string ReservationSourceCode { get; set; }

        public string ReservationType { get; set; }
        public bool? PrintRate { get; set; }
        public bool? NoPost { get; set; }
        public bool? DoNotMoveRoom { get; set; }
        public decimal? TotalAmount { get; set; }
        public decimal? TotalTax { get; set; }
        public bool IsTaxInclusive { get; set; }
        public decimal CurrentBalance { get; set; }
        public RoomDetails RoomDetails { get; set; }
        public RateDetails RateDetails { get; set; }
        public string PartyCode { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public bool? IsPrimary { get; set; }
        public DateTime? ETA { get; set; }
        public string FlightNo { get; set; }
        public bool? IsCardDetailPresent { get; set; }
        public bool? IsDepositAvailable { get; set; }
        public bool? IsPreCheckedInPMS { get; set; }
        public bool? IsSaavyPaid { get; set; }
        public List<OperaReservation> SharerReservations { get; set; }
        public List<DepositDetail> DepositDetail { get; set; }
        public List<PreferanceDetails> PreferanceDetails { get; set; }
        public List<PackageDetails> PackageDetails { get; set; }
        public List<UserDefinedFields> userDefinedFields { get; set; }
        public List<GuestProfile> GuestProfiles { get; set; }
        public List<Alert> Alerts { get; set; }
        public bool IsMemberShipEnrolled { get; set; }

        public ReservationDocument  reservationDocument {get;set;}
        public string GuestSignature { get; set; }
        public string FolioEmail { get; set; }
        public bool? IsBreakFastAvailable { get; set; }
    }

    public class RegulaRequest
    {
        
        public string Base64Image { get; set; }
        public string ImageFormat { get; set; }
        public string OCRURL { get; set; }
        public string Tag { get; set; }
    }

    public class UserDefinedFields
    {
        public string FieldName { get; set; }
        public string FieldValue { get; set; }
    }

    public class RoomDetails
    {
        public string RoomNumber { get; set; }
        public string RoomType { get; set; } //Addedd
        public string RoomTypeDescription { get; set; } //Addedd
        public string RoomTypeShortDescription { get; set; } //Addedd
        public string RoomStatus { get; set; }
        public string RTC { get; set; }
        public string RTCDescription { get; set; }
        public string RTCShortDescription { get; set; }
    }

    public class RateDetails
    {
        public string RateCode { get; set; } //Added
        public decimal? RateAmount { get; set; } //Added

        public List<DailyRates> DailyRates { get; set; }
        public bool IsMultipleRate { get; set; }
    }

    public class DailyRates
    {
        public DateTime PostingDate { get; set; }
        public decimal Amount { get; set; }
        public string description { get; set; }
        public bool IsTaxAmount { get; set; }
    }

    public class PaymentMethod
    {
        public string PaymentType { get; set; } //Added
        public string MaskedCardNumber { get; set; }
        public string ExpiryDate { get; set; }
    }

    public class DepositDetail
    {
        public string PaymentType { get; set; }
        public string CreditCardNumber { get; set; }
        public string CardExpiryDate { get; set; }
        public decimal Amount { get; set; }
        public bool? IsCreditCardDeposit { get; set; }
    }
    public class PreferanceDetails
    {
        public string PreferanceCode { get; set; }
        public decimal? PreferanceAmount { get; set; }
    }

    public class PackageDetails
    {
        public string PackageCode { get; set; } // Addedd
        public string PackageDescription { get; set; }
        public decimal? TotalAmount { get; set; } //Addedd
        public decimal? TaxAmount { get; set; } //Addedd
        public decimal? Allowance { get; set; } //Addedd
        public decimal? TotalPackageAmount { get; set; } //Addedd
        public bool isTaxIncluded { get; set; }
        public string CurrecncyCode { get; set; }
        public List<AmountDetails> packageCharges { get; set; }
    }

    public class AmountDetails
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal? UnitAmount { get; set; }
        public decimal? TotalAmount { get; set; }
        public decimal? Tax { get; set; }
        public int Quantity { get; set; }
    }

    public class GuestProfile
    {
        public string PmsProfileID { get; set; }
        public string FamilyName { get; set; }
        public string GivenName { get; set; }
        public string GuestName { get; set; }
        public string Nationality { get; set; }
        public string Gender { get; set; }
        public string PassportNumber { get; set; }
        public string DocumentType { get; set; }
        public bool IsPrimary { get; set; }
        public string MembershipType { get; set; }
        public string MembershipNumber { get; set; }
        public string MembershipID { get; set; }
        public string MembershipName { get; set; }
        public string MembershipClass { get; set; }
        public string MembershipLevel { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public List<Phone> Phones { get; set; }
        public List<Address> Address { get; set; }
        public List<Email> Email { get; set; }
        public string BirthDate { get; set; }
        public string IssueDate { get; set; }
        public string IssueCountry { get; set; }
        public bool IsActive { get; set; }
        public string Title { get; set; }
        public string VipCode { get; set; }
        public string CloudProfileDetailID { get; set; }
    }

    public class Phone
    {
        public string phoneType { get; set; }
        public string phoneRole { get; set; }
        public long operaId { get; set; }
        public bool? primary { get; set; }
        public int? displaySequence { get; set; }
        public string PhoneNumber { get; set; }

    }

    public class Email
    {
        public string emailType { get; set; } = "EMAIL";
        public long operaId { get; set; }
        public bool? primary { get; set; }
        public int? displaySequence { get; set; }
        public string email { get; set; }

    }

    public class Address
    {
        public string addressType { get; set; }
        public long operaId { get; set; }
        public bool? primary { get; set; }
        public int? displaySequence { get; set; }
        public string address1 { get; set; }
        public string address2 { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public string country { get; set; }
        public string zip { get; set; }

    }
    public class Alert
    {
        public string AlertID { get; set; }
        public string AlertCode { get; set; }
        public string Area { get; set; }
        public string Description { get; set; }
        public bool isPrinterNotificationEnabled { get; set; }
        public bool isScreenNotificationEnabled { get; set; }
        public bool isGlobal { get; set; }
    }
    ///paybylink
    public class PayLinkRequest
    {
        public string ReservationNumber { get; set; }
        public string ReservationNameID { get; set; }
        public string LinkExpiry { get; set; }
        public string WeblinkAmount { get; set; }
        public string TransferType { get; set; }
        public string EmailId { get; set; }
        public string phonenumber { get; set; }
        public string GuestName { get; set; }
        public string UniqueKey { get; set; }
          }
    public class PayLinkFetchRequest
    {
        public string ReservationNumber { get; set; }
        public string ReservationNameID { get; set; }
        public string LinkExpiry { get; set; }
        public string WeblinkAmount { get; set; }
        public string TransferType { get; set; }
        public string EmailId { get; set; }
        public string phonenumber { get; set; }
        public string GuestName { get; set; }
        public string UniqueKey { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public int PaymentLinkRequestId { get;set; }
    }

}