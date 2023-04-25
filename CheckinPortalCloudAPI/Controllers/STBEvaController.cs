using CheckinPortalCloudAPI.Helper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;

namespace CheckinPortalCloudAPI.Controllers
{
    public class STBEvaController : ApiController
    {
        [System.Web.Http.HttpPost]
        [System.Web.Http.ActionName("GetAccessToken")]
        public async Task<Models.EVA.EVAResponseModel> GetAccessToken(Models.EVA.EVARequestModel evaRequest)
        {
            try
            {
                new LogHelper().Debug("Generating access token", "", "GetAccessToken", "API", "EVA");
                if (evaRequest != null
                    && !string.IsNullOrEmpty(evaRequest.ClientId)
                    && !string.IsNullOrEmpty(evaRequest.ClientSecert))
                {
                    var authenticationBytes = System.Text.Encoding.ASCII.GetBytes($"{evaRequest.ClientId}:{evaRequest.ClientSecert}");

                    Dictionary<string, string> requestHeader = new Dictionary<string, string>() { { "Authorization", $"Basic {Convert.ToBase64String(authenticationBytes)}" } };
                    Dictionary<string, string> formData = new Dictionary<string, string>() { { "grant_type", "client_credentials" } };
                    HttpResponseMessage responseMessage = await new Helper.Helper().ExecutePostAsync(evaRequest.webUrl,null,requestHeader, null, formData);

                    if (responseMessage.IsSuccessStatusCode)
                    {
                        string responseString = await responseMessage.Content.ReadAsStringAsync();
                        if (!string.IsNullOrEmpty(responseString))
                        {
                            try
                            {
                                new LogHelper().Debug("Access token generated successfully", "", "GetAccessToken", "API", "EVA");
                                return new Models.EVA.EVAResponseModel()
                                {
                                    Response = JsonConvert.DeserializeObject<Models.EVA.AccessTokenResponse>(responseString),
                                    result = true
                                };
                            }
                            catch (Exception ex) 
                            {
                                new LogHelper().Debug($"Request : - {JsonConvert.SerializeObject(evaRequest)}", "", "GetAccessToken", "API", "EVA");
                                new LogHelper().Debug($"Access token generation failled with reason : - {ex.Message}", "", "GetAccessToken", "API", "EVA");
                                return new Models.EVA.EVAResponseModel()
                                {
                                    ResponseCode = null,
                                    responseMessage = ex.Message,
                                    result = false
                                };
                            }
                        }
                        else
                        {
                            new LogHelper().Debug($"Request : - {JsonConvert.SerializeObject(evaRequest)}", "", "GetAccessToken", "API", "EVA");
                            new LogHelper().Debug($"Access token generation failled with reason : - Service returned blank", "", "GetAccessToken", "API", "EVA");
                            return new Models.EVA.EVAResponseModel()
                            {
                                ResponseCode = null,
                                responseMessage = "Service returned blank",
                                result = false
                            };
                        }
                    }
                    else
                    {
                        new LogHelper().Debug($"Request : - {JsonConvert.SerializeObject(evaRequest)}", "", "GetAccessToken", "API", "EVA");
                        new LogHelper().Debug($"Access token generation failled with reason : - {responseMessage.ReasonPhrase}", "", "GetAccessToken", "API", "EVA");
                        return new Models.EVA.EVAResponseModel()
                        {
                            ResponseCode = responseMessage.StatusCode.ToString(),
                            responseMessage = responseMessage.ReasonPhrase,
                            result = false
                        };
                    }
                }
                else
                {
                    new LogHelper().Debug($"Request : - {JsonConvert.SerializeObject(evaRequest)}", "", "GetAccessToken", "API", "EVA");
                    new LogHelper().Debug($"Access token generation failled with reason : - Client ID and Client Secret can not be blank", "", "GetAccessToken", "API", "EVA");
                    return new Models.EVA.EVAResponseModel()
                    {
                        result = false,
                        responseMessage = "Client ID and Client Secret can not be blank"
                    };
                }

               
            }
            catch (Exception ex)
            {
                new LogHelper().Debug($"Request : - {JsonConvert.SerializeObject(evaRequest)}", "", "GetAccessToken", "API", "EVA");
                new LogHelper().Debug($"Access token generation failled with reason : - {ex.Message}", "", "GetAccessToken", "API", "EVA");
                return new Models.EVA.EVAResponseModel()
                {
                    ResponseCode = null,
                    responseMessage = ex.Message,
                    result = false
                };
            }

        }

