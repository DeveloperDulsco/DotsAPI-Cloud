using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CheckinPortalCloudAPI.Models.Fiserv
{
    public class PaymentResponseModel
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

    public class AdditionalInfo
    {
        public string key { get; set; }
        public string value { get; set; }
    }
    public class FiservPaymentResponseModel
    {
        public string hosteddataid { get; set; }
        public string txndate_processed { get; set; }
        public string ccbin { get; set; }
        public string timezone { get; set; }
        public string oid { get; set; }
        public string cccountry { get; set; }
        public string expmonth { get; set; }
        public string endpointTransactionId { get; set; }
        public string currency { get; set; }
        public string processor_response_code { get; set; }
        public string chargetotal { get; set; }
        public string terminal_id { get; set; }
        public string approval_code { get; set; }
        public string hiddenSharedsecret { get; set; }
        public string hiddenTxndatetime { get; set; }
        public string expyear { get; set; }
        public string response_hash { get; set; }
        public string hiddenStorename { get; set; }
        public string transactionNotificationURL { get; set; }
        public string ignore_deploymentType { get; set; }
        public string tdate { get; set; }
        public string installments_interest { get; set; }
        public string ignore_refreshTime { get; set; }
        public string ccbrand { get; set; }
        public string refnumber { get; set; }
        public string txntype { get; set; }
        public string paymentMethod { get; set; }
        public string txndatetime { get; set; }
        public string cardnumber { get; set; }
        public string ipgTransactionId { get; set; }
        public string status { get; set; }
        public string MerchantRefernce { get; set; }
        public string fail_reason { get; set; }
        public string FundingSource { get; set; }



    }
}