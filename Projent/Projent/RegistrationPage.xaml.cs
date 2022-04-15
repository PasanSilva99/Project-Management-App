using Microsoft.Toolkit.Uwp.UI.Controls;
using Projent.Model;
using static Projent.Model.DataStore;

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

namespace Projent
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class RegistrationPage : Page
    {
        StorageFile profileImage = null;
        StorageFile originalImage = null;
        byte[] ProfileImageStream = null;
        StorageFile croppedImage = null;  // Store the cropped images globally
        Login login = null;

        public RegistrationPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            var data = e.Parameter as RegisterData;
            login = data.login;

            btn_register.IsEnabled = false;
            tb_email.Text = data.user.Email;
            tb_password.Password = EncOperator.DecryptString(Login.key, data.user.Password);

            SetDefaultPic();  //  sets the default prifile pic in the temporary profile selection
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
        /// Add an image as the user profile photo
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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


        /// <summary>
        /// This will remove the image form the user. and assign the default image.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RemoveImage_Click(object sender, RoutedEventArgs e)
        {
            SetDefaultPic();  // Resets the image to the default image
            SetImage();  // Sets that pic to the view
            UnlockRegister();  // unlock the register button 
        }

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
        /// Tap to re crop the image
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void img_profielPhoto_Tapped(object sender, TappedRoutedEventArgs e)
        {

            // If selected
            // Load that file to the image cropper
            await ImageCropper.LoadImageFromFile(originalImage != null ? originalImage : profileImage);

            // Set the image cropper as Square
            ImageCropper.CropShape = CropShape.Rectangular;  // Cropper Shape set to rectangular
            ImageCropper.AspectRatio = 1;  // Cropper Aspect Ratio 1 means Square
            FlyoutBase.ShowAttachedFlyout(img_profielPhoto);  // Show the cropping flyout
            ImageCropper.AspectRatio = 1;  // Reset the Aspect ratio incase of reload

        }


        /// <summary>
        /// This function will register the user to the local and the Server database
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void btn_register_Click(object sender, RoutedEventArgs e)
        {
            // Get the entered details
            var email = tb_email.Text.Trim();
            var username = tb_username.Text;
            var password = DataStore.GetHashString(tb_password.Password);
            var re_password = DataStore.GetHashString(tb_rePassword.Password);
            var imageBuffer = await SaveToBytesAsync(profileImage);  // Byte Array for the image file


            if (DataStore.GlobalServiceType == ServiceType.Online)  // if the application is set to online mode
            {
                if (DataStore.CheckConnectivity())  // Check wether the computer is connected to any network
                {
                    // Save the selected image to a file
                    SaveImage();
                    // Try to register the user in the server. If it retured tru, it is successfull
                    var isSuccess = await DataStore.RegisterUserAsync(email, username, imageBuffer, password);
                    try
                    {
                        await RegisterUserLocallyAsync(email, username, username + ".png", password);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex);
                    }
                    if (!isSuccess)  // If not success registration
                    {
                        // Show the feedback to the user and ask wether if the user want to retry or just create offline
                        ContentDialog dialog = new ContentDialog();
                        dialog.Title = "Registration Failed";
                        dialog.PrimaryButtonText = "Retry";
                        dialog.SecondaryButtonText = "Try to Create Locally";
                        dialog.CloseButtonText = "Close";
                        dialog.DefaultButton = ContentDialogButton.Primary;
                        dialog.Content = "You can retry to create the account on the server or try to create in offline database only.";

                        var result = await dialog.ShowAsync();  // show the messgae and get the result

                        if (result == ContentDialogResult.Primary) // if the user click retry,
                            btn_register_Click(sender, e); // Re run this function
                        else if (result == ContentDialogResult.Secondary)  // if the user click Create Locally, 
                        {
                            var isSuccessLocal = await RegisterUserLocallyAsync(email, username, username + ".png", password);

                            if (!isSuccessLocal)
                            {
                                return;
                            }
                        }

                    }
                }
                else
                {
                    // If the computer is not connected to any network, show this error message and ask for users feedback
                    ContentDialog dialog = new ContentDialog();
                    dialog.Title = "Not Connected to the network";
                    dialog.PrimaryButtonText = "Retry";
                    dialog.CloseButtonText = "Ok";
                    dialog.DefaultButton = ContentDialogButton.Primary;
                    dialog.Content = "Your account will only be created locally. This will be synced with the server when you connect to the network again.";

                    var result = await dialog.ShowAsync();

                    if (result == ContentDialogResult.Primary)
                        btn_register_Click(sender, e);
                    else
                    {
                        var isSuccess = await RegisterUserLocallyAsync(email, username, username + ".png", password);

                        if (!isSuccess)
                        {
                            return;
                        }
                    }

                }
            }
            else
            {
                var isSuccess = await RegisterUserLocallyAsync(email, username, username + ".png", password);

                if (!isSuccess)
                {
                    return;
                }

            }

            login.SetUser(new User() { Email = email, Password = EncOperator.EncryptString(Login.key, tb_password.Password) });
            login.ResetRegistration();

        }

        private async Task<bool> RegisterUserLocallyAsync(string email, string username, string imageName, string password)
        {
            SaveImage(); // Save the image to Profile Pics Folder
            var isSuccess = DataStore.RegisterUserLocal(email, username, username + ".png", password);  // Create the user in the local database

            if (isSuccess)
            {
                return true;

            }
            else
            {
                // Show the user it is not successfull an there is an error in the applicatiobn
                ContentDialog dialog2 = new ContentDialog();
                dialog2.Title = "Registration Failed";
                dialog2.CloseButtonText = "Close";
                dialog2.DefaultButton = ContentDialogButton.Primary;
                dialog2.Content = "Please Restart the application and try again. If it is not resolved, Please contact your Administrator.";
                await dialog2.ShowAsync();
                return false;  // if it is not success, suspend the sunction here.
            }
        }

        /// <summary>
        /// This function will unlock(Enable) the Register button if the following criterias met
        /// Email, Username, Password and Repeat Password is not null
        /// and all error messages are hidden (No errors in the input)
        /// 
        /// </summary>
        private void UnlockRegister()
        {
            // Checks wether the following conditions are met and unlocks the Register button
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
            }

        }

        /// <summary>
        /// Save the image to file
        /// </summary>
        private async void SaveImage()
        {
            // Create new folder to store the ProfileImages. If it is already there, Open it. 
            StorageFolder storageFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("ProfilePics", CreationCollisionOption.OpenIfExists);

            // Save the User image by using user email for the name. If it is there, replace it.
            await profileImage.CopyAsync(storageFolder, tb_username.Text + ".png", NameCollisionOption.ReplaceExisting);


        }

        /// <summary>
        /// If the user image is not set, this will set the default image for the user.
        /// </summary>
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

        /// <summary>
        /// This function will generate the Byte array for the given storage file
        /// Purpose of this function is to get the byte array to pass the image file to the Server.
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public async Task<byte[]> SaveToBytesAsync(StorageFile file)
        {
            IRandomAccessStream filestream = await file.OpenAsync(FileAccessMode.Read);
            var reader = new DataReader(filestream.GetInputStreamAt(0));
            await reader.LoadAsync((uint)filestream.Size);

            byte[] pixels = new byte[filestream.Size];

            reader.ReadBytes(pixels);

            return pixels;
        }


        /// <summary>
        /// This will conver the received byte array back to a Storage file
        /// Purpose of having this function is to convert the imageBuffer received from the server back to a storage file.
        /// </summary>
        /// <param name="imageBuffer">byte[] received from the server</param>
        /// <param name="fileName">EmailOfTheUser.png</param>
        /// <returns></returns>
        public async Task<StorageFile> BytesToStorageFile(byte[] imageBuffer, string fileName)
        {
            StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
            StorageFile sampleFile = await storageFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
            await FileIO.WriteBytesAsync(sampleFile, imageBuffer);
            return sampleFile;
        }

        private void btn_cancel_Click(object sender, RoutedEventArgs e)
        {
            login.ResetRegistration();
        }
    }
}
