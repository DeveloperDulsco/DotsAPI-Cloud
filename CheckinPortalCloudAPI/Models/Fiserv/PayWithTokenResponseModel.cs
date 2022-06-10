using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CheckinPortalCloudAPI.Models.Fiserv
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class PaymentToken
    {
        public bool reusable { get; set; }
        public bool declineDuplicates { get; set; }
        public string brand { get; set; }
        public string type { get; set; }
    }

    public class ExpiryDate
    {
        public string month { get; set; }
        public string year { get; set; }
    }

    public class PaymentCard
    {
        public ExpiryDate expiryDate { get; set; }
        public string bin { get; set; }
        public string last4 { get; set; }
        public string brand { get; set; }
    }

    public class PaymentMethodDetails
    {
        public PaymentCard paymentCard { get; set; }
        public string paymentMethodType { get; set; }
    }

    public class Components
    {
        public double subtotal { get; set; }
    }

    public class ApprovedAmount
    {
        public double total { get; set; }
        public string currency { get; set; }
        public Components components { get; set; }
    }

    public class AvsResponse
    {
        public string streetMatch { get; set; }
        public string postalCodeMatch { get; set; }
    }

    public class Processor
    {
        public string referenceNumber { get; set; }
        public string authorizationCode { get; set; }
        public string responseCode { get; set; }
        public string responseMessage { get; set; }
        public AvsResponse avsResponse { get; set; }
    }

    public class PayWithTokenResponseModel
    {
        public string clientRequestId { get; set; }
        public string apiTraceId { get; set; }
        public string ipgTransactionId { get; set; }
        public string orderId { get; set; }
        public string transactionType { get; set; }
        public PaymentToken paymentToken { get; set; }
        public string transactionOrigin { get; set; }
        public PaymentMethodDetails paymentMethodDetails { get; set; }
        public string country { get; set; }
        public string terminalId { get; set; }
        public string merchantId { get; set; }
        public int transactionTime { get; set; }
        public ApprovedAmount approvedAmount { get; set; }
        public string transactionStatus { get; set; }
        public string approvalCode { get; set; }
        public string schemeTransactionId { get; set; }
        public Processor processor { get; set; }
    }
}