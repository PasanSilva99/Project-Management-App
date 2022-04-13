using PMService1.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace PMService1
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "MainService" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select MainService.svc or MainService.svc.cs at the Solution Explorer and start debugging.
    public class MainService : IMainService
    {
        private List<User> usersCache = new List<User>();
        public MainService()
        {
            DataStore.InitializeDatabase();
            usersCache = DataStore.GetUsers();
        }

        public DataStore.Status? GetUserStatus(string email)
        {
            return DataStore.Status.Online;
        }

        public bool RegisterUser(string email, string name, string password, byte[] image)
        {
            return true;
        }

        public bool SetUserStatus(User user, DataStore.Status status)
        {
            return true;
        }

        public bool ValidateLogin(string email, string password)
        {
            // Create Mock Database
            // Check wether the user is available in the database
            // if user is in the database, Return True Else, Return False

            // Refresh Users Cache
            usersCache = DataStore.GetUsers();

            foreach (User user in usersCache)
            {
                if (user.Email == email && user.Password == password)
                {
                    return true;
                }
            }

            return false;

        }
    }
}
