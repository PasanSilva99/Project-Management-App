using Microsoft.Toolkit.Uwp.UI.Controls;
using PCClient.Model;
using static PCClient.Model.DataStore;

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
        public RegistrationPage(string email, string password)
        {
            this.InitializeComponent();
            btn_register.IsEnabled = false;
            tb_email.Text = email;
            tb_password.Password = password;

            SetDefaultPic();  //  sets the default prifile pic in the temporary profile selection
        }

        private async void SetDefaultPic()
        {
            // Get the path to the app's Assets folder.
            string root = Windows.ApplicationModel.Package.Current.InstalledLocation.Path;
            string path = root + @"\Assets\Images";

            // Get the folder object that corresponds to this absolute path in the file system.
            StorageFolder folder = await StorageFolder.GetFolderFromPathAsync(path);
            StorageFile file = await folder.GetFileAsync("image.png");

            profileImage = file;
        }

        private void RemoveImage_Click(object sender, RoutedEventArgs e)
        {
            SetDefaultPic();  // Resets the image to the default image
            SetImage();  // Sets that pic to the view
            UnlockRegister();  // unlock the register button 
        }

        StorageFile profileImage = null;
        StorageFile originalImage = null;
        ImageSource profileImageSource = null;


        private async void AddImage_Click(object sender, RoutedEventArgs e)
        {

            // The file picker dialog
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary;  //  this is the suggested location

            // adds the below filters to filterout image files
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");
            picker.FileTypeFilter.Add(".png");

            StorageFile file = await picker.PickSingleFileAsync();  // saved the picked file into the file variable
            if (file != null)
            {
                // Application now has read/write access to the picked file
                Debug.WriteLine("Picked photo: " + file.Path);

                using (var fileStream = await file.OpenAsync(FileAccessMode.ReadWrite))
                {
                    BitmapImage bitmapImage = new BitmapImage();  // Creates a new bitmap file 
                    await bitmapImage.SetSourceAsync(fileStream);  // Sets the loded file as the new bitmap source
                    img_profielPhoto.Source = bitmapImage;  // sets the created bitmap as an image source
                    await ImageCropper.LoadImageFromFile(file);  // send that file to the image crooper 
                    profileImage = file;  // save the loaded file to the global variable for other functions use
                    originalImage = file;
                    profileImageSource = bitmapImage;

                    ImageCropper.CropShape = CropShape.Rectangular;  // sets the cropper as rectangular mask
                    ImageCropper.AspectRatio = 1;  // sets the masks aspect ratio as square
                    FlyoutBase.ShowAttachedFlyout(img_profielPhoto);  // show the flyout
                    ImageCropper.AspectRatio = 1;  // sets the mask aspect ratio again incase of imeediate reload

                }

            }
            else
            {
                Debug.WriteLine("Operation cancelled.");
            }

            UnlockRegister();


        }

        private async void btn_register_Click(object sender, RoutedEventArgs e)
        {
            var email = tb_email.Text.Trim();
            var username = tb_username.Text;
            var password = DataStore.GetHashString(tb_password.Password);
            var re_password = DataStore.GetHashString(tb_rePassword.Password);
            var imageName = profileImage.Name + ".png";
            var imageBuffer = await SaveToBytesAsync(profileImageSource);

            if (DataStore.GlobalServiceType == ServiceType.Online)
            {
                if (DataStore.CheckConnectivity())
                {

                    await DataStore.RegisterUserAsync(email, username, imageBuffer, imageName, password);
                    SaveImage();
                    registerBase.Visibility = Visibility.Collapsed;
                    RigistrationDone.Visibility = Visibility.Visible;
                }
                else
                {
                    ContentDialog dialog = new ContentDialog();
                    dialog.Title = "Connectivity Lost";
                    dialog.PrimaryButtonText = "Retry";
                    dialog.CloseButtonText = "Ok";
                    dialog.DefaultButton = ContentDialogButton.Primary;
                    dialog.Content = "Your account will only be created locally. This will be synced with the server when you connect to the network again.";

                    var result = await dialog.ShowAsync();

                    if (result == ContentDialogResult.Primary)
                        btn_register_Click(sender, e);
                    else
                        await DataStore.RegisterUserAsync(email, username, imageBuffer, imageName, password);
                    SaveImage();
                    RigistrationDone.Visibility = Visibility.Visible;
                    registerBase.Visibility = Visibility.Collapsed;

                }
            }
            else
            {
                await DataStore.RegisterUserAsync(email, username, imageBuffer, imageName, password);
                SaveImage();
                RigistrationDone.Visibility = Visibility.Visible;
                registerBase.Visibility = Visibility.Collapsed;

            }

        }

        private void tb_rePassword_PasswordChanging(PasswordBox sender, PasswordBoxPasswordChangingEventArgs args)
        {


            if (tb_rePassword.Password != tb_password.Password)
            {
                lbl_rePasswordError.Text = "❌ Passwords do not match";
                lbl_rePasswordError.Visibility = Visibility.Visible;
            }
            else
            {
                lbl_rePasswordError.Visibility = Visibility.Collapsed;
            }
            UnlockRegister();
        }

        private void tb_password_PasswordChanging(PasswordBox sender, PasswordBoxPasswordChangingEventArgs args)
        {


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
            UnlockRegister();
        }

        private void tb_rePassword_LostFocus(object sender, RoutedEventArgs e)
        {


            if (string.IsNullOrWhiteSpace(tb_rePassword.Password))
            {
                lbl_rePasswordError.Text = "❌ Password connot be blank";
                lbl_rePasswordError.Visibility = Visibility.Visible;
            }
            else
            {
                lbl_rePasswordError.Visibility = Visibility.Collapsed;
            }
            UnlockRegister();
        }

        private void tb_password_LostFocus(object sender, RoutedEventArgs e)
        {


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
            UnlockRegister();
        }

        /// <summary>
        /// This function will unlock(Enable) the Register button if the following criterias met
        /// Email, Username, Password and Repeat Password is not null
        /// and all error messages are hidden (No errors in the input)
        /// 
        /// </summary>
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
                btn_register.IsEnabled = true;  // Enable the button
            }
            else
            {
                btn_register.IsEnabled = false;  // Dissable the button
            }
        }

        StorageFile croppedImage = null;  // Store the cropped images globally


        /// <summary>
        /// The save button on the image cropper
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void btn_save_Click(object sender, RoutedEventArgs e)
        {
            // Create sample file; replace if exists.
            StorageFolder storageFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("Temp", CreationCollisionOption.OpenIfExists);  // Create Temp folder in the Local Folder
            StorageFile croppedfile = await storageFolder.CreateFileAsync("crop.png", CreationCollisionOption.ReplaceExisting);  // Create an empty image
            Debug.WriteLine("File Path " + storageFolder.Path);  // Show the file path for debuggin purposses

            using (var fileStream = await croppedfile.OpenAsync(FileAccessMode.ReadWrite))
            {
                await ImageCropper.SaveAsync(fileStream, BitmapFileFormat.Png);  // Saves the Cropped image to file
                croppedImage = croppedfile;  // Set the global variable for use of other functions
                profileImage = croppedfile;
                SetImage();   // Set image to the imageview

            }
            var fly = FlyoutBase.GetAttachedFlyout(img_profielPhoto);  // get the flyout object from the image
            fly.Hide();  // Hide the flyout from the view
        }

        /// <summary>
        /// Sets the image to the image box in the registrer form
        /// </summary>
        private async void SetImage()
        {

            using (IRandomAccessStream fileStream = await croppedImage.OpenAsync(FileAccessMode.Read))
            {
                BitmapImage bitmapImage = new BitmapImage();  // Creates anew Bitmap Image
                await bitmapImage.SetSourceAsync(fileStream);  // Loads the file in to the created bitmap
                img_profielPhoto.Source = bitmapImage;  // Sets the created bitmap as ImageSource             
                profileImageSource = bitmapImage;
            }

        }

        private async void img_profielPhoto_Tapped(object sender, TappedRoutedEventArgs e)
        {

            // If selected
            // Load that file to the image cropper
            await ImageCropper.LoadImageFromFile(originalImage!=null ? originalImage : profileImage);

            // Set the image cropper as Square
            ImageCropper.CropShape = CropShape.Rectangular;  // Cropper Shape set to rectangular
            ImageCropper.AspectRatio = 1;  // Cropper Aspect Ratio 1 means Square
            FlyoutBase.ShowAttachedFlyout(img_profielPhoto);  // Show the cropping flyout
            ImageCropper.AspectRatio = 1;  // Reset the Aspect ratio incase of reload

        }


        /// <summary>
        /// Save the image to file
        /// </summary>
        private async void SaveImage()
        {

            StorageFolder storageFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("ProfilePics", CreationCollisionOption.OpenIfExists);

            await profileImage.CopyAsync(storageFolder, tb_email.Text + ".png");
           

        }

        public async Task<byte[]> SaveToBytesAsync(ImageSource profileImageSource)
        {
            byte[] imageBuffer;
            var localFolder = ApplicationData.Current.LocalFolder;
            var tempprofilefile = await localFolder.CreateFileAsync("temp.jpg", CreationCollisionOption.ReplaceExisting);
            using (var ras = await tempprofilefile.OpenAsync(FileAccessMode.ReadWrite, StorageOpenOptions.None))
            {
                WriteableBitmap bitmap = profileImageSource as WriteableBitmap;
                var stream = bitmap.PixelBuffer.AsStream();
                byte[] buffer = new byte[stream.Length];
                await stream.ReadAsync(buffer, 0, buffer.Length);
                BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.JpegEncoderId, ras);
                encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Ignore, (uint)bitmap.PixelWidth, (uint)bitmap.PixelHeight, 96.0, 96.0, buffer);
                await encoder.FlushAsync();

                var imageStream = ras.AsStream();
                imageStream.Seek(0, SeekOrigin.Begin);
                imageBuffer = new byte[imageStream.Length];
                var re = await imageStream.ReadAsync(imageBuffer, 0, imageBuffer.Length);

            }
            await tempprofilefile.DeleteAsync(StorageDeleteOption.Default);
            return imageBuffer;
        }

    }
}
