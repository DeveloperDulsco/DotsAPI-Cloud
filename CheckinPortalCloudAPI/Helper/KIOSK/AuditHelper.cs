using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CheckinPortalCloudAPI.Helper.KIOSK
{
    public class AuditHelper
    {
        public static bool InsertAuditLog(string PageName, string UserName, string AuditMessage,string GroupIdentifier,string GeneralIdentifier,string DeviceIdentifier, List<Models.KIOSK.AuditJsonObject> jsonObjects,string connectionString)
        {
            try
            {
                var auditResponse = new DapperHelper().ExecuteSP<Models.KIOSK.DB.SPResponseModel>("Usp_InsertAuditDetails", connectionString, new
                {
                    ModuleName = PageName,
                    UserName = UserName,
                    ActionName = AuditMessage,
                    GroupIdentifier = GroupIdentifier,
                     GeneralIdentifier = GeneralIdentifier,
                     DeviceIdentifier = DeviceIdentifier,
                    ChangeJSON = (jsonObjects != null) ? JsonConvert.SerializeObject(jsonObjects) : null
                }).ToList();
                if (auditResponse == null || string.IsNullOrEmpty(auditResponse.First().Result) || !auditResponse.First().Result.Equals("200"))
                {
                    new LogHelper().Debug("Failled to update the audit log", "", "InsertAuditLog", "KIOSK", "Audit");
                    return false;
                }
                else
                    return true;
            }
            catch (Exception ex)
            {
                LogHelper.Instance.Error(ex,"", "InsertAuditLog", "KIOSK", "Audit");
                return false;
            }
        }
    }
}