
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace CheckinPortalCloudAPI.Helper.Local
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
            catch (Exception ex)//changed here
            {
                throw ex;
            }
        }

        public List<Models.Local.DB.DataClearResponseDataTableModel> ClearData(string ConnectionString)
        {
            DataTable transaction = new DataTable();
            SQLHelpers.Instance.SetConnectionString(ConnectionString);
            try
            {
                string Query = string.Empty;
                transaction = SQLHelpers.Instance.ExecuteSP("Usp_ClearData");
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return DataTableToList<Models.Local.DB.DataClearResponseDataTableModel>(transaction);
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
        

        public bool BulkUpdateReservationToDB(List<Models.Local.DB.ReservationListTypeModel> reservationListTypeModels,string ConnectionString)
        {
            SQLHelpers.Instance.SetConnectionString(ConnectionString);
            return BulkUpdateReservationToDB(this.ToDataTable(reservationListTypeModels));
        }

        public List<Models.Local.DB.ReservationListTypeModel> BulkFetchReservationStatus(List<Models.Local.DB.ReservationListTypeModel> reservationListTypeModels, string ConnectionString)
        {
            SQLHelpers.Instance.SetConnectionString(ConnectionString);
            //return BulkUpdateReservationToDB(this.ToDataTable(reservationListTypeModels));
            bool isSavedSuccessfully = false;
            try
            {
                SqlParameter reservationListTypeParmeter = new SqlParameter()
                {
                    ParameterName = "@TbReservationListType",
                    Value = this.ToDataTable(reservationListTypeModels),
                };

                var ResultTable = SQLHelpers.Instance.ExecuteSP("Usp_GetReservationStatusLocal", reservationListTypeParmeter);
                if (ResultTable != null)
                    return this.DataTableToList<Models.Local.DB.ReservationListTypeModel>(ResultTable);

                else
                    return null;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            
        }

        public List<Models.Local.DB.ReservationTrackStatus> FetchReservationTrackStatus(Models.Local.DB.ReservationTrackStatus reservationTrackStatus, string ConnectionString)
        {
            SQLHelpers.Instance.SetConnectionString(ConnectionString);
            try
            {
                SqlParameter reservationNameIDTypeParmeter = new SqlParameter()
                {
                    ParameterName = "@ReservationNameID",
                    SqlDbType = SqlDbType.VarChar,                    
                    Value = reservationTrackStatus.ReservationNameID,
                };
                SqlParameter processTypeParmeter = new SqlParameter()
                {
                    ParameterName = "@ProcessType",
                    SqlDbType = SqlDbType.VarChar,
                    Value = reservationTrackStatus.ProcessType,
                };

                var ResultTable = SQLHelpers.Instance.ExecuteSP("Usp_GetProcessTrackingStatus", reservationNameIDTypeParmeter, processTypeParmeter);
                if (ResultTable != null)
                    return this.DataTableToList<Models.Local.DB.ReservationTrackStatus>(ResultTable);

                else
                    return null;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public bool PushReservationTrackStatus(Models.Local.DB.ReservationTrackStatus reservationTrackStatus, string ConnectionString)
        {
            bool isSavedSuccessfully = false;
            SQLHelpers.Instance.SetConnectionString(ConnectionString);
            try
            {

                SqlParameter reservationNameIDTypeParmeter = new SqlParameter()
                {
                    ParameterName = "@ReservationNameID",
                    SqlDbType = SqlDbType.VarChar,
                    Value = reservationTrackStatus.ReservationNameID,
                };
                SqlParameter processTypeParmeter = new SqlParameter()
                {
                    ParameterName = "@ProcessType",
                    SqlDbType = SqlDbType.VarChar,
                    Value = reservationTrackStatus.ProcessType,
                };
                SqlParameter reservationNumberParmeter = new SqlParameter()
                {
                    ParameterName = "@ReservationNumber",
                    SqlDbType = SqlDbType.VarChar,
                    Value = reservationTrackStatus.ReservationNumber,
                };
                SqlParameter processStatusParmeter = new SqlParameter()
                {
                    ParameterName = "@ProcessStatus",
                    SqlDbType = SqlDbType.VarChar,
                    Value = reservationTrackStatus.ProcessStatus,
                };
                SqlParameter emailSentParmeter = new SqlParameter()
                {
                    ParameterName = "@EmailSent",
                    SqlDbType = SqlDbType.Bit,
                    Value = reservationTrackStatus.EmailSent,
                };

                var ResultTable = SQLHelpers.Instance.ExecuteSP("Usp_InsertProcessTracking", reservationNameIDTypeParmeter, processTypeParmeter, reservationNumberParmeter
                    ,processStatusParmeter, emailSentParmeter);
                
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

        public bool BulkUpdateReservationToDB(DataTable transactionTable)
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

        public bool InsertReservationDetails(List<Models.Local.DB.OperaReservationDataTableModel> reservationDetailsTypeModels, List<Models.Local.DB.ProfileDetailsDataTableModel> profileDetailsTypeModels, List<Models.Local.DB.ProfileDocumentDetailsModel> profileDocumentDetailsTypeModels,bool? IsCloud ,string ConnectionString)
        {
            try
            {
                SQLHelpers.Instance.SetConnectionString(ConnectionString);
                return InsertReservationDetails(this.ToDataTable(reservationDetailsTypeModels), this.ToDataTable(profileDetailsTypeModels), this.ToDataTable(profileDocumentDetailsTypeModels),IsCloud);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public bool InsertReservationDocuments(List<Models.Local.DB.ReservationDocumentsDataTableModel> reservationDocuments, bool IsCloud, string ConnectionString)
        {
            try
            {
                SQLHelpers.Instance.SetConnectionString(ConnectionString);
                return InsertReservationDocuments(this.ToDataTable(reservationDocuments),  IsCloud);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public bool InsertReservationDocuments(DataTable resetvationDocuments, bool Iscloud)
        {
            bool isSavedSuccessfully = false;
            try
            {
                #region Parameters
                SqlParameter reservationDocumentsTypeParameter = new SqlParameter()
                {
                    ParameterName = "@TbReservationDocumentType",
                    Value = resetvationDocuments,
                };

                

                SqlParameter IsCloudParmeter = new SqlParameter()
                {
                    ParameterName = "@IsCloud",
                    Value = Iscloud,
                    DbType = DbType.Boolean
                };

                #endregion

                var ResultTable = SQLHelpers.Instance.ExecuteSP("usp_InsertReservationDocumentDetails", reservationDocumentsTypeParameter,IsCloudParmeter);

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

        public bool InsertUpsellPackages(List<Models.Local.DB.UpsellPackageModel> upsellPackages,  string ConnectionString)
        {
            try
            {
                SQLHelpers.Instance.SetConnectionString(ConnectionString);
                return InsertUpsellPackages(this.ToDataTable(upsellPackages));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public bool InsertReservationPolicies(List<Models.Local.DB.ReservationPolicyModel> reservationPolicies, string ConnectionString)
        {
            try
            {
                SQLHelpers.Instance.SetConnectionString(ConnectionString);
                return InsertReservationPolicies(this.ToDataTable(reservationPolicies));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public bool InsertReservationPolicies(DataTable reservationPolicies)
        {
            bool isSavedSuccessfully = false;
            try
            {
                #region Parameters
                SqlParameter reservationPoliciesTypeParmeter = new SqlParameter()
                {
                    ParameterName = "@TbReservationPolicyType",
                    Value = reservationPolicies,
                };

                #endregion

                var ResultTable = SQLHelpers.Instance.ExecuteSP("Usp_InsertReservationPolicyDetails", reservationPoliciesTypeParmeter);

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

        public bool InsertCountrList(List<Models.Local.DB.CountryState> countryStates, string ConnectionString)
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

        public bool InsertUpsellPackages(DataTable upsellPackages)
        {
            bool isSavedSuccessfully = false;
            try
            {
                #region Parameters
                SqlParameter upsellPackagesTypeParmeter = new SqlParameter()
                {
                    ParameterName = "@TbReservationPackageType",
                    Value = upsellPackages,
                };

                #endregion

                var ResultTable = SQLHelpers.Instance.ExecuteSP("usp_InsertReservationPackageDetails", upsellPackagesTypeParmeter);

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

        public bool InsertFeedback(List<Models.Local.DB.FeedBackModel> feedBacks, string ConnectionString)
        {
            try
            {
                SQLHelpers.Instance.SetConnectionString(ConnectionString);
                return InsertFeedback(this.ToDataTable(feedBacks));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public bool InsertFeedback(DataTable feedBacks)
        {
            bool isSavedSuccessfully = false;
            try
            {
                #region Parameters
                SqlParameter FeedbackTypeParmeter = new SqlParameter()
                {
                    ParameterName = "@TbReservationFeedBackType",
                    Value = feedBacks,
                };

                #endregion

                var ResultTable = SQLHelpers.Instance.ExecuteSP("usp_InsertReservatinFeedbackDetails", FeedbackTypeParmeter);

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

        public bool InsertReservationDetails(DataTable reservationDetailsType, DataTable profileDetailsType, DataTable profileDocumentDetailsType,bool? Iscloud)
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

                SqlParameter IsCloudParmeter = new SqlParameter()
                {
                    ParameterName = "@IsCloud",
                    Value = Iscloud,
                    DbType = DbType.Boolean
                };

                #endregion
                DataTable ResultTable = null;
                if (Iscloud != null)
                    ResultTable = SQLHelpers.Instance.ExecuteSP("Usp_InsertReservationDetails", reservationDetailsTypeParmeter, profileDetailsTypeParmeter, profileDocumentDetailsTypeParmeter, IsCloudParmeter);
                else
                    ResultTable = SQLHelpers.Instance.ExecuteSP("Usp_InsertReservationDetails", reservationDetailsTypeParmeter, profileDetailsTypeParmeter, profileDocumentDetailsTypeParmeter);
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

        public bool InsertReservationAdditionalDetails(List<Models.Local.DB.ReservationAdditionalDetails> reservationAdditionalDetails,string ConnectionString)
        {
            bool isSavedSuccessfully = false;
            try
            {
                SQLHelpers.Instance.SetConnectionString(ConnectionString);

                #region Parameters
                SqlParameter reservationDetailsTypeParmeter = new SqlParameter()
                {
                    ParameterName = "@tbReservationAdditionalDetailstype",
                    Value = this.ToDataTable(reservationAdditionalDetails),
                };

                
                

                #endregion
                DataTable ResultTable = null;
                
                ResultTable = SQLHelpers.Instance.ExecuteSP("Usp_InsertReservationAdditionalDetails", reservationDetailsTypeParmeter);
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

        public DataTable FetchReservationForCloudPush()
        {
            DataTable transaction = new DataTable();
            try
            {
                string Query = string.Empty;
                transaction = SQLHelpers.Instance.ExecuteSP("usp_FetchReservationforCloudPush");
            }
            catch (Exception ex)
            {
                //Helpers.EvenLogHelper.Instance.LogError($"Unhandled Exception while executing SP Usp_FetchTopOneRecordsforProcessing {ex.ToString()}");
            }
            return transaction;
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
                    SqlDbType = SqlDbType.Int
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
                    SqlDbType = SqlDbType.VarChar
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
        public bool InsertPaymentDetails(List<Models.Local.DB.PaymentHistory> paymentHistories, List<Models.Local.DB.PushPaymentHeaderModel> paymentHeaders, List<Models.Local.DB.PaymentAdditionalInfo> paymentAdditionalInfos, string ConnectionString)
        {
            bool isSavedSuccessfully = false;
            try
            {

                SQLHelpers.Instance.SetConnectionString(ConnectionString);
                try
                {
                    #region Parameters
                    SqlParameter TbPaymentHeaderTypeParmeter = new SqlParameter()
                    {
                        ParameterName = "@TbPaymentHeaderType",
                        Value = this.ToDataTable(paymentHeaders),
                    };

                    SqlParameter TbPaymentHistoryTypeParmeter = new SqlParameter()
                    {
                        ParameterName = "@TbPaymentHistoryType",
                        Value = this.ToDataTable(paymentHistories),
                    };

                    SqlParameter TbPaymentAdditionalInfoTypeParmeter = new SqlParameter()
                    {
                        ParameterName = "@TbPaymentAdditionalInfoType",
                        Value = paymentAdditionalInfos != null ? this.ToDataTable(paymentAdditionalInfos) : null,
                    };

                    #endregion

                    var ResultTable = SQLHelpers.Instance.ExecuteSP("Usp_InsertPaymentTransactions", TbPaymentAdditionalInfoTypeParmeter, TbPaymentHeaderTypeParmeter, TbPaymentHistoryTypeParmeter);

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
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return isSavedSuccessfully;
        }

        public bool InsertPaymentNotifications(List<Models.Local.DB.PaymentNotification> paymentNotifications,  string ConnectionString)
        {
            bool isSavedSuccessfully = false;
            try
            {

                SQLHelpers.Instance.SetConnectionString(ConnectionString);
                try
                {
                    #region Parameters
                    SqlParameter tbPaymentNotificationTypeParmeter = new SqlParameter()
                    {
                        ParameterName = "@tbPaymentNotificationType",
                        Value = this.ToDataTable(paymentNotifications),
                    };

                   

                    #endregion

                    var ResultTable = SQLHelpers.Instance.ExecuteSP("Usp_MergePaymentNotification", tbPaymentNotificationTypeParmeter);

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
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return isSavedSuccessfully;
        }


        public List<Models.Local.DB.PaymentHeader> FetchPaymentDetails(string ReservationNameID,bool? isActive, string ConnectionString)
        {
            try
            {
                SQLHelpers.Instance.SetConnectionString(ConnectionString);
                #region Parameters
                SqlParameter ReservationNameIDParmeter = new SqlParameter()
                {
                    ParameterName = "@ReservationNameID",
                    Value = ReservationNameID,
                    SqlDbType = SqlDbType.VarChar
                };

                SqlParameter isActiveParmeter = new SqlParameter()
                {
                    ParameterName = "@IsActive",
                    Value = isActive.Value,
                    SqlDbType = SqlDbType.Bit
                };
                #endregion
                var ResultTable = SQLHelpers.Instance.ExecuteSP("Usp_FetchPaymentTransactionsForCloud", ReservationNameIDParmeter, isActiveParmeter);
                return this.DataTableToList<Models.Local.DB.PaymentHeader>(ResultTable); 
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<Models.Local.DB.PaymentTransactionDetails> FetchPaymentTransactionDetails(string ReservationNameID, bool? isActive, string ConnectionString)
        {
            try
            {
                SQLHelpers.Instance.SetConnectionString(ConnectionString);
                #region Parameters
                SqlParameter ReservationNameIDParmeter = new SqlParameter()
                {
                    ParameterName = "@ReservationNameID",
                    Value = ReservationNameID,
                    SqlDbType = SqlDbType.VarChar
                };


                SqlParameter isActiveParmeter = isActive != null ? new SqlParameter()
                {
                    ParameterName = "@IsActive",
                    Value = isActive.Value,
                    SqlDbType = SqlDbType.Bit
                } : new SqlParameter()
                {
                    ParameterName = "@IsActive",
                    Value =  null,
                    SqlDbType = SqlDbType.Bit
                };
                #endregion
                var ResultTable = SQLHelpers.Instance.ExecuteSP("Usp_FetchPaymentTransactionsForPortal_Test", ReservationNameIDParmeter, isActiveParmeter);
                return this.DataTableToList<Models.Local.DB.PaymentTransactionDetails>(ResultTable);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<string> FetchNonModefiedReservationList(string ConnectionString,int BufferDays)
        {
            try
            {

                SqlParameter isActiveParmeter = new SqlParameter()
                {
                    ParameterName = "@BufferDays",
                    Value = BufferDays,
                    SqlDbType = SqlDbType.Int
                };
                SQLHelpers.Instance.SetConnectionString(ConnectionString);

                var ResultTable = SQLHelpers.Instance.ExecuteSP("Usp_fetchNonModifiedReservations");
                return ResultTable.AsEnumerable()
                           .Select(r => r.Field<string>("ReservationNumber"))
                           .ToList();//this.DataTableToList<string>(ResultTable);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<Models.Local.DB.BISummaryArrivals> FetchBISummaryArrivals(string ConnectionString)
        {
            try
            {
                SQLHelpers.Instance.SetConnectionString(ConnectionString);
                
                var ResultTable = SQLHelpers.Instance.ExecuteSP("Usp_FetchDataForBIReportsArrivals");
                return this.DataTableToList<Models.Local.DB.BISummaryArrivals>(ResultTable);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<Models.Local.DB.BINationalityWiseSummaryArrivals> FetchBINationalityWiseSummaryArrivals(string ConnectionString)
        {
            try
            {
                SQLHelpers.Instance.SetConnectionString(ConnectionString);
                var ResultTable = SQLHelpers.Instance.ExecuteSP("Usp_FetchDataForBINationalityWiseSummaryArrivals");
                return this.DataTableToList<Models.Local.DB.BINationalityWiseSummaryArrivals>(ResultTable);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<Models.Local.PaymentHeaders> FetchPaymentDetailsByExpiry(string ConnectionString)
        {
            try
            {
                SQLHelpers.Instance.SetConnectionString(ConnectionString);
                var ResultTable = SQLHelpers.Instance.ExecuteSP("Usp_FetchPaymentHeaderPreauthupdate");
                return this.DataTableToList<Models.Local.PaymentHeaders>(ResultTable);
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }
        public bool UpdatePaymentHeaderData(Models.Local.PaymentHeader paymentHeader, string ConnectionString)
        {
            try
            {
                SQLHelpers.Instance.SetConnectionString(ConnectionString);



                List<SqlParameter> sqlParameters = new List<SqlParameter>();
                sqlParameters.Add(new SqlParameter() { ParameterName = "@TransactionID", Value = paymentHeader.TransactionID, SqlDbType = SqlDbType.VarChar });
                sqlParameters.Add(new SqlParameter() { ParameterName = "@ResultCode", Value = paymentHeader.ResultCode, SqlDbType = SqlDbType.VarChar });
                sqlParameters.Add(new SqlParameter() { ParameterName = "@ResponseMessage", Value = paymentHeader.ResponseMessage, SqlDbType = SqlDbType.VarChar });
                sqlParameters.Add(new SqlParameter() { ParameterName = "@IsActive", Value = paymentHeader.IsActive, SqlDbType = SqlDbType.Bit });
                sqlParameters.Add(new SqlParameter() { ParameterName = "@TransactionType", Value = paymentHeader.TransactionType, SqlDbType = SqlDbType.VarChar });
                sqlParameters.Add(new SqlParameter() { ParameterName = "@Amount", Value = paymentHeader.Amount, SqlDbType = SqlDbType.Decimal });
                sqlParameters.Add(new SqlParameter() { ParameterName = "@ReservationNumber", Value = paymentHeader.ReservationNumber, SqlDbType = SqlDbType.VarChar });



                var ProfilesDt = SQLHelpers.Instance.ExecuteSP("Usp_UpdatePaymentHeader", sqlParameters);

                if (ProfilesDt != null && ProfilesDt.Rows.Count > 0)
                {

                    if (ProfilesDt.Rows[0]["Result"].ToString() != "0")
                    {

                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return false;
        }
        public bool InsertPaymentData(Models.Local.PaymnetTopUP paymentDetailResponseModel, string ConnectionString)
        {
            try
            {
                SQLHelpers.Instance.SetConnectionString(ConnectionString);
                List<SqlParameter> sqlParameters = new List<SqlParameter>();
                string maskedCarNumber = paymentDetailResponseModel.paymentResponse.MaskCardNumber != null ? paymentDetailResponseModel.paymentResponse.MaskCardNumber : "";
                string fundingSource = paymentDetailResponseModel.paymentResponse.FundingSource != null ? paymentDetailResponseModel.paymentResponse.FundingSource : "";

                string amount = paymentDetailResponseModel.paymentResponse.Amount.Value.ToString("0.00");

                sqlParameters.Add(new SqlParameter() { ParameterName = "@TransactionID", Value = paymentDetailResponseModel.TransactionID, SqlDbType = SqlDbType.VarChar });
                sqlParameters.Add(new SqlParameter() { ParameterName = "@ReservationNumber", Value = paymentDetailResponseModel.ReservationNumber, SqlDbType = SqlDbType.VarChar });
                sqlParameters.Add(new SqlParameter() { ParameterName = "@ReservationNameID", Value = paymentDetailResponseModel.ReservationNameID, SqlDbType = SqlDbType.VarChar });
                sqlParameters.Add(new SqlParameter() { ParameterName = "@MaskedCardNumber", Value = maskedCarNumber, SqlDbType = SqlDbType.VarChar });
                sqlParameters.Add(new SqlParameter() { ParameterName = "@ExpiryDate", Value = paymentDetailResponseModel.paymentResponse.CardExpiryDate, SqlDbType = SqlDbType.VarChar });
                sqlParameters.Add(new SqlParameter() { ParameterName = "@FundingSource", Value = fundingSource, SqlDbType = SqlDbType.VarChar });
                sqlParameters.Add(new SqlParameter() { ParameterName = "@AuthorisationCode", Value = paymentDetailResponseModel.paymentResponse.AuthCode, SqlDbType = SqlDbType.VarChar });
                sqlParameters.Add(new SqlParameter() { ParameterName = "@Amount", Value = amount, SqlDbType = SqlDbType.VarChar });
                sqlParameters.Add(new SqlParameter() { ParameterName = "@Currency", Value = paymentDetailResponseModel.paymentResponse.Currency, SqlDbType = SqlDbType.VarChar });
                sqlParameters.Add(new SqlParameter() { ParameterName = "@RecurringIdentifier", Value = paymentDetailResponseModel.paymentResponse.PaymentToken, SqlDbType = SqlDbType.VarChar });
                sqlParameters.Add(new SqlParameter() { ParameterName = "@pspReferenceNumber", Value = paymentDetailResponseModel.paymentResponse.PspReference, SqlDbType = SqlDbType.VarChar });
                string parentPspReference = paymentDetailResponseModel.paymentResponse.PspReference;

                if (!string.IsNullOrEmpty(paymentDetailResponseModel.paymentResponse.ParentPSPReferece))
                {
                    parentPspReference = paymentDetailResponseModel.paymentResponse.ParentPSPReferece;
                }

                sqlParameters.Add(new SqlParameter() { ParameterName = "@ParentPspRefereceNumber", Value = parentPspReference, SqlDbType = SqlDbType.VarChar });
                sqlParameters.Add(new SqlParameter() { ParameterName = "@TransactionType", Value = paymentDetailResponseModel.transactionType, SqlDbType = SqlDbType.VarChar });
                sqlParameters.Add(new SqlParameter() { ParameterName = "@ResultCode", Value = paymentDetailResponseModel.paymentResponse.ResultCode, SqlDbType = SqlDbType.VarChar });
                sqlParameters.Add(new SqlParameter() { ParameterName = "@ResponseMessage", Value = paymentDetailResponseModel.paymentResponse.RefusalReason, SqlDbType = SqlDbType.VarChar });
                sqlParameters.Add(new SqlParameter() { ParameterName = "@CardType ", Value = paymentDetailResponseModel.paymentResponse.CardType, SqlDbType = SqlDbType.VarChar });
                if (paymentDetailResponseModel.transactionType.ToUpper() == "SALE")
                {
                    sqlParameters.Add(new SqlParameter() { ParameterName = "@IsActive ", Value = false, SqlDbType = SqlDbType.Bit });
                }
                else
                {
                    sqlParameters.Add(new SqlParameter() { ParameterName = "@IsActive ", Value = true, SqlDbType = SqlDbType.Bit });
                }
                sqlParameters.Add(new SqlParameter() { ParameterName = "@UserName ", Value = paymentDetailResponseModel.UserName, SqlDbType = SqlDbType.VarChar });
                sqlParameters.Add(new SqlParameter() { ParameterName = "@ClientIPAddress ", Value = paymentDetailResponseModel.ClientIPAddress, SqlDbType = SqlDbType.VarChar });

                DataTable dataTable = new DataTable();
                dataTable.Columns.Add("TransactionID", typeof(string));
                dataTable.Columns.Add("KeyHeader", typeof(string));
                dataTable.Columns.Add("KeyValue", typeof(string));



                if (paymentDetailResponseModel.paymentResponse.additionalInfos != null)
                {
                    foreach (var item in paymentDetailResponseModel.paymentResponse.additionalInfos)
                    {
                        DataRow drow1 = dataTable.NewRow();
                        drow1["TransactionID"] = paymentDetailResponseModel.TransactionID;
                        drow1["KeyHeader"] = item.key;
                        drow1["KeyValue"] = item.value;

                        dataTable.Rows.Add(drow1);
                    }
                }

                sqlParameters.Add(new SqlParameter() { ParameterName = "@TbAdditionalPaymentInfoType", Value = dataTable });



                var ProfilesDt = SQLHelpers.Instance.ExecuteSP("Usp_InsertPaymentDetails", sqlParameters);

                if (ProfilesDt != null && ProfilesDt.Rows.Count > 0)
                {
                    if (ProfilesDt.Rows[0]["Result"].ToString() != "0")
                    {

                        return true;
                    }
                    else
                    {

                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
            }
            return false;
        }

        public bool UpdatePaymentHeaderIsActive(string ConnectionString)
        {
            SQLHelpers.Instance.SetConnectionString(ConnectionString);
            var PhActiveFalse = SQLHelpers.Instance.ExecuteSP("Usp_UpdateIsActivePaymentHeader");

            if (PhActiveFalse != null && PhActiveFalse.Rows.Count > 0)
            {
                return true;
            }
            else
            {
                return false;
            }

        }
        public List<Models.Local.DB.ReservationCompareStatus> ReservationDetailCompare(Models.Local.DB.RequestReservationDetail reservationListTypeModels, string ConnectionString)
        {
            SQLHelpers.Instance.SetConnectionString(ConnectionString);
            //return BulkUpdateReservationToDB(this.ToDataTable(reservationListTypeModels));
            bool isSavedSuccessfully = false;
            try
            {
                List<SqlParameter> sqlParameters = new List<SqlParameter>();
                sqlParameters.Add(new SqlParameter() { ParameterName = "@ReservationNameID", Value = reservationListTypeModels.ReservationNameID, SqlDbType = SqlDbType.VarChar });

                var ResultTable = SQLHelpers.Instance.ExecuteSP("Usp_GetReservationStatusComapre", sqlParameters);
                if (ResultTable != null)
                    return this.DataTableToList<Models.Local.DB.ReservationCompareStatus>(ResultTable);

                else
                    return null;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public List<Models.Local.DB.ReservationDueoutAmountCompare> ReservationAmountCompare(Models.Local.DB.RequestReservationDetail RequestReservationDetail, string ConnectionString)
        {
            SQLHelpers.Instance.SetConnectionString(ConnectionString);
            //return BulkUpdateReservationToDB(this.ToDataTable(reservationListTypeModels));
            bool isSavedSuccessfully = false;
            try
            {
                List<SqlParameter> sqlParameters = new List<SqlParameter>();
                sqlParameters.Add(new SqlParameter() { ParameterName = "@ReservationNameID", Value = RequestReservationDetail.ReservationNameID, SqlDbType = SqlDbType.VarChar });

                var ResultTable = SQLHelpers.Instance.ExecuteSP("Usp_GetReservationAmountList", sqlParameters);
                if (ResultTable != null)
                    return this.DataTableToList<Models.Local.DB.ReservationDueoutAmountCompare>(ResultTable);

                else
                    return null;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        public List<Models.Local.DB.PaymentTransactionDetails> FetchPaymentTransactionDetailsByPaging(Models.Local.DB.PaymentListRequestModel paymentList, string ConnectionString)
        {
            try
            {
                SQLHelpers.Instance.SetConnectionString(ConnectionString);
                #region Parameters
                SqlParameter reservationNoParameter = new SqlParameter()
                {
                    ParameterName = "@ReservationNumber",
                    Value = paymentList.ReservationNumber,
                    SqlDbType = SqlDbType.VarChar
                };
                SqlParameter pageNumberParameter = new SqlParameter()
                {
                    ParameterName = "@PageNumber",
                    Value = paymentList.PageNumber,
                    SqlDbType = SqlDbType.Int
                };
                SqlParameter pageSizeParameter = new SqlParameter()
                {
                    ParameterName = "@PageSize",
                    Value = paymentList.Length,
                    SqlDbType = SqlDbType.Int
                };
                SqlParameter searchParameter = new SqlParameter()
                {
                    ParameterName = "@search",
                    Value = paymentList.FilterBy,
                    SqlDbType = SqlDbType.NVarChar
                };
                SqlParameter sortParameter = new SqlParameter()
                {
                    ParameterName = "@Sort",
                    Value = paymentList.SortingOrder,
                    SqlDbType = SqlDbType.VarChar
                };
                SqlParameter sortByParameter = new SqlParameter()
                {
                    ParameterName = "@SortBy",
                    Value = paymentList.SortBy,
                    SqlDbType = SqlDbType.NVarChar
                };
                SqlParameter activeParameter = new SqlParameter()
                {
                    ParameterName = "@IsActive",
                    Value = true,
                    SqlDbType = SqlDbType.Bit
                };
                #endregion

                var ResultTable = SQLHelpers.Instance.ExecuteSP("Usp_FetchPaymentTransactionsForPortal_Test_WithPaging", reservationNoParameter, pageNumberParameter, pageSizeParameter, searchParameter, sortParameter, sortByParameter, activeParameter);
                return this.DataTableToList<Models.Local.DB.PaymentTransactionDetails>(ResultTable);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
