using Adyen.Model.Nexo;
using Adyen.Service;
using CheckinPortalCloudAPI.Helper;
using CheckinPortalCloudAPI.Helper.Cloud;
using CheckinPortalCloudAPI.Models.Fiserv;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace CheckinPortalCloudAPI.Controllers
{
    public class PaymentController : ApiController
    {


        [HttpPost]
        [ActionName("POIMakePayment")] //Need to incoperate webclient
        public async Task<Models.AdyenPayment.AdyenEcomResponse> POIMakePayment(Models.AdyenPayment.PaymentRequest paymentRequest)
        {
            try
            {
                Models.AdyenPayment.MakePaymentRequest request = JsonConvert.DeserializeObject<Models.AdyenPayment.MakePaymentRequest>(paymentRequest.RequestObject.ToString());

                #region Nlog variable validation
                if(String.IsNullOrEmpty(request.MerchantReference) || request.MerchantReference.Split('-').Length == 0)
                {
                    new LogHelper().Debug("Merchant reference is Null or wrong format", paymentRequest.RequestIdentifier, "POIMakePayment", "API", "Payment");
                    return new Models.AdyenPayment.AdyenEcomResponse()
                    {
                        Result = false,
                        ResponseMessage = "Merchant reference is Null or wrong format"
                    };
                }
                #endregion

                new LogHelper().Debug("Processing make payment request", paymentRequest.RequestIdentifier, "POIMakePayment", "API", "Payment");

                new LogHelper().Log("Raw Payment request : "+ JsonConvert.SerializeObject(paymentRequest), paymentRequest.RequestIdentifier, "POIMakePayment", "API", "Payment");
                //if(request.PaymentTypes != null && request.PaymentTypes.Value.to)
                Models.AdyenPayment.TerminalRequest saleToPOIRequest = new Models.AdyenPayment.TerminalRequest()
                {
                    SaleToPOIRequest = new Models.AdyenPayment.SaleToPOIRequest()
                    {
                        MessageHeader = new Models.AdyenPayment.MessageHeader()
                        {
                            MessageClass = MessageClassType.Service.ToString(),
                            MessageCategory = MessageCategoryType.Payment.ToString(),
                            MessageType = MessageType.Request.ToString(),
                            SaleID = request.TerminalID,
                            ServiceID = request.RefernceUniqueID,
                            POIID = request.TerminalPOIID
                        },
                        PaymentRequest = new Models.AdyenPayment.POIPaymentRequest()
                        {
                            SaleData = new Models.AdyenPayment.SaleData()
                            {
                                SaleTransactionID = new Models.AdyenPayment.SaleTransactionID()
                                {
                                    TransactionID = request.MerchantReference,
                                    TimeStamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ")
                                },
                                TokenRequestedType = request.PaymentTypes != null && request.PaymentTypes.Value.ToString().Equals("wechatpay_pos") ?null : TokenRequestedType.Customer.ToString(),

                                SaleToAcquirerData = request.PaymentTypes != null && request.PaymentTypes.Value.ToString().Equals("wechatpay_pos") ? 
                                                            null : 
                                                        request.TransactionType.Equals(Models.AdyenPayment.TransactionType.PreAuth) ? "authorisationType=PreAuth&shopperReference=" + (string.IsNullOrEmpty(request.ReservationNameID) ? request.MerchantReference.Replace("-", "") : request.ReservationNameID) + "&recurringContract=RECURRING,ONECLICK" :
                                                       "captureDelayHours=0&shopperReference=" + (string.IsNullOrEmpty(request.ReservationNameID) ? request.MerchantReference.Replace("-", "") : request.ReservationNameID) + "&recurringContract=RECURRING,ONECLICK"

                            },
                            PaymentTransaction = new Models.AdyenPayment.PaymentTransaction()
                            {
                                AmountsReq = new Models.AdyenPayment.AmountsReq()
                                {
                                    Currency = ConfigurationManager.AppSettings["PaymentCurrency"].ToString(),
                                    RequestedAmount = request.Amount.Value
                                },
                                TransactionConditions = request.PaymentTypes != null && request.PaymentTypes.Value.ToString().Equals("wechatpay_pos") ?  new Models.AdyenPayment.TransactionConditions()
                                {
                                    AllowedPaymentBrand = new string[] { request.PaymentTypes.Value.ToString() }
                                } :null

                            },
                            PaymentData = request.PaymentTypes != null && request.PaymentTypes.Value.ToString().Equals("wechatpay_pos") ? null : new Models.AdyenPayment.PaymentData()
                            {
                                CardAcquisitionReference = (!string.IsNullOrEmpty(request.CardAquisitionID) && request.CardAquisitionID != "0") ? new Models.AdyenPayment.TransactionIdentification()
                                {
                                    TransactionID = request.CardAquisitionID,
                                    TimeStamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ")
                                } : null
                            }
                            
                        }
                    }
                };

                //Adyen.Config Config = new Adyen.Config()
                //{
                //    Environment = Adyen.Model.Enum.Environment.Live,
                //    Endpoint = ConfigurationManager.AppSettings["AdyenPOIPaymentURL"],
                //    XApiKey = paymentRequest.ApiKey,
                //    HttpRequestTimeout = 300000,
                //    CloudApiEndPoint = ConfigurationManager.AppSettings["AdyenPOIPaymentURL"]
                //};
                //Adyen.Client client = new Adyen.Client(Config);
                
                var temp1 = JsonConvert.SerializeObject(saleToPOIRequest,Formatting.None, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore
                });
                new LogHelper().Debug("Adyen Payment request : " + temp1, paymentRequest.RequestIdentifier, "POIMakePayment", "API", "Payment");
                HttpClient httpClient = null;
                if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["IsPaymentProxyEnabled"]) && (Convert.ToBoolean(ConfigurationManager.AppSettings["IsPaymentProxyEnabled"])))
                {
                    httpClient = new Helper.Helper().getProxyClient("Payment", ConfigurationManager.AppSettings["PaymentProxyURL"], ConfigurationManager.AppSettings["PaymentProxyUN"],
                        ConfigurationManager.AppSettings["PaymentProxyPSWD"]);
                }
                else
                    httpClient = new HttpClient();

                httpClient.BaseAddress = new Uri(ConfigurationManager.AppSettings["AdyenPOIPaymentURL"]);
                httpClient.DefaultRequestHeaders.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpClient.DefaultRequestHeaders.Add("x-api-key", paymentRequest.ApiKey);

                //string json_temp = JsonConvert.SerializeObject(makePaymentRequest);
                HttpContent requestContent = new StringContent(JsonConvert.SerializeObject(saleToPOIRequest, Formatting.None, new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore
                }), Encoding.UTF8, "application/json");
                HttpResponseMessage response = await httpClient.PostAsync(ConfigurationManager.AppSettings["AdyenPOIPaymentURL"] , requestContent);
                Models.AdyenPayment.PaymentResponse paymentResponseObject = new Models.AdyenPayment.PaymentResponse();
                Models.AdyenPayment.AdyenSaleToPOIResponse saleToPoiResponse = null;
                if (response != null)
                {
                    if (response.IsSuccessStatusCode)
                    {
                        var responsestr = await response.Content.ReadAsStringAsync();
                        new LogHelper().Debug("Make payment response : " + responsestr, paymentRequest.RequestIdentifier, "POIMakePayment", "API", "Payment");
                        
                        responsestr = responsestr.Replace("PaymentResponse", "MessagePayload");
                        saleToPoiResponse = JsonConvert.DeserializeObject<Models.AdyenPayment.AdyenSaleToPOIResponse>(responsestr);
                    }
                    else
                    {
                        new LogHelper().Log("Make payment failled with reason : - " + response.ReasonPhrase, paymentRequest.RequestIdentifier, "POIMakePayment", "API", "Payment");
                        new LogHelper().Debug("Make payment response : " + JsonConvert.SerializeObject(new Models.AdyenPayment.AdyenEcomResponse()
                        {
                            ResponseMessage = response.ReasonPhrase,
                            Result = false
                        }), paymentRequest.RequestIdentifier, "POIMakePayment", "API", "Payment");
                        return new Models.AdyenPayment.AdyenEcomResponse()
                        {
                            Result = false,
                            ResponseMessage = response.ReasonPhrase
                        };
                    }
                }
                else
                {
                    new LogHelper().Log("Make payment failled with reason : - Payment gateway returned blank", paymentRequest.RequestIdentifier, "POIMakePayment", "API", "Payment");
                    
                    return new Models.AdyenPayment.AdyenEcomResponse()
                    {
                        Result = false,
                        ResponseMessage = "Payment gateway returned blank"
                    };
                }
                if (saleToPoiResponse != null)
                {
                    new LogHelper().Debug("Adyen Payment response : " + JsonConvert.SerializeObject(saleToPoiResponse), paymentRequest.RequestIdentifier, "POIMakePayment", "API", "Payment");
                    Adyen.Model.Nexo.PaymentResponse PaymentResp = JsonConvert.DeserializeObject<Adyen.Model.Nexo.PaymentResponse>(JsonConvert.SerializeObject(saleToPoiResponse.SaleToPOIResponse.MessagePayload)); // (PaymentResponse)saleToPoiResponse.MessagePayload;
                    MessageHeader PaymentnRespHeader = (MessageHeader)saleToPoiResponse.SaleToPOIResponse.MessageHeader;
                    paymentResponseObject.PspReference = PaymentResp.POIData != null ? (PaymentResp.POIData.POITransactionID.TransactionID.IndexOf(".") + 1 > 0) ? PaymentResp.POIData.POITransactionID.TransactionID.Substring(PaymentResp.POIData.POITransactionID.TransactionID.IndexOf(".") + 1) : null : null;
                    if (PaymentResp.Response.Result == ResultType.Success)
                    {
                        List<Models.AdyenPayment.AdditionalInfo> additionalInfos = new List<Models.AdyenPayment.AdditionalInfo>();
                        if (PaymentResp.Response.AdditionalResponse != null)
                        {
                            foreach (string token in PaymentResp.Response.AdditionalResponse.Split('&'))
                            {
                                string[] subToken = token.Split('=');
                                if (subToken != null)
                                {
                                    Models.AdyenPayment.AdditionalInfo additional = new Models.AdyenPayment.AdditionalInfo();
                                    additional.key = HttpUtility.UrlDecode(subToken[0]);
                                    additional.value = HttpUtility.UrlDecode(subToken[1]);
                                    if (additional.key.Equals("recurring.recurringDetailReference")) 
                                        paymentResponseObject.PaymentToken = additional.value;
                                    if (additional.key.Equals("paymentMethod"))
                                        paymentResponseObject.CardType = additional.value;
                                    if (additional.key.Equals("expiryDate"))
                                        paymentResponseObject.CardExpiryDate = additional.value;
                                    if (additional.key.Equals("refusalReasonRaw"))
                                        paymentResponseObject.RefusalReason = additional.value;
                                    //if (additional.key.Equals("recurring.shopperReference"))
                                    //    paymentResponseObject.PaymentToken = additional.value;
                                    if (additional.key.Equals("fundingSource"))
                                        paymentResponseObject.FundingSource = additional.value;
                                    additionalInfos.Add(additional);
                                }
                            }
                            paymentResponseObject.additionalInfos = additionalInfos;
                        }
                        

                        if (PaymentResp.PaymentReceipt != null && PaymentResp.PaymentReceipt.Length > 0)
                        {
                            List<Models.AdyenPayment.PaymentReceipt> _paymentReceipt = new List<Models.AdyenPayment.PaymentReceipt>();
                            foreach (Adyen.Model.Nexo.PaymentReceipt paymentReceipt in PaymentResp.PaymentReceipt)
                            {
                                Models.AdyenPayment.PaymentReceipt paymentSlip = new Models.AdyenPayment.PaymentReceipt();

                                paymentSlip.isSignatureRequired = paymentReceipt.RequiredSignatureFlag != null ? (bool)paymentReceipt.RequiredSignatureFlag : false;
                                if (paymentReceipt.DocumentQualifier == DocumentQualifierType.CashierReceipt)
                                    paymentSlip.PaymentReceiptType = Models.AdyenPayment.PaymentReceiptType.MerchantCopy;
                                else
                                    paymentSlip.PaymentReceiptType = Models.AdyenPayment.PaymentReceiptType.CustomerCopy;
                                if (paymentReceipt.OutputContent != null)
                                {
                                    if (paymentReceipt.OutputContent.OutputFormat == OutputFormatType.Text)
                                    {
                                        List<Models.AdyenPayment.ReceiptItem> LReceiptItems = new List<Models.AdyenPayment.ReceiptItem>();
                                        foreach (OutputText outputText in paymentReceipt.OutputContent.OutputText)
                                        {
                                            Models.AdyenPayment.ReceiptItem receiptItem = new Models.AdyenPayment.ReceiptItem();
                                            string temp = HttpUtility.UrlDecode(outputText.Text);
                                            foreach (string str in temp.Split('&'))
                                            {
                                                string[] val = str.Split('=');
                                                switch (val[0])
                                                {
                                                    case "name":
                                                        receiptItem.ItemName = val[1];
                                                        break;
                                                    case "value":
                                                        receiptItem.ItemValue = val[1];
                                                        break;
                                                    case "key":
                                                        receiptItem.ItemKey = val[1];
                                                        break;
                                                    default: break;
                                                }
                                            }
                                            LReceiptItems.Add(receiptItem);
                                        }
                                        paymentSlip.receiptItems = LReceiptItems;
                                    }
                                }
                                _paymentReceipt.Add(paymentSlip);
                            }
                            paymentResponseObject.paymentReceipts = _paymentReceipt;
                        }

                        paymentResponseObject.Currency = PaymentResp.PaymentResult.AmountsResp != null ? PaymentResp.PaymentResult.AmountsResp.Currency : null;
                        paymentResponseObject.Amount = PaymentResp.PaymentResult.AmountsResp != null ? PaymentResp.PaymentResult.AmountsResp.AuthorizedAmount : null;
                        paymentResponseObject.AuthCode = PaymentResp.PaymentResult.PaymentAcquirerData != null ? PaymentResp.PaymentResult.PaymentAcquirerData.ApprovalCode : null;
                        paymentResponseObject.MaskCardNumber = PaymentResp.PaymentResult.PaymentInstrumentData != null ? (PaymentResp.PaymentResult.PaymentInstrumentData.CardData != null ? PaymentResp.PaymentResult.PaymentInstrumentData.CardData.MaskedPAN : null) : null;
                        paymentResponseObject.MaskCardNumber = paymentResponseObject.CardType != null && paymentResponseObject.CardType.Equals("wechatpay_pos") ? paymentResponseObject.PspReference : paymentResponseObject.MaskCardNumber;
                        paymentResponseObject.FundingSource = paymentResponseObject.CardType != null && paymentResponseObject.CardType.Equals("wechatpay_pos") ? "DEBIT" : null;
                        new LogHelper().Debug("Payment response : " + JsonConvert.SerializeObject(paymentResponseObject), paymentRequest.RequestIdentifier, "POIMakePayment", "API", "Payment");
                        new LogHelper().Debug("Make Payment response completed", paymentRequest.RequestIdentifier, "POIMakePayment", "API", "Payment");
                        return new Models.AdyenPayment.AdyenEcomResponse()
                        {
                            ResponseObject = paymentResponseObject,
                            Result = true
                        };
                    }
                    else
                    {
                        new LogHelper().Log("Make Payment failled with response "+ (!string.IsNullOrEmpty(PaymentResp.Response.AdditionalResponse) ? HttpUtility.UrlDecode(PaymentResp.Response.AdditionalResponse) : "Failled"), paymentRequest.RequestIdentifier, "POIMakePayment", "API", "Payment");
                        new LogHelper().Log("Adyen Payment response : " + JsonConvert.SerializeObject(saleToPoiResponse), paymentRequest.RequestIdentifier, "POIMakePayment", "API", "Payment");
                        return new Models.AdyenPayment.AdyenEcomResponse()
                        {
                            ResponseMessage = !string.IsNullOrEmpty(PaymentResp.Response.AdditionalResponse) ? HttpUtility.UrlDecode(PaymentResp.Response.AdditionalResponse) : "Failled",
                            ResponseObject = PaymentResp.Response,
                            Result = false
                        };
                    }
                }
                else
                {
                    new LogHelper().Log("Make Payment failled with response " + "Payment gateway responded NULL", paymentRequest.RequestIdentifier, "POIMakePayment", "API", "Payment");
                    return new Models.AdyenPayment.AdyenEcomResponse()
                    {
                        ResponseMessage = "Payment gateway responded NULL",
                        Result = false
                    };
                }
            }
            catch (Exception ex)
            {
                new LogHelper().Log("Make Payment failled with response " + "Generic Exception : " + ex.Message, paymentRequest.RequestIdentifier, "POIMakePayment", "API", "Payment");
                return new Models.AdyenPayment.AdyenEcomResponse()
                {

                    Result = false,
                    ResponseMessage = "Generic Exception : " + ex.Message
                };
            }
        }

        [HttpPost]
        [ActionName("CardAcquisition")] //Need to incoperate webclient
        public async Task<Models.AdyenPayment.AdyenEcomResponse> CardAcquisition(Models.AdyenPayment.PaymentRequest paymentRequest)
        {
            try
            {

                //return new Models.AdyenPayment.AdyenEcomResponse()
                //{
                //    Result = true,
                //    ResponseObject = new Models.AdyenPayment.PaymentResponse()
                //    {
                //        additionalInfos = null,
                //        Amount = 0,
                //        CardType = "Visa",
                //        AuthCode = null,
                //        CardAquisitionID = "0",
                //        FundingSource = "CREDIT",
                //        PspReference = ""

                //    }
                //};

                Models.AdyenPayment.MakePaymentRequest request = JsonConvert.DeserializeObject<Models.AdyenPayment.MakePaymentRequest>(paymentRequest.RequestObject.ToString());

                #region Nlog variable validation
                if (String.IsNullOrEmpty(request.MerchantReference) || request.MerchantReference.Split('-').Length == 0)
                {
                    new LogHelper().Debug("Merchant reference is Null or wrong format", paymentRequest.RequestIdentifier, "CardAcquisition", "API", "Payment");
                    return new Models.AdyenPayment.AdyenEcomResponse()
                    {
                        Result = false,
                        ResponseMessage = "Merchant reference is Null or wrong format"
                    };
                }
                #endregion

                new LogHelper().Debug("Processing Card Acquesition request", paymentRequest.RequestIdentifier, "CardAcquisition", "API", "Payment");

                new LogHelper().Log("Raw Card Acquesition request : " + JsonConvert.SerializeObject(paymentRequest), paymentRequest.RequestIdentifier, "CardAcquisition", "API", "Payment");
                HttpClient httpClient = null;
                if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["IsPaymentProxyEnabled"]) && (Convert.ToBoolean(ConfigurationManager.AppSettings["IsPaymentProxyEnabled"])))
                {
                    httpClient = new Helper.Helper().getProxyClient("Payment", ConfigurationManager.AppSettings["PaymentProxyURL"], ConfigurationManager.AppSettings["PaymentProxyUN"],
                        ConfigurationManager.AppSettings["PaymentProxyPSWD"]);
                }
                else
                    httpClient = new HttpClient();
                


                Models.AdyenPayment.TerminalRequest terminalRequest = new Models.AdyenPayment.TerminalRequest()
                {
                    SaleToPOIRequest = new Models.AdyenPayment.SaleToPOIRequest()
                    {
                        MessageHeader = new Models.AdyenPayment.MessageHeader()
                        {
                            MessageClass = Models.AdyenPayment.MessageClass.Service.ToString(),
                            MessageCategory = Models.AdyenPayment.MessageCategory.CardAcquisition.ToString(),
                            MessageType = Models.AdyenPayment.MessageType.Request.ToString(),
                            SaleID = request.TerminalID,
                            ServiceID = request.RefernceUniqueID,
                            POIID = request.TerminalPOIID
                        },
                        CardAcquisitionRequest = new Models.AdyenPayment.CardAcquisitionRequest()
                        {
                            SaleData = new Models.AdyenPayment.SaleData()
                            {
                                SaleTransactionID = new Models.AdyenPayment.SaleTransactionID()
                                {
                                    TransactionID = request.MerchantReference,
                                    TimeStamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ")
                                },
                                TokenRequestedType = Models.AdyenPayment.TokenRequestedType.Customer.ToString()
                            },
                            CardAcquisitionTransaction = new Models.AdyenPayment.CardAcquisitionTransaction()
                        }
                    }
                };


                //Adyen.Config Config = new Adyen.Config()
                //{
                //    Environment = Adyen.Model.Enum.Environment.Live,
                //    Endpoint = ConfigurationManager.AppSettings["AdyenPOIPaymentURL"],
                //    XApiKey = paymentRequest.ApiKey,
                //    HttpRequestTimeout = 300000,
                //    CloudApiEndPoint = ConfigurationManager.AppSettings["AdyenPOIPaymentURL"]
                //};
                //Adyen.Client client = new Adyen.Client(Config);

                //PosPaymentCloudApi posPaymentCloudApi = new PosPaymentCloudApi(client);
                string temp = JsonConvert.SerializeObject(terminalRequest);
                new LogHelper().Debug("Adyen Card Acquesition request : " + JsonConvert.SerializeObject(terminalRequest), paymentRequest.RequestIdentifier, "CardAcquisition", "API", "Payment");
                //SaleToPOIResponse saleToPoiResponse = posPaymentCloudApi.TerminalApiCloudSync(saleToPOIRequest);

                httpClient.BaseAddress = new Uri(ConfigurationManager.AppSettings["AdyenPOIPaymentURL"]);
                httpClient.DefaultRequestHeaders.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpClient.DefaultRequestHeaders.Add("x-api-key", paymentRequest.ApiKey);

                //string json_temp = JsonConvert.SerializeObject(makePaymentRequest);
                HttpContent requestContent = new StringContent(JsonConvert.SerializeObject(terminalRequest), Encoding.UTF8, "application/json");
                HttpResponseMessage response = await httpClient.PostAsync(ConfigurationManager.AppSettings["AdyenPOIPaymentURL"], requestContent);
                Models.AdyenPayment.PaymentResponse paymentResponseObject = new Models.AdyenPayment.PaymentResponse();
                Models.AdyenPayment.AdyenSaleToPOIResponse saleToPoiResponse = null;
                if (response != null)
                {
                    //return new Models.AdyenPayment.AdyenEcomResponse()
                    //{
                    //    Result = true,
                    //    ResponseObject = new Models.AdyenPayment.PaymentResponse()
                    //    {
                    //        additionalInfos = null,
                    //        Amount = 0,
                    //        AuthCode = null,
                    //        CardAquisitionID = null,
                    //        FundingSource = "CREDIT",
                    //        PspReference = ""

                    //    }
                    //};
                    var responsestr = await response.Content.ReadAsStringAsync();
                    if (response.IsSuccessStatusCode)
                    {
                        
                        new LogHelper().Debug("Raw card aquesition response : " + responsestr, paymentRequest.RequestIdentifier, "CardAcquisition", "API", "Payment");
                        responsestr = responsestr.Replace("CardAcquisitionResponse", "MessagePayload");
                        saleToPoiResponse = JsonConvert.DeserializeObject<Models.AdyenPayment.AdyenSaleToPOIResponse>(responsestr);
                    }
                    else
                    {
                        new LogHelper().Log("Card aquesition failled with reason : - " + response.ReasonPhrase, paymentRequest.RequestIdentifier, "CardAcquisition", "API", "Payment");
                        new LogHelper().Log("Raw card aquesition response : " + responsestr, paymentRequest.RequestIdentifier, "CardAcquisition", "API", "Payment");
                        new LogHelper().Debug("Raw card aquesition response : " + JsonConvert.SerializeObject(new Models.AdyenPayment.AdyenEcomResponse()
                        {
                            ResponseMessage = response.ReasonPhrase,
                            Result = false
                        }), paymentRequest.RequestIdentifier, "CardAcquisition", "API", "Payment");
                        return new Models.AdyenPayment.AdyenEcomResponse()
                        {
                            Result = false,
                            ResponseMessage = response.ReasonPhrase
                        };
                    }
                }
                else
                {
                    new LogHelper().Log("Card aquesition failled with reason : - Payment gateway returned blank", paymentRequest.RequestIdentifier, "CardAcquisition", "API", "Payment");
                    new LogHelper().Debug("Make payment response : " + JsonConvert.SerializeObject(new Models.AdyenPayment.AdyenEcomResponse()
                    {
                        ResponseMessage = "Payment gateway returned blank",
                        Result = false
                    }), paymentRequest.RequestIdentifier, "CardAcquisition", "API", "Payment");
                    return new Models.AdyenPayment.AdyenEcomResponse()
                    {
                        Result = false,
                        ResponseMessage = "Payment gateway returned blank"
                    };
                }



                if (saleToPoiResponse != null)
                {
                    CardAcquisitionResponse CardAcquisitionResp = JsonConvert.DeserializeObject<CardAcquisitionResponse>(JsonConvert.SerializeObject(saleToPoiResponse.SaleToPOIResponse.MessagePayload));//(CardAcquisitionResponse)saleToPoiResponse.SaleToPOIResponse.MessagePayload;
                    MessageHeader CardAcquisitionRespHeader = (MessageHeader)saleToPoiResponse.SaleToPOIResponse.MessageHeader;
                    new LogHelper().Debug("Adyen Card Acquesition response : " + JsonConvert.SerializeObject(saleToPoiResponse), paymentRequest.RequestIdentifier, "CardAcquisition", "API", "Payment");
                    paymentResponseObject.PspReference = CardAcquisitionResp.POIData != null ? (CardAcquisitionResp.POIData.POITransactionID.TransactionID.IndexOf(".") + 1 > 0) ? CardAcquisitionResp.POIData.POITransactionID.TransactionID.Substring(CardAcquisitionResp.POIData.POITransactionID.TransactionID.IndexOf(".") + 1) : null : null;
                    if (CardAcquisitionResp.Response.Result == ResultType.Success)
                    {
                        List<Models.AdyenPayment.AdditionalInfo> additionalInfos = new List<Models.AdyenPayment.AdditionalInfo>();
                        if (CardAcquisitionResp.Response.AdditionalResponse != null)
                        {
                            foreach (string token in CardAcquisitionResp.Response.AdditionalResponse.Split('&'))
                            {
                                string[] subToken = token.Split('=');
                                if (subToken != null)
                                {
                                    Models.AdyenPayment.AdditionalInfo additional = new Models.AdyenPayment.AdditionalInfo();
                                    additional.key = HttpUtility.UrlDecode(subToken[0]);
                                    additional.value = HttpUtility.UrlDecode(subToken[1]);
                                    if (additional.key.Equals("recurring.recurringDetailReference"))
                                        paymentResponseObject.PaymentToken = additional.value;
                                    if (additional.key.Equals("paymentMethod"))
                                        paymentResponseObject.CardType = additional.value;
                                    if (additional.key.Equals("expiryDate"))
                                        paymentResponseObject.CardExpiryDate = additional.value;
                                    if (additional.key.Equals("refusalReasonRaw"))
                                        paymentResponseObject.RefusalReason = additional.value;
                                    if (additional.key.Equals("recurring.shopperReference"))
                                        paymentResponseObject.PaymentToken = additional.value;
                                    if (additional.key.Equals("fundingSource"))
                                        paymentResponseObject.FundingSource = additional.value;
                                    additionalInfos.Add(additional);
                                }
                            }
                            paymentResponseObject.additionalInfos = additionalInfos;
                        }
                        try
                        {
                            if (bool.Parse(ConfigurationManager.AppSettings["IsSchemaBasedFundingSourceEnabled"].ToString()) && !string.IsNullOrEmpty(paymentResponseObject.CardType))
                            {
                                new LogHelper().Debug("Processing funding source based on schema", paymentRequest.RequestIdentifier, "CardAcquisition", "API", "Payment");
                                string tempResponse = Helper.Cloud.DBHelper.Instance.FetchCardTypeInfo(ConfigurationManager.AppSettings["SaavyConnectionString"], paymentResponseObject.CardType);
                                if (!string.IsNullOrEmpty(tempResponse))
                                {
                                    paymentResponseObject.FundingSource = tempResponse.ToUpper();
                                }
                                else
                                {
                                    new LogHelper().Debug("Processing funding source based on schema returned null from the DB", paymentRequest.RequestIdentifier, "CardAcquisition", "API", "Payment");
                                }
                            }
                        }
                        catch(Exception ex)
                        {
                            new LogHelper().Error(ex, paymentRequest.RequestIdentifier, "CardAcquisition", "API", "Payment");
                        }

                        paymentResponseObject.CardAquisitionID = CardAcquisitionResp.POIData.POITransactionID.TransactionID;
                        new LogHelper().Debug("Card Acquesition completed successfully", paymentRequest.RequestIdentifier, "CardAcquisition", "API", "Payment");
                        new LogHelper().Debug("Card Acquesition response : " + JsonConvert.SerializeObject(paymentResponseObject), paymentRequest.RequestIdentifier, "CardAcquisition", "API", "Payment");
                        return new Models.AdyenPayment.AdyenEcomResponse()
                        {
                            ResponseObject = paymentResponseObject,
                            Result = true
                        };
                    }
                    else
                    {
                        new LogHelper().Log("Card Acquesition failled with reason "+(!string.IsNullOrEmpty(CardAcquisitionResp.Response.AdditionalResponse) ? HttpUtility.UrlDecode(CardAcquisitionResp.Response.AdditionalResponse) : "Failled"), paymentRequest.RequestIdentifier, "CardAcquisition", "API", "Payment");
                        new LogHelper().Log("Adyen Card Acquesition response : " + JsonConvert.SerializeObject(saleToPoiResponse), paymentRequest.RequestIdentifier, "CardAcquisition", "API", "Payment");
                        return new Models.AdyenPayment.AdyenEcomResponse()
                        {
                            ResponseMessage = !string.IsNullOrEmpty(CardAcquisitionResp.Response.AdditionalResponse) ? HttpUtility.UrlDecode(CardAcquisitionResp.Response.AdditionalResponse) : "Failled",
                            ResponseObject = CardAcquisitionResp.Response,
                            Result = false
                        };
                    }
                }
                else
                {
                    new LogHelper().Log("Card Acquesition failled with reason Payment gateway responded NULL", paymentRequest.RequestIdentifier, "CardAcquisition", "API", "Payment");
                    return new Models.AdyenPayment.AdyenEcomResponse()
                    {
                        ResponseMessage = "Payment gateway responded NULL",
                        Result = false
                    };
                }
            }
            catch (Exception ex)
            {
                new LogHelper().Log("Card Acquesition failled with reason "+ ex.Message , paymentRequest.RequestIdentifier, "CardAcquisition", "API", "Payment");
                return new Models.AdyenPayment.AdyenEcomResponse()
                {

                    Result = false,
                    ResponseMessage = "Generic Exception : " + ex.Message
                };
            }
        }


        [HttpPost]
        [ActionName("FetchPaymentTypeMaster")]
        public async Task<Models.Local.LocalResponseModel> FetchPaymentTypeMaster()
        {
            try
            {
                List<Models.KIOSK.PaymentTypeMasterModel> resultSet = Helper.KIOSK.DBHelper.Instance.FetchPaymentTypeMasters(ConfigurationManager.AppSettings["SaavyConnectionString"]);
                if (resultSet != null)
                {
                    return new Models.Local.LocalResponseModel()
                    {
                        responseData = resultSet,
                        result = true,
                        responseMessage = "Success",
                        statusCode = 101
                    };
                }
                else
                    return new Models.Local.LocalResponseModel()
                    {
                        result = false,
                        responseMessage = "Failled to Fetch the document master",
                        statusCode = -1
                    };
            }
            catch (Exception ex)
            {
                return new Models.Local.LocalResponseModel()
                {
                    result = false,
                    responseMessage = ex.Message,
                    statusCode = -1
                };
            }

        }


        #region Payment

        [HttpPost]
        [ActionName("GetPaymentmethods")]
        public async Task<Models.AdyenPayment.AdyenEcomResponse> GetPaymentmethods(Models.AdyenPayment.PaymentRequest paymentRequest)
        {
            try
            {
                

                new LogHelper().Debug("Processing Get payment method request", paymentRequest.RequestIdentifier, "GetPaymentmethods", "API", "Payment");

                new LogHelper().Log("Raw Get payment method request : " + JsonConvert.SerializeObject(paymentRequest), paymentRequest.RequestIdentifier, "GetPaymentmethods", "API", "Payment");

                HttpClient httpClient = null;
                if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["IsPaymentProxyEnabled"]) && (Convert.ToBoolean(ConfigurationManager.AppSettings["IsPaymentProxyEnabled"])))
                {
                    httpClient = new Helper.Helper().getProxyClient("Payment", ConfigurationManager.AppSettings["PaymentProxyURL"], ConfigurationManager.AppSettings["PaymentProxyUN"],
                        ConfigurationManager.AppSettings["PaymentProxyPSWD"]);
                }
                else
                    httpClient = new HttpClient();

                httpClient.BaseAddress = new Uri(ConfigurationManager.AppSettings["AdyenCheckoutURL"]);
                httpClient.DefaultRequestHeaders.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpClient.DefaultRequestHeaders.Add("x-api-key", paymentRequest.ApiKey);
                string temp = JsonConvert.SerializeObject(paymentRequest);
                HttpContent requestContent = new StringContent(JsonConvert.SerializeObject(paymentRequest), Encoding.UTF8, "application/json");
                HttpResponseMessage response = await httpClient.PostAsync(ConfigurationManager.AppSettings["AdyenCheckoutURL"] + "/v53/paymentMethods", requestContent);
                if (response != null)
                {
                    Adyen.Model.Checkout.PaymentMethodsResponse paymentMethodsResponse = JsonConvert.DeserializeObject<Adyen.Model.Checkout.PaymentMethodsResponse>(await response.Content.ReadAsStringAsync());
                    if (response.IsSuccessStatusCode)
                    {
                        
                        new LogHelper().Debug("Adyen Get payment response : " + JsonConvert.SerializeObject(paymentMethodsResponse), paymentRequest.RequestIdentifier, "GetPaymentmethods", "API", "Payment");
                        new LogHelper().Debug("Get payment request processed successfully " , paymentRequest.RequestIdentifier, "GetPaymentmethods", "API", "Payment");
                        List<Adyen.Model.Checkout.PaymentMethodsGroup> paymentMethodsList = new List<Adyen.Model.Checkout.PaymentMethodsGroup>();
                        if (paymentMethodsResponse.Groups != null && paymentMethodsResponse.Groups.Count > 0 )
                        {
                            
                            foreach (Adyen.Model.Checkout.PaymentMethodsGroup paymentMethodsGroup in paymentMethodsResponse.Groups)
                            {
                                Adyen.Model.Checkout.PaymentMethodsGroup paymentMethods = new Adyen.Model.Checkout.PaymentMethodsGroup();
                                if(!string.IsNullOrEmpty(paymentMethodsGroup.Name) && paymentMethodsGroup.Name.ToLower().Contains("wechatpay")
                                    && paymentMethodsGroup.Types != null && paymentMethodsGroup.Types.Count > 0)
                                {
                                    paymentMethods.Name = paymentMethodsGroup.Name;
                                    paymentMethods.Types = new List<string> { "wechatpayQR" };
                                    paymentMethodsList.Add(paymentMethods);
                                }
                                else
                                {
                                    paymentMethodsList.Add(paymentMethodsGroup);
                                }
                            }
                        }
                        List<Adyen.Model.Checkout.PaymentMethod> PaymentMethodsDetailsList = new List<Adyen.Model.Checkout.PaymentMethod>();
                        if (paymentMethodsResponse.PaymentMethods != null && paymentMethodsResponse.PaymentMethods.Count > 0)
                        {
                           foreach(Adyen.Model.Checkout.PaymentMethod paymentMethod in paymentMethodsResponse.PaymentMethods)
                           {
                                if (!string.IsNullOrEmpty(paymentMethod.Name) && paymentMethod.Name.ToLower().Contains("wechat"))
                                {
                                    if (!string.IsNullOrEmpty(paymentMethod.Type) && paymentMethod.Type.ToLower().Contains("wechatpayqr"))
                                    {
                                        PaymentMethodsDetailsList.Add(paymentMethod);
                                    }
                                }
                                else
                                    PaymentMethodsDetailsList.Add(paymentMethod);
                            }
                        }
                        if (paymentMethodsList != null && paymentMethodsList.Count > 0)
                            paymentMethodsResponse.Groups = paymentMethodsList;
                        if (PaymentMethodsDetailsList != null && PaymentMethodsDetailsList.Count > 0)
                            paymentMethodsResponse.PaymentMethods = PaymentMethodsDetailsList;
                        return new Models.AdyenPayment.AdyenEcomResponse()
                        {
                            ResponseObject = paymentMethodsResponse,
                            Result = true
                        };
                    }
                    else
                    {
                        new LogHelper().Log("Get payment request failled : - " + response.ReasonPhrase, paymentRequest.RequestIdentifier, "GetPaymentmethods", "API", "Payment");
                        new LogHelper().Debug("Adyen Get payment response : " + JsonConvert.SerializeObject(paymentMethodsResponse), paymentRequest.RequestIdentifier, "GetPaymentmethods", "API", "Payment");
                        return new Models.AdyenPayment.AdyenEcomResponse()
                        {

                            Result = false,
                            ResponseMessage = "HTTP Error"
                        };
                    }
                }
                else
                {
                    new LogHelper().Log("Get payment request failled : - Payment gateway returned blank" , paymentRequest.RequestIdentifier, "GetPaymentmethods", "API", "Payment");
                    return new Models.AdyenPayment.AdyenEcomResponse()
                    {

                        Result = false,
                        ResponseMessage = "Payment gateway returned blank"
                    };
                }
            }
            catch (Exception ex)
            {
                new LogHelper().Log("Get payment request failled : - "+ex.Message, paymentRequest.RequestIdentifier, "GetPaymentmethods", "API", "Payment");
                return new Models.AdyenPayment.AdyenEcomResponse()
                {

                    Result = false,
                    ResponseMessage = "Generic Exception : " + ex.Message
                };
            }
        }

        [HttpPost]
        [ActionName("GetPaymentDetails")]
        public async Task<Models.AdyenPayment.AdyenEcomResponse> GetPaymentDetails(Models.AdyenPayment.PaymentRequest paymentRequest)
        {
            try
            {

                Models.AdyenPayment.PaymentDetailsRequestModel paymentDetailsRequestModel = JsonConvert.DeserializeObject<Models.AdyenPayment.PaymentDetailsRequestModel>(paymentRequest.RequestObject.ToString());

                Adyen.Model.Checkout.PaymentsDetailsRequest paymentsDetailsRequest = new Adyen.Model.Checkout.PaymentsDetailsRequest(paymentDetailsRequestModel.details, paymentDetailsRequestModel.paymentData);

                new LogHelper().Log("Get payment details processing the request", paymentRequest.RequestIdentifier, "GetPaymentDetails", "API", "Payment");
                new LogHelper().Debug("Raw Get payment details request : " + JsonConvert.SerializeObject(paymentRequest), paymentRequest.RequestIdentifier, "GetPaymentDetails", "API", "Payment");
                
                HttpClient httpClient = null;
                if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["IsPaymentProxyEnabled"]) && (Convert.ToBoolean(ConfigurationManager.AppSettings["IsPaymentProxyEnabled"])))
                {
                    httpClient = new Helper.Helper().getProxyClient("Payment", ConfigurationManager.AppSettings["PaymentProxyURL"], ConfigurationManager.AppSettings["PaymentProxyUN"],
                        ConfigurationManager.AppSettings["PaymentProxyPSWD"]);
                }
                else
                    httpClient = new HttpClient();

                httpClient.BaseAddress = new Uri(ConfigurationManager.AppSettings["AdyenCheckoutURL"]);
                httpClient.DefaultRequestHeaders.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpClient.DefaultRequestHeaders.Add("x-api-key", paymentRequest.ApiKey);
                string temp = JsonConvert.SerializeObject(paymentDetailsRequestModel);
                HttpContent requestContent = new StringContent(JsonConvert.SerializeObject(paymentDetailsRequestModel), Encoding.UTF8, "application/json");
                HttpResponseMessage response = await httpClient.PostAsync(ConfigurationManager.AppSettings["AdyenCheckoutURL"] + "/v53/payments/details", requestContent);
                Models.AdyenPayment.PaymentResponse paymentResponseObject = new Models.AdyenPayment.PaymentResponse();
                if (response != null)
                {
                    if (response.IsSuccessStatusCode)
                    {
                        var responsestr = await response.Content.ReadAsStringAsync();
                        Adyen.Model.Checkout.PaymentsResponse paymentsResponse = JsonConvert.DeserializeObject<Adyen.Model.Checkout.PaymentsResponse>(await response.Content.ReadAsStringAsync());
                        new LogHelper().Debug("Adyen Get payment details response : " + JsonConvert.SerializeObject(paymentsResponse), paymentRequest.RequestIdentifier, "GetPaymentDetails", "API", "Payment");
                        if (paymentsResponse != null && (paymentsResponse.ResultCode == Adyen.Model.Checkout.PaymentsResponse.ResultCodeEnum.AuthenticationFinished ||
                                                        paymentsResponse.ResultCode == Adyen.Model.Checkout.PaymentsResponse.ResultCodeEnum.Authorised || paymentsResponse.ResultCode == Adyen.Model.Checkout.PaymentsResponse.ResultCodeEnum.Received))
                        {

                            paymentResponseObject.PspReference = paymentsResponse.PspReference;
                            paymentResponseObject.MerchantRefernce = paymentsResponse.MerchantReference;
                            paymentResponseObject.ResultCode = paymentsResponse.ResultCode.ToString();
                            List<Models.AdyenPayment.AdditionalInfo> additionalInfos = new List<Models.AdyenPayment.AdditionalInfo>();
                            if (paymentsResponse.AdditionalData != null)
                            {
                                string temp_card_bin = null;
                                string temp_card_summary = null;
                                foreach (KeyValuePair<string, string> keyValuePair in paymentsResponse.AdditionalData)
                                {
                                    Models.AdyenPayment.AdditionalInfo additionalInfo = new Models.AdyenPayment.AdditionalInfo();
                                    additionalInfo.key = keyValuePair.Key;
                                    additionalInfo.value = keyValuePair.Value;
                                    switch (additionalInfo.key)
                                    {
                                        case "refusalReasonRaw":
                                            paymentResponseObject.RefusalReason = additionalInfo.value;
                                            break;
                                        case "expiryDate":
                                            paymentResponseObject.CardExpiryDate = additionalInfo.value;
                                            break;
                                        case "recurring.recurringDetailReference":
                                            paymentResponseObject.PaymentToken = additionalInfo.value;
                                            break;
                                        case "authCode":
                                            paymentResponseObject.AuthCode = additionalInfo.value;
                                            break;
                                        case "paymentMethod":
                                            paymentResponseObject.CardType = additionalInfo.value;
                                            break;
                                        case "fundingSource":
                                            paymentResponseObject.FundingSource = additionalInfo.value;
                                            break;
                                        case "authorisedAmountCurrency":
                                            paymentResponseObject.Currency = additionalInfo.value;
                                            break;
                                        case "authorisedAmountValue":
                                            paymentResponseObject.Amount = !string.IsNullOrEmpty(additionalInfo.value) ? Decimal.Divide(Convert.ToDecimal(long.Parse(additionalInfo.value)), Convert.ToDecimal(100)) : 0;
                                            break;
                                        case "cardBin":
                                            temp_card_bin = additionalInfo.value;
                                            break;
                                        case "cardSummary":
                                            temp_card_summary = additionalInfo.value;
                                            break;

                                    }
                                    additionalInfos.Add(additionalInfo);
                                    paymentResponseObject.MaskCardNumber = temp_card_bin + "xxxxxx" + temp_card_summary;
                                }
                                paymentResponseObject.additionalInfos = additionalInfos;
                            }
                            new LogHelper().Debug("Get payment details response : " + JsonConvert.SerializeObject(paymentResponseObject), paymentRequest.RequestIdentifier, "GetPaymentDetails", "API", "Payment");
                            new LogHelper().Log("Get payment details completed successfully", paymentRequest.RequestIdentifier, "GetPaymentDetails", "API", "Payment");
                            return new Models.AdyenPayment.AdyenEcomResponse()
                            {
                                ResponseObject = paymentResponseObject,
                                Result = true
                            };
                        }
                        else if (paymentsResponse != null && paymentsResponse.ResultCode == Adyen.Model.Checkout.PaymentsResponse.ResultCodeEnum.RedirectShopper)
                        {
                            new LogHelper().Debug("Get payment details response : " + JsonConvert.SerializeObject(paymentResponseObject), paymentRequest.RequestIdentifier, "GetPaymentDetails", "API", "Payment");
                            new LogHelper().Log("Get payment details completed successfully", paymentRequest.RequestIdentifier, "GetPaymentDetails", "API", "Payment");
                            return new Models.AdyenPayment.AdyenEcomResponse()
                            {
                                ResponseObject = paymentsResponse,
                                Result = true
                            };
                        }
                        else
                        {
                            if (paymentsResponse != null)
                            {
                                paymentResponseObject.PspReference = paymentsResponse.PspReference;
                                paymentResponseObject.MerchantRefernce = paymentsResponse.MerchantReference;
                                paymentResponseObject.ResultCode = paymentsResponse.ResultCode.ToString();
                                List<Models.AdyenPayment.AdditionalInfo> additionalInfos = new List<Models.AdyenPayment.AdditionalInfo>();
                                if (paymentsResponse.AdditionalData != null)
                                {
                                    string temp_card_bin = null;
                                    string temp_card_summary = null;
                                    foreach (KeyValuePair<string, string> keyValuePair in paymentsResponse.AdditionalData)
                                    {
                                        Models.AdyenPayment.AdditionalInfo additionalInfo = new Models.AdyenPayment.AdditionalInfo();
                                        additionalInfo.key = keyValuePair.Key;
                                        additionalInfo.value = keyValuePair.Value;
                                        switch (additionalInfo.key)
                                        {
                                            case "refusalReasonRaw":
                                                paymentResponseObject.RefusalReason = additionalInfo.value;
                                                break;
                                            case "expiryDate":
                                                paymentResponseObject.CardExpiryDate = additionalInfo.value;
                                                break;
                                            case "recurring.recurringDetailReference":
                                                paymentResponseObject.PaymentToken = additionalInfo.value;
                                                break;
                                            case "authCode":
                                                paymentResponseObject.AuthCode = additionalInfo.value;
                                                break;
                                            case "paymentMethod":
                                                paymentResponseObject.CardType = additionalInfo.value;
                                                break;
                                            case "fundingSource":
                                                paymentResponseObject.FundingSource = additionalInfo.value;
                                                break;
                                            case "authorisedAmountCurrency":
                                                paymentResponseObject.Currency = additionalInfo.value;
                                                break;
                                            case "authorisedAmountValue":
                                                paymentResponseObject.Amount = !string.IsNullOrEmpty(additionalInfo.value) ? Decimal.Divide(Convert.ToDecimal(long.Parse(additionalInfo.value)), Convert.ToDecimal(100)) : 0;
                                                break;
                                            case "cardBin":
                                                temp_card_bin = additionalInfo.value;
                                                break;
                                            case "cardSummary":
                                                temp_card_summary = additionalInfo.value;
                                                break;
                                        }
                                        additionalInfos.Add(additionalInfo);
                                    }
                                    paymentResponseObject.additionalInfos = additionalInfos;
                                    paymentResponseObject.MaskCardNumber = temp_card_bin + "xxxxxx" + temp_card_summary;
                                }
                                new LogHelper().Debug("Get payment details response : " + JsonConvert.SerializeObject(paymentResponseObject), paymentRequest.RequestIdentifier, "GetPaymentDetails", "API", "Payment");
                                new LogHelper().Log("Get payment details failled with reason : "+paymentsResponse.ResultCode, paymentRequest.RequestIdentifier, "GetPaymentDetails", "API", "Payment");
                                return new Models.AdyenPayment.AdyenEcomResponse()
                                {
                                    ResponseObject = paymentResponseObject,
                                    Result = false
                                };

                            }
                            else
                            {
                                new LogHelper().Log("Get payment details failled with reason : PG Responded NULL", paymentRequest.RequestIdentifier, "GetPaymentDetails", "API", "Payment");
                                return new Models.AdyenPayment.AdyenEcomResponse()
                                {
                                    ResponseMessage = "PG Responded NULL",
                                    Result = false
                                };
                            }

                        }

                    }
                    else
                    {
                        new LogHelper().Log("Get payment details failled with reason : "+response.ReasonPhrase, paymentRequest.RequestIdentifier, "GetPaymentDetails", "API", "Payment");
                        return new Models.AdyenPayment.AdyenEcomResponse()
                        {
                            Result = false,
                            ResponseMessage = response.ReasonPhrase
                        };
                    }
                }
                else
                {
                    new LogHelper().Log("Get payment details failled with reason : Payment gateway returned blank", paymentRequest.RequestIdentifier, "GetPaymentDetails", "API", "Payment");
                    return new Models.AdyenPayment.AdyenEcomResponse()
                    {

                        Result = false,
                        ResponseMessage = "Payment gateway returned blank"
                    };
                }
            }
            catch (Exception ex)
            {
                new LogHelper().Log("Get payment details failled with reason : " + ex.Message, paymentRequest.RequestIdentifier, "GetPaymentDetails", "API", "Payment");
                return new Models.AdyenPayment.AdyenEcomResponse()
                {

                    Result = false,
                    ResponseMessage = "Generic Exception : " + ex.Message
                };
            }
        }

        [HttpPost]
        [ActionName("GetCostEstimater")]
        public async Task<Models.AdyenPayment.AdyenEcomResponse> GetCostEstimater(Models.AdyenPayment.PaymentRequest paymentRequest)
        {
            try
            {
                new LogHelper().Log("Get cost estimator processing the request", paymentRequest.RequestIdentifier, "GetCostEstimater", "API", "Payment");
                new LogHelper().Debug("Raw Get cost estimator request : " + JsonConvert.SerializeObject(paymentRequest), paymentRequest.RequestIdentifier, "GetCostEstimater", "API", "Payment");
                Models.AdyenPayment.CostEstRequest costEstimaterRequest = JsonConvert.DeserializeObject<Models.AdyenPayment.CostEstRequest>(paymentRequest.RequestObject.ToString());
                long amnt = 0;
                if ((costEstimaterRequest.Amount != null) && costEstimaterRequest.Amount.ToString().Contains("."))
                {
                    string temp = "00";
                    if (costEstimaterRequest.Amount.ToString().Substring(costEstimaterRequest.Amount.ToString().IndexOf(".") + 1).Length == 1)
                        temp = costEstimaterRequest.Amount.ToString().Substring(costEstimaterRequest.Amount.ToString().IndexOf(".") + 1) + "0";
                    else if (costEstimaterRequest.Amount.ToString().Substring(costEstimaterRequest.Amount.ToString().IndexOf(".") + 1).Length > 2)
                        temp = costEstimaterRequest.Amount.ToString().Substring(costEstimaterRequest.Amount.ToString().IndexOf(".") + 1, 2);
                    else
                        temp = costEstimaterRequest.Amount.ToString().Substring(costEstimaterRequest.Amount.ToString().IndexOf(".") + 1);
                    amnt = Int64.Parse(costEstimaterRequest.Amount.ToString().Substring(0, costEstimaterRequest.Amount.ToString().IndexOf(".")) + temp);
                }
                else
                    amnt = Int64.Parse(costEstimaterRequest.Amount.ToString() + "00");//(long)PaymentReq.Amount;


                HttpClient httpClient = null;
                if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["IsPaymentProxyEnabled"]) && (Convert.ToBoolean(ConfigurationManager.AppSettings["IsPaymentProxyEnabled"])))
                {
                    httpClient = new Helper.Helper().getProxyClient("Payment", ConfigurationManager.AppSettings["PaymentProxyURL"], ConfigurationManager.AppSettings["PaymentProxyUN"],
                        ConfigurationManager.AppSettings["PaymentProxyPSWD"]);
                }
                else
                    httpClient = new HttpClient();

                httpClient.DefaultRequestHeaders.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpClient.DefaultRequestHeaders.Add("x-api-key", paymentRequest.ApiKey);
                Adyen.Model.BinLookup.CostEstimateRequest costEstimateRequest = new Adyen.Model.BinLookup.CostEstimateRequest()
                {
                    Amount = new Adyen.Model.BinLookup.Amount()
                    {
                        Currency = costEstimaterRequest.Currency != null ? costEstimaterRequest.Currency : "SGD",
                        Value = amnt
                    },
                    Assumptions = new Adyen.Model.BinLookup.CostEstimateAssumptions()
                    {
                        Assume3DSecureAuthenticated = true,
                        AssumeLevel3Data = true
                    },
                    EncryptedCard = costEstimaterRequest.EncryptedCard,
                    MerchantAccount = paymentRequest.merchantAccount,
                    MerchantDetails = new Adyen.Model.BinLookup.MerchantDetails()
                    {
                        //CountryCode = "UK",
                        EnrolledIn3DSecure = true,
                        Mcc = costEstimaterRequest.MCC
                    },
                    ShopperInteraction = Adyen.Model.Enum.ShopperInteraction.Ecommerce
                };
                new LogHelper().Debug("Adyen Get cost estimator request : " + JsonConvert.SerializeObject(costEstimateRequest), paymentRequest.RequestIdentifier, "GetCostEstimater", "API", "Payment");
                HttpContent requestContent = new StringContent(JsonConvert.SerializeObject(costEstimateRequest), Encoding.UTF8, "application/json");
                HttpResponseMessage response = await httpClient.PostAsync(ConfigurationManager.AppSettings["AdyenBinLookUpURL"] + "/v50/getCostEstimate", requestContent);

                if (response != null)
                {
                    if (response.IsSuccessStatusCode)
                    {
                        var responsestr = await response.Content.ReadAsStringAsync();
                        Adyen.Model.BinLookup.CostEstimateResponse costEstimateResponse = JsonConvert.DeserializeObject<Adyen.Model.BinLookup.CostEstimateResponse>(await response.Content.ReadAsStringAsync());
                        try
                        {
                            if (bool.Parse(ConfigurationManager.AppSettings["IsSchemaBasedFundingSourceEnabled"].ToString()) && costEstimateResponse.CardBin != null
                                && !string.IsNullOrEmpty(costEstimateResponse.CardBin.PaymentMethod))
                            {
                                new LogHelper().Debug("Processing funding source based on schema", paymentRequest.RequestIdentifier, "GetCostEstimater", "API", "Payment");
                                string tempResponse = Helper.Cloud.DBHelper.Instance.FetchCardTypeInfo(ConfigurationManager.AppSettings["ConnectionString"], costEstimateResponse.CardBin.PaymentMethod);
                                if(!string.IsNullOrEmpty(tempResponse))
                                {
                                    costEstimateResponse.CardBin.FundingSource = tempResponse.ToUpper();
                                }
                                else
                                {
                                    new LogHelper().Debug("Processing funding source based on schema returned null from the DB", paymentRequest.RequestIdentifier, "GetCostEstimater", "API", "Payment");
                                    //costEstimateResponse.CardBin.FundingSource = "DEBIT";
                                }    
                            }
                        }
                        catch(Exception ex)
                        {
                            new LogHelper().Log("Error while processing schema based funding source conversion", paymentRequest.RequestIdentifier, "GetCostEstimater", "API", "Payment");
                            new LogHelper().Error(ex, paymentRequest.RequestIdentifier, "GetCostEstimater", "API", "Payment");
                        }
                        new LogHelper().Debug("Adyen Get cost estimator response : " + JsonConvert.SerializeObject(costEstimateResponse), paymentRequest.RequestIdentifier, "GetCostEstimater", "API", "Payment");
                        new LogHelper().Log("Get cost estimator processed successfully", paymentRequest.RequestIdentifier, "GetCostEstimater", "API", "Payment");
                        return new Models.AdyenPayment.AdyenEcomResponse()
                        {
                            ResponseObject = costEstimateResponse,
                            Result = true
                        };
                    }
                    else
                    {
                        new LogHelper().Log("Get cost estimator failled with reason : - "+response.ReasonPhrase, paymentRequest.RequestIdentifier, "GetCostEstimater", "API", "Payment");
                        return new Models.AdyenPayment.AdyenEcomResponse()
                        {

                            Result = false,
                            ResponseMessage = response.ReasonPhrase
                        };
                    }

                }
                else
                {
                    new LogHelper().Log("Get cost estimator failled with reason : - Payment gateway returned blank" , paymentRequest.RequestIdentifier, "GetCostEstimater", "API", "Payment");
                    return new Models.AdyenPayment.AdyenEcomResponse()
                    {
                        Result = false,
                        ResponseMessage = "Payment gateway returned blank"
                    };
                }
            }
            catch (Exception ex)
            {
                new LogHelper().Log("Get cost estimator failled with reason : - "+ex.Message, paymentRequest.RequestIdentifier, "GetCostEstimater", "API", "Payment");
                return new Models.AdyenPayment.AdyenEcomResponse()
                {

                    Result = false,
                    ResponseMessage = "Generic Exception : " + ex.Message
                };
            }
        }
        
        [HttpPost]
        [ActionName("GetOrginKey")]
        public async Task<Models.AdyenPayment.AdyenEcomResponse> GetOrginKey(Models.AdyenPayment.PaymentRequest paymentRequest)
        {
            try
            {
                new LogHelper().Log("Get orgin key processing the request", paymentRequest.RequestIdentifier, "GetOrginKey", "API", "Payment");
                new LogHelper().Debug("Raw Get orgin key request : " + JsonConvert.SerializeObject(paymentRequest), paymentRequest.RequestIdentifier, "GetOrginKey", "API", "Payment");

                HttpClient httpClient = null;
                if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["IsPaymentProxyEnabled"]) && (Convert.ToBoolean(ConfigurationManager.AppSettings["IsPaymentProxyEnabled"])))
                {
                    httpClient = new Helper.Helper().getProxyClient("Payment", ConfigurationManager.AppSettings["PaymentProxyURL"], ConfigurationManager.AppSettings["PaymentProxyUN"],
                        ConfigurationManager.AppSettings["PaymentProxyPSWD"]);
                }
                else
                    httpClient = new HttpClient();

                httpClient.BaseAddress = new Uri(ConfigurationManager.AppSettings["AdyenCheckoutURL"]);
                httpClient.DefaultRequestHeaders.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpClient.DefaultRequestHeaders.Add("x-api-key", paymentRequest.ApiKey);
                Adyen.Model.CheckoutUtility.OriginKeysRequest originKeysRequest = new Adyen.Model.CheckoutUtility.OriginKeysRequest()
                {
                    OriginDomains = JsonConvert.DeserializeObject<List<string>>(paymentRequest.RequestObject.ToString())
                };
                new LogHelper().Debug("Adyen Get orgin key request : " + JsonConvert.SerializeObject(originKeysRequest), paymentRequest.RequestIdentifier, "GetOrginKey", "API", "Payment");
                HttpContent requestContent = new StringContent(JsonConvert.SerializeObject(originKeysRequest), Encoding.UTF8, "application/json");
                HttpResponseMessage response = await httpClient.PostAsync(ConfigurationManager.AppSettings["AdyenCheckoutURL"] + "/v53/originKeys", requestContent);

                if (response != null)
                {
                    if (response.IsSuccessStatusCode)
                    {
                        var responsestr = await response.Content.ReadAsStringAsync();
                        Adyen.Model.CheckoutUtility.OriginKeysResponse originKeysResponse = JsonConvert.DeserializeObject<Adyen.Model.CheckoutUtility.OriginKeysResponse>(await response.Content.ReadAsStringAsync());
                        new LogHelper().Debug("Adyen Get orgin key response : " + JsonConvert.SerializeObject(originKeysResponse), paymentRequest.RequestIdentifier, "GetOrginKey", "API", "Payment");
                        new LogHelper().Log("Get orgin key processed successfully", paymentRequest.RequestIdentifier, "GetOrginKey", "API", "Payment");
                        return new Models.AdyenPayment.AdyenEcomResponse()
                        {
                            ResponseObject = originKeysResponse,
                            Result = true
                        };
                    }
                    else
                    {
                        new LogHelper().Log("Get orgin key failled with reason :- "+response.ReasonPhrase, paymentRequest.RequestIdentifier, "GetOrginKey", "API", "Payment");
                        return new Models.AdyenPayment.AdyenEcomResponse()
                        {

                            Result = false,
                            ResponseMessage = response.ReasonPhrase
                        };
                    }

                }
                else
                {
                    new LogHelper().Log("Get orgin key failled with reason :- Payment gateway returned blank", paymentRequest.RequestIdentifier, "GetOrginKey", "API", "Payment");
                    return new Models.AdyenPayment.AdyenEcomResponse()
                    {
                        Result = false,
                        ResponseMessage = "Payment gateway returned blank"
                    };
                }
            }
            catch (Exception ex)
            {
                new LogHelper().Log("Get orgin key failled with reason :- "+ex.Message, paymentRequest.RequestIdentifier, "GetOrginKey", "API", "Payment");
                return new Models.AdyenPayment.AdyenEcomResponse()
                {

                    Result = false,
                    ResponseMessage = "Generic Exception : " + ex.Message
                };
            }
        }

        [HttpPost]
        [ActionName("PaymentCapture")]
        public async Task<Models.AdyenPayment.AdyenEcomResponse> PaymentCapture(Models.AdyenPayment.PaymentRequest paymentRequest)
        {
            try
            {
                new LogHelper().Log("Processing Payment capture request started", paymentRequest.RequestIdentifier, "PaymentCapture", "API", "Payment");
                new LogHelper().Debug("Raw Payment capture request : " + JsonConvert.SerializeObject(paymentRequest), paymentRequest.RequestIdentifier, "PaymentCapture", "API", "Payment");
                Models.AdyenPayment.CaptureRequest request = JsonConvert.DeserializeObject<Models.AdyenPayment.CaptureRequest>(paymentRequest.RequestObject.ToString());

                HttpClient httpClient = null;
                if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["IsPaymentProxyEnabled"]) && (Convert.ToBoolean(ConfigurationManager.AppSettings["IsPaymentProxyEnabled"])))
                {
                    httpClient = new Helper.Helper().getProxyClient("Payment", ConfigurationManager.AppSettings["PaymentProxyURL"], ConfigurationManager.AppSettings["PaymentProxyUN"],
                        ConfigurationManager.AppSettings["PaymentProxyPSWD"]);
                }
                else
                    httpClient = new HttpClient();

                //httpClient.BaseAddress = new Uri(ConfigurationManager.AppSettings["AdyenPaymentURL"]);
                httpClient.DefaultRequestHeaders.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpClient.DefaultRequestHeaders.Add("x-api-key", paymentRequest.ApiKey);

                long amnt = 0;
                if ((request.Amount != null) && request.Amount.ToString().Contains("."))
                {
                    string temp = "00";
                    if (request.Amount.ToString().Substring(request.Amount.ToString().IndexOf(".") + 1).Length == 1)
                        temp = request.Amount.ToString().Substring(request.Amount.ToString().IndexOf(".") + 1) + "0";
                    else if (request.Amount.ToString().Substring(request.Amount.ToString().IndexOf(".") + 1).Length > 2)
                        temp = request.Amount.ToString().Substring(request.Amount.ToString().IndexOf(".") + 1, 2);
                    else
                        temp = request.Amount.ToString().Substring(request.Amount.ToString().IndexOf(".") + 1);
                    amnt = Int64.Parse(request.Amount.ToString().Substring(0, request.Amount.ToString().IndexOf(".")) + temp);
                }
                else
                    amnt = Int64.Parse(request.Amount.ToString() + "00");


                Adyen.Model.Modification.CaptureRequest captureRequest = new Adyen.Model.Modification.CaptureRequest()
                {
                    MerchantAccount = paymentRequest.merchantAccount,
                    OriginalReference = request.OrginalPSPRefernce,
                    ModificationAmount = new Adyen.Model.Amount(ConfigurationManager.AppSettings["PaymentCurrency"].ToString(), amnt)
                };

                new LogHelper().Debug("Adyen Payment capture request : " + JsonConvert.SerializeObject(captureRequest), paymentRequest.RequestIdentifier, "PaymentCapture", "API", "Payment");
                HttpContent requestContent = new StringContent(JsonConvert.SerializeObject(captureRequest), Encoding.UTF8, "application/json");
                HttpResponseMessage response = await httpClient.PostAsync(ConfigurationManager.AppSettings["AdyenPaymentURL"] + "/v52/capture", requestContent);
                Models.AdyenPayment.PaymentResponse paymentResponseObject = new Models.AdyenPayment.PaymentResponse();
                if (response != null)
                {
                    if (response.IsSuccessStatusCode)
                    {
                        var responsestr = await response.Content.ReadAsStringAsync();
                        Adyen.Model.Modification.ModificationResult modificationResult = JsonConvert.DeserializeObject<Adyen.Model.Modification.ModificationResult>(await response.Content.ReadAsStringAsync());
                        new LogHelper().Debug("Adyen Payment capture response : " + JsonConvert.SerializeObject(modificationResult), paymentRequest.RequestIdentifier, "PaymentCapture", "API", "Payment");
                        if (modificationResult != null && modificationResult.Response == Adyen.Model.Enum.ResponseEnum.CaptureReceived)
                        {
                            paymentResponseObject.PspReference = modificationResult.PspReference;
                            List<Models.AdyenPayment.AdditionalInfo> additionalInfos = new List<Models.AdyenPayment.AdditionalInfo>();
                            if (modificationResult.AdditionalData != null)
                            {
                                foreach (KeyValuePair<string, string> keyValuePair in modificationResult.AdditionalData)
                                {
                                    Models.AdyenPayment.AdditionalInfo additionalInfo = new Models.AdyenPayment.AdditionalInfo();
                                    additionalInfo.key = keyValuePair.Key;
                                    additionalInfo.value = keyValuePair.Value;
                                    switch (additionalInfo.key)
                                    {
                                        case "refusalReasonRaw":
                                            paymentResponseObject.RefusalReason = additionalInfo.value;
                                            break;
                                        case "expiryDate":
                                            paymentResponseObject.CardExpiryDate = additionalInfo.value;
                                            break;
                                        case "recurring.recurringDetailReference":
                                            paymentResponseObject.PaymentToken = additionalInfo.value;
                                            break;
                                        case "authCode":
                                            paymentResponseObject.AuthCode = additionalInfo.value;
                                            break;
                                        case "paymentMethod":
                                            paymentResponseObject.CardType = additionalInfo.value;
                                            break;
                                        case "fundingSource":
                                            paymentResponseObject.FundingSource = additionalInfo.value;
                                            break;
                                        case "authorisedAmountCurrency":
                                            paymentResponseObject.Currency = additionalInfo.value;
                                            break;
                                        case "authorisedAmountValue":
                                            paymentResponseObject.Amount = !string.IsNullOrEmpty(additionalInfo.value) ? Decimal.Divide(Convert.ToDecimal(long.Parse(additionalInfo.value)), Convert.ToDecimal(100)) : 0;
                                            break;


                                    }
                                    additionalInfos.Add(additionalInfo);
                                }
                                paymentResponseObject.additionalInfos = additionalInfos;
                            }
                            
                            new LogHelper().Debug("Payment capture response : " + JsonConvert.SerializeObject(new Models.AdyenPayment.AdyenEcomResponse()
                            {
                                ResponseObject = paymentResponseObject,
                                Result = true
                            }), paymentRequest.RequestIdentifier, "PaymentCapture", "API", "Payment");
                            new LogHelper().Log("Processing Payment capture completed successfully", paymentRequest.RequestIdentifier, "PaymentCapture", "API", "Payment");
                            return new Models.AdyenPayment.AdyenEcomResponse()
                            {
                                ResponseObject = paymentResponseObject,
                                Result = true
                            };
                        }
                        else
                        {
                            

                            new LogHelper().Debug("Payment capture response : " + JsonConvert.SerializeObject(new Models.AdyenPayment.AdyenEcomResponse()
                            {
                                ResponseMessage = modificationResult.Message,
                                Result = false
                            }), paymentRequest.RequestIdentifier, "PaymentCapture", "API", "Payment");
                            new LogHelper().Log("Processing Payment capture failled with reason "+ modificationResult.Message, paymentRequest.RequestIdentifier, "PaymentCapture", "API", "Payment");
                            return new Models.AdyenPayment.AdyenEcomResponse()
                            {
                                ResponseObject = modificationResult.Message,
                                Result = false
                            };
                        }
                    }
                    else
                    {
                        new LogHelper().Debug("Payment capture response : " + JsonConvert.SerializeObject(new Models.AdyenPayment.AdyenEcomResponse()
                        {
                            ResponseMessage = response.ReasonPhrase,
                            Result = false
                        }), paymentRequest.RequestIdentifier, "PaymentCapture", "API", "Payment");
                        new LogHelper().Log("Processing Payment capture failled with reason " + response.ReasonPhrase, paymentRequest.RequestIdentifier, "PaymentCapture", "API", "Payment");
                        return new Models.AdyenPayment.AdyenEcomResponse()
                        {

                            Result = false,
                            ResponseMessage = response.ReasonPhrase
                        };
                    }

                }
                else
                {
                    new LogHelper().Debug("Payment capture response : " + JsonConvert.SerializeObject(new Models.AdyenPayment.AdyenEcomResponse()
                    {
                        ResponseMessage = "Payment gateway returned blank",
                        Result = false
                    }), paymentRequest.RequestIdentifier, "PaymentCapture", "API", "Payment");
                    new LogHelper().Log("Processing Payment capture failled with reason : - Payment gateway returned blank", paymentRequest.RequestIdentifier, "PaymentCapture", "API", "Payment");
                    return new Models.AdyenPayment.AdyenEcomResponse()
                    {
                        Result = false,
                        ResponseMessage = "Payment gateway returned blank"
                    };
                }
            }
            catch (Exception ex)
            {
                new LogHelper().Log("Processing Payment capture failled with reason : - " + ex.ToString(), paymentRequest.RequestIdentifier, "PaymentCapture", "API", "Payment");
                return new Models.AdyenPayment.AdyenEcomResponse()
                {

                    Result = false,
                    ResponseMessage = "Generic Exception : " + ex.Message
                };
            }
        }

        [HttpPost]
        [ActionName("CancelPayment")]
        public async Task<Models.AdyenPayment.AdyenEcomResponse> CancelPayment(Models.AdyenPayment.PaymentRequest paymentRequest)
        {
            try
            {
                new LogHelper().Log("Processing Cancel payment request started", paymentRequest.RequestIdentifier, "CancelPayment", "API", "Payment");
                new LogHelper().Debug("Raw cancel payment request : " + JsonConvert.SerializeObject(paymentRequest), paymentRequest.RequestIdentifier, "CancelPayment", "API", "Payment");
                Models.AdyenPayment.CancelRequest request = JsonConvert.DeserializeObject<Models.AdyenPayment.CancelRequest>(paymentRequest.RequestObject.ToString());

                HttpClient httpClient = null;
                if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["IsPaymentProxyEnabled"]) && (Convert.ToBoolean(ConfigurationManager.AppSettings["IsPaymentProxyEnabled"])))
                {
                    httpClient = new Helper.Helper().getProxyClient("Payment", ConfigurationManager.AppSettings["PaymentProxyURL"], ConfigurationManager.AppSettings["PaymentProxyUN"],
                        ConfigurationManager.AppSettings["PaymentProxyPSWD"]);
                }
                else
                    httpClient = new HttpClient();

                //httpClient.BaseAddress = new Uri(ConfigurationManager.AppSettings["AdyenPaymentURL"]);
                httpClient.DefaultRequestHeaders.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpClient.DefaultRequestHeaders.Add("x-api-key", paymentRequest.ApiKey);

                Adyen.Model.Modification.CancelRequest cancelRequest = new Adyen.Model.Modification.CancelRequest()
                {
                    MerchantAccount = paymentRequest.merchantAccount,
                    OriginalReference = request.OrginalPSPRefernce,
                    Reference = request.MerchantReference
                };

                new LogHelper().Debug("Adyen cancel payment request : " + JsonConvert.SerializeObject(cancelRequest), paymentRequest.RequestIdentifier, "CancelPayment", "API", "Payment");
                HttpContent requestContent = new StringContent(JsonConvert.SerializeObject(cancelRequest), Encoding.UTF8, "application/json");
                HttpResponseMessage response = await httpClient.PostAsync(ConfigurationManager.AppSettings["AdyenPaymentURL"] + "/v52/cancel", requestContent);
                Models.AdyenPayment.PaymentResponse paymentResponseObject = new Models.AdyenPayment.PaymentResponse();
                if (response != null)
                {
                    if (response.IsSuccessStatusCode)
                    {
                        var responsestr = await response.Content.ReadAsStringAsync();
                        Adyen.Model.Modification.ModificationResult modificationResult = JsonConvert.DeserializeObject<Adyen.Model.Modification.ModificationResult>(await response.Content.ReadAsStringAsync());
                        new LogHelper().Debug("Adyen cancel payment response : " + JsonConvert.SerializeObject(modificationResult), paymentRequest.RequestIdentifier, "CancelPayment", "API", "Payment");
                        if (modificationResult != null && modificationResult.Response == Adyen.Model.Enum.ResponseEnum.CaptureReceived)
                        {
                            paymentResponseObject.PspReference = modificationResult.PspReference;
                            List<Models.AdyenPayment.AdditionalInfo> additionalInfos = new List<Models.AdyenPayment.AdditionalInfo>();
                            if (modificationResult.AdditionalData != null)
                            {
                                foreach (KeyValuePair<string, string> keyValuePair in modificationResult.AdditionalData)
                                {
                                    Models.AdyenPayment.AdditionalInfo additionalInfo = new Models.AdyenPayment.AdditionalInfo();
                                    additionalInfo.key = keyValuePair.Key;
                                    additionalInfo.value = keyValuePair.Value;
                                    switch (additionalInfo.key)
                                    {
                                        case "refusalReasonRaw":
                                            paymentResponseObject.RefusalReason = additionalInfo.value;
                                            break;
                                        case "expiryDate":
                                            paymentResponseObject.CardExpiryDate = additionalInfo.value;
                                            break;
                                        case "recurring.recurringDetailReference":
                                            paymentResponseObject.PaymentToken = additionalInfo.value;
                                            break;
                                        case "authCode":
                                            paymentResponseObject.AuthCode = additionalInfo.value;
                                            break;
                                        case "paymentMethod":
                                            paymentResponseObject.CardType = additionalInfo.value;
                                            break;
                                        case "fundingSource":
                                            paymentResponseObject.FundingSource = additionalInfo.value;
                                            break;
                                        case "authorisedAmountCurrency":
                                            paymentResponseObject.Currency = additionalInfo.value;
                                            break;
                                        case "authorisedAmountValue":
                                            paymentResponseObject.Amount = !string.IsNullOrEmpty(additionalInfo.value) ? Decimal.Divide(Convert.ToDecimal(long.Parse(additionalInfo.value)), Convert.ToDecimal(100)) : 0;
                                            break;


                                    }
                                    additionalInfos.Add(additionalInfo);
                                }
                                paymentResponseObject.additionalInfos = additionalInfos;
                            }
                            new LogHelper().Log("Adyen cancel payment request completed successfully", paymentRequest.RequestIdentifier, "CancelPayment", "API", "Payment");
                            new LogHelper().Debug("cancel payment response : " + JsonConvert.SerializeObject(new Models.AdyenPayment.AdyenEcomResponse()
                            {
                                ResponseObject = paymentResponseObject,
                                Result = true
                            }), paymentRequest.RequestIdentifier, "CancelPayment", "API", "Payment");
                            return new Models.AdyenPayment.AdyenEcomResponse()
                            {
                                ResponseObject = paymentResponseObject,
                                Result = true
                            };
                        }
                        else
                        {
                           
                            new LogHelper().Log("Adyen cancel payment request failled with reason : - "+ modificationResult.Message, paymentRequest.RequestIdentifier, "CancelPayment", "API", "Payment");
                            new LogHelper().Debug("cancel payment response : " + JsonConvert.SerializeObject(new Models.AdyenPayment.AdyenEcomResponse()
                            {
                                ResponseMessage = modificationResult.Message,
                                Result = false
                            }), paymentRequest.RequestIdentifier, "CancelPayment", "API", "Payment");
                            return new Models.AdyenPayment.AdyenEcomResponse()
                            {
                                ResponseObject = modificationResult.Message,
                                Result = false
                            };
                        }
                    }
                    else
                    {
                        new LogHelper().Log("Adyen cancel payment request failled with reason : - " + response.ReasonPhrase, paymentRequest.RequestIdentifier, "CancelPayment", "API", "Payment");
                        new LogHelper().Debug("cancel payment response : " + JsonConvert.SerializeObject(new Models.AdyenPayment.AdyenEcomResponse()
                        {
                            Result = false,
                            ResponseMessage = response.ReasonPhrase
                        }), paymentRequest.RequestIdentifier, "CancelPayment", "API", "Payment");
                        return new Models.AdyenPayment.AdyenEcomResponse()
                        {

                            Result = false,
                            ResponseMessage = response.ReasonPhrase
                        };
                    }

                }
                else
                {
                    new LogHelper().Log("Adyen cancel payment request failled with reason : - Payment gateway returned blank", paymentRequest.RequestIdentifier, "CancelPayment", "API", "Payment");
                    new LogHelper().Debug("cancel payment response : " + JsonConvert.SerializeObject(new Models.AdyenPayment.AdyenEcomResponse()
                    {
                        Result = false,
                        ResponseMessage = "Payment gateway returned blank"
                    }), paymentRequest.RequestIdentifier, "CancelPayment", "API", "Payment");
                    return new Models.AdyenPayment.AdyenEcomResponse()
                    {
                        Result = false,
                        ResponseMessage = "Payment gateway returned blank"
                    };
                }
            }
            catch (Exception ex)
            {
                File.AppendAllText(System.Web.HttpContext.Current.Request.MapPath("~\\Resources\\CancelPayment.txt"), "Response : " + JsonConvert.SerializeObject(new Models.AdyenPayment.AdyenEcomResponse()
                {

                    Result = false,
                    ResponseMessage = "Generic Exception : " + ex.Message
                }));

                new LogHelper().Log("Adyen cancel payment request failled with reason : -"+ JsonConvert.SerializeObject(new Models.AdyenPayment.AdyenEcomResponse()
                {

                    Result = false,
                    ResponseMessage = "Generic Exception : " + ex.Message
                }), paymentRequest.RequestIdentifier, "CancelPayment", "API", "Payment");
                return new Models.AdyenPayment.AdyenEcomResponse()
                {

                    Result = false,
                    ResponseMessage = "Generic Exception : " + ex.Message
                };
            }
        }

        [HttpPost]
        [ActionName("GetConnectedTerminalList")]
        public async Task<Models.AdyenPayment.AdyenEcomResponse> GetConnectedTerminalList(Models.AdyenPayment.PaymentRequest paymentRequest)
        {
            try
            {
                new LogHelper().Log("Processing Get connected terminal list request started", paymentRequest.RequestIdentifier, "GetConnectedTerminalList", "API", "Payment");
                new LogHelper().Debug("Raw Get connected terminal list request : " + JsonConvert.SerializeObject(paymentRequest), paymentRequest.RequestIdentifier, "GetConnectedTerminalList", "API", "Payment");

                HttpClient httpClient = null;
                if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["IsPaymentProxyEnabled"]) && (Convert.ToBoolean(ConfigurationManager.AppSettings["IsPaymentProxyEnabled"])))
                {
                    httpClient = new Helper.Helper().getProxyClient("Payment", ConfigurationManager.AppSettings["PaymentProxyURL"], ConfigurationManager.AppSettings["PaymentProxyUN"],
                        ConfigurationManager.AppSettings["PaymentProxyPSWD"]);
                }
                else
                    httpClient = new HttpClient();

                httpClient.DefaultRequestHeaders.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpClient.DefaultRequestHeaders.Add("x-api-key", paymentRequest.ApiKey);

                Models.AdyenPayment.GetDeviceListRequest getDeviceListRequest = new Models.AdyenPayment.GetDeviceListRequest()
                {
                    merchantAccount = paymentRequest.merchantAccount
                };
                new LogHelper().Debug("Adyen Get connected terminal list request : " + JsonConvert.SerializeObject(getDeviceListRequest), paymentRequest.RequestIdentifier, "GetConnectedTerminalList", "API", "Payment");
                HttpContent requestContent = new StringContent(JsonConvert.SerializeObject(getDeviceListRequest), Encoding.UTF8, "application/json");
                HttpResponseMessage response = await httpClient.PostAsync(ConfigurationManager.AppSettings["AdyenDeviceListURL"], requestContent);
                Models.AdyenPayment.GetDeviceListResponse getDeviceListResponse = new Models.AdyenPayment.GetDeviceListResponse();
                if (response != null)
                {
                    if (response.IsSuccessStatusCode)
                    {
                        var responsestr = await response.Content.ReadAsStringAsync();
                        getDeviceListResponse = JsonConvert.DeserializeObject<Models.AdyenPayment.GetDeviceListResponse>(await response.Content.ReadAsStringAsync());
                        new LogHelper().Debug("Adyen Get connected terminal list response : " + JsonConvert.SerializeObject(getDeviceListResponse), paymentRequest.RequestIdentifier, "GetConnectedTerminalList", "API", "Payment");
                        if (getDeviceListResponse != null && getDeviceListResponse.uniqueTerminalIds != null && getDeviceListResponse.uniqueTerminalIds.Count > 0)
                        {
                            new LogHelper().Log("Get connected terminal list request completed successfully", paymentRequest.RequestIdentifier, "GetConnectedTerminalList", "API", "Payment");
                            new LogHelper().Debug("Get connected terminal list response : " + JsonConvert.SerializeObject(new Models.AdyenPayment.AdyenEcomResponse()
                            {
                                ResponseObject = getDeviceListResponse.uniqueTerminalIds,
                                Result = true
                            }), paymentRequest.RequestIdentifier, "GetConnectedTerminalList", "API", "Payment");
                            return new Models.AdyenPayment.AdyenEcomResponse()
                            {
                                ResponseObject = getDeviceListResponse.uniqueTerminalIds,
                                Result = true
                            };
                        }
                        else
                        {
                            new LogHelper().Log("Get connected terminal list request failled with the reason :- "+ getDeviceListResponse.Message, paymentRequest.RequestIdentifier, "GetConnectedTerminalList", "API", "Payment");
                            new LogHelper().Debug("Get connected terminal list response : " + JsonConvert.SerializeObject(new Models.AdyenPayment.AdyenEcomResponse()
                            {
                                ResponseMessage = getDeviceListResponse.Message,
                                Result = false
                            }), paymentRequest.RequestIdentifier, "GetConnectedTerminalList", "API", "Payment");
                            return new Models.AdyenPayment.AdyenEcomResponse()
                            {
                                ResponseMessage = getDeviceListResponse.Message,
                                Result = false
                            };
                        }
                    }
                    else
                    {
                        new LogHelper().Log("Get connected terminal list request failled with the reason :- " + response.ReasonPhrase, paymentRequest.RequestIdentifier, "GetConnectedTerminalList", "API", "Payment");
                        new LogHelper().Debug("Get connected terminal list response : " + JsonConvert.SerializeObject(new Models.AdyenPayment.AdyenEcomResponse()
                        {
                            Result = false,
                            ResponseMessage = response.ReasonPhrase
                        }), paymentRequest.RequestIdentifier, "GetConnectedTerminalList", "API", "Payment");
                        return new Models.AdyenPayment.AdyenEcomResponse()
                        {

                            Result = false,
                            ResponseMessage = response.ReasonPhrase
                        };
                    }

                }
                else
                {
                    new LogHelper().Log("Get connected terminal list request failled with the reason :- Payment gateway returned blank" , paymentRequest.RequestIdentifier, "GetConnectedTerminalList", "API", "Payment");
                    new LogHelper().Debug("Get connected terminal list response : " + JsonConvert.SerializeObject(new Models.AdyenPayment.AdyenEcomResponse()
                    {
                        Result = false,
                        ResponseMessage = "Payment gateway returned blank"
                    }), paymentRequest.RequestIdentifier, "GetConnectedTerminalList", "API", "Payment");
                    return new Models.AdyenPayment.AdyenEcomResponse()
                    {
                        Result = false,
                        ResponseMessage = "Payment gateway returned blank"
                    };
                }
            }
            catch (Exception ex)
            {
                new LogHelper().Log("Get connected terminal list request failled with the reason :- "+ex.ToString(), paymentRequest.RequestIdentifier, "GetConnectedTerminalList", "API", "Payment");
                return new Models.AdyenPayment.AdyenEcomResponse()
                {

                    Result = false,
                    ResponseMessage = "Generic Exception : " + ex.Message
                };
            }
        }


        [HttpPost]
        [ActionName("PaymentTopUp")]
        public async Task<Models.AdyenPayment.AdyenEcomResponse> PaymentTopUp(Models.AdyenPayment.PaymentRequest paymentRequest)
        {
            try
            {
                new LogHelper().Log("Processing Payment top up request started", paymentRequest.RequestIdentifier, "PaymentTopUp", "API", "Payment");
                new LogHelper().Debug("Raw Get payment top up request : " + JsonConvert.SerializeObject(paymentRequest), paymentRequest.RequestIdentifier, "PaymentTopUp", "API", "Payment");
                Models.AdyenPayment.CaptureRequest request = JsonConvert.DeserializeObject<Models.AdyenPayment.CaptureRequest>(paymentRequest.RequestObject.ToString());

                HttpClient httpClient = null;
                if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["IsPaymentProxyEnabled"]) && (Convert.ToBoolean(ConfigurationManager.AppSettings["IsPaymentProxyEnabled"])))
                {
                    httpClient = new Helper.Helper().getProxyClient("Payment", ConfigurationManager.AppSettings["PaymentProxyURL"], ConfigurationManager.AppSettings["PaymentProxyUN"],
                        ConfigurationManager.AppSettings["PaymentProxyPSWD"]);
                }
                else
                    httpClient = new HttpClient();

                //httpClient.BaseAddress = new Uri(ConfigurationManager.AppSettings["AdyenPaymentURL"]);
                httpClient.DefaultRequestHeaders.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpClient.DefaultRequestHeaders.Add("x-api-key", paymentRequest.ApiKey);

                long amnt = 0;
                if ((request.Amount != null) && request.Amount.ToString().Contains("."))
                {
                    string temp = "00";
                    if (request.Amount.ToString().Substring(request.Amount.ToString().IndexOf(".") + 1).Length == 1)
                        temp = request.Amount.ToString().Substring(request.Amount.ToString().IndexOf(".") + 1) + "0";
                    else if (request.Amount.ToString().Substring(request.Amount.ToString().IndexOf(".") + 1).Length > 2)
                        temp = request.Amount.ToString().Substring(request.Amount.ToString().IndexOf(".") + 1, 2);
                    else
                        temp = request.Amount.ToString().Substring(request.Amount.ToString().IndexOf(".") + 1);
                    amnt = Int64.Parse(request.Amount.ToString().Substring(0, request.Amount.ToString().IndexOf(".")) + temp);
                }
                else
                    amnt = Int64.Parse(request.Amount.ToString() + "00");
                
                if (string.IsNullOrEmpty(request.adjustAuthorisationData))
                {

                    Adyen.Model.Modification.AdjustAuthorisationRequest adjustAuthorizationRequest = new Adyen.Model.Modification.AdjustAuthorisationRequest()
                    {
                        MerchantAccount = paymentRequest.merchantAccount,
                        OriginalReference = request.OrginalPSPRefernce,
                        ModificationAmount = new Adyen.Model.Amount(ConfigurationManager.AppSettings["PaymentCurrency"].ToString(), amnt),
                        
                    };

                    new LogHelper().Debug("Adyen Get payment top up request : " + JsonConvert.SerializeObject(adjustAuthorizationRequest), paymentRequest.RequestIdentifier, "PaymentTopUp", "API", "Payment");
                    HttpContent requestContent = new StringContent(JsonConvert.SerializeObject(adjustAuthorizationRequest), Encoding.UTF8, "application/json");
                    HttpResponseMessage response = await httpClient.PostAsync(ConfigurationManager.AppSettings["AdyenPaymentURL"] + "/v52/adjustAuthorisation", requestContent);
                    Models.AdyenPayment.PaymentResponse paymentResponseObject = new Models.AdyenPayment.PaymentResponse();
                    if (response != null)
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            var responsestr = await response.Content.ReadAsStringAsync();
                            Adyen.Model.Modification.ModificationResult modificationResult = JsonConvert.DeserializeObject<Adyen.Model.Modification.ModificationResult>(await response.Content.ReadAsStringAsync());
                            new LogHelper().Debug("Adyen Get payment top up response : " + JsonConvert.SerializeObject(modificationResult), paymentRequest.RequestIdentifier, "PaymentTopUp", "API", "Payment");
                            if (modificationResult != null && modificationResult.Response == Adyen.Model.Enum.ResponseEnum.AdjustAuthorisationReceived)
                            {
                                paymentResponseObject.PspReference = modificationResult.PspReference;
                                List<Models.AdyenPayment.AdditionalInfo> additionalInfos = new List<Models.AdyenPayment.AdditionalInfo>();
                                if (modificationResult.AdditionalData != null)
                                {
                                    foreach (KeyValuePair<string, string> keyValuePair in modificationResult.AdditionalData)
                                    {
                                        Models.AdyenPayment.AdditionalInfo additionalInfo = new Models.AdyenPayment.AdditionalInfo();
                                        additionalInfo.key = keyValuePair.Key;
                                        additionalInfo.value = keyValuePair.Value;
                                        switch (additionalInfo.key)
                                        {
                                            case "refusalReasonRaw":
                                                paymentResponseObject.RefusalReason = additionalInfo.value;
                                                break;
                                            case "expiryDate":
                                                paymentResponseObject.CardExpiryDate = additionalInfo.value;
                                                break;
                                            case "recurring.recurringDetailReference":
                                                paymentResponseObject.PaymentToken = additionalInfo.value;
                                                break;
                                            case "authCode":
                                                paymentResponseObject.AuthCode = additionalInfo.value;
                                                break;
                                            case "paymentMethod":
                                                paymentResponseObject.CardType = additionalInfo.value;
                                                break;
                                            case "fundingSource":
                                                paymentResponseObject.FundingSource = additionalInfo.value;
                                                break;
                                            case "authorisedAmountCurrency":
                                                paymentResponseObject.Currency = additionalInfo.value;
                                                break;
                                            case "authorisedAmountValue":
                                                paymentResponseObject.Amount = !string.IsNullOrEmpty(additionalInfo.value) ? Decimal.Divide(Convert.ToDecimal(long.Parse(additionalInfo.value)), Convert.ToDecimal(100)) : 0;
                                                break;


                                        }
                                        additionalInfos.Add(additionalInfo);
                                    }
                                    paymentResponseObject.additionalInfos = additionalInfos;
                                }
                                
                                new LogHelper().Log("payment top up completed successfully" , paymentRequest.RequestIdentifier, "PaymentTopUp", "API", "Payment");
                                new LogHelper().Debug("Get payment top up response : " + JsonConvert.SerializeObject(new Models.AdyenPayment.AdyenEcomResponse()
                                {
                                    ResponseObject = paymentResponseObject,
                                    Result = true
                                }), paymentRequest.RequestIdentifier, "PaymentTopUp", "API", "Payment");
                                return new Models.AdyenPayment.AdyenEcomResponse()
                                {
                                    ResponseObject = paymentResponseObject,
                                    Result = true
                                };
                            }
                            else
                            {
                                new LogHelper().Log("payment top up completed failled with reason :- "+ modificationResult.Message, paymentRequest.RequestIdentifier, "PaymentTopUp", "API", "Payment");
                                new LogHelper().Debug("Get payment top up response : " + JsonConvert.SerializeObject(new Models.AdyenPayment.AdyenEcomResponse()
                                {
                                    ResponseMessage = modificationResult.Message,
                                    Result = false
                                }), paymentRequest.RequestIdentifier, "PaymentTopUp", "API", "Payment");
                                return new Models.AdyenPayment.AdyenEcomResponse()
                                {
                                    ResponseMessage = modificationResult.Message,
                                    Result = false
                                };
                            }
                        }
                        else
                        {
                            
                            new LogHelper().Log("payment top up completed failled with reason :- " + response.ReasonPhrase, paymentRequest.RequestIdentifier, "PaymentTopUp", "API", "Payment");
                            new LogHelper().Debug("Get payment top up response : " + JsonConvert.SerializeObject(new Models.AdyenPayment.AdyenEcomResponse()
                            {
                                ResponseMessage = response.ReasonPhrase,
                                Result = false
                            }), paymentRequest.RequestIdentifier, "PaymentTopUp", "API", "Payment");

                            return new Models.AdyenPayment.AdyenEcomResponse()
                            {

                                Result = false,
                                ResponseMessage = response.ReasonPhrase
                            };
                        }

                    }
                    else
                    {
                        new LogHelper().Log("payment top up completed failled with reason :- Payment gateway returned blank", paymentRequest.RequestIdentifier, "PaymentTopUp", "API", "Payment");
                        new LogHelper().Debug("Get payment top up response : " + JsonConvert.SerializeObject(new Models.AdyenPayment.AdyenEcomResponse()
                        {
                            ResponseMessage = "Payment gateway returned blank",
                            Result = false
                        }), paymentRequest.RequestIdentifier, "PaymentTopUp", "API", "Payment");
                        return new Models.AdyenPayment.AdyenEcomResponse()
                        {
                            Result = false,
                            ResponseMessage = "Payment gateway returned blank"
                        };
                    }
                }
                else
                {
                    Models.AdyenPayment.AdjustAuthorizationRequest adjustAuthorizationRequest = new Models.AdyenPayment.AdjustAuthorizationRequest()
                    {
                        merchantAccount = paymentRequest.merchantAccount,
                        originalReference = request.OrginalPSPRefernce,
                        modificationAmount = new Models.AdyenPayment.ModificationAmount()
                        {
                            currency = ConfigurationManager.AppSettings["PaymentCurrency"].ToString(),
                            value = amnt
                        },
                        additionalData = !string.IsNullOrEmpty(request.adjustAuthorisationData) ? new Models.AdyenPayment.AdditionalData()
                        {
                            adjustAuthorisationData = request.adjustAuthorisationData
                        } : null
                    };

                    new LogHelper().Debug("Adyen payment top up request : " + JsonConvert.SerializeObject(adjustAuthorizationRequest), paymentRequest.RequestIdentifier, "PaymentTopUp", "API", "Payment");
                    HttpContent requestContent = new StringContent(JsonConvert.SerializeObject(adjustAuthorizationRequest), Encoding.UTF8, "application/json");
                    HttpResponseMessage response = await httpClient.PostAsync(ConfigurationManager.AppSettings["AdyenPaymentURL"] + "/v52/adjustAuthorisation", requestContent);
                    Models.AdyenPayment.PaymentResponse paymentResponseObject = new Models.AdyenPayment.PaymentResponse();
                    if (response != null)
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            var responsestr = await response.Content.ReadAsStringAsync();
                            //Adyen.Model.Modification.ModificationResult modificationResult = JsonConvert.DeserializeObject<Adyen.Model.Modification.ModificationResult>(await response.Content.ReadAsStringAsync());
                            Models.AdyenPayment.AuthModificationResponse modificationResult = JsonConvert.DeserializeObject<Models.AdyenPayment.AuthModificationResponse>(await response.Content.ReadAsStringAsync());
                            new LogHelper().Debug("Adyen payment top up response : " + JsonConvert.SerializeObject(modificationResult), paymentRequest.RequestIdentifier, "PaymentTopUp", "API", "Payment");
                            if (modificationResult != null && modificationResult.response == Models.AdyenPayment.ResponseEnum.Authorised.ToString())
                            {
                                paymentResponseObject.PspReference = modificationResult.pspReference;
                                List<Models.AdyenPayment.AdditionalInfo> additionalInfos = new List<Models.AdyenPayment.AdditionalInfo>();
                                if (modificationResult.additionalData != null)
                                {
                                    foreach (KeyValuePair<string, string> keyValuePair in modificationResult.additionalData)
                                    {
                                        Models.AdyenPayment.AdditionalInfo additionalInfo = new Models.AdyenPayment.AdditionalInfo();
                                        additionalInfo.key = keyValuePair.Key;
                                        additionalInfo.value = keyValuePair.Value;
                                        switch (additionalInfo.key)
                                        {
                                            case "refusalReasonRaw":
                                                paymentResponseObject.RefusalReason = additionalInfo.value;
                                                break;
                                            case "expiryDate":
                                                paymentResponseObject.CardExpiryDate = additionalInfo.value;
                                                break;
                                            case "recurring.recurringDetailReference":
                                                paymentResponseObject.PaymentToken = additionalInfo.value;
                                                break;
                                            case "authCode":
                                                paymentResponseObject.AuthCode = additionalInfo.value;
                                                break;
                                            case "paymentMethod":
                                                paymentResponseObject.CardType = additionalInfo.value;
                                                break;
                                            case "fundingSource":
                                                paymentResponseObject.FundingSource = additionalInfo.value;
                                                break;
                                            case "authorisedAmountCurrency":
                                                paymentResponseObject.Currency = additionalInfo.value;
                                                break;
                                            case "authorisedAmountValue":
                                                paymentResponseObject.Amount = !string.IsNullOrEmpty(additionalInfo.value) ? Decimal.Divide(Convert.ToDecimal(long.Parse(additionalInfo.value)), Convert.ToDecimal(100)) : 0;
                                                break;


                                        }
                                        additionalInfos.Add(additionalInfo);
                                    }
                                    paymentResponseObject.additionalInfos = additionalInfos;
                                }
                                File.AppendAllText(System.Web.HttpContext.Current.Request.MapPath("~\\Resources\\PaymentTopup.txt"), "Response : " + JsonConvert.SerializeObject(new Models.AdyenPayment.AdyenEcomResponse()
                                {
                                    ResponseObject = paymentResponseObject,
                                    Result = true
                                }));
                                new LogHelper().Log("payment top up completed sucessfully", paymentRequest.RequestIdentifier, "PaymentTopUp", "API", "Payment");
                                new LogHelper().Debug("Get payment top up response : " + JsonConvert.SerializeObject(new Models.AdyenPayment.AdyenEcomResponse()
                                {
                                    ResponseObject = paymentResponseObject,
                                    Result = true
                                }), paymentRequest.RequestIdentifier, "PaymentTopUp", "API", "Payment");
                                return new Models.AdyenPayment.AdyenEcomResponse()
                                {
                                    ResponseObject = paymentResponseObject,
                                    Result = true
                                };
                            }
                            else
                            {
                                
                                new LogHelper().Log("payment top up completed failled with reason :- "+modificationResult.Message, paymentRequest.RequestIdentifier, "PaymentTopUp", "API", "Payment");
                                new LogHelper().Debug("Get payment top up response : " + JsonConvert.SerializeObject(new Models.AdyenPayment.AdyenEcomResponse()
                                {
                                    ResponseMessage = modificationResult.Message,
                                    Result = false
                                }), paymentRequest.RequestIdentifier, "PaymentTopUp", "API", "Payment");
                                return new Models.AdyenPayment.AdyenEcomResponse()
                                {
                                    ResponseMessage = modificationResult.Message,
                                    Result = false
                                };
                            }
                        }
                        else
                        {
                            new LogHelper().Log("payment top up completed failled with reason :- "+response.ReasonPhrase, paymentRequest.RequestIdentifier, "PaymentTopUp", "API", "Payment");
                            new LogHelper().Debug("Get payment top up response : " + JsonConvert.SerializeObject(new Models.AdyenPayment.AdyenEcomResponse()
                            {
                                ResponseMessage = response.ReasonPhrase,
                                Result = false
                            }), paymentRequest.RequestIdentifier, "PaymentTopUp", "API", "Payment");
                            return new Models.AdyenPayment.AdyenEcomResponse()
                            {
                                Result = false,
                                ResponseMessage = response.ReasonPhrase
                            };
                        }

                    }
                    else
                    {
                        new LogHelper().Log("payment top up completed failled with reason :- Payment gateway returned blank" , paymentRequest.RequestIdentifier, "PaymentTopUp", "API", "Payment");
                        new LogHelper().Debug("Get payment top up response : " + JsonConvert.SerializeObject(new Models.AdyenPayment.AdyenEcomResponse()
                        {
                            ResponseMessage = "Payment gateway returned blank",
                            Result = false
                        }), paymentRequest.RequestIdentifier, "PaymentTopUp", "API", "Payment");


                        return new Models.AdyenPayment.AdyenEcomResponse()
                        {
                            Result = false,
                            ResponseMessage = "Payment gateway returned blank"
                        };
                    }
                }

                
            }
            catch (Exception ex)
            {
                
                new LogHelper().Log("payment top up completed failled with reason :- "+ex.ToString(), paymentRequest.RequestIdentifier, "PaymentTopUp", "API", "Payment");
                new LogHelper().Debug("Get payment top up response : " + JsonConvert.SerializeObject(new Models.AdyenPayment.AdyenEcomResponse()
                {
                    Result = false,
                    ResponseMessage = "Generic Exception : " + ex.Message
                }), paymentRequest.RequestIdentifier, "PaymentTopUp", "API", "Payment");

                return new Models.AdyenPayment.AdyenEcomResponse()
                {
                    Result = false,
                    ResponseMessage = "Generic Exception : " + ex.Message
                };
            }
        }

        //Till here edited

        [HttpPost]
        [ActionName("MakePayment")]
        public async Task<Models.AdyenPayment.AdyenEcomResponse> MakePayment(Models.AdyenPayment.PaymentRequest paymentRequest)
        {
            try
            {
                new LogHelper().Log("Processing Payment started", paymentRequest.RequestIdentifier, "MakePayment", "API", "Payment");
                new LogHelper().Debug("Raw Make payment request : " + JsonConvert.SerializeObject(paymentRequest), paymentRequest.RequestIdentifier, "MakePayment", "API", "Payment");
                Models.AdyenPayment.MakePaymentRequest makePayment = JsonConvert.DeserializeObject<Models.AdyenPayment.MakePaymentRequest>(paymentRequest.RequestObject.ToString());

                HttpClient httpClient = null;
                if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["IsPaymentProxyEnabled"]) && (Convert.ToBoolean(ConfigurationManager.AppSettings["IsPaymentProxyEnabled"])))
                {
                    httpClient = new Helper.Helper().getProxyClient("Payment", ConfigurationManager.AppSettings["PaymentProxyURL"], ConfigurationManager.AppSettings["PaymentProxyUN"],
                        ConfigurationManager.AppSettings["PaymentProxyPSWD"]);
                }
                else
                    httpClient = new HttpClient();

                httpClient.BaseAddress = new Uri(ConfigurationManager.AppSettings["AdyenCheckoutURL"]);
                httpClient.DefaultRequestHeaders.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpClient.DefaultRequestHeaders.Add("x-api-key", paymentRequest.ApiKey);

                long amnt;
                if (makePayment.Amount.ToString().Contains("."))
                {
                    string temp = "00";
                    if (makePayment.Amount.ToString().Substring(makePayment.Amount.ToString().IndexOf(".") + 1).Length == 1)
                        temp = makePayment.Amount.ToString().Substring(makePayment.Amount.ToString().IndexOf(".") + 1) + "0";
                    else if (makePayment.Amount.ToString().Substring(makePayment.Amount.ToString().IndexOf(".") + 1).Length > 2)
                        temp = makePayment.Amount.ToString().Substring(makePayment.Amount.ToString().IndexOf(".") + 1, 2);
                    else
                        temp = makePayment.Amount.ToString().Substring(makePayment.Amount.ToString().IndexOf(".") + 1);
                    amnt = Int64.Parse(makePayment.Amount.ToString().Substring(0, makePayment.Amount.ToString().IndexOf(".")) + temp);
                }
                else
                    amnt = Int64.Parse(makePayment.Amount.ToString() + "00");//(long)PaymentReq.Amount;
                Adyen.Model.Checkout.Amount amount;
                
                if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["PaymentCurrency"]))
                {
                    amount = new Adyen.Model.Checkout.Amount(ConfigurationManager.AppSettings["PaymentCurrency"], amnt);
                }
                else
                    amount = new Adyen.Model.Checkout.Amount(ConfigurationManager.AppSettings["PaymentCurrency"].ToString(), amnt);
              



                Adyen.Model.Checkout.PaymentRequest makePaymentRequest = new Adyen.Model.Checkout.PaymentRequest()
                {
                    Amount = amount,

                    BrowserInfo = makePayment.BrowserInfo != null ?  new Adyen.Model.Checkout.BrowserInfo(makePayment.BrowserInfo.acceptHeader, makePayment.BrowserInfo.colorDepth, makePayment.BrowserInfo.javaEnabled
                    , makePayment.BrowserInfo.language, makePayment.BrowserInfo.screenHeight, makePayment.BrowserInfo.screenWidth, makePayment.BrowserInfo.timeZoneOffset, makePayment.BrowserInfo.userAgent, null) : null,
                    MerchantAccount = paymentRequest.merchantAccount,
                    //CountryCode = "SG",
                    //EnableOneClick = makePayment.isEnableOneClick != null ? makePayment.isEnableOneClick : false,
                    //EnableRecurring = makePayment.isRecurringEnable != null ? makePayment.isRecurringEnable : false,
                    //Mcc = makePayment.MCC,
                    //MerchantOrderReference = makePayment.MerchantReference,
                    //OrderReference = makePayment.Prev_PSPRefernce,


                    AdditionalData = new Dictionary<string, string>()
                    {
                        { "authorisationType", "PreAuth" }

                    },
                    PaymentMethod = makePayment.PaymentMethod,
                    RecurringProcessingModel = Adyen.Model.Checkout.PaymentRequest.RecurringProcessingModelEnum.CardOnFile,
                    StorePaymentMethod = true,
                    Reference = makePayment.MerchantReference,//makePayment.RefernceUniqueID,
                    ReturnUrl = makePayment.ReturnUrl,
                    ShopperInteraction = Adyen.Model.Checkout.PaymentRequest.ShopperInteractionEnum.Ecommerce,
                    ShopperReference = makePayment.ReservationNameID
                    
                };
                if (makePayment.PaymentMethod.Type.ToLower().Contains("wechat"))
                {
                    makePaymentRequest.RecurringProcessingModel = null;
                    makePaymentRequest.AdditionalData = null;
                    makePaymentRequest.ShopperInteraction = null;
                    makePaymentRequest.StorePaymentMethod = null;
                }

                if (makePayment.TransactionType == Models.AdyenPayment.TransactionType.Sale)
                    makePaymentRequest.CaptureDelayHours = 0;
                //else
                //    makePaymentRequest.CaptureDelayHours = Int32.Parse(ConfigurationManager.AppSettings["DefaultCaptureDelay"]);

                new LogHelper().Debug("Adyen Make payment request : " + JsonConvert.SerializeObject(makePaymentRequest), paymentRequest.RequestIdentifier, "MakePayment", "API", "Payment");
                string json_temp = JsonConvert.SerializeObject(makePaymentRequest);
                HttpContent requestContent = new StringContent(JsonConvert.SerializeObject(makePaymentRequest), Encoding.UTF8, "application/json");
                HttpResponseMessage response = await httpClient.PostAsync(ConfigurationManager.AppSettings["AdyenCheckoutURL"] + "/v53/payments", requestContent);
                Models.AdyenPayment.PaymentResponse paymentResponseObject = new Models.AdyenPayment.PaymentResponse();
                if (response != null)
                {
                    if (response.IsSuccessStatusCode)
                    {
                        var responsestr = await response.Content.ReadAsStringAsync();
                        Adyen.Model.Checkout.PaymentsResponse paymentsResponse = JsonConvert.DeserializeObject<Adyen.Model.Checkout.PaymentsResponse>(await response.Content.ReadAsStringAsync());
                        new LogHelper().Debug("Adyen Make payment response : " + JsonConvert.SerializeObject(paymentsResponse), paymentRequest.RequestIdentifier, "MakePayment", "API", "Payment");
                        if (paymentsResponse != null && (paymentsResponse.ResultCode == Adyen.Model.Checkout.PaymentsResponse.ResultCodeEnum.AuthenticationFinished ||
                                                        paymentsResponse.ResultCode == Adyen.Model.Checkout.PaymentsResponse.ResultCodeEnum.Authorised || paymentsResponse.ResultCode == Adyen.Model.Checkout.PaymentsResponse.ResultCodeEnum.Received))
                        {

                            paymentResponseObject.PspReference = paymentsResponse.PspReference;
                            paymentResponseObject.MerchantRefernce = paymentsResponse.MerchantReference;
                            paymentResponseObject.ResultCode = paymentsResponse.ResultCode.ToString();
                            List<Models.AdyenPayment.AdditionalInfo> additionalInfos = new List<Models.AdyenPayment.AdditionalInfo>();
                            if (paymentsResponse.AdditionalData != null)
                            {
                                string temp_card_bin = null;
                                string temp_card_summary = null;
                                foreach (KeyValuePair<string, string> keyValuePair in paymentsResponse.AdditionalData)
                                {
                                    Models.AdyenPayment.AdditionalInfo additionalInfo = new Models.AdyenPayment.AdditionalInfo();
                                    additionalInfo.key = keyValuePair.Key;
                                    additionalInfo.value = keyValuePair.Value;
                                    switch (additionalInfo.key)
                                    {
                                        case "refusalReasonRaw":
                                            paymentResponseObject.RefusalReason = additionalInfo.value;
                                            break;
                                        case "expiryDate":
                                            paymentResponseObject.CardExpiryDate = additionalInfo.value;
                                            break;
                                        case "recurring.recurringDetailReference":
                                            paymentResponseObject.PaymentToken = additionalInfo.value;
                                            break;
                                        case "authCode":
                                            paymentResponseObject.AuthCode = additionalInfo.value;
                                            break;
                                        case "paymentMethod":
                                            paymentResponseObject.CardType = additionalInfo.value;
                                            break;
                                        case "fundingSource":
                                            paymentResponseObject.FundingSource = additionalInfo.value;
                                            break;
                                        case "cardBin":
                                            temp_card_bin = additionalInfo.value;
                                            break;
                                        case "cardSummary":
                                            temp_card_summary = additionalInfo.value;
                                            break;
                                        case "adjustAuthorisationData":

                                            additionalInfo.value = HttpUtility.UrlDecode(additionalInfo.value);
                                            break;
                                        case "authorisedAmountCurrency":
                                            paymentResponseObject.Currency = additionalInfo.value;
                                            break;
                                        case "authorisedAmountValue":
                                            paymentResponseObject.Amount = !string.IsNullOrEmpty(additionalInfo.value) ? Decimal.Divide(Convert.ToDecimal(long.Parse(additionalInfo.value)), Convert.ToDecimal(100)) : 0;
                                            break;

                                    }
                                    additionalInfos.Add(additionalInfo);
                                }
                                paymentResponseObject.additionalInfos = additionalInfos;
                                paymentResponseObject.MaskCardNumber = temp_card_bin + "xxxxxx" + temp_card_summary;
                            }
                            
                            new LogHelper().Log("Make payment completed successfully", paymentRequest.RequestIdentifier, "MakePayment", "API", "Payment");
                            new LogHelper().Debug("Make payment response : " + JsonConvert.SerializeObject(new Models.AdyenPayment.AdyenEcomResponse()
                            {
                                ResponseObject = paymentResponseObject,
                                Result = true
                            }), paymentRequest.RequestIdentifier, "MakePayment", "API", "Payment");
                            return new Models.AdyenPayment.AdyenEcomResponse()
                            {
                                ResponseObject = paymentResponseObject,
                                Result = true
                            };
                        }
                        else if (paymentsResponse != null && paymentsResponse.ResultCode == Adyen.Model.Checkout.PaymentsResponse.ResultCodeEnum.RedirectShopper)
                        {
                            new LogHelper().Log("Make payment completed successfully", paymentRequest.RequestIdentifier, "MakePayment", "API", "Payment");
                            new LogHelper().Debug("Make payment response : " + JsonConvert.SerializeObject(new Models.AdyenPayment.AdyenEcomResponse()
                            {
                                ResponseObject = paymentsResponse,
                                Result = true
                            }), paymentRequest.RequestIdentifier, "MakePayment", "API", "Payment");
                            return new Models.AdyenPayment.AdyenEcomResponse()
                            {
                                ResponseObject = paymentsResponse,
                                Result = true
                            };
                        }
                        else
                        {
                            if (paymentsResponse != null)
                            {
                                paymentResponseObject.PspReference = paymentsResponse.PspReference;
                                paymentResponseObject.MerchantRefernce = paymentsResponse.MerchantReference;
                                paymentResponseObject.ResultCode = paymentsResponse.ResultCode.ToString();
                                List<Models.AdyenPayment.AdditionalInfo> additionalInfos = new List<Models.AdyenPayment.AdditionalInfo>();
                                if (paymentsResponse.AdditionalData != null)
                                {
                                    string temp_card_bin = null;
                                    string temp_card_summary = null;
                                    foreach (KeyValuePair<string, string> keyValuePair in paymentsResponse.AdditionalData)
                                    {
                                        Models.AdyenPayment.AdditionalInfo additionalInfo = new Models.AdyenPayment.AdditionalInfo();
                                        additionalInfo.key = keyValuePair.Key;
                                        additionalInfo.value = keyValuePair.Value;
                                        switch (additionalInfo.key)
                                        {
                                            case "refusalReasonRaw":
                                                paymentResponseObject.RefusalReason = additionalInfo.value;
                                                break;
                                            case "expiryDate":
                                                paymentResponseObject.CardExpiryDate = additionalInfo.value;
                                                break;
                                            case "recurring.recurringDetailReference":
                                                paymentResponseObject.PaymentToken = additionalInfo.value;
                                                break;
                                            case "authCode":
                                                paymentResponseObject.AuthCode = additionalInfo.value;
                                                break;
                                            case "paymentMethod":
                                                paymentResponseObject.CardType = additionalInfo.value;
                                                break;
                                            case "fundingSource":
                                                paymentResponseObject.FundingSource = additionalInfo.value;
                                                break;
                                            case "cardBin":
                                                temp_card_bin = additionalInfo.value;
                                                break;
                                            case "cardSummary":
                                                temp_card_summary = additionalInfo.value;
                                                break;
                                            case "authorisedAmountCurrency":
                                                paymentResponseObject.Currency = additionalInfo.value;
                                                break;
                                            case "authorisedAmountValue":
                                                paymentResponseObject.Amount = !string.IsNullOrEmpty(additionalInfo.value) ? Decimal.Divide(Convert.ToDecimal(long.Parse(additionalInfo.value)), Convert.ToDecimal(100)) : 0;
                                                break;

                                        }
                                        additionalInfos.Add(additionalInfo);
                                    }
                                    paymentResponseObject.additionalInfos = additionalInfos;
                                    paymentResponseObject.MaskCardNumber = temp_card_bin + "xxxxxx" + temp_card_summary;

                                }
                                
                                new LogHelper().Log("Make payment completed failled with reason : - "+ paymentResponseObject.ResultCode, paymentRequest.RequestIdentifier, "MakePayment", "API", "Payment");
                                new LogHelper().Debug("Make payment response : " + JsonConvert.SerializeObject(new Models.AdyenPayment.AdyenEcomResponse()
                                {
                                    ResponseObject = paymentResponseObject,
                                    Result = false
                                }), paymentRequest.RequestIdentifier, "MakePayment", "API", "Payment");
                                return new Models.AdyenPayment.AdyenEcomResponse()
                                {
                                    ResponseObject = paymentResponseObject,
                                    Result = false
                                };

                            }
                            else
                            {
                                

                                new LogHelper().Log("Make payment completed failled with reason : - PG Responded NULL" , paymentRequest.RequestIdentifier, "MakePayment", "API", "Payment");
                                new LogHelper().Debug("Make payment response : " + JsonConvert.SerializeObject(new Models.AdyenPayment.AdyenEcomResponse()
                                {
                                    ResponseObject = null,
                                    Result = false
                                }), paymentRequest.RequestIdentifier, "MakePayment", "API", "Payment");
                                return new Models.AdyenPayment.AdyenEcomResponse()
                                {
                                    ResponseMessage = "PG Responded NULL",
                                    Result = false
                                };
                            }

                        }

                    }
                    else
                    {
                        new LogHelper().Log("Make payment completed failled with reason : - "+response.ReasonPhrase, paymentRequest.RequestIdentifier, "MakePayment", "API", "Payment");
                        new LogHelper().Debug("Make payment response : " + JsonConvert.SerializeObject(new Models.AdyenPayment.AdyenEcomResponse()
                        {
                            ResponseMessage = response.ReasonPhrase,
                            Result = false
                        }), paymentRequest.RequestIdentifier, "MakePayment", "API", "Payment");
                        return new Models.AdyenPayment.AdyenEcomResponse()
                        {
                            Result = false,
                            ResponseMessage = response.ReasonPhrase
                        };
                    }
                }
                else
                {
                    
                    new LogHelper().Log("Make payment completed failled with reason : - Payment gateway returned blank" , paymentRequest.RequestIdentifier, "MakePayment", "API", "Payment");
                    new LogHelper().Debug("Make payment response : " + JsonConvert.SerializeObject(new Models.AdyenPayment.AdyenEcomResponse()
                    {
                        ResponseMessage = "Payment gateway returned blank",
                        Result = false
                    }), paymentRequest.RequestIdentifier, "MakePayment", "API", "Payment");
                    return new Models.AdyenPayment.AdyenEcomResponse()
                    {
                        Result = false,
                        ResponseMessage = "Payment gateway returned blank"
                    };
                }
            }
            catch (Exception ex)
            {
                new LogHelper().Log("Make payment completed failled with reason : - "+ex.ToString(), paymentRequest.RequestIdentifier, "MakePayment", "API", "Payment");
                new LogHelper().Debug("Make payment response : " + JsonConvert.SerializeObject(new Models.AdyenPayment.AdyenEcomResponse()
                {
                    ResponseMessage = ex.ToString(),
                    Result = false
                }), paymentRequest.RequestIdentifier, "MakePayment", "API", "Payment");
                return new Models.AdyenPayment.AdyenEcomResponse()
                {
                    Result = false,
                    ResponseMessage = "Generic Exception : " + ex.Message
                };
            }
        }

        [HttpPost]
        [ActionName("PaymentWithToken")]
        public async Task<Models.AdyenPayment.AdyenEcomResponse> PaymentWithToken(Models.AdyenPayment.PaymentRequest paymentRequest)
        {
            try
            {
                new LogHelper().Log("Processing Payment with token started", paymentRequest.RequestIdentifier, "PaymentWithToken", "API", "Payment");
                new LogHelper().Debug("Raw Make payment with token request : " + JsonConvert.SerializeObject(paymentRequest), paymentRequest.RequestIdentifier, "PaymentWithToken", "API", "Payment");
                Models.AdyenPayment.MakePaymentRequest request = JsonConvert.DeserializeObject<Models.AdyenPayment.MakePaymentRequest>(paymentRequest.RequestObject.ToString());

                HttpClient httpClient = null;
                if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["IsPaymentProxyEnabled"]) && (Convert.ToBoolean(ConfigurationManager.AppSettings["IsPaymentProxyEnabled"])))
                {
                    httpClient = new Helper.Helper().getProxyClient("Payment", ConfigurationManager.AppSettings["PaymentProxyURL"], ConfigurationManager.AppSettings["PaymentProxyUN"],
                        ConfigurationManager.AppSettings["PaymentProxyPSWD"]);
                }
                else
                    httpClient = new HttpClient();

                //httpClient.BaseAddress = new Uri(ConfigurationManager.AppSettings["AdyenPaymentURL"]);
                httpClient.DefaultRequestHeaders.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpClient.DefaultRequestHeaders.Add("x-api-key", paymentRequest.ApiKey);

                long amnt = 0;
                if ((request.Amount != null) && request.Amount.ToString().Contains("."))
                {
                    string temp = "00";
                    if (request.Amount.ToString().Substring(request.Amount.ToString().IndexOf(".") + 1).Length == 1)
                        temp = request.Amount.ToString().Substring(request.Amount.ToString().IndexOf(".") + 1) + "0";
                    else if (request.Amount.ToString().Substring(request.Amount.ToString().IndexOf(".") + 1).Length > 2)
                        temp = request.Amount.ToString().Substring(request.Amount.ToString().IndexOf(".") + 1, 2);
                    else
                        temp = request.Amount.ToString().Substring(request.Amount.ToString().IndexOf(".") + 1);
                    amnt = Int64.Parse(request.Amount.ToString().Substring(0, request.Amount.ToString().IndexOf(".")) + temp);
                }
                else
                    amnt = Int64.Parse(request.Amount.ToString() + "00");
                Adyen.Model.Checkout.Amount amount1;

                if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["PaymentCurrency"]))
                {
                    amount1 = new Adyen.Model.Checkout.Amount(ConfigurationManager.AppSettings["PaymentCurrency"], amnt);
                }
                else
                    amount1 = new Adyen.Model.Checkout.Amount(ConfigurationManager.AppSettings["PaymentCurrency"].ToString(), amnt);

                

                Adyen.Model.Checkout.PaymentRequest PaymentWithTokenRequest = new Adyen.Model.Checkout.PaymentRequest()
                {
                    Amount = amount1,

                    ShopperReference = !string.IsNullOrEmpty(request.ReservationNameID) ? request.ReservationNameID : "12345",
                    PaymentMethod = new Adyen.Model.Checkout.DefaultPaymentMethodDetails()
                    {
                        RecurringDetailReference = request.Token
                    },
                    Recurring = new Adyen.Model.Checkout.Recurring()
                    {
                        Contract = Adyen.Model.Checkout.Recurring.ContractEnum.RECURRING
                    },
                    ShopperInteraction = Adyen.Model.Checkout.PaymentRequest.ShopperInteractionEnum.ContAuth,
                    Reference = request.MerchantReference, //+ " - Incidental charge",//"Payment with out card present",
                                                          //mer
                    MerchantAccount = paymentRequest.merchantAccount,
                    RecurringProcessingModel = Adyen.Model.Checkout.PaymentRequest.RecurringProcessingModelEnum.UnscheduledCardOnFile,
                    ReturnUrl = request.ReturnUrl,//"https://changiairport.crowneplaza.com/",
                    CaptureDelayHours = 0
                    //Metadata = new Dictionary<string, string>() { { "UserName", pms_userName } }
                };

                new LogHelper().Debug("Adyen Make payment with token request : " + JsonConvert.SerializeObject(PaymentWithTokenRequest), paymentRequest.RequestIdentifier, "PaymentWithToken", "API", "Payment");
                new Helper.LogHelper().Debug("URL: - " + ConfigurationManager.AppSettings["AdyenCheckoutURL"] + "/v53/payments", null, "PaymentWithToken", "Cloud API", Helper.NlogGroupName.Payment.ToString());
                HttpContent requestContent = new StringContent(JsonConvert.SerializeObject(PaymentWithTokenRequest), Encoding.UTF8, "application/json");
                HttpResponseMessage response = await httpClient.PostAsync(ConfigurationManager.AppSettings["AdyenCheckoutURL"] + "/v53/payments", requestContent);
                Models.AdyenPayment.PaymentResponse paymentResponseObject = new Models.AdyenPayment.PaymentResponse();
                if (response != null)
                {
                    if (response.IsSuccessStatusCode)
                    {
                        var responsestr = await response.Content.ReadAsStringAsync();
                        Adyen.Model.Checkout.PaymentsResponse paymentResult = JsonConvert.DeserializeObject<Adyen.Model.Checkout.PaymentsResponse>(await response.Content.ReadAsStringAsync());
                        new LogHelper().Debug("Adyen Make payment with token response : " + JsonConvert.SerializeObject(paymentResult), paymentRequest.RequestIdentifier, "PaymentWithToken", "API", "Payment");
                        if (paymentResult != null && paymentResult.ResultCode == Adyen.Model.Checkout.PaymentsResponse.ResultCodeEnum.Authorised)
                        {
                            paymentResponseObject.PspReference = paymentResult.PspReference;
                            List<Models.AdyenPayment.AdditionalInfo> additionalInfos = new List<Models.AdyenPayment.AdditionalInfo>();
                            if (paymentResult.AdditionalData != null)
                            {
                                foreach (KeyValuePair<string, string> keyValuePair in paymentResult.AdditionalData)
                                {
                                    Models.AdyenPayment.AdditionalInfo additionalInfo = new Models.AdyenPayment.AdditionalInfo();
                                    additionalInfo.key = keyValuePair.Key;
                                    additionalInfo.value = keyValuePair.Value;
                                    switch (additionalInfo.key)
                                    {
                                        case "refusalReasonRaw":
                                            paymentResponseObject.RefusalReason = additionalInfo.value;
                                            break;
                                        case "expiryDate":
                                            paymentResponseObject.CardExpiryDate = additionalInfo.value;
                                            break;
                                        case "recurring.recurringDetailReference":
                                            paymentResponseObject.PaymentToken = additionalInfo.value;
                                            break;
                                        case "authCode":
                                            paymentResponseObject.AuthCode = additionalInfo.value;
                                            break;
                                        case "paymentMethod":
                                            paymentResponseObject.CardType = additionalInfo.value;
                                            break;
                                        case "fundingSource":
                                            paymentResponseObject.FundingSource = additionalInfo.value;
                                            break;
                                        case "authorisedAmountCurrency":
                                            paymentResponseObject.Currency = additionalInfo.value;
                                            break;
                                        case "authorisedAmountValue":
                                            paymentResponseObject.Amount = !string.IsNullOrEmpty(additionalInfo.value) ? Decimal.Divide(Convert.ToDecimal(long.Parse(additionalInfo.value)), Convert.ToDecimal(100)) : 0;
                                            break;


                                    }
                                    additionalInfos.Add(additionalInfo);
                                }
                                paymentResponseObject.additionalInfos = additionalInfos;
                            }
                            
                            new LogHelper().Log("Make payment with token completed successfully", paymentRequest.RequestIdentifier, "PaymentWithToken", "API", "Payment");
                            new LogHelper().Debug("Make payment with token response : " + JsonConvert.SerializeObject(new Models.AdyenPayment.AdyenEcomResponse()
                            {
                                ResponseObject = paymentResponseObject,
                                Result = true
                            }), paymentRequest.RequestIdentifier, "PaymentWithToken", "API", "Payment");
                            return new Models.AdyenPayment.AdyenEcomResponse()
                            {
                                ResponseObject = paymentResponseObject,
                                Result = true
                            };
                        }
                        else
                        {
                            new LogHelper().Log("Make payment with token failled with reason :- "+paymentResult.RefusalReason, paymentRequest.RequestIdentifier, "PaymentWithToken", "API", "Payment");
                            new LogHelper().Debug("Make payment with token response : " + JsonConvert.SerializeObject(new Models.AdyenPayment.AdyenEcomResponse()
                            {
                                ResponseMessage = paymentResult.RefusalReason,
                                Result = false
                            }), paymentRequest.RequestIdentifier, "PaymentWithToken", "API", "Payment");
                            return new Models.AdyenPayment.AdyenEcomResponse()
                            {
                                ResponseMessage = paymentResult.RefusalReason,
                                Result = false
                            };
                        }
                    }
                    else
                    {
                        new LogHelper().Log("Make payment with token failled with reason :- " + response.ReasonPhrase, paymentRequest.RequestIdentifier, "PaymentWithToken", "API", "Payment");
                        new LogHelper().Debug("Make payment with token response : " + JsonConvert.SerializeObject(new Models.AdyenPayment.AdyenEcomResponse()
                        {
                            ResponseMessage = response.ReasonPhrase,
                            Result = false
                        }), paymentRequest.RequestIdentifier, "PaymentWithToken", "API", "Payment");
                        return new Models.AdyenPayment.AdyenEcomResponse()
                        {

                            Result = false,
                            ResponseMessage = response.ReasonPhrase
                        };
                    }

                }
                else
                {
                    new LogHelper().Log("Make payment with token failled with reason :- Payment gateway returned blank", paymentRequest.RequestIdentifier, "PaymentWithToken", "API", "Payment");
                    new LogHelper().Debug("Make payment with token response : " + JsonConvert.SerializeObject(new Models.AdyenPayment.AdyenEcomResponse()
                    {
                        ResponseMessage = "Payment gateway returned blank",
                        Result = false
                    }), paymentRequest.RequestIdentifier, "PaymentWithToken", "API", "Payment");
                    return new Models.AdyenPayment.AdyenEcomResponse()
                    {
                        Result = false,
                        ResponseMessage = "Payment gateway returned blank"
                    };
                }
            }
            catch (Exception ex)
            {
                
                new LogHelper().Log("Make payment with token failled with reason :- "+ex.ToString(), paymentRequest.RequestIdentifier, "PaymentWithToken", "API", "Payment");
                new LogHelper().Debug("Make payment with token response : " + JsonConvert.SerializeObject(new Models.AdyenPayment.AdyenEcomResponse()
                {
                    ResponseMessage = ex.ToString(),
                    Result = false
                }), paymentRequest.RequestIdentifier, "PaymentWithToken", "API", "Payment");
                return new Models.AdyenPayment.AdyenEcomResponse()
                {

                    Result = false,
                    ResponseMessage = "Generic Exception : " + ex.Message
                };
            }
        }

        [BasicAuthentication]
        [HttpPost]
        [ActionName("GetPaymentNotifications")]
        public HttpResponseMessage GetPaymentNotifications(Adyen.Model.Notification.NotificationRequest NotificationObject)
        {
            try
            {
                new LogHelper().Log("Processing notification started", null, "GetPaymentNotifications", "API", "Payment");
                new LogHelper().Debug("Raw notification request : " + JsonConvert.SerializeObject(NotificationObject), null, "GetPaymentNotifications", "API", "Payment");
                if (NotificationObject != null)
                {
                    if(NotificationObject.NotificationItemContainers != null)
                    {
                        foreach( Adyen.Model.Notification.NotificationRequestItemContainer notification in NotificationObject.NotificationItemContainers)
                        {
                            if (notification.NotificationItem != null)
                            {
                                decimal amount = notification.NotificationItem.Amount.Value != null ? Math.Round(Convert.ToDecimal(notification.NotificationItem.Amount.Value / 100), 2) : 0;
                                DateTime eventDate = notification.NotificationItem.EventDate != null ? DateTime.ParseExact(notification.NotificationItem.EventDate, "yyyy-MM-ddTHH:mm:ssK", null).ToUniversalTime() : DateTime.UtcNow;
                                if (!Helper.Cloud.DBHelper.Instance.InsertPaymentNotifications(notification.NotificationItem.EventCode, eventDate, notification.NotificationItem.MerchantAccountCode,
                                                                notification.NotificationItem.MerchantReference, notification.NotificationItem.OriginalReference, notification.NotificationItem.PaymentMethod,
                                                                notification.NotificationItem.PspReference, amount, notification.NotificationItem.Amount.Currency, notification.NotificationItem.Reason,
                                                                notification.NotificationItem.Success, JsonConvert.SerializeObject(notification.NotificationItem), ConfigurationManager.AppSettings["ConnectionString"]))
                                {
                                    //File.AppendAllLines(System.Web.HttpContext.Current.Request.MapPath("~\\Resources\\GetPaymentNotifications.txt"), new string[] { "Notification : " + JsonConvert.SerializeObject(NotificationObject) });
                                    new Helper.LogHelper().Log("Pushing Notification to DB failled ", null, "GetPaymentNotifications", "API", Helper.NlogGroupName.Payment.ToString());
                                    new Helper.LogHelper().Log("Notification ; "+ JsonConvert.SerializeObject(NotificationObject), null, "GetPaymentNotifications", "Cloud API", Helper.NlogGroupName.Payment.ToString());
                                }
                            }
                            else
                            {
                                //File.AppendAllLines(System.Web.HttpContext.Current.Request.MapPath("~\\Resources\\GetPaymentNotifications.txt"), new string[] { "Notification : " + JsonConvert.SerializeObject(NotificationObject) });
                                new Helper.LogHelper().Log("Notification item is blank ", null, "GetPaymentNotifications", "API", Helper.NlogGroupName.Payment.ToString());
                                new Helper.LogHelper().Log("Notification ; " + JsonConvert.SerializeObject(NotificationObject), null, "GetPaymentNotifications", "Cloud API", Helper.NlogGroupName.Payment.ToString());
                            }
                        }
                    }
                    else
                    {
                        //File.AppendAllText(System.Web.HttpContext.Current.Request.MapPath("~\\Resources\\GetPaymentNotifications.txt"), "Notification : " + JsonConvert.SerializeObject(NotificationObject));
                        new Helper.LogHelper().Log("Notification item container is blank ", null, "GetPaymentNotifications", "API", Helper.NlogGroupName.Payment.ToString());
                        new Helper.LogHelper().Log("Notification ; " + JsonConvert.SerializeObject(NotificationObject), null, "GetPaymentNotifications", "Cloud API", Helper.NlogGroupName.Payment.ToString());
                    }
                }
                new Helper.LogHelper().Debug("Notification is blank ", null, "GetPaymentNotifications", "API", Helper.NlogGroupName.Payment.ToString());
                return Request.CreateResponse(HttpStatusCode.OK, "[accepted]");
            }
            catch(Exception ex)
            {
                new Helper.LogHelper().Error(ex, null, "GetPaymentNotifications", "API", Helper.NlogGroupName.Payment.ToString());
                return Request.CreateResponse(HttpStatusCode.OK, "[accepted]");
            }
        }

        //[HttpPost]
        //[ActionName("FiservPaymentWithToken")]
        //public async Task<Models.Fiserv.PaymentResponseModel> FiservPayWIthToken(Models.Fiserv.PaymentRequestModel paymentRequest)
        //{
        //    new LogHelper().Debug("Raw Make payment with token request : " + JsonConvert.SerializeObject(paymentRequest), paymentRequest.RequestIdentifier, "FiservPayWIthToken", "API", "Payment");
        //    Models.Fiserv.PaymentRequest request = JsonConvert.DeserializeObject<Models.Fiserv.PaymentRequest>(paymentRequest.RequestObject.ToString());

        //    string currency = "EUR";
        //    if (string.IsNullOrEmpty(request.Currency))
        //    {
        //        currency = ConfigurationManager.AppSettings["PaymentCurrency"].ToString();
        //    }
        //    else
        //    {
        //        currency = request.Currency;
        //    }
        //    Models.Fiserv.PayWithTokenRequestModel payWithTokenRequest = new PayWithTokenRequestModel()
        //    {
        //        requestType = "PaymentTokenSaleTransaction",
        //        paymentMethod = new PaymentMethod()
        //        {
        //            paymentToken = new PaymentTokenDetails()
        //            {
        //                securityCode = request.SecuirtyCode,
        //                value = request.Token
        //            }
        //        },
        //        transactionAmount = new TransactionAmount()
        //        {
        //            currency = currency,
        //            total = request.Amount.ToString()
        //        }
        //    };

        //    var requestSTring = JsonConvert.SerializeObject(payWithTokenRequest);
        //    new LogHelper().Debug($"Fiserv Make payment with token request : {requestSTring}", paymentRequest.RequestIdentifier, "FiservPayWIthToken", "API", "Payment");


        //    HttpClient httpClient = null;
        //    //if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["IsPaymentProxyEnabled"]) && (Convert.ToBoolean(ConfigurationManager.AppSettings["IsPaymentProxyEnabled"])))
        //    //{
        //    //    httpClient = new Helper.Helper().getProxyClient("Payment", ConfigurationManager.AppSettings["PaymentProxyURL"], ConfigurationManager.AppSettings["PaymentProxyUN"],
        //    //        ConfigurationManager.AppSettings["PaymentProxyPSWD"]);
        //    //}
        //    //else
        //    httpClient = new HttpClient();

        //    var ClientRequestId = Guid.NewGuid();
        //    DateTime baseDate = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        //    var time = (DateTime.Now.ToUniversalTime() - baseDate).TotalMilliseconds;
        //    var apiKey = "hAxypytkFGa394khuXEra0QAARkUvy0c";
        //    var rawSignature = apiKey + ClientRequestId + time + requestSTring;
        //    var secret = "XBeVfuIE2MMyjAHGJhCQ1orI9iXqFHACEMDyuR0OR2u";
        //    var provider = new System.Security.Cryptography.HMACSHA256(Encoding.UTF8.GetBytes(secret));
        //    var hash = provider.ComputeHash(Encoding.UTF8.GetBytes(rawSignature));
        //    var messageSignature = Convert.ToBase64String(hash);


        //    httpClient.DefaultRequestHeaders.Clear();
        //    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            
        //    httpClient.DefaultRequestHeaders.Add("Client-Request-Id", ClientRequestId.ToString());
        //    httpClient.DefaultRequestHeaders.Add("Api-Key", apiKey);
        //    httpClient.DefaultRequestHeaders.Add("Timestamp", time.ToString());
        //    httpClient.DefaultRequestHeaders.Add("Message-Signature", messageSignature);

        //    HttpContent requestContent = new StringContent(requestSTring, Encoding.UTF8, "application/json");
        //    HttpResponseMessage response = await httpClient.PostAsync("https://prod.emea.api.fiservapps.com/sandbox/ipp/payments-gateway/v2/payments", requestContent);
        //    if (response != null)
        //    {
        //        if (response.IsSuccessStatusCode)
        //        {
        //            var responsestr = await response.Content.ReadAsStringAsync();
        //        }
        //    }
        //    return response;
        //}

        #endregion

    }
}
