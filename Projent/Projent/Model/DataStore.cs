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
using Windows.Storage.Streams;
using Projent.PMServer1;

namespace Projent.Model
{
    public class DataStore
    {
        public static String DBName { get; set; } = "PMClientDB.db"; // Server
        private static List<User> lodedUsers = new List<User>();
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

        public static ServiceType GlobalServiceType { get; set; } = ServiceType.Online;

        public static String ApplicationName_Acronym { get; set; } = "Projent";  // Project name
        public static String ApplicationName_full { get; set; } = "Collaborative Project Mnagement Platform";  // Project description

        public static async Task<bool> SetUserStatus(User user, Status status)
        {
            try
            {
                if (user != null)
                {
                    PMServer1.User serverUser = new PMServer1.User() { Email = user.Email, Name = user.Name, Password = user.Password };

                    await Server.MainServer.mainServiceClient.SetUserStatusAsync(Converter.ToServerUser(user), Converter.ToServerStatus(status));
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return false;
            }
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
                await ApplicationData.Current.LocalFolder.CreateFileAsync(DBName, CreationCollisionOption.OpenIfExists);
                string pathTODB = Path.Combine(ApplicationData.Current.LocalFolder.Path, DBName);

                using (SqliteConnection con = new SqliteConnection($"filename={pathTODB}"))
                {
                    con.Open();
                    string dbScript = "CREATE TABLE IF NOT EXISTS " +
                    "user (" +
                        "Email TEXT, " +
                        "Name TEXT, " +
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
                        "User TEXT," +
                        "Email TEXT, " +
                        "Name TEXT); ";

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
                
                return await Server.MainServer.mainServiceClient.RegisterUserAsync(email, name, image, password);

            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }
            return false;
        }

        public static bool RegisterUserLocal(string email, string name, string imageName, string password)
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
                        cmd.CommandText = "INSERT INTO user VALUES(@email, @name, @password);";
                        cmd.Parameters.AddWithValue("@email", email);
                        cmd.Parameters.AddWithValue("@name", name);
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
        public async static Task<bool> ValidateUser(string email, string password)
        {
            if (CheckConnectivity())
            {
                // Replace with this the server function
                return await Server.MainServer.mainServiceClient.ValidateUserAsync(email, password);
            }
            else
            {
                Debug.WriteLine("Connectivity Error", "ERROR");
                return false;
            }
        }

        /// <summary>
        /// This will fetch all the users from the local database and save it to the lodedUsers List
        /// </summary>
        /// <returns></returns>
        public static List<User> FetchUsers()
        {
            // sync users 
            List<User> users = new List<User>();

            string pathToDB = Path.Combine(ApplicationData.Current.LocalFolder.Path, DBName);

            using (SqliteConnection con = new SqliteConnection($"filename={pathToDB}"))
            {
                if (con.State == System.Data.ConnectionState.Closed)
                    con.Open();

                string dbScript = "SELECT Email, Name, Password FROM user";
                SqliteCommand sqliteCommand = new SqliteCommand(dbScript, con);
                SqliteDataReader reader = sqliteCommand.ExecuteReader();

                while (reader.Read())
                {
                    users.Add(new User() { Email = reader.GetString(0), Name = reader.GetString(1), Password = reader.GetString(2) });
                }

                lodedUsers = users;

                con.Close();
            }

            return users;
        }

        /// <summary>
        /// This will validate the user from the local database. If the user is valid, The return will be true.
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static bool ValidateUserLocal(string email, string password)
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
        /// Find and get the user account
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public async static Task<User> FindUser(string email, string password)
        {
            if (CheckConnectivity())
            {
                if (!string.IsNullOrWhiteSpace(email) && !string.IsNullOrWhiteSpace(password))
                {
                    return Converter.ToLocalUser(await Server.MainServer.mainServiceClient.GetUserAsync(email, password));
                }
                else
                {
                    Debug.WriteLine("Error in the input data", "ERROR");
                }
            }
            return null;
        }

        /// <summary>
        /// This find user from the local database
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static User FindUserLocal(string email, string password)
        {
            FetchUsers();
            foreach (User user in lodedUsers)
            {
                if (user.Email == email && user.Password == password.ToUpper())
                    return user;
            }

            return null;
        }

        /// <summary>
        /// Check wether the entered emaill address is already in the database - checks from the server
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public async static Task<bool> IsUserRegistered(string email)
        {
            if (CheckConnectivity())
            {
                Debug.WriteLine("Checking User");
                return await Server.MainServer.mainServiceClient.IsUserRegisteredAsync(email);
            }
            else
            {
                Debug.WriteLine("Connectivity Error", "ERROR");
                FetchUsers();
                foreach (User user in lodedUsers)
                {
                    if (user.Email == email)
                        return true;
                }
                Debug.WriteLine("UsedLocalDB");
                return false;
            }
        }

        public async static Task<bool> SaveDashboardBGAsync(User user, byte[] imageBuffer)
        {
            try
            {
                if (CheckConnectivity())
                {
                    //return await Server.MainServer.mainServiceClient.SaveDashboardImageAsync(user, imageBuffer);
                    return false;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public static async Task<bool> UpdateUserLocally(User loggedUser, User tempUser)
        {
            if (lodedUsers != null && tempUser != null)
            {
                string pathToDB = Path.Combine(ApplicationData.Current.LocalFolder.Path, DBName);

                using (SqliteConnection con = new SqliteConnection($"filename={pathToDB}"))
                {
                    try
                    {
                        con.Open();

                        SqliteCommand cmd = con.CreateCommand();
                        cmd.CommandText = "UPDATE user SET Email=@email, Name=@name, Password=@password WHERE Email=@cemail AND Password=@cpassword";
                        cmd.Parameters.AddWithValue("@email", tempUser.Email);
                        cmd.Parameters.AddWithValue("@name", tempUser.Name);
                        cmd.Parameters.AddWithValue("@password", tempUser.Password.ToUpper());
                        cmd.Parameters.AddWithValue("@cemail", loggedUser.Email);
                        cmd.Parameters.AddWithValue("@cpassword", loggedUser.Password);
                        cmd.Connection = con;
                        var affectedRows = cmd.ExecuteNonQuery();

                        if (loggedUser.Name != tempUser.Name)
                        {
                            StorageFolder storageFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("ProfilePics", CreationCollisionOption.OpenIfExists);
                            StorageFile userImageFile = await storageFolder.GetFileAsync(loggedUser.Name + ".png");
                            await userImageFile.CopyAsync(storageFolder, tempUser.Name + ".png", NameCollisionOption.ReplaceExisting);
                            await userImageFile.DeleteAsync();
                        }


                        if (affectedRows != 0) return true;
                        else return false;
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                    }
                }
            }

            return false;
        }

        internal static async Task<bool> UpdateUser(User loggedUser, User tempUser)
        {
            StorageFolder storageFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("ProfilePics", CreationCollisionOption.OpenIfExists);
            StorageFile userImageFile = await storageFolder.GetFileAsync(loggedUser.Name + ".png");

            var image = await SaveToBytesAsync(userImageFile);

            try
            {
                await UpdateUserLocally(loggedUser, tempUser);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            return await Server.MainServer.mainServiceClient.UpdateUserAsync(Converter.ToServerUser(loggedUser), Converter.ToServerUser(tempUser), image);

        }

        // Pasan - C:\Users\pasan\AppData\Local\Packages\b112076d-ff1b-4f40-bf4a-497cdcf4cc3c_r36wv3zd9209g\LocalState\

        public static async Task<byte[]> SaveToBytesAsync(StorageFile file)
        {
            IRandomAccessStream filestream = await file.OpenAsync(FileAccessMode.Read);
            var reader = new DataReader(filestream.GetInputStreamAt(0));
            await reader.LoadAsync((uint)filestream.Size);

            byte[] pixels = new byte[filestream.Size];

            reader.ReadBytes(pixels);

            return pixels;
        }

        internal static List<DirectUser> FetchDirectUsers(string user)
        {
            List<DirectUser> users = new List<DirectUser>();

            string pathToDB = Path.Combine(ApplicationData.Current.LocalFolder.Path, DBName);

            using (SqliteConnection con = new SqliteConnection($"filename={pathToDB}"))
            {
                if (con.State == System.Data.ConnectionState.Closed)
                    con.Open();

                string dbScript = "SELECT Email, Name FROM DirectUsers WHERE User=@user";
                SqliteCommand sqliteCommand = new SqliteCommand(dbScript, con);
                sqliteCommand.Parameters.AddWithValue("@user", user);
                SqliteDataReader reader = sqliteCommand.ExecuteReader();

                while (reader.Read())
                {
                    users.Add(new DirectUser() { Email = reader.GetString(0), Name = reader.GetString(1) });
                }

                con.Close();
            }

            return users;
        }

        public static bool NewDirectUser(DirectUser directUser, string addedBy)
        {
            if (!string.IsNullOrEmpty(directUser.Name) &&
                !string.IsNullOrEmpty(directUser.Email))
            {
                string pathToDB = Path.Combine(ApplicationData.Current.LocalFolder.Path, DBName);

                using (SqliteConnection con = new SqliteConnection($"filename={pathToDB}"))
                {
                    try
                    {
                        con.Open();

                        SqliteCommand cmd = con.CreateCommand();
                        cmd.CommandText = "INSERT INTO DirectUsers VALUES(@user, @email, @name);";
                        cmd.Parameters.AddWithValue("@user", addedBy);
                        cmd.Parameters.AddWithValue("@email", directUser.Email);
                        cmd.Parameters.AddWithValue("@name", directUser.Name);
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

        public static bool RemoveDirectUser(DirectUser directUser, string removeFrom)
        {
            if (!string.IsNullOrEmpty(directUser.Name))
            {
                string pathToDB = Path.Combine(ApplicationData.Current.LocalFolder.Path, DBName);

                using (SqliteConnection con = new SqliteConnection($"filename={pathToDB}"))
                {
                    try
                    {
                        con.Open();

                        SqliteCommand cmd = con.CreateCommand();
                        cmd.CommandText = "DELETE FROM DirectUsers Where Name=@name AND User=@user";
                        cmd.Parameters.AddWithValue("@name", directUser.Name);
                        cmd.Parameters.AddWithValue("@user", removeFrom);
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

        public static string GetDefaultMacAddress()
        {
            Dictionary<string, long> macAddresses = new Dictionary<string, long>();
            foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (nic.OperationalStatus == OperationalStatus.Up)
                    macAddresses[nic.GetPhysicalAddress().ToString()] = nic.GetIPStatistics().BytesSent + nic.GetIPStatistics().BytesReceived;
            }
            long maxValue = 0;
            string mac = "";
            foreach (KeyValuePair<string, long> pair in macAddresses)
            {
                if (pair.Value > maxValue)
                {
                    mac = pair.Key;
                    maxValue = pair.Value;
                }
            }
            return mac;
        }
    }
}

