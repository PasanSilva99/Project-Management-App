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
using Windows.Storage;
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

        internal MainPage mainPage;

        private static string key = "hVmYq3t6v9y$B&E)H@McQfTjWnZr4u7x";

        internal bool isAutoLogin = false;

        public bool RememberUser { get; private set; }

        public Login()
        {
            this.InitializeComponent();
            IntializeLocalDatabase(); // comes from dataStore Class
            Server.PMServer1.IntializeDatabaseService1();
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
                ContentDialog DialogConLost = new ContentDialog();
                DialogConLost.Title = "Connectivity Lost";
                DialogConLost.PrimaryButtonText = "Retry";
                DialogConLost.CloseButtonText = "Continue with limited functions";
                DialogConLost.DefaultButton = ContentDialogButton.Primary;
                DialogConLost.Content = "Connection to ther server is not available. Please connect to the same network which is with the servers. If you continue without the connection, this app will swith to the offline mode.";

                var DialogConLost_Result = await DialogConLost.ShowAsync();

                if (DialogConLost_Result == ContentDialogResult.Primary)
                {
                    CheckConnectionAsync();
                }
                else
                {
                    Debug.WriteLine("Application Set to Offline Mode");
                    serviceType = ServiceType.Offline;
                    GlobalServiceType = ServiceType.Offline;
                }

            }
            else
            {
                Debug.WriteLine("Application Set to Online Mode");
                serviceType = ServiceType.Online;
                GlobalServiceType = ServiceType.Online;

            }
        }

        private void chb_rememberme_Click(object sender, RoutedEventArgs e)
        {
            if (chb_rememberme.IsChecked == true) RememberUser = true;
            else RememberUser = false;
        }

        private async void btn_login_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Login");
            var email = tb_email.Text;
            var password = GetHashString(tb_password.Password);

            if (email != null && password != null)
            {
                if (GlobalServiceType == ServiceType.Online)
                {
                    if (CheckConnectivity())
                    {
                        try
                        {
                            var isValidUser = ValidateUser(email, password);

                            if (isValidUser)
                            {
                                ContinueToNavigator(email, password);
                            }
                            else
                            {
                                if (IsUserRegistered(email))
                                {
                                    ShowPasswordErrorDialog();
                                }
                                else
                                {
                                    ShowRegisterDialog(email, tb_password.Password);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            ContentDialog RegistrationErrorDialog = new ContentDialog();
                            RegistrationErrorDialog.Title = "UnSpecified Error";
                            RegistrationErrorDialog.CloseButtonText = "Retry";
                            RegistrationErrorDialog.DefaultButton = ContentDialogButton.Close;
                            RegistrationErrorDialog.Content = "Please Contact Administrator 0x008E";

                            await RegistrationErrorDialog.ShowAsync();

                            Debug.WriteLine(ex.Message);
                        }

                    }
                    else
                    {
                        if (!isAutoLogin)
                        {
                            ContentDialog NotConnectedDialog = new ContentDialog();
                            NotConnectedDialog.Title = "Not Connected to the network";
                            NotConnectedDialog.PrimaryButtonText = "Retry";
                            NotConnectedDialog.SecondaryButtonText = "Offline Login";
                            NotConnectedDialog.CloseButtonText = "Ok";
                            NotConnectedDialog.DefaultButton = ContentDialogButton.Primary;
                            NotConnectedDialog.Content = "Press Retry after you connect to the network. If you want to continue with the cased data on your computer, press Offline Login";


                            var NotConnectedDialogResult = await NotConnectedDialog.ShowAsync();

                            if (NotConnectedDialogResult == ContentDialogResult.Primary)
                            {
                                btn_login_Click(sender, e);

                            }
                            else if (NotConnectedDialogResult == ContentDialogResult.Secondary)
                            {
                                GlobalServiceType = ServiceType.Offline;
                                var IsUserValid = ValidateUserLocal(email, password);

                                if (IsUserValid)
                                {
                                    ContinueToNavigator(email, password);
                                }
                                else
                                {
                                    if (IsUserRegistered(email))
                                    {
                                        ShowPasswordErrorDialog();

                                    }
                                    else
                                    {
                                        ShowRegisterDialog(email, tb_password.Password);
                                    }
                                }
                            }
                        }
                        else  // Approching heare means its offline
                        {
                            GlobalServiceType = ServiceType.Offline;
                            var IsUserValid = ValidateUserLocal(email, password);

                            if (IsUserValid)
                            {
                                ContinueToNavigator(email, password);
                            }
                            else
                            {
                                if (IsUserRegistered(email))
                                {
                                    ShowPasswordErrorDialog();

                                }
                                else
                                {
                                    ShowRegisterDialog(email, tb_password.Password);
                                }
                            }
                        }

                    }
                }
                else
                {
                    try
                    {
                        var result = ValidateUserLocal(email, password);

                        if (result)
                        {
                            mainPage.LoggedUser = FindUserLocal(email, password);
                            mainPage.NavigateToNavigationBase();
                        }
                        else
                        {
                            if (IsUserRegistered(email))
                            {
                                ShowPasswordErrorDialog();

                            }
                            else
                            {
                                ShowRegisterDialog(email, tb_password.Password);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                    }
                }
            }
        }

        private void ContinueToNavigator(String email, String password)
        {
            var user = FindUserLocal(email, password);
            mainPage.LoggedUser = user;
            mainPage.NavigateToNavigationBase();

            if (RememberUser)
            {
                ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
                localSettings.Values["RememberedUser"] = email;
                localSettings.Values["RememberedPassword"] = EncOperator.EncryptString(key, tb_password.Password);
                
            }
        }

        private async void ShowPasswordErrorDialog()
        {
            ContentDialog PasswordErrorDialog = new ContentDialog();
            PasswordErrorDialog.Title = "Passwords Do Not Match";
            PasswordErrorDialog.CloseButtonText = "Retry";
            PasswordErrorDialog.DefaultButton = ContentDialogButton.Close;
            PasswordErrorDialog.Content = "The passowrd you entered is wrong. Please try again. ";

            await PasswordErrorDialog.ShowAsync();
        }

        private async void ShowRegisterDialog(String email, String password)
        {
            ContentDialog RegisterDialog = new ContentDialog();
            RegisterDialog.Title = "Register";
            RegisterDialog.CloseButtonText = "Done";
            RegisterDialog.DefaultButton = ContentDialogButton.Close;

            RegisterDialog.Content = new RegistrationPage(email, password);

            await RegisterDialog.ShowAsync();
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

        private void btn_login_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

                var email = localSettings.Values["RememberedUser"] as string;
                var password = localSettings.Values["RememberedPassword"] as string;

                Debug.WriteLine(password);
                Debug.WriteLine(EncOperator.DecryptString(key, password));

                if (!string.IsNullOrWhiteSpace(email) && !string.IsNullOrWhiteSpace(password))
                {
                    isAutoLogin = true;
                    Debug.WriteLine("AutoLogin");
                    tb_email.Text = email;
                    tb_password.Password = EncOperator.DecryptString(key, password);
                    btn_login_Click(new object(), new RoutedEventArgs());
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }
    }
}
