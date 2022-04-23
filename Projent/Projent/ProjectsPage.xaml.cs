using Projent.Model;
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

namespace Projent
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ProjectsPage : Page
    {
        private NavigationBase basePage;
        public static PMServer2.Project Selectedproject;

        public ProjectsPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            basePage = e.Parameter as NavigationBase;

            Loadprojects();
        }

        private async void Loadprojects()
        {
            grid_projectsLoading.Visibility = Visibility.Visible;
            list_projects.Items.Clear();
            var projectList = await DataStore.FetchAllProjectsAsync();

            foreach (var project in projectList)
            {
                ProjectListViewItemControl projectListViewItemControl = new ProjectListViewItemControl();
                projectListViewItemControl.ProjectName = project.Title;
                projectListViewItemControl.ProjectDescription = project.Description;
                projectListViewItemControl.ProjectDate = project.EndDate;
                projectListViewItemControl.Manager = project.ProjectManager;
                projectListViewItemControl.Asignees = project.Assignees.ToList();

                ListViewItem projectListViewItem = new ListViewItem();
                projectListViewItem.Style = Resources["ProjectListItem"] as Style;
                projectListViewItem.Tag = project;
                projectListViewItem.Content = projectListViewItemControl;
                projectListViewItem.CornerRadius = new CornerRadius(10.0, 10.0, 10.0, 10.0);
                projectListViewItem.Tapped += ProjectListViewItem_Tapped;
                projectListViewItem.DoubleTapped += ProjectListViewItem_DoubleTapped;
                projectListViewItem.RightTapped += ProjectListViewItem_RightTapped;

                list_projects.Items.Add(projectListViewItem);
            }
            grid_projectsLoading.Visibility = Visibility.Collapsed;
            if(projectList.Count == 0)
            {
                TextBlock textBlock = new TextBlock();
                textBlock.Text = "📪 No Projects Available";
                textBlock.TextAlignment = TextAlignment.Center;
                textBlock.Opacity = 0.6;

                ListViewItem projectListViewItem = new ListViewItem();
                projectListViewItem.Style = Resources["ProjectListItem"] as Style;
                projectListViewItem.Content = textBlock;

                list_projects.Items.Add(projectListViewItem);
            }
        }

        private void ProjectListViewItem_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            var listItem = sender as ListViewItem;
            var project = listItem.Tag as PMServer2.Project;
        }

        private void ProjectListViewItem_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            var listItem = sender as ListViewItem;
            var project = listItem.Tag as PMServer2.Project;
            Selectedproject = project;
            SetTopNavigation();
            Debug.WriteLine($"Selected project {project.ProjectId}");
            basePage.ExternalNavigateRequst(this, typeof(ProjectViews.OverviewPage), 0);
        }

        private void ProjectListViewItem_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var listItem = sender as ListViewItem;
            var project = listItem.Tag as PMServer2.Project;
            Selectedproject = project;
            SetTopNavigation();
            Debug.WriteLine($"Selected project {project.ProjectId}");
        }

        private void SetTopNavigation()
        {
            List<NavigatorTag> TopNavigationItems = new List<NavigatorTag>();

            TopNavigationItems.Add(new NavigatorTag() { Name = "Overview", TagetPage = typeof(ProjectViews.OverviewPage) });
            TopNavigationItems.Add(new NavigatorTag() { Name = "Discussion", TagetPage = typeof(ProjectViews.DiscussionPage) });
            TopNavigationItems.Add(new NavigatorTag() { Name = "Tasks", TagetPage = typeof(ProjectViews.TasksPage) });
            TopNavigationItems.Add(new NavigatorTag() { Name = "Gantt", TagetPage = typeof(ProjectViews.GanttPage) });
            TopNavigationItems.Add(new NavigatorTag() { Name = "Calendar", TagetPage = typeof(ProjectViews.CalendarPage) });
            TopNavigationItems.Add(new NavigatorTag() { Name = "Notes", TagetPage = typeof(ProjectViews.NotesPage) });
            TopNavigationItems.Add(new NavigatorTag() { Name = "Files", TagetPage = typeof(ProjectViews.FilesPage) });

            basePage.SetTopNavigation(TopNavigationItems);
        }
    }
}
