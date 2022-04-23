using Projent.Model;
using static Projent.Model.DataStore;
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
using Windows.Storage;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Media.Animation;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Microsoft.Toolkit.Uwp.UI.Controls;
using System.Text.RegularExpressions;
using Windows.UI;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Projent
{
    public class NavigatorTag
    {
        public string Name { get; set; }
        public Type TagetPage { get; set; }
    }
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class NavigationBase : Page
    {
        public struct UserStatus
        {
            public DirectUser directUser { get; set; }
            public Status status { get; set; }
        }

        internal MainPage mainPage;

        Status userStatus = Status.Online;

        StorageFile ProfilePhoto = null;
        StorageFile croppedImage = null;
        StorageFile originalImage = null;
        internal ImageSource profileImageSource = null;
        internal User tempUser;
        DispatcherTimer UserVerificationTimer = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(15.0) };
        public List<DirectUser> directUsers = new List<DirectUser>();
        public ChatPanel loadedChatPanel = null;

        public NavigationBase()
        {
            this.InitializeComponent();
            TopNavStack.Children.Clear();
            UserVerificationTimer.Tick += Timer_Tick;
        }

        private async void Timer_Tick(object sender, object e)
        {
            await DataStore.SetUserStatus(MainPage.LoggedUser, userStatus);

            if (CheckConnectivity())
            {
                GlobalServiceType = ServiceType.Online;
            }
            else
            {
                GlobalServiceType = ServiceType.Offline;
            }

            // Get the path to the app's Assets folder.
            string root = Windows.ApplicationModel.Package.Current.InstalledLocation.Path;
            string path = root + @"\Assets\Icons";
            var imagesFolder = await Windows.Storage.StorageFolder.GetFolderFromPathAsync(path);

            if (GlobalServiceType == ServiceType.Online)
            {
                var cloudOnline = await imagesFolder.GetFileAsync("Cloud_onlineIcon.png");

                using (IRandomAccessStream fileStream = await cloudOnline.OpenAsync(FileAccessMode.Read))
                {
                    BitmapImage bitmapImage = new BitmapImage();  // Creates anew Bitmap Image
                    await bitmapImage.SetSourceAsync(fileStream);  // Loads the file in to the created bitmap
                    img_cloudStatus.Source = bitmapImage;  // Sets the created bitmap as ImageSource             
                }
                lbl_serverStatus.Text = "Synced With the Server";

            }
            else
            {
                var cloudOffline = await imagesFolder.GetFileAsync("Cloud_offlineIcon.png");

                using (IRandomAccessStream fileStream = await cloudOffline.OpenAsync(FileAccessMode.Read))
                {
                    BitmapImage bitmapImage = new BitmapImage();  // Creates anew Bitmap Image
                    await bitmapImage.SetSourceAsync(fileStream);  // Loads the file in to the created bitmap
                    img_cloudStatus.Source = bitmapImage;  // Sets the created bitmap as ImageSource             
                }
                lbl_serverStatus.Text = "Disconnected From The Server";
            }

            ValidateLoggedUser();

            if (directUsers != null)
            {
                if (directUsers.Count > 0)
                {
                    foreach (var user in directUsers)
                    {
                        try
                        {
                            var drStatus = await Server.MainServer.mainServiceClient.GetUserStatusAsync(user.Email);
                            loadedChatPanel.SetUserStaus(user, (Status)drStatus);
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex);
                        }
                    }
                }
            }

        }

        internal void SetTopNavigation(List<NavigatorTag> navigatorTags)
        {
            TopNavStack.Children.Clear();
            foreach (NavigatorTag navigatorTag in navigatorTags)
            {
                ToggleButton topNavItem = new ToggleButton();
                topNavItem.Tag = navigatorTag.TagetPage;
                topNavItem.Content = navigatorTag.Name;
                topNavItem.Style = (Style)Resources["TopNavLink"];
                topNavItem.Click += TopNavItem_Click;

                TopNavStack.Children.Add(topNavItem);
            }
        }

        private void TopNavItem_Click(object sender, RoutedEventArgs e)
        {
            Type type = (sender as ToggleButton).Tag.GetType();

            if (frame_page.SourcePageType != type)
                frame_page.Navigate(type, this);
        }

        internal async Task<bool> ValidateUser(string email, string password)
        {
            if (!string.IsNullOrWhiteSpace(email) && !string.IsNullOrWhiteSpace(password))
            {
                if (DataStore.GlobalServiceType == ServiceType.Online)
                {
                    try
                    {
                        var isValid = await DataStore.ValidateUser(email, password);
                        return isValid;
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("Error: " + ex.Message);
                        return false;
                    }
                }
                else
                {
                    var isValid = ValidateUserLocal(email, password);
                    return isValid;
                }
            }

            return false;
        }

        internal async void ValidateLoggedUser()
        {
            try
            {
                if (MainPage.LoggedUser == null)
                {
                    ContentDialog dialog = new ContentDialog();
                    dialog.Title = "Verification Faild";
                    dialog.CloseButtonText = "Login Again";
                    dialog.DefaultButton = ContentDialogButton.Close;
                    dialog.Content = "Failed to verify User";

                    var result = await dialog.ShowAsync();

                    LogoutUser();

                }
                else if (! await this.ValidateUser(MainPage.LoggedUser.Email, MainPage.LoggedUser.Password))
                {
                    ContentDialog dialog = new ContentDialog();
                    dialog.Title = "Verification Faild";
                    dialog.CloseButtonText = "Login Again";
                    dialog.DefaultButton = ContentDialogButton.Close;
                    dialog.Content = "Failed to verify User";

                    var result = await dialog.ShowAsync();

                    LogoutUser();
                }
                else
                {
                    try
                    {
                        // Get the user image
                        StorageFolder storageFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("ProfilePics", CreationCollisionOption.OpenIfExists);
                        StorageFile profilePicture = await storageFolder.GetFileAsync(MainPage.LoggedUser.Name + ".png");
                        Debug.WriteLine("File Path " + storageFolder.Path);
                        ProfilePhoto = profilePicture;


                        using (var fileStream = await profilePicture.OpenAsync(FileAccessMode.ReadWrite))
                        {
                            BitmapImage bitmapImage = new BitmapImage();  // Creates a new bitmap file 
                            await bitmapImage.SetSourceAsync(fileStream);  // Sets the loded file as the new bitmap source

                            img_profilePicture.Source = bitmapImage;
                            profileImageSource = bitmapImage;
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex);
                    }
                }
            }
            catch (Exception) { }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            mainPage = e.Parameter as MainPage;
            UserVerificationTimer.Start();
            ValidateLoggedUser();

            if(frame_tools.SourcePageType == null)
                frame_tools.Navigate(typeof(ChatPanel), this);

            if (!loadedChatPanel.ChatTimer.IsEnabled)
            {
                loadedChatPanel.ChatTimer.Start();
            }
        }

        private void NavigateTo(object sender, RoutedEventArgs e)
        {
            switch ((sender as Button).Tag)
            {
                case "Dashboard":
                    NavigateToDashboard();
                    break;
                case "Projects":
                    NavigateToProjects();
                    break;
                case "Reports":
                    NavigateToReports();
                    break;
                case "People":
                    NavigateToPeople();
                    break;
                case "StickyNotes":
                    OpenStickyNotes(sender);
                    break;
                case "Bookmarks":
                    OpenBookmarks(sender);
                    break;
                case "Chat":
                    OpenChat(sender);
                    break;
                case "Cloud":
                    OpenCloudInformation(true);
                    break;
                case "User":
                    OpenUserMenu(true);
                    break;
                default:
                    Debug.WriteLine("Navigation Tag not identified ", "ERROR");
                    break;
            }
        }

        #region Right Navigation 

        /// <summary>
        /// Open the User menu flyout
        /// this will open the flyout if the value is true and close if the value is false
        /// </summary>
        /// <param name="isOpen">Boolean value for Open/Close</param>
        internal void OpenUserMenu(bool isOpen)
        {


        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="isOpen"></param>
        internal void OpenCloudInformation(bool isOpen)
        {


        }



        /// <summary>
        /// This will open the chat panel
        /// </summary>
        internal void OpenChat(object sender)
        {
            OpenRightPanel(typeof(ChatPanel));
        }

        /// <summary>
        /// This will open the Bookmarks Panel
        /// </summary>
        internal void OpenBookmarks(object sender)
        {
            OpenRightPanel(typeof(BookmarkPanel));
        }

        /// <summary>
        /// Opens the sticky notes 
        /// </summary>
        internal void OpenStickyNotes(object sender)
        {
            OpenRightPanel(typeof(StickyNotePanel));
        }

        /// <summary>
        /// This page opens the right pannel with the target page
        /// </summary>
        /// <param name="target">The page that you want to show in the panel</param>
        internal void OpenRightPanel(Type target)
        {
            if (frame_tools.SourcePageType != null)
            {
                if (frame_tools.SourcePageType != target)
                {
                    frame_tools.Navigate(target, this);
                    if (targ.X == 500)
                        RightPanelExpand.Begin();
                }
                else
                {
                    if (targ.X != 500)
                    {
                        RightPanelMinimize.Begin();
                    }
                    else
                    {
                        RightPanelExpand.Begin();
                    }
                }
            }
            else
            {
                frame_tools.Navigate(target, this);
                if (targ.X == 500)
                    RightPanelExpand.Begin();
            }
        }


        #endregion

        #region Left Navigation

        /// <summary>
        /// This will navigate to the Projects page
        /// </summary>
        internal void NavigateToPeople()
        {
            frame_page.Navigate(typeof(PeoplePage), this);

        }

        /// <summary>
        /// This will navigate the frame to the Reports Page
        /// </summary>
        internal void NavigateToReports()
        {
            frame_page.Navigate(typeof(ReportsPage), this);

        }

        /// <summary>
        /// This will navigate the frame to the Projects page
        /// </summary>
        internal void NavigateToProjects()
        {
            frame_page.Navigate(typeof(ProjectsPage), this);

        }


        /// <summary>
        /// This function will navigate to the Dashboard page
        /// </summary>
        internal void NavigateToDashboard()
        {
            frame_page.Navigate(typeof(DashboardPage), this);
        }

        #endregion

        private async void MenuFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            var tag = (sender as MenuFlyoutItem).Tag as string;

            if (tag != null)
            {
                if (tag == "Online")
                {
                    btn_profile.BorderBrush = new SolidColorBrush(Windows.UI.Color.FromArgb((byte)255, (byte)28, (byte)255, (byte)21));
                    userStatus = Status.Online;
                    await SetUserStatus(MainPage.LoggedUser, Status.Online);
                }
                if (tag == "Idle")
                {
                    btn_profile.BorderBrush = new SolidColorBrush(Windows.UI.Color.FromArgb((byte)255, (byte)242, (byte)255, (byte)15));
                    userStatus = Status.Idle;
                    await SetUserStatus(MainPage.LoggedUser, Status.Idle);
                }
                if (tag == "Busy")
                {
                    btn_profile.BorderBrush = new SolidColorBrush(Windows.UI.Color.FromArgb((byte)255, (byte)255, (byte)16, (byte)16));
                    userStatus = Status.Busy;
                    await SetUserStatus(MainPage.LoggedUser, Status.Busy);
                }
                if (tag == "Invisible")
                {
                    btn_profile.BorderBrush = new SolidColorBrush(Windows.UI.Color.FromArgb((byte)255, (byte)36, (byte)35, (byte)35));
                    userStatus = Status.Busy;
                    await SetUserStatus(MainPage.LoggedUser, Status.Busy);
                }
                if (tag == "logout")
                {
                    LogoutUser();
                }
                if (tag == "account")
                {
                    Debug.WriteLine("UserSettings");
                    ShowUserSettings();
                }
            }
        }

        private void ShowUserSettings()
        {
            OpenRightPanel(typeof(UserSettingsPage));
        }

        private void LogoutUser()
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            localSettings.Values["RememberedUser"] = null;
            localSettings.Values["RememberedPassword"] = null;
            MainPage.LoggedUser = null;
            mainPage.NavigateToLoginPage();
            UserVerificationTimer.Stop();
        }
    }
}
