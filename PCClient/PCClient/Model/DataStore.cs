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
            Server_SetUserStatus(user, status);
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

            // This should be in the server constructor
            StartUserTimers();
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
                return await Server_RegisterUserAsync(email, name, image, password);
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
                string pathToDB = Path.Combine(ApplicationData.Current.LocalFolder.Path, UserDBName);

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
                return Server_ValidateUser(email, password);
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
                    return Server_GetUser(email, password);
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
                return Server_IsUserRegistered(email);
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
                    return await Server_SaveDashboardImage(user, imageBuffer);
                }
                return false;
            }
            catch
            {
                return false;
            }
        }


        // ===================================================================================================== //
        // ================================== Will work as PMServer1 temoprary ================================= //
        // ===================================================================================================== //
        #region PMServer1Functions

        public static String UserDBName { get; set; } = "PMUser.db"; // Server
        private static List<User> lodedUsers = new List<User>();
        private static List<UserStatus> undedUsers = new List<UserStatus>();

        struct UserStatus
        {
            public User User { get; set; }
            public Status Status { get; set; }
        }

        public static void StartUserTimers()
        {
            DispatcherTimer UserTimer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(30.0) };
            UserTimer.Tick += UserTimer_Tick;
            UserTimer.Start();
        }

        private static void UserTimer_Tick(object sender, object e)
        {
            FetchUsers();
            foreach (User user in lodedUsers)
            {
                undedUsers.Add(new UserStatus() { User = user, Status = Status.Offline });
            }
        }

        public static bool Server_SetUserStatus(User user, Status status)
        {
            FetchUsers();
            foreach (UserStatus ustatus in undedUsers)
            {
                if (ustatus.User == user)
                {
                    undedUsers.Remove(ustatus);
                    undedUsers.Add(new UserStatus() { User = user, Status = status });
                    return true;
                }
            }
            return false;
        }

        public static Status? Server_GetUserStatus(string email)
        {
            foreach(UserStatus ustatus in undedUsers)
            {
                if (ustatus.User.Email == email)
                {
                    return ustatus.Status;
                }
            }
            return null;
        }

        /// <summary>
        /// Add the user to the database
        /// </summary>
        /// <param name="email">Email Address</param>
        /// <param name="name">Name of the user</param>
        /// <param name="image">Image path for the reference image</param>
        /// <param name="password">Password Hash</param>
        /// <returns></returns>
        public static async Task<bool> Server_RegisterUserAsync(string email, string name, byte[] imageBuffer, string password)
        {
            if (!string.IsNullOrEmpty(email) &&
                !string.IsNullOrEmpty(name) &&
                !string.IsNullOrEmpty(password))
            {
                string pathToDB = Path.Combine(ApplicationData.Current.LocalFolder.Path, UserDBName);

                var ProfilePicFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("ProfilePicsServer", CreationCollisionOption.OpenIfExists);
                var ProfilePicFile = await ProfilePicFolder.CreateFileAsync(email + ".png", CreationCollisionOption.ReplaceExisting);

                await FileIO.WriteBytesAsync(ProfilePicFile, imageBuffer);

                using (SqliteConnection con = new SqliteConnection($"filename={pathToDB}"))
                {
                    try
                    {
                        con.Open();

                        SqliteCommand cmd = con.CreateCommand();
                        cmd.CommandText = "INSERT INTO user VALUES(@email, @name, @image, @password);";
                        cmd.Parameters.AddWithValue("@email", email);
                        cmd.Parameters.AddWithValue("@name", name);
                        cmd.Parameters.AddWithValue("@image", email + ".png");
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
                if (user.Email == email && user.Password.ToUpper() == password.ToUpper())
                {
                    Debug.WriteLine(user.Password + " :: " + password);
                    return true;
                }
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
        public static User Server_GetUser(string email, string password)
        {
            FetchUsers();
            foreach (User user in lodedUsers)
            {
                if (user.Email == email && user.Password == password.ToUpper())
                    return user;
            }

            return null;
        }

        public async static Task<bool> Server_SaveDashboardImage(User user, byte[] imageBuffer)
        {
            try
            {
                return true;
            }
            catch (Exception ex)
            {
                Debug.Write(ex.ToString());
                return false;
            }
        }

        #endregion
        // ===================================================================================================== //
    }
}
