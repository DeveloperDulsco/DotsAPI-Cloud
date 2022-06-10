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
            System.IO.File.WriteAllText(System.Web.Hosting.HostingEnvironment.MapPath(@"~\request.xml"), request.ToString());
            //request.Headers.Add(getSecurityHeader(wsdl_usr_name, wsdl_paswd));

            //request.Headers.RemoveAt(request.Headers.FindHeader("OGHeader", "http://webservices.micros.com/og/4.3/Core/"));

            //request.Headers.Add(GetOGHeader(hotel_domain, og_header_usr_name, og_header_paswd));
            return null;
        }

        #endregion

        
    }

    public class CustomHeader : MessageHeader
    {
        private string CUSTOM_HEADER_NAME = "";
        private string CUSTOM_HEADER_NAMESPACE = "";

        private readonly XmlDocument _xnlData = new XmlDocument();

        protected List<CusttomHeaderAttributes> _attributes = new List<CusttomHeaderAttributes>();

        public CustomHeader(XmlDocument elements, string HeaderName, string HeaderNameSpace)
        {
            _xnlData = elements;
            CUSTOM_HEADER_NAME = HeaderName.Equals(null) ? "" : HeaderName;
            CUSTOM_HEADER_NAMESPACE = HeaderNameSpace.Equals(null) ? "" : HeaderNameSpace;
        }

        public List<CusttomHeaderAttributes> Attributes
        {
            set { _attributes = value; }
        }

        public override string Name
        {
            get { return (CUSTOM_HEADER_NAME); }
        }

        public override string Namespace
        {
            get { return (CUSTOM_HEADER_NAMESPACE); }
        }

        protected override void OnWriteHeaderContents(System.Xml.XmlDictionaryWriter writer, MessageVersion messageVersion)
        {
            foreach (CusttomHeaderAttributes Attributes in _attributes)
            {
                writer.WriteAttributeString(Attributes.AttributPrefix, Attributes.AttributeLocalName, Attributes.Attributens, Attributes.Value);
            }
            foreach (XmlNode node in _xnlData.ChildNodes[0].ChildNodes)
            {
                writer.WriteNode(node.CreateNavigator(), false);
            }

        }

    }

    public class CusttomHeaderAttributes
    {
        public string AttributPrefix { get; set; }
        public string AttributeLocalName { get; set; }
        public string Attributens { get; set; }
        public string Value { get; set; }
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