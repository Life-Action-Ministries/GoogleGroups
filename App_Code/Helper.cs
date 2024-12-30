using Dapper;
using Google.Apis.Admin.Directory.directory_v1.Data;
using GoogleGroups.Models;
using System.Data.SQLite;
using System.Runtime.CompilerServices;

namespace GoogleGroups
{
    public class Helper
    {
        private static string? SQLite = CnnVal("SQLite");
        private static string? SettingsSQLite = CnnVal("SettingsSQLite");
        public static string? CnnVal(string name)
        {
            return new ConfigurationBuilder().AddJsonFile("./appsettings.json").Build().GetSection("ConnectionStrings")[name];
            //return connectionString;
        }
        public static string? CredVal(string name)
        {
            string? credVal;
            using (var connection = new SQLiteConnection(Helper.CnnVal("SettingsSQLite")))
            {
                credVal = connection.Query<string>("SELECT value FROM Credentials WHERE Name = '" + name + "'").FirstOrDefault();
            }
            //return new ConfigurationBuilder().AddJsonFile("./Config/appsettings.json").Build().GetSection("Credentials")[name];
            return credVal;
        }
        public static string? EnvVars(string name)
        {
            string? credVal;
            using (var connection = new SQLiteConnection(Helper.CnnVal("SettingsSQLite")))
            {
                credVal = connection.Query<string>("SELECT value FROM Credentials WHERE Name = '" + name + "'").FirstOrDefault();
            }
            //return new ConfigurationBuilder().AddJsonFile("./Config/appsettings.json").Build().GetSection("EnvVars")[name];
            return credVal;
        }

        public static void UpdateSettings(string name, string value)
        {
            using (var connection = new SQLiteConnection(Helper.CnnVal("SettingsSQLite")))
            {
                connection.Query("UPDATE Credentials SET value = '"+value+"' WHERE Name = '" + name + "'");
            }
        }

        public static void InitializeDatabase()
        {
            if (!File.Exists(@"./wwwroot/Data/Permissions.db"))
            {
                SQLiteConnection.CreateFile(@"./wwwroot/Data/Permissions.db");

                using (var connection = new SQLiteConnection(SQLite))
                {
                    connection.Open();

                    string CreateSuperAdminTable = @"
                        CREATE TABLE IF NOT EXISTS SuperAdmins (
                            Username TEXT NOT NULL
                        );";

                    string CreatePermissionsTable = @"
                        CREATE TABLE IF NOT EXISTS GroupUserPermissions (
                            UserName TEXT,
	                        GroupId TEXT,
	                        PermissionLevel INTEGER
                        );";

                    string CreateRolesTable = @"
                        CREATE TABLE IF NOT EXISTS Roles (
                            RoleId INTEGER,
	                        RoleTitle TEXT
                        );";

                    string CreateLogsTable = @"
                        CREATE TABLE IF NOT EXISTS Logs (
                            LogId INTEGER IDENTITY(1,1) NOT NULL,
	                        Date DATETIME NOT NULL,
                            HostName TEXT,
                            Thread TEXT NOT NULL,
                            Identity TEXT,
                            Level TEXT NOT NULL,
                            Browser TEXT,
                            Platform TEXT,
                            Logger TEXT NOT NULL,
                            User TEXT,
                            Message TEXT NOT NULL,
                            Exception TEXT
                        );";

                    using (var command = new SQLiteCommand(connection))
                    {
                        command.CommandText = CreateSuperAdminTable;
                        command.ExecuteNonQuery();

                        command.CommandText = CreatePermissionsTable;
                        command.ExecuteNonQuery();

                        command.CommandText = CreateRolesTable;
                        command.ExecuteNonQuery();
                    }
                }
            }

            if (!File.Exists(@"./wwwroot/Data/Settings.db"))
            {
                SQLiteConnection.CreateFile(@"./wwwroot/Data/Settings.db");

                using (var connection = new SQLiteConnection(SettingsSQLite))
                {
                    connection.Open();

                    string CreateCredentialsTable = @"
                        CREATE TABLE IF NOT EXISTS Credentials (
                            Name TEXT,
                            value TEXT
                        );";

                    string CreateCredentialsFields = @"
                        INSERT INTO Credentials VALUES
                            ('ClientID', '')
                            ,('ClientSecret', '')
                            ,('Domain', '')
                            ,('ServiceAccountClientID', '')
                            ,('AdminEmail', '')
                            ,('CredPass', '')
                            ,('OrganizationName', '');";

                    using (var command = new SQLiteCommand(connection))
                    {
                        command.CommandText = CreateCredentialsTable;
                        command.ExecuteNonQuery();

                        command.CommandText = CreateCredentialsFields;
                        command.ExecuteNonQuery();


                    }
                }
            }
        }

   
    }

    public class UploadHandler
    {
        public static string Upload(IFormFile file, List<string> validExtensions, string fileName, string location)
        {
            //Extension validation
            string extension = Path.GetExtension(file.FileName);
            if (!validExtensions.Contains(extension))
            {
                return $"Extension is not valid ({string.Join(',', validExtensions)})";
            }

            //File size validation
            long size = file.Length;
            if (size > 5 * 1024 * 1024)
            {
                return "File must not be larger than 5MB";
            }

            //name changing
            fileName = fileName + extension;
            string path = Path.Combine(Directory.GetCurrentDirectory(), location);
            using FileStream stream = new FileStream(Path.Combine(path, fileName), FileMode.Create);
            file.CopyTo(stream);
            

            return fileName;
        }
    }
}
