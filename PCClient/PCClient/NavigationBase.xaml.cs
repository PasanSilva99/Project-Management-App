using PCClient.Model;
using static PCClient.Model.DataStore;
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

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace PCClient
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
        internal MainPage mainPage;


        public NavigationBase()
        {
            this.InitializeComponent();
            TopNavStack.Children.Clear();
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

        private async void ValidateLoggedUser()
        {
            if (mainPage.LoggedUser == null && !ValidateUser(mainPage.LoggedUser.Email, mainPage.LoggedUser.Password))
            {
                ContentDialog dialog = new ContentDialog();
                dialog.Title = "Verification Faild";
                dialog.CloseButtonText = "Login Again";
                dialog.DefaultButton = ContentDialogButton.Close;
                dialog.Content = "Failed to verify User";

                var result = await dialog.ShowAsync();

                mainPage.NavigateToLoginPage();
            }
            else
            {
                // Get the user image
                StorageFolder storageFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("ProfilePics", CreationCollisionOption.OpenIfExists);
                StorageFile profilePicture = await storageFolder.GetFileAsync(mainPage.LoggedUser.Image);
                Debug.WriteLine("File Path " + storageFolder.Path);

                using (var fileStream = await profilePicture.OpenAsync(FileAccessMode.ReadWrite))
                {
                    BitmapImage bitmapImage = new BitmapImage();  // Creates a new bitmap file 
                    await bitmapImage.SetSourceAsync(fileStream);  // Sets the loded file as the new bitmap source

                    img_profilePicture.Source = bitmapImage;

                }
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            mainPage = e.Parameter as MainPage;

            ValidateLoggedUser();
        }

        private void NavigateTo(object sender, RoutedEventArgs e)
        {
            switch((sender as Button).Tag)
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
                    OpenStickyNotes();
                    break;
                case "Bookmarks":
                    OpenBookmarks();
                    break;
                case "Chat":
                    OpenChat();
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
        /// 
        /// </summary>
        internal void OpenChat()
        {

        }

        /// <summary>
        /// 
        /// </summary>
        internal void OpenBookmarks()
        {

        }

        /// <summary>
        /// 
        /// </summary>
        internal void OpenStickyNotes()
        {
            Debug.WriteLine("StickyNoteOpen");
            if (RightPanel.Translation.X == 500)
            {
                Debug.WriteLine("Expanded RP");
                RightPanelExpand.Begin();
            }
            else
            {
                Debug.WriteLine("Minimized RP");

                RightPanelMinimize.Begin();
            }
        }

        #endregion

        #region Left Navigation

        /// <summary>
        /// This will navigate to the Projects page
        /// </summary>
        internal void NavigateToPeople()
        {
            frame_page.Navigate(typeof(ProjectsPage), this);

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
    }
}
