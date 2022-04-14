﻿using PMService2.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace PMService2
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IProjectService" in both code and config file together.
    [ServiceContract]
    public interface IProjectService
    {
        [OperationContract]
        void IntializeDatabaseService();

        [OperationContract]
        List<Message> FindDirectMessagesFor(string loggedUser, Message lastMessage);

        [OperationContract]
        bool NewMessage(Message message);
    }
}
