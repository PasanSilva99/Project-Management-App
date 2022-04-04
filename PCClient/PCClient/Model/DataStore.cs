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
using System.Security.Cryptography;
using Windows.UI.Xaml;

namespace PCClient.Model
{
    public class DataStore
    {
        public static String DBName { get; set; } = "PMClientDB.db"; // Server
        public enum Status
        {
            Offline,
            Online,
            Idle,
            Busy,
            Invisible
        }
        public enum ServiceType
        {
            Online,
            Offline
        }

        private static byte[] GetHash(string inputString)
        {
            using (HashAlgorithm algorithm = SHA256.Create())
                return algorithm.ComputeHash(Encoding.UTF8.GetBytes(inputString));
        }

        public static string GetHashString(string inputString)
        {
            StringBuilder sb = new StringBuilder();
            foreach (byte b in GetHash(inputString))
                sb.Append(b.ToString("X2"));

            return sb.ToString();
        }

        public static ServiceType GlobalServiceType { get; set; }

        public static String ApplicationName_Acronym { get; set; } = "Projent";  // Project name
        public static String ApplicationName_full { get; set; } = "Collaborative Project Mnagement Platform";  // Project description

        public static bool SetUserStatus(User user, Status status)
        {
            Server.PMServer1.SetUserStatus(user, (Server.PMServer1.Status)status);
            return true;
        }

        /// <summary>
        /// Checks wether the computer is connected to any network
        /// </summary>
        /// <returns></returns>
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
                await ApplicationData.Current.LocalFolder.CreateFileAsync(DBName , CreationCollisionOption.OpenIfExists);
                string pathTODB = Path.Combine(ApplicationData.Current.LocalFolder.Path, DBName);

                using (SqliteConnection con = new SqliteConnection($"filename={pathTODB}"))
                {
                    con.Open();
                    string dbScript = "CREATE TABLE IF NOT EXISTS " +
                    "user (" +
                        "Email TEXT, " +
                        "Name TEXT, " +
                        "ImagePath TEXT, " +
                        "Password TEXT ); " +
                    "CREATE TABLE IF NOT EXISTS " +
                        "chat (" +
                        "SentOn TEXT, " +       // The date sent
                        "Sender TEXT, " +       // Auther of the message
                        "ProjectID TEXT, " +    // if the message is on the project discussion, the project id
                        "ReceiverID TEXT, " +   // If it is a direct message, the ID of the receiver (Email)
                        "Content TEXT, " +      // Message body
                        "IsEmogi INTEGER ); " + // is this a emogi / Sticker
                        "CREATE TABLE IF NOT EXISTS " +  
                    "DirectUsers (" +   // Saved Direct users
                        "Email TEXT, " +    
                        "Name TEXT, " +
                        "ImagePath TEXT); ";

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
        /// Sync the local database with the server
        /// </summary>
        public static void SyncWithTheServer()
        {

        }

        /// <summary>
        /// Add the user to the database - With the server and locally
        /// </summary>
        /// <param name="email">Email Address</param>
        /// <param name="name">Name of the user</param>
        /// <param name="image">Image path for the reference image</param>
        /// <param name="password">Password Hash</param>
        /// <returns></returns>
        public static async Task<bool> RegisterUserAsync(string email, string name, byte[] image, string password)
        {
            try
            {
                // Will be replaced after stabilize the server
                return await Server.PMServer1.RegisterUserAsync(email, name, image, password);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }
            return false;
        }

        public static bool RegisterUserLocalAsync(string email, string name, string imageName, string password)
        {
            if (!string.IsNullOrEmpty(email) &&
                !string.IsNullOrEmpty(name) &&
                !string.IsNullOrEmpty(imageName) &&
                !string.IsNullOrEmpty(password))
            {
                string pathToDB = Path.Combine(ApplicationData.Current.LocalFolder.Path, DBName);

                using (SqliteConnection con = new SqliteConnection($"filename={pathToDB}"))
                {
                    try
                    {
                        con.Open();

                        SqliteCommand cmd = con.CreateCommand();
                        cmd.CommandText = "INSERT INTO user VALUES(@email, @name, @image, @password);";
                        cmd.Parameters.AddWithValue("@email", email);
                        cmd.Parameters.AddWithValue("@name", name);
                        cmd.Parameters.AddWithValue("@image", imageName);
                        cmd.Parameters.AddWithValue("@password", password.ToUpper());
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
        /// Validates user wether entered details are corrent - Comes from the server
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static bool ValidateUser(string email, string password)
        {
            if (CheckConnectivity())
            {
                // Replace with this the server function
                return Server.PMServer1.ValidateUser(email, password);
            }
            else
            {
                Debug.WriteLine("Connectivity Error", "ERROR");
                return false;
            }
        }

        /// <summary>
        /// Find and get the user account
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static User FindUser(string email, string password)
        {
            if (CheckConnectivity())
            {
                if (!string.IsNullOrWhiteSpace(email) && !string.IsNullOrWhiteSpace(password))
                {
                    return Server.PMServer1.GetUser(email, password);
                }
                else
                {
                    Debug.WriteLine("Error in the input data", "ERROR");
                }
            }
            return null;
        }

        /// <summary>
        /// Check wether the entered emaill address is already in the database - checks from the server
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public static bool IsUserRegistered(string email)
        {
            if (CheckConnectivity())
            {
                //Debug.WriteLine("Checking User");
                return Server.PMServer1.IsUserRegistered(email);
            }
            else
            {
                Debug.WriteLine("Connectivity Error", "ERROR");
                return false;
            }
        }

        public async static Task<bool> SaveDashboardBGAsync(User user, byte[] imageBuffer)
        {
            try
            {
                if (CheckConnectivity())
                {
                    return await Server.PMServer1.SaveDashboardImage(user, imageBuffer);
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

    }
}
