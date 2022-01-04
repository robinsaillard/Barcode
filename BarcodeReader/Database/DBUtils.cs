using MySql.Data.MySqlClient;
using System;

namespace BarcodeReader.Database
{
    class DBUtils
    {
        public static MySqlConnection GetDBConnection()
        {
            string host = Config.AppSettings.Host;
            int port = Int32.Parse(Config.AppSettings.Port); 
            string database = Config.AppSettings.Database;
            string username = Config.AppSettings.Username;
            string password = Config.AppSettings.Password;

            return DBMySQLUtils.GetDBConnection(host, port, database, username, password);
        }
    }
}
