using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace PCClient.Model
{
    public class DataStore
    {

        public static String ApplicationName_Acronym { get; set; } = "Projent";
        public static String ApplicationName_full { get; set; } = "Collaborative Project Mnagement Platform";

        public static String UserDBName { get; set; } = "PMUser.db"; // Server Function

        private static List<User> syncedUsers = new List<User>();

        public static bool CheckConnectivity()
        {
            if (NetworkInterface.GetIsNetworkAvailable())
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Create the local database if not exists. This will be save on the application folder.
        /// </summary>
        public async static void IntializeLocalDatabase()
        {
            try
            {
                await ApplicationData.Current.LocalFolder.CreateFileAsync("PMUser.db", CreationCollisionOption.OpenIfExists);
                string pathTODB = Path.Combine(ApplicationData.Current.LocalFolder.Path, "PMUser.db");

                using (SqliteConnection con = new SqliteConnection($"filename={pathTODB}"))
                {
                    con.Open();
                    string dbScript = "CREATE TABLE IF NOT EXISTS " +
                    "user (" +
                        "Email TEXT, " +
                        "Name TEXT, " +
                        "ImagePath TEXT, " +
                        "Password TEXT );";

                    SqliteCommand sqliteCommand = new SqliteCommand(dbScript, con);
                    sqliteCommand.ExecuteNonQuery();

                    con.Close();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString(), "ERROR");


            }

        }

        /// <summary>
        /// Download the database from the server as a backup
        /// </summary>
        public static void SyncServerDatabase()
        {
            List<User> users = new List<User>();

            string pathToDB  = Path.Combine(ApplicationData.Current.LocalFolder.Path, UserDBName);

            using (SqliteConnection con = new SqliteConnection($"filename={pathToDB}"))
            {
                if ( con.State == System.Data.ConnectionState.Closed)
                    con.Open();

                string dbScript = "SELECT Email, Name, ImagePath, Password FROM user";
                SqliteCommand sqliteCommand = new SqliteCommand(dbScript, con);
                SqliteDataReader reader = sqliteCommand.ExecuteReader();

                while (reader.Read())
                {
                    users.Add(new User() { Email = reader.GetString(0), Name = reader.GetString(1), Image = reader.GetString(2), Password = reader.GetString(3) });
                }

                syncedUsers = users;

                con.Close();
            }
        }

        public static bool ValidateUser(string email, string password)
        {
            // Replace with this the server function
            return Server_ValidateUser(email, password);
        }

        public static User FindUser(string email, string password)
        {
            return null;
        }

        #region PMServer1Functions

        /// <summary>
        /// Validate the user wether exists on the database with the entered username and password
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static bool Server_ValidateUser(string email, string password)
        {
            foreach (User user in syncedUsers)
            {
                if (user.Email == email && user.Password == password.ToUpper())
                    return true;
            }

            return false;
        }

        public static bool Server_IsUserRegistered(string email)
        {
            foreach (User user in syncedUsers)
            {
                if (user.Email == email)
                    return true;
            }

            return false;
        }
        #endregion
    }
}
