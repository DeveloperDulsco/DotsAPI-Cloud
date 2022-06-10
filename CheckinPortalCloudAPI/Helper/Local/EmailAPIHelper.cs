using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace CheckinPortalCloudAPI.Helper.Local
{
    public class EmailAPIHelper
    {
        public static async Task<Models.Email.EmailResponse> SendEmail(string baseURL, Models.Email.EmailRequest emailRequest)
        {
            try
            {
                HttpClient httpClient = new HttpClient();
                httpClient.BaseAddress = new Uri(baseURL);
                httpClient.DefaultRequestHeaders.Clear();
                string requestString = JsonConvert.SerializeObject(emailRequest, Formatting.None);
                var requestContent = new StringContent(requestString, Encoding.UTF8, "application/json");
                HttpResponseMessage response = await httpClient.PostAsync(baseURL + @"/email/SendEmail", requestContent);
                if (response != null)
                {
                    if (response.IsSuccessStatusCode)
                    {
                        Models.Email.EmailResponse emailResponse = JsonConvert.DeserializeObject<Models.Email.EmailResponse>(await response.Content.ReadAsStringAsync());
                        return emailResponse;
                    }
                    else
                    {
                        return new Models.Email.EmailResponse()
                        {

                            result = false,
                            responseMessage = "HTTP Error"
                        };
                    }
                }
                else
                {
                    return new Models.Email.EmailResponse()
                    {

                        result = false,
                        responseMessage = "Email gateway returned blank"
                    };
                }
            }
            catch (Exception ex)
            {
                return new Models.Email.EmailResponse()
                {

                    result = true,
                    responseMessage = "Generic Exception : " + ex.Message
                };
            }
        }
    }
}