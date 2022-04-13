using Microsoft.Toolkit.Uwp.UI.Controls;
using PCClient.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
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

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace PCClient
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class UserSettingsPage : Page
    {
        NavigationBase navigationBase;

        StorageFile ProfilePhoto = null;
        StorageFile croppedImage = null;
        StorageFile originalImage = null;

        public UserSettingsPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            navigationBase = e.Parameter as NavigationBase;

            FillUserSettings(navigationBase.mainPage.LoggedUser);
        }

        private void FillUserSettings(User user)
        {
            if (user != null)
            {
                tb_username.Text = user.Name;
                tb_email.Text = user.Email;
                btn_ChangePassword.Tag = user;
                user_Settings_pic.Source = navigationBase.profileImageSource;
            }
        }

        private void user_Settings_pic_Tapped(object sender, TappedRoutedEventArgs e)
        {

        }

        private async void btn_addImage_Click(object sender, RoutedEventArgs e)
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
                    user_Settings_pic.Source = bitmapImage;  // sets the created bitmap as an image source
                    await ImageCropper.LoadImageFromFile(file);  // send that file to the image crooper 
                    ProfilePhoto = file;  // save the loaded file to the global variable for other functions use
                    originalImage = file;
                    ImageCropper.CropShape = CropShape.Rectangular;  // sets the cropper as rectangular mask
                    ImageCropper.AspectRatio = 1;  // sets the masks aspect ratio as square
                    FlyoutBase.ShowAttachedFlyout(user_Settings_pic);  // show the flyout
                    ImageCropper.AspectRatio = 1;  // sets the mask aspect ratio again incase of imeediate reload

                }

            }
            else
            {
                Debug.WriteLine("Operation cancelled.");
            }
        }

        private void btn_deleteImage_Click(object sender, RoutedEventArgs e)
        {

        }

        private async void SetImage()
        {

            using (IRandomAccessStream fileStream = await croppedImage.OpenAsync(FileAccessMode.Read))
            {
                BitmapImage bitmapImage = new BitmapImage();  // Creates anew Bitmap Image
                await bitmapImage.SetSourceAsync(fileStream);  // Loads the file in to the created bitmap
                user_Settings_pic.Source = bitmapImage;  // Sets the created bitmap as ImageSource             
            }

        }

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
                ProfilePhoto = croppedfile;
                SetImage();   // Set image to the imageview

            }
            var fly = FlyoutBase.GetAttachedFlyout(user_Settings_pic);  // get the flyout object from the image
            fly.Hide();  // Hide the flyout from the view

            Debug.WriteLine("Image Saved as Temp File");
        }

        private async void SaveImage()
        {
            try
            {

                if (ProfilePhoto != null)
                {
                    // Create new folder to store the ProfileImages. If it is already there, Open it. 
                    StorageFolder storageFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("ProfilePics", CreationCollisionOption.OpenIfExists);

                    // Save the User image by using user email for the name. If it is there, replace it.
                    await ProfilePhoto.CopyAsync(storageFolder, tb_username.Text + ".png", NameCollisionOption.ReplaceExisting);

                    Debug.WriteLine("Image Updated Locally");
                }

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

        }

        private async void btn_UpdateUser_Click(object sender, RoutedEventArgs e)
        {

            Debug.WriteLine("buh");
            Debug.WriteLine("Atempting to chanage Account Settings");
            // Create a temporarary user
            User tempUser = new User();
            tempUser.Name = tb_username.Text;
            tempUser.Email = tb_email.Text;
            tempUser.Image = tb_username.Text + ".png";
            tempUser.Password = navigationBase.mainPage.LoggedUser.Password;

            // Check the global Service type

            if (DataStore.GlobalServiceType == DataStore.ServiceType.Online)
            {

                // Check wether the computer is connected to any network
                if (DataStore.CheckConnectivity())
                {
                    try
                    {
                        var isValid = DataStore.ValidateUser(tempUser.Email, tempUser.Password);

                        if (isValid)
                        {
                            // Using the Remote Service
                            var isSuccess = await DataStore.UpdateUser(navigationBase.mainPage.LoggedUser, tempUser);

                            if (isSuccess)
                            {
                                // Cose the changing ui and show success ui
                                navigationBase.OpenRightPanel(typeof(UserSettingsPage));
                            }
                            else
                            {
                                // Cause 1 - Undefined Error
                                // Show the feedback to the user and ask wether if the user want to retry or just create offline
                                ContentDialog dialog = new ContentDialog();
                                dialog.Title = "Update Failed";
                                dialog.PrimaryButtonText = "Retry";
                                dialog.SecondaryButtonText = "Work Offline";
                                dialog.CloseButtonText = "Cancel";
                                dialog.DefaultButton = ContentDialogButton.Primary;
                                dialog.Content = "You can retry to uptade the account on the server or try to create in offline mode only.";
                                dialog.CloseButtonClick += Dialog_CloseButtonClick; ;

                                var result = await dialog.ShowAsync();  // show the messgae and get the result

                                if (result == ContentDialogResult.Primary) // if the user click retry,
                                    btn_UpdateUser_Click(sender, e); // Re run this function

                                else if (result == ContentDialogResult.Secondary)  // if the user click Create Locally, 
                                {
                                    var isSuccessLocal = await DataStore.UpdateUserLocally(navigationBase.mainPage.LoggedUser, tempUser);

                                    if (!isSuccessLocal)
                                    {
                                        return;
                                    }
                                }

                            }
                        }
                        else
                        {
                            ContentDialog PasswordErrorDialog = new ContentDialog();
                            PasswordErrorDialog.Title = "Passwords Do Not Match";
                            PasswordErrorDialog.CloseButtonText = "Retry";
                            PasswordErrorDialog.DefaultButton = ContentDialogButton.Close;
                            PasswordErrorDialog.Content = "The passowrd you entered is wrong. Please try again. If you think this is a mistake, please contact the adminsitrator.";

                            await PasswordErrorDialog.ShowAsync();
                        }

                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);

                        ContentDialog RegistrationErrorDialog = new ContentDialog();
                        RegistrationErrorDialog.Title = "UnSpecified Error";
                        RegistrationErrorDialog.CloseButtonText = "Retry";
                        RegistrationErrorDialog.DefaultButton = ContentDialogButton.Close;
                        RegistrationErrorDialog.Content = "Please Contact Administrator 0x0074";

                        await RegistrationErrorDialog.ShowAsync();


                    }
                }
                else
                {
                    ContentDialog NotConnectedDialog = new ContentDialog();
                    NotConnectedDialog.Title = "Not Connected to the network";
                    NotConnectedDialog.PrimaryButtonText = "Retry";
                    NotConnectedDialog.CloseButtonText = "Cancel";
                    NotConnectedDialog.DefaultButton = ContentDialogButton.Primary;
                    NotConnectedDialog.Content = "Press Retry after you connect to the network. You must be connected to the server to make changes";


                    var NotConnectedDialogResult = await NotConnectedDialog.ShowAsync();

                    if (NotConnectedDialogResult == ContentDialogResult.Primary)
                    {
                        btn_UpdateUser_Click(sender, e);

                    }
                }
            }
            else
            {
                ContentDialog NotConnectedDialog = new ContentDialog();
                NotConnectedDialog.Title = "Not Connected to the network";
                NotConnectedDialog.PrimaryButtonText = "Retry";
                NotConnectedDialog.CloseButtonText = "Cancel";
                NotConnectedDialog.DefaultButton = ContentDialogButton.Primary;
                NotConnectedDialog.Content = "Press Retry after you connect to the network. You must be connected to the server to make changes";


                var NotConnectedDialogResult = await NotConnectedDialog.ShowAsync();

                if (NotConnectedDialogResult == ContentDialogResult.Primary)
                {
                    btn_UpdateUser_Click(sender, e);

                }
            }
            SaveImage();
            navigationBase.ValidateLoggedUser();


        }

        private void Dialog_CloseButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            btn_UpdateUser_Click(new object(), new RoutedEventArgs());
            sender.Hide();
        }

        private void UnlockUpdate()
        {
            if (lbl_EmailError.Visibility == Visibility.Collapsed && !string.IsNullOrWhiteSpace(tb_username.Text))
            {
                btn_UpdateUser.IsEnabled = true;
                btn_ChangePassword.IsEnabled = true;
            }
        }

        private void tb_email_LostFocus(object sender, RoutedEventArgs e)
        {
            // if the email box is whitespace or not matching the corrrect email format
            if (string.IsNullOrWhiteSpace(tb_email.Text) || !Regex.IsMatch(tb_email.Text, @"\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*"))
            {
                lbl_EmailError.Visibility = Visibility.Visible;  //  show the error text
                lbl_EmailError.Text = "❌ Please enter a valid email";  // set the error text
                tb_email.BorderBrush = new SolidColorBrush(Windows.UI.Color.FromArgb((byte)255, (byte)214, (byte)0, (byte)0));  // set the border color to red in the email box
                btn_UpdateUser.IsEnabled = false;  // Dissable the login button
                btn_ChangePassword.IsEnabled = false;

            }
            else
            {
                // no error on email box 

                lbl_EmailError.Visibility = Visibility.Collapsed;  // Hide the Error text
                tb_email.BorderBrush = new SolidColorBrush(Windows.UI.Color.FromArgb((byte)255, (byte)0, (byte)214, (byte)19));  // Set the email box border color to green
                UnlockUpdate();  // unlock the login if the requrenments met


            }
        }

        private void tb_username_LostFocus(object sender, RoutedEventArgs e)
        {
            // if the email box is whitespace or not matching the corrrect email format
            if (string.IsNullOrWhiteSpace(tb_username.Text))
            {
                lbl_UserNameError.Visibility = Visibility.Visible;  //  show the error text
                lbl_UserNameError.Text = "❌ Username cannot be blank";  // set the error text
                tb_username.BorderBrush = new SolidColorBrush(Windows.UI.Color.FromArgb((byte)255, (byte)214, (byte)0, (byte)0));  // set the border color to red in the email box
                btn_UpdateUser.IsEnabled = false;  // Dissable the login button

            }
            else
            {
                // no error on email box 

                lbl_UserNameError.Visibility = Visibility.Collapsed;  // Hide the Error text
                tb_username.BorderBrush = new SolidColorBrush(Windows.UI.Color.FromArgb((byte)255, (byte)0, (byte)214, (byte)19));  // Set the email box border color to green
                UnlockUpdate();  // unlock the login if the requrenments met
            }
        }

        private void btn_ChangePassword_Click(object sender, RoutedEventArgs e)
        {
            navigationBase.OpenRightPanel(typeof(ChangePassword));
            navigationBase.tempUser = new User() { Name = tb_username.Text, Email = tb_email.Text, Password = navigationBase.mainPage.LoggedUser.Password, Image = tb_username.Text + ".png" };
        }
    }
}
