using MySql.Data.MySqlClient;

namespace BarcodeReader.Database
{
    class DBUtils
    {
        public static MySqlConnection GetDBConnection()
        {
            string host = "37.187.80.195";
            int port = 3306;
            string database = "barcode";
            string username = "barcode";
            string password = "pHy0y_66";

            return DBMySQLUtils.GetDBConnection(host, port, database, username, password);
        }
    }
}
