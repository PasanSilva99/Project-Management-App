using PCClient.Model;
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

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace PCClient
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ChangePassword : Page
    {
        private NavigationBase navigationBase;

        public ChangePassword()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            navigationBase = e.Parameter as NavigationBase;
        }

        private async void btn_ConfirmChange_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Atempting to chanage Password");
            // Create a temporarary user
            User tempUser = new User();
            tempUser.Name = navigationBase.tempUser.Name;
            tempUser.Email = navigationBase.tempUser.Email;
            tempUser.Image = navigationBase.tempUser.Image;
            tempUser.Password = DataStore.GetHashString(tb_newPassword.Password);

            // Check the global Service type

            if (DataStore.GlobalServiceType == DataStore.ServiceType.Online)
            {

                // Check wether the computer is connected to any network
                if (DataStore.CheckConnectivity())
                {
                    try
                    {
                        var isValid = DataStore.ValidateUser(tempUser.Email, DataStore.GetHashString(tb_currentPassword.Password));

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
                                dialog.CloseButtonClick += Dialog_CloseButtonClick;

                                var result = await dialog.ShowAsync();  // show the messgae and get the result

                                if (result == ContentDialogResult.Primary) // if the user click retry,
                                    btn_ConfirmChange_Click(sender, e); // Re run this function

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
                    NotConnectedDialog.SecondaryButtonText = "Offline Login";
                    NotConnectedDialog.CloseButtonText = "Cancel";
                    NotConnectedDialog.DefaultButton = ContentDialogButton.Primary;
                    NotConnectedDialog.Content = "Press Retry after you connect to the network. If you want to continue with the cached data on your computer, press Work Offline";


                    var NotConnectedDialogResult = await NotConnectedDialog.ShowAsync();

                    if (NotConnectedDialogResult == ContentDialogResult.Primary)
                    {
                        btn_ConfirmChange_Click(sender, e);

                    }
                    else if (NotConnectedDialogResult == ContentDialogResult.Secondary)
                    {
                        DataStore.GlobalServiceType = DataStore.ServiceType.Offline;
                        var IsUserValid = DataStore.ValidateUserLocal(tempUser.Email, DataStore.GetHashString(tb_currentPassword.Password));

                        if (IsUserValid)
                        {
                            var isSuccessLocal = await DataStore.UpdateUserLocally(navigationBase.mainPage.LoggedUser, tempUser);

                            if (!isSuccessLocal)
                            {
                                return;
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
                }
            }
            else
            {
                DataStore.GlobalServiceType = DataStore.ServiceType.Offline;
                var IsUserValid = DataStore.ValidateUserLocal(tempUser.Email, tb_currentPassword.Password);

                if (IsUserValid)
                {
                    var isSuccessLocal = await DataStore.UpdateUserLocally(navigationBase.mainPage.LoggedUser, tempUser);

                    if (!isSuccessLocal)
                    {
                        return;
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


        }

        private void Dialog_CloseButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            navigationBase.OpenRightPanel(typeof(UserSettingsPage));
        }

        private void tb_currentPassword_PasswordChanging(PasswordBox sender, PasswordBoxPasswordChangingEventArgs args)
        {
            lbl_currentPasswordEError.Visibility = Visibility.Collapsed;  // Hide the Error text
            sender.BorderBrush = new SolidColorBrush(Windows.UI.Color.FromArgb((byte)255, (byte)0, (byte)214, (byte)19));  // Set the email box border color to green
        }

        private void tb_currentPassword_LostFocus(object sender, RoutedEventArgs e)
        {
            CheckPassword(lbl_currentPasswordEError, tb_currentPassword.Password, sender as PasswordBox);
        }

        private void tb_newPassword_PasswordChanging(PasswordBox sender, PasswordBoxPasswordChangingEventArgs args)
        {
            lbl_newPasswordEError.Visibility = Visibility.Collapsed;  // Hide the Error text
            sender.BorderBrush = new SolidColorBrush(Windows.UI.Color.FromArgb((byte)255, (byte)0, (byte)214, (byte)19));  // Set the email box border color to green
        }

        private void tb_newPassword_LostFocus(object sender, RoutedEventArgs e)
        {
            CheckPassword(lbl_newPasswordEError, tb_newPassword.Password, sender as PasswordBox);

            // Check for empty passwords
            if (!string.IsNullOrWhiteSpace(tb_rePassword.Password) && !string.IsNullOrWhiteSpace(tb_newPassword.Password))
            {
                if (tb_rePassword.Password.Length > 3) // If it is only longer than 6 charactors
                {
                    if (tb_newPassword.Password != tb_rePassword.Password)  // If the passwords match
                    {
                        lbl_newPasswordEError.Visibility = Visibility.Visible;  //  show the error text
                        lbl_newPasswordEError.Text = "❌ Passwords Not Match";  // set the error text
                        tb_newPassword.BorderBrush = new SolidColorBrush(Windows.UI.Color.FromArgb((byte)255, (byte)214, (byte)0, (byte)0));  // set the border color to red in the email box
                        btn_ConfirmChange.IsEnabled = false;  // Dissable the login button
                    }
                    else
                    {
                        lbl_newPasswordEError.Visibility = Visibility.Collapsed;  // Hide the Error text
                        tb_newPassword.BorderBrush = new SolidColorBrush(Windows.UI.Color.FromArgb((byte)255, (byte)0, (byte)214, (byte)19));  // Set the email box border color to green
                        UnlockConfirm();  // unlock the login if the requrenments met
                    }
                }
            }
        }

        private void tb_rePassword_PasswordChanging(PasswordBox sender, PasswordBoxPasswordChangingEventArgs args)
        {
            lbl_rePasswordEError.Visibility = Visibility.Collapsed;  // Hide the Error text
            sender.BorderBrush = new SolidColorBrush(Windows.UI.Color.FromArgb((byte)255, (byte)0, (byte)214, (byte)19));  // Set the email box border color to green
        }

        private void tb_rePassword_LostFocus(object sender, RoutedEventArgs e)
        {
            CheckPassword(lbl_rePasswordEError, tb_rePassword.Password, sender as PasswordBox);

            // Check for empty passwords
            if (!string.IsNullOrWhiteSpace(tb_rePassword.Password) && !string.IsNullOrWhiteSpace(tb_newPassword.Password))
            {
                if (tb_newPassword.Password.Length > 3) // If it is only longer than 6 charactors
                {
                    if (tb_newPassword.Password != tb_rePassword.Password)  // If the passwords match
                    {
                        lbl_rePasswordEError.Visibility = Visibility.Visible;  //  show the error text
                        lbl_rePasswordEError.Text = "❌ Passwords Not Match";  // set the error text
                        tb_rePassword.BorderBrush = new SolidColorBrush(Windows.UI.Color.FromArgb((byte)255, (byte)214, (byte)0, (byte)0));  // set the border color to red in the email box
                        btn_ConfirmChange.IsEnabled = false;  // Dissable the login button
                    }
                    else
                    {
                        lbl_rePasswordEError.Visibility = Visibility.Collapsed;  // Hide the Error text
                        tb_rePassword.BorderBrush = new SolidColorBrush(Windows.UI.Color.FromArgb((byte)255, (byte)0, (byte)214, (byte)19));  // Set the email box border color to green
                        UnlockConfirm();  // unlock the login if the requrenments met
                    }
                }
            }
        }


        private void CheckPassword(TextBlock errorLabel, string password, PasswordBox sender)
        {
            // if the email box is whitespace or not matching the corrrect email format
            if (string.IsNullOrWhiteSpace(password))
            {
                errorLabel.Visibility = Visibility.Visible;  //  show the error text
                errorLabel.Text = "❌ Password cannot be blank";  // set the error text
                sender.BorderBrush = new SolidColorBrush(Windows.UI.Color.FromArgb((byte)255, (byte)214, (byte)0, (byte)0));  // set the border color to red in the email box
                btn_ConfirmChange.IsEnabled = false;  // Dissable the login button

            }
            if (password.Length < 6)
            {
                errorLabel.Visibility = Visibility.Visible;  //  show the error text
                errorLabel.Text = "❌ Password must be longer than 6 chanaractors";  // set the error text
                sender.BorderBrush = new SolidColorBrush(Windows.UI.Color.FromArgb((byte)255, (byte)214, (byte)0, (byte)0));  // set the border color to red in the email box
                btn_ConfirmChange.IsEnabled = false;  // Dissable the login button

            }
            else
            {
                // no error on email box 

                errorLabel.Visibility = Visibility.Collapsed;  // Hide the Error text
                sender.BorderBrush = new SolidColorBrush(Windows.UI.Color.FromArgb((byte)255, (byte)0, (byte)214, (byte)19));  // Set the email box border color to green
                UnlockConfirm();  // unlock the login if the requrenments met
            }
        }

        private void UnlockConfirm()
        {
            if (!string.IsNullOrWhiteSpace(tb_currentPassword.Password) && !string.IsNullOrWhiteSpace(tb_newPassword.Password) && !string.IsNullOrWhiteSpace(tb_rePassword.Password)
                && lbl_currentPasswordEError.Visibility == Visibility.Collapsed && lbl_newPasswordEError.Visibility == Visibility.Collapsed &&
                lbl_rePasswordEError.Visibility == Visibility.Collapsed)
            {
                btn_ConfirmChange.IsEnabled = true;
            }
        }
    }
}
