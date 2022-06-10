using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CheckinPortalCloudAPI.Helper.Local
{
    public class LocalDBModelConverter
    {
        public Models.Local.DB.OperaReservationDataTableModel getOperaReservationDataTable(Models.Local.OperaReservation operaReservation)
        {
            try
            {
                Models.Local.DB.OperaReservationDataTableModel operaReservationDataTable = new Models.Local.DB.OperaReservationDataTableModel();
                operaReservationDataTable.Adultcount = operaReservation.Adults != null ? operaReservation.Adults.Value : 0;
                operaReservationDataTable.ArrivalDate = operaReservation.ArrivalDate != null ? operaReservation.ArrivalDate.Value : new DateTime(1900, 01, 01);
                operaReservationDataTable.Childcount = operaReservation.Child != null ? operaReservation.Child.Value : 0;
                operaReservationDataTable.DepartureDate = operaReservation.DepartureDate != null ? operaReservation.DepartureDate.Value : new DateTime(1900, 01, 01);
                operaReservationDataTable.ETA = operaReservation.ExpectedArrivalTime != null ? operaReservation.ExpectedArrivalTime.Value.Equals(DateTime.MinValue) ? new DateTime(1900, 01, 01) : operaReservation.ExpectedArrivalTime.Value : new DateTime(1900, 01, 01);
                operaReservationDataTable.IsCardDetailPresent = false;
                Nullable<bool> b = null;
                operaReservationDataTable.IsDepositAvailable = operaReservation.DepositDetail != null && operaReservation.DepositDetail.Count > 0 ? true : b;
                operaReservationDataTable.IsSaavyPaid = false;
                operaReservationDataTable.ReservationSource = operaReservation.ReservationType;
                operaReservationDataTable.MembershipNo = (operaReservation.GuestProfiles != null && operaReservation.GuestProfiles.Count >= 0) ? operaReservation.GuestProfiles[0]. MembershipNumber : "";
                operaReservationDataTable.MembershipType = (operaReservation.GuestProfiles != null && operaReservation.GuestProfiles.Count >= 0) ? operaReservation.GuestProfiles[0].MembershipType : "";
                operaReservationDataTable.ReservationNameID = operaReservation.ReservationNameID;
                operaReservationDataTable.ReservationNumber = operaReservation.ReservationNumber;
                operaReservationDataTable.RoomType = operaReservation.RoomDetails != null ? operaReservation.RoomDetails.RoomType : null;
                operaReservationDataTable.RoomTypeDescription = operaReservation.RoomDetails != null ? operaReservation.RoomDetails.RTCDescription : null;
                operaReservationDataTable.IsMemberShipEnrolled = operaReservation.IsMemberShipEnrolled;
                operaReservationDataTable.FlightNo = operaReservation.FlightNo;
                operaReservationDataTable.PaidAmount = 0;
                operaReservationDataTable.EmailSent = operaReservation.IsEmailSend != null ? operaReservation.IsEmailSend.Value : b;
                operaReservationDataTable.BalanceAmount = operaReservation.CurrentBalance;
                //operaReservationDataTable.IsCloud = operaReservation.IsCloud != null ? operaReservation.IsCloud.Value : false;
                operaReservationDataTable.TotalAmount = operaReservation.TotalAmount != null ? operaReservation.TotalAmount.Value : 0;
                operaReservationDataTable.TotalTax = operaReservation.TotalTax != null ? operaReservation.TotalTax.Value:0;
                operaReservationDataTable.AverageRoomRate = operaReservation.RateDetails != null ? (operaReservation.RateDetails.RateAmount != null ? operaReservation.RateDetails.RateAmount.Value : 0):0;
                operaReservationDataTable.RoomNumber = operaReservation.RoomDetails != null ? operaReservation.RoomDetails.RoomNumber : null;
                operaReservationDataTable.StatusDescription = operaReservation.ReservationStatus;
                operaReservationDataTable.IsBreakFastAvailable = operaReservation.IsBreakFastAvailable != null ? operaReservation.IsBreakFastAvailable.Value : false;
                return operaReservationDataTable;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public Models.Local.DB.ProfileDocumentDetailsModel getProfileDocumentDetailsDataTable(Models.Local.DB.ProfileDocuments profileDocument)
        {
            try
            {
                Models.Local.DB.ProfileDocumentDetailsModel profileDocumentDataTable = new Models.Local.DB.ProfileDocumentDetailsModel();
                profileDocumentDataTable.DocumentImage1 = profileDocument.DocumentImage1;
                profileDocumentDataTable.DocumentImage2 = profileDocument.DocumentImage2;
                profileDocumentDataTable.DocumentImage3 = profileDocument.DocumentImage3;
                profileDocumentDataTable.DocumentNumber = profileDocument.DocumentNumber;
                profileDocumentDataTable.IssueCountry = profileDocument.IssueCountry;
                profileDocumentDataTable.DocumentTypeCode = profileDocument.DocumentTypeCode;
                profileDocumentDataTable.ExpiryDate = profileDocument.ExpiryDate != null ? profileDocument.ExpiryDate.Value : new DateTime(1900, 01, 01);
                profileDocumentDataTable.IssueDate = profileDocument.IssueDate != null ? profileDocument.IssueDate.Value : new DateTime(1900, 01, 01);
                profileDocumentDataTable.CloudProfileDetailID = profileDocument.CloudProfileDetailID;
                profileDocumentDataTable.ReservationNameID = profileDocument.ReservationNameID;
                profileDocumentDataTable.ProfileID = profileDocument.ProfileID;
                profileDocumentDataTable.IssueCountry = profileDocument.IssueCountry;
                profileDocumentDataTable.FaceImage = profileDocument.FaceImage;
                return profileDocumentDataTable;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public Models.Local.DB.ProfileDetailsDataTableModel getprofileDetailsDataTable(Models.Local.GuestProfile guestProfile,string reservationNameID)
        {
            try
            {
                Models.Local.DB.ProfileDetailsDataTableModel profileDetailsDataTable = new Models.Local.DB.ProfileDetailsDataTableModel();
                profileDetailsDataTable.AddressLine1 = guestProfile.Address != null && guestProfile.Address.Count > 0 ? guestProfile.Address[0].address1 : null;
                profileDetailsDataTable.AddressLine2 = guestProfile.Address != null && guestProfile.Address.Count > 0 ? guestProfile.Address[0].address2 : null;
                if (guestProfile.BirthDate != null)
                {
                    if (guestProfile.BirthDate.Contains("0001-01-01"))
                        guestProfile.BirthDate = "1900-01-01";
                }
                profileDetailsDataTable.BirthDate = guestProfile.BirthDate != null ? DateTime.ParseExact(guestProfile.BirthDate, "yyyy-MM-dd", null) : new DateTime(1900, 01, 01);


                profileDetailsDataTable.City = guestProfile.Address != null && guestProfile.Address.Count > 0 ? guestProfile.Address[0].city : null;
                profileDetailsDataTable.CountryCode = guestProfile.Address != null && guestProfile.Address.Count > 0 ? guestProfile.Address[0].country : null;
                profileDetailsDataTable.FirstName = guestProfile.FirstName;
                profileDetailsDataTable.MiddleName = guestProfile.MiddleName;
                profileDetailsDataTable.Gender = guestProfile.Gender;
                profileDetailsDataTable.LastName = guestProfile.LastName;
                profileDetailsDataTable.Nationality = guestProfile.Nationality;
                profileDetailsDataTable.Phone = guestProfile.Phones != null && guestProfile.Phones.Count > 0 ? guestProfile.Phones[0].PhoneNumber : null;
                profileDetailsDataTable.PostalCode = guestProfile.Address != null && guestProfile.Address.Count > 0 ? guestProfile.Address[0].zip : null;
                profileDetailsDataTable.ProfileID = guestProfile.PmsProfileID;
                profileDetailsDataTable.ReservationNameID = reservationNameID;
                profileDetailsDataTable.StateCode = guestProfile.Address != null && guestProfile.Address.Count > 0 ? guestProfile.Address[0].state : null;
                profileDetailsDataTable.CloudProfileDetailID = guestProfile.CloudProfileDetailID;
                if (guestProfile.Email != null)
                {
                    //bool isPrimaryPresent = false;
                    foreach(Models.Local.Email email in guestProfile.Email)
                    {
                        if(email.primary != null && email.primary.Value)
                        {
                            //isPrimaryPresent = true;
                            profileDetailsDataTable.Email = email.email;
                        }
                    }
                    
                }
                
                return profileDetailsDataTable;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

    }
}