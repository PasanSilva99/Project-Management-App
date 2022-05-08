using Projent.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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
        private void tb_searchAssignee_TextChanging(TextBox sender, TextBoxTextChangingEventArgs args)
        {
            List<DirectUser> directUsers = new List<DirectUser>();
            list_Assignees.Items.Clear();
            if (AllDirectUsers != null && AllDirectUsers.Count > 0)
            {
                foreach (var user in AllDirectUsers)
                {
                    if (user.Name != MainPage.LoggedUser.Name)
                    {
                        List<DirectUser> AddedDirectUsersList = new List<DirectUser>();
                        var AddedDRUControls = stack_assignees.Children;
                        foreach (var AddedDRUControl in AddedDRUControls)
                        {
                            var DRUControl = AddedDRUControl as Button;
                            var DRU = DRUControl.Tag as DirectUser;

                            AddedDirectUsersList.Add(DRU);
                        }

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

        private void AssigneeListItem_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var user = (sender as ListViewItem).Tag as DirectUser;
            if (user != null)
            {
                if (!Assignees.Where(x => x.Name == user.Name).Any())
                    Assignees.Add(new DirectUser() { Name = user.Name, Email = user.Email });
            }
            LoadDirectUsersAssignees();
            fly_NewAssignee.Hide();
        }

        public async Task<bool> IsFilePresent(string fileName, string foldername)
        {
            StorageFolder storageFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync(foldername, CreationCollisionOption.OpenIfExists);
            var item = await storageFolder.TryGetItemAsync(fileName);
            return item != null;
        }


        private async void LoadDirectUsersAssignees()
        {
            stack_assignees.Children.Clear();
            var DirectUserList = Assignees;
            if (DirectUserList != null && DirectUserList.Count > 0)
            {
                foreach (var user in DirectUserList)
                {
                    if (user.Name != MainPage.LoggedUser.Name)
                    {
                        Button directUserButton = new Button();
                        directUserButton.Style = Resources["UserButton"] as Style;
                        directUserButton.Tag = user;



                        var DirectUserImage = new Image();

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
                        FlyoutBase.SetAttachedFlyout(directUserButton, Resources["Assigneeinfo"] as Flyout);

                        if (!stack_assignees.Children.Contains(directUserButton))
                            stack_assignees.Children.Add(directUserButton);
                    }
                }
            }
        }

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

        private void ManagerListItem_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var user = (sender as ListViewItem).Tag as DirectUser;
            SetManager(user);
            fly_NewManager.Hide();
            ManagerInfo.Hide();
        }

        private void btn_ProjectManager_Click(object sender, RoutedEventArgs e)
        {
            var user = (sender as Button).Tag as DirectUser;

            img_ManagerImage.Source = ((sender as Button).Content as Image).Source;
            lbl_ManagerUsername.Text = user.Name;
            lbl_ManagerEmail.Text = user.Email;
        }

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
    }
}
