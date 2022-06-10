using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace CheckinPortalCloudAPI.Models.AdyenPayment
{
    public class AdjustAuthorizationRequest
    {
        public string merchantAccount { get; set; }
        public string originalReference { get; set; }
        public ModificationAmount modificationAmount { get; set; }
        public AdditionalData additionalData { get; set; }
        //public string reference { get; set; }

    }
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ResponseEnum
    {
        CaptureReceived = 0,
        CancelReceived = 1,
        RefundReceived = 2,
        CancelOrRefundReceived = 3,
        AdjustAuthorisationReceived = 4,
        VoidPendingRefundReceived = 5,
        Authorised = 6
    }

    public class AuthModificationResponse
    {
        public string pspReference { get; set; }
        public string response { get; set; }
        public Dictionary<string, string> additionalData { get; set; }
        public string Status { get; set; }
        [DataMember(Name = "errorCode", EmitDefaultValue = false)]
        public string ErrorCode { get; set; }
        [DataMember(Name = "errorType", EmitDefaultValue = false)]
        public string ErrorType { get; set; }
        [DataMember(Name = "message", EmitDefaultValue = false)]
        public string Message { get; set; }

    }

    public class AdditionalData
    {
        public string adjustAuthorisationData { get; set; }
    }

    public class ModificationAmount
    {
        public string currency { get; set; }
        public long value { get; set; }
    }

    public class PaymentRequest
    {
        public string merchantAccount { get; set; }
        public string ApiKey { get; set; }
        public object RequestObject { get; set; }
        public string RequestIdentifier { get; set; }
    }

    public class GetDeviceListRequest
    {
        public string merchantAccount { get; set; }
    }

    public class GetDeviceListResponse
    {
        public List<string> uniqueTerminalIds { get; set; }
        public string Status { get; set; }
        [DataMember(Name = "errorCode", EmitDefaultValue = false)]
        public string ErrorCode { get; set; }
        [DataMember(Name = "errorType", EmitDefaultValue = false)]
        public string ErrorType { get; set; }
        [DataMember(Name = "message", EmitDefaultValue = false)]
        public string Message { get; set; }
    }

    public class CancelRequest
    {
        public string OrginalPSPRefernce { get; set; }
        public string MerchantReference { get; set; }
    }

    public class CaptureRequest
    {
        public string OrginalPSPRefernce { get; set; }
        public decimal? Amount { get; set; }
        public string adjustAuthorisationData { get; set; }
        //public string MerchantReference { get; set; }
    }

    public class adjustAuthorisationDataClass
    {
        public string adjustAuthorisationData { get; set; }
    }

    public class TerminalRequest
    {
        public SaleToPOIRequest SaleToPOIRequest { get; set; }
    }

    public class SaleToPOIRequest
    {
        public MessageHeader MessageHeader { get; set; }
        public CardAcquisitionRequest CardAcquisitionRequest { get; set; }
        public POIPaymentRequest PaymentRequest { get; set; }
    }

    public class POIPaymentRequest
    {
        public SaleData SaleData { get; set; }
        public PaymentTransaction PaymentTransaction { get; set; }
        public PaymentData PaymentData { get; set; }
    }

    public class PaymentTransaction
    {
        public AmountsReq AmountsReq { get; set; }
        public TransactionConditions TransactionConditions { get; set; }
    }

    public class AmountsReq
    {
        public string Currency { get; set; }
        public decimal RequestedAmount { get; set; }
    }

    public class TransactionConditions
    {
        public string[] AllowedPaymentBrand { get; set; }
    }

    public class PaymentData
    {
        public TransactionIdentification CardAcquisitionReference { get; set; }
    }

    public class TransactionIdentification
    {
        public string TransactionID { get; set; }
        public string TimeStamp { get; set; }
    }

    public class MessageHeader
    {
        public string ProtocolVersion = "3.0";
        public string MessageClass { get; set; }
        public string MessageCategory { get; set; }
        public string MessageType { get; set; }
        public string ServiceID { get; set; }
        public string SaleID { get; set; }
        public string POIID { get; set; }
    }

    public class CardAcquisitionRequest
    {
        public SaleData SaleData { get; set; }
        public CardAcquisitionTransaction CardAcquisitionTransaction { get; set; }
    }

    public class SaleData
    {
        public SaleTransactionID SaleTransactionID { get; set; }
        public string TokenRequestedType { get; set; }
        public string SaleReferenceID { get; set; }
        public string SaleToAcquirerData { get; set; }
    }

    public class AdyenSaleToPOIResponse
    {
        public Adyen.Model.Nexo.SaleToPOIResponse SaleToPOIResponse { get; set; }
    }

    public class SaleTransactionID
    {
        public string TransactionID { get; set; }
        public string TimeStamp { get; set; }
    }

    public class CardAcquisitionTransaction
    {

    }

    public enum TokenRequestedType
    {
        Customer
    }

    public enum MessageClass
    {
        Service
    }

    public enum MessageCategory
    {
        CardAcquisition
    }

    public enum MessageType
    {
        Request
    }


    public class OrginKeyRequest
    {
        public string originDomains { get; set; }
    }

    public class CostEstRequest
    {
        public decimal? Amount { get; set; }
        public string Currency { get; set; }
        public string EncryptedCard { get; set; }
        public string MCC { get; set; }

    }

    public class PaymentReceipt
    {
        public List<ReceiptItem> receiptItems { get; set; }
        public PaymentReceiptType PaymentReceiptType { get; set; }
        public bool isSignatureRequired { get; set; }

    }

    public class ReceiptItem
    {
        public string ItemValue { get; set; }
        public string ItemName { get; set; }
        public string ItemKey { get; set; }
    }

    public enum PaymentReceiptType
    {
        CustomerCopy, MerchantCopy
    }

    public class MakePaymentRequest
    {
        public decimal? Amount { get; set; }
        public string Currency { get; set; }
        public BrowserInfo BrowserInfo { get; set; }
        public TransactionType TransactionType { get; set; }
        public bool? isEnableOneClick { get; set; }
        public bool? isRecurringEnable { get; set; }
        public string MCC { get; set; }
        public string MerchantReference { get; set; }
        public string Prev_PSPRefernce { get; set; }
        public Adyen.Model.Checkout.DefaultPaymentMethodDetails PaymentMethod { get; set; }
        public string RefernceUniqueID { get; set; }
        public string ReturnUrl { get; set; }
        public string ReservationNameID { get; set; }
        public string TerminalPOIID { get; set; }
        public string TerminalID { get; set; }
        public PaymentTypes? PaymentTypes { get; set; }
        public string CardAquisitionID { get; set; }
        public string Token { get; set; }
    }

    public class POIRequest
    {
        public Adyen.Model.Nexo.Message.SaleToPOIRequest SaleToPOIRequest { get; set; }
    }

    public class PaymentDetailsRequestModel
    {
        public Dictionary<string,string> details { get; set; }
          public string paymentData { get; set; }  
    }

    public class AdyenEcomResponse
    {
        public object ResponseObject { get; set; }
        public bool Result { get; set; }
        public string ResponseMessage { get; set; }
    }

    public class PaymentResponse
    {
        public string CardToken { get; set; }
        public string RefusalReason { get; set; }
        public string CardExpiryDate { get; set; }
        public string PaymentToken { get; set; }
        public string MerchantRefernce { get; set; }
        public string AuthCode { get; set; }
        public string CardType { get; set; }
        public string FundingSource { get; set; }
        public string PspReference { get; set; }
        public string ResultCode { get; set; }
        public string MaskCardNumber { get; set; }
        public string Currency { get; set; }
        public decimal? Amount { get; set; }
        public List<AdditionalInfo> additionalInfos { get; set; }
        public List<PaymentReceipt> paymentReceipts { get; set; }
        public string CardAquisitionID { get; set; }

    }

    public class AdditionalInfo
    {
        public string key { get; set; }
        public string value { get; set; }
    }
    

    public class BrowserInfo
    {
        public string acceptHeader { get; set; }
        public int? colorDepth { get; set; }
        public bool javaEnabled { get; set; }
        public string language { get; set; }
        public int? screenHeight { get; set; }
        public int? screenWidth { get; set; }

        public int? timeZoneOffset { get; set; }

        public string userAgent { get; set; }
    }

    public enum TransactionType
    {
        PreAuth,
        Capture,
        Sale
    }

    public enum PaymentTypes
    {
        alipay,
        wechatpay_pos
    }









}