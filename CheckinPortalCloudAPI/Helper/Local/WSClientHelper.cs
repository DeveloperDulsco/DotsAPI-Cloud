using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace CheckinPortalCloudAPI.Helper.Local
{
    public class WSClientHelper
    {
        public async Task<Models.Cloud.CloudResponseModel> FetchPrechedinRecord(Models.Cloud.CloudRequestModel cloudRequest ,Models.Local.ServiceParameters serviceParameters)
        {
            try
            {
                //new LogHelper().Debug("Fetching pre checked in records using web api", null, "FetchPrechedinRecord", serviceParameters.ClientID, "pre checked-in fetch");
                ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
                {
                    return true;
                };
                HttpClient httpClient = serviceParameters.isProxyEnableForCloudAPI ? getProxyClient("pre checked-in fetch", serviceParameters.CloudAPIProxyHost, serviceParameters.CloudAPIProxyUN, serviceParameters.CloudAPIProxyPswd,serviceParameters.ClientID) : new HttpClient();
                if (httpClient == null)
                {
                    new LogHelper().Debug("Failled to fetching pre checked in records using web api due to proxy error", null, "FetchPrechedinRecord", serviceParameters.ClientID, "pre checked-in fetch");
                    return new Models.Cloud.CloudResponseModel()
                    {
                        result = false,
                        responseMessage = "Failled to generate the proxy http client"
                    };
                }
                new LogHelper().Debug("web api url :- " + serviceParameters.CloudAPIURL + @"/cloud/FetchPreCheckedinReservationByReservationNumber", null, "FetchPrechedinRecord", serviceParameters.ClientID, "pre checked-in fetch");
                httpClient.DefaultRequestHeaders.Clear();
                string requestString = JsonConvert.SerializeObject(cloudRequest, Formatting.None);
                var requestContent = new StringContent(requestString, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await httpClient.PostAsync(serviceParameters.CloudAPIURL + @"/cloud/FetchPreCheckedinReservationByReservationNumber", requestContent);
                if (response != null)
                {
                    if (response.IsSuccessStatusCode)
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        new LogHelper().Debug("web API response :- " + apiResponse, null, "FetchPrechedinRecord", serviceParameters.ClientID, "pre checked-in fetch");
                        Models.Cloud.CloudResponseModel cloudResponse = JsonConvert.DeserializeObject<Models.Cloud.CloudResponseModel>(apiResponse);
                        return cloudResponse;
                    }
                    else
                    {
                        new LogHelper().Debug("Failled to fetching pre checked in records using web api due to HTTP error : " + response.ReasonPhrase, null, "FetchPrechedinRecord", serviceParameters.ClientID, "pre checked-in fetch");
                        return new Models.Cloud.CloudResponseModel()
                        {
                            result = false,
                            responseMessage = response.ReasonPhrase
                        };
                    }
                }
                else
                {
                    new LogHelper().Debug("Failled to fetching pre checked in records using web api due to null returned from the cloud web api", null, "FetchPrechedinRecord", serviceParameters.ClientID, "pre checked-in fetch");
                    return new Models.Cloud.CloudResponseModel()
                    {
                        result = false,
                        responseMessage = "Cloud web API returned null"
                    };
                }
            }
            catch (Exception ex)
            {
                new LogHelper().Error(ex, null, "FetchPrechedinRecord", serviceParameters.ClientID, "pre checked-in fetch");
                return new Models.Cloud.CloudResponseModel()
                {
                    result = false,
                    responseMessage = "Generic Exception : " + ex.Message
                };
            }
        }

        public async Task<Models.Cloud.CloudResponseModel> FetchPrechedoutRecord(Models.Cloud.CloudRequestModel cloudRequest, Models.Local.ServiceParameters serviceParameters)
        {
            try
            {
                
                new LogHelper().Debug("Fetching pre checked out records using web api", null, "FetchPrechedoutRecord", serviceParameters.ClientID, "pre checked-out fetch");
                ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
                {
                    return true;
                };
                HttpClient httpClient = serviceParameters.isProxyEnableForCloudAPI ? getProxyClient("pre checked-out fetch", serviceParameters.CloudAPIProxyHost, serviceParameters.CloudAPIProxyUN, serviceParameters.CloudAPIProxyPswd,serviceParameters.ClientID) : new HttpClient();
                if (httpClient == null)
                {
                    new LogHelper().Debug("Failled to fetching pre checked out records using web api due to proxy error", null, "FetchPrechedoutRecord", serviceParameters.ClientID, "pre checked-out fetch");
                    return new Models.Cloud.CloudResponseModel()
                    {
                        result = false,
                        responseMessage = "Failled to generate the proxy http client"
                    };
                }
                new LogHelper().Debug("web api url :- " + serviceParameters.CloudAPIURL + @"/cloud/FetchPreCheckedoutReservationByReservationNumber", null, "FetchPrechedoutRecord", serviceParameters.ClientID, "pre checked-out fetch");
                new LogHelper().Debug("Web API Request :- " + JsonConvert.SerializeObject(cloudRequest), null, "FetchPrechedoutRecord", serviceParameters.ClientID, "pre checked-out fetch");
                httpClient.DefaultRequestHeaders.Clear();
                string requestString = JsonConvert.SerializeObject(cloudRequest, Formatting.None);
                var requestContent = new StringContent(requestString, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await httpClient.PostAsync(serviceParameters.CloudAPIURL + @"/cloud/FetchPreCheckedoutReservationByReservationNumber", requestContent);
                if (response != null)
                {
                    if (response.IsSuccessStatusCode)
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        new LogHelper().Debug("web API response :- " + apiResponse, null, "FetchPrechedoutRecord", serviceParameters.ClientID, "pre checked-out fetch");
                        Models.Cloud.CloudResponseModel cloudResponse = JsonConvert.DeserializeObject<Models.Cloud.CloudResponseModel>(apiResponse);
                        return cloudResponse;
                    }
                    else
                    {
                        new LogHelper().Debug("Failled to fetching pre checked out records using web api due to HTTP error : " + response.ReasonPhrase, null, "FetchPrechedoutRecord", serviceParameters.ClientID, "pre checked-out fetch");
                        return new Models.Cloud.CloudResponseModel()
                        {
                            result = false,
                            responseMessage = response.ReasonPhrase
                        };
                    }
                }
                else
                {
                    new LogHelper().Debug("Failled to fetching pre checked out records using web api due to null returned from the cloud web api", null, "FetchPrechedoutRecord", serviceParameters.ClientID, "pre checked-out fetch");
                    return new Models.Cloud.CloudResponseModel()
                    {
                        result = false,
                        responseMessage = "Cloud web API returned null"
                    };
                }
            }
            catch (Exception ex)
            {
                new LogHelper().Error(ex, null, "FetchPrechedoutRecord", serviceParameters.ClientID, "pre checked-out fetch");
                return new Models.Cloud.CloudResponseModel()
                {
                    result = false,
                    responseMessage = "Generic Exception : " + ex.Message
                };
            }
        }

        HttpClient getProxyClient(string groupName, string ProxyHost, string proxyUserName, string proxyPassword,string clientID)
        {
            new LogHelper().Debug("assigning proxy credentials :- (host:" + ProxyHost + ",UN:" + proxyUserName + ",Password:" + proxyPassword + ")", null, "getProxyClient", clientID, groupName);
            try
            {
                HttpClientHandler handler = new HttpClientHandler();
                handler.UseDefaultCredentials = true;
                var proxy = new WebProxy
                {
                    Address = new Uri(ProxyHost),
                    BypassProxyOnLocal = false,
                    UseDefaultCredentials = false,

                    Credentials = new NetworkCredential(
                    userName: proxyUserName,
                    password: proxyPassword)
                };

                var httpClientHandler = new HttpClientHandler
                {
                    Proxy = proxy,
                };
                new LogHelper().Debug("proxy credentials assigned", null, "getProxyClient", clientID, groupName);
                return new HttpClient(handler: httpClientHandler, disposeHandler: true);
            }
            catch (Exception ex)
            {
                new LogHelper().Error(ex, null, "getProxyClient", clientID, groupName);
                return null;
            }

        }

        public async Task<Models.Local.LocalResponseModel> PushRecordLocally(Models.Local.LocalRequestModel localRequest, string reservationNameID, string groupName, Models.Local.ServiceParameters serviceParameters)
        {
            try
            {
                new LogHelper().Debug("Pushing records using web api", reservationNameID, "PushRecordLocally", serviceParameters.ClientID, groupName);
                //new Controllers.LocalController().PushReservationDetails(localRequest);

                HttpClient httpClient = serviceParameters.isProxyEnableForLocalAPI ? getProxyClient(groupName, serviceParameters.LocalAPIProxyHost, serviceParameters.LocalAPIProxyUN, serviceParameters.LocalAPIProxyPswd,serviceParameters.ClientID) : new HttpClient();
                if (httpClient == null)
                {
                    new LogHelper().Debug("Failled to push records using web api due to proxy error", reservationNameID, "PushRecordLocally", serviceParameters.ClientID, groupName);
                    return new Models.Local.LocalResponseModel()
                    {
                        result = false,
                        responseMessage = "Failled to generate the proxy http client"
                    };
                }
                httpClient.DefaultRequestHeaders.Clear();
                string requestString = JsonConvert.SerializeObject(localRequest, Formatting.None);
                var requestContent = new StringContent(requestString, Encoding.UTF8, "application/json");
                new LogHelper().Debug("web api url :- " + serviceParameters.LocalAPIURL + @"/local/PushReservationDetails", reservationNameID, "PushRecordLocally", serviceParameters.ClientID, groupName);
                new LogHelper().Debug("web api request :- " + requestString, reservationNameID, "PushRecordLocally", serviceParameters.ClientID, groupName);
                HttpResponseMessage response = await httpClient.PostAsync(serviceParameters.LocalAPIURL + @"/local/PushReservationDetails", requestContent);
                if (response != null)
                {
                    if (response.IsSuccessStatusCode)
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        new LogHelper().Debug("web API response :- " + apiResponse, reservationNameID, "PushRecordLocally", serviceParameters.ClientID, groupName);
                        Models.Local.LocalResponseModel localResponse = JsonConvert.DeserializeObject<Models.Local.LocalResponseModel>(apiResponse);
                        return localResponse;
                    }
                    else
                    {
                        new LogHelper().Debug("Failled to push pre checked in records using web api due to HTTP error : " + response.ReasonPhrase, reservationNameID, "PushRecordLocally", serviceParameters.ClientID, groupName);
                        return new Models.Local.LocalResponseModel()
                        {
                            result = false,
                            responseMessage = response.ReasonPhrase
                        };
                    }
                }
                else
                {
                    new LogHelper().Debug("Failled to push pre checked in records using web api due to null returned from the local web api", reservationNameID, "PushRecordLocally", serviceParameters.ClientID, groupName);
                    return new Models.Local.LocalResponseModel()
                    {
                        result = false,
                        responseMessage = "Local web API returned null"
                    };
                }
            }
            catch (Exception ex)
            {
                new LogHelper().Error(ex, reservationNameID, "PushRecordLocally", serviceParameters.ClientID, groupName);
                return new Models.Local.LocalResponseModel()
                {
                    result = false,
                    responseMessage = "Generic Exception : " + ex.Message
                };
            }
        }

        public async Task<Models.Email.EmailResponse> SendEmail(string reservationNameID, Models.Email.EmailRequest emailRequest, string groupName, Models.Local.ServiceParameters serviceParameters)
        {
            try
            {
                new LogHelper().Debug("Sending email using web api", reservationNameID, "SendEmail", serviceParameters.ClientID, groupName);
                ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
                {
                    return true;
                };

                HttpClient httpClient = serviceParameters.isProxyEnableForEmailAPI ? getProxyClient(groupName, serviceParameters.EmailAPIProxyHost, serviceParameters.EmailAPIProxyUN, serviceParameters.EmailAPIProxyPswd,serviceParameters.ClientID) : new HttpClient();
                if (httpClient == null)
                {
                    new LogHelper().Debug("Failled to send email using web api due to proxy error", reservationNameID, "SendEmail", serviceParameters.ClientID, groupName);
                    return new Models.Email.EmailResponse()
                    {
                        result = false,
                        responseMessage = "Failled to generate the proxy http client"
                    };
                }
                httpClient.DefaultRequestHeaders.Clear();
                string requestString = JsonConvert.SerializeObject(emailRequest, Formatting.None);
                var requestContent = new StringContent(requestString, Encoding.UTF8, "application/json");
                new LogHelper().Debug("web api url :- " + serviceParameters.EmailURL + @"/email/SendEmail", reservationNameID, "SendEmail", serviceParameters.ClientID, groupName);
                new LogHelper().Debug("web api request :- " + requestString, reservationNameID, "SendEmail", serviceParameters.ClientID, groupName);
                HttpResponseMessage response = await httpClient.PostAsync(serviceParameters.EmailURL + @"/email/SendEmail", requestContent);
                if (response != null)
                {
                    if (response.IsSuccessStatusCode)
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        new LogHelper().Debug("web API response :- " + apiResponse, reservationNameID, "SendEmail", serviceParameters.ClientID, groupName);
                        Models.Email.EmailResponse emailResponse = JsonConvert.DeserializeObject<Models.Email.EmailResponse>(apiResponse);
                        return emailResponse;
                    }
                    else
                    {
                        new LogHelper().Debug("Failled to send email using web api due to HTTP error : " + response.ReasonPhrase, reservationNameID, "SendEmail", serviceParameters.ClientID, groupName);
                        return new Models.Email.EmailResponse()
                        {
                            result = false,
                            responseMessage = response.ReasonPhrase
                        };
                    }
                }
                else
                {
                    new LogHelper().Debug("Failled to send email using web api due to null returned from the local web api", reservationNameID, "SendEmail", serviceParameters.ClientID, groupName);
                    return new Models.Email.EmailResponse()
                    {
                        result = false,
                        responseMessage = "Email web API returned null"
                    };
                }
            }
            catch (Exception ex)
            {
                new LogHelper().Error(ex, reservationNameID, "SendEmail", serviceParameters.ClientID, groupName);
                return new Models.Email.EmailResponse()
                {
                    result = false,
                    responseMessage = "Generic Exception : " + ex.Message
                };
            }
        }

        public async Task<Models.Cloud.CloudResponseModel> FetchDocumentDetails(string reservationNameID, Models.Cloud.CloudRequestModel cloudRequest, string groupName, Models.Local.ServiceParameters serviceParameters)
        {
            try
            {
                new LogHelper().Debug("Fetching profile documents using web api", reservationNameID, "FetchDocumentDetails", serviceParameters.ClientID, groupName);
                ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
                {
                    return true;
                };
                HttpClient httpClient = serviceParameters.isProxyEnableForCloudAPI ? getProxyClient(groupName, serviceParameters.CloudAPIProxyHost, serviceParameters.CloudAPIProxyUN, serviceParameters.CloudAPIProxyPswd,serviceParameters.ClientID) : new HttpClient();
                if (httpClient == null)
                {
                    new LogHelper().Debug("Failled to fetch profile documents using web api due to proxy error", reservationNameID, "FetchDocumentDetails", serviceParameters.ClientID, groupName);
                    return new Models.Cloud.CloudResponseModel()
                    {
                        result = false,
                        responseMessage = "Failled to generate the proxy http client"
                    };
                }
                new LogHelper().Debug("web api url :- " + serviceParameters.CloudAPIURL + @"/cloud/FetchProfileDocumentDetails", reservationNameID, "FetchDocumentDetails", serviceParameters.ClientID, groupName);
                httpClient.DefaultRequestHeaders.Clear();
                string requestString = JsonConvert.SerializeObject(cloudRequest, Formatting.None);
                new LogHelper().Debug("web api request :- " + requestString, reservationNameID, "FetchDocumentDetails", serviceParameters.ClientID, groupName);
                var requestContent = new StringContent(requestString, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await httpClient.PostAsync(serviceParameters.CloudAPIURL + @"/cloud/FetchProfileDocumentDetails", requestContent);
                if (response != null)
                {
                    if (response.IsSuccessStatusCode)
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        new LogHelper().Debug("web API response :- " + apiResponse, reservationNameID, "FetchDocumentDetails", serviceParameters.ClientID, groupName);
                        new LogHelper().Log("web API response :- " + apiResponse, reservationNameID, "FetchDocumentDetails", serviceParameters.ClientID, groupName);
                        Models.Cloud.CloudResponseModel cloudResponse = JsonConvert.DeserializeObject<Models.Cloud.CloudResponseModel>(apiResponse);
                        return cloudResponse;
                    }
                    else
                    {
                        new LogHelper().Debug("Failled to fetch profile documents using web api due to HTTP error : " + response.ReasonPhrase, reservationNameID, "FetchDocumentDetails", serviceParameters.ClientID, groupName);
                        return new Models.Cloud.CloudResponseModel()
                        {
                            result = false,
                            responseMessage = response.ReasonPhrase
                        };
                    }
                }
                else
                {
                    new LogHelper().Debug("Failled to fetch profile documents using web api due to null returned from the local web api", reservationNameID, "FetchDocumentDetails", serviceParameters.ClientID, groupName);
                    return new Models.Cloud.CloudResponseModel()
                    {
                        result = false,
                        responseMessage = "Cloud web API returned null"
                    };
                }
            }
            catch (Exception ex)
            {
                new LogHelper().Error(ex, reservationNameID, "FetchDocumentDetails", serviceParameters.ClientID, groupName);
                return new Models.Cloud.CloudResponseModel()
                {
                    result = false,
                    responseMessage = "Generic Exception : " + ex.Message
                };
            }
        }

        public async Task<Models.Local.LocalResponseModel> InsertDocuments(string reservationNameID, Models.Local.LocalRequestModel localRequest, string groupName, Models.Local.ServiceParameters serviceParameters)
        {
            try
            {
                new LogHelper().Debug("Pushing profile documents using web api", reservationNameID, "InsertDocuments", serviceParameters.ClientID, groupName);
                HttpClient httpClient = serviceParameters.isProxyEnableForLocalAPI ? getProxyClient(groupName, serviceParameters.LocalAPIProxyHost, serviceParameters.LocalAPIProxyUN, serviceParameters.LocalAPIProxyPswd,serviceParameters.ClientID) : new HttpClient();
                if (httpClient == null)
                {
                    new LogHelper().Debug("Failled to push profile documents using web api due to proxy error", reservationNameID, "InsertDocuments", serviceParameters.ClientID, groupName);
                    return new Models.Local.LocalResponseModel()
                    {
                        result = false,
                        responseMessage = "Failled to generate the proxy http client"
                    };
                }
                httpClient.DefaultRequestHeaders.Clear();
                string requestString = JsonConvert.SerializeObject(localRequest, Formatting.None);
                new LogHelper().Debug("web api url :- " + serviceParameters.LocalAPIURL + @"/local/PushDocumentDetails", reservationNameID, "InsertDocuments", serviceParameters.ClientID, groupName);
                new LogHelper().Debug("web api request :- " + requestString, reservationNameID, "InsertDocuments", serviceParameters.ClientID, groupName);
                var requestContent = new StringContent(requestString, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await httpClient.PostAsync(serviceParameters.LocalAPIURL + @"/local/PushDocumentDetails", requestContent);
                if (response != null)
                {
                    if (response.IsSuccessStatusCode)
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        new LogHelper().Debug("web API response :- " + apiResponse, reservationNameID, "InsertDocuments", serviceParameters.ClientID, groupName);
                        Models.Local.LocalResponseModel localResponse = JsonConvert.DeserializeObject<Models.Local.LocalResponseModel>(apiResponse);
                        return localResponse;
                    }
                    else
                    {
                        new LogHelper().Debug("Failled to push profile documents using web api due to HTTP error : " + response.ReasonPhrase, reservationNameID, "InsertDocuments", serviceParameters.ClientID, groupName);
                        return new Models.Local.LocalResponseModel()
                        {
                            result = false,
                            responseMessage = response.ReasonPhrase
                        };
                    }
                }
                else
                {
                    new LogHelper().Debug("Failled to push profile documents using web api due to null returned from the local web api", reservationNameID, "InsertDocuments", serviceParameters.ClientID, groupName);
                    return new Models.Local.LocalResponseModel()
                    {
                        result = false,
                        responseMessage = "Local web API returned null"
                    };
                }
            }
            catch (Exception ex)
            {
                new LogHelper().Error(ex, reservationNameID, "InsertDocuments", serviceParameters.ClientID, groupName);
                return new Models.Local.LocalResponseModel()
                {
                    result = false,
                    responseMessage = "Generic Exception : " + ex.Message
                };
            }
        }

        public async Task<Models.OWS.OwsResponseModel> CreateAccompanyingProfile(string reservationNameID, Models.OWS.OwsRequestModel owsRequest, string groupName, Models.Local.ServiceParameters serviceParameters)
        {
            try
            {
                new LogHelper().Debug("Create accompanying profile using web api", reservationNameID, "CreateAccompanyingProfile", serviceParameters.ClientID, groupName);
                HttpClient httpClient = serviceParameters.isProxyEnableForLocalAPI ? getProxyClient(groupName, serviceParameters.LocalAPIProxyHost, serviceParameters.LocalAPIProxyUN, serviceParameters.LocalAPIProxyPswd,serviceParameters.ClientID) : new HttpClient();
                if (httpClient == null)
                {
                    new LogHelper().Debug("Failled to Create accompanying profile using web api due to proxy error", reservationNameID, "CreateAccompanyingProfile", serviceParameters.ClientID, groupName);
                    return new Models.OWS.OwsResponseModel()
                    {
                        result = false,
                        responseMessage = "Failled to generate the proxy http client"
                    };
                }
                httpClient.DefaultRequestHeaders.Clear();
                string requestString = JsonConvert.SerializeObject(owsRequest, Formatting.None);
                new LogHelper().Debug("web api url :- " + serviceParameters.LocalAPIURL + @"/ows/CreateAccompanyingGuset", reservationNameID, "CreateAccompanyingProfile", serviceParameters.ClientID, groupName);
                new LogHelper().Debug("web api request :- " + requestString, reservationNameID, "CreateAccompanyingProfile", serviceParameters.ClientID, groupName);
                var requestContent = new StringContent(requestString, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await httpClient.PostAsync(serviceParameters.LocalAPIURL + @"/ows/CreateAccompanyingGuset", requestContent);
                if (response != null)
                {
                    if (response.IsSuccessStatusCode)
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        new LogHelper().Debug("web API response :- " + apiResponse, reservationNameID, "CreateAccompanyingProfile", serviceParameters.ClientID, groupName);
                        Models.OWS.OwsResponseModel owsResponse = JsonConvert.DeserializeObject<Models.OWS.OwsResponseModel>(apiResponse);
                        return owsResponse;
                    }
                    else
                    {
                        new LogHelper().Debug("Failled to create accompanying profile using web api due to HTTP error : " + response.ReasonPhrase, reservationNameID, "CreateAccompanyingProfile", serviceParameters.ClientID, groupName);
                        return new Models.OWS.OwsResponseModel()
                        {
                            result = false,
                            responseMessage = response.ReasonPhrase
                        };
                    }
                }
                else
                {
                    new LogHelper().Debug("Failled to create accompanying profile using web api due to null returned from the local web api", reservationNameID, "CreateAccompanyingProfile", serviceParameters.ClientID, groupName);
                    return new Models.OWS.OwsResponseModel()
                    {
                        result = false,
                        responseMessage = "Local web API returned null"
                    };
                }
            }
            catch (Exception ex)
            {
                new LogHelper().Error(ex, reservationNameID, "CreateAccompanyingProfile", serviceParameters.ClientID, groupName);
                return new Models.OWS.OwsResponseModel()
                {
                    result = false,
                    responseMessage = "Generic Exception : " + ex.Message
                };
            }
        }

        public async Task<Models.OWS.OwsResponseModel> UpdateGuestProfile(string reservationNameID, Models.OWS.OwsRequestModel owsRequest, string groupName, Models.Local.ServiceParameters serviceParameters)
        {
            try
            {
                new LogHelper().Debug("Updating guest profile using web api", reservationNameID, "UpdateGuestProfile", serviceParameters.ClientID, groupName);
                HttpClient httpClient = serviceParameters.isProxyEnableForLocalAPI ? getProxyClient(groupName, serviceParameters.LocalAPIProxyHost, serviceParameters.LocalAPIProxyUN, serviceParameters.LocalAPIProxyPswd,serviceParameters.ClientID) : new HttpClient();
                if (httpClient == null)
                {
                    new LogHelper().Debug("Failled to update guest profile using web api due to proxy error", reservationNameID, "UpdateGuestProfile", serviceParameters.ClientID, groupName);
                    return new Models.OWS.OwsResponseModel()
                    {
                        result = false,
                        responseMessage = "Failled to generate the proxy http client"
                    };
                }
                httpClient.DefaultRequestHeaders.Clear();
                string requestString = JsonConvert.SerializeObject(owsRequest, Formatting.None);
                new LogHelper().Debug("web api url :- " + serviceParameters.LocalAPIURL + @"/ows/UpdateName", reservationNameID, "UpdateGuestProfile", serviceParameters.ClientID, groupName);
                new LogHelper().Debug("web api request :- " + requestString, reservationNameID, "UpdateGuestProfile", serviceParameters.ClientID, groupName);
                var requestContent = new StringContent(requestString, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await httpClient.PostAsync(serviceParameters.LocalAPIURL + @"/ows/UpdateName", requestContent);
                if (response != null)
                {
                    if (response.IsSuccessStatusCode)
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        new LogHelper().Debug("web API response :- " + apiResponse, reservationNameID, "UpdateGuestProfile", serviceParameters.ClientID, groupName);
                        Models.OWS.OwsResponseModel owsResponse = JsonConvert.DeserializeObject<Models.OWS.OwsResponseModel>(apiResponse);
                        return owsResponse;
                    }
                    else
                    {
                        new LogHelper().Debug("Failled to update profile using web api due to HTTP error : " + response.ReasonPhrase, reservationNameID, "UpdateGuestProfile", serviceParameters.ClientID, groupName);
                        return new Models.OWS.OwsResponseModel()
                        {
                            result = false,
                            responseMessage = response.ReasonPhrase
                        };
                    }
                }
                else
                {
                    new LogHelper().Debug("Failled to update profile using web api due to null returned from the local web api", reservationNameID, "UpdateGuestProfile", serviceParameters.ClientID, groupName);
                    return new Models.OWS.OwsResponseModel()
                    {
                        result = false,
                        responseMessage = "Local web API returned null"
                    };
                }
            }
            catch (Exception ex)
            {
                new LogHelper().Error(ex, reservationNameID, "UpdateGuestProfile", serviceParameters.ClientID, groupName);
                return new Models.OWS.OwsResponseModel()
                {
                    result = false,
                    responseMessage = "Generic Exception : " + ex.Message
                };
            }
        }

        public async Task<Models.OWS.OwsResponseModel> UpdateGuestPassport(string reservationNameID, Models.OWS.OwsRequestModel owsRequest, string groupName, Models.Local.ServiceParameters serviceParameters)
        {
            try
            {
                new LogHelper().Debug("Updating passport info using web api", reservationNameID, "UpdateGuestPassport", serviceParameters.ClientID, groupName);
                HttpClient httpClient = serviceParameters.isProxyEnableForLocalAPI ? getProxyClient(groupName, serviceParameters.LocalAPIProxyHost, serviceParameters.LocalAPIProxyUN, serviceParameters.LocalAPIProxyPswd,serviceParameters.ClientID) : new HttpClient();
                if (httpClient == null)
                {
                    new LogHelper().Debug("Failled to update passport info using web api due to proxy error", reservationNameID, "UpdateGuestPassport", serviceParameters.ClientID, groupName);
                    return new Models.OWS.OwsResponseModel()
                    {
                        result = false,
                        responseMessage = "Failled to generate the proxy http client"
                    };
                }
                httpClient.DefaultRequestHeaders.Clear();
                string requestString = JsonConvert.SerializeObject(owsRequest, Formatting.None);
                new LogHelper().Debug("web api url :- " + serviceParameters.LocalAPIURL + @"/ows/UpdatePassport", reservationNameID, "UpdateGuestPassport", serviceParameters.ClientID, groupName);
                new LogHelper().Debug("web api request :- " + requestString, reservationNameID, "UpdateGuestPassport", serviceParameters.ClientID, groupName);
                var requestContent = new StringContent(requestString, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await httpClient.PostAsync(serviceParameters.LocalAPIURL + @"/ows/UpdatePassport", requestContent);
                if (response != null)
                {
                    if (response.IsSuccessStatusCode)
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        new LogHelper().Debug("web API response :- " + apiResponse, reservationNameID, "UpdateGuestPassport", serviceParameters.ClientID, groupName);
                        Models.OWS.OwsResponseModel owsResponse = JsonConvert.DeserializeObject<Models.OWS.OwsResponseModel>(apiResponse);
                        return owsResponse;
                    }
                    else
                    {
                        new LogHelper().Debug("Failled to update passport info using web api due to HTTP error : " + response.ReasonPhrase, reservationNameID, "UpdateGuestPassport", serviceParameters.ClientID, groupName);
                        return new Models.OWS.OwsResponseModel()
                        {
                            result = false,
                            responseMessage = response.ReasonPhrase
                        };
                    }
                }
                else
                {
                    new LogHelper().Debug("Failled to update passport info using web api due to null returned from the local web api", reservationNameID, "UpdateGuestPassport", serviceParameters.ClientID, groupName);
                    return new Models.OWS.OwsResponseModel()
                    {
                        result = false,
                        responseMessage = "Local web API returned null"
                    };
                }
            }
            catch (Exception ex)
            {
                new LogHelper().Error(ex, reservationNameID, "UpdateGuestPassport", serviceParameters.ClientID, groupName);
                return new Models.OWS.OwsResponseModel()
                {
                    result = false,
                    responseMessage = "Generic Exception : " + ex.Message
                };
            }
        }

        public async Task<Models.OWS.OwsResponseModel> UpdateProfileAddressAsync(string reservationNameID, Models.OWS.OwsRequestModel owsRequest, string groupName, Models.Local.ServiceParameters serviceParameters)
        {
            try
            {
                new LogHelper().Debug("Updating address info using web api", reservationNameID, "UpdateProfileAddressAsync", serviceParameters.ClientID, groupName);
                HttpClient httpClient = serviceParameters.isProxyEnableForLocalAPI ? getProxyClient(groupName, serviceParameters.LocalAPIProxyHost, serviceParameters.LocalAPIProxyUN, serviceParameters.LocalAPIProxyPswd,serviceParameters.ClientID) : new HttpClient();
                if (httpClient == null)
                {
                    new LogHelper().Debug("Failled to update address info using web api due to proxy error", reservationNameID, "UpdateProfileAddressAsync", serviceParameters.ClientID, groupName);
                    return new Models.OWS.OwsResponseModel()
                    {
                        result = false,
                        responseMessage = "Failled to generate the proxy http client"
                    };
                }
                httpClient.DefaultRequestHeaders.Clear();
                string requestString = JsonConvert.SerializeObject(owsRequest, Formatting.None);
                new LogHelper().Debug("web api url :- " + serviceParameters.LocalAPIURL + @"/ows/UpdateAddresList", reservationNameID, "UpdateProfileAddressAsync", serviceParameters.ClientID, groupName);
                new LogHelper().Debug("web api request :- " + requestString, reservationNameID, "UpdateProfileAddressAsync", serviceParameters.ClientID, groupName);
                var requestContent = new StringContent(requestString, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await httpClient.PostAsync(serviceParameters.LocalAPIURL + @"/ows/UpdateAddresList", requestContent);
                if (response != null)
                {
                    if (response.IsSuccessStatusCode)
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        new LogHelper().Debug("web API response :- " + apiResponse, reservationNameID, "UpdateProfileAddressAsync", serviceParameters.ClientID, groupName);
                        Models.OWS.OwsResponseModel owsResponse = JsonConvert.DeserializeObject<Models.OWS.OwsResponseModel>(apiResponse);
                        return owsResponse;
                    }
                    else
                    {
                        new LogHelper().Debug("Failled to update address info using web api due to HTTP error : " + response.ReasonPhrase, reservationNameID, "UpdateProfileAddressAsync", serviceParameters.ClientID, groupName);
                        return new Models.OWS.OwsResponseModel()
                        {
                            result = false,
                            responseMessage = response.ReasonPhrase
                        };
                    }
                }
                else
                {
                    new LogHelper().Debug("Failled to update address info using web api due to null returned from the local web api", reservationNameID, "UpdateProfileAddressAsync", serviceParameters.ClientID, groupName);
                    return new Models.OWS.OwsResponseModel()
                    {
                        result = false,
                        responseMessage = "Local web API returned null"
                    };
                }
            }
            catch (Exception ex)
            {
                new LogHelper().Error(ex, reservationNameID, "UpdateProfileAddressAsync", serviceParameters.ClientID, groupName);
                return new Models.OWS.OwsResponseModel()
                {
                    result = false,
                    responseMessage = "Generic Exception : " + ex.Message
                };
            }
        }

        public async Task<Models.OWS.OwsResponseModel> UpdateProfileEmailAsync(string reservationNameID, Models.OWS.OwsRequestModel owsRequest, string groupName, Models.Local.ServiceParameters serviceParameters)
        {
            try
            {
                new LogHelper().Debug("Updating Email info using web api", reservationNameID, "UpdateProfileEmailAsync", serviceParameters.ClientID, groupName);
                HttpClient httpClient = serviceParameters.isProxyEnableForLocalAPI ? getProxyClient(groupName, serviceParameters.LocalAPIProxyHost, serviceParameters.LocalAPIProxyUN, serviceParameters.LocalAPIProxyPswd,serviceParameters.ClientID) : new HttpClient();
                if (httpClient == null)
                {
                    new LogHelper().Debug("Failled to update email info using web api due to proxy error", reservationNameID, "UpdateProfileEmailAsync", serviceParameters.ClientID, groupName);
                    return new Models.OWS.OwsResponseModel()
                    {
                        result = false,
                        responseMessage = "Failled to generate the proxy http client"
                    };
                }
                httpClient.DefaultRequestHeaders.Clear();
                string requestString = JsonConvert.SerializeObject(owsRequest, Formatting.None);
                new LogHelper().Debug("web api url :- " + serviceParameters.LocalAPIURL + @"/ows/UpdateEmailList", reservationNameID, "UpdateProfileEmailAsync", serviceParameters.ClientID, groupName);
                new LogHelper().Debug("web api request :- " + requestString, reservationNameID, "UpdateProfileEmailAsync", serviceParameters.ClientID, groupName);
                var requestContent = new StringContent(requestString, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await httpClient.PostAsync(serviceParameters.LocalAPIURL + @"/ows/UpdateEmailList", requestContent);
                if (response != null)
                {
                    if (response.IsSuccessStatusCode)
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        new LogHelper().Debug("web API response :- " + apiResponse, reservationNameID, "UpdateProfileEmailAsync", serviceParameters.ClientID, groupName);
                        Models.OWS.OwsResponseModel owsResponse = JsonConvert.DeserializeObject<Models.OWS.OwsResponseModel>(apiResponse);
                        return owsResponse;
                    }
                    else
                    {
                        new LogHelper().Debug("Failled to update email info using web api due to HTTP error : " + response.ReasonPhrase, reservationNameID, "UpdateProfileEmailAsync", serviceParameters.ClientID, groupName);
                        return new Models.OWS.OwsResponseModel()
                        {
                            result = false,
                            responseMessage = response.ReasonPhrase
                        };
                    }
                }
                else
                {
                    new LogHelper().Debug("Failled to update email info using web api due to null returned from the local web api", reservationNameID, "UpdateProfileEmailAsync", serviceParameters.ClientID, groupName);
                    return new Models.OWS.OwsResponseModel()
                    {
                        result = false,
                        responseMessage = "Local web API returned null"
                    };
                }
            }
            catch (Exception ex)
            {
                new LogHelper().Error(ex, reservationNameID, "UpdateProfileEmailAsync", serviceParameters.ClientID, groupName);
                return new Models.OWS.OwsResponseModel()
                {
                    result = false,
                    responseMessage = "Generic Exception : " + ex.Message
                };
            }
        }

        public async Task<Models.OWS.OwsResponseModel> UpdateProfilePhoneAsync(string reservationNameID, Models.OWS.OwsRequestModel owsRequest, string groupName, Models.Local.ServiceParameters serviceParameters)
        {
            try
            {
                new LogHelper().Debug("Updating phone info using web api", reservationNameID, "UpdateProfilePhoneAsync", serviceParameters.ClientID, groupName);
                HttpClient httpClient = serviceParameters.isProxyEnableForLocalAPI ? getProxyClient(groupName, serviceParameters.LocalAPIProxyHost, serviceParameters.LocalAPIProxyUN, serviceParameters.LocalAPIProxyPswd,serviceParameters.ClientID) : new HttpClient();
                if (httpClient == null)
                {
                    new LogHelper().Debug("Failled to update phone info using web api due to proxy error", reservationNameID, "UpdateProfilePhoneAsync", serviceParameters.ClientID, groupName);
                    return new Models.OWS.OwsResponseModel()
                    {
                        result = false,
                        responseMessage = "Failled to generate the proxy http client"
                    };
                }
                httpClient.DefaultRequestHeaders.Clear();
                string requestString = JsonConvert.SerializeObject(owsRequest, Formatting.None);
                new LogHelper().Debug("web api url :- " + serviceParameters.LocalAPIURL + @"/ows/UpdatePhoneList", reservationNameID, "UpdateProfilePhoneAsync", serviceParameters.ClientID, groupName);
                new LogHelper().Debug("web api request :- " + requestString, reservationNameID, "UpdateProfilePhoneAsync", serviceParameters.ClientID, groupName);
                var requestContent = new StringContent(requestString, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await httpClient.PostAsync(serviceParameters.LocalAPIURL + @"/ows/UpdatePhoneList", requestContent);
                if (response != null)
                {
                    if (response.IsSuccessStatusCode)
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        new LogHelper().Debug("web API response :- " + apiResponse, reservationNameID, "UpdateProfilePhoneAsync", serviceParameters.ClientID, groupName);
                        Models.OWS.OwsResponseModel owsResponse = JsonConvert.DeserializeObject<Models.OWS.OwsResponseModel>(apiResponse);
                        return owsResponse;
                    }
                    else
                    {
                        new LogHelper().Debug("Failled to update phone info using web api due to HTTP error : " + response.ReasonPhrase, reservationNameID, "UpdateProfilePhoneAsync", serviceParameters.ClientID, groupName);
                        return new Models.OWS.OwsResponseModel()
                        {
                            result = false,
                            responseMessage = response.ReasonPhrase
                        };
                    }
                }
                else
                {
                    new LogHelper().Debug("Failled to update phone info using web api due to null returned from the local web api", reservationNameID, "UpdateProfilePhoneAsync", serviceParameters.ClientID, groupName);
                    return new Models.OWS.OwsResponseModel()
                    {
                        result = false,
                        responseMessage = "Local web API returned null"
                    };
                }
            }
            catch (Exception ex)
            {
                new LogHelper().Error(ex, reservationNameID, "UpdateProfilePhoneAsync", serviceParameters.ClientID, groupName);
                return new Models.OWS.OwsResponseModel()
                {
                    result = false,
                    responseMessage = "Generic Exception : " + ex.Message
                };
            }
        }

        public async Task<Models.OWS.OwsResponseModel> FetchReservationAsync(string reservationNameID, Models.OWS.OwsRequestModel owsRequest, string groupName, Models.Local.ServiceParameters serviceParameters)
        {
            try
            {
                new LogHelper().Debug("Fetching opera reservation using web api", reservationNameID, "FetchReservationAsync", serviceParameters.ClientID, groupName);
                HttpClient httpClient = serviceParameters.isProxyEnableForLocalAPI ? getProxyClient(groupName, serviceParameters.LocalAPIProxyHost, serviceParameters.LocalAPIProxyUN, serviceParameters.LocalAPIProxyPswd,serviceParameters.ClientID) : new HttpClient();
                if (httpClient == null)
                {
                    new LogHelper().Debug("Failled to fetch opera reservation using web api due to proxy error", reservationNameID, "FetchReservationAsync", serviceParameters.ClientID, groupName);
                    return new Models.OWS.OwsResponseModel()
                    {
                        result = false,
                        responseMessage = "Failled to generate the proxy http client"
                    };
                }
                httpClient.DefaultRequestHeaders.Clear();
                string requestString = JsonConvert.SerializeObject(owsRequest, Formatting.None);
                new LogHelper().Debug("web api url :- " + serviceParameters.LocalAPIURL + @"/ows/FetchReservation", reservationNameID, "FetchReservationAsync", serviceParameters.ClientID, groupName);
                new LogHelper().Debug("web api request :- " + requestString, reservationNameID, "FetchReservationAsync", serviceParameters.ClientID, groupName);
                var requestContent = new StringContent(requestString, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await httpClient.PostAsync(serviceParameters.LocalAPIURL + @"/ows/FetchReservation", requestContent);
                if (response != null)
                {
                    if (response.IsSuccessStatusCode)
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        new LogHelper().Debug("web API response :- " + apiResponse, reservationNameID, "FetchReservationAsync", serviceParameters.ClientID, groupName);
                        Models.OWS.OwsResponseModel owsResponse = JsonConvert.DeserializeObject<Models.OWS.OwsResponseModel>(apiResponse);
                        return owsResponse;
                    }
                    else
                    {
                        new LogHelper().Debug("Failled to fetch opera reservation using web api due to HTTP error : " + response.ReasonPhrase, reservationNameID, "FetchReservationAsync", serviceParameters.ClientID, groupName);
                        return new Models.OWS.OwsResponseModel()
                        {
                            result = false,
                            responseMessage = response.ReasonPhrase
                        };
                    }
                }
                else
                {
                    new LogHelper().Debug("Failled to fetch opera reservation using web api due to null returned from the local web api", reservationNameID, "FetchReservationAsync", serviceParameters.ClientID, groupName);
                    return new Models.OWS.OwsResponseModel()
                    {
                        result = false,
                        responseMessage = "Local web API returned null"
                    };
                }
            }
            catch (Exception ex)
            {
                new LogHelper().Error(ex, reservationNameID, "FetchReservationAsync", serviceParameters.ClientID, groupName);
                return new Models.OWS.OwsResponseModel()
                {
                    result = false,
                    responseMessage = "Generic Exception : " + ex.Message
                };
            }
        }

        public async Task<Models.OWS.OwsResponseModel> GetRegistrationCard(string reservationNameID, Models.OWS.OwsRequestModel owsRequest, string groupName, Models.Local.ServiceParameters serviceParameters)
        {
            try
            {
                new LogHelper().Debug("Generating regcard using web api", reservationNameID, "GetRegistrationCard", serviceParameters.ClientID, groupName);
                HttpClient httpClient = serviceParameters.isProxyEnableForCloudAPI ? getProxyClient(groupName, serviceParameters.CloudAPIProxyHost, serviceParameters.CloudAPIProxyUN, serviceParameters.CloudAPIProxyPswd,serviceParameters.ClientID) : new HttpClient();
                if (httpClient == null)
                {
                    new LogHelper().Debug("Failled to generate regcard using web api due to proxy error", reservationNameID, "GetRegistrationCard", serviceParameters.ClientID, groupName);
                    return new Models.OWS.OwsResponseModel()
                    {
                        result = false,
                        responseMessage = "Failled to generate the proxy http client"
                    };
                }
                httpClient.DefaultRequestHeaders.Clear();
                string requestString = JsonConvert.SerializeObject(owsRequest, Formatting.None);
                new LogHelper().Debug("web api url :- " + serviceParameters.CloudAPIURL + @"/ows/GetRegCardAsBase64", reservationNameID, "GetRegistrationCard", serviceParameters.ClientID, groupName);
                new LogHelper().Debug("web api request :- " + requestString, reservationNameID, "GetRegistrationCard", serviceParameters.ClientID, groupName);
                var requestContent = new StringContent(requestString, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await httpClient.PostAsync(serviceParameters.CloudAPIURL + @"/ows/GetRegCardAsBase64", requestContent);
                if (response != null)
                {
                    if (response.IsSuccessStatusCode)
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        new LogHelper().Debug("web API response :- " + apiResponse, reservationNameID, "GetRegistrationCard", serviceParameters.ClientID, groupName);
                        Models.OWS.OwsResponseModel owsResponse = JsonConvert.DeserializeObject<Models.OWS.OwsResponseModel>(apiResponse);
                        return owsResponse;
                    }
                    else
                    {
                        new LogHelper().Debug("Failled to generate regcard using web api due to HTTP error : " + response.ReasonPhrase, reservationNameID, "GetRegistrationCard", serviceParameters.ClientID, groupName);
                        return new Models.OWS.OwsResponseModel()
                        {
                            result = false,
                            responseMessage = response.ReasonPhrase
                        };
                    }
                }
                else
                {
                    new LogHelper().Debug("Failled to generate regcard using web api due to null returned from the local web api", reservationNameID, "GetRegistrationCard", serviceParameters.ClientID, groupName);
                    return new Models.OWS.OwsResponseModel()
                    {
                        result = false,
                        responseMessage = "Local web API returned null"
                    };
                }
            }
            catch (Exception ex)
            {
                new LogHelper().Error(ex, reservationNameID, "GetRegistrationCard", serviceParameters.ClientID, groupName);
                return new Models.OWS.OwsResponseModel()
                {
                    result = false,
                    responseMessage = "Generic Exception : " + ex.Message
                };
            }
        }

        public async Task<Models.Local.LocalResponseModel> InsertReservationDocuments(string reservationNameID, Models.Local.LocalRequestModel localRequest, string groupName, Models.Local.ServiceParameters serviceParameters)
        {
            try
            {
                new LogHelper().Debug("Pushing reservation document using web api", reservationNameID, "InsertReservationDocuments", serviceParameters.ClientID, groupName);
                HttpClient httpClient = serviceParameters.isProxyEnableForLocalAPI ? getProxyClient(groupName, serviceParameters.LocalAPIProxyHost, serviceParameters.LocalAPIProxyUN, serviceParameters.LocalAPIProxyPswd,serviceParameters.ClientID) : new HttpClient();
                if (httpClient == null)
                {
                    new LogHelper().Debug("Failled to push reservation document using web api due to proxy error", reservationNameID, "InsertReservationDocuments", serviceParameters.ClientID, groupName);
                    return new Models.Local.LocalResponseModel()
                    {
                        result = false,
                        responseMessage = "Failled to generate the proxy http client"
                    };
                }
                httpClient.DefaultRequestHeaders.Clear();
                string requestString = JsonConvert.SerializeObject(localRequest, Formatting.None);
                new LogHelper().Debug("web api url :- " + serviceParameters.LocalAPIURL + @"/local/PushReservationDocumentDetails", reservationNameID, "InsertReservationDocuments", serviceParameters.ClientID, groupName);
                new LogHelper().Debug("web api request :- " + requestString, reservationNameID, "InsertReservationDocuments", serviceParameters.ClientID, groupName);
                var requestContent = new StringContent(requestString, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await httpClient.PostAsync(serviceParameters.LocalAPIURL + @"/local/PushReservationDocumentDetails", requestContent);
                if (response != null)
                {
                    if (response.IsSuccessStatusCode)
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        new LogHelper().Debug("web API response :- " + apiResponse, reservationNameID, "InsertReservationDocuments", serviceParameters.ClientID, groupName);
                        Models.Local.LocalResponseModel localResponse = JsonConvert.DeserializeObject<Models.Local.LocalResponseModel>(apiResponse);
                        return localResponse;
                    }
                    else
                    {
                        new LogHelper().Debug("Failled to push reservation documents using web api due to HTTP error : " + response.ReasonPhrase, reservationNameID, "InsertReservationDocuments", serviceParameters.ClientID, groupName);
                        return new Models.Local.LocalResponseModel()
                        {
                            result = false,
                            responseMessage = response.ReasonPhrase
                        };
                    }
                }
                else
                {
                    new LogHelper().Debug("Failled to push reservation documents using web api due to null returned from the local web api", reservationNameID, "InsertReservationDocuments", serviceParameters.ClientID, groupName);
                    return new Models.Local.LocalResponseModel()
                    {
                        result = false,
                        responseMessage = "Local web API returned null"
                    };
                }
            }
            catch (Exception ex)
            {
                new LogHelper().Error(ex, reservationNameID, "InsertReservationDocuments", serviceParameters.ClientID, groupName);
                return new Models.Local.LocalResponseModel()
                {
                    result = false,
                    responseMessage = "Generic Exception : " + ex.Message
                };
            }
        }

        public async Task<Models.Cloud.CloudResponseModel> FetchUpsellPackages(string reservationNameID, Models.Cloud.CloudRequestModel cloudRequest, string groupName, Models.Local.ServiceParameters serviceParameters)
        {
            try
            {
                new LogHelper().Debug("Fetching upsell details using web api", reservationNameID, "FetchUpsellPackages", serviceParameters.ClientID, groupName);
                ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
                {
                    return true;
                };
                HttpClient httpClient = serviceParameters.isProxyEnableForCloudAPI ? getProxyClient(groupName, serviceParameters.CloudAPIProxyHost, serviceParameters.CloudAPIProxyUN, serviceParameters.CloudAPIProxyPswd,serviceParameters.ClientID) : new HttpClient();
                if (httpClient == null)
                {
                    new LogHelper().Debug("Failled to fetch upsell details using web api due to proxy error", reservationNameID, "FetchUpsellPackages", serviceParameters.ClientID, groupName);
                    return new Models.Cloud.CloudResponseModel()
                    {
                        result = false,
                        responseMessage = "Failled to generate the proxy http client"
                    };
                }
                httpClient.DefaultRequestHeaders.Clear();
                string requestString = JsonConvert.SerializeObject(cloudRequest, Formatting.None);
                new LogHelper().Debug("web api url :- " + serviceParameters.CloudAPIURL + @"/cloud/FetchUpsellPackages", reservationNameID, "FetchUpsellPackages", serviceParameters.ClientID, groupName);
                new LogHelper().Debug("web api request :- " + requestString, reservationNameID, "FetchUpsellPackages", serviceParameters.ClientID, groupName);
                var requestContent = new StringContent(requestString, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await httpClient.PostAsync(serviceParameters.CloudAPIURL + @"/cloud/FetchUpsellPackages", requestContent);
                if (response != null)
                {
                    if (response.IsSuccessStatusCode)
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        new LogHelper().Debug("web API response :- " + apiResponse, reservationNameID, "FetchUpsellPackages", serviceParameters.ClientID, groupName);
                        new LogHelper().Log("web API response :- " + apiResponse, reservationNameID, "FetchUpsellPackages", serviceParameters.ClientID, groupName);
                        Models.Cloud.CloudResponseModel cloudResponse = JsonConvert.DeserializeObject<Models.Cloud.CloudResponseModel>(apiResponse);
                        return cloudResponse;
                    }
                    else
                    {
                        new LogHelper().Debug("Failled to fetch upsell details using web api due to HTTP error : " + response.ReasonPhrase, reservationNameID, "FetchUpsellPackages", serviceParameters.ClientID, groupName);
                        return new Models.Cloud.CloudResponseModel()
                        {
                            result = false,
                            responseMessage = response.ReasonPhrase
                        };
                    }
                }
                else
                {
                    new LogHelper().Debug("Failled to fetch upsell details using web api due to null returned from the local web api", reservationNameID, "FetchUpsellPackages", serviceParameters.ClientID, groupName);
                    return new Models.Cloud.CloudResponseModel()
                    {
                        result = false,
                        responseMessage = "Cloud web API returned null"
                    };
                }
            }
            catch (Exception ex)
            {
                new LogHelper().Error(ex, reservationNameID, "FetchUpsellPackages", serviceParameters.ClientID, groupName);
                return new Models.Cloud.CloudResponseModel()
                {
                    result = false,
                    responseMessage = "Generic Exception : " + ex.Message
                };
            }
        }

        public async Task<Models.Cloud.CloudResponseModel> FetchReservationPolicies(string reservationNameID, Models.Cloud.CloudRequestModel cloudRequest, string groupName, Models.Local.ServiceParameters serviceParameters)
        {
            try
            {
                new LogHelper().Debug("Fetching reservation policies using web api", reservationNameID, "FetchReservationPolicies", serviceParameters.ClientID, groupName);
                ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
                {
                    return true;
                };
                HttpClient httpClient = serviceParameters.isProxyEnableForCloudAPI ? getProxyClient(groupName, serviceParameters.CloudAPIProxyHost, serviceParameters.CloudAPIProxyUN, serviceParameters.CloudAPIProxyPswd, serviceParameters.ClientID) : new HttpClient();
                if (httpClient == null)
                {
                    new LogHelper().Debug("Failled to fetch reservation policies using web api due to proxy error", reservationNameID, "FetchReservationPolicies", serviceParameters.ClientID, groupName);
                    return new Models.Cloud.CloudResponseModel()
                    {
                        result = false,
                        responseMessage = "Failled to generate the proxy http client"
                    };
                }
                httpClient.DefaultRequestHeaders.Clear();
                string requestString = JsonConvert.SerializeObject(cloudRequest, Formatting.None);
                new LogHelper().Debug("web api url :- " + serviceParameters.CloudAPIURL + @"/cloud/FetchPreCheckedinPolicyDetailsByReservationNumber", reservationNameID, "FetchReservationPolicies", serviceParameters.ClientID, groupName);
                new LogHelper().Debug("web api request :- " + requestString, reservationNameID, "FetchReservationPolicies", serviceParameters.ClientID, groupName);
                var requestContent = new StringContent(requestString, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await httpClient.PostAsync(serviceParameters.CloudAPIURL + @"/cloud/FetchPreCheckedinPolicyDetailsByReservationNumber", requestContent);
                if (response != null)
                {
                    if (response.IsSuccessStatusCode)
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        new LogHelper().Debug("web API response :- " + apiResponse, reservationNameID, "FetchReservationPolicies", serviceParameters.ClientID, groupName);
                        new LogHelper().Log("web API response :- " + apiResponse, reservationNameID, "FetchReservationPolicies", serviceParameters.ClientID, groupName);
                        Models.Cloud.CloudResponseModel cloudResponse = JsonConvert.DeserializeObject<Models.Cloud.CloudResponseModel>(apiResponse);
                        return cloudResponse;
                    }
                    else
                    {
                        new LogHelper().Debug("Failled to fetch upsell details policies using web api due to HTTP error : " + response.ReasonPhrase, reservationNameID, "FetchReservationPolicies", serviceParameters.ClientID, groupName);
                        return new Models.Cloud.CloudResponseModel()
                        {
                            result = false,
                            responseMessage = response.ReasonPhrase
                        };
                    }
                }
                else
                {
                    new LogHelper().Debug("Failled to fetch upsell details policies using web api due to null returned from the local web api", reservationNameID, "FetchReservationPolicies", serviceParameters.ClientID, groupName);
                    return new Models.Cloud.CloudResponseModel()
                    {
                        result = false,
                        responseMessage = "Cloud web API returned null"
                    };
                }
            }
            catch (Exception ex)
            {
                new LogHelper().Error(ex, reservationNameID, "FetchReservationPolicies", serviceParameters.ClientID, groupName);
                return new Models.Cloud.CloudResponseModel()
                {
                    result = false,
                    responseMessage = "Generic Exception : " + ex.Message
                };
            }
        }

        public async Task<Models.Cloud.CloudResponseModel> FetchFeedback(string reservationNameID, Models.Cloud.CloudRequestModel cloudRequest, string groupName, Models.Local.ServiceParameters serviceParameters)
        {
            try
            {
                new LogHelper().Debug("Fetching feedback using web api", reservationNameID, "FetchFeedback", serviceParameters.ClientID, groupName);
                ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
                {
                    return true;
                };
                HttpClient httpClient = serviceParameters.isProxyEnableForCloudAPI ? getProxyClient(groupName, serviceParameters.CloudAPIProxyHost, serviceParameters.CloudAPIProxyUN, serviceParameters.CloudAPIProxyPswd,serviceParameters.ClientID) : new HttpClient();
                if (httpClient == null)
                {
                    new LogHelper().Debug("Failled to fetch feedback using web api due to proxy error", reservationNameID, "FetchFeedback", serviceParameters.ClientID, groupName);
                    return new Models.Cloud.CloudResponseModel()
                    {
                        result = false,
                        responseMessage = "Failled to generate the proxy http client"
                    };
                }
                httpClient.DefaultRequestHeaders.Clear();
                string requestString = JsonConvert.SerializeObject(cloudRequest, Formatting.None);
                new LogHelper().Debug("web api url :- " + serviceParameters.CloudAPIURL + @"/cloud/FetchReservationFeedBack", reservationNameID, "FetchFeedback", serviceParameters.ClientID, groupName);
                new LogHelper().Debug("web api request :- " + requestString, reservationNameID, "FetchFeedback", serviceParameters.ClientID, groupName);
                var requestContent = new StringContent(requestString, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await httpClient.PostAsync(serviceParameters.CloudAPIURL + @"/cloud/FetchReservationFeedBack", requestContent);
                if (response != null)
                {
                    if (response.IsSuccessStatusCode)
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        new LogHelper().Debug("web API response :- " + apiResponse, reservationNameID, "FetchFeedback", serviceParameters.ClientID, groupName);
                        new LogHelper().Log("web API response :- " + apiResponse, reservationNameID, "FetchFeedback", serviceParameters.ClientID, groupName);
                        Models.Cloud.CloudResponseModel cloudResponse = JsonConvert.DeserializeObject<Models.Cloud.CloudResponseModel>(apiResponse);
                        return cloudResponse;
                    }
                    else
                    {
                        new LogHelper().Debug("Failled to fetch feedback using web api due to HTTP error : " + response.ReasonPhrase, reservationNameID, "FetchFeedback", serviceParameters.ClientID, groupName);
                        return new Models.Cloud.CloudResponseModel()
                        {
                            result = false,
                            responseMessage = response.ReasonPhrase
                        };
                    }
                }
                else
                {
                    new LogHelper().Debug("Failled to fetch feedback using web api due to null returned from the local web api", reservationNameID, "FetchFeedback", serviceParameters.ClientID, groupName);
                    return new Models.Cloud.CloudResponseModel()
                    {
                        result = false,
                        responseMessage = "Cloud web API returned null"
                    };
                }
            }
            catch (Exception ex)
            {
                new LogHelper().Error(ex, reservationNameID, "FetchFeedback", serviceParameters.ClientID, groupName);
                return new Models.Cloud.CloudResponseModel()
                {
                    result = false,
                    responseMessage = "Generic Exception : " + ex.Message
                };
            }
        }

        public async Task<Models.Cloud.CloudResponseModel> FetchPaymentDetails(string reservationNameID, Models.Cloud.CloudRequestModel cloudRequest, string groupName, Models.Local.ServiceParameters serviceParameters)
        {
            try
            {
                new LogHelper().Debug("Fetching payment details using web api", reservationNameID, "FetchPaymentDetails", serviceParameters.ClientID, groupName);
                ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
                {
                    return true;
                };
                HttpClient httpClient = serviceParameters.isProxyEnableForCloudAPI ? getProxyClient(groupName, serviceParameters.CloudAPIProxyHost, serviceParameters.CloudAPIProxyUN, serviceParameters.CloudAPIProxyPswd,serviceParameters.ClientID) : new HttpClient();
                if (httpClient == null)
                {
                    new LogHelper().Debug("Failled to fetch payment details using web api due to proxy error", reservationNameID, "FetchPaymentDetails", serviceParameters.ClientID, groupName);
                    return new Models.Cloud.CloudResponseModel()
                    {
                        result = false,
                        responseMessage = "Failled to generate the proxy http client"
                    };
                }
                httpClient.DefaultRequestHeaders.Clear();
                string requestString = JsonConvert.SerializeObject(cloudRequest, Formatting.None);
                new LogHelper().Debug("web api url :- " + serviceParameters.CloudAPIURL + @"/cloud/FetchPaymentDetails", reservationNameID, "FetchPaymentDetails", serviceParameters.ClientID, groupName);
                new LogHelper().Debug("web api request :- " + requestString, reservationNameID, "FetchPaymentDetails", serviceParameters.ClientID, groupName);
                var requestContent = new StringContent(requestString, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await httpClient.PostAsync(serviceParameters.CloudAPIURL + @"/cloud/FetchPaymentDetails", requestContent);
                if (response != null)
                {
                    if (response.IsSuccessStatusCode)
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        new LogHelper().Debug("web API response :- " + apiResponse, reservationNameID, "FetchPaymentDetails", serviceParameters.ClientID, groupName);
                        new LogHelper().Log("web API response :- " + apiResponse, reservationNameID, "FetchPaymentDetails", serviceParameters.ClientID, groupName);
                        Models.Cloud.CloudResponseModel cloudResponse = JsonConvert.DeserializeObject<Models.Cloud.CloudResponseModel>(apiResponse);
                        return cloudResponse;
                    }
                    else
                    {
                        new LogHelper().Debug("Failled to fetch payment details using web api due to HTTP error : " + response.ReasonPhrase, reservationNameID, "FetchPaymentDetails", serviceParameters.ClientID, groupName);
                        return new Models.Cloud.CloudResponseModel()
                        {
                            result = false,
                            responseMessage = response.ReasonPhrase
                        };
                    }
                }
                else
                {
                    new LogHelper().Debug("Failled to fetch payment details using web api due to null returned from the local web api", reservationNameID, "FetchPaymentDetails", serviceParameters.ClientID, groupName);
                    return new Models.Cloud.CloudResponseModel()
                    {
                        result = false,
                        responseMessage = "Cloud web API returned null"
                    };
                }
            }
            catch (Exception ex)
            {
                new LogHelper().Error(ex, reservationNameID, "FetchPaymentDetails", serviceParameters.ClientID, groupName);
                return new Models.Cloud.CloudResponseModel()
                {
                    result = false,
                    responseMessage = "Generic Exception : " + ex.Message
                };
            }
        }

        public async Task<Models.Cloud.CloudResponseModel> PushDueoutRecord(string reservationNameID, Models.Cloud.CloudRequestModel cloudRequest, string groupName, Models.Local.ServiceParameters serviceParameters)
        {
            try
            {
                new LogHelper().Debug("Pushing due out record", reservationNameID, "PushDueoutRecord", serviceParameters.ClientID, groupName);
                ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
                {
                    return true;
                };
                HttpClient httpClient = serviceParameters.isProxyEnableForCloudAPI ? getProxyClient(groupName, serviceParameters.CloudAPIProxyHost, serviceParameters.CloudAPIProxyUN, serviceParameters.CloudAPIProxyPswd,serviceParameters.ClientID) : new HttpClient();
                if (httpClient == null)
                {
                    new LogHelper().Debug("Failled to push due out record using web api due to proxy error", reservationNameID, "PushDueoutRecord", serviceParameters.ClientID, groupName);
                    return new Models.Cloud.CloudResponseModel()
                    {
                        result = false,
                        responseMessage = "Failled to generate the proxy http client"
                    };
                }
                httpClient.DefaultRequestHeaders.Clear();
                string requestString = JsonConvert.SerializeObject(cloudRequest, Formatting.None);
                new LogHelper().Debug("web api url :- " + serviceParameters.CloudAPIURL + @"/cloud/PushDueOutReservationDetails", reservationNameID, "PushDueoutRecord", serviceParameters.ClientID, groupName);
                new LogHelper().Debug("web api request :- " + requestString, reservationNameID, "PushDueoutRecord", serviceParameters.ClientID, groupName);
                var requestContent = new StringContent(requestString, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await httpClient.PostAsync(serviceParameters.CloudAPIURL + @"/cloud/PushDueOutReservationDetails", requestContent);
                if (response != null)
                {
                    if (response.IsSuccessStatusCode)
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        new LogHelper().Debug("web API response :- " + apiResponse, reservationNameID, "PushDueoutRecord", serviceParameters.ClientID, groupName);
                        Models.Cloud.CloudResponseModel cloudResponse = JsonConvert.DeserializeObject<Models.Cloud.CloudResponseModel>(apiResponse);
                        return cloudResponse;
                    }
                    else
                    {
                        new LogHelper().Debug("Failled to push due out reservations using web api due to HTTP error : " + response.ReasonPhrase, reservationNameID, "PushDueoutRecord", serviceParameters.ClientID, groupName);
                        return new Models.Cloud.CloudResponseModel()
                        {
                            result = false,
                            responseMessage = response.ReasonPhrase
                        };
                    }
                }
                else
                {
                    new LogHelper().Debug("Failled to push due out reservations using web api due to null returned from the local web api", reservationNameID, "PushDueoutRecord", serviceParameters.ClientID, groupName);
                    return new Models.Cloud.CloudResponseModel()
                    {
                        result = false,
                        responseMessage = "Cloud web API returned null"
                    };
                }
            }
            catch (Exception ex)
            {
                new LogHelper().Error(ex, reservationNameID, "PushDueoutRecord", serviceParameters.ClientID, groupName);
                return new Models.Cloud.CloudResponseModel()
                {
                    result = false,
                    responseMessage = "Generic Exception : " + ex.Message
                };
            }
        }

        public async Task<Models.Local.LocalResponseModel> PushReservationAdditionalDetails(string reservationNameID, Models.Local.LocalRequestModel localRequest, string groupName, Models.Local.ServiceParameters serviceParameters)
        {
            try
            {
                new LogHelper().Debug("Pushing reservation additional details using web api", reservationNameID, "PushReservationAdditionalDetails", serviceParameters.ClientID, groupName);
                HttpClient httpClient = serviceParameters.isProxyEnableForLocalAPI ? getProxyClient(groupName, serviceParameters.LocalAPIProxyHost, serviceParameters.LocalAPIProxyUN, serviceParameters.LocalAPIProxyPswd,serviceParameters.ClientID) : new HttpClient();
                if (httpClient == null)
                {
                    new LogHelper().Debug("Failled to push reservation additional details using web api due to proxy error", reservationNameID, "PushReservationAdditionalDetails", serviceParameters.ClientID, groupName);
                    return new Models.Local.LocalResponseModel()
                    {
                        result = false,
                        responseMessage = "Failled to generate the proxy http client"
                    };
                }
                httpClient.DefaultRequestHeaders.Clear();
                string requestString = JsonConvert.SerializeObject(localRequest, Formatting.None);
                new LogHelper().Debug("web api url :- " + serviceParameters.LocalAPIURL + @"/local/PushReservationAdditionalDetails", reservationNameID, "PushReservationAdditionalDetails", serviceParameters.ClientID, groupName);
                new LogHelper().Debug("web api request :- " + requestString, reservationNameID, "PushReservationAdditionalDetails", "Grabber", groupName);
                var requestContent = new StringContent(requestString, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await httpClient.PostAsync(serviceParameters.LocalAPIURL + @"/local/PushReservationAdditionalDetails", requestContent);
                if (response != null)
                {
                    if (response.IsSuccessStatusCode)
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        new LogHelper().Debug("web API response :- " + apiResponse, reservationNameID, "PushReservationAdditionalDetails", serviceParameters.ClientID, groupName);
                        Models.Local.LocalResponseModel localResponse = JsonConvert.DeserializeObject<Models.Local.LocalResponseModel>(apiResponse);
                        return localResponse;
                    }
                    else
                    {
                        new LogHelper().Debug("Failled to push reservation additional details using web api due to HTTP error : " + response.ReasonPhrase, reservationNameID, "PushReservationAdditionalDetails", serviceParameters.ClientID, groupName);
                        return new Models.Local.LocalResponseModel()
                        {
                            result = false,
                            responseMessage = response.ReasonPhrase
                        };
                    }
                }
                else
                {
                    new LogHelper().Debug("Failled to push reservation additional details using web api due to null returned from the local web api", reservationNameID, "PushReservationAdditionalDetails", serviceParameters.ClientID, groupName);
                    return new Models.Local.LocalResponseModel()
                    {
                        result = false,
                        responseMessage = "Local web API returned null"
                    };
                }
            }
            catch (Exception ex)
            {
                new LogHelper().Error(ex, reservationNameID, "PushReservationPolicies", serviceParameters.ClientID, groupName);
                return new Models.Local.LocalResponseModel()
                {
                    result = false,
                    responseMessage = "Generic Exception : " + ex.Message
                };
            }
        }

        public async Task<Models.Cloud.CloudResponseModel> FetchReservationAdditionalDetails(string reservationNameID, string cloudRequest, string groupName, Models.Local.ServiceParameters serviceParameters)
        {
            try
            {
                new LogHelper().Debug("Fetching reservation additional details using web api", reservationNameID, "FetchReservationAdditionalDetails", "Grabber", groupName);
                ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
                {
                    return true;
                };
                HttpClient httpClient = serviceParameters.isProxyEnableForCloudAPI ? getProxyClient(groupName, serviceParameters.CloudAPIProxyHost, serviceParameters.CloudAPIProxyUN, serviceParameters.CloudAPIProxyPswd, serviceParameters.ClientID) : new HttpClient();
                if (httpClient == null)
                {
                    new LogHelper().Debug("Failled to fetch reservation additional details using web api due to proxy error", reservationNameID, "FetchReservationAdditionalDetails", "Grabber", groupName);
                    return new Models.Cloud.CloudResponseModel()
                    {
                        result = false,
                        responseMessage = "Failled to generate the proxy http client"
                    };
                }
                httpClient.DefaultRequestHeaders.Clear();
                string requestString = cloudRequest;
                new LogHelper().Debug("web api url :- " + serviceParameters.CloudAPIURL + @"/cloud/FetchReservationAdditionalDetails", reservationNameID, "FetchReservationAdditionalDetails", "Grabber", groupName);
                new LogHelper().Debug("web api request :- " + requestString, reservationNameID, "FetchReservationAdditionalDetails", "Grabber", groupName);
                var requestContent = new StringContent(requestString, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await httpClient.PostAsync(serviceParameters.CloudAPIURL + @"/cloud/FetchReservationAdditionalDetails", requestContent);
                if (response != null)
                {
                    if (response.IsSuccessStatusCode)
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        new LogHelper().Debug("web API response :- " + apiResponse, reservationNameID, "FetchReservationAdditionalDetails", "Grabber", groupName);
                        new LogHelper().Debug("web API response :- " + apiResponse, reservationNameID, "FetchReservationAdditionalDetails", "Grabber", groupName);
                        Models.Cloud.CloudResponseModel cloudResponse = JsonConvert.DeserializeObject<Models.Cloud.CloudResponseModel>(apiResponse);
                        return cloudResponse;
                    }
                    else
                    {
                        new LogHelper().Debug("Failled to fetch reservation additional details policies using web api due to HTTP error : " + response.ReasonPhrase, reservationNameID, "FetchReservationAdditionalDetails", "Grabber", groupName);
                        return new Models.Cloud.CloudResponseModel()
                        {
                            result = false,
                            responseMessage = response.ReasonPhrase
                        };
                    }
                }
                else
                {
                    new LogHelper().Debug("Failled to fetch reservation additional details policies using web api due to null returned from the local web api", reservationNameID, "FetchReservationAdditionalDetails", "Grabber", groupName);
                    return new Models.Cloud.CloudResponseModel()
                    {
                        result = false,
                        responseMessage = "Cloud web API returned null"
                    };
                }
            }
            catch (Exception ex)
            {
                new LogHelper().Error(ex, reservationNameID, "FetchReservationAdditionalDetails", "Grabber", groupName);
                return new Models.Cloud.CloudResponseModel()
                {
                    result = false,
                    responseMessage = "Generic Exception : " + ex.Message
                };
            }
        }
        public async Task<Models.Local.LocalResponseModel> PushFeedback(string reservationNameID, Models.Local.LocalRequestModel localRequest, string groupName, Models.Local.ServiceParameters serviceParameters)
        {
            try
            {
                new LogHelper().Debug("Pushing feedback using web api", reservationNameID, "PushFeedback", serviceParameters.ClientID, groupName);
                HttpClient httpClient = serviceParameters.isProxyEnableForLocalAPI ? getProxyClient(groupName, serviceParameters.LocalAPIProxyHost, serviceParameters.LocalAPIProxyUN, serviceParameters.LocalAPIProxyPswd,serviceParameters.ClientID) : new HttpClient();
                if (httpClient == null)
                {
                    new LogHelper().Debug("Failled to push feedback using web api due to proxy error", reservationNameID, "PushFeedback", serviceParameters.ClientID, groupName);
                    return new Models.Local.LocalResponseModel()
                    {
                        result = false,
                        responseMessage = "Failled to generate the proxy http client"
                    };
                }
                httpClient.DefaultRequestHeaders.Clear();
                string requestString = JsonConvert.SerializeObject(localRequest, Formatting.None);
                new LogHelper().Debug("web api url :- " + serviceParameters.LocalAPIURL + @"/local/PushFeedback", reservationNameID, "PushFeedback", serviceParameters.ClientID, groupName);
                new LogHelper().Debug("web api request :- " + requestString, reservationNameID, "PushFeedback", serviceParameters.ClientID, groupName);
                var requestContent = new StringContent(requestString, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await httpClient.PostAsync(serviceParameters.LocalAPIURL + @"/local/PushFeedback", requestContent);
                if (response != null)
                {
                    if (response.IsSuccessStatusCode)
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        new LogHelper().Debug("web API response :- " + apiResponse, reservationNameID, "PushFeedback", serviceParameters.ClientID, groupName);
                        Models.Local.LocalResponseModel localResponse = JsonConvert.DeserializeObject<Models.Local.LocalResponseModel>(apiResponse);
                        return localResponse;
                    }
                    else
                    {
                        new LogHelper().Debug("Failled to push feedback using web api due to HTTP error : " + response.ReasonPhrase, reservationNameID, "PushFeedback", serviceParameters.ClientID, groupName);
                        return new Models.Local.LocalResponseModel()
                        {
                            result = false,
                            responseMessage = response.ReasonPhrase
                        };
                    }
                }
                else
                {
                    new LogHelper().Debug("Failled to push feedback using web api due to null returned from the local web api", reservationNameID, "PushFeedback", serviceParameters.ClientID, groupName);
                    return new Models.Local.LocalResponseModel()
                    {
                        result = false,
                        responseMessage = "Local web API returned null"
                    };
                }
            }
            catch (Exception ex)
            {
                new LogHelper().Error(ex, reservationNameID, "PushFeedback", serviceParameters.ClientID, groupName);
                return new Models.Local.LocalResponseModel()
                {
                    result = false,
                    responseMessage = "Generic Exception : " + ex.Message
                };
            }
        }

        public async Task<Models.Local.LocalResponseModel> PushPaymentDetails(string reservationNameID, Models.Local.LocalRequestModel localRequest, string groupName, Models.Local.ServiceParameters serviceParameters)
        {
            try
            {
                new LogHelper().Debug("Pushing payment details using web api", reservationNameID, "PushPaymentDetails", serviceParameters.ClientID, groupName);
                HttpClient httpClient = serviceParameters.isProxyEnableForLocalAPI ? getProxyClient(groupName, serviceParameters.LocalAPIProxyHost, serviceParameters.LocalAPIProxyUN, serviceParameters.LocalAPIProxyPswd,serviceParameters.ClientID) : new HttpClient();
                if (httpClient == null)
                {
                    new LogHelper().Debug("Failled to push payment details using web api due to proxy error", reservationNameID, "PushPaymentDetails", serviceParameters.ClientID, groupName);
                    return new Models.Local.LocalResponseModel()
                    {
                        result = false,
                        responseMessage = "Failled to generate the proxy http client"
                    };
                }
                httpClient.DefaultRequestHeaders.Clear();
                string requestString = JsonConvert.SerializeObject(localRequest, Formatting.None);
                new LogHelper().Debug("web api url :- " + serviceParameters.LocalAPIURL + @"/local/PushPaymentDetails", reservationNameID, "PushPaymentDetails", serviceParameters.ClientID, groupName);
                new LogHelper().Debug("web api request :- " + requestString, reservationNameID, "PushPaymentDetails", serviceParameters.ClientID, groupName);
                var requestContent = new StringContent(requestString, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await httpClient.PostAsync(serviceParameters.LocalAPIURL + @"/local/PushPaymentDetails", requestContent);
                if (response != null)
                {
                    if (response.IsSuccessStatusCode)
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        new LogHelper().Debug("web API response :- " + apiResponse, reservationNameID, "PushPaymentDetails", serviceParameters.ClientID, groupName);
                        Models.Local.LocalResponseModel localResponse = JsonConvert.DeserializeObject<Models.Local.LocalResponseModel>(apiResponse);
                        return localResponse;
                    }
                    else
                    {
                        new LogHelper().Debug("Failled to push payment details using web api due to HTTP error : " + response.ReasonPhrase, reservationNameID, "PushPaymentDetails", serviceParameters.ClientID, groupName);
                        return new Models.Local.LocalResponseModel()
                        {
                            result = false,
                            responseMessage = response.ReasonPhrase
                        };
                    }
                }
                else
                {
                    new LogHelper().Debug("Failled to push payment details using web api due to null returned from the local web api", reservationNameID, "PushPaymentDetails", serviceParameters.ClientID, groupName);
                    return new Models.Local.LocalResponseModel()
                    {
                        result = false,
                        responseMessage = "Local web API returned null"
                    };
                }
            }
            catch (Exception ex)
            {
                new LogHelper().Error(ex, reservationNameID, "PushPaymentDetails", serviceParameters.ClientID, groupName);
                return new Models.Local.LocalResponseModel()
                {
                    result = false,
                    responseMessage = "Generic Exception : " + ex.Message
                };
            }
        }

        public async Task<Models.Local.LocalResponseModel> PushUpsellpackages(string reservationNameID, Models.Local.LocalRequestModel localRequest, string groupName, Models.Local.ServiceParameters serviceParameters)
        {
            try
            {
                new LogHelper().Debug("Pushing upsell details using web api", reservationNameID, "PushUpsellpackages", serviceParameters.ClientID, groupName);
                HttpClient httpClient = serviceParameters.isProxyEnableForLocalAPI ? getProxyClient(groupName, serviceParameters.LocalAPIProxyHost, serviceParameters.LocalAPIProxyUN, serviceParameters.LocalAPIProxyPswd,serviceParameters.ClientID) : new HttpClient();
                if (httpClient == null)
                {
                    new LogHelper().Debug("Failled to push upsell details using web api due to proxy error", reservationNameID, "PushUpsellpackages", serviceParameters.ClientID, groupName);
                    return new Models.Local.LocalResponseModel()
                    {
                        result = false,
                        responseMessage = "Failled to generate the proxy http client"
                    };
                }
                httpClient.DefaultRequestHeaders.Clear();
                string requestString = JsonConvert.SerializeObject(localRequest, Formatting.None);
                new LogHelper().Debug("web api url :- " + serviceParameters.LocalAPIURL + @"/local/PushUpsellPackages", reservationNameID, "PushUpsellpackages", serviceParameters.ClientID, groupName);
                new LogHelper().Debug("web api request :- " + requestString, reservationNameID, "PushUpsellpackages", serviceParameters.ClientID, groupName);
                var requestContent = new StringContent(requestString, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await httpClient.PostAsync(serviceParameters.LocalAPIURL + @"/local/PushUpsellPackages", requestContent);
                if (response != null)
                {
                    if (response.IsSuccessStatusCode)
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        new LogHelper().Debug("web API response :- " + apiResponse, reservationNameID, "PushUpsellpackages", serviceParameters.ClientID, groupName);
                        Models.Local.LocalResponseModel localResponse = JsonConvert.DeserializeObject<Models.Local.LocalResponseModel>(apiResponse);
                        return localResponse;
                    }
                    else
                    {
                        new LogHelper().Debug("Failled to push upsell details using web api due to HTTP error : " + response.ReasonPhrase, reservationNameID, "PushUpsellpackages", serviceParameters.ClientID, groupName);
                        return new Models.Local.LocalResponseModel()
                        {
                            result = false,
                            responseMessage = response.ReasonPhrase
                        };
                    }
                }
                else
                {
                    new LogHelper().Debug("Failled to push upsell details using web api due to null returned from the local web api", reservationNameID, "PushUpsellpackages", serviceParameters.ClientID, groupName);
                    return new Models.Local.LocalResponseModel()
                    {
                        result = false,
                        responseMessage = "Local web API returned null"
                    };
                }
            }
            catch (Exception ex)
            {
                new LogHelper().Error(ex, reservationNameID, "PushUpsellpackages", serviceParameters.ClientID, groupName);
                return new Models.Local.LocalResponseModel()
                {
                    result = false,
                    responseMessage = "Generic Exception : " + ex.Message
                };
            }
        }

        public async Task<Models.Local.LocalResponseModel> PushReservationPolicies(string reservationNameID, Models.Local.LocalRequestModel localRequest, string groupName, Models.Local.ServiceParameters serviceParameters)
        {
            try
            {
                new LogHelper().Debug("Pushing reservation policies using web api", reservationNameID, "PushReservationPolicies", serviceParameters.ClientID, groupName);
                HttpClient httpClient = serviceParameters.isProxyEnableForLocalAPI ? getProxyClient(groupName, serviceParameters.LocalAPIProxyHost, serviceParameters.LocalAPIProxyUN, serviceParameters.LocalAPIProxyPswd, serviceParameters.ClientID) : new HttpClient();
                if (httpClient == null)
                {
                    new LogHelper().Debug("Failled to push reservation policies using web api due to proxy error", reservationNameID, "PushReservationPolicies", serviceParameters.ClientID, groupName);
                    return new Models.Local.LocalResponseModel()
                    {
                        result = false,
                        responseMessage = "Failled to generate the proxy http client"
                    };
                }
                httpClient.DefaultRequestHeaders.Clear();
                string requestString = JsonConvert.SerializeObject(localRequest, Formatting.None);
                new LogHelper().Debug("web api url :- " + serviceParameters.LocalAPIURL + @"/local/PushReservationPolicy", reservationNameID, "PushReservationPolicies", serviceParameters.ClientID, groupName);
                new LogHelper().Debug("web api request :- " + requestString, reservationNameID, "PushReservationPolicies", serviceParameters.ClientID, groupName);
                var requestContent = new StringContent(requestString, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await httpClient.PostAsync(serviceParameters.LocalAPIURL + @"/local/PushReservationPolicy", requestContent);
                if (response != null)
                {
                    if (response.IsSuccessStatusCode)
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        new LogHelper().Debug("web API response :- " + apiResponse, reservationNameID, "PushReservationPolicies", serviceParameters.ClientID, groupName);
                        Models.Local.LocalResponseModel localResponse = JsonConvert.DeserializeObject<Models.Local.LocalResponseModel>(apiResponse);
                        return localResponse;
                    }
                    else
                    {
                        new LogHelper().Debug("Failled to push reservation policies using web api due to HTTP error : " + response.ReasonPhrase, reservationNameID, "PushReservationPolicies", serviceParameters.ClientID, groupName);
                        return new Models.Local.LocalResponseModel()
                        {
                            result = false,
                            responseMessage = response.ReasonPhrase
                        };
                    }
                }
                else
                {
                    new LogHelper().Debug("Failled to push reservation policies using web api due to null returned from the local web api", reservationNameID, "PushReservationPolicies", serviceParameters.ClientID, groupName);
                    return new Models.Local.LocalResponseModel()
                    {
                        result = false,
                        responseMessage = "Local web API returned null"
                    };
                }
            }
            catch (Exception ex)
            {
                new LogHelper().Error(ex, reservationNameID, "PushReservationPolicies", serviceParameters.ClientID, groupName);
                return new Models.Local.LocalResponseModel()
                {
                    result = false,
                    responseMessage = "Generic Exception : " + ex.Message
                };
            }
        }
        public async Task<Models.OWS.OwsResponseModel> UpdateCardDetailsInReservationAsyn(string reservationNameID, Models.OWS.OwsRequestModel owsRequest, string groupName, Models.Local.ServiceParameters serviceParameters)
        {
            try
            {
                new LogHelper().Debug("Updating CC details info using web api", reservationNameID, "UpdateCardDetailsInReservationAsyn", serviceParameters.ClientID, groupName);
                HttpClient httpClient = serviceParameters.isProxyEnableForLocalAPI ? getProxyClient(groupName, serviceParameters.LocalAPIProxyHost, serviceParameters.LocalAPIProxyUN, serviceParameters.LocalAPIProxyPswd,serviceParameters.ClientID) : new HttpClient();
                if (httpClient == null)
                {
                    new LogHelper().Debug("Failled to update CC details info using web api due to proxy error", reservationNameID, "UpdateCardDetailsInReservationAsyn", serviceParameters.ClientID, groupName);
                    return new Models.OWS.OwsResponseModel()
                    {
                        result = false,
                        responseMessage = "Failled to generate the proxy http client"
                    };
                }
                httpClient.DefaultRequestHeaders.Clear();
                string requestString = JsonConvert.SerializeObject(owsRequest, Formatting.None);
                new LogHelper().Debug("web api url :- " + serviceParameters.LocalAPIURL + @"/ows/ModifyBooking", reservationNameID, "UpdateCardDetailsInReservationAsyn", serviceParameters.ClientID, groupName);
                new LogHelper().Debug("web api request :- " + requestString, reservationNameID, "UpdateCardDetailsInReservationAsyn", serviceParameters.ClientID, groupName);
                var requestContent = new StringContent(requestString, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await httpClient.PostAsync(serviceParameters.LocalAPIURL + @"/ows/ModifyBooking", requestContent);
                if (response != null)
                {
                    if (response.IsSuccessStatusCode)
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        new LogHelper().Debug("web API response :- " + apiResponse, reservationNameID, "UpdateCardDetailsInReservationAsyn", serviceParameters.ClientID, groupName);
                        Models.OWS.OwsResponseModel owsResponse = JsonConvert.DeserializeObject<Models.OWS.OwsResponseModel>(apiResponse);
                        return owsResponse;
                    }
                    else
                    {
                        new LogHelper().Debug("Failled to update CC details using web api due to HTTP error : " + response.ReasonPhrase, reservationNameID, "UpdateCardDetailsInReservationAsyn", serviceParameters.ClientID, groupName);
                        return new Models.OWS.OwsResponseModel()
                        {
                            result = false,
                            responseMessage = response.ReasonPhrase
                        };
                    }
                }
                else
                {
                    new LogHelper().Debug("Failled to update CC details using web api due to null returned from the local web api", reservationNameID, "UpdateCardDetailsInReservationAsyn", serviceParameters.ClientID, groupName);
                    return new Models.OWS.OwsResponseModel()
                    {
                        result = false,
                        responseMessage = "Local web API returned null"
                    };
                }
            }
            catch (Exception ex)
            {
                new LogHelper().Error(ex, reservationNameID, "UpdateCardDetailsInReservationAsyn", serviceParameters.ClientID, groupName);
                return new Models.OWS.OwsResponseModel()
                {
                    result = false,
                    responseMessage = "Generic Exception : " + ex.Message
                };
            }
        }

        public async Task<Models.OWS.OwsResponseModel> ModifyBooking(string reservationNameID, Models.OWS.OwsRequestModel owsRequest, string groupName, Models.Local.ServiceParameters serviceParameters)
        {
            try
            {
                new LogHelper().Debug("Modifying reservatioon using web api", reservationNameID, "ModifyBooking", serviceParameters.ClientID, groupName);
                HttpClient httpClient = serviceParameters.isProxyEnableForLocalAPI ? getProxyClient(groupName, serviceParameters.LocalAPIProxyHost, serviceParameters.LocalAPIProxyUN, serviceParameters.LocalAPIProxyPswd,serviceParameters.ClientID) : new HttpClient();
                if (httpClient == null)
                {
                    new LogHelper().Debug("Failled to modify reservation using web api due to proxy error", reservationNameID, "ModifyBooking", serviceParameters.ClientID, groupName);
                    return new Models.OWS.OwsResponseModel()
                    {
                        result = false,
                        responseMessage = "Failled to generate the proxy http client"
                    };
                }
                httpClient.DefaultRequestHeaders.Clear();
                string requestString = JsonConvert.SerializeObject(owsRequest, Formatting.None);
                new LogHelper().Debug("web api url :- " + serviceParameters.LocalAPIURL + @"/ows/ModifyBooking", reservationNameID, "ModifyBooking", serviceParameters.ClientID, groupName);
                new LogHelper().Debug("web api request :- " + requestString, reservationNameID, "ModifyBooking", serviceParameters.ClientID, groupName);
                var requestContent = new StringContent(requestString, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await httpClient.PostAsync(serviceParameters.LocalAPIURL + @"/ows/ModifyBooking", requestContent);
                if (response != null)
                {
                    if (response.IsSuccessStatusCode)
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        new LogHelper().Debug("web API response :- " + apiResponse, reservationNameID, "ModifyBooking", serviceParameters.ClientID, groupName);
                        Models.OWS.OwsResponseModel owsResponse = JsonConvert.DeserializeObject<Models.OWS.OwsResponseModel>(apiResponse);
                        return owsResponse;
                    }
                    else
                    {
                        new LogHelper().Debug("Failled to modify reservation using web api due to HTTP error : " + response.ReasonPhrase, reservationNameID, "ModifyBooking", serviceParameters.ClientID, groupName);
                        return new Models.OWS.OwsResponseModel()
                        {
                            result = false,
                            responseMessage = response.ReasonPhrase
                        };
                    }
                }
                else
                {
                    new LogHelper().Debug("Failled to modify reservation using web api due to null returned from the local web api", reservationNameID, "ModifyBooking", serviceParameters.ClientID, groupName);
                    return new Models.OWS.OwsResponseModel()
                    {
                        result = false,
                        responseMessage = "Local web API returned null"
                    };
                }
            }
            catch (Exception ex)
            {
                new LogHelper().Error(ex, reservationNameID, "ModifyBooking", serviceParameters.ClientID, groupName);
                return new Models.OWS.OwsResponseModel()
                {
                    result = false,
                    responseMessage = "Generic Exception : " + ex.Message
                };
            }
        }

        public async Task<Models.OWS.OwsResponseModel> MakePayment(string reservationNameID, Models.OWS.OwsRequestModel owsRequest, string groupName, Models.Local.ServiceParameters serviceParameters)
        {
            try
            {
                new LogHelper().Debug("Posting payment using web api", reservationNameID, "MakePayment", serviceParameters.ClientID, groupName);
                HttpClient httpClient = serviceParameters.isProxyEnableForLocalAPI ? getProxyClient(groupName, serviceParameters.LocalAPIProxyHost, serviceParameters.LocalAPIProxyUN, serviceParameters.LocalAPIProxyPswd,serviceParameters.ClientID) : new HttpClient();
                if (httpClient == null)
                {
                    new LogHelper().Debug("Failled to post payment using web api due to proxy error", reservationNameID, "MakePayment", serviceParameters.ClientID, groupName);
                    return new Models.OWS.OwsResponseModel()
                    {
                        result = false,
                        responseMessage = "Failled to generate the proxy http client"
                    };
                }
                httpClient.DefaultRequestHeaders.Clear();
                string requestString = JsonConvert.SerializeObject(owsRequest, Formatting.None);
                new LogHelper().Debug("web api url :- " + serviceParameters.LocalAPIURL + @"/ows/MakePayment", reservationNameID, "MakePayment", serviceParameters.ClientID, groupName);
                new LogHelper().Debug("web api request :- " + requestString, reservationNameID, "MakePayment", serviceParameters.ClientID, groupName);
                var requestContent = new StringContent(requestString, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await httpClient.PostAsync(serviceParameters.LocalAPIURL + @"/ows/MakePayment", requestContent);
                if (response != null)
                {
                    if (response.IsSuccessStatusCode)
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        new LogHelper().Debug("web API response :- " + apiResponse, reservationNameID, "MakePayment", serviceParameters.ClientID, groupName);
                        Models.OWS.OwsResponseModel owsResponse = JsonConvert.DeserializeObject<Models.OWS.OwsResponseModel>(apiResponse);
                        return owsResponse;
                    }
                    else
                    {
                        new LogHelper().Debug("Failled to post payment using web api due to HTTP error : " + response.ReasonPhrase, reservationNameID, "MakePayment", serviceParameters.ClientID, groupName);
                        return new Models.OWS.OwsResponseModel()
                        {
                            result = false,
                            responseMessage = response.ReasonPhrase
                        };
                    }
                }
                else
                {
                    new LogHelper().Debug("Failled to post payment using web api due to null returned from the local web api", reservationNameID, "MakePayment", serviceParameters.ClientID, groupName);
                    return new Models.OWS.OwsResponseModel()
                    {
                        result = false,
                        responseMessage = "Local web API returned null"
                    };
                }
            }
            catch (Exception ex)
            {
                new LogHelper().Error(ex, reservationNameID, "MakePayment", serviceParameters.ClientID, groupName);
                return new Models.OWS.OwsResponseModel()
                {
                    result = false,
                    responseMessage = "Generic Exception : " + ex.Message
                };
            }
        }
        public async Task<Models.OWS.OwsResponseModel> GetFolioByWindow(string reservationNameID, Models.OWS.OwsRequestModel owsRequest, string groupName, Models.Local.ServiceParameters serviceParameters)
        {
            try
            {
                new LogHelper().Debug("Fetching reservation folio by window using web api", reservationNameID, "GetFolioByWindow", serviceParameters.ClientID, groupName);
                HttpClient httpClient = serviceParameters.isProxyEnableForLocalAPI ? getProxyClient(groupName, serviceParameters.LocalAPIProxyHost, serviceParameters.LocalAPIProxyUN, serviceParameters.LocalAPIProxyPswd,serviceParameters.ClientID) : new HttpClient();
                if (httpClient == null)
                {
                    new LogHelper().Debug("Failled to fetch folio by window using web api due to proxy error", reservationNameID, "GetFolioByWindow", serviceParameters.ClientID, groupName);
                    return new Models.OWS.OwsResponseModel()
                    {
                        result = false,
                        responseMessage = "Failled to generate the proxy http client"
                    };
                }
                httpClient.DefaultRequestHeaders.Clear();
                string requestString = JsonConvert.SerializeObject(owsRequest, Formatting.None);
                new LogHelper().Debug("web api url :- " + serviceParameters.LocalAPIURL + @"/ows/GetGuestFolioByWindow", reservationNameID, "GetFolioByWindow", serviceParameters.ClientID, groupName);
                new LogHelper().Debug("web api request :- " + requestString, reservationNameID, "GetFolioByWindow", serviceParameters.ClientID, groupName);
                var requestContent = new StringContent(requestString, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await httpClient.PostAsync(serviceParameters.LocalAPIURL + @"/ows/GetGuestFolioByWindow", requestContent);
                if (response != null)
                {
                    if (response.IsSuccessStatusCode)
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        new LogHelper().Debug("web API response :- " + apiResponse, reservationNameID, "GetFolioByWindow", serviceParameters.ClientID, groupName);
                        Models.OWS.OwsResponseModel owsResponse = JsonConvert.DeserializeObject<Models.OWS.OwsResponseModel>(apiResponse);
                        return owsResponse;
                    }
                    else
                    {
                        new LogHelper().Debug("Failled to fetch folio by window using web api due to HTTP error : " + response.ReasonPhrase, reservationNameID, "GetFolioByWindow", serviceParameters.ClientID, groupName);
                        return new Models.OWS.OwsResponseModel()
                        {
                            result = false,
                            responseMessage = response.ReasonPhrase
                        };
                    }
                }
                else
                {
                    new LogHelper().Debug("Failled to fetch folio by window using web api due to null returned from the local web api", reservationNameID, "GetFolioByWindow", serviceParameters.ClientID, groupName);
                    return new Models.OWS.OwsResponseModel()
                    {
                        result = false,
                        responseMessage = "Local web API returned null"
                    };
                }
            }
            catch (Exception ex)
            {
                new LogHelper().Error(ex, reservationNameID, "GetFolioByWindow", serviceParameters.ClientID, groupName);
                return new Models.OWS.OwsResponseModel()
                {
                    result = false,
                    responseMessage = "Generic Exception : " + ex.Message
                };
            }
        }
        public async Task<Models.OWS.OwsResponseModel> GetFolio(string reservationNameID, Models.OWS.OwsRequestModel owsRequest, string groupName, Models.Local.ServiceParameters serviceParameters)
        {
            try
            {
                new LogHelper().Debug("Fetching reservation folio using web api", reservationNameID, "GetFolio", serviceParameters.ClientID, groupName);
                HttpClient httpClient = serviceParameters.isProxyEnableForCloudAPI ? getProxyClient(groupName, serviceParameters.CloudAPIProxyHost, serviceParameters.CloudAPIProxyUN, serviceParameters.CloudAPIProxyPswd,serviceParameters.ClientID) : new HttpClient();
                if (httpClient == null)
                {
                    new LogHelper().Debug("Failled to fetch folio using web api due to proxy error", reservationNameID, "GetFolio", serviceParameters.ClientID, groupName);
                    return new Models.OWS.OwsResponseModel()
                    {
                        result = false,
                        responseMessage = "Failled to generate the proxy http client"
                    };
                }
                httpClient.DefaultRequestHeaders.Clear();
                string requestString = JsonConvert.SerializeObject(owsRequest, Formatting.None);
                new LogHelper().Debug("web api url :- " + serviceParameters.CloudAPIURL + @"/ows/GetGuestFolioAsBase64", reservationNameID, "GetFolio", serviceParameters.ClientID, groupName);
                new LogHelper().Debug("web api request :- " + requestString, reservationNameID, "GetFolio", serviceParameters.ClientID, groupName);
                var requestContent = new StringContent(requestString, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await httpClient.PostAsync(serviceParameters.CloudAPIURL + @"/ows/GetGuestFolioAsBase64", requestContent);
                if (response != null)
                {
                    if (response.IsSuccessStatusCode)
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        new LogHelper().Debug("web API response :- " + apiResponse, reservationNameID, "GetFolio", serviceParameters.ClientID, groupName);
                        Models.OWS.OwsResponseModel owsResponse = JsonConvert.DeserializeObject<Models.OWS.OwsResponseModel>(apiResponse);
                        return owsResponse;
                    }
                    else
                    {
                        new LogHelper().Debug("Failled to fetch folio using web api due to HTTP error : " + response.ReasonPhrase, reservationNameID, "GetFolio", serviceParameters.ClientID, groupName);
                        return new Models.OWS.OwsResponseModel()
                        {
                            result = false,
                            responseMessage = response.ReasonPhrase
                        };
                    }
                }
                else
                {
                    new LogHelper().Debug("Failled to fetch folio using web api due to null returned from the local web api", reservationNameID, "GetFolio", serviceParameters.ClientID, groupName);
                    return new Models.OWS.OwsResponseModel()
                    {
                        result = false,
                        responseMessage = "Local web API returned null"
                    };
                }
            }
            catch (Exception ex)
            {
                new LogHelper().Error(ex, reservationNameID, "GetFolio", serviceParameters.ClientID, groupName);
                return new Models.OWS.OwsResponseModel()
                {
                    result = false,
                    responseMessage = "Generic Exception : " + ex.Message
                };
            }
        }

        public async Task<Models.Cloud.CloudResponseModel> UpdateRecordInCloud(string reservationNameID, Models.Cloud.CloudRequestModel cloudRequest, string groupName, Models.Local.ServiceParameters serviceParameters)
        {
            try
            {
                new LogHelper().Debug("Updating reservation sync to local true", reservationNameID, "UpdateRecordInCloud", serviceParameters.ClientID, groupName);
                ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
                {
                    return true;
                };
                HttpClient httpClient = serviceParameters.isProxyEnableForCloudAPI ? getProxyClient(groupName, serviceParameters.CloudAPIProxyHost, serviceParameters.CloudAPIProxyUN, serviceParameters.CloudAPIProxyPswd,serviceParameters.ClientID) : new HttpClient();
                if (httpClient == null)
                {
                    new LogHelper().Debug("Failled to update record using web api due to proxy error", reservationNameID, "UpdateRecordInCloud", serviceParameters.ClientID, groupName);
                    return new Models.Cloud.CloudResponseModel()
                    {
                        result = false,
                        responseMessage = "Failled to generate the proxy http client"
                    };
                }
                httpClient.DefaultRequestHeaders.Clear();
                string requestString = JsonConvert.SerializeObject(cloudRequest, Formatting.None);
                new LogHelper().Debug("web api url :- " + serviceParameters.CloudAPIURL + @"/cloud/UpdateReservationDetails", reservationNameID, "UpdateRecordInCloud", serviceParameters.ClientID, groupName);
                new LogHelper().Debug("web api request :- " + requestString, reservationNameID, "UpdateRecordInCloud", serviceParameters.ClientID, groupName);
                var requestContent = new StringContent(requestString, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await httpClient.PostAsync(serviceParameters.CloudAPIURL + @"/cloud/UpdateReservationDetails", requestContent);
                if (response != null)
                {
                    if (response.IsSuccessStatusCode)
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        new LogHelper().Debug("web API response :- " + apiResponse, reservationNameID, "UpdateRecordInCloud", serviceParameters.ClientID, groupName);
                        Models.Cloud.CloudResponseModel cloudResponse = JsonConvert.DeserializeObject<Models.Cloud.CloudResponseModel>(apiResponse);
                        return cloudResponse;
                    }
                    else
                    {
                        new LogHelper().Debug("Failled to update record using web api due to HTTP error : " + response.ReasonPhrase, reservationNameID, "UpdateRecordInCloud", serviceParameters.ClientID, groupName);
                        return new Models.Cloud.CloudResponseModel()
                        {
                            result = false,
                            responseMessage = response.ReasonPhrase
                        };
                    }
                }
                else
                {
                    new LogHelper().Debug("Failled to update record using web api due to null returned from the local web api", reservationNameID, "UpdateRecordInCloud", serviceParameters.ClientID, groupName);
                    return new Models.Cloud.CloudResponseModel()
                    {
                        result = false,
                        responseMessage = "Cloud web API returned null"
                    };
                }
            }
            catch (Exception ex)
            {
                new LogHelper().Error(ex, reservationNameID, "UpdateRecordInCloud", serviceParameters.ClientID, groupName);
                return new Models.Cloud.CloudResponseModel()
                {
                    result = false,
                    responseMessage = "Generic Exception : " + ex.Message
                };
            }
        }

        public async Task<Models.Cloud.CloudResponseModel> ClearRecords(string reservationNameID, Models.Cloud.CloudRequestModel cloudRequest, string groupName, Models.Local.ServiceParameters serviceParameters)
        {
            try
            {
                new LogHelper().Debug("Clearing record in cloud", reservationNameID, "ClearRecords", serviceParameters.ClientID, groupName);
                ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
                {
                    return true;
                };
                HttpClient httpClient = serviceParameters.isProxyEnableForCloudAPI ? getProxyClient(groupName, serviceParameters.CloudAPIProxyHost, serviceParameters.CloudAPIProxyUN, serviceParameters.CloudAPIProxyPswd,serviceParameters.ClientID) : new HttpClient();
                if (httpClient == null)
                {
                    new LogHelper().Debug("Failled to clear record using web api due to proxy error", reservationNameID, "ClearRecords", serviceParameters.ClientID, groupName);
                    return new Models.Cloud.CloudResponseModel()
                    {
                        result = false,
                        responseMessage = "Failled to generate the proxy http client"
                    };
                }
                httpClient.DefaultRequestHeaders.Clear();
                string requestString = JsonConvert.SerializeObject(cloudRequest, Formatting.None);
                new LogHelper().Debug("web api url :- " + serviceParameters.CloudAPIURL + @"/cloud/ClearCloudData", reservationNameID, "ClearRecords", serviceParameters.ClientID, groupName);
                new LogHelper().Debug("web api request :- " + requestString, reservationNameID, "ClearRecords", serviceParameters.ClientID, groupName);
                var requestContent = new StringContent(requestString, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await httpClient.PostAsync(serviceParameters.CloudAPIURL + @"/cloud/ClearCloudData", requestContent);
                if (response != null)
                {
                    if (response.IsSuccessStatusCode)
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        new LogHelper().Debug("web API response :- " + apiResponse, reservationNameID, "ClearRecords", serviceParameters.ClientID, groupName);
                        Models.Cloud.CloudResponseModel cloudResponse = JsonConvert.DeserializeObject<Models.Cloud.CloudResponseModel>(apiResponse);
                        return cloudResponse;
                    }
                    else
                    {
                        new LogHelper().Debug("Failled to clear record using web api due to HTTP error : " + response.ReasonPhrase, reservationNameID, "ClearRecords", serviceParameters.ClientID, groupName);
                        return new Models.Cloud.CloudResponseModel()
                        {
                            result = false,
                            responseMessage = response.ReasonPhrase
                        };
                    }
                }
                else
                {
                    new LogHelper().Debug("Failled to clear record using web api due to null returned from the local web api", reservationNameID, "ClearRecords", serviceParameters.ClientID, groupName);
                    return new Models.Cloud.CloudResponseModel()
                    {
                        result = false,
                        responseMessage = "Cloud web API returned null"
                    };
                }
            }
            catch (Exception ex)
            {
                new LogHelper().Error(ex, reservationNameID, "ClearRecords", serviceParameters.ClientID, groupName);
                return new Models.Cloud.CloudResponseModel()
                {
                    result = false,
                    responseMessage = "Generic Exception : " + ex.Message
                };
            }
        }

        public async Task<Models.Cloud.CloudResponseModel> PushRecordToCloud(string reservationNameID, Models.Cloud.CloudRequestModel cloudRequest, string groupName, Models.Local.ServiceParameters serviceParameters)
        {
            try
            {
                new LogHelper().Debug("Pushing due in record", reservationNameID, "PushRecordToCloud", serviceParameters.ClientID, groupName);
                ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
                {
                    return true;
                };
                HttpClient httpClient = serviceParameters.isProxyEnableForCloudAPI ? getProxyClient(groupName, serviceParameters.CloudAPIProxyHost, serviceParameters.CloudAPIProxyUN, serviceParameters.CloudAPIProxyPswd,serviceParameters.ClientID) : new HttpClient();
                if (httpClient == null)
                {
                    new LogHelper().Debug("Failled to push due in record using web api due to proxy error", reservationNameID, "PushRecordToCloud", serviceParameters.ClientID, groupName);
                    return new Models.Cloud.CloudResponseModel()
                    {
                        result = false,
                        responseMessage = "Failled to generate the proxy http client"
                    };
                }
                httpClient.DefaultRequestHeaders.Clear();


                string requestString = JsonConvert.SerializeObject(cloudRequest, Formatting.None);
                new LogHelper().Debug("web api url :- " + serviceParameters.CloudAPIURL + @"/cloud/PushReservationDetails", reservationNameID, "PushRecordToCloud", serviceParameters.ClientID, groupName);
                new LogHelper().Debug("web api request :- " + requestString, reservationNameID, "PushRecordToCloud", serviceParameters.ClientID, groupName);
                var requestContent = new StringContent(requestString, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await httpClient.PostAsync(serviceParameters.CloudAPIURL + @"/cloud/PushReservationDetails", requestContent);
                if (response != null)
                {
                    if (response.IsSuccessStatusCode)
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        new LogHelper().Debug("web API response :- " + apiResponse, reservationNameID, "PushRecordToCloud", serviceParameters.ClientID, groupName);
                        Models.Cloud.CloudResponseModel cloudResponse = JsonConvert.DeserializeObject<Models.Cloud.CloudResponseModel>(apiResponse);
                        return cloudResponse;
                    }
                    else
                    {
                        new LogHelper().Debug("Failled to push due in reservations using web api due to HTTP error : " + response.ReasonPhrase, reservationNameID, "PushRecordToCloud", serviceParameters.ClientID, groupName);
                        return new Models.Cloud.CloudResponseModel()
                        {
                            result = false,
                            responseMessage = response.ReasonPhrase
                        };
                    }
                }
                else
                {
                    new LogHelper().Debug("Failled to push due in reservations using web api due to null returned from the local web api", reservationNameID, "PushRecordToCloud", serviceParameters.ClientID, groupName);
                    return new Models.Cloud.CloudResponseModel()
                    {
                        result = false,
                        responseMessage = "Cloud web API returned null"
                    };
                }
            }
            catch (Exception ex)
            {
                new LogHelper().Error(ex, reservationNameID, "PushRecordToCloud", serviceParameters.ClientID, groupName);
                return new Models.Cloud.CloudResponseModel()
                {
                    result = false,
                    responseMessage = "Generic Exception : " + ex.Message
                };
            }
        }

        public async Task<Models.Cloud.CloudResponseModel> PushPaymentDetails(string reservationNameID, Models.Cloud.CloudRequestModel cloudRequest, string groupName, Models.Local.ServiceParameters serviceParameters)
        {
            try
            {
                new LogHelper().Debug("Pushing payment details", reservationNameID, "PushPaymentDetails", serviceParameters.ClientID, groupName);
                ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
                {
                    return true;
                };
                HttpClient httpClient = serviceParameters.isProxyEnableForCloudAPI ? getProxyClient(groupName, serviceParameters.CloudAPIProxyHost, serviceParameters.CloudAPIProxyUN, serviceParameters.CloudAPIProxyPswd,serviceParameters.ClientID) : new HttpClient();
                if (httpClient == null)
                {
                    new LogHelper().Debug("Failled to push payment details using web api due to proxy error", reservationNameID, "PushPaymentDetails", serviceParameters.ClientID, groupName);
                    return new Models.Cloud.CloudResponseModel()
                    {
                        result = false,
                        responseMessage = "Failled to generate the proxy http client"
                    };
                }
                httpClient.DefaultRequestHeaders.Clear();
                string requestString = JsonConvert.SerializeObject(cloudRequest, Formatting.None);
                new LogHelper().Debug("web api url :- " + serviceParameters.CloudAPIURL + @"/cloud/PushPaymentDeatils", reservationNameID, "PushPaymentDetails", serviceParameters.ClientID, groupName);
                new LogHelper().Debug("web api request :- " + requestString, reservationNameID, "PushPaymentDetails", serviceParameters.ClientID, groupName);
                var requestContent = new StringContent(requestString, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await httpClient.PostAsync(serviceParameters.CloudAPIURL + @"/cloud/PushPaymentDeatils", requestContent);
                if (response != null)
                {
                    if (response.IsSuccessStatusCode)
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        new LogHelper().Debug("web API response :- " + apiResponse, reservationNameID, "PushPaymentDetails", serviceParameters.ClientID, groupName);
                        Models.Cloud.CloudResponseModel cloudResponse = JsonConvert.DeserializeObject<Models.Cloud.CloudResponseModel>(apiResponse);
                        return cloudResponse;
                    }
                    else
                    {
                        new LogHelper().Debug("Failled to push payment details using web api due to HTTP error : " + response.ReasonPhrase, reservationNameID, "PushPaymentDetails", serviceParameters.ClientID, groupName);
                        return new Models.Cloud.CloudResponseModel()
                        {
                            result = false,
                            responseMessage = response.ReasonPhrase
                        };
                    }
                }
                else
                {
                    new LogHelper().Debug("Failled to push payment details using web api due to null returned from the local web api", reservationNameID, "PushPaymentDetails", serviceParameters.ClientID, groupName);
                    return new Models.Cloud.CloudResponseModel()
                    {
                        result = false,
                        responseMessage = "Cloud web API returned null"
                    };
                }
            }
            catch (Exception ex)
            {
                new LogHelper().Error(ex, reservationNameID, "PushPaymentDetails", serviceParameters.ClientID, groupName);
                return new Models.Cloud.CloudResponseModel()
                {
                    result = false,
                    responseMessage = "Generic Exception : " + ex.Message
                };
            }
        }

        public async Task<Models.OWS.OwsResponseModel> CheckoutReservation(string reservationNameID, Models.OWS.OwsRequestModel owsRequest, string groupName,Models.Local.ServiceParameters serviceParameters)
        {
            try
            {
                new LogHelper().Debug("Checking out reservation using web api", reservationNameID, "CheckoutReservation", serviceParameters.ClientID, groupName);
                HttpClient httpClient = serviceParameters.isProxyEnableForLocalAPI ? getProxyClient(groupName, serviceParameters.LocalAPIProxyHost, serviceParameters.LocalAPIProxyUN, serviceParameters.LocalAPIProxyPswd,serviceParameters.ClientID) : new HttpClient();
                if (httpClient == null)
                {
                    new LogHelper().Debug("Failled to check-out using web api due to proxy error", reservationNameID, "CheckoutReservation", serviceParameters.ClientID, groupName);
                    return new Models.OWS.OwsResponseModel()
                    {
                        result = false,
                        responseMessage = "Failled to generate the proxy http client"
                    };
                }
                httpClient.DefaultRequestHeaders.Clear();
                string requestString = JsonConvert.SerializeObject(owsRequest, Formatting.None);
                new LogHelper().Debug("web api url :- " + serviceParameters.LocalAPIURL + @"/ows/GuestCheckOut", reservationNameID, "CheckoutReservation", serviceParameters.ClientID, groupName);
                new LogHelper().Debug("web api request :- " + requestString, reservationNameID, "CheckoutReservation", serviceParameters.ClientID, groupName);
                var requestContent = new StringContent(requestString, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await httpClient.PostAsync(serviceParameters.LocalAPIURL + @"/ows/GuestCheckOut", requestContent);
                if (response != null)
                {
                    if (response.IsSuccessStatusCode)
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        new LogHelper().Debug("web API response :- " + apiResponse, reservationNameID, "CheckoutReservation", serviceParameters.ClientID, groupName);
                        Models.OWS.OwsResponseModel owsResponse = JsonConvert.DeserializeObject<Models.OWS.OwsResponseModel>(apiResponse);
                        return owsResponse;
                    }
                    else
                    {
                        new LogHelper().Debug("Failled to check-out using web api due to HTTP error : " + response.ReasonPhrase, reservationNameID, "CheckoutReservation", serviceParameters.ClientID, groupName);
                        return new Models.OWS.OwsResponseModel()
                        {
                            result = false,
                            responseMessage = response.ReasonPhrase
                        };
                    }
                }
                else
                {
                    new LogHelper().Debug("Failled to check-out using web api due to null returned from the local web api", reservationNameID, "CheckoutReservation", serviceParameters.ClientID, groupName);
                    return new Models.OWS.OwsResponseModel()
                    {
                        result = false,
                        responseMessage = "Local web API returned null"
                    };
                }
            }
            catch (Exception ex)
            {
                new LogHelper().Error(ex, reservationNameID, "CheckoutReservation", serviceParameters.ClientID, groupName);
                return new Models.OWS.OwsResponseModel()
                {
                    result = false,
                    responseMessage = "Generic Exception : " + ex.Message
                };
            }
        }
        public async Task<Models.Local.LocalResponseModel> PushReservationTrackLocally(string reservationNameID, Models.Local.LocalRequestModel localRequest, string groupName, Models.Local.ServiceParameters serviceParameters)
        {
            try
            {
                new LogHelper().Debug("Pushing reservation track in local DB using web api", reservationNameID, "PushReservationTrackLocally", serviceParameters.ClientID, groupName);
                HttpClient httpClient = serviceParameters.isProxyEnableForLocalAPI ? getProxyClient(groupName, serviceParameters.LocalAPIProxyHost, serviceParameters.LocalAPIProxyUN, serviceParameters.LocalAPIProxyPswd,serviceParameters.ClientID) : new HttpClient();
                if (httpClient == null)
                {
                    new LogHelper().Debug("Failled to push reservation track in local DB using web api due to proxy error", reservationNameID, "PushReservationTrackLocally", serviceParameters.ClientID, groupName);
                    return new Models.Local.LocalResponseModel()
                    {
                        result = false,
                        responseMessage = "Failled to generate the proxy http client"
                    };
                }
                httpClient.DefaultRequestHeaders.Clear();
                string requestString = JsonConvert.SerializeObject(localRequest, Formatting.None);
                new LogHelper().Debug("web api url :- " + serviceParameters.LocalAPIURL + @"/local/PushReservationTrackDetailStatus", reservationNameID, "PushReservationTrackLocally", serviceParameters.ClientID, groupName);
                new LogHelper().Debug("web api request :- " + requestString, reservationNameID, "PushReservationTrackLocally", serviceParameters.ClientID, groupName);
                var requestContent = new StringContent(requestString, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await httpClient.PostAsync(serviceParameters.LocalAPIURL + @"/local/PushReservationTrackDetailStatus", requestContent);
                if (response != null)
                {
                    if (response.IsSuccessStatusCode)
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        new LogHelper().Debug("web API response :- " + apiResponse, reservationNameID, "PushReservationTrackLocally", serviceParameters.ClientID, groupName);
                        Models.Local.LocalResponseModel localResponse = JsonConvert.DeserializeObject<Models.Local.LocalResponseModel>(apiResponse);
                        return localResponse;
                    }
                    else
                    {
                        new LogHelper().Debug("Failled to push reservation track in local DB using web api due to HTTP error : " + response.ReasonPhrase, reservationNameID, "PushReservationTrackLocally", serviceParameters.ClientID, groupName);
                        return new Models.Local.LocalResponseModel()
                        {
                            result = false,
                            responseMessage = response.ReasonPhrase
                        };
                    }
                }
                else
                {
                    new LogHelper().Debug("Failled to push reservation track in local DB using web api due to null returned from the local web api", reservationNameID, "PushReservationTrackLocally", serviceParameters.ClientID, groupName);
                    return new Models.Local.LocalResponseModel()
                    {
                        result = false,
                        responseMessage = "Local web API returned null"
                    };
                }
            }
            catch (Exception ex)
            {
                new LogHelper().Error(ex, reservationNameID, "PushReservationTrackLocally", serviceParameters.ClientID, groupName);
                return new Models.Local.LocalResponseModel()
                {
                    result = false,
                    responseMessage = "Generic Exception : " + ex.Message
                };
            }
        }
        public async Task<Models.Local.LocalResponseModel> UpdateRecordLocally(string reservationNameID, Models.Local.LocalRequestModel localRequest, string groupName, Models.Local.ServiceParameters serviceParameters)
        {
            try
            {
                new LogHelper().Debug("Updating reservation status in local DB using web api", reservationNameID, "UpdateRecordLocally", serviceParameters.ClientID, groupName);
                HttpClient httpClient = serviceParameters.isProxyEnableForLocalAPI ? getProxyClient(groupName, serviceParameters.LocalAPIProxyHost, serviceParameters.LocalAPIProxyUN, serviceParameters.LocalAPIProxyPswd,serviceParameters.ClientID) : new HttpClient();
                if (httpClient == null)
                {
                    new LogHelper().Debug("Failled to update reservation status in local DB using web api due to proxy error", reservationNameID, "UpdateRecordLocally", serviceParameters.ClientID, groupName);
                    return new Models.Local.LocalResponseModel()
                    {
                        result = false,
                        responseMessage = "Failled to generate the proxy http client"
                    };
                }
                httpClient.DefaultRequestHeaders.Clear();
                string requestString = JsonConvert.SerializeObject(localRequest, Formatting.None);
                new LogHelper().Debug("web api url :- " + serviceParameters.LocalAPIURL + @"/local/UpdateReservationDetails", reservationNameID, "UpdateRecordLocally", serviceParameters.ClientID, groupName);
                new LogHelper().Debug("web api request :- " + requestString, reservationNameID, "UpdateRecordLocally", serviceParameters.ClientID, groupName);
                var requestContent = new StringContent(requestString, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await httpClient.PostAsync(serviceParameters.LocalAPIURL + @"/local/UpdateReservationDetails", requestContent);
                if (response != null)
                {
                    if (response.IsSuccessStatusCode)
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        new LogHelper().Debug("web API response :- " + apiResponse, reservationNameID, "UpdateRecordLocally", serviceParameters.ClientID, groupName);
                        Models.Local.LocalResponseModel localResponse = JsonConvert.DeserializeObject<Models.Local.LocalResponseModel>(apiResponse);
                        return localResponse;
                    }
                    else
                    {
                        new LogHelper().Debug("Failled to update reservation status in local DB using web api due to HTTP error : " + response.ReasonPhrase, reservationNameID, "UpdateRecordLocally", serviceParameters.ClientID, groupName);
                        return new Models.Local.LocalResponseModel()
                        {
                            result = false,
                            responseMessage = response.ReasonPhrase
                        };
                    }
                }
                else
                {
                    new LogHelper().Debug("Failled to update reservation status in local DB using web api due to null returned from the local web api", reservationNameID, "UpdateRecordLocally", serviceParameters.ClientID, groupName);
                    return new Models.Local.LocalResponseModel()
                    {
                        result = false,
                        responseMessage = "Local web API returned null"
                    };
                }
            }
            catch (Exception ex)
            {
                new LogHelper().Error(ex, reservationNameID, "UpdateRecordLocally", serviceParameters.ClientID, groupName);
                return new Models.Local.LocalResponseModel()
                {
                    result = false,
                    responseMessage = "Generic Exception : " + ex.Message
                };
            }
        }
        public async Task<Models.Local.LocalResponseModel> FetchPaymentDetails(string reservationNameID, Models.Local.LocalRequestModel localRequest, string groupName, Models.Local.ServiceParameters serviceParameters)
        {
            try
            {
                new LogHelper().Debug("Fetching payment details using web api", reservationNameID, "FetchPaymentDetails", serviceParameters.ClientID, groupName);
                HttpClient httpClient = serviceParameters.isProxyEnableForLocalAPI ? getProxyClient(groupName, serviceParameters.LocalAPIProxyHost, serviceParameters.LocalAPIProxyUN, serviceParameters.LocalAPIProxyPswd,serviceParameters.ClientID) : new HttpClient();
                if (httpClient == null)
                {
                    new LogHelper().Debug("Failled to payment details using web api due to proxy error", reservationNameID, "FetchPaymentDetails", serviceParameters.ClientID, groupName);
                    return new Models.Local.LocalResponseModel()
                    {
                        result = false,
                        responseMessage = "Failled to generate the proxy http client"
                    };
                }
                httpClient.DefaultRequestHeaders.Clear();
                string requestString = JsonConvert.SerializeObject(localRequest, Formatting.None);
                new LogHelper().Debug("web api url :- " + serviceParameters.LocalAPIURL + @"/local/FetchPaymentDetails", reservationNameID, "FetchPaymentDetails", serviceParameters.ClientID, groupName);
                new LogHelper().Debug("web api request :- " + requestString, reservationNameID, "FetchPaymentDetails", serviceParameters.ClientID, groupName);
                var requestContent = new StringContent(requestString, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await httpClient.PostAsync(serviceParameters.LocalAPIURL + @"/local/FetchPaymentDetails", requestContent);
                if (response != null)
                {
                    if (response.IsSuccessStatusCode)
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        new LogHelper().Debug("web API response :- " + apiResponse, reservationNameID, "FetchPaymentDetails", serviceParameters.ClientID, groupName);
                        Models.Local.LocalResponseModel localResponse = JsonConvert.DeserializeObject<Models.Local.LocalResponseModel>(apiResponse);
                        return localResponse;
                    }
                    else
                    {
                        new LogHelper().Debug("Failled to payment details using web api due to HTTP error : " + response.ReasonPhrase, reservationNameID, "FetchPaymentDetails", serviceParameters.ClientID, groupName);
                        return new Models.Local.LocalResponseModel()
                        {
                            result = false,
                            responseMessage = response.ReasonPhrase
                        };
                    }
                }
                else
                {
                    new LogHelper().Debug("Failled to payment details using web api due to null returned from the local web api", reservationNameID, "FetchPaymentDetails", serviceParameters.ClientID, groupName);
                    return new Models.Local.LocalResponseModel()
                    {
                        result = false,
                        responseMessage = "Local web API returned null"
                    };
                }
            }
            catch (Exception ex)
            {
                new LogHelper().Error(ex, reservationNameID, "FetchPaymentDetails", serviceParameters.ClientID, groupName);
                return new Models.Local.LocalResponseModel()
                {
                    result = false,
                    responseMessage = "Generic Exception : " + ex.Message
                };
            }
        }

        public async Task<Models.Local.LocalResponseModel> FetchReservationTracjLocally(string reservationNameID, Models.Local.LocalRequestModel localRequest, string groupName, Models.Local.ServiceParameters serviceParameters)
        {
            try
            {
                new LogHelper().Debug("Fetching reservation track in local DB using web api", reservationNameID, "FetchReservationTracjLocally", serviceParameters.ClientID, groupName);
                HttpClient httpClient = serviceParameters.isProxyEnableForLocalAPI ? getProxyClient(groupName, serviceParameters.LocalAPIProxyHost, serviceParameters.LocalAPIProxyUN, serviceParameters.LocalAPIProxyPswd,serviceParameters.ClientID) : new HttpClient();
                if (httpClient == null)
                {
                    new LogHelper().Debug("Failled to fetch reservation track in local DB using web api due to proxy error", reservationNameID, "FetchReservationTracjLocally", serviceParameters.ClientID, groupName);
                    return new Models.Local.LocalResponseModel()
                    {
                        result = false,
                        responseMessage = "Failled to generate the proxy http client"
                    };
                }
                httpClient.DefaultRequestHeaders.Clear();
                string requestString = JsonConvert.SerializeObject(localRequest, Formatting.None);
                new LogHelper().Debug("web api url :- " + serviceParameters.LocalAPIURL + @"/local/FetchReservationTrackDetailStatus", reservationNameID, "FetchReservationTracjLocally", serviceParameters.ClientID, groupName);
                new LogHelper().Debug("web api request :- " + requestString, reservationNameID, "FetchReservationTracjLocally", serviceParameters.ClientID, groupName);
                var requestContent = new StringContent(requestString, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await httpClient.PostAsync(serviceParameters.LocalAPIURL + @"/local/FetchReservationTrackDetailStatus", requestContent);
                if (response != null)
                {
                    if (response.IsSuccessStatusCode)
                    {
                        string apiResponse = await response.Content.ReadAsStringAsync();
                        new LogHelper().Debug("web API response :- " + apiResponse, reservationNameID, "FetchReservationTracjLocally", serviceParameters.ClientID, groupName);
                        Models.Local.LocalResponseModel localResponse = JsonConvert.DeserializeObject<Models.Local.LocalResponseModel>(apiResponse);
                        return localResponse;
                    }
                    else
                    {
                        new LogHelper().Debug("Failled to fetch reservation track in local DB using web api due to HTTP error : " + response.ReasonPhrase, reservationNameID, "FetchReservationTracjLocally", serviceParameters.ClientID, groupName);
                        return new Models.Local.LocalResponseModel()
                        {
                            result = false,
                            responseMessage = response.ReasonPhrase
                        };
                    }
                }
                else
                {
                    new LogHelper().Debug("Failled to fetch reservation track in local DB using web api due to null returned from the local web api", reservationNameID, "FetchReservationTracjLocally", serviceParameters.ClientID, groupName);
                    return new Models.Local.LocalResponseModel()
                    {
                        result = false,
                        responseMessage = "Local web API returned null"
                    };
                }
            }
            catch (Exception ex)
            {
                new LogHelper().Error(ex, reservationNameID, "FetchReservationTracjLocally", serviceParameters.ClientID, groupName);
                return new Models.Local.LocalResponseModel()
                {
                    result = false,
                    responseMessage = "Generic Exception : " + ex.Message
                };
            }
        }

    }
}