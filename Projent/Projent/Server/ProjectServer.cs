using Projent.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projent.Server
{
    public class ProjectServer
    { 
        public static PMServer2.ProjectServiceClient projectServiceClient;
        public async static void InitializeServer()
        {
            try
            {
                projectServiceClient = new PMServer2.ProjectServiceClient(PMServer2.ProjectServiceClient.EndpointConfiguration.BasicHttpBinding_IProjectService);
                Debug.WriteLine("Project Client Initalized");
                await projectServiceClient.RequestStateAsync(DataStore.GetDefaultMacAddress());
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }
    }
}
