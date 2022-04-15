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
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace Projent
{
    public sealed partial class DirectUserControl : UserControl
    {
        public DirectUser _directUser = null;
        public DirectUser directUser
        {
            get { return _directUser; }
            set
            {
                if (value != null)
                {
                    _directUser = value;
                    lbl_email.Text = value.Email;
                    lbl_username.Text = value.Name;

                    LoadImageAsync();

                }
            }
        }

        public async Task<bool> IsFilePresent(string fileName)
        {
            StorageFolder storageFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("Cache", CreationCollisionOption.OpenIfExists);
            var item = await storageFolder.TryGetItemAsync(fileName);
            return item != null;
        }

        private async void LoadImageAsync()
        {
            try
            {
                if (!await IsFilePresent(_directUser.Name + ".png"))
                {
                    var imageBuffer = await Server.MainServer.mainServiceClient.RequestUserImageAsync(_directUser.Name);

                    var ProfilePicFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("Cache", CreationCollisionOption.OpenIfExists);
                    var ProfilePicFile = await ProfilePicFolder.CreateFileAsync(_directUser.Name + ".png", CreationCollisionOption.OpenIfExists);

                    await FileIO.WriteBytesAsync(ProfilePicFile, imageBuffer);

                    using (var fileStream = await ProfilePicFile.OpenAsync(FileAccessMode.Read))
                    {
                        BitmapImage bitmapImage = new BitmapImage();  // Creates a new bitmap file 
                        await bitmapImage.SetSourceAsync(fileStream);  // Sets the loded file as the new bitmap source
                        img_profilePic.Source = bitmapImage;  // sets the created bitmap as an image source
                    }

                }
                else
                {
                    var ProfilePicFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("Cache", CreationCollisionOption.OpenIfExists);
                    var ProfilePicFile = await ProfilePicFolder.GetFileAsync(_directUser.Name + ".png");

                    using (var fileStream = await ProfilePicFile.OpenAsync(FileAccessMode.Read))
                    {
                        BitmapImage bitmapImage = new BitmapImage();  // Creates a new bitmap file 
                        await bitmapImage.SetSourceAsync(fileStream);  // Sets the loded file as the new bitmap source
                        img_profilePic.Source = bitmapImage;  // sets the created bitmap as an image source
                    }
                }

                

                
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        public DirectUserControl()
        {
            this.InitializeComponent();
        }

        public DirectUserControl(DirectUser directUser)
        {
            this.InitializeComponent();
            this.directUser = directUser;
        }
    }
}
