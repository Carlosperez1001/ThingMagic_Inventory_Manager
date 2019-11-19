using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows;
using System.Windows.Media;
using static System.Net.Mime.MediaTypeNames;

namespace RFID_Demo
{

    public static class DBHelper
    {
        
        private static MySqlConnection connection;
        private static MySqlCommand cmd = null;
        private static MySqlDataAdapter da;
        public static DataTable dt = new DataTable();
        private static MySqlDataAdapter sda;

        public static void EstablishConnection()
        {
            try
            {
                MySqlConnectionStringBuilder builder = new MySqlConnectionStringBuilder();
                builder.Server = "127.0.0.1";
                builder.UserID = "root";
                builder.Password = "12345";
                builder.Database = "item_book";
                builder.SslMode = MySqlSslMode.None;

                connection = new MySqlConnection(builder.ToString());
                MessageBox.Show("YES");
            }
            catch (Exception ex)
            {
                MessageBox.Show("help");
            }

        }
      
        public static void UpdateBookTable()
        {
            dt.Clear();
            string query = "Select Book_Title, Book_Autor, Book_Genre, Book_Image, Book_RFID_EPC, Book_RFID_TimeStamp, Book_RFID_RSSI from books";
            if (connection != null)
            {
                connection.Open();
                cmd = new MySqlCommand(query, connection);
                da = new MySqlDataAdapter(cmd);
                da.Fill(dt);
                connection.Close();
            }
         
        }

        public static void addBookQuery(string EPC, string TimeStamp, string RSSI, string Book_Title, string Book_Autor, string Book_Genre, ImageSource source)
        {
            MemoryStream ms = new MemoryStream();
            byte[] img = ms.ToArray();

            String query = "INSERT INTO books (Book_id, Book_Title, Book_Autor, Book_Genre, Book_Image, Book_RFID_EPC, Book_RFID_TimeStamp, Book_RFID_RSSI) " +
                "VALUE(@Book_id, @Book_Title, @Book_Autor, @Book_Genre, @Book_Image, @Book_RFID_EPC, @Book_RFID_TimeStamp, @Book_RFID_RSSI)";
            if (connection != null)
            {
                connection.Open();
                cmd = new MySqlCommand(query, connection);

                cmd.Parameters.Add("@Book_id", MySqlDbType.VarChar, 255);
                cmd.Parameters.Add("@Book_Title", MySqlDbType.VarChar, 255);
                cmd.Parameters.Add("@Book_Autor", MySqlDbType.VarChar, 255);
                cmd.Parameters.Add("@Book_Genre", MySqlDbType.VarChar, 255);
                cmd.Parameters.Add("@Book_Image", MySqlDbType.Blob);
                cmd.Parameters.Add("@Book_RFID_EPC", MySqlDbType.VarChar, 255);
                cmd.Parameters.Add("@Book_RFID_TimeStamp", MySqlDbType.VarChar, 255);
                cmd.Parameters.Add("@Book_RFID_RSSI", MySqlDbType.VarChar, 255);


                cmd.Parameters["@Book_id"].Value = Guid.NewGuid().ToString();
                cmd.Parameters["@Book_Title"].Value = Book_Title;
                cmd.Parameters["@Book_Autor"].Value = Book_Autor;
                cmd.Parameters["@Book_Genre"].Value = Book_Genre;
                cmd.Parameters["@Book_Image"].Value = source;
                cmd.Parameters["@Book_RFID_EPC"].Value = EPC;
                cmd.Parameters["@Book_RFID_TimeStamp"].Value = TimeStamp;
                cmd.Parameters["@Book_RFID_RSSI"].Value = RSSI;

                if (cmd.ExecuteNonQuery() == 1)
                {
                    MessageBox.Show("nice job");
                }
                connection.Close();
                UpdateBookTable();
            }

        }

        public static void RemoveBookQuery(string EPC) {
            connection.Open();
            String query = "DELETE FROM books Where Book_RFID_EPC = '" + EPC+ "'";
            if (connection != null)
            {
                cmd = new MySqlCommand(query, connection);

                if (cmd.ExecuteNonQuery() == 1)
                {
                    MessageBox.Show("nice job");
                }
                connection.Close();
                UpdateBookTable();
            }
        }


        public static DataTable GetDT() {
            
            foreach (DataRow dataRow in dt.Rows)
            {
                foreach (var item in dataRow.ItemArray)
                {
                    Console.WriteLine(item);
                }
            }
            return dt;
        }


        public static MySqlCommand RunQuery(string query)
        {
            try
            {
                if (connection != null)
                {
                    connection.Open();
                    cmd = connection.CreateCommand();
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = query;

                }

            }
            catch (Exception ex)
            {
                connection.Close();
                MessageBox.Show("Error");
            }
            return cmd;
        }
    }

}
