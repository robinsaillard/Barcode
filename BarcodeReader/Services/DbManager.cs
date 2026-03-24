using BarcodeReader.Database;
using BarcodeReader.Models;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;

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

        public static int UpdateOptions(string postName, Options option)
        {
            MySqlConnection conn = DBUtils.GetDBConnection();
            conn.Open();
            string sql = "UPDATE Options AS o INNER JOIN Posts AS p ON (o.Post = p.Id) SET o.Value = @Value WHERE p.Name = @Postname AND o.Variable = @Variable ";
            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = conn;
            cmd.CommandText = sql;
            MySqlParameter value = new MySqlParameter("@Value", option.Value);
            cmd.Parameters.Add(value);
            MySqlParameter post = new MySqlParameter("@Postname", postName);
            cmd.Parameters.Add(post);
            MySqlParameter variable = new MySqlParameter("@Variable", option.Variable);
            cmd.Parameters.Add(variable);
            int rowCount = cmd.ExecuteNonQuery();
            return rowCount;
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
                        $"({id}, 'DOWNLOAD_DIRECTORY', '{downloadsPath.Replace("\\","\\\\")}')," +
                        $"({id}, 'PRINTER_NAME', 'Adobe PDF')," +
                        $"({id}, 'PDF_EXTENSION', 'pdf')";

                    MySqlCommand cmdOptions = new MySqlCommand
                    {
                        Connection = conn,
                        CommandText = sqlOptions
                    };
                    int rowCount = cmdOptions.ExecuteNonQuery();
                }
                catch (Exception)
                {

                    throw;
                }
            }
        }

        public static void InsertScanList(string url)
        {
            MySqlConnection conn = DBUtils.GetDBConnection();
            conn.Open();
            try
            {
                string sql = "INSERT INTO ScanList (Url, Date) VALUES(@Url, @Date)";
                MySqlCommand cmd = new MySqlCommand();
                cmd.Connection = conn;
                cmd.CommandText = sql;
                MySqlParameter p1 = new MySqlParameter("@Url", url);
                cmd.Parameters.Add(p1);
                MySqlParameter p2 = new MySqlParameter("@Date", DateTime.Now);
                cmd.Parameters.Add(p2);
                int rowCount = cmd.ExecuteNonQuery();
            }
            catch (Exception )
            {

                throw;
            }
        }

        public static List<Historique> GetScanList()
        {
            List<Historique> historique = new List<Historique>();
            string sql = "SELECT * FROM ScanList ORDER BY Date DESC LIMIT 100 ";
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
                        historique.Add(new Historique
                        {
                            Id = reader.GetInt32(0),
                            Url = reader.GetString(1),
                            Date = reader.GetDateTime(2),
                        });
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

            return historique;
        }

        public static int InsertOption(string postName, string variable, string value)
        {
            MySqlConnection conn = DBUtils.GetDBConnection();
            conn.Open();

            try
            {
                // D'abord récupérer l'ID du post
                string sqlGetPostId = "SELECT Id FROM Posts WHERE Name = @PostName";
                MySqlCommand cmdGetPostId = new MySqlCommand(sqlGetPostId, conn);
                cmdGetPostId.Parameters.AddWithValue("@PostName", postName);

                var postId = cmdGetPostId.ExecuteScalar();

                if (postId == null)
                {
                    throw new Exception($"Le poste '{postName}' n'existe pas en base de données");
                }

                // Insérer la nouvelle option
                string sql = "INSERT INTO Options (Post, Variable, Value) VALUES (@Post, @Variable, @Value)";
                MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Post", postId);
                cmd.Parameters.AddWithValue("@Variable", variable);
                cmd.Parameters.AddWithValue("@Value", value);

                int rowCount = cmd.ExecuteNonQuery();
                return rowCount;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex);
                Console.WriteLine(ex.StackTrace);
                throw;
            }
            finally
            {
                conn.Close();
                conn.Dispose();
            }
        }




        public static void EnsureAutoPrintOptionExists(string postName)
        {
            try
            {
                var options = GetOptions(postName);

                if (options == null || !options.ContainsKey("AUTOPRINT_ENABLED"))
                {
                    InsertOption(postName, "AUTOPRINT_ENABLED", "false");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error creating AUTOPRINT option: " + ex);
            }
        }

        public static Options GetOption(string postName, string variable)
        {
            try
            {
                var options = GetOptions(postName);

                if (options != null && options.ContainsKey(variable))
                {
                    return options[variable];
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in GetOption: " + ex);
                Console.WriteLine(ex.StackTrace);
                return null;
            }
        }

        public static string GetOptionValue(string postName, string variable)
        {
            try
            {
                var option = GetOption(postName, variable);
                return option?.Value;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in GetOptionValue: " + ex);
                return null;
            }
        }

        public static bool IsOptionEnabled(string postName, string variable)
        {
            try
            {
                var option = GetOption(postName, variable);
                if (option != null && !string.IsNullOrEmpty(option.Value))
                {
                    return bool.TryParse(option.Value, out bool result) && result;
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in IsOptionEnabled: " + ex);
                return false;
            }
        }

    }
}
