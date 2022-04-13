using Projent.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
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
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Projent
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ChatPanel : Page
    {
        NavigationBase navigationBase;
        private string SelectedReceiver;

        public ChatPanel()
        {
            this.InitializeComponent();

            list_messages.Items.Clear();

            btn_emogi.IsEnabled = true;
            btn_emogi.Visibility = Visibility.Visible;

            btn_send.IsEnabled = false;
            btn_send.Visibility = Visibility.Collapsed;
        }

        private async void FetchMessages()
        {
            if (DataStore.GlobalServiceType == DataStore.ServiceType.Online)
            {
                if (DataStore.CheckConnectivity())
                {
                    try
                    {
                        List<Message> messages = null;
                        if (list_messages.Items.Count > 0)
                        {
                            messages = Server.PMServer2.FindDirectMessagesFor(navigationBase.mainPage.LoggedUser, list_messages.Items[list_messages.Items.Count - 1] as Message);
                        }
                        else
                        {
                            messages = Server.PMServer2.FindDirectMessagesFor(navigationBase.mainPage.LoggedUser, null);

                        }
                        if (messages != null)
                        {
                            UpdateMessagesList(messages);
                        }

                    }
                    catch (Exception ex)
                    {
                        ContentDialog dialog = new ContentDialog();
                        dialog.Title = "UnExpected Error";
                        dialog.PrimaryButtonText = "Retry";
                        dialog.SecondaryButtonText = "Close";
                        dialog.DefaultButton = ContentDialogButton.Primary;
                        dialog.Content = "Unexpected Error Occured 0x4368617450616E656C3633";

                        var result = await dialog.ShowAsync();  // show the messgae and get the result
                        if (result == ContentDialogResult.Primary)
                        {
                            FetchMessages();
                        }
                        else
                        {
                            navigationBase.OpenChat(this);
                        }
                    }
                }
                else
                {
                    ContentDialog dialog = new ContentDialog();
                    dialog.Title = "Connection Lost";
                    dialog.PrimaryButtonText = "Retry";
                    dialog.SecondaryButtonText = "Dismiss";
                    dialog.DefaultButton = ContentDialogButton.Primary;
                    dialog.Content = "Connection to the server is lost. Application is unable to Send/ Receive messages while it is offline. Please make sure you are connected to the network before proceding.";

                    var result = await dialog.ShowAsync();  // show the messgae and get the result
                    if (result == ContentDialogResult.Primary)
                    {
                        FetchMessages();
                    }
                    else
                    {
                        navigationBase.OpenChat(this);
                    }
                }
            }
            else
            {
                ContentDialog dialog = new ContentDialog();
                dialog.Title = "In Offline Mode";
                dialog.PrimaryButtonText = "Retry";
                dialog.SecondaryButtonText = "Dismiss";
                dialog.DefaultButton = ContentDialogButton.Primary;
                dialog.Content = "Application is unable to Send/ Receive messages while it is in offline mode. Please make sure you are connected to the network before proceding.";

                var result = await dialog.ShowAsync();  // show the messgae and get the result
                if (result == ContentDialogResult.Primary)
                {
                    FetchMessages();
                }
                else
                {
                    navigationBase.OpenChat(this);
                }
            }

        }

        public async Task<bool> IsFilePresent(string fileName)
        {
            StorageFolder storageFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("ProfilePics", CreationCollisionOption.OpenIfExists);
            var item = await storageFolder.TryGetItemAsync(fileName);
            return item != null;
        }

        private async void UpdateMessagesList(List<Message> messages)
        {
            foreach (var message in messages)
            {
                if (!message.isSticker)
                {
                    if (message.sender == navigationBase.mainPage.LoggedUser.Name)
                    {
                        SendMessageControl sendMessageControl = new SendMessageControl();
                        sendMessageControl.sender = message.sender;
                        sendMessageControl.MessageContent = message.MessageContent;
                        sendMessageControl.ProfileImage = navigationBase.profileImageSource;
                        sendMessageControl.Time = message.Time;

                        ListViewItem chatBubble = new ListViewItem();
                        chatBubble.Style = Resources["ChatMessageStyle"] as Style;
                        chatBubble.Content = sendMessageControl;

                        list_messages.Items.Add(chatBubble);
                    }
                    else
                    {
                        ReceiveMessageControl receiveMessageControl = new ReceiveMessageControl();
                        receiveMessageControl.sender = message.sender;
                        receiveMessageControl.MessageContent = message.MessageContent;

                        StorageFolder storageFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("ProfilePics", CreationCollisionOption.OpenIfExists);
                        if (await IsFilePresent(message.sender + ".png"))
                        {
                            Debug.WriteLine("Has Sender Image");
                            StorageFile imageFile = await storageFolder.GetFileAsync(message.sender + ".png");
                            using (var fileStream = await imageFile.OpenAsync(FileAccessMode.Read))
                            {
                                BitmapImage bitmapImage = new BitmapImage();  // Creates a new bitmap file 
                                await bitmapImage.SetSourceAsync(fileStream);  // Sets the loded file as the new bitmap source
                                receiveMessageControl.ProfileImage = bitmapImage;  // sets the created bitmap as an image source
                            }
                        }
                        else
                        {
                            Debug.WriteLine("Requesting Sender Image");
                            var image = await Server.PMServer1.RequestUserImage(message.sender);
                            Debug.WriteLine(image == null ? ":::Error:::" : ":::HasBuffer:::");

                            var ProfilePicFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("ProfilePics", CreationCollisionOption.OpenIfExists);
                            var ProfilePicFile = await ProfilePicFolder.CreateFileAsync(message.sender + ".png", CreationCollisionOption.ReplaceExisting);

                            await FileIO.WriteBytesAsync(ProfilePicFile, image);

                            using (var fileStream = await ProfilePicFile.OpenAsync(FileAccessMode.Read))
                            {
                                BitmapImage bitmapImage = new BitmapImage();  // Creates a new bitmap file 
                                await bitmapImage.SetSourceAsync(fileStream);  // Sets the loded file as the new bitmap source
                                receiveMessageControl.ProfileImage = bitmapImage;  // sets the created bitmap as an image source
                            }

                        }

                        receiveMessageControl.Time = message.Time;

                        ListViewItem chatBubble = new ListViewItem();
                        chatBubble.Content = receiveMessageControl;
                        chatBubble.Style = Resources["ChatMessageStyle"] as Style;
                        list_messages.Items.Add(chatBubble);
                    }
                }
            }
        }

        private async void SendMessage()
        {

            if (!string.IsNullOrWhiteSpace(tb_message.Text))
            {
                Message message = new Message();
                message.sender = navigationBase.mainPage.LoggedUser.Name;
                message.receiver = "LilyKi";
                message.MessageContent = tb_message.Text;
                message.isSticker = false;
                message.MentionedUsers = null;
                message.Time = DateTime.Now;

                if (DataStore.GlobalServiceType == DataStore.ServiceType.Online)
                {
                    if (DataStore.CheckConnectivity())
                    {
                        try
                        {
                            bool isSussess = false;
                            if (list_messages.Items.Count > 0)
                            {
                                isSussess = Server.PMServer2.NewMessage(message);
                            }
                            else
                            {
                                isSussess = Server.PMServer2.NewMessage(message);

                            }
                            if (isSussess)
                            {
                                FetchMessages();
                            }

                        }
                        catch (Exception ex)
                        {
                            ContentDialog dialog = new ContentDialog();
                            dialog.Title = "UnExpected Error";
                            dialog.PrimaryButtonText = "Retry";
                            dialog.SecondaryButtonText = "Close";
                            dialog.DefaultButton = ContentDialogButton.Primary;
                            dialog.Content = "Unexpected Error Occured 0x4368617450616E656C3633";

                            var result = await dialog.ShowAsync();  // show the messgae and get the result
                            if (result == ContentDialogResult.Primary)
                            {
                                SendMessage();
                            }
                            else
                            {
                                navigationBase.OpenChat(this);
                            }
                        }
                    }
                    else
                    {
                        ContentDialog dialog = new ContentDialog();
                        dialog.Title = "Connection Lost";
                        dialog.PrimaryButtonText = "Retry";
                        dialog.SecondaryButtonText = "Dismiss";
                        dialog.DefaultButton = ContentDialogButton.Primary;
                        dialog.Content = "Connection to the server is lost. Application is unable to Send/ Receive messages while it is offline. Please make sure you are connected to the network before proceding.";

                        var result = await dialog.ShowAsync();  // show the messgae and get the result
                        if (result == ContentDialogResult.Primary)
                        {
                            SendMessage();
                        }
                        else
                        {
                            navigationBase.OpenChat(this);
                        }
                    }
                }
                else
                {
                    ContentDialog dialog = new ContentDialog();
                    dialog.Title = "In Offline Mode";
                    dialog.PrimaryButtonText = "Retry";
                    dialog.SecondaryButtonText = "Dismiss";
                    dialog.DefaultButton = ContentDialogButton.Primary;
                    dialog.Content = "Application is unable to Send/ Receive messages while it is in offline mode. Please make sure you are connected to the network before proceding.";

                    var result = await dialog.ShowAsync();  // show the messgae and get the result
                    if (result == ContentDialogResult.Primary)
                    {
                        SendMessage();
                    }
                    else
                    {
                        navigationBase.OpenChat(this);
                    }
                }

            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            navigationBase = e.Parameter as NavigationBase;
            FetchMessages();
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
                FetchMessages();
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
        public async Task<StorageFile> BytesToStorageFile(byte[] imageBuffer, string fileName)
        {
            StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
            StorageFile sampleFile = await storageFolder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
            await FileIO.WriteBytesAsync(sampleFile, imageBuffer);
            return sampleFile;
        }
    }
}
