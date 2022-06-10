using CheckinPortalCloudAPI.Helper;
using CheckinPortalCloudAPI.Models.Fiserv;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace CheckinPortalCloudAPI.Controllers
{
    public class FiservPaymentController : Controller
    {
        // GET: FiservPayment
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ActionName("FiservPaymentWithToken")]
        public async Task<Models.Fiserv.PaymentResponseModel> FiservPayWIthToken(Models.Fiserv.PaymentRequest paymentRequest)
        {
            new LogHelper().Debug("Raw Make payment with token request : " + JsonConvert.SerializeObject(paymentRequest), paymentRequest.RequestIdentifier, "FiservPayWIthToken", "API", "FiservPayment");
            Models.Fiserv.CaptureRequest request = JsonConvert.DeserializeObject<Models.Fiserv.CaptureRequest>(paymentRequest.RequestObject.ToString());

            string currency = "EUR";
            if (string.IsNullOrEmpty(request.Currency))
            {
                currency = ConfigurationManager.AppSettings["PaymentCurrency"].ToString();
            }
            else
            {
                currency = request.Currency;
            }
            Models.Fiserv.PayWithTokenRequestModel payWithTokenRequest = new PayWithTokenRequestModel()
            {
                requestType = paymentRequest.RequestIdentifier,
                paymentMethod = new PaymentMethod()
                {
                    paymentToken = new PaymentTokenDetails()
                    {
                        securityCode = "002",
                        value = request.OrginalPSPRefernce
                    }
                },
                transactionAmount = new TransactionAmount()
                {
                    currency = currency,
                    total = request.Amount.ToString()
                }
            };

            var requestSTring = JsonConvert.SerializeObject(payWithTokenRequest);
            new LogHelper().Debug($"Fiserv Make payment with token request : {requestSTring}", paymentRequest.RequestIdentifier, "FiservPayWIthToken", "API", "FiservPayment");


            HttpClient httpClient = null;
            httpClient = new HttpClient();

            var ClientRequestId = Guid.NewGuid();
            DateTime baseDate = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var time = (DateTime.Now.ToUniversalTime() - baseDate).TotalMilliseconds;
            var apiKey = paymentRequest.ApiKey;
            var rawSignature = apiKey + ClientRequestId + time + requestSTring;
            var secret = paymentRequest.merchantAccount;
            var provider = new System.Security.Cryptography.HMACSHA256(Encoding.UTF8.GetBytes(secret));
            var hash = provider.ComputeHash(Encoding.UTF8.GetBytes(rawSignature));
            var messageSignature = Convert.ToBase64String(hash);


            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            httpClient.DefaultRequestHeaders.Add("Client-Request-Id", ClientRequestId.ToString());
            httpClient.DefaultRequestHeaders.Add("Api-Key", apiKey);
            httpClient.DefaultRequestHeaders.Add("Timestamp", time.ToString());
            httpClient.DefaultRequestHeaders.Add("Message-Signature", messageSignature);

            HttpContent requestContent = new StringContent(requestSTring, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await httpClient.PostAsync("https://prod.emea.api.fiservapps.com/sandbox/ipp/payments-gateway/v2/payments", requestContent);
            Models.Fiserv.PaymentResponse paymentResponseObject = new Models.Fiserv.PaymentResponse();
            if (response != null)
            {
                if (response.IsSuccessStatusCode)
                {

                    Models.Fiserv.FiservPaymentResponseModel modificationResult = JsonConvert.DeserializeObject<Models.Fiserv.FiservPaymentResponseModel>(await response.Content.ReadAsStringAsync());
                    new LogHelper().Debug("Fiserv Get payment top up response : " + JsonConvert.SerializeObject(modificationResult), paymentRequest.RequestIdentifier, "FiservPaymentCapture", "API", "FiservPayment");
                    if (modificationResult != null)
                    {


                        paymentResponseObject.MaskCardNumber = modificationResult.cardnumber;
                        paymentResponseObject.CardExpiryDate = modificationResult.expmonth = "/" + modificationResult.expyear;
                        paymentResponseObject.AuthCode = modificationResult.oid;
                        paymentResponseObject.Amount = Convert.ToDecimal(modificationResult.chargetotal);
                        paymentResponseObject.Currency = modificationResult.currency;
                        paymentResponseObject.PspReference = modificationResult.ipgTransactionId;
                        paymentResponseObject.ResultCode = modificationResult.status;
                        paymentResponseObject.CardToken = modificationResult.hosteddataid;

                        paymentResponseObject.CardType = modificationResult.ccbrand;
                        List<Models.Fiserv.AdditionalInfo> additionalInfos = new List<Models.Fiserv.AdditionalInfo>();
                        var values = Newtonsoft.Json.JsonConvert.SerializeObject(modificationResult);
                        var paymentDetailResponseModels = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(values);

                        foreach (KeyValuePair<string, string> keyValuePair in paymentDetailResponseModels)
                        {
                            Models.Fiserv.AdditionalInfo additionalInfo = new Models.Fiserv.AdditionalInfo();
                            additionalInfo.key = keyValuePair.Key;
                            additionalInfo.value = keyValuePair.Value;

                            additionalInfos.Add(additionalInfo);
                        }
                        paymentResponseObject.additionalInfos = additionalInfos;

                        new LogHelper().Log("Make payment with token completed successfully", paymentRequest.RequestIdentifier, "PaymentWithToken", "API", "Payment");
                        new LogHelper().Debug("Make payment with token response : " + JsonConvert.SerializeObject(new Models.Fiserv.PaymentResponseModel()
                        {
                            ResponseObject = paymentResponseObject,
                            Result = true
                        }), paymentRequest.RequestIdentifier, "PaymentWithToken", "API", "Payment");
                        return new Models.Fiserv.PaymentResponseModel()
                        {
                            ResponseObject = paymentResponseObject,
                            Result = true
                        };
                    }
                    else
                    {
                        new LogHelper().Log("Make payment with token failled with reason :- " + modificationResult.fail_reason, paymentRequest.RequestIdentifier, "PaymentWithToken", "API", "Payment");
                        new LogHelper().Debug("Make payment with token response : " + JsonConvert.SerializeObject(new Models.Fiserv.PaymentResponseModel()
                        {
                            ResponseMessage = modificationResult.fail_reason,
                            Result = false
                        }), paymentRequest.RequestIdentifier, "PaymentWithToken", "API", "Payment");
                        return new Models.Fiserv.PaymentResponseModel()
                        {
                            ResponseMessage = modificationResult.fail_reason,
                            Result = false
                        };
                    }

                }
                else
                {
                    new LogHelper().Debug("Fiserv Payment with token response failled with reason: " + JsonConvert.SerializeObject(new Models.Fiserv.PaymentResponseModel()
                    {
                        Result = false,
                        ResponseMessage = response.ReasonPhrase
                    }), paymentRequest.RequestIdentifier, "FiservTopup", "API", "FiservPayment");
                    return new Models.Fiserv.PaymentResponseModel()
                    {
                        Result = false,
                        ResponseMessage = response.ReasonPhrase
                    };
                }
            }
            else
            {
                //new LogHelper().Log("FiservTopup request failled with reason : - " + response.ReasonPhrase, paymentRequest.RequestIdentifier, "CancelPayment", "API", "Payment");
                new LogHelper().Debug("FiservTopup request failled with reason: " + JsonConvert.SerializeObject(new Models.Fiserv.PaymentResponseModel()
                {
                    Result = false,
                    ResponseMessage = response.ReasonPhrase
                }), paymentRequest.RequestIdentifier, "FiservPayWIthToken", "API", "FiservPayment");
                return new Models.Fiserv.PaymentResponseModel()
                {

                    Result = false,
                    ResponseMessage = response.ReasonPhrase
                };
            }
           
        }

        [HttpPost]
        [ActionName("FiservPostAuthorization")]
        public async Task<Models.Fiserv.PaymentResponseModel> FiservPostAuthorization(Models.Fiserv.PaymentRequest paymentRequest)
        {
            new LogHelper().Debug("FiservPostAuthorization started : " + JsonConvert.SerializeObject(paymentRequest), paymentRequest.RequestIdentifier, "FiservPostAuthorization", "API", "FiservPayment");
            Models.Fiserv.CaptureRequest request = JsonConvert.DeserializeObject<Models.Fiserv.CaptureRequest>(paymentRequest.RequestObject.ToString());

            string currency = "EUR";
            if (string.IsNullOrEmpty(request.Currency))
            {
                currency = ConfigurationManager.AppSettings["PaymentCurrency"].ToString();
            }
            else
            {
                currency = request.Currency;
            }
            Models.Fiserv.PostAuthorizationRequestModel payWithTokenRequest = new PostAuthorizationRequestModel()
            {
                requestType = paymentRequest.RequestIdentifier,
                splitShipment = new SplitShipment()
                {
                   totalCount=1,
                   finalShipment= true
                },
                transactionAmount = new TransactionAmount()
                {
                    currency = currency,
                    total = request.Amount.ToString()
                }
            };

            var requestSTring = JsonConvert.SerializeObject(payWithTokenRequest);
            new LogHelper().Debug($"Fiserv PostAuthorization request : {requestSTring}", paymentRequest.RequestIdentifier, "FiservPostAuthorization", "API", "FiservPayment");
            HttpClient httpClient = null;
            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["IsPaymentProxyEnabled"]) && (Convert.ToBoolean(ConfigurationManager.AppSettings["IsPaymentProxyEnabled"])))
            {
                httpClient = new Helper.Helper().getProxyClient("Payment", ConfigurationManager.AppSettings["PaymentProxyURL"], ConfigurationManager.AppSettings["PaymentProxyUN"],
                    ConfigurationManager.AppSettings["PaymentProxyPSWD"]);
            }
            else
                httpClient = new HttpClient();

            var ClientRequestId = Guid.NewGuid();
            DateTime baseDate = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var time = (DateTime.Now.ToUniversalTime() - baseDate).TotalMilliseconds;
            var apiKey = paymentRequest.ApiKey;
            var rawSignature = apiKey + ClientRequestId + time + requestSTring;
            var secret = paymentRequest.merchantAccount;
            var provider = new System.Security.Cryptography.HMACSHA256(Encoding.UTF8.GetBytes(secret));
            var hash = provider.ComputeHash(Encoding.UTF8.GetBytes(rawSignature));
            var messageSignature = Convert.ToBase64String(hash);


            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            httpClient.DefaultRequestHeaders.Add("Client-Request-Id", ClientRequestId.ToString());
            httpClient.DefaultRequestHeaders.Add("Api-Key", apiKey);
            httpClient.DefaultRequestHeaders.Add("Timestamp", time.ToString());
            httpClient.DefaultRequestHeaders.Add("Message-Signature", messageSignature);

            HttpContent requestContent = new StringContent(requestSTring, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await httpClient.PostAsync("https://prod.emea.api.fiservapps.com/sandbox/ipp/payments-gateway/v2/payments/", requestContent);
            Models.Fiserv.PaymentResponse paymentResponseObject = new Models.Fiserv.PaymentResponse();
            if (response != null)
            {
                if (response.IsSuccessStatusCode)
                {


                    Models.Fiserv.FiservPaymentResponseModel modificationResult = JsonConvert.DeserializeObject<Models.Fiserv.FiservPaymentResponseModel>(await response.Content.ReadAsStringAsync());
                    new LogHelper().Debug("Fiserv Get payment top up response : " + JsonConvert.SerializeObject(modificationResult), paymentRequest.RequestIdentifier, "FiservPaymentCapture", "API", "FiservPayment");
                    if (modificationResult != null)
                    {

                        paymentResponseObject.MaskCardNumber = modificationResult.cardnumber;
                        paymentResponseObject.CardExpiryDate = modificationResult.expmonth = "/" + modificationResult.expyear;
                        paymentResponseObject.AuthCode = modificationResult.oid;
                        paymentResponseObject.Amount = Convert.ToDecimal(modificationResult.chargetotal);
                        paymentResponseObject.Currency = modificationResult.currency;
                        paymentResponseObject.PspReference = modificationResult.ipgTransactionId;
                        paymentResponseObject.ResultCode = modificationResult.status;
                        paymentResponseObject.CardToken = modificationResult.hosteddataid;

                        paymentResponseObject.CardType = modificationResult.ccbrand;
                        List<Models.Fiserv.AdditionalInfo> additionalInfos = new List<Models.Fiserv.AdditionalInfo>();
                        var values = Newtonsoft.Json.JsonConvert.SerializeObject(modificationResult);
                        var paymentDetailResponseModels = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(values);

                        foreach (KeyValuePair<string, string> keyValuePair in paymentDetailResponseModels)
                        {
                            Models.Fiserv.AdditionalInfo additionalInfo = new Models.Fiserv.AdditionalInfo();
                            additionalInfo.key = keyValuePair.Key;
                            additionalInfo.value = keyValuePair.Value;

                            additionalInfos.Add(additionalInfo);
                        }
                        paymentResponseObject.additionalInfos = additionalInfos;


                        new LogHelper().Log("payment top up completed successfully", paymentRequest.RequestIdentifier, "PaymentTopUp", "API", "Payment");
                        new LogHelper().Debug("Get payment top up response : " + JsonConvert.SerializeObject(new Models.Fiserv.PaymentResponseModel()
                        {
                            ResponseObject = paymentResponseObject,
                            Result = true
                        }), paymentRequest.RequestIdentifier, "PaymentTopUp", "API", "Payment");
                        return new Models.Fiserv.PaymentResponseModel()
                        {
                            ResponseObject = paymentResponseObject,
                            Result = true
                        };
                    }
                    else
                    {
                        new LogHelper().Log("payment top up completed failled with reason :- " + modificationResult.fail_reason, paymentRequest.RequestIdentifier, "PaymentTopUp", "API", "Payment");
                        new LogHelper().Debug("Get payment top up response : " + JsonConvert.SerializeObject(new Models.Fiserv.PaymentResponseModel()
                        {
                            ResponseMessage = modificationResult.fail_reason,
                            Result = false
                        }), paymentRequest.RequestIdentifier, "PaymentTopUp", "API", "Payment");
                        return new Models.Fiserv.PaymentResponseModel()
                        {
                            ResponseMessage = modificationResult.fail_reason,
                            Result = false
                        };
                    }
                }
                else
                {
                    new LogHelper().Debug("FiservTopup request failled with reason: " + JsonConvert.SerializeObject(new Models.Fiserv.PaymentResponseModel()
                    {
                        Result = false,
                        ResponseMessage = response.ReasonPhrase
                    }), paymentRequest.RequestIdentifier, "FiservPostAuthorization", "API", "FiservPayment");
                    return new Models.Fiserv.PaymentResponseModel()
                    {
                        Result = false,
                        ResponseMessage = response.ReasonPhrase
                    };
                }
            }
            else
            {
                //new LogHelper().Log("FiservTopup request failled with reason : - " + response.ReasonPhrase, paymentRequest.RequestIdentifier, "CancelPayment", "API", "Payment");
                new LogHelper().Debug("FiservTopup request failled with reason: " + JsonConvert.SerializeObject(new Models.Fiserv.PaymentResponseModel()
                {
                    Result = false,
                    ResponseMessage = response.ReasonPhrase
                }), paymentRequest.RequestIdentifier, "FiservPostAuthorization", "API", "FiservPayment");
                return new Models.Fiserv.PaymentResponseModel()
                {

                    Result = false,
                    ResponseMessage = response.ReasonPhrase
                };
            }
          
        }
        [HttpPost]
        [ActionName("FiservTopup")]
        public async Task<Models.Fiserv.PaymentResponseModel> FiservPaymentCapture(Models.Fiserv.PaymentRequestModel paymentRequest)
        {
            new LogHelper().Debug("FiservTopup started : " + JsonConvert.SerializeObject(paymentRequest), paymentRequest.RequestIdentifier, "FiservPaymentCapture", "API", "FiservPayment");
            Models.Fiserv.CaptureRequest request = JsonConvert.DeserializeObject<Models.Fiserv.CaptureRequest>(paymentRequest.RequestObject.ToString());

            string currency = "EUR";
            if (string.IsNullOrEmpty(request.Currency))
            {
                currency = ConfigurationManager.AppSettings["PaymentCurrency"].ToString();
            }
            else
            {
                currency = request.Currency;
            }
            Models.Fiserv.PostTopUpFiserv payWithTokenRequest = new PostTopUpFiserv()
            {
                requestType = paymentRequest.RequestIdentifier,
                merchantTransactionId=request.MerchantReference,
                decrementalFlag=false,
                order = new order()
                {
                    orderId = request.OrginalPSPRefernce
                   
                },
                transactionAmount = new TransactionAmount()
                {
                    currency = currency,
                    total = request.Amount.ToString()
                }
            };

            var requestSTring = JsonConvert.SerializeObject(payWithTokenRequest);
            new LogHelper().Debug($"Fiserv PostAuthorization request : {requestSTring}", paymentRequest.RequestIdentifier, "FiservPaymentCapture", "API", "FiservPayment");
            HttpClient httpClient = null;
            if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["IsPaymentProxyEnabled"]) && (Convert.ToBoolean(ConfigurationManager.AppSettings["IsPaymentProxyEnabled"])))
            {
                httpClient = new Helper.Helper().getProxyClient("Payment", ConfigurationManager.AppSettings["PaymentProxyURL"], ConfigurationManager.AppSettings["PaymentProxyUN"],
                    ConfigurationManager.AppSettings["PaymentProxyPSWD"]);
            }
            else
                httpClient = new HttpClient();

            var ClientRequestId = Guid.NewGuid();
            DateTime baseDate = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var time = (DateTime.Now.ToUniversalTime() - baseDate).TotalMilliseconds;
            var apiKey = paymentRequest.ApiKey;
            var rawSignature = apiKey + ClientRequestId + time + requestSTring;
            var secret = paymentRequest.merchantAccount;
            var provider = new System.Security.Cryptography.HMACSHA256(Encoding.UTF8.GetBytes(secret));
            var hash = provider.ComputeHash(Encoding.UTF8.GetBytes(rawSignature));
            var messageSignature = Convert.ToBase64String(hash);


            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            httpClient.DefaultRequestHeaders.Add("Client-Request-Id", ClientRequestId.ToString());
            httpClient.DefaultRequestHeaders.Add("Api-Key", apiKey);
            httpClient.DefaultRequestHeaders.Add("Timestamp", time.ToString());
            httpClient.DefaultRequestHeaders.Add("Message-Signature", messageSignature);

            HttpContent requestContent = new StringContent(requestSTring, Encoding.UTF8, "application/json");
            new LogHelper().Debug("Fiserv Get payment top up request", paymentRequest.RequestIdentifier, "FiservPaymentCapture", "API", "FiservPayment");

            HttpResponseMessage response = await httpClient.PostAsync("https://prod.emea.api.fiservapps.com/sandbox/ipp/payments-gateway/v2/payments", requestContent);
            Models.Fiserv.PaymentResponse paymentResponseObject = new Models.Fiserv.PaymentResponse();
            if (response != null)
            {
                if (response.IsSuccessStatusCode)
                {


                    Models.Fiserv.FiservPaymentResponseModel modificationResult = JsonConvert.DeserializeObject<Models.Fiserv.FiservPaymentResponseModel>(await response.Content.ReadAsStringAsync());
                    new LogHelper().Debug("Fiserv Get payment top up response : " + JsonConvert.SerializeObject(modificationResult), paymentRequest.RequestIdentifier, "FiservPaymentCapture", "API", "FiservPayment");
                    if (modificationResult != null)
                    {
                        
                  paymentResponseObject.MaskCardNumber = modificationResult.cardnumber;
                  paymentResponseObject.CardExpiryDate= modificationResult.expmonth="/"+modificationResult.expyear;
                  paymentResponseObject.AuthCode= modificationResult.oid;
                  paymentResponseObject.Amount = Convert.ToDecimal(modificationResult.chargetotal);
                  paymentResponseObject.Currency = modificationResult.currency;
                  paymentResponseObject.PspReference= modificationResult.ipgTransactionId;
                   paymentResponseObject.ResultCode=modificationResult.status;
                        paymentResponseObject.CardToken = modificationResult.hosteddataid;

                        paymentResponseObject.CardType = modificationResult.ccbrand;
                        List<Models.Fiserv.AdditionalInfo> additionalInfos = new List<Models.Fiserv.AdditionalInfo>();
                        var values = Newtonsoft.Json.JsonConvert.SerializeObject(modificationResult);
                        var paymentDetailResponseModels = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(values);

                        foreach (KeyValuePair<string, string> keyValuePair in paymentDetailResponseModels)
                            {
                                Models.Fiserv.AdditionalInfo additionalInfo = new Models.Fiserv.AdditionalInfo();
                                additionalInfo.key = keyValuePair.Key;
                                additionalInfo.value = keyValuePair.Value;
                              
                                additionalInfos.Add(additionalInfo);
                            }
                            paymentResponseObject.additionalInfos = additionalInfos;
                        

                        new LogHelper().Debug("Payment capture response : " + JsonConvert.SerializeObject(new Models.Fiserv.PaymentResponseModel()
                        {
                            ResponseObject = paymentResponseObject,
                            Result = true
                        }), paymentRequest.RequestIdentifier, "PaymentCapture", "API", "Payment");
                        new LogHelper().Log("Processing Payment capture completed successfully", paymentRequest.RequestIdentifier, "PaymentCapture", "API", "Payment");
                        return new Models.Fiserv.PaymentResponseModel()
                        {
                            ResponseObject = paymentResponseObject,
                            Result = true
                        };
                    }
                    else
                    {


                        new LogHelper().Debug("Payment capture response : " + JsonConvert.SerializeObject(new Models.Fiserv.PaymentResponseModel()
                        {
                            ResponseMessage = modificationResult.fail_reason,
                            Result = false
                        }), paymentRequest.RequestIdentifier, "PaymentCapture", "API", "Payment");
                        new LogHelper().Log("Processing Payment capture failled with reason " + modificationResult.fail_reason, paymentRequest.RequestIdentifier, "PaymentCapture", "API", "Payment");
                        return new Models.Fiserv.PaymentResponseModel()
                        {
                            ResponseObject = modificationResult.fail_reason,
                            Result = false
                        };
                    }
                }
                else
                {
                    new LogHelper().Debug("FiservTopup request failled with reason: " + JsonConvert.SerializeObject(new Models.Fiserv.PaymentResponseModel()
                    {
                        Result = false,
                        ResponseMessage = response.ReasonPhrase
                    }), paymentRequest.RequestIdentifier, "FiservPaymentCapture", "API", "FiservPayment");
                    return new Models.Fiserv.PaymentResponseModel()
                    {
                        Result = false,
                        ResponseMessage= response.ReasonPhrase
                    };
                }
            }
            else
            {
                //new LogHelper().Log("FiservTopup request failled with reason : - " + response.ReasonPhrase, paymentRequest.RequestIdentifier, "CancelPayment", "API", "Payment");
                new LogHelper().Debug("FiservTopup request failled with reason: " + JsonConvert.SerializeObject(new Models.Fiserv.PaymentResponseModel()
                {
                    Result = false,
                    ResponseMessage = response.ReasonPhrase
                }), paymentRequest.RequestIdentifier, "TopUpPayment", "API", "Payment");
                return new Models.Fiserv.PaymentResponseModel()
                {

                    Result = false,
                    ResponseMessage = response.ReasonPhrase
                };
            }
            
          
        }

    }
}