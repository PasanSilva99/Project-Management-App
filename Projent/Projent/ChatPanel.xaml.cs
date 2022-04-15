using Projent.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        private string SelectedReceiver = "LilyKi";
        public DispatcherTimer ChatTimer = new DispatcherTimer();
        public DateTime latestMessageTime = DateTime.MinValue;

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
                            var MessageControlList = list_messages.Items;

                            var LastMessageControl = MessageControlList[MessageControlList.Count - 1] as ListViewItem;

                            var LastMessage = LastMessageControl.Content;

                            var messagesOList = new ObservableCollection<PMServer2.Message>();

                            if (LastMessageControl.Content.GetType() == typeof(ReceiveMessageControl))
                            {
                                messagesOList = await Server.ProjectServer.projectServiceClient.FindDirectMessagesForAsync(
                                navigationBase.mainPage.LoggedUser.Name, (LastMessage as ReceiveMessageControl).Time);
                            }
                            else
                            {
                                messagesOList = await Server.ProjectServer.projectServiceClient.FindDirectMessagesForAsync(
                                navigationBase.mainPage.LoggedUser.Name, (LastMessage as SendMessageControl).Time);
                            }
                            //Debug.WriteLine(LastMessage.ToString());

                            messages = Converter.GetLocalMessageList(messagesOList.ToList());
                        }
                        else
                        {
                            var messagesOList = await Server.ProjectServer.projectServiceClient.FindDirectMessagesForAsync(
                                navigationBase.mainPage.LoggedUser.Name,
                                DateTime.MinValue);
                            messages = Converter.GetLocalMessageList(messagesOList.ToList());

                        }
                        if (messages != null)
                        {
                            var latestMessage = messages.OrderByDescending(m => m.Time).FirstOrDefault();
                            latestMessageTime = latestMessage.Time;
                            UpdateMessagesList(
                                messages.Where(
                                    message => 
                                    ( message.sender == navigationBase.mainPage.LoggedUser.Name && message.receiver == SelectedReceiver) ||
                                    ( message.sender == SelectedReceiver && message.receiver == navigationBase.mainPage.LoggedUser.Name)).ToList());
                        }

                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex);
                        ContentDialog dialog = new ContentDialog();
                        dialog.Title = "UnExpected Error";
                        dialog.PrimaryButtonText = "Retry";
                        dialog.SecondaryButtonText = "Close";
                        dialog.DefaultButton = ContentDialogButton.Primary;
                        dialog.Content = $"Unexpected Error Occured \n{ex.Message}";

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
                            var image = await Server.MainServer.mainServiceClient.RequestUserImageAsync(message.sender);
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
                message.receiver = SelectedReceiver;
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

                            isSussess = await Server.ProjectServer.projectServiceClient.NewMessageAsync(Converter.ToServerMessage(message));

                            if (isSussess)
                            {
                                FetchMessages();

                                tb_message.Text = "";
                            }
                            else
                            {
                                ContentDialog dialog = new ContentDialog();
                                dialog.Title = "UnExpected Error";
                                dialog.CloseButtonText = "Ok";
                                dialog.DefaultButton = ContentDialogButton.Primary;
                                dialog.Content = "Couldnt Send the message.";

                                await dialog.ShowAsync();
                            }

                        }
                        catch (Exception ex)
                        {
                            ContentDialog dialog = new ContentDialog();
                            dialog.Title = "UnExpected Error";
                            dialog.PrimaryButtonText = "Retry";
                            dialog.SecondaryButtonText = "Close";
                            dialog.DefaultButton = ContentDialogButton.Primary;
                            dialog.Content = $"Unexpected Error Occured\n{ex.Message}";

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

            stack_users.Children.Clear();  // Clear all Test Users

            LoadDirectUsers();
            navigationBase.loadedChatPanel = this;

            ChatTimer.Interval = new TimeSpan(0, 0, 1);
            ChatTimer.Tick += ChatTimer_Tick;
            ChatTimer.Start();
        }

        private async void ChatTimer_Tick(object sender, object e)
        {
            var isNewMessagesAvailable = await Server.ProjectServer.projectServiceClient.CheckNewMessagesForAsync(navigationBase.mainPage.LoggedUser.Name, latestMessageTime);
            if (isNewMessagesAvailable)
            {
                // get the new messages list from the latest message time
                // show the toast notifications if the user is the receiver
                // if the user is the sender, and the selected receiver is the new message receiver update the message list
                // 
            }
        }

        private async void LoadDirectUsers()
        {
            stack_users.Children.Clear();
            var DirectUserList = DataStore.FetchDirectUsers(navigationBase.mainPage.LoggedUser.Name);
            navigationBase.directUsers = DirectUserList;
            if (DirectUserList != null && DirectUserList.Count > 0)
            {
                foreach (var user in DirectUserList)
                {
                    Button directUserButton = new Button();
                    directUserButton.Style = Resources["UserButton"] as Style;
                    directUserButton.Tag = user;

                    var DirectUserImage = new Image();

                    if (!await IsFilePresent(user.Name + ".png"))
                    {
                        var imageBuffer = await Server.MainServer.mainServiceClient.RequestUserImageAsync(user.Name);

                        var ProfilePicFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("Cache", CreationCollisionOption.OpenIfExists);
                        var ProfilePicFile = await ProfilePicFolder.CreateFileAsync(user.Name + ".png", CreationCollisionOption.OpenIfExists);

                        await FileIO.WriteBytesAsync(ProfilePicFile, imageBuffer);

                        using (var fileStream = await ProfilePicFile.OpenAsync(FileAccessMode.Read))
                        {
                            BitmapImage bitmapImage = new BitmapImage();  // Creates a new bitmap file 
                            await bitmapImage.SetSourceAsync(fileStream);  // Sets the loded file as the new bitmap source
                            DirectUserImage.Source = bitmapImage;  // sets the created bitmap as an image source
                        }

                    }
                    else
                    {
                        var ProfilePicFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("Cache", CreationCollisionOption.OpenIfExists);
                        var ProfilePicFile = await ProfilePicFolder.GetFileAsync(user.Name + ".png");

                        using (var fileStream = await ProfilePicFile.OpenAsync(FileAccessMode.Read))
                        {
                            BitmapImage bitmapImage = new BitmapImage();  // Creates a new bitmap file 
                            await bitmapImage.SetSourceAsync(fileStream);  // Sets the loded file as the new bitmap source
                            DirectUserImage.Source = bitmapImage;  // sets the created bitmap as an image source
                        }
                    }

                    directUserButton.Content = DirectUserImage;
                    directUserButton.RightTapped += DirectUserButton_RightTapped;
                    directUserButton.Tapped += DirectUserButton_Tapped;
                    FlyoutBase.SetAttachedFlyout(directUserButton, Resources["UserInfo"] as Flyout);

                    stack_users.Children.Add(directUserButton);
                }
            }
        }

        private void DirectUserButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            list_messages.Items.Clear();
            SelectedReceiver = ((sender as Button).Tag as DirectUser).Name;
            FetchMessages();
        }

        private void DirectUserButton_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            var user = (sender as Button).Tag as DirectUser;
            lbl_DirectUserEmail.Text = user.Email;
            lbl_DirectUsername.Text = user.Name;

            img_directUserImage.Source = ((sender as Button).Content as Image).Source;

            btn_removeDirectuser.Tag = user;
            btn_removeClearChat.Tag = user;

            FlyoutBase.ShowAttachedFlyout(sender as Button);
        }

        private async void NewDirectUser_Click(object sender, RoutedEventArgs e)
        {
            list_directUsers.Items.Clear();
            var users = await Server.MainServer.mainServiceClient.FetchUsersAsync();

            List<DirectUser> directUsers = new List<DirectUser>();

            if (users != null && users.Count > 0)
            {
                foreach (var user in users)
                {
                    var directUserControl = new DirectUserControl() { directUser = new DirectUser() { Name = user.Name, Email = user.Email } };
                    var DirectUserListItem = new ListViewItem();
                    DirectUserListItem.Style = Resources["DirectUserItem"] as Style;
                    DirectUserListItem.Content = directUserControl;
                    DirectUserListItem.Tag = new DirectUser() { Name = user.Name, Email = user.Email };
                    DirectUserListItem.Tapped += DirectUserListItem_Tapped;

                    list_directUsers.Items.Add(DirectUserListItem);

                }
            }

        }

        public void SetUserStaus(DirectUser directUser, DataStore.Status status)
        {
            var DirectUsersButtons = stack_users.Children;

            foreach (var DirectUserButton in DirectUsersButtons)
            {
                var btn = DirectUserButton as Button;
                var dr = btn.Tag as DirectUser;
                if(dr != null)
                    if(dr.Email == directUser.Email)
                    {
                        btn.BorderBrush = GetColorForStatus(status);
                    }

            }
        }

        internal SolidColorBrush GetColorForStatus(DataStore.Status status)
        {
            switch (status)
            {
                case DataStore.Status.Offline: 
                    return new SolidColorBrush(Windows.UI.Color.FromArgb((byte)255, (byte)74, (byte)74, (byte)74));
                case DataStore.Status.Online:
                    return new SolidColorBrush(Windows.UI.Color.FromArgb((byte)255, (byte)36, (byte)230, (byte)13));
                case DataStore.Status.Busy:
                    return new SolidColorBrush(Windows.UI.Color.FromArgb((byte)255, (byte)237, (byte)14, (byte)55));
                case DataStore.Status.Invisible:
                    return new SolidColorBrush(Windows.UI.Color.FromArgb((byte)255, (byte)0, (byte)0, (byte)0));
                case DataStore.Status.Idle:
                    return new SolidColorBrush(Windows.UI.Color.FromArgb((byte)255, (byte)247, (byte)195, (byte)51));
                default: return null;
            }
        }

        private void DirectUserListItem_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var user = (sender as ListViewItem).Tag as DirectUser;
            if (user != null)
            {
                var isSuccess = DataStore.NewDirectUser(user, navigationBase.mainPage.LoggedUser.Name);
            }
            LoadDirectUsers();
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
                tb_message.AcceptsReturn = false;
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

        private void Flyout_Opened(object sender, object e)
        {

        }

        private void btn_removeDirectuser_Click(object sender, RoutedEventArgs e)
        {
            var user = (sender as Button).Tag as DirectUser;

            var DirectUsersButtons = stack_users.Children;

            foreach (var DirectUserButton in DirectUsersButtons)
            {
                var btn = DirectUserButton as Button;
                var dr = btn.Tag as DirectUser;
                if(dr.Name == user.Name)
                    stack_users.Children.Remove(DirectUserButton);


            }

            DataStore.RemoveDirectUser(user, navigationBase.mainPage.LoggedUser.Name);
        }

        private void btn_removeClearChat_Click(object sender, RoutedEventArgs e)
        {

        }

    }
}
