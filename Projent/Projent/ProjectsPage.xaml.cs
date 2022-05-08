using Microsoft.UI.Xaml.Controls;
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
        internal static InfoBar projectServerError;

        public ProjectsPage()
        {
            this.InitializeComponent();
            projectServerError = info_connectionFailed; // to access from other classes
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

            if(projectList == null)
            {
                projectList = new List<PMServer2.Project>();
            }

            // Get filters 
            var allProjects = (bool)tggl_allprojects.IsChecked;
            var createdByMe = (bool)tggl_CreatedByMe.IsChecked;
            var managedByme = (bool)tggl_managedByMe.IsChecked;
            var assignedToMe = (bool)tggl_assignedToMe.IsChecked;

            var selectedStatus = cmb_status.SelectedItem as ComboBoxItem;
            var status = "";
            if(selectedStatus != null)
                status = selectedStatus.Tag as string;



            var filteredProjects = new List<PMServer2.Project>();

            if(allProjects)
            {
                filteredProjects = projectList;
            }
            if (createdByMe)
            {
                filteredProjects = projectList.Where(x => x.CreatedBy == MainPage.LoggedUser.Name).ToList();
            }
            if (managedByme)
            {
                filteredProjects = projectList.Where(x => x.ProjectManager == MainPage.LoggedUser.Name).ToList();
            }
            if (assignedToMe)
            {
                filteredProjects = projectList.Where(x => x.Assignees.Contains(MainPage.LoggedUser.Name)).ToList();
            }

            var selectedSortMode = cmb_sort.SelectedItem as ComboBoxItem;
            var sortBy = "";

            if (selectedSortMode != null)
                sortBy = selectedSortMode.Tag as string;

            List<PMServer2.Project> sortedProjects = new List<PMServer2.Project>();

            switch (sortBy)
            {
                case "ProjectID": sortedProjects = filteredProjects.OrderBy(x => x.ProjectId).ToList();
                    break;
                case "Category":
                    sortedProjects = filteredProjects.OrderBy(x => x.Category).ToList();
                    break;
                case "Title":
                    sortedProjects = filteredProjects.OrderBy(x => x.Title).ToList();
                    break;
                case "Manager":
                    sortedProjects = filteredProjects.OrderBy(x => x.ProjectManager).ToList();
                    break;
                case "CreatedDate":
                    sortedProjects = filteredProjects.OrderBy(x => x.CreatedOn).ToList();
                    break;
                case "DueDate":
                    sortedProjects = filteredProjects.OrderBy(x => x.EndDate).ToList();
                    break;
                default:
                    sortedProjects = filteredProjects;
                    break ;
            }

            var statusfilteredProjects = new List<PMServer2.Project>();
            if (status != "All")
            {
                statusfilteredProjects = sortedProjects.Where(x => x.Status.Contains(status)).ToList();
            }
            else
            {
                statusfilteredProjects = sortedProjects;
            }
            cmb_category.Items.Clear();
            foreach (var project in sortedProjects)
            {
                if(!cmb_category.Items.Contains(project.Category))
                    cmb_category.Items.Add(project.Category);
            }

            var categoryFilteredProjects = new List<PMServer2.Project>();
            var categoryFilter = cmb_category.Text;
            if (!string.IsNullOrWhiteSpace(categoryFilter))
            {
                categoryFilteredProjects = statusfilteredProjects.Where(x => x.Category.Contains(categoryFilter)).ToList();
            }
            else
            {
                categoryFilteredProjects = statusfilteredProjects;
            }

            foreach (var project in categoryFilteredProjects)
            {

                MainProjectListViewItemControl projectListViewItemControl = new MainProjectListViewItemControl
                {
                    ProjectName = project.Title,
                    ProjectDescription = project.Description,
                    ProjectStatus = project.Status,
                    ProjectDate = project.EndDate,
                    Manager = project.ProjectManager,
                    Asignees = project.Assignees.ToList()
                };

                ListViewItem projectListViewItem = new ListViewItem();
                projectListViewItem.Style = Resources["ProjectListItem"] as Style;
                projectListViewItem.Tag = project;
                projectListViewItem.Content = projectListViewItemControl;
                projectListViewItem.CornerRadius = new CornerRadius(10.0, 10.0, 10.0, 10.0);
                FlyoutBase.SetAttachedFlyout(projectListViewItem, Resources["ProjectMenu"] as MenuFlyout);
                projectListViewItem.Tapped += ProjectListViewItem_Tapped;
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

            // set the flyout menu location manually
            var tappedListItem = (UIElement)e.OriginalSource;
            var attachedMenuFlyout = (MenuFlyout)FlyoutBase.GetAttachedFlyout(sender as ListViewItem);

            // open the flyout at the mouse location
            attachedMenuFlyout.ShowAt(tappedListItem, e.GetPosition(tappedListItem));


        }


        private void ProjectListViewItem_Tapped(object sender, TappedRoutedEventArgs e)
        {
            SetTopNavigation();
            var listItem = sender as ListViewItem;
            var project = listItem.Tag as PMServer2.Project;
            if (Selectedproject == project)
            {
                basePage.ExternalNavigateRequst(this, typeof(ProjectViews.OverviewPage), 1);
            }
            else
            {
                Selectedproject = project;
            }
            
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

        private void tggl_allprojects_Click(object sender, RoutedEventArgs e)
        {
            UpdateProjectFilters(sender as ToggleButton);
        }

        private void tggl_CreatedByMe_Click(object sender, RoutedEventArgs e)
        {
            UpdateProjectFilters(sender as ToggleButton);

        }

        private void tggl_managedByMe_Click(object sender, RoutedEventArgs e)
        {
            UpdateProjectFilters(sender as ToggleButton);

        }

        private void tggl_assignedToMe_Click(object sender, RoutedEventArgs e)
        {
            UpdateProjectFilters(sender as ToggleButton);

        }

        private void UpdateProjectFilters(ToggleButton selectedToggle)
        {
            tggl_allprojects.IsChecked = false;
            tggl_CreatedByMe.IsChecked = false;
            tggl_managedByMe.IsChecked = false;
            tggl_assignedToMe.IsChecked = false;

            selectedToggle.IsChecked = true;

            Loadprojects();
        }

        private void cmb_sort_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Loadprojects();
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (grid_projectsLoading != null)
                Loadprojects();
        }

        private void cmb_category_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Loadprojects();
        }

        private void btn_addProject_Click(object sender, RoutedEventArgs e)
        {
            basePage.OpenRightPanel(typeof(ProjectViews.CreateProject));
            basePage.loadedProjectPage = this;
        }

        private async void DeleteProject_Click(object sender, RoutedEventArgs e)
        {
            var selectedprojectID = Selectedproject.ProjectId;
            // send the delete request to the server
            try
            {
                if (await Server.ProjectServer.CheckConnectivity())
                {
                    var isSuccess = await Server.ProjectServer.projectServiceClient.DeleteProjectAsync(selectedprojectID, MainPage.LoggedUser.Name);
                    
                    if (isSuccess)
                    {
                        Loadprojects();
                    }
                    else
                    {
                        ContentDialog dialog = new ContentDialog();
                        dialog.Title = "Something Went Wrong";
                        dialog.PrimaryButtonText = "Retry";
                        dialog.CloseButtonText = "Cancel";
                        dialog.DefaultButton = ContentDialogButton.Close;
                        dialog.Content = "Failed to Delete the project";

                        var result = await dialog.ShowAsync();

                        if (result == ContentDialogResult.Primary)
                        {
                            DeleteProject_Click(sender, e);
                        }
                    }
                }
                else
                {
                    ContentDialog dialog = new ContentDialog();
                    dialog.Title = "Not Connected";
                    dialog.PrimaryButtonText = "Retry";
                    dialog.CloseButtonText = "Cancel";
                    dialog.DefaultButton = ContentDialogButton.Close;
                    dialog.Content = "Failed to connect to the server. You have to be connected with the server to create/ delete or edit projects";

                    var result = await dialog.ShowAsync();

                    if (result == ContentDialogResult.Primary)
                    {
                        DeleteProject_Click(sender, e);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                ContentDialog dialog = new ContentDialog();
                dialog.Title = "Something Went Wrong";
                dialog.PrimaryButtonText = "Retry";
                dialog.CloseButtonText = "Cancel";
                dialog.DefaultButton = ContentDialogButton.Close;
                dialog.Content = "Failed to Delete the project";

                var result = await dialog.ShowAsync();

                if (result == ContentDialogResult.Primary)
                {
                    DeleteProject_Click(sender, e);
                }
            }
        }
    }
}
