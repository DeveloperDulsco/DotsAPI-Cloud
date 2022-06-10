using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace CheckinPortalCloudAPI.Helper
{
    public class Helper
    {
        public static string Get8Digits()
        {
            var bytes = new byte[4];
            var rng = RandomNumberGenerator.Create();
            rng.GetBytes(bytes);
            uint random = BitConverter.ToUInt32(bytes, 0) % 100000000;
            return String.Format("{0:D8}", random);
        }

        public string EncryptString(string key, string plainText)
        {
            byte[] iv = new byte[16];
            byte[] array;

            using (Aes aes = Aes.Create())
            {
                aes.Key = Encoding.UTF8.GetBytes(key);
                aes.IV = iv;

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter streamWriter = new StreamWriter((Stream)cryptoStream))
                        {
                            streamWriter.Write(plainText);
                        }

                        array = memoryStream.ToArray();
                    }
                }
            }

            return Convert.ToBase64String(array);
        }

        public HttpClient getProxyClient(string groupName, string ProxyHost, string proxyUserName, string proxyPassword)
        {
            new LogHelper().Debug("assigning proxy credentials :- (host:" + ProxyHost + ",UN:" + proxyUserName + ",Password:" + proxyPassword + ")", null, "getProxyClient", "Grabber", groupName);
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
                new LogHelper().Debug("proxy credentials assigned", null, "getProxyClient", "Grabber", groupName);
                return new HttpClient(handler: httpClientHandler, disposeHandler: true);
            }
            catch (Exception ex)
            {
                new LogHelper().Error(ex, null, "getProxyClient", "Grabber", groupName);
                return null;
            }

        }

       

        public async Task<HttpResponseMessage> ExecutePostAsync(string web_url, object body = null, Dictionary<string, string> headers = null, string accesToken = null, Dictionary<string, string> formDataBody = null)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(web_url + (web_url.EndsWith("/") ? "" : "/"));

                    client.DefaultRequestHeaders.Clear();

                    if (headers != null && headers.Count > 0)
                    {
                        foreach (var header in headers)
                        {
                            client.DefaultRequestHeaders.Add(header.Key, header.Value);
                        }
                    }

                    
                    
                    if (!string.IsNullOrEmpty(accesToken))
                    {
                        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accesToken);
                    }
                    HttpResponseMessage response = new HttpResponseMessage();
                    if (body != null)
                    {

                        string jsondata = JsonConvert.SerializeObject(body);
                        StringContent content = new StringContent(jsondata, Encoding.UTF8, "application/json");
                        content.Headers.ContentType.CharSet = "";
                        response = await client.PostAsync(web_url, content);
                    }
                    else
                    {
                        if(formDataBody != null && formDataBody.Count > 0)
                        {
                            var req = new HttpRequestMessage(HttpMethod.Post, web_url);
                            req.Content = new FormUrlEncodedContent(formDataBody);
                            response = await client.SendAsync(req);
                        }
                        else
                            response = await client.PostAsync(web_url, null);
                    }

                    
                    return response;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}