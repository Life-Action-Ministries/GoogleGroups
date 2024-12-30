using Dapper;
using GoogleGroups.Models;
using System.Data;
using System.Data.SqlClient;
using System.Data.SQLite;

namespace GoogleGroups.App_Code
{
    public class SQL
    {
        public static List<GroupUserModel> GetGroupUsers(string groupId)
        {
            List<GroupUserModel> users = new List<GroupUserModel>();
            using (var connection = new SQLiteConnection(Helper.CnnVal("SQLite")))
            {                
                users = connection.Query<GroupUserModel>("SELECT UserName, GroupId, PermissionLevel, RoleTitle FROM GroupUserPermissions Left Outer Join Roles on PermissionLevel = RoleId  Where GroupId = '" + groupId + "'").ToList();
            }
            return users;
        }

        public static List<string> GetSuperAdmins()
        {
            List<string> users = new List<string>();
            using (IDbConnection connection = new SQLiteConnection(Helper.CnnVal("SQLite")))
            {
                users = connection.Query<string>("SELECT Username FROM SuperAdmins").ToList();
            }
            return users;
        }

        public static void AddManager(string username, string groupId, int accessLevel)
        {
            using (IDbConnection connection = new SQLiteConnection(Helper.CnnVal("SQLite")))
            {
                connection.Query("Insert into GroupUserPermissions values('" + username + "', '" + groupId + "', " + accessLevel + ")");
            }
        }

        public static void UpdateManager(string username, string groupId, int accessLevel)
        {
            using (IDbConnection connection = new SQLiteConnection(Helper.CnnVal("SQLite")))
            {
                connection.Query("Update GroupUserPermissions Set PermissionLevel = " + accessLevel + " Where UserName = '" + username + "' and GroupId = '" + groupId + "'");
            }
        }

        public static void DeleteManager(string username, string groupId)
        {
            using (IDbConnection connection = new SQLiteConnection(Helper.CnnVal("SQLite")))
            {
                connection.Query("Delete From GroupUserPermissions Where UserName = '" + username + "' and GroupId = '" + groupId + "'");
            }
        }
        public static void AddSuperAdmin(string username)
        {
            using (IDbConnection connection = new SQLiteConnection(Helper.CnnVal("SQLite")))
            {
                connection.Query("Insert Into SuperAdmins values('" + username + "')");
            }
        }

        public static void DeleteSuperAdmin(string username)
        {
            using (IDbConnection connection = new SQLiteConnection(Helper.CnnVal("SQLite")))
            {
                connection.Query("Delete From SuperAdmins Where UserName = '" + username + "'");
            }
        }
    }
}
