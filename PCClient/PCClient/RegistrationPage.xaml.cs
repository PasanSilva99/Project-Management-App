using Microsoft.Toolkit.Uwp.UI.Controls;
using PCClient.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Imaging;
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

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace PCClient
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class RegistrationPage : Page
    {
        public RegistrationPage(string email)
        {
            this.InitializeComponent();
            btn_register.IsEnabled = false;
        }

        private void RemoveImage_Click(object sender, RoutedEventArgs e)
        {
            UnlockRegister();
        }

        StorageFile profileImage = null;

        private async void AddImage_Click(object sender, RoutedEventArgs e)
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary;
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");
            picker.FileTypeFilter.Add(".png");

            StorageFile file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                // Application now has read/write access to the picked file
                Debug.WriteLine("Picked photo: " + file.Path);

                using (var fileStream = await file.OpenAsync(FileAccessMode.ReadWrite))
                {
                    BitmapImage bitmapImage = new BitmapImage();    
                    await bitmapImage.SetSourceAsync(fileStream);
                    img_profielPhoto.Source = bitmapImage;
                    await ImageCropper.LoadImageFromFile(file);
                    profileImage = file;

                    ImageCropper.CropShape = CropShape.Rectangular;
                    ImageCropper.AspectRatio = 1;
                    FlyoutBase.ShowAttachedFlyout(img_profielPhoto);
                    ImageCropper.AspectRatio = 1;

                }

                //img_profielPhoto.Source = new BitmapImage(new Uri(file.Path));
                //img_cropImage.Source = img_profielPhoto.Source;
                //
            }
            else
            {
                Debug.WriteLine("Operation cancelled.");
            }

            UnlockRegister();


        }

        private void btn_register_Click(object sender, RoutedEventArgs e)
        {
            var email = tb_email.Text;
            var username = tb_username.Text;
            var password = DataStore.GetHashString(tb_password.Password);
            var re_password = DataStore.GetHashString(tb_rePassword.Password);

            
        }

        private void tb_rePassword_PasswordChanging(PasswordBox sender, PasswordBoxPasswordChangingEventArgs args)
        {
            UnlockRegister();

            if (tb_rePassword.Password != tb_password.Password)
            {
                lbl_rePasswordError.Text = "❌ Passwords do not match";
                lbl_rePasswordError.Visibility = Visibility.Visible;
            }
            else
            {
                lbl_rePasswordError.Visibility = Visibility.Collapsed;
            }
        }

        private void tb_password_PasswordChanging(PasswordBox sender, PasswordBoxPasswordChangingEventArgs args)
        {
            UnlockRegister();

            lbl_passwordError.Visibility = Visibility.Collapsed;
            if (tb_rePassword.Password != tb_password.Password && !string.IsNullOrWhiteSpace(tb_rePassword.Password))
            {
                lbl_passwordError.Text = "❌ Passwords do not match";
                lbl_passwordError.Visibility = Visibility.Visible;
            }
            else
            {
                lbl_passwordError.Visibility = Visibility.Collapsed;
            }
        }

        private void tb_rePassword_LostFocus(object sender, RoutedEventArgs e)
        {
            UnlockRegister();

            if (string.IsNullOrWhiteSpace(tb_rePassword.Password))
            {
                lbl_rePasswordError.Text = "❌ Password connot be blank";
                lbl_rePasswordError.Visibility = Visibility.Visible;
            }
            else
            {
                lbl_rePasswordError.Visibility = Visibility.Collapsed;
            }
        }

        private void tb_password_LostFocus(object sender, RoutedEventArgs e)
        {
            UnlockRegister();

            if (string.IsNullOrWhiteSpace(tb_password.Password))
            {
                lbl_passwordError.Text = "❌ Password connot be blank";
                lbl_passwordError.Visibility = Visibility.Visible;
            }
            else if (tb_password.Password.Length < 6)
            {
                lbl_passwordError.Text = "❌ Password must be at least 6 charactors long";
                lbl_passwordError.Visibility = Visibility.Visible;
            }
            else
            {
                lbl_passwordError.Visibility = Visibility.Collapsed;
            }
        }

        private void UnlockRegister()
        {
            if (
                !string.IsNullOrWhiteSpace(tb_email.Text) &&
                !string.IsNullOrWhiteSpace(tb_username.Text) &&
                !string.IsNullOrWhiteSpace(tb_password.Password) &&
                !string.IsNullOrWhiteSpace(tb_rePassword.Password) &&
                profileImage != null &&
                lbl_EmailError.Visibility == Visibility.Collapsed &&
                lbl_passwordError.Visibility == Visibility.Collapsed &&
                lbl_usernameError.Visibility == Visibility.Collapsed &&
                lbl_rePasswordError.Visibility == Visibility.Collapsed 
                )
            {
                btn_register.IsEnabled = true;
            }
            else
            {
                btn_register.IsEnabled = false;
            }
        }

        StorageFile croppedImage = null;

        private async void btn_save_Click(object sender, RoutedEventArgs e)
        {

            // Create sample file; replace if exists.
            StorageFolder storageFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("Temp", CreationCollisionOption.OpenIfExists);
            StorageFile croppedfile = await storageFolder.CreateFileAsync("crop.png", CreationCollisionOption.ReplaceExisting);
            Debug.WriteLine("File Path " + storageFolder.Path);

            using (var fileStream = await croppedfile.OpenAsync(FileAccessMode.ReadWrite))
            {
                await ImageCropper.SaveAsync(fileStream, BitmapFileFormat.Png);
                croppedImage = croppedfile;

                SetImage();
                
            }
            var fly = FlyoutBase.GetAttachedFlyout(img_profielPhoto);
            fly.Hide();
        }

        private async void SetImage()
        {

            using (IRandomAccessStream fileStream = await croppedImage.OpenAsync(FileAccessMode.Read))
            {
                BitmapImage bitmapImage = new BitmapImage();
                await bitmapImage.SetSourceAsync(fileStream);
                img_profielPhoto.Source = bitmapImage;

            }
            
        }

        private async void img_profielPhoto_Tapped(object sender, TappedRoutedEventArgs e)
        {
            // Check wther the user is selected a picture
            if (profileImage != null)
            {
                // If selected
                // Load that file to the image cropper
                await ImageCropper.LoadImageFromFile(profileImage);

                // Set the image cropper as Square
                ImageCropper.CropShape = CropShape.Rectangular;  // Cropper Shape set to rectangular
                ImageCropper.AspectRatio = 1;  // Cropper Aspect Ratio 1 means Square
                FlyoutBase.ShowAttachedFlyout(img_profielPhoto);  // Show the cropping flyout
                ImageCropper.AspectRatio = 1;  // Reset the Aspect ratio incase of reload
            }
            else
            {
                // Get the path to the app's Assets folder.
                string root = Windows.ApplicationModel.Package.Current.InstalledLocation.Path;
                string path = root + @"\Assets\Images";

                // Get the folder object that corresponds to this absolute path in the file system.
                StorageFolder folder = await StorageFolder.GetFolderFromPathAsync(path);
                StorageFile file = await folder.GetFileAsync("image.png");

                profileImage = file;

                // Load that file to the image cropper
                await ImageCropper.LoadImageFromFile(profileImage);

                // Set the image cropper as Square
                ImageCropper.CropShape = CropShape.Rectangular;  // Cropper Shape set to rectangular
                ImageCropper.AspectRatio = 1;  // Cropper Aspect Ratio 1 means Square
                FlyoutBase.ShowAttachedFlyout(img_profielPhoto);  // Show the cropping flyout
                ImageCropper.AspectRatio = 1;  // Reset the Aspect ratio incase of reload

                Debug.WriteLine(file.Path);
            }
        }
    }
}
