using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;


namespace CheckinPortalCloudAPI.Controllers
{
    public class LocalServiceController : ApiController
    {

        

        [HttpPost]
        [ActionName("FetchPreCheckedInReservation")]
        public async Task<Models.Local.LocalResponseModel> FetchPreCheckedInReservation(Models.Local.LocalRequestModel localRequest)
        {
            Models.Local.FetchReservationRequest reservationRequest = JsonConvert.DeserializeObject<Models.Local.FetchReservationRequest>(localRequest.RequestObject.ToString());
            return await new Helper.Local.LocalAPI().FetchPreCheckedInReservation(reservationRequest);
        }

        [HttpPost]
        [ActionName("FetchPreCheckedOutReservation")]
        public async Task<Models.Local.LocalResponseModel> FetchPreCheckedOutReservation(Models.Local.LocalRequestModel localRequest)
        {
            Models.Local.FetchReservationRequest reservationRequest = JsonConvert.DeserializeObject<Models.Local.FetchReservationRequest>(localRequest.RequestObject.ToString());
            return await new Helper.Local.LocalAPI().FetchPreCheckedOutReservation(reservationRequest);
        }

        [HttpPost]
        [ActionName("PushDueInReservation")]
        public async Task<Models.Local.LocalResponseModel> PushDueInReservation(Models.Local.LocalRequestModel localRequest)
        {
            Models.Local.PushReservationRequest reservationRequest = JsonConvert.DeserializeObject<Models.Local.PushReservationRequest>(localRequest.RequestObject.ToString());
            bool IsCloud = true;
            if (IsCloud)
            {
                return await new Helper.Local.LocalAPI().PushCloudDueInReservation(reservationRequest);
            }
            else
            {
                return await new Helper.Local.LocalAPI().PushDueInReservation(reservationRequest);
            }
        }

        [HttpPost]
        [ActionName("PushPaymentLink")]
        public async Task<Models.Local.LocalResponseModel> PushPaymentLink(Models.Local.LocalRequestModel localRequest)
        {
            Models.Local.PushReservationRequest reservationRequest = JsonConvert.DeserializeObject<Models.Local.PushReservationRequest>(localRequest.RequestObject.ToString());
            return await new Helper.Local.LocalAPI().PushDueInReservation(reservationRequest);
        }

        [HttpPost]
        [ActionName("PushDueOutReservation")]
        public async Task<Models.Local.LocalResponseModel> PushDueOutReservation(Models.Local.LocalRequestModel localRequest)
        {
            Models.Local.PushReservationRequest reservationRequest = JsonConvert.DeserializeObject<Models.Local.PushReservationRequest>(localRequest.RequestObject.ToString());

            bool IsCloud = true;
            if (IsCloud)
            {
                return await new Helper.Local.LocalAPI().PushCloudDueOutReservationDetails(reservationRequest);
            }
            else
            {
                return await new Helper.Local.LocalAPI().PushDueOutReservation(reservationRequest);
            }
           
        }

        [HttpPost]
        [ActionName("PushDueOutReservationDetails")]
        public async Task<Models.Local.LocalResponseModel> PushDueOutReservationDetails(Models.Local.LocalRequestModel localRequest)
        {
            Models.Local.PushReservationRequest reservationRequest = JsonConvert.DeserializeObject<Models.Local.PushReservationRequest>(localRequest.RequestObject.ToString());
            bool IsCloud = true;
            if (!IsCloud)
            {
                return await new Helper.Local.LocalAPI().PushDueOutReservationDetails(reservationRequest);
            }
            else
            {
                return await new Helper.Local.LocalAPI().PushCloudDueOutReservationDetails(reservationRequest);
            }
        }

        

    }
}
