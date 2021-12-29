using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BarcodeReader.Services
{
    public static class DbManager
    {


        public static SqlConnection Get_DB_Connection()
        {
           /* string connString = @"Data Source=(LocalDB)\MSSQLLocalDB;
                    AttachDbFilename=" + AppDomain.CurrentDomain.BaseDirectory +
                    "Data\\Database.mdf" + ";Integrated Security = True; "; */
            string connString = @"Data Source=(LocalDB)\MSSQLLocalDB;
                    AttachDbFilename=|DataDirectory|\Data\Database.mdf;Integrated Security = True; ";
            SqlConnection bdd = new SqlConnection(connString);
            if (bdd.State != ConnectionState.Open) bdd.Open();

            return bdd;
        }

        public static DataTable Get_DataTable(string SQL_Text, string table,  SqlConnection bdd)
        {
            SqlCommand cmd = bdd.CreateCommand();
            cmd.CommandText = SQL_Text;
            SqlDataAdapter sda = new SqlDataAdapter(cmd);
            DataTable dt = new DataTable(table);
            sda.Fill(dt);
            return dt;
        }

        public static SqlDataReader FindAllBy(SqlConnection bdd, string table, Dictionary<string,string> parameters = null)
        {
            string sqlquery = "SELECT * FROM " + table;

            if(parameters != null)
            {
                int i = 0;
                sqlquery = sqlquery + " WHERE ";
                foreach (var param in parameters)
                {
                    if(i != 0) {
                        sqlquery = sqlquery + " AND ";
                    }
                    sqlquery = sqlquery + "[" + param.Key + "] = '" + param.Value + "'";
                    i++;
                }
            }
            SqlCommand command = new SqlCommand(sqlquery, bdd);
            SqlDataReader sReader;
            sReader = command.ExecuteReader();
            return sReader;
        }

        public static SqlDataReader Execute_SQL(string SQL_Text, SqlConnection bdd)
        {
            SqlCommand cmd_Command = new SqlCommand(SQL_Text, bdd);
            return cmd_Command.ExecuteReader();
        }

        public static void Close_DB_Connection(SqlConnection bdd)
        {
            bdd = Get_DB_Connection();
            if (bdd.State != ConnectionState.Closed) bdd.Close();
        }

    }
}
