using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using System.Xml.Serialization;

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
            new LogHelper().Log("assigning proxy credentials :- (host:" + ProxyHost + ",UN:" + proxyUserName + ",Password:" + proxyPassword + ")", null, "getProxyClient", "Grabber", groupName);
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
                new LogHelper().Log("proxy credentials assigned", null, "getProxyClient", "Grabber", groupName);
                return new HttpClient(handler: httpClientHandler, disposeHandler: true);
            }
            catch (Exception ex)
            {
                new LogHelper().Error(ex, null, "getProxyClient", "Grabber", groupName);
                return null;
            }

        }

        private XmlDocument getSecurityHeaderXML(string UserName, string Password)
        {
            XmlDocument Xdoc = new XmlDocument();
            Models.Header.Security SecurityXMl = new Models.Header.Security();
            Models.Header.UsernameToken UNToken = new Models.Header.UsernameToken();
            Models.Header.Password Pass = new Models.Header.Password();
            
            Pass.Text = Password;
            Pass.Type = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-username-token-profile-1.0#PasswordText";
            UNToken.Password = Pass;
            
            UNToken.Username = UserName;
            SecurityXMl.UsernameToken = UNToken;
            SecurityXMl.Wsse = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd";
            SecurityXMl.Wsu = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd";
            Models.Header.Timestamp TStamp = new Models.Header.Timestamp();
            TStamp.Created = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss") + "Z";
            TStamp.Expires = DateTime.UtcNow.AddMinutes(5).ToString("yyyy-MM-ddTHH:mm:ss") + "Z";
            SecurityXMl.Timestamp = TStamp;
            XmlSerializer ser = new XmlSerializer(SecurityXMl.GetType());
            using (MemoryStream memStm = new MemoryStream())
            {
                ser.Serialize(memStm, SecurityXMl);

                memStm.Position = 0;

                XmlReaderSettings Readersettings = new XmlReaderSettings();
                Readersettings.IgnoreWhitespace = true;

                using (var xtr = XmlReader.Create(memStm, Readersettings))
                {
                    Xdoc = new XmlDocument();
                    Xdoc.Load(xtr);
                }
            }
            Xdoc.RemoveChild(Xdoc.FirstChild);
            return Xdoc;
        }

        private XmlDocument getOGHeaderXML(string HotelDoman, string Username, string Password,string DestinationEntityID,string DestinationSystemType, string SourceEntityID, string SourceSystemType)
        {
            XmlDocument Xdoc = new XmlDocument();
            Models.Header.OGHeader OGHeader = new Models.Header.OGHeader();
            Models.Header.Authentication Authentication = new Models.Header.Authentication();
            Authentication.Xmlns = "http://webservices.micros.com/og/4.3/Core/";
            Models.Header.UserCredentials UserCredentials = new Models.Header.UserCredentials();
            //UserCredentials.Domain = "OWS";
            UserCredentials.Domain = HotelDoman;//"CLA";
            UserCredentials.UserName = Username;//"CLA_SAMSOTECH@OHC01-OHO-PROD-AD.OHRC";
            UserCredentials.UserPassword = Password;// "c9eS2iFkAP!";
            Authentication.UserCredentials = UserCredentials;
            Authentication.Xmlns = "http://webservices.micros.com/og/4.3/Core/";
            OGHeader.Authentication = Authentication;
            Models.Header.Destination Destination = new Models.Header.Destination();
            Destination.EntityID = DestinationEntityID;//"KIOSK";
            Destination.SystemType = DestinationSystemType;//"PMS";
            OGHeader.Destination = Destination;
            Models.Header.Origin Orgin = new Models.Header.Origin();
            Orgin.EntityID = SourceEntityID;//"KIOSK";
            Orgin.SystemType = SourceSystemType;// "KIOSK";
            OGHeader.Origin = Orgin;
            XmlSerializer ser = new XmlSerializer(OGHeader.GetType());
            //XmlDocument xd = null;

            using (MemoryStream memStm = new MemoryStream())
            {
                ser.Serialize(memStm, OGHeader);

                memStm.Position = 0;

                XmlReaderSettings Readersettings = new XmlReaderSettings();
                Readersettings.IgnoreWhitespace = true;

                using (var xtr = XmlReader.Create(memStm, Readersettings))
                {
                    Xdoc = new XmlDocument();
                    Xdoc.Load(xtr);
                }
            }
            Xdoc.RemoveChild(Xdoc.FirstChild);
            return Xdoc;
        }

        public CustomHeader getSecurityHeader(string UserName, string Password)
        {
            CustomHeader securityHeader = new CustomHeader(getSecurityHeaderXML(UserName, Password), "wsse:Security", "");
            List<CusttomHeaderAttributes> attributesList = new List<CusttomHeaderAttributes>();
            CusttomHeaderAttributes CustomAttributes = new CusttomHeaderAttributes();
            CustomAttributes.AttributPrefix = "xmlns";
            CustomAttributes.AttributeLocalName = "wsse";
            CustomAttributes.Attributens = null;
            CustomAttributes.Value = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd";
            attributesList.Add(CustomAttributes);
            CustomAttributes = new CusttomHeaderAttributes();
            CustomAttributes.AttributPrefix = "xmlns";
            CustomAttributes.AttributeLocalName = "wsu";
            CustomAttributes.Attributens = null;
            CustomAttributes.Value = "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd";
            attributesList.Add(CustomAttributes);
            securityHeader.Attributes = attributesList;
            return securityHeader;
        }

        public CustomHeader GetOGHeader(string HotelDoman, string Username, string Password, string DestinationEntityID, string DestinationSystemType, string SourceEntityID, string SourceSystemType)
        {
            CustomHeader OgHeader = new CustomHeader(getOGHeaderXML(HotelDoman, Username, Password, DestinationEntityID, DestinationSystemType, SourceEntityID,
                   SourceSystemType), "OGHeader", "http://webservices.micros.com/og/4.3/Core/");
            List<CusttomHeaderAttributes> attributesList = new List<CusttomHeaderAttributes>();
            CusttomHeaderAttributes CustomAttributes = new CusttomHeaderAttributes();
            CustomAttributes.AttributPrefix = null;
            CustomAttributes.AttributeLocalName = "transactionID";
            CustomAttributes.Attributens = null;
            CustomAttributes.Value = Get8Digits();//"3297907325" // RandomNumber
            attributesList.Add(CustomAttributes);
            CustomAttributes = new CusttomHeaderAttributes();
            CustomAttributes.AttributPrefix = null;
            CustomAttributes.AttributeLocalName = "timeStamp";
            CustomAttributes.Attributens = null;
            CustomAttributes.Value = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss");
            attributesList.Add(CustomAttributes);
            OgHeader.Attributes = attributesList;
            return OgHeader;
        }

        public async Task<HttpResponseMessage> ExecutePostAsync(string web_url, object body = null, Dictionary<string, string> headers = null, string accesToken = null, Dictionary<string, string> formDataBody = null)
        {
            try
            {
                HttpClient httpClient = null;
                if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["IsEVAProxyEnabled"]) && (Convert.ToBoolean(ConfigurationManager.AppSettings["IsEVAProxyEnabled"])))
                {
                    httpClient = new Helper().getProxyClient("EVA", ConfigurationManager.AppSettings["EVAProxyURL"], ConfigurationManager.AppSettings["EVAProxyUN"],
                        ConfigurationManager.AppSettings["EVAProxyPSWD"]);
                }
                else
                    httpClient = new HttpClient();

                //using (var client = new HttpClient())
                {
                    httpClient.BaseAddress = new Uri(web_url + (web_url.EndsWith("/") ? "" : "/"));

                    httpClient.DefaultRequestHeaders.Clear();

                    if (headers != null && headers.Count > 0)
                    {
                        foreach (var header in headers)
                        {
                            httpClient.DefaultRequestHeaders.Add(header.Key, header.Value);
                        }
                    }

                    
                    
                    if (!string.IsNullOrEmpty(accesToken))
                    {
                        httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accesToken);
                    }
                    HttpResponseMessage response = new HttpResponseMessage();
                    if (body != null)
                    {

                        string jsondata = JsonConvert.SerializeObject(body);
                        StringContent content = new StringContent(jsondata, Encoding.UTF8, "application/json");
                        content.Headers.ContentType.CharSet = "";
                        response = await httpClient.PostAsync(web_url, content);
                    }
                    else
                    {
                        if(formDataBody != null && formDataBody.Count > 0)
                        {
                            var req = new HttpRequestMessage(HttpMethod.Post, web_url);
                            req.Content = new FormUrlEncodedContent(formDataBody);
                            response = await httpClient.SendAsync(req);
                        }
                        else
                            response = await httpClient.PostAsync(web_url, null);
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