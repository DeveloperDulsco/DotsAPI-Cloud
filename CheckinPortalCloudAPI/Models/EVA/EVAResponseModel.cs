using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;

namespace CheckinPortalCloudAPI.Models.EVA
{

    
    public class EVAResponseModel
    {
        public bool result { get; set; }
        public string responseMessage { get; set; }
        public object Response { get; set; }
        public string ResponseCode { get; set; }
    }

    public class AccessTokenResponse
    {
        [JsonProperty("refresh_token_expires_in", NullValueHandling = NullValueHandling.Ignore)]
        public string RefreshTokenExpiresIn { get; set; }

        [JsonProperty("api_product_list_json", NullValueHandling = NullValueHandling.Ignore)]
        public List<string> ApiProductListJson { get; set; }

        [JsonProperty("organization_name", NullValueHandling = NullValueHandling.Ignore)]
        public string OrganizationName { get; set; }

        [JsonProperty("developer.email", NullValueHandling = NullValueHandling.Ignore)]
        public string DeveloperEmail { get; set; }

        [JsonProperty("token_type", NullValueHandling = NullValueHandling.Ignore)]
        public string TokenType { get; set; }

        [JsonProperty("issued_at", NullValueHandling = NullValueHandling.Ignore)]
        public string IssuedAt { get; set; }

        [JsonProperty("client_id", NullValueHandling = NullValueHandling.Ignore)]
        public string ClientId { get; set; }

        [JsonProperty("access_token", NullValueHandling = NullValueHandling.Ignore)]
        public string AccessToken { get; set; }

        [JsonProperty("application_name", NullValueHandling = NullValueHandling.Ignore)]
        public string ApplicationName { get; set; }

        [JsonProperty("expires_in", NullValueHandling = NullValueHandling.Ignore)]
        public string ExpiresIn { get; set; }

        [JsonProperty("refresh_count", NullValueHandling = NullValueHandling.Ignore)]
        public string RefreshCount { get; set; }

        [JsonProperty("status", NullValueHandling = NullValueHandling.Ignore)]
        public string Status { get; set; }
    }

    #region Visitor Check-in Response
    public class VisitorCheckInResponse
    {
        [JsonProperty("data", NullValueHandling = NullValueHandling.Ignore)]
        public VisitorCheckInResponseData Data { get; set; }

        [JsonProperty("createdAt", NullValueHandling = NullValueHandling.Ignore)]
        public DateTime? CreatedAt { get; set; }

        [JsonProperty("status", NullValueHandling = NullValueHandling.Ignore)]
        public VisitorCheckInResponseStatus Status { get; set; }
    }

    public class VisitorCheckInResponseData
    {
        [JsonProperty("errorCodeList", NullValueHandling = NullValueHandling.Ignore)]
        public List<string> ErrorCodeList { get; set; }

        [JsonProperty("errorMsg", NullValueHandling = NullValueHandling.Ignore)]
        public string Message { get; set; }

        [JsonProperty("responseDateTime", NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(DateTimeFormatConverter), "yyyyMMddHHmmss")]
        public DateTime? ResponseDateTime { get; set; }

        [JsonProperty("resultCode", NullValueHandling = NullValueHandling.Ignore)]
        public int? ResultCode { get; set; }

        [JsonProperty("stbEvaTransactionId", NullValueHandling = NullValueHandling.Ignore)]
        public string TransactionId { get; set; }
    }

    public class VisitorCheckInResponseStatus
    {
        [JsonProperty("code", NullValueHandling = NullValueHandling.Ignore)]
        public int? Code { get; set; }

        [JsonProperty("message", NullValueHandling = NullValueHandling.Ignore)]
        public string Message { get; set; }

        [JsonProperty("errorCodeList", NullValueHandling = NullValueHandling.Ignore)]
        public List<string> ErrorCodeList { get; set; }

        [JsonProperty("debugId", NullValueHandling = NullValueHandling.Ignore)]
        public string DebugId { get; set; }
        
        
    }
    #endregion
}