        [System.Web.Http.HttpPost]
        [System.Web.Http.ActionName("VisitorCheckin")]
        public async Task<Models.EVA.EVAResponseModel> VisitorCheckin(Models.EVA.EVARequestModel evaRequest)
        {
            try
            {
                new LogHelper().Debug("Guest checking in (EVA)", "", "VisitorCheckin", "API", "EVA");
                string token = evaRequest.accessToken;
                if (!string.IsNullOrEmpty(token))
                {
                    Models.EVA.VisitorCheckInRequest visitorCheckInRequest = JsonConvert.DeserializeObject<Models.EVA.VisitorCheckInRequest>(evaRequest.RequestObject.ToString());
                    //Models.EVA.VisitorCheckInRequest visitorCheckInRequest = (Models.EVA.VisitorCheckInRequest)evaRequest.RequestObject;
                    //System.IO.File.WriteAllText(System.Web.Hosting.HostingEnvironment.MapPath(@"~\Request.txt"), JsonConvert.SerializeObject(visitorCheckInRequest));
                    HttpResponseMessage responseMessage = await new Helper.Helper().ExecutePostAsync(evaRequest.webUrl, visitorCheckInRequest, null, token, null);

                    if (responseMessage.IsSuccessStatusCode)
                    {
                        string responseString = await responseMessage.Content.ReadAsStringAsync();
                        if (!string.IsNullOrEmpty(responseString))
                        {
                            try
                            {
                                new LogHelper().Debug($"Guest checked in successsfully in EVA", "", "GetAccessToken", "API", "EVA");
                                return new Models.EVA.EVAResponseModel()
                                {
                                    Response = JsonConvert.DeserializeObject<Models.EVA.VisitorCheckInResponse>(responseString),
                                    result = true
                                };
                            }
                            catch (Exception ex)
                            {
                                new LogHelper().Debug($"Request : - {JsonConvert.SerializeObject(evaRequest)}", "", "GetAccessToken", "API", "EVA");
                                new LogHelper().Debug($"Guest check in failed(EVA) : - {ex.Message}", "", "GetAccessToken", "API", "EVA");
                                return new Models.EVA.EVAResponseModel()
                                {
                                    ResponseCode = null,
                                    responseMessage = ex.Message,
                                    result = false
                                };
                            }
                        }
                        else
                        {
                            new LogHelper().Debug($"Request : - {JsonConvert.SerializeObject(evaRequest)}", "", "GetAccessToken", "API", "EVA");
                            new LogHelper().Debug($"Guest check in failed(EVA) : - Service returned blank", "", "GetAccessToken", "API", "EVA");
                            return new Models.EVA.EVAResponseModel()
                            {
                                ResponseCode = null,
                                responseMessage = "Service returned blank",
                                result = false
                            };
                        }
                    }
                    else if (responseMessage.Content != null)
                    {
                        string responseString = await responseMessage.Content.ReadAsStringAsync();
                        new LogHelper().Debug($"Request : - {JsonConvert.SerializeObject(evaRequest)}", "", "GetAccessToken", "API", "EVA");
                        new LogHelper().Debug($"Guest check in failed(EVA) : - {JsonConvert.DeserializeObject<Models.EVA.VisitorCheckInResponse>(responseString)}", "", "GetAccessToken", "API", "EVA");
                        return new Models.EVA.EVAResponseModel()
                        {
                            Response = JsonConvert.DeserializeObject<Models.EVA.VisitorCheckInResponse>(responseString),
                            result = false
                        };
                    }
                    else
                    {
                        new LogHelper().Debug($"Request : - {JsonConvert.SerializeObject(evaRequest)}", "", "GetAccessToken", "API", "EVA");
                        new LogHelper().Debug($"Guest check in failed(EVA) : - { responseMessage.ReasonPhrase}", "", "GetAccessToken", "API", "EVA");
                        return new Models.EVA.EVAResponseModel()
                        {
                            ResponseCode = responseMessage.StatusCode.ToString(),
                            responseMessage = responseMessage.ReasonPhrase,
                            result = false
                        };
                    }
                }
                else
                {
                    new LogHelper().Debug($"Request : - {JsonConvert.SerializeObject(evaRequest)}", "", "GetAccessToken", "API", "EVA");
                    new LogHelper().Debug($"Guest check in failed(EVA) : - Access token can not be blank", "", "GetAccessToken", "API", "EVA");
                    return new Models.EVA.EVAResponseModel()
                    {
                        result = false,
                        responseMessage = "Access token can not be blank"
                    };
                }

                

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

       



    }
}