using Dapper;
using Microsoft.AspNetCore.Authorization;
using NLog;
using System.Data;
using System.Data.SqlClient;
using System.Data.SQLite;


namespace GoogleGroups.App_Code
{
    public class MyAuthorizeAttribute : AuthorizeAttribute
    {
        //
        //       MethodBase.GetCurrentMethod().DeclaringType);
        private static Logger log = LogManager.GetCurrentClassLogger();

        public static int GetUserPermission(string username, string groupId)
        {
            string superAdmin = "";
            int permission = 0;
            using (IDbConnection connection = new SQLiteConnection(Helper.CnnVal("SQLite")))
            {
                try
                {
                    superAdmin = connection.Query<string>("Select Username from SuperAdmins where Username = '" + username + "'").First();
                }
                catch { }
                if (superAdmin == username)
                {
                    permission = 4;
                }
                else
                {
                    try
                    {
                        permission = connection.Query<int>("Select PermissionLevel from GroupUserPermissions where UserName = '" + username + "' and GroupId = '" + groupId + "'").First();
                    }
                    catch { permission = 0; }
                }
            }
            return permission;
        }

    }
}
