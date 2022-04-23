using PMService2.Model;
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
        List<Message> FindDirectMessagesFor(string sender, DateTime lastMessage);

        [OperationContract]
        bool NewMessage(Message message);

        [OperationContract]
        bool RequestState(string DeviceID);

        [OperationContract]
        bool CheckNewMessagesFor(string username, DateTime latestMessageTime);

        [OperationContract]
        bool DeleteMessagesFrom(string sender, string receiver);

        [OperationContract]
        bool CreateProject(Project project);

        /// <summary>
        /// Project ID connot be changed. So, We can get all the details as a single project Object and match with the project ID
        /// </summary>
        /// <param name="project"></param>
        /// <returns></returns>
        [OperationContract]
        bool UpdateProject(Project project, string username);

        /// <summary>
        /// Deletes the project from the database
        /// </summary>
        /// <param name="projectID">What Project</param>
        /// <param name="computerID">Which computer is requesting this action</param>
        /// <param name="userID">Name of the person that requests this change</param>
        /// <returns></returns>
        [OperationContract]
        bool DeleteProject(string projectID, string username);

        [OperationContract]
        List<Project> FetchAllProjects(string username);

        [OperationContract]
        List<Project> SyncAllProjects(string username);
    }
}
