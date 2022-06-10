
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace CheckinPortalCloudAPI.Helper.Cloud
{
    public class DBHelper
    {
        private static readonly Lazy<DBHelper>
           lazy = new Lazy<DBHelper>(() => 
           new DBHelper()
           );

        public static DBHelper Instance 
        {
            get { 
                return lazy.Value; 
            } 
        }

        public  DataTable ToDataTable<T>(IList<T> data)
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

        public List<T> DataTableToList<T>(DataTable table) where T : class, new()
        {
            try
            {
                T tempT = new T();
                var tType = tempT.GetType();
                List<T> list = new List<T>();
                foreach (var row in table.Rows.Cast<DataRow>())
                {
                    T obj = new T();
                    foreach (var prop in obj.GetType().GetProperties())
                    {
                        var propertyInfo = tType.GetProperty(prop.Name);
                        var rowValue = row[prop.Name];
                        var t = Nullable.GetUnderlyingType(propertyInfo.PropertyType) ?? propertyInfo.PropertyType;
                        object safeValue = (rowValue == null || DBNull.Value.Equals(rowValue)) ? null : Convert.ChangeType(rowValue, t);
                        propertyInfo.SetValue(obj, safeValue, null);
                    }
                    list.Add(obj);
                }
                return list;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public bool BulkUpdateLocallyPushedReservations(List<Models.Cloud.DB.ReservationListTypeModel> reservationListTypeModels, string ConnectionString)
        {
            SQLHelpers.Instance.SetConnectionString(ConnectionString);
            return BulkUpdateLocallyPushedReservations(this.ToDataTable(reservationListTypeModels));
        }

        public bool BulkUpdateLocallyPushedReservations(DataTable transactionTable)
        {
            bool isSavedSuccessfully = false;
            try
            {
                SqlParameter reservationListTypeParmeter = new SqlParameter()
                {
                    ParameterName = "@TbReservationListType",
                    Value = transactionTable,
                };




                var ResultTable = SQLHelpers.Instance.ExecuteSP("Usp_BulkUpdateLocallyPushedReservations", reservationListTypeParmeter);

                if (ResultTable != null && ResultTable.Rows.Count > 0)
                {
                    isSavedSuccessfully = ResultTable.Rows[0][0].ToString() == "1";
                }
            }
            catch (Exception ex)
            {
                //Helpers.EvenLogHelper.Instance.LogError($"Unhandled Exception while executing SP Usp_InsertTransaction {ex.ToString()}");
            }
            return isSavedSuccessfully;
        }

        public bool InsertReservationDetails(List<Models.Cloud.DB.OperaReservationDataTableModel> reservationDetailsTypeModels, List<Models.Cloud.DB.ProfileDetailsDataTableModel> profileDetailsTypeModels, List<Models.Cloud.DB.ProfileDocumentDetailsModel> profileDocumentDetailsTypeModels,string ConnectionString)
        {
            try
            {
                SQLHelpers.Instance.SetConnectionString(ConnectionString);
                return InsertReservationDetails(this.ToDataTable(reservationDetailsTypeModels), this.ToDataTable(profileDetailsTypeModels), this.ToDataTable(profileDocumentDetailsTypeModels));
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        public bool InsertPaymentDetails(List<Models.Cloud.DB.PaymentHeader> paymentHeaders, string ConnectionString)
        {
            bool isSavedSuccessfully = false;
            try
            {
                SQLHelpers.Instance.SetConnectionString(ConnectionString);
                
                
                #region Parameters
                SqlParameter TbPaymentHeaderTypeParmeter = new SqlParameter()
                {
                    ParameterName = "@TbPaymentHeaderType",
                    Value = this.ToDataTable(paymentHeaders),
                };

                SqlParameter TbPaymentHistoryTypeParmeter = new SqlParameter()
                {
                    ParameterName = "@TbPaymentHistoryType",
                    Value = this.ToDataTable(new List<Models.Cloud.DB.PaymentHistory>())
                };

                SqlParameter TbPaymentAdditionalInfoTypeParmeter = new SqlParameter()
                {
                    ParameterName = "@TbPaymentAdditionalInfoType",
                    Value = this.ToDataTable(new List<Models.Cloud.DB.PaymentAdditionalInfo>())
                };
                #endregion

                var ResultTable = SQLHelpers.Instance.ExecuteSP("Usp_InsertPaymentTransactionsFromLocal", TbPaymentHeaderTypeParmeter, TbPaymentHistoryTypeParmeter, TbPaymentAdditionalInfoTypeParmeter);

                if (ResultTable != null && ResultTable.Rows.Count > 0)
                {
                    isSavedSuccessfully = ResultTable.Rows[0][0].ToString() == "1";
                }
                
                return isSavedSuccessfully;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public bool InsertCountrList(List<Models.Cloud.DB.CountryState> countryStates, string ConnectionString)
        {
            bool isSavedSuccessfully = false;
            try
            {
                SQLHelpers.Instance.SetConnectionString(ConnectionString);

                #region Parameters
                SqlParameter TbCountryMasterTypeParmeter = new SqlParameter()
                {
                    ParameterName = "@TbCountryMasterType",
                    Value = this.ToDataTable(countryStates),
                };

                #endregion

                var ResultTable = SQLHelpers.Instance.ExecuteSP("Usp_SyncCountryCodesFromPMS", TbCountryMasterTypeParmeter);

                if (ResultTable != null && ResultTable.Rows.Count > 0)
                {
                    isSavedSuccessfully = ResultTable.Rows[0][0].ToString() == "1";
                }

                return isSavedSuccessfully;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public bool PushBISummaryArrivals(List<Models.Cloud.DB.BISummaryArrivals> bISummaryArrivals, string ConnectionString)
        {
            bool isSavedSuccessfully = false;
            try
            {
                SQLHelpers.Instance.SetConnectionString(ConnectionString);

                #region Parameters
                SqlParameter TbPowerBISummaryArrivalsParmeter = new SqlParameter()
                {
                    ParameterName = "@tbPowerBISummaryArrivals",
                    Value = this.ToDataTable(bISummaryArrivals),
                };

                #endregion

                var ResultTable = SQLHelpers.Instance.ExecuteSP("Usp_MergeBISummaryArrivals", TbPowerBISummaryArrivalsParmeter);

                if (ResultTable != null && ResultTable.Rows.Count > 0)
                {
                    isSavedSuccessfully = ResultTable.Rows[0][0].ToString() == "1";
                }

                return isSavedSuccessfully;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public bool PushBINationalityWiseSummaryArrivals(List<Models.Cloud.DB.BINationalityWiseSummaryArrivals> bINationalityWiseSummaryArrivals, string ConnectionString)
        {
            bool isSavedSuccessfully = false;
            try
            {
                SQLHelpers.Instance.SetConnectionString(ConnectionString);

                #region Parameters
                SqlParameter TbPowerBINationalityWiseSummaryArrivalsParmeter = new SqlParameter()
                {
                    ParameterName = "@TBPowerBINationalityWiseSummaryArrivals",
                    Value = this.ToDataTable(bINationalityWiseSummaryArrivals),
                };

                #endregion

                var ResultTable = SQLHelpers.Instance.ExecuteSP("Usp_MergeBINationalityWiseSummaryArrivals", TbPowerBINationalityWiseSummaryArrivalsParmeter);

                if (ResultTable != null && ResultTable.Rows.Count > 0)
                {
                    isSavedSuccessfully = ResultTable.Rows[0][0].ToString() == "1";
                }

                return isSavedSuccessfully;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public bool UpdatePaymentNotifications(string ConnectionString,List<Models.Cloud.DB.TbNotificationListType> pspRefernceList )
        {
            bool isUpdatedSuccessfully = false;
            try
            {
                SQLHelpers.Instance.SetConnectionString(ConnectionString);

                #region Parameters
                SqlParameter TbNotificationListTypeParmeter = new SqlParameter()
                {
                    ParameterName = "@TbNotificationListType",
                    Value = this.ToDataTable(pspRefernceList),
                };

                #endregion

                var ResultTable = SQLHelpers.Instance.ExecuteSP("usp_UpdateNotificationStatus", TbNotificationListTypeParmeter);

                if (ResultTable != null && ResultTable.Rows.Count > 0)
                {
                    isUpdatedSuccessfully = ResultTable.Rows[0][0].ToString() == "1";
                }

                return isUpdatedSuccessfully;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public bool InsertDueOutReservationDetails(List<Models.Cloud.DB.OperaReservationDataTableModel> reservationDetailsTypeModels, List<Models.Cloud.DB.ProfileDetailsDataTableModel> profileDetailsTypeModels, List<Models.Cloud.DB.ReservationDocumentsDataTableModel> reservationDocumentsDataTables , string ConnectionString)
        {
            try
            {
                SQLHelpers.Instance.SetConnectionString(ConnectionString);
                return InsertDueOutReservationDetails(this.ToDataTable(reservationDetailsTypeModels), this.ToDataTable(profileDetailsTypeModels), this.ToDataTable(new List<Models.Cloud.DB.ProfileDocumentDetailsModel>()), this.ToDataTable(reservationDocumentsDataTables));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public bool InsertDueOutReservationDetails(DataTable reservationDetailsType, DataTable profileDetailsType, DataTable profileDocumentDetailsType,DataTable reservationDocuments)
        {
            bool isSavedSuccessfully = false;
            try
            {
                #region Parameters
                SqlParameter reservationDetailsTypeParmeter = new SqlParameter()
                {
                    ParameterName = "@ReservationDetailsType",
                    Value = reservationDetailsType,
                };

                SqlParameter profileDetailsTypeParmeter = new SqlParameter()
                {
                    ParameterName = "@tbProfileDetailsType",
                    Value = profileDetailsType,
                };

                SqlParameter profileDocumentDetailsTypeParmeter = new SqlParameter()
                {
                    ParameterName = "@tbProfileDocumentDetailsType",
                    Value = profileDocumentDetailsType,
                };

                SqlParameter reservationDOcumentsTypeParmeter = new SqlParameter()
                {
                    ParameterName = "@tbReservationDocumentsType",
                    Value = reservationDocuments,
                };

                #endregion

                var ResultTable = SQLHelpers.Instance.ExecuteSP("Usp_InsertReservationDetailsCheckOut", reservationDetailsTypeParmeter, profileDetailsTypeParmeter, profileDocumentDetailsTypeParmeter, reservationDOcumentsTypeParmeter);

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

        public bool InsertReservationDetails(DataTable reservationDetailsType, DataTable profileDetailsType, DataTable profileDocumentDetailsType)
        {
            bool isSavedSuccessfully = false;
            try
            {
                #region Parameters
                SqlParameter reservationDetailsTypeParmeter = new SqlParameter()
                {
                    ParameterName = "@ReservationDetailsType",
                    Value = reservationDetailsType,
                };

                SqlParameter profileDetailsTypeParmeter = new SqlParameter()
                {
                    ParameterName = "@tbProfileDetailsType",
                    Value = profileDetailsType,
                };

                SqlParameter profileDocumentDetailsTypeParmeter = new SqlParameter()
                {
                    ParameterName = "@tbProfileDocumentDetailsType",
                    Value = profileDocumentDetailsType,
                };

                #endregion

                var ResultTable = SQLHelpers.Instance.ExecuteSP("Usp_InsertReservationDetails", reservationDetailsTypeParmeter, profileDetailsTypeParmeter, profileDocumentDetailsTypeParmeter);

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
    
        public List<Models.Cloud.DB.CloudFetchReservationModel> FetchReservationForLocalPush(string ConnectionString)
        {
            SQLHelpers.Instance.SetConnectionString(ConnectionString);
            DataTable transaction = new DataTable();
            try
            {
                string Query = string.Empty;
                transaction = SQLHelpers.Instance.ExecuteSP("usp_FetchReservationforLocalPush");
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return DataTableToList<Models.Cloud.DB.CloudFetchReservationModel>(transaction);
        }

        public List<Models.Cloud.DB.CloudFetchReservationModel> FetchReservationForLocalPush(string ConnectionString,string ReservationNumber, bool isForceFetch)
        {
            SQLHelpers.Instance.SetConnectionString(ConnectionString);
            
            #region Parameters
            SqlParameter reservationNumberParmeter = new SqlParameter()
            {
                ParameterName = "@ReservationNumber",
                Value = ReservationNumber,
            };

            SqlParameter forceFetchParmeter = new SqlParameter()
            {
                ParameterName = "@ForceFetch",
                Value = isForceFetch,
                SqlDbType = SqlDbType.Bit
            };

            #endregion

            DataTable transaction = new DataTable();
            try
            {
                string Query = string.Empty;
                transaction = SQLHelpers.Instance.ExecuteSP("usp_FetchReservationforLocalPush", reservationNumberParmeter, forceFetchParmeter);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return DataTableToList<Models.Cloud.DB.CloudFetchReservationModel>(transaction);
        }

        public List<Models.Cloud.DB.CloudFetchReservationPolicyModel> FetchReservationPolicyForLocalPush(string ConnectionString, string ReservationNumber)
        {
            SQLHelpers.Instance.SetConnectionString(ConnectionString);

            #region Parameters
            SqlParameter reservationNumberParmeter = new SqlParameter()
            {
                ParameterName = "@ReservationNumber",
                Value = ReservationNumber,
            };

            

            #endregion

            DataTable transaction = new DataTable();
            try
            {
                string Query = string.Empty;
                transaction = SQLHelpers.Instance.ExecuteSP("Usp_FetchPrecheckedPolicyDetails", reservationNumberParmeter);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return DataTableToList<Models.Cloud.DB.CloudFetchReservationPolicyModel>(transaction);
        }



        public List<Models.Cloud.DB.ReservationStatusInCloud> FetchReservationStatusInCloud(string ConnectionString, string ReservationNumber)
        {
            SQLHelpers.Instance.SetConnectionString(ConnectionString);
            DataTable transaction = new DataTable();
            #region Parameters
            SqlParameter reservationNumberParmeter = new SqlParameter()
            {
                ParameterName = "@ReservationNumber",
                Value = ReservationNumber,
            };

            
            #endregion

            try
            {
                string Query = string.Empty;
                transaction = SQLHelpers.Instance.ExecuteSP("Usp_CheckReservationStatusinCloud", reservationNumberParmeter);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return DataTableToList<Models.Cloud.DB.ReservationStatusInCloud>(transaction);
        }

        public List<Models.Cloud.DB.DataClearResponseDataTableModel> ClearData(string ConnectionString,string reservationNameID)
        {
            DataTable transaction = new DataTable();
            SQLHelpers.Instance.SetConnectionString(ConnectionString);
            try
            {
                SqlParameter reservationNameIDTypeParmeter = new SqlParameter()
                {
                    ParameterName = "@ReservationNameID",
                    Value = reservationNameID,
                };
                string Query = string.Empty;
                transaction = SQLHelpers.Instance.ExecuteSP("Usp_JobDailyClearReservationTransactions", reservationNameIDTypeParmeter);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return DataTableToList<Models.Cloud.DB.DataClearResponseDataTableModel>(transaction);
        }


        public List<Models.Cloud.DB.CloudFetchCheckoutReservationModel> FetchReservationforLocalPushCheckOut(string ConnectionString)
        {
            SQLHelpers.Instance.SetConnectionString(ConnectionString);
            DataTable transaction = new DataTable();
            try
            {
                string Query = string.Empty;
                transaction = SQLHelpers.Instance.ExecuteSP("usp_FetchReservationforLocalPushCheckOut");
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return DataTableToList<Models.Cloud.DB.CloudFetchCheckoutReservationModel>(transaction);
        }
        public List<Models.Cloud.DB.CloudFetchCheckoutReservationModel> FetchReservationforLocalPushCheckOut(string ConnectionString,string reservationID)
        {
            SQLHelpers.Instance.SetConnectionString(ConnectionString);
            DataTable transaction = new DataTable();
            try
            {
                SqlParameter reservationNameIDTypeParmeter = new SqlParameter()
                {
                    ParameterName = "@ReservationNumber",
                    Value = reservationID,
                    SqlDbType = SqlDbType.VarChar
                };
                string Query = string.Empty;
                transaction = SQLHelpers.Instance.ExecuteSP("usp_FetchReservationforLocalPushCheckOut", reservationNameIDTypeParmeter);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return DataTableToList<Models.Cloud.DB.CloudFetchCheckoutReservationModel>(transaction);
        }


        public List<Models.Cloud.DB.ProfileDocuments> FetchProfileDocuments(string ConnectionString,string ReservationNameID)
        {
            SQLHelpers.Instance.SetConnectionString(ConnectionString);
            SqlParameter reservationDetailsTypeParmeter = new SqlParameter()
            {
                ParameterName = "@ReservationNameID",
                Value = ReservationNameID,
                DbType = DbType.String
            };
            DataTable transaction = new DataTable();
            try
            {
                string Query = string.Empty;
                transaction = SQLHelpers.Instance.ExecuteSP("usp_FetchProfileDocumentsforLocalPush", reservationDetailsTypeParmeter);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return DataTableToList<Models.Cloud.DB.ProfileDocuments>(transaction);
        }

        public List<Models.Cloud.DB.ReservationAdditionalDetails> FetchReservationAdditionalDetails(string ConnectionString, string ReservationNumber)
        {
            SQLHelpers.Instance.SetConnectionString(ConnectionString);
            SqlParameter reservationNumberParmeter = new SqlParameter()
            {
                ParameterName = "@ReservationNumber",
                Value = ReservationNumber,
                DbType = DbType.String
            };
            DataTable transaction = new DataTable();
            try
            {
                string Query = string.Empty;
                transaction = SQLHelpers.Instance.ExecuteSP("usp_FetchAdditionalReservationDetails", reservationNumberParmeter);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return DataTableToList<Models.Cloud.DB.ReservationAdditionalDetails>(transaction);
        }



        public List<Models.Cloud.DB.PaymentHeader> FetchpaymentHeader(string ConnectionString, string ReservationNameID)
        {

            SQLHelpers.Instance.SetConnectionString(ConnectionString);
            SqlParameter reservationnameIDParmeter = new SqlParameter()
            {
                ParameterName = "@ReservationNameID",
                Value = ReservationNameID,
                DbType = DbType.String
            };
            DataTable transaction = new DataTable();
            try
            {
                string Query = string.Empty;
                transaction = SQLHelpers.Instance.ExecuteSP("usp_FetchPaymentforLocalPush", reservationnameIDParmeter);
                if (transaction != null)
                {
                    List<Models.Cloud.DB.PaymentHeader> paymentHeaders = DataTableToList<Models.Cloud.DB.PaymentHeader>(transaction);
                    return paymentHeaders;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<Models.Cloud.DB.PaymentAdditionalInfo> FetchpaymentAdditionalInfo(string ConnectionString,string ReservationNameID)
        {
            
            SQLHelpers.Instance.SetConnectionString(ConnectionString);
            SqlParameter reservationnameIDParmeter = new SqlParameter()
            {
                ParameterName = "@ReservationNameID",
                Value = ReservationNameID,
                DbType = DbType.String
            };
            DataTable transaction = new DataTable();
            try
            {
                string Query = string.Empty;
                transaction = SQLHelpers.Instance.ExecuteSP("usp_FetchPaymentAdditionalInfoforLocalPush", reservationnameIDParmeter);
                if (transaction != null)
                {
                    List<Models.Cloud.DB.PaymentAdditionalInfo> paymentAdditionalInfos = DataTableToList<Models.Cloud.DB.PaymentAdditionalInfo>(transaction);
                    return paymentAdditionalInfos;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<Models.Cloud.DB.PaymentHistory> FetchPaymentHistory(string ConnectionString, string ReservationNameID)
        {
            SQLHelpers.Instance.SetConnectionString(ConnectionString);
            SqlParameter reservationnameIDParmeter = new SqlParameter()
            {
                ParameterName = "@ReservationNameID",
                Value = ReservationNameID,
                DbType = DbType.String
            };
            DataTable transaction = new DataTable();
            try
            {
                string Query = string.Empty;
                transaction = SQLHelpers.Instance.ExecuteSP("usp_FetchPaymentHistoryforLocalPush", reservationnameIDParmeter);
                if(transaction != null)
                {
                    List<Models.Cloud.DB.PaymentHistory> paymentHistories = DataTableToList<Models.Cloud.DB.PaymentHistory>(transaction);
                    return paymentHistories;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
           
        }

       

        public List<Models.Cloud.DB.PaymentNotification> FetchPaymentNotifications(string ConnectionString)
        {
            SQLHelpers.Instance.SetConnectionString(ConnectionString);
            
            DataTable transaction = new DataTable();
            try
            {
                string Query = string.Empty;
                transaction = SQLHelpers.Instance.ExecuteSP("Usp_FetchNotificationForLocalPush");
                if (transaction != null)
                {
                    List<Models.Cloud.DB.PaymentNotification> paymentHistories = DataTableToList<Models.Cloud.DB.PaymentNotification>(transaction);
                    return paymentHistories;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public List<Models.Cloud.DB.ReservationDocumentsDataTableModel> FetchReservationDocuments(string ConnectionString, string ReservationNameID)
        {
            SQLHelpers.Instance.SetConnectionString(ConnectionString);
            SqlParameter reservationDetailsTypeParmeter = new SqlParameter()
            {
                ParameterName = "@ReservationNameID",
                Value = ReservationNameID,
                DbType = DbType.String
            };
            DataTable transaction = new DataTable();
            try
            {
                string Query = string.Empty;
                transaction = SQLHelpers.Instance.ExecuteSP("usp_FetchReservationDocumentsforLocalPushCheckOut");
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return DataTableToList<Models.Cloud.DB.ReservationDocumentsDataTableModel>(transaction);
        }

        public List<Models.Cloud.DB.UpsellPackageModel> FetchUpsellPackages(string ConnectionString, string ReservationNameID)
        {
            SQLHelpers.Instance.SetConnectionString(ConnectionString);
            SqlParameter reservationDetailsTypeParmeter = new SqlParameter()
            {
                ParameterName = "@ReservationNameID",
                Value = ReservationNameID,
                DbType = DbType.String
            };
            DataTable transaction = new DataTable();
            try
            {
                string Query = string.Empty;
                transaction = SQLHelpers.Instance.ExecuteSP("usp_FetchReservationPackageforLocalPush");
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return DataTableToList<Models.Cloud.DB.UpsellPackageModel>(transaction);
        }

        public List<Models.Cloud.DB.FeedBackModel> FetchFeedback(string ConnectionString, string ReservationNameID)
        {
            SQLHelpers.Instance.SetConnectionString(ConnectionString);
            SqlParameter reservationDetailsTypeParmeter = new SqlParameter()
            {
                ParameterName = "@ReservationNameID",
                Value = ReservationNameID,
                DbType = DbType.String
            };
            DataTable transaction = new DataTable();
            try
            {
                string Query = string.Empty;
                transaction = SQLHelpers.Instance.ExecuteSP("usp_FetchReservationFeedBackforLocalPush");
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return DataTableToList<Models.Cloud.DB.FeedBackModel>(transaction);
        }

        public List<Models.Cloud.DB.RoomTypeMaster> FetchRoomTypeMaster(string ConnectionString)
        {
            SQLHelpers.Instance.SetConnectionString(ConnectionString);
            
            DataTable transaction = new DataTable();
            try
            {
                string Query = string.Empty;
                transaction = SQLHelpers.Instance.ExecuteSP("Usp_GetRoomTypeMaster");
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return DataTableToList<Models.Cloud.DB.RoomTypeMaster>(transaction);
        }

        public bool InsertPaymentNotifications(string EventCode, DateTime EventDate, string MerchantAccountCode,string MerchantReference,string OrginalReference,
                                                string PaymentMethod,string PspReference,decimal Amount,string Currency,string Reason,bool TransactionStatus,
                                                string NotificationJSON,string ConnectionString)
        {
            bool isSavedSuccessfully = false;
            try
            {
                SQLHelpers.Instance.SetConnectionString(ConnectionString);
                #region Parameters
                SqlParameter eventCodeParmeter = new SqlParameter()
                {
                    ParameterName = "@EventCode",
                    Value = EventCode,
                    SqlDbType = SqlDbType.VarChar
                };

                SqlParameter eventDateParmeter = new SqlParameter()
                {
                    ParameterName = "@EventDate",
                    Value = EventDate,
                    SqlDbType = SqlDbType.DateTime2
                };

                SqlParameter merchantAccountCodeParmeter = new SqlParameter()
                {
                    ParameterName = "@MerchantAccountCode",
                    Value = MerchantAccountCode,
                    SqlDbType = SqlDbType.VarChar
                };

                SqlParameter merchantReferenceParmeter = new SqlParameter()
                {
                    ParameterName = "@MerchantReference",
                    Value = MerchantReference,
                    SqlDbType = SqlDbType.VarChar
                };

                SqlParameter orginalReferenceParmeter = new SqlParameter()
                {
                    ParameterName = "@OrginalReference",
                    Value = OrginalReference,
                    SqlDbType = SqlDbType.VarChar
                };

                SqlParameter paymentMethodParmeter = new SqlParameter()
                {
                    ParameterName = "@PaymentMethod",
                    Value = PaymentMethod,
                    SqlDbType = SqlDbType.VarChar
                };

                SqlParameter pspReferenceParmeter = new SqlParameter()
                {
                    ParameterName = "@PspReference",
                    Value = PspReference,
                    SqlDbType = SqlDbType.VarChar
                };

                SqlParameter amountParmeter = new SqlParameter()
                {
                    ParameterName = "@Amount",
                    Value = Amount,
                    SqlDbType = SqlDbType.Decimal
                };

                SqlParameter currencyParmeter = new SqlParameter()
                {
                    ParameterName = "@Currency",
                    Value = Currency,
                    SqlDbType = SqlDbType.VarChar
                };

                SqlParameter reasonParmeter = new SqlParameter()
                {
                    ParameterName = "@Reason",
                    Value = Reason,
                    SqlDbType = SqlDbType.VarChar
                };

                SqlParameter transactionStatusParmeter = new SqlParameter()
                {
                    ParameterName = "@TransactionStatus",
                    Value = TransactionStatus,
                    SqlDbType = SqlDbType.Bit
                };

                SqlParameter notificationJSONParmeter = new SqlParameter()
                {
                    ParameterName = "@NotificationJSON",
                    Value = NotificationJSON,
                    SqlDbType = SqlDbType.NVarChar
                };

                #endregion

                var ResultTable = SQLHelpers.Instance.ExecuteSP("Usp_InsertNotification", eventCodeParmeter, eventDateParmeter, merchantAccountCodeParmeter,
                                  merchantReferenceParmeter, orginalReferenceParmeter, paymentMethodParmeter, pspReferenceParmeter, amountParmeter, currencyParmeter,
                                  reasonParmeter, transactionStatusParmeter, notificationJSONParmeter);

                if (ResultTable != null && ResultTable.Rows.Count > 0)
                {
                    isSavedSuccessfully = ResultTable.Rows[0][0].ToString() == "1";
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return isSavedSuccessfully;
        }

        public bool InsertFeedbackData(int ReservationID, int QuestionID, string Answer)
        {
            bool isSavedSuccessfully = false;
            try
            {
                #region Parameters
                SqlParameter reservationIDParmeter = new SqlParameter()
                {
                    ParameterName = "@ReservationID",
                    Value = ReservationID,
                    SqlDbType= SqlDbType.Int
                };

                SqlParameter questionParmeter = new SqlParameter()
                {
                    ParameterName = "@QuestionID",
                    Value = QuestionID,
                    SqlDbType = SqlDbType.Int
                };

                SqlParameter answerParmeter = new SqlParameter()
                {
                    ParameterName = "@Answer",
                    Value = Answer,
                     SqlDbType= SqlDbType.VarChar
                };

                #endregion

                var ResultTable = SQLHelpers.Instance.ExecuteSP("Usp_InsertFeedbackData", reservationIDParmeter, questionParmeter, answerParmeter);

                if (ResultTable != null && ResultTable.Rows.Count > 0)
                {
                    isSavedSuccessfully = ResultTable.Rows[0][0].ToString() == "1";
                }
            }
            catch (Exception ex)
            {
                //Helpers.EvenLogHelper.Instance.LogError($"Unhandled Exception while executing SP Usp_InsertTransaction {ex.ToString()}");
            }
            return isSavedSuccessfully;
        }

        public bool InsertPackageMaster(string ConnectionString, Models.Cloud.DB.PackageMasterModel packageMaster)

        {
            bool isSavedSuccessfully = false;
            //SQLHelpers.Instance.SetConnectionString(ConnectionString);
            byte[] packageImage = null;
            if (!string.IsNullOrEmpty(packageMaster.PackageImage))
                packageImage = Convert.FromBase64String(packageMaster.PackageImage);
            try
            {
                SQLHelpers.Instance.SetConnectionString(ConnectionString);
                //this.ToDataTable
                SqlParameter packageIDParameter = new SqlParameter()
                {
                    ParameterName = "@PackageID",
                    Value = packageMaster.PackageID,
                    SqlDbType = SqlDbType.Int
                };

                SqlParameter PackageCodeParmeter = new SqlParameter()
                {
                    ParameterName = "@PackageCode",
                    Value = packageMaster.PackageCode,
                    SqlDbType = SqlDbType.VarChar
                };
                SqlParameter PackageNameParmeter = new SqlParameter()
                {
                    ParameterName = "@PackageName",
                    Value = packageMaster.PackageName,
                    SqlDbType = SqlDbType.VarChar
                };
                SqlParameter PackageAmountParmeter = new SqlParameter()
                {
                    ParameterName = "@PackageAmount",
                    Value = packageMaster.PackageAmount,
                    SqlDbType = SqlDbType.Decimal
                };
                SqlParameter PackageDescParmeter = new SqlParameter()
                {
                    ParameterName = "@PackageDesc",
                    Value = packageMaster.PackageDesc,
                    SqlDbType = SqlDbType.VarChar
                };
                SqlParameter UnitsParmeter = new SqlParameter()
                {
                    ParameterName = "@Units",
                    Value = packageMaster.Units,
                    SqlDbType = SqlDbType.VarChar
                };
                SqlParameter PackageImageParmeter;
                if (!string.IsNullOrEmpty(packageMaster.PackageImage))
                {
                    PackageImageParmeter = new SqlParameter()
                    {
                        ParameterName = "@PackageImage",
                        Value = packageImage ,
                        SqlDbType = SqlDbType.VarBinary
                    };
                }
                else
                {
                    PackageImageParmeter = new SqlParameter()
                    {
                        ParameterName = "@PackageImage",
                        Value = DBNull.Value,
                        SqlDbType = SqlDbType.VarBinary
                    };
                }
                SqlParameter RoomCodeMappingListParmeter = new SqlParameter()
                {
                    ParameterName = "@tbRoomCodeMappingList",
                    Value = this.ToDataTable(packageMaster.RoomTypeCode)
                };
                SqlParameter isActiveParmeter = new SqlParameter()
                {
                    ParameterName = "@isActive",
                    Value = packageMaster.isActive,
                    SqlDbType = SqlDbType.Bit
                };

                SqlParameter isRoomUpsellParmeter = new SqlParameter()
                {
                    ParameterName = "@IsRoomUpsell",
                    Value = packageMaster.isRoomUpsell != null ? packageMaster.isRoomUpsell.Value : false,
                    SqlDbType = SqlDbType.Bit
                };

                DataTable transaction = new DataTable();
                try
                {
                    string Query = string.Empty;
                    var ResultTable = SQLHelpers.Instance.ExecuteSP("Usp_InsertPackageMaster", packageIDParameter, PackageCodeParmeter, PackageNameParmeter,
                        PackageAmountParmeter, PackageDescParmeter, PackageImageParmeter, RoomCodeMappingListParmeter, isActiveParmeter, isRoomUpsellParmeter, UnitsParmeter);
                    if (ResultTable != null && ResultTable.Rows.Count > 0)
                    {
                        isSavedSuccessfully = ResultTable.Rows[0][0].ToString() == "1";
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            catch (Exception ex)
            {
                //Helpers.EvenLogHelper.Instance.LogError($"Unhandled Exception while executing SP Usp_InsertTransaction {ex.ToString()}");

                throw ex;
            }
            return isSavedSuccessfully;
        }

        public List<Models.Cloud.DB.PackageMasterDataTableModel> FetchPackageMaster(string ConnectionString, int PackageID)

        {
            try
            {
                SQLHelpers.Instance.SetConnectionString(ConnectionString);
                SqlParameter packageIDParameter = new SqlParameter()
                {
                    ParameterName = "@PackageID",
                    Value = PackageID,
                    SqlDbType = SqlDbType.Int
                };

                DataTable transaction = new DataTable();
                try
                {
                    string Query = string.Empty;
                    transaction = SQLHelpers.Instance.ExecuteSP("Usp_GetPackageMasterDetailedList", packageIDParameter);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                return DataTableToList<Models.Cloud.DB.PackageMasterDataTableModel>(transaction);    
            }
            catch (Exception ex)
            {
                
                throw ex;
            }
           
        }

        public string FetchCardTypeInfo(string ConnectionString, string CardType)
        {
            try
            {
                string fundingSource = null;
                SQLHelpers.Instance.SetConnectionString(ConnectionString);
                SqlParameter cardTypeParameter = new SqlParameter()
                {
                    ParameterName = "@CardType",
                    Value = CardType,
                    SqlDbType = SqlDbType.VarChar
                };

                DataTable transaction = new DataTable();
                
                string Query = string.Empty;
                transaction = SQLHelpers.Instance.ExecuteSP("Usp_GetCardTypeInfo", cardTypeParameter);
                if (transaction != null && transaction.Rows.Count > 0)
                {
                    fundingSource = transaction.Rows[0][0].ToString();

                }
                
                return fundingSource;
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }

    }

}