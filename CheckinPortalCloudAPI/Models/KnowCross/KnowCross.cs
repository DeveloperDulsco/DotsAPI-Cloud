using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckinPortalCloudAPI.Models.KnowCross
{
    public class AccesstokenRequestModel
    {
        public string apiBaseAddress { get; set; }
        public string public_key { get; set; }
        public string private_key { get; set; }
        public string request_method_type { get; set; }
    }
    public class UpdateLuggageTagRequestModel
    {
        public int PropertyID { get; set; }
        public string PMSReservationId { get; set; }
        public string ConfirmationId { get; set; }
        public int? GuestID { get; set; }
        public string BaggageTagValue { get; set; }
    }
    public class UpdateLuggageTagAPIRequestModel
    {
        public UpdateLuggageTagRequestModel TagRequestModel { get; set; }
        public string apiBaseAddress { get; set; }
        public string access_token { get; set; }


    }

    public class UpdateLuggageTagResponseModel
    {
        public bool HasError { get; set; }
        public List<ErrorResponse> Errors { get; set; }
        public string Result { get; set; }
    }

    public class ErrorResponse
    {
        public int? ErrorCode { get; set; }
        public string ErrorMessage { get; set; }
        public bool HasError { get; set; }
    }
}
