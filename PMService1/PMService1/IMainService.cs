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
        [OperationContract]
        bool ValidateLogin(String email, String password);

        [OperationContract]
        bool SetUserStatus(User user, Status status);

        [OperationContract]
        Status? GetUserStatus(string email);

        [OperationContract]
        bool RegisterUser(string email, string name, string password, byte[] image);
        
    }
}
