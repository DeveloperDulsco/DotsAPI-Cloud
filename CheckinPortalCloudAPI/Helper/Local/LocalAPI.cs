using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace CheckinPortalCloudAPI.Helper.Local
{
    public class LocalAPI
    {
        public  Models.Local.LocalResponseModel PushReservationDetails(string reservations)
        {
            try
            {

                List<Models.Local.OperaReservation> Localreservations = JsonConvert.DeserializeObject <List<Models.Local.OperaReservation>>(reservations);
                //foreach(Models.OWS.OperaReservation operaReservation1 in reservations)
                //{
                //    Models.Local.OperaReservation reservation = new Models.Local.OperaReservation();
                //    reservation = (Models.OWS.OperaReservation)operaReservation1;

                //}
                //HttpResponseMessage response = await httpClient.PostAsync($"/v52/payments", requestContent);
                List<Models.Local.DB.OperaReservationDataTableModel> operaReservationDataTables = new List<Models.Local.DB.OperaReservationDataTableModel>();
                List<Models.Local.DB.ProfileDetailsDataTableModel> profileDetailsDataTables = new List<Models.Local.DB.ProfileDetailsDataTableModel>();
                

                foreach (Models.Local.OperaReservation operaReservation in Localreservations)
                {
                    LocalDBModelConverter dBModelConverter = new LocalDBModelConverter();
                    operaReservationDataTables.Add(dBModelConverter.getOperaReservationDataTable(operaReservation));
                    foreach (Models.Local.GuestProfile guestProfile in operaReservation.GuestProfiles)
                    {
                        profileDetailsDataTables.Add(dBModelConverter.getprofileDetailsDataTable(guestProfile, operaReservation.ReservationNameID));
                    }
                }
                if (DBHelper.Instance.InsertReservationDetails(operaReservationDataTables, profileDetailsDataTables, new List<Models.Local.DB.ProfileDocumentDetailsModel>(), false, ConfigurationManager.AppSettings["LocalConnectionString"]))
                {
                    return new Models.Local.LocalResponseModel()
                    {
                        result = true,
                        responseMessage = "Success",
                        statusCode = 101
                    };
                }
                else
                    return new Models.Local.LocalResponseModel()
                    {
                        result = false,
                        responseMessage = "Failled to insert the data",
                        statusCode = -1
                    };
            }
            catch (Exception ex)
            {
                return new Models.Local.LocalResponseModel()
                {
                    result = false,
                    responseMessage = ex.Message,
                    statusCode = -1
                };
            }

        }

        public async Task<Models.Local.LocalResponseModel> FetchPreCheckedInReservation(Models.Local.FetchReservationRequest fetchReservationRequest)
        {

            #region Variables
            List<Models.Cloud.OperaReservation> PrecheckedinReservationList = null;
            List<Models.Local.ProfileDocuments> profileDocuments = null;
            List<Models.OWS.OperaReservation> operaReservations = null;
            Models.Local.PaymentDetails paymentDetails = null;
            string RegcardBase64 = null;
            #endregion

            try
            {

                #region Fetching pre-checkein records
                new LogHelper().Log("Fetching pre checked-in reservation", null, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                Models.Cloud.CloudResponseModel cloudResponse = await new WSClientHelper().FetchPrechedinRecord(new Models.Cloud.CloudRequestModel() { RequestObject = fetchReservationRequest }, fetchReservationRequest.ServiceParameters);
                if (!cloudResponse.result)
                {
                    new LogHelper().Log("Failled to fetch pre checked-in reservation with reason :- " + cloudResponse.responseMessage, null, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                    return new Models.Local.LocalResponseModel()
                    {
                        result = false,
                        responseMessage = "Failled to fetch pre checked-in reservation with reason :- " + cloudResponse.responseMessage
                    };
                }
                if (cloudResponse.responseData == null)
                {
                    new LogHelper().Log("Failled to fetch pre checked-in reservation with reason :- API response data is NULL" + cloudResponse.responseMessage, null, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                    return new Models.Local.LocalResponseModel()
                    {
                        result = false,
                        responseMessage = "Failled to fetch pre checked-in reservation with reason :- API response data is NULL" + cloudResponse.responseMessage
                    };
                }
                new LogHelper().Debug("Converting API json to object", null, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                try
                {
                    PrecheckedinReservationList = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Models.Cloud.OperaReservation>>(cloudResponse.responseData.ToString());
                }
                catch (Exception ex)
                {
                    new LogHelper().Error(ex, null, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                    new LogHelper().Log("Failled to covert API response to object", null, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                    return new Models.Local.LocalResponseModel()
                    {
                        result = false,
                        responseMessage = ex.Message
                    };
                }
                new LogHelper().Log("Pre checked-in reservation fetched", null, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");

                #endregion

                #region Iterating Pre checked in reservations
                new LogHelper().Log("Iterating reservation list, Pre checked-in reservation count : - " + PrecheckedinReservationList != null && PrecheckedinReservationList.Count >= 0 ?
                                                                PrecheckedinReservationList.Count.ToString() : "", null, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                if (PrecheckedinReservationList != null)
                {
                    foreach (Models.Cloud.OperaReservation reservation in PrecheckedinReservationList)
                    {
                        new LogHelper().Log("Processing Reservation No. : " + reservation.ReservationNumber, reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");

                        #region Updating Locally
                        new LogHelper().Log("Pushing reservation to Local DB", reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                        //reservation.IsEmailSend = false;
                        Models.Local.LocalResponseModel localResponse = await new WSClientHelper().PushRecordLocally(new Models.Local.LocalRequestModel()
                        {
                            RequestObject = new List<Models.Cloud.OperaReservation>() { reservation },
                            SyncFromCloud = true
                        }, reservation.ReservationNameID, "pre checked-in fetch", fetchReservationRequest.ServiceParameters);
                        if (!localResponse.result)
                        {
                            new LogHelper().Log("Failled to push pre checked-in reservation with reason :- " + localResponse.responseMessage, reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                            return new Models.Local.LocalResponseModel()
                            {
                                result = true,
                                responseMessage = "Failled to push pre checked-in reservation with reason :- " + localResponse.responseMessage
                            };
                        }

                        new LogHelper().Log("Reservation successfully updated in Local DB", reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                        #endregion

                        #region Sending Email
                        new LogHelper().Log("Sending confirmation email", reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                        if (!string.IsNullOrEmpty(reservation.GuestProfiles[0].Email[0].email))
                        {
                            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
                            Models.Email.EmailResponse emailResponse = await new WSClientHelper().SendEmail(reservation.ReservationNameID, new Models.Email.EmailRequest()
                            {
                                FromEmail = fetchReservationRequest.ServiceParameters.PreArrivalConfirmationEmail,
                                ToEmail = reservation.GuestProfiles[0].Email[0].email,
                                GuestName = "" + (!string.IsNullOrEmpty(reservation.GuestProfiles[0].FirstName) ? textInfo.ToTitleCase(reservation.GuestProfiles[0].FirstName) + " " : "")
                                                + (!string.IsNullOrEmpty(reservation.GuestProfiles[0].MiddleName) ? textInfo.ToTitleCase(reservation.GuestProfiles[0].MiddleName) + " " : "")
                                                + (!string.IsNullOrEmpty(reservation.GuestProfiles[0].LastName) ? textInfo.ToTitleCase(reservation.GuestProfiles[0].LastName) : ""),
                                
                                Subject = fetchReservationRequest.ServiceParameters.PreArrivalConfirmationEmailSubject,
                                confirmationNumber = reservation.ReservationNumber,
                                displayFromEmail = fetchReservationRequest.ServiceParameters.EmailDisplayName,
                                EmailType = Models.Email.EmailType.CheckinConfirmation,
                                ReservationNumber = reservation.ReservationNumber,
                                ArrivalDate = reservation.ArrivalDate.Value.ToString("dd-MMM-yyyy"),
                                DepartureDate = reservation.DepartureDate.Value.ToString("dd-MMM-yyy")

                            }, "pre checked-in fetch", fetchReservationRequest.ServiceParameters);
                            if (!emailResponse.result)
                            {
                                new LogHelper().Log("Failled to send confirmation email with reason :- " + emailResponse.responseMessage, reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                                new LogHelper().Warn("Failled to send confirmation email with reason :- " + emailResponse.responseMessage, reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                            }
                            else
                                new LogHelper().Log("Email send successfully", reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                        }
                        else
                        {
                            new LogHelper().Log("Failled to send confirmation email since email address not found from pre checked in list response", reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                            new LogHelper().Warn("Failled to send confirmation email since email address not found from pre checked in list response", reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                        }
                        #endregion

                        #region Fetching documents
                        new LogHelper().Log("Fetching profile documents", reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");

                        cloudResponse = await new WSClientHelper().FetchDocumentDetails(reservation.ReservationNameID, new Models.Cloud.CloudRequestModel()
                        {
                            RequestObject = reservation.ReservationNameID
                        }, "pre checked-in fetch", fetchReservationRequest.ServiceParameters);
                        if (!cloudResponse.result)
                        {
                            new LogHelper().Log("Failled to fetch profile documents with reason :- " + cloudResponse.responseMessage, reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                            new LogHelper().Warn("Failled to fetch profile documents with reason :- " + cloudResponse.responseMessage, reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                        }

                        if (cloudResponse.responseData == null)
                        {
                            new LogHelper().Log("Failled to fetch profile documents with reason :- API response data is NULL" + cloudResponse.responseMessage, reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                            new LogHelper().Warn("Failled to fetch profile documents with reason :- API response data is NULL" + cloudResponse.responseMessage, reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                        }
                        else
                        {
                            new LogHelper().Debug("Converting API json to object", reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                            try
                            {
                                profileDocuments = JsonConvert.DeserializeObject<List<Models.Local.ProfileDocuments>>(cloudResponse.responseData.ToString());
                                new LogHelper().Log("Profile documents fetched successfully", reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                            }
                            catch (Exception ex)
                            {
                                new LogHelper().Error(ex, reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                                new LogHelper().Log("Failled to covert API response to object", reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                                new LogHelper().Warn("Failled to fetch profile documents with reason :- " + ex.Message, reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                                new LogHelper().Debug("Failled to fetch profile documents with reason :- " + ex.Message, reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                            }

                        }
                        #endregion

                        #region Updating Profile documents
                        new LogHelper().Log("Pushing profile documents", reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                        localResponse = await new WSClientHelper().InsertDocuments(reservation.ReservationNameID, new Models.Local.LocalRequestModel()
                        {
                            RequestObject = cloudResponse.responseData,
                            SyncFromCloud = true

                        }, "pre checked-in fetch", fetchReservationRequest.ServiceParameters);
                        if (!localResponse.result)
                        {
                            new LogHelper().Log("Failled to pushing profile documents with reason :- " + localResponse.responseMessage, reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                            new LogHelper().Warn("Failled to pushing profile documents with reason :- " + localResponse.responseMessage, reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                        }
                        else
                            new LogHelper().Log("Profile documents updated in Local DB successfully", reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                        #endregion

                        #region Pushing Guest Signature to Local DB
                        if (!string.IsNullOrEmpty(reservation.GuestSignature))
                        {
                            try
                            {
                                new LogHelper().Log("Pushing guest signature to Local DB", reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                                byte[] guestSignature = null;
                                guestSignature = Convert.FromBase64String(reservation.GuestSignature);
                                localResponse = await new WSClientHelper().InsertReservationDocuments(reservation.ReservationNameID, new Models.Local.LocalRequestModel()
                                {
                                    RequestObject = new List<Models.Local.ReservationDocumentsDataTableModel>()
                                {
                                    new Models.Local.ReservationDocumentsDataTableModel()
                                    {
                                        Document = guestSignature,
                                        DocumentType = "Signature",
                                        ReservationNameID = reservation.ReservationNameID
                                    }
                                }
                                }, "pre checked-in fetch", fetchReservationRequest.ServiceParameters);
                                if (!localResponse.result)
                                {
                                    new LogHelper().Log("Failled to push guest signature with reason :- " + localResponse.responseMessage, reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                                    new LogHelper().Warn("Failled to push guest signature with reason :- " + localResponse.responseMessage, reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                                }
                                else
                                {
                                    new LogHelper().Log("Signature updated successfully", reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                                }
                            }
                            catch (Exception exc)
                            {
                                new LogHelper().Error(exc, reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                            }
                        }
                        #endregion

                        #region Updating Opera profile
                        try
                        {
                            new LogHelper().Log("Updating opera profile", reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                            if (reservation.GuestProfiles != null && reservation.GuestProfiles.Count > 0)
                            {
                                int y = 0;
                                new LogHelper().Log("Iterating opera profiles", reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                                foreach (Models.Cloud.GuestProfile guestProfile in reservation.GuestProfiles)
                                {
                                    new LogHelper().Log("Processing opera profile - (Last name - +" + guestProfile.LastName + ")", reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                                    #region GuestDocumentDetails
                                    DateTime? dt = null;

                                    if (string.IsNullOrEmpty(guestProfile.PmsProfileID))
                                    {
                                        new LogHelper().Log("Creating accompanying profile in opera  - (Last name - +" + guestProfile.LastName + ")", reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                                        Models.OWS.OwsResponseModel responseModel = await new WSClientHelper().CreateAccompanyingProfile(reservation.ReservationNameID, new Models.OWS.OwsRequestModel()
                                        {
                                            ChainCode = fetchReservationRequest.ServiceParameters.ChainCode,
                                            DestinationEntityID = fetchReservationRequest.ServiceParameters.DestinationEntityID,
                                            HotelDomain = fetchReservationRequest.ServiceParameters.HotelDomain,
                                            KioskID = fetchReservationRequest.ServiceParameters.KioskID,
                                            Language = fetchReservationRequest.ServiceParameters.Language,
                                            LegNumber = fetchReservationRequest.ServiceParameters.Legnumber,
                                            Password = fetchReservationRequest.ServiceParameters.Password,
                                            SystemType = fetchReservationRequest.ServiceParameters.SystemType,
                                            Username = fetchReservationRequest.ServiceParameters.Username,
                                            CreateAccompanyingProfileRequest = new Models.OWS.CreateAccompanyingProfileRequest()
                                            {
                                                FirstName = guestProfile.FirstName,
                                                MiddleName = guestProfile.MiddleName,
                                                LastName = guestProfile.LastName,
                                                Gender = !string.IsNullOrEmpty(guestProfile.Gender) ? (guestProfile.Gender.ToUpper().Equals("M") ? "Male" : (guestProfile.Gender.ToUpper().Equals("F") ? "Female" : null)) : null,
                                                ReservationNumber = reservation.ReservationNumber
                                            }
                                        }, "pre checked-in fetch", fetchReservationRequest.ServiceParameters);
                                        if (!responseModel.result)
                                        {
                                            new LogHelper().Log("Failled to create accompanying profile with reason :- " + responseModel.responseMessage, reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                                            new LogHelper().Warn("Failled to fetch profile documents with reason :- " + responseModel.responseMessage, reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                                        }

                                        else if (responseModel.responseData == null)
                                        {
                                            new LogHelper().Log("Failled to create accompanying profile with reason :- API response data is NULL" + cloudResponse.responseMessage, reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                                            new LogHelper().Warn("Failled to create accompanying profile with reason :- API response data is NULL" + cloudResponse.responseMessage, reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                                        }
                                        else
                                        {
                                            new LogHelper().Debug("Converting API json to object", reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                                            try
                                            {
                                                Models.OWS.GuestProfile guest = JsonConvert.DeserializeObject<Models.OWS.GuestProfile>(responseModel.responseData.ToString());
                                                guestProfile.PmsProfileID = guest.PmsProfileID;
                                            }
                                            catch (Exception ex)
                                            {
                                                new LogHelper().Error(ex, reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                                                new LogHelper().Log("Failled to covert API response to object", reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                                                new LogHelper().Warn("Failled to create accompanying profile with reason :- " + ex.Message, reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                                                new LogHelper().Debug("Failled to create accompanying profile with reason :- " + ex.Message, reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                                            }
                                            new LogHelper().Log("Accompanying profile created successfully", reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                                        }
                                    }

                                    if (!string.IsNullOrEmpty(guestProfile.PmsProfileID))
                                    {
                                        new LogHelper().Log("Updating guest profile in opera  - (Last name - +" + guestProfile.LastName + ")", reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                                        Models.OWS.OwsResponseModel owsResponse = await new WSClientHelper().UpdateGuestProfile(reservation.ReservationNameID, new Models.OWS.OwsRequestModel()
                                        {
                                            ChainCode = fetchReservationRequest.ServiceParameters.ChainCode,
                                            DestinationEntityID = fetchReservationRequest.ServiceParameters.DestinationEntityID,
                                            HotelDomain = fetchReservationRequest.ServiceParameters.HotelDomain,
                                            KioskID = fetchReservationRequest.ServiceParameters.KioskID,
                                            Language = fetchReservationRequest.ServiceParameters.Language,
                                            LegNumber = fetchReservationRequest.ServiceParameters.Legnumber,
                                            Password = fetchReservationRequest.ServiceParameters.Password,
                                            SystemType = fetchReservationRequest.ServiceParameters.SystemType,
                                            Username = fetchReservationRequest.ServiceParameters.Username,
                                            UpdateProileRequest = new Models.OWS.UpdateProfile()
                                            {
                                                ProfileID = guestProfile.PmsProfileID,

                                                DOB = !string.IsNullOrEmpty(guestProfile.BirthDate) ? DateTime.ParseExact(guestProfile.BirthDate, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture) : dt,

                                                DocumentNumber = (profileDocuments != null && profileDocuments.Count > 0 && profileDocuments.FindIndex(z => z.CloudProfileDetailID.Equals(guestProfile.CloudProfileDetailID)) > -1) ? profileDocuments[profileDocuments.FindIndex(z => z.CloudProfileDetailID.Equals(guestProfile.CloudProfileDetailID))].DocumentNumber : null,
                                                DocumentType = (profileDocuments != null && profileDocuments.Count > 0 && profileDocuments.FindIndex(z => z.CloudProfileDetailID.Equals(guestProfile.CloudProfileDetailID)) > -1) ? profileDocuments[profileDocuments.FindIndex(z => z.CloudProfileDetailID.Equals(guestProfile.CloudProfileDetailID))].DocumentTypeCode : null,
                                                Gender = !string.IsNullOrEmpty(guestProfile.Gender) ? (guestProfile.Gender.ToUpper().Equals("M") ? "Male" : (guestProfile.Gender.ToUpper().Equals("F") ? "Female" : null)) : null,
                                                IssueCountry = (profileDocuments != null && profileDocuments.Count > 0 && profileDocuments.FindIndex(z => z.CloudProfileDetailID.Equals(guestProfile.CloudProfileDetailID)) > -1) ? profileDocuments[profileDocuments.FindIndex(z => z.CloudProfileDetailID.Equals(guestProfile.CloudProfileDetailID))].IssueCountry : null,
                                                IssueDate = (profileDocuments != null && profileDocuments.Count > 0 && profileDocuments.FindIndex(z => z.CloudProfileDetailID.Equals(guestProfile.CloudProfileDetailID)) > -1) ? profileDocuments[profileDocuments.FindIndex(z => z.CloudProfileDetailID.Equals(guestProfile.CloudProfileDetailID))].IssueDate : dt,
                                                Nationality = guestProfile.Nationality

                                            }
                                        }, "pre checked-in fetch", fetchReservationRequest.ServiceParameters);

                                        if (!owsResponse.result)
                                        {
                                            new LogHelper().Log("Failled to update profile with reason :- " + owsResponse.responseMessage, reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                                            new LogHelper().Warn("Failled to update profile with reason :- " + owsResponse.responseMessage, reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                                        }
                                        else
                                        {
                                            new LogHelper().Log("Updated profile successfully", reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                                        }
                                    }

                                    if (!string.IsNullOrEmpty(guestProfile.PmsProfileID))
                                    {
                                        new LogHelper().Log("Updating passport info in opera  - (Last name - +" + guestProfile.LastName + ")", reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                                        Models.OWS.OwsResponseModel owsResponse = await new WSClientHelper().UpdateGuestPassport(reservation.ReservationNameID, new Models.OWS.OwsRequestModel()
                                        {
                                            ChainCode = fetchReservationRequest.ServiceParameters.ChainCode,
                                            DestinationEntityID = fetchReservationRequest.ServiceParameters.DestinationEntityID,
                                            HotelDomain = fetchReservationRequest.ServiceParameters.HotelDomain,
                                            KioskID = fetchReservationRequest.ServiceParameters.KioskID,
                                            Language = fetchReservationRequest.ServiceParameters.Language,
                                            LegNumber = fetchReservationRequest.ServiceParameters.Legnumber,
                                            Password = fetchReservationRequest.ServiceParameters.Password,
                                            SystemType = fetchReservationRequest.ServiceParameters.SystemType,
                                            Username = fetchReservationRequest.ServiceParameters.Username,
                                            UpdateProileRequest = new Models.OWS.UpdateProfile()
                                            {
                                                ProfileID = guestProfile.PmsProfileID,

                                                DOB = !string.IsNullOrEmpty(guestProfile.BirthDate) ? DateTime.ParseExact(guestProfile.BirthDate, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture) : dt,

                                                DocumentNumber = (profileDocuments != null && profileDocuments.Count > 0 && profileDocuments.FindIndex(z => z.CloudProfileDetailID.Equals(guestProfile.CloudProfileDetailID)) > -1) ? profileDocuments[profileDocuments.FindIndex(z => z.CloudProfileDetailID.Equals(guestProfile.CloudProfileDetailID))].DocumentNumber : null,
                                                DocumentType = (profileDocuments != null && profileDocuments.Count > 0 && profileDocuments.FindIndex(z => z.CloudProfileDetailID.Equals(guestProfile.CloudProfileDetailID)) > -1) ? profileDocuments[profileDocuments.FindIndex(z => z.CloudProfileDetailID.Equals(guestProfile.CloudProfileDetailID))].DocumentTypeCode : null,
                                                Gender = !string.IsNullOrEmpty(guestProfile.Gender) ? (guestProfile.Gender.ToUpper().Equals("M") ? "Male" : (guestProfile.Gender.ToUpper().Equals("F") ? "Female" : null)) : null,
                                                IssueCountry = (profileDocuments != null && profileDocuments.Count > 0 && profileDocuments.FindIndex(z => z.CloudProfileDetailID.Equals(guestProfile.CloudProfileDetailID)) > -1) ? profileDocuments[profileDocuments.FindIndex(z => z.CloudProfileDetailID.Equals(guestProfile.CloudProfileDetailID))].IssueCountry : null,
                                                IssueDate = (profileDocuments != null && profileDocuments.Count > 0 && profileDocuments.FindIndex(z => z.CloudProfileDetailID.Equals(guestProfile.CloudProfileDetailID)) > -1) ? profileDocuments[profileDocuments.FindIndex(z => z.CloudProfileDetailID.Equals(guestProfile.CloudProfileDetailID))].IssueDate : dt,
                                                Nationality = guestProfile.Nationality

                                            }
                                        }, "pre checked-in fetch", fetchReservationRequest.ServiceParameters);

                                        if (!owsResponse.result)
                                        {
                                            new LogHelper().Log("Failled to update passport info with reason :- " + owsResponse.responseMessage, reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                                            new LogHelper().Warn("Failled to update passport info with reason :- " + owsResponse.responseMessage, reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                                        }
                                        else
                                        {
                                            new LogHelper().Log("Updated passport info successfully", reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                                        }
                                    }
                                    #endregion


                                    if (guestProfile.Address != null && guestProfile.Address.Count > 0)
                                    {
                                        #region Address 
                                        new LogHelper().Log("Updating address in opera  - (Last name - +" + guestProfile.LastName + ")", reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                                        Models.OWS.OwsResponseModel owsResponse = await new WSClientHelper().UpdateProfileAddressAsync(reservation.ReservationNameID, new Models.OWS.OwsRequestModel()
                                        {
                                            ChainCode = fetchReservationRequest.ServiceParameters.ChainCode,
                                            DestinationEntityID = fetchReservationRequest.ServiceParameters.DestinationEntityID,
                                            HotelDomain = fetchReservationRequest.ServiceParameters.HotelDomain,
                                            KioskID = fetchReservationRequest.ServiceParameters.KioskID,
                                            Language = fetchReservationRequest.ServiceParameters.Language,
                                            LegNumber = fetchReservationRequest.ServiceParameters.Legnumber,
                                            Password = fetchReservationRequest.ServiceParameters.Password,
                                            SystemType = fetchReservationRequest.ServiceParameters.SystemType,
                                            Username = fetchReservationRequest.ServiceParameters.Username,
                                            UpdateProileRequest = new Models.OWS.UpdateProfile()
                                            {
                                                ProfileID = guestProfile.PmsProfileID,
                                                Addresses = new List<Models.OWS.Address>()
                                        {
                                            new Models.OWS.Address()
                                            {
                                                address1 = guestProfile.Address[0].address1,
                                                address2 = guestProfile.Address[0].address2,
                                                city = guestProfile.Address[0].city,
                                                state = guestProfile.Address[0].state,
                                                zip = guestProfile.Address[0].zip,
                                                country = guestProfile.Address[0].country,
                                                displaySequence = 1,
                                                primary = true,
                                                addressType = "BUSINESS"
                                            }
                                        }
                                            }
                                        }, "pre checked-in fetch", fetchReservationRequest.ServiceParameters);
                                        if (!owsResponse.result)
                                        {
                                            new LogHelper().Log("Failled to update address info with reason :- " + owsResponse.responseMessage, reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                                            new LogHelper().Warn("Failled to update address info with reason :- " + owsResponse.responseMessage, reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                                        }
                                        else
                                        {
                                            new LogHelper().Log("Updated address successfully", reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                                        }

                                        #endregion
                                    }
                                    if (guestProfile.Email != null && guestProfile.Email.Count > 0)
                                    {
                                        #region Email

                                        new LogHelper().Log("Updating email in opera  - (Last name - +" + guestProfile.LastName + ")", reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                                        Models.OWS.OwsResponseModel owsResponse = await new WSClientHelper().UpdateProfileEmailAsync(reservation.ReservationNameID, new Models.OWS.OwsRequestModel()
                                        {
                                            ChainCode = fetchReservationRequest.ServiceParameters.ChainCode,
                                            DestinationEntityID = fetchReservationRequest.ServiceParameters.DestinationEntityID,
                                            HotelDomain = fetchReservationRequest.ServiceParameters.HotelDomain,
                                            KioskID = fetchReservationRequest.ServiceParameters.KioskID,
                                            Language = fetchReservationRequest.ServiceParameters.Language,
                                            LegNumber = fetchReservationRequest.ServiceParameters.Legnumber,
                                            Password = fetchReservationRequest.ServiceParameters.Password,
                                            SystemType = fetchReservationRequest.ServiceParameters.SystemType,
                                            Username = fetchReservationRequest.ServiceParameters.Username,
                                            UpdateProileRequest = new Models.OWS.UpdateProfile()
                                            {
                                                ProfileID = guestProfile.PmsProfileID,
                                                Emails = new List<Models.OWS.Email>()
                                        {
                                            new Models.OWS.Email()
                                            {
                                                email = guestProfile.Email[0].email,
                                                //emailType = "BUSINESS",
                                                displaySequence = 1,
                                                primary = true
                                            }
                                        }
                                            }
                                        }, "pre checked-in fetch", fetchReservationRequest.ServiceParameters);
                                        if (!owsResponse.result)
                                        {
                                            new LogHelper().Log("Failled to update email info with reason :- " + owsResponse.responseMessage, reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                                            new LogHelper().Warn("Failled to update email info with reason :- " + owsResponse.responseMessage, reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                                        }
                                        else
                                        {
                                            new LogHelper().Log("Updated email successfully", reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                                        }


                                        #endregion
                                    }
                                    if (guestProfile.Phones != null && guestProfile.Phones.Count > 0)
                                    {
                                        #region Phone 

                                        new LogHelper().Log("Updating phone in opera  - (Last name - +" + guestProfile.LastName + ")", reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                                        Models.OWS.OwsResponseModel owsResponse = await new WSClientHelper().UpdateProfilePhoneAsync(reservation.ReservationNameID, new Models.OWS.OwsRequestModel()
                                        {
                                            ChainCode = fetchReservationRequest.ServiceParameters.ChainCode,
                                            DestinationEntityID = fetchReservationRequest.ServiceParameters.DestinationEntityID,
                                            HotelDomain = fetchReservationRequest.ServiceParameters.HotelDomain,
                                            KioskID = fetchReservationRequest.ServiceParameters.KioskID,
                                            Language = fetchReservationRequest.ServiceParameters.Language,
                                            LegNumber = fetchReservationRequest.ServiceParameters.Legnumber,
                                            Password = fetchReservationRequest.ServiceParameters.Password,
                                            SystemType = fetchReservationRequest.ServiceParameters.SystemType,
                                            Username = fetchReservationRequest.ServiceParameters.Username,
                                            UpdateProileRequest = new Models.OWS.UpdateProfile()
                                            {
                                                ProfileID = guestProfile.PmsProfileID,
                                                Phones = new List<Models.OWS.Phone>()
                                        {
                                            new Models.OWS.Phone()
                                            {
                                                phoneRole = "PHONE",
                                                phoneType = "HOME",
                                                PhoneNumber = guestProfile.Phones[0].PhoneNumber,
                                                displaySequence = 1,
                                                primary = true
                                            }
                                        }
                                            }
                                        }, "pre checked-in fetch", fetchReservationRequest.ServiceParameters);
                                        if (!owsResponse.result)
                                        {
                                            new LogHelper().Log("Failled to update phone info with reason :- " + owsResponse.responseMessage, reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                                            new LogHelper().Warn("Failled to update phone info with reason :- " + owsResponse.responseMessage, reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                                        }
                                        else
                                        {
                                            new LogHelper().Log("Updated phone successfully", reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                                        }

                                        #endregion
                                    }
                                    y++;
                                }

                                new LogHelper().Log("Iterating opera profiles completed", reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                            }
                            else
                            {
                                new LogHelper().Log("Updating opera profile failled since there is no profile in the pre checked in response", reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                            }
                        }
                        catch (Exception ex)
                        {
                            new LogHelper().Error(ex, reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                        }
                        #endregion

                        #region Fetching Reservation from OWS
                        new LogHelper().Log("Fetching opera reservation", reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                        Models.OWS.OwsResponseModel owsResponse1 = await new WSClientHelper().FetchReservationAsync(reservation.ReservationNameID, new Models.OWS.OwsRequestModel()
                        {
                            ChainCode = fetchReservationRequest.ServiceParameters.ChainCode,
                            DestinationEntityID = fetchReservationRequest.ServiceParameters.DestinationEntityID,
                            HotelDomain = fetchReservationRequest.ServiceParameters.HotelDomain,
                            KioskID = fetchReservationRequest.ServiceParameters.KioskID,
                            Language = fetchReservationRequest.ServiceParameters.Language,
                            LegNumber = fetchReservationRequest.ServiceParameters.Legnumber,
                            Password = fetchReservationRequest.ServiceParameters.Password,
                            SystemType = fetchReservationRequest.ServiceParameters.SystemType,
                            Username = fetchReservationRequest.ServiceParameters.Username,
                            FetchBookingRequest = new Models.OWS.FetchBookingRequestModel()
                            {
                                ReservationNumber = reservation.ReservationNumber
                            }
                        }, "pre checked-in fetch", fetchReservationRequest.ServiceParameters);

                        if (!owsResponse1.result)
                        {
                            new LogHelper().Log("Failled to fetch opera reservation with reason :- " + owsResponse1.responseMessage, reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                            new LogHelper().Warn("Failled to fetch opera reservation with reason :- " + owsResponse1.responseMessage, reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                        }


                        if (owsResponse1.responseData == null)
                        {
                            new LogHelper().Log("Failled to fetch opera reservation with reason :- API response data is NULL" + owsResponse1.responseMessage, reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                            new LogHelper().Warn("Failled to fetch opera reservation with reason :- API response data is NULL" + owsResponse1.responseMessage, reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                        }
                        else
                        {
                            new LogHelper().Debug("Converting API json to object", reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                            try
                            {
                                operaReservations = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Models.OWS.OperaReservation>>(owsResponse1.responseData.ToString());
                                new LogHelper().Log("Opera reservation fetched successfully", reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                            }
                            catch (Exception ex)
                            {
                                new LogHelper().Error(ex, reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                                new LogHelper().Log("Failled to covert API response to object", reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                                new LogHelper().Warn("Failled to fetch opera reservation with reason :- " + ex.Message, reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                                new LogHelper().Debug("Failled to fetch opera reservation with reason :- " + ex.Message, reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                            }
                        }

                        #endregion

                        #region Get Regcard

                        try
                        {

                            if (operaReservations != null && operaReservations.Count >= 0)
                            {
                                new LogHelper().Log("Generating registration card", reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                                if (string.IsNullOrEmpty(reservation.GuestSignature))
                                    new LogHelper().Log("Guest signature missing", reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                                operaReservations[0].GuestSignature = !string.IsNullOrEmpty(reservation.GuestSignature) ? reservation.GuestSignature : "";
                                Models.OWS.OwsResponseModel regcardResponse = await new WSClientHelper().GetRegistrationCard(reservation.ReservationNameID, new Models.OWS.OwsRequestModel()
                                {
                                    ChainCode = fetchReservationRequest.ServiceParameters.ChainCode,
                                    DestinationEntityID = fetchReservationRequest.ServiceParameters.DestinationEntityID,
                                    HotelDomain = fetchReservationRequest.ServiceParameters.HotelDomain,
                                    KioskID = fetchReservationRequest.ServiceParameters.KioskID,
                                    Language = fetchReservationRequest.ServiceParameters.Language,
                                    LegNumber = fetchReservationRequest.ServiceParameters.Legnumber,
                                    Password = fetchReservationRequest.ServiceParameters.Password,
                                    SystemType = fetchReservationRequest.ServiceParameters.SystemType,
                                    Username = fetchReservationRequest.ServiceParameters.Username,
                                    OperaReservation = operaReservations[0]
                                }, "pre checked-in fetch", fetchReservationRequest.ServiceParameters);

                                if (!regcardResponse.result || regcardResponse.responseData == null)
                                {
                                    new LogHelper().Log("Failled to generate regcard with reason :- " + regcardResponse.responseMessage, reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                                    new LogHelper().Warn("Failled to generate regcard with reason :- " + regcardResponse.responseMessage, reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                                }
                                else
                                {
                                    new LogHelper().Log("Regcard generated successfully", reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                                    RegcardBase64 = regcardResponse.responseData.ToString();
                                }
                            }
                            else
                            {
                                new LogHelper().Log("Failled to generating registration card, failled to get opera reservation", reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                                new LogHelper().Warn("Failled to generating registration card, failled to get opera reservation", reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                            }
                        }
                        catch (Exception ex)
                        {
                            new LogHelper().Error(ex, reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                        }
                        #endregion

                        #region Pushing regcard to Local DB
                        if (!string.IsNullOrEmpty(RegcardBase64))
                        {
                            try
                            {
                                new LogHelper().Log("Pushing registration card to Local DB", reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                                byte[] regCard = null;
                                regCard = Convert.FromBase64String(RegcardBase64);
                                localResponse = await new WSClientHelper().InsertReservationDocuments(reservation.ReservationNameID, new Models.Local.LocalRequestModel()
                                {
                                    RequestObject = new List<Models.Local.ReservationDocumentsDataTableModel>()
                                {
                                    new Models.Local.ReservationDocumentsDataTableModel()
                                    {
                                        Document = regCard,
                                        DocumentType = "Registration Card",
                                        ReservationNameID = operaReservations[0].ReservationNameID
                                    }
                                }
                                }, "pre checked-in fetch", fetchReservationRequest.ServiceParameters);
                                if (!localResponse.result)
                                {
                                    new LogHelper().Log("Failled to push reservation document with reason :- " + localResponse.responseMessage, reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                                    new LogHelper().Warn("Failled to push reservation document with reason :- " + localResponse.responseMessage, reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                                }
                                else
                                {
                                    new LogHelper().Log("Reservation document updated successfully", reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                                }
                            }
                            catch (Exception exc)
                            {
                                new LogHelper().Error(exc, reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                            }
                        }
                        #endregion

                        #region Fetching Selected Upsell
                        new LogHelper().Log("Fetching upsell selected", reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                        cloudResponse = await new WSClientHelper().FetchUpsellPackages(reservation.ReservationNameID, new Models.Cloud.CloudRequestModel()
                        {
                            RequestObject = reservation.ReservationNameID
                        }, "pre checked-in fetch", fetchReservationRequest.ServiceParameters);
                        if (!cloudResponse.result || cloudResponse.responseData == null)
                        {
                            new LogHelper().Log("Failled to fetching upsell with reason :- " + cloudResponse.responseMessage, reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                            new LogHelper().Warn("Failled to fetching upsell with reason :- " + cloudResponse.responseMessage, reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                        }
                        else
                            new LogHelper().Log("Upsell selected fetch successfully", reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");

                        #endregion

                        #region Updating upsell package locally
                        new LogHelper().Log("Pushing upsell selected to local DB", reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                        localResponse = await new WSClientHelper().PushUpsellpackages(reservation.ReservationNameID, new Models.Local.LocalRequestModel()
                        {
                            RequestObject = cloudResponse.responseData
                        }, "pre checked-in fetch", fetchReservationRequest.ServiceParameters);
                        if (!localResponse.result)
                        {
                            new LogHelper().Log("Failled to update upsell with reason :- " + localResponse.responseMessage, reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                            new LogHelper().Warn("Failled to update upsell with reason :- " + localResponse.responseMessage, reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                        }
                        else
                            new LogHelper().Log("Upsell updated successfully", reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                        #endregion

                        #region Fetching Reservation Policies
                        List<Models.Local.FetchReservationPolicyModel> ReservationPolicyList = new List<Models.Local.FetchReservationPolicyModel>();
                        new LogHelper().Log("Fetching reservation policies", reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                        cloudResponse = await new WSClientHelper().FetchReservationPolicies(reservation.ReservationNameID, new Models.Cloud.CloudRequestModel()
                        {
                            RequestObject = fetchReservationRequest
    
                        }, "pre checked-in fetch", fetchReservationRequest.ServiceParameters);
                        if (!cloudResponse.result || cloudResponse.responseData == null)
                        {
                            new LogHelper().Log("Failled to fetching reservation policies with reason :- " + cloudResponse.responseMessage, reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                            new LogHelper().Warn("Failled to fetching reservation policies with reason :- " + cloudResponse.responseMessage, reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                        }
                        else
                        {
                            new LogHelper().Log("reservation policies fetch successfully", reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                            ReservationPolicyList = JsonConvert.DeserializeObject<List<Models.Local.FetchReservationPolicyModel>>(cloudResponse.responseData.ToString());
                        }
                        #endregion

                        #region Updating reservation policy locally
                        if (ReservationPolicyList != null && ReservationPolicyList.Count > 0)
                        {
                            new LogHelper().Log("Pushing reservation policy to local DB", reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                            List<Models.Local.DB.ReservationPolicyModel> reservationPolicyTable = new List<Models.Local.DB.ReservationPolicyModel>();
                            foreach(Models.Local.FetchReservationPolicyModel policyModel in ReservationPolicyList)
                            {
                                if(!string.IsNullOrEmpty(policyModel.PolicyDescription) && policyModel.PolicyDescription.ToLower().Contains("marketing")
                                    && policyModel.PolicyValue != null && policyModel.PolicyValue.Value)
                                {
                                    Models.Local.DB.ReservationPolicyModel reservationPolicy = new Models.Local.DB.ReservationPolicyModel()
                                    {
                                        IsRoomUpsell = false,
                                        PackageCode = null,
                                        PackageDescription = null,
                                        ReqStatus = true,
                                        RequestType = "Marketing Email",
                                        ReservationNameID = reservation.ReservationNameID,
                                        UserID = 0
                                    };
                                    reservationPolicyTable.Add(reservationPolicy);
                                    break;
                                }
                            }
                            if (reservationPolicyTable != null && reservationPolicyTable.Count > 0)
                            {
                                localResponse = await new WSClientHelper().PushReservationPolicies(reservation.ReservationNameID, new Models.Local.LocalRequestModel()
                                {
                                    RequestObject = reservationPolicyTable
                                }, "pre checked-in fetch", fetchReservationRequest.ServiceParameters);
                                if (!localResponse.result)
                                {
                                    new LogHelper().Log("Failled to push reservation policy with reason :- " + localResponse.responseMessage, reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                                    new LogHelper().Warn("Failled to update reservation policy with reason :- " + localResponse.responseMessage, reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                                }
                                else
                                    new LogHelper().Log("reservation policy updated successfully", reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                            }
                            else
                                new LogHelper().Log("No reservation policy is selected by guest to update in the DB", reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                        }
                        #endregion

                        #region Fetching Reservation Additional Fields

                        new LogHelper().Debug("Fetching reservation additional details", reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");


                        cloudResponse = await new WSClientHelper().FetchReservationAdditionalDetails(reservation.ReservationNameID, JsonConvert.SerializeObject(new Models.Cloud.CloudRequestModel()
                        {
                            RequestObject = reservation.ReservationNumber
                        })

                        , "pre checked-in fetch", fetchReservationRequest.ServiceParameters);
                        if (!cloudResponse.result || cloudResponse.responseData == null)
                        {
                            new LogHelper().Log("Failled to fetching reservation additional details with reason :- " + cloudResponse.responseMessage, reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                            new LogHelper().Warn("Failled to fetching reservation additional details with reason :- " + cloudResponse.responseMessage, reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                        }
                        else
                        {
                            new LogHelper().Debug("reservation additional details fetch successfully", reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");

                        }
                        #endregion

                        #region Updating reservation additional details locally
                        if (cloudResponse != null && cloudResponse.responseData != null && cloudResponse.result)
                        {
                            new LogHelper().Debug("Pushing reservation additional details to local DB", reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");

                            localResponse = await new WSClientHelper().PushReservationAdditionalDetails(reservation.ReservationNameID, new Models.Local.LocalRequestModel()
                            {
                                RequestObject = cloudResponse.responseData
                            }, "pre checked-in fetch",fetchReservationRequest.ServiceParameters);
                            if (!localResponse.result)
                            {
                                new LogHelper().Log("Failled to push reservation policy with reason :- " + localResponse.responseMessage, reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                                new LogHelper().Warn("Failled to update reservation policy with reason :- " + localResponse.responseMessage, reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                            }
                            else
                                new LogHelper().Debug("reservation policy updated successfully", reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");

                        }
                        #endregion

                        #region Fetching Feedback
                        new LogHelper().Log("Fetching feedback", reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                        cloudResponse = await new WSClientHelper().FetchFeedback(reservation.ReservationNameID, new Models.Cloud.CloudRequestModel()
                        {
                            RequestObject = reservation.ReservationNameID
                        }, "pre checked-in fetch", fetchReservationRequest.ServiceParameters);
                        if (!cloudResponse.result || cloudResponse.responseData == null)
                        {
                            new LogHelper().Log("Failled to fetch feedback with reason :- " + cloudResponse.responseMessage, reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                            new LogHelper().Warn("Failled to fetch feedback with reason :- " + cloudResponse.responseMessage, reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                        }
                        else
                            new LogHelper().Log("Feedback fetched successfully", reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                        #endregion

                        #region Updating Feedback in Local DB                    
                        new LogHelper().Log("Pushing feedback to local DB", reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                        localResponse = await new WSClientHelper().PushFeedback(reservation.ReservationNameID, new Models.Local.LocalRequestModel()
                        {
                            RequestObject = cloudResponse.responseData
                        }, "pre checked-in fetch", fetchReservationRequest.ServiceParameters);
                        if (!localResponse.result)
                        {
                            new LogHelper().Log("Failled to update feedback with reason :- " + localResponse.responseMessage, reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                            new LogHelper().Warn("Failled to update feedback with reason :- " + localResponse.responseMessage, reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                        }
                        else
                            new LogHelper().Log("Feedback updated successfully", reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                        #endregion

                        #region Fetchpaymentdetails
                        new LogHelper().Log("Fetching payment details", reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                        cloudResponse = await new WSClientHelper().FetchPaymentDetails(reservation.ReservationNameID, new Models.Cloud.CloudRequestModel()
                        {
                            RequestObject = reservation.ReservationNameID
                        }, "pre checked-in fetch", fetchReservationRequest.ServiceParameters);
                        if (!cloudResponse.result || cloudResponse.responseData == null)
                        {
                            new LogHelper().Log("Failled to fetch payment details with reason :- " + cloudResponse.responseMessage, reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                            new LogHelper().Warn("Failled to fetch payment details with reason :- " + cloudResponse.responseMessage, reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                        }
                        else
                            new LogHelper().Log("Payment details fetched successfully", reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                        #endregion

                        #region Update payment in opera
                        if (cloudResponse.responseData != null)
                        {
                            new LogHelper().Log("Converting Json string to object", reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                            try
                            {
                                paymentDetails = JsonConvert.DeserializeObject<Models.Local.PaymentDetails>(cloudResponse.responseData.ToString());
                            }
                            catch (Exception ex)
                            {
                                new LogHelper().Error(ex, reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                            }
                            if (paymentDetails == null)
                            {
                                new LogHelper().Log("payment detail object is null, skipping the payment update", reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                            }
                            else
                            {
                                int x = 0;
                                new LogHelper().Log("Iterating the payment headers", reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                                foreach (Models.Local.PaymentHeader paymentHeader in paymentDetails.paymentHeaders)
                                {
                                    if (paymentHeader.IsActive == null)
                                    {
                                        new LogHelper().Log("Processing the payment header with psprefernce - " + paymentHeader.pspReferenceNumber + " where IsActive falg is NULL", reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");

                                        #region Update Opera
                                        if (paymentDetails.paymentHeaders[x].TransactionType.Equals(Models.Local.TransactionType.PreAuth.ToString()))
                                        {
                                            new LogHelper().Log("Processing the payment header with psprefernce - " + paymentHeader.pspReferenceNumber + " as a pre-auth transaction", reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");

                                            paymentDetails.paymentHeaders[x].IsActive = true;

                                            #region Updating Card details in opera Reservation
                                            new LogHelper().Log("Updating credit card details in the reservation", reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");

                                            Models.OWS.OwsResponseModel owsResponse = await new WSClientHelper().UpdateCardDetailsInReservationAsyn(reservation.ReservationNameID, new Models.OWS.OwsRequestModel()
                                            {
                                                ChainCode = fetchReservationRequest.ServiceParameters.ChainCode,
                                                DestinationEntityID = fetchReservationRequest.ServiceParameters.DestinationEntityID,
                                                HotelDomain = fetchReservationRequest.ServiceParameters.HotelDomain,
                                                KioskID = fetchReservationRequest.ServiceParameters.KioskID,
                                                Language = fetchReservationRequest.ServiceParameters.Language,
                                                LegNumber = fetchReservationRequest.ServiceParameters.Legnumber,
                                                Password = fetchReservationRequest.ServiceParameters.Password,
                                                SystemType = fetchReservationRequest.ServiceParameters.SystemType,
                                                Username = fetchReservationRequest.ServiceParameters.Username,
                                                modifyBookingRequest = new Models.OWS.ModifyBookingRequest()
                                                {
                                                    ReservationNumber = reservation.ReservationNumber,
                                                    isUDFFieldSpecified = false,
                                                    updateCreditCardDetails = true,
                                                    GarunteeTypeCode = fetchReservationRequest.ServiceParameters.GarunteeTypeCode,// "CC",
                                                    PaymentMethod = new Models.OWS.PaymentMethod()
                                                    {
                                                        ExpiryDate = !string.IsNullOrEmpty(paymentDetails.paymentHeaders[x].ExpiryDate) ? "01/" + paymentDetails.paymentHeaders[x].ExpiryDate : null,
                                                        MaskedCardNumber = paymentDetails.paymentHeaders[x].MaskedCardNumber,
                                                        PaymentType = paymentDetails.paymentHeaders[x].OperaPaymentTypeCode

                                                    }
                                                }
                                            }, "pre checked-in fetch", fetchReservationRequest.ServiceParameters);
                                            if (!owsResponse.result)
                                            {
                                                new LogHelper().Log("Updating credit card details in the reservation failled with reason :- " + owsResponse.responseMessage, reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                                                new LogHelper().Warn("Updating credit card details in the reservation failled with reason :- " + owsResponse.responseMessage, reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                                            }
                                            else
                                            {
                                                new LogHelper().Log("Updating credit card details in the reservation succeeded", reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                                                new LogHelper().Warn("Updating credit card details in the reservation succeeded", reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                                            }
                                            #endregion

                                            #region Updating UDF fields in Opera reservation
                                            try
                                            {
                                                new LogHelper().Log("Updating pre auth code and amount in UDF fileds", reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                                                owsResponse = await new WSClientHelper().ModifyBooking(reservation.ReservationNameID, new Models.OWS.OwsRequestModel()
                                                {
                                                    ChainCode = fetchReservationRequest.ServiceParameters.ChainCode,
                                                    DestinationEntityID = fetchReservationRequest.ServiceParameters.DestinationEntityID,
                                                    HotelDomain = fetchReservationRequest.ServiceParameters.HotelDomain,
                                                    KioskID = fetchReservationRequest.ServiceParameters.KioskID,
                                                    Language = fetchReservationRequest.ServiceParameters.Language,
                                                    LegNumber = fetchReservationRequest.ServiceParameters.Legnumber,
                                                    Password = fetchReservationRequest.ServiceParameters.Password,
                                                    SystemType = fetchReservationRequest.ServiceParameters.SystemType,
                                                    Username = fetchReservationRequest.ServiceParameters.Username,
                                                    modifyBookingRequest = new Models.OWS.ModifyBookingRequest()
                                                    {
                                                        isUDFFieldSpecified = true,
                                                        ReservationNumber = reservation.ReservationNumber,
                                                        uDFFields = new List<Models.OWS.UDFField>()
                                                                                {
                                                                                    new Models.OWS.UDFField()
                                                                                    {
                                                                                        FieldName  = fetchReservationRequest.ServiceParameters.PreAuthUDF,
                                                                                        FieldValue = paymentHeader.pspReferenceNumber
                                                                                    },
                                                                                    new Models.OWS.UDFField()
                                                                                    {
                                                                                        FieldName  = fetchReservationRequest.ServiceParameters.PreAuthAmntUDF,
                                                                                        FieldValue = paymentHeader.Amount
                                                                                    }
                                                                                }
                                                    }
                                                }, "pre checked-in fetch", fetchReservationRequest.ServiceParameters);
                                                if (!owsResponse.result)
                                                {
                                                    new LogHelper().Log("Updating pre auth code and amount in UDF fileds failled with reason : - " + owsResponse.responseMessage, reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                                                    new LogHelper().Warn("Updating pre auth code and amount in UDF fileds failled with reason : - " + owsResponse.responseMessage, reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                                                }
                                                else
                                                    new LogHelper().Log("Updating pre auth code and amount in UDF fileds succeeded ", reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                                            }
                                            catch (Exception ex)
                                            {
                                                new LogHelper().Error(ex, reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                                            }
                                            #endregion

                                        }
                                        else if (paymentDetails.paymentHeaders[x].TransactionType.Equals(Models.Local.TransactionType.Sale.ToString()))
                                        {
                                            paymentDetails.paymentHeaders[x].IsActive = false;

                                            #region Updating Card details in opera Reservation
                                            new LogHelper().Log("Updating credit card details in the reservation", reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");

                                            Models.OWS.OwsResponseModel owsResponse = await new WSClientHelper().UpdateCardDetailsInReservationAsyn(reservation.ReservationNameID, new Models.OWS.OwsRequestModel()
                                            {
                                                ChainCode = fetchReservationRequest.ServiceParameters.ChainCode,
                                                DestinationEntityID = fetchReservationRequest.ServiceParameters.DestinationEntityID,
                                                HotelDomain = fetchReservationRequest.ServiceParameters.HotelDomain,
                                                KioskID = fetchReservationRequest.ServiceParameters.KioskID,
                                                Language = fetchReservationRequest.ServiceParameters.Language,
                                                LegNumber = fetchReservationRequest.ServiceParameters.Legnumber,
                                                Password = fetchReservationRequest.ServiceParameters.Password,
                                                SystemType = fetchReservationRequest.ServiceParameters.SystemType,
                                                Username = fetchReservationRequest.ServiceParameters.Username,
                                                modifyBookingRequest = new Models.OWS.ModifyBookingRequest()
                                                {
                                                    ReservationNumber = reservation.ReservationNumber,
                                                    isUDFFieldSpecified = false,
                                                    updateCreditCardDetails = true,
                                                    GarunteeTypeCode = fetchReservationRequest.ServiceParameters.GarunteeTypeCode,//"CC",
                                                    PaymentMethod = new Models.OWS.PaymentMethod()
                                                    {
                                                        ExpiryDate = !string.IsNullOrEmpty(paymentDetails.paymentHeaders[x].ExpiryDate) ? "01/" + paymentDetails.paymentHeaders[x].ExpiryDate : null,
                                                        MaskedCardNumber = paymentDetails.paymentHeaders[x].MaskedCardNumber,
                                                        PaymentType = paymentDetails.paymentHeaders[x].OperaPaymentTypeCode

                                                    }
                                                }
                                            }, "pre checked-in fetch", fetchReservationRequest.ServiceParameters);
                                            if (!owsResponse.result)
                                            {
                                                new LogHelper().Log("Updating credit card details in the reservation failled with reason :- " + owsResponse.responseMessage, reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                                                new LogHelper().Warn("Updating credit card details in the reservation failled with reason :- " + owsResponse.responseMessage, reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                                            }
                                            else
                                            {
                                                new LogHelper().Log("Updating credit card details in the reservation succeeded", reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                                                new LogHelper().Warn("Updating credit card details in the reservation succeeded", reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                                            }
                                            #endregion

                                            #region Updating UDF fields in Opera reservation
                                            try
                                            {
                                                new LogHelper().Log("Updating pre auth code and amount in UDF fileds", reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                                                owsResponse = await new WSClientHelper().ModifyBooking(reservation.ReservationNameID, new Models.OWS.OwsRequestModel()
                                                {
                                                    ChainCode = fetchReservationRequest.ServiceParameters.ChainCode,
                                                    DestinationEntityID = fetchReservationRequest.ServiceParameters.DestinationEntityID,
                                                    HotelDomain = fetchReservationRequest.ServiceParameters.HotelDomain,
                                                    KioskID = fetchReservationRequest.ServiceParameters.KioskID,
                                                    Language = fetchReservationRequest.ServiceParameters.Language,
                                                    LegNumber = fetchReservationRequest.ServiceParameters.Legnumber,
                                                    Password = fetchReservationRequest.ServiceParameters.Password,
                                                    SystemType = fetchReservationRequest.ServiceParameters.SystemType,
                                                    Username = fetchReservationRequest.ServiceParameters.Username,
                                                    modifyBookingRequest = new Models.OWS.ModifyBookingRequest()
                                                    {
                                                        isUDFFieldSpecified = true,
                                                        ReservationNumber = reservation.ReservationNumber,
                                                        uDFFields = new List<Models.OWS.UDFField>()
                                                                                {
                                                                                    new Models.OWS.UDFField()
                                                                                    {
                                                                                        FieldName  = fetchReservationRequest.ServiceParameters.PreAuthUDF,
                                                                                        FieldValue = paymentHeader.pspReferenceNumber
                                                                                    },
                                                                                    new Models.OWS.UDFField()
                                                                                    {
                                                                                        FieldName  = fetchReservationRequest.ServiceParameters.PreAuthAmntUDF,
                                                                                        FieldValue = paymentHeader.Amount
                                                                                    }
                                                                                }
                                                    }
                                                }, "pre checked-in fetch", fetchReservationRequest.ServiceParameters);
                                                if (!owsResponse.result)
                                                {
                                                    new LogHelper().Log("Updating pre auth code and amount in UDF fileds failled with reason : - " + owsResponse.responseMessage, reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                                                    new LogHelper().Warn("Updating pre auth code and amount in UDF fileds failled with reason : - " + owsResponse.responseMessage, reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                                                }
                                                else
                                                    new LogHelper().Log("Updating pre auth code and amount in UDF fileds succeeded ", reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                                            }
                                            catch (Exception ex)
                                            {
                                                new LogHelper().Error(ex, reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                                            }
                                            #endregion

                                            #region Posting payment in opera reservation
                                            try
                                            {
                                                new LogHelper().Log("Posting payment in the reservation", reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                                                owsResponse = await new WSClientHelper().MakePayment(reservation.ReservationNameID, new Models.OWS.OwsRequestModel()
                                                {
                                                    ChainCode = fetchReservationRequest.ServiceParameters.ChainCode,
                                                    DestinationEntityID = fetchReservationRequest.ServiceParameters.DestinationEntityID,
                                                    HotelDomain = fetchReservationRequest.ServiceParameters.HotelDomain,
                                                    KioskID = fetchReservationRequest.ServiceParameters.KioskID,
                                                    Language = fetchReservationRequest.ServiceParameters.Language,
                                                    LegNumber = fetchReservationRequest.ServiceParameters.Legnumber,
                                                    Password = fetchReservationRequest.ServiceParameters.Password,
                                                    SystemType = fetchReservationRequest.ServiceParameters.SystemType,
                                                    Username = fetchReservationRequest.ServiceParameters.Username,
                                                    MakePaymentRequest = new Models.OWS.MakePaymentRequest()
                                                    {
                                                        Amount = Convert.ToDecimal(paymentHeader.Amount),
                                                        PaymentInfo = "Payment from web checkin",
                                                        StationID = "MCI",
                                                        WindowNumber = 1,
                                                        ReservationNameID = reservation.ReservationNameID,
                                                        MaskedCardNumber = paymentHeader.MaskedCardNumber.ToLower(),
                                                        PaymentRefernce = "Payment from web checkin - Sale",
                                                        PaymentTypeCode = paymentHeader.OperaPaymentTypeCode,
                                                        ApprovalCode = paymentHeader.pspReferenceNumber
                                                    }
                                                }, "pre checked-in fetch", fetchReservationRequest.ServiceParameters);
                                                if (!owsResponse.result)
                                                {
                                                    new LogHelper().Log("Posting payment failled with reason : - " + owsResponse.responseMessage, reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                                                    new LogHelper().Warn("Posting payment failled with reason : - " + owsResponse.responseMessage, reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                                                }
                                                else
                                                    new LogHelper().Log("Posting payment in opera reservation succeeded ", reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                                            }
                                            catch (Exception ex)
                                            {
                                                new LogHelper().Error(ex, reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                                            }
                                            #endregion
                                        }
                                        else if (paymentDetails.paymentHeaders[x].TransactionType.Equals(Models.Local.TransactionType.Capture.ToString()))
                                        {
                                            new LogHelper().Log("Wrong payment header retuned and Is active NULL (Capture)", reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                                            paymentDetails.paymentHeaders[x].IsActive = false;
                                        }

                                        #endregion
                                    }
                                    x++;
                                }
                                new LogHelper().Log("payment details updated successfully", reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                            }
                        }
                        #endregion

                        #region Pushing Payment details in LOcal Db

                        new LogHelper().Log("Updating payment details in local DB", reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                        localResponse = await new WSClientHelper().PushPaymentDetails(reservation.ReservationNameID, new Models.Local.LocalRequestModel()
                        {
                            RequestObject = paymentDetails
                        }, "pre checked-in fetch", fetchReservationRequest.ServiceParameters);
                        if (!localResponse.result)
                        {
                            new LogHelper().Log("Failled to update payment details in Local DB with reason :- " + localResponse.responseMessage, reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                            new LogHelper().Warn("Failled to update payment details in local DB with reason :- " + localResponse.responseMessage, reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                        }
                        else
                            new LogHelper().Log("Payment details updated successfully", reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                        #endregion

                        #region Final step

                        new LogHelper().Log("Updating record in cloud as synced to local true", reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                        cloudResponse = await new WSClientHelper().UpdateRecordInCloud(reservation.ReservationNameID, new Models.Cloud.CloudRequestModel()
                        {
                            RequestObject = new List<Models.Local.UpdateReservationByReservationNameIDModel>()
                                                            {
                                                                new Models.Local.UpdateReservationByReservationNameIDModel
                                                                {
                                                                    IsPushedToLocal = true,
                                                                    ReservationNameID = reservation.ReservationNameID
                                                                }
                                                            }
                        }, "pre checked-in fetch", fetchReservationRequest.ServiceParameters);
                        if (!cloudResponse.result)
                        {
                            new LogHelper().Log("Failled to update record with reason :- " + cloudResponse.responseMessage, reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");

                        }
                        else
                            new LogHelper().Log("Record updated successfully", reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");

                        new LogHelper().Log("Clearing the record in the cloud", reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                        cloudResponse = await new WSClientHelper().ClearRecords(reservation.ReservationNameID, new Models.Cloud.CloudRequestModel() { RequestObject = reservation.ReservationNameID }, "pre checked-in fetch", fetchReservationRequest.ServiceParameters);
                        if (!cloudResponse.result)
                        {
                            new LogHelper().Log("Failled to clear the record with reason :- " + cloudResponse.responseMessage, reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                        }
                        else
                            new LogHelper().Log("Record cleared successfully", reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                        #endregion

                        #region Pushing Reservation Track

                        new LogHelper().Log("Pushing reservation track in local DB ", reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");

                        localResponse = await new WSClientHelper().PushReservationTrackLocally(reservation.ReservationNameID, new Models.Local.LocalRequestModel()
                        {
                            RequestObject = new Models.Local.ReservationTrackStatus()
                            {
                                ReservationNameID = reservation.ReservationNameID,
                                ProcessType = Models.Local.ReservationProcessType.PreCheckedInFetched.ToString(),
                                ReservationNumber = reservation.ReservationNumber,
                                ProcessStatus = "",
                                EmailSent = false
                            }
                        }, "pre checked-in fetch", fetchReservationRequest.ServiceParameters);
                        if (localResponse.result)
                        {
                            new LogHelper().Log("Reservation track in local DB updated successfully ", reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                        }
                        else
                        {
                            new LogHelper().Log("Failled to update reservation track in local DB with reason :- " + localResponse.responseMessage, reservation.ReservationNameID, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                        }

                        #endregion
                    }
                }
                else
                {
                    new LogHelper().Log("Failled to process pre checked-in reservation list, since it is NULL", null, "FetchPreCheckedInReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                    return new Models.Local.LocalResponseModel()
                    {
                        result = false,
                        responseMessage = "Failled to process pre checked-in reservation list, since it is NULL"
                    };
                }
                #endregion

                return new Models.Local.LocalResponseModel()
                {
                    result = true,
                    responseMessage = "Success"
                };
            }
            catch (Exception ex)
            {
                return new Models.Local.LocalResponseModel()
                {
                    result = false,
                    responseMessage = ex.Message
                };
            }
        }

        public async Task<Models.Local.LocalResponseModel> FetchPreCheckedOutReservation(Models.Local.FetchReservationRequest fetchReservationRequest)
        {
            #region Variables
            List<Models.Cloud.OperaReservation> PrecheckedoutReservationList = null;
            Models.Local.PaymentDetails paymentDetails = null;
            List<Models.OWS.OperaReservation> operaReservations = null;
            string folioAsBase64 = "";
            Models.OWS.FolioModel guestFolio = null;
            #endregion

            try
            {

                #region Fetching pre-checkeout records
                new LogHelper().Log("Fetching pre checked-out reservation", null, "FetchDueOutReservation",  fetchReservationRequest.ServiceParameters.ClientID, "pre checked-out fetch");
                Models.Cloud.CloudResponseModel cloudResponse = await new WSClientHelper().FetchPrechedoutRecord(new Models.Cloud.CloudRequestModel() { RequestObject = fetchReservationRequest }, fetchReservationRequest.ServiceParameters);
                if (!cloudResponse.result)
                {
                    new LogHelper().Log("Failled to fetch pre checked-out reservation with reason :- " + cloudResponse.responseMessage, null, "FetchDueOutReservation",  fetchReservationRequest.ServiceParameters.ClientID, "pre checked-out fetch");
                    return new Models.Local.LocalResponseModel()
                    {
                        result = false,
                        responseMessage = "Failled to fetch pre checked-out reservation with reason :- " + cloudResponse.responseMessage
                    };
                }
                if (cloudResponse.responseData == null)
                {
                    new LogHelper().Log("Failled to fetch pre checked-out reservation with reason :- API response data is NULL" + cloudResponse.responseMessage, null, "FetchDueOutReservation",  fetchReservationRequest.ServiceParameters.ClientID, "pre checked-out fetch");
                    return new Models.Local.LocalResponseModel()
                    {
                        result = false,
                        responseMessage = "Failled to fetch pre checked-out reservation with reason :- API response data is NULL" + cloudResponse.responseMessage
                    };
                }
                new LogHelper().Debug("Converting API json to object", null, "FetchDueOutReservation",  fetchReservationRequest.ServiceParameters.ClientID, "pre checked-out fetch");
                try
                {
                    PrecheckedoutReservationList = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Models.Cloud.OperaReservation>>(cloudResponse.responseData.ToString());
                }
                catch (Exception ex)
                {
                    new LogHelper().Error(ex, null, "FetchDueOutReservation",  fetchReservationRequest.ServiceParameters.ClientID, "pre checked-out fetch");
                    new LogHelper().Log("Failled to covert API response to object", null, "FetchDueOutReservation",  fetchReservationRequest.ServiceParameters.ClientID, "pre checked-out fetch");
                    return new Models.Local.LocalResponseModel()
                    {
                        result = false,
                        responseMessage = "Failled to covert API response to object" + cloudResponse.responseMessage
                    };
                }
                new LogHelper().Log("Pre checked-out reservation fetched", null, "FetchDueOutReservation",  fetchReservationRequest.ServiceParameters.ClientID, "pre checked-out fetch");

                #endregion

                new LogHelper().Log("Iterating reservation list, Pre checked-out reservation count : - " + PrecheckedoutReservationList != null && PrecheckedoutReservationList.Count >= 0 ?
                                                                PrecheckedoutReservationList.Count.ToString() : "", null, "FetchDueOutReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-out fetch");
                if (PrecheckedoutReservationList != null)
                {
                    foreach (Models.Cloud.OperaReservation reservation in PrecheckedoutReservationList)
                    {
                        new LogHelper().Log("Processing reservation No. : " + reservation.ReservationNumber, reservation.ReservationNameID, "FetchDueOutReservation",  fetchReservationRequest.ServiceParameters.ClientID, "pre checked-out fetch");

                        #region Fetchpaymentdetails
                        new LogHelper().Log("Fetching payment details", reservation.ReservationNameID, "FetchDueOutReservation",  fetchReservationRequest.ServiceParameters.ClientID, "pre checked-out fetch");
                        cloudResponse = await new WSClientHelper().FetchPaymentDetails(reservation.ReservationNameID, new Models.Cloud.CloudRequestModel()
                        {
                            RequestObject = reservation.ReservationNameID
                        }, "pre checked-out fetch", fetchReservationRequest.ServiceParameters);
                        if (!cloudResponse.result || cloudResponse.responseData == null)
                        {
                            new LogHelper().Log("Failled to fetch payment details with reason :- " + cloudResponse.responseMessage, reservation.ReservationNameID, "FetchDueOutReservation",  fetchReservationRequest.ServiceParameters.ClientID, "pre checked-out fetch");
                            new LogHelper().Warn("Failled to fetch payment details with reason :- " + cloudResponse.responseMessage, reservation.ReservationNameID, "FetchDueOutReservation",  fetchReservationRequest.ServiceParameters.ClientID, "pre checked-out fetch");
                        }
                        else
                            new LogHelper().Log("Payment details fetched successfully", reservation.ReservationNameID, "FetchDueOutReservation",  fetchReservationRequest.ServiceParameters.ClientID, "pre checked-out fetch");
                        #endregion

                        #region Update payment in opera
                        if (cloudResponse.responseData != null)
                        {
                            new LogHelper().Log("Converting Json string to object", reservation.ReservationNameID, "FetchDueOutReservation",  fetchReservationRequest.ServiceParameters.ClientID, "pre checked-out fetch");
                            try
                            {
                                paymentDetails = JsonConvert.DeserializeObject<Models.Local.PaymentDetails>(cloudResponse.responseData.ToString());
                            }
                            catch (Exception ex)
                            {
                                new LogHelper().Error(ex, reservation.ReservationNameID, "FetchDueOutReservation",  fetchReservationRequest.ServiceParameters.ClientID, "pre checked-out fetch");
                            }
                            if (paymentDetails == null)
                            {
                                new LogHelper().Log("payment detail object is null, skipping the payment update", reservation.ReservationNameID, "FetchDueOutReservation",  fetchReservationRequest.ServiceParameters.ClientID, "pre checked-out fetch");
                            }
                            else
                            {
                                int x = 0;
                                new LogHelper().Log("Iterating the payment headers", reservation.ReservationNameID, "FetchDueOutReservation",  fetchReservationRequest.ServiceParameters.ClientID, "pre checked-out fetch");
                                foreach (Models.Local.PaymentHeader paymentHeader in paymentDetails.paymentHeaders)
                                {
                                    if (paymentHeader.IsActive == null)
                                    {
                                        new LogHelper().Log("Processing the payment header with psprefernce - " + paymentHeader.pspReferenceNumber + " where IsActive falg is NULL", reservation.ReservationNameID, "FetchDueOutReservation",  fetchReservationRequest.ServiceParameters.ClientID, "pre checked-out fetch");

                                        #region Update Opera
                                        if (paymentDetails.paymentHeaders[x].TransactionType.Equals(Models.Local.TransactionType.PreAuth.ToString()))
                                        {
                                            new LogHelper().Log("Processing the payment header with psprefernce - " + paymentHeader.pspReferenceNumber + " as a pre-auth transaction", reservation.ReservationNameID, "FetchDueOutReservation",  fetchReservationRequest.ServiceParameters.ClientID, "pre checked-out fetch");

                                            paymentDetails.paymentHeaders[x].IsActive = true;

                                            #region Updating UDF fields in Opera reservation
                                            try
                                            {
                                                new LogHelper().Log("Updating pre auth code and amount in UDF fileds", reservation.ReservationNameID, "FetchDueOutReservation",  fetchReservationRequest.ServiceParameters.ClientID, "pre checked-out fetch");
                                                Models.OWS.OwsResponseModel owsResponse = await new WSClientHelper().ModifyBooking(reservation.ReservationNameID, new Models.OWS.OwsRequestModel()
                                                {
                                                    ChainCode = fetchReservationRequest.ServiceParameters.ChainCode,
                                                    DestinationEntityID = fetchReservationRequest.ServiceParameters.DestinationEntityID,
                                                    HotelDomain = fetchReservationRequest.ServiceParameters.HotelDomain,
                                                    KioskID = fetchReservationRequest.ServiceParameters.KioskID,
                                                    Language = fetchReservationRequest.ServiceParameters.Language,
                                                    LegNumber = fetchReservationRequest.ServiceParameters.Legnumber,
                                                    Password = fetchReservationRequest.ServiceParameters.Password,
                                                    SystemType = fetchReservationRequest.ServiceParameters.SystemType,
                                                    Username = fetchReservationRequest.ServiceParameters.Username,
                                                    modifyBookingRequest = new Models.OWS.ModifyBookingRequest()
                                                    {
                                                        isUDFFieldSpecified = true,
                                                        ReservationNumber = reservation.ReservationNumber,
                                                        uDFFields = new List<Models.OWS.UDFField>()
                                                                                {
                                                                                    new Models.OWS.UDFField()
                                                                                    {
                                                                                        FieldName  = fetchReservationRequest.ServiceParameters.PreAuthUDF,
                                                                                        FieldValue = paymentHeader.pspReferenceNumber
                                                                                    },
                                                                                    new Models.OWS.UDFField()
                                                                                    {
                                                                                        FieldName  = fetchReservationRequest.ServiceParameters.PreAuthAmntUDF,
                                                                                        FieldValue = paymentHeader.Amount
                                                                                    }
                                                                                }
                                                    }
                                                }, "pre checked-out fetch", fetchReservationRequest.ServiceParameters);
                                                if (!owsResponse.result)
                                                {
                                                    new LogHelper().Log("Updating pre auth code and amount in UDF fileds failled with reason : - " + owsResponse.responseMessage, reservation.ReservationNameID, "FetchDueOutReservation",  fetchReservationRequest.ServiceParameters.ClientID, "pre checked-out fetch");
                                                    new LogHelper().Warn("Updating pre auth code and amount in UDF fileds failled with reason : - " + owsResponse.responseMessage, reservation.ReservationNameID, "FetchDueOutReservation",  fetchReservationRequest.ServiceParameters.ClientID, "pre checked-out fetch");
                                                }
                                                else
                                                    new LogHelper().Log("Updating pre auth code and amount in UDF fileds succeeded ", reservation.ReservationNameID, "FetchDueOutReservation",  fetchReservationRequest.ServiceParameters.ClientID, "pre checked-out fetch");
                                            }
                                            catch (Exception ex)
                                            {
                                                new LogHelper().Error(ex, reservation.ReservationNameID, "FetchDueOutReservation",  fetchReservationRequest.ServiceParameters.ClientID, "pre checked-out fetch");
                                            }
                                            #endregion

                                        }
                                        else if (paymentDetails.paymentHeaders[x].TransactionType.Equals(Models.Local.TransactionType.Sale.ToString()))
                                        {
                                            paymentDetails.paymentHeaders[x].IsActive = false;

                                            #region Updating UDF fields in Opera reservation
                                            try
                                            {
                                                new LogHelper().Log("Updating pre auth code and amount in UDF fileds", reservation.ReservationNameID, "FetchDueOutReservation",  fetchReservationRequest.ServiceParameters.ClientID, "pre checked-out fetch");
                                                Models.OWS.OwsResponseModel owsResponse = await new WSClientHelper().ModifyBooking(reservation.ReservationNameID, new Models.OWS.OwsRequestModel()
                                                {
                                                    ChainCode = fetchReservationRequest.ServiceParameters.ChainCode,
                                                    DestinationEntityID = fetchReservationRequest.ServiceParameters.DestinationEntityID,
                                                    HotelDomain = fetchReservationRequest.ServiceParameters.HotelDomain,
                                                    KioskID = fetchReservationRequest.ServiceParameters.KioskID,
                                                    Language = fetchReservationRequest.ServiceParameters.Language,
                                                    LegNumber = fetchReservationRequest.ServiceParameters.Legnumber,
                                                    Password = fetchReservationRequest.ServiceParameters.Password,
                                                    SystemType = fetchReservationRequest.ServiceParameters.SystemType,
                                                    Username = fetchReservationRequest.ServiceParameters.Username,
                                                    modifyBookingRequest = new Models.OWS.ModifyBookingRequest()
                                                    {
                                                        isUDFFieldSpecified = true,
                                                        ReservationNumber = reservation.ReservationNumber,
                                                        uDFFields = new List<Models.OWS.UDFField>()
                                                                                {
                                                                                    new Models.OWS.UDFField()
                                                                                    {
                                                                                        FieldName  = fetchReservationRequest.ServiceParameters.PreAuthUDF,
                                                                                        FieldValue = paymentHeader.pspReferenceNumber
                                                                                    },
                                                                                    new Models.OWS.UDFField()
                                                                                    {
                                                                                        FieldName  = fetchReservationRequest.ServiceParameters.PreAuthAmntUDF,
                                                                                        FieldValue = paymentHeader.Amount
                                                                                    }
                                                                                }
                                                    }
                                                }, "pre checked-out fetch", fetchReservationRequest.ServiceParameters);
                                                if (!owsResponse.result)
                                                {
                                                    new LogHelper().Log("Updating pre auth code and amount in UDF fileds failled with reason : - " + owsResponse.responseMessage, reservation.ReservationNameID, "FetchDueOutReservation",  fetchReservationRequest.ServiceParameters.ClientID, "pre checked-out fetch");
                                                    new LogHelper().Warn("Updating pre auth code and amount in UDF fileds failled with reason : - " + owsResponse.responseMessage, reservation.ReservationNameID, "FetchDueOutReservation",  fetchReservationRequest.ServiceParameters.ClientID, "pre checked-out fetch");
                                                }
                                                else
                                                    new LogHelper().Log("Updating pre auth code and amount in UDF fileds succeeded ", reservation.ReservationNameID, "FetchDueOutReservation",  fetchReservationRequest.ServiceParameters.ClientID, "pre checked-out fetch");
                                            }
                                            catch (Exception ex)
                                            {
                                                new LogHelper().Error(ex, reservation.ReservationNameID, "FetchPreCheckedInReservation",  fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                                            }
                                            #endregion

                                            #region Posting payment in opera reservation
                                            try
                                            {
                                                new LogHelper().Log("Posting payment in the reservation", reservation.ReservationNameID, "FetchDueOutReservation",  fetchReservationRequest.ServiceParameters.ClientID, "pre checked-out fetch");
                                                Models.OWS.OwsResponseModel owsResponse = await new WSClientHelper().MakePayment(reservation.ReservationNameID, new Models.OWS.OwsRequestModel()
                                                {
                                                    ChainCode = fetchReservationRequest.ServiceParameters.ChainCode,
                                                    DestinationEntityID = fetchReservationRequest.ServiceParameters.DestinationEntityID,
                                                    HotelDomain = fetchReservationRequest.ServiceParameters.HotelDomain,
                                                    KioskID = fetchReservationRequest.ServiceParameters.KioskID,
                                                    Language = fetchReservationRequest.ServiceParameters.Language,
                                                    LegNumber = fetchReservationRequest.ServiceParameters.Legnumber,
                                                    Password = fetchReservationRequest.ServiceParameters.Password,
                                                    SystemType = fetchReservationRequest.ServiceParameters.SystemType,
                                                    Username = fetchReservationRequest.ServiceParameters.Username,
                                                    MakePaymentRequest = new Models.OWS.MakePaymentRequest()
                                                    {
                                                        Amount = Convert.ToDecimal(paymentHeader.Amount),
                                                        PaymentInfo = "Auth code - (" + paymentHeader.pspReferenceNumber + ")",
                                                        StationID = "MCI",
                                                        WindowNumber = 1,
                                                        ReservationNameID = reservation.ReservationNameID,
                                                        MaskedCardNumber = paymentHeader.MaskedCardNumber.ToLower(),
                                                        PaymentRefernce = "web checkin - (" + paymentHeader.MaskedCardNumber + ")",
                                                        PaymentTypeCode = paymentHeader.OperaPaymentTypeCode,
                                                        ApprovalCode = paymentHeader.pspReferenceNumber
                                                    }
                                                }, "pre checked-out fetch", fetchReservationRequest.ServiceParameters);
                                                if (!owsResponse.result)
                                                {
                                                    new LogHelper().Log("Posting payment failled with reason : - " + owsResponse.responseMessage, reservation.ReservationNameID, "FetchDueOutReservation",  fetchReservationRequest.ServiceParameters.ClientID, "pre checked-out fetch");
                                                    new LogHelper().Warn("Posting payment failled with reason : - " + owsResponse.responseMessage, reservation.ReservationNameID, "FetchDueOutReservation",  fetchReservationRequest.ServiceParameters.ClientID, "pre checked-out fetch");
                                                }
                                                else
                                                    new LogHelper().Log("Posting payment in opera reservation succeeded ", reservation.ReservationNameID, "FetchDueOutReservation",  fetchReservationRequest.ServiceParameters.ClientID, "pre checked-out fetch");
                                            }
                                            catch (Exception ex)
                                            {
                                                new LogHelper().Error(ex, reservation.ReservationNameID, "FetchDueOutReservation",  fetchReservationRequest.ServiceParameters.ClientID, "pre checked-out fetch");
                                            }
                                            #endregion
                                        }
                                        else if (paymentDetails.paymentHeaders[x].TransactionType.Equals(Models.Local.TransactionType.Capture.ToString()))
                                        {
                                            paymentDetails.paymentHeaders[x].IsActive = false;

                                            #region Updating UDF fields in Opera reservation
                                            try
                                            {
                                                new LogHelper().Log("Updating pre auth code and amount in UDF fileds", reservation.ReservationNameID, "FetchDueOutReservation",  fetchReservationRequest.ServiceParameters.ClientID, "pre checked-out fetch");
                                                Models.OWS.OwsResponseModel owsResponse = await new WSClientHelper().ModifyBooking(reservation.ReservationNameID, new Models.OWS.OwsRequestModel()
                                                {
                                                    ChainCode = fetchReservationRequest.ServiceParameters.ChainCode,
                                                    DestinationEntityID = fetchReservationRequest.ServiceParameters.DestinationEntityID,
                                                    HotelDomain = fetchReservationRequest.ServiceParameters.HotelDomain,
                                                    KioskID = fetchReservationRequest.ServiceParameters.KioskID,
                                                    Language = fetchReservationRequest.ServiceParameters.Language,
                                                    LegNumber = fetchReservationRequest.ServiceParameters.Legnumber,
                                                    Password = fetchReservationRequest.ServiceParameters.Password,
                                                    SystemType = fetchReservationRequest.ServiceParameters.SystemType,
                                                    Username = fetchReservationRequest.ServiceParameters.Username,
                                                    modifyBookingRequest = new Models.OWS.ModifyBookingRequest()
                                                    {
                                                        isUDFFieldSpecified = true,
                                                        ReservationNumber = reservation.ReservationNumber,
                                                        uDFFields = new List<Models.OWS.UDFField>()
                                                                                {
                                                                                    new Models.OWS.UDFField()
                                                                                    {
                                                                                        FieldName  = fetchReservationRequest.ServiceParameters.PreAuthUDF,
                                                                                        FieldValue = paymentHeader.pspReferenceNumber
                                                                                    },
                                                                                    new Models.OWS.UDFField()
                                                                                    {
                                                                                        FieldName  = fetchReservationRequest.ServiceParameters.PreAuthAmntUDF,
                                                                                        FieldValue = paymentHeader.Amount
                                                                                    }
                                                                                }
                                                    }
                                                }, "pre checked-out fetch", fetchReservationRequest.ServiceParameters);
                                                if (!owsResponse.result)
                                                {
                                                    new LogHelper().Log("Updating pre auth code and amount in UDF fileds failled with reason : - " + owsResponse.responseMessage, reservation.ReservationNameID, "FetchDueOutReservation",  fetchReservationRequest.ServiceParameters.ClientID, "pre checked-out fetch");
                                                    new LogHelper().Warn("Updating pre auth code and amount in UDF fileds failled with reason : - " + owsResponse.responseMessage, reservation.ReservationNameID, "FetchDueOutReservation",  fetchReservationRequest.ServiceParameters.ClientID, "pre checked-out fetch");
                                                }
                                                else
                                                    new LogHelper().Log("Updating pre auth code and amount in UDF fileds succeeded ", reservation.ReservationNameID, "FetchDueOutReservation",  fetchReservationRequest.ServiceParameters.ClientID, "pre checked-out fetch");
                                            }
                                            catch (Exception ex)
                                            {
                                                new LogHelper().Error(ex, reservation.ReservationNameID, "FetchPreCheckedInReservation",  fetchReservationRequest.ServiceParameters.ClientID, "pre checked-in fetch");
                                            }
                                            #endregion

                                            #region Posting payment in opera reservation
                                            try
                                            {
                                                new LogHelper().Log("Posting payment in the reservation", reservation.ReservationNameID, "FetchDueOutReservation",  fetchReservationRequest.ServiceParameters.ClientID, "pre checked-out fetch");
                                                Models.OWS.OwsResponseModel owsResponse = await new WSClientHelper().MakePayment(reservation.ReservationNameID, new Models.OWS.OwsRequestModel()
                                                {
                                                    ChainCode = fetchReservationRequest.ServiceParameters.ChainCode,
                                                    DestinationEntityID = fetchReservationRequest.ServiceParameters.DestinationEntityID,
                                                    HotelDomain = fetchReservationRequest.ServiceParameters.HotelDomain,
                                                    KioskID = fetchReservationRequest.ServiceParameters.KioskID,
                                                    Language = fetchReservationRequest.ServiceParameters.Language,
                                                    LegNumber = fetchReservationRequest.ServiceParameters.Legnumber,
                                                    Password = fetchReservationRequest.ServiceParameters.Password,
                                                    SystemType = fetchReservationRequest.ServiceParameters.SystemType,
                                                    Username = fetchReservationRequest.ServiceParameters.Username,
                                                    MakePaymentRequest = new Models.OWS.MakePaymentRequest()
                                                    {
                                                        Amount = Convert.ToDecimal(paymentHeader.Amount),
                                                        PaymentInfo = "Auth code - (" + paymentHeader.AuthorisationCode + ")",
                                                        StationID = "MCI",
                                                        WindowNumber = 1,
                                                        ReservationNameID = reservation.ReservationNameID,
                                                        MaskedCardNumber = paymentHeader.MaskedCardNumber.ToLower(),
                                                        PaymentRefernce = "web checkin - (" + paymentHeader.MaskedCardNumber + ")",
                                                        PaymentTypeCode = paymentHeader.OperaPaymentTypeCode,
                                                        ApprovalCode = paymentHeader.pspReferenceNumber
                                                    }
                                                }, "pre checked-out fetch", fetchReservationRequest.ServiceParameters);
                                                if (!owsResponse.result)
                                                {
                                                    new LogHelper().Log("Posting payment failled with reason : - " + owsResponse.responseMessage, reservation.ReservationNameID, "FetchDueOutReservation",  fetchReservationRequest.ServiceParameters.ClientID, "pre checked-out fetch");
                                                    new LogHelper().Warn("Posting payment failled with reason : - " + owsResponse.responseMessage, reservation.ReservationNameID, "FetchDueOutReservation",  fetchReservationRequest.ServiceParameters.ClientID, "pre checked-out fetch");
                                                }
                                                else
                                                    new LogHelper().Log("Posting payment in opera reservation succeeded ", reservation.ReservationNameID, "FetchDueOutReservation",  fetchReservationRequest.ServiceParameters.ClientID, "pre checked-out fetch");
                                            }
                                            catch (Exception ex)
                                            {
                                                new LogHelper().Error(ex, reservation.ReservationNameID, "FetchDueOutReservation",  fetchReservationRequest.ServiceParameters.ClientID, "pre checked-out fetch");
                                            }
                                            #endregion
                                        }

                                        #endregion
                                    }
                                    x++;
                                }
                                //new LogHelper().Log("payment details updated successfully", reservation.ReservationNameID, "FetchDueOutReservation",  fetchReservationRequest.ServiceParameters.ClientID, "pre checked-out fetch");
                            }
                        }
                        #endregion

                        #region Pushing Payment details in LOcal Db

                        new LogHelper().Log("Updating payment details in local DB", reservation.ReservationNameID, "FetchDueOutReservation",  fetchReservationRequest.ServiceParameters.ClientID, "pre checked-out fetch");
                        Models.Local.LocalResponseModel localResponse = await new WSClientHelper().PushPaymentDetails(reservation.ReservationNameID, new Models.Local.LocalRequestModel()
                        {
                            RequestObject = paymentDetails
                        }, "pre checked-out fetch", fetchReservationRequest.ServiceParameters);
                        if (!localResponse.result)
                        {
                            new LogHelper().Log("Failled to update payment details in Local DB with reason :- " + localResponse.responseMessage, reservation.ReservationNameID, "FetchDueOutReservation",  fetchReservationRequest.ServiceParameters.ClientID, "pre checked-out fetch");
                            new LogHelper().Warn("Failled to update payment details in local DB with reason :- " + localResponse.responseMessage, reservation.ReservationNameID, "FetchDueOutReservation",  fetchReservationRequest.ServiceParameters.ClientID, "pre checked-out fetch");
                        }
                        else
                            new LogHelper().Log("Payment details updated successfully", reservation.ReservationNameID, "FetchDueOutReservation",  fetchReservationRequest.ServiceParameters.ClientID, "pre checked-out fetch");
                        #endregion

                        #region Fetching Reservation from OWS
                        new LogHelper().Log("Fetching opera reservation", reservation.ReservationNameID, "FetchDueOutReservation",  fetchReservationRequest.ServiceParameters.ClientID, "pre checked-out fetch");
                        Models.OWS.OwsResponseModel owsResponse1 = await new WSClientHelper().FetchReservationAsync(reservation.ReservationNameID, new Models.OWS.OwsRequestModel()
                        {
                            ChainCode = fetchReservationRequest.ServiceParameters.ChainCode,
                            DestinationEntityID = fetchReservationRequest.ServiceParameters.DestinationEntityID,
                            HotelDomain = fetchReservationRequest.ServiceParameters.HotelDomain,
                            KioskID = fetchReservationRequest.ServiceParameters.KioskID,
                            Language = fetchReservationRequest.ServiceParameters.Language,
                            LegNumber = fetchReservationRequest.ServiceParameters.Legnumber,
                            Password = fetchReservationRequest.ServiceParameters.Password,
                            SystemType = fetchReservationRequest.ServiceParameters.SystemType,
                            Username = fetchReservationRequest.ServiceParameters.Username,
                            FetchBookingRequest = new Models.OWS.FetchBookingRequestModel()
                            {
                                ReservationNumber = reservation.ReservationNumber
                            }
                        }, "pre checked-out fetch", fetchReservationRequest.ServiceParameters);

                        if (!owsResponse1.result)
                        {
                            new LogHelper().Log("Failled to fetch opera reservation with reason :- " + owsResponse1.responseMessage, reservation.ReservationNameID, "FetchDueOutReservation",  fetchReservationRequest.ServiceParameters.ClientID, "pre checked-out fetch");
                            new LogHelper().Warn("Failled to fetch opera reservation with reason :- " + owsResponse1.responseMessage, reservation.ReservationNameID, "FetchDueOutReservation",  fetchReservationRequest.ServiceParameters.ClientID, "pre checked-out fetch");
                        }


                        if (owsResponse1.responseData == null)
                        {
                            new LogHelper().Log("Failled to fetch opera reservation with reason :- API response data is NULL" + owsResponse1.responseMessage, reservation.ReservationNameID, "FetchDueOutReservation",  fetchReservationRequest.ServiceParameters.ClientID, "pre checked-out fetch");
                            new LogHelper().Warn("Failled to fetch opera reservation with reason :- API response data is NULL" + owsResponse1.responseMessage, reservation.ReservationNameID, "FetchDueOutReservation",  fetchReservationRequest.ServiceParameters.ClientID, "pre checked-out fetch");
                        }
                        else
                        {
                            new LogHelper().Debug("Converting API json to object", reservation.ReservationNameID, "FetchDueOutReservation",  fetchReservationRequest.ServiceParameters.ClientID, "pre checked-out fetch");
                            try
                            {
                                operaReservations = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Models.OWS.OperaReservation>>(owsResponse1.responseData.ToString());
                                new LogHelper().Log("Opera reservation fetched successfully", reservation.ReservationNameID, "FetchDueOutReservation",  fetchReservationRequest.ServiceParameters.ClientID, "pre checked-out fetch");
                            }
                            catch (Exception ex)
                            {
                                new LogHelper().Error(ex, reservation.ReservationNameID, "FetchDueOutReservation",  fetchReservationRequest.ServiceParameters.ClientID, "pre checked-out fetch");
                                new LogHelper().Log("Failled to covert API response to object", reservation.ReservationNameID, "FetchDueOutReservation",  fetchReservationRequest.ServiceParameters.ClientID, "pre checked-out fetch");
                                new LogHelper().Warn("Failled to fetch opera reservation with reason :- " + ex.Message, reservation.ReservationNameID, "FetchDueOutReservation",  fetchReservationRequest.ServiceParameters.ClientID, "pre checked-out fetch");
                                new LogHelper().Debug("Failled to fetch opera reservation with reason :- " + ex.Message, reservation.ReservationNameID, "FetchDueOutReservation",  fetchReservationRequest.ServiceParameters.ClientID, "pre checked-out fetch");
                            }
                        }

                        #endregion

                        #region FetchFolioItemsByWindow
                        new LogHelper().Log("Fetching reservation folio by window for reservation No. : " + operaReservations[0].ReservationNumber, operaReservations[0].ReservationNameID, "FetchDueOutReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-out fetch");
                        owsResponse1 = await new WSClientHelper().GetFolioByWindow(operaReservations[0].ReservationNameID, new Models.OWS.OwsRequestModel()
                        {
                            ChainCode = fetchReservationRequest.ServiceParameters.ChainCode,
                            DestinationEntityID = fetchReservationRequest.ServiceParameters.DestinationEntityID,
                            HotelDomain = fetchReservationRequest.ServiceParameters.HotelDomain,
                            KioskID = fetchReservationRequest.ServiceParameters.KioskID,
                            Language = fetchReservationRequest.ServiceParameters.Language,
                            LegNumber = fetchReservationRequest.ServiceParameters.Legnumber,
                            Password = fetchReservationRequest.ServiceParameters.Password,
                            SystemType = fetchReservationRequest.ServiceParameters.SystemType,
                            Username = fetchReservationRequest.ServiceParameters.Username,
                            FetchFolioRequest = new Models.OWS.FetchFolioRequest()
                            {
                                ReservationNameID = operaReservations[0].ReservationNameID,
                                ProfileID = (operaReservations[0].GuestProfiles != null && operaReservations[0].GuestProfiles.Count > 0) ? operaReservations[0].GuestProfiles[0].PmsProfileID : ""
                            }
                        }, "pre checked-out fetch", fetchReservationRequest.ServiceParameters);

                        if (!owsResponse1.result || owsResponse1.responseData == null)
                        {
                            new LogHelper().Log("Failled to fetch folio by window with reason :- " + owsResponse1.responseMessage, operaReservations[0].ReservationNameID, "FetchDueOutReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-out fetch");
                            new LogHelper().Warn("Failled to fetch folio by window with reason :- " + owsResponse1.responseMessage, operaReservations[0].ReservationNameID, "FetchDueOutReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-out fetch");
                            continue;
                        }
                        else
                        {
                            new LogHelper().Debug("Converting API json to object", operaReservations[0].ReservationNameID, "FetchDueOutReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-out fetch");
                            try
                            {
                                guestFolio = JsonConvert.DeserializeObject<Models.OWS.FolioModel>(owsResponse1.responseData.ToString());
                                new LogHelper().Log("Current guest balance of the reservation is : " + guestFolio.BalanceAmount, operaReservations[0].ReservationNameID, "FetchDueOutReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-out fetch");
                                new LogHelper().Log("Reservation folio by window fetched successfully", operaReservations[0].ReservationNameID, "FetchDueOutReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-out fetch");
                            }
                            catch (Exception ex)
                            {
                                new LogHelper().Error(ex, operaReservations[0].ReservationNameID, "FetchDueOutReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-out fetch");
                                new LogHelper().Log("Failled to covert API response to object", operaReservations[0].ReservationNameID, "FetchDueOutReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-out fetch");
                                new LogHelper().Warn("Failled to fetch folio by window with reason :- " + ex.Message, operaReservations[0].ReservationNameID, "FetchDueOutReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-out fetch");
                                new LogHelper().Debug("Failled to fetch folio by window with reason :- " + ex.Message, operaReservations[0].ReservationNameID, "FetchDueOutReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-out fetch");
                            }
                        }
                        #endregion

                        #region FetchFolioAsBase64
                        if (fetchReservationRequest.ServiceParameters.sendFolioFromOpera != null && !fetchReservationRequest.ServiceParameters.sendFolioFromOpera.Value)
                        {
                            new LogHelper().Log("Fetching reservation folio as base64 for reservation No. : " + reservation.ReservationNumber, reservation.ReservationNameID, "FetchDueOutReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-out fetch");
                            owsResponse1 = await new WSClientHelper().GetFolio(reservation.ReservationNameID, new Models.OWS.OwsRequestModel()
                            {
                                ChainCode = fetchReservationRequest.ServiceParameters.ChainCode,
                                DestinationEntityID = fetchReservationRequest.ServiceParameters.DestinationEntityID,
                                HotelDomain = fetchReservationRequest.ServiceParameters.HotelDomain,
                                KioskID = fetchReservationRequest.ServiceParameters.KioskID,
                                Language = fetchReservationRequest.ServiceParameters.Language,
                                LegNumber = fetchReservationRequest.ServiceParameters.Legnumber,
                                Password = fetchReservationRequest.ServiceParameters.Password,
                                SystemType = fetchReservationRequest.ServiceParameters.SystemType,
                                Username = fetchReservationRequest.ServiceParameters.Username,
                                FetchFolioRequest = new Models.OWS.FetchFolioRequest()
                                {
                                    ReservationNameID = reservation.ReservationNameID,
                                    OperaReservation = operaReservations[0],
                                    GuestSignature = reservation.GuestSignature,
                                    ProfileID = (operaReservations[0].GuestProfiles != null && operaReservations[0].GuestProfiles.Count > 0) ? operaReservations[0].GuestProfiles[0].PmsProfileID : "",
                                    FolioList = guestFolio

                                }
                            }, "pre checked-out fetch", fetchReservationRequest.ServiceParameters);

                            if (!owsResponse1.result || owsResponse1.responseData == null)
                            {
                                new LogHelper().Log("Failled to fetch folio with reason :- " + owsResponse1.responseMessage, reservation.ReservationNameID, "FetchDueOutReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-out fetch");
                                new LogHelper().Warn("Failled to fetch folio with reason :- " + owsResponse1.responseMessage, reservation.ReservationNameID, "FetchDueOutReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-out fetch");
                                System.Threading.Thread.Sleep(60000);

                                #region try again
                                new LogHelper().Log("Fetching reservation folio as base64 for reservation No. : " + reservation.ReservationNumber + " again after 1 minute", reservation.ReservationNameID, "FetchDueOutReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-out fetch");
                                owsResponse1 = await new WSClientHelper().GetFolio(reservation.ReservationNameID, new Models.OWS.OwsRequestModel()
                                {
                                    ChainCode = fetchReservationRequest.ServiceParameters.ChainCode,
                                    DestinationEntityID = fetchReservationRequest.ServiceParameters.DestinationEntityID,
                                    HotelDomain = fetchReservationRequest.ServiceParameters.HotelDomain,
                                    KioskID = fetchReservationRequest.ServiceParameters.KioskID,
                                    Language = fetchReservationRequest.ServiceParameters.Language,
                                    LegNumber = fetchReservationRequest.ServiceParameters.Legnumber,
                                    Password = fetchReservationRequest.ServiceParameters.Password,
                                    SystemType = fetchReservationRequest.ServiceParameters.SystemType,
                                    Username = fetchReservationRequest.ServiceParameters.Username,
                                    FetchFolioRequest = new Models.OWS.FetchFolioRequest()
                                    {
                                        ReservationNameID = reservation.ReservationNameID,
                                        OperaReservation = operaReservations[0],
                                        GuestSignature = reservation.GuestSignature,
                                        ProfileID = (operaReservations[0].GuestProfiles != null && operaReservations[0].GuestProfiles.Count > 0) ? operaReservations[0].GuestProfiles[0].PmsProfileID : ""
                                    }
                                }, "pre checked-out fetch", fetchReservationRequest.ServiceParameters);

                                if (!owsResponse1.result || owsResponse1.responseData == null)
                                {
                                    new LogHelper().Log("Failled to fetch folio with reason :- " + owsResponse1.responseMessage, reservation.ReservationNameID, "FetchDueOutReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-out fetch");
                                    new LogHelper().Warn("Failled to fetch folio with reason :- " + owsResponse1.responseMessage, reservation.ReservationNameID, "FetchDueOutReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-out fetch");
                                }
                                #endregion

                                //new LogHelper().Log("Skipping the reservation since the fetching folio failled", reservation.ReservationNameID, "FetchDueOutReservation",  fetchReservationRequest.ServiceParameters.ClientID, "pre checked-out fetch");
                                //continue;
                            }
                            else
                            {
                                folioAsBase64 = owsResponse1.responseData.ToString();
                                new LogHelper().Log("Fetched guest folio as base64 successfully", reservation.ReservationNameID, "FetchDueOutReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-out fetch");
                            }
                        }
                        #endregion

                        #region UpdateEmailInProfile
                        new LogHelper().Debug("Verifying the email to send is different from profile or not", reservation.ReservationNameID, "FetchDueOutReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-out fetch");
                        {
                            if(fetchReservationRequest.ServiceParameters.sendFolioFromOpera != null && fetchReservationRequest.ServiceParameters.sendFolioFromOpera.Value && operaReservations != null && operaReservations.Count > 0 && operaReservations[0].GuestProfiles != null && operaReservations[0].GuestProfiles.Count > 0
                                && operaReservations[0].GuestProfiles[0].Email != null && operaReservations[0].GuestProfiles[0].Email.Count > 0)
                            {
                                var email = operaReservations[0].GuestProfiles[0].Email.Find(x => x.primary != null && x.primary.Value);
                                if(email != null && !string.IsNullOrEmpty(email.email) && !email.email.Equals(reservation.FolioEmail))
                                {
                                    #region Email

                                    new LogHelper().Log("Updating email in opera ", reservation.ReservationNameID, "FetchDueOutReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-out fetch");
                                    Models.OWS.OwsResponseModel owsResponse = await new WSClientHelper().UpdateProfileEmailAsync(reservation.ReservationNameID, new Models.OWS.OwsRequestModel()
                                    {
                                        ChainCode = fetchReservationRequest.ServiceParameters.ChainCode,
                                        DestinationEntityID = fetchReservationRequest.ServiceParameters.DestinationEntityID,
                                        HotelDomain = fetchReservationRequest.ServiceParameters.HotelDomain,
                                        KioskID = fetchReservationRequest.ServiceParameters.KioskID,
                                        Language = fetchReservationRequest.ServiceParameters.Language,
                                        LegNumber = fetchReservationRequest.ServiceParameters.Legnumber,
                                        Password = fetchReservationRequest.ServiceParameters.Password,
                                        SystemType = fetchReservationRequest.ServiceParameters.SystemType,
                                        Username = fetchReservationRequest.ServiceParameters.Username,
                                        UpdateProileRequest = new Models.OWS.UpdateProfile()
                                        {
                                            ProfileID = operaReservations[0].GuestProfiles[0].PmsProfileID,
                                            Emails = new List<Models.OWS.Email>()
                                        {
                                            new Models.OWS.Email()
                                            {
                                                email = operaReservations[0].GuestProfiles[0].Email[0].email,
                                                //emailType = "BUSINESS",
                                                displaySequence = 1,
                                                primary = true
                                            }
                                        }
                                        }
                                    }, "pre checked-out fetch", fetchReservationRequest.ServiceParameters);
                                    if (!owsResponse.result)
                                    {
                                        new LogHelper().Log("Failled to update email info with reason :- " + owsResponse.responseMessage, reservation.ReservationNameID, "FetchDueOutReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-out fetch");
                                        new LogHelper().Warn("Failled to update email info with reason :- " + owsResponse.responseMessage, reservation.ReservationNameID, "FetchDueOutReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-out fetch");
                                    }
                                    else
                                    {
                                        new LogHelper().Log("Updated email successfully", reservation.ReservationNameID, "FetchDueOutReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-out fetch");
                                    }


                                    #endregion
                                }
                            }
                        }
                        #endregion

                        #region Checkout Reservation

                        if (fetchReservationRequest.ServiceParameters.isAutoCheckOutEnabled != null && fetchReservationRequest.ServiceParameters.isAutoCheckOutEnabled.Value)
                        {
                            new LogHelper().Log("Processing reservation No. : " + operaReservations[0].ReservationNumber + " to do check out", operaReservations[0].ReservationNameID, "FetchDueOutReservation",  fetchReservationRequest.ServiceParameters.ClientID, "pre checked-out fetch");
                            if (guestFolio != null)
                            {
                                new LogHelper().Log("verifying guest balance : " + guestFolio.BalanceAmount, operaReservations[0].ReservationNameID, "FetchDueOutReservation",  fetchReservationRequest.ServiceParameters.ClientID, "pre checked-out fetch");
                                if (guestFolio.BalanceAmount > 0)
                                {
                                    #region Pushing Reservation Track

                                    new LogHelper().Log("Pushing reservation track in local DB ", operaReservations[0].ReservationNameID, "FetchDueOutReservation",  fetchReservationRequest.ServiceParameters.ClientID, "pre checked-out fetch");

                                    localResponse = await new WSClientHelper().PushReservationTrackLocally(operaReservations[0].ReservationNameID, new Models.Local.LocalRequestModel()
                                    {
                                        RequestObject = new Models.Local.ReservationTrackStatus()
                                        {
                                            ReservationNameID = operaReservations[0].ReservationNameID,
                                            ProcessType = Models.Local.ReservationProcessType.CheckoutFailled.ToString(),
                                            ReservationNumber = operaReservations[0].ReservationNumber,
                                            ProcessStatus = "",
                                            EmailSent = false
                                        }
                                    }, "pre checked-out fetch", fetchReservationRequest.ServiceParameters);
                                    if (localResponse.result)
                                    {
                                        new LogHelper().Log("Reservation track in local DB updated successfully ", operaReservations[0].ReservationNameID, "FetchDueOutReservation",  fetchReservationRequest.ServiceParameters.ClientID, "pre checked-out fetch");
                                    }
                                    else
                                    {
                                        new LogHelper().Log("Failled to update reservation track in local DB with reason :- " + localResponse.responseMessage, operaReservations[0].ReservationNameID, "FetchDueOutReservation",  fetchReservationRequest.ServiceParameters.ClientID, "pre checked-out fetch");
                                    }

                                    #endregion

                                    new LogHelper().Log("Failled to process check out, where the guest balance is greater than 0", operaReservations[0].ReservationNameID, "FetchDueOutReservation",  fetchReservationRequest.ServiceParameters.ClientID, "pre checked-out fetch");
                                }
                                else
                                {
                                    new LogHelper().Log("verifying reservation balance : " + guestFolio.ReservationBalance + " and isallowed to check out flag", operaReservations[0].ReservationNameID, "FetchDueOutReservation",  fetchReservationRequest.ServiceParameters.ClientID, "pre checked-out fetch");

                                    if (guestFolio.ReservationBalance > 0)
                                    {
                                        #region Pushing Reservation Track

                                        new LogHelper().Log("Pushing reservation track in local DB ", operaReservations[0].ReservationNameID, "FetchDueOutReservation",  fetchReservationRequest.ServiceParameters.ClientID, "pre checked-out fetch");

                                        localResponse = await new WSClientHelper().PushReservationTrackLocally(operaReservations[0].ReservationNameID, new Models.Local.LocalRequestModel()
                                        {
                                            RequestObject = new Models.Local.ReservationTrackStatus()
                                            {
                                                ReservationNameID = operaReservations[0].ReservationNameID,
                                                ProcessType = Models.Local.ReservationProcessType.CheckoutFailled.ToString(),
                                                ReservationNumber = operaReservations[0].ReservationNumber,
                                                ProcessStatus = "",
                                                EmailSent = false
                                            }
                                        }, "pre checked-out fetch", fetchReservationRequest.ServiceParameters);
                                        if (localResponse.result)
                                        {
                                            new LogHelper().Log("Reservation track in local DB updated successfully ", operaReservations[0].ReservationNameID, "FetchDueOutReservation",  fetchReservationRequest.ServiceParameters.ClientID, "pre checked-out fetch");
                                        }
                                        else
                                        {
                                            new LogHelper().Log("Failled to update reservation track in local DB with reason :- " + localResponse.responseMessage, operaReservations[0].ReservationNameID, "FetchDueOutReservation",  fetchReservationRequest.ServiceParameters.ClientID, "pre checked-out fetch");
                                        }

                                        #endregion

                                        new LogHelper().Log("Failled to process check out, where the reservation balance is greater than 0", operaReservations[0].ReservationNameID, "FetchDueOutReservation",  fetchReservationRequest.ServiceParameters.ClientID, "pre checked-out fetch");
                                    }
                                    else
                                    {
                                        if (guestFolio.IsAllowedForCheckOut != null && guestFolio.IsAllowedForCheckOut.Value)
                                        {

                                            bool isAllowedForCheckout = true;
                                            if (guestFolio != null && guestFolio.FolioWindows != null && guestFolio.FolioWindows.Count > 0)
                                            {
                                                int x = 0;
                                                foreach (var folio in guestFolio.FolioWindows)
                                                {
                                                    if (x > 0)
                                                    {
                                                        if (folio.BalanceAmount != 0)
                                                        {
                                                            isAllowedForCheckout = false;
                                                        }
                                                    }
                                                    x++;
                                                }
                                            }


                                            if (isAllowedForCheckout)
                                            {
                                                owsResponse1 = await new WSClientHelper().CheckoutReservation(operaReservations[0].ReservationNameID, new Models.OWS.OwsRequestModel()
                                                {
                                                    ChainCode = fetchReservationRequest.ServiceParameters.ChainCode,
                                                    DestinationEntityID = fetchReservationRequest.ServiceParameters.DestinationEntityID,
                                                    HotelDomain = fetchReservationRequest.ServiceParameters.HotelDomain,
                                                    KioskID = fetchReservationRequest.ServiceParameters.KioskID,
                                                    Language = fetchReservationRequest.ServiceParameters.Language,
                                                    LegNumber = fetchReservationRequest.ServiceParameters.Legnumber,
                                                    Password = fetchReservationRequest.ServiceParameters.Password,
                                                    SystemType = fetchReservationRequest.ServiceParameters.SystemType,
                                                    Username = fetchReservationRequest.ServiceParameters.Username,
                                                    SendFolio = fetchReservationRequest.ServiceParameters.sendFolioFromOpera != null && fetchReservationRequest.ServiceParameters.sendFolioFromOpera.Value ? true : false,
                                                    OperaReservation = new Models.OWS.OperaReservation()
                                                    {
                                                        ReservationNameID = operaReservations[0].ReservationNameID
                                                    }
                                                }, "pre checked-out fetch", fetchReservationRequest.ServiceParameters);

                                                if (!owsResponse1.result || owsResponse1.responseData == null)
                                                {
                                                    #region Pushing Reservation Track

                                                    new LogHelper().Log("Pushing reservation track in local DB ", operaReservations[0].ReservationNameID, "FetchDueOutReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-out fetch");

                                                    localResponse = await new WSClientHelper().PushReservationTrackLocally(operaReservations[0].ReservationNameID, new Models.Local.LocalRequestModel()
                                                    {
                                                        RequestObject = new Models.Local.ReservationTrackStatus()
                                                        {
                                                            ReservationNameID = operaReservations[0].ReservationNameID,
                                                            ProcessType = Models.Local.ReservationProcessType.CheckoutFailled.ToString(),
                                                            ReservationNumber = operaReservations[0].ReservationNumber,
                                                            ProcessStatus = "",
                                                            EmailSent = false
                                                        }
                                                    }, "pre checked-out fetch", fetchReservationRequest.ServiceParameters);
                                                    if (localResponse.result)
                                                    {
                                                        new LogHelper().Log("Reservation track in local DB updated successfully ", operaReservations[0].ReservationNameID, "FetchDueOutReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-out fetch");
                                                    }
                                                    else
                                                    {
                                                        new LogHelper().Log("Failled to update reservation track in local DB with reason :- " + localResponse.responseMessage, operaReservations[0].ReservationNameID, "FetchDueOutReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-out fetch");
                                                    }

                                                    #endregion

                                                    new LogHelper().Log("Failled to check-out with reason :- " + owsResponse1.responseMessage, operaReservations[0].ReservationNameID, "FetchDueOutReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-out fetch");
                                                    new LogHelper().Warn("Failled to check-out with reason :- " + owsResponse1.responseMessage, operaReservations[0].ReservationNameID, "FetchDueOutReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-out fetch");

                                                }
                                                else
                                                {
                                                    new LogHelper().Log("Checked out successfully", operaReservations[0].ReservationNameID, "FetchDueOutReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-out fetch");

                                                    #region Pushing Reservation Track

                                                    new LogHelper().Log("Pushing reservation track in local DB ", operaReservations[0].ReservationNameID, "FetchDueOutReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-out fetch");

                                                    localResponse = await new WSClientHelper().PushReservationTrackLocally(operaReservations[0].ReservationNameID, new Models.Local.LocalRequestModel()
                                                    {
                                                        RequestObject = new Models.Local.ReservationTrackStatus()
                                                        {
                                                            ReservationNameID = operaReservations[0].ReservationNameID,
                                                            ProcessType = Models.Local.ReservationProcessType.CheckedoutSuccessfully.ToString(),
                                                            ReservationNumber = operaReservations[0].ReservationNumber,
                                                            ProcessStatus = "",
                                                            EmailSent = false
                                                        }
                                                    }, "pre checked-out fetch", fetchReservationRequest.ServiceParameters);
                                                    if (localResponse.result)
                                                    {
                                                        new LogHelper().Log("Reservation track in local DB updated successfully ", operaReservations[0].ReservationNameID, "FetchDueOutReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-out fetch");
                                                    }
                                                    else
                                                    {
                                                        new LogHelper().Log("Failled to update reservation track in local DB with reason :- " + localResponse.responseMessage, operaReservations[0].ReservationNameID, "FetchDueOutReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-out fetch");
                                                    }

                                                    #endregion

                                                    if (operaReservations[0].SharerReservations != null && operaReservations.Count > 0)
                                                    {
                                                        new LogHelper().Log("Processing sharers", operaReservations[0].ReservationNameID, "FetchDueOutReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-out fetch");
                                                        foreach (Models.OWS.OperaReservation sharer in operaReservations[0].SharerReservations)
                                                        {
                                                            new LogHelper().Log("Processing sharer reservation - " + sharer.ReservationNumber, operaReservations[0].ReservationNameID, "FetchDueOutReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-out fetch");

                                                            if (!string.IsNullOrEmpty(sharer.ReservationNameID))
                                                            {
                                                                owsResponse1 = await new WSClientHelper().CheckoutReservation(operaReservations[0].ReservationNameID, new Models.OWS.OwsRequestModel()
                                                                {
                                                                    ChainCode = fetchReservationRequest.ServiceParameters.ChainCode,
                                                                    DestinationEntityID = fetchReservationRequest.ServiceParameters.DestinationEntityID,
                                                                    HotelDomain = fetchReservationRequest.ServiceParameters.HotelDomain,
                                                                    KioskID = fetchReservationRequest.ServiceParameters.KioskID,
                                                                    Language = fetchReservationRequest.ServiceParameters.Language,
                                                                    LegNumber = fetchReservationRequest.ServiceParameters.Legnumber,
                                                                    Password = fetchReservationRequest.ServiceParameters.Password,
                                                                    SystemType = fetchReservationRequest.ServiceParameters.SystemType,
                                                                    Username = fetchReservationRequest.ServiceParameters.Username,
                                                                    OperaReservation = new Models.OWS.OperaReservation()
                                                                    {
                                                                        ReservationNameID = sharer.ReservationNameID
                                                                    }
                                                                }, "pre checked-out fetch", fetchReservationRequest.ServiceParameters);

                                                                if (!owsResponse1.result || owsResponse1.responseData == null)
                                                                {
                                                                    #region Pushing Reservation Track

                                                                    new LogHelper().Log("Pushing reservation track in local DB for sharer " + sharer.ReservationNumber, operaReservations[0].ReservationNameID, "FetchDueOutReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-out fetch");

                                                                    localResponse = await new WSClientHelper().PushReservationTrackLocally(operaReservations[0].ReservationNameID, new Models.Local.LocalRequestModel()
                                                                    {
                                                                        RequestObject = new Models.Local.ReservationTrackStatus()
                                                                        {
                                                                            ReservationNameID = operaReservations[0].ReservationNameID,
                                                                            ProcessType = Models.Local.ReservationProcessType.CheckoutFailled.ToString(),
                                                                            ReservationNumber = sharer.ReservationNumber,
                                                                            ProcessStatus = "",
                                                                            EmailSent = false
                                                                        }
                                                                    }, "pre checked-out fetch", fetchReservationRequest.ServiceParameters);
                                                                    if (localResponse.result)
                                                                    {
                                                                        new LogHelper().Log("Reservation track in local DB updated successfully ", operaReservations[0].ReservationNameID, "FetchDueOutReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-out fetch");
                                                                    }
                                                                    else
                                                                    {
                                                                        new LogHelper().Log("Failled to update reservation track in local DB with reason :- " + localResponse.responseMessage, operaReservations[0].ReservationNameID, "FetchDueOutReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-out fetch");
                                                                    }

                                                                    #endregion

                                                                    new LogHelper().Log("Failled to check-out sharer with reason :- " + owsResponse1.responseMessage + "sharer reservation no. : " + sharer.ReservationNumber, operaReservations[0].ReservationNameID, "FetchDueOutReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-out fetch");
                                                                    new LogHelper().Warn("Failled to check-out sharer with reason :- " + owsResponse1.responseMessage + "sharer reservation no. : " + sharer.ReservationNumber, operaReservations[0].ReservationNameID, "FetchDueOutReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-out fetch");

                                                                }
                                                                else
                                                                {
                                                                    #region Pushing Reservation Track

                                                                    new LogHelper().Log("Pushing reservation track in local DB for sharer " + sharer.ReservationNumber, operaReservations[0].ReservationNameID, "FetchDueOutReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-out fetch");

                                                                    localResponse = await new WSClientHelper().PushReservationTrackLocally(operaReservations[0].ReservationNameID, new Models.Local.LocalRequestModel()
                                                                    {
                                                                        RequestObject = new Models.Local.ReservationTrackStatus()
                                                                        {
                                                                            ReservationNameID = operaReservations[0].ReservationNameID,
                                                                            ProcessType = Models.Local.ReservationProcessType.CheckedoutSuccessfully.ToString(),
                                                                            ReservationNumber = sharer.ReservationNumber,
                                                                            ProcessStatus = "",
                                                                            EmailSent = false
                                                                        }
                                                                    }, "pre checked-out fetch", fetchReservationRequest.ServiceParameters);
                                                                    if (localResponse.result)
                                                                    {
                                                                        new LogHelper().Log("Reservation track in local DB updated successfully ", operaReservations[0].ReservationNameID, "FetchDueOutReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-out fetch");
                                                                    }
                                                                    else
                                                                    {
                                                                        new LogHelper().Log("Failled to update reservation track in local DB with reason :- " + localResponse.responseMessage, operaReservations[0].ReservationNameID, "FetchDueOutReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-out fetch");
                                                                    }

                                                                    #endregion

                                                                    new LogHelper().Log("Sharer checked out successfully", operaReservations[0].ReservationNameID, "FetchDueOutReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-out fetch");
                                                                }
                                                            }
                                                            else
                                                            {
                                                                #region Pushing Reservation Track

                                                                new LogHelper().Log("Pushing reservation track in local DB for sharer " + sharer.ReservationNumber, operaReservations[0].ReservationNameID, "FetchDueOutReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-out fetch");

                                                                localResponse = await new WSClientHelper().PushReservationTrackLocally(operaReservations[0].ReservationNameID, new Models.Local.LocalRequestModel()
                                                                {
                                                                    RequestObject = new Models.Local.ReservationTrackStatus()
                                                                    {
                                                                        ReservationNameID = operaReservations[0].ReservationNameID,
                                                                        ProcessType = Models.Local.ReservationProcessType.CheckoutFailled.ToString(),
                                                                        ReservationNumber = sharer.ReservationNumber,
                                                                        ProcessStatus = "",
                                                                        EmailSent = false
                                                                    }
                                                                }, "pre checked-out fetch", fetchReservationRequest.ServiceParameters);
                                                                if (localResponse.result)
                                                                {
                                                                    new LogHelper().Log("Reservation track in local DB updated successfully ", operaReservations[0].ReservationNameID, "FetchDueOutReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-out fetch");
                                                                }
                                                                else
                                                                {
                                                                    new LogHelper().Log("Failled to update reservation track in local DB with reason :- " + localResponse.responseMessage, operaReservations[0].ReservationNameID, "FetchDueOutReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-out fetch");
                                                                }

                                                                #endregion

                                                                new LogHelper().Log("Reservation name ID is NULL ", operaReservations[0].ReservationNameID, "FetchDueOutReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-out fetch");
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                new LogHelper().Log("Skipping check out because other than main window balnce is non zero", operaReservations[0].ReservationNameID, "FetchDueOutReservation", fetchReservationRequest.ServiceParameters.ClientID, "pre checked-out fetch");
                                            }
                                        }   
                                        else
                                        {
                                            #region Pushing Reservation Track

                                            new LogHelper().Log("Pushing reservation track in local DB ", operaReservations[0].ReservationNameID, "FetchDueOutReservation",  fetchReservationRequest.ServiceParameters.ClientID, "pre checked-out fetch");

                                            localResponse = await new WSClientHelper().PushReservationTrackLocally(operaReservations[0].ReservationNameID, new Models.Local.LocalRequestModel()
                                            {
                                                RequestObject = new Models.Local.ReservationTrackStatus()
                                                {
                                                    ReservationNameID = operaReservations[0].ReservationNameID,
                                                    ProcessType = Models.Local.ReservationProcessType.CheckoutFailled.ToString(),
                                                    ReservationNumber = operaReservations[0].ReservationNumber,
                                                    ProcessStatus = "",
                                                    EmailSent = false
                                                }
                                            }, "pre checked-out fetch", fetchReservationRequest.ServiceParameters);
                                            if (localResponse.result)
                                            {
                                                new LogHelper().Log("Reservation track in local DB updated successfully ", operaReservations[0].ReservationNameID, "FetchDueOutReservation",  fetchReservationRequest.ServiceParameters.ClientID, "pre checked-out fetch");
                                            }
                                            else
                                            {
                                                new LogHelper().Log("Failled to update reservation track in local DB with reason :- " + localResponse.responseMessage, operaReservations[0].ReservationNameID, "FetchDueOutReservation",  fetchReservationRequest.ServiceParameters.ClientID, "pre checked-out fetch");
                                            }

                                            #endregion

                                            new LogHelper().Log("Failled to process check out, where the is allowedfor checkout flag is either null or  false", operaReservations[0].ReservationNameID, "FetchDueOutReservation",  fetchReservationRequest.ServiceParameters.ClientID, "pre checked-out fetch");
                                        }
                                    }
                                }
                            }
                            else
                            {
                                #region Pushing Reservation Track

                                new LogHelper().Log("Pushing reservation track in local DB ", operaReservations[0].ReservationNameID, "FetchDueOutReservation",  fetchReservationRequest.ServiceParameters.ClientID, "pre checked-out fetch");

                                localResponse = await new WSClientHelper().PushReservationTrackLocally(operaReservations[0].ReservationNameID, new Models.Local.LocalRequestModel()
                                {
                                    RequestObject = new Models.Local.ReservationTrackStatus()
                                    {
                                        ReservationNameID = operaReservations[0].ReservationNameID,
                                        ProcessType = Models.Local.ReservationProcessType.CheckoutFailled.ToString(),
                                        ReservationNumber = operaReservations[0].ReservationNumber,
                                        ProcessStatus = "",
                                        EmailSent = false
                                    }
                                }, "pre checked-out fetch", fetchReservationRequest.ServiceParameters);
                                if (localResponse.result)
                                {
                                    new LogHelper().Log("Reservation track in local DB updated successfully ", operaReservations[0].ReservationNameID, "FetchDueOutReservation",  fetchReservationRequest.ServiceParameters.ClientID, "pre checked-out fetch");
                                }
                                else
                                {
                                    new LogHelper().Log("Failled to update reservation track in local DB with reason :- " + localResponse.responseMessage, operaReservations[0].ReservationNameID, "FetchDueOutReservation",  fetchReservationRequest.ServiceParameters.ClientID, "pre checked-out fetch");
                                }

                                #endregion

                                new LogHelper().Log("Failled to process check out, failled to retreave the guest balance", operaReservations[0].ReservationNameID, "FetchDueOutReservation",  fetchReservationRequest.ServiceParameters.ClientID, "pre checked-out fetch");
                            }
                        }
                        #endregion

                        #region Updating Locally
                        new LogHelper().Log("Pushing reservation to Local DB", reservation.ReservationNameID, "FetchDueOutReservation",  fetchReservationRequest.ServiceParameters.ClientID, "pre checked-out fetch");
                        localResponse = await new WSClientHelper().PushRecordLocally(new Models.Local.LocalRequestModel()
                        {
                            RequestObject = new List<Models.Cloud.OperaReservation>() { reservation },
                            SyncFromCloud = true
                        }, reservation.ReservationNameID, "pre checked-out fetch", fetchReservationRequest.ServiceParameters);
                        if (!localResponse.result)
                        {
                            new LogHelper().Log("Failled to push pre checked-out reservation with reason :- " + localResponse.responseMessage, reservation.ReservationNameID, "FetchDueOutReservation",  fetchReservationRequest.ServiceParameters.ClientID, "pre checked-out fetch");
                            return new Models.Local.LocalResponseModel()
                            {
                                result = false,
                                responseMessage = "Failled to push pre checked-out reservation with reason :- " + localResponse.responseMessage
                            };
                        }

                        new LogHelper().Log("Reservation successfully updated in Local DB", reservation.ReservationNameID, "FetchDueOutReservation",  fetchReservationRequest.ServiceParameters.ClientID, "pre checked-out fetch");
                        #endregion

                        #region Pushing folio to Local DB
                        if (!string.IsNullOrEmpty(folioAsBase64))
                        {
                            try
                            {
                                new LogHelper().Log("Pushing signed folio to Local DB", reservation.ReservationNameID, "FetchDueOutReservation",  fetchReservationRequest.ServiceParameters.ClientID, "pre checked-out fetch");
                                byte[] folio = null;
                                folio = Convert.FromBase64String(folioAsBase64);
                                localResponse = await new WSClientHelper().InsertReservationDocuments(reservation.ReservationNameID, new Models.Local.LocalRequestModel()
                                {
                                    RequestObject = new List<Models.Local.ReservationDocumentsDataTableModel>()
                                {
                                    new Models.Local.ReservationDocumentsDataTableModel()
                                    {
                                        Document = folio,
                                        DocumentType = "Folio",
                                        ReservationNameID = operaReservations[0].ReservationNameID
                                    }
                                }
                                }, "pre checked-out fetch", fetchReservationRequest.ServiceParameters);
                                if (!localResponse.result)
                                {
                                    new LogHelper().Log("Failled to push reservation document with reason :- " + localResponse.responseMessage, reservation.ReservationNameID, "FetchDueOutReservation",  fetchReservationRequest.ServiceParameters.ClientID, "pre checked-out fetch");
                                    new LogHelper().Warn("Failled to push reservation document with reason :- " + localResponse.responseMessage, reservation.ReservationNameID, "FetchDueOutReservation",  fetchReservationRequest.ServiceParameters.ClientID, "pre checked-out fetch");
                                }
                                else
                                {
                                    new LogHelper().Log("Reservation document updated successfully", reservation.ReservationNameID, "FetchDueOutReservation",  fetchReservationRequest.ServiceParameters.ClientID, "pre checked-out fetch");
                                }
                            }
                            catch (Exception exc)
                            {
                                new LogHelper().Error(exc, reservation.ReservationNameID, "FetchDueOutReservation",  fetchReservationRequest.ServiceParameters.ClientID, "pre checked-out fetch");
                            }
                        }
                        #endregion

                        #region Pushing Guest Signature to Local DB
                        if (!string.IsNullOrEmpty(reservation.GuestSignature))
                        {
                            try
                            {
                                new LogHelper().Log("Pushing guest signature to Local DB", reservation.ReservationNameID, "FetchDueOutReservation",  fetchReservationRequest.ServiceParameters.ClientID, "pre checked-out fetch");
                                byte[] guestSignature = null;
                                guestSignature = Convert.FromBase64String(reservation.GuestSignature);
                                localResponse = await new WSClientHelper().InsertReservationDocuments(reservation.ReservationNameID, new Models.Local.LocalRequestModel()
                                {
                                    RequestObject = new List<Models.Local.ReservationDocumentsDataTableModel>()
                                {
                                    new Models.Local.ReservationDocumentsDataTableModel()
                                    {
                                        Document = guestSignature,
                                        DocumentType = "Signature",
                                        ReservationNameID = reservation.ReservationNameID
                                    }
                                }
                                }, "pre checked-out fetch", fetchReservationRequest.ServiceParameters);
                                if (!localResponse.result)
                                {
                                    new LogHelper().Log("Failled to push guest signature with reason :- " + localResponse.responseMessage, reservation.ReservationNameID, "FetchDueOutReservation",  fetchReservationRequest.ServiceParameters.ClientID, "pre checked-out fetch");
                                    new LogHelper().Warn("Failled to push guest signature with reason :- " + localResponse.responseMessage, reservation.ReservationNameID, "FetchDueOutReservation",  fetchReservationRequest.ServiceParameters.ClientID, "pre checked-out fetch");
                                }
                                else
                                {
                                    new LogHelper().Log("Signature updated successfully", reservation.ReservationNameID, "FetchDueOutReservation",  fetchReservationRequest.ServiceParameters.ClientID, "pre checked-out fetch");
                                }
                            }
                            catch (Exception exc)
                            {
                                new LogHelper().Error(exc, reservation.ReservationNameID, "FetchDueOutReservation",  fetchReservationRequest.ServiceParameters.ClientID, "pre checked-out fetch");
                            }
                        }
                        #endregion

                        #region Sending Email
                        new LogHelper().Log("Sending final folio email", reservation.ReservationNameID, "FetchDueOutReservation",  fetchReservationRequest.ServiceParameters.ClientID, "pre checked-out fetch");
                        if (!string.IsNullOrEmpty(reservation.FolioEmail) && !string.IsNullOrEmpty(folioAsBase64))
                        {
                            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
                            Models.Email.EmailResponse emailResponse = await new WSClientHelper().SendEmail(reservation.ReservationNameID, new Models.Email.EmailRequest()
                            {
                                FromEmail = fetchReservationRequest.ServiceParameters.PreCheckoutFolioEmail,
                                ToEmail = reservation.FolioEmail,
                                GuestName = !string.IsNullOrEmpty(operaReservations[0].GuestProfiles[0].GuestName) ? textInfo.ToTitleCase(operaReservations[0].GuestProfiles[0].GuestName)
                                        : operaReservations[0].GuestProfiles[0].GuestName,//operaReservations[0].GuestProfiles[0].GuestName,
                                Subject = fetchReservationRequest.ServiceParameters.PreCheckoutFolioEmailSubject,
                                confirmationNumber = operaReservations[0].ReservationNumber,
                                displayFromEmail = fetchReservationRequest.ServiceParameters.EmailDisplayName,
                                EmailType = Models.Email.EmailType.GuestFolio,
                                AttchmentBase64 = folioAsBase64

                            }, "pre checked-out fetch", fetchReservationRequest.ServiceParameters);
                            if (!emailResponse.result)
                            {
                                new LogHelper().Log("Failled to send final folio email with reason :- " + emailResponse.responseMessage, reservation.ReservationNameID, "FetchDueOutReservation",  fetchReservationRequest.ServiceParameters.ClientID, "pre checked-out fetch");
                                new LogHelper().Warn("Failled to send final folio email with reason :- " + emailResponse.responseMessage, reservation.ReservationNameID, "FetchDueOutReservation",  fetchReservationRequest.ServiceParameters.ClientID, "pre checked-out fetch");

                                #region Pushing Reservation Track

                                new LogHelper().Log("Pushing reservation track in local DB ", reservation.ReservationNameID, "FetchDueOutReservation",  fetchReservationRequest.ServiceParameters.ClientID, "pre checked-out fetch");

                                localResponse = await new WSClientHelper().PushReservationTrackLocally(reservation.ReservationNameID, new Models.Local.LocalRequestModel()
                                {
                                    RequestObject = new Models.Local.ReservationTrackStatus()
                                    {
                                        ReservationNameID = reservation.ReservationNameID,
                                        ProcessType = Models.Local.ReservationProcessType.GuestFolioEmail.ToString(),
                                        ReservationNumber = reservation.ReservationNumber,
                                        ProcessStatus = "",
                                        EmailSent = false
                                    }
                                }, "pre checked-out fetch", fetchReservationRequest.ServiceParameters);
                                if (localResponse.result)
                                {
                                    new LogHelper().Log("Reservation track in local DB updated successfully ", reservation.ReservationNameID, "FetchDueOutReservation",  fetchReservationRequest.ServiceParameters.ClientID, "pre checked-out fetch");
                                }
                                else
                                {
                                    new LogHelper().Log("Failled to update reservation track in local DB with reason :- " + localResponse.responseMessage, reservation.ReservationNameID, "FetchDueOutReservation",  fetchReservationRequest.ServiceParameters.ClientID, "pre checked-out fetch");
                                }

                                #endregion
                            }
                            else
                            {
                                new LogHelper().Log("Email send successfully", reservation.ReservationNameID, "FetchDueOutReservation",  fetchReservationRequest.ServiceParameters.ClientID, "pre checked-out fetch");

                                #region Pushing Reservation Track

                                new LogHelper().Log("Pushing reservation track in local DB ", reservation.ReservationNameID, "FetchDueOutReservation",  fetchReservationRequest.ServiceParameters.ClientID, "pre checked-out fetch");

                                localResponse = await new WSClientHelper().PushReservationTrackLocally(reservation.ReservationNameID, new Models.Local.LocalRequestModel()
                                {
                                    RequestObject = new Models.Local.ReservationTrackStatus()
                                    {
                                        ReservationNameID = reservation.ReservationNameID,
                                        ProcessType = Models.Local.ReservationProcessType.GuestFolioEmail.ToString(),
                                        ReservationNumber = reservation.ReservationNumber,
                                        ProcessStatus = "",
                                        EmailSent = true
                                    }
                                }, "pre checked-out fetch", fetchReservationRequest.ServiceParameters);
                                if (localResponse.result)
                                {
                                    new LogHelper().Log("Reservation track in local DB updated successfully ", reservation.ReservationNameID, "FetchDueOutReservation",  fetchReservationRequest.ServiceParameters.ClientID, "pre checked-out fetch");
                                }
                                else
                                {
                                    new LogHelper().Log("Failled to update reservation track in local DB with reason :- " + localResponse.responseMessage, reservation.ReservationNameID, "FetchDueOutReservation",  fetchReservationRequest.ServiceParameters.ClientID, "pre checked-out fetch");
                                }

                                #endregion
                            }
                        }
                        else
                        {
                            new LogHelper().Log("Failled to send guest folio email since email address not found from pre checked out list response", reservation.ReservationNameID, "FetchDueOutReservation",  fetchReservationRequest.ServiceParameters.ClientID, "pre checked-out fetch");
                            new LogHelper().Warn("Failled to send guest folio email since email address not found from pre checked out list response", reservation.ReservationNameID, "FetchDueOutReservation",  fetchReservationRequest.ServiceParameters.ClientID, "pre checked-out fetch");

                            #region Pushing Reservation Track

                            new LogHelper().Log("Pushing reservation track in local DB ", reservation.ReservationNameID, "FetchDueOutReservation",  fetchReservationRequest.ServiceParameters.ClientID, "pre checked-out fetch");

                            localResponse = await new WSClientHelper().PushReservationTrackLocally(reservation.ReservationNameID, new Models.Local.LocalRequestModel()
                            {
                                RequestObject = new Models.Local.ReservationTrackStatus()
                                {
                                    ReservationNameID = reservation.ReservationNameID,
                                    ProcessType = Models.Local.ReservationProcessType.GuestFolioEmail.ToString(),
                                    ReservationNumber = reservation.ReservationNumber,
                                    ProcessStatus = "",
                                    EmailSent = false
                                }
                            }, "pre checked-out fetch", fetchReservationRequest.ServiceParameters);
                            if (localResponse.result)
                            {
                                new LogHelper().Log("Reservation track in local DB updated successfully ", reservation.ReservationNameID, "FetchDueOutReservation",  fetchReservationRequest.ServiceParameters.ClientID, "pre checked-out fetch");
                            }
                            else
                            {
                                new LogHelper().Log("Failled to update reservation track in local DB with reason :- " + localResponse.responseMessage, reservation.ReservationNameID, "FetchDueOutReservation",  fetchReservationRequest.ServiceParameters.ClientID, "pre checked-out fetch");
                            }

                            #endregion
                        }
                        #endregion

                        #region Final step

                        new LogHelper().Log("Updating record in cloud as synced to local true", reservation.ReservationNameID, "FetchDueOutReservation",  fetchReservationRequest.ServiceParameters.ClientID, "pre checked-out fetch");
                        cloudResponse = await new WSClientHelper().UpdateRecordInCloud(reservation.ReservationNameID, new Models.Cloud.CloudRequestModel()
                        {
                            RequestObject = new List<Models.Local.UpdateReservationByReservationNameIDModel>()
                                                            {
                                                                new Models.Local.UpdateReservationByReservationNameIDModel
                                                                {
                                                                    IsPushedToLocal = true,
                                                                    ReservationNameID = reservation.ReservationNameID
                                                                }
                                                            }
                        }, "pre checked-out fetch", fetchReservationRequest.ServiceParameters);
                        if (!cloudResponse.result)
                        {
                            new LogHelper().Log("Failled to update record with reason :- " + cloudResponse.responseMessage, reservation.ReservationNameID, "FetchDueOutReservation",  fetchReservationRequest.ServiceParameters.ClientID, "pre checked-out fetch");

                        }
                        else
                            new LogHelper().Log("Record updated successfully", reservation.ReservationNameID, "FetchDueOutReservation",  fetchReservationRequest.ServiceParameters.ClientID, "pre checked-out fetch");

                        new LogHelper().Log("Clearing the record in the cloud", reservation.ReservationNameID, "FetchDueOutReservation",  fetchReservationRequest.ServiceParameters.ClientID, "pre checked-out fetch");
                        cloudResponse = await new WSClientHelper().ClearRecords(reservation.ReservationNameID, new Models.Cloud.CloudRequestModel() { RequestObject = reservation.ReservationNameID }, "pre checked-out fetch", fetchReservationRequest.ServiceParameters);
                        if (!cloudResponse.result)
                        {
                            new LogHelper().Log("Failled to clear the record with reason :- " + cloudResponse.responseMessage, reservation.ReservationNameID, "FetchDueOutReservation",  fetchReservationRequest.ServiceParameters.ClientID, "pre checked-out fetch");
                        }
                        else
                            new LogHelper().Log("Record cleared successfully", reservation.ReservationNameID, "FetchDueOutReservation",  fetchReservationRequest.ServiceParameters.ClientID, "pre checked-out fetch");
                        #endregion

                        #region Pushing Reservation Track

                        new LogHelper().Log("Pushing reservation track in local DB ", reservation.ReservationNameID, "FetchDueOutReservation",  fetchReservationRequest.ServiceParameters.ClientID, "pre checked-out fetch");

                        localResponse = await new WSClientHelper().PushReservationTrackLocally(reservation.ReservationNameID, new Models.Local.LocalRequestModel()
                        {
                            RequestObject = new Models.Local.ReservationTrackStatus()
                            {
                                ReservationNameID = reservation.ReservationNameID,
                                ProcessType = Models.Local.ReservationProcessType.PreCheckedOutFetched.ToString(),
                                ReservationNumber = reservation.ReservationNumber,
                                ProcessStatus = "",
                                EmailSent = false
                            }
                        }, "pre checked-out fetch", fetchReservationRequest.ServiceParameters);
                        if (localResponse.result)
                        {
                            new LogHelper().Log("Reservation track in local DB updated successfully ", reservation.ReservationNameID, "FetchDueOutReservation",  fetchReservationRequest.ServiceParameters.ClientID, "pre checked-out fetch");
                        }
                        else
                        {
                            new LogHelper().Log("Failled to update reservation track in local DB with reason :- " + localResponse.responseMessage, reservation.ReservationNameID, "FetchDueOutReservation",  fetchReservationRequest.ServiceParameters.ClientID, "pre checked-out fetch");
                        }

                        #endregion
                    }


                    return new Models.Local.LocalResponseModel()
                    {
                        result = true,
                        responseMessage = "Success"
                    };

                }
                else
                {
                    return new Models.Local.LocalResponseModel()
                    {
                        result = false,
                        responseMessage = "No reservation found"
                    };
                }
            }
            catch(Exception ex)
            {
                return new Models.Local.LocalResponseModel()
                {
                    result = false,
                    responseMessage = ex.Message,
                    statusCode = -1
                };
            }
        }

        public async Task<Models.Local.LocalResponseModel> PushDueInReservation(Models.Local.PushReservationRequest pushReservationRequest)
        {
            
            #region Variables
            List<Models.OWS.OperaReservation> operaReservationList = null;
            Models.OWS.OperaReservation Reservation = null;
            #endregion

            try
            {
                new LogHelper().Log("Pushing due in reservation list", null, "PushDueInReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-In push");


                bool? isEmailSent = null;
                bool? IsEmailProcessed = null;
                //new LogHelper().Log("Processing reservation No. : " + pushReservationRequest.ReservationNumber, null, "PushDueInReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-In push");

                List<Models.OWS.OperaReservation> temp_operaReservations = null;
                
                #region Fetching Reservation from OWS
                //new LogHelper().Log("Fetching opera reservation", null, "PushDueInReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-In push");
                Models.OWS.OwsResponseModel owsResponse1 = await new WSClientHelper().FetchReservationAsync(pushReservationRequest.ReservationNumber, new Models.OWS.OwsRequestModel()
                {
                    ChainCode = pushReservationRequest.ServiceParameters.ChainCode,
                    DestinationEntityID = pushReservationRequest.ServiceParameters.DestinationEntityID,
                    HotelDomain = pushReservationRequest.ServiceParameters.HotelDomain,
                    KioskID = pushReservationRequest.ServiceParameters.KioskID,
                    Language = pushReservationRequest.ServiceParameters.Language,
                    LegNumber = pushReservationRequest.ServiceParameters.Legnumber,
                    Password = pushReservationRequest.ServiceParameters.Password,
                    SystemType = pushReservationRequest.ServiceParameters.SystemType,
                    Username = pushReservationRequest.ServiceParameters.Username,
                    FetchBookingRequest = new Models.OWS.FetchBookingRequestModel()
                    {
                        ReservationNumber = pushReservationRequest.ReservationNumber
                        //ReservationNameID = pushReservationRequest.ReservationNameID
                    }
                }, "Due-In push", pushReservationRequest.ServiceParameters);
                if (!owsResponse1.result)
                {
                    new LogHelper().Log("Fetching opera reservation", null, "PushDueInReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-In push");
                    new LogHelper().Warn("Failled to fetch opera reservation with reason :- " + owsResponse1.responseMessage, pushReservationRequest.ReservationNumber, "PushDueInReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-In push");
                    return new Models.Local.LocalResponseModel()
                    {
                        result = false,
                        responseMessage = "Failled to fetch opera reservation with reason :- " + owsResponse1.responseMessage
                    };
                }
                if (owsResponse1.responseData == null)
                {
                    new LogHelper().Log("Failled to fetch opera reservation with reason :- API response data is NULL" + owsResponse1.responseMessage, pushReservationRequest.ReservationNumber, "PushDueInReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-In push");
                    return new Models.Local.LocalResponseModel()
                    {
                        result = false,
                        responseMessage = "Failled to fetch opera reservation with reason :- API response data is NULL" + owsResponse1.responseMessage
                    };
                }
                else
                {
                    new LogHelper().Debug("Converting API json to object", pushReservationRequest.ReservationNumber, "PushDueInReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-In push");
                    try
                    {
                        temp_operaReservations = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Models.OWS.OperaReservation>>(owsResponse1.responseData.ToString());
                        Reservation = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Models.OWS.OperaReservation>>(owsResponse1.responseData.ToString())[0];
                        new LogHelper().Log("Opera reservation fetched successfully", Reservation.ReservationNameID, "PushDueInReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-In push");
                    }
                    catch (Exception ex)
                    {
                        new LogHelper().Error(ex, pushReservationRequest.ReservationNumber, "PushDueInReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-In push");
                        new LogHelper().Log("Failled to covert API response to object", pushReservationRequest.ReservationNumber, "PushDueInReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-In push");
                        new LogHelper().Warn("Failled to fetch opera reservation with reason :- " + ex.Message, pushReservationRequest.ReservationNumber, "PushDueInReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-In push");
                        new LogHelper().Debug("Failled to fetch opera reservation with reason :- " + ex.Message, pushReservationRequest.ReservationNumber, "PushDueInReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-IN push");
                        return new Models.Local.LocalResponseModel()
                        {
                            result = false,
                            responseMessage = ex.Message
                        };
                    }
                }

                #endregion

                #region Processing SHarers to Local DB

                try
                {
                    List<Models.OWS.OperaReservation> tempList = null;
                    if (temp_operaReservations[0].SharerReservations != null && temp_operaReservations[0].SharerReservations.Count > 0)
                    {
                        //MessageBox.Show("Sharer present");
                        string shareID = temp_operaReservations[0].ReservationNumber;
                        temp_operaReservations[0].ReservationNumber += "||" + temp_operaReservations[0].ReservationNumber;

                        foreach (Models.OWS.OperaReservation sharerReservation in temp_operaReservations[0].SharerReservations)
                        {
                            owsResponse1 = await new WSClientHelper().FetchReservationAsync(pushReservationRequest.ReservationNumber, new Models.OWS.OwsRequestModel()
                            {
                                ChainCode = pushReservationRequest.ServiceParameters.ChainCode,
                                DestinationEntityID = pushReservationRequest.ServiceParameters.DestinationEntityID,
                                HotelDomain = pushReservationRequest.ServiceParameters.HotelDomain,
                                KioskID = pushReservationRequest.ServiceParameters.KioskID,
                                Language = pushReservationRequest.ServiceParameters.Language,
                                LegNumber = pushReservationRequest.ServiceParameters.Legnumber,
                                Password = pushReservationRequest.ServiceParameters.Password,
                                SystemType = pushReservationRequest.ServiceParameters.SystemType,
                                Username = pushReservationRequest.ServiceParameters.Username,
                                FetchBookingRequest = new Models.OWS.FetchBookingRequestModel()
                                {
                                    ReservationNumber = pushReservationRequest.ReservationNumber
                                }
                            }, "Due-In push", pushReservationRequest.ServiceParameters);

                            if (owsResponse1.result && owsResponse1.responseData != null)
                            {
                                tempList = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Models.OWS.OperaReservation>>(owsResponse1.responseData.ToString());
                                tempList[0].ReservationNumber += "||" + shareID;
                                temp_operaReservations.AddRange(tempList);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    new LogHelper().Error(ex, pushReservationRequest.ReservationNumber, "PushDueInReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-In push");
                }
                #endregion

                #region Pushing record copy in local DB
                new LogHelper().Log("Updating the reservation in Local DB", Reservation.ReservationNameID, "PushDueInReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-In push");
                //Reservation.IsEmailSend = false;
                //Reservation.reservationDocument = null;
                Models.Local.LocalResponseModel localResponse = await new WSClientHelper().PushRecordLocally(new Models.Local.LocalRequestModel()
                {
                    SyncFromCloud = false,
                    RequestObject = temp_operaReservations,//new List<Models.OWS.OperaReservation>() { Reservation }
                }, Reservation.ReservationNameID, "Due-In push", pushReservationRequest.ServiceParameters);
                if (!localResponse.result)
                {
                    new LogHelper().Log("Updating the reservation in Local DB with email send flag failled with reason :- " + localResponse.responseMessage, Reservation.ReservationNameID, "PushDueInReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-In push");
                }
                else
                    new LogHelper().Log("Updating the reservation in Local DB with email send flag fsucceeded", Reservation.ReservationNameID, "PushDueInReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-In push");
                #endregion

                #region Validating email present or not
                bool isEmailPresent = false;
                if (Reservation != null && Reservation.GuestProfiles != null && Reservation.GuestProfiles.Count > 0 &&
                    Reservation.GuestProfiles[0].Email != null && Reservation.GuestProfiles[0].Email.Count > 0 &&
                    !string.IsNullOrEmpty(Reservation.GuestProfiles[0].Email[0].email))
                    isEmailPresent = true;
                if (!isEmailPresent)
                {
                    new LogHelper().Log("Skipping reservation No. : " + Reservation.ReservationNumber + " no email ID present", Reservation.ReservationNameID, "PushDueInReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-In push");
                    return new Models.Local.LocalResponseModel()
                    {
                        result = false,
                        responseMessage = " no email ID present"
                    };
                }
                #endregion

                #region Validating Reservation
                new LogHelper().Log("Validating reservation", Reservation.ReservationNameID, "PushDueInReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-In push");

                if (Reservation == null)
                {
                    new LogHelper().Log("Reservation returned as NULL", Reservation.ReservationNameID, "PushDueInReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-In push");
                    return new Models.Local.LocalResponseModel()
                    {
                        result = false,
                        responseMessage = "Reservation returned as NULL"
                    };
                }
                else if (Reservation.Adults == null && Reservation.Adults == 0)
                {
                    new LogHelper().Log("Reservation adult count is NULL or 0", Reservation.ReservationNameID, "PushDueInReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-In push");
                    return new Models.Local.LocalResponseModel()
                    {
                        result = false,
                        responseMessage = "Reservation adult count is NULL or 0"
                    };
                }
                else if (string.IsNullOrEmpty(Reservation.ReservationStatus) && (!Reservation.ReservationStatus.ToUpper().Equals("DUEIN") || !Reservation.ReservationStatus.ToUpper().Equals("RESERVED")))
                {
                    new LogHelper().Debug("Reservation status is : " + Reservation.ReservationStatus + " not elogible for pre-checkin", Reservation.ReservationNameID, "PushDueInReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-In push");
                    return new Models.Local.LocalResponseModel()
                    {
                        result = false,
                        responseMessage = "Reservation adult count is NULL or 0"
                    };
                }
                new LogHelper().Log("Reservation Validated ", Reservation.ReservationNameID, "PushDueInReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-In push");
                #endregion

                #region Processing sharer Profiles
                new LogHelper().Log("Processing sharer in the reservation", Reservation.ReservationNameID, "PushDueInReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-In push");
                if (Reservation.SharerReservations != null && Reservation.SharerReservations.Count > 0)
                {
                    foreach (Models.OWS.OperaReservation sharer in Reservation.SharerReservations)
                    {
                        if (sharer.GuestProfiles != null && sharer.GuestProfiles.Count > 0)
                        {
                            foreach (Models.OWS.GuestProfile guestProfile in sharer.GuestProfiles)
                            {
                                Reservation.GuestProfiles.Add(guestProfile);
                            }
                        }
                    }
                    new LogHelper().Log("Processing sharer in the reservation completed", Reservation.ReservationNameID, "PushDueInReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-In push");
                }
                else
                    new LogHelper().Log("No sharers found", Reservation.ReservationNameID, "PushDueInReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-In push");
                #endregion

                #region Processing RoomRate
                new LogHelper().Log("Updating rate details", Reservation.ReservationNameID, "PushDueInReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-In push");
                if (Reservation.RateDetails != null && Reservation.RateDetails.DailyRates != null && Reservation.RateDetails.DailyRates.Count > 0)
                {
                    decimal total_roomrate = Reservation.RateDetails.DailyRates.Sum(x => x.Amount);
                    if (Reservation.PrintRate != null && Reservation.PrintRate.Value)
                        Reservation.RateDetails.RateAmount = total_roomrate;
                    else
                        Reservation.TotalAmount = 0;
                    new LogHelper().Log("Updating rate details completed", Reservation.ReservationNameID, "PushDueInReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-In push");
                }
                else
                    new LogHelper().Log("Updating rate details failed because Rate details are blank in the reservation", Reservation.ReservationNameID, "PushDueInReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-In push");
                #endregion


                #region Processing MealPlan
                if (pushReservationRequest.ServiceParameters.IsBreakFastValidationWithUDF != null && pushReservationRequest.ServiceParameters.IsBreakFastValidationWithUDF.Value)
                {
                    new LogHelper().Debug("Processing meal plan from UDF fields", Reservation.ReservationNameID, "PushDueInReservation", "Grabber", "Due-In push");
                    if (Reservation.userDefinedFields != null && Reservation.userDefinedFields.Count > 0)
                    {
                        if (Reservation.userDefinedFields.Find(x => x.FieldName.Equals(pushReservationRequest.ServiceParameters.MealPlanFieldName)) != null)
                        {

                            if (!Reservation.userDefinedFields.Find(x => x.FieldName.Equals(pushReservationRequest.ServiceParameters.MealPlanFieldName)).FieldValue.Equals("NP"))
                            {
                                string tempUDFValue = Reservation.userDefinedFields.Find(x => x.FieldName.Equals(pushReservationRequest.ServiceParameters.MealPlanFieldName)).FieldValue;
                                if (!string.IsNullOrEmpty(tempUDFValue))
                                {
                                    bool isPackageFound = false;
                                    if (pushReservationRequest.ServiceParameters.PackageCodes.Split(';').ToList().Contains(tempUDFValue))
                                        isPackageFound = true;
                                    if (isPackageFound)
                                    {
                                        Reservation.IsBreakFastAvailable = true;
                                        new LogHelper().Debug("Meal plan updated", Reservation.ReservationNameID, "PushDueInReservation", "Grabber", "Due-In push");
                                    }
                                }
                            }
                            else
                                new LogHelper().Log("Processing meal plan not updated (NP not present in UDF)", Reservation.ReservationNameID, "PushDueInReservation", "Grabber", "Due-In push");
                        }
                    }
                    else
                        new LogHelper().Log("No UDF fields for meal plan not found", Reservation.ReservationNameID, "PushDueInReservation", "Grabber", "Due-In push");
                }
                if (pushReservationRequest.ServiceParameters.IsBreakFastValidationWithPackage != null && pushReservationRequest.ServiceParameters.IsBreakFastValidationWithPackage.Value)
                {
                    new LogHelper().Debug("Processing package list in the reservation for meal plan", Reservation.ReservationNameID, "PushDueInReservation", "Grabber", "Due-In push");
                    if (((Reservation.PackageDetails != null && Reservation.PackageDetails.Count > 0) || (Reservation.PreferanceDetails != null && Reservation.PreferanceDetails.Count > 0)) && (!string.IsNullOrEmpty(pushReservationRequest.ServiceParameters.PackageCodes) && pushReservationRequest.ServiceParameters.PackageCodes.Split(';').ToList() != null))
                    {
                        if (Reservation.PackageDetails != null && Reservation.PackageDetails.Count > 0)
                        {
                            bool isPackageFound = false;
                            foreach (Models.OWS.PackageDetails package in Reservation.PackageDetails)
                            {
                                if (pushReservationRequest.ServiceParameters.PackageCodes.Split(';').ToList().Contains(package.PackageCode))
                                {
                                    isPackageFound = true;
                                    break;
                                }
                            }
                            if (isPackageFound)
                            {
                                Reservation.IsBreakFastAvailable = true;
                                new LogHelper().Debug("Meal plan updated", Reservation.ReservationNameID, "PushDueInReservation", "Grabber", "Due-In push");
                            }
                        }
                        if (Reservation.PreferanceDetails != null && Reservation.PreferanceDetails.Count > 0)
                        {
                            if (Reservation.IsBreakFastAvailable == null || !Reservation.IsBreakFastAvailable.Value)
                            {
                                bool isPrefernceFound = false;
                                foreach (Models.OWS.PreferanceDetails prefernce in Reservation.PreferanceDetails)
                                {
                                    if (pushReservationRequest.ServiceParameters.PackageCodes.Split(';').ToList().Contains(prefernce.PreferanceCode))
                                    {
                                        isPrefernceFound = true;
                                        break;
                                    }
                                }
                                if (isPrefernceFound)
                                {
                                    Reservation.IsBreakFastAvailable = true;
                                    new LogHelper().Debug("Meal plan updated", Reservation.ReservationNameID, "PushDueInReservation", "Grabber", "Due-In push");
                                }
                            }
                        }
                    }
                    else
                        new LogHelper().Log("No package or prefernce list in the reservation for meal plan not found", Reservation.ReservationNameID, "PushDueInReservation", "Grabber", "Due-In push");
                }
                #endregion

                

                #region VerifyVIPReservationOrNot
                new LogHelper().Log("Verifying reservation VIP status in UDF field", Reservation.ReservationNameID, "PushDueInReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-In push");
                if (Reservation.userDefinedFields != null && Reservation.userDefinedFields.Count > 0)
                {
                    if (Reservation.userDefinedFields.Find(x => x.FieldName.Equals(pushReservationRequest.ServiceParameters.PreAuthUDF)) != null &&
                        Reservation.userDefinedFields.Find(x => x.FieldName.Equals(pushReservationRequest.ServiceParameters.PreAuthUDF)).FieldValue.Equals("NO"))
                    {
                        new LogHelper().Log("Reservation is flagged not to take payment", Reservation.ReservationNameID, "PushDueInReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-In push");
                        Reservation.DepositDetail = new List<Models.OWS.DepositDetail>()
                {
                    new Models.OWS.DepositDetail()
                    {
                        Amount = 0,
                        CardExpiryDate = null,
                        CreditCardNumber = null,
                        IsCreditCardDeposit = false,
                        PaymentType = null
                    }
                };
                        Reservation.IsDepositAvailable = true;
                    }
                    else if (Reservation.userDefinedFields.Find(x => x.FieldName.Equals(pushReservationRequest.ServiceParameters.PreAuthAmntUDF)) != null &&
                        Reservation.userDefinedFields.Find(x => x.FieldName.Equals(pushReservationRequest.ServiceParameters.PreAuthAmntUDF)).FieldValue.Equals("NO"))
                    {
                        
                        new LogHelper().Log("Reservation is flagged not to take payment", Reservation.ReservationNameID, "PushDueInReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-In push");
                        Reservation.DepositDetail = new List<Models.OWS.DepositDetail>()
                        {
                            new Models.OWS.DepositDetail()
                            {
                                Amount = 0,
                                CardExpiryDate = null,
                                CreditCardNumber = null,
                                IsCreditCardDeposit = false,
                                PaymentType = null
                            }
                        };

                        Reservation.IsDepositAvailable = true;
                    }
                }
                new LogHelper().Log("Verifying reservation VIP status in UDF field completed", Reservation.ReservationNameID, "PushDueInReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-In push");
                #endregion

                #region PaymentDesabling
                if (pushReservationRequest.ServiceParameters.IsPaymentDisabled)
                {
                    new LogHelper().Log("Disabling the payment as per the config", Reservation.ReservationNameID, "PushDueInReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-In push");
                    Reservation.DepositDetail = new List<Models.OWS.DepositDetail>()
                {
                    new Models.OWS.DepositDetail()
                    {
                        Amount = 0,
                        CardExpiryDate = null,
                        CreditCardNumber = null,
                        IsCreditCardDeposit = false,
                        PaymentType = null
                    }
                };
                    Reservation.IsDepositAvailable = true;
                }
                #endregion

                #region Update ETA
                if (pushReservationRequest.ServiceParameters.IsETADefault)
                {
                    new LogHelper().Log("Assigning NULL value to ETA as per the config", Reservation.ReservationNameID, "PushDueInReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-In push");
                    Reservation.ExpectedArrivalTime = null;
                }
                #endregion

                #region Push due in record
                new LogHelper().Log("Pushing due in reservation, reservation No. : " + Reservation.ReservationNumber, Reservation.ReservationNameID, "PushDueInReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-In push");
                Models.Cloud.CloudResponseModel cloudResponse = await new WSClientHelper().PushRecordToCloud(Reservation.ReservationNameID, new Models.Cloud.CloudRequestModel()
                {
                    RequestObject = new List<Models.OWS.OperaReservation>() { Reservation }
                }, "Due-In push", pushReservationRequest.ServiceParameters);
                if (!cloudResponse.result)
                {
                    new LogHelper().Log("Failled to push the reservation to cloud, so skipping the reservation ", Reservation.ReservationNameID, "PushDueInReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-In push");
                    return new Models.Local.LocalResponseModel()
                    {
                        result = false,
                        responseMessage = "Failled to push the reservation to cloud, so skipping the reservation "
                    };
                }
                new LogHelper().Log("Reservation pushed to cloud successfully", Reservation.ReservationNameID, "PushDueInReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-In push");

                #endregion

                #region Fetching Reservation Track whethere is email is already send or not
                //new LogHelper().Log("Fetching reservation track from local DB for status (email send or not)", Reservation.ReservationNameID, "PushDueInReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-In push");

                //Models.Local.LocalResponseModel localWebResponse = await new WSClientHelper().FetchReservationTracjLocally(Reservation.ReservationNameID, new Models.Local.LocalRequestModel()
                //{

                //    RequestObject = new Models.Local.ReservationTrackStatus()
                //    {
                //        ReservationNameID = Reservation.ReservationNameID,
                //        ProcessType = Models.Local.ReservationProcessType.Precheckinemail.ToString()
                //    }
                //}, "Due-In push", pushReservationRequest.ServiceParameters);

                //if (localWebResponse.result)
                //{
                //    new LogHelper().Log("Reservation track in local DB fetched successfully ", Reservation.ReservationNameID, "PushDueInReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-In push");
                //    if (localWebResponse.responseData != null)
                //    {
                //        List<Models.Local.ReservationTrackStatus> reservationTracks = null;
                //        new LogHelper().Debug("Converting API json to object", Reservation.ReservationNameID, "PushDueInReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-In push");
                //        try
                //        {
                //            reservationTracks = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Models.Local.ReservationTrackStatus>>(localWebResponse.responseData.ToString());
                //        }
                //        catch (Exception ex)
                //        {
                //            isEmailSent = false;
                //            new LogHelper().Error(ex, Reservation.ReservationNameID, "PushDueInReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-In push");
                //        }
                //        if (reservationTracks != null && reservationTracks.Count > 0)
                //        {
                //            isEmailSent = true;
                //            new LogHelper().Debug("Pre checkin email already send", Reservation.ReservationNameID, "PushDueInReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-In push");
                //        }
                //    }
                //    else
                //    {
                //        isEmailSent = false;
                //        new LogHelper().Log("Resrvation track not present ", Reservation.ReservationNameID, "PushDueInReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-In push");
                //    }
                //}
                //else
                //{
                //    isEmailSent = false;
                //    new LogHelper().Log("Failled to fetch reservation track in local DB with reason :- " + localWebResponse.responseMessage, Reservation.ReservationNameID, "PushDueInReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-In push");
                //}


                #endregion

                #region Sending Email
                try
                {

                    new LogHelper().Log("Verifying email send or not ", Reservation.ReservationNameID, "PushDueInReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-In push");
                    //if (isEmailSent == null || !isEmailSent.Value)
                    {
                        new LogHelper().Log("Sending pre-checkin email", Reservation.ReservationNameID, "PushDueInReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-In push");
                        if (Reservation != null && Reservation.GuestProfiles != null && Reservation.GuestProfiles[0].Email != null && Reservation.GuestProfiles[0].Email.Count > 0)
                        {
                            foreach (Models.OWS.Email email in Reservation.GuestProfiles[0].Email)
                            {
                                if (email.primary != null && email.primary.Value)
                                {
                                    TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
                                    Models.Email.EmailResponse emailResponse = await new WSClientHelper().SendEmail(Reservation.ReservationNameID, new Models.Email.EmailRequest()
                                    {
                                        FromEmail = pushReservationRequest.ServiceParameters.PreArrivalFromEmail,
                                        ToEmail = email.email,
                                        GuestName = !string.IsNullOrEmpty(Reservation.GuestProfiles[0].GuestName) ? textInfo.ToTitleCase(Reservation.GuestProfiles[0].GuestName)
                                        : Reservation.GuestProfiles[0].GuestName,
                                        Subject = pushReservationRequest.ServiceParameters.PreArrivalEmailSubject,
                                        confirmationNumber = "?id=" + HttpUtility.UrlEncode(new Helper().EncryptString("b14ca5898a4e4133bbce2ea2315a1916", Reservation.ReservationNumber)),
                                        displayFromEmail = pushReservationRequest.ServiceParameters.EmailDisplayName,
                                        EmailType = Models.Email.EmailType.Precheckedin,
                                        AttachmentFileName = "WelcomeEmail.pdf",
                                        ReservationNumber = !string.IsNullOrEmpty(Reservation.ReservationNumber)?(Reservation.ReservationNumber.Contains("||") ?
                                                            (Reservation.ReservationNumber.Substring(0,Reservation.ReservationNumber.IndexOf('|')-1)):(Reservation.ReservationNumber)):(Reservation.ReservationNumber),
                                        ArrivalDate = Reservation.ArrivalDate.Value.ToString("dd-MMM-yyyy"),
                                        DepartureDate = Reservation.DepartureDate.Value.ToString("dd-MMM-yyy")

                                    }, "Due-In push", pushReservationRequest.ServiceParameters);
                                    if (!emailResponse.result)
                                    {
                                        isEmailSent = false;
                                        IsEmailProcessed = false;
                                        new LogHelper().Log("Failled to send confirmation email with reason :- " + emailResponse.responseMessage, Reservation.ReservationNameID, "PushDueInReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-In push");
                                        new LogHelper().Warn("Failled to send confirmation email with reason :- " + emailResponse.responseMessage, Reservation.ReservationNameID, "PushDueInReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-In push");
                                    }
                                    else
                                    {
                                        isEmailSent = true;
                                        IsEmailProcessed = true;
                                        new LogHelper().Log("Email send successfully", Reservation.ReservationNameID, "PushDueInReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-In push");
                                    }
                                }
                            }
                        }
                        else
                        {
                            new LogHelper().Log("Failled to send pre-checkin since email address not found from pre checked in list response", Reservation.ReservationNameID, "PushDueInReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-In push");
                            new LogHelper().Warn("Failled to send pre-checkin since email address not found from pre checked in list response", Reservation.ReservationNameID, "PushDueInReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-In push");
                        }
                    }
                    
                }
                catch (Exception ex)
                {
                    new LogHelper().Error(ex, Reservation.ReservationNameID, "PushDueInReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-In push");
                }
                #endregion

                #region Pushing Reservation Track
                if (IsEmailProcessed != null && IsEmailProcessed.Value)
                {
                    new LogHelper().Log("Pushing reservation track from local DB ", Reservation.ReservationNameID, "PushDueInReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-In push");

                    localResponse = await new WSClientHelper().PushReservationTrackLocally(Reservation.ReservationNameID, new Models.Local.LocalRequestModel()
                    {
                        RequestObject = new Models.Local.ReservationTrackStatus()
                        {
                            ReservationNameID = Reservation.ReservationNameID,
                            ProcessType = Models.Local.ReservationProcessType.Precheckinemail.ToString(),
                            ReservationNumber = !string.IsNullOrEmpty(Reservation.ReservationNumber) ? (Reservation.ReservationNumber.Contains("||") ?
                                                            (Reservation.ReservationNumber.Substring(0, Reservation.ReservationNumber.IndexOf('|') - 1)) : (Reservation.ReservationNumber)) : (Reservation.ReservationNumber),
                            EmailSent = IsEmailProcessed.Value,
                            ProcessStatus = ""
                        }
                    }, "Due-In push",pushReservationRequest.ServiceParameters);
                    if (localResponse.result)
                    {
                        new LogHelper().Log("Reservation track in local DB updated successfully ", Reservation.ReservationNameID, "PushDueInReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-In push");
                    }
                    else
                    {
                        //isReservationTrackPresent = true;
                        new LogHelper().Log("Failled to update reservation track in local DB with reason :- " + localResponse.responseMessage, Reservation.ReservationNameID, "PushDueInReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-In push");
                    }
                }
                #endregion

                #region Pushing record copy in local DB
                new LogHelper().Log("Updating the reservation in Local DB with email send flag ", Reservation.ReservationNameID, "PushDueInReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-In push");
                Reservation.IsEmailSend = isEmailSent;
                Reservation.reservationDocument = null;
                localResponse = await new WSClientHelper().PushRecordLocally(new Models.Local.LocalRequestModel()
                {
                    SyncFromCloud = false,
                    RequestObject = new List<Models.OWS.OperaReservation>() { Reservation }
                }, Reservation.ReservationNameID, "Due-In push", pushReservationRequest.ServiceParameters);
                if (!localResponse.result)
                {
                    new LogHelper().Log("Updating the reservation in Local DB with email send flag failled with reason :- " + localResponse.responseMessage, Reservation.ReservationNameID, "PushDueInReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-In push");
                }
                else
                    new LogHelper().Log("Updating the reservation in Local DB with email send flag fsucceeded", Reservation.ReservationNameID, "PushDueInReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-In push");
                #endregion

                #region Updating record status in Local DB
                new LogHelper().Log("Updating the reservation status in Local DB", Reservation.ReservationNameID, "PushDueInReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-In push");
                localResponse = await new WSClientHelper().UpdateRecordLocally(Reservation.ReservationNameID, new Models.Local.LocalRequestModel()
                {
                    RequestObject = new List<Models.Local.UpdateReservationByReservationNameIDModel>()
                {
                    new Models.Local.UpdateReservationByReservationNameIDModel{
                        IsPushedToCloud = true,
                        ReservationNameID = Reservation.ReservationNameID
                        //StatusDescription = "Success"
                    }
                }
                }, "Due-In push", pushReservationRequest.ServiceParameters);
                if (!localResponse.result)
                {
                    new LogHelper().Log("Updating the reservation status in Local DB with email send flag failled with reason :- " + localResponse.responseMessage, Reservation.ReservationNameID, "PushDueInReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-In push");
                }
                else
                    new LogHelper().Log("Updating the reservation status in Local DB ", Reservation.ReservationNameID, "PushDueInReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-In push");
                #endregion

                return new Models.Local.LocalResponseModel()
                {
                    result = true,
                    responseMessage = "Success"
                };
            }
            catch(Exception ex)
            {
                return new Models.Local.LocalResponseModel()
                {
                    result = false,
                    responseMessage = ex.Message
                };
            }

        }

        public async Task<Models.Local.LocalResponseModel> PushPaymentLink(Models.Local.PushReservationRequest pushReservationRequest)
        {

            #region Variables
            List<Models.OWS.OperaReservation> operaReservationList = null;
            Models.OWS.OperaReservation Reservation = null;
            #endregion

            try
            {
                new LogHelper().Log("Pushing payment link", null, "PushPaymentLink", pushReservationRequest.ServiceParameters.ClientID, "Payment link push");


                bool? isEmailSent = null;
                bool? IsEmailProcessed = null;
                //new LogHelper().Log("Processing reservation No. : " + pushReservationRequest.ReservationNumber, null, "PushPaymentLink", pushReservationRequest.ServiceParameters.ClientID, "Payment link push");

                List<Models.OWS.OperaReservation> temp_operaReservations = null;

                #region Fetching Reservation from OWS
                //new LogHelper().Log("Fetching opera reservation", null, "PushPaymentLink", pushReservationRequest.ServiceParameters.ClientID, "Payment link push");
                Models.OWS.OwsResponseModel owsResponse1 = await new WSClientHelper().FetchReservationAsync(pushReservationRequest.ReservationNumber, new Models.OWS.OwsRequestModel()
                {
                    ChainCode = pushReservationRequest.ServiceParameters.ChainCode,
                    DestinationEntityID = pushReservationRequest.ServiceParameters.DestinationEntityID,
                    HotelDomain = pushReservationRequest.ServiceParameters.HotelDomain,
                    KioskID = pushReservationRequest.ServiceParameters.KioskID,
                    Language = pushReservationRequest.ServiceParameters.Language,
                    LegNumber = pushReservationRequest.ServiceParameters.Legnumber,
                    Password = pushReservationRequest.ServiceParameters.Password,
                    SystemType = pushReservationRequest.ServiceParameters.SystemType,
                    Username = pushReservationRequest.ServiceParameters.Username,
                    FetchBookingRequest = new Models.OWS.FetchBookingRequestModel()
                    {
                        ReservationNumber = pushReservationRequest.ReservationNumber
                        //ReservationNameID = pushReservationRequest.ReservationNameID
                    }
                }, "Payment link push", pushReservationRequest.ServiceParameters);
                if (!owsResponse1.result)
                {
                    new LogHelper().Log("Fetching opera reservation", null, "PushPaymentLink", pushReservationRequest.ServiceParameters.ClientID, "Payment link push");
                    new LogHelper().Warn("Failled to fetch opera reservation with reason :- " + owsResponse1.responseMessage, pushReservationRequest.ReservationNumber, "PushPaymentLink", pushReservationRequest.ServiceParameters.ClientID, "Payment link push");
                    return new Models.Local.LocalResponseModel()
                    {
                        result = false,
                        responseMessage = "Failled to fetch opera reservation with reason :- " + owsResponse1.responseMessage
                    };
                }
                if (owsResponse1.responseData == null)
                {
                    new LogHelper().Log("Failled to fetch opera reservation with reason :- API response data is NULL" + owsResponse1.responseMessage, pushReservationRequest.ReservationNumber, "PushPaymentLink", pushReservationRequest.ServiceParameters.ClientID, "Payment link push");
                    return new Models.Local.LocalResponseModel()
                    {
                        result = false,
                        responseMessage = "Failled to fetch opera reservation with reason :- API response data is NULL" + owsResponse1.responseMessage
                    };
                }
                else
                {
                    new LogHelper().Debug("Converting API json to object", pushReservationRequest.ReservationNumber, "PushPaymentLink", pushReservationRequest.ServiceParameters.ClientID, "Payment link push");
                    try
                    {
                        temp_operaReservations = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Models.OWS.OperaReservation>>(owsResponse1.responseData.ToString());
                        Reservation = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Models.OWS.OperaReservation>>(owsResponse1.responseData.ToString())[0];
                        new LogHelper().Log("Opera reservation fetched successfully", Reservation.ReservationNameID, "PushPaymentLink", pushReservationRequest.ServiceParameters.ClientID, "Payment link push");
                    }
                    catch (Exception ex)
                    {
                        new LogHelper().Error(ex, pushReservationRequest.ReservationNumber, "PushPaymentLink", pushReservationRequest.ServiceParameters.ClientID, "Payment link push");
                        new LogHelper().Log("Failled to covert API response to object", pushReservationRequest.ReservationNumber, "PushPaymentLink", pushReservationRequest.ServiceParameters.ClientID, "Payment link push");
                        new LogHelper().Warn("Failled to fetch opera reservation with reason :- " + ex.Message, pushReservationRequest.ReservationNumber, "PushPaymentLink", pushReservationRequest.ServiceParameters.ClientID, "Payment link push");
                        new LogHelper().Debug("Failled to fetch opera reservation with reason :- " + ex.Message, pushReservationRequest.ReservationNumber, "PushPaymentLink", pushReservationRequest.ServiceParameters.ClientID, "Payment link push");
                        return new Models.Local.LocalResponseModel()
                        {
                            result = false,
                            responseMessage = ex.Message
                        };
                    }
                }

                #endregion
                

                #region Pushing record copy in local DB
                new LogHelper().Log("Updating the reservation in Local DB", Reservation.ReservationNameID, "PushPaymentLink", pushReservationRequest.ServiceParameters.ClientID, "Payment link push");
                //Reservation.IsEmailSend = false;
                //Reservation.reservationDocument = null;
                Models.Local.LocalResponseModel localResponse = await new WSClientHelper().PushRecordLocally(new Models.Local.LocalRequestModel()
                {
                    SyncFromCloud = false,
                    RequestObject = temp_operaReservations,//new List<Models.OWS.OperaReservation>() { Reservation }
                }, Reservation.ReservationNameID, "Payment link push", pushReservationRequest.ServiceParameters);
                if (!localResponse.result)
                {
                    new LogHelper().Log("Updating the reservation in Local DB with email send flag failled with reason :- " + localResponse.responseMessage, Reservation.ReservationNameID, "PushPaymentLink", pushReservationRequest.ServiceParameters.ClientID, "Payment link push");
                }
                else
                    new LogHelper().Log("Updating the reservation in Local DB with email send flag fsucceeded", Reservation.ReservationNameID, "PushPaymentLink", pushReservationRequest.ServiceParameters.ClientID, "Payment link push");
                #endregion

                #region Validating email present or not
                bool isEmailPresent = false;
                if (Reservation != null && Reservation.GuestProfiles != null && Reservation.GuestProfiles.Count > 0 &&
                    Reservation.GuestProfiles[0].Email != null && Reservation.GuestProfiles[0].Email.Count > 0 &&
                    !string.IsNullOrEmpty(Reservation.GuestProfiles[0].Email[0].email))
                    isEmailPresent = true;
                if (!isEmailPresent)
                {
                    new LogHelper().Log("Skipping reservation No. : " + Reservation.ReservationNumber + " no email ID present", Reservation.ReservationNameID, "PushPaymentLink", pushReservationRequest.ServiceParameters.ClientID, "Payment link push");
                    return new Models.Local.LocalResponseModel()
                    {
                        result = false,
                        responseMessage = " no email ID present"
                    };
                }
                #endregion

                #region Processing RoomRate
                new LogHelper().Log("Updating rate details", Reservation.ReservationNameID, "PushPaymentLink", pushReservationRequest.ServiceParameters.ClientID, "Payment link push");
                if (Reservation.RateDetails != null && Reservation.RateDetails.DailyRates != null && Reservation.RateDetails.DailyRates.Count > 0)
                {
                    decimal total_roomrate = Reservation.RateDetails.DailyRates.Sum(x => x.Amount);
                    if (Reservation.PrintRate != null && Reservation.PrintRate.Value)
                        Reservation.RateDetails.RateAmount = total_roomrate;
                    else
                        Reservation.TotalAmount = 0;
                    new LogHelper().Log("Updating rate details completed", Reservation.ReservationNameID, "PushPaymentLink", pushReservationRequest.ServiceParameters.ClientID, "Payment link push");
                }
                else
                    new LogHelper().Log("Updating rate details failed because Rate details are blank in the reservation", Reservation.ReservationNameID, "PushPaymentLink", pushReservationRequest.ServiceParameters.ClientID, "Payment link push");
                #endregion


                #region Processing MealPlan
                if (pushReservationRequest.ServiceParameters.IsBreakFastValidationWithUDF != null && pushReservationRequest.ServiceParameters.IsBreakFastValidationWithUDF.Value)
                {
                    new LogHelper().Debug("Processing meal plan from UDF fields", Reservation.ReservationNameID, "PushPaymentLink", "Grabber", "Payment link push");
                    if (Reservation.userDefinedFields != null && Reservation.userDefinedFields.Count > 0)
                    {
                        if (Reservation.userDefinedFields.Find(x => x.FieldName.Equals(pushReservationRequest.ServiceParameters.MealPlanFieldName)) != null)
                        {

                            if (!Reservation.userDefinedFields.Find(x => x.FieldName.Equals(pushReservationRequest.ServiceParameters.MealPlanFieldName)).FieldValue.Equals("NP"))
                            {
                                string tempUDFValue = Reservation.userDefinedFields.Find(x => x.FieldName.Equals(pushReservationRequest.ServiceParameters.MealPlanFieldName)).FieldValue;
                                if (!string.IsNullOrEmpty(tempUDFValue))
                                {
                                    bool isPackageFound = false;
                                    if (pushReservationRequest.ServiceParameters.PackageCodes.Split(';').ToList().Contains(tempUDFValue))
                                        isPackageFound = true;
                                    if (isPackageFound)
                                    {
                                        Reservation.IsBreakFastAvailable = true;
                                        new LogHelper().Debug("Meal plan updated", Reservation.ReservationNameID, "PushPaymentLink", "Grabber", "Payment link push");
                                    }
                                }
                            }
                            else
                                new LogHelper().Log("Processing meal plan not updated (NP not present in UDF)", Reservation.ReservationNameID, "PushPaymentLink", "Grabber", "Payment link push");
                        }
                    }
                    else
                        new LogHelper().Log("No UDF fields for meal plan not found", Reservation.ReservationNameID, "PushPaymentLink", "Grabber", "Payment link push");
                }
                if (pushReservationRequest.ServiceParameters.IsBreakFastValidationWithPackage != null && pushReservationRequest.ServiceParameters.IsBreakFastValidationWithPackage.Value)
                {
                    new LogHelper().Debug("Processing package list in the reservation for meal plan", Reservation.ReservationNameID, "PushPaymentLink", "Grabber", "Payment link push");
                    if (((Reservation.PackageDetails != null && Reservation.PackageDetails.Count > 0) || (Reservation.PreferanceDetails != null && Reservation.PreferanceDetails.Count > 0)) && (!string.IsNullOrEmpty(pushReservationRequest.ServiceParameters.PackageCodes) && pushReservationRequest.ServiceParameters.PackageCodes.Split(';').ToList() != null))
                    {
                        if (Reservation.PackageDetails != null && Reservation.PackageDetails.Count > 0)
                        {
                            bool isPackageFound = false;
                            foreach (Models.OWS.PackageDetails package in Reservation.PackageDetails)
                            {
                                if (pushReservationRequest.ServiceParameters.PackageCodes.Split(';').ToList().Contains(package.PackageCode))
                                {
                                    isPackageFound = true;
                                    break;
                                }
                            }
                            if (isPackageFound)
                            {
                                Reservation.IsBreakFastAvailable = true;
                                new LogHelper().Debug("Meal plan updated", Reservation.ReservationNameID, "PushPaymentLink", "Grabber", "Payment link push");
                            }
                        }
                        if (Reservation.PreferanceDetails != null && Reservation.PreferanceDetails.Count > 0)
                        {
                            if (Reservation.IsBreakFastAvailable == null || !Reservation.IsBreakFastAvailable.Value)
                            {
                                bool isPrefernceFound = false;
                                foreach (Models.OWS.PreferanceDetails prefernce in Reservation.PreferanceDetails)
                                {
                                    if (pushReservationRequest.ServiceParameters.PackageCodes.Split(';').ToList().Contains(prefernce.PreferanceCode))
                                    {
                                        isPrefernceFound = true;
                                        break;
                                    }
                                }
                                if (isPrefernceFound)
                                {
                                    Reservation.IsBreakFastAvailable = true;
                                    new LogHelper().Debug("Meal plan updated", Reservation.ReservationNameID, "PushPaymentLink", "Grabber", "Payment link push");
                                }
                            }
                        }
                    }
                    else
                        new LogHelper().Log("No package or prefernce list in the reservation for meal plan not found", Reservation.ReservationNameID, "PushPaymentLink", "Grabber", "Payment link push");
                }
                #endregion



                #region VerifyVIPReservationOrNot
                new LogHelper().Log("Verifying reservation VIP status in UDF field", Reservation.ReservationNameID, "PushPaymentLink", pushReservationRequest.ServiceParameters.ClientID, "Payment link push");
                if (Reservation.userDefinedFields != null && Reservation.userDefinedFields.Count > 0)
                {
                    if (Reservation.userDefinedFields.Find(x => x.FieldName.Equals(pushReservationRequest.ServiceParameters.PreAuthUDF)) != null &&
                        Reservation.userDefinedFields.Find(x => x.FieldName.Equals(pushReservationRequest.ServiceParameters.PreAuthUDF)).FieldValue.Equals("NO"))
                    {
                        new LogHelper().Log("Reservation is flagged not to take payment", Reservation.ReservationNameID, "PushPaymentLink", pushReservationRequest.ServiceParameters.ClientID, "Payment link push");
                        Reservation.DepositDetail = new List<Models.OWS.DepositDetail>()
                {
                    new Models.OWS.DepositDetail()
                    {
                        Amount = 0,
                        CardExpiryDate = null,
                        CreditCardNumber = null,
                        IsCreditCardDeposit = false,
                        PaymentType = null
                    }
                };
                    }
                    else if (Reservation.userDefinedFields.Find(x => x.FieldName.Equals(pushReservationRequest.ServiceParameters.PreAuthAmntUDF)) != null &&
                        Reservation.userDefinedFields.Find(x => x.FieldName.Equals(pushReservationRequest.ServiceParameters.PreAuthAmntUDF)).FieldValue.Equals("NO"))
                    {
                        new LogHelper().Log("Reservation is flagged not to take payment", Reservation.ReservationNameID, "PushPaymentLink", pushReservationRequest.ServiceParameters.ClientID, "Payment link push");
                        Reservation.DepositDetail = new List<Models.OWS.DepositDetail>()
                        {
                            new Models.OWS.DepositDetail()
                            {
                                Amount = 0,
                                CardExpiryDate = null,
                                CreditCardNumber = null,
                                IsCreditCardDeposit = false,
                                PaymentType = null
                            }
                        };
                    }
                }
                new LogHelper().Log("Verifying reservation VIP status in UDF field completed", Reservation.ReservationNameID, "PushPaymentLink", pushReservationRequest.ServiceParameters.ClientID, "Payment link push");
                #endregion

                #region PaymentDesabling
                if (pushReservationRequest.ServiceParameters.IsPaymentDisabled)
                {
                    new LogHelper().Log("Disabling the payment as per the config", Reservation.ReservationNameID, "PushPaymentLink", pushReservationRequest.ServiceParameters.ClientID, "Payment link push");
                    Reservation.DepositDetail = new List<Models.OWS.DepositDetail>()
                {
                    new Models.OWS.DepositDetail()
                    {
                        Amount = 0,
                        CardExpiryDate = null,
                        CreditCardNumber = null,
                        IsCreditCardDeposit = false,
                        PaymentType = null
                    }
                };
                }
                #endregion

                #region Update ETA
                if (pushReservationRequest.ServiceParameters.IsETADefault)
                {
                    new LogHelper().Log("Assigning NULL value to ETA as per the config", Reservation.ReservationNameID, "PushPaymentLink", pushReservationRequest.ServiceParameters.ClientID, "Payment link push");
                    Reservation.ExpectedArrivalTime = null;
                }
                #endregion

                #region Push due in record
                new LogHelper().Log("Pushing due in reservation, reservation No. : " + Reservation.ReservationNumber, Reservation.ReservationNameID, "PushPaymentLink", pushReservationRequest.ServiceParameters.ClientID, "Payment link push");
                Models.Cloud.CloudResponseModel cloudResponse = await new WSClientHelper().PushRecordToCloud(Reservation.ReservationNameID, new Models.Cloud.CloudRequestModel()
                {
                    RequestObject = new List<Models.OWS.OperaReservation>() { Reservation }
                }, "Payment link push", pushReservationRequest.ServiceParameters);
                if (!cloudResponse.result)
                {
                    new LogHelper().Log("Failled to push the reservation to cloud, so skipping the reservation ", Reservation.ReservationNameID, "PushPaymentLink", pushReservationRequest.ServiceParameters.ClientID, "Payment link push");
                    return new Models.Local.LocalResponseModel()
                    {
                        result = false,
                        responseMessage = "Failled to push the reservation to cloud, so skipping the reservation "
                    };
                }
                new LogHelper().Log("Reservation pushed to cloud successfully", Reservation.ReservationNameID, "PushPaymentLink", pushReservationRequest.ServiceParameters.ClientID, "Payment link push");

                #endregion

                #region Fetching Reservation Track whethere is email is already send or not
                //new LogHelper().Log("Fetching reservation track from local DB for status (email send or not)", Reservation.ReservationNameID, "PushPaymentLink", pushReservationRequest.ServiceParameters.ClientID, "Payment link push");

                //Models.Local.LocalResponseModel localWebResponse = await new WSClientHelper().FetchReservationTracjLocally(Reservation.ReservationNameID, new Models.Local.LocalRequestModel()
                //{

                //    RequestObject = new Models.Local.ReservationTrackStatus()
                //    {
                //        ReservationNameID = Reservation.ReservationNameID,
                //        ProcessType = Models.Local.ReservationProcessType.Precheckinemail.ToString()
                //    }
                //}, "Payment link push", pushReservationRequest.ServiceParameters);

                //if (localWebResponse.result)
                //{
                //    new LogHelper().Log("Reservation track in local DB fetched successfully ", Reservation.ReservationNameID, "PushPaymentLink", pushReservationRequest.ServiceParameters.ClientID, "Payment link push");
                //    if (localWebResponse.responseData != null)
                //    {
                //        List<Models.Local.ReservationTrackStatus> reservationTracks = null;
                //        new LogHelper().Debug("Converting API json to object", Reservation.ReservationNameID, "PushPaymentLink", pushReservationRequest.ServiceParameters.ClientID, "Payment link push");
                //        try
                //        {
                //            reservationTracks = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Models.Local.ReservationTrackStatus>>(localWebResponse.responseData.ToString());
                //        }
                //        catch (Exception ex)
                //        {
                //            isEmailSent = false;
                //            new LogHelper().Error(ex, Reservation.ReservationNameID, "PushPaymentLink", pushReservationRequest.ServiceParameters.ClientID, "Payment link push");
                //        }
                //        if (reservationTracks != null && reservationTracks.Count > 0)
                //        {
                //            isEmailSent = true;
                //            new LogHelper().Debug("Pre checkin email already send", Reservation.ReservationNameID, "PushPaymentLink", pushReservationRequest.ServiceParameters.ClientID, "Payment link push");
                //        }
                //    }
                //    else
                //    {
                //        isEmailSent = false;
                //        new LogHelper().Log("Resrvation track not present ", Reservation.ReservationNameID, "PushPaymentLink", pushReservationRequest.ServiceParameters.ClientID, "Payment link push");
                //    }
                //}
                //else
                //{
                //    isEmailSent = false;
                //    new LogHelper().Log("Failled to fetch reservation track in local DB with reason :- " + localWebResponse.responseMessage, Reservation.ReservationNameID, "PushPaymentLink", pushReservationRequest.ServiceParameters.ClientID, "Payment link push");
                //}


                #endregion

                #region Sending Email
                try
                {

                    new LogHelper().Log("Verifying email send or not ", Reservation.ReservationNameID, "PushPaymentLink", pushReservationRequest.ServiceParameters.ClientID, "Payment link push");
                    //if (isEmailSent == null || !isEmailSent.Value)
                    {
                        new LogHelper().Log("Sending pre-checkin email", Reservation.ReservationNameID, "PushPaymentLink", pushReservationRequest.ServiceParameters.ClientID, "Payment link push");
                        if (Reservation != null && Reservation.GuestProfiles != null && Reservation.GuestProfiles[0].Email != null && Reservation.GuestProfiles[0].Email.Count > 0)
                        {
                            foreach (Models.OWS.Email email in Reservation.GuestProfiles[0].Email)
                            {
                                if (email.primary != null && email.primary.Value)
                                {
                                    TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
                                    Models.Email.EmailResponse emailResponse = await new WSClientHelper().SendEmail(Reservation.ReservationNameID, new Models.Email.EmailRequest()
                                    {
                                        FromEmail = pushReservationRequest.ServiceParameters.PreArrivalFromEmail,
                                        ToEmail = email.email,
                                        GuestName = !string.IsNullOrEmpty(Reservation.GuestProfiles[0].GuestName) ? textInfo.ToTitleCase(Reservation.GuestProfiles[0].GuestName)
                                        : Reservation.GuestProfiles[0].GuestName,
                                        Subject = pushReservationRequest.ServiceParameters.PreArrivalEmailSubject,
                                        confirmationNumber = "?id=" + HttpUtility.UrlEncode(new Helper().EncryptString("b14ca5898a4e4133bbce2ea2315a1916", Reservation.ReservationNumber)),
                                        displayFromEmail = pushReservationRequest.ServiceParameters.EmailDisplayName,
                                        EmailType = Models.Email.EmailType.Precheckedin,
                                        AttachmentFileName = "WelcomeEmail.pdf",
                                        ReservationNumber = !string.IsNullOrEmpty(Reservation.ReservationNumber) ? (Reservation.ReservationNumber.Contains("||") ?
                                                            (Reservation.ReservationNumber.Substring(0, Reservation.ReservationNumber.IndexOf('|') - 1)) : (Reservation.ReservationNumber)) : (Reservation.ReservationNumber),
                                        ArrivalDate = Reservation.ArrivalDate.Value.ToString("dd-MMM-yyyy"),
                                        DepartureDate = Reservation.DepartureDate.Value.ToString("dd-MMM-yyy")

                                    }, "Payment link push", pushReservationRequest.ServiceParameters);
                                    if (!emailResponse.result)
                                    {
                                        isEmailSent = false;
                                        IsEmailProcessed = false;
                                        new LogHelper().Log("Failled to send confirmation email with reason :- " + emailResponse.responseMessage, Reservation.ReservationNameID, "PushPaymentLink", pushReservationRequest.ServiceParameters.ClientID, "Payment link push");
                                        new LogHelper().Warn("Failled to send confirmation email with reason :- " + emailResponse.responseMessage, Reservation.ReservationNameID, "PushPaymentLink", pushReservationRequest.ServiceParameters.ClientID, "Payment link push");
                                    }
                                    else
                                    {
                                        isEmailSent = true;
                                        IsEmailProcessed = true;
                                        new LogHelper().Log("Email send successfully", Reservation.ReservationNameID, "PushPaymentLink", pushReservationRequest.ServiceParameters.ClientID, "Payment link push");
                                    }
                                }
                            }
                        }
                        else
                        {
                            new LogHelper().Log("Failled to send pre-checkin since email address not found from pre checked in list response", Reservation.ReservationNameID, "PushPaymentLink", pushReservationRequest.ServiceParameters.ClientID, "Payment link push");
                            new LogHelper().Warn("Failled to send pre-checkin since email address not found from pre checked in list response", Reservation.ReservationNameID, "PushPaymentLink", pushReservationRequest.ServiceParameters.ClientID, "Payment link push");
                        }
                    }

                }
                catch (Exception ex)
                {
                    new LogHelper().Error(ex, Reservation.ReservationNameID, "PushPaymentLink", pushReservationRequest.ServiceParameters.ClientID, "Payment link push");
                }
                #endregion

                #region Pushing Reservation Track
                if (IsEmailProcessed != null && IsEmailProcessed.Value)
                {
                    new LogHelper().Log("Pushing reservation track from local DB ", Reservation.ReservationNameID, "PushPaymentLink", pushReservationRequest.ServiceParameters.ClientID, "Payment link push");

                    localResponse = await new WSClientHelper().PushReservationTrackLocally(Reservation.ReservationNameID, new Models.Local.LocalRequestModel()
                    {
                        RequestObject = new Models.Local.ReservationTrackStatus()
                        {
                            ReservationNameID = Reservation.ReservationNameID,
                            ProcessType = Models.Local.ReservationProcessType.Precheckinemail.ToString(),
                            ReservationNumber = !string.IsNullOrEmpty(Reservation.ReservationNumber) ? (Reservation.ReservationNumber.Contains("||") ?
                                                            (Reservation.ReservationNumber.Substring(0, Reservation.ReservationNumber.IndexOf('|') - 1)) : (Reservation.ReservationNumber)) : (Reservation.ReservationNumber),
                            EmailSent = IsEmailProcessed.Value,
                            ProcessStatus = ""
                        }
                    }, "Payment link push", pushReservationRequest.ServiceParameters);
                    if (localResponse.result)
                    {
                        new LogHelper().Log("Reservation track in local DB updated successfully ", Reservation.ReservationNameID, "PushPaymentLink", pushReservationRequest.ServiceParameters.ClientID, "Payment link push");
                    }
                    else
                    {
                        //isReservationTrackPresent = true;
                        new LogHelper().Log("Failled to update reservation track in local DB with reason :- " + localResponse.responseMessage, Reservation.ReservationNameID, "PushPaymentLink", pushReservationRequest.ServiceParameters.ClientID, "Payment link push");
                    }
                }
                #endregion

                #region Pushing record copy in local DB
                new LogHelper().Log("Updating the reservation in Local DB with email send flag ", Reservation.ReservationNameID, "PushPaymentLink", pushReservationRequest.ServiceParameters.ClientID, "Payment link push");
                Reservation.IsEmailSend = isEmailSent;
                Reservation.reservationDocument = null;
                localResponse = await new WSClientHelper().PushRecordLocally(new Models.Local.LocalRequestModel()
                {
                    SyncFromCloud = false,
                    RequestObject = new List<Models.OWS.OperaReservation>() { Reservation }
                }, Reservation.ReservationNameID, "Payment link push", pushReservationRequest.ServiceParameters);
                if (!localResponse.result)
                {
                    new LogHelper().Log("Updating the reservation in Local DB with email send flag failled with reason :- " + localResponse.responseMessage, Reservation.ReservationNameID, "PushPaymentLink", pushReservationRequest.ServiceParameters.ClientID, "Payment link push");
                }
                else
                    new LogHelper().Log("Updating the reservation in Local DB with email send flag fsucceeded", Reservation.ReservationNameID, "PushPaymentLink", pushReservationRequest.ServiceParameters.ClientID, "Payment link push");
                #endregion

                #region Updating record status in Local DB
                new LogHelper().Log("Updating the reservation status in Local DB", Reservation.ReservationNameID, "PushPaymentLink", pushReservationRequest.ServiceParameters.ClientID, "Payment link push");
                localResponse = await new WSClientHelper().UpdateRecordLocally(Reservation.ReservationNameID, new Models.Local.LocalRequestModel()
                {
                    RequestObject = new List<Models.Local.UpdateReservationByReservationNameIDModel>()
                {
                    new Models.Local.UpdateReservationByReservationNameIDModel{
                        IsPushedToCloud = true,
                        ReservationNameID = Reservation.ReservationNameID
                        //StatusDescription = "Success"
                    }
                }
                }, "Payment link push", pushReservationRequest.ServiceParameters);
                if (!localResponse.result)
                {
                    new LogHelper().Log("Updating the reservation status in Local DB with email send flag failled with reason :- " + localResponse.responseMessage, Reservation.ReservationNameID, "PushPaymentLink", pushReservationRequest.ServiceParameters.ClientID, "Payment link push");
                }
                else
                    new LogHelper().Log("Updating the reservation status in Local DB ", Reservation.ReservationNameID, "PushPaymentLink", pushReservationRequest.ServiceParameters.ClientID, "Payment link push");
                #endregion

                return new Models.Local.LocalResponseModel()
                {
                    result = true,
                    responseMessage = "Success"
                };
            }
            catch (Exception ex)
            {
                return new Models.Local.LocalResponseModel()
                {
                    result = false,
                    responseMessage = ex.Message
                };
            }

        }

        public async Task<Models.Local.LocalResponseModel> PushDueOutReservation(Models.Local.PushReservationRequest pushReservationRequest)
        {
            

            #region Variables

            List<Models.OWS.OperaReservation> operaReservationList = null;
            Models.OWS.OperaReservation Reservation = null;
            List<Models.Local.PaymentHeader> paymentHeaders = null;
            Models.OWS.FolioModel guestFolio = null;
            string folioAsBase64 = "";
            bool isEmailSend = false;
            #endregion

            try
            {
                new LogHelper().Log("Pushing due out reservation list", null, "PushDueOutReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-Out push");


                isEmailSend = false;

                new LogHelper().Log("Processing reservation No. : " + pushReservationRequest.ReservationNumber, null, "PushDueOutReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-Out push");

                #region Fetching Reservation from OWS
                new LogHelper().Log("Fetching opera reservation", pushReservationRequest.ReservationNumber, "PushDueOutReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-Out push");
                Models.OWS.OwsResponseModel owsResponse1 = await new WSClientHelper().FetchReservationAsync(pushReservationRequest.ReservationNumber, new Models.OWS.OwsRequestModel()
                {
                    ChainCode = pushReservationRequest.ServiceParameters.ChainCode,
                    DestinationEntityID = pushReservationRequest.ServiceParameters.DestinationEntityID,
                    HotelDomain = pushReservationRequest.ServiceParameters.HotelDomain,
                    KioskID = pushReservationRequest.ServiceParameters.KioskID,
                    Language = pushReservationRequest.ServiceParameters.Language,
                    LegNumber = pushReservationRequest.ServiceParameters.Legnumber,
                    Password = pushReservationRequest.ServiceParameters.Password,
                    SystemType = pushReservationRequest.ServiceParameters.SystemType,
                    Username = pushReservationRequest.ServiceParameters.Username,
                    FetchBookingRequest = new Models.OWS.FetchBookingRequestModel()
                    {
                        ReservationNumber = pushReservationRequest.ReservationNumber
                    }
                }, "Due-Out push", pushReservationRequest.ServiceParameters);
                if (!owsResponse1.result)
                {
                    new LogHelper().Log("Failled to fetch opera reservation with reason :- " + owsResponse1.responseMessage, pushReservationRequest.ReservationNumber, "PushDueOutReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-Out push");
                    return new Models.Local.LocalResponseModel()
                    {
                        result = false,
                        responseMessage = "Failled to fetch opera reservation with reason :- " + owsResponse1.responseMessage
                    };
                }
                if (owsResponse1.responseData == null)
                {
                    new LogHelper().Log("Failled to fetch opera reservation with reason :- API response data is NULL" + owsResponse1.responseMessage, pushReservationRequest.ReservationNumber, "PushDueOutReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-Out push");
                    return new Models.Local.LocalResponseModel()
                    {
                        result = false,
                        responseMessage = "Failled to fetch opera reservation with reason :- API response data is NULL" + owsResponse1.responseMessage
                    };
                }
                else
                {
                    new LogHelper().Debug("Converting API json to object", pushReservationRequest.ReservationNumber, "PushDueOutReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-Out push");
                    try
                    {
                        List<Models.OWS.OperaReservation> temp_operaReservations = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Models.OWS.OperaReservation>>(owsResponse1.responseData.ToString());
                        Reservation = temp_operaReservations[0];
                        new LogHelper().Log("Opera reservation fetched successfully", Reservation.ReservationNameID, "PushDueOutReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-Out push");
                    }
                    catch (Exception ex)
                    {
                        new LogHelper().Error(ex, Reservation.ReservationNameID, "PushDueOutReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-Out push");
                        new LogHelper().Log("Failled to covert API response to object", Reservation.ReservationNameID, "PushDueOutReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-Out push");
                        new LogHelper().Warn("Failled to fetch opera reservation with reason :- " + ex.Message, Reservation.ReservationNameID, "PushDueOutReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-Out push");
                        new LogHelper().Debug("Failled to fetch opera reservation with reason :- " + ex.Message, Reservation.ReservationNameID, "PushDueOutReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-Out push");
                        return new Models.Local.LocalResponseModel()
                        {
                            result = false,
                            responseMessage = ex.Message
                        };
                    }
                }

                #endregion

                #region Verifying Day use guest
                if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["IsDayUseReservationCheckOutEnabled"]) && !bool.TryParse(ConfigurationManager.AppSettings["IsDayUseReservationCheckOutEnabled"].ToString(), out bool result))
                {
                    new LogHelper().Log("verifying day use guest or not", Reservation.ReservationNameID, "PushDueOutReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-Out push");
                    if (Reservation != null)
                    {
                        if (Reservation.ArrivalDate != null && Reservation.DepartureDate != null && Reservation.ArrivalDate.Value.Equals(Reservation.DepartureDate.Value))
                        {
                            new LogHelper().Log("reservation is a day use guest, so skipping the reservation", Reservation.ReservationNameID, "PushDueOutReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-Out push");
                            return new Models.Local.LocalResponseModel()
                            {
                                result = false,
                                responseMessage = "reservation is a day use guest, so skipping the reservation"
                            };
                        }
                        else
                            new LogHelper().Log("reservation is not a day use guest", Reservation.ReservationNameID, "PushDueOutReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-Out push");
                    }
                    else
                        new LogHelper().Log("verifying day use guest or not failled reservation object is blank", Reservation.ReservationNameID, "PushDueOutReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-Out push");
                }
                #endregion

                #region Validating email present or not
                bool isEmailPresent = false;
                if (Reservation != null && Reservation.GuestProfiles != null && Reservation.GuestProfiles.Count > 0 &&
                    Reservation.GuestProfiles[0].Email != null && Reservation.GuestProfiles[0].Email.Count > 0 &&
                    !string.IsNullOrEmpty(Reservation.GuestProfiles[0].Email[0].email))
                    isEmailPresent = true;
                if (!isEmailPresent)
                {
                    new LogHelper().Log("Skipping reservation No. : " + Reservation.ReservationNumber + " no email ID present", Reservation.ReservationNameID, "PushDueOutReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-Out push");
                    return new Models.Local.LocalResponseModel()
                    {
                        result = false,
                        responseMessage = "Skipping reservation No. : " + Reservation.ReservationNumber + " no email ID present"
                    };
                }
                #endregion

                #region Verifying the reservation Status
                if (string.IsNullOrEmpty(Reservation.ReservationStatus) || !Reservation.ReservationStatus.Equals("DUEOUT"))
                {
                    new LogHelper().Log("Reservation status : " + Reservation.ReservationStatus + " (not DUEOUT, so skipping the reservation)", Reservation.ReservationNameID, "PushDueOutReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-Out push");
                    return new Models.Local.LocalResponseModel()
                    {
                        result = false,
                        responseMessage = "Reservation status : " + Reservation.ReservationStatus + " (not DUEOUT, so skipping the reservation)"
                    };
                }
                #endregion

                #region Check Payment in Saavy
                //Models.Local.LocalResponseModel localResponse = null;
                new LogHelper().Log("Fetching payment details for reservation No. : " + Reservation.ReservationNumber + " in Saavy Pay", Reservation.ReservationNameID, "PushDueOutReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-Out push");
                Models.Local.LocalResponseModel localResponse = await new WSClientHelper().FetchPaymentDetails(Reservation.ReservationNameID, new Models.Local.LocalRequestModel()
                {
                    RequestObject = new Models.Local.FetchPaymentRequest()
                    {
                        ReservationNameID = Reservation.ReservationNameID,
                        isActive = true
                    }
                }, "Due-Out push", pushReservationRequest.ServiceParameters);

                if (!localResponse.result || localResponse.responseData == null)
                {
                    new LogHelper().Log("Failled to fetch payment details with reason :- " + localResponse.responseMessage, Reservation.ReservationNameID, "PushDueOutReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-Out push");
                    new LogHelper().Warn("Failled to fetch payment details with reason :- " + localResponse.responseMessage, Reservation.ReservationNameID, "PushDueOutReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-Out push");

                }
                else

                {
                    new LogHelper().Debug("Converting API json to object", Reservation.ReservationNameID, "PushDueOutReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-Out push");
                    try
                    {
                        paymentHeaders = JsonConvert.DeserializeObject<List<Models.Local.PaymentHeader>>(localResponse.responseData.ToString());
                        new LogHelper().Log("Payment details fetched successfully", Reservation.ReservationNameID, "PushDueOutReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-Out push");
                    }
                    catch (Exception ex)
                    {
                        new LogHelper().Error(ex, Reservation.ReservationNameID, "PushDueOutReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-Out push");
                        new LogHelper().Log("Failled to covert API response to object", Reservation.ReservationNameID, "PushDueOutReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-Out push");
                        new LogHelper().Warn("Failled to fetch payment details with reason :- " + ex.Message, Reservation.ReservationNameID, "PushDueOutReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-Out push");
                        new LogHelper().Debug("Failled to fetch payment details with reason :- " + ex.Message, Reservation.ReservationNameID, "PushDueOutReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-Out push");
                    }
                }

                #endregion

                #region FetchFolioItemsByWindow
                new LogHelper().Log("Fetching reservation folio by window for reservation No. : " + Reservation.ReservationNumber, Reservation.ReservationNameID, "PushDueOutReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-Out push");
                owsResponse1 = await new WSClientHelper().GetFolioByWindow(Reservation.ReservationNameID, new Models.OWS.OwsRequestModel()
                {
                    ChainCode = pushReservationRequest.ServiceParameters.ChainCode,
                    DestinationEntityID = pushReservationRequest.ServiceParameters.DestinationEntityID,
                    HotelDomain = pushReservationRequest.ServiceParameters.HotelDomain,
                    KioskID = pushReservationRequest.ServiceParameters.KioskID,
                    Language = pushReservationRequest.ServiceParameters.Language,
                    LegNumber = pushReservationRequest.ServiceParameters.Legnumber,
                    Password = pushReservationRequest.ServiceParameters.Password,
                    SystemType = pushReservationRequest.ServiceParameters.SystemType,
                    Username = pushReservationRequest.ServiceParameters.Username,
                    FetchFolioRequest = new Models.OWS.FetchFolioRequest()
                    {
                        ReservationNameID = Reservation.ReservationNameID,
                        ProfileID = (Reservation.GuestProfiles != null && Reservation.GuestProfiles.Count > 0) ? Reservation.GuestProfiles[0].PmsProfileID : ""
                    }
                }, "Due-Out push", pushReservationRequest.ServiceParameters);

                if (!owsResponse1.result )
                {
                    new LogHelper().Log("Failled to fetch folio by window with reason :- " + owsResponse1.responseMessage, Reservation.ReservationNameID, "PushDueOutReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-Out push");
                    new LogHelper().Warn("Failled to fetch folio by window with reason :- " + owsResponse1.responseMessage, Reservation.ReservationNameID, "PushDueOutReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-Out push");
                    return new Models.Local.LocalResponseModel()
                    {
                        result = false,
                        responseMessage = "Failled to fetch folio by window with reason :- " + owsResponse1.responseMessage
                    };
                }
                else
                {
                    new LogHelper().Debug("Converting API json to object", Reservation.ReservationNameID, "PushDueOutReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-Out push");
                    try
                    {
                       
                        guestFolio = JsonConvert.DeserializeObject<Models.OWS.FolioModel>(owsResponse1.responseData.ToString());
                        if (guestFolio != null)
                        {
                            new LogHelper().Log("Current guest balance of the reservation is : " + guestFolio.BalanceAmount, Reservation.ReservationNameID, "PushDueOutReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-Out push");
                            new LogHelper().Log("Reservation folio by window fetched successfully", Reservation.ReservationNameID, "PushDueOutReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-Out push");
                        }
                        else
                        {
                            new LogHelper().Debug("No folio items found in the reservation", Reservation.ReservationNameID, "PushDueOutReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-Out push");
                        }
                    }
                    catch (Exception ex)
                    {
                        new LogHelper().Error(ex, Reservation.ReservationNameID, "PushDueOutReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-Out push");
                        new LogHelper().Log("Failled to covert API response to object", Reservation.ReservationNameID, "PushDueOutReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-Out push");
                        new LogHelper().Warn("Failled to fetch folio by window with reason :- " + ex.Message, Reservation.ReservationNameID, "PushDueOutReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-Out push");
                        new LogHelper().Debug("Failled to fetch folio by window with reason :- " + ex.Message, Reservation.ReservationNameID, "PushDueOutReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-Out push");
                    }
                }
                #endregion

                bool isOPIEnabled = false;
                isOPIEnabled = (ConfigurationManager.AppSettings["OPIEnabled"] != null
                                && !string.IsNullOrEmpty(ConfigurationManager.AppSettings["OPIEnabled"].ToString())
                                && bool.TryParse(ConfigurationManager.AppSettings["OPIEnabled"].ToString(), out isOPIEnabled)) ? isOPIEnabled : false;
                bool IsPaymentDisabled = false;
                IsPaymentDisabled = (ConfigurationManager.AppSettings["IsPaymentDisabled"] != null
                                && !string.IsNullOrEmpty(ConfigurationManager.AppSettings["IsPaymentDisabled"].ToString())
                                && bool.TryParse(ConfigurationManager.AppSettings["IsPaymentDisabled"].ToString(), out IsPaymentDisabled)) ? IsPaymentDisabled : false;
                if (isOPIEnabled || IsPaymentDisabled)
                {
                    
                    if ((paymentHeaders == null || paymentHeaders.Count == 0) && guestFolio.BalanceAmount > 0)
                    {
                        new LogHelper().Log("Not able to process reservation No. : " + Reservation.ReservationNumber + " because there is no payment details in the saavy pay as well as current balance is greater than 0", Reservation.ReservationNameID, "PushDueOutReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-Out push");
                        return new Models.Local.LocalResponseModel()
                        {
                            result = false,
                            responseMessage = "Not able to process reservation No. : " + Reservation.ReservationNumber + " because there is no payment details in the saavy pay as well as current balance is greater than 0"
                        };
                    }
                }

                #region FetchFolioAsBase64
                new LogHelper().Log("Fetching reservation folio as base64 for reservation No. : " + Reservation.ReservationNumber, Reservation.ReservationNameID, "PushDueOutReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-Out push");
                owsResponse1 = await new WSClientHelper().GetFolio(Reservation.ReservationNameID, new Models.OWS.OwsRequestModel()
                {
                    ChainCode = pushReservationRequest.ServiceParameters.ChainCode,
                    DestinationEntityID = pushReservationRequest.ServiceParameters.DestinationEntityID,
                    HotelDomain = pushReservationRequest.ServiceParameters.HotelDomain,
                    KioskID = pushReservationRequest.ServiceParameters.KioskID,
                    Language = pushReservationRequest.ServiceParameters.Language,
                    LegNumber = pushReservationRequest.ServiceParameters.Legnumber,
                    Password = pushReservationRequest.ServiceParameters.Password,
                    SystemType = pushReservationRequest.ServiceParameters.SystemType,
                    Username = pushReservationRequest.ServiceParameters.Username,
                    FetchFolioRequest = new Models.OWS.FetchFolioRequest()
                    {
                        ReservationNameID = Reservation.ReservationNameID,
                        OperaReservation = Reservation,
                        ProfileID = (Reservation.GuestProfiles != null && Reservation.GuestProfiles.Count > 0) ? Reservation.GuestProfiles[0].PmsProfileID : "",
                        FolioList = guestFolio
                    }
                }, "Due-Out push", pushReservationRequest.ServiceParameters);

                if (!owsResponse1.result || owsResponse1.responseData == null)
                {
                    new LogHelper().Log("Failled to fetch folio with reason :- " + owsResponse1.responseMessage, Reservation.ReservationNameID, "PushDueOutReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-Out push");
                    new LogHelper().Warn("Failled to fetch folio with reason :- " + owsResponse1.responseMessage, Reservation.ReservationNameID, "PushDueOutReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-Out push");
                    return new Models.Local.LocalResponseModel()
                    {
                        result = false,
                        responseMessage = "Failled to fetch folio with reason :- " + owsResponse1.responseMessage
                    };
                }
                else
                {
                    folioAsBase64 = owsResponse1.responseData.ToString();
                    new LogHelper().Log("Fetched guest folio as base64 successfully", Reservation.ReservationNameID, "PushDueOutReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-Out push");
                }
                #endregion

                bool isAllowedforCheckout = true;
                if (guestFolio != null && guestFolio.FolioWindows != null && guestFolio.FolioWindows.Count > 0)
                {
                    
                    foreach(var folio in guestFolio.FolioWindows)
                    {
                        if (folio.BalanceAmount < 0)
                        {
                            isAllowedforCheckout = false;
                            break;
                        }
                    }
                }
                if(!isAllowedforCheckout)
                {
                    return new Models.Local.LocalResponseModel()
                    {
                        responseMessage = "Folio window is having negative balance, not allowed",
                        result = false
                    };
                }


                #region Push due out record
                new LogHelper().Log("Pushing due out reservation, reservation No. : " + Reservation.ReservationNumber, Reservation.ReservationNameID, "PushDueOutReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-Out push");

                Reservation.CurrentBalance = guestFolio  != null ? guestFolio.BalanceAmount : 0;
                Reservation.reservationDocument = new Models.OWS.ReservationDocument()
                {
                    DocumentBase64 = folioAsBase64,
                    DocumentType = "Folio"

                };
                Models.Cloud.CloudResponseModel cloudResponse = await new WSClientHelper().PushDueoutRecord(Reservation.ReservationNameID, new Models.Cloud.CloudRequestModel()
                {
                    RequestObject = new List<Models.OWS.OperaReservation>() { Reservation }
                }, "Due-Out push", pushReservationRequest.ServiceParameters);
                if (!cloudResponse.result)
                {
                    new LogHelper().Log("Failled to push the reservation to cloud, so skipping the reservation ", Reservation.ReservationNameID, "PushDueOutReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-Out push");
                    return new Models.Local.LocalResponseModel()
                    {
                        result = false,
                        responseMessage = "Failled to push the reservation to cloud"
                    };
                }
                new LogHelper().Log("Reservation pushed to cloud successfully", Reservation.ReservationNameID, "PushDueOutReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-Out push");

                    #endregion
                    if (!IsPaymentDisabled)
                    {
                        #region Pushing Payment Details

                        new LogHelper().Log("Pushing payment details to the cloud, reservation No. : " + Reservation.ReservationNumber, Reservation.ReservationNameID, "PushDueOutReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-Out push");
                        cloudResponse = await new WSClientHelper().PushPaymentDetails(Reservation.ReservationNameID, new Models.Cloud.CloudRequestModel()
                        {
                            RequestObject = paymentHeaders
                        }, "Due-Out push", pushReservationRequest.ServiceParameters);
                        if (!cloudResponse.result)
                        {
                            new LogHelper().Log("Failled to push the payment details to cloud, so skipping the reservation ", Reservation.ReservationNameID, "PushDueOutReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-Out push");
                            return new Models.Local.LocalResponseModel()
                            {
                                result = false,
                                responseMessage = "Failled to push the payment details to cloud, so skipping the reservation "
                            };
                        }
                        new LogHelper().Log("Payment details to cloud successfully", Reservation.ReservationNameID, "PushDueOutReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-Out push");
                        #endregion

                    }

                #region Sending Email
                try
                {

                    new LogHelper().Log("Verifying email send or not ", Reservation.ReservationNameID, "PushDueOutReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-Out push");
                    //if (!isReservationTrackPresent)
                    new LogHelper().Log("Sending pre-checkout email", Reservation.ReservationNameID, "PushDueOutReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-Out push");
                    if (Reservation != null && Reservation.GuestProfiles != null && Reservation.GuestProfiles[0].Email != null && Reservation.GuestProfiles[0].Email.Count > 0)
                    {
                        TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
                        foreach (Models.OWS.Email email in Reservation.GuestProfiles[0].Email)
                        {
                            if (email.primary != null && email.primary.Value)
                            {
                                Models.Email.EmailResponse emailResponse = await new WSClientHelper().SendEmail(Reservation.ReservationNameID, new Models.Email.EmailRequest()
                                {
                                    FromEmail = pushReservationRequest.ServiceParameters.PreCheckoutFromEmail,
                                    ToEmail = email.email,
                                    IsPrecheckinEmail = false,
                                    GuestName = !string.IsNullOrEmpty(Reservation.GuestProfiles[0].GuestName) ? textInfo.ToTitleCase(Reservation.GuestProfiles[0].GuestName)
                                        : Reservation.GuestProfiles[0].GuestName,//Reservation.GuestProfiles[0].GuestName,
                                    Subject = pushReservationRequest.ServiceParameters.PreCheckoutEmailSubject,
                                    confirmationNumber = "?id=" + HttpUtility.UrlEncode(new Helper().EncryptString("b14ca5898a4e4133bbce2ea2315a1916", Reservation.ReservationNumber)),
                                    displayFromEmail = pushReservationRequest.ServiceParameters.EmailDisplayName,
                                    EmailType = Models.Email.EmailType.PreCheckedout

                                }, "Due-Out push", pushReservationRequest.ServiceParameters);
                                if (!emailResponse.result)
                                {
                                    isEmailSend = false;
                                    new LogHelper().Log("Failled to send confirmation email with reason :- " + emailResponse.responseMessage, Reservation.ReservationNameID, "PushDueOutReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-Out push");
                                    new LogHelper().Warn("Failled to send confirmation email with reason :- " + emailResponse.responseMessage, Reservation.ReservationNameID, "PushDueOutReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-Out push");
                                }
                                else
                                {
                                    isEmailSend = true;
                                    new LogHelper().Log("Email send successfully", Reservation.ReservationNameID, "PushDueOutReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-Out push");
                                }
                            }
                        }
                    }
                    else
                    {
                        new LogHelper().Log("Failled to send pre-checkout since email address not found from pre checked in list response", Reservation.ReservationNameID, "PushDueOutReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-Out push");
                        new LogHelper().Warn("Failled to send pre-checkout since email address not found from pre checked in list response", Reservation.ReservationNameID, "PushDueOutReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-Out push");
                    }

                }
                catch (Exception ex)
                {
                    new LogHelper().Error(ex, Reservation.ReservationNameID, "PushDueOutReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-Out push");
                }
                #endregion

                #region Pushing Reservation Track
                if (isEmailSend)
                {
                    new LogHelper().Log("Pushing reservation track from local DB ", Reservation.ReservationNameID, "PushDueOutReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-Out push");

                    localResponse = await new WSClientHelper().PushReservationTrackLocally(Reservation.ReservationNameID, new Models.Local.LocalRequestModel()
                    {
                        RequestObject = new Models.Local.ReservationTrackStatus()
                        {
                            ReservationNameID = Reservation.ReservationNameID,
                            ProcessType = Models.Local.ReservationProcessType.Precheckoutemail.ToString(),
                            ReservationNumber = Reservation.ReservationNumber,
                            EmailSent = isEmailSend,
                            ProcessStatus = ""
                        }
                    }, "Due-Out push", pushReservationRequest.ServiceParameters);
                    if (localResponse.result)
                    {
                        new LogHelper().Log("Reservation track in local DB updated successfully ", Reservation.ReservationNameID, "PushDueOutReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-Out push");
                    }
                    else
                    {
                        new LogHelper().Log("Failled to update reservation track in local DB with reason :- " + localResponse.responseMessage, Reservation.ReservationNameID, "PushDueOutReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-Out push");
                    }
                }
                #endregion

                #region Pushing record copy in local DB
                new LogHelper().Log("Updating the reservation in Local DB with email send flag ", Reservation.ReservationNameID, "PushDueOutReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-Out push");

                Reservation.reservationDocument = null;
                localResponse = await new WSClientHelper().PushRecordLocally(new Models.Local.LocalRequestModel()
                {
                    SyncFromCloud = false,
                    RequestObject = new List<Models.OWS.OperaReservation>() { Reservation }
                }, Reservation.ReservationNameID, "Due-Out push", pushReservationRequest.ServiceParameters);
                if (!localResponse.result)
                {
                    new LogHelper().Log("Updating the reservation in Local DB with email send flag failled with reason :- " + localResponse.responseMessage, Reservation.ReservationNameID, "PushDueOutReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-Out push");
                }
                else
                    new LogHelper().Log("Updating the reservation in Local DB with email send flag fsucceeded", Reservation.ReservationNameID, "PushDueOutReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-Out push");
                #endregion

                new LogHelper().Log("Reservation :- " + Reservation.ReservationNumber + " processed", Reservation.ReservationNameID, "PushDueOutReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-Out push");
                
                return new Models.Local.LocalResponseModel()
                {
                    result = true,
                    responseMessage = "Success"
                };
            }
            catch (Exception ex)
            {
                return new Models.Local.LocalResponseModel()
                {
                    result = false,
                    responseMessage = ex.Message
                };
            }
        }

        public async Task<Models.Local.LocalResponseModel> PushDueOutReservationDetails(Models.Local.PushReservationRequest pushReservationRequest)
        {


            #region Variables

            List<Models.OWS.OperaReservation> operaReservationList = null;
            Models.OWS.OperaReservation Reservation = null;
            List<Models.Local.PaymentHeader> paymentHeaders = null;
            Models.OWS.FolioModel guestFolio = null;
            string folioAsBase64 = "";
            bool isEmailSend = false;
            #endregion

            try
            {
                new LogHelper().Debug("Pushing due out reservation details", null, "PushDueOutReservationDetails", pushReservationRequest.ServiceParameters.ClientID, "Due-Out push");


                isEmailSend = false;

                new LogHelper().Debug("Processing reservation No. : " + pushReservationRequest.ReservationNumber, null, "PushDueOutReservationDetails", pushReservationRequest.ServiceParameters.ClientID, "Due-Out push");

                #region Fetching Reservation from OWS
                new LogHelper().Debug("Fetching opera reservation", pushReservationRequest.ReservationNumber, "PushDueOutReservationDetails", pushReservationRequest.ServiceParameters.ClientID, "Due-Out push");
                Models.OWS.OwsResponseModel owsResponse1 = await new WSClientHelper().FetchReservationAsync(pushReservationRequest.ReservationNumber, new Models.OWS.OwsRequestModel()
                {
                    ChainCode = pushReservationRequest.ServiceParameters.ChainCode,
                    DestinationEntityID = pushReservationRequest.ServiceParameters.DestinationEntityID,
                    HotelDomain = pushReservationRequest.ServiceParameters.HotelDomain,
                    KioskID = pushReservationRequest.ServiceParameters.KioskID,
                    Language = pushReservationRequest.ServiceParameters.Language,
                    LegNumber = pushReservationRequest.ServiceParameters.Legnumber,
                    Password = pushReservationRequest.ServiceParameters.Password,
                    SystemType = pushReservationRequest.ServiceParameters.SystemType,
                    Username = pushReservationRequest.ServiceParameters.Username,
                    FetchBookingRequest = new Models.OWS.FetchBookingRequestModel()
                    {
                        ReservationNumber = pushReservationRequest.ReservationNumber
                    }
                }, "Due-Out push", pushReservationRequest.ServiceParameters);
                if (!owsResponse1.result)
                {
                    new LogHelper().Log("Failled to fetch opera reservation with reason :- " + owsResponse1.responseMessage, pushReservationRequest.ReservationNumber, "PushDueOutReservationDetails", pushReservationRequest.ServiceParameters.ClientID, "Due-Out push");
                    return new Models.Local.LocalResponseModel()
                    {
                        result = false,
                        responseMessage = "Failled to fetch opera reservation with reason :- " + owsResponse1.responseMessage
                    };
                }
                if (owsResponse1.responseData == null)
                {
                    new LogHelper().Log("Failled to fetch opera reservation with reason :- API response data is NULL" + owsResponse1.responseMessage, pushReservationRequest.ReservationNumber, "PushDueOutReservationDetails", pushReservationRequest.ServiceParameters.ClientID, "Due-Out push");
                    return new Models.Local.LocalResponseModel()
                    {
                        result = false,
                        responseMessage = "Failled to fetch opera reservation with reason :- API response data is NULL" + owsResponse1.responseMessage
                    };
                }
                else
                {
                    new LogHelper().Debug("Converting API json to object", pushReservationRequest.ReservationNumber, "PushDueOutReservationDetails", pushReservationRequest.ServiceParameters.ClientID, "Due-Out push");
                    try
                    {
                        List<Models.OWS.OperaReservation> temp_operaReservations = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Models.OWS.OperaReservation>>(owsResponse1.responseData.ToString());
                        Reservation = temp_operaReservations[0];
                        new LogHelper().Debug("Opera reservation fetched successfully", Reservation.ReservationNameID, "PushDueOutReservationDetails", pushReservationRequest.ServiceParameters.ClientID, "Due-Out push");
                    }
                    catch (Exception ex)
                    {
                        new LogHelper().Error(ex, Reservation.ReservationNameID, "PushDueOutReservationDetails", pushReservationRequest.ServiceParameters.ClientID, "Due-Out push");
                        //new LogHelper().Log("Failled to covert API response to object", Reservation.ReservationNameID, "PushDueOutReservationDetails", pushReservationRequest.ServiceParameters.ClientID, "Due-Out push");
                        //new LogHelper().Warn("Failled to fetch opera reservation with reason :- " + ex.Message, Reservation.ReservationNameID, "PushDueOutReservationDetails", pushReservationRequest.ServiceParameters.ClientID, "Due-Out push");
                        //new LogHelper().Debug("Failled to fetch opera reservation with reason :- " + ex.Message, Reservation.ReservationNameID, "PushDueOutReservationDetails", pushReservationRequest.ServiceParameters.ClientID, "Due-Out push");
                        return new Models.Local.LocalResponseModel()
                        {
                            result = false,
                            responseMessage = ex.Message
                        };
                    }
                }

                #endregion

                #region Verifying Day use guest
                if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["IsDayUseReservationCheckOutEnabled"]) && !bool.TryParse(ConfigurationManager.AppSettings["IsDayUseReservationCheckOutEnabled"].ToString(), out bool result))
                {
                    new LogHelper().Debug("verifying day use guest or not", Reservation.ReservationNameID, "PushDueOutReservationDetails", pushReservationRequest.ServiceParameters.ClientID, "Due-Out push");
                    if (Reservation != null)
                    {
                        if (Reservation.ArrivalDate != null && Reservation.DepartureDate != null && Reservation.ArrivalDate.Value.Equals(Reservation.DepartureDate.Value))
                        {
                            new LogHelper().Debug("reservation is a day use guest, so skipping the reservation", Reservation.ReservationNameID, "PushDueOutReservationDetails", pushReservationRequest.ServiceParameters.ClientID, "Due-Out push");
                            return new Models.Local.LocalResponseModel()
                            {
                                result = false,
                                responseMessage = "reservation is a day use guest, so skipping the reservation"
                            };
                        }
                        else
                            new LogHelper().Debug("reservation is not a day use guest", Reservation.ReservationNameID, "PushDueOutReservationDetails", pushReservationRequest.ServiceParameters.ClientID, "Due-Out push");
                    }
                    else
                        new LogHelper().Debug("verifying day use guest or not failled reservation object is blank", Reservation.ReservationNameID, "PushDueOutReservationDetails", pushReservationRequest.ServiceParameters.ClientID, "Due-Out push");
                }
                #endregion

                #region Validating email present or not
                bool isEmailPresent = false;
                if (Reservation != null && Reservation.GuestProfiles != null && Reservation.GuestProfiles.Count > 0 &&
                    Reservation.GuestProfiles[0].Email != null && Reservation.GuestProfiles[0].Email.Count > 0 &&
                    !string.IsNullOrEmpty(Reservation.GuestProfiles[0].Email[0].email))
                    isEmailPresent = true;
                if (!isEmailPresent)
                {
                    new LogHelper().Debug("Skipping reservation No. : " + Reservation.ReservationNumber + " no email ID present", Reservation.ReservationNameID, "PushDueOutReservationDetails", pushReservationRequest.ServiceParameters.ClientID, "Due-Out push");
                    return new Models.Local.LocalResponseModel()
                    {
                        result = false,
                        responseMessage = "Skipping reservation No. : " + Reservation.ReservationNumber + " no email ID present"
                    };
                }
                #endregion

                #region Verifying the reservation Status
                if (string.IsNullOrEmpty(Reservation.ReservationStatus) || !Reservation.ReservationStatus.Equals("DUEOUT"))
                {
                    new LogHelper().Debug("Reservation status : " + Reservation.ReservationStatus + " (not DUEOUT, so skipping the reservation)", Reservation.ReservationNameID, "PushDueOutReservationDetails", pushReservationRequest.ServiceParameters.ClientID, "Due-Out push");
                    return new Models.Local.LocalResponseModel()
                    {
                        result = false,
                        responseMessage = "Reservation status : " + Reservation.ReservationStatus + " (not DUEOUT, so skipping the reservation)"
                    };
                }
                #endregion

                #region Check Payment in Saavy
                //Models.Local.LocalResponseModel localResponse = null;
                new LogHelper().Debug("Fetching payment details for reservation No. : " + Reservation.ReservationNumber + " in Saavy Pay", Reservation.ReservationNameID, "PushDueOutReservationDetails", pushReservationRequest.ServiceParameters.ClientID, "Due-Out push");
                Models.Local.LocalResponseModel localResponse = await new WSClientHelper().FetchPaymentDetails(Reservation.ReservationNameID, new Models.Local.LocalRequestModel()
                {
                    RequestObject = new Models.Local.FetchPaymentRequest()
                    {
                        ReservationNameID = Reservation.ReservationNameID,
                        isActive = true
                    }
                }, "Due-Out push", pushReservationRequest.ServiceParameters);

                if (!localResponse.result || localResponse.responseData == null)
                {
                    new LogHelper().Debug("Failled to fetch payment details with reason :- " + localResponse.responseMessage, Reservation.ReservationNameID, "PushDueOutReservationDetails", pushReservationRequest.ServiceParameters.ClientID, "Due-Out push");
                   

                }
                else
                {
                    new LogHelper().Debug("Converting API json to object", Reservation.ReservationNameID, "PushDueOutReservationDetails", pushReservationRequest.ServiceParameters.ClientID, "Due-Out push");
                    try
                    {
                        paymentHeaders = JsonConvert.DeserializeObject<List<Models.Local.PaymentHeader>>(localResponse.responseData.ToString());
                        new LogHelper().Debug("Payment details fetched successfully", Reservation.ReservationNameID, "PushDueOutReservationDetails", pushReservationRequest.ServiceParameters.ClientID, "Due-Out push");
                    }
                    catch (Exception ex)
                    {
                        new LogHelper().Error(ex, Reservation.ReservationNameID, "PushDueOutReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-Out push");
                        new LogHelper().Log("Failled to covert API response to object", Reservation.ReservationNameID, "PushDueOutReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-Out push");
                        
                    }
                }

                #endregion

                #region FetchFolioItemsByWindow
                new LogHelper().Debug("Fetching reservation folio by window for reservation No. : " + Reservation.ReservationNumber, Reservation.ReservationNameID, "PushDueOutReservationDetails", pushReservationRequest.ServiceParameters.ClientID, "Due-Out push");
                owsResponse1 = await new WSClientHelper().GetFolioByWindow(Reservation.ReservationNameID, new Models.OWS.OwsRequestModel()
                {
                    ChainCode = pushReservationRequest.ServiceParameters.ChainCode,
                    DestinationEntityID = pushReservationRequest.ServiceParameters.DestinationEntityID,
                    HotelDomain = pushReservationRequest.ServiceParameters.HotelDomain,
                    KioskID = pushReservationRequest.ServiceParameters.KioskID,
                    Language = pushReservationRequest.ServiceParameters.Language,
                    LegNumber = pushReservationRequest.ServiceParameters.Legnumber,
                    Password = pushReservationRequest.ServiceParameters.Password,
                    SystemType = pushReservationRequest.ServiceParameters.SystemType,
                    Username = pushReservationRequest.ServiceParameters.Username,
                    FetchFolioRequest = new Models.OWS.FetchFolioRequest()
                    {
                        ReservationNameID = Reservation.ReservationNameID,
                        ProfileID = (Reservation.GuestProfiles != null && Reservation.GuestProfiles.Count > 0) ? Reservation.GuestProfiles[0].PmsProfileID : ""
                    }
                }, "Due-Out push", pushReservationRequest.ServiceParameters);

                if (!owsResponse1.result)
                {
                    new LogHelper().Log("Failled to fetch folio by window with reason :- " + owsResponse1.responseMessage, Reservation.ReservationNameID, "PushDueOutReservationDetails", pushReservationRequest.ServiceParameters.ClientID, "Due-Out push");
                   
                    return new Models.Local.LocalResponseModel()
                    {
                        result = false,
                        responseMessage = "Failled to fetch folio by window with reason :- " + owsResponse1.responseMessage
                    };
                }
                else
                {
                    new LogHelper().Debug("Converting API json to object", Reservation.ReservationNameID, "PushDueOutReservationDetails", pushReservationRequest.ServiceParameters.ClientID, "Due-Out push");
                    try
                    {

                        guestFolio = JsonConvert.DeserializeObject<Models.OWS.FolioModel>(owsResponse1.responseData.ToString());
                        if (guestFolio != null)
                        {
                            new LogHelper().Debug("Current guest balance of the reservation is : " + guestFolio.BalanceAmount, Reservation.ReservationNameID, "PushDueOutReservationDetails", pushReservationRequest.ServiceParameters.ClientID, "Due-Out push");
                            new LogHelper().Debug("Reservation folio by window fetched successfully", Reservation.ReservationNameID, "PushDueOutReservationDetails", pushReservationRequest.ServiceParameters.ClientID, "Due-Out push");
                        }
                        else
                        {
                            new LogHelper().Log("No folio items found in the reservation", Reservation.ReservationNameID, "PushDueOutReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-Out push");
                        }
                    }
                    catch (Exception ex)
                    {
                        new LogHelper().Error(ex, Reservation.ReservationNameID, "PushDueOutReservationDetails", pushReservationRequest.ServiceParameters.ClientID, "Due-Out push");
                        new LogHelper().Log("Failled to covert API response to object", Reservation.ReservationNameID, "PushDueOutReservationDetails", pushReservationRequest.ServiceParameters.ClientID, "Due-Out push");
                       
                    }
                }
                #endregion

                

                #region FetchFolioAsBase64
                new LogHelper().Debug("Fetching reservation folio as base64 for reservation No. : " + Reservation.ReservationNumber, Reservation.ReservationNameID, "PushDueOutReservationDetails", pushReservationRequest.ServiceParameters.ClientID, "Due-Out push");
                owsResponse1 = await new WSClientHelper().GetFolio(Reservation.ReservationNameID, new Models.OWS.OwsRequestModel()
                {
                    ChainCode = pushReservationRequest.ServiceParameters.ChainCode,
                    DestinationEntityID = pushReservationRequest.ServiceParameters.DestinationEntityID,
                    HotelDomain = pushReservationRequest.ServiceParameters.HotelDomain,
                    KioskID = pushReservationRequest.ServiceParameters.KioskID,
                    Language = pushReservationRequest.ServiceParameters.Language,
                    LegNumber = pushReservationRequest.ServiceParameters.Legnumber,
                    Password = pushReservationRequest.ServiceParameters.Password,
                    SystemType = pushReservationRequest.ServiceParameters.SystemType,
                    Username = pushReservationRequest.ServiceParameters.Username,
                    FetchFolioRequest = new Models.OWS.FetchFolioRequest()
                    {
                        ReservationNameID = Reservation.ReservationNameID,
                        OperaReservation = Reservation,
                        ProfileID = (Reservation.GuestProfiles != null && Reservation.GuestProfiles.Count > 0) ? Reservation.GuestProfiles[0].PmsProfileID : "",
                        FolioList = guestFolio
                    }
                }, "Due-Out push", pushReservationRequest.ServiceParameters);

                if (!owsResponse1.result || owsResponse1.responseData == null)
                {
                    new LogHelper().Log("Failled to fetch folio with reason :- " + owsResponse1.responseMessage, Reservation.ReservationNameID, "PushDueOutReservationDetails", pushReservationRequest.ServiceParameters.ClientID, "Due-Out push");
                    
                    return new Models.Local.LocalResponseModel()
                    {
                        result = false,
                        responseMessage = "Failled to fetch folio with reason :- " + owsResponse1.responseMessage
                    };
                }
                else
                {
                    folioAsBase64 = owsResponse1.responseData.ToString();
                    new LogHelper().Debug("Fetched guest folio as base64 successfully", Reservation.ReservationNameID, "PushDueOutReservationDetails", pushReservationRequest.ServiceParameters.ClientID, "Due-Out push");
                }
                #endregion



                #region Push due out record
                new LogHelper().Debug("Pushing due out reservation, reservation No. : " + Reservation.ReservationNumber, Reservation.ReservationNameID, "PushDueOutReservationDetails", pushReservationRequest.ServiceParameters.ClientID, "Due-Out push");

                Reservation.CurrentBalance = guestFolio != null ? guestFolio.BalanceAmount : 0;
                Reservation.reservationDocument = new Models.OWS.ReservationDocument()
                {
                    DocumentBase64 = folioAsBase64,
                    DocumentType = "Folio"

                };
                Models.Cloud.CloudResponseModel cloudResponse = await new WSClientHelper().PushDueoutRecord(Reservation.ReservationNameID, new Models.Cloud.CloudRequestModel()
                {
                    RequestObject = new List<Models.OWS.OperaReservation>() { Reservation }
                }, "Due-Out push", pushReservationRequest.ServiceParameters);
                if (!cloudResponse.result)
                {
                    new LogHelper().Log("Failled to push the reservation to cloud, so skipping the reservation ", Reservation.ReservationNameID, "PushDueOutReservationDetails", pushReservationRequest.ServiceParameters.ClientID, "Due-Out push");
                    return new Models.Local.LocalResponseModel()
                    {
                        result = false,
                        responseMessage = "Failled to push the reservation to cloud"
                    };
                }
                new LogHelper().Debug("Reservation pushed to cloud successfully", Reservation.ReservationNameID, "PushDueOutReservationDetails", pushReservationRequest.ServiceParameters.ClientID, "Due-Out push");

                #endregion

                #region Pushing Payment Details
                new LogHelper().Debug("Pushing payment details to the cloud, reservation No. : " + Reservation.ReservationNumber, Reservation.ReservationNameID, "PushDueOutReservationDetails", pushReservationRequest.ServiceParameters.ClientID, "Due-Out push");
                cloudResponse = await new WSClientHelper().PushPaymentDetails(Reservation.ReservationNameID, new Models.Cloud.CloudRequestModel()
                {
                    RequestObject = paymentHeaders
                }, "Due-Out push", pushReservationRequest.ServiceParameters);
                if (!cloudResponse.result)
                {
                    new LogHelper().Log("Failled to push the payment details to cloud, so skipping the reservation ", Reservation.ReservationNameID, "PushDueOutReservationDetails", pushReservationRequest.ServiceParameters.ClientID, "Due-Out push");
                    return new Models.Local.LocalResponseModel()
                    {
                        result = false,
                        responseMessage = "Failled to push the payment details to cloud, so skipping the reservation "
                    };
                }
                new LogHelper().Debug("Payment details to cloud successfully", Reservation.ReservationNameID, "PushDueOutReservationDetails", pushReservationRequest.ServiceParameters.ClientID, "Due-Out push");
                #endregion

                

                #region Pushing record copy in local DB
                new LogHelper().Debug("Updating the reservation in Local DB with email send flag ", Reservation.ReservationNameID, "PushDueOutReservationDetails", pushReservationRequest.ServiceParameters.ClientID, "Due-Out push");

                Reservation.reservationDocument = null;
                localResponse = await new WSClientHelper().PushRecordLocally(new Models.Local.LocalRequestModel()
                {
                    SyncFromCloud = false,
                    RequestObject = new List<Models.OWS.OperaReservation>() { Reservation }
                }, Reservation.ReservationNameID, "Due-Out push", pushReservationRequest.ServiceParameters);
                if (!localResponse.result)
                {
                    new LogHelper().Log("Updating the reservation in Local DB with email send flag failled with reason :- " + localResponse.responseMessage, Reservation.ReservationNameID, "PushDueOutReservationDetails", pushReservationRequest.ServiceParameters.ClientID, "Due-Out push");
                }
                else
                    new LogHelper().Debug("Updating the reservation in Local DB with email send flag fsucceeded", Reservation.ReservationNameID, "PushDueOutReservationDetails", pushReservationRequest.ServiceParameters.ClientID, "Due-Out push");
                #endregion

                new LogHelper().Debug("Reservation :- " + Reservation.ReservationNumber + " processed", Reservation.ReservationNameID, "PushDueOutReservationDetails", pushReservationRequest.ServiceParameters.ClientID, "Due-Out push");

                return new Models.Local.LocalResponseModel()
                {
                    result = true,
                    responseMessage = "Success"
                };
            }
            catch (Exception ex)
            {
                return new Models.Local.LocalResponseModel()
                {
                    result = false,
                    responseMessage = ex.Message
                };
            }
        }
        public async Task<Models.Local.LocalResponseModel> PushReservationLocally(Models.Local.PushReservationRequest pushReservationRequest)
        {

            #region Variables
            List<Models.OWS.OperaReservation> operaReservationList = null;
            Models.OWS.OperaReservation Reservation = null;
            #endregion

            try
            {
                new LogHelper().Log("Pushing due in reservation list", null, "PushReservationLocally", pushReservationRequest.ServiceParameters.ClientID, "Local Manual");


                bool? isEmailSent = null;

                List<Models.OWS.OperaReservation> temp_operaReservations = null;

                #region Fetching Reservation from OWS
                //new LogHelper().Log("Fetching opera reservation", null, "PushReservationLocally", pushReservationRequest.ServiceParameters.ClientID, "Local Manual");
                Models.OWS.OwsResponseModel owsResponse1 = await new WSClientHelper().FetchReservationAsync(pushReservationRequest.ReservationNumber, new Models.OWS.OwsRequestModel()
                {
                    ChainCode = pushReservationRequest.ServiceParameters.ChainCode,
                    DestinationEntityID = pushReservationRequest.ServiceParameters.DestinationEntityID,
                    HotelDomain = pushReservationRequest.ServiceParameters.HotelDomain,
                    KioskID = pushReservationRequest.ServiceParameters.KioskID,
                    Language = pushReservationRequest.ServiceParameters.Language,
                    LegNumber = pushReservationRequest.ServiceParameters.Legnumber,
                    Password = pushReservationRequest.ServiceParameters.Password,
                    SystemType = pushReservationRequest.ServiceParameters.SystemType,
                    Username = pushReservationRequest.ServiceParameters.Username,
                    FetchBookingRequest = new Models.OWS.FetchBookingRequestModel()
                    {
                        ReservationNumber = pushReservationRequest.ReservationNumber
                        //ReservationNameID = pushReservationRequest.ReservationNameID
                    }
                }, "Local Manual", pushReservationRequest.ServiceParameters);
                if (!owsResponse1.result)
                {
                    new LogHelper().Log("Fetching opera reservation", null, "PushReservationLocally", pushReservationRequest.ServiceParameters.ClientID, "Local Manual");
                    new LogHelper().Warn("Failled to fetch opera reservation with reason :- " + owsResponse1.responseMessage, pushReservationRequest.ReservationNumber, "PushReservationLocally", pushReservationRequest.ServiceParameters.ClientID, "Local Manual");
                    return new Models.Local.LocalResponseModel()
                    {
                        result = false,
                        responseMessage = "Failled to fetch opera reservation with reason :- " + owsResponse1.responseMessage
                    };
                }
                if (owsResponse1.responseData == null)
                {
                    new LogHelper().Log("Failled to fetch opera reservation with reason :- API response data is NULL" + owsResponse1.responseMessage, pushReservationRequest.ReservationNumber, "PushReservationLocally", pushReservationRequest.ServiceParameters.ClientID, "Local Manual");
                    return new Models.Local.LocalResponseModel()
                    {
                        result = false,
                        responseMessage = "Failled to fetch opera reservation with reason :- API response data is NULL" + owsResponse1.responseMessage
                    };
                }
                else
                {
                    new LogHelper().Debug("Converting API json to object", pushReservationRequest.ReservationNumber, "PushReservationLocally", pushReservationRequest.ServiceParameters.ClientID, "Local Manual");
                    try
                    {
                        temp_operaReservations = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Models.OWS.OperaReservation>>(owsResponse1.responseData.ToString());
                        Reservation = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Models.OWS.OperaReservation>>(owsResponse1.responseData.ToString())[0];
                        new LogHelper().Log("Opera reservation fetched successfully", Reservation.ReservationNameID, "PushReservationLocally", pushReservationRequest.ServiceParameters.ClientID, "Local Manual");
                    }
                    catch (Exception ex)
                    {
                        new LogHelper().Error(ex, pushReservationRequest.ReservationNumber, "PushReservationLocally", pushReservationRequest.ServiceParameters.ClientID, "Local Manual");
                        new LogHelper().Log("Failled to covert API response to object", pushReservationRequest.ReservationNumber, "PushReservationLocally", pushReservationRequest.ServiceParameters.ClientID, "Local Manual");
                        new LogHelper().Warn("Failled to fetch opera reservation with reason :- " + ex.Message, pushReservationRequest.ReservationNumber, "PushReservationLocally", pushReservationRequest.ServiceParameters.ClientID, "Local Manual");
                        new LogHelper().Debug("Failled to fetch opera reservation with reason :- " + ex.Message, pushReservationRequest.ReservationNumber, "PushReservationLocally", pushReservationRequest.ServiceParameters.ClientID, "Due-IN push");
                        return new Models.Local.LocalResponseModel()
                        {
                            result = false,
                            responseMessage = ex.Message
                        };
                    }
                }

                #endregion
                #region Processing SHarers to Local DB

                try
                {
                    List<Models.OWS.OperaReservation> tempList = null;
                    if (temp_operaReservations[0].SharerReservations != null && temp_operaReservations[0].SharerReservations.Count > 0)
                    {
                        //MessageBox.Show("Sharer present");
                        string shareID = temp_operaReservations[0].ReservationNumber;
                        temp_operaReservations[0].ReservationNumber += "||" + temp_operaReservations[0].ReservationNumber;

                        foreach (Models.OWS.OperaReservation sharerReservation in temp_operaReservations[0].SharerReservations)
                        {
                            owsResponse1 = await new WSClientHelper().FetchReservationAsync(pushReservationRequest.ReservationNumber, new Models.OWS.OwsRequestModel()
                            {
                                ChainCode = pushReservationRequest.ServiceParameters.ChainCode,
                                DestinationEntityID = pushReservationRequest.ServiceParameters.DestinationEntityID,
                                HotelDomain = pushReservationRequest.ServiceParameters.HotelDomain,
                                KioskID = pushReservationRequest.ServiceParameters.KioskID,
                                Language = pushReservationRequest.ServiceParameters.Language,
                                LegNumber = pushReservationRequest.ServiceParameters.Legnumber,
                                Password = pushReservationRequest.ServiceParameters.Password,
                                SystemType = pushReservationRequest.ServiceParameters.SystemType,
                                Username = pushReservationRequest.ServiceParameters.Username,
                                FetchBookingRequest = new Models.OWS.FetchBookingRequestModel()
                                {
                                    ReservationNumber = pushReservationRequest.ReservationNumber
                                }
                            }, "Local Manual", pushReservationRequest.ServiceParameters);

                            if (owsResponse1.result && owsResponse1.responseData != null)
                            {
                                tempList = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Models.OWS.OperaReservation>>(owsResponse1.responseData.ToString());
                                tempList[0].ReservationNumber += "||" + shareID;
                                temp_operaReservations.AddRange(tempList);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    new LogHelper().Error(ex, pushReservationRequest.ReservationNumber, "PushReservationLocally", pushReservationRequest.ServiceParameters.ClientID, "Local Manual");
                }
                #endregion
                #region Pushing record copy in local DB
                new LogHelper().Log("Updating the reservation in Local DB", Reservation.ReservationNameID, "PushReservationLocally", pushReservationRequest.ServiceParameters.ClientID, "Local Manual");

                Models.Local.LocalResponseModel localResponse = await new WSClientHelper().PushRecordLocally(new Models.Local.LocalRequestModel()
                {
                    SyncFromCloud = false,
                    RequestObject = temp_operaReservations,
                }, Reservation.ReservationNameID, "Local Manual", pushReservationRequest.ServiceParameters);
                if (!localResponse.result)
                {
                    new LogHelper().Log("Updating the reservation in Local DB with email send flag failled with reason :- " + localResponse.responseMessage, Reservation.ReservationNameID, "PushReservationLocally", pushReservationRequest.ServiceParameters.ClientID, "Local Manual");
                }
                else
                    new LogHelper().Log("Updating the reservation in Local DB with email send flag fsucceeded", Reservation.ReservationNameID, "PushReservationLocally", pushReservationRequest.ServiceParameters.ClientID, "Local Manual");
                #endregion
               

                #region Validating Reservation
                new LogHelper().Log("Validating reservation", Reservation.ReservationNameID, "PushReservationLocally", pushReservationRequest.ServiceParameters.ClientID, "Local Manual");

                if (Reservation == null)
                {
                    new LogHelper().Log("Reservation returned as NULL", Reservation.ReservationNameID, "PushReservationLocally", pushReservationRequest.ServiceParameters.ClientID, "Local Manual");
                    //return new Models.Local.LocalResponseModel()
                    //{
                    //    result = false,
                    //    responseMessage = "Reservation returned as NULL"
                    //};
                }
                else if (Reservation.Adults == null && Reservation.Adults == 0)
                {
                    new LogHelper().Log("Reservation adult count is NULL or 0", Reservation.ReservationNameID, "PushReservationLocally", pushReservationRequest.ServiceParameters.ClientID, "Local Manual");
                    //return new Models.Local.LocalResponseModel()
                    //{
                    //    result = false,
                    //    responseMessage = "Reservation adult count is NULL or 0"
                    //};
                }
                else if (string.IsNullOrEmpty(Reservation.ReservationStatus) && (!Reservation.ReservationStatus.ToUpper().Equals("DUEIN") || !Reservation.ReservationStatus.ToUpper().Equals("RESERVED")))
                {
                    new LogHelper().Debug("Reservation status is : " + Reservation.ReservationStatus + " not elogible for pre-checkin", Reservation.ReservationNameID, "PushReservationLocally", pushReservationRequest.ServiceParameters.ClientID, "Local Manual");
                    //return new Models.Local.LocalResponseModel()
                    //{
                    //    result = false,
                    //    responseMessage = "Reservation adult count is NULL or 0"
                    //};
                }
                new LogHelper().Log("Reservation Validated ", Reservation.ReservationNameID, "PushReservationLocally", pushReservationRequest.ServiceParameters.ClientID, "Local Manual");
                #endregion
                #region Processing sharer Profiles
                new LogHelper().Log("Processing sharer in the reservation", Reservation.ReservationNameID, "PushReservationLocally", pushReservationRequest.ServiceParameters.ClientID, "Local Manual");
                if (Reservation.SharerReservations != null && Reservation.SharerReservations.Count > 0)
                {
                    foreach (Models.OWS.OperaReservation sharer in Reservation.SharerReservations)
                    {
                        if (sharer.GuestProfiles != null && sharer.GuestProfiles.Count > 0)
                        {
                            foreach (Models.OWS.GuestProfile guestProfile in sharer.GuestProfiles)
                            {
                                Reservation.GuestProfiles.Add(guestProfile);
                            }
                        }
                    }
                    new LogHelper().Log("Processing sharer in the reservation completed", Reservation.ReservationNameID, "PushReservationLocally", pushReservationRequest.ServiceParameters.ClientID, "Local Manual");
                }
                else
                    new LogHelper().Log("No sharers found", Reservation.ReservationNameID, "PushReservationLocally", pushReservationRequest.ServiceParameters.ClientID, "Local Manual");
                #endregion
                #region Processing RoomRate
                new LogHelper().Log("Updating rate details", Reservation.ReservationNameID, "PushReservationLocally", pushReservationRequest.ServiceParameters.ClientID, "Local Manual");
                if (Reservation.RateDetails != null && Reservation.RateDetails.DailyRates != null && Reservation.RateDetails.DailyRates.Count > 0)
                {
                    decimal total_roomrate = Reservation.RateDetails.DailyRates.Sum(x => x.Amount);
                    if (Reservation.PrintRate != null && Reservation.PrintRate.Value)
                        Reservation.RateDetails.RateAmount = total_roomrate;
                    else
                        Reservation.TotalAmount = 0;
                    new LogHelper().Log("Updating rate details completed", Reservation.ReservationNameID, "PushReservationLocally", pushReservationRequest.ServiceParameters.ClientID, "Local Manual");
                }
                else
                    new LogHelper().Log("Updating rate details failed because Rate details are blank in the reservation", Reservation.ReservationNameID, "PushReservationLocally", pushReservationRequest.ServiceParameters.ClientID, "Local Manual");
                #endregion
                #region Processing MealPlan
                if (pushReservationRequest.ServiceParameters.IsBreakFastValidationWithUDF != null && pushReservationRequest.ServiceParameters.IsBreakFastValidationWithUDF.Value)
                {
                    new LogHelper().Debug("Processing meal plan from UDF fields", Reservation.ReservationNameID, "PushReservationLocally", "Grabber", "Local Manual");
                    if (Reservation.userDefinedFields != null && Reservation.userDefinedFields.Count > 0)
                    {
                        if (Reservation.userDefinedFields.Find(x => x.FieldName.Equals(pushReservationRequest.ServiceParameters.MealPlanFieldName)) != null)
                        {

                            if (!Reservation.userDefinedFields.Find(x => x.FieldName.Equals(pushReservationRequest.ServiceParameters.MealPlanFieldName)).FieldValue.Equals("NP"))
                            {
                                string tempUDFValue = Reservation.userDefinedFields.Find(x => x.FieldName.Equals(pushReservationRequest.ServiceParameters.MealPlanFieldName)).FieldValue;
                                if (!string.IsNullOrEmpty(tempUDFValue))
                                {
                                    bool isPackageFound = false;
                                    if (pushReservationRequest.ServiceParameters.PackageCodes.Split(';').ToList().Contains(tempUDFValue))
                                        isPackageFound = true;
                                    if (isPackageFound)
                                    {
                                        Reservation.IsBreakFastAvailable = true;
                                        new LogHelper().Debug("Meal plan updated", Reservation.ReservationNameID, "PushReservationLocally", "Grabber", "Local Manual");
                                    }
                                }
                            }
                            else
                                new LogHelper().Log("Processing meal plan not updated (NP not present in UDF)", Reservation.ReservationNameID, "PushReservationLocally", "Grabber", "Local Manual");
                        }
                    }
                    else
                        new LogHelper().Log("No UDF fields for meal plan not found", Reservation.ReservationNameID, "PushReservationLocally", "Grabber", "Local Manual");
                }
                if (pushReservationRequest.ServiceParameters.IsBreakFastValidationWithPackage != null && pushReservationRequest.ServiceParameters.IsBreakFastValidationWithPackage.Value)
                {
                    new LogHelper().Debug("Processing package list in the reservation for meal plan", Reservation.ReservationNameID, "PushReservationLocally", "Grabber", "Local Manual");
                    if (((Reservation.PackageDetails != null && Reservation.PackageDetails.Count > 0) || (Reservation.PreferanceDetails != null && Reservation.PreferanceDetails.Count > 0)) && (!string.IsNullOrEmpty(pushReservationRequest.ServiceParameters.PackageCodes) && pushReservationRequest.ServiceParameters.PackageCodes.Split(';').ToList() != null))
                    {
                        if (Reservation.PackageDetails != null && Reservation.PackageDetails.Count > 0)
                        {
                            bool isPackageFound = false;
                            foreach (Models.OWS.PackageDetails package in Reservation.PackageDetails)
                            {
                                if (pushReservationRequest.ServiceParameters.PackageCodes.Split(';').ToList().Contains(package.PackageCode))
                                {
                                    isPackageFound = true;
                                    break;
                                }
                            }
                            if (isPackageFound)
                            {
                                Reservation.IsBreakFastAvailable = true;
                                new LogHelper().Debug("Meal plan updated", Reservation.ReservationNameID, "PushReservationLocally", "Grabber", "Local Manual");
                            }
                        }
                        if (Reservation.PreferanceDetails != null && Reservation.PreferanceDetails.Count > 0)
                        {
                            if (Reservation.IsBreakFastAvailable == null || !Reservation.IsBreakFastAvailable.Value)
                            {
                                bool isPrefernceFound = false;
                                foreach (Models.OWS.PreferanceDetails prefernce in Reservation.PreferanceDetails)
                                {
                                    if (pushReservationRequest.ServiceParameters.PackageCodes.Split(';').ToList().Contains(prefernce.PreferanceCode))
                                    {
                                        isPrefernceFound = true;
                                        break;
                                    }
                                }
                                if (isPrefernceFound)
                                {
                                    Reservation.IsBreakFastAvailable = true;
                                    new LogHelper().Debug("Meal plan updated", Reservation.ReservationNameID, "PushReservationLocally", "Grabber", "Local Manual");
                                }
                            }
                        }
                    }
                    else
                        new LogHelper().Log("No package or prefernce list in the reservation for meal plan not found", Reservation.ReservationNameID, "PushReservationLocally", "Grabber", "Local Manual");
                }
                #endregion
                #region VerifyVIPReservationOrNot
                new LogHelper().Log("Verifying reservation VIP status in UDF field", Reservation.ReservationNameID, "PushReservationLocally", pushReservationRequest.ServiceParameters.ClientID, "Local Manual");
                if (Reservation.userDefinedFields != null && Reservation.userDefinedFields.Count > 0)
                {
                    if (Reservation.userDefinedFields.Find(x => x.FieldName.Equals(pushReservationRequest.ServiceParameters.PreAuthUDF)) != null &&
                        Reservation.userDefinedFields.Find(x => x.FieldName.Equals(pushReservationRequest.ServiceParameters.PreAuthUDF)).FieldValue.Equals("NO"))
                    {
                        new LogHelper().Log("Reservation is flagged not to take payment", Reservation.ReservationNameID, "PushReservationLocally", pushReservationRequest.ServiceParameters.ClientID, "Local Manual");
                        Reservation.DepositDetail = new List<Models.OWS.DepositDetail>()
                {
                    new Models.OWS.DepositDetail()
                    {
                        Amount = 0,
                        CardExpiryDate = null,
                        CreditCardNumber = null,
                        IsCreditCardDeposit = false,
                        PaymentType = null
                    }
                };
                    }
                    else if (Reservation.userDefinedFields.Find(x => x.FieldName.Equals(pushReservationRequest.ServiceParameters.PreAuthAmntUDF)) != null &&
                        Reservation.userDefinedFields.Find(x => x.FieldName.Equals(pushReservationRequest.ServiceParameters.PreAuthAmntUDF)).FieldValue.Equals("NO"))
                    {
                        new LogHelper().Log("Reservation is flagged not to take payment", Reservation.ReservationNameID, "PushReservationLocally", pushReservationRequest.ServiceParameters.ClientID, "Local Manual");
                        Reservation.DepositDetail = new List<Models.OWS.DepositDetail>()
                        {
                            new Models.OWS.DepositDetail()
                            {
                                Amount = 0,
                                CardExpiryDate = null,
                                CreditCardNumber = null,
                                IsCreditCardDeposit = false,
                                PaymentType = null
                            }
                        };
                    }
                }
                new LogHelper().Log("Verifying reservation VIP status in UDF field completed", Reservation.ReservationNameID, "PushReservationLocally", pushReservationRequest.ServiceParameters.ClientID, "Local Manual");
                #endregion

                #region PaymentDesabling
                if (pushReservationRequest.ServiceParameters.IsPaymentDisabled)
                {
                    new LogHelper().Log("Disabling the payment as per the config", Reservation.ReservationNameID, "PushReservationLocally", pushReservationRequest.ServiceParameters.ClientID, "Local Manual");
                    Reservation.DepositDetail = new List<Models.OWS.DepositDetail>()
                {
                    new Models.OWS.DepositDetail()
                    {
                        Amount = 0,
                        CardExpiryDate = null,
                        CreditCardNumber = null,
                        IsCreditCardDeposit = false,
                        PaymentType = null
                    }
                };
                }
                #endregion

                #region Update ETA
                if (pushReservationRequest.ServiceParameters.IsETADefault)
                {
                    new LogHelper().Log("Assigning NULL value to ETA as per the config", Reservation.ReservationNameID, "PushReservationLocally", pushReservationRequest.ServiceParameters.ClientID, "Local Manual");
                    Reservation.ExpectedArrivalTime = null;
                }
                #endregion              

                #region Pushing record copy in local DB
                new LogHelper().Log("Updating the reservation in Local DB with email send flag ", Reservation.ReservationNameID, "PushReservationLocally", pushReservationRequest.ServiceParameters.ClientID, "Local Manual");
                Reservation.IsEmailSend = isEmailSent;
                Reservation.reservationDocument = null;
                localResponse = await new WSClientHelper().PushRecordLocally(new Models.Local.LocalRequestModel()
                {
                    SyncFromCloud = false,
                    RequestObject = new List<Models.OWS.OperaReservation>() { Reservation }
                }, Reservation.ReservationNameID, "Local Manual", pushReservationRequest.ServiceParameters);
                if (!localResponse.result)
                {
                    new LogHelper().Log("Updating the reservation in Local DB with email send flag failled with reason :- " + localResponse.responseMessage, Reservation.ReservationNameID, "PushReservationLocally", pushReservationRequest.ServiceParameters.ClientID, "Local Manual");
                }
                else
                    new LogHelper().Log("Updating the reservation in Local DB with email send flag fsucceeded", Reservation.ReservationNameID, "PushReservationLocally", pushReservationRequest.ServiceParameters.ClientID, "Local Manual");
                #endregion



                return new Models.Local.LocalResponseModel()
                {
                    result = true,
                    responseMessage = "Success"
                };
            }
            catch (Exception ex)
            {
                return new Models.Local.LocalResponseModel()
                {
                    result = false,
                    responseMessage = ex.Message
                };
            }

        }
        #region Cloud Merging API
        public async Task<Models.Local.LocalResponseModel> PushCloudDueInReservation(Models.Local.PushReservationRequest pushReservationRequest)
        {

            #region Variables
            List<Models.OWS.OperaReservation> operaReservationList = null;
            Models.OWS.OperaReservation Reservation = null;
            #endregion

            try
            {
                new LogHelper().Log("Pushing due in reservation list", null, "PushCloudDueInReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-In push");


                bool? isEmailSent = null;
                bool? IsEmailProcessed = null;
              
                List<Models.OWS.OperaReservation> temp_operaReservations = null;

                #region Fetching Reservation from OWS
                   Models.OWS.OwsResponseModel owsResponse1 = await new WSClientHelper().FetchReservationAsync(pushReservationRequest.ReservationNumber, new Models.OWS.OwsRequestModel()
                {
                    ChainCode = pushReservationRequest.ServiceParameters.ChainCode,
                    DestinationEntityID = pushReservationRequest.ServiceParameters.DestinationEntityID,
                    HotelDomain = pushReservationRequest.ServiceParameters.HotelDomain,
                    KioskID = pushReservationRequest.ServiceParameters.KioskID,
                    Language = pushReservationRequest.ServiceParameters.Language,
                    LegNumber = pushReservationRequest.ServiceParameters.Legnumber,
                    Password = pushReservationRequest.ServiceParameters.Password,
                    SystemType = pushReservationRequest.ServiceParameters.SystemType,
                    Username = pushReservationRequest.ServiceParameters.Username,
                    FetchBookingRequest = new Models.OWS.FetchBookingRequestModel()
                    {
                        ReservationNumber = pushReservationRequest.ReservationNumber
                        //ReservationNameID = pushReservationRequest.ReservationNameID
                    }
                }, "Due-In push", pushReservationRequest.ServiceParameters);
                if (!owsResponse1.result)
                {
                    new LogHelper().Log("Fetching opera reservation", null, "PushCloudDueInReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-In push");
                    new LogHelper().Warn("Failled to fetch opera reservation with reason :- " + owsResponse1.responseMessage, pushReservationRequest.ReservationNumber, "PushCloudDueInReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-In push");
                    return new Models.Local.LocalResponseModel()
                    {
                        result = false,
                        responseMessage = "Failled to fetch opera reservation with reason :- " + owsResponse1.responseMessage
                    };
                }
                if (owsResponse1.responseData == null)
                {
                    new LogHelper().Log("Failled to fetch opera reservation with reason :- API response data is NULL" + owsResponse1.responseMessage, pushReservationRequest.ReservationNumber, "PushCloudDueInReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-In push");
                    return new Models.Local.LocalResponseModel()
                    {
                        result = false,
                        responseMessage = "Failled to fetch opera reservation with reason :- API response data is NULL" + owsResponse1.responseMessage
                    };
                }
                else
                {
                    new LogHelper().Debug("Converting API json to object", pushReservationRequest.ReservationNumber, "PushCloudDueInReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-In push");
                    try
                    {
                        temp_operaReservations = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Models.OWS.OperaReservation>>(owsResponse1.responseData.ToString());
                        Reservation = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Models.OWS.OperaReservation>>(owsResponse1.responseData.ToString())[0];
                        new LogHelper().Log("Opera reservation fetched successfully", Reservation.ReservationNameID, "PushCloudDueInReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-In push");
                    }
                    catch (Exception ex)
                    {
                        new LogHelper().Error(ex, pushReservationRequest.ReservationNumber, "PushCloudDueInReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-In push");
                        new LogHelper().Log("Failled to covert API response to object", pushReservationRequest.ReservationNumber, "PushCloudDueInReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-In push");
                        new LogHelper().Warn("Failled to fetch opera reservation with reason :- " + ex.Message, pushReservationRequest.ReservationNumber, "PushCloudDueInReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-In push");
                        new LogHelper().Debug("Failled to fetch opera reservation with reason :- " + ex.Message, pushReservationRequest.ReservationNumber, "PushCloudDueInReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-IN push");
                        return new Models.Local.LocalResponseModel()
                        {
                            result = false,
                            responseMessage = ex.Message
                        };
                    }
                }

                #endregion

                #region Processing SHarers to Local DB

                try
                {
                    List<Models.OWS.OperaReservation> tempList = null;
                    if (temp_operaReservations[0].SharerReservations != null && temp_operaReservations[0].SharerReservations.Count > 0)
                    {
                        //MessageBox.Show("Sharer present");
                        string shareID = temp_operaReservations[0].ReservationNumber;
                        temp_operaReservations[0].ReservationNumber += "||" + temp_operaReservations[0].ReservationNumber;

                        foreach (Models.OWS.OperaReservation sharerReservation in temp_operaReservations[0].SharerReservations)
                        {
                            owsResponse1 = await new WSClientHelper().FetchReservationAsync(pushReservationRequest.ReservationNumber, new Models.OWS.OwsRequestModel()
                            {
                                ChainCode = pushReservationRequest.ServiceParameters.ChainCode,
                                DestinationEntityID = pushReservationRequest.ServiceParameters.DestinationEntityID,
                                HotelDomain = pushReservationRequest.ServiceParameters.HotelDomain,
                                KioskID = pushReservationRequest.ServiceParameters.KioskID,
                                Language = pushReservationRequest.ServiceParameters.Language,
                                LegNumber = pushReservationRequest.ServiceParameters.Legnumber,
                                Password = pushReservationRequest.ServiceParameters.Password,
                                SystemType = pushReservationRequest.ServiceParameters.SystemType,
                                Username = pushReservationRequest.ServiceParameters.Username,
                                FetchBookingRequest = new Models.OWS.FetchBookingRequestModel()
                                {
                                    ReservationNumber = pushReservationRequest.ReservationNumber
                                }
                            }, "Due-In push", pushReservationRequest.ServiceParameters);

                            if (owsResponse1.result && owsResponse1.responseData != null)
                            {
                                tempList = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Models.OWS.OperaReservation>>(owsResponse1.responseData.ToString());
                                tempList[0].ReservationNumber += "||" + shareID;
                                temp_operaReservations.AddRange(tempList);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    new LogHelper().Error(ex, pushReservationRequest.ReservationNumber, "PushCloudDueInReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-In push");
                }
                #endregion

                #region Pushing record copy in local DB
                new LogHelper().Log("Updating the reservation in Local DB", Reservation.ReservationNameID, "PushCloudDueInReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-In push");
                //Reservation.IsEmailSend = false;
                //Reservation.reservationDocument = null;
                Models.Local.LocalResponseModel localResponse = await new WSClientHelper().PushRecordLocally(new Models.Local.LocalRequestModel()
                {
                    SyncFromCloud = false,
                    RequestObject = temp_operaReservations,//new List<Models.OWS.OperaReservation>() { Reservation }
                }, Reservation.ReservationNameID, "Due-In push", pushReservationRequest.ServiceParameters);
                if (!localResponse.result)
                {
                    new LogHelper().Log("Updating the reservation in Local DB with email send flag failled with reason :- " + localResponse.responseMessage, Reservation.ReservationNameID, "PushCloudDueInReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-In push");
                }
                else
                    new LogHelper().Log("Updating the reservation in Local DB with email send flag fsucceeded", Reservation.ReservationNameID, "PushCloudDueInReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-In push");
                #endregion

                #region Validating email present or not
                bool isEmailPresent = false;
                if (Reservation != null && Reservation.GuestProfiles != null && Reservation.GuestProfiles.Count > 0 &&
                    Reservation.GuestProfiles[0].Email != null && Reservation.GuestProfiles[0].Email.Count > 0 &&
                    !string.IsNullOrEmpty(Reservation.GuestProfiles[0].Email[0].email))
                    isEmailPresent = true;
                if (!isEmailPresent)
                {
                    new LogHelper().Log("Skipping reservation No. : " + Reservation.ReservationNumber + " no email ID present", Reservation.ReservationNameID, "PushCloudDueInReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-In push");
                    return new Models.Local.LocalResponseModel()
                    {
                        result = false,
                        responseMessage = " no email ID present"
                    };
                }
                #endregion

                #region Validating Reservation
                new LogHelper().Log("Validating reservation", Reservation.ReservationNameID, "PushCloudDueInReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-In push");

                if (Reservation == null)
                {
                    new LogHelper().Log("Reservation returned as NULL", Reservation.ReservationNameID, "PushCloudDueInReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-In push");
                    return new Models.Local.LocalResponseModel()
                    {
                        result = false,
                        responseMessage = "Reservation returned as NULL"
                    };
                }
                else if (Reservation.Adults == null && Reservation.Adults == 0)
                {
                    new LogHelper().Log("Reservation adult count is NULL or 0", Reservation.ReservationNameID, "PushCloudDueInReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-In push");
                    return new Models.Local.LocalResponseModel()
                    {
                        result = false,
                        responseMessage = "Reservation adult count is NULL or 0"
                    };
                }
                else if (string.IsNullOrEmpty(Reservation.ReservationStatus) && (!Reservation.ReservationStatus.ToUpper().Equals("DUEIN") || !Reservation.ReservationStatus.ToUpper().Equals("RESERVED")))
                {
                    new LogHelper().Debug("Reservation status is : " + Reservation.ReservationStatus + " not elogible for pre-checkin", Reservation.ReservationNameID, "PushCloudDueInReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-In push");
                    return new Models.Local.LocalResponseModel()
                    {
                        result = false,
                        responseMessage = "Reservation adult count is NULL or 0"
                    };
                }
                new LogHelper().Log("Reservation Validated ", Reservation.ReservationNameID, "PushCloudDueInReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-In push");
                #endregion

                #region Processing sharer Profiles
                new LogHelper().Log("Processing sharer in the reservation", Reservation.ReservationNameID, "PushCloudDueInReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-In push");
                if (Reservation.SharerReservations != null && Reservation.SharerReservations.Count > 0)
                {
                    foreach (Models.OWS.OperaReservation sharer in Reservation.SharerReservations)
                    {
                        if (sharer.GuestProfiles != null && sharer.GuestProfiles.Count > 0)
                        {
                            foreach (Models.OWS.GuestProfile guestProfile in sharer.GuestProfiles)
                            {
                                Reservation.GuestProfiles.Add(guestProfile);
                            }
                        }
                    }
                    new LogHelper().Log("Processing sharer in the reservation completed", Reservation.ReservationNameID, "PushCloudDueInReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-In push");
                }
                else
                    new LogHelper().Log("No sharers found", Reservation.ReservationNameID, "PushCloudDueInReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-In push");
                #endregion

                #region Processing RoomRate
                new LogHelper().Log("Updating rate details", Reservation.ReservationNameID, "PushCloudDueInReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-In push");
                if (Reservation.RateDetails != null && Reservation.RateDetails.DailyRates != null && Reservation.RateDetails.DailyRates.Count > 0)
                {
                    decimal total_roomrate = Reservation.RateDetails.DailyRates.Sum(x => x.Amount);
                    if (Reservation.PrintRate != null && Reservation.PrintRate.Value)
                        Reservation.RateDetails.RateAmount = total_roomrate;
                    else
                        Reservation.TotalAmount = 0;
                    new LogHelper().Log("Updating rate details completed", Reservation.ReservationNameID, "PushCloudDueInReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-In push");
                }
                else
                    new LogHelper().Log("Updating rate details failed because Rate details are blank in the reservation", Reservation.ReservationNameID, "PushCloudDueInReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-In push");
                #endregion

                #region Processing MealPlan
                if (pushReservationRequest.ServiceParameters.IsBreakFastValidationWithUDF != null && pushReservationRequest.ServiceParameters.IsBreakFastValidationWithUDF.Value)
                {
                    new LogHelper().Debug("Processing meal plan from UDF fields", Reservation.ReservationNameID, "PushCloudDueInReservation", "Grabber", "Due-In push");
                    if (Reservation.userDefinedFields != null && Reservation.userDefinedFields.Count > 0)
                    {
                        if (Reservation.userDefinedFields.Find(x => x.FieldName.Equals(pushReservationRequest.ServiceParameters.MealPlanFieldName)) != null)
                        {

                            if (!Reservation.userDefinedFields.Find(x => x.FieldName.Equals(pushReservationRequest.ServiceParameters.MealPlanFieldName)).FieldValue.Equals("NP"))
                            {
                                string tempUDFValue = Reservation.userDefinedFields.Find(x => x.FieldName.Equals(pushReservationRequest.ServiceParameters.MealPlanFieldName)).FieldValue;
                                if (!string.IsNullOrEmpty(tempUDFValue))
                                {
                                    bool isPackageFound = false;
                                    if (pushReservationRequest.ServiceParameters.PackageCodes.Split(';').ToList().Contains(tempUDFValue))
                                        isPackageFound = true;
                                    if (isPackageFound)
                                    {
                                        Reservation.IsBreakFastAvailable = true;
                                        new LogHelper().Debug("Meal plan updated", Reservation.ReservationNameID, "PushCloudDueInReservation", "Grabber", "Due-In push");
                                    }
                                }
                            }
                            else
                                new LogHelper().Log("Processing meal plan not updated (NP not present in UDF)", Reservation.ReservationNameID, "PushCloudDueInReservation", "Grabber", "Due-In push");
                        }
                    }
                    else
                        new LogHelper().Log("No UDF fields for meal plan not found", Reservation.ReservationNameID, "PushCloudDueInReservation", "Grabber", "Due-In push");
                }
                if (pushReservationRequest.ServiceParameters.IsBreakFastValidationWithPackage != null && pushReservationRequest.ServiceParameters.IsBreakFastValidationWithPackage.Value)
                {
                    new LogHelper().Debug("Processing package list in the reservation for meal plan", Reservation.ReservationNameID, "PushCloudDueInReservation", "Grabber", "Due-In push");
                    if (((Reservation.PackageDetails != null && Reservation.PackageDetails.Count > 0) || (Reservation.PreferanceDetails != null && Reservation.PreferanceDetails.Count > 0)) && (!string.IsNullOrEmpty(pushReservationRequest.ServiceParameters.PackageCodes) && pushReservationRequest.ServiceParameters.PackageCodes.Split(';').ToList() != null))
                    {
                        if (Reservation.PackageDetails != null && Reservation.PackageDetails.Count > 0)
                        {
                            bool isPackageFound = false;
                            foreach (Models.OWS.PackageDetails package in Reservation.PackageDetails)
                            {
                                if (pushReservationRequest.ServiceParameters.PackageCodes.Split(';').ToList().Contains(package.PackageCode))
                                {
                                    isPackageFound = true;
                                    break;
                                }
                            }
                            if (isPackageFound)
                            {
                                Reservation.IsBreakFastAvailable = true;
                                new LogHelper().Debug("Meal plan updated", Reservation.ReservationNameID, "PushCloudDueInReservation", "Grabber", "Due-In push");
                            }
                        }
                        if (Reservation.PreferanceDetails != null && Reservation.PreferanceDetails.Count > 0)
                        {
                            if (Reservation.IsBreakFastAvailable == null || !Reservation.IsBreakFastAvailable.Value)
                            {
                                bool isPrefernceFound = false;
                                foreach (Models.OWS.PreferanceDetails prefernce in Reservation.PreferanceDetails)
                                {
                                    if (pushReservationRequest.ServiceParameters.PackageCodes.Split(';').ToList().Contains(prefernce.PreferanceCode))
                                    {
                                        isPrefernceFound = true;
                                        break;
                                    }
                                }
                                if (isPrefernceFound)
                                {
                                    Reservation.IsBreakFastAvailable = true;
                                    new LogHelper().Debug("Meal plan updated", Reservation.ReservationNameID, "PushCloudDueInReservation", "Grabber", "Due-In push");
                                }
                            }
                        }
                    }
                    else
                        new LogHelper().Log("No package or prefernce list in the reservation for meal plan not found", Reservation.ReservationNameID, "PushCloudDueInReservation", "Grabber", "Due-In push");
                }
                #endregion

                #region VerifyVIPReservationOrNot
                new LogHelper().Log("Verifying reservation VIP status in UDF field", Reservation.ReservationNameID, "PushCloudDueInReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-In push");
                if (Reservation.userDefinedFields != null && Reservation.userDefinedFields.Count > 0)
                {
                    if (Reservation.userDefinedFields.Find(x => x.FieldName.Equals(pushReservationRequest.ServiceParameters.PreAuthUDF)) != null &&
                        Reservation.userDefinedFields.Find(x => x.FieldName.Equals(pushReservationRequest.ServiceParameters.PreAuthUDF)).FieldValue.Equals("NO"))
                    {
                        new LogHelper().Log("Reservation is flagged not to take payment", Reservation.ReservationNameID, "PushCloudDueInReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-In push");
                        Reservation.DepositDetail = new List<Models.OWS.DepositDetail>()
                {
                    new Models.OWS.DepositDetail()
                    {
                        Amount = 0,
                        CardExpiryDate = null,
                        CreditCardNumber = null,
                        IsCreditCardDeposit = false,
                        PaymentType = null
                    }
                };
                        Reservation.IsDepositAvailable = true;
                    }
                    else if (Reservation.userDefinedFields.Find(x => x.FieldName.Equals(pushReservationRequest.ServiceParameters.PreAuthAmntUDF)) != null &&
                        Reservation.userDefinedFields.Find(x => x.FieldName.Equals(pushReservationRequest.ServiceParameters.PreAuthAmntUDF)).FieldValue.Equals("NO"))
                    {

                        new LogHelper().Log("Reservation is flagged not to take payment", Reservation.ReservationNameID, "PushCloudDueInReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-In push");
                        Reservation.DepositDetail = new List<Models.OWS.DepositDetail>()
                        {
                            new Models.OWS.DepositDetail()
                            {
                                Amount = 0,
                                CardExpiryDate = null,
                                CreditCardNumber = null,
                                IsCreditCardDeposit = false,
                                PaymentType = null
                            }
                        };

                        Reservation.IsDepositAvailable = true;
                    }
                }
                new LogHelper().Log("Verifying reservation VIP status in UDF field completed", Reservation.ReservationNameID, "PushCloudDueInReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-In push");
                #endregion

                #region PaymentDesabling
                if (pushReservationRequest.ServiceParameters.IsPaymentDisabled)
                {
                    new LogHelper().Log("Disabling the payment as per the config", Reservation.ReservationNameID, "PushCloudDueInReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-In push");
                    Reservation.DepositDetail = new List<Models.OWS.DepositDetail>()
                {
                    new Models.OWS.DepositDetail()
                    {
                        Amount = 0,
                        CardExpiryDate = null,
                        CreditCardNumber = null,
                        IsCreditCardDeposit = false,
                        PaymentType = null
                    }
                };
                    Reservation.IsDepositAvailable = true;
                }
                #endregion

                #region Update ETA
                if (pushReservationRequest.ServiceParameters.IsETADefault)
                {
                    new LogHelper().Log("Assigning NULL value to ETA as per the config", Reservation.ReservationNameID, "PushCloudDueInReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-In push");
                    Reservation.ExpectedArrivalTime = null;
                }
                #endregion
            
                #region Sending Email
                try
                {

                    new LogHelper().Log("Verifying email send or not ", Reservation.ReservationNameID, "PushCloudDueInReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-In push");
                    //if (isEmailSent == null || !isEmailSent.Value)
                    {
                        new LogHelper().Log("Sending pre-checkin email", Reservation.ReservationNameID, "PushCloudDueInReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-In push");
                        if (Reservation != null && Reservation.GuestProfiles != null && Reservation.GuestProfiles[0].Email != null && Reservation.GuestProfiles[0].Email.Count > 0)
                        {
                            foreach (Models.OWS.Email email in Reservation.GuestProfiles[0].Email)
                            {
                                if (email.primary != null && email.primary.Value)
                                {
                                    TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
                                    Models.Email.EmailResponse emailResponse = await new WSClientHelper().SendEmail(Reservation.ReservationNameID, new Models.Email.EmailRequest()
                                    {
                                        FromEmail = pushReservationRequest.ServiceParameters.PreArrivalFromEmail,
                                        ToEmail = email.email,
                                        GuestName = !string.IsNullOrEmpty(Reservation.GuestProfiles[0].GuestName) ? textInfo.ToTitleCase(Reservation.GuestProfiles[0].GuestName)
                                        : Reservation.GuestProfiles[0].GuestName,
                                        Subject = pushReservationRequest.ServiceParameters.PreArrivalEmailSubject,
                                        confirmationNumber = "?id=" + HttpUtility.UrlEncode(new Helper().EncryptString("b14ca5898a4e4133bbce2ea2315a1916", Reservation.ReservationNumber)),
                                        displayFromEmail = pushReservationRequest.ServiceParameters.EmailDisplayName,
                                        EmailType = Models.Email.EmailType.Precheckedin,
                                        AttachmentFileName = "WelcomeEmail.pdf",
                                        ReservationNumber = !string.IsNullOrEmpty(Reservation.ReservationNumber) ? (Reservation.ReservationNumber.Contains("||") ?
                                                            (Reservation.ReservationNumber.Substring(0, Reservation.ReservationNumber.IndexOf('|') - 1)) : (Reservation.ReservationNumber)) : (Reservation.ReservationNumber),
                                        ArrivalDate = Reservation.ArrivalDate.Value.ToString("dd-MMM-yyyy"),
                                        DepartureDate = Reservation.DepartureDate.Value.ToString("dd-MMM-yyy")

                                    }, "Due-In push", pushReservationRequest.ServiceParameters);
                                    if (!emailResponse.result)
                                    {
                                        isEmailSent = false;
                                        IsEmailProcessed = false;
                                        new LogHelper().Log("Failled to send confirmation email with reason :- " + emailResponse.responseMessage, Reservation.ReservationNameID, "PushCloudDueInReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-In push");
                                        new LogHelper().Warn("Failled to send confirmation email with reason :- " + emailResponse.responseMessage, Reservation.ReservationNameID, "PushCloudDueInReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-In push");
                                    }
                                    else
                                    {
                                        isEmailSent = true;
                                        IsEmailProcessed = true;
                                        new LogHelper().Log("Email send successfully", Reservation.ReservationNameID, "PushCloudDueInReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-In push");
                                    }
                                }
                            }
                        }
                        else
                        {
                            new LogHelper().Log("Failled to send pre-checkin since email address not found from pre checked in list response", Reservation.ReservationNameID, "PushCloudDueInReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-In push");
                            new LogHelper().Warn("Failled to send pre-checkin since email address not found from pre checked in list response", Reservation.ReservationNameID, "PushCloudDueInReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-In push");
                        }
                    }

                }
                catch (Exception ex)
                {
                    new LogHelper().Error(ex, Reservation.ReservationNameID, "PushCloudDueInReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-In push");
                }
                #endregion

                #region Pushing Reservation Track
                if (IsEmailProcessed != null && IsEmailProcessed.Value)
                {
                    new LogHelper().Log("Pushing reservation track from local DB ", Reservation.ReservationNameID, "PushCloudDueInReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-In push");

                    localResponse = await new WSClientHelper().PushReservationTrackLocally(Reservation.ReservationNameID, new Models.Local.LocalRequestModel()
                    {
                        RequestObject = new Models.Local.ReservationTrackStatus()
                        {
                            ReservationNameID = Reservation.ReservationNameID,
                            ProcessType = Models.Local.ReservationProcessType.Precheckinemail.ToString(),
                            ReservationNumber = !string.IsNullOrEmpty(Reservation.ReservationNumber) ? (Reservation.ReservationNumber.Contains("||") ?
                                                            (Reservation.ReservationNumber.Substring(0, Reservation.ReservationNumber.IndexOf('|') - 1)) : (Reservation.ReservationNumber)) : (Reservation.ReservationNumber),
                            EmailSent = IsEmailProcessed.Value,
                            ProcessStatus = ""
                        }
                    }, "Due-In push", pushReservationRequest.ServiceParameters);
                    if (localResponse.result)
                    {
                        new LogHelper().Log("Reservation track in local DB updated successfully ", Reservation.ReservationNameID, "PushCloudDueInReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-In push");
                    }
                    else
                    {
                        //isReservationTrackPresent = true;
                        new LogHelper().Log("Failled to update reservation track in local DB with reason :- " + localResponse.responseMessage, Reservation.ReservationNameID, "PushCloudDueInReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-In push");
                    }
                }
                #endregion

                #region Pushing record copy in local DB
                new LogHelper().Log("Updating the reservation in Local DB with email send flag ", Reservation.ReservationNameID, "PushCloudDueInReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-In push");
                Reservation.IsEmailSend = isEmailSent;
                Reservation.reservationDocument = null;
                localResponse = await new WSClientHelper().PushRecordLocally(new Models.Local.LocalRequestModel()
                {
                    SyncFromCloud = false,
                    RequestObject = new List<Models.OWS.OperaReservation>() { Reservation }
                }, Reservation.ReservationNameID, "Due-In push", pushReservationRequest.ServiceParameters);
                if (!localResponse.result)
                {
                    new LogHelper().Log("Updating the reservation in Local DB with email send flag failled with reason :- " + localResponse.responseMessage, Reservation.ReservationNameID, "PushCloudDueInReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-In push");
                }
                else
                    new LogHelper().Log("Updating the reservation in Local DB with email send flag fsucceeded", Reservation.ReservationNameID, "PushCloudDueInReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-In push");
                #endregion

                #region Updating record status in Local DB
                new LogHelper().Log("Updating the reservation status in Local DB", Reservation.ReservationNameID, "PushCloudDueInReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-In push");
                localResponse = await new WSClientHelper().UpdateRecordLocally(Reservation.ReservationNameID, new Models.Local.LocalRequestModel()
                {
                    RequestObject = new List<Models.Local.UpdateReservationByReservationNameIDModel>()
                {
                    new Models.Local.UpdateReservationByReservationNameIDModel{
                        IsPushedToCloud = true,
                        ReservationNameID = Reservation.ReservationNameID
                        //StatusDescription = "Success"
                    }
                }
                }, "Due-In push", pushReservationRequest.ServiceParameters);
                if (!localResponse.result)
                {
                    new LogHelper().Log("Updating the reservation status in Local DB with email send flag failled with reason :- " + localResponse.responseMessage, Reservation.ReservationNameID, "PushCloudDueInReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-In push");
                }
                else
                    new LogHelper().Log("Updating the reservation status in Local DB ", Reservation.ReservationNameID, "PushCloudDueInReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-In push");
                #endregion

                return new Models.Local.LocalResponseModel()
                {
                    result = true,
                    responseMessage = "Success"
                };
            }
            catch (Exception ex)
            {
                return new Models.Local.LocalResponseModel()
                {
                    result = false,
                    responseMessage = ex.Message
                };
            }

        }
        public async Task<Models.Local.LocalResponseModel> PushCloudDueOutReservationDetails(Models.Local.PushReservationRequest pushReservationRequest)
        {


            #region Variables

            List<Models.OWS.OperaReservation> operaReservationList = null;
            Models.OWS.OperaReservation Reservation = null;
            List<Models.Local.PaymentHeader> paymentHeaders = null;
            Models.OWS.FolioModel guestFolio = null;
            string folioAsBase64 = "";
            bool isEmailSend = false;
            //bool IsEmailProcessed = false;

            bool IsPaymentDisabled = false;
            IsPaymentDisabled = (ConfigurationManager.AppSettings["IsPaymentDisabled"] != null
                            && !string.IsNullOrEmpty(ConfigurationManager.AppSettings["IsPaymentDisabled"].ToString())
                            && bool.TryParse(ConfigurationManager.AppSettings["IsPaymentDisabled"].ToString(), out IsPaymentDisabled)) ? IsPaymentDisabled : false;

            #endregion

            try
            {
                new LogHelper().Debug("Pushing due out reservation details", null, "PushCloudDueOutReservationDetails", pushReservationRequest.ServiceParameters.ClientID, "Due-Out push");

                new LogHelper().Debug("Processing reservation No. : " + pushReservationRequest.ReservationNumber, null, "PushCloudDueOutReservationDetails", pushReservationRequest.ServiceParameters.ClientID, "Due-Out push");

                #region Fetching Reservation from OWS
                new LogHelper().Debug("Fetching opera reservation", pushReservationRequest.ReservationNumber, "PushCloudDueOutReservationDetails", pushReservationRequest.ServiceParameters.ClientID, "Due-Out push");
                Models.OWS.OwsResponseModel owsResponse1 = await new WSClientHelper().FetchReservationAsync(pushReservationRequest.ReservationNumber, new Models.OWS.OwsRequestModel()
                {
                    ChainCode = pushReservationRequest.ServiceParameters.ChainCode,
                    DestinationEntityID = pushReservationRequest.ServiceParameters.DestinationEntityID,
                    HotelDomain = pushReservationRequest.ServiceParameters.HotelDomain,
                    KioskID = pushReservationRequest.ServiceParameters.KioskID,
                    Language = pushReservationRequest.ServiceParameters.Language,
                    LegNumber = pushReservationRequest.ServiceParameters.Legnumber,
                    Password = pushReservationRequest.ServiceParameters.Password,
                    SystemType = pushReservationRequest.ServiceParameters.SystemType,
                    Username = pushReservationRequest.ServiceParameters.Username,
                    FetchBookingRequest = new Models.OWS.FetchBookingRequestModel()
                    {
                        ReservationNumber = pushReservationRequest.ReservationNumber
                    }
                }, "Due-Out push", pushReservationRequest.ServiceParameters);
                if (!owsResponse1.result)
                {
                    new LogHelper().Log("Failled to fetch opera reservation with reason :- " + owsResponse1.responseMessage, pushReservationRequest.ReservationNumber, "PushCloudDueOutReservationDetails", pushReservationRequest.ServiceParameters.ClientID, "Due-Out push");
                    return new Models.Local.LocalResponseModel()
                    {
                        result = false,
                        responseMessage = "Failled to fetch opera reservation with reason :- " + owsResponse1.responseMessage
                    };
                }
                if (owsResponse1.responseData == null)
                {
                    new LogHelper().Log("Failled to fetch opera reservation with reason :- API response data is NULL" + owsResponse1.responseMessage, pushReservationRequest.ReservationNumber, "PushCloudDueOutReservationDetails", pushReservationRequest.ServiceParameters.ClientID, "Due-Out push");
                    return new Models.Local.LocalResponseModel()
                    {
                        result = false,
                        responseMessage = "Failled to fetch opera reservation with reason :- API response data is NULL" + owsResponse1.responseMessage
                    };
                }
                else
                {
                    new LogHelper().Debug("Converting API json to object", pushReservationRequest.ReservationNumber, "PushCloudDueOutReservationDetails", pushReservationRequest.ServiceParameters.ClientID, "Due-Out push");
                    try
                    {
                        List<Models.OWS.OperaReservation> temp_operaReservations = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Models.OWS.OperaReservation>>(owsResponse1.responseData.ToString());
                        Reservation = temp_operaReservations[0];
                        new LogHelper().Debug("Opera reservation fetched successfully", Reservation.ReservationNameID, "PushCloudDueOutReservationDetails", pushReservationRequest.ServiceParameters.ClientID, "Due-Out push");
                    }
                    catch (Exception ex)
                    {
                        new LogHelper().Error(ex, Reservation.ReservationNameID, "PushCloudDueOutReservationDetails", pushReservationRequest.ServiceParameters.ClientID, "Due-Out push");
                        //new LogHelper().Log("Failled to covert API response to object", Reservation.ReservationNameID, "PushCloudDueOutReservationDetails", pushReservationRequest.ServiceParameters.ClientID, "Due-Out push");
                        //new LogHelper().Warn("Failled to fetch opera reservation with reason :- " + ex.Message, Reservation.ReservationNameID, "PushCloudDueOutReservationDetails", pushReservationRequest.ServiceParameters.ClientID, "Due-Out push");
                        //new LogHelper().Debug("Failled to fetch opera reservation with reason :- " + ex.Message, Reservation.ReservationNameID, "PushCloudDueOutReservationDetails", pushReservationRequest.ServiceParameters.ClientID, "Due-Out push");
                        return new Models.Local.LocalResponseModel()
                        {
                            result = false,
                            responseMessage = ex.Message
                        };
                    }
                }

                #endregion

                #region Verifying Day use guest
                if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["IsDayUseReservationCheckOutEnabled"]) && !bool.TryParse(ConfigurationManager.AppSettings["IsDayUseReservationCheckOutEnabled"].ToString(), out bool result))
                {
                    new LogHelper().Debug("verifying day use guest or not", Reservation.ReservationNameID, "PushCloudDueOutReservationDetails", pushReservationRequest.ServiceParameters.ClientID, "Due-Out push");
                    if (Reservation != null)
                    {
                        if (Reservation.ArrivalDate != null && Reservation.DepartureDate != null && Reservation.ArrivalDate.Value.Equals(Reservation.DepartureDate.Value))
                        {
                            new LogHelper().Debug("reservation is a day use guest, so skipping the reservation", Reservation.ReservationNameID, "PushCloudDueOutReservationDetails", pushReservationRequest.ServiceParameters.ClientID, "Due-Out push");
                            return new Models.Local.LocalResponseModel()
                            {
                                result = false,
                                responseMessage = "reservation is a day use guest, so skipping the reservation"
                            };
                        }
                        else
                            new LogHelper().Debug("reservation is not a day use guest", Reservation.ReservationNameID, "PushCloudDueOutReservationDetails", pushReservationRequest.ServiceParameters.ClientID, "Due-Out push");
                    }
                    else
                        new LogHelper().Debug("verifying day use guest or not failled reservation object is blank", Reservation.ReservationNameID, "PushCloudDueOutReservationDetails", pushReservationRequest.ServiceParameters.ClientID, "Due-Out push");
                }
                #endregion

                #region Validating email present or not
                bool isEmailPresent = false;
                if (Reservation != null && Reservation.GuestProfiles != null && Reservation.GuestProfiles.Count > 0 &&
                    Reservation.GuestProfiles[0].Email != null && Reservation.GuestProfiles[0].Email.Count > 0 &&
                    !string.IsNullOrEmpty(Reservation.GuestProfiles[0].Email[0].email))
                    isEmailPresent = true;
                if (!isEmailPresent)
                {
                    new LogHelper().Debug("Skipping reservation No. : " + Reservation.ReservationNumber + " no email ID present", Reservation.ReservationNameID, "PushCloudDueOutReservationDetails", pushReservationRequest.ServiceParameters.ClientID, "Due-Out push");
                    return new Models.Local.LocalResponseModel()
                    {
                        result = false,
                        responseMessage = "Skipping reservation No. : " + Reservation.ReservationNumber + " no email ID present"
                    };
                }
                #endregion

                #region Verifying the reservation Status
                if (string.IsNullOrEmpty(Reservation.ReservationStatus) || !Reservation.ReservationStatus.Equals("DUEOUT"))
                {
                    new LogHelper().Debug("Reservation status : " + Reservation.ReservationStatus + " (not DUEOUT, so skipping the reservation)", Reservation.ReservationNameID, "PushCloudDueOutReservationDetails", pushReservationRequest.ServiceParameters.ClientID, "Due-Out push");
                    return new Models.Local.LocalResponseModel()
                    {
                        result = false,
                        responseMessage = "Reservation status : " + Reservation.ReservationStatus + " (not DUEOUT, so skipping the reservation)"
                    };
                }
                #endregion

                #region Check Payment in Saavy
                //Models.Local.LocalResponseModel localResponse = null;
                new LogHelper().Debug("Fetching payment details for reservation No. : " + Reservation.ReservationNumber + " in Saavy Pay", Reservation.ReservationNameID, "PushCloudDueOutReservationDetails", pushReservationRequest.ServiceParameters.ClientID, "Due-Out push");
                Models.Local.LocalResponseModel localResponse = await new WSClientHelper().FetchPaymentDetails(Reservation.ReservationNameID, new Models.Local.LocalRequestModel()
                {
                    RequestObject = new Models.Local.FetchPaymentRequest()
                    {
                        ReservationNameID = Reservation.ReservationNameID,
                        isActive = true
                    }
                }, "Due-Out push", pushReservationRequest.ServiceParameters);

                if (!localResponse.result || localResponse.responseData == null)
                {
                    new LogHelper().Debug("Failled to fetch payment details with reason :- " + localResponse.responseMessage, Reservation.ReservationNameID, "PushCloudDueOutReservationDetails", pushReservationRequest.ServiceParameters.ClientID, "Due-Out push");


                }
                else
                {
                    new LogHelper().Debug("Converting API json to object", Reservation.ReservationNameID, "PushCloudDueOutReservationDetails", pushReservationRequest.ServiceParameters.ClientID, "Due-Out push");
                    try
                    {
                        paymentHeaders = JsonConvert.DeserializeObject<List<Models.Local.PaymentHeader>>(localResponse.responseData.ToString());
                        new LogHelper().Debug("Payment details fetched successfully", Reservation.ReservationNameID, "PushCloudDueOutReservationDetails", pushReservationRequest.ServiceParameters.ClientID, "Due-Out push");
                    }
                    catch (Exception ex)
                    {
                        new LogHelper().Error(ex, Reservation.ReservationNameID, "PushDueOutReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-Out push");
                        new LogHelper().Log("Failled to covert API response to object", Reservation.ReservationNameID, "PushDueOutReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-Out push");

                    }
                }

                #endregion

                #region FetchFolioItemsByWindow
                new LogHelper().Debug("Fetching reservation folio by window for reservation No. : " + Reservation.ReservationNumber, Reservation.ReservationNameID, "PushCloudDueOutReservationDetails", pushReservationRequest.ServiceParameters.ClientID, "Due-Out push");
                owsResponse1 = await new WSClientHelper().GetFolioByWindow(Reservation.ReservationNameID, new Models.OWS.OwsRequestModel()
                {
                    ChainCode = pushReservationRequest.ServiceParameters.ChainCode,
                    DestinationEntityID = pushReservationRequest.ServiceParameters.DestinationEntityID,
                    HotelDomain = pushReservationRequest.ServiceParameters.HotelDomain,
                    KioskID = pushReservationRequest.ServiceParameters.KioskID,
                    Language = pushReservationRequest.ServiceParameters.Language,
                    LegNumber = pushReservationRequest.ServiceParameters.Legnumber,
                    Password = pushReservationRequest.ServiceParameters.Password,
                    SystemType = pushReservationRequest.ServiceParameters.SystemType,
                    Username = pushReservationRequest.ServiceParameters.Username,
                    FetchFolioRequest = new Models.OWS.FetchFolioRequest()
                    {
                        ReservationNameID = Reservation.ReservationNameID,
                        ProfileID = (Reservation.GuestProfiles != null && Reservation.GuestProfiles.Count > 0) ? Reservation.GuestProfiles[0].PmsProfileID : ""
                    }
                }, "Due-Out push", pushReservationRequest.ServiceParameters);

                if (!owsResponse1.result)
                {
                    new LogHelper().Log("Failled to fetch folio by window with reason :- " + owsResponse1.responseMessage, Reservation.ReservationNameID, "PushCloudDueOutReservationDetails", pushReservationRequest.ServiceParameters.ClientID, "Due-Out push");

                    return new Models.Local.LocalResponseModel()
                    {
                        result = false,
                        responseMessage = "Failled to fetch folio by window with reason :- " + owsResponse1.responseMessage
                    };
                }
                else
                {
                    new LogHelper().Debug("Converting API json to object", Reservation.ReservationNameID, "PushCloudDueOutReservationDetails", pushReservationRequest.ServiceParameters.ClientID, "Due-Out push");
                    try
                    {

                        guestFolio = JsonConvert.DeserializeObject<Models.OWS.FolioModel>(owsResponse1.responseData.ToString());
                        if (guestFolio != null)
                        {
                            new LogHelper().Debug("Current guest balance of the reservation is : " + guestFolio.BalanceAmount, Reservation.ReservationNameID, "PushCloudDueOutReservationDetails", pushReservationRequest.ServiceParameters.ClientID, "Due-Out push");
                            new LogHelper().Debug("Reservation folio by window fetched successfully", Reservation.ReservationNameID, "PushCloudDueOutReservationDetails", pushReservationRequest.ServiceParameters.ClientID, "Due-Out push");
                        }
                        else
                        {
                            new LogHelper().Log("No folio items found in the reservation", Reservation.ReservationNameID, "PushDueOutReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-Out push");
                        }
                    }
                    catch (Exception ex)
                    {
                        new LogHelper().Error(ex, Reservation.ReservationNameID, "PushCloudDueOutReservationDetails", pushReservationRequest.ServiceParameters.ClientID, "Due-Out push");
                        new LogHelper().Log("Failled to covert API response to object", Reservation.ReservationNameID, "PushCloudDueOutReservationDetails", pushReservationRequest.ServiceParameters.ClientID, "Due-Out push");

                    }
                }
                #endregion



                #region FetchFolioAsBase64
                new LogHelper().Debug("Fetching reservation folio as base64 for reservation No. : " + Reservation.ReservationNumber, Reservation.ReservationNameID, "PushCloudDueOutReservationDetails", pushReservationRequest.ServiceParameters.ClientID, "Due-Out push");
                owsResponse1 = await new WSClientHelper().GetFolio(Reservation.ReservationNameID, new Models.OWS.OwsRequestModel()
                {
                    ChainCode = pushReservationRequest.ServiceParameters.ChainCode,
                    DestinationEntityID = pushReservationRequest.ServiceParameters.DestinationEntityID,
                    HotelDomain = pushReservationRequest.ServiceParameters.HotelDomain,
                    KioskID = pushReservationRequest.ServiceParameters.KioskID,
                    Language = pushReservationRequest.ServiceParameters.Language,
                    LegNumber = pushReservationRequest.ServiceParameters.Legnumber,
                    Password = pushReservationRequest.ServiceParameters.Password,
                    SystemType = pushReservationRequest.ServiceParameters.SystemType,
                    Username = pushReservationRequest.ServiceParameters.Username,
                    FetchFolioRequest = new Models.OWS.FetchFolioRequest()
                    {
                        ReservationNameID = Reservation.ReservationNameID,
                        OperaReservation = Reservation,
                        ProfileID = (Reservation.GuestProfiles != null && Reservation.GuestProfiles.Count > 0) ? Reservation.GuestProfiles[0].PmsProfileID : "",
                        FolioList = guestFolio
                    }
                }, "Due-Out push", pushReservationRequest.ServiceParameters);

                if (!owsResponse1.result || owsResponse1.responseData == null)
                {
                    new LogHelper().Log("Failled to fetch folio with reason :- " + owsResponse1.responseMessage, Reservation.ReservationNameID, "PushCloudDueOutReservationDetails", pushReservationRequest.ServiceParameters.ClientID, "Due-Out push");

                    return new Models.Local.LocalResponseModel()
                    {
                        result = false,
                        responseMessage = "Failled to fetch folio with reason :- " + owsResponse1.responseMessage
                    };
                }
                else
                {
                    folioAsBase64 = owsResponse1.responseData.ToString();
                    new LogHelper().Debug("Fetched guest folio as base64 successfully", Reservation.ReservationNameID, "PushCloudDueOutReservationDetails", pushReservationRequest.ServiceParameters.ClientID, "Due-Out push");
                }
                #endregion


                //if (!IsPaymentDisabled)
                //{
                //    #region Pushing Payment Details

                //    new LogHelper().Log("Pushing payment details to the cloud, reservation No. : " + Reservation.ReservationNumber, Reservation.ReservationNameID, "PushDueOutReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-Out push");
                //    Models.Cloud.CloudResponseModel cloudResponse = await new WSClientHelper().PushPaymentDetails(Reservation.ReservationNameID, new Models.Cloud.CloudRequestModel()
                //    {
                //        RequestObject = paymentHeaders
                //    }, "Due-Out push", pushReservationRequest.ServiceParameters);
                //    if (!cloudResponse.result)
                //    {
                //        new LogHelper().Log("Failled to push the payment details to cloud, so skipping the reservation ", Reservation.ReservationNameID, "PushDueOutReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-Out push");
                //        return new Models.Local.LocalResponseModel()
                //        {
                //            result = false,
                //            responseMessage = "Failled to push the payment details to cloud, so skipping the reservation "
                //        };
                //    }
                //    new LogHelper().Log("Payment details to cloud successfully", Reservation.ReservationNameID, "PushDueOutReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-Out push");
                //    #endregion

                //}

                #region Sending Email
                try
                {

                    new LogHelper().Log("Verifying email send or not ", Reservation.ReservationNameID, "PushDueOutReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-Out push");
                    //if (!isReservationTrackPresent)
                    new LogHelper().Log("Sending pre-checkout email", Reservation.ReservationNameID, "PushDueOutReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-Out push");
                    if (Reservation != null && Reservation.GuestProfiles != null && Reservation.GuestProfiles[0].Email != null && Reservation.GuestProfiles[0].Email.Count > 0)
                    {
                        TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
                        foreach (Models.OWS.Email email in Reservation.GuestProfiles[0].Email)
                        {
                            if (email.primary != null && email.primary.Value)
                            {
                                Models.Email.EmailResponse emailResponse = await new WSClientHelper().SendEmail(Reservation.ReservationNameID, new Models.Email.EmailRequest()
                                {
                                    FromEmail = pushReservationRequest.ServiceParameters.PreCheckoutFromEmail,
                                    ToEmail = email.email,
                                    IsPrecheckinEmail = false,
                                    GuestName = !string.IsNullOrEmpty(Reservation.GuestProfiles[0].GuestName) ? textInfo.ToTitleCase(Reservation.GuestProfiles[0].GuestName)
                                        : Reservation.GuestProfiles[0].GuestName,//Reservation.GuestProfiles[0].GuestName,
                                    Subject = pushReservationRequest.ServiceParameters.PreCheckoutEmailSubject,
                                    confirmationNumber = "?id=" + HttpUtility.UrlEncode(new Helper().EncryptString("b14ca5898a4e4133bbce2ea2315a1916", Reservation.ReservationNumber)),
                                    displayFromEmail = pushReservationRequest.ServiceParameters.EmailDisplayName,
                                    EmailType = Models.Email.EmailType.PreCheckedout

                                }, "Due-Out push", pushReservationRequest.ServiceParameters);
                                if (!emailResponse.result)
                                {
                                    isEmailSend = false;
                                    new LogHelper().Log("Failled to send confirmation email with reason :- " + emailResponse.responseMessage, Reservation.ReservationNameID, "PushDueOutReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-Out push");
                                    new LogHelper().Warn("Failled to send confirmation email with reason :- " + emailResponse.responseMessage, Reservation.ReservationNameID, "PushDueOutReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-Out push");
                                }
                                else
                                {
                                    isEmailSend = true;
                                    new LogHelper().Log("Email send successfully", Reservation.ReservationNameID, "PushDueOutReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-Out push");
                                }
                            }
                        }
                    }
                    else
                    {
                        new LogHelper().Log("Failled to send pre-checkout since email address not found from pre checked in list response", Reservation.ReservationNameID, "PushDueOutReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-Out push");
                        new LogHelper().Warn("Failled to send pre-checkout since email address not found from pre checked in list response", Reservation.ReservationNameID, "PushDueOutReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-Out push");
                    }

                }
                catch (Exception ex)
                {
                    new LogHelper().Error(ex, Reservation.ReservationNameID, "PushDueOutReservation", pushReservationRequest.ServiceParameters.ClientID, "Due-Out push");
                }
                #endregion




                #region Pushing record copy in local DB
                new LogHelper().Debug("Updating the reservation in Local DB with email send flag ", Reservation.ReservationNameID, "PushCloudDueOutReservationDetails", pushReservationRequest.ServiceParameters.ClientID, "Due-Out push");

                Reservation.reservationDocument = null;
                localResponse = await new WSClientHelper().PushRecordLocally(new Models.Local.LocalRequestModel()
                {
                    SyncFromCloud = false,
                    RequestObject = new List<Models.OWS.OperaReservation>() { Reservation }
                }, Reservation.ReservationNameID, "Due-Out push", pushReservationRequest.ServiceParameters);
                if (!localResponse.result)
                {
                    new LogHelper().Log("Updating the reservation in Local DB with email send flag failled with reason :- " + localResponse.responseMessage, Reservation.ReservationNameID, "PushCloudDueOutReservationDetails", pushReservationRequest.ServiceParameters.ClientID, "Due-Out push");
                }
                else
                    new LogHelper().Debug("Updating the reservation in Local DB with email send flag fsucceeded", Reservation.ReservationNameID, "PushCloudDueOutReservationDetails", pushReservationRequest.ServiceParameters.ClientID, "Due-Out push");
                #endregion

                new LogHelper().Debug("Reservation :- " + Reservation.ReservationNumber + " processed", Reservation.ReservationNameID, "PushCloudDueOutReservationDetails", pushReservationRequest.ServiceParameters.ClientID, "Due-Out push");

                return new Models.Local.LocalResponseModel()
                {
                    result = true,
                    responseMessage = "Success"
                };
            }
            catch (Exception ex)
            {
                return new Models.Local.LocalResponseModel()
                {
                    result = false,
                    responseMessage = ex.Message
                };
            }
        }

        #endregion
    }
}