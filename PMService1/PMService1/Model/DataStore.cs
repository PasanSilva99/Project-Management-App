using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PMService1.Model
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

        struct UserStatus
        {
            public User User { get; set; }
            public Status Status { get; set; }
        }

        static List<User> lodedUsers = new List<User>();

        public static void InitializeDatabase()
        {
            //Mock Data
            lodedUsers.Add(new User("lilyki@gmail.com", "Lily Ki", "81DC9BDB52D04DC20036DBD8313ED055"));
            lodedUsers.Add(new User("sophielandresse@gmail.com", "Sophie Landresse", "81DC9BDB52D04DC20036DBD8313ED055"));
            lodedUsers.Add(new User("marinlafitte@gmail.com", "Marin Lafitte", "81DC9BDB52D04DC20036DBD8313ED055"));
            lodedUsers.Add(new User("marianaganawa@gmail.com", "Maria Naganawa", "81DC9BDB52D04DC20036DBD8313ED055"));
            lodedUsers.Add(new User("christiecate@gmail.com", "Christie Cate", "81DC9BDB52D04DC20036DBD8313ED055"));
            lodedUsers.Add(new User("aoikoga@gmail.com", "Aoi Koga", "81DC9BDB52D04DC20036DBD8313ED055"));

        }

        public static List<User> GetUsers()
        {
            return lodedUsers;
        }


    }
}