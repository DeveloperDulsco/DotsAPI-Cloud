using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CheckinPortalCloudAPI.Helper.KIOSK
{
    public class KIOSKDBModelConverter
    {
        public Models.KIOSK.DB.ReservationDataTableModel getOperaReservationDataTable(Models.KIOSK.ReservationModel operaReservation)
        {
            try
            {
                Models.KIOSK.DB.ReservationDataTableModel operaReservationDataTable = new Models.KIOSK.DB.ReservationDataTableModel();
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
                operaReservationDataTable.MembershipNo = (operaReservation.GuestProfiles != null && operaReservation.GuestProfiles.Count >= 0) ? operaReservation.GuestProfiles[0].MembershipNumber : "";
                operaReservationDataTable.MembershipType = (operaReservation.GuestProfiles != null && operaReservation.GuestProfiles.Count >= 0) ? operaReservation.GuestProfiles[0].MembershipType : "";
                operaReservationDataTable.ReservationNameID = operaReservation.ReservationNameID;
                operaReservationDataTable.ReservationNumber = operaReservation.ReservationNumber;
                operaReservationDataTable.RoomType = operaReservation.RoomDetails != null ? operaReservation.RoomDetails.RTC : null;
                operaReservationDataTable.RoomTypeDescription = operaReservation.RoomDetails != null ? operaReservation.RoomDetails.RTCDescription : null;
                operaReservationDataTable.IsMemberShipEnrolled = operaReservation.IsMemberShipEnrolled;
                operaReservationDataTable.FlightNo = operaReservation.FlightNo;
                operaReservationDataTable.PaidAmount = 0;
                operaReservationDataTable.EmailSent = operaReservation.IsEmailSend != null ? operaReservation.IsEmailSend.Value : b;
                operaReservationDataTable.BalanceAmount = operaReservation.CurrentBalance;
                //operaReservationDataTable.IsCloud = operaReservation.IsCloud != null ? operaReservation.IsCloud.Value : false;
                operaReservationDataTable.TotalAmount = operaReservation.TotalAmount != null ? operaReservation.TotalAmount.Value : 0;
                operaReservationDataTable.TotalTax = operaReservation.TotalTax != null ? operaReservation.TotalTax.Value : 0;
                operaReservationDataTable.AverageRoomRate = operaReservation.RateDetails != null ? (operaReservation.RateDetails.RateAmount != null ? operaReservation.RateDetails.RateAmount.Value : 0) : 0;
                operaReservationDataTable.RoomNumber = operaReservation.RoomDetails != null ? operaReservation.RoomDetails.RoomNumber : null;
                operaReservationDataTable.StatusDescription = operaReservation.ReservationStatus;
                operaReservationDataTable.IsBreakFastAvailable = operaReservation.IsBreakFastAvailable != null ? operaReservation.IsBreakFastAvailable.Value : false;
                operaReservationDataTable.ShareFlag = operaReservation.ShareFlag != null ? operaReservation.ShareFlag.Value : false;
                operaReservationDataTable.ShareID = operaReservation.ShareID;
                operaReservationDataTable.ExternalRefNumber = operaReservation.ExternalRefNumber;
                operaReservationDataTable.CSRNumber = operaReservation.CRSNumber;
                operaReservationDataTable.ReservationStatus = operaReservation.ReservationStatus;
                
                return operaReservationDataTable;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public Models.KIOSK.DB.ProfileDataTableModel getprofileDetailsDataTable(Models.KIOSK.GuestProfile guestProfile, string reservationNameID)
        {
            try
            {
                Models.KIOSK.DB.ProfileDataTableModel profileDetailsDataTable = new Models.KIOSK.DB.ProfileDataTableModel();
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
                    foreach (Models.KIOSK.Email email in guestProfile.Email)
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
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}