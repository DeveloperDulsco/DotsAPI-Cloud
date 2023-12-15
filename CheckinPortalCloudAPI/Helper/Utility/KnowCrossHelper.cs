using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace CheckinPortalCloudAPI.Helper.Utility
{
    public class KnowCrossHelper
    {

        #region Instance
        private static readonly Lazy<KnowCrossHelper> lazy = new Lazy<KnowCrossHelper>(() => new KnowCrossHelper());
        public static KnowCrossHelper Instance => lazy.Value;
        #endregion

        public string generateAccessToken(Models.KnowCross.AccesstokenRequestModel requestModel)
        {
            try
            {
                if (string.IsNullOrEmpty(requestModel.apiBaseAddress) || string.IsNullOrEmpty(requestModel.public_key)
                    || string.IsNullOrEmpty(requestModel.private_key) || string.IsNullOrEmpty(requestModel.request_method_type))
                {
                    return null;
                }

                string requestUri = System.Web.HttpUtility.UrlEncode(WebRequest.Create(requestModel.apiBaseAddress).RequestUri.AbsoluteUri.ToLower());

                DateTime epochStart = new DateTime(1970, 01, 01, 0, 0, 0, 0, DateTimeKind.Utc);
                TimeSpan timeSpan = DateTime.UtcNow - epochStart;
                string requestTimeStamp = Convert.ToUInt64(timeSpan.TotalSeconds).ToString();
                string signatureRawData = String.Format("{0}{1}{2}{3}", requestModel.public_key, requestModel.request_method_type,
                requestUri, requestTimeStamp);
                var secretKeyByteArray = Encoding.UTF8.GetBytes(requestModel.private_key);
                byte[] signature = Encoding.UTF8.GetBytes(signatureRawData);
                string access_token = null;
                using (HMACSHA256 hmac = new HMACSHA256(secretKeyByteArray))
                {
                    byte[] signatureBytes = hmac.ComputeHash(signature);
                    string requestSignatureBase64String = Convert.ToBase64String(signatureBytes);
                    access_token = string.Format("{0}:{1}:{2}", requestModel.public_key, requestSignatureBase64String,
                    requestTimeStamp);
                }

                return access_token;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
