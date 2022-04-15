using PMService1.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using static PMService1.Model.DataStore;

namespace PMService1
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IMainService" in both code and config file together.
    [ServiceContract]
    public interface IMainService
    {
        /// <summary>
        /// Initialize the server database
        /// </summary>
        [OperationContract]
        void IntializeDatabaseService();

        /// <summary>
        /// Gets the SQLite version
        /// </summary>
        /// 
        [OperationContract]
        void GetSqliteVersion();

        [OperationContract]
        bool RequestState(string DeviceID);

        [OperationContract]
        bool SetUserStatus(User user, Status status);

        [OperationContract]
        Status? GetUserStatus(string email);

        [OperationContract]
        bool RegisterUser(string email, string name, byte[] imageBuffer, string password);

        [OperationContract]
        List<User> FetchUsers();

        [OperationContract]
        bool ValidateUser(string email, string password);

        [OperationContract]
        bool IsUserRegistered(string email);

        [OperationContract]
        User GetUser(string email, string password);

        [OperationContract]
        bool UpdateUser(User loggedUser, User tempUser, byte[] image);

        [OperationContract]
        byte[] RequestUserImage(string username);
    }
}
