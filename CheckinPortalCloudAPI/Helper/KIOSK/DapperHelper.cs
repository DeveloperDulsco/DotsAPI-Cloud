using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace CheckinPortalCloudAPI.Helper.KIOSK
{
    public class DapperHelper
    {
        public IEnumerable<T> ExecuteSP<T>(string sprocName, string connectionstring, object sprocParams = null)
        {
            IEnumerable<T> data = Activator.CreateInstance<List<T>>();

            using (var sc = new SqlConnection(connectionstring))
            {
                sc.Open();
                data = sc.Query<T>(sprocName, param: sprocParams, commandType: CommandType.StoredProcedure);
            }

            return data;
        }
        public IEnumerable<dynamic> ExecuteSP(string sprocName, string connectionstring, object sprocParams = null)
        {

            using (var sc = new SqlConnection(connectionstring))
            {
                sc.Open();
                var data = sc.Query(sprocName, param: sprocParams, commandType: CommandType.StoredProcedure);
                return data;
            }

        }
    }
}