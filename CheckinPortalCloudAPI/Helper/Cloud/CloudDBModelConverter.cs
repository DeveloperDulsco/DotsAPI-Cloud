using CheckinPortalCloudAPI.Models.Cloud.DB;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace CheckinPortalCloudAPI.Helper.Cloud
{
    public class CloudDBModelConverter
    {
        public Models.Cloud.DB.OperaReservationDataTableModel getOperaReservationDataTable(Models.Cloud.OperaReservation operaReservation)
        {
            try
            {
                Models.Cloud.DB.OperaReservationDataTableModel operaReservationDataTable = new Models.Cloud.DB.OperaReservationDataTableModel();
                operaReservationDataTable.Adultcount = operaReservation.Adults != null ? operaReservation.Adults.Value : 0;
                operaReservationDataTable.ArrivalDate = operaReservation.ArrivalDate != null ? operaReservation.ArrivalDate.Value : new DateTime(1900,01,01);
                operaReservationDataTable.Childcount = operaReservation.Child != null ? operaReservation.Child.Value : 0;
                operaReservationDataTable.DepartureDate = operaReservation.DepartureDate != null ? operaReservation.DepartureDate.Value : new DateTime(1900, 01, 01);
                operaReservationDataTable.ETA = operaReservation.ExpectedArrivalTime != null ? operaReservation.ExpectedArrivalTime.Value.Equals(DateTime.MinValue) ? new DateTime(1900, 01, 01) : operaReservation.ExpectedArrivalTime.Value : new DateTime(1900, 01, 01);
                operaReservationDataTable.IsCardDetailPresent = false;
                operaReservationDataTable.IsDepositAvailable = operaReservation.IsDepositAvailable != null? operaReservation.IsDepositAvailable.Value : false;
                operaReservationDataTable.IsSaavyPaid = false;
                operaReservationDataTable.MembershipNo = operaReservation.GuestProfiles[0].MembershipNumber;
                operaReservationDataTable.MembershipType = operaReservation.GuestProfiles[0].MembershipType;
                operaReservationDataTable.ReservationNameID = operaReservation.ReservationNameID;
                operaReservationDataTable.ReservationNumber = operaReservation.ReservationNumber;
                operaReservationDataTable.RoomType = operaReservation.RoomDetails != null ? operaReservation.RoomDetails.RTC : null;
                operaReservationDataTable.RoomTypeDescription = operaReservation.RoomDetails!= null ? 
                                                                (string.IsNullOrEmpty(ConfigurationManager.AppSettings["IsRoomTypeShortDescriptionEnabled"]) ? 
                                                                    operaReservation.RoomDetails.RTCDescription : 
                                                                    (Convert.ToBoolean(ConfigurationManager.AppSettings["IsRoomTypeShortDescriptionEnabled"]) == false ? operaReservation.RoomDetails.RTCDescription: operaReservation.RoomDetails.RTCShortDescription)) 
                                                                : null;
                operaReservationDataTable.TotalAmount = operaReservation.TotalAmount != null ? operaReservation.TotalAmount.Value : 0;
                operaReservationDataTable.TotalTax = operaReservation.TotalTax != null ? operaReservation.TotalTax.Value : 0;
                operaReservationDataTable.BalanceAmount = operaReservation.CurrentBalance;
                operaReservationDataTable.IsCheckOutFlag = !string.IsNullOrEmpty(operaReservation.ComputedReservationStatus) ? (operaReservation.ComputedReservationStatus.Equals("DUEOUT") ? true : false) : false; 
                operaReservationDataTable.PaidAmount = (operaReservation.DepositDetail != null && operaReservation.DepositDetail.Count > 0) ? operaReservation.DepositDetail[0].Amount : 0;
                operaReservationDataTable.AverageRoomRate = operaReservation.RateDetails != null ? (operaReservation.RateDetails.RateAmount != null ? operaReservation.RateDetails.RateAmount.Value : 0):0;
                operaReservationDataTable.StatusDescription = operaReservation.ReservationStatus;
                operaReservationDataTable.ReservationSource = operaReservation.ReservationType;
                operaReservationDataTable.IsBreakFastAvailable = operaReservation.IsBreakFastAvailable != null ? operaReservation.IsBreakFastAvailable.Value : false;
                return operaReservationDataTable;
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        public List<Models.Cloud.DB.PackageMasterModel> GetPackageMasterModelsFromDataTable(List<Models.Cloud.DB.PackageMasterDataTableModel> packageMasterDataTables)
        {
            try
            {
                List<Models.Cloud.DB.PackageMasterModel> PackageMasterModel = new List<PackageMasterModel>();
                if (packageMasterDataTables != null)
                {
                    foreach (Models.Cloud.DB.PackageMasterDataTableModel masterDataTableModel in packageMasterDataTables)
                    {
                        if (masterDataTableModel.UniquePackageFlag == 1)
                        {
                            Models.Cloud.DB.PackageMasterModel packageMaster = new PackageMasterModel();
                            packageMaster.isActive = masterDataTableModel.isActive;
                            packageMaster.PackageAmount = masterDataTableModel.PackageAmount;
                            packageMaster.PackageCode = masterDataTableModel.PackageCode;
                            packageMaster.PackageDesc = masterDataTableModel.PackageDesc;
                            packageMaster.PackageID = masterDataTableModel.PackageID;
                            packageMaster.PackageImage = masterDataTableModel.PackageImage != null ? Convert.ToBase64String(masterDataTableModel.PackageImage) : null;
                            packageMaster.PackageName = masterDataTableModel.PackageName;
                            packageMaster.isRoomUpsell = masterDataTableModel.IsRoomUpsell;
                            packageMaster.Units = masterDataTableModel.Units;
                            if (masterDataTableModel.RoomTypeCode != null)
                            {
                                packageMaster.RoomTypeCode = new List<RoomTypeCode>() { new RoomTypeCode()
                                {
                                    RoomCode = masterDataTableModel.RoomTypeCode
                                }};
                            }
                            
                            PackageMasterModel.Add(packageMaster);
                        }
                        else
                        {
                            int index = PackageMasterModel.FindIndex(a => a.PackageID == masterDataTableModel.PackageID);
                            if (index >= 0)
                            {
                                
                                if (PackageMasterModel[index].RoomTypeCode != null && PackageMasterModel[index].RoomTypeCode.Count > 0)
                                {
                                    PackageMasterModel[index].RoomTypeCode.Add(new RoomTypeCode()
                                    {
                                        RoomCode = masterDataTableModel.RoomTypeCode
                                    });
                                }
                                
                            }
                        }
                    }
                }
                return PackageMasterModel;
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        public Models.Cloud.DB.ReservationDocumentsDataTableModel GetReservationDocumentsDataTable(Models.Cloud.OperaReservation operaReservation,string documentType)
        {
            try
            {
                Models.Cloud.DB.ReservationDocumentsDataTableModel reservationDocumentsData = new Models.Cloud.DB.ReservationDocumentsDataTableModel();
                reservationDocumentsData.ReservationNameID = operaReservation.ReservationNameID;
                reservationDocumentsData.Document = operaReservation.reservationDocument != null ? Convert.FromBase64String(operaReservation.reservationDocument.DocumentBase64) : null;
                reservationDocumentsData.DocumentType = documentType;
                return reservationDocumentsData;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public Models.Cloud.DB.ProfileDocumentDetailsModel getProfileDocumentDetailsDataTable(Models.Cloud.DB.ProfileDocuments profileDocument)
        {
            try
            {
                Models.Cloud.DB.ProfileDocumentDetailsModel profileDocumentDataTable = new Models.Cloud.DB.ProfileDocumentDetailsModel();
                profileDocumentDataTable.DocumentImage1 = profileDocument.DocumentImage1;
                profileDocumentDataTable.DocumentImage2 = profileDocument.DocumentImage2;
                profileDocumentDataTable.DocumentImage3 = profileDocument.DocumentImage3;
                profileDocumentDataTable.DocumentNumber = profileDocument.DocumentNumber;
                profileDocumentDataTable.DocumentTypeCode = profileDocument.DocumentTypeCode;
                profileDocumentDataTable.ExpiryDate = profileDocument.ExpiryDate != null ? profileDocument.ExpiryDate.Value : new DateTime(1900,01,01);
                //profileDocumentDataTable.CloudProfileDetailID = profileDocument.CloudProfileDetailID;
                profileDocumentDataTable.ReservationNameID = profileDocument.ReservationNameID;
                profileDocumentDataTable.ProfileID = profileDocument.ProfileID;
                
                return profileDocumentDataTable;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public Models.Cloud.DB.ProfileDetailsDataTableModel getprofileDetailsDataTable(Models.Cloud.GuestProfile guestProfile,string ReservationNameID)
        {
            try
            {

                Models.Cloud.DB.ProfileDetailsDataTableModel profileDetailsDataTable = new Models.Cloud.DB.ProfileDetailsDataTableModel();
                profileDetailsDataTable.AddressLine1 = guestProfile.Address != null && guestProfile.Address.Count > 0 ? guestProfile.Address[0].address1 : null;
                profileDetailsDataTable.AddressLine2 = guestProfile.Address != null && guestProfile.Address.Count > 0 ? guestProfile.Address[0].address2 : null;
                if (guestProfile.BirthDate != null)
                {
                    if (guestProfile.BirthDate.Contains("0001-01-01"))
                        guestProfile.BirthDate = "1900-01-01";
                }
                profileDetailsDataTable.BirthDate = guestProfile.BirthDate != null ? DateTime.ParseExact(guestProfile.BirthDate, "yyyy-MM-dd",null) : new DateTime(1900, 01, 01);
                profileDetailsDataTable.City = guestProfile.Address != null && guestProfile.Address.Count > 0 ? guestProfile.Address[0].city : null;
                profileDetailsDataTable.CountryCode = guestProfile.Address != null && guestProfile.Address.Count > 0 ? guestProfile.Address[0].country : null;
                profileDetailsDataTable.FirstName = guestProfile.FirstName;
                profileDetailsDataTable.MiddleName = guestProfile.MiddleName;
                profileDetailsDataTable.LastName = guestProfile.LastName;
                profileDetailsDataTable.Nationality = guestProfile.Nationality;
                profileDetailsDataTable.Phone = guestProfile.Phones != null && guestProfile.Phones.Count > 0 ? guestProfile.Phones[0].PhoneNumber : null;
                profileDetailsDataTable.PostalCode = guestProfile.Address != null && guestProfile.Address.Count > 0 ? guestProfile.Address[0].zip : null;
                profileDetailsDataTable.ProfileID = guestProfile.PmsProfileID;
                profileDetailsDataTable.ReservationNameID = ReservationNameID;
                profileDetailsDataTable.StateCode = guestProfile.Address != null && guestProfile.Address.Count > 0 ? guestProfile.Address[0].state : null;
                //profileDetailsDataTable.Email = guestProfile.Email != null && guestProfile.Email.Count > 0 ? guestProfile.Email[0].email : null;

                if (guestProfile.Email != null)
                {
                    //bool isPrimaryPresent = false;
                    foreach (Models.Cloud.Email email in guestProfile.Email)
                    {
                        if (email.primary != null && email.primary.Value)
                        {
                            //isPrimaryPresent = true;
                            profileDetailsDataTable.Email = email.email;
                        }
                    }

                }

                return profileDetailsDataTable;
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }
    }
}