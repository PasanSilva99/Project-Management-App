using Projent.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace Projent
{
    public sealed partial class ProjectListViewItemControl : UserControl
    {
        private string _projectName;
        private string _projectDescription;
        private DateTime _projectDate;
        private string _manager;
        private List<string> _asignees;

        public string ProjectName
        {
            get { return _projectName; }
            set
            {
                _projectName = value;
                lbl_name.Text = value;
                ToolTip nameTT = new ToolTip();
                nameTT.Content = lbl_name.Text;
                ToolTipService.SetToolTip(lbl_name, nameTT);
            }
        }

        public string ProjectDescription
        {
            get { return _projectDescription; }
            set
            {
                _projectDescription = value;
                lbl_description.Text = value;
                ToolTip descriptionTT = new ToolTip();
                descriptionTT.Content = lbl_description.Text;
                ToolTipService.SetToolTip(lbl_description, descriptionTT);
            }
        }

        public DateTime ProjectDate
        {
            get { return _projectDate; }
            set
            {
                _projectDate = value;
                lbl_date.Text = DateTime.Now.ToString("d");
            }
        }

        public string Manager
        {
            get { return _manager; }
            set
            {
                _manager = value;
                ShowManagerProfile();
            }
        }

        private async void ShowManagerProfile()
        {
            // Manager Profile Pic Generation
            if (_manager != null)
            {

                Border imageBorder = new Border();
                imageBorder.Width = 37.0;
                imageBorder.Height = 37.0;
                imageBorder.CornerRadius = new CornerRadius(37.0);
                imageBorder.Tag = _manager;

                Image imageControl = new Image();

                StorageFolder ProfilePicFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("Cache", CreationCollisionOption.OpenIfExists);
                if (await IsFilePresent(_manager + ".png", "Cache"))
                {
                    Debug.WriteLine("Has Sender Image");
                    StorageFile imageFile = await ProfilePicFolder.GetFileAsync(_manager + ".png");
                    using (var fileStream = await imageFile.OpenAsync(FileAccessMode.Read))
                    {
                        BitmapImage bitmapImage = new BitmapImage();  // Creates a new bitmap file 
                        await bitmapImage.SetSourceAsync(fileStream);  // Sets the loded file as the new bitmap source
                        imageControl.Source = bitmapImage;  // sets the created bitmap as an image source
                    }
                }
                else
                {
                    Debug.WriteLine("Requesting Sender Image");
                    var image = await Server.MainServer.mainServiceClient.RequestUserImageAsync(_manager);
                    Debug.WriteLine(image == null ? ":::Error:::" : ":::HasBuffer:::");

                    var ProfilePicFile = await ProfilePicFolder.CreateFileAsync(_manager + ".png", CreationCollisionOption.ReplaceExisting);

                    await FileIO.WriteBytesAsync(ProfilePicFile, image);

                    using (var fileStream = await ProfilePicFile.OpenAsync(FileAccessMode.Read))
                    {
                        BitmapImage bitmapImage = new BitmapImage();  // Creates a new bitmap file 
                        await bitmapImage.SetSourceAsync(fileStream);  // Sets the loded file as the new bitmap source
                        imageControl.Source = bitmapImage;  // sets the created bitmap as an image source
                    }

                }

                imageBorder.Child = imageControl;
                FlyoutBase.SetAttachedFlyout(imageBorder, Resources["UserInfo"] as Flyout);

                imageBorder.Tapped += ImageBorder_Tapped;

                stack_manager.Children.Add(imageBorder);

            }

        }

        public async Task<bool> IsFilePresent(string fileName, string foldername)
        {
            StorageFolder storageFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync(foldername, CreationCollisionOption.OpenIfExists);
            var item = await storageFolder.TryGetItemAsync(fileName);
            return item != null;
        }

        public List<string> Asignees
        {
            get { return _asignees; }
            set
            {
                _asignees = value;
                ShowAssigneesProfiles();
            }
        }

        Thickness DefaultMargin = new Thickness(0, 0, 0, 0);

        private async void ShowAssigneesProfiles()
        {
            stack_assignees.Children.Clear();
            bool showMore = false;

            // Assignees profile Pic Generation
            var assigneesToShow = new List<string>();
            if (_asignees != null && _asignees.Count < 5)
            {
                assigneesToShow = _asignees;
                showMore = false;
            }
            else
            {
                assigneesToShow = _asignees.GetRange(0, 4);
                showMore = true;

            }

            // Create the vavigation button control generation code
            foreach (var asignee in assigneesToShow)
            {

                if (asignee != null)
                {

                    Border imageBorder = new Border();
                    imageBorder.CornerRadius = new CornerRadius(37.0);
                    imageBorder.Tag = asignee;

                    Image imageControl = new Image();

                    StorageFolder ProfilePicFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("Cache", CreationCollisionOption.OpenIfExists);
                    if (await IsFilePresent(asignee + ".png", "Cache"))
                    {
                        Debug.WriteLine("Has Sender Image");
                        StorageFile imageFile = await ProfilePicFolder.GetFileAsync(asignee + ".png");
                        using (var fileStream = await imageFile.OpenAsync(FileAccessMode.Read))
                        {
                            BitmapImage bitmapImage = new BitmapImage();  // Creates a new bitmap file 
                            await bitmapImage.SetSourceAsync(fileStream);  // Sets the loded file as the new bitmap source
                            imageControl.Source = bitmapImage;  // sets the created bitmap as an image source
                        }
                    }
                    else
                    {
                        Debug.WriteLine("Requesting Sender Image");
                        var image = await Server.MainServer.mainServiceClient.RequestUserImageAsync(asignee);
                        Debug.WriteLine(image == null ? ":::Error:::" : ":::HasBuffer:::");

                        var ProfilePicFile = await ProfilePicFolder.CreateFileAsync(asignee + ".png", CreationCollisionOption.ReplaceExisting);

                        await FileIO.WriteBytesAsync(ProfilePicFile, image);

                        using (var fileStream = await ProfilePicFile.OpenAsync(FileAccessMode.Read))
                        {
                            BitmapImage bitmapImage = new BitmapImage();  // Creates a new bitmap file 
                            await bitmapImage.SetSourceAsync(fileStream);  // Sets the loded file as the new bitmap source
                            imageControl.Source = bitmapImage;  // sets the created bitmap as an image source
                        }

                    }

                    if (_asignees.Count > 2)
                    {
                        imageBorder.Margin = new Thickness(-9, 0, -9, 0);
                        DefaultMargin = new Thickness(-9, 0, -9, 0);
                    }
                    else
                    {
                        DefaultMargin = new Thickness(0, 0, 0, 0);
                    }

                    imageBorder.Child = imageControl;

                    imageBorder.Width = 37;
                    imageBorder.Height = 37;

                    imageBorder.Scale = new System.Numerics.Vector3(1, 1, 1);

                    imageBorder.ScaleTransition = new Vector3Transition();



                    imageBorder.PointerEntered += ImageBorder_PointerEntered;
                    imageBorder.PointerExited += ImageBorder_PointerExited;
                    imageBorder.Tapped += ImageBorder_Tapped;

                    imageBorder.VerticalAlignment = VerticalAlignment.Center;

                    FlyoutBase.SetAttachedFlyout(imageBorder, Resources["UserInfo"] as Flyout);

                    stack_assignees.Children.Add(imageBorder);
                    

                }
            }

            Button morebtn = new Button();
            morebtn.BorderThickness = new Thickness(0, 0, 0, 0);

            morebtn.Background = new SolidColorBrush(Windows.UI.Color.FromArgb((byte)255, (byte)43, (byte)93, (byte)203));
            morebtn.Foreground = new SolidColorBrush(Windows.UI.Colors.White);
            morebtn.CornerRadius = new CornerRadius(37.0);
            morebtn.Width = 37.0;
            morebtn.Height = 37.0;
            morebtn.Margin = DefaultMargin;
            morebtn.Scale = new System.Numerics.Vector3(1, 1, 1);
            morebtn.ScaleTransition = new Vector3Transition();
            morebtn.PointerEntered += Morebtn_PointerEntered;
            morebtn.PointerExited += Morebtn_PointerExited;
            morebtn.Click += Morebtn_Click;
            morebtn.VerticalAlignment = VerticalAlignment.Center;

            morebtn.Content = (_asignees.Count - assigneesToShow.Count).ToString();

            FlyoutBase.SetAttachedFlyout(morebtn, Resources["AllAsignees"] as Flyout);

            if (showMore)
                stack_assignees.Children.Add(morebtn);
        }

        private async void Morebtn_Click(object sender, RoutedEventArgs e)
        {
            list_directUsers.Items.Clear();
            FlyoutBase.ShowAttachedFlyout(sender as Button);

            foreach(var assignee in _asignees)
            {
                string email = "Loading...";
                try
                {
                    email = await DataStore.GetEmail(assignee, MainPage.LoggedUser.Name);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                    lbl_DirectUserEmail.Text = "Something Went Wrong";
                }

                var directUserControl = new DirectUserControl() { directUser = new DirectUser() { Name = assignee, Email = email } };
                var DirectUserListItem = new ListViewItem();
                DirectUserListItem.Style = Resources["DirectUserItem"] as Style;
                DirectUserListItem.Content = directUserControl;
                DirectUserListItem.Tag = new DirectUser() { Name = assignee, Email = email };

                list_directUsers.Items.Add(DirectUserListItem);
            }

        }

        private void Morebtn_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            var btn = sender as Button;
            btn.Margin = DefaultMargin;
        }

        private void Morebtn_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            var btn = sender as Button;
            btn.Margin  = new Thickness(5, 0, 5, 0);
        }

        private async void ImageBorder_Tapped(object sender, TappedRoutedEventArgs e)
        {

            FlyoutBase.ShowAttachedFlyout(sender as Border);
            var imageBorder = sender as Border;
            var image = imageBorder.Child as Image;

            lbl_DirectUserEmail.Text = "Loading ...";

            img_directUserImage.Source = image.Source;
            lbl_DirectUsername.Text = imageBorder.Tag as string;
            try
            {
                lbl_DirectUserEmail.Text = await DataStore.GetEmail(imageBorder.Tag as string, MainPage.LoggedUser.Name);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                lbl_DirectUserEmail.Text = "Something Went Wrong";
            }

        }

        private void ImageBorder_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            var border = sender as Border;

            border.Margin = DefaultMargin;
        }

        private void ImageBorder_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            var border = sender as Border;
            border.Margin = new Thickness(5, 0, 5, 0);
        }

        public ProjectListViewItemControl()
        {
            this.InitializeComponent();
        }

        private void Grid_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            var grid = sender as Grid;
            grid.Background = new SolidColorBrush(Windows.UI.Color.FromArgb((byte)255, (byte)241, (byte)222, (byte)252));
        }

        private void Grid_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            var grid = sender as Grid;
            grid.Background = new SolidColorBrush(Windows.UI.Colors.White);
        }
    }
}
