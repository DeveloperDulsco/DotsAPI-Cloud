using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CheckinPortalCloudAPI.Models.Fiserv
{
    public class TransactionAmount
    {
        public string total { get; set; }
        public string currency { get; set; }
    }

    public class PaymentTokenDetails
    {
        public string value { get; set; }
        public string securityCode { get; set; }
    }

    public class PaymentMethod
    {
        public PaymentTokenDetails paymentToken { get; set; }
    }
    
    public class PayWithTokenRequestModel
    {
        public string requestType { get; set; }
        public TransactionAmount transactionAmount { get; set; }
        public PaymentMethod paymentMethod { get; set; }
    }

    public class PostAuthorizationRequestModel
    {
        public string requestType { get; set; }
        public TransactionAmount transactionAmount { get; set; }
        public SplitShipment splitShipment { get; set; }
    }
    public class PostTopUpFiserv
    {
        public string requestType { get; set; }
        public string merchantTransactionId { get; set; }
        public TransactionAmount transactionAmount { get; set; }
        public bool decrementalFlag { get; set; }
        public order order { get; set; }
}
    public class SplitShipment
    {
        public int totalCount { get; set; }
        public bool finalShipment { get; set; }
    }
public class order
{
    public string orderId { get; set; }
}
}
