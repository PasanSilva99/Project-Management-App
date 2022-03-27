using PCClient.Model;
using System;
using System.Collections.Generic;
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
    public sealed partial class RegistrationPage : Page
    {
        public RegistrationPage(string email)
        {
            this.InitializeComponent();
        }

        private void RemoveImage_Click(object sender, RoutedEventArgs e)
        {

        }
        private void AddImage_Click(object sender, RoutedEventArgs e)
        {

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
            if(tb_rePassword.Password != tb_password.Password)
            {

            }
        }

        private void tb_password_PasswordChanging(PasswordBox sender, PasswordBoxPasswordChangingEventArgs args)
        {

        }
    }
}
