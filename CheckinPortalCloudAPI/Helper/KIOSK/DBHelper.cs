using CheckinPortalCloudAPI.Helper.Local;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace CheckinPortalCloudAPI.Helper.KIOSK
{
    public class DBHelper
    {
        private static readonly Lazy<DBHelper>
           lazy = new Lazy<DBHelper>(() =>
           new DBHelper()
           );

        public static DBHelper Instance
        {
            get
            {
                return lazy.Value;
            }
        }

        public DataTable ToDataTable<T>(IList<T> data)
        {
            PropertyDescriptorCollection properties =
                TypeDescriptor.GetProperties(typeof(T));
            DataTable table = new DataTable();
            foreach (PropertyDescriptor prop in properties)
                table.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
            foreach (T item in data)
            {
                DataRow row = table.NewRow();
                foreach (PropertyDescriptor prop in properties)
                    row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;
                table.Rows.Add(row);
            }
            return table;
        }

        

        public bool InsertReservationDetails(List<Models.KIOSK.DB.ReservationDataTableModel> reservationDetailsTypeModels, List<Models.KIOSK.DB.ProfileDataTableModel> profileDetailsTypeModels, string ConnectionString)
        {
            try
            {
                SQLHelpers.Instance.SetConnectionString(ConnectionString);
                bool isSavedSuccessfully = false;
                try
                {
                    #region Parameters
                    SqlParameter reservationDetailsTypeParmeter = new SqlParameter()
                    {
                        ParameterName = "@ReservationDetailsType",
                        Value = this.ToDataTable(reservationDetailsTypeModels),
                    };

                    SqlParameter profileDetailsTypeParmeter = new SqlParameter()
                    {
                        ParameterName = "@tbProfileDetailsType",
                        Value = this.ToDataTable(profileDetailsTypeModels),
                    };

                    #endregion
                    DataTable ResultTable = null;
                    ResultTable = SQLHelpers.Instance.ExecuteSP("usp_InsertReservationDetailsKiosk", reservationDetailsTypeParmeter, profileDetailsTypeParmeter);
                    if (ResultTable != null && ResultTable.Rows.Count > 0)
                    {
                        isSavedSuccessfully = ResultTable.Rows[0][0].ToString() == "1";
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                    //Helpers.EvenLogHelper.Instance.LogError($"Unhandled Exception while executing SP Usp_InsertTransaction {ex.ToString()}");
                }
                return isSavedSuccessfully;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<Models.KIOSK.DB.ReservationDataTableModel> FetchReservationDetails(string ReferenceNumber, DateTime? ArrivalDate, string ConnectionString)
        {
            try
            {
                var spResponse = new DapperHelper().ExecuteSP<Models.KIOSK.DB.ReservationDataTableModel>("Usp_GetReservationDetailsByReferenceNumber",
                    ConnectionString, new { ReferenceNumber = ReferenceNumber, ArrivalDate = (ArrivalDate != null ? ArrivalDate.Value.ToString("yyyyMMdd") : null) }).ToList();
                return spResponse;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<Models.KIOSK.DB.ReservationDataTableModel> FetchProfileDetails(string ReferenceNumber, string ConnectionString)
        {
            try
            {
                var spResponse = new DapperHelper().ExecuteSP<Models.KIOSK.DB.ReservationDataTableModel>("Usp_GetReservationDetailsByReferenceNumber",
                    ConnectionString, new { ReferenceNumber = ReferenceNumber}).ToList();
                return spResponse;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<Models.KIOSK.DB.ReservationDataTableModel> FetchPrecheckedInReservationDetails(string ReferenceNumber, DateTime? ArrivalDate, string ConnectionString)
        {
            try
            {
                var spResponse = new DapperHelper().ExecuteSP<Models.KIOSK.DB.ReservationDataTableModel>("usp_GetReservationByReservationNumber",
                    ConnectionString, new { ReferenceNumber = ReferenceNumber}).ToList();
                return spResponse;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public bool InsertNotificationDetails(string ReservationNameID, string RequestTypeCode,string UserName,bool isActionTaken, string Message,int? ID,string DeviceID, string ConnectionString)
        {
            try
            {
                var spResponse = new DapperHelper().ExecuteSP("Usp_InsertNotification",
                    ConnectionString, new { RESERVATIONNAME = ReservationNameID, TYPECODE = RequestTypeCode, USERNAME = UserName, ISACTIONTAKEN = isActionTaken, MESSAGE= Message, ID = ID ,DEVICEID = DeviceID}).ToList();
                if (spResponse != null)
                    return true;
                else
                    return false;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<Models.KIOSK.DB.DocumentTypeMasterModel> FetchDocumentMasters(string ConnectionString)
        {
            try
            {

                var spResponse = new DapperHelper().ExecuteSP<Models.KIOSK.DB.DocumentTypeMasterModel>("Usp_GetDocumentMaster",
                    ConnectionString).ToList();
                return spResponse;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<Models.KIOSK.PaymentTypeMasterModel> FetchPaymentTypeMasters(string ConnectionString)
        {
            try
            {
                var spResponse = new DapperHelper().ExecuteSP<Models.KIOSK.PaymentTypeMasterModel>("Usp_GetOperaPaymentTypeCode",
                    ConnectionString).ToList();
                return spResponse;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<Models.GeneralSettingsModel> FetchSettingsDetails(string ConnectionString)
        {
            try
            {
                var spResponse = new DapperHelper().ExecuteSP<Models.GeneralSettingsModel>("usp_GetSettingsList",ConnectionString).ToList();
                return spResponse;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public List<Models.KIOSK.PackageMasterModel> FetchPacakgeMaster(string ConnectionString)
        {
            try
            {
                var spResponse = new DapperHelper().ExecuteSP<Models.KIOSK.PackageMasterModel>("Usp_FetchPackageMaster", ConnectionString).ToList();
                return spResponse;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<Models.Local.DB.UserDetails> FetchUserByQrCode(string ConnectionString,string qrCode)
        {
            try
            {
                var spResponse = new DapperHelper().ExecuteSP<Models.Local.DB.UserDetails>("usp_GetUserByQrCode", ConnectionString,new { QrCode = qrCode }).ToList();
                return spResponse;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public List<Models.KIOSK.DB.ProfileDocumentDataTableModel> FetchProfileDocumentByReservationNumber(string ReferenceNumber, string ConnectionString)
        {
            try
            {
                var spResponse = new DapperHelper().ExecuteSP<Models.KIOSK.DB.ProfileDocumentDataTableModel>("usp_getProfileDocumentByReservationNumber",
                    ConnectionString, new { ReferenceNumber = ReferenceNumber}).ToList();
                return spResponse;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public bool InsertKioskManualAuthorizeDetails(Models.KIOSK.KiokManualAuthorizationModel kiok, string ConnectionString)
        {
            try
            {
                var spResponse = new DapperHelper().ExecuteSP("Usp_InsertkioskManualAuthorizationInsertorUpdate",
                    ConnectionString, new
                    {
                        ReservationNameID = kiok.ReservationNameID,
                        IsAdultCountAmmended = kiok.IsAdultCountAmmended,
                        IsECTAmmended = kiok.IsECTAmmended,
                        IsManualyFacialAuthrised = kiok.IsManualyFacialAuthrised,
                        IsManuallyRoomAssigned = kiok.IsManuallyRoomAssigned,
                        ManualyFacialAuthorisedUserName=kiok.ManualyFacialAuthorisedUserName,
                        ManuallyRoomAssignedUsername = kiok.ManuallyRoomAssignedUsername,
                        IsKeyEncodedFailled = kiok.IsKeyEncodedFailled,
                        IsPrintFailled = kiok.IsPrintFailled,
                        IsChekedinEmailSend = kiok.IsChekedinEmailSend,
                        IsDataSendToEVA = kiok.IsDataSendToEVA,
                        EVATransactionDateTime = kiok.EVATransactionDateTime,
                        CreatedDateTime = kiok.CreatedDateTime,
                        Process = kiok.Process,
                        IscheckedOutFailed = kiok.IscheckedOutFailed,
                        IsCheckedinFailled=kiok.IsCheckedinFailled
                    });
                if (spResponse != null)
                    if (spResponse.Count() > 0
                       && !string.IsNullOrEmpty(spResponse.First().Result)
                       && spResponse.First().Result.Equals("1"))
                        return true;
                else
                return false;
                else
                    return false;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}