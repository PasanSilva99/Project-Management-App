using Projent.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Projent.Server
{
    public class MainServer
    {
        public static PMServer1.MainServiceClient mainServiceClient = null;
        public async static void InitializeServer()
        {
            try
            {
                mainServiceClient = new PMServer1.MainServiceClient(PMServer1.MainServiceClient.EndpointConfiguration.BasicHttpBinding_IMainService);
                Debug.WriteLine("PMServer1 Client Started");
                await mainServiceClient.RequestStateAsync(DataStore.GetDefaultMacAddress());
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }
    }
}
