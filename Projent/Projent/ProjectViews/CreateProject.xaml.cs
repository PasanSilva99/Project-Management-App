using Projent.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Projent.ProjectViews
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CreateProject : Page
    {
        private NavigationBase basePage;

        public CreateProject()
        {
            this.InitializeComponent();
            stack_assignees.Children.Clear();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            basePage = e.Parameter as NavigationBase;

            SetManager(new DirectUser()
            {
                Name = MainPage.LoggedUser.Name,
                Email = MainPage.LoggedUser.Email,
            });

            date_Start.Date = DateTime.Now;
            date_End.Date = DateTime.Now.AddMonths(1);
            cb_Category.Text = "General";
        }

        private async void SetManager(DirectUser user)
        {
            try
            {
                btn_ProjectManager.Tag = user;
                var ProfilePicFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("Cache", CreationCollisionOption.OpenIfExists);
                var ProfilePicFile = await ProfilePicFolder.GetFileAsync(user.Name + ".png");

                using (var fileStream = await ProfilePicFile.OpenAsync(FileAccessMode.Read))
                {
                    BitmapImage bitmapImage = new BitmapImage();  // Creates a new bitmap file 
                    await bitmapImage.SetSourceAsync(fileStream);  // Sets the loded file as the new bitmap source
                    var ProfImage = new Image();
                    ProfImage.Source = bitmapImage;  // sets the created bitmap as an image source
                    btn_ProjectManager.Content = ProfImage;
                }
                SelectedMnaager = user;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private void btn_close_Click(object sender, RoutedEventArgs e)
        {
            basePage.OpenRightPanel(typeof(CreateProject));
        }
        List<PMServer1.User> AllDirectUsers = new List<PMServer1.User>();

        /// <summary>
        /// This function will update the User list according to the search string 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void tb_searchAssignee_TextChanging(TextBox sender, TextBoxTextChangingEventArgs args)
        {
            List<DirectUser> directUsers = new List<DirectUser>();
            list_Assignees.Items.Clear();
            if (AllDirectUsers != null && AllDirectUsers.Count > 0)
            {
                foreach (var user in AllDirectUsers)
                {
                    // Show the users who is not the logged user
                    if (user.Name != MainPage.LoggedUser.Name)
                    {
                        List<DirectUser> AddedDirectUsersList = new List<DirectUser>();
                        var AddedDRUControls = stack_assignees.Children; // already assigned users
                        foreach (var AddedDRUControl in AddedDRUControls)
                        {
                            var DRUControl = AddedDRUControl as Button;
                            var DRU = DRUControl.Tag as DirectUser;

                            AddedDirectUsersList.Add(DRU);
                        }

                        // Filterout already assigned users and show only not assigned ones
                        if (AddedDirectUsersList.Where(u => u.Name == user.Name).Count() == 0)
                        {
                            if (user.Name.ToLower().Trim().Contains(tb_searchAssignee.Text.ToLower().Trim()) || user.Email.ToLower().Trim().Contains(tb_searchAssignee.Text.ToLower().Trim()))
                            {
                                var directUserControl = new DirectUserControl() { directUser = new DirectUser() { Name = user.Name, Email = user.Email } };
                                var AssigneeListItem = new ListViewItem();
                                AssigneeListItem.Style = Resources["DirectUserItem"] as Style;
                                AssigneeListItem.Content = directUserControl;
                                AssigneeListItem.Tag = new DirectUser() { Name = user.Name, Email = user.Email };
                                AssigneeListItem.Tapped += AssigneeListItem_Tapped;

                                list_Assignees.Items.Add(AssigneeListItem);
                            }
                        }
                    }

                }
            }

            if (list_Assignees.Items.Count == 0)
            {
                list_Assignees.Items.Add("No Users Found!");
            }
        }

        private List<DirectUser> Assignees = new List<DirectUser>();

        /// <summary>
        /// This will add a new assignee to the list (Stack Panel)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AssigneeListItem_Tapped(object sender, TappedRoutedEventArgs e)
        {
            // Previously I set the loaded user to the Tag of this control as a DirectUser Object
            // We will retrive that here 
            var user = (sender as ListViewItem).Tag as DirectUser;
            if (user != null)
            {
                // if the user is not already assigned,
                // Assign him/ her
                if (!Assignees.Where(x => x.Name == user.Name).Any())
                    Assignees.Add(new DirectUser() { Name = user.Name, Email = user.Email });
            }
            // This will koad them into view
            LoadDirectUsersAssignees();
            // close the opened flyout
            fly_NewAssignee.Hide();
        }

        /// <summary>
        /// This function will check if there in any file in the required path
        /// </summary>
        /// <param name="fileName">Name of the File (file.extension)</param>
        /// <param name="foldername">File path (C:/FolderName/)</param>
        /// <returns>True if the File exists, False if not</returns>
        public async Task<bool> IsFilePresent(string fileName, string foldername)
        {
            // Get the folder from the parameter
            StorageFolder storageFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync(foldername, CreationCollisionOption.OpenIfExists);
            // Try and get the file mentioned
            var item = await storageFolder.TryGetItemAsync(fileName);

            // this will return true if the item is not null and
            // return false if the item is null
            return item != null;
        }

        /// <summary>
        /// This will Loadd the users in to the view in the flyout
        /// </summary>
        private async void LoadDirectUsersAssignees()
        {
            // first clear all the loadedusers from the previous session
            stack_assignees.Children.Clear();
            // This will hold the currently loaded users
            // Got a seperate variable because it generated an error on yjhe foreach last time saying
            // the list is updated on the run. 
            var DirectUserList = Assignees;

            // Will continue if the list is not null and 
            // it has valus
            if (DirectUserList != null && DirectUserList.Count > 0)
            {
                foreach (var user in DirectUserList)
                {
                    // only show the users who is not the logged user
                    // because created user is autometically assigened as the owner of thuis project. 
                    if (user.Name != MainPage.LoggedUser.Name)
                    {
                        // this controll will hold our user and
                        // will show it to the user as a button., 
                        Button directUserButton = new Button();
                        directUserButton.Style = Resources["UserButton"] as Style;
                        directUserButton.Tag = user;

                        var DirectUserImage = new Image();

                        // will load thge image from the local cashe if exissts to save the loading time
                        // this can save more than 5 seconds of loading time
                        if (!await IsFilePresent(user.Name + ".png", "Cache"))
                        {
                            var imageBuffer = await Server.MainServer.mainServiceClient.RequestUserImageAsync(user.Name);

                            var ProfilePicFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("Cache", CreationCollisionOption.OpenIfExists);
                            var ProfilePicFile = await ProfilePicFolder.CreateFileAsync(user.Name + ".png", CreationCollisionOption.OpenIfExists);

                            await FileIO.WriteBytesAsync(ProfilePicFile, imageBuffer);

                            using (var fileStream = await ProfilePicFile.OpenAsync(FileAccessMode.Read))
                            {
                                BitmapImage bitmapImage = new BitmapImage();  // Creates a new bitmap file 
                                await bitmapImage.SetSourceAsync(fileStream);  // Sets the loded file as the new bitmap source
                                DirectUserImage.Source = bitmapImage;  // sets the created bitmap as an image source
                            }

                        }
                        else
                        {
                            var ProfilePicFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("Cache", CreationCollisionOption.OpenIfExists);
                            var ProfilePicFile = await ProfilePicFolder.GetFileAsync(user.Name + ".png");

                            using (var fileStream = await ProfilePicFile.OpenAsync(FileAccessMode.Read))
                            {
                                BitmapImage bitmapImage = new BitmapImage();  // Creates a new bitmap file 
                                await bitmapImage.SetSourceAsync(fileStream);  // Sets the loded file as the new bitmap source
                                DirectUserImage.Source = bitmapImage;  // sets the created bitmap as an image source
                            }
                        }

                        directUserButton.Content = DirectUserImage;
                        directUserButton.Tapped += DirectUserButton_Tapped;
                        // this will attach the user info flyout for this button. 
                        FlyoutBase.SetAttachedFlyout(directUserButton, Resources["Assigneeinfo"] as Flyout);

                        // if the user is not shown already on the list, add it to the list
                        if (!stack_assignees.Children.Contains(directUserButton))
                            stack_assignees.Children.Add(directUserButton);
                    }
                }
            }
        }

        /// <summary>
        /// This will invokke wen the user nutton tapped,
        /// and populate the information of the attached user
        /// to the UserInfo Flyout
        /// </summary>
        /// <param name="sender">User Button</param>
        /// <param name="e"></param>
        private void DirectUserButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var user = (sender as Button).Tag as DirectUser;
            lbl_DirectUserEmail.Text = user.Email;
            lbl_DirectUsername.Text = user.Name;

            img_directUserImage.Source = ((sender as Button).Content as Image).Source;

            btn_removeAsignee.Tag = user;

            FlyoutBase.ShowAttachedFlyout(sender as Button);
        }

        /// <summary>
        /// When the user click on the Add new assignee button on the create project
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void NewAssigneeButton_Click(object sender, RoutedEventArgs e)
        {
            list_Assignees.Items.Clear();  // first clear all the users that is loaded in to the list in the add users flyout
            var users = await Server.MainServer.mainServiceClient.FetchUsersAsync();
            AllDirectUsers = users.ToList();

            if (users != null && users.Count > 0)
            {
                foreach (var user in users)
                {
                    if (user.Name != MainPage.LoggedUser.Name)
                    {
                        List<DirectUser> AddedAssigneeList = new List<DirectUser>();
                        var AddedAssigneeControls = stack_assignees.Children;
                        foreach (var AddedAssigneeControl in AddedAssigneeControls)
                        {
                            var DRUControl = AddedAssigneeControl as Button;
                            var assignee = DRUControl.Tag as DirectUser;

                            AddedAssigneeList.Add(assignee);
                        }

                        if (AddedAssigneeList.Where(u => u.Name == user.Name).Count() == 0)
                        {
                            var assigneeControl = new DirectUserControl() { directUser = new DirectUser() { Name = user.Name, Email = user.Email } };
                            var AssigneeListItem = new ListViewItem();
                            AssigneeListItem.Style = Resources["DirectUserItem"] as Style;
                            AssigneeListItem.Content = assigneeControl;
                            AssigneeListItem.Tag = new DirectUser() { Name = user.Name, Email = user.Email };
                            AssigneeListItem.Tapped += AssigneeListItem_Tapped;

                            list_Assignees.Items.Add(AssigneeListItem);
                        }
                    }

                }
            }
        }

        /// <summary>
        /// This will remove the assignee from the list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_removeAsignee_Click(object sender, RoutedEventArgs e)
        {
            var user = (sender as Button).Tag as DirectUser;

            var DirectUsersButtons = stack_assignees.Children;

            foreach (var DirectUserButton in DirectUsersButtons)
            {
                var btn = DirectUserButton as Button;
                var dr = btn.Tag as DirectUser;
                if (dr.Name == user.Name)
                    stack_assignees.Children.Remove(DirectUserButton);
            }

            Assignees.Remove(user);

        }

        DirectUser SelectedMnaager = null;

        /// <summary>
        /// this will call a flyout wish is similar to the add new assignee 
        /// Except, this shows the logged user on the list too. 
        /// A user can be the owner and the manager at the same time
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void btn_ChangeManager_Click(object sender, RoutedEventArgs e)
        {
            list_Managers.Items.Clear();
            var users = await Server.MainServer.mainServiceClient.FetchUsersAsync();

            if (users != null && users.Count > 0)
            {
                foreach (var user in users)
                {
                    List<DirectUser> Userslist = new List<DirectUser>();

                    if (user.Name != SelectedMnaager.Name)
                    {
                        var managerControl = new DirectUserControl() { directUser = new DirectUser() { Name = user.Name, Email = user.Email } };
                        var ManagerListItem = new ListViewItem();
                        ManagerListItem.Style = Resources["DirectUserItem"] as Style;
                        ManagerListItem.Content = managerControl;
                        ManagerListItem.Tag = new DirectUser() { Name = user.Name, Email = user.Email };
                        ManagerListItem.Tapped += ManagerListItem_Tapped;

                        list_Managers.Items.Add(ManagerListItem);
                    }
                }
            }
        }

        /// <summary>
        /// This will set the manager and update the UI
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ManagerListItem_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var user = (sender as ListViewItem).Tag as DirectUser;
            SetManager(user);
            fly_NewManager.Hide();
            ManagerInfo.Hide();
        }

        /// <summary>
        /// This will show the manager info and a option to change manager as a flyout
        /// Basically this populates the Customized User Info Flyout which is Attached to the sender
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btn_ProjectManager_Click(object sender, RoutedEventArgs e)
        {
            var user = (sender as Button).Tag as DirectUser;

            try { img_ManagerImage.Source = ((sender as Button).Content as Image).Source; }
            catch (Exception d) {
                Console.WriteLine("project create exeception");
            }

            lbl_ManagerUsername.Text = user.Name;
            lbl_ManagerEmail.Text = user.Email;
        }

        /// <summary>
        /// This function will update the User list according to the search string 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void tb_searchManager_TextChanging(TextBox sender, TextBoxTextChangingEventArgs args)
        {
            List<DirectUser> directUsers = new List<DirectUser>();
            list_Managers.Items.Clear();
            if (AllDirectUsers != null && AllDirectUsers.Count > 0)
            {
                foreach (var user in AllDirectUsers)
                {

                    if (user.Name != SelectedMnaager.Name)
                    {
                        if (user.Name.ToLower().Trim().Contains(tb_searchAssignee.Text.ToLower().Trim()) || user.Email.ToLower().Trim().Contains(tb_searchAssignee.Text.ToLower().Trim()))
                        {
                            var directUserControl = new DirectUserControl() { directUser = new DirectUser() { Name = user.Name, Email = user.Email } };
                            var AssigneeListItem = new ListViewItem();
                            AssigneeListItem.Style = Resources["DirectUserItem"] as Style;
                            AssigneeListItem.Content = directUserControl;
                            AssigneeListItem.Tag = new DirectUser() { Name = user.Name, Email = user.Email };
                            AssigneeListItem.Tapped += AssigneeListItem_Tapped;

                            list_Assignees.Items.Add(AssigneeListItem);
                        }
                    }


                }
            }

            if (list_Assignees.Items.Count == 0)
            {
                list_Managers.Items.Add("No Users Found!");
            }
        }

        /// <summary>
        /// This will be triggered when the user clicked create project function
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void btn_createProject_Click(object sender, RoutedEventArgs e)
        {
            btn_createProject.IsEnabled = false;
            btn_createProject.Content = "Creating ⏳";

            var project = new PMServer2.Project();
            // first validate the info
            var title = tb_projectTitle.Text;  // Mandatory
            var description = tb_projectDescription.Text;
            var assignees = new List<string>();
            var manager = SelectedMnaager.Name;  // Mandatory - Autosets Owner
            var startDate = date_Start.Date.Value.DateTime;  // Mandatory - Autosets Todays Date
            var endDate = date_End.Date.Value.DateTime;  // Mandatory - Autosets a month from Todays Date
            var category = cb_Category.SelectedItem as ComboBoxItem;  // Mandatory - Auto Sets Genaral Category
            var status = cb_Status.SelectedItem as ComboBoxItem;  // Mandatory - Auto Sets Active

            if(Assignees.Count > 1)
            {
                foreach (var item in Assignees)
                {
                    assignees.Add(item.Name);
                }
            }

            if (!string.IsNullOrWhiteSpace(title))
            {

                var cat = "";

                if (category != null)
                {
                    cat = category.Tag.ToString();
                }
                else if (!string.IsNullOrWhiteSpace(cb_Category.Text))
                {
                    cat = cb_Category.Text;
                }
                else
                {
                    cat = "General";
                }

                project.Title = title;
                project.Description = description;
                project.Assignees = new System.Collections.ObjectModel.ObservableCollection<string>(assignees.ToArray());
                project.Status = status.Tag.ToString();
                project.StartDate = startDate;
                project.EndDate = endDate;
                project.Category = cat;
                project.ProjectManager = manager;
                // if the Logged user is null, seems like the function is delayed. 
                // if it occured, set the owner as the manager
                project.CreatedBy = MainPage.LoggedUser != null ? MainPage.LoggedUser.Name : manager;

                try
                {
                    // check wether the server is connected
                    if(await Server.ProjectServer.CheckConnectivity())
                    {
                        // send the new project to the server
                        var isSuccess =  await Server.ProjectServer.projectServiceClient.CreateProjectAsync(project);
                        if (isSuccess)
                        {
                            basePage.OpenRightPanel(typeof(CreateProject));
                        }
                        else
                        {
                            ContentDialog dialog = new ContentDialog();
                            dialog.Title = "Something Went Wrong";
                            dialog.PrimaryButtonText = "Retry";
                            dialog.CloseButtonText = "Cancel";
                            dialog.DefaultButton = ContentDialogButton.Close;
                            dialog.Content = "Failed to Create the project";

                            var result = await dialog.ShowAsync();

                            if (result == ContentDialogResult.Primary)
                            {
                                btn_createProject_Click(sender, e);
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
                            btn_createProject_Click(sender, e);
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
                    dialog.Content = "Failed to Create the project";

                    var result = await dialog.ShowAsync();

                    if (result == ContentDialogResult.Primary)
                    {
                        btn_createProject_Click(sender, e);
                    }
                }
            }

            btn_createProject.IsEnabled = true;
            btn_createProject.Content = "Create";

        }
    }
}
