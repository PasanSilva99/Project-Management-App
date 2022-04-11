using PCClient.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace PCClient
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ChatPanel : Page
    {
        NavigationBase navigationBase;

        public ChatPanel()
        {
            this.InitializeComponent();

            list_messages.Items.Clear();

            btn_emogi.IsEnabled = true;
            btn_emogi.Visibility = Visibility.Visible;

            btn_send.IsEnabled = false;
            btn_send.Visibility = Visibility.Collapsed;
        }

        private void SendMessage()
        {

            if (!string.IsNullOrWhiteSpace(tb_message.Text))
            {
                SendMessageControl sendMessageControl = new SendMessageControl();
                sendMessageControl.MessageContent = tb_message.Text;

                sendMessageControl.isSticker = false;
                sendMessageControl.sender = navigationBase.mainPage.LoggedUser.Name;
                sendMessageControl.ProfileImage = navigationBase.profileImageSource;
                sendMessageControl.Time = DateTime.Now;

                ListViewItem chatBubble = new ListViewItem();
                chatBubble.Style = Resources["ChatMessageStyle"] as Style;
                chatBubble.Content = sendMessageControl;

                list_messages.Items.Add(chatBubble);
                tb_message.Text = "";
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            navigationBase = e.Parameter as NavigationBase;
        }

        private void NewDirectUser_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ShowSendButton(bool state)
        {
            if (state)
            {
                btn_emogi.IsEnabled = false;
                btn_emogi.Visibility = Visibility.Collapsed;

                btn_send.IsEnabled = true;
                btn_send.Visibility = Visibility.Visible;
            }
            else
            {
                btn_emogi.IsEnabled = true;
                btn_emogi.Visibility = Visibility.Visible;

                btn_send.IsEnabled = false;
                btn_send.Visibility = Visibility.Collapsed;
            }
        }

        private void btn_send_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(tb_message.Text))
            {
                SendMessage();
            }
        }

        private void tb_message_TextChanging(TextBox sender, TextBoxTextChangingEventArgs args)
        {
            if (tb_message.Text.Length > 0)
            {
                Debug.WriteLine("Has Text");
                ShowSendButton(true);
            }
            else
            {
                Debug.WriteLine("No text");
                ShowSendButton(false);
                tb_message.AcceptsReturn = false;
            }
        }

        private void tb_message_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Shift)
            {
                tb_message.AcceptsReturn = true;
            }
            else
            {
                if (e.Key == Windows.System.VirtualKey.Enter)
                {
                    SendMessage();
                }
            }
        }

        private async void tb_message_Paste(object sender, TextControlPasteEventArgs e)
        {
            TextBox messageBox = sender as TextBox;
            if (messageBox != null)
            {
                // Mark the event as messageBoxhandled first. Otherwise, the
                // default paste action will happen, then the custom paste
                // action, and the user will see the text box content change.
                e.Handled = true;

                // Get content from the clipboard.
                var dataPackageView = Windows.ApplicationModel.DataTransfer.Clipboard.GetContent();
                if (dataPackageView.Contains(Windows.ApplicationModel.DataTransfer.StandardDataFormats.Text))
                {
                    try
                    {
                        var text = await dataPackageView.GetTextAsync();
                        if (text.Contains('\n'))
                            messageBox.AcceptsReturn = true;
                        messageBox.Text = text;
                        
                    }
                    catch (Exception)
                    {
                        // Ignore or handle exception as needed.
                    }
                }
            }
        }
    }
}
