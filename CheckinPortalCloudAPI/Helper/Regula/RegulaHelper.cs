using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml;

namespace CheckinPortalCloudAPI.Helper.Regula
{
    public class RegulaHelper
    {
        private string BaseURL;
        public RegulaHelper(string baseURL)
        {
            BaseURL = baseURL;
        }
        public async Task<Models.Regula.LoginResponse> Authenticate(string userName, string password)
        {
            Models.Regula.LoginResponse loginResponse = new Models.Regula.LoginResponse()
            {
                IsLoggedIn = false
            };

            using (var client = new HttpClient())
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

                var userData = new Models.Regula.UserData() { UserId = userName, Password = password };
                var url = BaseURL + "Authentication/Authenticate";

                StringContent stringContent = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(userData), Encoding.UTF8, "application/json");

                var response = await client.PostAsync(url, stringContent);
                if (response.IsSuccessStatusCode)
                {
                    var token = response.Headers.GetValues("X-Token").FirstOrDefault();

                    if (!string.IsNullOrEmpty(token))
                    {
                        loginResponse.Token = token;
                        loginResponse.IsLoggedIn = true;
                    }
                }
            }

            return loginResponse;
        }


        public async Task<string> ReadDocument(Models.Regula.ReadDocumentModel readDocumentModel, string Token)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("X-Token", Token);

                var url = BaseURL + $"/Transaction2/SubmitTransaction?capabilities=508&authenticity=0";

                string jsonRequest = Newtonsoft.Json.JsonConvert.SerializeObject(readDocumentModel.Picture);
                StringContent jsonBodyString = new StringContent(jsonRequest, Encoding.UTF8, "application/json");



                var response = await client.PostAsync(url, jsonBodyString);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadAsStringAsync();
                }

                return string.Empty;
            }
        }

        public async Task<string> GetTransactionStatus(string transactionId, string Token)
        {
            using (var client = new HttpClient())
            {
                var url = BaseURL + $"/Transaction2/GetTransactionStatus?transactionId={transactionId}";
                client.DefaultRequestHeaders.Add("X-Token", Token);
                var response = await client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var jObject = Newtonsoft.Json.Linq.JObject.Parse(response.Content.ReadAsStringAsync().Result);
                    return jObject.GetValue("Status").ToString();
                }

            }
            return null;
        }

        public async Task<Models.Regula.ReadDocumentResponseModel> GetTransactionResult(string regulaTransactionID, string token)
        {
            using (var client = new HttpClient())
            {
                var url = BaseURL + $"/Transaction2/GetTransactionResult?transactionId={regulaTransactionID}&resultType=15";
                client.DefaultRequestHeaders.Add("X-Token", token);
                var Response = await client.GetAsync(url);
                if (Response.IsSuccessStatusCode)
                {
                    var ResponseString = await Response.Content.ReadAsStringAsync();
                    var documentResult = ProcessDocumentResponse(ResponseString,regulaTransactionID,token);
                    
                    //get image and update in 

                    return documentResult;
                }
            }
            return null;
        }

        public async Task<string> GetTransactionImages(string transactionId, string token)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("X-Token", token);
                var url = BaseURL + $"/Transaction2/GetImages?transactionId={transactionId}";
                var response = await client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    var result = response.Content.ReadAsStringAsync().Result;
                    return result.Equals("null") ? null : result;
                }
            }
            return null;
        }


        public Models.Regula.ReadDocumentResponseModel ProcessDocumentResponse(string TransactionResult,string transactionId,string token)
        {
            Models.Regula.ReadDocumentResponseModel returnResponse = new Models.Regula.ReadDocumentResponseModel();
            if (!string.IsNullOrWhiteSpace(TransactionResult) && !TransactionResult.Equals("null"))
            {
                TransactionResult = TransactionResult.TrimStart('[').TrimEnd(']').TrimStart('\"').TrimEnd('\"');
                TransactionResult = TransactionResult.Replace("\\n", string.Empty);
                TransactionResult = TransactionResult.Replace("\\", string.Empty);
                var o = new XmlDocument();
                o.LoadXml(TransactionResult);
                var noList = o.GetElementsByTagName("Document_Field_Analysis_Info");
                Dictionary<string, string> table = new Dictionary<string, string>();

                for (int i = 0; i < noList.Count; i++)
                {
                    var Item = (XmlElement)noList.Item(i);
                    int FieldType = Convert.ToInt32(Item.GetElementsByTagName("FieldType").Item(0).InnerText);
                    int LCID = 0;
                    if (FieldType > 0xFFFF)
                    {
                        LCID = FieldType >> 16;
                        FieldType = (FieldType << 16) >> 16;
                    }

                    string key = Models.Regula.eVisualFieldType.GetName(typeof(Models.Regula.eVisualFieldType), FieldType);
                    if (key != null)
                    {
                        if (LCID > 0)
                        {
                            key += string.Format("({0})", LCID);
                        }
                        string Value = Item.GetElementsByTagName("Field_MRZ").Item(0).InnerText;
                        if (string.IsNullOrEmpty(Value))
                        {
                            Value = Item.GetElementsByTagName("Field_Visual").Item(0).InnerText;
                        }
                        table.Add(key, Value);
                    }
                    //if (LCID > 0)
                    //{
                    //    key += string.Format("({0})", LCID);
                    //}
                    //string Value = Item.GetElementsByTagName("Field_MRZ").Item(0).InnerText;
                    //if (string.IsNullOrEmpty(Value))
                    //{
                    //    Value = Item.GetElementsByTagName("Field_Visual").Item(0).InnerText;
                    //}
                    //table.Add(key, Value);
                }
                returnResponse.result = true;
                returnResponse.idType = GetValue(table, "ft_Document_Class_Code");

                returnResponse.firstName = GetValue(table, "ft_Surname");
                returnResponse.middleName = GetValue(table, "ft_Middle_Name");
                returnResponse.lastName = GetValue(table, "ft_Given_Names");
                returnResponse.lastName = returnResponse.lastName == null ? GetValue(table, "ft_Surname_And_Given_Names") : returnResponse.lastName;
                returnResponse.gender = GetValue(table, "ft_Sex");
                if (returnResponse.gender != null)
                {
                    if (returnResponse.gender.Equals("M"))
                    {
                        returnResponse.genderAbrevation = "Male";
                    }
                    else if (returnResponse.gender.Equals("F"))
                    {
                        returnResponse.genderAbrevation = "Female";
                    }
                }

                returnResponse.dateOfBirth = GetValue(table, "ft_Date_of_Birth"); // ? GetValue(table,"ft_Date_of_Birth") : "";

                returnResponse.address1 = GetValue(table, "ft_Address_Flat");
                returnResponse.address2 = GetValue(table, "ft_Address_State");
                returnResponse.city = GetValue(table, "ft_Address_City");
                returnResponse.state = GetValue(table, "ft_Address_State");
                returnResponse.zip = GetValue(table, "ft_Address_Postal_Code");
                returnResponse.issueCountry = GetValue(table, "ft_Issuing_State_Code");
                returnResponse.issueCountry_code2 = "";//GetValue(table,"ft_Issuing_State_Code");
                returnResponse.issueCountry_fullname = GetValue(table, "ft_Issuing_State_Name");
                returnResponse.nationality = GetValue(table, "ft_Nationality_Code");
                returnResponse.nationality_code2 = ""; //GetValue(table,"ft_Sex"); 
                returnResponse.nationality_fullname = GetValue(table, "ft_Nationality");
                returnResponse.documentNumber = GetValue(table, "ft_Document_Number");
                returnResponse.personalNumber = GetValue(table, "ft_Personal_Number");
                returnResponse.issueDate = GetValue(table, "ft_Date_of_Issue");

                if (!string.IsNullOrEmpty(returnResponse.issueDate))
                {
                    DateTime issueDate;
                    if (DateTime.TryParseExact(returnResponse.issueDate, "M'/'d'/'yyyy", CultureInfo.InvariantCulture,
                                 DateTimeStyles.AllowLeadingWhite, out issueDate))
                    {
                        returnResponse.issueDate = issueDate.ToString("yyyy-MM-dd");
                    }
                }



                returnResponse.expiryDate = GetValue(table, "ft_Date_of_Expiry");

                if (!string.IsNullOrEmpty(returnResponse.expiryDate))
                {
                    DateTime expiryDate;
                    if (DateTime.TryParseExact(returnResponse.expiryDate, "M'/'d'/'yyyy", CultureInfo.InvariantCulture,
                                 DateTimeStyles.AllowLeadingWhite, out expiryDate))
                    {
                        returnResponse.expiryDate = expiryDate.ToString("yyyy-MM-dd");
                    }
                }

                returnResponse.optionalData1 = GetValue(table, "ft_Optional_Data");
                returnResponse.optionalData2 = "";
                returnResponse.personalEyeColor = GetValue(table, "ft_Eyes_Color");
                returnResponse.personalHeight = GetValue(table, "ft_Height");
                returnResponse.issuingPlace = GetValue(table, "ft_Place_of_Issue");
                returnResponse.placeOfBirth = GetValue(table, "ft_Place_of_Birth");
                returnResponse.fathersName = GetValue(table, "ft_Fathers_Name");
                returnResponse.mothersName = GetValue(table, "ft_Mothers_Name");
                returnResponse.countryOfBirth = GetValue(table, "ft_Optional_Data");
                returnResponse.spouseName = GetValue(table, "ft_Sponsor");
                if (returnResponse.spouseName != null)
                    returnResponse.martialStatus = "MARRIED";
                //switch (returnResponse.idType)
                //{
                //    case "P":
                //        returnResponse.idType = "P";
                //        returnResponse.documentTypeAbrevation = "Passport";
                //        break;

                //    case "DL":
                //        returnResponse.idType = "DL";
                //        returnResponse.documentTypeAbrevation = "Driving License";
                //        break;

                //    case "NID":
                //        returnResponse.idType = "NID";
                //        returnResponse.documentTypeAbrevation = "Identification Card";
                //        break;

                //    case "IL":
                //        returnResponse.idType = "NID";
                //        returnResponse.documentTypeAbrevation = "Identification Card";
                //        break;
                //    case "ID":
                //        returnResponse.idType = "ID";
                //        returnResponse.documentTypeAbrevation = "Identification Card";
                //        break;
                //    case "V":
                //        returnResponse.idType = "V";
                //        returnResponse.documentTypeAbrevation = "Visa";
                //        break;

                //    case "OC":
                //        returnResponse.idType = "OC";
                //        returnResponse.documentTypeAbrevation = "Unknown";
                //        break;

                //    case "RES":
                //        returnResponse.idType = "RES";
                //        returnResponse.documentTypeAbrevation = "Resident Permit";
                //        break;
                //}
                returnResponse.errorMessage = null;

                try
                {
                    if (!string.IsNullOrEmpty(returnResponse.dateOfBirth))
                    {
                        DateTime DOB = DateTime.ParseExact(returnResponse.dateOfBirth, "M'/'d'/'yyyy", CultureInfo.InvariantCulture);
                        var today = DateTime.Today;
                        // Calculate the age.
                        var age = today.Year - DOB.Year;
                        // Do stuff with it.
                        if (DOB > today.AddYears(-age))
                        {
                            age--;
                        }

                        returnResponse.dateOfBirth = DOB.ToString("yyyy-MM-dd");

                        returnResponse.age = age;
                    }
                }
                catch
                {

                }
                try
                {
                    if (!string.IsNullOrEmpty(returnResponse.expiryDate))
                    {
                        DateTime exdate = DateTime.ParseExact(returnResponse.expiryDate, "M'/'d'/'yyyy", CultureInfo.InvariantCulture);
                        returnResponse.IsExpired = exdate < DateTime.Now;
                    }
                }
                catch
                {
                    returnResponse.IsExpired = false;
                }

            }
            //var documentImage = GetTransactionImages(transactionId, token).Result;
            //returnResponse.fullImage = documentImage;
            //returnResponse.ac
            returnResponse.TransactionID = transactionId;
            return returnResponse;
        }

        private string GetValue(Dictionary<string, string> dictionary, string key)
        {
            string returnValue;
            if (!dictionary.TryGetValue(key, out returnValue))
            {
                returnValue = string.Empty;
            }
            return returnValue;
        }

        
    }

    
}