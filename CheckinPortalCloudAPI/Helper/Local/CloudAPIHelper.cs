using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace CheckinPortalCloudAPI.Helper.Local
{
    public class CloudAPIHelper
    {
        public static async Task<Models.Cloud.CloudResponseModel> PushRecordToCloud(string baseURL, Models.Cloud.CloudRequestModel cloudRequest)
        {
            try
            {

                HttpClient httpClient = getProxyClient();
                httpClient.BaseAddress = new Uri(baseURL);
                httpClient.DefaultRequestHeaders.Clear();


                string requestString = JsonConvert.SerializeObject(cloudRequest, Formatting.None);
                baseURL = baseURL + @"/cloud/PushReservationDetails";
                //System.IO.File.AppendAllLines("log.txt", new List<string>() { "url :- " + baseURL + " request :- " + requestString });
                var requestContent = new StringContent(requestString, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await httpClient.PostAsync(baseURL, requestContent);
                if (response != null)
                {
                    if (response.IsSuccessStatusCode)
                    {
                        Models.Cloud.CloudResponseModel owsResponse = JsonConvert.DeserializeObject<Models.Cloud.CloudResponseModel>(await response.Content.ReadAsStringAsync());
                        return owsResponse;
                    }
                    else
                    {
                        return new Models.Cloud.CloudResponseModel()
                        {

                            result = false,
                            responseMessage = "HTTP Error"
                        };
                    }
                }
                else
                {
                    return new Models.Cloud.CloudResponseModel()
                    {

                        result = false,
                        responseMessage = "Service returned blank"
                    };
                }
            }
            catch (Exception ex)
            {
                return new Models.Cloud.CloudResponseModel()
                {

                    result = false,
                    responseMessage = "Generic Exception : " + ex.Message
                };
            }
        }

        static HttpClient getProxyClient()
        {
            try
            {
                HttpClientHandler handler = new HttpClientHandler();
                handler.UseDefaultCredentials = true;


                var proxy = new WebProxy
                {
                    Address = new Uri($"http://rtputm01.myfairmont.com:8080"),
                    BypassProxyOnLocal = false,
                    UseDefaultCredentials = false,

                    Credentials = new NetworkCredential(
                    userName: @"CPH\_rtpfps",
                    password: "IT$upp0rt")
                };

                //        // Now create a client handler which uses that proxy
                var httpClientHandler = new HttpClientHandler
                {
                    Proxy = proxy,
                };


                return new HttpClient(handler: httpClientHandler, disposeHandler: true);
                //return new HttpClient();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}