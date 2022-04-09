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
        public ChatPanel()
        {
            this.InitializeComponent();

            list_messages.Items.Clear();            
        }

        private async void SendMessage()
        {
            // Create new folder to store the ProfileImages. If it is already there, Open it. 
            StorageFolder storageFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("Messages", CreationCollisionOption.OpenIfExists);


            StorageFile file = await storageFolder.CreateFileAsync("TempMessage", CreationCollisionOption.ReplaceExisting);
            if (file != null)
            {
                Debug.WriteLine(file.Path);
                // Prevent updates to the remote version of the file until we
                // finish making changes and call CompleteUpdatesAsync.
                CachedFileManager.DeferUpdates(file);
                // write to file
                Windows.Storage.Streams.IRandomAccessStream randAccStream =
                    await file.OpenAsync(FileAccessMode.ReadWrite);

                reb_message.Document.SaveToStream(TextGetOptions.FormatRtf, randAccStream);

                // Let Windows know that we're finished changing the file so the
                // other app can update the remote version of the file.
                Windows.Storage.Provider.FileUpdateStatus status = await CachedFileManager.CompleteUpdatesAsync(file);
                if (status != Windows.Storage.Provider.FileUpdateStatus.Complete)
                {
                    Windows.UI.Popups.MessageDialog errorBox =
                        new Windows.UI.Popups.MessageDialog("File " + file.Name + " couldn't be saved.");
                    await errorBox.ShowAsync();
                }
            }

            ReceiveMessageControl receiveMessageControl = new ReceiveMessageControl();
            

        }

        private void HighlightMentionedUsers()
        {

            var users = new List<User>();
            users = DataStore.FetchUsers();

            foreach (var user in users)
            {
                ITextRange searchRange = reb_message.Document.GetRange(0, 0);
                while (searchRange.FindText("@"+user.Name, TextConstants.MaxUnitCount, FindOptions.Word) > 0)
                {
                    searchRange.CharacterFormat.BackgroundColor = Windows.UI.Color.FromArgb((byte)255, (byte)226, (byte)245, (byte)201); ;
                    searchRange.CharacterFormat.ForegroundColor = Windows.UI.Color.FromArgb((byte)255, (byte)0, (byte)0, (byte)0);
                }
            }
        }

        private void RemoveHightlight()
        {
            ITextRange documentRange = reb_message.Document.GetRange(0, TextConstants.MaxUnitCount);
            SolidColorBrush defaultBackground = reb_message.Background as SolidColorBrush;
            SolidColorBrush defaultForeground = reb_message.Foreground as SolidColorBrush;

            documentRange.CharacterFormat.BackgroundColor = defaultBackground.Color;
            documentRange.CharacterFormat.ForegroundColor = defaultForeground.Color;

        }

        private void NewDirectUser_Click(object sender, RoutedEventArgs e)
        {

        }

        private void reb_message_TextChanging(RichEditBox sender, RichEditBoxTextChangingEventArgs args)
        {
            RemoveHightlight();
            HighlightMentionedUsers();

            string cText;
            reb_message.Document.GetText(TextGetOptions.UseCrlf,out cText);

            if (cText.Length > 0)
            {
                Debug.WriteLine("Has Text");
                ShowSendButton(true);
            }
            else
            {
                Debug.WriteLine("No text");
                ShowSendButton(false);
            }
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

        private void reb_message_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Shift)
            {
                reb_message.AcceptsReturn = true;
            }
            else
            {
                reb_message.AcceptsReturn = false;
            }
        }

        private void btn_send_Click(object sender, RoutedEventArgs e)
        {
            if(reb_message.Document != null)
            {
                SendMessage();
            }
        }
    }
}
