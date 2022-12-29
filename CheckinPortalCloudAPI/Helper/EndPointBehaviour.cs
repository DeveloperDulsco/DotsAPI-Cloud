using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Web;
using System.Xml;
using System.Xml.Serialization;

namespace CheckinPortalCloudAPI.Helper
{
    public class MessageInspector : IClientMessageInspector
    {

        static string wsdl_usr_name = null;
        static string wsdl_paswd = null;
        static string og_header_usr_name = null;
        static string og_header_paswd = null;
        static string hotel_domain = null;
        static string dest_entity_id = "KIOSK";
        static string dest_system_type = "PMS";
        static string source_entity_id = "KIOSK"; 
        static string source_system_type = "KIOSK";
        public MessageInspector(string WSSE_Username, string WSSE_Password, string Username, string Password, string HotelDomain)
        {
            wsdl_usr_name = WSSE_Username;
            wsdl_paswd = WSSE_Password;
            og_header_usr_name = Username;
            og_header_paswd = Password;
            hotel_domain = HotelDomain;
        }

        #region IClientMessageInspector Members
        public void AfterReceiveReply(ref System.ServiceModel.Channels.Message reply, object correlationState)
        {
            if (reply.Headers.FindHeader("Security", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd") > 0)
                reply.Headers.RemoveAt(reply.Headers.FindHeader("Security", "http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd"));
        }

        public object BeforeSendRequest(ref System.ServiceModel.Channels.Message request, IClientChannel channel)
        {
            Helper HelperClass = new Helper();
            //System.IO.File.WriteAllText(System.Web.Hosting.HostingEnvironment.MapPath(@"~\request1.txt"), Newtonsoft.Json.JsonConvert.SerializeObject(request));
            request.Headers.Add(HelperClass.getSecurityHeader(wsdl_usr_name, wsdl_paswd));

            request.Headers.RemoveAt(request.Headers.FindHeader("OGHeader", "http://webservices.micros.com/og/4.3/Core/"));

            request.Headers.Add(HelperClass.GetOGHeader(hotel_domain, og_header_usr_name, og_header_paswd,dest_entity_id,dest_system_type,source_entity_id,source_system_type));
            
            return null;
        }

        #endregion
    }

    public class CustomEndpointBehaviour : IEndpointBehavior
    {

        static string wsdl_usr_name = null;
        static string wsdl_paswd = null;
        static string og_header_usr_name = null;
        static string og_header_paswd = null;
        static string hotel_domain = null;

        public CustomEndpointBehaviour(string WSSE_Username, string WSSE_Password, string Username, string Password, string HotelDomain)
        {
            wsdl_usr_name = WSSE_Username;
            wsdl_paswd = WSSE_Password;
            og_header_usr_name = Username;
            og_header_paswd = Password;
            hotel_domain = HotelDomain;
        }
        public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
            clientRuntime.MessageInspectors.Add(new MessageInspector(wsdl_usr_name, wsdl_paswd, og_header_usr_name, og_header_paswd, hotel_domain));
        }

        public void AddBindingParameters(ServiceEndpoint serviceEndpoint, System.ServiceModel.Channels.BindingParameterCollection bindingParameters)
        {
            return;
        }

        public void ApplyDispatchBehavior(ServiceEndpoint serviceEndpoint, EndpointDispatcher endpointDispatcher)
        {
            //endpointDispatcher.DispatchRuntime.MessageInspectors.Add(new EndpointBehaviorMessageInspector());
        }

        public void Validate(ServiceEndpoint serviceEndpoint)
        {
            return;
        }
    }
}