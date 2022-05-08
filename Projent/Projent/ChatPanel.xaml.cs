using Microsoft.Toolkit.Uwp.Notifications;
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
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Text;
using Windows.UI.ViewManagement.Core;
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
        private string SelectedReceiver = "";
        public DispatcherTimer ChatTimer = new DispatcherTimer();
        public DateTime latestMessageTime = DateTime.MinValue;
        public bool isInCooldown = false;
        public object LastMessage = new SendMessageControl();

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
                                MainPage.LoggedUser.Name, (LastMessage as ReceiveMessageControl).Time);
                            }
                            else
                            {
                                messagesOList = await Server.ProjectServer.projectServiceClient.FindDirectMessagesForAsync(
                                MainPage.LoggedUser.Name, (LastMessage as SendMessageControl).Time);
                            }
                            //Debug.WriteLine(LastMessage.ToString());

                            messages = Converter.GetLocalMessageList(messagesOList.ToList());
                        }
                        else
                        {
                            var messagesOList = await Server.ProjectServer.projectServiceClient.FindDirectMessagesForAsync(
                                MainPage.LoggedUser.Name,
                                DateTime.MinValue);
                            messages = Converter.GetLocalMessageList(messagesOList.ToList());

                        }
                        if (messages != null && latestMessageTime != null)
                        {
                            var latestMessage = messages.OrderByDescending(m => m.Time).FirstOrDefault();
                            if (latestMessage != null)
                                latestMessageTime = latestMessage.Time;
                            UpdateMessagesList(
                                messages.Where(
                                    message =>
                                    (message.sender == MainPage.LoggedUser.Name && message.receiver == SelectedReceiver) ||
                                    (message.sender == SelectedReceiver && message.receiver == MainPage.LoggedUser.Name)).ToList());
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
                try
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
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }

        }

        public async Task<bool> IsFilePresent(string fileName)
        {
            StorageFolder storageFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("ProfilePics", CreationCollisionOption.OpenIfExists);
            var item = await storageFolder.TryGetItemAsync(fileName);
            return item != null;
        }

        public async Task<bool> IsFilePresent(string fileName, string foldername)
        {
            StorageFolder storageFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync(foldername, CreationCollisionOption.OpenIfExists);
            var item = await storageFolder.TryGetItemAsync(fileName);
            return item != null;
        }

        private async void UpdateMessagesList(List<Message> messages)
        {
            if (messages.Count > 10)
            {
                grid_chatLoading.Visibility = Visibility.Visible;
            }
            isInCooldown = true;
            foreach (var message in messages)
            {
                if (!message.isSticker)
                {
                    if (message.sender == MainPage.LoggedUser.Name)
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
            isInCooldown = false;
            grid_chatLoading.Visibility = Visibility.Collapsed;
        }

        private async void SendMessage()
        {
            if (!string.IsNullOrWhiteSpace(tb_message.Text) && !isInCooldown)
            {
                isInCooldown = true;
                Message message = new Message();
                message.sender = MainPage.LoggedUser.Name;
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
                                LastMessage = new SendMessageControl()
                                {
                                    sender = message.sender,
                                    MessageContent = message.MessageContent,
                                    ProfileImage = navigationBase.profileImageSource,
                                    Time = message.Time
                                };
                                tb_message.Text = "";
                                isInCooldown = false;
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
            grid_chatLoading.Visibility = Visibility.Visible;
            FetchMessages();
            grid_chatLoading.Visibility = Visibility.Collapsed;

            stack_users.Children.Clear();  // Clear all Test Users

            LoadDirectUsers();
            navigationBase.loadedChatPanel = this;

            ChatTimer.Interval = new TimeSpan(0, 0, 1);
            ChatTimer.Tick += ChatTimer_Tick;
            ChatTimer.Start();
        }

        private async void ChatTimer_Tick(object sender, object e)
        {
            try
            {
                // Check wether the logged user is null. 
                // this can happen if the user logged out and the time hit an delay of processing
                if (MainPage.LoggedUser != null)
                {
                    // get the new messages from the server
                    var isNewMessagesAvailable = await Server.ProjectServer.projectServiceClient.CheckNewMessagesForAsync(MainPage.LoggedUser.Name, latestMessageTime);

                    // if it is there any new messages
                    if (isNewMessagesAvailable)
                    {
                        // fetch messages from the server
                        FetchMessages();
                        // load the direct user panel. 
                        // Direct users cannot be null if the chat pannel is not expanded after the launch
                        // thats why we're loading it here
                        LoadDirectUsers();
                        /*Temp*/


                        // get the new messages list from the latest message time
                        var newMessages = await Server.ProjectServer.projectServiceClient.FindDirectMessagesForAsync(MainPage.LoggedUser.Name, latestMessageTime);


                        // Stop the timer to avoid duplicate messages
                        ChatTimer.Stop();

                        foreach (var message in newMessages)
                        {
                            if (message != null)
                            {
                                // if the receiver is the user
                                if (message.receiver == MainPage.LoggedUser.Name)
                                {
                                    // the cash folder
                                    StorageFolder storageFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("Cache", CreationCollisionOption.OpenIfExists);
                                    // var to store the image location
                                    Uri imageURI; 

                                    // if the profile image is available
                                    if (await IsFilePresent(message.sender + ".png", "Cache"))
                                    {
                                        Debug.WriteLine("Has Sender Image");
                                        // get the file 
                                        StorageFile imageFile = await storageFolder.GetFileAsync(message.sender + ".png");

                                        // get its URI and set it
                                        imageURI = new Uri(imageFile.Path);
                                    }
                                    else
                                    {
                                        // if the user image is not available locally,
                                        Debug.WriteLine("Requesting Sender Image");
                                        // Request it from the server
                                        // here we are saving it for more faster access in the future. 
                                        var image = await Server.MainServer.mainServiceClient.RequestUserImageAsync(message.sender);

                                        // if the image is null , show the error as this
                                        Debug.WriteLine(image == null ? ":::Error:::" : ":::HasBuffer:::");

                                        // Get the location to the cache folder
                                        var ProfilePicFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("Cache", CreationCollisionOption.OpenIfExists);
                                        // create the file to save the profile pic
                                        var ProfilePicFile = await ProfilePicFolder.CreateFileAsync(message.sender + ".png", CreationCollisionOption.ReplaceExisting);

                                        // save the pic
                                        await FileIO.WriteBytesAsync(ProfilePicFile, image);

                                        // save the image path
                                        imageURI = new Uri(ProfilePicFile.Path);

                                    }

                                    // This list is to store the Loaded direct users
                                    List<DirectUser> AddedDirectUsersList = new List<DirectUser>();
                                    // Retrive the user controls from the users stack ( Direct Message Users )
                                    var AddedDRUControls = stack_users.Children;

                                    // Apply them in to the view
                                    // This loads the Direct Users in to the view
                                    foreach (var AddedDRUControl in AddedDRUControls)
                                    {
                                        var DRUControl = AddedDRUControl as Button;
                                        var DRU = DRUControl.Tag as DirectUser;

                                        AddedDirectUsersList.Add(DRU);
                                    }

                                    // if the new message sender is not in the added list,
                                    // autometically add the message to tehv view.
                                    if (AddedDirectUsersList.Where(u => u.Name == message.sender).Count() == 0)
                                    {
                                        var newDRU = new DirectUser { Name = message.sender, Email = await DataStore.GetEmail(message.sender, MainPage.LoggedUser.Name) };
                                        DataStore.NewDirectUser(newDRU, MainPage.LoggedUser.Name);
                                        LoadDirectUsers();

                                    }

                                    // show the user a notification about the message
                                    new ToastContentBuilder()
                                            .AddAppLogoOverride(imageURI)
                                            .AddArgument("action", "viewConversation")
                                            .AddArgument("conversationId", 9813)
                                            .AddText(message.sender)
                                            .AddText(message.MessageContent)
                                            .Show();
                                }
                            }
                        }

                        // continue the timer
                        ChatTimer.Start();
                    }
                }

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }


        private async void LoadDirectUsers()
        {
            // get the function in to a cooldown state
            // this prevents duplicate additions
            isInCooldown = true;
            
            // Clear all the user from the loaded list
            stack_users.Children.Clear();

            // Fetch Saved direct users from the db
            var DirectUserList = DataStore.FetchDirectUsers(MainPage.LoggedUser.Name);

            // send them back for navigation base for the further purposes
            navigationBase.directUsers = DirectUserList;

            // if the list is not null and
            // it has users
            if (DirectUserList != null && DirectUserList.Count > 0)
            {
                // for all the users in the direct users
                foreach (var user in DirectUserList)
                {
                    // if it is not the same user as the logged user
                    if (user.Name != MainPage.LoggedUser.Name)
                    {
                        // create a new control to show it on the UI
                        Button directUserButton = new Button();
                        directUserButton.Style = Resources["UserButton"] as Style;
                        directUserButton.Tag = user;


                        // this will hold the image of the user
                        var DirectUserImage = new Image();

                        // check wether the users photo is in the cache folder.  (NOT)
                        if (!await IsFilePresent(user.Name + ".png"))
                        {
                            // Request it from the server
                            var imageBuffer = await Server.MainServer.mainServiceClient.RequestUserImageAsync(user.Name);

                            // Save it in the local cache folder
                            var ProfilePicFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("Cache", CreationCollisionOption.OpenIfExists);
                            var ProfilePicFile = await ProfilePicFolder.CreateFileAsync(user.Name + ".png", CreationCollisionOption.OpenIfExists);

                            await FileIO.WriteBytesAsync(ProfilePicFile, imageBuffer);

                            // set it to the UI element
                            using (var fileStream = await ProfilePicFile.OpenAsync(FileAccessMode.Read))
                            {
                                BitmapImage bitmapImage = new BitmapImage();  // Creates a new bitmap file 
                                await bitmapImage.SetSourceAsync(fileStream);  // Sets the loded file as the new bitmap source
                                DirectUserImage.Source = bitmapImage;  // sets the created bitmap as an image source
                            }

                        }
                        else
                        {
                            // if it is already exists, 
                            // directly load it from the file
                            var ProfilePicFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("Cache", CreationCollisionOption.OpenIfExists);
                            var ProfilePicFile = await ProfilePicFolder.GetFileAsync(user.Name + ".png");

                            // Load it to the Ui Element
                            using (var fileStream = await ProfilePicFile.OpenAsync(FileAccessMode.Read))
                            {
                                BitmapImage bitmapImage = new BitmapImage();  // Creates a new bitmap file 
                                await bitmapImage.SetSourceAsync(fileStream);  // Sets the loded file as the new bitmap source
                                DirectUserImage.Source = bitmapImage;  // sets the created bitmap as an image source
                            }

                            // directly loading it using the local file saves minimum of 10 Seconds of loading time
                        }

                        // Apply the other contect to the UI elemnt
                        directUserButton.Content = DirectUserImage;
                        directUserButton.RightTapped += DirectUserButton_RightTapped;
                        directUserButton.Tapped += DirectUserButton_Tapped;
                        // attach the flyout 
                        // this flyout shows the user info
                        FlyoutBase.SetAttachedFlyout(directUserButton, Resources["UserInfo"] as Flyout);

                        // if the user control is not in the Direct Users panel, add it as a new Item
                        if(!stack_users.Children.Contains(directUserButton))
                            stack_users.Children.Add(directUserButton);
                    }
                }
            }

            // turn off the cool down period
            isInCooldown = false;
        }

        private void DirectUserButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            try
            {
                // This will load all the messages that is related to the direct user
                grid_chatLoading.Visibility = Visibility.Visible;
                isInCooldown = true;
                list_messages.Items.Clear();
                // set the selected user name for feathing messages
                SelectedReceiver = ((sender as Button).Tag as DirectUser).Name;

                // fetch messages
                FetchMessages();
                isInCooldown = false;
                // this will hide the loading screen ans show the chat messages list
                ShowMessagePannel(true);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
        }

        private void DirectUserButton_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            // in the right tapped even,
            // this will show the users info flyout which has be attached before. 
            var user = (sender as Button).Tag as DirectUser;
            lbl_DirectUserEmail.Text = user.Email;
            lbl_DirectUsername.Text = user.Name;

            // image is directly taken from the Direct User UI element
            // to save loading time
            img_directUserImage.Source = ((sender as Button).Content as Image).Source;

            btn_removeDirectuser.Tag = user;
            btn_removeClearChat.Tag = user;

            // thisline will show the flyout
            FlyoutBase.ShowAttachedFlyout(sender as Button);
        }

        // This will hold all the loaded Direct user temporary for faster loading
        List<PMServer1.User> AllDirectUsers = new List<PMServer1.User>();
        private async void NewDirectUser_Click(object sender, RoutedEventArgs e)
        {
            // Clear all the loaded direct users in the flyout
            list_directUsers.Items.Clear();

            // fetch the users from the server
            var users = await Server.MainServer.mainServiceClient.FetchUsersAsync();

            // Save all of them in to a list for easy access
            AllDirectUsers = users.ToList();

            // this will store the lirect users temporary for to 
            // user to view and select
            List<DirectUser> directUsers = new List<DirectUser>();

            // if the retrived user list it not 0 ot null
            // this privents the null exception
            if (users != null && users.Count > 0)
            {
                foreach (var user in users)
                {
                    // show the user on the list only if it is not the same as the 
                    // currently logged user/ 
                    if (user.Name != MainPage.LoggedUser.Name)
                    {
                        // this will hold the direct users 
                        List<DirectUser> AddedDirectUsersList = new List<DirectUser>();

                        // retrived currently loaded direct users from the stack
                        var AddedDRUControls = stack_users.Children;
                        
                        // add them in to the list for filtering purposes
                        foreach (var AddedDRUControl in AddedDRUControls)
                        {
                            var DRUControl = AddedDRUControl as Button;
                            var DRU = DRUControl.Tag as DirectUser;

                            AddedDirectUsersList.Add(DRU);
                        }

                        // show all the users expect those currently added to the stack
                        if (AddedDirectUsersList.Where(u => u.Name == user.Name).Count() == 0)
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
            }

        }

        public void SetUserStaus(DirectUser directUser, DataStore.Status status)
        {
            var DirectUsersButtons = stack_users.Children;

            foreach (var DirectUserButton in DirectUsersButtons)
            {
                var btn = DirectUserButton as Button;
                var dr = btn.Tag as DirectUser;
                if (dr != null)
                    if (dr.Email == directUser.Email)
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
                DataStore.NewDirectUser(user, MainPage.LoggedUser.Name);
            }
            LoadDirectUsers();
            fly_NewDRU.Hide();
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

        private void ShowMessagePannel(bool visibility)
        {
            if (visibility)
            {
                grid_MessagePanel.Visibility = Visibility.Visible;
                grid_selectUser.Visibility = Visibility.Collapsed;
            }
            else
            {
                grid_MessagePanel.Visibility = Visibility.Collapsed;
                grid_selectUser.Visibility = Visibility.Visible;
            }
        }

        private void btn_removeDirectuser_Click(object sender, RoutedEventArgs e)
        {

            var user = (sender as Button).Tag as DirectUser;

            if (SelectedReceiver == user.Name)
                ShowMessagePannel(false);

            var DirectUsersButtons = stack_users.Children;

            foreach (var DirectUserButton in DirectUsersButtons)
            {
                var btn = DirectUserButton as Button;
                var dr = btn.Tag as DirectUser;
                if (dr.Name == user.Name)
                    stack_users.Children.Remove(DirectUserButton);
            }

            DataStore.RemoveDirectUser(user, MainPage.LoggedUser.Name);
        }

        private void btn_removeClearChat_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var btn = sender as Button;
                var directUser = (sender as Button).Tag as DirectUser;
                Server.ProjectServer.projectServiceClient.DeleteMessagesFromAsync(MainPage.LoggedUser.Name, directUser.Name);
                FetchMessages();
                list_messages.Items.Clear();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private void tb_search_TextChanging(TextBox sender, TextBoxTextChangingEventArgs args)
        {
            List<DirectUser> directUsers = new List<DirectUser>();
            list_directUsers.Items.Clear();
            if (AllDirectUsers != null && AllDirectUsers.Count > 0)
            {
                foreach (var user in AllDirectUsers)
                {
                    if (user.Name != MainPage.LoggedUser.Name)
                    {
                        List<DirectUser> AddedDirectUsersList = new List<DirectUser>();
                        var AddedDRUControls = stack_users.Children;
                        foreach (var AddedDRUControl in AddedDRUControls)
                        {
                            var DRUControl = AddedDRUControl as Button;
                            var DRU = DRUControl.Tag as DirectUser;

                            AddedDirectUsersList.Add(DRU);
                        }

                        if (AddedDirectUsersList.Where(u => u.Name == user.Name).Count() == 0)
                        {
                            if (user.Name.ToLower().Trim().Contains(tb_search.Text.ToLower().Trim()) || user.Email.ToLower().Trim().Contains(tb_search.Text.ToLower().Trim()))
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

                }
            }

            if (list_directUsers.Items.Count == 0)
            {
                list_directUsers.Items.Add("No Users Found!");
            }
        }

        private void btn_emogi_Click(object sender, RoutedEventArgs e)
        {
            tb_message.Focus(FocusState.Keyboard);
            CoreInputView.GetForCurrentView().TryShow(CoreInputViewKind.Emoji);

        }
    }
}
