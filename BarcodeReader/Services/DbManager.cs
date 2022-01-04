using BarcodeReader.Database;
using BarcodeReader.Models;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BarcodeReader.Services
{
    public static class DbManager
    {

        public static Dictionary<string, Options> GetOptions(string postName)
        {
            var list = new Dictionary<string, Options>();
            string sql = "SELECT o.Id, o.Post, o.Variable, o.Value FROM Options AS o INNER JOIN Posts AS p ON (o.Post = p.Id) WHERE p.Name = '" + postName + "'";
            MySqlConnection conn = DBUtils.GetDBConnection();
            conn.Open();
            MySqlCommand cmd = new MySqlCommand();        
            try
            {
                cmd.Connection = conn;
                cmd.CommandText = sql;
                using (var reader = cmd.ExecuteReader())
                {
               
                    while (reader.Read())
                        list[reader.GetString(2)] = new Options {
                            Id = reader.GetInt32(0),
                            Post = reader.GetInt32(1), 
                            Variable = reader.GetString(2), 
                            Value = reader.GetString(3) 
                        };
                }

            }
            catch (Exception e)
            {
                Console.WriteLine("Error: " + e);
                Console.WriteLine(e.StackTrace);
            }
            finally
            {
                conn.Close();
                conn.Dispose();
            }
           
            return list;
        }

        public static bool PostNameExist(string postName)
        {
            string sql = "SELECT Name FROM Posts WHERE Name = '" + postName + "'";
            MySqlConnection conn = DBUtils.GetDBConnection();
            conn.Open();
            MySqlCommand cmd = new MySqlCommand();    
            try
            {
                cmd.Connection = conn;
                cmd.CommandText = sql;
                using (var reader = cmd.ExecuteReader())
                {

                    while (reader.Read())
                    {
                        if(reader.GetString(0) == postName)
                        {
                            return true;
                        }
                    }
                       
                }
            }
            catch (Exception)
            {
                throw;
            }
            return false; 
        }



        public static void InsertPost(string name)
        {
            if(!PostNameExist(name))
            {
                MySqlConnection conn = DBUtils.GetDBConnection();
                conn.Open();
                try
                {

                    string sql = "INSERT INTO Posts (Name) VALUES(@Name); SELECT LAST_INSERT_ID();";
                    MySqlCommand cmd = new MySqlCommand();
                    cmd.Connection = conn;
                    cmd.CommandText = sql;
                    MySqlParameter gradeParam = new MySqlParameter("@Name", name);
                    cmd.Parameters.Add(gradeParam);

                    int id = Convert.ToInt32(cmd.ExecuteScalar());
                    string downloadsPath = KnownFolders.GetPath(KnownFolder.Downloads);
                    string sqlOptions = "INSERT INTO Options (Post, Variable, Value) VALUES " +
                        $"({id}, 'PDF_FILENAME', 'colissimo;prepa')," +
                        $"({id}, 'DOWNLOAD_DIRECTORY', {downloadsPath} )," +
                        $"({id}, 'PRINTER_NAME', 'Adobe PDF')," +
                        $"({id}, 'PDF_EXTENSION', 'pdf')";

                    MySqlCommand cmdOptions = new MySqlCommand();
                    cmdOptions.Connection = conn;
                    cmdOptions.CommandText = sqlOptions; 
                    int rowCount = cmdOptions.ExecuteNonQuery();
                }
                catch (Exception e)
                {

                    throw;
                }
            }
        }

    }
}
