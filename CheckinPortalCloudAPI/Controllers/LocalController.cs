﻿using CheckinPortalCloudAPI.Helper.Local;
using CheckinPortalCloudAPI.KeyEncoderService;
using CheckinPortalCloudAPI.Models.Cloud.DB;
using CheckinPortalCloudAPI.Models.Local;
using CheckinPortalCloudAPI.Helper;
using Fare;
using Microsoft.Ajax.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Xml;

using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web;
using System.Web.Http;

namespace CheckinPortalCloudAPI.Controllers
{
    public class LocalController : ApiController
    {
       


        [HttpPost]
        [ActionName("GetConnectedTerminalList")]
        public async Task<Models.AdyenPayment.AdyenEcomResponse> GetConnectedTerminalList(Models.AdyenPayment.PaymentRequest paymentRequest)
        {
            try
            {
                new LogHelper().Log("Processing Get connected terminal list request started", paymentRequest.RequestIdentifier, "GetConnectedTerminalList", "API", "Payment");
                new LogHelper().Debug("Raw Get connected terminal list request : " + JsonConvert.SerializeObject(paymentRequest), paymentRequest.RequestIdentifier, "GetConnectedTerminalList", "API", "Payment");

                HttpClient httpClient = null;
                if (!string.IsNullOrEmpty(ConfigurationManager.AppSettings["IsPaymentProxyEnabled"]) && (Convert.ToBoolean(ConfigurationManager.AppSettings["IsPaymentProxyEnabled"])))
                {
                    httpClient = new Helper.Helper().getProxyClient("Payment", ConfigurationManager.AppSettings["PaymentProxyURL"], ConfigurationManager.AppSettings["PaymentProxyUN"],
                        ConfigurationManager.AppSettings["PaymentProxyPSWD"]);
                }
                else
                    httpClient = new HttpClient();

                httpClient.DefaultRequestHeaders.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                httpClient.DefaultRequestHeaders.Add("x-api-key", paymentRequest.ApiKey);

                Models.AdyenPayment.GetDeviceListRequest getDeviceListRequest = new Models.AdyenPayment.GetDeviceListRequest()
                {
                    merchantAccount = paymentRequest.merchantAccount
                };
                new LogHelper().Debug("Adyen Get connected terminal list request : " + JsonConvert.SerializeObject(getDeviceListRequest), paymentRequest.RequestIdentifier, "GetConnectedTerminalList", "API", "Payment");
                HttpContent requestContent = new StringContent(JsonConvert.SerializeObject(getDeviceListRequest), Encoding.UTF8, "application/json");
                HttpResponseMessage response = await httpClient.PostAsync(ConfigurationManager.AppSettings["AdyenDeviceListURL"], requestContent);
                Models.AdyenPayment.GetDeviceListResponse getDeviceListResponse = new Models.AdyenPayment.GetDeviceListResponse();
                if (response != null)
                {
                    if (response.IsSuccessStatusCode)
                    {
                        var responsestr = await response.Content.ReadAsStringAsync();
                        getDeviceListResponse = JsonConvert.DeserializeObject<Models.AdyenPayment.GetDeviceListResponse>(await response.Content.ReadAsStringAsync());
                        new LogHelper().Debug("Adyen Get connected terminal list response : " + JsonConvert.SerializeObject(getDeviceListResponse), paymentRequest.RequestIdentifier, "GetConnectedTerminalList", "API", "Payment");
                        if (getDeviceListResponse != null && getDeviceListResponse.uniqueTerminalIds != null && getDeviceListResponse.uniqueTerminalIds.Count > 0)
                        {
                            new LogHelper().Log("Get connected terminal list request completed successfully", paymentRequest.RequestIdentifier, "GetConnectedTerminalList", "API", "Payment");
                            new LogHelper().Debug("Get connected terminal list response : " + JsonConvert.SerializeObject(new Models.AdyenPayment.AdyenEcomResponse()
                            {
                                ResponseObject = getDeviceListResponse.uniqueTerminalIds,
                                Result = true
                            }), paymentRequest.RequestIdentifier, "GetConnectedTerminalList", "API", "Payment");
                            return new Models.AdyenPayment.AdyenEcomResponse()
                            {
                                ResponseObject = getDeviceListResponse.uniqueTerminalIds,
                                Result = true
                            };
                        }
                        else
                        {
                            new LogHelper().Log("Get connected terminal list request failled with the reason :- " + getDeviceListResponse.Message, paymentRequest.RequestIdentifier, "GetConnectedTerminalList", "API", "Payment");
                            new LogHelper().Debug("Get connected terminal list response : " + JsonConvert.SerializeObject(new Models.AdyenPayment.AdyenEcomResponse()
                            {
                                ResponseMessage = getDeviceListResponse.Message,
                                Result = false
                            }), paymentRequest.RequestIdentifier, "GetConnectedTerminalList", "API", "Payment");
                            return new Models.AdyenPayment.AdyenEcomResponse()
                            {
                                ResponseMessage = getDeviceListResponse.Message,
                                Result = false
                            };
                        }
                    }
                    else
                    {
                        new LogHelper().Log("Get connected terminal list request failled with the reason :- " + response.ReasonPhrase, paymentRequest.RequestIdentifier, "GetConnectedTerminalList", "API", "Payment");
                        new LogHelper().Debug("Get connected terminal list response : " + JsonConvert.SerializeObject(new Models.AdyenPayment.AdyenEcomResponse()
                        {
                            Result = false,
                            ResponseMessage = response.ReasonPhrase
                        }), paymentRequest.RequestIdentifier, "GetConnectedTerminalList", "API", "Payment");
                        return new Models.AdyenPayment.AdyenEcomResponse()
                        {

                            Result = false,
                            ResponseMessage = response.ReasonPhrase
                        };
                    }

                }
                else
                {
                    new LogHelper().Log("Get connected terminal list request failled with the reason :- Payment gateway returned blank", paymentRequest.RequestIdentifier, "GetConnectedTerminalList", "API", "Payment");
                    new LogHelper().Debug("Get connected terminal list response : " + JsonConvert.SerializeObject(new Models.AdyenPayment.AdyenEcomResponse()
                    {
                        Result = false,
                        ResponseMessage = "Payment gateway returned blank"
                    }), paymentRequest.RequestIdentifier, "GetConnectedTerminalList", "API", "Payment");
                    return new Models.AdyenPayment.AdyenEcomResponse()
                    {
                        Result = false,
                        ResponseMessage = "Payment gateway returned blank"
                    };
                }
            }
            catch (Exception ex)
            {
                new LogHelper().Log("Get connected terminal list request failled with the reason :- " + ex.ToString(), paymentRequest.RequestIdentifier, "GetConnectedTerminalList", "API", "Payment");
                return new Models.AdyenPayment.AdyenEcomResponse()
                {

                    Result = false,
                    ResponseMessage = "Generic Exception : " + ex.Message
                };
            }
        }


