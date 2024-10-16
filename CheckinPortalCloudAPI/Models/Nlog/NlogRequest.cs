using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace CheckinPortalCloudAPI.Models.Nlog
{
    public class NlogRequest
    {
        public LogLevel Level { get; set; }
        public string Message { get; set; }
        public Exception Exception { get; set; }
        public string ActionName { get; set; }
        public string ApplicationName { get; set; }
        public string ActionGroup { get; set; }

    }
}