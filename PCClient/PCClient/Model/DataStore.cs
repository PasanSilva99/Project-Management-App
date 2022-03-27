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
        /// Validates user wether entered details are corrent - Comes from the server
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static bool ValidateUser(string email, string password)
        {
            // Replace with this the server function
            return Server_ValidateUser(email, password);
        }

        /// <summary>
        /// Find and get the user account
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static User FindUser(string email, string password)
        {

            return null;
        }

        /// <summary>
        /// Check wether the entered emaill address is already in the database - checks from the server
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public static bool IsUserRegistered(string email)
        {
            return Server_IsUserRegistered(email);
        }




        // ===================================================================================================== //
        // ================================== Will work as PMServer1 temoprary ================================= //
        // ===================================================================================================== //
        #region PMServer1Functions

        public static String UserDBName { get; set; } = "PMUser.db"; // Server
        private static List<User> lodedUsers = new List<User>();


        /// <summary>
        /// Add the user to the database
        /// </summary>
        /// <param name="email">Email Address</param>
        /// <param name="name">Name of the user</param>
        /// <param name="image">Image path for the reference image</param>
        /// <param name="password">Password Hash</param>
        /// <returns></returns>
        public static bool RegisterUser(string email, string name, string image, string password)
        {
            if (!string.IsNullOrEmpty(email) && 
                !string.IsNullOrEmpty(name) &&
                !string.IsNullOrEmpty(image) &&
                !string.IsNullOrEmpty(password))
            {
                string pathToDB = Path.Combine(ApplicationData.Current.LocalFolder.Path, UserDBName);

                using (SqliteConnection con = new SqliteConnection(pathToDB))
                {
                    try
                    {
                        con.Open();

                        SqliteCommand cmd = con.CreateCommand();
                        cmd.CommandText = "INSERT INTO Item VALUES(@email, @name, @image, @password);";
                        cmd.Parameters.AddWithValue("@emal", email);
                        cmd.Parameters.AddWithValue("@name", name);
                        cmd.Parameters.AddWithValue("@image", image);
                        cmd.Parameters.AddWithValue("@password", password);
                        cmd.Connection = con;
                        cmd.ExecuteNonQuery();
                        return true;
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                        
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Fetch all users
        /// </summary>
        /// <returns></returns>
        public static List<User> FetchUsers()
        {
            // sync users 
            List<User> users = new List<User>();

            string pathToDB = Path.Combine(ApplicationData.Current.LocalFolder.Path, UserDBName);

            using (SqliteConnection con = new SqliteConnection($"filename={pathToDB}"))
            {
                if (con.State == System.Data.ConnectionState.Closed)
                    con.Open();

                string dbScript = "SELECT Email, Name, ImagePath, Password FROM user";
                SqliteCommand sqliteCommand = new SqliteCommand(dbScript, con);
                SqliteDataReader reader = sqliteCommand.ExecuteReader();

                while (reader.Read())
                {
                    users.Add(new User() { Email = reader.GetString(0), Name = reader.GetString(1), Image = reader.GetString(2), Password = reader.GetString(3) });
                }

                lodedUsers = users;

                con.Close();
            }

            return users;
        }

        /// <summary>
        /// Validate the user wether exists on the database with the entered username and password
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static bool Server_ValidateUser(string email, string password)
        {
            FetchUsers();
            foreach (User user in lodedUsers)
            {
                if (user.Email == email && user.Password == password.ToUpper())
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Check wether the entered emaill address is already in the database
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public static bool Server_IsUserRegistered(string email)
        {
            FetchUsers();
            foreach (User user in lodedUsers)
            {
                if (user.Email == email)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Get the user
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static User GetUSer(string email, string password)
        {
            FetchUsers();
            foreach (User user in lodedUsers)
            {
                if (user.Email == email && user.Password == password.ToUpper())
                    return user;
            }

            return null;
        }
        #endregion
        // ===================================================================================================== //
    }
}
