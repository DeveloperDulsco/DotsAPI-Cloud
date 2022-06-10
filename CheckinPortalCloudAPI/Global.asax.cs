using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace CheckinPortalCloudAPI
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            if(System.IO.File.Exists(System.Web.Hosting.HostingEnvironment.MapPath("~/Resources/RDLC/ReportDataset.xsd")))
            {
                Models.Global.GlobalModel.FolioXSDMemoryStream = new System.IO.FileStream(System.Web.Hosting.HostingEnvironment.MapPath("~/Resources/RDLC/ReportDataset.xsd"), System.IO.FileMode.Open, System.IO.FileAccess.Read);
                
            }
        }
    }
}
