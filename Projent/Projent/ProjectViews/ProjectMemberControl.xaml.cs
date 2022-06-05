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

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace Projent.ProjectViews
{
    public sealed partial class ProjectMemberControl : UserControl
    {
        private string _username = "";
        private int _permLevel = 2;

        public string UserName 
        {
            get
            {
                return _username;
            }
            set
            {
                _username = value;
                lbl_userName.Text = value;
                SetImage(_username);
                SetEmail(_username);
            } 
        }

        /// <summary>
        /// 2 - Assignee
        /// 1 - Manager
        /// 0 - Owner
        /// </summary>
        public int PermLevel 
        {
            get
            {
                return _permLevel;
            }
            set
            {
                _permLevel = value;
                if(value == 0)
                {
                    img_userAdmin.Visibility = Visibility.Collapsed;
                    img_userOwner.Visibility = Visibility.Visible;
                }
                else if(value == 1)
                {
                    img_userAdmin.Visibility = Visibility.Visible;
                    img_userOwner.Visibility = Visibility.Collapsed;
                }
                else
                {
                    img_userAdmin.Visibility=Visibility.Collapsed;
                    img_userOwner.Visibility=Visibility.Collapsed;
                }
            } 
        }

        public ProjectMemberControl()
        {
            this.InitializeComponent();
        }

        public async void SetEmail(string username)
        {
            lbl_email.Text = "Loading...";
            try
            {
                lbl_email.Text = await DataStore.GetEmail(username, MainPage.LoggedUser.Name);
                 
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                lbl_email.Text = "Something Went Wrong";
            }
        }

        public async Task<bool> IsFilePresent(string fileName, string foldername)
        {
            StorageFolder storageFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync(foldername, CreationCollisionOption.OpenIfExists);
            var item = await storageFolder.TryGetItemAsync(fileName);
            return item != null;
        }

        private async void SetImage(string username)
        {
            // the cash folder
            StorageFolder storageFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("Cache", CreationCollisionOption.OpenIfExists);
           

            // if the profile image is available
            if (await IsFilePresent(username + ".png", "Cache"))
            {
                Debug.WriteLine("Has Sender Image");
                // get the file 
                StorageFile imageFile = await storageFolder.GetFileAsync(username + ".png");

                // get the file and set it
                using (var fileStream = await imageFile.OpenAsync(FileAccessMode.Read))
                {
                    BitmapImage bitmapImage = new BitmapImage();  // Creates a new bitmap file 
                    await bitmapImage.SetSourceAsync(fileStream);  // Sets the loded file as the new bitmap source
                    img_userImage.Source = bitmapImage;  // sets the created bitmap as an image source
                }
            }
            else
            {
                // if the user image is not available locally,
                Debug.WriteLine("Requesting Sender Image");
                // Request it from the server
                // here we are saving it for more faster access in the future. 
                var image = await Server.MainServer.mainServiceClient.RequestUserImageAsync(username);

                // if the image is null , show the error as this
                Debug.WriteLine(image == null ? ":::Error:::" : ":::HasBuffer:::");

                // Get the location to the cache folder
                var ProfilePicFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("Cache", CreationCollisionOption.OpenIfExists);
                // create the file to save the profile pic
                var ProfilePicFile = await ProfilePicFolder.CreateFileAsync(username + ".png", CreationCollisionOption.ReplaceExisting);

                // save the pic
                await FileIO.WriteBytesAsync(ProfilePicFile, image);

                // get the file and set it
                using (var fileStream = await ProfilePicFile.OpenAsync(FileAccessMode.Read))
                {
                    BitmapImage bitmapImage = new BitmapImage();  // Creates a new bitmap file 
                    await bitmapImage.SetSourceAsync(fileStream);  // Sets the loded file as the new bitmap source
                    img_userImage.Source = bitmapImage;  // sets the created bitmap as an image source
                }

            }
        }

    }
}
