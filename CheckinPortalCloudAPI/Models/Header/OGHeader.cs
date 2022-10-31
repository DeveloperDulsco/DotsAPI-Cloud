using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Serialization;

namespace CheckinPortalCloudAPI.Models.Header
{
    [XmlRoot(ElementName = "OGHeader", Namespace = "http://webservices.micros.com/og/4.3/Core/")]
    public class OGHeader
    {
        [XmlElement(ElementName = "Origin", Namespace = "http://webservices.micros.com/og/4.3/Core/")]
        public Origin Origin { get; set; }
        [XmlElement(ElementName = "Destination", Namespace = "http://webservices.micros.com/og/4.3/Core/")]
        public Destination Destination { get; set; }
        [XmlElement(ElementName = "Authentication", Namespace = "http://webservices.micros.com/og/4.3/Core/")]
        public Authentication Authentication { get; set; }
        //[XmlAttribute(AttributeName = "transactionID")]
        //public string TransactionID { get; set; }
        //[XmlAttribute(AttributeName = "primaryLangID")]
        //public string PrimaryLangID { get; set; }
        //[XmlAttribute(AttributeName = "xmlns")]
        //public string Xmlns { get; set; }
        //[XmlAttribute(AttributeName = "xsi", Namespace = "http://www.w3.org/2000/xmlns/")]
        //public string Xsi { get; set; }
        //[XmlAttribute(AttributeName = "xsd", Namespace = "http://www.w3.org/2000/xmlns/")]
        //public string Xsd { get; set; }
    }

    [XmlRoot(ElementName = "Origin", Namespace = "http://webservices.micros.com/og/4.3/Core/")]
    public class Origin
    {
        [XmlAttribute(AttributeName = "entityID")]
        public string EntityID { get; set; }
        [XmlAttribute(AttributeName = "systemType")]
        public string SystemType { get; set; }
    }

    [XmlRoot(ElementName = "Destination", Namespace = "http://webservices.micros.com/og/4.3/Core/")]
    public class Destination
    {
        [XmlAttribute(AttributeName = "entityID")]
        public string EntityID { get; set; }
        [XmlAttribute(AttributeName = "systemType")]
        public string SystemType { get; set; }
    }

    [XmlRoot(ElementName = "UserCredentials", Namespace = "http://webservices.micros.com/og/4.3/Core/")]
    public class UserCredentials
    {
        [XmlElement(ElementName = "UserName", Namespace = "http://webservices.micros.com/og/4.3/Core/")]
        public string UserName { get; set; }
        [XmlElement(ElementName = "UserPassword", Namespace = "http://webservices.micros.com/og/4.3/Core/")]
        public string UserPassword { get; set; }
        [XmlElement(ElementName = "Domain", Namespace = "http://webservices.micros.com/og/4.3/Core/")]
        public string Domain { get; set; }
    }

    [XmlRoot(ElementName = "Authentication", Namespace = "http://webservices.micros.com/og/4.3/Core/")]
    public class Authentication
    {
        [XmlElement(ElementName = "UserCredentials", Namespace = "http://webservices.micros.com/og/4.3/Core/")]
        public UserCredentials UserCredentials { get; set; }
        [XmlAttribute(AttributeName = "xmlns")]
        public string Xmlns { get; set; }
    }
}