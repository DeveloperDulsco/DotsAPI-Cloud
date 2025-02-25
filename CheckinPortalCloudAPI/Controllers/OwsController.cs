﻿using CheckinPortalCloudAPI.Helper;
using CheckinPortalCloudAPI.ServiceLib;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace CheckinPortalCloudAPI.Controllers
{
    public class OwsController : ApiController
    {
        [HttpPost]
        [ActionName("FetchReservationSummaryList")]
        public async Task<Models.OWS.OwsResponseModel> FetchReservationSummaryList(Models.OWS.OwsRequestModel owsRequest)
        {

            return new ServiceLib.OWS.OperaServiceLib().GetReservationSummaryList(owsRequest);
        }

        [HttpPost]
        [ActionName("FetchGuestMessages")]
        public Models.OWS.OwsResponseModel FetchGuestMessages(Models.OWS.OwsRequestModel Request)
        {
            return new ServiceLib.OWS.OperaServiceLib().GetMessages(Request);
        }

        [HttpPost]
        [ActionName("FetchGuestComments")]
        public Models.OWS.OwsResponseModel FetchGuestComments(Models.OWS.OwsRequestModel Request)
        {
            return new ServiceLib.OWS.OperaServiceLib().GetComments(Request);
        }

        [HttpPost]
        [ActionName("FetchReservationAlerts")]
        public Models.OWS.OwsResponseModel FetchReservationAlerts(Models.OWS.OwsRequestModel Request)
        {
            return new ServiceLib.OWS.OperaServiceLib().GetAlerts(Request);
        }



        [HttpPost]
        [ActionName("AddToReservationQueue")]
        public Models.OWS.OwsResponseModel AddToReservationQueue(Models.OWS.OwsRequestModel Request)
        {
            return new ServiceLib.OWS.OperaServiceLib().AddToReservationQueue(Request);
        }

        [HttpPost]
        [ActionName("ModifyPackage")]
        public async Task<Models.OWS.OwsResponseModel> ModifyPackage(Models.OWS.OwsRequestModel owsRequest)
        {
            return new ServiceLib.OWS.OperaServiceLib().ModifyPackage(owsRequest);
        }

        [HttpPost]
        [ActionName("FetchPackages")]
        public async Task<Models.OWS.OwsResponseModel> FetchPackages(Models.OWS.OwsRequestModel owsRequest)
        {
            return new ServiceLib.OWS.OperaServiceLib().ModifyPackage(owsRequest);
        }

        [HttpPost]
        [ActionName("GetFolioFromPMSAsFile")]
        public async Task<Models.OWS.OwsResponseModel> GetFolioFromPMSAsFile(Models.OWS.OwsRequestModel owsRequest)
        {
            return new ServiceLib.OWS.OperaServiceLib().getFolioPrint(owsRequest);
        }

        [HttpPost]
        [ActionName("ModifyBooking")]
        public async Task<Models.OWS.OwsResponseModel> ModifyBooking(Models.OWS.OwsRequestModel owsRequest)
        {
            if (owsRequest.modifyBookingRequest?.updateCreditCardDetails ?? false)
            {
                return new ServiceLib.OWS.OperaServiceLib().UpdateMethodOfPayment(owsRequest);
            }
            return new ServiceLib.OWS.OperaServiceLib().ModifyReservation(owsRequest);
        }

        [HttpPost]
        [ActionName("GetCountryList")]
        public async Task<Models.OWS.OwsResponseModel> GetCountryList(Models.OWS.OwsRequestModel owsRequest)
        {
            return new ServiceLib.OWS.OperaServiceLib().GetCountryCodes(owsRequest);
        }

        [HttpPost]
        [ActionName("GetNationalityList")]
        public async Task<Models.OWS.OwsResponseModel> GetNationalityList(Models.OWS.OwsRequestModel owsRequest)
        {
            return new ServiceLib.OWS.OperaServiceLib().GetNationalityCodes(owsRequest);
        }

        [HttpPost]
        [ActionName("GetDocumentTypeList")]
        public async Task<Models.OWS.OwsResponseModel> GetDocumentTypeList(Models.OWS.OwsRequestModel owsRequest)
        {
            return new ServiceLib.OWS.OperaServiceLib().GetDocumentTypes(owsRequest);
        }
        [HttpPost]
        [ActionName("GetPhoneTypeList")]
        public async Task<Models.OWS.OwsResponseModel> GetPhoneTypeList(Models.OWS.OwsRequestModel owsRequest)
        {
            return new ServiceLib.OWS.OperaServiceLib().GetPhoneTypes(owsRequest);
        }


        [HttpPost]
        [ActionName("GetStateCodesByCountryCode")]
        public async Task<Models.OWS.OwsResponseModel> GetStateCodesByCountryCode(Models.OWS.OwsRequestModel owsRequest)
        {
            return new ServiceLib.OWS.OperaServiceLib().GetStateCodesByCountryCode(owsRequest);
        }

        [HttpPost]
        [ActionName("MakePayment")]
        public async Task<Models.OWS.OwsResponseModel> MakePayment(Models.OWS.OwsRequestModel owsRequest)
        {
            return new ServiceLib.OWS.OperaServiceLib().MakePayment(owsRequest);
        }


        [HttpPost]
        [ActionName("AddPayment")]
        public async Task<Models.OWS.OwsResponseModel> AddPayment(Models.OWS.OwsRequestModel owsRequest)
        {
            var AddPayresponse = new ServiceLib.OWS.OperaServiceLib().AddPayment(owsRequest);
            try
            {


                if (AddPayresponse != null)
                {
                    if (AddPayresponse.result)
                    {
                        if (AddPayresponse.responseData != null)
                        {
                            var paymentresponse = (Models.OWS.OPIPaymentResponseModel)AddPayresponse.responseData;
                            if (paymentresponse != null)
                            {
                                bool resultSet = Helper.Local.DBHelper.Instance.PushOPeraPaymentDetails(paymentresponse, owsRequest.MakePaymentRequest.ReservationNameID, ConfigurationManager.AppSettings["LocalConnectionString"]);
                                if (resultSet)
                                {
                                    new LogHelper().Debug("Operaadd payment details Inserted Successfully ", owsRequest.MakePaymentRequest.ReservationNameID, "PushOPeraPaymentDetails", "API", "AddPayment");
                                }
                                else
                                {
                                    new LogHelper().Debug("Failed to Insert operaadd payment details ", owsRequest.MakePaymentRequest.ReservationNameID, "PushOPeraPaymentDetails", "API", "AddPayment");
                                }
                            }
                        }
                    }
                }
                return AddPayresponse;
            }
            catch (Exception ex)
            {
                new LogHelper().Error(ex, null, "PushOPeraPaymentDetails", "API", "AddPayment");
                return AddPayresponse;
            }


        }


        [HttpPost]
        [ActionName("GuestCheckIn")]
        public Models.OWS.OwsResponseModel GuestCheckIn(Models.OWS.OwsRequestModel Request)
        {
            return new ServiceLib.OWS.OperaServiceLib().GuestCheckIn(Request);
        }



        [HttpPost]
        [ActionName("GetRegCardAsBase64")]
        public Models.OWS.OwsResponseModel GetRegCardAsBase64(Models.OWS.OwsRequestModel Request)
        {
            return new ServiceLib.OWS.OperaServiceLib().GetRegcarBase64(Request.OperaReservation);
        }

        [HttpPost]
        [ActionName("GetGuestFolioAsBase64")]
        public Models.OWS.OwsResponseModel GetGuestFolioAsBase64(Models.OWS.OwsRequestModel Request)
        {
            return new ServiceLib.OWS.OperaServiceLib().FetchFolio(Request);
        }

        [HttpPost]
        [ActionName("FetchRoomStatus")]
        public Models.OWS.OwsResponseModel FetchRoomStatus(Models.OWS.OwsRequestModel Request)
        {
            return new ServiceLib.OWS.OperaServiceLib().FetchRoomStatus(Request);
        }


        [HttpPost]
        [ActionName("GetGuestFolioByWindow")]
        public Models.OWS.OwsResponseModel GetGuestFolioByWindow(Models.OWS.OwsRequestModel Request)
        {
            return new ServiceLib.OWS.OperaServiceLib().getFolioAsAList(Request);
        }

        [HttpPost]
        [ActionName("UpdateName")]
        public Models.OWS.OwsResponseModel UpdateName(Models.OWS.OwsRequestModel Request)
        {
            return new ServiceLib.OWS.OperaServiceLib().UpdateName(Request);
        }

        [HttpPost]
        [ActionName("CreateAccompanyingGuset")]
        public Models.OWS.OwsResponseModel CreateAccompanyingGuset(Models.OWS.OwsRequestModel Request)
        {
            return new ServiceLib.OWS.OperaServiceLib().createAccompanyingGuset(Request);
        }

        [HttpPost]
        [ActionName("UpdatePassport")]
        public Models.OWS.OwsResponseModel UpdatePassport(Models.OWS.OwsRequestModel Request)
        {
            return new ServiceLib.OWS.OperaServiceLib().UpdatePassport(Request);
        }

        [HttpPost]
        [ActionName("GuestCheckOut")]
        public Models.OWS.OwsResponseModel GuestCheckOut(Models.OWS.OwsRequestModel Request)
        {
            return new ServiceLib.OWS.OperaServiceLib().GuestCheckOut(Request);
        }

        [HttpPost]
        [ActionName("GetBusinessDate")]
        public Models.OWS.OwsResponseModel GetBusinessDate(Models.OWS.OwsRequestModel Request)
        {
            return new ServiceLib.OWS.OperaServiceLib().GetBusinessDate(Request);
        }



        [HttpPost]
        [ActionName("AssignRoom")]
        public Models.OWS.OwsResponseModel AssignRoom(Models.OWS.OwsRequestModel Request)
        {
            return new ServiceLib.OWS.OperaServiceLib().AssignRoomToReservation(Request);
        }

        [HttpPost]
        [ActionName("FetchRoomList")]
        public Models.OWS.OwsResponseModel FetchRoomList(Models.OWS.OwsRequestModel Request)
        {

            return new ServiceLib.OWS.OperaServiceLib().FetchRoomList(Request);
        }

        [HttpPost]
        [ActionName("FetchReservation")]
        public async Task<Models.OWS.OwsResponseModel> FetchReservation(Models.OWS.OwsRequestModel owsRequest)
        {
            return new ServiceLib.OWS.OperaServiceLib().GetReservationDetailsFromPMS(owsRequest);
        }

        [HttpPost]
        [ActionName("UpdateAddresList")]
        public async Task<Models.OWS.OwsResponseModel> UpdateAddresList(Models.OWS.OwsRequestModel owsRequest)
        {
            return new ServiceLib.OWS.OperaServiceLib().UpdateAddress(owsRequest);
        }

        [HttpPost]
        [ActionName("UpdateEmailList")]
        public async Task<Models.OWS.OwsResponseModel> UpdateEmailList(Models.OWS.OwsRequestModel owsRequest)
        {
            return new ServiceLib.OWS.OperaServiceLib().UpdateEmail(owsRequest);
        }

        [HttpPost]
        [ActionName("UpdatePhoneList")]
        public async Task<Models.OWS.OwsResponseModel> UpdatePhoneList(Models.OWS.OwsRequestModel owsRequest)
        {
            return new ServiceLib.OWS.OperaServiceLib().UpdatePhone(owsRequest);
        }

        [HttpPost]
        [ActionName("FetchPhoneList")]
        public async Task<Models.OWS.OwsResponseModel> FetchPhoneList(Models.OWS.OwsRequestModel owsRequest)
        {
            return new ServiceLib.OWS.OperaServiceLib().GetPhonelistForProfile(owsRequest);
        }

        [HttpPost]
        [ActionName("FetchEmailList")]
        public async Task<Models.OWS.OwsResponseModel> FetchEmailList(Models.OWS.OwsRequestModel owsRequest)
        {
            return new ServiceLib.OWS.OperaServiceLib().GetEmaillistForProfile(owsRequest);
        }

        [HttpPost]
        [ActionName("FetchAddressList")]
        public async Task<Models.OWS.OwsResponseModel> FetchAddressList(Models.OWS.OwsRequestModel owsRequest)
        {
            return new ServiceLib.OWS.OperaServiceLib().GetAddressListForProfile(owsRequest);
        }


        [HttpPost]
        [ActionName("PreregisterReservation")]
        public async Task<Models.OWS.OwsResponseModel> PreregisterReservation(Models.OWS.OwsRequestModel owsRequest)
        {
            return new ServiceLib.OWS.OperaServiceLib().PreRegisterGuest(owsRequest);
        }

        [HttpPost]
        [ActionName("UpdateProfileAddress")]
        public async Task<Models.OWS.OwsResponseModel> UpdateProfileAddress(Models.OWS.OwsRequestModel owsRequest)
        {
            return new ServiceLib.OWS.OperaServiceLib().GetReservationDetailsFromPMS(owsRequest);
        }

        [HttpPost]
        [ActionName("EncodeKey")]
        public async Task<Models.OWS.OwsResponseModel> EncodeKey(Models.OWS.OwsRequestModel owsRequest)
        {
            return new ServiceLib.OWS.OperaServiceLib().EncodeKey(owsRequest);
        }
        [HttpPost]
        [ActionName("CreditLimit")]
        public async Task<Models.OWS.OwsResponseModel> CreditLimit(Models.OWS.OwsRequestModel owsRequest)
        {
            try
            {
                var result = new ServiceLib.OWS.OperaServiceLib().GetCreditlimitRecord(owsRequest);
                if (result != null && result.responseData != null)
                {
                    return result;
                }


            }
            catch (Exception ex)
            {
                return new Models.OWS.OwsResponseModel
                {
                    responseData = "",
                    result = false
                };
            }
            return new Models.OWS.OwsResponseModel
            {
                responseData = "",
                result = false
            };
        }
    }
}
