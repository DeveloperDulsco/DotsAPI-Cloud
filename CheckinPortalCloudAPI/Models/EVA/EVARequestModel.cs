using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;



namespace CheckinPortalCloudAPI.Models.EVA
{
    public class EVARequestModel
    {
        public string ClientId { get; set; }
        public string ClientSecert { get; set; }
        public string webUrl { get; set; }
        public string accessToken { get; set; }
        public object RequestObject { get; set; }
    }

    internal class DateTimeFormatConverter : IsoDateTimeConverter
    {
        public DateTimeFormatConverter(string format)
        {
            DateTimeFormat = format;
        }
    }

    internal class GenderConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return true;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            List<string> male = new List<string>() { "m", "male" };
            List<string> female = new List<string>() { "f", "female" };
            string gender = ((string)value).ToLower();

            writer.WriteValue(
                male.Any(s => gender.Equals(s)) ? "M"
                : female.Any(s => gender.Equals(s)) ? "F"
                : "U");
        }
    }

    #region Visitor Check-in Request
    public class VisitorCheckInRequest
    {
        [JsonProperty("visitor", NullValueHandling = NullValueHandling.Ignore)]
        public Visitor Visitor { get; set; }
    }

    public class Visitor
    {
        [JsonProperty("info", NullValueHandling = NullValueHandling.Ignore)]
        public VisitorInfo Info { get; set; }

        [JsonProperty("hotelDetails", NullValueHandling = NullValueHandling.Ignore)]
        public HotelDetails Hotel { get; set; }

        [JsonProperty("images", NullValueHandling = NullValueHandling.Ignore)]
        public List<DocImage> Images { get; set; }
    }

    public class VisitorInfo
    {
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        [JsonProperty("nationality", NullValueHandling = NullValueHandling.Ignore)]
        public string Nationality { get; set; }

        [JsonProperty("gender", NullValueHandling = NullValueHandling.Ignore)]
        
        public string Gender { get; set; }

        [JsonProperty("passportNumber", NullValueHandling = NullValueHandling.Ignore)]
        public string PassportNumber { get; set; }

        //[JsonProperty("dateOfBirth", NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(DateTimeFormatConverter), "yyyyMMdd")]
        public DateTime? DateOfBirth { get; set; }

        [JsonProperty("passportType", NullValueHandling = NullValueHandling.Ignore)]
        public string PassportType { get; set; }

        //[JsonProperty("checkIn", NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(DateTimeFormatConverter), "yyyyMMdd")]
        public DateTime? CheckIn { get; set; }

        //[JsonProperty("checkOut", NullValueHandling = NullValueHandling.Ignore)]
        [JsonConverter(typeof(DateTimeFormatConverter), "yyyyMMdd")]
        public DateTime? CheckOut { get; set; }

        [JsonProperty("mrzData", NullValueHandling = NullValueHandling.Ignore)]
        public string MRZ { get; set; }

        [JsonProperty("manuallyInputted")]
        public bool ManuallyInputted { get; set; }
    }

    public class HotelDetails
    {
        [JsonProperty("bookingId", NullValueHandling = NullValueHandling.Ignore)]
        public string BookingId { get; set; }

        [JsonProperty("hotelCode", NullValueHandling = NullValueHandling.Ignore)]
        public string HotelCode { get; set; }

        [JsonProperty("kioskId", NullValueHandling = NullValueHandling.Ignore)]
        public string KioskId { get; set; }

        [JsonProperty("kioskUserId", NullValueHandling = NullValueHandling.Ignore)]
        public string KioskUserId { get; set; }

        [JsonProperty("staffId", NullValueHandling = NullValueHandling.Ignore)]
        public string StaffId { get; set; }

        [JsonProperty("staffName", NullValueHandling = NullValueHandling.Ignore)]
        public string StaffName { get; set; }
    }

    public class DocImage
    {
        [JsonProperty("prefix_name", NullValueHandling = NullValueHandling.Ignore)]
        public string PrefixName { get; set; }

        [JsonProperty("base64_data", NullValueHandling = NullValueHandling.Ignore)]
        public string Base64Data { get; set; }
    }
    #endregion
}