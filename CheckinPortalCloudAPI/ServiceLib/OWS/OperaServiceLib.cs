//using Adyen.Model.Nexo;
using Antlr.Runtime.Misc;
using CheckinPortalCloudAPI.Helper;
using CheckinPortalCloudAPI.InformationService;
using CheckinPortalCloudAPI.Models;
using CheckinPortalCloudAPI.Models.OWS;
using Microsoft.Ajax.Utilities;
using Microsoft.Reporting.WebForms;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI.WebControls;
using System.Xml;

namespace CheckinPortalCloudAPI.ServiceLib.OWS
{
    public class OperaServiceLib
    {
        public Models.OWS.OwsResponseModel ModifyPackage(Models.OWS.OwsRequestModel modifyReservation)
        {
            try
            {

                ReservationService.UpdatePackagesRequest updatePackageReq = new ReservationService.UpdatePackagesRequest();
                ReservationService.UpdatePackagesResponse updatePackageRes = new ReservationService.UpdatePackagesResponse();

                #region Request Header

                string temp = Helper.Helper.Get8Digits();
                ReservationService.OGHeader OGHeader = new ReservationService.OGHeader();
                OGHeader.transactionID = temp;
                OGHeader.timeStamp = DateTime.Now;
                OGHeader.primaryLangID = modifyReservation.Language; //English
                ReservationService.EndPoint orginEndPOint = new ReservationService.EndPoint();
                orginEndPOint.entityID = modifyReservation.KioskID; //Kiosk Identifier
                orginEndPOint.systemType = modifyReservation.SystemType;//"KIOSK";
                OGHeader.Origin = orginEndPOint;
                ReservationService.EndPoint destEndPOint = new ReservationService.EndPoint();
                destEndPOint.entityID = modifyReservation.DestinationEntityID;
                destEndPOint.systemType = modifyReservation.DestinationSystemType;
                OGHeader.Destination = destEndPOint;
                ReservationService.OGHeaderAuthentication Auth = new ReservationService.OGHeaderAuthentication();
                ReservationService.OGHeaderAuthenticationUserCredentials userCredentials = new ReservationService.OGHeaderAuthenticationUserCredentials();
                userCredentials.UserName = modifyReservation.Username;
                userCredentials.UserPassword = modifyReservation.Password;
                Auth.UserCredentials = userCredentials;
                OGHeader.Authentication = Auth;
                #endregion

                ReservationService.ReservationServiceSoapClient ResSoapCLient = new ReservationService.ReservationServiceSoapClient();
                bool isOperaCloudEnabled = false;
                isOperaCloudEnabled = (ConfigurationManager.AppSettings["OperaCloudEnabled"] != null
                                && !string.IsNullOrEmpty(ConfigurationManager.AppSettings["OperaCloudEnabled"].ToString())
                                && bool.TryParse(ConfigurationManager.AppSettings["OperaCloudEnabled"].ToString(), out isOperaCloudEnabled)) ? isOperaCloudEnabled : false;
                if(isOperaCloudEnabled)
                {
                    ResSoapCLient.Endpoint.Behaviors.Add(new Helper.CustomEndpointBehaviour(ConfigurationManager.AppSettings["WSSEUserName"].ToString(),
                                            ConfigurationManager.AppSettings["WSSEPassword"].ToString(), 
                                            modifyReservation.Username, modifyReservation.Password, modifyReservation.HotelDomain));                                            
                }

                ReservationService.UniqueID uID = new ReservationService.UniqueID();
                uID.type = ReservationService.UniqueIDType.INTERNAL;
                uID.Value = modifyReservation.ModifyPackageRequest.ReservationNumber;
                updatePackageReq.HotelReference = new ReservationService.HotelReference()
                {
                    chainCode = modifyReservation.ChainCode,
                    hotelCode = modifyReservation.HotelDomain
                };
                updatePackageReq.ConfirmationNumber = uID;



                uID = new ReservationService.UniqueID();
                uID.type = ReservationService.UniqueIDType.INTERNAL;
                uID.source = "LEGNUMBER";
                uID.Value = modifyReservation.LegNumber;
                updatePackageReq.LegNumber = uID;

                updatePackageReq.ProductCode = modifyReservation.ModifyPackageRequest.ProductCode;
                updatePackageReq.Quantity = modifyReservation.ModifyPackageRequest.Quantity != null ? (int)modifyReservation.ModifyPackageRequest.Quantity : 1;
                updatePackageReq.QuantitySpecified = modifyReservation.ModifyPackageRequest.QuantitySpecified;

                updatePackageRes = ResSoapCLient.UpdatePackages(ref OGHeader, updatePackageReq);

                if (updatePackageRes.Result.resultStatusFlag == ReservationService.ResultStatusFlag.SUCCESS)
                {
                    return new Models.OWS.OwsResponseModel()
                    {
                        responseData = null,
                        result = true,
                        responseMessage = "Success"
                    };
                }
                else
                {
                    return new Models.OWS.OwsResponseModel()
                    {
                        responseData = null,
                        result = false,
                        responseMessage = "Failled to update package"
                    };
                }



            }
            catch (Exception ex)
            {
                return new Models.OWS.OwsResponseModel
                {
                    responseMessage = ex.ToString(),
                    statusCode = -1,
                    result = false
                };

            }
        }

        public Models.OWS.OwsResponseModel AddToReservationQueue(Models.OWS.OwsRequestModel reservationQueRquest)
        {
            try
            {

                ReservationAdvancedService.QueueReservationRequest queueReservationReq = new ReservationAdvancedService.QueueReservationRequest();
                ReservationAdvancedService.QueueReservationResponse queueReservationRes = new ReservationAdvancedService.QueueReservationResponse();

                #region Request Header

                string temp = Helper.Helper.Get8Digits();
                ReservationAdvancedService.OGHeader OGHeader = new ReservationAdvancedService.OGHeader();
                OGHeader.transactionID = temp;
                OGHeader.timeStamp = DateTime.Now;
                OGHeader.primaryLangID = reservationQueRquest.Language; //English
                ReservationAdvancedService.EndPoint orginEndPOint = new ReservationAdvancedService.EndPoint();
                orginEndPOint.entityID = reservationQueRquest.KioskID; //Kiosk Identifier
                orginEndPOint.systemType = reservationQueRquest.SystemType;//"KIOSK";
                OGHeader.Origin = orginEndPOint;
                ReservationAdvancedService.EndPoint destEndPOint = new ReservationAdvancedService.EndPoint();
                destEndPOint.entityID = reservationQueRquest.DestinationEntityID;
                destEndPOint.systemType = reservationQueRquest.DestinationSystemType;
                OGHeader.Destination = destEndPOint;
                ReservationAdvancedService.OGHeaderAuthentication Auth = new ReservationAdvancedService.OGHeaderAuthentication();
                ReservationAdvancedService.OGHeaderAuthenticationUserCredentials userCredentials = new ReservationAdvancedService.OGHeaderAuthenticationUserCredentials();
                userCredentials.UserName = reservationQueRquest.Username;
                userCredentials.UserPassword = reservationQueRquest.Password;
                Auth.UserCredentials = userCredentials;
                OGHeader.Authentication = Auth;
                #endregion

                ReservationAdvancedService.ResvAdvancedServiceSoapClient ResSoapCLient = new ReservationAdvancedService.ResvAdvancedServiceSoapClient();
                bool isOperaCloudEnabled = false;
                isOperaCloudEnabled = (ConfigurationManager.AppSettings["OperaCloudEnabled"] != null
                                && !string.IsNullOrEmpty(ConfigurationManager.AppSettings["OperaCloudEnabled"].ToString())
                                && bool.TryParse(ConfigurationManager.AppSettings["OperaCloudEnabled"].ToString(), out isOperaCloudEnabled)) ? isOperaCloudEnabled : false;
                if (isOperaCloudEnabled)
                {
                    ResSoapCLient.Endpoint.Behaviors.Add(new Helper.CustomEndpointBehaviour(ConfigurationManager.AppSettings["WSSEUserName"].ToString(),
                                            ConfigurationManager.AppSettings["WSSEPassword"].ToString(),
                                            reservationQueRquest.Username, reservationQueRquest.Password, reservationQueRquest.HotelDomain));
                }

                ReservationAdvancedService.UniqueID uID = new ReservationAdvancedService.UniqueID();
                uID.type = ReservationAdvancedService.UniqueIDType.INTERNAL;
                uID.source = "RESV_NAME_ID";
                uID.Value = reservationQueRquest.ReservationQueueRequest.ReservationNameID;
                ReservationAdvancedService.UniqueID[] UIDLIST = new ReservationAdvancedService.UniqueID[1];
                UIDLIST[0] = uID;
                queueReservationReq.ReservationRequest = new ReservationAdvancedService.ReservationRequestBase()
                {
                    HotelReference = new ReservationAdvancedService.HotelReference()
                    {
                        chainCode = reservationQueRquest.ChainCode,
                        hotelCode = reservationQueRquest.HotelDomain
                    },
                    ReservationID = UIDLIST
                    
                };

                

                
               

                queueReservationReq.ActionType = ReservationAdvancedService.RequestActionType.ADD;



               

                

                queueReservationRes = ResSoapCLient.QueueReservation(ref OGHeader, queueReservationReq);

                if (queueReservationRes.Result.resultStatusFlag == ReservationAdvancedService.ResultStatusFlag.SUCCESS)
                {
                    return new Models.OWS.OwsResponseModel()
                    {
                        responseData = null,
                        result = true,
                        responseMessage = "Success," + queueReservationRes.QueueMessage
                    };
                }
                else
                {
                    return new Models.OWS.OwsResponseModel()
                    {
                        responseData = null,
                        result = false,
                        responseMessage = "Failled to update package"
                    };
                }



            }
            catch (Exception ex)
            {
                return new Models.OWS.OwsResponseModel
                {
                    responseMessage = ex.ToString(),
                    statusCode = -1,
                    result = false
                };

            }
        }


        

        public Models.OWS.OwsResponseModel FetchPackages(Models.OWS.OwsRequestModel modifyReservation)
        {
            try
            {

                ReservationService.UpdatePackagesRequest updatePackageReq = new ReservationService.UpdatePackagesRequest();
                ReservationService.UpdatePackagesResponse updatePackageRes = new ReservationService.UpdatePackagesResponse();

                #region Request Header

                string temp = Helper.Helper.Get8Digits();
                ReservationService.OGHeader OGHeader = new ReservationService.OGHeader();
                OGHeader.transactionID = temp;
                OGHeader.timeStamp = DateTime.Now;
                OGHeader.primaryLangID = modifyReservation.Language; //English
                ReservationService.EndPoint orginEndPOint = new ReservationService.EndPoint();
                orginEndPOint.entityID = modifyReservation.KioskID; //Kiosk Identifier
                orginEndPOint.systemType = modifyReservation.SystemType;//"KIOSK";
                OGHeader.Origin = orginEndPOint;
                ReservationService.EndPoint destEndPOint = new ReservationService.EndPoint();
                destEndPOint.entityID = modifyReservation.DestinationEntityID;
                destEndPOint.systemType = modifyReservation.DestinationSystemType;
                OGHeader.Destination = destEndPOint;
                ReservationService.OGHeaderAuthentication Auth = new ReservationService.OGHeaderAuthentication();
                ReservationService.OGHeaderAuthenticationUserCredentials userCredentials = new ReservationService.OGHeaderAuthenticationUserCredentials();
                userCredentials.UserName = modifyReservation.Username;
                userCredentials.UserPassword = modifyReservation.Password;
                Auth.UserCredentials = userCredentials;
                OGHeader.Authentication = Auth;
                #endregion

                ReservationService.ReservationServiceSoapClient ResSoapCLient = new ReservationService.ReservationServiceSoapClient();
                bool isOperaCloudEnabled = false;
                isOperaCloudEnabled = (ConfigurationManager.AppSettings["OperaCloudEnabled"] != null
                                && !string.IsNullOrEmpty(ConfigurationManager.AppSettings["OperaCloudEnabled"].ToString())
                                && bool.TryParse(ConfigurationManager.AppSettings["OperaCloudEnabled"].ToString(), out isOperaCloudEnabled)) ? isOperaCloudEnabled : false;
                if (isOperaCloudEnabled)
                {
                    ResSoapCLient.Endpoint.Behaviors.Add(new Helper.CustomEndpointBehaviour(ConfigurationManager.AppSettings["WSSEUserName"].ToString(),
                                            ConfigurationManager.AppSettings["WSSEPassword"].ToString(),
                                            modifyReservation.Username, modifyReservation.Password, modifyReservation.HotelDomain));
                }

                ReservationService.UniqueID uID = new ReservationService.UniqueID();
                uID.type = ReservationService.UniqueIDType.INTERNAL;
                uID.Value = modifyReservation.ModifyPackageRequest.ReservationNumber;
                updatePackageReq.HotelReference = new ReservationService.HotelReference()
                {
                    chainCode = modifyReservation.ChainCode,
                    hotelCode = modifyReservation.HotelDomain
                };
                updatePackageReq.ConfirmationNumber = uID;



                uID = new ReservationService.UniqueID();
                uID.type = ReservationService.UniqueIDType.INTERNAL;
                uID.source = "LEGNUMBER";
                uID.Value = modifyReservation.LegNumber;
                updatePackageReq.LegNumber = uID;

                updatePackageReq.ProductCode = modifyReservation.ModifyPackageRequest.ProductCode;
                updatePackageReq.Quantity = modifyReservation.ModifyPackageRequest.Quantity != null ? (int)modifyReservation.ModifyPackageRequest.Quantity : 1;
                updatePackageReq.QuantitySpecified = modifyReservation.ModifyPackageRequest.QuantitySpecified;

                updatePackageRes = ResSoapCLient.UpdatePackages(ref OGHeader, updatePackageReq);

                if (updatePackageRes.Result.resultStatusFlag == ReservationService.ResultStatusFlag.SUCCESS)
                {
                    return new Models.OWS.OwsResponseModel()
                    {
                        responseData = null,
                        result = true,
                        responseMessage = "Success"
                    };
                }
                else
                {
                    return new Models.OWS.OwsResponseModel()
                    {
                        responseData = null,
                        result = false,
                        responseMessage = "Failled to update package"
                    };
                }



            }
            catch (Exception ex)
            {
                return new Models.OWS.OwsResponseModel
                {
                    responseMessage = ex.ToString(),
                    statusCode = -1,
                    result = false
                };

            }
        }
        public Models.OWS.OwsResponseModel createAccompanyingGuset(Models.OWS.OwsRequestModel Request)
        {
            try
            {
                new LogHelper().Debug("Create accompany request : " + JsonConvert.SerializeObject(Request), Request.CreateAccompanyingProfileRequest.ReservationNumber, "createAccompanyingGuset", "API", "OWS");
                #region Request

                #region Request Header
                string temp = Helper.Helper.Get8Digits();
                ReservationService.OGHeader OGHeader = new ReservationService.OGHeader();
                OGHeader.transactionID = temp;
                OGHeader.timeStamp = DateTime.Now;
                OGHeader.primaryLangID = Request.Language; //English
                ReservationService.EndPoint orginEndPOint = new ReservationService.EndPoint();
                orginEndPOint.entityID = Request.KioskID; //Kiosk Identifier
                orginEndPOint.systemType = Request.SystemType;
                OGHeader.Origin = orginEndPOint;
                ReservationService.EndPoint destEndPOint = new ReservationService.EndPoint();
                destEndPOint.entityID = Request.DestinationEntityID;
                destEndPOint.systemType = Request.SystemType;
                OGHeader.Destination = destEndPOint;
                ReservationService.OGHeaderAuthentication Auth = new ReservationService.OGHeaderAuthentication();
                ReservationService.OGHeaderAuthenticationUserCredentials userCredentials = new ReservationService.OGHeaderAuthenticationUserCredentials();
                userCredentials.UserName = Request.Username;
                userCredentials.UserPassword = Request.Password;
                userCredentials.Domain = Request.HotelDomain;
                Auth.UserCredentials = userCredentials;
                OGHeader.Authentication = Auth;
                #endregion

                #region Request Body

                ReservationService.AddAccompanyGuestRequest AcmpnyRequest = new ReservationService.AddAccompanyGuestRequest();
                ReservationService.HotelReference HR = new ReservationService.HotelReference();
                HR.chainCode = Request.ChainCode;
                HR.hotelCode = Request.HotelDomain;

                AcmpnyRequest.HotelReference = HR;

                ReservationService.UniqueID uID = new ReservationService.UniqueID();
                uID.type = ReservationService.UniqueIDType.INTERNAL;
                uID.Value = Request.CreateAccompanyingProfileRequest.ReservationNumber;

                AcmpnyRequest.ConfirmationNumber = uID;

                ReservationService.UniqueID LID = new ReservationService.UniqueID();
                LID.type = ReservationService.UniqueIDType.INTERNAL;
                LID.Value = Request.LegNumber;

                AcmpnyRequest.LegNumber = LID;

                ReservationService.Profile AccompanyGuestProfile = new ReservationService.Profile();
                ReservationService.Customer AccompanyingCustomer = new ReservationService.Customer();
                if (!string.IsNullOrEmpty(Request.CreateAccompanyingProfileRequest.Gender))
                {
                    AccompanyingCustomer.gender = Request.CreateAccompanyingProfileRequest.Gender.ToLower().Equals("male") ? ReservationService.Gender.MALE : Request.CreateAccompanyingProfileRequest.Gender.ToLower().Equals("female") ? ReservationService.Gender.FEMALE : ReservationService.Gender.UNKNOWN;
                    AccompanyingCustomer.genderSpecified = true;
                }
                ReservationService.PersonName PName = new ReservationService.PersonName();
                PName.firstName = string.IsNullOrEmpty(Request.CreateAccompanyingProfileRequest.FirstName) ? " " : Request.CreateAccompanyingProfileRequest.FirstName;
                PName.lastName = Request.CreateAccompanyingProfileRequest.LastName;
                string[] middleName = { Request.CreateAccompanyingProfileRequest.MiddleName };
                PName.middleName = middleName;

                AccompanyingCustomer.PersonName = PName;
                
                AccompanyGuestProfile.Item = (ReservationService.Customer)AccompanyingCustomer;

                AcmpnyRequest.Profile = AccompanyGuestProfile;



                ReservationService.ReservationServiceSoapClient ResPortClient = new ReservationService.ReservationServiceSoapClient();
                bool isOperaCloudEnabled = false;
                isOperaCloudEnabled = (ConfigurationManager.AppSettings["OperaCloudEnabled"] != null
                                && !string.IsNullOrEmpty(ConfigurationManager.AppSettings["OperaCloudEnabled"].ToString())
                                && bool.TryParse(ConfigurationManager.AppSettings["OperaCloudEnabled"].ToString(), out isOperaCloudEnabled)) ? isOperaCloudEnabled : false;
                if (isOperaCloudEnabled)
                {
                    ResPortClient.Endpoint.Behaviors.Add(new Helper.CustomEndpointBehaviour(ConfigurationManager.AppSettings["WSSEUserName"].ToString(),
                                            ConfigurationManager.AppSettings["WSSEPassword"].ToString(),
                                            Request.Username, Request.Password, Request.HotelDomain));
                }
                //ResPortClient.Endpoint.Behaviors.Add(new Helper.CustomEndpointBehaviour("Test USE", "Request.WSSEPassword", "Request.KioskUserName", "Request.KioskPassword", "Request.HotelDomain"));
                ReservationService.AddAccompanyGuestResponse AcmpnyResponse = new ReservationService.AddAccompanyGuestResponse();
                AcmpnyResponse = ResPortClient.AddAccompanyGuest(ref OGHeader, AcmpnyRequest);
                if (AcmpnyResponse.Result.resultStatusFlag == ReservationService.ResultStatusFlag.SUCCESS)
                {
                    Request.FetchBookingRequest = new Models.OWS.FetchBookingRequestModel()
                    {
                        ReservationNumber = Request.CreateAccompanyingProfileRequest.ReservationNumber
                    };
                    Models.OWS.OwsResponseModel Booking = GetReservationDetailsFromPMS(Request);

                    if (Booking.result && Booking.responseData != null)
                    {
                        List<Models.OWS.OperaReservation> operaReservations = (List<Models.OWS.OperaReservation>)Booking.responseData;
                        if (operaReservations != null && operaReservations.Count > 0 && operaReservations[0].GuestProfiles != null && operaReservations[0].GuestProfiles.Count > 0)
                        {
                            foreach (Models.OWS.GuestProfile GuestProfile in operaReservations[0].GuestProfiles)
                            {
                                if (GuestProfile.LastName.ToUpper().Equals(Request.CreateAccompanyingProfileRequest.LastName.ToUpper()) && (!string.IsNullOrEmpty(Request.CreateAccompanyingProfileRequest.FirstName) ? GuestProfile.FirstName.ToUpper().Equals(Request.CreateAccompanyingProfileRequest.FirstName.ToUpper()) : true) && (!string.IsNullOrEmpty(Request.CreateAccompanyingProfileRequest.MiddleName) ? GuestProfile.MiddleName.ToUpper().Equals(Request.CreateAccompanyingProfileRequest.MiddleName.ToUpper()) : true))
                                {
                                    return new Models.OWS.OwsResponseModel()
                                    {
                                        responseData = GuestProfile,
                                        result = true
                                    };
                                }
                            }
                            return new Models.OWS.OwsResponseModel()
                            {
                                responseMessage = "Not able to find the created profile",
                                result = false
                            };
                        }
                        else
                        {
                            return new Models.OWS.OwsResponseModel()
                            {
                                responseMessage = "Not able to create the profile",
                                result = false
                            };
                        }
                    }
                    else
                    {
                        return new Models.OWS.OwsResponseModel()
                        {
                            responseMessage = "Not able to fetch the updated reservation",
                            result = false
                        };
                    }
                }
                else
                {
                    return new Models.OWS.OwsResponseModel()
                    {
                        responseMessage = AcmpnyResponse.Result.GDSError.Value,
                        result = false
                    };
                    
                }


                #endregion

                #endregion

            }
            catch (Exception ex)
            {
                return new Models.OWS.OwsResponseModel()
                {
                    responseMessage = ex.Message,
                    result = false
                };
            }
        }

        public Models.OWS.OwsResponseModel getFolioAsAList(Models.OWS.OwsRequestModel Request)
        {
            try
            {
               #region Request
                
                #region Request Header
                string temp = Helper.Helper.Get8Digits();
                ReservationAdvancedService.OGHeader OGHeader = new ReservationAdvancedService.OGHeader();
                OGHeader.transactionID = temp;
                OGHeader.timeStamp = DateTime.Now;
                OGHeader.primaryLangID = Request.Language; //English
                ReservationAdvancedService.EndPoint orginEndPOint = new ReservationAdvancedService.EndPoint();
                orginEndPOint.entityID = Request.KioskID; //Kiosk Identifier
                orginEndPOint.systemType = Request.SystemType;
                OGHeader.Origin = orginEndPOint;
                ReservationAdvancedService.EndPoint destEndPOint = new ReservationAdvancedService.EndPoint();
                destEndPOint.entityID = Request.DestinationEntityID;
                destEndPOint.systemType = Request.DestinationSystemType;
                OGHeader.Destination = destEndPOint;
                ReservationAdvancedService.OGHeaderAuthentication Auth = new ReservationAdvancedService.OGHeaderAuthentication();
                ReservationAdvancedService.OGHeaderAuthenticationUserCredentials userCredentials = new ReservationAdvancedService.OGHeaderAuthenticationUserCredentials();
                userCredentials.UserName = Request.Username;
                userCredentials.UserPassword = Request.Password;
                userCredentials.Domain = Request.HotelDomain;
                Auth.UserCredentials = userCredentials;
                OGHeader.Authentication = Auth;
                #endregion

                #region Request Body

                ReservationAdvancedService.InvoiceRequest InvRequest = new ReservationAdvancedService.InvoiceRequest();


                ReservationAdvancedService.ReservationRequestBase RB = new ReservationAdvancedService.ReservationRequestBase();

                ReservationAdvancedService.HotelReference HF = new ReservationAdvancedService.HotelReference();
                HF.chainCode = Request.ChainCode;
                HF.hotelCode = Request.HotelDomain;
                RB.HotelReference = HF;

                ReservationAdvancedService.UniqueID uID = new ReservationAdvancedService.UniqueID();
                uID.type = (ReservationAdvancedService.UniqueIDType)ReservationAdvancedService.UniqueIDType.EXTERNAL;
                uID.source = "RESV_NAME_ID";
                uID.Value = Request.FetchFolioRequest.ReservationNameID;
                ReservationAdvancedService.UniqueID[] UIDLIST = new ReservationAdvancedService.UniqueID[1];
                UIDLIST[0] = uID;
                RB.ReservationID = UIDLIST;
                InvRequest.ReservationRequest = RB;

                ReservationAdvancedService.ResvAdvancedServiceSoapClient ResAdvPortClient = new ReservationAdvancedService.ResvAdvancedServiceSoapClient();
                bool isOperaCloudEnabled = false;
                isOperaCloudEnabled = (ConfigurationManager.AppSettings["OperaCloudEnabled"] != null
                                && !string.IsNullOrEmpty(ConfigurationManager.AppSettings["OperaCloudEnabled"].ToString())
                                && bool.TryParse(ConfigurationManager.AppSettings["OperaCloudEnabled"].ToString(), out isOperaCloudEnabled)) ? isOperaCloudEnabled : false;
                if (isOperaCloudEnabled)
                {
                    ResAdvPortClient.Endpoint.Behaviors.Add(new Helper.CustomEndpointBehaviour(ConfigurationManager.AppSettings["WSSEUserName"].ToString(),
                                            ConfigurationManager.AppSettings["WSSEPassword"].ToString(),
                                            Request.Username, Request.Password, Request.HotelDomain));
                }
                ReservationAdvancedService.InvoiceResponse InvResponse = new ReservationAdvancedService.InvoiceResponse();

                InvResponse = ResAdvPortClient.Invoice(ref OGHeader, InvRequest);
                if (InvResponse.Result.resultStatusFlag == ReservationAdvancedService.ResultStatusFlag.SUCCESS)
                {
                    Models.OWS.FolioModel GFolio = new Models.OWS.FolioModel();
                    GFolio.IsAllowedForCheckOut = true;
                    if (InvResponse.Invoice != null)
                    {
                        List<Models.OWS.FolioWindow> FolioWindows = new List<FolioWindow>();
                        int InvoiceWindows = 0;
                        
                        foreach (ReservationAdvancedService.BillHeader Invoices in InvResponse.Invoice)
                        {
                            InvoiceWindows++;
                            if (Invoices.ProfileIDs != null)
                            {
                                foreach(ReservationAdvancedService.UniqueID uniqueID in Invoices.ProfileIDs)
                                {
                                    if (uniqueID.source != null && uniqueID.source.Equals("OPERA_NAME_ID"))
                                    {
                                        if (!string.IsNullOrEmpty(Request.FetchFolioRequest.ProfileID) && Request.FetchFolioRequest.ProfileID.Equals(uniqueID.Value))
                                        {
                                            FolioWindow folioWindow = new FolioWindow();
                                            
                                            
                                            ReservationAdvancedService.NativeName Name = new ReservationAdvancedService.NativeName();
                                            Name = Invoices.Name;
                                            ReservationAdvancedService.NameAddress FolioAddress = Invoices.Address;
                                            if (FolioAddress != null && string.IsNullOrEmpty(GFolio.AddressLine))
                                            {
                                                if (FolioAddress.AddressLine != null)
                                                {
                                                    foreach (string addrLine in FolioAddress.AddressLine)
                                                    {
                                                        if (!string.IsNullOrEmpty(GFolio.AddressLine))
                                                            GFolio.AddressLine += " " + addrLine;
                                                        else
                                                            GFolio.AddressLine = addrLine;
                                                    }
                                                    GFolio.City = FolioAddress.cityName;
                                                    GFolio.Country = FolioAddress.countryCode;
                                                    GFolio.PostalCode = FolioAddress.postalCode;
                                                }
                                            }
                                            GFolio.GuestName = Name.firstName != null ? Name.firstName : "";
                                            GFolio.GuestName += " " + Name.lastName;
                                            folioWindow.GuestName = GFolio.GuestName;
                                            folioWindow.PMSProfileID = uniqueID.Value;
                                            folioWindow.WindowNumber = InvoiceWindows;
                                            //ResAdvanced.Amount BalanceAmnt = Invoices.CurrentBalance;
                                            ReservationAdvancedService.UniqueID UId = new ReservationAdvancedService.UniqueID();
                                            UId = Invoices.BillNumber;
                                            GFolio.FolioNo = uID.Value;
                                            List<Models.OWS.FolioItemsModel> ListFolioItems = new List<Models.OWS.FolioItemsModel>();
                                            List< Models.OWS.FolioTaxItemsModel > ListFolioTaxItems = new List<Models.OWS.FolioTaxItemsModel>();
                                            if (Invoices.BillItems != null)
                                            {
                                                foreach (ReservationAdvancedService.BillItem BItems in Invoices.BillItems)
                                                {
                                                    Models.OWS.FolioItemsModel FolioItems = new Models.OWS.FolioItemsModel();
                                                    FolioItems.IsCredit = false;
                                                    FolioItems.WindowNumber = InvoiceWindows;
                                                    ReservationAdvancedService.Amount Amnt = new ReservationAdvancedService.Amount();
                                                    Amnt = BItems.Amount;
                                                    FolioItems.Amount = Amnt != null ? (decimal)Amnt.Value : 0;
                                                    FolioItems.TransactionCode = BItems.TransactionCode;
                                                    if (FolioItems.TransactionCode != null)
                                                    {
                                                        if (BItems.TransactionCode.Substring(0, 1).Equals("9"))
                                                        {
                                                            FolioItems.IsCredit = true;
                                                        }
                                                    }
                                                    FolioItems.ItemName = BItems.Description;
                                                    FolioItems.Date = BItems.Date;
                                                    if (GFolio.Items != null)
                                                    {
                                                        GFolio.Items.Add(FolioItems);
                                                    }
                                                    else
                                                    {
                                                        GFolio.Items = new List<FolioItemsModel>();
                                                        GFolio.Items.Add(FolioItems);
                                                    }
                                                    ListFolioItems.Add(FolioItems);
                                                }
                                                
                                            }
                                            if (Invoices.BillTaxes != null)
                                            {
                                                foreach (ReservationAdvancedService.BillTax BTaxItems in Invoices.BillTaxes)
                                                {
                                                    Models.OWS.FolioTaxItemsModel FolioTaxItems = new Models.OWS.FolioTaxItemsModel();
                                                   
                                                    FolioTaxItems.WindowNumber = InvoiceWindows;
                                                   
                                                    ReservationAdvancedService.Amount Amnt = new ReservationAdvancedService.Amount();
                                                    Amnt = BTaxItems.VatAmount;
                                                    FolioTaxItems.Amount = Amnt != null ? (decimal)Amnt.Value : 0;
                                                    
                                                    FolioTaxItems.ItemName = BTaxItems.Description;
                                                    
                                                    if (GFolio.TaxItems != null)
                                                        GFolio.TaxItems.Add(FolioTaxItems);
                                                    else
                                                    {
                                                        GFolio.TaxItems = new List<FolioTaxItemsModel>();
                                                        GFolio.TaxItems.Add(FolioTaxItems);
                                                    }
                                                    ListFolioTaxItems.Add(FolioTaxItems);
                                                }
                                                
                                            }
                                            folioWindow.Items = ListFolioItems;
                                            folioWindow.TaxItems = ListFolioTaxItems;
                                            try
                                            {
                                                ReservationAdvancedService.Amount Balance = new ReservationAdvancedService.Amount();
                                                Balance = Invoices.CurrentBalance;
                                                GFolio.BalanceAmount += Balance != null ? (decimal)Balance.Value : 0;
                                                GFolio.ReservationBalance += Balance != null ? (decimal)Balance.Value : 0;
                                                folioWindow.BalanceAmount = Balance != null ? (decimal)Balance.Value : 0;
                                            }
                                            catch (Exception ex)
                                            {
                                                //System.IO.File.AppendAllText(System.Web.Hosting.HostingEnvironment.MapPath(@"~\log.txt"), "Balance item error " + ex.ToString());
                                            }
                                            FolioWindows.Add(folioWindow);     
                                            if(!string.IsNullOrEmpty(ConfigurationManager.AppSettings["isFolioWindowControlled"]) && bool.Parse(ConfigurationManager.AppSettings["isFolioWindowControlled"])
                                                && !string.IsNullOrEmpty(ConfigurationManager.AppSettings["GuestFolioWindowNumber"]) && Int32.Parse(ConfigurationManager.AppSettings["GuestFolioWindowNumber"]) > 0)
                                            {
                                                if(Int32.Parse(ConfigurationManager.AppSettings["GuestFolioWindowNumber"]) <= InvoiceWindows)
                                                {
                                                    GFolio.FolioWindows = FolioWindows;

                                                    return new Models.OWS.OwsResponseModel()
                                                    {
                                                        result = true,
                                                        responseData = GFolio,
                                                        responseMessage = "Success"
                                                    };
                                                }
                                            }
                                        }
                                        else
                                        {
                                            ReservationAdvancedService.Amount Balance = new ReservationAdvancedService.Amount();
                                            Balance = Invoices.CurrentBalance;
                                            GFolio.ReservationBalance += Balance != null ? (decimal)Balance.Value : 0;
                                            if(Balance != null && (decimal)Balance.Value > 0)
                                                GFolio.IsAllowedForCheckOut = false;
                                        }

                                    }
                                    else
                                    {
                                        GFolio.IsAllowedForCheckOut = false;
                                    }
                                }
                            }
                        }
                        GFolio.FolioWindows = FolioWindows;

                        return new Models.OWS.OwsResponseModel()
                        {
                            result = true,
                            responseData = GFolio,
                            responseMessage = "Success"
                        };
                    }
                    else
                    {
                        return new Models.OWS.OwsResponseModel()
                        {
                            result = true,
                            responseData = GFolio,
                            responseMessage = "No transactions found"
                        };
                    }
                }
                else
                {
                    if (InvResponse.Result != null && InvResponse.Result.Text.Length > 0)
                    {
                        if (InvResponse.Result.Text[0].ToString().Contains("payrouting"))
                        {
                            return new Models.OWS.OwsResponseModel()
                            {
                                responseMessage = "Folio have payrouting",
                                statusCode = 101,
                                result = true
                            };
                        }

                    }
                    return new Models.OWS.OwsResponseModel()
                    {
                        responseMessage = "Get folio function failled",
                        statusCode = 1402,
                        result = false
                    };
                }


                #endregion

                #endregion
            }
            catch (Exception ex)
            {
                return new Models.OWS.OwsResponseModel()
                {
                    responseMessage = ex.Message,
                    statusCode = -1,
                    result = false
                };
            }
        }

       

        public Models.OWS.OwsResponseModel FetchFolio(Models.OWS.OwsRequestModel reservationRequest)
        {
            try
            {
                //Models.OWS.OwsResponseModel owsResponse = getFolioAsAList(reservationRequest);
                Models.OWS.FetchFolioRequest fetchFolioRequest = reservationRequest.FetchFolioRequest;
                if (fetchFolioRequest.FolioList == null )
                    // serviceResponse.IsFolioContains == false && //this condition need to be added once muhammed added
                    {
                    if (fetchFolioRequest != null && fetchFolioRequest.OperaReservation != null && fetchFolioRequest.OperaReservation.RateDetails != null)
                    {

                       
                        {

                            if (fetchFolioRequest.OperaReservation.RateDetails.DailyRates != null && fetchFolioRequest.OperaReservation.RateDetails.DailyRates.Count > 0)
                            {

                                ReportViewer rv = new Microsoft.Reporting.WebForms.ReportViewer();
                                rv.ProcessingMode = ProcessingMode.Local;
                                if (!System.IO.File.Exists(System.Web.Hosting.HostingEnvironment.MapPath("~/Resources/RDLC/FolioTemplate.rdlc")))
                                    return new Models.OWS.OwsResponseModel()
                                    {
                                        result = false,
                                        responseMessage = "RDLC file not found"
                                    };
                                rv.LocalReport.ReportPath = System.Web.Hosting.HostingEnvironment.MapPath("~/Resources/RDLC/FolioTemplate.rdlc");
                                string GSTRegNo = ConfigurationManager.AppSettings["GSTRegNo"].ToString();

                                if (Models.Global.GlobalModel.FolioXSDMemoryStream != null)
                                {
                                    try
                                    {

                                        var Nights = (fetchFolioRequest.OperaReservation.DepartureDate.Value.Date - fetchFolioRequest.OperaReservation.ArrivalDate.Value.Date).TotalDays;
                                        #region Private Variable
                                        decimal TotalAmount = 0;
                                        decimal TotalCredit = 0;
                                        decimal TaxAmount = 0;
                                        int group = 1;
                                        int itemCount = 0;
                                        #endregion
                                        Models.Global.GlobalModel.FolioXSDMemoryStream.Position = 0;
                                        XmlTextReader xtr = new XmlTextReader(Models.Global.GlobalModel.FolioXSDMemoryStream);
                                        DataSet ds = new DataSet();
                                        ds.ReadXmlSchema(xtr);
                                        DataTable folioItemsTable = new DataTable();
                                        DataTable ReportParameters = new DataTable();
                                        if (ds.Tables.Count == 2)
                                        {
                                            decimal totalAmount = 0;
                                            decimal totalCredit = 0;
                                            foreach (DataTable dataTable in ds.Tables)
                                            {
                                                if (dataTable.TableName.Equals("FolioItems"))
                                                {
                                                    if ((fetchFolioRequest.OperaReservation.DepartureDate.Value - fetchFolioRequest.OperaReservation.ArrivalDate.Value).TotalDays == 0)
                                                    {
                                                        if (fetchFolioRequest.OperaReservation.RateDetails != null && fetchFolioRequest.OperaReservation.RateDetails.DailyRates != null && fetchFolioRequest.OperaReservation.RateDetails.DailyRates.Count > 0)
                                                        {
                                                            foreach (var items in fetchFolioRequest.OperaReservation.RateDetails.DailyRates)
                                                            {
                                                                DataRow folioRow = dataTable.NewRow();
                                                                if (dataTable.Columns.Contains("Date"))
                                                                    folioRow["Date"] = items.PostingDate != null ? items.PostingDate.ToString("dd/MM/yyyy") : DateTime.Now.ToString("dd/MM/yyyy");
                                                                if (dataTable.Columns.Contains("Description"))
                                                                    folioRow["Description"] = items.description;
                                                                if (dataTable.Columns.Contains("AdditionalInformation"))
                                                                    folioRow["AdditionalInformation"] = "";

                                                                folioRow["Charges"] = items.Amount.ToString("0.00");
                                                                folioRow["Credits"] = "";
                                                                TotalAmount += items.Amount;
                                                                if (dataTable.Columns.Contains("ItemGroup"))
                                                                    folioRow["ItemGroup"] = group;



                                                                if (items.description.Contains("0195"))
                                                                {
                                                                    TaxAmount += items.Amount;
                                                                }
                                                                else
                                                                {
                                                                    folioRow["Charges"] = items.Amount.ToString("0.00");
                                                                    folioRow["Credits"] = "";
                                                                    //TotalAmount += items.Amount;
                                                                }
                                                                //folioRow["ItemGroup"] = 1;
                                                                dataTable.Rows.Add(folioRow);
                                                            }

                                                            folioItemsTable = dataTable;
                                                        }
                                                    }
                                                }
                                                else if (dataTable.TableName.Equals("FolioParameters"))
                                                {
                                                    #region Assign Values to DataRow

                                                    string FullAddress = "";
                                                    if (fetchFolioRequest.OperaReservation.GuestProfiles != null && fetchFolioRequest.OperaReservation.GuestProfiles.Count > 0 && fetchFolioRequest.OperaReservation.GuestProfiles[0].Address != null && fetchFolioRequest.OperaReservation.GuestProfiles[0].Address.Count > 0)
                                                    {
                                                        FullAddress = fetchFolioRequest.OperaReservation.GuestProfiles[0].FirstName + " " + fetchFolioRequest.OperaReservation.GuestProfiles[0].MiddleName + " " + fetchFolioRequest.OperaReservation.GuestProfiles[0].LastName + "\n";
                                                        FullAddress += !string.IsNullOrEmpty(fetchFolioRequest.OperaReservation.GuestProfiles[0].Address[0].address1) ? fetchFolioRequest.OperaReservation.GuestProfiles[0].Address[0].address1 : "" + "\n";
                                                        FullAddress += !string.IsNullOrEmpty(fetchFolioRequest.OperaReservation.GuestProfiles[0].Address[0].address2) ? fetchFolioRequest.OperaReservation.GuestProfiles[0].Address[0].address2 : "" + "\n";
                                                        FullAddress += !string.IsNullOrEmpty(fetchFolioRequest.OperaReservation.GuestProfiles[0].Address[0].city) ? fetchFolioRequest.OperaReservation.GuestProfiles[0].Address[0].city : "" + "\n";
                                                        FullAddress += !string.IsNullOrEmpty(fetchFolioRequest.OperaReservation.GuestProfiles[0].Address[0].country) ? fetchFolioRequest.OperaReservation.GuestProfiles[0].Address[0].country : "" + "\n";
                                                        FullAddress += !string.IsNullOrEmpty(fetchFolioRequest.OperaReservation.GuestProfiles[0].Address[0].zip) ? fetchFolioRequest.OperaReservation.GuestProfiles[0].Address[0].zip : "" + "\n";
                                                    }


                                                    DataRow ParameterRow = dataTable.NewRow();
                                                    if (dataTable.Columns.Contains("Address"))
                                                        ParameterRow["Address"] = FullAddress;

                                                    if (dataTable.Columns.Contains("RoomNo"))
                                                        ParameterRow["RoomNo"] = !string.IsNullOrEmpty(fetchFolioRequest.OperaReservation.RoomDetails.RoomNumber) ? fetchFolioRequest.OperaReservation.RoomDetails.RoomNumber : "";
                                                    if (dataTable.Columns.Contains("FolioNo"))
                                                        ParameterRow["FolioNo"] = "";
                                                    if (dataTable.Columns.Contains("GuestCount"))
                                                        ParameterRow["GuestCount"] = fetchFolioRequest.OperaReservation.Adults != null ? fetchFolioRequest.OperaReservation.Adults.ToString() : "0";
                                                    if (dataTable.Columns.Contains("ConfirmationNumber"))
                                                        ParameterRow["ConfirmationNo"] = !string.IsNullOrEmpty(fetchFolioRequest.OperaReservation.ReservationNumber) ? fetchFolioRequest.OperaReservation.ReservationNumber : "";
                                                    if (dataTable.Columns.Contains("CashierNo"))
                                                        ParameterRow["CashierNo"] = "KIOSK";
                                                    if (dataTable.Columns.Contains("GSTRegNo"))
                                                        ParameterRow["GSTRegNo"] = GSTRegNo;
                                                    if (dataTable.Columns.Contains("ArrivalDate"))
                                                        ParameterRow["ArrivalDate"] = fetchFolioRequest.OperaReservation.ArrivalDate != null ? fetchFolioRequest.OperaReservation.ArrivalDate.Value.ToString("dd/MM/yyyy") : "";
                                                    if (dataTable.Columns.Contains("DepartureDate"))
                                                        ParameterRow["DepartureDate"] = fetchFolioRequest.OperaReservation.DepartureDate != null ? fetchFolioRequest.OperaReservation.DepartureDate.Value.ToString("dd/MM/yyyy") : "";
                                                    if (dataTable.Columns.Contains("MembershipNo"))
                                                        ParameterRow["MembershipNo"] = (fetchFolioRequest.OperaReservation.GuestProfiles != null && fetchFolioRequest.OperaReservation.GuestProfiles.Count > 0) ? fetchFolioRequest.OperaReservation.GuestProfiles[0].MembershipNumber : "";
                                                    if (dataTable.Columns.Contains("BalanceDue"))
                                                        ParameterRow["BalanceDue"] = (TotalAmount).ToString("0.00");
                                                    if (dataTable.Columns.Contains("TotalBeforeGST"))
                                                        ParameterRow["TotalBeforeGST"] = (TotalAmount - TaxAmount).ToString("0.00");
                                                    if (dataTable.Columns.Contains("GST7Per"))
                                                        ParameterRow["GST7Per"] = TaxAmount.ToString("0.00");
                                                    if (dataTable.Columns.Contains("ZeroRatedSupplies"))
                                                        ParameterRow["ZeroRatedSupplies"] = "0.00";
                                                    if (dataTable.Columns.Contains("NonHotelSupplies"))
                                                        ParameterRow["NonHotelSupplies"] = "0.00";
                                                    if (dataTable.Columns.Contains("PaidoutCreditRefund"))
                                                        ParameterRow["PaidoutCreditRefund"] = "0.00";
                                                    if (dataTable.Columns.Contains("DepositSettlements"))
                                                        ParameterRow["DepositSettlements"] = totalCredit == 0 ? Math.Abs(totalCredit).ToString("0.00") : ""; ;
                                                    if (dataTable.Columns.Contains("Balance"))
                                                        ParameterRow["Balance"] = (TotalAmount).ToString("0.00");
                                                    if (dataTable.Columns.Contains("SignatureImagePath"))
                                                        ParameterRow["SignatureImagePath"] = string.IsNullOrEmpty(fetchFolioRequest.GuestSignature) ? "" : fetchFolioRequest.GuestSignature;
                                                    if (dataTable.Columns.Contains("TotalAmount"))
                                                        ParameterRow["TotalAmount"] = TotalAmount.ToString("0.00");
                                                    if (dataTable.Columns.Contains("TotalCredit"))
                                                        ParameterRow["TotalCredit"] = totalCredit != 0 ? Math.Abs(totalCredit).ToString("0.00") : "";
                                                    #endregion
                                                    dataTable.Rows.Add(ParameterRow);
                                                    ReportParameters = dataTable;
                                                }
                                            }

                                            if (folioItemsTable != null && ReportParameters != null && folioItemsTable.Rows.Count > 0
                                                && ReportParameters.Rows.Count > 0)
                                            {
                                                #region Assign Datasources
                                                ReportDataSource FolioItemsDatasource = new ReportDataSource();
                                                FolioItemsDatasource.Name = "DataSet1";
                                                FolioItemsDatasource.Value = folioItemsTable;

                                                ReportDataSource reportParameterDatasource = new ReportDataSource();
                                                reportParameterDatasource.Name = "DataSet2";
                                                reportParameterDatasource.Value = ReportParameters;

                                                rv.LocalReport.DataSources.Add(FolioItemsDatasource);
                                                rv.LocalReport.DataSources.Add(reportParameterDatasource);

                                                rv.LocalReport.EnableExternalImages = true;

                                                rv.LocalReport.Refresh();
                                                #endregion

                                                #region MyRegion
                                                byte[] streamBytes = null;
                                                string mimeType = "";
                                                string encoding = "";
                                                string filenameExtension = "";
                                                string[] streamids = null;
                                                Warning[] warnings = null;
                                                #endregion

                                                streamBytes = rv.LocalReport.Render("PDF", null, out mimeType, out encoding, out filenameExtension, out streamids, out warnings);
                                                string Base64Folio = Convert.ToBase64String(streamBytes);

                                                return new Models.OWS.OwsResponseModel()
                                                {
                                                    result = true,
                                                    responseData = Base64Folio,
                                                    responseMessage = "Success"
                                                };
                                            }
                                            else
                                            {
                                                return new Models.OWS.OwsResponseModel()
                                                {
                                                    result = false,
                                                    responseData = null,
                                                    responseMessage = "Not able to process the data set"
                                                };
                                            }
                                        }
                                        else
                                        {
                                            return new Models.OWS.OwsResponseModel()
                                            {
                                                result = false,
                                                responseData = null,
                                                responseMessage = "Invalid XSD format"
                                            };
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        return new Models.OWS.OwsResponseModel()
                                        {
                                            result = false,
                                            responseData = null,
                                            responseMessage = "Error while process the XSD file : - " + ex.ToString()
                                        };
                                    }
                                }
                                else
                                {

                                    #region Reservation Folio



                                    DataTable FolioItemsTable = new DataTable();

                                    #region DataTable Columns
                                    FolioItemsTable.Columns.Add("Date", typeof(string));
                                    FolioItemsTable.Columns.Add("Description", typeof(string));
                                    FolioItemsTable.Columns.Add("AdditionalInformation", typeof(string));
                                    FolioItemsTable.Columns.Add("Charges", typeof(string));
                                    FolioItemsTable.Columns.Add("Credits", typeof(string));
                                    FolioItemsTable.Columns.Add("ItemGroup", typeof(string));
                                    //FolioItemsTable.Columns.Add("Quantity", typeof(Int32));
                                    #endregion

                                    #region Private Variable
                                    decimal TotalAmount = 0;
                                    decimal TotalCredit = 0;
                                    decimal TaxAmount = 0;
                                    int group = 1;
                                    int itemCount = 0;
                                    #endregion

                                    var Nights = (fetchFolioRequest.OperaReservation.DepartureDate.Value.Date - fetchFolioRequest.OperaReservation.ArrivalDate.Value.Date).TotalDays;

                                    if (Nights < 1)
                                    {
                                        foreach (var items in fetchFolioRequest.OperaReservation.RateDetails.DailyRates)
                                        {
                                            //if (!items.IsTaxAmount)
                                            {
                                                itemCount++;
                                                if (itemCount % 8 == 0)
                                                {
                                                    group += 1;
                                                }
                                                DataRow folioRow = FolioItemsTable.NewRow();
                                                if (FolioItemsTable.Columns.Contains("Date"))
                                                    folioRow["Date"] = items.PostingDate != null ? items.PostingDate.ToString("dd/MM/yyyy") : DateTime.Now.ToString("dd/MM/yyyy");
                                                if (FolioItemsTable.Columns.Contains("Description"))
                                                    folioRow["Description"] = items.description;
                                                if (FolioItemsTable.Columns.Contains("AdditionalInformation"))
                                                    folioRow["AdditionalInformation"] = "";
                                                folioRow["Charges"] = items.Amount.ToString("0.00");
                                                folioRow["Credits"] = "";
                                                TotalAmount += items.Amount;
                                                if (FolioItemsTable.Columns.Contains("ItemGroup"))
                                                    folioRow["ItemGroup"] = group;

                                                FolioItemsTable.Rows.Add(folioRow);

                                                if (items.description.Contains("0195"))
                                                {
                                                    TaxAmount += items.Amount;
                                                }
                                                else
                                                {
                                                    folioRow["Charges"] = items.Amount.ToString("0.00");
                                                    folioRow["Credits"] = "";
                                                    //TotalAmount += items.Amount;
                                                }
                                            }
                                        }
                                    }


                                    string FullAddress = "";
                                    if (fetchFolioRequest.OperaReservation.GuestProfiles != null && fetchFolioRequest.OperaReservation.GuestProfiles.Count > 0 && fetchFolioRequest.OperaReservation.GuestProfiles[0].Address != null && fetchFolioRequest.OperaReservation.GuestProfiles[0].Address.Count > 0)
                                    {
                                        FullAddress = fetchFolioRequest.OperaReservation.GuestProfiles[0].FirstName + " " + fetchFolioRequest.OperaReservation.GuestProfiles[0].MiddleName + " " + fetchFolioRequest.OperaReservation.GuestProfiles[0].LastName + "\n";
                                        FullAddress += !string.IsNullOrEmpty(fetchFolioRequest.OperaReservation.GuestProfiles[0].Address[0].address1) ? fetchFolioRequest.OperaReservation.GuestProfiles[0].Address[0].address1 : "" + "\n";
                                        FullAddress += !string.IsNullOrEmpty(fetchFolioRequest.OperaReservation.GuestProfiles[0].Address[0].address2) ? fetchFolioRequest.OperaReservation.GuestProfiles[0].Address[0].address2 : "" + "\n";
                                        FullAddress += !string.IsNullOrEmpty(fetchFolioRequest.OperaReservation.GuestProfiles[0].Address[0].city) ? fetchFolioRequest.OperaReservation.GuestProfiles[0].Address[0].city : "" + "\n";
                                        FullAddress += !string.IsNullOrEmpty(fetchFolioRequest.OperaReservation.GuestProfiles[0].Address[0].country) ? fetchFolioRequest.OperaReservation.GuestProfiles[0].Address[0].country : "" + "\n";
                                        FullAddress += !string.IsNullOrEmpty(fetchFolioRequest.OperaReservation.GuestProfiles[0].Address[0].zip) ? fetchFolioRequest.OperaReservation.GuestProfiles[0].Address[0].zip : "" + "\n";
                                    }

                                    DataTable reportParameters = new DataTable();

                                    #region Report Parameter Columns
                                    reportParameters.Columns.Add("Address", typeof(string));
                                    reportParameters.Columns.Add("RoomNo", typeof(string));
                                    reportParameters.Columns.Add("FolioNo", typeof(string));
                                    reportParameters.Columns.Add("ConfirmationNo", typeof(string));
                                    reportParameters.Columns.Add("CashierNo", typeof(string));
                                    reportParameters.Columns.Add("GSTRegNo", typeof(string));
                                    reportParameters.Columns.Add("ArrivalDate", typeof(string));
                                    reportParameters.Columns.Add("DepartureDate", typeof(string));
                                    reportParameters.Columns.Add("MembershipNo", typeof(string));
                                    reportParameters.Columns.Add("BalanceDue", typeof(string));
                                    reportParameters.Columns.Add("TotalBeforeGST", typeof(string));
                                    reportParameters.Columns.Add("GST7Per", typeof(string));
                                    reportParameters.Columns.Add("ZeroRatedSupplies", typeof(string));
                                    reportParameters.Columns.Add("NonHotelSupplies", typeof(string));
                                    reportParameters.Columns.Add("PaidoutCreditRefund", typeof(string));
                                    reportParameters.Columns.Add("DepositSettlements", typeof(string));
                                    reportParameters.Columns.Add("Balance", typeof(string));
                                    reportParameters.Columns.Add("SignatureImagePath", typeof(string));
                                    reportParameters.Columns.Add("TotalAmount", typeof(string));
                                    reportParameters.Columns.Add("TotalCredit", typeof(string));


                                    #endregion

                                    #region Signature Image
                                    string signatureImage = "";
                                    //if (!string.IsNullOrEmpty(fetchFolioRequest.GuestSignature))
                                    //{
                                    //    //string SignatureImageFileName = "Folio_" + fetchFolioRequest.ReservationNameID + ".jpeg";
                                    //    byte[] bytes = Convert.FromBase64String(fetchFolioRequest.GuestSignature);
                                    //    using (MemoryStream ms = new MemoryStream(bytes))
                                    //    {
                                    //        System.Drawing.Image.FromStream(ms).Save(System.Web.Hosting.HostingEnvironment.MapPath("~/Resources/Images/Temp/" + SignatureImageFileName));
                                    //    }
                                    //    signatureImage = new Uri(System.Web.Hosting.HostingEnvironment.MapPath("~/Resources/Images/Temp/" + SignatureImageFileName)).AbsoluteUri;
                                    //}
                                    #endregion

                                    DataRow parameterRow = reportParameters.NewRow();

                                    #region Assign Values to DataRow
                                    parameterRow["Address"] = FullAddress;
                                    parameterRow["RoomNo"] = fetchFolioRequest.OperaReservation.RoomDetails.RoomNumber;
                                    parameterRow["FolioNo"] = "";
                                    parameterRow["ConfirmationNo"] = fetchFolioRequest.OperaReservation.ReservationNumber;
                                    parameterRow["CashierNo"] = "KIOSK";
                                    parameterRow["GSTRegNo"] = GSTRegNo;
                                    parameterRow["ArrivalDate"] = fetchFolioRequest.OperaReservation.ArrivalDate != null ? fetchFolioRequest.OperaReservation.ArrivalDate.Value.ToString("dd/MM/yyyy") : "";
                                    parameterRow["DepartureDate"] = fetchFolioRequest.OperaReservation.DepartureDate != null ? fetchFolioRequest.OperaReservation.DepartureDate.Value.ToString("dd/MM/yyyy") : "";
                                    parameterRow["MembershipNo"] = fetchFolioRequest.OperaReservation.GuestProfiles[0].MembershipNumber;
                                    parameterRow["BalanceDue"] = (TotalAmount).ToString("0.00");
                                    parameterRow["TotalBeforeGST"] = (TotalAmount - TaxAmount).ToString("0.00");
                                    parameterRow["GST7Per"] = ((7 / 100) * TotalAmount).ToString("0.00");
                                    parameterRow["ZeroRatedSupplies"] = "0.00";
                                    parameterRow["NonHotelSupplies"] = "0.00";
                                    parameterRow["PaidoutCreditRefund"] = "0.00";
                                    parameterRow["DepositSettlements"] = TotalCredit == 0 ? Math.Abs(TotalCredit).ToString("0.00") : "";
                                    parameterRow["Balance"] = (TotalAmount).ToString("0.00");
                                    parameterRow["SignatureImagePath"] = string.IsNullOrEmpty(fetchFolioRequest.GuestSignature) ? "" : fetchFolioRequest.GuestSignature;
                                    parameterRow["TotalAmount"] = (TotalAmount).ToString("0.00");
                                    parameterRow["TotalCredit"] = TotalCredit != 0 ? Math.Abs(TotalCredit).ToString("0.00") : "";


                                    #endregion

                                    reportParameters.Rows.Add(parameterRow);

                                    #region Assign Datasources
                                    ReportDataSource FolioItemsDatasource = new ReportDataSource();
                                    FolioItemsDatasource.Name = "DataSet1";
                                    FolioItemsDatasource.Value = FolioItemsTable;

                                    ReportDataSource reportParameterDatasource = new ReportDataSource();
                                    reportParameterDatasource.Name = "DataSet2";
                                    reportParameterDatasource.Value = reportParameters;

                                    rv.LocalReport.DataSources.Add(FolioItemsDatasource);
                                    rv.LocalReport.DataSources.Add(reportParameterDatasource);

                                    rv.LocalReport.EnableExternalImages = true;

                                    rv.LocalReport.Refresh();
                                    #endregion

                                    #region MyRegion
                                    byte[] streamBytes = null;
                                    string mimeType = "";
                                    string encoding = "";
                                    string filenameExtension = "";
                                    string[] streamids = null;
                                    Warning[] warnings = null;
                                    #endregion

                                    streamBytes = rv.LocalReport.Render("PDF", null, out mimeType, out encoding, out filenameExtension, out streamids, out warnings);
                                    string Base64Folio = Convert.ToBase64String(streamBytes);

                                    return new Models.OWS.OwsResponseModel()
                                    {
                                        result = true,
                                        responseData = Base64Folio,
                                        responseMessage = "Success",
                                        statusCode = 10001
                                    };


                                    #endregion
                                }
                            }
                            else
                            {
                                return new Models.OWS.OwsResponseModel()
                                {
                                    result = false,
                                    responseMessage = "Unable to get folio",
                                    statusCode = 10004,
                                };
                            }
                        }
                        
                    }
                    else
                    {
                        return new Models.OWS.OwsResponseModel()
                        {
                            result = false,
                            responseMessage = "Unable to get folio",
                            statusCode = 10004,
                        };
                    }
                }
                else
                {
                    #region Normal Folio

                    ReportViewer rv = new Microsoft.Reporting.WebForms.ReportViewer();
                    rv.ProcessingMode = ProcessingMode.Local;
                    if (!System.IO.File.Exists(System.Web.Hosting.HostingEnvironment.MapPath("~/Resources/RDLC/FolioTemplate.rdlc")))
                        return new Models.OWS.OwsResponseModel()
                        {
                            result = false,
                            responseMessage = "RDLC file not found"
                        };
                    rv.LocalReport.ReportPath = System.Web.Hosting.HostingEnvironment.MapPath("~/Resources/RDLC/FolioTemplate.rdlc");
                    


                    string GSTRegNo = ConfigurationManager.AppSettings["GSTRegNo"].ToString();
                    Models.OWS.FolioModel GuestFolio = fetchFolioRequest.FolioList;
                    decimal TaxAmount = 0;
                    if (GuestFolio.TaxItems != null && GuestFolio.TaxItems.Count() > 0)
                    {
                        foreach (var taxItem in GuestFolio.TaxItems)
                        {
                            TaxAmount += taxItem.Amount;
                        }
                    }

                    if (Models.Global.GlobalModel.FolioXSDMemoryStream != null)
                    {
                        try
                        {
                            Models.Global.GlobalModel.FolioXSDMemoryStream.Position = 0;
                            XmlTextReader xtr = new XmlTextReader(Models.Global.GlobalModel.FolioXSDMemoryStream);
                            DataSet ds = new DataSet();
                            ds.ReadXmlSchema(xtr);
                            DataTable folioItemsTable = new DataTable();
                            DataTable ReportParameters = new DataTable();
                            if (ds.Tables.Count == 2)
                            {
                                decimal totalAmount = 0;
                                decimal totalCredit = 0;
                                foreach (DataTable dataTable in ds.Tables)
                                {
                                    if (dataTable.TableName.Equals("FolioItems"))
                                    {

                                        if (GuestFolio != null && GuestFolio.Items != null && GuestFolio.Items.Count > 0)
                                        {
                                            foreach (var items in GuestFolio.Items)
                                            {
                                                DataRow folioRow = dataTable.NewRow();
                                                if (dataTable.Columns.Contains("Date"))
                                                    folioRow["Date"] = items.Date != null ? items.Date.Value.ToString("dd/MM/yyyy") : DateTime.Now.ToString("dd/MM/yyyy");
                                                if (dataTable.Columns.Contains("Description"))
                                                    folioRow["Description"] = items.ItemName;
                                                if (dataTable.Columns.Contains("AdditionalInformation"))
                                                    folioRow["AdditionalInformation"] = "";

                                                if (items.IsCredit)
                                                {
                                                    if (dataTable.Columns.Contains("Credits"))
                                                        folioRow["Credits"] = items.Amount.ToString("0.00");
                                                    if (dataTable.Columns.Contains("Charges"))
                                                        folioRow["Charges"] = "";
                                                    totalCredit += items.Amount;
                                                }
                                                else
                                                {
                                                    if (dataTable.Columns.Contains("Charges"))
                                                        folioRow["Charges"] = items.Amount.ToString("0.00");
                                                    if (dataTable.Columns.Contains("Credits"))
                                                        folioRow["Credits"] = "";
                                                    totalAmount += items.Amount;
                                                }
                                                if (dataTable.Columns.Contains("ItemGroup"))
                                                    folioRow["ItemGroup"] = 1;
                                                dataTable.Rows.Add(folioRow);
                                            }

                                            folioItemsTable = dataTable;
                                        }
                                    }
                                    else if (dataTable.TableName.Equals("FolioParameters"))
                                    {

                                        #region Assign Values to DataRow

                                        string fullAddress = "";
                                        fullAddress = GuestFolio.GuestName + "\n";
                                        fullAddress = fullAddress + GuestFolio.AddressLine + "\n";
                                        fullAddress = fullAddress + GuestFolio.City + "\n";
                                        fullAddress = fullAddress + GuestFolio.Country + "\n";
                                        fullAddress = fullAddress + GuestFolio.PostalCode + "\n";

                                        #region calculation
                                        string GST = (totalAmount - (totalAmount / (decimal)1.07)).ToString("0.00");
                                        string ServiceCharge = ((totalAmount / (decimal)1.07) - ((totalAmount / (decimal)1.07) / (decimal)1.10)).ToString("0.00");
                                        string amountBeforeGST = (totalAmount - (totalAmount - (totalAmount / (decimal)1.07))).ToString("0.00");
                                        #endregion

                                        DataRow ParameterRow = dataTable.NewRow();
                                        if (dataTable.Columns.Contains("Address"))
                                            ParameterRow["Address"] = fullAddress;

                                        if (dataTable.Columns.Contains("RoomNo"))
                                            ParameterRow["RoomNo"] = !string.IsNullOrEmpty(fetchFolioRequest.OperaReservation.RoomDetails.RoomNumber) ? fetchFolioRequest.OperaReservation.RoomDetails.RoomNumber : "";
                                        if (dataTable.Columns.Contains("FolioNo"))
                                            ParameterRow["FolioNo"] = "";
                                        if (dataTable.Columns.Contains("ConfirmationNumber"))
                                            ParameterRow["ConfirmationNumber"] = !string.IsNullOrEmpty(fetchFolioRequest.OperaReservation.ReservationNumber) ? fetchFolioRequest.OperaReservation.ReservationNumber : "";
                                        if (dataTable.Columns.Contains("GuestCount"))
                                            ParameterRow["GuestCount"] = fetchFolioRequest.OperaReservation.Adults != null ? fetchFolioRequest.OperaReservation.Adults.ToString() : "0";
                                        if (dataTable.Columns.Contains("CashierNo"))
                                            ParameterRow["CashierNo"] = "KIOSK";
                                        if (dataTable.Columns.Contains("GSTRegNo"))
                                            ParameterRow["GSTRegNo"] = GSTRegNo;
                                        if (dataTable.Columns.Contains("ArrivalDate"))
                                            ParameterRow["ArrivalDate"] = fetchFolioRequest.OperaReservation.ArrivalDate != null ? fetchFolioRequest.OperaReservation.ArrivalDate.Value.ToString("dd/MM/yyyy") : "";
                                        if (dataTable.Columns.Contains("DepartureDate"))
                                            ParameterRow["DepartureDate"] = fetchFolioRequest.OperaReservation.DepartureDate != null ? fetchFolioRequest.OperaReservation.DepartureDate.Value.ToString("dd/MM/yyyy") : "";
                                        if (dataTable.Columns.Contains("MembershipNo"))
                                            ParameterRow["MembershipNo"] = (fetchFolioRequest.OperaReservation.GuestProfiles != null && fetchFolioRequest.OperaReservation.GuestProfiles.Count > 0) ? fetchFolioRequest.OperaReservation.GuestProfiles[0].MembershipNumber : "";
                                        if (dataTable.Columns.Contains("BalanceDue"))
                                            ParameterRow["BalanceDue"] = GuestFolio.BalanceAmount.ToString("0.00");
                                        if (dataTable.Columns.Contains("TotalBeforeGST"))
                                            ParameterRow["TotalBeforeGST"] = (totalAmount - (totalAmount - (totalAmount / (decimal)1.07))).ToString("0.00");
                                        if (dataTable.Columns.Contains("GST7Per"))
                                            ParameterRow["GST7Per"] = GST;//TaxAmount.ToString("0.00");
                                        if (dataTable.Columns.Contains("ZeroRatedSupplies"))
                                            ParameterRow["ZeroRatedSupplies"] = ServiceCharge; 
                                        if (dataTable.Columns.Contains("NonHotelSupplies"))
                                            ParameterRow["NonHotelSupplies"] = "0.00";
                                        if (dataTable.Columns.Contains("PaidoutCreditRefund"))
                                            ParameterRow["PaidoutCreditRefund"] = "0.00";
                                        if (dataTable.Columns.Contains("DepositSettlements"))
                                            ParameterRow["DepositSettlements"] = totalCredit == 0 ? Math.Abs(totalCredit).ToString("0.00") : ""; ;
                                        if (dataTable.Columns.Contains("Balance"))
                                            ParameterRow["Balance"] = GuestFolio.BalanceAmount.ToString("0.00");
                                        if (dataTable.Columns.Contains("SignatureImagePath"))
                                            ParameterRow["SignatureImagePath"] = string.IsNullOrEmpty(fetchFolioRequest.GuestSignature) ? "" : fetchFolioRequest.GuestSignature;
                                        if (dataTable.Columns.Contains("TotalAmount"))
                                            ParameterRow["TotalAmount"] = totalAmount.ToString("0.00");
                                        if (dataTable.Columns.Contains("TotalCredit"))
                                            ParameterRow["TotalCredit"] = totalCredit != 0 ? Math.Abs(totalCredit).ToString("0.00") : "";
                                        #endregion

                                        dataTable.Rows.Add(ParameterRow);
                                        ReportParameters = dataTable;
                                    }
                                }

                                if (folioItemsTable != null && ReportParameters != null && folioItemsTable.Rows.Count > 0
                                    && ReportParameters.Rows.Count > 0)
                                {
                                    #region Assign Datasources
                                    ReportDataSource FolioItemsDatasource = new ReportDataSource();
                                    FolioItemsDatasource.Name = "DataSet1";
                                    FolioItemsDatasource.Value = folioItemsTable;

                                    ReportDataSource reportParameterDatasource = new ReportDataSource();
                                    reportParameterDatasource.Name = "DataSet2";
                                    reportParameterDatasource.Value = ReportParameters;

                                    rv.LocalReport.DataSources.Add(FolioItemsDatasource);
                                    rv.LocalReport.DataSources.Add(reportParameterDatasource);

                                    rv.LocalReport.EnableExternalImages = true;

                                    rv.LocalReport.Refresh();
                                    #endregion

                                    #region MyRegion
                                    byte[] streamBytes = null;
                                    string mimeType = "";
                                    string encoding = "";
                                    string filenameExtension = "";
                                    string[] streamids = null;
                                    Warning[] warnings = null;
                                    #endregion

                                    streamBytes = rv.LocalReport.Render("PDF", null, out mimeType, out encoding, out filenameExtension, out streamids, out warnings);
                                    string Base64Folio = Convert.ToBase64String(streamBytes);

                                    return new Models.OWS.OwsResponseModel()
                                    {
                                        result = true,
                                        responseData = Base64Folio,
                                        responseMessage = "Success"
                                    };
                                }
                                else
                                {
                                    return new Models.OWS.OwsResponseModel()
                                    {
                                        result = false,
                                        responseData = null,
                                        responseMessage = "Not able to process the data set"
                                    };
                                }
                            }
                            else
                            {
                                return new Models.OWS.OwsResponseModel()
                                {
                                    result = false,
                                    responseData = null,
                                    responseMessage = "Invalid XSD format"
                                };
                            }
                        }
                        catch(Exception ex)
                        {
                            return new Models.OWS.OwsResponseModel()
                            {
                                result = false,
                                responseData = null,
                                responseMessage = "Error while process the XSD file : - " + ex.ToString()
                            };
                        }
                    }
                    else
                    {
                        #region Old method  

                        DataTable FolioItemsTable = new DataTable();

                        FolioItemsTable.Columns.Add("Date", typeof(string));
                        FolioItemsTable.Columns.Add("Description", typeof(string));
                        FolioItemsTable.Columns.Add("AdditionalInformation", typeof(string));
                        FolioItemsTable.Columns.Add("Charges", typeof(string));
                        FolioItemsTable.Columns.Add("Credits", typeof(string));
                        FolioItemsTable.Columns.Add("ItemGroup", typeof(string));
                        decimal TotalAmount = 0;
                        decimal TotalCredit = 0;

                        if (GuestFolio != null && GuestFolio.Items != null && GuestFolio.Items.Count > 0)
                        {
                            foreach (var items in GuestFolio.Items)
                            {
                                DataRow folioRow = FolioItemsTable.NewRow();
                                folioRow["Date"] = items.Date != null ? items.Date.Value.ToString("dd/MM/yyyy") : DateTime.Now.ToString("dd/MM/yyyy");
                                folioRow["Description"] = items.ItemName;
                                folioRow["AdditionalInformation"] = "";

                                if (items.IsCredit)
                                {
                                    folioRow["Credits"] = items.Amount.ToString();
                                    folioRow["Charges"] = "";
                                    TotalCredit += items.Amount;
                                }
                                else
                                {
                                    folioRow["Charges"] = items.Amount.ToString("0.00");
                                    folioRow["Credits"] = "";
                                    TotalAmount += items.Amount;
                                }


                                folioRow["ItemGroup"] = 1;
                                FolioItemsTable.Rows.Add(folioRow);


                            }
                        }
                        
                        string FullAddress = "";
                        FullAddress = GuestFolio.GuestName + "\n";
                        FullAddress = FullAddress + GuestFolio.AddressLine + "\n";
                        FullAddress = FullAddress + GuestFolio.City + "\n";
                        FullAddress = FullAddress + GuestFolio.Country + "\n";
                        FullAddress = FullAddress + GuestFolio.PostalCode + "\n";

                        DataTable reportParameters = new DataTable();

                        #region Report Parameter Columns
                        reportParameters.Columns.Add("Address", typeof(string));
                        reportParameters.Columns.Add("RoomNo", typeof(string));
                        reportParameters.Columns.Add("FolioNo", typeof(string));
                        reportParameters.Columns.Add("ConfirmationNo", typeof(string));
                        reportParameters.Columns.Add("CashierNo", typeof(string));
                        reportParameters.Columns.Add("GSTRegNo", typeof(string));
                        reportParameters.Columns.Add("ArrivalDate", typeof(string));
                        reportParameters.Columns.Add("DepartureDate", typeof(string));
                        reportParameters.Columns.Add("MembershipNo", typeof(string));
                        reportParameters.Columns.Add("BalanceDue", typeof(string));
                        reportParameters.Columns.Add("TotalBeforeGST", typeof(string));
                        reportParameters.Columns.Add("GST7Per", typeof(string));
                        reportParameters.Columns.Add("ZeroRatedSupplies", typeof(string));
                        reportParameters.Columns.Add("NonHotelSupplies", typeof(string));
                        reportParameters.Columns.Add("PaidoutCreditRefund", typeof(string));
                        reportParameters.Columns.Add("DepositSettlements", typeof(string));
                        reportParameters.Columns.Add("Balance", typeof(string));
                        reportParameters.Columns.Add("SignatureImagePath", typeof(string));
                        reportParameters.Columns.Add("TotalAmount", typeof(string));
                        reportParameters.Columns.Add("TotalCredit", typeof(string));
                        
                        #endregion



                        #region Signature Image
                        string signatureImage = "";
                        #endregion


                        DataRow parameterRow = reportParameters.NewRow();

                        #region Assign Values to DataRow
                        parameterRow["Address"] = FullAddress;


                        parameterRow["RoomNo"] = !string.IsNullOrEmpty(fetchFolioRequest.OperaReservation.RoomDetails.RoomNumber) ? fetchFolioRequest.OperaReservation.RoomDetails.RoomNumber : "";
                        parameterRow["FolioNo"] = "";
                        parameterRow["ConfirmationNo"] = !string.IsNullOrEmpty(fetchFolioRequest.OperaReservation.ReservationNumber) ? fetchFolioRequest.OperaReservation.ReservationNumber : "";
                        parameterRow["CashierNo"] = "KIOSK";
                        parameterRow["GSTRegNo"] = GSTRegNo;
                        parameterRow["ArrivalDate"] = fetchFolioRequest.OperaReservation.ArrivalDate != null ? fetchFolioRequest.OperaReservation.ArrivalDate.Value.ToString("dd/MM/yyyy") : "";
                        parameterRow["DepartureDate"] = fetchFolioRequest.OperaReservation.DepartureDate != null ? fetchFolioRequest.OperaReservation.DepartureDate.Value.ToString("dd/MM/yyyy") : "";
                        parameterRow["MembershipNo"] = (fetchFolioRequest.OperaReservation.GuestProfiles != null && fetchFolioRequest.OperaReservation.GuestProfiles.Count > 0) ? fetchFolioRequest.OperaReservation.GuestProfiles[0].MembershipNumber : "";
                        parameterRow["BalanceDue"] = GuestFolio.BalanceAmount.ToString("0.00");
                        parameterRow["TotalBeforeGST"] = (TotalAmount - TaxAmount).ToString("0.00");
                        parameterRow["GST7Per"] = TaxAmount.ToString("0.00");
                        parameterRow["ZeroRatedSupplies"] = "0.00";
                        parameterRow["NonHotelSupplies"] = "0.00";
                        parameterRow["PaidoutCreditRefund"] = "0.00";
                        parameterRow["DepositSettlements"] = TotalCredit == 0 ? Math.Abs(TotalCredit).ToString("0.00") : ""; ;
                        parameterRow["Balance"] = GuestFolio.BalanceAmount.ToString("0.00");
                        parameterRow["SignatureImagePath"] = string.IsNullOrEmpty(fetchFolioRequest.GuestSignature) ? "" : fetchFolioRequest.GuestSignature;
                        parameterRow["TotalAmount"] = TotalAmount.ToString("0.00");
                        parameterRow["TotalCredit"] = TotalCredit != 0 ? Math.Abs(TotalCredit).ToString("0.00") : "";
                        #endregion

                        reportParameters.Rows.Add(parameterRow);

                        #region Assign Datasources
                        ReportDataSource FolioItemsDatasource = new ReportDataSource();
                        FolioItemsDatasource.Name = "DataSet1";
                        FolioItemsDatasource.Value = FolioItemsTable;

                        ReportDataSource reportParameterDatasource = new ReportDataSource();
                        reportParameterDatasource.Name = "DataSet2";
                        reportParameterDatasource.Value = reportParameters;

                        rv.LocalReport.DataSources.Add(FolioItemsDatasource);
                        rv.LocalReport.DataSources.Add(reportParameterDatasource);

                        rv.LocalReport.EnableExternalImages = true;

                        rv.LocalReport.Refresh();
                        #endregion

                        #region MyRegion
                        byte[] streamBytes = null;
                        string mimeType = "";
                        string encoding = "";
                        string filenameExtension = "";
                        string[] streamids = null;
                        Warning[] warnings = null;
                        #endregion

                        streamBytes = rv.LocalReport.Render("PDF", null, out mimeType, out encoding, out filenameExtension, out streamids, out warnings);
                        string Base64Folio = Convert.ToBase64String(streamBytes);

                        return new Models.OWS.OwsResponseModel()
                        {
                            result = true,
                            responseData = Base64Folio,
                            responseMessage = "Success"
                        };
                        #endregion
                    }

                    #endregion
                }
            }
            catch (Exception err)
            {
                return new Models.OWS.OwsResponseModel()
                {
                    result = false,
                    responseMessage = err.ToString()                    
                };
            }
        }

        Models.OWS.OwsResponseModel GetPassport(Models.OWS.OwsRequestModel Request)
        {
            try
            {

                #region Request Header
                string temp = Helper.Helper.Get8Digits();
                NameService.OGHeader OGHeader = new NameService.OGHeader();
                OGHeader.transactionID = temp;
                OGHeader.timeStamp = DateTime.Now;
                OGHeader.primaryLangID = Request.Language; //English
                NameService.EndPoint orginEndPOint = new NameService.EndPoint();
                orginEndPOint.entityID = Request.KioskID; //Kiosk Identifier
                orginEndPOint.systemType = Request.SystemType;
                OGHeader.Origin = orginEndPOint;
                NameService.EndPoint destEndPOint = new NameService.EndPoint();
                destEndPOint.entityID = Request.DestinationEntityID;
                destEndPOint.systemType =Request.DestinationSystemType;
                OGHeader.Destination = destEndPOint;
                NameService.OGHeaderAuthentication Auth = new NameService.OGHeaderAuthentication();
                NameService.OGHeaderAuthenticationUserCredentials userCredentials = new NameService.OGHeaderAuthenticationUserCredentials();
                userCredentials.UserName = Request.Username;
                userCredentials.UserPassword = Request.Password;
                userCredentials.Domain = Request.HotelDomain;
                Auth.UserCredentials = userCredentials;
                OGHeader.Authentication = Auth;
                #endregion

                NameService.GetPassportResponse PassportResponse = new NameService.GetPassportResponse();
                NameService.GetPassportRequest PassportRequest = new NameService.GetPassportRequest();
                NameService.UniqueID UId = new NameService.UniqueID();
                UId.type = NameService.UniqueIDType.INTERNAL;
                UId.Value = Request.UpdateProileRequest.ProfileID;
                PassportRequest.NameID = UId;

                #region Response

                NameService.NameServiceSoapClient NamePortClient = new NameService.NameServiceSoapClient();
                bool isOperaCloudEnabled = false;
                isOperaCloudEnabled = (ConfigurationManager.AppSettings["OperaCloudEnabled"] != null
                                && !string.IsNullOrEmpty(ConfigurationManager.AppSettings["OperaCloudEnabled"].ToString())
                                && bool.TryParse(ConfigurationManager.AppSettings["OperaCloudEnabled"].ToString(), out isOperaCloudEnabled)) ? isOperaCloudEnabled : false;
                if (isOperaCloudEnabled)
                {
                    NamePortClient.Endpoint.Behaviors.Add(new Helper.CustomEndpointBehaviour(ConfigurationManager.AppSettings["WSSEUserName"].ToString(),
                                            ConfigurationManager.AppSettings["WSSEPassword"].ToString(),
                                            Request.Username, Request.Password, Request.HotelDomain));
                }
                PassportResponse = NamePortClient.GetPassport(ref OGHeader, PassportRequest);
                if (PassportResponse.Result.resultStatusFlag == NameService.ResultStatusFlag.SUCCESS)
                {
                    return new Models.OWS.OwsResponseModel
                    {
                        responseMessage = "Success",
                        statusCode = 101,
                        result = true,
                        responseData = PassportResponse.Passport
                    };
                }
                else
                {
                    return new Models.OWS.OwsResponseModel
                    {
                        
                        responseMessage = PassportResponse.Result != null ? string.Join(" ", PassportResponse.Result.Text[0].Value) : "Document not returned",
                        statusCode = 8002,
                        result = false
                    };
                }
                #endregion

            }
            catch (Exception ex)
            {
                return new Models.OWS.OwsResponseModel
                {

                    responseMessage = ex.Message,
                    statusCode = -1,
                    result = false
                };
            }
        }

        public Models.OWS.OwsResponseModel UpdatePassport(Models.OWS.OwsRequestModel Request)
        {
            try
            {
                new LogHelper().Debug("Update Passport request : " + JsonConvert.SerializeObject(Request), Request.UpdateProileRequest.ProfileID, "UpdatePassport", "API", "OWS");
                #region Request Header
                string temp = Helper.Helper.Get8Digits();
                NameService.OGHeader OGHeader = new NameService.OGHeader();
                OGHeader.transactionID = temp;
                OGHeader.timeStamp = DateTime.Now;
                OGHeader.primaryLangID = Request.Language; //English
                NameService.EndPoint orginEndPOint = new NameService.EndPoint();
                orginEndPOint.entityID = Request.KioskID; //Kiosk Identifier
                orginEndPOint.systemType = Request.SystemType;
                OGHeader.Origin = orginEndPOint;
                NameService.EndPoint destEndPOint = new NameService.EndPoint();
                destEndPOint.entityID = Request.DestinationEntityID;
                destEndPOint.systemType = Request.DestinationSystemType;
                OGHeader.Destination = destEndPOint;
                NameService.OGHeaderAuthentication Auth = new NameService.OGHeaderAuthentication();
                NameService.OGHeaderAuthenticationUserCredentials userCredentials = new NameService.OGHeaderAuthenticationUserCredentials();
                userCredentials.UserName = Request.Username;
                userCredentials.UserPassword = Request.Password;
                userCredentials.Domain = Request.HotelDomain;
                Auth.UserCredentials = userCredentials;
                OGHeader.Authentication = Auth;
                #endregion

                NameService.InsertUpdateDocumentResponse DocumentResponse = new NameService.InsertUpdateDocumentResponse();
                NameService.InsertUpdateDocumentRequest DocumentRequest = new NameService.InsertUpdateDocumentRequest();
                NameService.UniqueID UId = new NameService.UniqueID();
                UId.type = NameService.UniqueIDType.INTERNAL;
                UId.Value = Request.UpdateProileRequest.ProfileID;
                DocumentRequest.NameID = UId;
                NameService.GovernmentID GId = new NameService.GovernmentID();
                if (!string.IsNullOrEmpty(Request.UpdateProileRequest.DocumentType))
                {
                    GId.documentType = Request.UpdateProileRequest.DocumentType;
                }
                else
                {
                    GId.documentType = "UNKNOWN";
                }
                GId.documentNumber = string.IsNullOrEmpty(Request.UpdateProileRequest.DocumentNumber) ? " ": Request.UpdateProileRequest.DocumentNumber;

                GId.documentIDSpecified = false;
                GId.effectiveDate = Request.UpdateProileRequest.IssueDate != null ? Request.UpdateProileRequest.IssueDate.Value : new DateTime();
                GId.effectiveDateSpecified = Request.UpdateProileRequest.IssueDate != null ? true : false;
                

                GId.countryOfIssue = Request.UpdateProileRequest.IssueCountry;
                GId.displaySequence = 1;
                GId.primary = true;
                GId.primarySpecified = true;

                DocumentRequest.Document = GId;


                #region Response

                NameService.NameServiceSoapClient NamePortClient = new NameService.NameServiceSoapClient();
                bool isOperaCloudEnabled = false;
                isOperaCloudEnabled = (ConfigurationManager.AppSettings["OperaCloudEnabled"] != null
                                && !string.IsNullOrEmpty(ConfigurationManager.AppSettings["OperaCloudEnabled"].ToString())
                                && bool.TryParse(ConfigurationManager.AppSettings["OperaCloudEnabled"].ToString(), out isOperaCloudEnabled)) ? isOperaCloudEnabled : false;
                if (isOperaCloudEnabled)
                {
                    NamePortClient.Endpoint.Behaviors.Add(new Helper.CustomEndpointBehaviour(ConfigurationManager.AppSettings["WSSEUserName"].ToString(),
                                            ConfigurationManager.AppSettings["WSSEPassword"].ToString(),
                                            Request.Username, Request.Password, Request.HotelDomain));
                }
                DocumentResponse = NamePortClient.InsertUpdateDocument(ref OGHeader, DocumentRequest);
                if (DocumentResponse.Result.resultStatusFlag == NameService.ResultStatusFlag.SUCCESS)
                {
                    return new Models.OWS.OwsResponseModel
                    {
                        responseMessage = "Success",
                        statusCode = 101,
                        result = true
                    };
                }
                else
                {
                    return new Models.OWS.OwsResponseModel
                    {
                        responseMessage = DocumentResponse.Result != null ? string.Join(" ", DocumentResponse.Result.Text[0].Value) : "Document not updated",
                        statusCode = 8002,
                        result = false
                    };
                }
                #endregion

            }
            catch (Exception ex)
            {
                
                return new Models.OWS.OwsResponseModel
                {
                    responseMessage = ex.Message,
                    statusCode = -1,
                    result = false
                };
            }
        }

        public Models.OWS.OwsResponseModel UpdateName(Models.OWS.OwsRequestModel Request)
        {
            try
            {
                new LogHelper().Debug("Update Name request : " + JsonConvert.SerializeObject(Request), Request.UpdateProileRequest.ProfileID, "UpdateName", "API", "OWS");

                #region Request Header
                string temp = Helper.Helper.Get8Digits();
                NameService.OGHeader OGHeader = new NameService.OGHeader();
                OGHeader.transactionID = temp;
                OGHeader.timeStamp = DateTime.Now;
                OGHeader.primaryLangID = Request.Language; //English
                NameService.EndPoint orginEndPOint = new NameService.EndPoint();
                orginEndPOint.entityID = Request.KioskID; //Kiosk Identifier
                orginEndPOint.systemType = Request.SystemType;
                OGHeader.Origin = orginEndPOint;
                NameService.EndPoint destEndPOint = new NameService.EndPoint();
                destEndPOint.entityID = Request.DestinationEntityID;
                destEndPOint.systemType = Request.SystemType;
                OGHeader.Destination = destEndPOint;
                NameService.OGHeaderAuthentication Auth = new NameService.OGHeaderAuthentication();
                NameService.OGHeaderAuthenticationUserCredentials userCredentials = new NameService.OGHeaderAuthenticationUserCredentials();
                userCredentials.UserName = Request.Username;
                userCredentials.UserPassword = Request.Password;
                userCredentials.Domain = Request.HotelDomain;
                Auth.UserCredentials = userCredentials;
                OGHeader.Authentication = Auth;
                #endregion

                NameService.UpdateNameRequest UpdateNameReq = new NameService.UpdateNameRequest();
                NameService.UpdateNameResponse UpdateNameRes = new NameService.UpdateNameResponse();
                NameService.UniqueID UId = new NameService.UniqueID();
                UId.type = NameService.UniqueIDType.INTERNAL;
                UId.Value = Request.UpdateProileRequest.ProfileID;
                UpdateNameReq.NameID = UId;
                NameService.PersonName PN = new NameService.PersonName();
                //if (!string.IsNullOrEmpty(Request.LastName))
                //{

                //    if (!string.IsNullOrEmpty(Request.MiddleName))
                //    {
                //        string[] middleName = { Request.MiddleName };
                //        PN.middleName = middleName;
                //    }
                //    PN.lastName = Request.LastName;
                //    PN.firstName = Request.FirstName;
                //}
                //if(!string.IsNullOrEmpty(Request.FirstName))
                //{
                //    PN.firstName = Request.FirstName;
                //}
                UpdateNameReq.PersonName = PN;
                //if (Request.IsNativeNameSpecified)
                //{
                //    NameReference.NativeName NN = new NameReference.NativeName();
                //    NN.firstName = Request.NativeFirstName;

                //    if (!string.IsNullOrEmpty(Request.NativeMiddleName))
                //    {
                //        string[] middleName = { Request.NativeMiddleName };
                //        NN.middleName = middleName;
                //    }

                //    NN.lastName = Request.NativeLastName;

                //    NN.active = true;
                //    NN.activeSpecified = true;
                //    NN.languageCode = "ZH-T";

                //    UpdateNameReq.NativeName = NN;

                //}

               
                
                UpdateNameReq.Birthdate = Request.UpdateProileRequest.DOB != null ? Request.UpdateProileRequest.DOB.Value : new DateTime() ;
                UpdateNameReq.BirthdateSpecified = Request.UpdateProileRequest.DOB != null ? true : false;
                
                //UpdateNameReq.Birthdate = DateTime.Now;
                //UpdateNameReq.BirthdateSpecified = true;


                if (!string.IsNullOrEmpty(Request.UpdateProileRequest.Gender))
                {
                    UpdateNameReq.Gender = Request.UpdateProileRequest.Gender.ToUpper().Equals("MALE") ? NameService.Gender.MALE : Request.UpdateProileRequest.Gender.ToUpper().Equals("FEMALE") ? NameService.Gender.FEMALE : NameService.Gender.UNKNOWN;
                    UpdateNameReq.GenderSpecified = true;
                }
                UpdateNameReq.nationality = Request.UpdateProileRequest.Nationality;

                #region Call Get Passport
                Models.OWS.OwsResponseModel PassportResponse = new Models.OWS.OwsResponseModel();
                PassportResponse = GetPassport(Request);
                if (PassportResponse.result)
                {
                    NameService.GovernmentID GovID = (NameService.GovernmentID)PassportResponse.responseData;
                    if (GovID != null && !string.IsNullOrEmpty(GovID.documentType))
                    {
                        GovID.documentType = GovID.documentType.ToUpper();
                        if (!string.IsNullOrEmpty(GovID.documentNumber))
                        {
                            UpdateNameReq.Id = (NameService.GovernmentID)PassportResponse.responseData;
                            UpdateNameReq.Id.primary = true;
                            UpdateNameReq.Id.primarySpecified = true;

                        }
                        else
                        {
                            GovID = new NameService.GovernmentID();
                            GovID.countryOfIssue = Request.UpdateProileRequest.IssueCountry;
                            GovID.displaySequence = 1;
                            GovID.displaySequenceSpecified = true;
                            GovID.documentNumber = Request.UpdateProileRequest.DocumentNumber;
                            if (!string.IsNullOrEmpty(Request.UpdateProileRequest.DocumentType))
                                GovID.documentType = Request.UpdateProileRequest.DocumentType.ToUpper();
                            //GovID.documentType = GovID.documentType.ToUpper();
                            
                            
                            GovID.effectiveDate = Request.UpdateProileRequest.IssueDate != null ? Request.UpdateProileRequest.IssueDate.Value : new DateTime() ;
                            GovID.effectiveDateSpecified = Request.UpdateProileRequest.IssueDate != null ? true : false;
                            
                            GovID.primary = true;
                            GovID.primarySpecified = true;
                            UpdateNameReq.Id = GovID;
                        }
                    }
                }
                else
                {
                    NameService.GovernmentID GovID = new NameService.GovernmentID();
                    GovID.countryOfIssue = Request.UpdateProileRequest.IssueCountry;
                    GovID.displaySequence = 1;
                    GovID.displaySequenceSpecified = true;
                    GovID.documentNumber = Request.UpdateProileRequest.DocumentNumber;
                    GovID.documentType = Request.UpdateProileRequest.DocumentType;
                    
                    
                    GovID.effectiveDate = Request.UpdateProileRequest.IssueDate != null ? Request.UpdateProileRequest.IssueDate.Value : new DateTime();
                    GovID.effectiveDateSpecified = Request.UpdateProileRequest.IssueDate != null ? true:false;
                    
                    GovID.primary = true;
                    GovID.primarySpecified = true;
                    UpdateNameReq.Id = GovID;
                }
                #endregion


                #region Response

                NameService.NameServiceSoapClient NamePortClient = new NameService.NameServiceSoapClient();
                bool isOperaCloudEnabled = false;
                isOperaCloudEnabled = (ConfigurationManager.AppSettings["OperaCloudEnabled"] != null
                                && !string.IsNullOrEmpty(ConfigurationManager.AppSettings["OperaCloudEnabled"].ToString())
                                && bool.TryParse(ConfigurationManager.AppSettings["OperaCloudEnabled"].ToString(), out isOperaCloudEnabled)) ? isOperaCloudEnabled : false;
                if (isOperaCloudEnabled)
                {
                    NamePortClient.Endpoint.Behaviors.Add(new Helper.CustomEndpointBehaviour(ConfigurationManager.AppSettings["WSSEUserName"].ToString(),
                                            ConfigurationManager.AppSettings["WSSEPassword"].ToString(),
                                            Request.Username, Request.Password, Request.HotelDomain));
                }
                UpdateNameRes = NamePortClient.UpdateName(ref OGHeader, UpdateNameReq);
                if (UpdateNameRes.Result.resultStatusFlag == NameService.ResultStatusFlag.SUCCESS)
                {
                    return new Models.OWS.OwsResponseModel
                    {
                        responseMessage = "Success",
                        statusCode = 101,
                        result = true
                    };
                }
                else
                {
                    
                    return new Models.OWS.OwsResponseModel
                    {
                        responseMessage = UpdateNameRes.Result != null ? UpdateNameRes.Result.Text[0].Value : "Document not updated",
                        statusCode = 8002,
                        result = false
                    };
                }
                #endregion

            }
            catch (Exception ex)
            {

                return new Models.OWS.OwsResponseModel
                {
                    responseMessage = ex.Message,
                    statusCode = -1,
                    result = false
                };
            }
        }

        public Models.OWS.OwsResponseModel GetCountryCodes(Models.OWS.OwsRequestModel request)
        {
            try
            {
                #region Request Header
                string temp1 = Helper.Helper.Get8Digits();
                InformationService.OGHeader OG = new InformationService.OGHeader();
                OG.transactionID = temp1;
                OG.timeStamp = DateTime.Now;
                OG.primaryLangID = request.Language; //English
                InformationService.EndPoint orginEndPnt = new InformationService.EndPoint();
                orginEndPnt.entityID = request.KioskID; //Kiosk Identifier
                orginEndPnt.systemType = request.SystemType;
                OG.Origin = orginEndPnt;
                InformationService.EndPoint destEndPnt = new InformationService.EndPoint();
                destEndPnt.entityID = request.DestinationEntityID;
                destEndPnt.systemType = request.DestinationSystemType;
                OG.Destination = destEndPnt;
                InformationService.OGHeaderAuthentication Authnt = new InformationService.OGHeaderAuthentication();
                InformationService.OGHeaderAuthenticationUserCredentials userCrdntls = new InformationService.OGHeaderAuthenticationUserCredentials();
                userCrdntls.UserName = request.Username;
                userCrdntls.UserPassword = request.Password;
                userCrdntls.Domain = request.HotelDomain;
                Authnt.UserCredentials = userCrdntls;
                OG.Authentication = Authnt;
                #endregion



                #region Request Body
                InformationService.LovRequest LOVReq = new InformationService.LovRequest();
                InformationService.LovResponse LOVRes = new InformationService.LovResponse();

                InformationService.LovQueryType2 LOVQuery = new InformationService.LovQueryType2();
                LOVQuery.LovIdentifier = "COUNTRYCODES";
                LOVReq.Item = LOVQuery;
                #endregion

                InformationService.InformationSoapClient InfoPortClient = new InformationService.InformationSoapClient();
                bool isOperaCloudEnabled = false;
                isOperaCloudEnabled = (ConfigurationManager.AppSettings["OperaCloudEnabled"] != null
                                && !string.IsNullOrEmpty(ConfigurationManager.AppSettings["OperaCloudEnabled"].ToString())
                                && bool.TryParse(ConfigurationManager.AppSettings["OperaCloudEnabled"].ToString(), out isOperaCloudEnabled)) ? isOperaCloudEnabled : false;
                if (isOperaCloudEnabled)
                {
                    InfoPortClient.Endpoint.Behaviors.Add(new Helper.CustomEndpointBehaviour(ConfigurationManager.AppSettings["WSSEUserName"].ToString(),
                                            ConfigurationManager.AppSettings["WSSEPassword"].ToString(),
                                            request.Username, request.Password, request.HotelDomain));
                }
                LOVRes = InfoPortClient.QueryLov(ref OG, LOVReq);
                List<Models.OWS.CountryState> CountryStateList = new List<Models.OWS.CountryState>();

                if (LOVRes.Result.resultStatusFlag == InformationService.ResultStatusFlag.SUCCESS)
                {
                    if (LOVRes.LovQueryResult != null)
                    {

                        foreach (InformationService.LovQueryResultType lovQueryResult in LOVRes.LovQueryResult)
                        {
                            if (lovQueryResult.LovValue != null)
                            {
                                foreach (InformationService.LovValueType lovValue in lovQueryResult.LovValue)
                                {
                                    Models.OWS.CountryState countryState = new Models.OWS.CountryState();
                                    countryState.CountryName = lovValue.description;
                                    countryState.CountryCode = lovValue.Value;
                                    CountryStateList.Add(countryState);
                                }
                            }
                        }
                        return new Models.OWS.OwsResponseModel
                        {
                            result = true,
                            statusCode = 200,
                            responseData = CountryStateList
                        };
                    }
                    else
                    {
                        return new Models.OWS.OwsResponseModel
                        {
                            result = false,
                            statusCode = 200,
                            responseData = null
                        };
                    }
                }
                else
                {
                    return new Models.OWS.OwsResponseModel
                    {
                        result = false,
                        statusCode = -1,
                        responseData = null
                    };
                }

            }
            catch (Exception ex)
            {
                return new Models.OWS.OwsResponseModel
                {
                    responseMessage = ex.Message,
                    result = false,
                    statusCode = -1
                };
            }
        }

        public Models.OWS.OwsResponseModel GetNationalityCodes(Models.OWS.OwsRequestModel request)
        {
            try
            {
                #region Request Header
                string temp1 = Helper.Helper.Get8Digits();
                InformationService.OGHeader OG = new InformationService.OGHeader();
                OG.transactionID = temp1;
                OG.timeStamp = DateTime.Now;
                OG.primaryLangID = request.Language; //English
                InformationService.EndPoint orginEndPnt = new InformationService.EndPoint();
                orginEndPnt.entityID = request.KioskID; //Kiosk Identifier
                orginEndPnt.systemType = request.SystemType;
                OG.Origin = orginEndPnt;
                InformationService.EndPoint destEndPnt = new InformationService.EndPoint();
                destEndPnt.entityID = request.DestinationEntityID;
                destEndPnt.systemType = request.DestinationSystemType;
                OG.Destination = destEndPnt;
                InformationService.OGHeaderAuthentication Authnt = new InformationService.OGHeaderAuthentication();
                InformationService.OGHeaderAuthenticationUserCredentials userCrdntls = new InformationService.OGHeaderAuthenticationUserCredentials();
                userCrdntls.UserName = request.Username;
                userCrdntls.UserPassword = request.Password;
                userCrdntls.Domain = request.HotelDomain;
                Authnt.UserCredentials = userCrdntls;
                OG.Authentication = Authnt;
                #endregion



                #region Request Body
                InformationService.LovRequest LOVReq = new InformationService.LovRequest();
                InformationService.LovResponse LOVRes = new InformationService.LovResponse();

                InformationService.LovQueryType2 LOVQuery = new InformationService.LovQueryType2();
                LOVQuery.LovIdentifier = "NATIONALITY";
                LOVReq.Item = LOVQuery;
                #endregion

                InformationService.InformationSoapClient InfoPortClient = new InformationService.InformationSoapClient();
                bool isOperaCloudEnabled = false;
                isOperaCloudEnabled = (ConfigurationManager.AppSettings["OperaCloudEnabled"] != null
                                && !string.IsNullOrEmpty(ConfigurationManager.AppSettings["OperaCloudEnabled"].ToString())
                                && bool.TryParse(ConfigurationManager.AppSettings["OperaCloudEnabled"].ToString(), out isOperaCloudEnabled)) ? isOperaCloudEnabled : false;
                if (isOperaCloudEnabled)
                {
                    InfoPortClient.Endpoint.Behaviors.Add(new Helper.CustomEndpointBehaviour(ConfigurationManager.AppSettings["WSSEUserName"].ToString(),
                                            ConfigurationManager.AppSettings["WSSEPassword"].ToString(),
                                            request.Username, request.Password, request.HotelDomain));
                }
                LOVRes = InfoPortClient.QueryLov(ref OG, LOVReq);
                List<Models.OWS.CountryState> CountryStateList = new List<Models.OWS.CountryState>();

                if (LOVRes.Result.resultStatusFlag == InformationService.ResultStatusFlag.SUCCESS)
                {
                    if (LOVRes.LovQueryResult != null)
                    {

                        foreach (InformationService.LovQueryResultType lovQueryResult in LOVRes.LovQueryResult)
                        {
                            if (lovQueryResult.LovValue != null)
                            {
                                foreach (InformationService.LovValueType lovValue in lovQueryResult.LovValue)
                                {
                                    Models.OWS.CountryState countryState = new Models.OWS.CountryState();
                                    countryState.CountryName = lovValue.description;
                                    countryState.CountryCode = lovValue.Value;
                                    CountryStateList.Add(countryState);
                                }
                            }
                        }
                        return new Models.OWS.OwsResponseModel
                        {
                            result = true,
                            statusCode = 200,
                            responseData = CountryStateList
                        };
                    }
                    else
                    {
                        return new Models.OWS.OwsResponseModel
                        {
                            result = false,
                            statusCode = 200,
                            responseData = null
                        };
                    }
                }
                else
                {
                    return new Models.OWS.OwsResponseModel
                    {
                        result = false,
                        statusCode = -1,
                        responseData = null
                    };
                }

            }
            catch (Exception ex)
            {
                return new Models.OWS.OwsResponseModel
                {
                    responseMessage = ex.Message,
                    result = false,
                    statusCode = -1
                };
            }
        }
        public Models.OWS.OwsResponseModel GetDocumentTypes(Models.OWS.OwsRequestModel request)
        {
            try
            {
                #region Request Header
                string temp1 = Helper.Helper.Get8Digits();
                InformationService.OGHeader OG = new InformationService.OGHeader();
                OG.transactionID = temp1;
                OG.timeStamp = DateTime.Now;
                OG.primaryLangID = request.Language; //English
                InformationService.EndPoint orginEndPnt = new InformationService.EndPoint();
                orginEndPnt.entityID = "WEBHOTEL";//request.KioskID; //Kiosk Identifier
                orginEndPnt.systemType = "WEB";//request.SystemType;
                OG.Origin = orginEndPnt;
                InformationService.EndPoint destEndPnt = new InformationService.EndPoint();
                destEndPnt.entityID = "TI";//request.DestinationEntityID;
                destEndPnt.systemType = "ORS";//request.DestinationSystemType;
                OG.Destination = destEndPnt;
                InformationService.OGHeaderAuthentication Authnt = new InformationService.OGHeaderAuthentication();
                InformationService.OGHeaderAuthenticationUserCredentials userCrdntls = new InformationService.OGHeaderAuthenticationUserCredentials();
                userCrdntls.UserName = request.Username;
                userCrdntls.UserPassword = request.Password;
                userCrdntls.Domain = request.HotelDomain;
                Authnt.UserCredentials = userCrdntls;
                OG.Authentication = Authnt;
                #endregion



                #region Request Body
                InformationService.LovRequest LOVReq = new InformationService.LovRequest();
                InformationService.LovResponse LOVRes = new InformationService.LovResponse();

                InformationService.LovQueryType2 LOVQuery = new InformationService.LovQueryType2();
                LOVQuery.LovIdentifier = "DOCUMENT_TYPES";
                LOVReq.Item = LOVQuery;
                #endregion

                InformationService.InformationSoapClient InfoPortClient = new InformationService.InformationSoapClient();
                bool isOperaCloudEnabled = false;
                isOperaCloudEnabled = (ConfigurationManager.AppSettings["OperaCloudEnabled"] != null
                                && !string.IsNullOrEmpty(ConfigurationManager.AppSettings["OperaCloudEnabled"].ToString())
                                && bool.TryParse(ConfigurationManager.AppSettings["OperaCloudEnabled"].ToString(), out isOperaCloudEnabled)) ? isOperaCloudEnabled : false;
                if (isOperaCloudEnabled)
                {
                    InfoPortClient.Endpoint.Behaviors.Add(new Helper.CustomEndpointBehaviour(ConfigurationManager.AppSettings["WSSEUserName"].ToString(),
                                            ConfigurationManager.AppSettings["WSSEPassword"].ToString(),
                                            request.Username, request.Password, request.HotelDomain));
                }
                LOVRes = InfoPortClient.QueryLov(ref OG, LOVReq);
                List<Models.OWS.DocumentTypeMaster> DocumentTypeList = new List<Models.OWS.DocumentTypeMaster>();

                if (LOVRes.Result.resultStatusFlag == InformationService.ResultStatusFlag.SUCCESS)
                {
                    if (LOVRes.LovQueryResult != null)
                    {

                        foreach (InformationService.LovQueryResultType lovQueryResult in LOVRes.LovQueryResult)
                        {
                            if (lovQueryResult.LovValue != null)
                            {
                                foreach (InformationService.LovValueType lovValue in lovQueryResult.LovValue)
                                {
                                    Models.OWS.DocumentTypeMaster document = new Models.OWS.DocumentTypeMaster();
                                    document.DocumentTypeDescription = lovValue.description;
                                    document.DocumentTypeCode = lovValue.Value;
                                    DocumentTypeList.Add(document);
                                }
                            }
                        }
                        return new Models.OWS.OwsResponseModel
                        {
                            result = true,
                            statusCode = 200,
                            responseData = DocumentTypeList
                        };
                    }
                    else
                    {
                        return new Models.OWS.OwsResponseModel
                        {
                            result = false,
                            statusCode = 200,
                            responseData = null
                        };
                    }
                }
                else
                {
                    return new Models.OWS.OwsResponseModel
                    {
                        result = false,
                        statusCode = -1,
                        responseData = null
                    };
                }

            }
            catch (Exception ex)
            {
                return new Models.OWS.OwsResponseModel
                {
                    responseMessage = ex.Message,
                    result = false,
                    statusCode = -1
                };
            }
        }

        public Models.OWS.OwsResponseModel GetPhoneTypes(Models.OWS.OwsRequestModel request)
        {
            try
            {
                #region Request Header
                string temp1 = Helper.Helper.Get8Digits();
                InformationService.OGHeader OG = new InformationService.OGHeader();
                OG.transactionID = temp1;
                OG.timeStamp = DateTime.Now;
                OG.primaryLangID = request.Language; //English
                InformationService.EndPoint orginEndPnt = new InformationService.EndPoint();
                orginEndPnt.entityID = request.KioskID; //Kiosk Identifier
                orginEndPnt.systemType = request.SystemType;
                OG.Origin = orginEndPnt;
                InformationService.EndPoint destEndPnt = new InformationService.EndPoint();
                destEndPnt.entityID = request.DestinationEntityID;
                destEndPnt.systemType = request.DestinationSystemType;
                OG.Destination = destEndPnt;
                InformationService.OGHeaderAuthentication Authnt = new InformationService.OGHeaderAuthentication();
                InformationService.OGHeaderAuthenticationUserCredentials userCrdntls = new InformationService.OGHeaderAuthenticationUserCredentials();
                userCrdntls.UserName = request.Username;
                userCrdntls.UserPassword = request.Password;
                userCrdntls.Domain = request.HotelDomain;
                Authnt.UserCredentials = userCrdntls;
                OG.Authentication = Authnt;
                #endregion



                #region Request Body
                InformationService.LovRequest LOVReq = new InformationService.LovRequest();
                InformationService.LovResponse LOVRes = new InformationService.LovResponse();

                InformationService.LovQueryType2 LOVQuery = new InformationService.LovQueryType2();
                LOVQuery.LovIdentifier = "phoneType";
                LOVReq.Item = LOVQuery;
                #endregion

                InformationService.InformationSoapClient InfoPortClient = new InformationService.InformationSoapClient();
                bool isOperaCloudEnabled = false;
                isOperaCloudEnabled = (ConfigurationManager.AppSettings["OperaCloudEnabled"] != null
                                && !string.IsNullOrEmpty(ConfigurationManager.AppSettings["OperaCloudEnabled"].ToString())
                                && bool.TryParse(ConfigurationManager.AppSettings["OperaCloudEnabled"].ToString(), out isOperaCloudEnabled)) ? isOperaCloudEnabled : false;
                if (isOperaCloudEnabled)
                {
                    InfoPortClient.Endpoint.Behaviors.Add(new Helper.CustomEndpointBehaviour(ConfigurationManager.AppSettings["WSSEUserName"].ToString(),
                                            ConfigurationManager.AppSettings["WSSEPassword"].ToString(),
                                            request.Username, request.Password, request.HotelDomain));
                }
                LOVRes = InfoPortClient.QueryLov(ref OG, LOVReq);
                List<Models.OWS.PhoneTypeMaster> phoneTypes = new List<Models.OWS.PhoneTypeMaster>();

                if (LOVRes.Result.resultStatusFlag == InformationService.ResultStatusFlag.SUCCESS)
                {
                    if (LOVRes.LovQueryResult != null)
                    {

                        foreach (InformationService.LovQueryResultType lovQueryResult in LOVRes.LovQueryResult)
                        {
                            if (lovQueryResult.LovValue != null)
                            {
                                foreach (InformationService.LovValueType lovValue in lovQueryResult.LovValue)
                                {
                                    Models.OWS.PhoneTypeMaster phoneType = new Models.OWS.PhoneTypeMaster();
                                    phoneType.PhoneTypeDescription = lovValue.description;
                                    phoneType.PhoneTypeCode = lovValue.Value;
                                    phoneTypes.Add(phoneType);
                                }
                            }
                        }
                        return new Models.OWS.OwsResponseModel
                        {
                            result = true,
                            statusCode = 200,
                            responseData = phoneTypes
                        };
                    }
                    else
                    {
                        return new Models.OWS.OwsResponseModel
                        {
                            result = false,
                            statusCode = 200,
                            responseData = null
                        };
                    }
                }
                else
                {
                    return new Models.OWS.OwsResponseModel
                    {
                        result = false,
                        statusCode = -1,
                        responseData = null
                    };
                }

            }
            catch (Exception ex)
            {
                return new Models.OWS.OwsResponseModel
                {
                    responseMessage = ex.Message,
                    result = false,
                    statusCode = -1
                };
            }
        }
        public Models.OWS.OwsResponseModel GetStateCodesByCountryCode(Models.OWS.OwsRequestModel request)
        {
            try
            {
                #region Request Header
                string temp1 = Helper.Helper.Get8Digits();
                InformationService.OGHeader OG = new InformationService.OGHeader();
                OG.transactionID = temp1;
                OG.timeStamp = DateTime.Now;
                OG.primaryLangID = request.Language; //English
                InformationService.EndPoint orginEndPnt = new InformationService.EndPoint();
                orginEndPnt.entityID = request.KioskID; //Kiosk Identifier
                orginEndPnt.systemType = request.SystemType;
                OG.Origin = orginEndPnt;
                InformationService.EndPoint destEndPnt = new InformationService.EndPoint();
                destEndPnt.entityID = request.DestinationEntityID;
                destEndPnt.systemType = request.DestinationSystemType;
                OG.Destination = destEndPnt;
                InformationService.OGHeaderAuthentication Authnt = new InformationService.OGHeaderAuthentication();
                InformationService.OGHeaderAuthenticationUserCredentials userCrdntls = new InformationService.OGHeaderAuthenticationUserCredentials();
                userCrdntls.UserName = request.Username;
                userCrdntls.UserPassword = request.Password;
                userCrdntls.Domain = request.HotelDomain;
                Authnt.UserCredentials = userCrdntls;
                OG.Authentication = Authnt;
                #endregion



                #region Request Body
                InformationService.LovRequest LOVReq = new InformationService.LovRequest();
                InformationService.LovResponse LOVRes = new InformationService.LovResponse();

                InformationService.LovQueryType2 LOVQuery = new InformationService.LovQueryType2();
                LOVQuery.LovIdentifier = "STATEBYCOUNTRY";
                LOVQuery.LovQueryQualifier = new LovQueryQualifierType[] {new LovQueryQualifierType()
                {
                    qualifierType = "COUNTRY",
                    Value = request.FetchMaster != null ? request.FetchMaster.ToString() : ""
                }
                };
                LOVReq.Item = LOVQuery;
                #endregion

                InformationService.InformationSoapClient InfoPortClient = new InformationService.InformationSoapClient();
                bool isOperaCloudEnabled = false;
                isOperaCloudEnabled = (ConfigurationManager.AppSettings["OperaCloudEnabled"] != null
                                && !string.IsNullOrEmpty(ConfigurationManager.AppSettings["OperaCloudEnabled"].ToString())
                                && bool.TryParse(ConfigurationManager.AppSettings["OperaCloudEnabled"].ToString(), out isOperaCloudEnabled)) ? isOperaCloudEnabled : false;
                if (isOperaCloudEnabled)
                {
                    InfoPortClient.Endpoint.Behaviors.Add(new Helper.CustomEndpointBehaviour(ConfigurationManager.AppSettings["WSSEUserName"].ToString(),
                                            ConfigurationManager.AppSettings["WSSEPassword"].ToString(),
                                            request.Username, request.Password, request.HotelDomain));
                }
                LOVRes = InfoPortClient.QueryLov(ref OG, LOVReq);
                List<Models.OWS.CountryState> CountryStateList = new List<Models.OWS.CountryState>();

                if (LOVRes.Result.resultStatusFlag == InformationService.ResultStatusFlag.SUCCESS)
                {
                    if (LOVRes.LovQueryResult != null)
                    {
                        foreach (InformationService.LovQueryResultType lovQueryResult in LOVRes.LovQueryResult)
                        {
                            if (lovQueryResult.LovValue != null)
                            {
                                foreach (InformationService.LovValueType lovValue in lovQueryResult.LovValue)
                                {
                                    Models.OWS.CountryState countryState = new Models.OWS.CountryState();
                                    countryState.CountryCode = request.FetchMaster.ToString();
                                    countryState.StateName = lovValue.description;
                                    countryState.StateCode = lovValue.Value;
                                    CountryStateList.Add(countryState);
                                }
                            }
                        }
                        return new Models.OWS.OwsResponseModel
                        {
                            result = true,
                            statusCode = 200,
                            responseData = CountryStateList
                        };
                    }
                    else
                    {
                        return new Models.OWS.OwsResponseModel
                        {
                            result = false,
                            statusCode = 200,
                            responseData = null
                        };
                    }
                }
                else
                {
                    return new Models.OWS.OwsResponseModel
                    {
                        result = false,
                        statusCode = -1,
                        responseData = null
                    };
                }

            }
            catch (Exception ex)
            {
                return new Models.OWS.OwsResponseModel
                {
                    responseMessage = ex.Message,
                    result = false,
                    statusCode = -1
                };
            }
        }
        public Models.OWS.OwsResponseModel GetReservationSummaryList(Models.OWS.OwsRequestModel Request)
        {
            try
            {
                //NLog with Debug
                List<Models.OWS.OperaReservation> RList = new List<Models.OWS.OperaReservation>();
                List<Models.OWS.PreferanceDetails> RPreferencesList = new List<Models.OWS.PreferanceDetails> ();
                List<Models.OWS.PackageDetails> RPackagesList = new List<Models.OWS.PackageDetails>();


                #region Call FetchReservation by conf no
                ReservationService.FutureBookingSummaryRequest fetchBookingSummaryReq = new ReservationService.FutureBookingSummaryRequest();
                ReservationService.FutureBookingSummaryResponse fetchBookingSummaryRes = new ReservationService.FutureBookingSummaryResponse();

                #region Request Header

                string temp = Helper.Helper.Get8Digits();
                ReservationService.OGHeader OGHeader = new ReservationService.OGHeader();
                OGHeader.transactionID = temp;
                OGHeader.timeStamp = DateTime.Now;
                OGHeader.primaryLangID = Request.Language; //English
                ReservationService.EndPoint orginEndPOint = new ReservationService.EndPoint();
                orginEndPOint.entityID = Request.KioskID; //Kiosk Identifier
                orginEndPOint.systemType = Request.SystemType;//"KIOSK";
                OGHeader.Origin = orginEndPOint;
                ReservationService.EndPoint destEndPOint = new ReservationService.EndPoint();
                destEndPOint.entityID = Request.DestinationEntityID;//"TI";
                destEndPOint.systemType = Request.DestinationSystemType;//"PMS";
                OGHeader.Destination = destEndPOint;
                ReservationService.OGHeaderAuthentication Auth = new ReservationService.OGHeaderAuthentication();
                ReservationService.OGHeaderAuthenticationUserCredentials userCredentials = new ReservationService.OGHeaderAuthenticationUserCredentials();
                userCredentials.UserName = Request.Username;
                userCredentials.UserPassword = Request.Password;
                userCredentials.Domain = Request.HotelDomain;
                Auth.UserCredentials = userCredentials;
                OGHeader.Authentication = Auth;
                #endregion

                ReservationService.ReservationServiceSoapClient ResSoapCLient = new ReservationService.ReservationServiceSoapClient();
                bool isOperaCloudEnabled = false;
                isOperaCloudEnabled = (ConfigurationManager.AppSettings["OperaCloudEnabled"] != null
                                && !string.IsNullOrEmpty(ConfigurationManager.AppSettings["OperaCloudEnabled"].ToString())
                                && bool.TryParse(ConfigurationManager.AppSettings["OperaCloudEnabled"].ToString(), out isOperaCloudEnabled)) ? isOperaCloudEnabled : false;
                if (isOperaCloudEnabled)
                {
                    ResSoapCLient.Endpoint.Behaviors.Add(new Helper.CustomEndpointBehaviour(ConfigurationManager.AppSettings["WSSEUserName"].ToString(),
                                            ConfigurationManager.AppSettings["WSSEPassword"].ToString(),
                                            Request.Username, Request.Password, Request.HotelDomain));
                }

                #region Request Body



                fetchBookingSummaryReq.numberOfReservationsToBeFetched = Request.BookingSummaryRequest.ReservationCountToBeFetched != null ? Request.BookingSummaryRequest.ReservationCountToBeFetched.Value : 0;
                fetchBookingSummaryReq.numberOfReservationsToBeFetchedSpecified = Request.BookingSummaryRequest.ReservationCountToBeFetched != null ? Request.BookingSummaryRequest.ReservationCountToBeFetched != 0 ? true :  false : false;
                fetchBookingSummaryReq.summaryOnly = Request.BookingSummaryRequest.IsSummaryOnly != null ? Request.BookingSummaryRequest.IsSummaryOnly.Value : false;

                var st = ConfigurationManager.AppSettings["IsOperaOlderVersion"].ToString();
                if (Request.BookingSummaryRequest.FromArrivalDate != null && Request.BookingSummaryRequest.ToArrivalDate != null)
                {
                    fetchBookingSummaryReq.QueryDateRange = new ReservationService.FutureBookingSummaryRequestQueryDateRange()
                    {
                        dateType = ReservationService.FutureBookingSummaryRequestQueryDateRangeDateType.ARRIVAL_DATE,
                        dateTypeSpecified = true,
                        StartDate = Request.BookingSummaryRequest.FromArrivalDate.Value,
                        Item = (DateTime)Request.BookingSummaryRequest.ToArrivalDate.Value

                    };


                  
                }
                
                else if (Request.BookingSummaryRequest.FromDepartureDate != null && Request.BookingSummaryRequest.ToDepartureDate != null
                    && (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["IsOperaOlderVersion"].ToString()) && !bool.Parse(ConfigurationManager.AppSettings["IsOperaOlderVersion"].ToString())))
                {
                    fetchBookingSummaryReq.QueryDateRange = new ReservationService.FutureBookingSummaryRequestQueryDateRange()
                    {
                        dateType = ReservationService.FutureBookingSummaryRequestQueryDateRangeDateType.DEPARTURE_DATE,
                        dateTypeSpecified = true,
                        StartDate = Request.BookingSummaryRequest.FromDepartureDate.Value,
                        Item = (DateTime)Request.BookingSummaryRequest.ToDepartureDate.Value

                    };
                }


                ReservationService.ReservationStatusType reservationStatusType = ReservationService.ReservationStatusType.PROSPECT;
                if (!string.IsNullOrEmpty(Request.BookingSummaryRequest.ReservatioStatus))
                {
                    switch (Request.BookingSummaryRequest.ReservatioStatus)
                    {
                        case "INHOUSE":
                            reservationStatusType = ReservationService.ReservationStatusType.INHOUSE;
                            break;
                        case "CANCELED":
                            reservationStatusType = ReservationService.ReservationStatusType.CANCELED;
                            break;
                        case "CHANGED":
                            reservationStatusType = ReservationService.ReservationStatusType.CHANGED;
                            break;
                        case "CHECKEDOUT":
                            reservationStatusType = ReservationService.ReservationStatusType.CHECKEDOUT;
                            break;
                        case "DUEIN":
                            reservationStatusType = ReservationService.ReservationStatusType.DUEIN;
                            break;
                        case "DUEOUT":
                            reservationStatusType = ReservationService.ReservationStatusType.DUEOUT;
                            break;
                        case "NOSHOW":
                            reservationStatusType = ReservationService.ReservationStatusType.NOSHOW;
                            break;
                        case "PRECHECKEDIN":
                            reservationStatusType = ReservationService.ReservationStatusType.PRECHECKEDIN;
                            break;
                        case "PROSPECT":
                            reservationStatusType = ReservationService.ReservationStatusType.PROSPECT;
                            break;
                        case "RESERVED":
                            reservationStatusType = ReservationService.ReservationStatusType.RESERVED;
                            break;
                        case "WAITLISTED":
                            reservationStatusType = ReservationService.ReservationStatusType.WAITLISTED;
                            break;
                    }
                }

                ReservationService.ReservationDispositionType reservationDispositionType = ReservationService.ReservationDispositionType.NONE;
                if(!string.IsNullOrEmpty(ConfigurationManager.AppSettings["IsOperaOlderVersion"].ToString()) && bool.Parse(ConfigurationManager.AppSettings["IsOperaOlderVersion"].ToString())
                    && !string.IsNullOrEmpty(Request.BookingSummaryRequest.ReservatioStatus))
                {
                    switch (Request.BookingSummaryRequest.ReservatioStatus)
                    {
                        case "INHOUSE":
                            reservationDispositionType = ReservationService.ReservationDispositionType.INHOUSE;
                            break;
                        case "DUEIN":
                            reservationDispositionType = ReservationService.ReservationDispositionType.DUEIN;
                            break;
                        case "DUEOUT":
                            reservationDispositionType = ReservationService.ReservationDispositionType.DUEOUT;
                            break;

                        //default: reservationDispositionType = ReservationService.ReservationDispositionType.NONE;
                        //    break;
                    }
                }
                ReservationService.FetchBookingFilters fetchBookingFilters = new ReservationService.FetchBookingFilters();
                fetchBookingFilters.GetList = true;
                fetchBookingFilters.GetListSpecified = true;

                if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["IsOperaOlderVersion"].ToString()) && bool.Parse(ConfigurationManager.AppSettings["IsOperaOlderVersion"].ToString()))
                {
                    fetchBookingFilters.ReservationDisposition = reservationDispositionType;
                    fetchBookingFilters.ReservationDispositionSpecified = true;
                }
                else
                {
                    fetchBookingFilters.ReservationStatus = reservationStatusType;
                    fetchBookingFilters.ReservationStatusSpecified = true;
                }


                //if (!string.IsNullOrEmpty(Request.BookingSummaryRequest.ReservatioStatus) && reservationStatusType != ReservationService.ReservationStatusType.PROSPECT)
                //{
                //    if (reservationDispositionType == ReservationService.ReservationDispositionType.NONE)
                //    {
                //        fetchBookingFilters.ReservationStatus = reservationStatusType;
                //        fetchBookingFilters.ReservationStatusSpecified = true;
                //    }
                //    else
                //    {
                //        fetchBookingFilters.ReservationDisposition = reservationDispositionType;
                //        fetchBookingFilters.ReservationDispositionSpecified = true;
                //    }
                //}

                fetchBookingFilters.IncludePseudoRoom = false;
                fetchBookingFilters.IncludePseudoRoomSpecified = true;
                fetchBookingFilters.HotelReference = new ReservationService.HotelReference()
                {
                    chainCode = Request.ChainCode,
                    hotelCode = Request.HotelDomain,
                    Value = Request.HotelDomain
                };
                fetchBookingFilters.RoomClass = string.IsNullOrEmpty(Request.BookingSummaryRequest.RoomClass) ? null : Request.BookingSummaryRequest.RoomClass;
                fetchBookingFilters.RoomNumber = string.IsNullOrEmpty(Request.BookingSummaryRequest.RoomNumber) ? null : Request.BookingSummaryRequest.RoomNumber;
                
                fetchBookingSummaryReq.AdditionalFilters = fetchBookingFilters;


                #endregion

                //ResSoapCLient.Endpoint.Behaviors.Add(new Helper.CustomEndpointBehaviour("Test USE", "Request.WSSEPassword", "Request.KioskUserName", "Request.KioskPassword", "Request.HotelDomain"));

                fetchBookingSummaryRes = ResSoapCLient.FutureBookingSummary(ref OGHeader, fetchBookingSummaryReq);
                ReservationService.GDSResultStatus status = fetchBookingSummaryRes.Result;

                #region Response Success
                if (status.resultStatusFlag.Equals(ReservationService.ResultStatusFlag.SUCCESS))
                {
                    try
                    {
                        foreach (ReservationService.HotelReservation Hreservation in fetchBookingSummaryRes.HotelReservations)
                        {
                            List<Models.OWS.GuestProfile> GPList = new List<Models.OWS.GuestProfile>();
                            Models.OWS.OperaReservation Reservation = new Models.OWS.OperaReservation();
                            ReservationService.HotelReservation hReservation = new ReservationService.HotelReservation();

                            hReservation = Hreservation;
                            if (hReservation.printRateSpecified)
                                Reservation.PrintRate = hReservation.printRate;
                            if (hReservation.noPostSpecified)
                                Reservation.NoPost = hReservation.noPost;
                            if (hReservation.DoNotMoveRoomSpecified)
                                Reservation.DoNotMoveRoom = hReservation.DoNotMoveRoom;
                            Reservation.ReservationSourceCode = hReservation.sourceCode;
                            Reservation.ExpectedDepartureTime = hReservation.checkOutTime;
                            string temp1 = hReservation.checkOutTime.ToString("HH:mm");

                            Reservation.ReservationStatus = hReservation.computedReservationStatus.ToString();
                            Reservation.ComputedReservationStatus = hReservation.computedReservationStatus.ToString();

                            ReservationService.Paragraph p;
                            Reservation.isInQueue = hReservation.queueExists;

                            #region Creation Date
                            Reservation.CreatedDateTime = hReservation.insertDate;
                            #endregion

                            #region Confirmation No
                            if (hReservation.UniqueIDList != null)
                            {
                                ReservationService.UniqueID[] rUniqueIDList = hReservation.UniqueIDList;
                                if (rUniqueIDList.Length > 0)
                                {
                                    Reservation.ReservationNumber = rUniqueIDList[0].Value;
                                    foreach (ReservationService.UniqueID UID in rUniqueIDList)
                                    {
                                        if (UID.source != null && UID.source.Equals("RESVID"))
                                            Reservation.ReservationNameID = UID.Value;
                                        if (UID.source != null && UID.source.Equals("LEGNUMBER"))
                                            Reservation.LegNumber = UID.Value;

                                    }

                                }
                            }
                            #endregion

                            #region List Of RoomStays
                            foreach (ReservationService.RoomStay ReservationRoomStay in hReservation.RoomStays)
                            {
                                #region Arrival Date and departure date
                                ReservationService.TimeSpan resTimeSpan = ReservationRoomStay.TimeSpan;
                                if (resTimeSpan != null)
                                {
                                    Reservation.ArrivalDate = resTimeSpan.StartDate;
                                    try
                                    {
                                        Reservation.DepartureDate = DateTime.Parse(resTimeSpan.Item.ToString());
                                    }
                                    catch (Exception) { }
                                }
                                #endregion

                                #region Total Rate amount
                                Reservation.IsTaxInclusive = ReservationRoomStay.ExpectedCharges != null ? ReservationRoomStay.ExpectedCharges.TaxInclusive : false;
                                Reservation.TotalAmount = ReservationRoomStay.ExpectedCharges != null ? (decimal)ReservationRoomStay.ExpectedCharges.TotalRoomRateAndPackages : 0;
                                //if (!taxInculsive && ReservationRoomStay.ExpectedCharges.TotalTaxesAndFeesSpecified)
                                Reservation.TotalAmount = ReservationRoomStay.ExpectedCharges != null ? Reservation.TotalAmount + (decimal)ReservationRoomStay.ExpectedCharges.TotalTaxesAndFees : 0;
                                ReservationService.Amount Amnt = ReservationRoomStay.CurrentBalance;
                                Reservation.CurrentBalance = Amnt != null ? (decimal)Amnt.Value : 0;

                                #endregion

                                #region ExpectedCharges
                                ReservationService.DailyChargeList DChargeList = ReservationRoomStay.ExpectedCharges != null ? ReservationRoomStay.ExpectedCharges : null;
                                if (DChargeList != null)
                                {
                                    try
                                    {
                                        List<Models.OWS.DailyRates> DailyRates = new List<Models.OWS.DailyRates>();
                                        if (DChargeList.ChargesForPostingDate != null)
                                        {
                                            foreach (ReservationService.ChargesForTheDay DayCharge in DChargeList.ChargesForPostingDate)
                                            {
                                                Models.OWS.DailyRates dailyRate = null;
                                                if (DayCharge.PostingDateSpecified)
                                                {
                                                    ReservationService.ChargeList RoomCharges = DayCharge.RoomRateAndPackages != null ? DayCharge.RoomRateAndPackages : null;
                                                    if (RoomCharges != null)
                                                    {
                                                        dailyRate = new Models.OWS.DailyRates();
                                                        dailyRate.description = "Room Charge";
                                                        dailyRate.Amount = (decimal)RoomCharges.TotalCharges;
                                                        dailyRate.PostingDate = DayCharge.PostingDate;
                                                        DailyRates.Add(dailyRate);
                                                    }

                                                    ReservationService.ChargeList TaxCharges = DayCharge.TaxesAndFees != null ? DayCharge.TaxesAndFees : null;
                                                    if (TaxCharges != null)
                                                    {
                                                        if (TaxCharges.Charges != null)
                                                        {
                                                            foreach (ReservationService.Charge Chg in TaxCharges.Charges)
                                                            {
                                                                if (Chg.CodeType != null && !string.IsNullOrEmpty(Chg.Description))
                                                                {
                                                                    dailyRate = new Models.OWS.DailyRates();
                                                                    Amnt = (ReservationService.Amount)Chg.Amount;
                                                                    dailyRate.Amount = Amnt.ToString() != null ? (decimal)Amnt.Value : 0;
                                                                    dailyRate.description = Chg.Description;
                                                                    dailyRate.PostingDate = DayCharge.PostingDate;
                                                                    DailyRates.Add(dailyRate);
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        Reservation.RateDetails = new Models.OWS.RateDetails()
                                        {
                                            DailyRates = DailyRates
                                        };
                                    }
                                    catch (Exception ex)
                                    {
                                        //NLog with Debug
                                        
                                        Reservation.RateDetails = new Models.OWS.RateDetails();
                                    }
                                }
                                #endregion

                                foreach (ReservationService.RoomType rmType in ReservationRoomStay.RoomTypes)
                                {
                                    Reservation.RoomDetails = new Models.OWS.RoomDetails()
                                    {
                                        RoomStatus = rmType.roomStatus
                                    };

                                    #region RoomNumber
                                    string[] roomNo = rmType.RoomNumber;
                                    if (roomNo != null)
                                        Reservation.RoomDetails.RoomNumber = roomNo.Length > 0 ? roomNo[0] : "";
                                    #endregion

                                    #region Room Type
                                    Reservation.RoomDetails.RoomType = rmType.roomTypeCode;
                                    Reservation.RoomDetails.RTC = rmType.roomToChargeCode;
                                    #endregion

                                    #region Room type description
                                    p = rmType.RoomTypeDescription;
                                    if (p != null)
                                    {
                                        ReservationService.Text obj = (ReservationService.Text)p.Items[0];
                                        if (obj != null)
                                            Reservation.RoomDetails.RoomTypeDescription = obj.Value;
                                    }

                                    p = rmType.RoomTypeShortDescription;
                                    if (p != null)
                                    {
                                        ReservationService.Text obj = (ReservationService.Text)p.Items[0];
                                        if (obj != null)
                                            Reservation.RoomDetails.RoomTypeShortDescription = obj.Value;
                                    }
                                    #endregion

                                    break;
                                }


                                foreach (ReservationService.RoomRate rmRate in ReservationRoomStay.RoomRates)
                                {
                                    #region  Rate Code
                                    if (Reservation.RateDetails != null)
                                        Reservation.RateDetails.RateCode = rmRate.ratePlanCode;
                                    else
                                        Reservation.RateDetails = new Models.OWS.RateDetails()
                                        {
                                            RateCode = rmRate.ratePlanCode
                                        };
                                    #endregion

                                    if (ReservationRoomStay.RoomRates.Length > 1)
                                    {
                                        if (Reservation.RateDetails != null)
                                        {
                                            Reservation.RateDetails.IsMultipleRate = true;
                                        }
                                        else
                                        {
                                            Reservation.RateDetails = new Models.OWS.RateDetails()
                                            {
                                                IsMultipleRate = true
                                            };
                                        }
                                    }
                                    else
                                    {
                                        if (Reservation.RateDetails != null)
                                        {
                                            Reservation.RateDetails.IsMultipleRate = false;
                                        }
                                        else
                                        {
                                            Reservation.RateDetails = new Models.OWS.RateDetails()
                                            {
                                                IsMultipleRate = false
                                            };
                                        }
                                    }

                                    #region Room Rate
                                    if (rmRate.Rates != null)
                                    {

                                        ReservationService.Amount amnt = (ReservationService.Amount)rmRate.Rates[0].Base;
                                        if (amnt != null)
                                        {
                                            if (Reservation.RateDetails != null)
                                            {
                                                Reservation.RateDetails.RateAmount = (decimal)amnt.Value;
                                            }
                                            else
                                            {
                                                Reservation.RateDetails = new Models.OWS.RateDetails()
                                                {
                                                    RateAmount = (decimal)amnt.Value
                                                };
                                            }
                                        }
                                    }
                                    #endregion

                                    break;
                                }

                                #region Special Prefernces
                                if (ReservationRoomStay.SpecialRequests != null)
                                {
                                    foreach (ReservationService.SpecialRequest SR in ReservationRoomStay.SpecialRequests)
                                    {
                                        Models.OWS.PreferanceDetails RPrefernce = new Models.OWS.PreferanceDetails();
                                        RPrefernce.PreferanceCode = SR.requestCode;
                                        RPreferencesList.Add(RPrefernce);
                                    }
                                }
                                #endregion

                                #region Package Code,Description,Amount
                                if (ReservationRoomStay.Packages != null)
                                {
                                    foreach (ReservationService.PackageElement pe in ReservationRoomStay.Packages)
                                    {
                                        if (!string.IsNullOrEmpty(pe.packageCode))
                                        {
                                            Models.OWS.PackageDetails pk = new Models.OWS.PackageDetails();
                                            pk.PackageCode = pe.packageCode;
                                            ReservationService.Amount pckAmount = pe.PackageAmount;
                                            if (pckAmount != null)
                                                pk.TotalPackageAmount = (decimal)pckAmount.Value;                                           
                                            RPackagesList.Add(pk);
                                        }
                                    }
                                }
                                #endregion

                                #region Adult and child count
                                foreach (ReservationService.GuestCount gc in ReservationRoomStay.GuestCounts.GuestCount)
                                {
                                    if (gc.ageQualifyingCode == ReservationService.AgeQualifyingCode.ADULT)
                                        Reservation.Adults = gc.count;
                                    else if (gc.ageQualifyingCode == ReservationService.AgeQualifyingCode.CHILD)
                                    {
                                        if (Reservation.Child == null)
                                            Reservation.Child = gc.count;
                                        else
                                            Reservation.Child += gc.count;
                                    }
                                }
                                #endregion

                                #region ReservationType
                                if (ReservationRoomStay.Guarantee != null)
                                {
                                    ReservationService.Guarantee ResType = ReservationRoomStay.Guarantee;
                                    Reservation.ReservationType = ResType.guaranteeType;
                                }
                                #endregion

                                #region Payment type
                                if (ReservationRoomStay.Payment != null)
                                {
                                    ReservationService.Payment reservationPayment = new ReservationService.Payment();
                                    reservationPayment = ReservationRoomStay.Payment;
                                    ReservationService.PaymentType[] pt = new ReservationService.PaymentType[1];
                                    ReservationService.OtherPaymentType opt = new ReservationService.OtherPaymentType();
                                    pt = (ReservationService.PaymentType[])reservationPayment.Item;
                                    opt = (ReservationService.OtherPaymentType)pt[0].Item;
                                    Reservation.PaymentMethod = new Models.OWS.PaymentMethod()
                                    {
                                        PaymentType = opt.type
                                    };
                                }
                                #endregion
                            }
                            #endregion


                            #region List Of Guest Profile

                            foreach (ReservationService.ResGuest rGuest in hReservation.ResGuests)
                            {
                                int count = 0;

                                Reservation.ExpectedArrivalTime = rGuest.arrivalTime;

                                foreach (ReservationService.Profile gProfile in rGuest.Profiles)
                                {
                                    if (gProfile.Item != null)
                                    {
                                        Type classType = gProfile.Item.GetType();
                                        if (classType.Name.Equals("Customer"))
                                        {
                                            Models.OWS.GuestProfile guestProfile = new Models.OWS.GuestProfile();
                                            ReservationService.Customer customerProf = (ReservationService.Customer)gProfile.Item;

                                            #region Profile ID
                                            guestProfile.PmsProfileID = gProfile.ProfileIDs[0].Value;
                                            
                                            #endregion

                                            #region Fetch Profile details seperately


                                            #region Guest name
                                            string tempName = "";
                                            tempName = customerProf.PersonName.nameTitle != null ? customerProf.PersonName.nameTitle[0] + " " : tempName + "";
                                            tempName = customerProf.PersonName.firstName != null ? tempName + customerProf.PersonName.firstName + " " : tempName + "";
                                            tempName = customerProf.PersonName.middleName != null ? tempName + customerProf.PersonName.middleName[0] + " " : tempName + "";
                                            tempName = customerProf.PersonName.lastName != null ? tempName + customerProf.PersonName.lastName : tempName + "";
                                            guestProfile.GuestName = tempName;
                                            guestProfile.GivenName = customerProf.PersonName.middleName != null ?
                                                customerProf.PersonName.firstName + " " + customerProf.PersonName.middleName[0] :
                                                customerProf.PersonName.firstName;
                                            guestProfile.FamilyName = customerProf.PersonName.lastName;
                                            guestProfile.FirstName = customerProf.PersonName.firstName;
                                            guestProfile.MiddleName = customerProf.PersonName.middleName != null ? customerProf.PersonName.middleName[0] : "";
                                            guestProfile.LastName = customerProf.PersonName.lastName;
                                            guestProfile.Title = customerProf.PersonName.nameTitle != null ? customerProf.PersonName.nameTitle[0] : null;
                                            #endregion

                                            #region Nationality
                                            guestProfile.Nationality = gProfile.nationality;
                                            #endregion

                                            #region Gender
                                            guestProfile.Gender = customerProf.gender.ToString();
                                            #endregion

                                            guestProfile.VipCode = gProfile.vipCode;

                                            #endregion

                                            #region Mebership

                                            if (gProfile.Memberships != null)
                                            {
                                                try
                                                {
                                                    //System.IO.File.AppendAllLines(System.Web.Hosting.HostingEnvironment.MapPath(@"~\FetchReservation.txt"), new string[] { "Member ship Details found :- " });
                                                    foreach (ReservationService.NameMembership MS in gProfile.Memberships)
                                                    {
                                                        if (MS.usedInReservationSpecified && MS.usedInReservation)
                                                        {
                                                            guestProfile.MembershipType = MS.membershipType;
                                                            guestProfile.MembershipNumber = MS.membershipNumber;
                                                            ReservationService.UniqueID Uid = MS.membershipid;
                                                            guestProfile.MembershipID = Uid.Value;
                                                            guestProfile.MembershipName = MS.memberName;
                                                            guestProfile.MembershipClass = MS.membershipClass;
                                                            guestProfile.MembershipLevel = MS.membershipLevel;

                                                        }
                                                    }
                                                }
                                                catch (Exception ex)
                                                {
                                                    //System.IO.File.AppendAllLines(System.Web.Hosting.HostingEnvironment.MapPath(@"~\FetchReservation.txt"), new string[] { "Error :- ", "", ex.Message });
                                                }
                                            }
                                            #endregion

                                            if (count == 0)
                                                guestProfile.IsPrimary = true;
                                            else
                                                guestProfile.IsPrimary = false;

                                            GPList.Add(guestProfile);
                                            count++;
                                        }
                                    }
                                }

                                break;
                            }
                            #endregion

                            #region Accompanying Guest

                            if (hReservation.AccompanyGuests != null)
                            {
                                foreach (ReservationService.AccompanyGuest AG in hReservation.AccompanyGuests)
                                {
                                    Models.OWS.GuestProfile guestProfile = new Models.OWS.GuestProfile();

                                    ReservationService.UniqueID UID = new ReservationService.UniqueID();
                                    UID.Value = AG.NameID.Value;

                                    Models.OWS.OwsResponseModel SResponse = new Models.OWS.OwsResponseModel();
                                    Models.OWS.OwsRequestModel PRequest = new Models.OWS.OwsRequestModel();
                                    PRequest.HotelDomain = Request.HotelDomain;
                                    PRequest.KioskID = Request.KioskID;
                                    PRequest.Password = Request.Password;
                                    PRequest.Username = Request.Username;
                                    PRequest.Language = Request.Language;
                                    PRequest.LegNumber = Request.LegNumber;
                                    PRequest.fetchProfileRequest = new Models.OWS.FetchProfileRequest()
                                    {
                                        NameID = UID.Value
                                    };
                                    PRequest.SystemType = Request.SystemType;
                                    SResponse = FetchProfileWithProfileID(PRequest);
                                    if (SResponse.result)
                                    {
                                        if (SResponse.responseData != null)
                                            guestProfile = (Models.OWS.GuestProfile)SResponse.responseData;
                                        else
                                        {
                                            guestProfile.PmsProfileID = UID.Value;
                                            guestProfile.IsPrimary = false;
                                            guestProfile.FirstName = AG.FirstName;
                                            guestProfile.LastName = AG.LastName;
                                        }
                                    }
                                    else
                                    {
                                        guestProfile.PmsProfileID = UID.Value;
                                        guestProfile.IsPrimary = false;
                                        guestProfile.FirstName = AG.FirstName;
                                        guestProfile.LastName = AG.LastName;
                                    }
                                    GPList.Add(guestProfile);
                                }
                            }
                            #endregion

                            #region User Defined Fields
                            if (hReservation.UserDefinedValues != null)
                            {
                                foreach (ReservationService.UserDefinedValue UDFValues in hReservation.UserDefinedValues)
                                {
                                    try
                                    {

                                        if (UDFValues.valueName.Equals("Mealplan"))
                                        {
                                            Models.OWS.PackageDetails pk = new Models.OWS.PackageDetails();
                                            pk.PackageDescription = UDFValues.valueName;
                                            pk.PackageCode = UDFValues.Item.ToString();
                                            pk.TotalPackageAmount = 0;
                                            RPackagesList.Add(pk);
                                        }

                                    }
                                    catch (Exception ex)
                                    {
                                        //Nlog debug
                                        //System.IO.File.AppendAllText(System.Web.Hosting.HostingEnvironment.MapPath(@"~\log.txt"), "Inside meal plan " + ex.Message);
                                    }
                                }
                            }
                            #endregion

                            List<Models.OWS.UserDefinedFields> userDefinedFields = new List<Models.OWS.UserDefinedFields>();

                            if (hReservation.UserDefinedValues != null)
                            {

                                foreach (ReservationService.UserDefinedValue UDF in hReservation.UserDefinedValues)
                                {
                                    Models.OWS.UserDefinedFields UFields = new Models.OWS.UserDefinedFields();
                                    UFields.FieldName = UDF.valueName;
                                    UFields.FieldValue = UDF.Item.ToString();
                                    userDefinedFields.Add(UFields);
                                }
                            }
                            if (hReservation.shareExistsSpecified)
                            {
                                if (hReservation.shareExists)
                                {
                                    foreach (ReservationService.ShareReservation shareReservation in hReservation.ShareReservations)
                                    {
                                        //shareReservation.
                                    }
                                }
                            }

                            Reservation.userDefinedFields = userDefinedFields;
                            Reservation.GuestProfiles = GPList;
                            Reservation.PreferanceDetails = RPreferencesList;
                            Reservation.PackageDetails = RPackagesList;
                            RList.Add(Reservation);
                        }
                        return new Models.OWS.OwsResponseModel
                        {
                            responseData = RList,
                            responseMessage = "Success",
                            statusCode = 101,
                            result = true
                        };
                    }
                    catch (Exception ex)
                    {
                        //Nlog Debug
                        return new Models.OWS.OwsResponseModel
                        {
                            responseMessage = "Error in processing reservation summary : " + ex.Message,
                            statusCode = 1103,
                            result = false
                        };
                    }
                }
                else
                {
                    //Nlog Debug
                    return new Models.OWS.OwsResponseModel
                    {
                        responseMessage = status.GDSError.Value,
                        statusCode = 1101,
                        result = false
                    };
                }

            }
            catch (Exception ex)
            {
                //Nlog debug
                return new Models.OWS.OwsResponseModel
                {
                    responseMessage = "Generic Exception : " + ex.Message,
                    statusCode = 1002,
                    result = false
                };
            }
            #endregion

            #endregion
        }
        public Models.OWS.OwsResponseModel FetchProfileWithProfileID(Models.OWS.OwsRequestModel Request)
        {
            try
            {
                #region Request Header
                string temp = Helper.Helper.Get8Digits();
                NameService.OGHeader OGHeader = new NameService.OGHeader();
                OGHeader.transactionID = temp;
                OGHeader.timeStamp = DateTime.Now;
                OGHeader.primaryLangID = Request.Language; //English
                NameService.EndPoint orginEndPOint = new NameService.EndPoint();
                orginEndPOint.entityID = Request.KioskID; //Kiosk Identifier
                orginEndPOint.systemType = Request.SystemType;
                OGHeader.Origin = orginEndPOint;
                NameService.EndPoint destEndPOint = new NameService.EndPoint();
                destEndPOint.entityID = Request.DestinationEntityID;
                destEndPOint.systemType = Request.DestinationSystemType;
                OGHeader.Destination = destEndPOint;
                NameService.OGHeaderAuthentication Auth = new NameService.OGHeaderAuthentication();
                NameService.OGHeaderAuthenticationUserCredentials userCredentials = new NameService.OGHeaderAuthenticationUserCredentials();
                userCredentials.UserName = Request.Username;
                userCredentials.UserPassword = Request.Password;
                userCredentials.Domain = Request.HotelDomain;
                Auth.UserCredentials = userCredentials;
                OGHeader.Authentication = Auth;
                #endregion

                #region Request Body

                NameService.FetchProfileRequest FetchRequest = new NameService.FetchProfileRequest();


                if (Request.fetchProfileRequest != null)
                {
                    NameService.UniqueID uID = new NameService.UniqueID();
                    uID.type = NameService.UniqueIDType.INTERNAL;
                    uID.Value = Request.fetchProfileRequest.NameID;
                    FetchRequest.NameID = uID;
                }
                //GMRequest.ReturnAllMessages = true;
                //GMRequest.ReturnAllMessagesSpecified = true;
                #endregion

                NameService.NameServiceSoapClient PortClient = new NameService.NameServiceSoapClient();
                bool isOperaCloudEnabled = false;
                isOperaCloudEnabled = (ConfigurationManager.AppSettings["OperaCloudEnabled"] != null
                                && !string.IsNullOrEmpty(ConfigurationManager.AppSettings["OperaCloudEnabled"].ToString())
                                && bool.TryParse(ConfigurationManager.AppSettings["OperaCloudEnabled"].ToString(), out isOperaCloudEnabled)) ? isOperaCloudEnabled : false;
                if (isOperaCloudEnabled)
                {
                    PortClient.Endpoint.Behaviors.Add(new Helper.CustomEndpointBehaviour(ConfigurationManager.AppSettings["WSSEUserName"].ToString(),
                                            ConfigurationManager.AppSettings["WSSEPassword"].ToString(),
                                            Request.Username, Request.Password, Request.HotelDomain));
                }
                NameService.FetchProfileResponse FetchResponse = new NameService.FetchProfileResponse();
                FetchResponse = PortClient.FetchProfile(ref OGHeader, FetchRequest);
                if (FetchResponse.Result.resultStatusFlag == NameService.ResultStatusFlag.SUCCESS)
                {
                    Models.OWS.OwsResponseModel owsResponse = new Models.OWS.OwsResponseModel();
                    Models.OWS.GuestProfile GuestProfileDetails = new Models.OWS.GuestProfile();
                    List<Models.OWS.Address> GuestAddressList = new List<Models.OWS.Address>();
                    List<Models.OWS.Email> GuestEmailList = new List<Models.OWS.Email>();
                    List<Models.OWS.Phone> GuestPhoneList = new List<Models.OWS.Phone>();

                    NameService.Profile GuestProfile = FetchResponse.ProfileDetails;
                    GuestProfileDetails.IsActive = GuestProfile.activeSpecified ? GuestProfile.active : false;
                    GuestProfileDetails.Nationality = GuestProfile.nationality;
                    foreach (NameService.UniqueID UID in GuestProfile.ProfileIDs)
                    {
                        if (UID.type == NameService.UniqueIDType.INTERNAL)
                        {
                            GuestProfileDetails.PmsProfileID = UID.Value;
                        }
                    }

                    if (GuestProfile.Phones != null)
                    {
                        foreach (NameService.NamePhone Phone in GuestProfile.Phones)
                        {
                            Models.OWS.Phone Phones = new Models.OWS.Phone();
                            Phones.displaySequence = Phone.displaySequence;
                            Phones.operaId = Phone.operaId;
                            Phones.PhoneNumber = Phone.Item.ToString();
                            Phones.phoneRole = Phone.phoneRole;
                            Phones.phoneType = Phone.phoneType;
                            Phones.primary = Phone.primary;
                            GuestPhoneList.Add(Phones);
                        }
                        GuestProfileDetails.Phones = GuestPhoneList;
                    }

                    if (GuestProfile.Addresses != null)
                    {
                        foreach (NameService.NameAddress GuestAddress in GuestProfile.Addresses)
                        {
                            Models.OWS.Address GuestAddr = new Models.OWS.Address();
                            GuestAddr.displaySequence = GuestAddress.displaySequence;
                            GuestAddr.operaId = GuestAddress.operaId;
                            GuestAddr.address1 = GuestAddress.AddressLine != null ? GuestAddress.AddressLine[0] : null;
                            GuestAddr.address2 = GuestAddress.AddressLine != null && GuestAddress.AddressLine.Length >= 2 ? GuestAddress.AddressLine[1] : null;
                            GuestAddr.addressType = GuestAddress.addressType;
                            GuestAddr.city = GuestAddress.cityName;
                            GuestAddr.state = GuestAddress.stateProv;
                            GuestAddr.zip = GuestAddress.postalCode;
                            GuestAddr.country = GuestAddress.countryCode;
                            GuestAddr.primary = GuestAddress.primary;
                            GuestAddressList.Add(GuestAddr);
                        }
                    }
                    GuestProfileDetails.Address = GuestAddressList;

                    #region Mebership

                    if (GuestProfile.Memberships != null)
                    {
                        try
                        {
                            foreach (NameService.NameMembership MS in GuestProfile.Memberships)
                            {
                                if (MS.usedInReservationSpecified && MS.usedInReservation)
                                {
                                    GuestProfileDetails.MembershipType = MS.membershipType;
                                    GuestProfileDetails.MembershipNumber = MS.membershipNumber;
                                    NameService.UniqueID Uid = MS.membershipid;
                                    GuestProfileDetails.MembershipID = Uid.Value;
                                    GuestProfileDetails.MembershipClass = MS.membershipClass;
                                    GuestProfileDetails.MembershipLevel = MS.membershipLevel;
                                    GuestProfileDetails.MembershipName = MS.memberName;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            //Nlog Debug
                            //System.IO.File.AppendAllLines(System.Web.Hosting.HostingEnvironment.MapPath(@"~\FetchReservation.txt"), new string[] { "Error :- ", "", ex.Message });
                        }
                    }
                    #endregion

                    if (GuestProfile.EMails != null)
                    {
                        foreach (NameService.NameEmail NEmail in GuestProfile.EMails)
                        {
                            Models.OWS.Email Email = new Models.OWS.Email();
                            Email.displaySequence = NEmail.displaySequence;
                            Email.operaId = NEmail.operaId;
                            Email.email = NEmail.Value;
                            Email.emailType = NEmail.emailType;
                            Email.primary = NEmail.primary;
                            GuestEmailList.Add(Email);
                        }
                    }
                    GuestProfileDetails.Email = GuestEmailList;
                    NameService.GovernmentID GuestIDInfo = GuestProfile.Id;
                    if (GuestIDInfo != null)
                    {
                        GuestProfileDetails.IssueCountry = GuestIDInfo.countryOfIssue;
                        GuestProfileDetails.PassportNumber = GuestIDInfo.documentNumber;
                        GuestProfileDetails.DocumentType = GuestIDInfo.documentType;
                        GuestProfileDetails.IsPrimary = GuestIDInfo.primarySpecified ? GuestIDInfo.primary : false;
                    }
                    Type classType = GuestProfile.Item.GetType();
                    if (classType != null && classType.Name.Equals("Customer"))
                    {
                        NameService.Customer customerProf = (NameService.Customer)GuestProfile.Item;
                        GuestProfileDetails.BirthDate = customerProf.birthDate.ToString("yyyy-MM-dd");

                        NameService.PersonName PName = customerProf.PersonName;
                        GuestProfileDetails.FirstName = PName.firstName;
                        GuestProfileDetails.MiddleName = (PName.middleName != null && PName.middleName.Length > 0) ? string.Join(" ", PName.middleName) : null;
                        GuestProfileDetails.LastName = PName.lastName;
                        GuestProfileDetails.Title = (PName.nameTitle != null && PName.nameTitle.Length > 0) ? string.Join(" ", PName.nameTitle) : null;

                        string tempName = "";
                        tempName = GuestProfileDetails.Title != null ? GuestProfileDetails.Title + " " : tempName + "";
                        tempName = GuestProfileDetails.FirstName != null ? tempName + GuestProfileDetails.FirstName + " " : tempName + "";
                        tempName = GuestProfileDetails.MiddleName != null ? tempName + GuestProfileDetails.MiddleName + " " : tempName + "";
                        tempName = GuestProfileDetails.LastName != null ? tempName + GuestProfileDetails.LastName : tempName + "";

                        GuestProfileDetails.GuestName = tempName;

                        if (customerProf.genderSpecified)
                        {
                            if (customerProf.gender == NameService.Gender.MALE)
                                GuestProfileDetails.Gender = "male";
                            else if (customerProf.gender == NameService.Gender.FEMALE)
                                GuestProfileDetails.Gender = "female";

                        }
                        if (customerProf.GovernmentIDList != null)
                        {
                            foreach (NameService.GovernmentID GID in customerProf.GovernmentIDList)
                            {
                                if ((GID.primarySpecified && GID.primary) || (!GID.primarySpecified))
                                {
                                    if (string.IsNullOrEmpty(GuestProfileDetails.IssueCountry))
                                        GuestProfileDetails.IssueCountry = GID.countryOfIssue;
                                }
                            }
                        }
                        //Nlog Debug
                        owsResponse.responseData = GuestProfileDetails;
                        owsResponse.responseMessage = "Success";
                        owsResponse.result = true;
                        owsResponse.statusCode = 101;
                    }
                    return owsResponse;
                }
                else
                {
                    return new Models.OWS.OwsResponseModel
                    {
                        //Nlog debug
                        responseMessage = FetchResponse.Result != null ? FetchResponse.Result.Text[0].Value : "Failled",
                        statusCode = 2001,
                        result = false
                    };
                }


            }
            catch (Exception ex)
            {
                return new Models.OWS.OwsResponseModel
                {
                    //Nlog debug
                    responseMessage = ex.Message,
                    statusCode = 2001,
                    result = false
                };
            }
        }

        public Models.OWS.OwsResponseModel FetchRoomList(Models.OWS.OwsRequestModel Request)
        {
            try
            {
                List<Models.OWS.RoomDetails> LRoomTypes = new List<Models.OWS.RoomDetails>();
                int x = 0;
                foreach (string roomTypeCode in Request.FetchRoomList.RoomTypes)
                {

                    #region Request Header
                    string temp = Helper.Helper.Get8Digits();
                    ReservationAdvancedService.OGHeader OGHeader = new ReservationAdvancedService.OGHeader();
                    OGHeader.transactionID = temp;
                    OGHeader.timeStamp = DateTime.Now;
                    OGHeader.primaryLangID = Request.Language; //English
                    ReservationAdvancedService.EndPoint orginEndPOint = new ReservationAdvancedService.EndPoint();
                    orginEndPOint.entityID = Request.KioskID; //Kiosk Identifier
                    orginEndPOint.systemType = Request.SystemType;
                    OGHeader.Origin = orginEndPOint;
                    ReservationAdvancedService.EndPoint destEndPOint = new ReservationAdvancedService.EndPoint();
                    destEndPOint.entityID = Request.DestinationEntityID;
                    destEndPOint.systemType = Request.DestinationSystemType;
                    OGHeader.Destination = destEndPOint;
                    ReservationAdvancedService.OGHeaderAuthentication Auth = new ReservationAdvancedService.OGHeaderAuthentication();
                    ReservationAdvancedService.OGHeaderAuthenticationUserCredentials userCredentials = new ReservationAdvancedService.OGHeaderAuthenticationUserCredentials();
                    userCredentials.UserName = Request.Username;
                    userCredentials.UserPassword = Request.Password;
                    userCredentials.Domain = Request.HotelDomain;
                    Auth.UserCredentials = userCredentials;
                    OGHeader.Authentication = Auth;
                    #endregion

                    #region Request Body


                    x++;
                    ReservationAdvancedService.FetchRoomStatusRequest RSRequest = new ReservationAdvancedService.FetchRoomStatusRequest();
                    RSRequest.RoomType = roomTypeCode;

                    ReservationAdvancedService.HotelReference HF = new ReservationAdvancedService.HotelReference();
                    HF.hotelCode = Request.HotelDomain;
                    RSRequest.HotelReference = HF;

                    RSRequest.IncludePseudoRoom = false;
                    RSRequest.IncludePseudoRoomSpecified = true;
                    //ReservationAdvancedService.RoomFeature roomFeature = new ReservationAdvancedService.RoomFeature()
                    //{
                        
                    //}
                    //RSRequest.Features features = new RSRequest.Features[]
                    ReservationAdvancedService.ResvAdvancedServiceSoapClient ResAdvPortClient = new ReservationAdvancedService.ResvAdvancedServiceSoapClient();
                    bool isOperaCloudEnabled = false;
                    isOperaCloudEnabled = (ConfigurationManager.AppSettings["OperaCloudEnabled"] != null
                                    && !string.IsNullOrEmpty(ConfigurationManager.AppSettings["OperaCloudEnabled"].ToString())
                                    && bool.TryParse(ConfigurationManager.AppSettings["OperaCloudEnabled"].ToString(), out isOperaCloudEnabled)) ? isOperaCloudEnabled : false;
                    if (isOperaCloudEnabled)
                    {
                        ResAdvPortClient.Endpoint.Behaviors.Add(new Helper.CustomEndpointBehaviour(ConfigurationManager.AppSettings["WSSEUserName"].ToString(),
                                                ConfigurationManager.AppSettings["WSSEPassword"].ToString(),
                                                Request.Username, Request.Password, Request.HotelDomain));
                    }
                    ReservationAdvancedService.FetchRoomStatusResponse RSResponse = new ReservationAdvancedService.FetchRoomStatusResponse();
                    #endregion

                    RSResponse = ResAdvPortClient.FetchRoomStatus(ref OGHeader, RSRequest);
                    
                    if (RSResponse.Result.resultStatusFlag == ReservationAdvancedService.ResultStatusFlag.SUCCESS)
                    {
                        foreach (ReservationAdvancedService.RoomStatus RS in RSResponse.RoomStatus)
                        {
                            Models.OWS.RoomDetails RT = new Models.OWS.RoomDetails();

                            if ((RS.RoomStatus1 == "CL" || RS.RoomStatus1 == "IP") 
                                && RS.FrontOfficeStatus == "VAC")//|| RSResponse.RoomStatus[0].RoomStatus1 == "IP"
                            {
                                if (RS.NextReservationDateSpecified)
                                {
                                    
                                    if (Request.FetchRoomList.DepartureDate != null)
                                    {
                                        DateTime dt = Request.FetchRoomList.DepartureDate.Value;//DateTime.ParseExact(Request.DepartureDate, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);

                                        if (DateTime.Compare(dt, RS.NextReservationDate) < 0)
                                        {
                                            RT.RoomNumber = RS.RoomNumber;
                                            RT.RoomType = RS.RoomType;
                                            RT.RoomStatus = RS.RoomStatus1;
                                            RT.Floor = RS.Floor;
                                            
                                            LRoomTypes.Add(RT);
                                        }
                                    }
                                    else
                                    {
                                        RT.RoomNumber = RS.RoomNumber;
                                        RT.RoomType = RS.RoomType;
                                        RT.Floor = RS.Floor;
                                        RT.RoomStatus = RS.RoomStatus1;
                                        LRoomTypes.Add(RT);
                                    }
                                }
                                else
                                {
                                    RT.RoomNumber = RS.RoomNumber;
                                    RT.RoomType = RS.RoomType;
                                    RT.Floor = RS.Floor;
                                    RT.RoomStatus = RS.RoomStatus1;
                                    LRoomTypes.Add(RT);
                                }
                            }
                        }
                        

                    }
                    else
                    {
                        if (LRoomTypes.Count == 0)
                        {
                            return new Models.OWS.OwsResponseModel
                            {
                                responseMessage = RSResponse.Result != null ? RSResponse.Result.Text[0].Value : "Failled",
                                statusCode = 2001,
                                result = false
                            };
                        }
                    }
                }
                if (LRoomTypes.Count > 0)
                {
                    return new Models.OWS.OwsResponseModel()
                    {
                        responseData = LRoomTypes,
                        result = true,
                        statusCode = 101
                    };

                }
                else
                {
                    return new Models.OWS.OwsResponseModel()
                    {
                        
                    responseMessage = "No Vaccant or availavle room for check-in",
                    statusCode = 102,
                    result = true
                    };
                }
            }
            catch (Exception ex)
            {
                return new Models.OWS.OwsResponseModel()
                {

                    responseMessage = ex.Message,
                    statusCode = -1,
                    result = false
                };
            }
        }

        public Models.OWS.OwsResponseModel GuestCheckIn(Models.OWS.OwsRequestModel Request)
        {
            try
            {
                //System.IO.File.AppendAllLines(System.Web.Hosting.HostingEnvironment.MapPath(@"~\GuestCheckin.txt"), new string[] { JsonConvert.SerializeObject(Request) });

                #region Request
                #region Request Header
                string temp = Helper.Helper.Get8Digits();
                ReservationAdvancedService.OGHeader OGHeader = new ReservationAdvancedService.OGHeader();
                OGHeader.transactionID = temp;
                OGHeader.timeStamp = DateTime.Now;
                OGHeader.primaryLangID = Request.Language; //English
                ReservationAdvancedService.EndPoint orginEndPOint = new ReservationAdvancedService.EndPoint();
                orginEndPOint.entityID = Request.KioskID; //Kiosk Identifier
                orginEndPOint.systemType = Request.SystemType;
                OGHeader.Origin = orginEndPOint;
                ReservationAdvancedService.EndPoint destEndPOint = new ReservationAdvancedService.EndPoint();
                destEndPOint.entityID = Request.DestinationEntityID;
                destEndPOint.systemType = Request.DestinationSystemType;
                OGHeader.Destination = destEndPOint;
                ReservationAdvancedService.OGHeaderAuthentication Auth = new ReservationAdvancedService.OGHeaderAuthentication();
                ReservationAdvancedService.OGHeaderAuthenticationUserCredentials userCredentials = new ReservationAdvancedService.OGHeaderAuthenticationUserCredentials();
                userCredentials.UserName = Request.Username;
                userCredentials.UserPassword = Request.Password;
                userCredentials.Domain = Request.HotelDomain;
                Auth.UserCredentials = userCredentials;
                OGHeader.Authentication = Auth;
                #endregion

                #region Request Body

                ReservationAdvancedService.CheckInRequest CIRequest = new ReservationAdvancedService.CheckInRequest();
                //CIRequest.NoPost = Request.isNoPost;
                if (Request.paymentMethod != null)
                    CIRequest.ApprovalCode = string.IsNullOrEmpty(Request.paymentMethod.AprovalCode) ? "11111" : Request.paymentMethod.AprovalCode;
                //CIRequest.EmailFolio = false;
                //CIRequest.EmailStaff = false;
                //CIRequest.GetKeyTrack = false;
                // CIRequest.Keys = 0;
                //CIRequest.PrintRegistration = false;
                //CIRequest.KeyEncoder = null;

                //CIRequest

                ReservationAdvancedService.ReservationRequestBase RB = new ReservationAdvancedService.ReservationRequestBase();

                ReservationAdvancedService.HotelReference HF = new ReservationAdvancedService.HotelReference();
                HF.chainCode = Request.ChainCode;
                HF.hotelCode = Request.HotelDomain;
                RB.HotelReference = HF;

                ReservationAdvancedService.UniqueID uID = new ReservationAdvancedService.UniqueID();
                uID.type = (ReservationAdvancedService.UniqueIDType)ReservationService.UniqueIDType.EXTERNAL;
                uID.source = "RESV_NAME_ID";
                uID.Value = Request.OperaReservation.ReservationNameID;
                ReservationAdvancedService.UniqueID[] UIDLIST = new ReservationAdvancedService.UniqueID[1];
                UIDLIST[0] = uID;
                RB.ReservationID = UIDLIST;
                CIRequest.ReservationRequest = RB;

                if (Request.paymentMethod != null)
                {
                    #region Later
                    //if (!string.IsNullOrEmpty(Request.paymentMethod.MaskedCardNumber) && !Request.paymentMethod.MaskedCardNumber.ToUpper().Equals("XXXXXXXXXXXXXXXX"))
                    //{
                    //    if (Request.isPreAuthSpecified != null && Request.isPreAuthSpecified.Value)
                    //    {
                    //        ReservationAdvancedService.Transaction transaction = new ReservationAdvancedService.Transaction();
                    //        ReservationAdvancedService.CreditCardPayment creditCardPayment = new ReservationAdvancedService.CreditCardPayment();
                    //        creditCardPayment.ApprovalCode = Request.ApprovalCode;
                    //        //creditCardPayment.cardHolderName = Request.CardHolderName;
                    //        creditCardPayment.cardType = Request.CardType;
                    //        creditCardPayment.chipAndPin = Request.ChipAndPin;
                    //        creditCardPayment.chipAndPinSpecified = true;
                    //        if (Request.PreAuthAmount != 0)
                    //        {
                    //            ReservationAdvancedService.Amount amount = new ReservationAdvancedService.Amount();
                    //            amount.decimals = 2;
                    //            amount.decimalsSpecified = true;
                    //            amount.Value = Request.PreAuthAmount;
                    //            creditCardPayment.DepositAmount = amount;
                    //        }
                    //        if (Request.CardExpiry != null)
                    //        {
                    //            creditCardPayment.expirationDate = Request.CardExpiry.Value;
                    //            creditCardPayment.expirationDateSpecified = true;
                    //        }
                    //        else
                    //        {
                    //            creditCardPayment.expirationDate = DateTime.Now.AddYears(2);
                    //            creditCardPayment.expirationDateSpecified = true;
                    //        }
                    //        creditCardPayment.Item = Request.CardNumber;
                    //        transaction.CreditCardApproved = creditCardPayment;
                    //        transaction.FolioViewNo = Request.FolioWindowsNo;
                    //        transaction.FolioViewNoSpecified = true;
                    //        ReservationAdvancedService.Transaction[] transactions = { transaction };
                    //        CIRequest.Transactions = transactions;
                    //    }

                    //    else
                    //    {
                    //        Request.CardNumber = Request.CardNumber.ToLower();
                    //        ReservationAdvancedService.CreditCardInfo CCInfo = new ReservationAdvancedService.CreditCardInfo();
                    //        ReservationAdvancedService.CreditCard CC = new ReservationAdvancedService.CreditCard();
                    //        //CC.
                    //        CC.chipAndPin = false;
                    //        CC.chipAndPinSpecified = true;
                    //        CC.cardType = Request.CardType;
                    //        CC.Item = Request.CardNumber;
                    //        if (Request.CardExpiry != null)
                    //        {
                    //            CC.expirationDate = Request.CardExpiry.Value;
                    //            CC.expirationDateSpecified = true;
                    //        }
                    //        else
                    //        {
                    //            CC.expirationDate = DateTime.Now.AddYears(2);
                    //            CC.expirationDateSpecified = true;
                    //        }
                    //        CCInfo.Item = CC;
                    //        CIRequest.CreditCardInfo = CCInfo;
                    //    }
                    //}
                    //else
                    //{

                    //}
                    #endregion
                }

                //if (!string.IsNullOrEmpty(Request.ERegCard))
                //{
                //    ReservationAdvancedService.FileData ERegcard = new ReservationAdvancedService.FileData();
                //    ERegcard.fileType = ReservationAdvancedService.FileType.PDF;
                //    byte[] toBytes = Convert.FromBase64String(Request.ERegCard);
                //    ERegcard.FileContents = toBytes;
                //    CIRequest.SignedDocument = ERegcard;

                //}

                #endregion

                #endregion

                #region Response

                ReservationAdvancedService.ResvAdvancedServiceSoapClient ResAdvPortClient = new ReservationAdvancedService.ResvAdvancedServiceSoapClient();
                bool isOperaCloudEnabled = false;
                isOperaCloudEnabled = (ConfigurationManager.AppSettings["OperaCloudEnabled"] != null
                                && !string.IsNullOrEmpty(ConfigurationManager.AppSettings["OperaCloudEnabled"].ToString())
                                && bool.TryParse(ConfigurationManager.AppSettings["OperaCloudEnabled"].ToString(), out isOperaCloudEnabled)) ? isOperaCloudEnabled : false;
                if (isOperaCloudEnabled)
                {
                    ResAdvPortClient.Endpoint.Behaviors.Add(new Helper.CustomEndpointBehaviour(ConfigurationManager.AppSettings["WSSEUserName"].ToString(),
                                            ConfigurationManager.AppSettings["WSSEPassword"].ToString(),
                                            Request.Username, Request.Password, Request.HotelDomain));
                }
                ReservationAdvancedService.CheckInResponse CIResponse = new ReservationAdvancedService.CheckInResponse();
                CIResponse = ResAdvPortClient.CheckIn(ref OGHeader, CIRequest);
                
                if (CIResponse.Result.resultStatusFlag == ReservationAdvancedService.ResultStatusFlag.SUCCESS)
                {
                    return new Models.OWS.OwsResponseModel
                    {
                        responseData = CIResponse.CheckInComplete.Room.RoomNumber,
                        responseMessage = "Success",
                        statusCode = 101,
                        result = true
                    };
                }
                else
                {

                    return new Models.OWS.OwsResponseModel
                    {
                        responseMessage = CIResponse.Result != null ? CIResponse.Result.Text[0].Value : "Check in Failled",
                        statusCode = 2001,
                        result = false
                    };

                    #region OS And DI Handled

                    //if (CIResponse.Result != null && CIResponse.Result.Text.Length > 0)
                    //{
                    //    if (CIResponse.Result.Text[0].Value.Contains("Kiosk cannot Checkin at this time Room is  DI  Please proceed to front desk for assistance"))
                    //    {

                    //        #region Modify Booking with Creditcard Number
                    //        //try
                    //        //{
                    //        //    Models.ModifyReservation ModifyReservationRequest = new Models.ModifyReservation();
                    //        //    ModifyReservationRequest.AdultCount = null;
                    //        //    ModifyReservationRequest.AdultCountSpecified = false;
                    //        //    ModifyReservationRequest.ApprovalCode = null;
                    //        //    ModifyReservationRequest.CreditCardSpecified = true;
                    //        //    ModifyReservationRequest.CreditCardNumber = Request.CardNumber;
                    //        //    ModifyReservationRequest.CreditCardType = Request.CardType;
                    //        //    ModifyReservationRequest.ChainCode = Request.ChainCode;
                    //        //    ModifyReservationRequest.CheckoutTime = null;
                    //        //    ModifyReservationRequest.CheckOutTimeSpecified = false;
                    //        //    ModifyReservationRequest.ConfirmationNo = Request.ConfirmationNo;
                    //        //    ModifyReservationRequest.HotelDomain = Request.HotelDomain;
                    //        //    ModifyReservationRequest.KioskID = Request.KioskID;
                    //        //    ModifyReservationRequest.KioskPassword = Request.KioskPassword;
                    //        //    ModifyReservationRequest.KioskUserName = Request.KioskUserName;
                    //        //    ModifyReservationRequest.Language = "EN";
                    //        //    ModifyReservationRequest.LegNumber = Request.LegNumber;
                    //        //    ModifyReservationRequest.PrimaryGuestNameID = Request.PrimaryGuestNameID;
                    //        //    ModifyReservationRequest.ReservationNameID = Request.ReservationNameID;
                    //        //    ModifyReservationRequest.SystemType = Request.SystemType;
                    //        //    ModifyReservationRequest.UDFFieldSpecified = false;
                    //        //    ModifyBooking(ModifyReservationRequest);
                    //        //}
                    //        //catch (Exception) { }
                    //        #endregion

                    //        #region GetReservationDeatils
                    //        //try
                    //        //{
                    //        //    Models.ReservationRequest RRequest = new Models.ReservationRequest();
                    //        //    RRequest.ChainCode = Request.ChainCode;
                    //        //    RRequest.ConfirmationNo = Request.ConfirmationNo;
                    //        //    RRequest.HotelDomain = Request.HotelDomain;
                    //        //    RRequest.KioskID = Request.KioskID;
                    //        //    RRequest.KioskPassword = Request.KioskPassword;
                    //        //    RRequest.KioskUserName = Request.KioskUserName;
                    //        //    RRequest.Language = "EN";
                    //        //    RRequest.LegNumber = Request.LegNumber;
                    //        //    RRequest.ReservationNameID = Request.ReservationNameID;
                    //        //    RRequest.SystemType = Request.SystemType;
                    //        //    SamsotechOWSGateway.Models.ServiceResponse ReservationDetails = new Models.ServiceResponse();
                    //        //    //OWSControllerServiceLib ServiceLib = new OWSControllerServiceLib();
                    //        //    ReservationDetails = ServiceLib.GetReservationDetailsFromPMS(RRequest);
                    //        //    return new SamsotechOWSGateway.Models.ServiceResponse
                    //        //    {
                    //        //        Data = ReservationDetails.Data != null ? ReservationDetails.Data : null,
                    //        //        ResponseMessage = CIResponse.Result != null ? CIResponse.Result.Text[0].Value : "Check in Failled",
                    //        //        StatusCode = 2001,
                    //        //        ProceedWithCheckin = true,
                    //        //        Result = false
                    //        //    };
                    //        //}
                    //        //catch (Exception)
                    //        //{
                    //        //    return new SamsotechOWSGateway.Models.ServiceResponse
                    //        //    {
                    //        //        Data = null,
                    //        //        ResponseMessage = CIResponse.Result != null ? CIResponse.Result.Text[0].Value : "Check in Failled",
                    //        //        StatusCode = 2001,
                    //        //        ProceedWithCheckin = true,
                    //        //        Result = false
                    //        //    };
                    //        //}
                    //        #endregion

                    //        //return new SamsotechOWSGateway.Models.ServiceResponse
                    //        //{
                    //        //    Data = null,
                    //        //    ResponseMessage = CIResponse.Result != null ? CIResponse.Result.Text[0].Value : "Check in Failled",
                    //        //    StatusCode = 2001,
                    //        //    ProceedWithCheckin = true,
                    //        //    Result = false
                    //        //};
                    //    }
                    //    else if (CIResponse.Result.Text[0].Value.Contains("Kiosk cannot Checkin at this time Room is  OS  Please proceed to front desk for assistance"))
                    //    {
                    //        #region Modify Booking with Creditcard Number
                    //        //try
                    //        //{
                    //        //    Models.ModifyReservation ModifyReservationRequest = new Models.ModifyReservation();
                    //        //    ModifyReservationRequest.AdultCount = null;
                    //        //    ModifyReservationRequest.AdultCountSpecified = false;
                    //        //    ModifyReservationRequest.ApprovalCode = null;
                    //        //    ModifyReservationRequest.CreditCardNumber = Request.CardNumber;
                    //        //    ModifyReservationRequest.CreditCardType = Request.CardType;
                    //        //    ModifyReservationRequest.ChainCode = Request.ChainCode;
                    //        //    ModifyReservationRequest.CheckoutTime = null;
                    //        //    ModifyReservationRequest.CheckOutTimeSpecified = false;
                    //        //    ModifyReservationRequest.ConfirmationNo = Request.ConfirmationNo;
                    //        //    ModifyReservationRequest.HotelDomain = Request.HotelDomain;
                    //        //    ModifyReservationRequest.KioskID = Request.KioskID;
                    //        //    ModifyReservationRequest.KioskPassword = Request.KioskPassword;
                    //        //    ModifyReservationRequest.KioskUserName = Request.KioskUserName;
                    //        //    ModifyReservationRequest.Language = "EN";
                    //        //    ModifyReservationRequest.LegNumber = Request.LegNumber;
                    //        //    ModifyReservationRequest.PrimaryGuestNameID = Request.PrimaryGuestNameID;
                    //        //    ModifyReservationRequest.ReservationNameID = Request.ReservationNameID;
                    //        //    ModifyReservationRequest.SystemType = Request.SystemType;
                    //        //    ModifyReservationRequest.UDFFieldSpecified = false;
                    //        //    ModifyBooking(ModifyReservationRequest);
                    //        //}
                    //        //catch (Exception) { }
                    //        #endregion

                    //        #region GetReservationDeatils
                    //        //try
                    //        //{
                    //        //    Models.ReservationRequest RRequest = new Models.ReservationRequest();
                    //        //    RRequest.ChainCode = Request.ChainCode;
                    //        //    RRequest.ConfirmationNo = Request.ConfirmationNo;
                    //        //    RRequest.HotelDomain = Request.HotelDomain;
                    //        //    RRequest.KioskID = Request.KioskID;
                    //        //    RRequest.KioskPassword = Request.KioskPassword;
                    //        //    RRequest.KioskUserName = Request.KioskUserName;
                    //        //    RRequest.Language = "EN";
                    //        //    RRequest.LegNumber = Request.LegNumber;
                    //        //    RRequest.ReservationNameID = Request.ReservationNameID;
                    //        //    RRequest.SystemType = Request.SystemType;
                    //        //    SamsotechOWSGateway.Models.ServiceResponse ReservationDetails = new Models.ServiceResponse();
                    //        //    //OWSControllerServiceLib ServiceLib = new OWSControllerServiceLib();
                    //        //    ReservationDetails = ServiceLib.GetReservationDetailsFromPMS(RRequest);
                    //        //    return new SamsotechOWSGateway.Models.ServiceResponse
                    //        //    {
                    //        //        Data = ReservationDetails.Data != null ? ReservationDetails.Data : null,
                    //        //        ResponseMessage = CIResponse.Result != null ? CIResponse.Result.Text[0].Value : "Check in Failled",
                    //        //        StatusCode = 2001,
                    //        //        ProceedWithCheckin = true,
                    //        //        Result = false
                    //        //    };
                    //        //}
                    //        //catch (Exception)
                    //        //{
                    //        //    return new SamsotechOWSGateway.Models.ServiceResponse
                    //        //    {
                    //        //        Data = null,
                    //        //        ResponseMessage = CIResponse.Result != null ? CIResponse.Result.Text[0].Value : "Check in Failled",
                    //        //        StatusCode = 2001,
                    //        //        ProceedWithCheckin = true,
                    //        //        Result = false
                    //        //    };
                    //        //}
                    //        #endregion

                    //        //return new SamsotechOWSGateway.Models.ServiceResponse
                    //        //{
                    //        //    Data = null,
                    //        //    ResponseMessage = CIResponse.Result != null ? CIResponse.Result.Text[0].Value : "Check in Failled",
                    //        //    StatusCode = 2001,
                    //        //    ProceedWithCheckin = true,
                    //        //    Result = false
                    //        //};
                    //    }
                    //    else if (CIResponse.Result.Text[0].Value.Contains("Kiosk cannot Checkin at this time Room is  OO  Please proceed to front desk for assistance"))
                    //    {
                    //        #region Modify Booking with Creditcard Number
                    //        //try
                    //        //{
                    //        //    Models.ModifyReservation ModifyReservationRequest = new Models.ModifyReservation();
                    //        //    ModifyReservationRequest.AdultCount = null;
                    //        //    ModifyReservationRequest.AdultCountSpecified = false;
                    //        //    ModifyReservationRequest.ApprovalCode = null;
                    //        //    ModifyReservationRequest.CreditCardNumber = Request.CardNumber;
                    //        //    ModifyReservationRequest.CreditCardType = Request.CardType;
                    //        //    ModifyReservationRequest.ChainCode = Request.ChainCode;
                    //        //    ModifyReservationRequest.CheckoutTime = null;
                    //        //    ModifyReservationRequest.CheckOutTimeSpecified = false;
                    //        //    ModifyReservationRequest.ConfirmationNo = Request.ConfirmationNo;
                    //        //    ModifyReservationRequest.HotelDomain = Request.HotelDomain;
                    //        //    ModifyReservationRequest.KioskID = Request.KioskID;
                    //        //    ModifyReservationRequest.KioskPassword = Request.KioskPassword;
                    //        //    ModifyReservationRequest.KioskUserName = Request.KioskUserName;
                    //        //    ModifyReservationRequest.Language = "EN";
                    //        //    ModifyReservationRequest.LegNumber = Request.LegNumber;
                    //        //    ModifyReservationRequest.PrimaryGuestNameID = Request.PrimaryGuestNameID;
                    //        //    ModifyReservationRequest.ReservationNameID = Request.ReservationNameID;
                    //        //    ModifyReservationRequest.SystemType = Request.SystemType;
                    //        //    ModifyReservationRequest.UDFFieldSpecified = false;
                    //        //    ModifyBooking(ModifyReservationRequest);
                    //        //}
                    //        //catch (Exception) { }
                    //        #endregion

                    //        //return new SamsotechOWSGateway.Models.ServiceResponse
                    //        //{
                    //        //    Data = null,
                    //        //    ResponseMessage = CIResponse.Result != null ? CIResponse.Result.Text[0].Value : "Check in Failled",
                    //        //    StatusCode = 2001,
                    //        //    ProceedWithCheckin = true,
                    //        //    Result = false
                    //        //};

                    //        #region GetReservationDeatils
                    //        //try
                    //        //{
                    //        //    Models.ReservationRequest RRequest = new Models.ReservationRequest();
                    //        //    RRequest.ChainCode = Request.ChainCode;
                    //        //    RRequest.ConfirmationNo = Request.ConfirmationNo;
                    //        //    RRequest.HotelDomain = Request.HotelDomain;
                    //        //    RRequest.KioskID = Request.KioskID;
                    //        //    RRequest.KioskPassword = Request.KioskPassword;
                    //        //    RRequest.KioskUserName = Request.KioskUserName;
                    //        //    RRequest.Language = "EN";
                    //        //    RRequest.LegNumber = Request.LegNumber;
                    //        //    RRequest.ReservationNameID = Request.ReservationNameID;
                    //        //    RRequest.SystemType = Request.SystemType;
                    //        //    SamsotechOWSGateway.Models.ServiceResponse ReservationDetails = new Models.ServiceResponse();

                    //        //    ReservationDetails = ServiceLib.GetReservationDetailsFromPMS(RRequest);
                    //        //    return new SamsotechOWSGateway.Models.ServiceResponse
                    //        //    {
                    //        //        Data = ReservationDetails.Data != null ? ReservationDetails.Data : null,
                    //        //        ResponseMessage = CIResponse.Result != null ? CIResponse.Result.Text[0].Value : "Check in Failled",
                    //        //        StatusCode = 2001,
                    //        //        ProceedWithCheckin = true,
                    //        //        Result = false
                    //        //    };
                    //        //}
                    //        //catch (Exception)
                    //        //{
                    //        //    return new SamsotechOWSGateway.Models.ServiceResponse
                    //        //    {
                    //        //        Data = null,
                    //        //        ResponseMessage = CIResponse.Result != null ? CIResponse.Result.Text[0].Value : "Check in Failled",
                    //        //        StatusCode = 2001,
                    //        //        ProceedWithCheckin = true,
                    //        //        Result = false
                    //        //    };
                    //        //}
                    //        #endregion
                    //    }
                    //    else
                    //    {
                    //        return new SamsotechOWSGateway.Models.ServiceResponse
                    //        {
                    //            Data = null,
                    //            ResponseMessage = CIResponse.Result != null ? CIResponse.Result.Text[0].Value : "Check in Failled",
                    //            StatusCode = 2001,
                    //            ProceedWithCheckin = false,
                    //            Result = false
                    //        };
                    //    }
                    //}
                    #endregion
                }
                #endregion
            }
            catch (Exception ex)
            {
                return new Models.OWS.OwsResponseModel
                {
                    responseMessage = "Generic Exception : " + ex.Message,
                    statusCode = 1002,
                    result = false
                };
            }
        }

        public Models.OWS.OwsResponseModel FetchRoomStatus(Models.OWS.OwsRequestModel Request)
        {
            try
            {




                #region Request Header
                string temp = Helper.Helper.Get8Digits();
                ReservationAdvancedService.OGHeader OGHeader = new ReservationAdvancedService.OGHeader();
                OGHeader.transactionID = temp;
                OGHeader.timeStamp = DateTime.Now;
                OGHeader.primaryLangID = Request.Language; //English
                ReservationAdvancedService.EndPoint orginEndPOint = new ReservationAdvancedService.EndPoint();
                orginEndPOint.entityID = Request.KioskID; //Kiosk Identifier
                orginEndPOint.systemType = Request.SystemType;
                OGHeader.Origin = orginEndPOint;
                ReservationAdvancedService.EndPoint destEndPOint = new ReservationAdvancedService.EndPoint();
                destEndPOint.entityID = Request.DestinationEntityID;
                destEndPOint.systemType = Request.DestinationSystemType;
                OGHeader.Destination = destEndPOint;
                ReservationAdvancedService.OGHeaderAuthentication Auth = new ReservationAdvancedService.OGHeaderAuthentication();
                ReservationAdvancedService.OGHeaderAuthenticationUserCredentials userCredentials = new ReservationAdvancedService.OGHeaderAuthenticationUserCredentials();
                userCredentials.UserName = Request.Username;
                userCredentials.UserPassword = Request.Password;
                userCredentials.Domain = Request.HotelDomain;
                Auth.UserCredentials = userCredentials;
                OGHeader.Authentication = Auth;
                #endregion

                #region Request Body

                ReservationAdvancedService.FetchRoomStatusRequest RSRequest = new ReservationAdvancedService.FetchRoomStatusRequest();
                RSRequest.RoomType = Request.FetchRoomList.RoomType;

                RSRequest.RoomNumber = Request.FetchRoomList.RoomNumber;

                ReservationAdvancedService.HotelReference HF = new ReservationAdvancedService.HotelReference();
                HF.hotelCode = Request.HotelDomain;
                RSRequest.HotelReference = HF;

                RSRequest.IncludePseudoRoom = false;
                RSRequest.IncludePseudoRoomSpecified = true;



                ReservationAdvancedService.ResvAdvancedServiceSoapClient ResAdvPortClient = new ReservationAdvancedService.ResvAdvancedServiceSoapClient();
                bool isOperaCloudEnabled = false;
                isOperaCloudEnabled = (ConfigurationManager.AppSettings["OperaCloudEnabled"] != null
                                && !string.IsNullOrEmpty(ConfigurationManager.AppSettings["OperaCloudEnabled"].ToString())
                                && bool.TryParse(ConfigurationManager.AppSettings["OperaCloudEnabled"].ToString(), out isOperaCloudEnabled)) ? isOperaCloudEnabled : false;
                if (isOperaCloudEnabled)
                {
                    ResAdvPortClient.Endpoint.Behaviors.Add(new Helper.CustomEndpointBehaviour(ConfigurationManager.AppSettings["WSSEUserName"].ToString(),
                                            ConfigurationManager.AppSettings["WSSEPassword"].ToString(),
                                            Request.Username, Request.Password, Request.HotelDomain));
                }
                ReservationAdvancedService.FetchRoomStatusResponse RSResponse = new ReservationAdvancedService.FetchRoomStatusResponse();
                #endregion

                RSResponse = ResAdvPortClient.FetchRoomStatus(ref OGHeader, RSRequest);

                if (RSResponse.Result.resultStatusFlag == ReservationAdvancedService.ResultStatusFlag.SUCCESS)
                {
                    if (RSResponse.RoomStatus != null && RSResponse.RoomStatus.Length > 0)
                    {
                        return new OwsResponseModel()
                        {
                            responseData = new Models.OWS.RoomDetails()
                            {
                                RoomNumber = RSResponse.RoomStatus.First().RoomNumber,
                                RoomStatus = RSResponse.RoomStatus.First().RoomStatus1,
                                RoomType = RSResponse.RoomStatus.First().RoomType,
                                FORoomStatus = RSResponse.RoomStatus.First().FrontOfficeStatus
                            },
                            responseMessage = "Success",
                            result = true,
                            statusCode = 0
                        };
                    }
                    else
                    {
                        return new OwsResponseModel()
                        {
                            responseData = null,
                            responseMessage = "Failled to retreave the list",
                            result = false,
                            statusCode = -1
                        };
                    }



                }
                else
                {

                    return new Models.OWS.OwsResponseModel
                    {
                        responseMessage = RSResponse.Result != null ? RSResponse.Result.Text[0].Value : "Failled",
                        statusCode = 2001,
                        result = false
                    };

                }


            }
            catch (Exception ex)
            {
                return new Models.OWS.OwsResponseModel()
                {

                    responseMessage = ex.Message,
                    statusCode = -1,
                    result = false
                };
            }
        }

        public Models.OWS.OwsResponseModel GetMessages(Models.OWS.OwsRequestModel Request)
        {
            try
            {
                #region Request Header
                string temp = Helper.Helper.Get8Digits();
                ReservationAdvancedService.OGHeader OGHeader = new ReservationAdvancedService.OGHeader();
                OGHeader.transactionID = temp;
                OGHeader.timeStamp = DateTime.Now;
                OGHeader.primaryLangID = Request.Language; //English
                ReservationAdvancedService.EndPoint orginEndPOint = new ReservationAdvancedService.EndPoint();
                orginEndPOint.entityID = Request.KioskID; //Kiosk Identifier
                orginEndPOint.systemType = Request.SystemType;
                OGHeader.Origin = orginEndPOint;
                ReservationAdvancedService.EndPoint destEndPOint = new ReservationAdvancedService.EndPoint();
                destEndPOint.entityID = "TI";
                destEndPOint.systemType = "PMS";
                OGHeader.Destination = destEndPOint;
                ReservationAdvancedService.OGHeaderAuthentication Auth = new ReservationAdvancedService.OGHeaderAuthentication();
                ReservationAdvancedService.OGHeaderAuthenticationUserCredentials userCredentials = new ReservationAdvancedService.OGHeaderAuthenticationUserCredentials();
                userCredentials.UserName = Request.Username;
                userCredentials.UserPassword = Request.Password;
                userCredentials.Domain = Request.HotelDomain;
                Auth.UserCredentials = userCredentials;
                OGHeader.Authentication = Auth;
                #endregion

                #region Request Body

                ReservationAdvancedService.GuestMessagesRequest GMRequest = new ReservationAdvancedService.GuestMessagesRequest();
                GMRequest.LineLength = 100;
                GMRequest.LineLengthSpecified = true;

                GMRequest.ReturnAllMessages = true;
                GMRequest.ReturnAllMessagesSpecified = true;

                ReservationAdvancedService.ReservationRequestBase ReservationRequest = new ReservationAdvancedService.ReservationRequestBase();
                ReservationAdvancedService.HotelReference HF = new ReservationAdvancedService.HotelReference();
                HF.hotelCode = Request.HotelDomain;
                ReservationRequest.HotelReference = HF;


                ReservationAdvancedService.UniqueID[] rUniqueIDList = new ReservationAdvancedService.UniqueID[1];
                ReservationAdvancedService.UniqueID uID = new ReservationAdvancedService.UniqueID();
                uID.type = ReservationAdvancedService.UniqueIDType.INTERNAL;
                uID.source = "RESV_NAME_ID";
                uID.Value = Request.FetchGuestMessageRequest.ReservationNameID;
                rUniqueIDList[0] = uID;
                ReservationRequest.ReservationID = rUniqueIDList;
                GMRequest.ReservationRequest = ReservationRequest;
                GMRequest.ReturnAllMessages = true;
                GMRequest.ReturnAllMessagesSpecified = true;


                ReservationAdvancedService.ResvAdvancedServiceSoapClient ResAdvPortClient = new ReservationAdvancedService.ResvAdvancedServiceSoapClient();
                bool isOperaCloudEnabled = false;
                isOperaCloudEnabled = (ConfigurationManager.AppSettings["OperaCloudEnabled"] != null
                                && !string.IsNullOrEmpty(ConfigurationManager.AppSettings["OperaCloudEnabled"].ToString())
                                && bool.TryParse(ConfigurationManager.AppSettings["OperaCloudEnabled"].ToString(), out isOperaCloudEnabled)) ? isOperaCloudEnabled : false;
                if (isOperaCloudEnabled)
                {
                    ResAdvPortClient.Endpoint.Behaviors.Add(new Helper.CustomEndpointBehaviour(ConfigurationManager.AppSettings["WSSEUserName"].ToString(),
                                            ConfigurationManager.AppSettings["WSSEPassword"].ToString(),
                                            Request.Username, Request.Password, Request.HotelDomain));
                }
                ReservationAdvancedService.GuestMessagesResponse GMResponse = new ReservationAdvancedService.GuestMessagesResponse();
                #endregion

                GMResponse = ResAdvPortClient.GuestMessages(ref OGHeader, GMRequest);
                if (GMResponse.Result.resultStatusFlag == ReservationAdvancedService.ResultStatusFlag.SUCCESS)
                {


                    List<Models.OWS.GuestMessage> LGM = new List<Models.OWS.GuestMessage>();
                    foreach (ReservationAdvancedService.GuestMessage GM in GMResponse.GuestMessages)
                    {
                        Models.OWS.GuestMessage GuestMessages = new Models.OWS.GuestMessage();
                        GuestMessages.MessageDate = GM.Date;
                        ReservationAdvancedService.UniqueID UID = GM.GuestMessageID;
                        GuestMessages.MessageID = UID.Value;
                        GuestMessages.RecepientName = GM.RecipientName;
                        GuestMessages.Message = GM.Value;
                        GuestMessages.MessageStatus = GM.StatusFlag;
                        LGM.Add(GuestMessages);
                    }

                    return new Models.OWS.OwsResponseModel
                    {
                        responseData = LGM,
                        responseMessage = "Success",
                        result = true,
                        statusCode = 101
                    };

                }
                else
                {
                    return new Models.OWS.OwsResponseModel
                    {
                        responseData = null,
                        responseMessage = GMResponse.Result != null ? GMResponse.Result.Text[0].Value : "Failled",
                        result = false,
                        statusCode = 102
                    };

                }
            }
            catch (Exception ex)
            {
                return new Models.OWS.OwsResponseModel
                {
                    responseData = null,
                    responseMessage = ex.Message,
                    result = false,
                    statusCode = -1
                };

            }
        }

        public Models.OWS.OwsResponseModel GetComments(Models.OWS.OwsRequestModel Request)
        {
            try
            {
                #region Request Header
                string temp = Helper.Helper.Get8Digits();
                ReservationService.OGHeader OGHeader = new ReservationService.OGHeader();
                OGHeader.transactionID = temp;
                OGHeader.timeStamp = DateTime.Now;
                OGHeader.primaryLangID = Request.Language; //English
                ReservationService.EndPoint orginEndPOint = new ReservationService.EndPoint();
                orginEndPOint.entityID = Request.KioskID; //Kiosk Identifier
                orginEndPOint.systemType = Request.SystemType;
                OGHeader.Origin = orginEndPOint;
                ReservationService.EndPoint destEndPOint = new ReservationService.EndPoint();
                destEndPOint.entityID = "TI";
                destEndPOint.systemType = "PMS";
                OGHeader.Destination = destEndPOint;
                ReservationService.OGHeaderAuthentication Auth = new ReservationService.OGHeaderAuthentication();
                ReservationService.OGHeaderAuthenticationUserCredentials userCredentials = new ReservationService.OGHeaderAuthenticationUserCredentials();
                userCredentials.UserName = Request.Username;
                userCredentials.UserPassword = Request.Password;
                userCredentials.Domain = Request.HotelDomain;
                Auth.UserCredentials = userCredentials;
                OGHeader.Authentication = Auth;
                #endregion

                #region Request Body

                ReservationService.GuestRequestsRequest GRRequest = new ReservationService.GuestRequestsRequest();

                GRRequest.ActionType = ReservationService.RequestActionType.FETCH;

                ReservationService.UniqueID uID = new ReservationService.UniqueID();
                uID.type = ReservationService.UniqueIDType.INTERNAL;
                
                uID.Value = Request.FetchGuestRequest.ReservationNumber;
                GRRequest.ConfirmationNumber = uID;

                ReservationService.HotelReference HF = new ReservationService.HotelReference();
                HF.hotelCode = Request.HotelDomain;
                GRRequest.HotelReference = HF;

                GRRequest.RequestType = "COMMENTS";

               


                ReservationService.ReservationServiceSoapClient ResPortClient = new ReservationService.ReservationServiceSoapClient();
                bool isOperaCloudEnabled = false;
                isOperaCloudEnabled = (ConfigurationManager.AppSettings["OperaCloudEnabled"] != null
                                && !string.IsNullOrEmpty(ConfigurationManager.AppSettings["OperaCloudEnabled"].ToString())
                                && bool.TryParse(ConfigurationManager.AppSettings["OperaCloudEnabled"].ToString(), out isOperaCloudEnabled)) ? isOperaCloudEnabled : false;
                if (isOperaCloudEnabled)
                {
                    ResPortClient.Endpoint.Behaviors.Add(new Helper.CustomEndpointBehaviour(ConfigurationManager.AppSettings["WSSEUserName"].ToString(),
                                            ConfigurationManager.AppSettings["WSSEPassword"].ToString(),
                                            Request.Username, Request.Password, Request.HotelDomain));
                }
                ReservationService.GuestRequestsResponse GRResponse = new ReservationService.GuestRequestsResponse();
                #endregion

                GRResponse = ResPortClient.GuestRequests(ref OGHeader, GRRequest);
                if (GRResponse.Result.resultStatusFlag == ReservationService.ResultStatusFlag.SUCCESS)
                {


                    List<Models.OWS.GuestComments> LGC = new List<Models.OWS.GuestComments>();

                    if (GRResponse.GuestRequests != null && GRResponse.GuestRequests.Comments.Length > 0)
                    {
                        foreach (ReservationService.ReservationComment GC in GRResponse.GuestRequests.Comments)
                        {
                            Models.OWS.GuestComments GuestComments = new Models.OWS.GuestComments();
                            GuestComments.commentID = GC.CommentIdSpecified ?  GC.CommentId.ToString() : "";
                            if (GC.Items.Length > 0)
                            {
                                ReservationService.Text obj = (ReservationService.Text)GC.Items.First();
                                if (obj != null)
                                {
                                    GuestComments.Comment = obj.Value;
                                }
                            }
                            GuestComments.isGuestViewable = GC.guestViewableSpecified ? GC.guestViewable : false;
                            GuestComments.CommentType = GC.CommentType;
                            GuestComments.isInternal = GC.InternalYnSpecified ? GC.InternalYn : false;
                            LGC.Add(GuestComments);
                        }

                        return new Models.OWS.OwsResponseModel
                        {
                            responseData = LGC,
                            responseMessage = "Success",
                            result = true,
                            statusCode = 101
                        };
                    }
                    else
                    {
                        return new Models.OWS.OwsResponseModel
                        {
                            responseData = null,
                            responseMessage = "No guest comments found",
                            result = true,
                            statusCode = 101
                        };
                    }

                }
                else
                {
                    return new Models.OWS.OwsResponseModel
                    {
                        responseData = null,
                        responseMessage = GRResponse.Result != null ? GRResponse.Result.Text[0].Value : "Failled",
                        result = false,
                        statusCode = 102
                    };

                }
            }
            catch (Exception ex)
            {
                return new Models.OWS.OwsResponseModel
                {
                    responseData = null,
                    responseMessage = ex.Message,
                    result = false,
                    statusCode = -1
                };

            }
        }



        public Models.OWS.OwsResponseModel GetReservationDetailsFromPMS(Models.OWS.OwsRequestModel Request)
        {
            try
            {
                //Nlog Debug
                List<Models.OWS.OperaReservation> RList = new List<Models.OWS.OperaReservation>();
                List<Models.OWS.OperaReservation> SRList = new List<Models.OWS.OperaReservation>();
                List<Models.OWS.GuestProfile> GPList = new List<Models.OWS.GuestProfile>();
                List<Models.OWS.PreferanceDetails> RPreferencesList = new List<Models.OWS.PreferanceDetails>();
                List<Models.OWS.PackageDetails> RPackagesList = new List<Models.OWS.PackageDetails>();
                Models.OWS.OperaReservation Reservation = new Models.OWS.OperaReservation();

                #region Call FetchReservation by conf no
                ReservationService.FetchBookingRequest fetchBookingReq = new ReservationService.FetchBookingRequest();
                ReservationService.FetchBookingResponse fetchBookingRes = new ReservationService.FetchBookingResponse();

                #region Request Header

                string temp = Helper.Helper.Get8Digits();
                ReservationService.OGHeader OGHeader = new ReservationService.OGHeader();
                OGHeader.transactionID = temp;
                OGHeader.timeStamp = DateTime.Now;
                OGHeader.primaryLangID = Request.Language; //English
                ReservationService.EndPoint orginEndPOint = new ReservationService.EndPoint();
                orginEndPOint.entityID = Request.KioskID; //Kiosk Identifier
                orginEndPOint.systemType = Request.SystemType;//"KIOSK";
                OGHeader.Origin = orginEndPOint;
                ReservationService.EndPoint destEndPOint = new ReservationService.EndPoint();
                destEndPOint.entityID = Request.DestinationEntityID;
                destEndPOint.systemType = Request.DestinationSystemType;
                OGHeader.Destination = destEndPOint;
                ReservationService.OGHeaderAuthentication Auth = new ReservationService.OGHeaderAuthentication();
                ReservationService.OGHeaderAuthenticationUserCredentials userCredentials = new ReservationService.OGHeaderAuthenticationUserCredentials();
                userCredentials.UserName = Request.Username;
                userCredentials.UserPassword = Request.Password;
                userCredentials.Domain = Request.HotelDomain;
                Auth.UserCredentials = userCredentials;
                OGHeader.Authentication = Auth;
                #endregion

                ReservationService.ReservationServiceSoapClient ResSoapCLient = new ReservationService.ReservationServiceSoapClient();
                bool isOperaCloudEnabled = false;
                isOperaCloudEnabled = (ConfigurationManager.AppSettings["OperaCloudEnabled"] != null
                                && !string.IsNullOrEmpty(ConfigurationManager.AppSettings["OperaCloudEnabled"].ToString())
                                && bool.TryParse(ConfigurationManager.AppSettings["OperaCloudEnabled"].ToString(), out isOperaCloudEnabled)) ? isOperaCloudEnabled : false;
                if (isOperaCloudEnabled)
                {
                    ResSoapCLient.Endpoint.Behaviors.Add(new Helper.CustomEndpointBehaviour(ConfigurationManager.AppSettings["WSSEUserName"].ToString(),
                                            ConfigurationManager.AppSettings["WSSEPassword"].ToString(),
                                            Request.Username, Request.Password, Request.HotelDomain));
                }

                #region Request Body
                ReservationService.UniqueID uID = new ReservationService.UniqueID();
                uID.type = ReservationService.UniqueIDType.INTERNAL;
                if (!string.IsNullOrEmpty(Request.FetchBookingRequest.ReservationNumber))
                {
                    uID.Value = Request.FetchBookingRequest.ReservationNumber;
                    fetchBookingReq.ConfirmationNumber = uID;
                }
                else if (!string.IsNullOrEmpty(Request.FetchBookingRequest.ReservationNameID))
                {
                    uID.Value = Request.FetchBookingRequest.ReservationNameID;
                    fetchBookingReq.ResvNameId = uID;
                }
                else if (!string.IsNullOrEmpty(Request.FetchBookingRequest.CRSNumber))
                {
                    fetchBookingReq.ExternalSystemNumber = new ReservationService.ExternalReference()
                    {
                        LegNumber = string.IsNullOrEmpty(Request.LegNumber) ? 0 : Int32.Parse(Request.LegNumber),
                        LegNumberSpecified = string.IsNullOrEmpty(Request.LegNumber) ? false : true,
                        ReferenceNumber = Request.FetchBookingRequest.CRSNumber,
                        ReferenceType = "OWS"
                    };
                    //uID.Value = Request.FetchBookingRequest.CRSNumber;
                    //fetchBookingReq.ResvNameId = uID;
                    OGHeader.Origin.systemType = "PMS";
                    OGHeader.Origin.entityID = "OWS";
                }
                else

                {
                    //Nlog debug
                    return new Models.OWS.OwsResponseModel
                    {
                        responseMessage = "Generic Exception : Either conf no.or reservation ID is mandatory",
                        statusCode = 1002,
                        result = false
                    };
                }

                uID = new ReservationService.UniqueID();
                uID.Value = Request.LegNumber;
                fetchBookingReq.LegNumber = uID;

                ReservationService.HotelReference HR = new ReservationService.HotelReference();
                HR.chainCode = Request.ChainCode;
                HR.hotelCode = Request.HotelDomain;
                fetchBookingReq.HotelReference = HR;
                #endregion



                fetchBookingRes = ResSoapCLient.FetchBooking(ref OGHeader, fetchBookingReq);
                //fetchBookingRes = new ReservationService.FetchBookingResponse();
                //string temp1 = System.IO.File.ReadAllText(System.Web.Hosting.HostingEnvironment.MapPath(@"~\Log.txt"));
                //fetchBookingRes  = Newtonsoft.Json.JsonConvert.DeserializeObject<ReservationService.FetchBookingResponse>(temp1);
                //System.IO.File.WriteAllText(System.Web.Hosting.HostingEnvironment.MapPath(@"~\Log.txt"), Newtonsoft.Json.JsonConvert.SerializeObject(fetchBookingRes));

                ReservationService.GDSResultStatus status = fetchBookingRes.Result;
                //Nlog debug
                #region Response Success
                if (status.resultStatusFlag.Equals(ReservationService.ResultStatusFlag.SUCCESS))
                {
                    try
                    {

                        ReservationService.HotelReservation hReservation = new ReservationService.HotelReservation();
                        hReservation = fetchBookingRes.HotelReservation;
                        if (hReservation.printRateSpecified)
                            Reservation.PrintRate = hReservation.printRate;
                        if (hReservation.noPostSpecified)
                            Reservation.NoPost = hReservation.noPost;
                        if (hReservation.DoNotMoveRoomSpecified)
                            Reservation.DoNotMoveRoom = hReservation.DoNotMoveRoom;
                        Reservation.ReservationSourceCode = hReservation.sourceCode;
                        Reservation.ExpectedDepartureTime = hReservation.checkOutTime.ToUniversalTime();
                        //string temp1 = hReservation.checkOutTime.ToString("HH:mm");

                        //"INHOUSE"
                        Reservation.isInQueue = hReservation.queueExists;
                        Reservation.ReservationStatus = hReservation.computedReservationStatus.ToString();
                        Reservation.ComputedReservationStatus = hReservation.computedReservationStatus.ToString();

                        ReservationService.Paragraph p;

                        #region Creation Date
                        Reservation.CreatedDateTime = hReservation.insertDate;
                        #endregion

                        #region Confirmation No
                        if (hReservation.UniqueIDList != null)
                        {
                            ReservationService.UniqueID[] rUniqueIDList = hReservation.UniqueIDList;
                            if (rUniqueIDList.Length > 0)
                            {
                                Reservation.ReservationNumber = rUniqueIDList[0].Value;
                                foreach (ReservationService.UniqueID UID in rUniqueIDList)
                                {
                                    if (UID.source != null && UID.source.Equals("RESVID"))
                                        Reservation.ReservationNameID = UID.Value;
                                }

                            }
                        }
                        #endregion

                        #region List Of RoomStays
                        foreach (ReservationService.RoomStay ReservationRoomStay in hReservation.RoomStays)
                        {
                            #region Arrival Date and departure date
                            ReservationService.TimeSpan resTimeSpan = ReservationRoomStay.TimeSpan;
                            if (resTimeSpan != null)
                            {
                                Reservation.ArrivalDate = resTimeSpan.StartDate;
                                try
                                {
                                    Reservation.DepartureDate = DateTime.Parse(resTimeSpan.Item.ToString());
                                }
                                catch (Exception) { }
                            }
                            #endregion

                            #region Total Rate amount
                            Reservation.IsTaxInclusive = ReservationRoomStay.ExpectedCharges != null ? ReservationRoomStay.ExpectedCharges.TaxInclusive : false;
                            Reservation.TotalAmount = ReservationRoomStay.ExpectedCharges != null ? (decimal)ReservationRoomStay.ExpectedCharges.TotalRoomRateAndPackages : 0;
                            //if (!taxInculsive && ReservationRoomStay.ExpectedCharges.TotalTaxesAndFeesSpecified)
                            Reservation.TotalTax = ReservationRoomStay.ExpectedCharges != null && ReservationRoomStay.ExpectedCharges.TotalTaxesAndFeesSpecified ? (decimal)ReservationRoomStay.ExpectedCharges.TotalTaxesAndFees : 0;
                            Reservation.TotalAmount = ReservationRoomStay.ExpectedCharges != null ? Reservation.TotalAmount + (decimal)ReservationRoomStay.ExpectedCharges.TotalTaxesAndFees : 0;
                            ReservationService.Amount Amnt = ReservationRoomStay.CurrentBalance;
                            Reservation.CurrentBalance = Amnt != null ? (decimal)Amnt.Value : 0;

                            #endregion

                            #region ExpectedCharges
                            ReservationService.DailyChargeList DChargeList = ReservationRoomStay.ExpectedCharges != null ? ReservationRoomStay.ExpectedCharges : null;
                            if (DChargeList != null)
                            {
                                try
                                {
                                    List<Models.OWS.DailyRates> DailyRateList = new List<Models.OWS.DailyRates>();
                                    if (DChargeList.ChargesForPostingDate != null)
                                    {

                                        foreach (ReservationService.ChargesForTheDay DayCharge in DChargeList.ChargesForPostingDate)
                                        {
                                            Models.OWS.DailyRates Items = null;
                                            if (DayCharge.PostingDateSpecified)
                                            {
                                                ReservationService.ChargeList RoomCharges = DayCharge.RoomRateAndPackages != null ? DayCharge.RoomRateAndPackages : null;
                                                if (RoomCharges != null)
                                                {
                                                    Items = new Models.OWS.DailyRates();
                                                    Items.description = "Room Charge";
                                                    Items.Amount = (decimal)RoomCharges.TotalCharges;
                                                    Items.PostingDate = DayCharge.PostingDate;
                                                    Items.IsTaxAmount = false;
                                                    DailyRateList.Add(Items);
                                                }

                                                ReservationService.ChargeList TaxCharges = DayCharge.TaxesAndFees != null ? DayCharge.TaxesAndFees : null;
                                                if (TaxCharges != null)
                                                {
                                                    if (TaxCharges.Charges != null)
                                                    {
                                                        foreach (ReservationService.Charge Chg in TaxCharges.Charges)
                                                        {
                                                            if (Chg.CodeType != null && !string.IsNullOrEmpty(Chg.Description))
                                                            {
                                                                Items = new Models.OWS.DailyRates();
                                                                Amnt = (ReservationService.Amount)Chg.Amount;
                                                                Items.Amount = Amnt.ToString() != null ? (decimal)Amnt.Value : 0;
                                                                Items.description = Chg.Description;
                                                                Items.PostingDate = DayCharge.PostingDate;
                                                                Items.IsTaxAmount = true;
                                                                DailyRateList.Add(Items);
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    Reservation.RateDetails = new Models.OWS.RateDetails()
                                    {
                                        DailyRates = DailyRateList
                                    };
                                }
                                catch (Exception ex)
                                {
                                    //Nlog Debug
                                    Reservation.RateDetails = new Models.OWS.RateDetails();
                                }
                            }
                            #endregion

                            foreach (ReservationService.RoomType rmType in ReservationRoomStay.RoomTypes)
                            {
                                if (Reservation.RoomDetails != null)
                                {
                                    Reservation.RoomDetails.RoomStatus = rmType.roomStatus;
                                }
                                else
                                {
                                    Reservation.RoomDetails = new Models.OWS.RoomDetails()
                                    {
                                        
                                        RoomStatus = rmType.roomStatus
                                    };
                                }

                                

                                #region RoomNumber
                                string[] roomNo = rmType.RoomNumber;
                                if (roomNo != null)
                                {
                                    if (Reservation.RoomDetails != null)
                                    {
                                        Reservation.RoomDetails.RoomNumber = roomNo.Length > 0 ? roomNo[0] : "";
                                    }
                                    else
                                    {
                                        Reservation.RoomDetails = new Models.OWS.RoomDetails()
                                        {
                                            RoomNumber = roomNo.Length > 0 ? roomNo[0] : ""
                                        };
                                    }
                                }
                                #endregion

                                #region Room Type
                                if (Reservation.RoomDetails != null)
                                {
                                    Reservation.RoomDetails.RoomType = rmType.roomTypeCode;
                                    Reservation.RoomDetails.RTC = rmType.roomToChargeCode;
                                }
                                else
                                {
                                    Reservation.RoomDetails = new Models.OWS.RoomDetails()
                                    {
                                        RoomType = rmType.roomTypeCode,
                                        RTC = rmType.roomToChargeCode
                                    };
                                }


                                #endregion

                                #region Room type description
                                p = rmType.RoomTypeDescription;
                                if (p != null)
                                {
                                    ReservationService.Text obj = (ReservationService.Text)p.Items[0];
                                    if (obj != null)
                                    {
                                        if (Reservation.RoomDetails != null)
                                        {
                                            Reservation.RoomDetails.RoomTypeDescription = obj.Value;
                                        }
                                        else
                                        {
                                            Reservation.RoomDetails = new Models.OWS.RoomDetails()
                                            {
                                                RoomTypeDescription = obj.Value
                                            };
                                        }
                                    }

                                }

                                p = rmType.RoomTypeShortDescription;
                                if (p != null)
                                {
                                    ReservationService.Text obj = (ReservationService.Text)p.Items[0];
                                    if (obj != null)
                                    {
                                        if (Reservation.RoomDetails != null)
                                        {
                                            Reservation.RoomDetails.RoomTypeShortDescription = obj.Value;
                                        }
                                        else
                                        {
                                            Reservation.RoomDetails = new Models.OWS.RoomDetails()
                                            {
                                                RoomTypeShortDescription = obj.Value
                                            };
                                        }
                                    }
                                }

                                p = rmType.RoomToChargeDescription;
                                if (p != null)
                                {
                                    ReservationService.Text obj = (ReservationService.Text)p.Items[0];
                                    if (obj != null)
                                    {
                                        if (Reservation.RoomDetails != null)
                                        {
                                            Reservation.RoomDetails.RTCDescription = obj.Value;
                                        }
                                        else
                                        {
                                            Reservation.RoomDetails = new Models.OWS.RoomDetails()
                                            {
                                                RTCDescription = obj.Value
                                            };
                                        }
                                    }
                                }

                                p = rmType.RoomToChargeShortDescription;
                                if (p != null)
                                {
                                    ReservationService.Text obj = (ReservationService.Text)p.Items[0];
                                    if (obj != null)
                                    {
                                        if (Reservation.RoomDetails != null)
                                        {
                                            Reservation.RoomDetails.RTCShortDescription = obj.Value;
                                        }
                                        else
                                        {
                                            Reservation.RoomDetails = new Models.OWS.RoomDetails()
                                            {
                                                RTCShortDescription = obj.Value
                                            };
                                        }
                                    }
                                }
                                #endregion

                                break;
                            }


                            foreach (ReservationService.RoomRate rmRate in ReservationRoomStay.RoomRates)
                            {
                                #region  Rate Code
                                if (Reservation.RateDetails != null)
                                {
                                    Reservation.RateDetails.RateCode = rmRate.ratePlanCode;
                                }
                                else
                                {
                                    Reservation.RateDetails = new Models.OWS.RateDetails()
                                    {
                                        RateCode = rmRate.ratePlanCode
                                    };
                                }
                                #endregion

                                if (ReservationRoomStay.RoomRates.Length > 1)
                                {
                                    if (Reservation.RateDetails != null)
                                    {
                                        Reservation.RateDetails.IsMultipleRate = true;
                                    }
                                    else
                                    {
                                        Reservation.RateDetails = new Models.OWS.RateDetails()
                                        {
                                            IsMultipleRate = true
                                        };
                                    }
                                }
                                else
                                {
                                    if (Reservation.RateDetails != null)
                                    {
                                        Reservation.RateDetails.IsMultipleRate = false;
                                    }
                                    else
                                    {
                                        Reservation.RateDetails = new Models.OWS.RateDetails()
                                        {
                                            IsMultipleRate = false
                                        };
                                    }
                                }

                                #region Room Rate
                                if (rmRate.Rates != null)
                                {

                                    ReservationService.Amount amnt = (ReservationService.Amount)rmRate.Rates[0].Base;
                                    if (amnt != null)
                                    {
                                        if (Reservation.RateDetails != null)
                                        {
                                            Reservation.RateDetails.RateAmount = (decimal)amnt.Value;
                                        }
                                        else
                                        {
                                            Reservation.RateDetails = new Models.OWS.RateDetails()
                                            {
                                                RateAmount = (decimal)amnt.Value
                                            };
                                        }

                                    }
                                }
                                #endregion

                                break;
                            }

                            #region Special Prefernces
                            if (ReservationRoomStay.SpecialRequests != null)
                            {
                                foreach (ReservationService.SpecialRequest SR in ReservationRoomStay.SpecialRequests)
                                {
                                    Models.OWS.PreferanceDetails RPrefernce = new Models.OWS.PreferanceDetails();
                                    RPrefernce.PreferanceCode = SR.requestCode;
                                    RPreferencesList.Add(RPrefernce);
                                }
                            }
                            #endregion

                            #region Package Code,Description,Amount
                            if (ReservationRoomStay.Packages != null)
                            {
                                foreach (ReservationService.PackageElement pe in ReservationRoomStay.Packages)
                                {
                                    if (!string.IsNullOrEmpty(pe.packageCode))
                                    {
                                        Models.OWS.PackageDetails pk = new Models.OWS.PackageDetails();
                                        pk.PackageCode = pe.packageCode;
                                        ReservationService.Amount pckAmount = pe.PackageAmount;
                                        if (pckAmount != null)
                                            pk.TotalPackageAmount = (decimal)pckAmount.Value;
                                        RPackagesList.Add(pk);
                                    }
                                }
                            }
                            else
                            {
                                #region FetchPackage
                                Request.FetchBookedPackagesRequest = new Models.OWS.FetchBookedPackagesRequestModel()
                                {
                                    ReservationNumber = Request.FetchBookingRequest.ReservationNumber
                                };
                                Models.OWS.OwsResponseModel serviceResponse = FetchPackagesInAReservation(Request);
                                if (serviceResponse.result)
                                    RPackagesList = (List<Models.OWS.PackageDetails>)serviceResponse.responseData;
                                #endregion
                            }
                            #endregion

                            #region Adult and child count
                            foreach (ReservationService.GuestCount gc in ReservationRoomStay.GuestCounts.GuestCount)
                            {
                                if (gc.ageQualifyingCode == ReservationService.AgeQualifyingCode.ADULT)
                                    Reservation.Adults = gc.count;
                                else if (gc.ageQualifyingCode == ReservationService.AgeQualifyingCode.CHILD)
                                {
                                    if (Reservation.Child == null)
                                        Reservation.Child = gc.count;
                                    else
                                        Reservation.Child += gc.count;
                                }
                            }
                            #endregion

                            #region ReservationType
                            if (ReservationRoomStay.Guarantee != null)
                            {
                                ReservationService.Guarantee ResType = ReservationRoomStay.Guarantee;
                                Reservation.ReservationType = ResType.guaranteeType;
                            }
                            #endregion



                            #region Deposit

                            List<Models.OWS.DepositDetail> depositDetails = new List<Models.OWS.DepositDetail>();

                            #region Payment type
                            if (ReservationRoomStay.Payment != null)
                            {
                                ReservationService.Payment reservationPayment = new ReservationService.Payment();
                                reservationPayment = ReservationRoomStay.Payment;
                                ReservationService.PaymentType[] pt = new ReservationService.PaymentType[1];
                                ReservationService.OtherPaymentType opt = new ReservationService.OtherPaymentType();
                                pt = (ReservationService.PaymentType[])reservationPayment.Item;
                                opt = (ReservationService.OtherPaymentType)pt[0].Item;
                                if (Reservation.PaymentMethod != null)
                                {
                                    Reservation.PaymentMethod.PaymentType = opt.type;
                                }
                                else
                                {
                                    Reservation.PaymentMethod = new Models.OWS.PaymentMethod()
                                    {
                                        PaymentType = opt.type
                                    };
                                }

                            }
                            #endregion


                            if (ReservationRoomStay.CreditCardDeposit != null)
                            {

                                foreach (ReservationService.CreditCardPayment creditCardPayment in ReservationRoomStay.CreditCardDeposit)
                                {
                                    Models.OWS.DepositDetail depositDetail = new Models.OWS.DepositDetail();
                                    depositDetail.IsCreditCardDeposit = true;
                                    ReservationService.Amount DepositAmnt = creditCardPayment.DepositAmount;
                                    depositDetail.Amount = DepositAmnt != null ? (decimal)DepositAmnt.Value : 0;
                                    depositDetail.CardExpiryDate = creditCardPayment.expirationDateSpecified ? creditCardPayment.expirationDate.ToShortDateString() : null;
                                    depositDetail.CreditCardNumber = creditCardPayment.Item != null ? creditCardPayment.Item.ToString() : "";

                                    if (!string.IsNullOrEmpty(Reservation.PaymentMethod.PaymentType))
                                    {
                                        depositDetail.PaymentType = Reservation.PaymentMethod.PaymentType;
                                    }

                                    depositDetails.Add(depositDetail);
                                }

                            }
                            else
                            {

                                if (Reservation.CurrentBalance < 0)
                                {
                                    Models.OWS.DepositDetail depositDetail = new Models.OWS.DepositDetail();
                                    if (!string.IsNullOrEmpty(Reservation.PaymentMethod.PaymentType))
                                    {
                                        depositDetail.PaymentType = Reservation.PaymentMethod.PaymentType;
                                    }
                                    depositDetail.Amount = Reservation.CurrentBalance;
                                    depositDetail.IsCreditCardDeposit = false;
                                    depositDetails.Add(depositDetail);
                                }
                            }

                            Reservation.DepositDetail = depositDetails;
                            #endregion


                        }
                        #endregion


                        #region List Of Guest Profile

                        foreach (ReservationService.ResGuest rGuest in hReservation.ResGuests)
                        {
                            int count = 0;
                            Reservation.ExpectedArrivalTime = rGuest.arrivalTime.AddYears(1899);
                            Reservation.ExpectedArrivalTime = Reservation.ExpectedArrivalTime.Value;

                            foreach (ReservationService.Profile gProfile in rGuest.Profiles)
                            {
                                Type classType = gProfile.Item.GetType();
                                if (classType.Name.Equals("Customer"))
                                {
                                    Models.OWS.GuestProfile guestProfile = new Models.OWS.GuestProfile();
                                    ReservationService.Customer customerProf = (ReservationService.Customer)gProfile.Item;

                                    #region Profile ID
                                    guestProfile.PmsProfileID = gProfile.ProfileIDs[0].Value;

                                    #endregion

                                    #region Fetch Profile

                                    ReservationService.UniqueID UID = new ReservationService.UniqueID();
                                    UID = gProfile.ProfileIDs[0];

                                    Models.OWS.OwsResponseModel SResponse = new Models.OWS.OwsResponseModel();
                                    Models.OWS.OwsRequestModel PRequest = new Models.OWS.OwsRequestModel();
                                    PRequest.HotelDomain = Request.HotelDomain;
                                    PRequest.KioskID = Request.KioskID;
                                    PRequest.Password = Request.Password;
                                    PRequest.Username = Request.Username;
                                    PRequest.Language = Request.Language;
                                    PRequest.LegNumber = Request.LegNumber;
                                    PRequest.fetchProfileRequest = new Models.OWS.FetchProfileRequest()
                                    {
                                        NameID = UID.Value
                                    };
                                    PRequest.SystemType = Request.SystemType;
                                    SResponse = FetchProfileWithProfileID(PRequest);
                                    if (SResponse.result)
                                    {
                                        if (SResponse.responseData != null)
                                            guestProfile = (Models.OWS.GuestProfile)SResponse.responseData;
                                        else
                                        {
                                            #region Fetch Profile details seperately

                                            #region Guest name
                                            string tempName = "";
                                            tempName = customerProf.PersonName.nameTitle != null ? customerProf.PersonName.nameTitle[0] + " " : tempName + "";
                                            tempName = customerProf.PersonName.firstName != null ? tempName + customerProf.PersonName.firstName + " " : tempName + "";
                                            tempName = customerProf.PersonName.middleName != null ? tempName + customerProf.PersonName.middleName[0] + " " : tempName + "";
                                            tempName = customerProf.PersonName.lastName != null ? tempName + customerProf.PersonName.lastName : tempName + "";
                                            guestProfile.GuestName = tempName;
                                            guestProfile.GivenName = customerProf.PersonName.middleName != null ?
                                                customerProf.PersonName.firstName + " " + customerProf.PersonName.middleName[0] :
                                                customerProf.PersonName.firstName;
                                            guestProfile.FamilyName = customerProf.PersonName.lastName;
                                            guestProfile.FirstName = customerProf.PersonName.firstName;
                                            guestProfile.MiddleName = customerProf.PersonName.middleName != null ? customerProf.PersonName.middleName[0] : "";
                                            guestProfile.LastName = customerProf.PersonName.lastName;
                                            guestProfile.Title = customerProf.PersonName.nameTitle != null ? customerProf.PersonName.nameTitle[0] : null;
                                            #endregion

                                            #region Nationality
                                            guestProfile.Nationality = gProfile.nationality;
                                            #endregion

                                            #region Gender
                                            guestProfile.Gender = customerProf.gender.ToString();
                                            #endregion

                                            #region Address

                                            Models.OWS.OwsResponseModel TempSResponse = GetAddressListForProfile(Request);
                                            if (TempSResponse.result)
                                            {
                                                if (TempSResponse.responseData != null)
                                                {
                                                    List<Models.OWS.Address> addresses = (List<Models.OWS.Address>)TempSResponse.responseData;
                                                    if (addresses.Count > 0)
                                                    {
                                                        List<Models.OWS.Address> TAddress = new List<Models.OWS.Address>();
                                                        foreach (Models.OWS.Address Address in addresses)
                                                        {
                                                            TAddress.Add(Address);
                                                        }
                                                        if (TAddress.Count > 0)
                                                            guestProfile.Address = TAddress;
                                                    }
                                                }
                                            }
                                            #endregion

                                            #region email
                                            TempSResponse = GetEmaillistForProfile(Request);
                                            if (TempSResponse.result)
                                            {
                                                if (TempSResponse.responseData != null)
                                                {

                                                    List<Models.OWS.Email> TLEmailData = (List<Models.OWS.Email>)TempSResponse.responseData;
                                                    foreach (Models.OWS.Email Email in TLEmailData)
                                                    {
                                                        TLEmailData.Add(Email);
                                                    }
                                                    if (TLEmailData.Count > 0)
                                                        guestProfile.Email = TLEmailData;
                                                }
                                            }

                                            #endregion

                                            #region Mebership

                                            //if (gProfile.Memberships != null)
                                            //{
                                            //    try
                                            //    {
                                            //        System.IO.File.AppendAllLines(System.Web.Hosting.HostingEnvironment.MapPath(@"~\FetchReservation.txt"), new string[] { "Member ship Details found :- " });
                                            //        foreach (ReservationService.NameMembership MS in gProfile.Memberships)
                                            //        {
                                            //            if (MS.usedInReservationSpecified && MS.usedInReservation)
                                            //            {
                                            //                guestProfile.MembershipType = MS.membershipType;
                                            //                guestProfile.MembershipNumber = MS.membershipNumber;
                                            //                ReservationService.UniqueID Uid = MS.membershipid;
                                            //                guestProfile.MembershipID = Uid.Value;
                                            //            }
                                            //        }
                                            //    }
                                            //    catch (Exception ex)
                                            //    {
                                            //        System.IO.File.AppendAllLines(System.Web.Hosting.HostingEnvironment.MapPath(@"~\FetchReservation.txt"), new string[] { "Error :- ", "", ex.Message });
                                            //    }
                                            //}
                                            #endregion

                                            #region Phone
                                            TempSResponse = GetPhonelistForProfile(Request);
                                            if (TempSResponse.result)
                                            {
                                                if (TempSResponse.responseData != null)
                                                {
                                                    List<Models.OWS.Phone> TLPhoneData = (List<Models.OWS.Phone>)TempSResponse.responseData;
                                                    foreach (Models.OWS.Phone Phone in TLPhoneData)
                                                    {
                                                        TLPhoneData.Add(Phone);
                                                    }
                                                    if (TLPhoneData.Count > 0)
                                                        guestProfile.Phones = TLPhoneData;
                                                }
                                            }


                                            #endregion

                                            #endregion
                                        }
                                    }
                                    else
                                    {
                                        #region Fetch Profile details seperately


                                        #region Guest name
                                        string tempName = "";
                                        tempName = customerProf.PersonName.nameTitle != null ? customerProf.PersonName.nameTitle[0] + " " : tempName + "";
                                        tempName = customerProf.PersonName.firstName != null ? tempName + customerProf.PersonName.firstName + " " : tempName + "";
                                        tempName = customerProf.PersonName.middleName != null ? tempName + customerProf.PersonName.middleName[0] + " " : tempName + "";
                                        tempName = customerProf.PersonName.lastName != null ? tempName + customerProf.PersonName.lastName : tempName + "";
                                        guestProfile.GuestName = tempName;
                                        guestProfile.GivenName = customerProf.PersonName.middleName != null ?
                                            customerProf.PersonName.firstName + " " + customerProf.PersonName.middleName[0] :
                                            customerProf.PersonName.firstName;
                                        guestProfile.FamilyName = customerProf.PersonName.lastName;
                                        guestProfile.FirstName = customerProf.PersonName.firstName;
                                        guestProfile.MiddleName = customerProf.PersonName.middleName != null ? customerProf.PersonName.middleName[0] : "";
                                        guestProfile.LastName = customerProf.PersonName.lastName;
                                        guestProfile.Title = customerProf.PersonName.nameTitle != null ? customerProf.PersonName.nameTitle[0] : null;
                                        #endregion

                                        #region Nationality
                                        guestProfile.Nationality = gProfile.nationality;
                                        #endregion

                                        #region Gender
                                        guestProfile.Gender = customerProf.gender.ToString();
                                        #endregion

                                        #region Address

                                        Models.OWS.OwsResponseModel TempSResponse = GetAddressListForProfile(Request);
                                        if (TempSResponse.result)
                                        {
                                            if (TempSResponse.responseData != null)
                                            {
                                                List<Models.OWS.Address> TAddress = (List<Models.OWS.Address>)TempSResponse.responseData;
                                                foreach (Models.OWS.Address Address in TAddress)
                                                {
                                                    TAddress.Add(Address);
                                                }
                                                if (TAddress.Count > 0)
                                                    guestProfile.Address = TAddress;
                                            }
                                        }
                                        #endregion

                                        #region email
                                        TempSResponse = GetEmaillistForProfile(Request);
                                        if (TempSResponse.result)
                                        {
                                            if (TempSResponse.responseData != null)
                                            {
                                                List<Models.OWS.Email> TLEmailData = (List<Models.OWS.Email>)TempSResponse.responseData;
                                                foreach (Models.OWS.Email Email in TLEmailData)
                                                {
                                                    TLEmailData.Add(Email);
                                                }
                                                if (TLEmailData.Count > 0)
                                                    guestProfile.Email = TLEmailData;
                                            }
                                        }

                                        #endregion

                                        #region Phone
                                        TempSResponse = GetPhonelistForProfile(Request);
                                        if (TempSResponse.result)
                                        {
                                            if (TempSResponse.responseData != null)
                                            {
                                                List<Models.OWS.Phone> TLPhoneData = (List<Models.OWS.Phone>)TempSResponse.responseData;
                                                foreach (Models.OWS.Phone Phone in TLPhoneData)
                                                {
                                                    TLPhoneData.Add(Phone);
                                                }
                                                if (TLPhoneData.Count > 0)
                                                    guestProfile.Phones = TLPhoneData;
                                            }
                                        }


                                        #endregion

                                        #endregion
                                    }



                                    #endregion

                                    #region Mebership

                                    if (gProfile.Memberships != null)
                                    {
                                        try
                                        {
                                            foreach (ReservationService.NameMembership MS in gProfile.Memberships)
                                            {
                                                if (MS.usedInReservationSpecified && MS.usedInReservation)
                                                {
                                                    guestProfile.MembershipType = MS.membershipType;
                                                    guestProfile.MembershipNumber = MS.membershipNumber;
                                                    ReservationService.UniqueID Uid = MS.membershipid;
                                                    guestProfile.MembershipID = Uid.Value;
                                                    guestProfile.MembershipName = MS.memberName;
                                                    guestProfile.MembershipClass = MS.membershipClass;
                                                    guestProfile.MembershipLevel = MS.membershipLevel;

                                                }
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            //Nlog debug
                                        }
                                    }
                                    #endregion

                                    if (count == 0)
                                        guestProfile.IsPrimary = true;
                                    else
                                        guestProfile.IsPrimary = false;

                                    GPList.Add(guestProfile);
                                    count++;
                                }
                            }

                            break;
                        }
                        #endregion

                        #region Accompanying Guest
                        if (hReservation.AccompanyGuests != null)
                        {
                            foreach (ReservationService.AccompanyGuest AG in hReservation.AccompanyGuests)
                            {
                                Models.OWS.GuestProfile guestProfile = new Models.OWS.GuestProfile();

                                ReservationService.UniqueID UID = new ReservationService.UniqueID();
                                UID.Value = AG.NameID.Value;

                                Models.OWS.OwsResponseModel SResponse = new Models.OWS.OwsResponseModel();
                                Models.OWS.OwsRequestModel PRequest = new Models.OWS.OwsRequestModel();
                                PRequest.HotelDomain = Request.HotelDomain;
                                PRequest.KioskID = Request.KioskID;
                                PRequest.Password = Request.Password;
                                PRequest.Username = Request.Username;
                                PRequest.Language = Request.Language;
                                PRequest.LegNumber = Request.LegNumber;
                                PRequest.fetchProfileRequest = new Models.OWS.FetchProfileRequest()
                                {
                                    NameID = UID.Value
                                };
                                PRequest.SystemType = Request.SystemType;
                                SResponse = FetchProfileWithProfileID(PRequest);
                                if (SResponse.result)
                                {
                                    if (SResponse.responseData != null)
                                        guestProfile = (Models.OWS.GuestProfile)SResponse.responseData;
                                    else
                                    {
                                        guestProfile.PmsProfileID = UID.Value;
                                        guestProfile.IsPrimary = false;
                                        guestProfile.FirstName = AG.FirstName;
                                        guestProfile.LastName = AG.LastName;
                                    }
                                }
                                else
                                {
                                    guestProfile.PmsProfileID = UID.Value;
                                    guestProfile.IsPrimary = false;
                                    guestProfile.FirstName = AG.FirstName;
                                    guestProfile.LastName = AG.LastName;
                                }
                                GPList.Add(guestProfile);
                            }
                        }
                        #endregion

                        #region User Defined Fields
                        //if (hReservation.UserDefinedValues != null)
                        //{
                        //    foreach (ReservationService.UserDefinedValue UDFValues in hReservation.UserDefinedValues)
                        //    {
                        //        try
                        //        {

                        //            //if (UDFValues.valueName.Equals("Mealplan"))
                        //            //{
                        //            //    SamsotechOWSGateway.Models.OWS.ResPackage pk = new SamsotechOWSGateway.Models.OWS.ResPackage();
                        //            //    pk.PackageDescription = UDFValues.valueName;
                        //            //    pk.PackageCode = UDFValues.Item.ToString();
                        //            //    pk.TotalPackageAmount = 0;
                        //            //    RPackagesList.Add(pk);
                        //            //}

                        //        }
                        //        catch (Exception ex)
                        //        {
                        //            System.IO.File.AppendAllText(System.Web.Hosting.HostingEnvironment.MapPath(@"~\log.txt"), "Inside meal plan " + ex.Message);
                        //        }
                        //    }
                        //}
                        #endregion

                        #region Alerts
                        List<Models.OWS.Alert> AlertList = new List<Models.OWS.Alert>();
                        if (hReservation.Alerts != null && hReservation.Alerts.Length > 0)
                        {

                            foreach (ReservationService.Alert alert in hReservation.Alerts)
                            {
                                Models.OWS.Alert resAlerts = new Models.OWS.Alert();
                                resAlerts.AlertCode = alert.Code;
                                resAlerts.AlertID = new ReservationService.UniqueID()
                                {
                                    source = alert.AlertId.source,
                                    type = alert.AlertId.type,
                                    Value = alert.AlertId.Value
                                }.Value;
                                resAlerts.Area = alert.Area;
                                resAlerts.Description = alert.Description;
                                resAlerts.isGlobal = alert.Global;
                                resAlerts.isPrinterNotificationEnabled = alert.PrinterNotification;
                                resAlerts.isScreenNotificationEnabled = alert.ScreenNotification;
                                AlertList.Add(resAlerts);
                            }
                            Reservation.Alerts = AlertList;
                        }
                        #endregion

                        #region Miscelenious
                        if (fetchBookingRes.HotelReservation.Miscellaneous != null)
                        {
                            Reservation.PartyCode = fetchBookingRes.HotelReservation.Miscellaneous.PartyCode;
                        }
                        #endregion

                        List<Models.OWS.UserDefinedFields> UDFields = new List<Models.OWS.UserDefinedFields>();

                        if (fetchBookingRes.HotelReservation.UserDefinedValues != null)
                        {

                            foreach (ReservationService.UserDefinedValue UDF in fetchBookingRes.HotelReservation.UserDefinedValues)
                            {
                                Models.OWS.UserDefinedFields UFields = new Models.OWS.UserDefinedFields();
                                UFields.FieldName = UDF.valueName;
                                UFields.FieldValue = UDF.Item.ToString();
                                UDFields.Add(UFields);
                            }
                            Reservation.userDefinedFields = UDFields;
                        }

                        if (fetchBookingRes.HotelReservation.ShareReservations != null && fetchBookingRes.HotelReservation.ShareReservations.Length > 0)
                        {
                            bool isSameReservation = false;
                            string tempReservationNo = null;
                            string tempReservationNameID = null;
                            foreach (ReservationService.ShareReservation shareReservation in fetchBookingRes.HotelReservation.ShareReservations)
                            {
                                Models.OWS.OperaReservation reservationList = new Models.OWS.OperaReservation();

                                if (shareReservation.UniqueIDList != null)
                                {
                                    ReservationService.UniqueID[] rUniqueIDList = shareReservation.UniqueIDList;
                                    if (rUniqueIDList.Length > 0)
                                    {
                                        int x = 0;
                                        foreach (ReservationService.UniqueID UID in rUniqueIDList)
                                        {
                                            if (UID.Value.Equals(Reservation.ReservationNumber))
                                            {
                                                isSameReservation = true;
                                                break;
                                            }
                                            else
                                            {
                                                isSameReservation = false;
                                                if (x == 0)
                                                    tempReservationNo = UID.Value;
                                                if (UID.source != null && UID.source.Equals("RESVID"))
                                                    tempReservationNameID = UID.Value;
                                                x++;
                                            }
                                        }

                                    }
                                }

                                if (isSameReservation)
                                    continue;

                                reservationList.ReservationNumber = tempReservationNo;
                                reservationList.ReservationNameID = tempReservationNameID;

                                foreach (ReservationService.GuestCount guestCount in shareReservation.GuestCounts.GuestCount)
                                {
                                    if (guestCount.ageQualifyingCodeSpecified)
                                    {
                                        if (guestCount.ageQualifyingCode.Equals(ReservationService.AgeQualifyingCode.ADULT) && guestCount.countSpecified)
                                            reservationList.Adults = guestCount.count;
                                        else if (guestCount.ageQualifyingCode.Equals(ReservationService.AgeQualifyingCode.CHILD) && guestCount.countSpecified)
                                            reservationList.Child = guestCount.count;
                                    }
                                }

                                ReservationService.Customer customer = new ReservationService.Customer();
                                customer = (ReservationService.Customer)shareReservation.Profile.Item;

                                Models.OWS.GuestProfile guestProfile = null;

                                #region Fetch Profile
                                Models.OWS.OwsResponseModel SResponse = new Models.OWS.OwsResponseModel();
                                Models.OWS.OwsRequestModel PRequest = new Models.OWS.OwsRequestModel();
                                PRequest.HotelDomain = Request.HotelDomain;
                                PRequest.KioskID = Request.KioskID;
                                PRequest.Password = Request.Password;
                                PRequest.Username = Request.Username;
                                PRequest.Language = Request.Language;
                                PRequest.LegNumber = Request.LegNumber;
                                PRequest.fetchProfileRequest = new Models.OWS.FetchProfileRequest()
                                {
                                    NameID = new ReservationService.UniqueID[]
                                        {
                                            new ReservationService.UniqueID()
                                            {
                                                Value = shareReservation.Profile.ProfileIDs[0].Value,
                                                type = shareReservation.Profile.ProfileIDs[0].type
                                            }
                                        }[0].Value
                                };
                                PRequest.SystemType = Request.SystemType;
                                SResponse = FetchProfileWithProfileID(PRequest);
                                if (SResponse.result)
                                {
                                    if (SResponse.responseData != null)
                                        guestProfile = (Models.OWS.GuestProfile)SResponse.responseData;

                                }


                                #endregion

                                if (guestProfile == null)
                                {
                                    reservationList.GuestProfiles = new List<Models.OWS.GuestProfile>()
                                    {
                                        new Models.OWS.GuestProfile()
                                        {
                                            PmsProfileID = new ReservationService.UniqueID[]
                                            {
                                                new ReservationService.UniqueID()
                                                {
                                                    Value = shareReservation.Profile.ProfileIDs[0].Value,
                                                    type = shareReservation.Profile.ProfileIDs[0].type
                                                }
                                            }[0].Value,
                                            FirstName = customer.PersonName != null ? customer.PersonName.firstName : null,
                                            LastName = customer.PersonName != null ? customer.PersonName.lastName : null,
                                            MiddleName = customer.PersonName != null ? ((customer.PersonName.middleName != null && customer.PersonName.middleName.Length > 0 ? customer.PersonName.middleName[0] : null)) : null,
                                        }
                                    };
                                }
                                else
                                {
                                    reservationList.GuestProfiles = new List<Models.OWS.GuestProfile>()
                                    {
                                        guestProfile
                                    };
                                }

                                reservationList.IsPrimary = !string.IsNullOrEmpty(shareReservation.primary) ? (shareReservation.primary.Equals("Y") ? true : false) : false;

                                SRList.Add(reservationList);
                            }
                        }

                        Reservation.SharerReservations = SRList;
                        Reservation.userDefinedFields = UDFields;
                        Reservation.GuestProfiles = GPList;
                        Reservation.PreferanceDetails = RPreferencesList;
                        Reservation.PackageDetails = RPackagesList;
                        Reservation.Alerts = AlertList;
                        RList.Add(Reservation);
                    }
                    catch (Exception ex)
                    {
                        //System.IO.File.WriteAllText(System.Web.Hosting.HostingEnvironment.MapPath(@"~\Log.txt"), ex.ToString());
                        //Nlog debug
                        return new Models.OWS.OwsResponseModel
                        {
                            responseMessage = "Generic Exception : " + ex.Message,
                            statusCode = 1002,
                            result = false
                        };
                    }
                }
                else
                {
                    return new Models.OWS.OwsResponseModel
                    {
                        responseMessage = status.GDSError.Value,
                        statusCode = 1001,
                        result = false
                    };
                }
                #endregion

                #endregion

                return new Models.OWS.OwsResponseModel
                {
                    responseData = RList,
                    responseMessage = "SUCCESS",
                    statusCode = 101,
                    result = true
                };
            }
            catch (Exception ex)
            {
                //Nlog debug
                return new Models.OWS.OwsResponseModel
                {
                    responseMessage = "Generic Exception : " + ex.Message,
                    statusCode = 1002,
                    result = false
                };
            }
        }

        public Models.OWS.OwsResponseModel FetchPackagesInAReservation(Models.OWS.OwsRequestModel Request)
        {
            try
            {
                //Nlog Debug


                #region Call FetchReservation by conf no
                ReservationService.FetchBookedPackagesRequest fetchBookedPackgReq = new ReservationService.FetchBookedPackagesRequest();
                ReservationService.FetchBookedPackagesResponse fetchBookedPackgRes = new ReservationService.FetchBookedPackagesResponse();

                #region Request Header

                string temp = Helper.Helper.Get8Digits();
                ReservationService.OGHeader OGHeader = new ReservationService.OGHeader();
                OGHeader.transactionID = temp;
                OGHeader.timeStamp = DateTime.Now;
                OGHeader.primaryLangID = Request.Language; //English
                ReservationService.EndPoint orginEndPOint = new ReservationService.EndPoint();
                orginEndPOint.entityID = Request.KioskID; //Kiosk Identifier
                orginEndPOint.systemType = Request.SystemType;//"KIOSK";
                OGHeader.Origin = orginEndPOint;
                ReservationService.EndPoint destEndPOint = new ReservationService.EndPoint();
                destEndPOint.entityID = Request.DestinationEntityID;
                destEndPOint.systemType = Request.DestinationSystemType;
                OGHeader.Destination = destEndPOint;
                ReservationService.OGHeaderAuthentication Auth = new ReservationService.OGHeaderAuthentication();
                ReservationService.OGHeaderAuthenticationUserCredentials userCredentials = new ReservationService.OGHeaderAuthenticationUserCredentials();
                userCredentials.UserName = Request.Username;
                userCredentials.UserPassword = Request.Password;
                userCredentials.Domain = Request.HotelDomain;
                Auth.UserCredentials = userCredentials;
                OGHeader.Authentication = Auth;
                #endregion

                ReservationService.ReservationServiceSoapClient ResSoapCLient = new ReservationService.ReservationServiceSoapClient();
                bool isOperaCloudEnabled = false;
                isOperaCloudEnabled = (ConfigurationManager.AppSettings["OperaCloudEnabled"] != null
                                && !string.IsNullOrEmpty(ConfigurationManager.AppSettings["OperaCloudEnabled"].ToString())
                                && bool.TryParse(ConfigurationManager.AppSettings["OperaCloudEnabled"].ToString(), out isOperaCloudEnabled)) ? isOperaCloudEnabled : false;
                if (isOperaCloudEnabled)
                {
                    ResSoapCLient.Endpoint.Behaviors.Add(new Helper.CustomEndpointBehaviour(ConfigurationManager.AppSettings["WSSEUserName"].ToString(),
                                            ConfigurationManager.AppSettings["WSSEPassword"].ToString(),
                                            Request.Username, Request.Password, Request.HotelDomain));
                }

                #region Request Body
                ReservationService.UniqueID uID = new ReservationService.UniqueID();
                uID.type = ReservationService.UniqueIDType.INTERNAL;
                if (!string.IsNullOrEmpty(Request.FetchBookedPackagesRequest.ReservationNumber))
                {
                    uID.Value = Request.FetchBookedPackagesRequest.ReservationNumber;
                    fetchBookedPackgReq.ConfirmationNumber = uID;
                }
                else

                {
                    //Nlog debug
                    return new Models.OWS.OwsResponseModel
                    {
                        responseData = null,
                        responseMessage = "Generic Exception : Either conf no.or reservation ID is mandatory",
                        statusCode = 1002,
                        result = false
                    };
                }



                ReservationService.HotelReference HR = new ReservationService.HotelReference();
                HR.chainCode = "CHA";
                HR.hotelCode = Request.HotelDomain;
                fetchBookedPackgReq.HotelReference = HR;
                #endregion


                fetchBookedPackgRes = ResSoapCLient.FetchBookedPackages(ref OGHeader, fetchBookedPackgReq);
                ReservationService.GDSResultStatus status = fetchBookedPackgRes.Result;
                //Nlog Debug

                #region Response
                if (status.resultStatusFlag.Equals(ReservationService.ResultStatusFlag.SUCCESS))
                {
                    try
                    {
                        List<Models.OWS.PackageDetails> resPackages = new List<Models.OWS.PackageDetails>();
                        #region Package Code,Description,Amount
                        foreach (ReservationService.PackageDetail pd in fetchBookedPackgRes.BookedPackageList)
                        {
                            Models.OWS.PackageDetails pk = new Models.OWS.PackageDetails();
                            ReservationService.PackageElement pe = pd.PackageInfo;
                            pk.PackageCode = pe.packageCode;
                            pk.isTaxIncluded = pe.taxIncludedSpecified ? pe.taxIncluded : false;
                            if (pe.Description != null)
                            {
                                foreach (ReservationService.DescriptiveText txt in pe.Description)
                                {

                                    ReservationService.TextElement[] textElement = (ReservationService.TextElement[])txt.Item;
                                    pk.PackageDescription = textElement[0].Value;
                                    break;
                                }
                            }

                            ReservationService.Amount Amount = pe.Amount;
                            if (Amount != null)
                            {
                                pk.TotalAmount = (decimal)Amount.Value;
                                pk.CurrecncyCode = pe.Amount.currencyCode;
                            }
                            Amount = null;
                            if (pk.isTaxIncluded)
                            {
                                Amount = pe.TaxAmount;
                                if (Amount != null)
                                    pk.TaxAmount = (decimal)Amount.Value;
                            }
                            Amount = null;
                            Amount = pe.Allowance;
                            if (Amount != null)
                                pk.Allowance = (decimal)Amount.Value;
                            List<Models.OWS.AmountDetails> packageCharges = new List<Models.OWS.AmountDetails>();
                            foreach (ReservationService.PackageCharge pc in pd.ExpectedCharges)
                            {
                                Models.OWS.AmountDetails packageCharge = new Models.OWS.AmountDetails();
                                packageCharge.StartDate = pc.ValidDates.StartDate;
                                packageCharge.Quantity = pc.Quantity;
                                packageCharge.EndDate = (DateTime)pc.ValidDates.Item;
                                packageCharge.UnitAmount = pc.UnitAmount != null ? (decimal)pc.UnitAmount.Value : 0;
                                packageCharge.TotalAmount = pc.TotalAmount != null ? (decimal)pc.TotalAmount.Value : 0;
                                packageCharge.Tax = pc.Tax != null ? (decimal)pc.Tax.Value : 0;
                                packageCharges.Add(packageCharge);
                            }
                            pk.packageCharges = packageCharges;
                            resPackages.Add(pk);
                        }
                        #endregion
                        return new Models.OWS.OwsResponseModel
                        {
                            responseData = resPackages,
                            responseMessage = "SUCCESS",
                            statusCode = 101,
                            result = true
                        };
                    }
                    catch (Exception ex)
                    {
                        //Nlog debug
                        return new Models.OWS.OwsResponseModel
                        {
                            responseMessage = "Generic Exception : " + ex.Message,
                            statusCode = 1002,
                            result = false
                        };
                    }
                }
                else
                {
                    return new Models.OWS.OwsResponseModel
                    {
                        responseMessage = status.OperaErrorCode,
                        statusCode = 1002,
                        result = false
                    };
                }
                #endregion

                #endregion
            }
            catch (Exception ex)
            {
                //Nlog Debug
                return new Models.OWS.OwsResponseModel
                {
                    responseMessage = "Generic Exception : " + ex.Message,
                    statusCode = 1002,
                    result = false
                };
            }
        }
        public Models.OWS.OwsResponseModel GetAddressListForProfile(Models.OWS.OwsRequestModel Request)
        {
            try
            {
                #region Request

                #region Request Header
                string temp = Helper.Helper.Get8Digits();
                NameService.OGHeader OGHeader = new NameService.OGHeader();
                OGHeader.transactionID = temp;
                OGHeader.timeStamp = DateTime.Now;
                OGHeader.primaryLangID = Request.Language; //English
                NameService.EndPoint orginEndPOint = new NameService.EndPoint();
                orginEndPOint.entityID = Request.KioskID; //Kiosk Identifier
                orginEndPOint.systemType = Request.SystemType;
                OGHeader.Origin = orginEndPOint;
                NameService.EndPoint destEndPOint = new NameService.EndPoint();
                destEndPOint.entityID = Request.DestinationEntityID;
                destEndPOint.systemType = Request.DestinationSystemType;
                OGHeader.Destination = destEndPOint;
                NameService.OGHeaderAuthentication Auth = new NameService.OGHeaderAuthentication();
                NameService.OGHeaderAuthenticationUserCredentials userCredentials = new NameService.OGHeaderAuthenticationUserCredentials();
                userCredentials.UserName = Request.Username;
                userCredentials.UserPassword = Request.Password;
                userCredentials.Domain = Request.HotelDomain;
                Auth.UserCredentials = userCredentials;
                OGHeader.Authentication = Auth;
                #endregion

                #region Request Body

                NameService.FetchAddressListRequest FetchAddressListReq = new NameService.FetchAddressListRequest();
                NameService.UniqueID uID = new NameService.UniqueID();
                uID.type = (NameService.UniqueIDType)NameService.UniqueIDType.INTERNAL;
                uID.Value = Request.fetchProfileRequest.NameID;
                FetchAddressListReq.NameID = uID;
                #endregion

                #endregion

                NameService.NameServiceSoapClient PortClient = new NameService.NameServiceSoapClient();
                bool isOperaCloudEnabled = false;
                isOperaCloudEnabled = (ConfigurationManager.AppSettings["OperaCloudEnabled"] != null
                                && !string.IsNullOrEmpty(ConfigurationManager.AppSettings["OperaCloudEnabled"].ToString())
                                && bool.TryParse(ConfigurationManager.AppSettings["OperaCloudEnabled"].ToString(), out isOperaCloudEnabled)) ? isOperaCloudEnabled : false;
                if (isOperaCloudEnabled)
                {
                    PortClient.Endpoint.Behaviors.Add(new Helper.CustomEndpointBehaviour(ConfigurationManager.AppSettings["WSSEUserName"].ToString(),
                                            ConfigurationManager.AppSettings["WSSEPassword"].ToString(),
                                            Request.Username, Request.Password, Request.HotelDomain));
                }
                NameService.FetchAddressListResponse AddressListResp = PortClient.FetchAddressList(ref OGHeader, FetchAddressListReq);
                if (AddressListResp.Result.resultStatusFlag == NameService.ResultStatusFlag.SUCCESS)
                {
                    List<Models.OWS.Address> AddrList = new List<Models.OWS.Address>();
                    foreach (NameService.NameAddress NAddress in AddressListResp.NameAddressList)
                    {
                        Models.OWS.Address Address = new Models.OWS.Address();
                        Address.displaySequence = NAddress.displaySequence;
                        Address.operaId = NAddress.operaId;
                        Address.address1 = NAddress.AddressLine != null ? NAddress.AddressLine[0] : null;
                        Address.address2 = NAddress.AddressLine != null && NAddress.AddressLine.Length >= 2 ? NAddress.AddressLine[1] : null;
                        Address.addressType = NAddress.addressType;
                        Address.city = NAddress.cityName;
                        Address.state = NAddress.stateProv;
                        Address.zip = NAddress.postalCode;
                        Address.country = NAddress.countryCode;
                        Address.primary = NAddress.primary;
                        AddrList.Add(Address);
                    }
                    
                    return new Models.OWS.OwsResponseModel
                    {
                        responseData = AddrList,
                        responseMessage = "Success",
                        statusCode = 101,
                        result = true
                    };
                }
                else
                {
                    return new Models.OWS.OwsResponseModel
                    {
                        responseMessage = AddressListResp.Result != null ? AddressListResp.Result.Text[0].ToString() : "Error to retreave Address list",
                        statusCode = 6001,
                        result = false
                    };
                }





            }
            catch (Exception ex)
            {
                //Nlog debug
                return new Models.OWS.OwsResponseModel
                {
                    
                    responseMessage = "Generic Exception : " + ex.Message,
                    statusCode = 1002,
                    result = false
                };
            }
        }

        public Models.OWS.OwsResponseModel getFolioPrint(Models.OWS.OwsRequestModel Request)
        {
            try
            {
                Models.OWS.OwsResponseModel SResponse = new Models.OWS.OwsResponseModel();
                #region Request

                #region Request Header
                string temp = Helper.Helper.Get8Digits();
                ReservationAdvancedService.OGHeader OGHeader = new ReservationAdvancedService.OGHeader();
                OGHeader.transactionID = temp;
                OGHeader.timeStamp = DateTime.Now;
                OGHeader.primaryLangID = Request.Language; //English
                ReservationAdvancedService.EndPoint orginEndPOint = new ReservationAdvancedService.EndPoint();
                orginEndPOint.entityID = Request.KioskID; //Kiosk Identifier
                orginEndPOint.systemType = Request.SystemType;
                OGHeader.Origin = orginEndPOint;
                ReservationAdvancedService.EndPoint destEndPOint = new ReservationAdvancedService.EndPoint();
                destEndPOint.entityID = "TI";
                destEndPOint.systemType = "PMS";
                OGHeader.Destination = destEndPOint;
                ReservationAdvancedService.OGHeaderAuthentication Auth = new ReservationAdvancedService.OGHeaderAuthentication();
                ReservationAdvancedService.OGHeaderAuthenticationUserCredentials userCredentials = new ReservationAdvancedService.OGHeaderAuthenticationUserCredentials();
                userCredentials.UserName = Request.Username;
                userCredentials.UserPassword = Request.Password;
                userCredentials.Domain = Request.HotelDomain;
                Auth.UserCredentials = userCredentials;
                OGHeader.Authentication = Auth;
                #endregion

                #region Request Body

                ReservationAdvancedService.PrintPreCheckOutBillRequest InvRequest = new ReservationAdvancedService.PrintPreCheckOutBillRequest();


                ReservationAdvancedService.ReservationRequestBase RB = new ReservationAdvancedService.ReservationRequestBase();

                ReservationAdvancedService.HotelReference HF = new ReservationAdvancedService.HotelReference();
                HF.chainCode = Request.ChainCode;
                HF.hotelCode = Request.HotelDomain;
                RB.HotelReference = HF;

                ReservationAdvancedService.UniqueID uID = new ReservationAdvancedService.UniqueID();
                uID.type = (ReservationAdvancedService.UniqueIDType)ReservationAdvancedService.UniqueIDType.EXTERNAL;
                uID.source = "RESV_NAME_ID";
                uID.Value = Request.OperaReservation.ReservationNameID;
                ReservationAdvancedService.UniqueID[] UIDLIST = new ReservationAdvancedService.UniqueID[1];
                UIDLIST[0] = uID;
                RB.ReservationID = UIDLIST;
                InvRequest.ReservationRequest = RB;

                InvRequest.EmailFolio = false;
                InvRequest.EmailFolioSpecified = false;
                InvRequest.generateFile = true;
                InvRequest.generateFileSpecified = true;
                InvRequest.returnFileContents = false;
                InvRequest.returnFileContentsSpecified = false;
                ReservationAdvancedService.ResvAdvancedServiceSoapClient ResAdvPortClient = new ReservationAdvancedService.ResvAdvancedServiceSoapClient();
                bool isOperaCloudEnabled = false;
                isOperaCloudEnabled = (ConfigurationManager.AppSettings["OperaCloudEnabled"] != null
                                && !string.IsNullOrEmpty(ConfigurationManager.AppSettings["OperaCloudEnabled"].ToString())
                                && bool.TryParse(ConfigurationManager.AppSettings["OperaCloudEnabled"].ToString(), out isOperaCloudEnabled)) ? isOperaCloudEnabled : false;
                if (isOperaCloudEnabled)
                {
                    ResAdvPortClient.Endpoint.Behaviors.Add(new Helper.CustomEndpointBehaviour(ConfigurationManager.AppSettings["WSSEUserName"].ToString(),
                                            ConfigurationManager.AppSettings["WSSEPassword"].ToString(),
                                            Request.Username, Request.Password, Request.HotelDomain));
                }
                ReservationAdvancedService.PrintPreCheckOutBillResponse InvResponse = new ReservationAdvancedService.PrintPreCheckOutBillResponse();

                InvResponse = ResAdvPortClient.PrintPreCheckOutBill(ref OGHeader, InvRequest);
                if (InvResponse.Result.resultStatusFlag == ReservationAdvancedService.ResultStatusFlag.SUCCESS)
                {
                    return new Models.OWS.OwsResponseModel()
                    {
                        responseData = InvResponse.GuestBill,
                        result = true,
                        responseMessage = "Success"
                    };
                }
                else
                {
                    return new Models.OWS.OwsResponseModel()
                    {
                       
                        responseMessage = "Get folio function failled",
                        statusCode = 1402,
                        result = false
                    };
                }


                #endregion

                #endregion
            }
            catch (Exception ex)
            {
                return new Models.OWS.OwsResponseModel()
                {

                    responseMessage = ex.Message,
                    statusCode = 1402,
                    result = false
                };
            }
        }

        public Models.OWS.OwsResponseModel ModifyReservation(Models.OWS.OwsRequestModel modifyReservation)
        {
            try
            {
                new LogHelper().Debug("ModifyReservation request : " + JsonConvert.SerializeObject(modifyReservation), modifyReservation.modifyBookingRequest.ReservationNumber, "ModifyReservation", "API", "OWS");
                ReservationService.ModifyBookingRequest modifyBookingReq = new ReservationService.ModifyBookingRequest();
                ReservationService.ModifyBookingResponse modifyBookingRes = new ReservationService.ModifyBookingResponse();

                #region Request Header

                string temp = Helper.Helper.Get8Digits();
                ReservationService.OGHeader OGHeader = new ReservationService.OGHeader();
                OGHeader.transactionID = temp;
                OGHeader.timeStamp = DateTime.Now;
                OGHeader.primaryLangID = modifyReservation.Language; //English
                ReservationService.EndPoint orginEndPOint = new ReservationService.EndPoint();
                orginEndPOint.entityID = modifyReservation.KioskID; //Kiosk Identifier
                orginEndPOint.systemType = modifyReservation.SystemType;//"KIOSK";
                OGHeader.Origin = orginEndPOint;
                ReservationService.EndPoint destEndPOint = new ReservationService.EndPoint();
                destEndPOint.entityID = modifyReservation.DestinationEntityID;
                destEndPOint.systemType = modifyReservation.DestinationSystemType;
                OGHeader.Destination = destEndPOint;
                ReservationService.OGHeaderAuthentication Auth = new ReservationService.OGHeaderAuthentication();
                ReservationService.OGHeaderAuthenticationUserCredentials userCredentials = new ReservationService.OGHeaderAuthenticationUserCredentials();
                userCredentials.UserName = modifyReservation.Username;
                userCredentials.UserPassword = modifyReservation.Password;
                userCredentials.Domain = modifyReservation.HotelDomain;
                Auth.UserCredentials = userCredentials;
                OGHeader.Authentication = Auth;
                #endregion

                ReservationService.ReservationServiceSoapClient ResSoapCLient = new ReservationService.ReservationServiceSoapClient();
                bool isOperaCloudEnabled = false;
                isOperaCloudEnabled = (ConfigurationManager.AppSettings["OperaCloudEnabled"] != null
                                && !string.IsNullOrEmpty(ConfigurationManager.AppSettings["OperaCloudEnabled"].ToString())
                                && bool.TryParse(ConfigurationManager.AppSettings["OperaCloudEnabled"].ToString(), out isOperaCloudEnabled)) ? isOperaCloudEnabled : false;
                if (isOperaCloudEnabled)
                {
                    ResSoapCLient.Endpoint.Behaviors.Add(new Helper.CustomEndpointBehaviour(ConfigurationManager.AppSettings["WSSEUserName"].ToString(),
                                            ConfigurationManager.AppSettings["WSSEPassword"].ToString(),
                                            modifyReservation.Username, modifyReservation.Password, modifyReservation.HotelDomain));
                }

                ReservationService.HotelReservation hReservation = new ReservationService.HotelReservation();

                ReservationService.UniqueID[] rUniqueIDList = new ReservationService.UniqueID[2];
                ReservationService.UniqueID uID = new ReservationService.UniqueID();
                uID.type = ReservationService.UniqueIDType.INTERNAL;
                uID.source = "RESERVATIONNUMBER";
                uID.Value = modifyReservation.modifyBookingRequest.ReservationNumber;
                rUniqueIDList[0] = uID;
                uID = new ReservationService.UniqueID();
                uID.type = ReservationService.UniqueIDType.INTERNAL;
                uID.source = "LEGNUMBER";
                uID.Value = modifyReservation.LegNumber;
                rUniqueIDList[1] = uID;
                hReservation.UniqueIDList = rUniqueIDList;
                ReservationService.RoomStay Rstay = new ReservationService.RoomStay();

                #region Update UDF Fields
                if (modifyReservation.modifyBookingRequest.isUDFFieldSpecified != null && modifyReservation.modifyBookingRequest.isUDFFieldSpecified.Value)
                {
                    ReservationService.UserDefinedValue[] UDFFields = new ReservationService.UserDefinedValue[modifyReservation.modifyBookingRequest.uDFFields.Count];
                    int x = 0;
                    foreach (Models.OWS.UDFField uDFField in modifyReservation.modifyBookingRequest.uDFFields)
                    {
                        ReservationService.UserDefinedValue UDF = new ReservationService.UserDefinedValue();
                        UDF.valueName = uDFField.FieldName;
                        UDF.Item = uDFField.FieldValue;
                        UDFFields[x] = UDF;
                        x++;
                    }
                    hReservation.UserDefinedValues = UDFFields;
                    ReservationService.HotelReference HF = new ReservationService.HotelReference();
                    HF.hotelCode = modifyReservation.HotelDomain;
                    Rstay.HotelReference = HF;
                    ReservationService.RoomStay[] ArrayRstay = { Rstay };
                    hReservation.RoomStays = ArrayRstay;

                    
                    modifyBookingReq.HotelReservation = hReservation;
                    //ResSoapCLient.Endpoint.Behaviors.Add(new Helper.CustomEndpointBehaviour("Test USE", "Request.WSSEPassword", "Request.KioskUserName", "Request.KioskPassword", "Request.HotelDomain"));
                    modifyBookingRes = ResSoapCLient.ModifyBooking(ref OGHeader, modifyBookingReq);
                    ReservationService.GDSResultStatus status = modifyBookingRes.Result;
                    if (status.resultStatusFlag.Equals(ReservationService.ResultStatusFlag.SUCCESS))
                    {
                        
                        if (modifyBookingRes.HotelReservation != null)
                        {
                            bool foundFlag = true;
                           
                            if (foundFlag)
                            {
                                return new Models.OWS.OwsResponseModel
                                {
                                    responseMessage = "SUCCESS",
                                    statusCode = 101,
                                    result = true
                                };
                            }
                            else
                            {
                                return new Models.OWS.OwsResponseModel
                                {
                                    responseMessage = "Failled to Update UDF please check the request",
                                    statusCode = -1,
                                    result = false
                                };
                                
                            }
                        }
                        else
                        {
                            return new Models.OWS.OwsResponseModel
                            {
                                responseMessage = "Failled to Update UDF please check the request",
                                statusCode = -1,
                                result = false
                            };
                        }
                    }
                    else
                    {
                        return new Models.OWS.OwsResponseModel
                        {
                            responseMessage = status.OperaErrorCode != null ? status.OperaErrorCode : "",
                            statusCode = -102,
                            result = false
                        };
                        
                    }
                        
                    
                }
                #endregion

                #region UpdatePaymentDetails

                else if(modifyReservation.modifyBookingRequest.updateCreditCardDetails != null && modifyReservation.modifyBookingRequest.updateCreditCardDetails.Value && modifyReservation.modifyBookingRequest.PaymentMethod != null && !string.IsNullOrEmpty(modifyReservation.modifyBookingRequest.PaymentMethod.MaskedCardNumber))
                {

                    ReservationService.Guarantee Gurantee = new ReservationService.Guarantee();
                    Gurantee.guaranteeType = modifyReservation.modifyBookingRequest.GarunteeTypeCode;

                    ReservationService.GuaranteeAccepted GuranteeAccptd = new ReservationService.GuaranteeAccepted();

                    ReservationService.CreditCard CC = new ReservationService.CreditCard();
                    CC.cardCode = modifyReservation.modifyBookingRequest.PaymentMethod.PaymentType;
                    CC.chipAndPin = false;
                    CC.chipAndPinSpecified = true;
                    CC.cardType = modifyReservation.modifyBookingRequest.PaymentMethod.PaymentType;//"WEB";
                    CC.Item = modifyReservation.modifyBookingRequest.PaymentMethod.MaskedCardNumber.ToLower() ;// "4687560100136162";
                    CC.expirationDate =  !string.IsNullOrEmpty(modifyReservation.modifyBookingRequest.PaymentMethod.ExpiryDate) ? 
                        DateTime.ParseExact(modifyReservation.modifyBookingRequest.PaymentMethod.ExpiryDate,"d/M/yyyy", CultureInfo.InvariantCulture,
    DateTimeStyles.None) : DateTime.Now.AddYears(2);
                    CC.expirationDateSpecified = true;
                    GuranteeAccptd.GuaranteeCreditCard = CC;
                    ReservationService.GuaranteeAccepted[] ArrayGA = { GuranteeAccptd };

                    Gurantee.GuaranteesAccepted = ArrayGA;
                    Rstay.Guarantee = Gurantee;
                    ReservationService.HotelReference HF = new ReservationService.HotelReference();
                    HF.hotelCode = modifyReservation.HotelDomain;
                    HF.chainCode = modifyReservation.ChainCode;
                    Rstay.HotelReference = HF;
                    
                    ReservationService.RoomStay[] ArrayRstay = { Rstay };
                    hReservation.RoomStays = ArrayRstay;
                    
                    modifyBookingReq.HotelReservation = hReservation;
                    //ResSoapCLient.Endpoint.Behaviors.Add(new Helper.CustomEndpointBehaviour("Test USE", "Request.WSSEPassword", "Request.KioskUserName", "Request.KioskPassword", "Request.HotelDomain"));
                    modifyBookingRes = ResSoapCLient.ModifyBooking(ref OGHeader, modifyBookingReq);
                    ReservationService.GDSResultStatus status = modifyBookingRes.Result;
                    
                    if (status.resultStatusFlag.Equals(ReservationService.ResultStatusFlag.SUCCESS))
                    {
                        return new Models.OWS.OwsResponseModel
                        {
                            responseData = null,
                            responseMessage = "SUCCESS",
                            statusCode = 101,
                            result = true
                        };
                        
                    }
                    else
                    {
                        return new Models.OWS.OwsResponseModel
                        {
                            
                            responseMessage = string.IsNullOrEmpty(status.OperaErrorCode) ? (status.GDSError != null ? status.GDSError.Value : "Opera error") : status.OperaErrorCode,
                            statusCode = 102,
                            result = false
                        };
                    }
                }
                #endregion

                #region Update ETA
                if (modifyReservation.modifyBookingRequest.isETASpecified != null && modifyReservation.modifyBookingRequest.isETASpecified.Value && modifyReservation.modifyBookingRequest.ETA != null)
                {
                    try
                    {                        
                        hReservation.checkInTime = modifyReservation.modifyBookingRequest.ETA.Value;
                        hReservation.checkInTimeSpecified = true;
                        ReservationService.HotelReference HF = new ReservationService.HotelReference();
                        HF.hotelCode = modifyReservation.HotelDomain;
                        Rstay.HotelReference = HF;
                        ReservationService.RoomStay[] ArrayRstay = { Rstay };
                        hReservation.RoomStays = ArrayRstay;

                        modifyBookingRes = ResSoapCLient.ModifyBooking(ref OGHeader, modifyBookingReq);
                        ReservationService.GDSResultStatus status = modifyBookingRes.Result;


                        if (status.resultStatusFlag.Equals(ReservationService.ResultStatusFlag.SUCCESS))
                        {

                            if (modifyBookingRes.HotelReservation != null)
                            {
                                bool foundFlag = true;

                                if (foundFlag)
                                {
                                    return new Models.OWS.OwsResponseModel
                                    {
                                        responseMessage = "SUCCESS",
                                        statusCode = 101,
                                        result = true
                                    };
                                }
                                else
                                {
                                    return new Models.OWS.OwsResponseModel
                                    {
                                        responseMessage = "Failled to Update ETA please check the request",
                                        statusCode = -1,
                                        result = false
                                    };

                                }
                            }
                            else
                            {
                                return new Models.OWS.OwsResponseModel
                                {
                                    responseMessage = "Failled to Update ETA please check the request",
                                    statusCode = -1,
                                    result = false
                                };
                            }
                        }
                        else
                        {
                            return new Models.OWS.OwsResponseModel
                            {
                                responseMessage = status.GDSError != null ? status.GDSError.Value : "",
                                statusCode = -102,
                                result = false
                            };

                        }
                        
                    }
                    catch (Exception ex)
                    {
                        //System.IO.File.AppendAllText(System.Web.Hosting.HostingEnvironment.MapPath(@"~\Error.txt"), ex.Message);
                        hReservation.checkOutTimeSpecified = false;
                    }
                }
                #endregion


                return new Models.OWS.OwsResponseModel
                {
                    responseMessage = "Action is not defined",
                    statusCode = -10002,
                    result = false
                };
            }
            catch (Exception ex)
            {
                return new Models.OWS.OwsResponseModel
                {
                    responseMessage = ex.ToString(),
                    statusCode = -1,
                    result = false
                };
                
            }
        }

        public Models.OWS.OwsResponseModel GuestCheckOut(Models.OWS.OwsRequestModel Request)
        {
            try
            {
                new LogHelper().Debug("Checkout request : " + JsonConvert.SerializeObject(Request), Request.OperaReservation.ReservationNameID, "GuestCheckOut", "API", "OWS");
                #region Request

                #region Request Header
                string temp = Helper.Helper.Get8Digits();
                ReservationAdvancedService.OGHeader OGHeader = new ReservationAdvancedService.OGHeader();
                OGHeader.transactionID = temp;
                OGHeader.timeStamp = DateTime.Now;
                OGHeader.primaryLangID = Request.Language; //English
                ReservationAdvancedService.EndPoint orginEndPOint = new ReservationAdvancedService.EndPoint();
                orginEndPOint.entityID = Request.KioskID; //Kiosk Identifier
                orginEndPOint.systemType = Request.SystemType;
                OGHeader.Origin = orginEndPOint;
                ReservationAdvancedService.EndPoint destEndPOint = new ReservationAdvancedService.EndPoint();
                destEndPOint.entityID = Request.DestinationEntityID;
                destEndPOint.systemType = Request.DestinationSystemType;
                OGHeader.Destination = destEndPOint;
                ReservationAdvancedService.OGHeaderAuthentication Auth = new ReservationAdvancedService.OGHeaderAuthentication();
                ReservationAdvancedService.OGHeaderAuthenticationUserCredentials userCredentials = new ReservationAdvancedService.OGHeaderAuthenticationUserCredentials();
                userCredentials.UserName = Request.Username;
                userCredentials.UserPassword = Request.Password;
                userCredentials.Domain = Request.HotelDomain;
                Auth.UserCredentials = userCredentials;
                OGHeader.Authentication = Auth;
                #endregion

                #region Request Body

                ReservationAdvancedService.CheckOutRequest CORequest = new ReservationAdvancedService.CheckOutRequest();
                if (Request.SendFolio != null && Request.SendFolio.Value)
                {
                    CORequest.EmailFolio = true;
                    CORequest.overrideEmailPrivacy = true;
                }


                ReservationAdvancedService.ReservationRequestBase RB = new ReservationAdvancedService.ReservationRequestBase();

                ReservationAdvancedService.HotelReference HF = new ReservationAdvancedService.HotelReference();
                HF.chainCode = Request.ChainCode;
                HF.hotelCode = Request.HotelDomain;
                RB.HotelReference = HF;

                ReservationAdvancedService.UniqueID uID = new ReservationAdvancedService.UniqueID();
                uID.type = (ReservationAdvancedService.UniqueIDType)ReservationService.UniqueIDType.EXTERNAL;
                uID.source = "RESV_NAME_ID";
                uID.Value = Request.OperaReservation.ReservationNameID;
                ReservationAdvancedService.UniqueID[] UIDLIST = new ReservationAdvancedService.UniqueID[1];
                UIDLIST[0] = uID;
                RB.ReservationID = UIDLIST;
                CORequest.ReservationRequest = RB;
                //CORequest.CreditCardInfo = null;
                //CORequest.canHandleVaultedCreditCard = false;
                //CORequest.canHandleVaultedCreditCardSpecified = true;



                ReservationAdvancedService.ResvAdvancedServiceSoapClient ResAdvPortClient = new ReservationAdvancedService.ResvAdvancedServiceSoapClient();
                bool isOperaCloudEnabled = false;
                isOperaCloudEnabled = (ConfigurationManager.AppSettings["OperaCloudEnabled"] != null
                                && !string.IsNullOrEmpty(ConfigurationManager.AppSettings["OperaCloudEnabled"].ToString())
                                && bool.TryParse(ConfigurationManager.AppSettings["OperaCloudEnabled"].ToString(), out isOperaCloudEnabled)) ? isOperaCloudEnabled : false;
                if (isOperaCloudEnabled)
                {
                    ResAdvPortClient.Endpoint.Behaviors.Add(new Helper.CustomEndpointBehaviour(ConfigurationManager.AppSettings["WSSEUserName"].ToString(),
                                            ConfigurationManager.AppSettings["WSSEPassword"].ToString(),
                                            Request.Username, Request.Password, Request.HotelDomain));
                }
                ReservationAdvancedService.CheckOutResponse COResponse = new ReservationAdvancedService.CheckOutResponse();
                COResponse = ResAdvPortClient.CheckOut(ref OGHeader, CORequest);
                

                if (COResponse.Result.resultStatusFlag == ReservationAdvancedService.ResultStatusFlag.SUCCESS)
                {
                    
                    return new Models.OWS.OwsResponseModel()
                    {
                        responseMessage = "Success",
                        statusCode = 101,
                        result = true
                    };
                }
                else
                {
                    return new Models.OWS.OwsResponseModel()
                    {
                        responseMessage = COResponse.Result != null ? (COResponse.Result.Text != null ? COResponse.Result.Text[0].Value : COResponse.Result.OperaErrorCode ): "Check out failled",
                        statusCode = 4001,
                        result = false
                    };
                }


                #endregion

                #endregion
            }
            catch (Exception ex)
            {
                return new Models.OWS.OwsResponseModel()
                {
                    responseMessage = "Generic Exception : " + ex.Message,
                    statusCode = 1002,
                    result = false
                };
            }
        }

        public Models.OWS.OwsResponseModel GetRegcarBase64(Models.OWS.OperaReservation reservation)
        {
            try
            {
                string Base64RegCard = null;
                

                #region Initilize Reportviewer
                ReportViewer rv = new ReportViewer();
                rv.ProcessingMode = ProcessingMode.Local;
                //rv.LocalReport.
                //using (FileStream fs = new FileStream(System.Web.Hosting.HostingEnvironment.MapPath("~/Resources/RDLC/RegCard.rdlc"), FileMode.Open))
                {
                    using (StreamReader rdlcSR = new StreamReader(System.Web.Hosting.HostingEnvironment.MapPath("~/Resources/RDLC/RegCard.rdlc")))
                    {
                        rv.LocalReport.LoadReportDefinition(rdlcSR);// = System.Web.Hosting.HostingEnvironment.MapPath("~/Resources/RDLC/RegCard.rdlc");
                        rv.LocalReport.EnableExternalImages = true;
                        #endregion


                string noOfNights = "";
                if (reservation.ArrivalDate != null && reservation.DepartureDate != null)
                {
                    System.TimeSpan timeSpan = reservation.DepartureDate.Value.Subtract(reservation.ArrivalDate.Value);
                    noOfNights = timeSpan.Days.ToString();
                }

                string address = "";
                if (reservation.GuestProfiles != null && reservation.GuestProfiles.Count > 0 && reservation.GuestProfiles[0].Address != null && reservation.GuestProfiles[0].Address.Count > 0)
                {
                    address += reservation.GuestProfiles[0].Address[0].address1 + (!string.IsNullOrEmpty(reservation.GuestProfiles[0].Address[0].address2) ?
                                                                                                    ", " + reservation.GuestProfiles[0].Address[0].address2 : "");
                }

                string email = "";
                if (reservation.GuestProfiles != null && reservation.GuestProfiles.Count > 0 && reservation.GuestProfiles[0].Email != null && reservation.GuestProfiles[0].Email.Count > 0)
                {
                    foreach (Models.OWS.Email email1 in reservation.GuestProfiles[0].Email)
                    {
                        if (email1.primary != null && email1.primary.Value)
                        {
                            email = email1.email;
                            break;
                        }
                    }
                    if (string.IsNullOrEmpty(email))
                    {
                        email = reservation.GuestProfiles[0].Email[0].email;
                    }

                }

                string phone = "";
                if (reservation.GuestProfiles != null && reservation.GuestProfiles.Count > 0 && reservation.GuestProfiles[0].Phones != null && reservation.GuestProfiles[0].Phones.Count > 0)
                {
                    foreach (Models.OWS.Phone phones in reservation.GuestProfiles[0].Phones)
                    {
                        if (phones.primary != null && phones.primary.Value)
                        {
                            phone = phones.PhoneNumber;
                            break;
                        }
                    }
                    if (string.IsNullOrEmpty(phone))
                    {
                        phone = reservation.GuestProfiles[0].Phones[0].PhoneNumber;
                    }

                }

                string signatureImage = "";
                if (!string.IsNullOrEmpty(reservation.GuestSignature))
                {
                    byte[] bytes = Convert.FromBase64String(reservation.GuestSignature);
                    using (MemoryStream ms = new MemoryStream(bytes))
                    {
                        System.Drawing.Image.FromStream(ms).Save(System.Web.Hosting.HostingEnvironment.MapPath("~/Resources/Images/Temp/" + reservation.ReservationNumber + "_signature.jpeg"));
                    }
                    signatureImage = new Uri(System.Web.Hosting.HostingEnvironment.MapPath("~/Resources/Images/Temp/" + reservation.ReservationNumber + "_signature.jpeg")).AbsoluteUri;
                }

                List<ReportParameter> reportParameters = new List<ReportParameter>();

                //ReportParameter parameter = System.IO.File.Exists(System.Web.Hosting.HostingEnvironment.MapPath("~/Resources/RDLC/hotel-logo.png")) ?
                //                                new ReportParameter
                //                                ("logo",
                //                                new Uri(System.Web.Hosting.HostingEnvironment.MapPath("~/Resources/RDLC/hotel-logo.png")).AbsoluteUri) : null;

                //if (parameter != null)
                //    reportParameters.Add(parameter);
                ReportParameter parameter = null;
                foreach (ReportParameterInfo reportParameterInfos in rv.LocalReport.GetParameters())
                {
                    switch (reportParameterInfos.Name)
                    {
                                case "RoomType":
                                    parameter = new ReportParameter("RoomType", (reservation.RoomDetails != null ? reservation.RoomDetails.RoomType : ""));
                                    reportParameters.Add(parameter);
                                    break;

                                case "State":
                                    parameter = new ReportParameter("State", (reservation.GuestProfiles != null && reservation.GuestProfiles.Count > 0) ? (reservation.GuestProfiles[0].Address != null && reservation.GuestProfiles[0].Address.Count > 0)
                                    ? reservation.GuestProfiles[0].Address[0].state : "" : "");
                                    reportParameters.Add(parameter);
                                    break;

                                case "Title":
                            parameter = new ReportParameter("Title", (reservation.GuestProfiles != null && reservation.GuestProfiles.Count > 0) ? reservation.GuestProfiles[0].Title : "");
                            reportParameters.Add(parameter);
                            break;

                        case "FamilyName":
                            parameter = new ReportParameter("FamilyName", (reservation.GuestProfiles != null && reservation.GuestProfiles.Count > 0) ? 
                                                            (string.IsNullOrEmpty(reservation.GuestProfiles[0].LastName)?  reservation.GuestProfiles[0].FamilyName : reservation.GuestProfiles[0].LastName) : "");
                            reportParameters.Add(parameter);
                            break;

                        case "RoomNo":
                            parameter = new ReportParameter("RoomNo", (reservation.RoomDetails != null) ? reservation.RoomDetails.RoomNumber : "");
                            reportParameters.Add(parameter);
                            break;

                        case "GivenName":
                            parameter = new ReportParameter("GivenName", (reservation.GuestProfiles != null && reservation.GuestProfiles.Count > 0) ? reservation.GuestProfiles[0].FirstName : "");
                            reportParameters.Add(parameter);
                            break;

                        case "AdultCount":
                            parameter = new ReportParameter("AdultCount", reservation.Adults != null ? reservation.Adults.Value.ToString() : "");
                            reportParameters.Add(parameter);
                            break;

                        case "NoOfNights":
                            parameter = new ReportParameter("NoOfNights", noOfNights);
                            reportParameters.Add(parameter);
                            break;

                        case "Address":
                            parameter = new ReportParameter("Address", address);
                            reportParameters.Add(parameter);
                            break;

                        case "ContactNumber":
                            parameter = new ReportParameter("ContactNumber", phone);
                            reportParameters.Add(parameter);
                            break;

                        case "ArrivalDate":
                            parameter = new ReportParameter("ArrivalDate", reservation.ArrivalDate != null ? reservation.ArrivalDate.Value.ToString("dd/MM/yyyy") : "");
                            reportParameters.Add(parameter);
                            break;

                        case "DepartureDate":
                            parameter = new ReportParameter("DepartureDate", reservation.DepartureDate != null ? reservation.DepartureDate.Value.ToString("dd/MM/yyyy") : "");
                            reportParameters.Add(parameter);
                            break;

                        case "ReservationNumber":
                            parameter = new ReportParameter("ReservationNumber", !string.IsNullOrEmpty(reservation.ReservationNumber) ? reservation.ReservationNumber : "");
                            reportParameters.Add(parameter);
                            break;

                        case "DailyRate":
                            parameter = new ReportParameter("DailyRate", (reservation.RateDetails != null) ? (reservation.RateDetails.RateAmount != null ? reservation.RateDetails.RateAmount.Value.ToString() : "") : "");
                            reportParameters.Add(parameter);
                            break;

                        case "CityOrState":
                            parameter = new ReportParameter("CityOrState", (reservation.GuestProfiles != null && reservation.GuestProfiles.Count > 0) ? (reservation.GuestProfiles[0].Address != null && reservation.GuestProfiles[0].Address.Count > 0)
                                    ? reservation.GuestProfiles[0].Address[0].city : "" : "");
                            reportParameters.Add(parameter);
                            break;

                        case "PostalCode":
                            parameter = new ReportParameter("PostalCode", (reservation.GuestProfiles != null && reservation.GuestProfiles.Count > 0) ? (reservation.GuestProfiles[0].Address != null && reservation.GuestProfiles[0].Address.Count > 0)
                                    ? reservation.GuestProfiles[0].Address[0].zip : "" : "");
                            reportParameters.Add(parameter);
                            break;

                        case "Country":
                            parameter = new ReportParameter("Country", (reservation.GuestProfiles != null && reservation.GuestProfiles.Count > 0) ? (reservation.GuestProfiles[0].Address != null && reservation.GuestProfiles[0].Address.Count > 0)
                                    ? reservation.GuestProfiles[0].Address[0].country : "" : "");
                            reportParameters.Add(parameter);
                            break;

                        case "MembershipProgram":
                            parameter = new ReportParameter("MembershipProgram", (reservation.GuestProfiles != null && reservation.GuestProfiles.Count > 0) ? reservation.GuestProfiles[0].MembershipNumber : "");
                            reportParameters.Add(parameter);
                            break;

                        case "Email":
                            parameter = new ReportParameter("Email", email);
                            reportParameters.Add(parameter);
                            break;
                        case "Nationality":
                            parameter = new ReportParameter("Nationality", (reservation.GuestProfiles != null && reservation.GuestProfiles.Count > 0) ? reservation.GuestProfiles[0].Nationality : "");
                            reportParameters.Add(parameter);
                            break;

                        case "PassportNumber":
                            parameter = new ReportParameter("PassportNumber", (reservation.GuestProfiles != null && reservation.GuestProfiles.Count > 0) ? reservation.GuestProfiles[0].PassportNumber : "");
                            reportParameters.Add(parameter);
                            break;

                        case "DateOfBirth":
                            parameter = new ReportParameter("DateOfBirth", (reservation.GuestProfiles != null && reservation.GuestProfiles.Count > 0) ? "xx/xx/xxxx" : "xx/xx/xxxx");
                            reportParameters.Add(parameter);
                            break;

                        case "ModeOfPayment":
                            parameter = new ReportParameter("ModeOfPayment", (reservation.PaymentMethod != null) ? reservation.PaymentMethod.PaymentType : "");
                            reportParameters.Add(parameter);
                            break;

                        case "GuestSignature":
                            parameter = new ReportParameter("GuestSignature", reservation.GuestSignature);
                            reportParameters.Add(parameter);
                            break;
                    }
                }



                rv.LocalReport.SetParameters(reportParameters);

                rv.LocalReport.Refresh();


                #region Private Members
                byte[] streamBytes = null;
                string mimeType = "";
                string encoding = "";
                string filenameExtension = "";
                string[] streamids = null;
                Warning[] warnings = null;
                #endregion

                streamBytes = rv.LocalReport.Render("PDF", null, out mimeType, out encoding, out filenameExtension, out streamids, out warnings);
                Base64RegCard = Convert.ToBase64String(streamBytes);
                rv.LocalReport.Refresh();
                    }
                }

                rv.LocalReport.Dispose();
                
                return new Models.OWS.OwsResponseModel()
                {
                    result = true,
                    responseData = Base64RegCard
                };
                
            }
            catch(Exception ex)
            {
                return new Models.OWS.OwsResponseModel()
                {
                    result = false,
                    responseData = null,
                    responseMessage = ex.ToString()
                };
            }
        }

        public Models.OWS.OwsResponseModel AssignRoomToReservation(Models.OWS.OwsRequestModel Request)
        {
            try
            {
                new LogHelper().Debug("Assignroom request : " + JsonConvert.SerializeObject(Request), Request.AssignRoomRequest.ReservationNameID, "AssignRoomToReservation", "API", "OWS");
                ReservationService.AssignRoomRequest assignRoomReq = new ReservationService.AssignRoomRequest();
                ReservationService.AssignRoomResponse assignRoomRes = new ReservationService.AssignRoomResponse();
                
                #region Request Header

                string temp = Helper.Helper.Get8Digits();
                ReservationService.OGHeader OGHeader = new ReservationService.OGHeader();
                OGHeader.transactionID = temp;
                OGHeader.timeStamp = DateTime.Now;
                OGHeader.primaryLangID = Request.Language; //English
                ReservationService.EndPoint orginEndPOint = new ReservationService.EndPoint();
                orginEndPOint.entityID = Request.KioskID; //Kiosk Identifier
                orginEndPOint.systemType = Request.SystemType;//"KIOSK";
                OGHeader.Origin = orginEndPOint;
                ReservationService.EndPoint destEndPOint = new ReservationService.EndPoint();
                destEndPOint.entityID = Request.DestinationEntityID;
                destEndPOint.systemType = Request.DestinationSystemType;
                OGHeader.Destination = destEndPOint;
                ReservationService.OGHeaderAuthentication Auth = new ReservationService.OGHeaderAuthentication();
                ReservationService.OGHeaderAuthenticationUserCredentials userCredentials = new ReservationService.OGHeaderAuthenticationUserCredentials();
                userCredentials.UserName = Request.Username;
                userCredentials.UserPassword = Request.Password;
                //userCredentials.Domain = Request.HotelDomain;
                Auth.UserCredentials = userCredentials;
                OGHeader.Authentication = Auth;
                #endregion

                ReservationService.ReservationServiceSoapClient ResSoapCLient = new ReservationService.ReservationServiceSoapClient();
                bool isOperaCloudEnabled = false;
                isOperaCloudEnabled = (ConfigurationManager.AppSettings["OperaCloudEnabled"] != null
                                && !string.IsNullOrEmpty(ConfigurationManager.AppSettings["OperaCloudEnabled"].ToString())
                                && bool.TryParse(ConfigurationManager.AppSettings["OperaCloudEnabled"].ToString(), out isOperaCloudEnabled)) ? isOperaCloudEnabled : false;
                if (isOperaCloudEnabled)
                {
                    ResSoapCLient.Endpoint.Behaviors.Add(new Helper.CustomEndpointBehaviour(ConfigurationManager.AppSettings["WSSEUserName"].ToString(),
                                            ConfigurationManager.AppSettings["WSSEPassword"].ToString(),
                                            Request.Username, Request.Password, Request.HotelDomain));
                }

                #region Request Body
                ReservationService.UniqueID uID = new ReservationService.UniqueID();
                uID.type = ReservationService.UniqueIDType.INTERNAL;
                uID.source = "RESV_NAME_ID";
                uID.Value = Request.AssignRoomRequest.ReservationNameID;
                assignRoomReq.ResvNameId = uID;

                ReservationService.HotelReference HF = new ReservationService.HotelReference();
                HF.hotelCode = Request.HotelDomain;
                assignRoomReq.HotelReference = HF;

                assignRoomReq.RoomNoRequested = Request.AssignRoomRequest.RoomNumber;
                assignRoomReq.StationID = Request.AssignRoomRequest.StationID;



                #endregion


                assignRoomRes = ResSoapCLient.AssignRoom(ref OGHeader, assignRoomReq);
                ReservationService.GDSResultStatus status = assignRoomRes.Result;
                
                if (status.resultStatusFlag.Equals(ReservationService.ResultStatusFlag.SUCCESS))
                {
                    return new Models.OWS.OwsResponseModel
                    {
                        responseMessage = "SUCCESS",
                        statusCode = 101,
                        result = true
                    };
                }
                else
                {
                    return new Models.OWS.OwsResponseModel
                    {
                        responseMessage = "FAILED, ",
                        statusCode = 102,
                        result = false
                    };
                }
            }
            catch (Exception ex)
            {
                return new Models.OWS.OwsResponseModel
                {
                    responseMessage = "Generic Exception : " + ex.ToString(),
                    statusCode = 10002,
                    result = false
                };
            }
        }
        public Models.OWS.OwsResponseModel GetBusinessDate(Models.OWS.OwsRequestModel Request)
        {
            #region Getting BusinessDate
            try
            {
                new LogHelper().Debug("Get Business Date request : " + JsonConvert.SerializeObject(Request), "", "GetBusinessDate", "API", "OWS");
                #region Request Header
                string temp1 = Helper.Helper.Get8Digits();
                InformationService.OGHeader OG = new InformationService.OGHeader();
                OG.transactionID = temp1;
                OG.timeStamp = DateTime.Now;
                OG.primaryLangID = Request.Language; //English
                InformationService.EndPoint orginEndPnt = new InformationService.EndPoint();
                orginEndPnt.entityID = Request.KioskID; //Kiosk Identifier
                orginEndPnt.systemType = Request.SystemType;
                OG.Origin = orginEndPnt;
                InformationService.EndPoint destEndPnt = new InformationService.EndPoint();
                destEndPnt.entityID = Request.DestinationEntityID;
                destEndPnt.systemType = Request.DestinationSystemType;
                OG.Destination = destEndPnt;
                InformationService.OGHeaderAuthentication Authnt = new InformationService.OGHeaderAuthentication();
                InformationService.OGHeaderAuthenticationUserCredentials userCrdntls = new InformationService.OGHeaderAuthenticationUserCredentials();
                userCrdntls.UserName = Request.Username;
                userCrdntls.UserPassword = Request.Password;
                userCrdntls.Domain = Request.HotelDomain;
                Authnt.UserCredentials = userCrdntls;
                OG.Authentication = Authnt;
                #endregion

                #region Request Body
                InformationService.LovRequest LOVReq = new InformationService.LovRequest();
                InformationService.LovResponse LOVRes = new InformationService.LovResponse();

                InformationService.LovQueryType2 LOVQuery = new InformationService.LovQueryType2();
                LOVQuery.LovIdentifier = "BUSINESSDATE";
                LOVReq.Item = LOVQuery;
                #endregion

                InformationService.InformationSoapClient InfoPortClient = new InformationService.InformationSoapClient();
                bool isOperaCloudEnabled = false;
                isOperaCloudEnabled = (ConfigurationManager.AppSettings["OperaCloudEnabled"] != null
                                && !string.IsNullOrEmpty(ConfigurationManager.AppSettings["OperaCloudEnabled"].ToString())
                                && bool.TryParse(ConfigurationManager.AppSettings["OperaCloudEnabled"].ToString(), out isOperaCloudEnabled)) ? isOperaCloudEnabled : false;
                if (isOperaCloudEnabled)
                {
                    InfoPortClient.Endpoint.Behaviors.Add(new Helper.CustomEndpointBehaviour(ConfigurationManager.AppSettings["WSSEUserName"].ToString(),
                                            ConfigurationManager.AppSettings["WSSEPassword"].ToString(),
                                            Request.Username, Request.Password, Request.HotelDomain));
                }
                LOVRes = InfoPortClient.QueryLov(ref OG, LOVReq);
                if (LOVRes.Result.resultStatusFlag == InformationService.ResultStatusFlag.SUCCESS)
                {
                    string date = null;
                    string month = null;
                    string year = null;
                    foreach (InformationService.LovQueryResultType Value in LOVRes.LovQueryResult)
                    {
                        if (!string.IsNullOrEmpty(Value.qualifierType) && Value.qualifierType.Equals("Day"))
                            date = Value.qualifierValue;
                        if (!string.IsNullOrEmpty(Value.secondaryQualifierType) && Value.secondaryQualifierType.Equals("Month"))
                            month = Value.secondaryQualifierValue;
                        if (!string.IsNullOrEmpty(Value.tertiaryQualifierType) && Value.tertiaryQualifierType.Equals("Year"))
                            year = Value.tertiaryQualifierValue;
                    }

                    if (!string.IsNullOrEmpty(date) && !string.IsNullOrEmpty(month) && !string.IsNullOrEmpty(year))
                    {
                        return new OwsResponseModel()
                        {
                            result = true,
                            responseData = new DateTime(Int32.Parse(year), Int32.Parse(month), Int32.Parse(date)),
                            responseMessage = "Success"
                        };
                    }
                    else
                    {
                        return new OwsResponseModel()
                        {
                            result = false,
                            responseData = null,
                            responseMessage = "Null returned by opera"
                        };
                    }
                }
                else
                {
                    return new OwsResponseModel()
                    {
                        result = false,
                        responseData = null,
                        responseMessage = (LOVRes.Result.Text != null && LOVRes.Result.Text.Length > 0) ? string.Join(" ",LOVRes.Result.Text.Select(x => x.Value).ToArray()) : LOVRes.Result.OperaErrorCode
                    };
                }

            }
            catch (Exception ex) 
            {
                return new OwsResponseModel()
                {
                    result = false,
                    responseData = null,
                    responseMessage = ex.Message
                };
            }
            #endregion
        }
        public Models.OWS.OwsResponseModel MakePayment(Models.OWS.OwsRequestModel Request)
        {
            try
            {

                new LogHelper().Debug("MakePayment request : " + JsonConvert.SerializeObject(Request), Request.MakePaymentRequest.ReservationNameID, "MakePayment", "API", "OWS");
                DateTime? PostingDate = null;
                
                
                #region Getting BusinessDate
                try
                {
                    #region Request Header
                    string temp1 = Helper.Helper.Get8Digits();
                    InformationService.OGHeader OG = new InformationService.OGHeader();
                    OG.transactionID = temp1;
                    OG.timeStamp = DateTime.Now;
                    OG.primaryLangID = Request.Language; //English
                    InformationService.EndPoint orginEndPnt = new InformationService.EndPoint();
                    orginEndPnt.entityID = Request.KioskID; //Kiosk Identifier
                    orginEndPnt.systemType = Request.SystemType;
                    OG.Origin = orginEndPnt;
                    InformationService.EndPoint destEndPnt = new InformationService.EndPoint();
                    destEndPnt.entityID = Request.DestinationEntityID;
                    destEndPnt.systemType = Request.DestinationSystemType;
                    OG.Destination = destEndPnt;
                    InformationService.OGHeaderAuthentication Authnt = new InformationService.OGHeaderAuthentication();
                    InformationService.OGHeaderAuthenticationUserCredentials userCrdntls = new InformationService.OGHeaderAuthenticationUserCredentials();
                    userCrdntls.UserName = Request.Username;
                    userCrdntls.UserPassword = Request.Password;
                    userCrdntls.Domain = Request.HotelDomain;
                    Authnt.UserCredentials = userCrdntls;
                    OG.Authentication = Authnt;
                    #endregion

                    #region Request Body
                    InformationService.LovRequest LOVReq = new InformationService.LovRequest();
                    InformationService.LovResponse LOVRes = new InformationService.LovResponse();

                    InformationService.LovQueryType2 LOVQuery = new InformationService.LovQueryType2();
                    LOVQuery.LovIdentifier = "BUSINESSDATE";
                    LOVReq.Item = LOVQuery;
                    #endregion

                    InformationService.InformationSoapClient InfoPortClient = new InformationService.InformationSoapClient();
                    bool isOperaCloudEnabled1 = false;
                    isOperaCloudEnabled1 = (ConfigurationManager.AppSettings["OperaCloudEnabled"] != null
                                    && !string.IsNullOrEmpty(ConfigurationManager.AppSettings["OperaCloudEnabled"].ToString())
                                    && bool.TryParse(ConfigurationManager.AppSettings["OperaCloudEnabled"].ToString(), out isOperaCloudEnabled1)) ? isOperaCloudEnabled1 : false;
                    if (isOperaCloudEnabled1)
                    {
                        InfoPortClient.Endpoint.Behaviors.Add(new Helper.CustomEndpointBehaviour(ConfigurationManager.AppSettings["WSSEUserName"].ToString(),
                                                ConfigurationManager.AppSettings["WSSEPassword"].ToString(),
                                                Request.Username, Request.Password, Request.HotelDomain));
                    }
                    LOVRes = InfoPortClient.QueryLov(ref OG, LOVReq);
                    if (LOVRes.Result.resultStatusFlag == InformationService.ResultStatusFlag.SUCCESS)
                    {
                        string date = null;
                        string month = null;
                        string year = null;
                        foreach (InformationService.LovQueryResultType Value in LOVRes.LovQueryResult)
                        {
                            if (!string.IsNullOrEmpty(Value.qualifierType) && Value.qualifierType.Equals("Day"))
                                date = Value.qualifierValue;
                            if (!string.IsNullOrEmpty(Value.secondaryQualifierType) && Value.secondaryQualifierType.Equals("Month"))
                                month = Value.secondaryQualifierValue;
                            if (!string.IsNullOrEmpty(Value.tertiaryQualifierType) && Value.tertiaryQualifierType.Equals("Year"))
                                year = Value.tertiaryQualifierValue;
                        }

                        if (!string.IsNullOrEmpty(date) && !string.IsNullOrEmpty(month) && !string.IsNullOrEmpty(year))
                        {
                            PostingDate = new DateTime(Int32.Parse(year), Int32.Parse(month), Int32.Parse(date));
                        }
                    }

                }
                catch (Exception ex) { //System.IO.File.AppendAllText(System.Web.Hosting.HostingEnvironment.MapPath(@"~\Error.txt"), ex.Message);
                                       }
                #endregion



                #region Request Header
                string temp = Helper.Helper.Get8Digits();
                ReservationAdvancedService.OGHeader OGHeader = new ReservationAdvancedService.OGHeader();
                OGHeader.transactionID = temp;
                OGHeader.timeStamp = DateTime.Now;
                OGHeader.primaryLangID = Request.Language; //English
                ReservationAdvancedService.EndPoint orginEndPOint = new ReservationAdvancedService.EndPoint();
                orginEndPOint.entityID = Request.KioskID; //Kiosk Identifier
                orginEndPOint.systemType = Request.SystemType;
                OGHeader.Origin = orginEndPOint;
                ReservationAdvancedService.EndPoint destEndPOint = new ReservationAdvancedService.EndPoint();
                destEndPOint.entityID = Request.DestinationEntityID;
                destEndPOint.systemType = Request.DestinationSystemType;
                OGHeader.Destination = destEndPOint;
                ReservationAdvancedService.OGHeaderAuthentication Auth = new ReservationAdvancedService.OGHeaderAuthentication();
                ReservationAdvancedService.OGHeaderAuthenticationUserCredentials userCredentials = new ReservationAdvancedService.OGHeaderAuthenticationUserCredentials();
                userCredentials.UserName = Request.Username;
                userCredentials.UserPassword = Request.Password;
                userCredentials.Domain = Request.HotelDomain;
                Auth.UserCredentials = userCredentials;
                OGHeader.Authentication = Auth;
                #endregion

                #region Request Body

                ReservationAdvancedService.MakePaymentRequest MPRequest = new ReservationAdvancedService.MakePaymentRequest();
                ReservationAdvancedService.Posting PaymentPosting = new ReservationAdvancedService.Posting();
                PaymentPosting.PostDate = PostingDate != null ? (DateTime)PostingDate : DateTime.Now;

                PaymentPosting.PostDateSpecified = true;
                PaymentPosting.PostTime = DateTime.Now;
                PaymentPosting.PostTimeSpecified = true;
                PaymentPosting.LongInfo = Request.MakePaymentRequest.PaymentInfo;
                PaymentPosting.Charge = Request.MakePaymentRequest.Amount;
                PaymentPosting.ChargeSpecified = true;
                PaymentPosting.StationID = Request.MakePaymentRequest.StationID;

                PaymentPosting.UserID = !string.IsNullOrEmpty(Request.MakePaymentRequest.UserName) ? Request.MakePaymentRequest.UserName : Request.Username;
                PaymentPosting.FolioViewNo = Request.MakePaymentRequest.WindowNumber != null ? Request.MakePaymentRequest.WindowNumber.Value : 1;
                PaymentPosting.FolioViewNoSpecified = true;

                ReservationAdvancedService.ReservationRequestBase RRBase = new ReservationAdvancedService.ReservationRequestBase();
                ReservationAdvancedService.UniqueID[] rUniqueIDList = new ReservationAdvancedService.UniqueID[1];
                ReservationAdvancedService.UniqueID uID = new ReservationAdvancedService.UniqueID();
                uID.type = ReservationAdvancedService.UniqueIDType.INTERNAL;
                uID.source = "RESV_NAME_ID";
                uID.Value = Request.MakePaymentRequest.ReservationNameID;
                rUniqueIDList[0] = uID;
                RRBase.ReservationID = rUniqueIDList;
                ReservationAdvancedService.HotelReference HF = new ReservationAdvancedService.HotelReference();
                HF.hotelCode = Request.HotelDomain;
                RRBase.HotelReference = HF;
                PaymentPosting.ReservationRequestBase = RRBase;
                MPRequest.Posting = PaymentPosting;

                ReservationAdvancedService.CreditCardInfo CCInfo = new ReservationAdvancedService.CreditCardInfo();
                
                ReservationAdvancedService.CreditCard CC = new ReservationAdvancedService.CreditCard();

                //ReservationAdvancedService.cred
                if (Request.MakePaymentRequest.PaymentTypeCode != "CA")
                {
                    bool isOPIEnabled = false;
                    isOPIEnabled = (ConfigurationManager.AppSettings["OPIEnabled"] != null
                                    && !string.IsNullOrEmpty(ConfigurationManager.AppSettings["OPIEnabled"].ToString())
                                    && bool.TryParse(ConfigurationManager.AppSettings["OPIEnabled"].ToString()  , out isOPIEnabled)) ? isOPIEnabled : false;
                    CC.chipAndPin = false;
                    CC.chipAndPinSpecified = true;
                    CC.cardType = Request.MakePaymentRequest.PaymentTypeCode;//"WEB";
                    
                    if(isOPIEnabled)
                    {
                        CC.Item = new ReservationAdvancedService.VaultedCardType()
                        {
                            vaultedCardID = Request.MakePaymentRequest.ApprovalCode,
                            lastFourDigits = Request.MakePaymentRequest.MaskedCardNumber.Substring(Request.MakePaymentRequest.MaskedCardNumber.Length - 4)
                        };
                    }
                    else
                    {
                        CC.Item = Request.MakePaymentRequest.MaskedCardNumber;// "4687560100136162";
                        CC.cardCode = Request.MakePaymentRequest.PaymentTypeCode;
                    }
                    if (Request.MakePaymentRequest.ExpiryDate != null)
                    {
                        CC.expirationDate = Request.MakePaymentRequest.ExpiryDate.Value;
                    }
                    else
                    {
                        CC.expirationDate = DateTime.Now.AddYears(2);
                    }
                    CC.expirationDateSpecified = true;
                    CCInfo.Item = CC;
                }
                else
                {
                    CC.cardType = Request.MakePaymentRequest.PaymentTypeCode;
                    CCInfo.Item = CC;
                }
                MPRequest.Reference = Request.MakePaymentRequest.PaymentRefernce;
                MPRequest.CreditCardInfo = CCInfo;

                ReservationAdvancedService.ResvAdvancedServiceSoapClient ResAdvPortClient = new ReservationAdvancedService.ResvAdvancedServiceSoapClient();
                bool isOperaCloudEnabled = false;
                isOperaCloudEnabled = (ConfigurationManager.AppSettings["OperaCloudEnabled"] != null
                                && !string.IsNullOrEmpty(ConfigurationManager.AppSettings["OperaCloudEnabled"].ToString())
                                && bool.TryParse(ConfigurationManager.AppSettings["OperaCloudEnabled"].ToString(), out isOperaCloudEnabled)) ? isOperaCloudEnabled : false;
                if (isOperaCloudEnabled)
                {
                    ResAdvPortClient.Endpoint.Behaviors.Add(new Helper.CustomEndpointBehaviour(ConfigurationManager.AppSettings["WSSEUserName"].ToString(),
                                            ConfigurationManager.AppSettings["WSSEPassword"].ToString(),
                                            Request.Username, Request.Password, Request.HotelDomain));
                }
                ReservationAdvancedService.MakePaymentResponse RSResponse = new ReservationAdvancedService.MakePaymentResponse();
                #endregion
                //ResAdvPortClient.Endpoint.Behaviors.Add(new Helper.CustomEndpointBehaviour("Test USE", "Request.WSSEPassword", "Request.KioskUserName", "Request.KioskPassword", "Request.HotelDomain"));
                RSResponse = ResAdvPortClient.MakePayment(ref OGHeader, MPRequest);

                

                if (RSResponse.Result.resultStatusFlag == ReservationAdvancedService.ResultStatusFlag.SUCCESS)
                {
                    return new Models.OWS.OwsResponseModel()
                    {
                        responseMessage = "Success",
                        statusCode = 101,
                        result = true
                    };

                }
                else
                {
                    //System.IO.File.WriteAllText(System.Web.Hosting.HostingEnvironment.MapPath(@"~\Log.txt"), Newtonsoft.Json.JsonConvert.SerializeObject(RSResponse));
                    return new Models.OWS.OwsResponseModel()
                    {                        
                        responseMessage = RSResponse.Result != null ? RSResponse.Result.Text != null ? string.Join(" ", RSResponse.Result.Text.Select(x=> x.Value).ToArray()) : "Failled" : "Failled",
                        statusCode = -1,
                        result = false
                    };
                }
            }
            catch (Exception ex)
            {
                return new Models.OWS.OwsResponseModel()
                {
                    responseMessage = "Generic Exception : " + ex.Message,
                    statusCode = -1,
                    result = false
                };
                
            }
        }
        public Models.OWS.OwsResponseModel GetEmaillistForProfile(Models.OWS.OwsRequestModel Request)
        {
            try
            {
                
                #region Request

                #region Request Header
                string temp = Helper.Helper.Get8Digits();
                NameService.OGHeader OGHeader = new NameService.OGHeader();
                OGHeader.transactionID = temp;
                OGHeader.timeStamp = DateTime.Now;
                OGHeader.primaryLangID = Request.Language; //English
                NameService.EndPoint orginEndPOint = new NameService.EndPoint();
                orginEndPOint.entityID = Request.KioskID; //Kiosk Identifier
                orginEndPOint.systemType = Request.SystemType;
                OGHeader.Origin = orginEndPOint;
                NameService.EndPoint destEndPOint = new NameService.EndPoint();
                destEndPOint.entityID = Request.DestinationEntityID;
                destEndPOint.systemType = Request.DestinationSystemType;
                OGHeader.Destination = destEndPOint;
                NameService.OGHeaderAuthentication Auth = new NameService.OGHeaderAuthentication();
                NameService.OGHeaderAuthenticationUserCredentials userCredentials = new NameService.OGHeaderAuthenticationUserCredentials();
                userCredentials.UserName = Request.Username;
                userCredentials.UserPassword = Request.Password;
                userCredentials.Domain = Request.HotelDomain;
                Auth.UserCredentials = userCredentials;
                OGHeader.Authentication = Auth;
                #endregion

                #region Request Body

                NameService.FetchEmailListRequest FetchEmailListReq = new NameService.FetchEmailListRequest();
                NameService.UniqueID uID = new NameService.UniqueID();
                uID.type = (NameService.UniqueIDType)NameService.UniqueIDType.INTERNAL;
                uID.Value = Request.fetchProfileRequest.NameID;
                FetchEmailListReq.NameID = uID;
                #endregion

                #endregion

                NameService.NameServiceSoapClient PortClient = new NameService.NameServiceSoapClient();
                bool isOperaCloudEnabled = false;
                isOperaCloudEnabled = (ConfigurationManager.AppSettings["OperaCloudEnabled"] != null
                                && !string.IsNullOrEmpty(ConfigurationManager.AppSettings["OperaCloudEnabled"].ToString())
                                && bool.TryParse(ConfigurationManager.AppSettings["OperaCloudEnabled"].ToString(), out isOperaCloudEnabled)) ? isOperaCloudEnabled : false;
                if (isOperaCloudEnabled)
                {
                    PortClient.Endpoint.Behaviors.Add(new Helper.CustomEndpointBehaviour(ConfigurationManager.AppSettings["WSSEUserName"].ToString(),
                                            ConfigurationManager.AppSettings["WSSEPassword"].ToString(),
                                            Request.Username, Request.Password, Request.HotelDomain));
                }
                NameService.FetchEmailListResponse EMailListResp = PortClient.FetchEmailList(ref OGHeader, FetchEmailListReq);
                //System.IO.File.AppendAllLines(System.Web.Hosting.HostingEnvironment.MapPath(@"~\EmailList.txt"), new string[] { JsonConvert.SerializeObject(EMailListResp) });
                if (EMailListResp.Result.resultStatusFlag == NameService.ResultStatusFlag.SUCCESS)
                {
                    //System.IO.File.AppendAllLines(System.Web.Hosting.HostingEnvironment.MapPath(@"~\EmailList.txt"), new string[] { "Email found :- " });
                    List<Models.OWS.Email> LEmail = new List<Models.OWS.Email>();
                    

                    foreach (NameService.NameEmail NEmail in EMailListResp.NameEmailList)
                    {
                        Models.OWS.Email Email = new Models.OWS.Email();
                        Email.displaySequence = NEmail.displaySequence;
                        Email.operaId = NEmail.operaId;
                        Email.email = NEmail.Value;
                        Email.emailType = NEmail.emailType;
                        Email.primary = NEmail.primary;
                        LEmail.Add(Email);

                    }
                    

                    return new Models.OWS.OwsResponseModel
                    {
                        responseData = LEmail,
                        responseMessage = "Success",
                        statusCode = 101,
                        result = true
                    };
                }
                else
                {
                    return new Models.OWS.OwsResponseModel
                    {
                        responseMessage = EMailListResp.Result != null ? EMailListResp.Result.Text[0].ToString() : "Error to retreave phone list",
                        statusCode = 9001,
                        result = false
                    };
                }





            }
            catch (Exception ex)
            {
                //Nlog debug
                return new Models.OWS.OwsResponseModel
                {
                    responseMessage = "Generic Exception : " + ex.Message,
                    statusCode = 1002,
                    result = false
                };
            }
        }
        public Models.OWS.OwsResponseModel GetPhonelistForProfile(Models.OWS.OwsRequestModel Request)
        {
            try
            {
                #region Request

                #region Request Header
                string temp = Helper.Helper.Get8Digits();
                NameService.OGHeader OGHeader = new NameService.OGHeader();
                OGHeader.transactionID = temp;
                OGHeader.timeStamp = DateTime.Now;
                OGHeader.primaryLangID = Request.Language; //English
                NameService.EndPoint orginEndPOint = new NameService.EndPoint();
                orginEndPOint.entityID = Request.KioskID; //Kiosk Identifier
                orginEndPOint.systemType = Request.SystemType;
                OGHeader.Origin = orginEndPOint;
                NameService.EndPoint destEndPOint = new NameService.EndPoint();
                destEndPOint.entityID =Request.DestinationEntityID;
                destEndPOint.systemType = Request.DestinationSystemType;
                OGHeader.Destination = destEndPOint;
                NameService.OGHeaderAuthentication Auth = new NameService.OGHeaderAuthentication();
                NameService.OGHeaderAuthenticationUserCredentials userCredentials = new NameService.OGHeaderAuthenticationUserCredentials();
                userCredentials.UserName = Request.Username;
                userCredentials.UserPassword = Request.Password;
                userCredentials.Domain = Request.HotelDomain;
                Auth.UserCredentials = userCredentials;
                OGHeader.Authentication = Auth;
                #endregion

                #region Request Body

                NameService.FetchPhoneListRequest FetchPhoneListReq = new NameService.FetchPhoneListRequest();
                NameService.UniqueID uID = new NameService.UniqueID();
                uID.type = (NameService.UniqueIDType)NameService.UniqueIDType.INTERNAL;
                uID.Value = Request.fetchProfileRequest.NameID;
                FetchPhoneListReq.NameID = uID;
                #endregion

                #endregion

                NameService.NameServiceSoapClient PortClient = new NameService.NameServiceSoapClient();
                bool isOperaCloudEnabled = false;
                isOperaCloudEnabled = (ConfigurationManager.AppSettings["OperaCloudEnabled"] != null
                                && !string.IsNullOrEmpty(ConfigurationManager.AppSettings["OperaCloudEnabled"].ToString())
                                && bool.TryParse(ConfigurationManager.AppSettings["OperaCloudEnabled"].ToString(), out isOperaCloudEnabled)) ? isOperaCloudEnabled : false;
                if (isOperaCloudEnabled)
                {
                    PortClient.Endpoint.Behaviors.Add(new Helper.CustomEndpointBehaviour(ConfigurationManager.AppSettings["WSSEUserName"].ToString(),
                                            ConfigurationManager.AppSettings["WSSEPassword"].ToString(),
                                            Request.Username, Request.Password, Request.HotelDomain));
                }
                NameService.FetchPhoneListResponse PhoneListResp = PortClient.FetchPhoneList(ref OGHeader, FetchPhoneListReq);
                if (PhoneListResp.Result.resultStatusFlag == NameService.ResultStatusFlag.SUCCESS)
                {
                    List<Models.OWS.Phone> LPhones = new List<Models.OWS.Phone>();

                    if (PhoneListResp.NamePhoneList != null && PhoneListResp.NamePhoneList.Length > 0)
                    {
                        foreach (NameService.NamePhone NPhone in PhoneListResp.NamePhoneList)
                        {
                            Models.OWS.Phone Phones = new Models.OWS.Phone();
                            Phones.displaySequence = NPhone.displaySequence;
                            Phones.operaId = NPhone.operaId;
                            Phones.PhoneNumber = NPhone.Item.ToString();
                            Phones.phoneRole = NPhone.phoneRole;
                            Phones.phoneType = NPhone.phoneType;
                            Phones.primary = NPhone.primary;
                            LPhones.Add(Phones);
                        }

                        return new Models.OWS.OwsResponseModel
                        {
                            responseData = LPhones,
                            responseMessage = "Success",
                            statusCode = 101,
                            result = true
                        };
                    }
                    else
                    {
                        return new Models.OWS.OwsResponseModel
                        {
                            responseMessage = "No items found",
                            statusCode = 8001,
                            result = false
                        };
                    }
                }
                else
                {
                    return new Models.OWS.OwsResponseModel
                    {
                        responseMessage = PhoneListResp.Result != null ? PhoneListResp.Result.Text[0].ToString() : "Error to retreave phone list",
                        statusCode = 8001,
                        result = false
                    };
                }
            }
            catch (Exception ex)
            {

                //Nlog debug

                return new Models.OWS.OwsResponseModel
                {
                    responseMessage = "Generic Exception : " + ex.Message,
                    statusCode = 1002,
                    result = false
                };
            }
        }
        public Models.OWS.OwsResponseModel PreRegisterGuest(Models.OWS.OwsRequestModel Request)
        {
            try
            {
                #region Request

                #region Request Header
                string temp = Helper.Helper.Get8Digits();
                ReservationService.OGHeader OGHeader = new ReservationService.OGHeader();
                OGHeader.transactionID = temp;
                OGHeader.timeStamp = DateTime.Now;
                OGHeader.primaryLangID = Request.Language; //English
                ReservationService.EndPoint orginEndPOint = new ReservationService.EndPoint();
                orginEndPOint.entityID = Request.KioskID; //Kiosk Identifier
                orginEndPOint.systemType = Request.SystemType;
                OGHeader.Origin = orginEndPOint;
                ReservationService.EndPoint destEndPOint = new ReservationService.EndPoint();
                destEndPOint.entityID = Request.DestinationEntityID;
                destEndPOint.systemType = Request.DestinationSystemType;
                OGHeader.Destination = destEndPOint;
                ReservationService.OGHeaderAuthentication Auth = new ReservationService.OGHeaderAuthentication();
                ReservationService.OGHeaderAuthenticationUserCredentials userCredentials = new ReservationService.OGHeaderAuthenticationUserCredentials();
                userCredentials.UserName = Request.Username;
                userCredentials.UserPassword = Request.Password;
                userCredentials.Domain = Request.HotelDomain;
                Auth.UserCredentials = userCredentials;
                OGHeader.Authentication = Auth;
                #endregion

                #region Request Body

                ReservationService.ModifyBookingRequest ModifyBookingReq = new ReservationService.ModifyBookingRequest();
                //CORequest. = approval_code;
                ReservationService.HotelReservation Reservation = new ReservationService.HotelReservation();
                ReservationService.UniqueID uID = new ReservationService.UniqueID();
                Reservation.preRegisterFlag = true;
                uID.type = (ReservationService.UniqueIDType)ReservationService.UniqueIDType.INTERNAL;
                uID.Value = Request.PreregisterReservationRequest.ReservationNumber;
                ReservationService.UniqueID[] UIDLIST = new ReservationService.UniqueID[2];
                UIDLIST[0] = uID;
                uID = new ReservationService.UniqueID();
                uID.type = ReservationService.UniqueIDType.INTERNAL;
                uID.source = "LEGNUMBER";
                uID.Value = Request.PreregisterReservationRequest.LegNumber;
                UIDLIST[1] = uID;
                Reservation.UniqueIDList = UIDLIST;
                Reservation.preRegisterFlagSpecified = true;
                ModifyBookingReq.HotelReservation = Reservation;
                ModifyBookingReq.HotelReservation.preRegisterFlag = true;
                ModifyBookingReq.HotelReservation.preRegisterFlagSpecified = true;





                ReservationService.ReservationServiceSoapClient ResPortClient = new ReservationService.ReservationServiceSoapClient();
                bool isOperaCloudEnabled = false;
                isOperaCloudEnabled = (ConfigurationManager.AppSettings["OperaCloudEnabled"] != null
                                && !string.IsNullOrEmpty(ConfigurationManager.AppSettings["OperaCloudEnabled"].ToString())
                                && bool.TryParse(ConfigurationManager.AppSettings["OperaCloudEnabled"].ToString(), out isOperaCloudEnabled)) ? isOperaCloudEnabled : false;
                if (isOperaCloudEnabled)
                {
                    ResPortClient.Endpoint.Behaviors.Add(new Helper.CustomEndpointBehaviour(ConfigurationManager.AppSettings["WSSEUserName"].ToString(),
                                            ConfigurationManager.AppSettings["WSSEPassword"].ToString(),
                                            Request.Username, Request.Password, Request.HotelDomain));
                }
                ReservationService.ModifyBookingResponse MBookingRes = ResPortClient.ModifyBooking(ref OGHeader, ModifyBookingReq);
                if (MBookingRes.Result.resultStatusFlag == ReservationService.ResultStatusFlag.SUCCESS)
                {
                    return new Models.OWS.OwsResponseModel
                    {
                        responseMessage = "Success",
                        responseData = 101,
                        result = true
                    };
                }
                else
                {                    
                    return new Models.OWS.OwsResponseModel
                    {
                        responseMessage = MBookingRes.Result.GDSError.Value,
                        result = false,
                        statusCode = 5001
                    };
                }


                #endregion

                #endregion
            }
            catch (Exception ex)
            {
                
                return new Models.OWS.OwsResponseModel
                {
                    responseMessage = "Generic Exception : " + ex.Message,
                    result = false,
                    statusCode = 1002
                };
            }
        }

        public Models.OWS.OwsResponseModel UpdateAddress(Models.OWS.OwsRequestModel Request)
        {
            try
            {
                new LogHelper().Debug("Update Address request : " + JsonConvert.SerializeObject(Request), Request.UpdateProileRequest.ProfileID, "UpdateAddress", "API", "OWS");
                #region Request Header
                string temp = Helper.Helper.Get8Digits();
                NameService.OGHeader OGHeader = new NameService.OGHeader();
                OGHeader.transactionID = temp;
                OGHeader.timeStamp = DateTime.Now;
                OGHeader.primaryLangID = Request.Language; //English
                NameService.EndPoint orginEndPOint = new NameService.EndPoint();
                orginEndPOint.entityID = Request.KioskID; //Kiosk Identifier
                orginEndPOint.systemType = Request.SystemType;
                OGHeader.Origin = orginEndPOint;
                NameService.EndPoint destEndPOint = new NameService.EndPoint();
                destEndPOint.entityID = Request.DestinationEntityID;
                destEndPOint.systemType = Request.DestinationSystemType;
                OGHeader.Destination = destEndPOint;
                NameService.OGHeaderAuthentication Auth = new NameService.OGHeaderAuthentication();
                NameService.OGHeaderAuthenticationUserCredentials userCredentials = new NameService.OGHeaderAuthenticationUserCredentials();
                userCredentials.UserName = Request.Username;
                userCredentials.UserPassword = Request.Password;
                userCredentials.Domain = Request.HotelDomain;
                Auth.UserCredentials = userCredentials;
                OGHeader.Authentication = Auth;
                #endregion

                if (Request.UpdateProileRequest != null && Request.UpdateProileRequest.Addresses != null && Request.UpdateProileRequest.Addresses.Count > 0)
                {
                    foreach (Models.OWS.Address address in Request.UpdateProileRequest.Addresses)
                    {
                        NameService.UpdateAddressResponse AddressResponse = new NameService.UpdateAddressResponse();
                        NameService.InsertAddressResponse InsertAddressResponse = new NameService.InsertAddressResponse();
                        if (address.operaId != 0)
                        {
                            NameService.UpdateAddressRequest AddressRequest = new NameService.UpdateAddressRequest();
                            NameService.NameAddress NAddress = new NameService.NameAddress();
                            NAddress.primary = address.primary != null ? (bool)address.primary : false;
                            NAddress.primarySpecified = NAddress.primary;
                            NAddress.displaySequence = address.displaySequence != null ? (int)address.displaySequence : 1;
                            NAddress.addressType = address.addressType;
                            string[] AddressLines = new string[2];
                            AddressLines[0] = address.address1 != null ? address.address1 : "";
                            AddressLines[1] = address.address2 != null ? address.address2 : "";
                            NAddress.AddressLine = AddressLines;
                            NAddress.addressType = address.addressType;
                            NAddress.cityName = address.city;
                            NAddress.countryCode = address.country != null ? address.country : "US";
                            NAddress.displaySequenceSpecified = true;
                            NAddress.operaId = address.operaId;
                            NAddress.operaIdSpecified = true;
                            NAddress.postalCode = address.zip;
                            NAddress.stateProv = address.state;
                            AddressRequest.NameAddress = NAddress;
                            #region Response

                            NameService.NameServiceSoapClient NamePortClient = new NameService.NameServiceSoapClient();
                            bool isOperaCloudEnabled = false;
                            isOperaCloudEnabled = (ConfigurationManager.AppSettings["OperaCloudEnabled"] != null
                                            && !string.IsNullOrEmpty(ConfigurationManager.AppSettings["OperaCloudEnabled"].ToString())
                                            && bool.TryParse(ConfigurationManager.AppSettings["OperaCloudEnabled"].ToString(), out isOperaCloudEnabled)) ? isOperaCloudEnabled : false;
                            if (isOperaCloudEnabled)
                            {
                                NamePortClient.Endpoint.Behaviors.Add(new Helper.CustomEndpointBehaviour(ConfigurationManager.AppSettings["WSSEUserName"].ToString(),
                                                        ConfigurationManager.AppSettings["WSSEPassword"].ToString(),
                                                        Request.Username, Request.Password, Request.HotelDomain));
                            }
                            AddressResponse = NamePortClient.UpdateAddress(ref OGHeader, AddressRequest);
                            if (AddressResponse.Result.resultStatusFlag != NameService.ResultStatusFlag.SUCCESS)
                            {
                                return new Models.OWS.OwsResponseModel()
                                {
                                    result = false,
                                    responseMessage = AddressResponse.Result.Text != null ? AddressResponse.Result.Text[0].Value: "Error returned by opera"
                                };
                            }
                            #endregion
                        }
                        else
                        {
                            NameService.InsertAddressRequest AddressRequest = new NameService.InsertAddressRequest();
                            NameService.NameAddress NAddress = new NameService.NameAddress();
                            NAddress.primary = true;
                            NAddress.primarySpecified = true;
                            NAddress.displaySequence = address.displaySequence != null ? address.displaySequence.Value :0;
                            NAddress.displaySequenceSpecified = address.displaySequence != null ? true : false;
                            NAddress.addressType = address.addressType;
                            string[] AddressLines = new string[2];
                            AddressLines[0] = address.address1 != null ? address.address1 : "";
                            AddressLines[1] = address.address2 != null ? address.address2 : "";
                            NAddress.AddressLine = AddressLines;
                            NAddress.cityName = address.city;
                            NAddress.countryCode = address.country != null ? address.country : "US";
                            NAddress.displaySequenceSpecified = true;
                            NAddress.postalCode = address.zip;
                            NAddress.stateProv = address.state;
                            AddressRequest.NameAddress = NAddress;
                            NameService.UniqueID UId = new NameService.UniqueID();
                            UId.type = NameService.UniqueIDType.INTERNAL;
                            UId.Value = Request.UpdateProileRequest.ProfileID;
                            AddressRequest.NameID = UId;
                            #region Response
                            NameService.NameServiceSoapClient NamePortClient = new NameService.NameServiceSoapClient();
                            bool isOperaCloudEnabled = false;
                            isOperaCloudEnabled = (ConfigurationManager.AppSettings["OperaCloudEnabled"] != null
                                            && !string.IsNullOrEmpty(ConfigurationManager.AppSettings["OperaCloudEnabled"].ToString())
                                            && bool.TryParse(ConfigurationManager.AppSettings["OperaCloudEnabled"].ToString(), out isOperaCloudEnabled)) ? isOperaCloudEnabled : false;
                            if (isOperaCloudEnabled)
                            {
                                NamePortClient.Endpoint.Behaviors.Add(new Helper.CustomEndpointBehaviour(ConfigurationManager.AppSettings["WSSEUserName"].ToString(),
                                                        ConfigurationManager.AppSettings["WSSEPassword"].ToString(),
                                                        Request.Username, Request.Password, Request.HotelDomain));
                            }
                            InsertAddressResponse = NamePortClient.InsertAddress(ref OGHeader, AddressRequest);
                            if (InsertAddressResponse.Result.resultStatusFlag != NameService.ResultStatusFlag.SUCCESS)
                            {
                                //if (AddressResponse.Result.resultStatusFlag != NameService.ResultStatusFlag.SUCCESS)
                                {
                                    return new Models.OWS.OwsResponseModel()
                                    {
                                        result = false,
                                        responseMessage = InsertAddressResponse.Result.Text != null ? InsertAddressResponse.Result.Text[0].Value : "Error returned by opera"
                                    };
                                }
                            }
                            #endregion
                        }
                    }
                    return new Models.OWS.OwsResponseModel()
                    {
                        result = true,
                        responseMessage = "Success",
                        statusCode = 101
                    };
                }
                else
                {
                    return new Models.OWS.OwsResponseModel()
                    {
                        result = false,
                        responseMessage = "Empty Address passed",
                        statusCode = -1
                    };
                    
                }
            }
            catch (Exception ex)
            {                
                return new Models.OWS.OwsResponseModel()
                {
                    result = false,
                    responseMessage = "Generic Exception : " + ex.Message,
                    statusCode = -1
                };
            }
        }

        public Models.OWS.OwsResponseModel UpdateEmail(Models.OWS.OwsRequestModel Request)
        {
            try
            {
                new LogHelper().Debug("Update Email request : " + JsonConvert.SerializeObject(Request), Request.UpdateProileRequest.ProfileID, "UpdateEmail", "API", "OWS");
                #region Request Header
                string temp = Helper.Helper.Get8Digits();
                NameService.OGHeader OGHeader = new NameService.OGHeader();
                OGHeader.transactionID = temp;
                OGHeader.timeStamp = DateTime.Now;
                OGHeader.primaryLangID = Request.Language; //English
                NameService.EndPoint orginEndPOint = new NameService.EndPoint();
                orginEndPOint.entityID = Request.KioskID; //Kiosk Identifier
                orginEndPOint.systemType = Request.SystemType;
                OGHeader.Origin = orginEndPOint;
                NameService.EndPoint destEndPOint = new NameService.EndPoint();
                destEndPOint.entityID = Request.DestinationEntityID;
                destEndPOint.systemType = Request.DestinationSystemType;
                OGHeader.Destination = destEndPOint;
                NameService.OGHeaderAuthentication Auth = new NameService.OGHeaderAuthentication();
                NameService.OGHeaderAuthenticationUserCredentials userCredentials = new NameService.OGHeaderAuthenticationUserCredentials();
                userCredentials.UserName = Request.Username;
                userCredentials.UserPassword = Request.Password;
                userCredentials.Domain = Request.HotelDomain;
                Auth.UserCredentials = userCredentials;
                OGHeader.Authentication = Auth;
                #endregion

                if (Request.UpdateProileRequest != null && Request.UpdateProileRequest.Emails != null && Request.UpdateProileRequest.Emails.Count > 0)
                {
                    NameService.UpdateEmailResponse EMailResponse = new NameService.UpdateEmailResponse();
                    foreach (Models.OWS.Email email in Request.UpdateProileRequest.Emails)
                    {
                        if (email.operaId != 0)
                        {
                            NameService.UpdateEmailRequest EmailRequest = new NameService.UpdateEmailRequest();
                            NameService.NameEmail NEmail = new NameService.NameEmail();
                            NEmail.primary = email.primary != null ? (bool)email.primary : false;
                            NEmail.primarySpecified = NEmail.primary;
                            NEmail.displaySequence = email.displaySequence != null ? (int)email.displaySequence : 1;
                            //NEmail.emailType = Email.emailType;
                            NEmail.operaId = email.operaId;
                            NEmail.Value = email.email;
                            NEmail.operaIdSpecified = true;
                            EmailRequest.NameEmail = NEmail;

                            #region Response

                            NameService.NameServiceSoapClient NamePortClient = new NameService.NameServiceSoapClient();
                            bool isOperaCloudEnabled = false;
                            isOperaCloudEnabled = (ConfigurationManager.AppSettings["OperaCloudEnabled"] != null
                                            && !string.IsNullOrEmpty(ConfigurationManager.AppSettings["OperaCloudEnabled"].ToString())
                                            && bool.TryParse(ConfigurationManager.AppSettings["OperaCloudEnabled"].ToString(), out isOperaCloudEnabled)) ? isOperaCloudEnabled : false;
                            if (isOperaCloudEnabled)
                            {
                                NamePortClient.Endpoint.Behaviors.Add(new Helper.CustomEndpointBehaviour(ConfigurationManager.AppSettings["WSSEUserName"].ToString(),
                                                        ConfigurationManager.AppSettings["WSSEPassword"].ToString(),
                                                        Request.Username, Request.Password, Request.HotelDomain));
                            }
                            EMailResponse = NamePortClient.UpdateEmail(ref OGHeader, EmailRequest);
                            if (EMailResponse.Result.resultStatusFlag != NameService.ResultStatusFlag.SUCCESS)
                            {
                                return new Models.OWS.OwsResponseModel()
                                {
                                    result = false,
                                    responseMessage = EMailResponse.Result.Text != null ? EMailResponse.Result.Text[0].ToString(): null
                                };
                            }
                            #endregion
                        }
                        else
                        {
                            NameService.InsertEmailRequest EmailRequest = new NameService.InsertEmailRequest();
                            NameService.NameEmail NEmail = new NameService.NameEmail();
                            NEmail.primary = true;
                            NEmail.primarySpecified = true;
                            //NEmail.displaySequence = 1;
                            NEmail.emailType = email.emailType;
                            NEmail.Value = email.email;
                            NameService.UniqueID UId = new NameService.UniqueID();
                            UId.type = NameService.UniqueIDType.INTERNAL;
                            UId.Value = Request.UpdateProileRequest.ProfileID;
                            NEmail.displaySequence = email.displaySequence != null ? (int)email.displaySequence : 1;
                            NEmail.displaySequenceSpecified = email.displaySequence != null ? true: false;
                            EmailRequest.NameID = UId;
                            EmailRequest.NameEmail = NEmail;



                            #region Response
                            NameService.InsertEmailResponse InsertEmailResponse = new NameService.InsertEmailResponse();
                            NameService.NameServiceSoapClient NamePortClient = new NameService.NameServiceSoapClient();
                            bool isOperaCloudEnabled = false;
                            isOperaCloudEnabled = (ConfigurationManager.AppSettings["OperaCloudEnabled"] != null
                                            && !string.IsNullOrEmpty(ConfigurationManager.AppSettings["OperaCloudEnabled"].ToString())
                                            && bool.TryParse(ConfigurationManager.AppSettings["OperaCloudEnabled"].ToString(), out isOperaCloudEnabled)) ? isOperaCloudEnabled : false;
                            if (isOperaCloudEnabled)
                            {
                                NamePortClient.Endpoint.Behaviors.Add(new Helper.CustomEndpointBehaviour(ConfigurationManager.AppSettings["WSSEUserName"].ToString(),
                                                        ConfigurationManager.AppSettings["WSSEPassword"].ToString(),
                                                        Request.Username, Request.Password, Request.HotelDomain));
                            }
                            InsertEmailResponse = NamePortClient.InsertEmail(ref OGHeader, EmailRequest);
                            if (InsertEmailResponse.Result.resultStatusFlag != NameService.ResultStatusFlag.SUCCESS)
                            {
                                return new Models.OWS.OwsResponseModel()
                                {
                                    result = false,
                                    responseMessage = InsertEmailResponse.Result.Text != null ? InsertEmailResponse.Result.Text[0].Value : null
                                };
                            }
                            #endregion
                        }
                    }
                    return new Models.OWS.OwsResponseModel
                    {
                        responseMessage = "Success",
                        statusCode = 101,
                        result = true
                    };
                }
                else
                {
                    return new Models.OWS.OwsResponseModel
                    {
                        responseMessage = "Blank email address provided",
                        statusCode = 9002,
                        result = false
                    };
                }
            }
            catch (Exception ex)
            {
                return new Models.OWS.OwsResponseModel
                {
                    responseMessage = ex.Message,
                    statusCode = 1002,
                    result = false
                };
            }
        }

        public Models.OWS.OwsResponseModel UpdatePhone(Models.OWS.OwsRequestModel Request)
        {
            try
            {
                new LogHelper().Debug("Update Phone request : " + JsonConvert.SerializeObject(Request), Request.UpdateProileRequest.ProfileID, "UpdatePhone", "API", "OWS");
                #region Request Header
                string temp = Helper.Helper.Get8Digits();
                NameService.OGHeader OGHeader = new NameService.OGHeader();
                OGHeader.transactionID = temp;
                OGHeader.timeStamp = DateTime.Now;
                OGHeader.primaryLangID = Request.Language; //English
                NameService.EndPoint orginEndPOint = new NameService.EndPoint();
                orginEndPOint.entityID = Request.KioskID; //Kiosk Identifier
                orginEndPOint.systemType = Request.SystemType;
                OGHeader.Origin = orginEndPOint;
                NameService.EndPoint destEndPOint = new NameService.EndPoint();
                destEndPOint.entityID = Request.DestinationEntityID;
                destEndPOint.systemType = Request.DestinationSystemType;
                OGHeader.Destination = destEndPOint;
                NameService.OGHeaderAuthentication Auth = new NameService.OGHeaderAuthentication();
                NameService.OGHeaderAuthenticationUserCredentials userCredentials = new NameService.OGHeaderAuthenticationUserCredentials();
                userCredentials.UserName = Request.Username;
                userCredentials.UserPassword = Request.Password;
                userCredentials.Domain = Request.HotelDomain;
                Auth.UserCredentials = userCredentials;
                OGHeader.Authentication = Auth;
                #endregion

                if (Request.UpdateProileRequest != null && Request.UpdateProileRequest.Phones != null && Request.UpdateProileRequest.Phones.Count > 0)
                {
                    NameService.UpdatePhoneResponse PhoneResponse = new NameService.UpdatePhoneResponse();
                    NameService.InsertPhoneResponse InsertPhoneResponse = new NameService.InsertPhoneResponse();
                    foreach (Models.OWS.Phone phone in Request.UpdateProileRequest.Phones)
                    {
                        if (phone.operaId != 0)
                        {
                            NameService.UpdatePhoneRequest PhoneRequest = new NameService.UpdatePhoneRequest();
                            NameService.NamePhone NPhone = new NameService.NamePhone();
                            NPhone.primary = phone.primary != null ? (bool)phone.primary : false;
                            NPhone.primarySpecified = NPhone.primary;
                            NPhone.displaySequence = phone.displaySequence != null ? (int)phone.displaySequence : 1;
                            NPhone.displaySequenceSpecified = true;
                            NPhone.phoneType = phone.phoneType;
                            NPhone.phoneRole = phone.phoneRole;
                            NPhone.operaId = phone.operaId;
                            NPhone.operaIdSpecified = true;
                            NPhone.Item = phone.PhoneNumber;
                            PhoneRequest.NamePhone = NPhone;

                            #region Response

                            NameService.NameServiceSoapClient NamePortClient = new NameService.NameServiceSoapClient();
                            bool isOperaCloudEnabled = false;
                            isOperaCloudEnabled = (ConfigurationManager.AppSettings["OperaCloudEnabled"] != null
                                            && !string.IsNullOrEmpty(ConfigurationManager.AppSettings["OperaCloudEnabled"].ToString())
                                            && bool.TryParse(ConfigurationManager.AppSettings["OperaCloudEnabled"].ToString(), out isOperaCloudEnabled)) ? isOperaCloudEnabled : false;
                            if (isOperaCloudEnabled)
                            {
                                NamePortClient.Endpoint.Behaviors.Add(new Helper.CustomEndpointBehaviour(ConfigurationManager.AppSettings["WSSEUserName"].ToString(),
                                                        ConfigurationManager.AppSettings["WSSEPassword"].ToString(),
                                                        Request.Username, Request.Password, Request.HotelDomain));
                            }
                            PhoneResponse = NamePortClient.UpdatePhone(ref OGHeader, PhoneRequest);
                            if (PhoneResponse.Result.resultStatusFlag != NameService.ResultStatusFlag.SUCCESS)
                            {
                                return new Models.OWS.OwsResponseModel()
                                {
                                    result = false,
                                    responseMessage = PhoneResponse.Result.Text != null ? PhoneResponse.Result.Text[0].Value : null,
                                    statusCode = -1
                                };
                            }
                            #endregion
                        }
                        else
                        {
                            NameService.InsertPhoneRequest PhoneRequest = new NameService.InsertPhoneRequest();
                            NameService.NamePhone NPhone = new NameService.NamePhone();
                            NPhone.primary = true;
                            NPhone.primarySpecified = true;
                            NPhone.displaySequence = phone.displaySequence != null ? (int)phone.displaySequence.Value:0;
                            NPhone.displaySequenceSpecified = phone.displaySequence != null ? true : false;
                            NPhone.phoneType = phone.phoneType;
                            NPhone.phoneRole = phone.phoneRole;
                            NPhone.Item = phone.PhoneNumber;
                            PhoneRequest.NamePhone = NPhone;
                            NameService.UniqueID UId = new NameService.UniqueID();
                            UId.type = NameService.UniqueIDType.INTERNAL;
                            UId.Value = Request.UpdateProileRequest.ProfileID;
                            PhoneRequest.NameID = UId;
                            #region Response

                            NameService.NameServiceSoapClient NamePortClient = new NameService.NameServiceSoapClient();
                            bool isOperaCloudEnabled = false;
                            isOperaCloudEnabled = (ConfigurationManager.AppSettings["OperaCloudEnabled"] != null
                                            && !string.IsNullOrEmpty(ConfigurationManager.AppSettings["OperaCloudEnabled"].ToString())
                                            && bool.TryParse(ConfigurationManager.AppSettings["OperaCloudEnabled"].ToString(), out isOperaCloudEnabled)) ? isOperaCloudEnabled : false;
                            if (isOperaCloudEnabled)
                            {
                                NamePortClient.Endpoint.Behaviors.Add(new Helper.CustomEndpointBehaviour(ConfigurationManager.AppSettings["WSSEUserName"].ToString(),
                                                        ConfigurationManager.AppSettings["WSSEPassword"].ToString(),
                                                        Request.Username, Request.Password, Request.HotelDomain));
                            }
                            InsertPhoneResponse = NamePortClient.InsertPhone(ref OGHeader, PhoneRequest);
                            if (InsertPhoneResponse.Result.resultStatusFlag != NameService.ResultStatusFlag.SUCCESS)
                            {
                                return new Models.OWS.OwsResponseModel()
                                {
                                    result = false,
                                    responseMessage = InsertPhoneResponse.Result.Text != null ? InsertPhoneResponse.Result.Text[0].Value : null,
                                    statusCode = -1
                                };
                            }
                            #endregion
                        }
                        
                    }
                    return new Models.OWS.OwsResponseModel()
                    {
                        result = true,
                        responseMessage = "Success"
                    };
                }
                else
                {
                    
                    return new Models.OWS.OwsResponseModel()
                    {
                        result = false,
                        responseMessage = "Null phone value passed",
                        statusCode = 8002
                    };
                }
            }
            catch (Exception ex)
            {
                return new Models.OWS.OwsResponseModel()
                {
                    result = false,
                    responseMessage = ex.Message,
                    statusCode = 8002
                };
            }
        }

    }
}