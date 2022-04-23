using Microsoft.Data.Sqlite;
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

        internal static async Task SyncProjectsAsync()
        {
            if (DataStore.CheckConnectivity() && await projectServiceClient.RequestStateAsync(DataStore.GetDefaultMacAddress()))
            {
                var allProjectsFromServer = new List<PMServer2.Project>(await projectServiceClient.SyncAllProjectsAsync(MainPage.LoggedUser.Name));
                var allProjectsFromLocal = await DataStore.FetchAllLocalProjectsAsync();

                if (allProjectsFromServer != allProjectsFromLocal)
                {
                    var sortedServerProjects = allProjectsFromServer.OrderBy(p => p.ProjectId);
                    var sortedLocalProjects = allProjectsFromLocal.OrderBy(p => p.ProjectId);

                    // check for deteled projects 
                    var deletedProjects = sortedLocalProjects.Except(sortedServerProjects);

                    foreach (var project in deletedProjects)
                    {
                        // delete the project form the local db
                        await DataStore.DeleteProject(project.ProjectId);
                    }

                    // filter out deleted projects from the local projects
                    var filteredLocalProjects = new List<PMServer2.Project>();

                    for(int i = 0; i < sortedLocalProjects.Count(); i++)
                    {
                        if(sortedLocalProjects.ToArray()[i].ProjectId == sortedServerProjects.ToArray()[i].ProjectId)
                        {
                            filteredLocalProjects.Add(sortedLocalProjects.ToArray()[i]);
                        }
                    }

                    // check for new projects
                    var newProjects = sortedServerProjects.Except(filteredLocalProjects);
                    // save the new projects
                    foreach (PMServer2.Project project in newProjects)
                    {
                        // Create project
                        await DataStore.CreateProject(project);
                    }


                    // check for updates
                    for(int i = 0; i < filteredLocalProjects.Count(); i++)
                    {
                        var project = filteredLocalProjects[i];
                        var projectMatch = sortedServerProjects.FirstOrDefault(p => p.ProjectId == project.ProjectId);
                        if (project != projectMatch)
                        {
                            // update the project
                            await DataStore.UpdateLocalProject(projectMatch);
                        }
                    }


                }

            }



        }
    }
}