        [HttpPost]
        [ActionName("PushReservationDetails")]
        public async Task<Models.Local.LocalResponseModel> PushReservationDetails(Models.Local.LocalRequestModel localDataRequest)
        {
            try
            {
                //System.IO.File.AppendAllText(System.Web.HttpContext.Current.Request.MapPath("~\\Resources\\LocalPush.txt"), localDataRequest.RequestObject.ToString());
                List<OperaReservation> reservations = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Models.Local.OperaReservation>>(localDataRequest.RequestObject.ToString());
                //HttpResponseMessage response = await httpClient.PostAsync($"/v52/payments", requestContent);
                List<Models.Local.DB.OperaReservationDataTableModel> operaReservationDataTables = new List<Models.Local.DB.OperaReservationDataTableModel>();
                List<Models.Local.DB.ProfileDetailsDataTableModel> profileDetailsDataTables = new List<Models.Local.DB.ProfileDetailsDataTableModel>();

                foreach (Models.Local.OperaReservation operaReservation in reservations)
                {
                    Helper.Local.LocalDBModelConverter dBModelConverter = new Helper.Local.LocalDBModelConverter();
                    operaReservationDataTables.Add(dBModelConverter.getOperaReservationDataTable(operaReservation));
                    if ((operaReservation.GuestProfiles != null && operaReservation.GuestProfiles.Count >= 0))
                    {
                        foreach (Models.Local.GuestProfile guestProfile in operaReservation.GuestProfiles)
                        {
                            profileDetailsDataTables.Add(dBModelConverter.getprofileDetailsDataTable(guestProfile, operaReservation.ReservationNameID));
                        }
                    }
                }
                if (Helper.Local.DBHelper.Instance.InsertReservationDetails(operaReservationDataTables, profileDetailsDataTables, new List<Models.Local.DB.ProfileDocumentDetailsModel>(),localDataRequest.SyncFromCloud , ConfigurationManager.AppSettings["LocalConnectionString"]))
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

        [HttpPost]
        [ActionName("PushKioskReservationReportDetails")]
        public async Task<Models.Local.LocalResponseModel> PushKisokReservationReportDetails(Models.Local.LocalRequestModel localDataRequest)
        {
            try
            {
                List<Models.KIOSK.ReservationModel> reservations = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Models.KIOSK.ReservationModel>>(localDataRequest.RequestObject.ToString());
                List<Models.KIOSK.DB.ReservationDataTableModel> operaReservationDataTables = new List<Models.KIOSK.DB.ReservationDataTableModel>();
                List<Models.KIOSK.DB.ProfileDataTableModel> profileDetailsDataTables = new List<Models.KIOSK.DB.ProfileDataTableModel>();

                foreach (Models.KIOSK.ReservationModel operaReservation in reservations)
                {
                    Helper.KIOSK.KIOSKDBModelConverter dBModelConverter = new Helper.KIOSK.KIOSKDBModelConverter();
                    operaReservationDataTables.Add(dBModelConverter.getOperaReservationDataTable(operaReservation));
                    if ((operaReservation.GuestProfiles != null && operaReservation.GuestProfiles.Count >= 0))
                    {
                        foreach (Models.KIOSK.GuestProfile guestProfile in operaReservation.GuestProfiles)
                        {
                            profileDetailsDataTables.Add(dBModelConverter.getprofileDetailsDataTable(guestProfile, operaReservation.ReservationNameID));
                        }
                    }
                }

                if (Helper.KIOSK.DBHelper.Instance.InsertReservationDetails(operaReservationDataTables, profileDetailsDataTables, ConfigurationManager.AppSettings["LocalConnectionString"]))
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

        [HttpPost]
        [ActionName("UpdateReservationDetails")]
        public async Task<Models.Local.LocalResponseModel> UpdateReservationDetails(Models.Local.LocalRequestModel localDataRequest)
        {
            try
            {
                List<Models.Local.DB.ReservationListTypeModel> ReservationListTypeModel = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Models.Local.DB.ReservationListTypeModel>>(localDataRequest.RequestObject.ToString());
                
                if (Helper.Local.DBHelper.Instance.BulkUpdateReservationToDB(ReservationListTypeModel, ConfigurationManager.AppSettings["LocalConnectionString"]))
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

        [HttpPost]
        [ActionName("FetchReservationDetailStatus")]
        public async Task<Models.Local.LocalResponseModel> FetchReservationDetailStatus(Models.Local.LocalRequestModel localDataRequest)
        {
            try
            {
                List<Models.Local.DB.ReservationListTypeModel> ReservationListTypeModel = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Models.Local.DB.ReservationListTypeModel>>(localDataRequest.RequestObject.ToString());

                List<Models.Local.DB.ReservationListTypeModel> resultSet = Helper.Local.DBHelper.Instance.BulkFetchReservationStatus(ReservationListTypeModel, ConfigurationManager.AppSettings["LocalConnectionString"]);
                if(resultSet != null)
                {
                    return new Models.Local.LocalResponseModel()
                    {
                        responseData = resultSet,
                        result = true,
                        responseMessage = "Success",
                        statusCode = 101
                    };
                }
                else
                    return new Models.Local.LocalResponseModel()
                    {
                        result = false,
                        responseMessage = "Failled to Fetch the data",
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

        [HttpPost]
        [ActionName("FetchKioskReceipt")]
        public async Task<Models.Local.LocalResponseModel> FetchKioskReceipt(Models.Local.LocalRequestModel localRequest)
        {
            if(localRequest == null || localRequest.RequestObject == null)
            {
                return new LocalResponseModel()
                {
                    result = false,
                    responseMessage = "Request can not be empty"
                };
            }
            Models.Local.KioskReceiptRequest receiptRequest = JsonConvert.DeserializeObject<Models.Local.KioskReceiptRequest>(localRequest.RequestObject.ToString());
            return new ServiceLib.Local.LocalServiceLib().GenerateKioskReceipt(receiptRequest);
        }

        [HttpPost]
        [ActionName("FetchKioskReceiptForPrint")]
        public async Task<Models.Local.LocalResponseModel> FetchKioskReceiptForPrint(Models.Local.LocalRequestModel localRequest)
        {
            if (localRequest == null || localRequest.RequestObject == null)
            {
                return new LocalResponseModel()
                {
                    result = false,
                    responseMessage = "Request can not be empty"
                };
            }
            Models.Local.KioskReceiptRequest receiptRequest = JsonConvert.DeserializeObject<Models.Local.KioskReceiptRequest>(localRequest.RequestObject.ToString());
            return new ServiceLib.Local.LocalServiceLib().GenerateKioskReceiptForPrint(receiptRequest);
        }

        [HttpPost]
        [ActionName("FetchReservationDetailsByRefNumber")]
        public async Task<Models.Local.LocalResponseModel> FetchReservationDetailsByRefNumber(Models.Local.LocalRequestModel localDataRequest)
        {
            try
            {
                Models.KIOSK.ReservationRequestModel reservationRequests = Newtonsoft.Json.JsonConvert.DeserializeObject<Models.KIOSK.ReservationRequestModel>(localDataRequest.RequestObject.ToString());

                List<Models.KIOSK.DB.ReservationDataTableModel> resultSet = Helper.KIOSK.DBHelper.Instance.FetchReservationDetails(reservationRequests.ReferenceNumber,reservationRequests.ArrivalDate, ConfigurationManager.AppSettings["LocalConnectionString"]);
                if (resultSet != null)
                {
                    return new Models.Local.LocalResponseModel()
                    {
                        responseData = resultSet,
                        result = true,
                        responseMessage = "Success",
                        statusCode = 101
                    };
                }
                else
                    return new Models.Local.LocalResponseModel()
                    {
                        result = false,
                        responseMessage = "Failled to Fetch the data",
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

        [HttpPost]
        [ActionName("FetchPrecheckedinReservationDetailsByRefNumber")]
        public async Task<Models.Local.LocalResponseModel> FetchPrecheckedinReservationDetailsByRefNumber(Models.Local.LocalRequestModel localDataRequest)
        {
            try
            {
                Models.KIOSK.ReservationRequestModel reservationRequests = Newtonsoft.Json.JsonConvert.DeserializeObject<Models.KIOSK.ReservationRequestModel>(localDataRequest.RequestObject.ToString());

                List<Models.KIOSK.DB.ReservationDataTableModel> resultSet = Helper.KIOSK.DBHelper.Instance.FetchReservationDetails(reservationRequests.ReferenceNumber, reservationRequests.ArrivalDate, ConfigurationManager.AppSettings["LocalConnectionString"]);
                if (resultSet != null)
                {
                    return new Models.Local.LocalResponseModel()
                    {
                        responseData = resultSet,
                        result = true,
                        responseMessage = "Success",
                        statusCode = 101
                    };
                }
                else
                    return new Models.Local.LocalResponseModel()
                    {
                        result = false,
                        responseMessage = "Failled to Fetch the data",
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

        [HttpPost]
        [ActionName("FetchProfileDetailsByReservationNameID")]
        public async Task<Models.Local.LocalResponseModel> FetchProfileDetailsByReservationNameID(Models.Local.LocalRequestModel localDataRequest)
        {
            try
            {
                Models.KIOSK.ReservationRequestModel reservationRequests = Newtonsoft.Json.JsonConvert.DeserializeObject<Models.KIOSK.ReservationRequestModel>(localDataRequest.RequestObject.ToString());

                List<Models.KIOSK.DB.ReservationDataTableModel> resultSet = Helper.KIOSK.DBHelper.Instance.FetchReservationDetails(reservationRequests.ReferenceNumber, reservationRequests.ArrivalDate, ConfigurationManager.AppSettings["LocalConnectionString"]);
                if (resultSet != null)
                {
                    return new Models.Local.LocalResponseModel()
                    {
                        responseData = resultSet,
                        result = true,
                        responseMessage = "Success",
                        statusCode = 101
                    };
                }
                else
                    return new Models.Local.LocalResponseModel()
                    {
                        result = false,
                        responseMessage = "Failled to Fetch the data",
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

        //[HttpPost]
        //[ActionName("UpdateGeneralNotification")]
        //public async Task<Models.Local.LocalResponseModel> UpdateGeneralNotification(Models.Local.LocalRequestModel localDataRequest)
        //{
        //    try
        //    {
        //        Models.KIOSK.NotificationRequestModel notificationRequests = Newtonsoft.Json.JsonConvert.DeserializeObject<Models.KIOSK.NotificationRequestModel>(localDataRequest.RequestObject.ToString());

        //        var resultSet = Helper.KIOSK.DBHelper.Instance.InsertNotificationDetails(notificationRequests.ReservationNameID,
        //                                                                    notificationRequests.NotificationType,
        //                                                                    notificationRequests.UserID,
        //                                                                    ConfigurationManager.AppSettings["LocalConnectionString"]);
        //        if (resultSet)
        //        {
        //            return new Models.Local.LocalResponseModel()
        //            {
        //                result = true,
        //                responseMessage = "Success",
        //                statusCode = 101
        //            };
        //        }
        //        else
        //            return new Models.Local.LocalResponseModel()
        //            {
        //                result = false,
        //                responseMessage = "Failled to insert notification",
        //                statusCode = -1
        //            };
        //    }
        //    catch (Exception ex)
        //    {
        //        return new Models.Local.LocalResponseModel()
        //        {
        //            result = false,
        //            responseMessage = ex.Message,
        //            statusCode = -1
        //        };
        //    }

        //}

        [HttpPost]
        [ActionName("InsertNotifications")]
        public async Task<Models.Local.LocalResponseModel> InsertNotifications(Models.Local.LocalRequestModel localDataRequest)
        {
            try
            {
                Models.KIOSK.NotificationRequestModel notificationRequests = Newtonsoft.Json.JsonConvert.DeserializeObject<Models.KIOSK.NotificationRequestModel>(localDataRequest.RequestObject.ToString());

                var resultSet = Helper.KIOSK.DBHelper.Instance.InsertNotificationDetails(notificationRequests.ReservationNameID,
                                                                            notificationRequests.NotificationType,
                                                                            notificationRequests.UserName,
                                                                            notificationRequests.isActionTaken,
                                                                            notificationRequests.NotificationMessage,
                                                                            notificationRequests.NotificationID,
                                                                            notificationRequests.DeviceIdentifier,
                                                                            ConfigurationManager.AppSettings["LocalConnectionString"]);
                if (resultSet)
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
                        responseMessage = "Failled to insert notification",
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

        [HttpPost]
        [ActionName("FetchDocumentTypeMaster")]
        public async Task<Models.Local.LocalResponseModel> FetchDocumentTypeMaster()
        {
            try
            {
                List<Models.KIOSK.DB.DocumentTypeMasterModel> resultSet = Helper.KIOSK.DBHelper.Instance.FetchDocumentMasters(ConfigurationManager.AppSettings["LocalConnectionString"]);
                if (resultSet != null)
                {
                    return new Models.Local.LocalResponseModel()
                    {
                        responseData = resultSet,
                        result = true,
                        responseMessage = "Success",
                        statusCode = 101
                    };
                }
                else
                    return new Models.Local.LocalResponseModel()
                    {
                        result = false,
                        responseMessage = "Failled to Fetch the document master",
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

        [HttpPost]
        [ActionName("FetchGeneralSettings")]
        public async Task<Models.Local.LocalResponseModel> FetchGeneralSettings(Models.Local.LocalRequestModel localDataRequest)
        {
            try
            {
                var resultSet = Helper.KIOSK.DBHelper.Instance.FetchSettingsDetails(ConfigurationManager.AppSettings["LocalConnectionString"]);
                if (resultSet != null)
                {
                    return new Models.Local.LocalResponseModel()
                    {
                        responseData = resultSet,
                        result = true,
                        responseMessage = "Success",
                        statusCode = 101
                    };
                }
                else
                    return new Models.Local.LocalResponseModel()
                    {
                        result = false,
                        responseMessage = "Failled to Fetch settings",
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

        [HttpPost]
        [ActionName("FetchPackageMaster")]
        public async Task<Models.Local.LocalResponseModel> FetchPackageMaster(Models.Local.LocalRequestModel localDataRequest)
        {
            try
            {

                var resultSet = Helper.KIOSK.DBHelper.Instance.FetchPacakgeMaster(ConfigurationManager.AppSettings["LocalConnectionString"]);
                if (resultSet != null)
                {
                    return new Models.Local.LocalResponseModel()
                    {
                        responseData = resultSet,
                        result = true,
                        responseMessage = "Success",
                        statusCode = 101
                    };
                }
                else
                    return new Models.Local.LocalResponseModel()
                    {
                        result = false,
                        responseMessage = "Failled to Fetch packageMaster",
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

        [HttpPost]
        [ActionName("FetchUserDetailsByQrCode")]
        public async Task<Models.Local.LocalResponseModel> FetchUserDetailsByQrCode(Models.Local.LocalRequestModel localDataRequest)
        {
            try
            {
                var resultSet = Helper.KIOSK.DBHelper.Instance.FetchUserByQrCode(ConfigurationManager.AppSettings["LocalConnectionString"], localDataRequest.RequestObject.ToString());
                if (resultSet != null)
                {
                    if (resultSet.Count > 0
                        && !string.IsNullOrEmpty(resultSet.First().Result)
                        && resultSet.First().Result.Equals("200"))
                    {
                        return new Models.Local.LocalResponseModel()
                        {
                            responseData = resultSet,
                            result = true,
                            responseMessage = "Success",
                            statusCode = 101
                        };
                    }
                    else if(resultSet.Count > 0
                        && !string.IsNullOrEmpty(resultSet.First().Message))
                    {
                        return new Models.Local.LocalResponseModel()
                        {
                           
                            result = false,
                            responseMessage = resultSet.First().Message,
                            statusCode = 102
                        };
                    }
                    else
                    {
                        return new Models.Local.LocalResponseModel()
                        {
                            
                            result = false,
                            responseMessage = "Failled to retreave user detals",
                            statusCode = 103
                        };
                    }
                }
                else
                    return new Models.Local.LocalResponseModel()
                    {
                        result = false,
                        responseMessage = "Failled to Fetch user details by qrcode",
                        statusCode = -2
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


        [HttpPost]
        [ActionName("FetchReservationTrackDetailStatus")]
        public async Task<Models.Local.LocalResponseModel> FetchReservationTrackDetailStatus(Models.Local.LocalRequestModel localDataRequest)
        {
            try
            {
                Models.Local.DB.ReservationTrackStatus reservationTrackStatus = Newtonsoft.Json.JsonConvert.DeserializeObject<Models.Local.DB.ReservationTrackStatus>(localDataRequest.RequestObject.ToString());

                List<Models.Local.DB.ReservationTrackStatus> resultSet = Helper.Local.DBHelper.Instance.FetchReservationTrackStatus(reservationTrackStatus, ConfigurationManager.AppSettings["LocalConnectionString"]);
                if (resultSet != null)
                {
                    return new Models.Local.LocalResponseModel()
                    {
                        responseData = resultSet,
                        result = true,
                        responseMessage = "Success",
                        statusCode = 101
                    };
                }
                else
                    return new Models.Local.LocalResponseModel()
                    {
                        result = false,
                        responseMessage = "Failled to Fetch the data",
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

        [HttpPost]
        [ActionName("PushReservationTrackDetailStatus")]
        public async Task<Models.Local.LocalResponseModel> PushReservationTrackDetailStatus(Models.Local.LocalRequestModel localDataRequest)
        {
            try
            {
                Models.Local.DB.ReservationTrackStatus reservationTrackStatus = Newtonsoft.Json.JsonConvert.DeserializeObject<Models.Local.DB.ReservationTrackStatus>(localDataRequest.RequestObject.ToString());

                bool resultSet = Helper.Local.DBHelper.Instance.PushReservationTrackStatus(reservationTrackStatus, ConfigurationManager.AppSettings["LocalConnectionString"]);
                if (resultSet)
                {
                    return new Models.Local.LocalResponseModel()
                    {
                        responseData = null,
                        result = true,
                        responseMessage = "Success",
                        statusCode = 101
                    };
                }
                else
                    return new Models.Local.LocalResponseModel()
                    {
                        result = false,
                        responseMessage = "Failled to Insert the data",
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

        [HttpPost]
        [ActionName("ClearLocalData")]
        public async Task<Models.Local.LocalResponseModel> ClearLocalData()
        {
            try
            {

                List<Models.Local.DB.DataClearResponseDataTableModel> dataClearResponses = Helper.Local.DBHelper.Instance.ClearData(ConfigurationManager.AppSettings["LocalConnectionString"]);
                if (dataClearResponses != null && dataClearResponses[0].ResultMessage.Equals("Success"))
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
                        responseMessage = "Failled to get the data",
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

        [HttpPost]
        [ActionName("PushDocumentDetails")]
        public async Task<Models.Local.LocalResponseModel> PushDocumentDetails(Models.Local.LocalRequestModel localRequest)
        {
            try
            {
                //HttpResponseMessage response = await httpClient.PostAsync($"/v52/payments", requestContent);
                List<Models.Local.DB.ProfileDocumentDetailsModel> profileDocumentList = new List<Models.Local.DB.ProfileDocumentDetailsModel>();
                List<Models.Local.DB.ProfileDocuments> ProfileDocuments = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Models.Local.DB.ProfileDocuments>>(localRequest.RequestObject.ToString());
                foreach (Models.Local.DB.ProfileDocuments profileDocument in ProfileDocuments)
                {
                    Helper.Local.LocalDBModelConverter dBModelConverter = new Helper.Local.LocalDBModelConverter();
                    profileDocumentList.Add(dBModelConverter.getProfileDocumentDetailsDataTable(profileDocument));

                }
                if (Helper.Local.DBHelper.Instance.InsertReservationDetails(new List<Models.Local.DB.OperaReservationDataTableModel>() , new List<Models.Local.DB.ProfileDetailsDataTableModel>(), profileDocumentList, localRequest.SyncFromCloud != null ? localRequest.SyncFromCloud.Value : false, ConfigurationManager.AppSettings["LocalConnectionString"]))
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

        [HttpPost]
        [ActionName("PushReservationAdditionalDetails")]
        public async Task<Models.Local.LocalResponseModel> PushReservationAdditionalDetails(Models.Local.LocalRequestModel localRequest)
        {
            try
            {
                
               
                List<Models.Local.DB.ReservationAdditionalDetails> additionalDetails = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Models.Local.DB.ReservationAdditionalDetails>>(localRequest.RequestObject.ToString());
                //foreach (Models.Local.DB.ReservationAdditionalDetails additionalDetail in additionalDetails)
                //{
                //    Helper.Local.LocalDBModelConverter dBModelConverter = new Helper.Local.LocalDBModelConverter();
                //    profileDocumentList.Add(dBModelConverter.getProfileDocumentDetailsDataTable(profileDocument));

                //}
                if (Helper.Local.DBHelper.Instance.InsertReservationAdditionalDetails(additionalDetails, ConfigurationManager.AppSettings["LocalConnectionString"]))
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

        [HttpPost]
        [ActionName("PushPaymentNotifications")]
        public async Task<Models.Local.LocalResponseModel> PushPaymentNotifications(Models.Local.LocalRequestModel localRequest)
        {
            try
            {
                List<Models.Local.DB.PaymentNotification> paymentNotifications = JsonConvert.DeserializeObject<List<Models.Local.DB.PaymentNotification>>(localRequest.RequestObject.ToString());
                if (paymentNotifications != null && paymentNotifications.Count > 0)
                {
                    if (Helper.Local.DBHelper.Instance.InsertPaymentNotifications(paymentNotifications, ConfigurationManager.AppSettings["SaavyConnectionString"]))
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
                else
                {
                    return new Models.Local.LocalResponseModel()
                    {
                        result = false,
                        responseMessage = "Bad request",
                        statusCode = -1
                    };
                }
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

        [HttpPost]
        [ActionName("FetchBIData")]
        public async Task<Models.Local.LocalResponseModel> FetchBIData(Models.Local.LocalRequestModel localRequest)
        {
            try
            {
                
                List<Models.Local.DB.BISummaryArrivals> bISummaryArrivals =  Helper.Local.DBHelper.Instance.FetchBISummaryArrivals(ConfigurationManager.AppSettings["LocalConnectionString"]);
                List<Models.Local.DB.BINationalityWiseSummaryArrivals> bINationalityWiseSummaryArrivals = Helper.Local.DBHelper.Instance.FetchBINationalityWiseSummaryArrivals(ConfigurationManager.AppSettings["LocalConnectionString"]);
                
                return new Models.Local.LocalResponseModel()
                {
                    result = true,
                    responseMessage = "Success",
                    statusCode = 101,
                    responseData = new Models.Local.DB.BIData()
                    {
                        BINationalityWiseSummaryArrivals = bINationalityWiseSummaryArrivals,
                        BISummaryArrivals = bISummaryArrivals
                    }
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

        [HttpPost]
        [ActionName("PushPaymentDetails")]
        public async Task<Models.Local.LocalResponseModel> PushPaymentDetails(Models.Local.LocalRequestModel localRequest)
        {
            try
            {
                //HttpResponseMessage response = await httpClient.PostAsync($"/v52/payments", requestContent);
                Models.Local.DB.PaymentDetails paymentDetails = JsonConvert.DeserializeObject<Models.Local.DB.PaymentDetails>(localRequest.RequestObject.ToString());
                if (paymentDetails != null)
                {
                    if (Helper.Local.DBHelper.Instance.InsertPaymentDetails(paymentDetails.paymentHistories, paymentDetails.paymentHeaders, paymentDetails.paymentAdditionalInfos,  ConfigurationManager.AppSettings["SaavyConnectionString"]))
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
                else
                {
                    return new Models.Local.LocalResponseModel()
                    {
                        result = false,
                        responseMessage = "Request object can not be null",
                        statusCode = -1
                    };
                }
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

        [HttpPost]
        [ActionName("InsertAuditDetails")]
        public async Task<Models.Local.LocalResponseModel> InsertAuditDetails(Models.Local.LocalRequestModel localRequest)
        {
            try
            {
                Models.KIOSK.AuditRequestModel auditRequest = JsonConvert.DeserializeObject<Models.KIOSK.AuditRequestModel>(localRequest.RequestObject.ToString());
                if (auditRequest != null)
                {
                    if (Helper.KIOSK.AuditHelper.InsertAuditLog(auditRequest.PageName,auditRequest.UserName,auditRequest.AuditMessage,auditRequest.GroupIdentifier, auditRequest.GeneralIdentifier,auditRequest.DeviceIdentifier, auditRequest.jsonObjects,ConfigurationManager.AppSettings["LocalConnectionString"]))
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
                            responseMessage = "Failled to insert audit details",
                            statusCode = -1
                        };
                }
                else
                {
                    return new Models.Local.LocalResponseModel()
                    {
                        result = false,
                        responseMessage = "Request object can not be null",
                        statusCode = -1
                    };
                }
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

        [HttpPost]
        [ActionName("FetchPaymentDetails")]
        public async Task<Models.Local.LocalResponseModel> FetchPaymentDetails(Models.Local.LocalRequestModel localRequest)
        {
            try
            {
                Models.Local.FetchPaymentRequest fetchPaymentRequest = JsonConvert.DeserializeObject<Models.Local.FetchPaymentRequest>(localRequest.RequestObject.ToString());
                if (fetchPaymentRequest != null)
                {
                    List<Models.Local.DB.PaymentHeader> paymentHeader = Helper.Local.DBHelper.Instance.FetchPaymentDetails(fetchPaymentRequest.ReservationNameID,fetchPaymentRequest.isActive, ConfigurationManager.AppSettings["SaavyConnectionString"]);
                    if(paymentHeader != null )
                    {
                        return new Models.Local.LocalResponseModel()
                        {
                            result = true,
                            responseMessage = "Success",
                            statusCode = 101,
                            responseData = paymentHeader

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
                else
                {
                    return new Models.Local.LocalResponseModel()
                    {
                        result = false,
                        responseMessage = "Request object can not be null",
                        statusCode = -1
                    };
                }
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

        [HttpPost]
        [ActionName("FetchPaymentTransactionDetails")]
        public async Task<Models.Local.LocalResponseModel> FetchPaymentTransactionDetails(Models.Local.LocalRequestModel localRequest)
        {
            try
            {
                Models.Local.FetchPaymentRequest fetchPaymentRequest = JsonConvert.DeserializeObject<Models.Local.FetchPaymentRequest>(localRequest.RequestObject.ToString());
                if (fetchPaymentRequest != null)
                {
                    List<Models.Local.DB.PaymentTransactionDetails> paymentHeader = Helper.Local.DBHelper.Instance.FetchPaymentTransactionDetails(fetchPaymentRequest.ReservationNameID, fetchPaymentRequest.isActive, ConfigurationManager.AppSettings["SaavyConnectionString"]);
                    if (paymentHeader != null)
                    {
                        return new Models.Local.LocalResponseModel()
                        {
                            result = true,
                            responseMessage = "Success",
                            statusCode = 101,
                            responseData = paymentHeader

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
                else
                {
                    return new Models.Local.LocalResponseModel()
                    {
                        result = false,
                        responseMessage = "Request object can not be null",
                        statusCode = -1
                    };
                }
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

        [HttpPost]
        [ActionName("PushReservationDocumentDetails")]
        public async Task<Models.Local.LocalResponseModel> PushReservationDocumentDetails(Models.Local.LocalRequestModel localRequest)
        {
            try
            {
                List<Models.Local.DB.ReservationDocumentsDataTableModel> reservationDocuments = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Models.Local.DB.ReservationDocumentsDataTableModel>>(localRequest.RequestObject.ToString());
                
                if (Helper.Local.DBHelper.Instance.InsertReservationDocuments(reservationDocuments,  localRequest.SyncFromCloud != null ? localRequest.SyncFromCloud.Value : false , ConfigurationManager.AppSettings["LocalConnectionString"]))
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

        [HttpPost]
        [ActionName("PushUpsellPackages")]
        public async Task<Models.Local.LocalResponseModel> PushUpsellPackages(Models.Local.LocalRequestModel localRequest)
        {
            try
            {
                //HttpResponseMessage response = await httpClient.PostAsync($"/v52/payments", requestContent);
                List<Models.Local.DB.UpsellPackageModel> upsellPackages = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Models.Local.DB.UpsellPackageModel>>(localRequest.RequestObject.ToString());
                
                if (Helper.Local.DBHelper.Instance.InsertUpsellPackages(upsellPackages, ConfigurationManager.AppSettings["LocalConnectionString"]))
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

        [HttpPost]
        [ActionName("PushReservationPolicy")]
        public async Task<Models.Local.LocalResponseModel> PushReservationPolicy(Models.Local.LocalRequestModel localRequest)
        {
            try
            {
                //HttpResponseMessage response = await httpClient.PostAsync($"/v52/payments", requestContent);
                List<Models.Local.DB.ReservationPolicyModel> reservationPolicies = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Models.Local.DB.ReservationPolicyModel>>(localRequest.RequestObject.ToString());

                if (Helper.Local.DBHelper.Instance.InsertReservationPolicies(reservationPolicies, ConfigurationManager.AppSettings["LocalConnectionString"]))
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

        [HttpPost]
        [ActionName("PushCountryMaster")]
        public async Task<Models.Local.LocalResponseModel> PushCountryMaster(Models.Local.LocalRequestModel localRequest)
        {
            try
            {
                List<Models.Local.DB.CountryState> countryStates = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Models.Local.DB.CountryState>>(localRequest.RequestObject.ToString());

                if (Helper.Local.DBHelper.Instance.InsertCountrList(countryStates, ConfigurationManager.AppSettings["LocalConnectionString"]))
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

        [HttpPost]
        [ActionName("PushFeedback")]
        public async Task<Models.Local.LocalResponseModel> PushFeedback(Models.Local.LocalRequestModel localRequest)
        {
            try
            {
                List<Models.Local.DB.FeedBackModel> feedBacks = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Models.Local.DB.FeedBackModel>>(localRequest.RequestObject.ToString());

                if (Helper.Local.DBHelper.Instance.InsertFeedback(feedBacks, ConfigurationManager.AppSettings["LocalConnectionString"]))
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

        [HttpPost]
        [ActionName("FetchNonModefiedReservations")]
        public async Task<Models.Local.LocalResponseModel> FetchNonModefiedReservations(Models.Local.LocalRequestModel localRequest)
        {
            try
            {
                int bufferDays = localRequest.RequestObject != null ? Int32.Parse(localRequest.RequestObject.ToString()) : 0;
                List<string> reservationList = Helper.Local.DBHelper.Instance.FetchNonModefiedReservationList(ConfigurationManager.AppSettings["LocalConnectionString"],bufferDays);

                return new LocalResponseModel()
                {
                    responseData = reservationList,
                    result = true
                };
            }
            catch(Exception ex)
            {
                return new LocalResponseModel()
                {
                    result = false,
                    responseMessage = ex.Message
                };
            }
        }



        [HttpPost]
        [ActionName("EncodeKey")]
        public async Task<Models.Local.LocalResponseModel> EncodeKey(Models.Local.LocalRequestModel localRequest)
        {
            try
            {
                Models.Local.KeyEncodeRequestModel Request = JsonConvert.DeserializeObject<Models.Local.KeyEncodeRequestModel>(localRequest.RequestObject.ToString());

                #region FIAS Protocol
                if (Request.IsNewKey)
                {
                    Xeger xeger = new Xeger("[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}");

                    AuthHeader Auth = new AuthHeader();

                    Auth.From = "KABA";

                    string CtimeGUID_str = xeger.Generate();
                    KeyEncoderService.CTimestamp Ctime = new KeyEncoderService.CTimestamp();
                    Guid CtimeGUID = new Guid(CtimeGUID_str);
                    Ctime.Created = DateTime.Now;
                    Ctime.Expires = DateTime.Now.AddDays(1);
                    Ctime.Id = CtimeGUID;

                    KeyEncoderService.CUserNameToken CUser = new KeyEncoderService.CUserNameToken();
                    string CUserGUID_str = xeger.Generate();
                    Guid CUserGUID = new Guid(CUserGUID_str);//(CUserGUID_str)"f541e3a5-7852-4acc-9fa9-b67f0b294d88";
                    CUser.Id = CUserGUID;
                    CUser.Password = ConfigurationManager.AppSettings["KeyEncoderAPIPassword"];//"DummyPwd";//Pass
                    CUser.Username = ConfigurationManager.AppSettings["KeyEncoderAPIUsername"];//"DummyUser";//User
                    CUser.Created = DateTime.Now;

                    KeyEncoderService.SecurityHeader SecurityHeader = new KeyEncoderService.SecurityHeader();
                    SecurityHeader.Timestamp = Ctime;
                    SecurityHeader.UsernameToken = CUser;

                    string CAuthGUID_str = xeger.Generate();
                    Guid CAuthGUID = new Guid(CAuthGUID_str);//(CAuthGUID_str);"731171c1-6d22-4735-a4f2-f8d6004e0be0"
                    Auth.MessageID = CAuthGUID;
                    Auth.Security = SecurityHeader;

                    KeyEncoderService.CreateNewBookingRequest MakeKeyReq = new KeyEncoderService.CreateNewBookingRequest();
                    KeyEncoderService.CreateNewBookingResponse MakeKeyRes = new KeyEncoderService.CreateNewBookingResponse();

                    MakeKeyReq.CheckIn = DateTime.Now;
                    MakeKeyReq.CheckOut = DateTime.ParseExact(Request.CheckoutDate, "MMddyy", CultureInfo.InvariantCulture).AddDays(1);
                    MakeKeyReq.EncoderID = Request.EncoderID;
                    MakeKeyReq.GuestName = Request.GuestName != null ? Request.GuestName : "";
                    MakeKeyReq.KeyCount = 1;
                    MakeKeyReq.UID = "AAAAAAA";
                    MakeKeyReq.KeySize = 1;
                    MakeKeyReq.TrackIIFolioNo = Request.ReservationNo;
                    MakeKeyReq.MainRoomNo = Request.RoomNo;
                    MakeKeyReq.PMSTerminalID = Request.EncoderID;
                    MakeKeyReq.ReservationID = "2" + Request.ReservationNo;
                    MakeKeyReq.SiteName = ConfigurationManager.AppSettings["HotelDomain"];//"SMST";//STS
                    MakeKeyReq.bGrantAccessPredefinedSuiteDoors = false;
                    KeyEncoderService.CCommonAreas[] CommonAreasArray = new CCommonAreas[20];
                    CCommonAreas CArea;
                    int i = 1;
                    for (int x = 0; x < 20; x++)
                    {
                        CArea = new CCommonAreas();
                        CArea.PassLevelNo = Convert.ToUInt32(i);
                        CArea.eMode = eCommonAreaSelMode.DefaultConfiguredAccess;
                        CommonAreasArray[x] = CArea;
                        i++;
                    }
                    MakeKeyReq.CommonAreaList = CommonAreasArray;
                    MessengerPMSWSServiceSoapClient SoapClnt = new MessengerPMSWSServiceSoapClient();
                    System.Xml.XmlNode XN = SoapClnt.CreateNewBooking(ref Auth, MakeKeyReq.ReservationID, MakeKeyReq.SiteName, MakeKeyReq.PMSTerminalID, MakeKeyReq.EncoderID, MakeKeyReq.CheckIn, MakeKeyReq.CheckOut, MakeKeyReq.GuestName, MakeKeyReq.MainRoomNo, MakeKeyReq.bGrantAccessPredefinedSuiteDoors, null, MakeKeyReq.CommonAreaList, MakeKeyReq.TrackIIFolioNo, null, MakeKeyReq.KeyCount, MakeKeyReq.KeySize, MakeKeyReq.UID);

                    if (XN != null)
                    {
                        bool IsFoult = false;
                        Models.Local.LocalResponseModel localResponse = new LocalResponseModel();
                        foreach (XmlElement ResultElement in XN.ChildNodes)
                        {
                            if (!IsFoult)
                            {
                                if (ResultElement.Name.Equals("bSuccess"))
                                {
                                    return new Models.Local.LocalResponseModel()
                                    {
                                        result = true,
                                        responseMessage = "Success"
                                    };

                                }
                                else if (ResultElement.Name.Equals("faultcode"))
                                {
                                    IsFoult = true;
                                    localResponse.responseMessage += ResultElement.InnerText;
                                }
                            }
                            else
                            {
                                if (ResultElement.Name.Equals("faultcode"))
                                {
                                    localResponse.result = false;
                                }
                                else if (ResultElement.Name.Equals("detailInfo"))
                                {
                                    localResponse.responseMessage += ResultElement.InnerText;
                                }
                            }
                        }

                        return localResponse;
                    }
                    else
                    {
                        return new LocalResponseModel()
                        {
                            result = false,
                            responseMessage = "Encoder server returned NULL"
                        };
                    }
                }
                else
                {
                    #region Create Duplicate Key

                    KeyEncoderService.ChangeKeyAccessRequest ChangeKeyAccessReq = new KeyEncoderService.ChangeKeyAccessRequest();
                    KeyEncoderService.ChangeKeyAccessResponse ChangeKeyAccessRes = new KeyEncoderService.ChangeKeyAccessResponse();

                    MessengerPMSWSServiceSoapClient SoapClnt = new MessengerPMSWSServiceSoapClient();
                    Xeger xeger = new Xeger("[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}");
                    AuthHeader Auth = new AuthHeader();

                    Auth.From = "KABA";

                    string CtimeGUID_str = xeger.Generate();
                    KeyEncoderService.CTimestamp Ctime = new KeyEncoderService.CTimestamp();
                    Guid CtimeGUID = new Guid(CtimeGUID_str);//CtimeGUID_str);"fc177220-81be-4333-a7b8-f0cb31b84a0e"
                    Ctime.Created = DateTime.Now;
                    Ctime.Expires = DateTime.Now.AddDays(1);
                    Ctime.Id = CtimeGUID;

                    KeyEncoderService.CUserNameToken CUser = new KeyEncoderService.CUserNameToken();
                    string CUserGUID_str = xeger.Generate();
                    Guid CUserGUID = new Guid(CUserGUID_str);//(CUserGUID_str);"f541e3a5-7852-4acc-9fa9-b67f0b294d88"
                    CUser.Id = CUserGUID;
                    CUser.Password = ConfigurationManager.AppSettings["KeyEncoderAPIPassword"];//"Pass";
                    CUser.Username = ConfigurationManager.AppSettings["KeyEncoderAPIUsername"];//"User";
                    CUser.Created = DateTime.Now;

                    KeyEncoderService.SecurityHeader SecurityHeader = new KeyEncoderService.SecurityHeader();
                    SecurityHeader.Timestamp = Ctime;
                    SecurityHeader.UsernameToken = CUser;

                    string CAuthGUID_str = xeger.Generate();
                    Guid CAuthGUID = new Guid(CAuthGUID_str);//(CAuthGUID_str);"731171c1-6d22-4735-a4f2-f8d6004e0be0"
                    Auth.MessageID = CAuthGUID;
                    Auth.Security = SecurityHeader;

                    ChangeKeyAccessReq.ReservationID = "2" + Request.ReservationNo;
                    ChangeKeyAccessReq.SiteName = ConfigurationManager.AppSettings["HotelDomain"];//"STS";
                    ChangeKeyAccessReq.PMSTerminalID = Request.EncoderID;
                    ChangeKeyAccessReq.EncoderID = Request.EncoderID;
                    ChangeKeyAccessReq.CheckIn = DateTime.Now;
                    ChangeKeyAccessReq.CheckOut = DateTime.ParseExact(Request.CheckoutDate, "MMddyy", CultureInfo.InvariantCulture).AddDays(1);
                    ChangeKeyAccessReq.GuestName = Request.GuestName != null ? Request.GuestName : "";
                    KeyEncoderService.CCommonAreas[] CommonAreasArray = new CCommonAreas[20];
                    CCommonAreas CArea;
                    int i = 1;
                    for (int x = 0; x < 20; x++)
                    {
                        CArea = new CCommonAreas();
                        CArea.PassLevelNo = Convert.ToUInt32(i);
                        CArea.eMode = eCommonAreaSelMode.DefaultConfiguredAccess;
                        CommonAreasArray[x] = CArea;
                        i++;
                    }
                    ChangeKeyAccessReq.CommonAreaList = CommonAreasArray;
                    ChangeKeyAccessReq.TrackIIFolioNo = Request.ReservationNo;
                    ChangeKeyAccessReq.KeyCount = 1;
                    ChangeKeyAccessReq.KeySize = 1;
                    ChangeKeyAccessReq.UID = "AAAAAAAA";


                    XmlNode XN = SoapClnt.ChangeKeyAccess(ref Auth, ChangeKeyAccessReq.ReservationID, ChangeKeyAccessReq.SiteName, ChangeKeyAccessReq.PMSTerminalID, ChangeKeyAccessReq.EncoderID,
                                                            ChangeKeyAccessReq.CheckIn, ChangeKeyAccessReq.CheckOut, ChangeKeyAccessReq.GuestName, null, ChangeKeyAccessReq.CommonAreaList,
                                                            ChangeKeyAccessReq.TrackIIFolioNo, ChangeKeyAccessReq.TrackIIFolioNo, ChangeKeyAccessReq.KeyCount, ChangeKeyAccessReq.KeySize, ChangeKeyAccessReq.UID);
                    if (XN != null)
                    {
                        bool IsFoult = false;
                        Models.Local.LocalResponseModel localResponse = new LocalResponseModel();
                        foreach (XmlElement ResultElement in XN.ChildNodes)
                        {
                            if (!IsFoult)
                            {
                                if (ResultElement.Name.Equals("bSuccess"))
                                {
                                    return new Models.Local.LocalResponseModel()
                                    {
                                        result = true,
                                        responseMessage = "Success"
                                    };

                                }
                                else if (ResultElement.Name.Equals("faultcode"))
                                {
                                    IsFoult = true;
                                    localResponse.responseMessage += ResultElement.InnerText;
                                }
                            }
                            else
                            {
                                if (ResultElement.Name.Equals("faultcode"))
                                {
                                    localResponse.result = false;
                                }
                                else if (ResultElement.Name.Equals("detailInfo"))
                                {
                                    localResponse.responseMessage += ResultElement.InnerText;
                                }
                            }
                        }

                        return localResponse;
                    }
                    else
                    {
                        return new LocalResponseModel()
                        {
                            result = false,
                            responseMessage = "Encoder server returned NULL"
                        };
                    }

                    #endregion
                }
                #endregion
            }
            catch (Exception ex)
            {
                return new LocalResponseModel()
                {
                    result = false,
                    responseMessage = ex.Message
                };
            }
        }


        [HttpPost]
        [ActionName("FetchPreauthUpdate")]
        public async Task<Models.Local.LocalResponseModel> FetchPreauthUpdate()
        {
            try
            {

                List<Models.Local.PaymentHeaders> paymentHeader = Helper.Local.DBHelper.Instance.FetchPaymentDetailsByExpiry(ConfigurationManager.AppSettings["SaavyConnectionString"]);
                if (paymentHeader != null)
                {
                    return new Models.Local.LocalResponseModel()
                    {
                        result = true,
                        responseMessage = "Failled to read the data",
                        statusCode = 1,
                        responseData = paymentHeader
                    };

                }
                else
                    return new Models.Local.LocalResponseModel()
                    {
                        result = false,
                        responseMessage = "Failled to read the data",
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

        [HttpPost]
        [ActionName("UpdatePaymentDetails")]
        public async Task<Models.Local.LocalResponseModel> UpdatePaymentDetails(Models.Local.LocalRequestModel localDataRequest)
        {
            try
            {
                Models.Local.PaymentHeader ReservationListTypeModel = Newtonsoft.Json.JsonConvert.DeserializeObject<Models.Local.PaymentHeader>(localDataRequest.RequestObject.ToString());

                if (Helper.Local.DBHelper.Instance.UpdatePaymentHeaderData(ReservationListTypeModel, ConfigurationManager.AppSettings["SaavyConnectionString"]))
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


        [HttpPost]
        [ActionName("PushPaymentData")]
        public async Task<Models.Local.LocalResponseModel> PushPaymentData(Models.Local.LocalRequestModel localRequest)
        {
            try
            {

                Models.Local.PaymnetTopUP paymentDetails = JsonConvert.DeserializeObject<Models.Local.PaymnetTopUP>(localRequest.RequestObject.ToString());
                if (paymentDetails != null)
                {
                    if (Helper.Local.DBHelper.Instance.InsertPaymentData(paymentDetails, ConfigurationManager.AppSettings["SaavyConnectionString"]))
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
                else
                {
                    return new Models.Local.LocalResponseModel()
                    {
                        result = false,
                        responseMessage = "Request object can not be null",
                        statusCode = -1
                    };
                }
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

        [HttpPost]
        [ActionName("UpdatePhActiveFalse")]
        public async Task<Models.Local.LocalResponseModel> UpdatePhActiveFalse()
        {
            try
            {
                if (Helper.Local.DBHelper.Instance.UpdatePaymentHeaderIsActive(ConfigurationManager.AppSettings["SaavyConnectionString"]))
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
                        responseMessage = "Failled to update the data",
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
        /// <summary>
        /// Added for kiosk on 5-2-2022 
        /// </summary>
        /// <param name="localDataRequest"></param>
        /// <returns></returns>
        [HttpPost]
        [ActionName("FetchProfileDocumentByReservationNumber")]
        public async Task<Models.Local.LocalResponseModel> FetchProfileDocumentByReservationNumber
            (Models.Local.LocalRequestModel localDataRequest)
        {
            try
          {
                Models.KIOSK.ProfileDocumentRequestModel reservationRequests = Newtonsoft.Json.JsonConvert.DeserializeObject<Models.KIOSK.ProfileDocumentRequestModel>(localDataRequest.RequestObject.ToString());

                List<Models.KIOSK.DB.ProfileDocumentDataTableModel> resultSet = Helper.KIOSK.DBHelper.Instance.FetchProfileDocumentByReservationNumber(reservationRequests.ReferenceNumber, ConfigurationManager.AppSettings["LocalConnectionString"]);
                if (resultSet != null)
                {
                    return new Models.Local.LocalResponseModel()
                    {
                        responseData = resultSet,
                        result = true,
                        responseMessage = "Success",
                        statusCode = 101
                    };
                }
                else
                    return new Models.Local.LocalResponseModel()
                    {
                        result = false,
                        responseMessage = "Failled to Fetch the data",
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

        /// <summary>
        /// Added for kiosk on 5-2-2022 
        /// </summary>
        /// <param name="localDataRequest"></param>
        /// <returns></returns>
        [HttpPost]
        [ActionName("InsertUpdateKioskManualAuthorizeDetails")]
        public async Task<Models.Local.LocalResponseModel> InsertUpdateKioskManualAuthorizeDetails(Models.Local.LocalRequestModel localDataRequest)
        {
            try
            {
                Models.KIOSK.KiokManualAuthorizationModel kioskmanualRequests = Newtonsoft.Json.JsonConvert.DeserializeObject<Models.KIOSK.KiokManualAuthorizationModel>(localDataRequest.RequestObject.ToString());

                var resultSet = Helper.KIOSK.DBHelper.Instance.InsertKioskManualAuthorizeDetails(kioskmanualRequests, ConfigurationManager.AppSettings["LocalConnectionString"]);
                if (resultSet)
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
                        responseMessage = "Failled to insert KiosManualAuthorize",
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

    }
}