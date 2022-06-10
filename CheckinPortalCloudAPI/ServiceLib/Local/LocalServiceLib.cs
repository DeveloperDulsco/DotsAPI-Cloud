using CheckinPortalCloudAPI.Models.Local;
using Microsoft.Reporting.WebForms;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace CheckinPortalCloudAPI.ServiceLib.Local
{
    public class LocalServiceLib
    {

        public Models.Local.LocalResponseModel GenerateKioskReceipt(KioskReceiptRequest receiptRequest)
        {
            try
            {
                string Base64 = null;
                if (!System.IO.File.Exists(System.Web.Hosting.HostingEnvironment.MapPath("~/Resources/RDLC/CheckinMessageMember.rdlc")))
                {
                    return new LocalResponseModel()
                    {
                        result = false,
                        responseData = null,
                        responseMessage = "Failled to locate the report file"
                    };
                }
                else if(!System.IO.File.Exists(System.Web.Hosting.HostingEnvironment.MapPath("~/Resources/RDLC/CheckinMessageNonMember.rdlc")))
                {
                    return new LocalResponseModel()
                    {
                        result = false,
                        responseData = null,
                        responseMessage = "Failled to locate the report file"
                    };
                }

                ReportViewer rv = new Microsoft.Reporting.WebForms.ReportViewer();
                rv.ProcessingMode = ProcessingMode.Local;
               
                string reportPath = null;
                if (receiptRequest.IsMember != null && receiptRequest.IsMember.Value)
                {
                    reportPath = System.Web.Hosting.HostingEnvironment.MapPath("~/Resources/RDLC/CheckinMessageMember.rdlc");
                }
                else
                {
                    reportPath = System.Web.Hosting.HostingEnvironment.MapPath("~/Resources/RDLC/CheckinMessageNonMember.rdlc");
                }
                using (StreamReader rdlcSR = new StreamReader(reportPath))
                {

                    rv.LocalReport.LoadReportDefinition(rdlcSR);// = System.Web.Hosting.HostingEnvironment.MapPath("~/Resources/RDLC/RegCard.rdlc");
                    rv.LocalReport.EnableExternalImages = true;


                    ReportParameter p1 = new ReportParameter("RoomNumber", receiptRequest.RoomNumber);




                    rv.LocalReport.SetParameters(new ReportParameter[] { p1 });
                    rv.LocalReport.Refresh();
                    byte[] streamBytes = null;
                    string mimeType = "";
                    string encoding = "";
                    string filenameExtension = "";
                    string[] streamids = null;
                    Warning[] warnings = null;
                    streamBytes = rv.LocalReport.Render("PDF", null, out mimeType, out encoding, out filenameExtension, out streamids, out warnings);
                    Base64 = Convert.ToBase64String(streamBytes);
                    rv.LocalReport.Refresh();
                }
                rv.LocalReport.Dispose();
                //PrintHelpers.Export(report);
                //PrintHelpers.Print();
                return new LocalResponseModel()
                {
                    result = true,
                    responseData = Base64
                };
            }
            catch (Exception ex)
            {
                return new LocalResponseModel()
                {
                    result = false,
                    responseData = null,
                    responseMessage = ex.Message
                };
            }
        }
        public Models.Local.LocalResponseModel GenerateKioskReceiptForPrint(KioskReceiptRequest receiptRequest)
        {
            try
            {
                string Base64 = null;
                if (!System.IO.File.Exists(System.Web.Hosting.HostingEnvironment.MapPath("~/Resources/RDLC/PrintCheckinMessageMember.rdlc")))
                {
                    return new LocalResponseModel()
                    {
                        result = false,
                        responseData = null,
                        responseMessage = "Failled to locate the report file"
                    };
                }
                else if (!System.IO.File.Exists(System.Web.Hosting.HostingEnvironment.MapPath("~/Resources/RDLC/PrintCheckinMessageNonMember.rdlc")))
                {
                    return new LocalResponseModel()
                    {
                        result = false,
                        responseData = null,
                        responseMessage = "Failled to locate the report file"
                    };
                }

                ReportViewer rv = new Microsoft.Reporting.WebForms.ReportViewer();
                rv.ProcessingMode = ProcessingMode.Local;

                string reportPath = null;
                if (receiptRequest.IsMember != null && receiptRequest.IsMember.Value)
                {
                    reportPath = System.Web.Hosting.HostingEnvironment.MapPath("~/Resources/RDLC/PrintCheckinMessageMember.rdlc");
                }
                else
                {
                    reportPath = System.Web.Hosting.HostingEnvironment.MapPath("~/Resources/RDLC/PrintCheckinMessageNonMember.rdlc");
                }
                using (StreamReader rdlcSR = new StreamReader(reportPath))
                {

                    rv.LocalReport.LoadReportDefinition(rdlcSR);// = System.Web.Hosting.HostingEnvironment.MapPath("~/Resources/RDLC/RegCard.rdlc");
                    rv.LocalReport.EnableExternalImages = true;


                    ReportParameter p1 = new ReportParameter("RoomNumber", receiptRequest.RoomNumber);




                    rv.LocalReport.SetParameters(new ReportParameter[] { p1 });
                    rv.LocalReport.Refresh();
                    byte[] streamBytes = null;
                    string mimeType = "";
                    string encoding = "";
                    string filenameExtension = "";
                    string[] streamids = null;
                    Warning[] warnings = null;
                    streamBytes = rv.LocalReport.Render("PDF", null, out mimeType, out encoding, out filenameExtension, out streamids, out warnings);
                    Base64 = Convert.ToBase64String(streamBytes);
                    rv.LocalReport.Refresh();
                }
                rv.LocalReport.Dispose();
                //PrintHelpers.Export(report);
                //PrintHelpers.Print();
                return new LocalResponseModel()
                {
                    result = true,
                    responseData = Base64
                };
            }
            catch (Exception ex)
            {
                return new LocalResponseModel()
                {
                    result = false,
                    responseData = null,
                    responseMessage = ex.Message
                };
            }
        }
    }
}