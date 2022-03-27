using PCClient.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using static PCClient.Model.DataStore;


// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace PCClient
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Login : Page
    {
        private ServiceType serviceType = ServiceType.Online;

        MainPage mainPage;

        public Login()
        {
            this.InitializeComponent();
            IntializeLocalDatabase(); // comes from dataStore Class
            CheckConnectionAsync();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            mainPage = e.Parameter as MainPage;
        }

        private async void CheckConnectionAsync()
        {
            // Connectivity check is coming from DataStore Class
            if (!CheckConnectivity()) 
            {
                ContentDialog dialog = new ContentDialog();
                dialog.Title = "Connectivity Lost";
                dialog.PrimaryButtonText = "Retry";
                dialog.CloseButtonText = "Continue with limited functions";
                dialog.DefaultButton = ContentDialogButton.Primary;
                dialog.Content = "Connection to ther server is not available. Please connect to the same network which is with the servers. If you continue without the connection, this app will swith to the offline mode.";

                var result = await dialog.ShowAsync();

                if (result == ContentDialogResult.Primary)
                {
                    CheckConnectionAsync();
                }
                else
                {
                    Debug.WriteLine("Application Set to Offline Mode");
                    serviceType = ServiceType.Offline;
                }

            }
            else
            {
                Debug.WriteLine("Application Set to Online Mode");
                serviceType= ServiceType.Online;


            }
        }

        private void chb_rememberme_Click(object sender, RoutedEventArgs e)
        {

        }

        private async void btn_login_Click(object sender, RoutedEventArgs e)
        {
            CheckConnectionAsync();
            var email = tb_email.Text;
            var password = GetHashString(tb_password.Password);

            if (email != null && password != null)
            {
                if(ValidateUser(email, password))
                {
                    mainPage.NavigateToNavigationBase();
                }
                else
                {
                    if (IsUserRegistered(email))
                    {
                        ContentDialog dialog = new ContentDialog();
                        dialog.Title = "Passwords Do Not Match";
                        dialog.CloseButtonText = "Retry";
                        dialog.DefaultButton = ContentDialogButton.Close;
                        dialog.Content = "The passowrd you entered is wrong. Please try again. ";

                        var result = await dialog.ShowAsync();

                    }
                    else
                    {
                        ContentDialog dialog = new ContentDialog();
                        dialog.Title = "Register";
                        dialog.CloseButtonText = "Done";
                        dialog.DefaultButton = ContentDialogButton.Close;

                        dialog.Content = new RegistrationPage(email);

                        var result = await dialog.ShowAsync();
                    }
                }
            }
        }

        /// <summary>
        /// Check the string inside the email box wether the entered email is in correct format
        /// if it is not in the correct format, show the error text
        /// and dissable the login button
        /// if it is valid, enable the login button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tb_email_LostFocus(object sender, RoutedEventArgs e)
        {
            // if the email box is whitespace or not matching the corrrect email format
            if (string.IsNullOrWhiteSpace(tb_email.Text) || !Regex.IsMatch(tb_email.Text, @"\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*"))
            {
                lbl_email_error.Visibility = Visibility.Visible;  //  show the error text
                lbl_email_error.Text = "❌ Please enter a valid email";  // set the error text
                tb_email.BorderBrush = new SolidColorBrush(Windows.UI.Color.FromArgb((byte)255, (byte)214, (byte)0, (byte)0));  // set the border color to red in the email box
                btn_login.IsEnabled = false;  // Dissable the login button

            }
            else
            {
                // no error on email box 

                lbl_email_error.Visibility = Visibility.Collapsed;  // Hide the Error text
                tb_email.BorderBrush = new SolidColorBrush(Windows.UI.Color.FromArgb((byte)255, (byte)0, (byte)214, (byte)19));  // Set the email box border color to green
                UnlockLogin();  // unlock the login if the requrenments met


            }
        }

        private void tb_email_TextChanging(TextBox sender, TextBoxTextChangingEventArgs args)
        {
            lbl_email_error.Visibility = Visibility.Collapsed;  // Hide the error text when updating text
            UnlockLogin();  // unlock the login if the requrenments met (User might entered the email after the password)

        }

        private void tb_password_LostFocus(object sender, RoutedEventArgs e)
        {
            // If the password is not entered
            if (string.IsNullOrWhiteSpace(tb_password.Password))
            {
                lbl_password_error.Visibility = Visibility.Visible;  // Show the error text
                lbl_password_error.Text = "❌ Password cannot be blank";  // Set the Error Text
                tb_password.BorderBrush = new SolidColorBrush(Windows.UI.Color.FromArgb((byte)255, (byte)214, (byte)0, (byte)0));  // Password box border to red
                btn_login.IsEnabled = false;  // dissable the login button

            }

            //  if the password is too short ( less than 6 charactors)
            else if (tb_password.Password.Length < 6)
            {

                lbl_password_error.Visibility = Visibility.Visible;  // shoe the error text
                lbl_password_error.Text = "❌ Password is too short. Should be minimum 6 Charactors";  // set the text
                tb_password.BorderBrush = new SolidColorBrush(Windows.UI.Color.FromArgb((byte)255, (byte)214, (byte)0, (byte)0));  // set the border of password to red
                btn_login.IsEnabled = false;  // dissable the login button

                
            }
            else
            {

                // no error in the password box

                lbl_password_error.Visibility = Visibility.Collapsed;  // Hide the error text
                tb_password.BorderBrush = new SolidColorBrush(Windows.UI.Color.FromArgb((byte)255, (byte)214, (byte)201, (byte)201));  // set the password bpx border color to gray (Password can be still wrong.)
                UnlockLogin();  // unlock the login button if the all requenments met


            }
        }
        private void tb_password_PasswordChanging(PasswordBox sender, PasswordBoxPasswordChangingEventArgs args)
        {
            lbl_password_error.Visibility = Visibility.Collapsed;  // hide error text
            UnlockLogin();  // unlock the login button if the all requenments met
        }
        private void UnlockLogin()
        { 
            // if the error messages are not visible
            // if the email and password is not empty
            // enable the button
            if (lbl_password_error.Visibility != Visibility.Visible && lbl_email_error.Visibility != Visibility.Visible && !string.IsNullOrWhiteSpace(tb_email.Text) && !string.IsNullOrWhiteSpace(tb_password.Password))
                btn_login.IsEnabled = true;
        }
    }
}
