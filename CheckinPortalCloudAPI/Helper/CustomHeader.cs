using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Channels;
using System.Web;
using System.Xml;

namespace CheckinPortalCloudAPI.Helper
{
    public class CustomHeader : MessageHeader
    {
        private string CUSTOM_HEADER_NAME = "";
        private string CUSTOM_HEADER_NAMESPACE = "";

        private readonly XmlDocument _xnlData = new XmlDocument();

        protected List<CusttomHeaderAttributes> _attributes = new List<CusttomHeaderAttributes>();
        
        public CustomHeader(XmlDocument elements,string HeaderName,string HeaderNameSpace)
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
}