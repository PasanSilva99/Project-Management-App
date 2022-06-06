using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Projent.ProjectViews
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class OverviewPage : Page
    {
        private NavigationBase basePage;

        public OverviewPage()
        {

            this.InitializeComponent();

        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            basePage = e.Parameter as NavigationBase;

            lbl_projectName.Text = ProjectsPage.Selectedproject.Title;
            lbl_projectDescription.Text = ProjectsPage.Selectedproject.Description;
            lbl_category.Text = ProjectsPage.Selectedproject.Category;
            lbl_createdOn.Text = ProjectsPage.Selectedproject.StartDate.ToString("d");
            lbl_endDate.Text = ProjectsPage.Selectedproject.EndDate.ToString("d");
            lbl_startDate.Text = ProjectsPage.Selectedproject.StartDate.ToString("d");
            lbl_status.Text = ProjectsPage.Selectedproject.Status;

            SetProjetcOwner(ProjectsPage.Selectedproject.CreatedBy);
            SetAssignees(ProjectsPage.Selectedproject.Assignees);
            SetProjectProgress(ProjectsPage.Selectedproject.ProjectId);
        }

        private async void SetProjectProgress(string prID)
        {
            var projectProgress = await Server.ProjectServer.projectServiceClient.GetProjectProgressAsync(prID);
            lbl_prg_overall.Text = projectProgress.ToString() + "%";
            ring_overallProgress.Value = projectProgress;

            // Not impl yet
            lbl_issues_fixed.Text = "0%";
            lbl_issues_todo.Text = "0%";
            lbl_prg_issues.Text = "0%";
            lbl_prg_tasks.Text = "0%";
            lbl_tasks_todo.Text = "0";
            lbl_task_compl.Text = "0";
            ring_issues.Value = 100.00;
            ring_tasks.Value = 100.00;
        }

        private void SetProjetcOwner(string username)
        {
            user_owner.UserName = username;
            user_owner.PermLevel = 0;
        }

        private void SetAssignees(System.Collections.ObjectModel.ObservableCollection<string> assignees)
        {
            stack_assignees.Children.Clear();
            var assigneelist = new List<ProjectMemberControl>();
            foreach(string assignee in assignees)
            {
                var newAssigneeControl = new ProjectMemberControl();
                newAssigneeControl.UserName = assignee;
                if(ProjectsPage.Selectedproject.ProjectManager == assignee)
                {
                    newAssigneeControl.PermLevel = 1;
                }
                else
                {
                    newAssigneeControl.PermLevel = 2;
                }
                assigneelist.Add(newAssigneeControl);
            }

            foreach (var assignee in assigneelist.OrderBy(x => x.PermLevel))
            {
                stack_assignees.Children.Add(assignee);
            }
        }

        private void AllProjects_Click(object sender, RoutedEventArgs e)
        {
            basePage.NavigateToProjects();
        }
    }
}
