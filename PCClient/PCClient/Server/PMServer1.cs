using Microsoft.Data.Sqlite;
using PCClient.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml;

namespace PCClient.Server
{
    public class PMServer1
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
        // ===================================================================================================== //
        // ================================== Will work as PMServer1 temoprary ================================= //
        // ===================================================================================================== //
        #region PMServer1Functions

        public static String DBName { get; set; } = "PMService1DB.db"; // Server
        private static List<User> lodedUsers = new List<User>();
        private static List<UserStatus> undedUsers = new List<UserStatus>();

        struct UserStatus
        {
            public User User { get; set; }
            public Status Status { get; set; }
        }

        public async static void IntializeDatabaseService1()
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

        public static bool SetUserStatus(User user, Status status)
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

        public static Status? GetUserStatus(string email)
        {
            foreach (UserStatus ustatus in undedUsers)
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
        public static async Task<bool> RegisterUserAsync(string email, string name, byte[] imageBuffer, string password)
        {
            if (!string.IsNullOrEmpty(email) &&
                !string.IsNullOrEmpty(name) &&
                !string.IsNullOrEmpty(password))
            {
                string pathToDB = Path.Combine(ApplicationData.Current.LocalFolder.Path, DBName);

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

            string pathToDB = Path.Combine(ApplicationData.Current.LocalFolder.Path, DBName);

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
        public static bool ValidateUser(string email, string password)
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
        public static bool IsUserRegistered(string email)
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
        public static User GetUser(string email, string password)
        {
            FetchUsers();
            foreach (User user in lodedUsers)
            {
                if (user.Email == email && user.Password == password.ToUpper())
                    return user;
            }

            return null;
        }

        public async static Task<bool> SaveDashboardImage(User user, byte[] imageBuffer)
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

        internal static async Task<bool> UpdateUser(User loggedUser, User tempUser, byte[] image)
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
                        cmd.CommandText = "UPDATE user SET Email=@email, Name=@name, ImagePath=@image, Password=@password WHERE Email=@cemail AND Password=@cpassword";
                        cmd.Parameters.AddWithValue("@email", tempUser.Email);
                        cmd.Parameters.AddWithValue("@name", tempUser.Name);
                        cmd.Parameters.AddWithValue("@image", tempUser.Image);
                        cmd.Parameters.AddWithValue("@password", tempUser.Password.ToUpper());
                        cmd.Parameters.AddWithValue("@cemail", loggedUser.Email);
                        cmd.Parameters.AddWithValue("@cpassword", loggedUser.Password);
                        cmd.Connection = con;
                        var affectedRows = cmd.ExecuteNonQuery();

                        if (loggedUser.Image != tempUser.Image)
                        {
                            try
                            {
                                StorageFolder storageFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("ProfilePics", CreationCollisionOption.OpenIfExists);
                                StorageFile userImageFile = await storageFolder.GetFileAsync(loggedUser.Image);
                                await userImageFile.DeleteAsync();
                            }
                            catch (Exception _ex)
                            {

                            }

                            var ProfilePicFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("ProfilePicsServer", CreationCollisionOption.OpenIfExists);
                            var ProfilePicFile = await ProfilePicFolder.CreateFileAsync(tempUser.Image, CreationCollisionOption.ReplaceExisting);
                            await FileIO.WriteBytesAsync(ProfilePicFile, image);
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

        #endregion
        // ===================================================================================================== //
    }
}
