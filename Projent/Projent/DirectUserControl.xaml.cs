using Projent.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.Streams;
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
    public sealed partial class DirectUserControl : UserControl
    {
        public DirectUser _directUser = null;
        public DirectUser directUser
        {
            get { return _directUser; }
            set
            {
                if (value != null)
                {
                    _directUser = value;
                    lbl_email.Text = value.Email;
                    lbl_username.Text = value.Name;

                    LoadImageAsync();

                }
            }
        }

        private async void LoadImageAsync()
        {
            using (InMemoryRandomAccessStream ms = new InMemoryRandomAccessStream())
            {
                // Writes the image byte array in an InMemoryRandomAccessStream
                // that is needed to set the source of BitmapImage.
                using (DataWriter writer = new DataWriter(ms.GetOutputStreamAt(0)))
                {
                    var imageBuffer = await Server.MainServer.mainServiceClient.RequestUserImageAsync(_directUser.Name);
                    writer.WriteBytes(imageBuffer);
                    await writer.StoreAsync();
                    await writer.FlushAsync();
                }

                var image = new BitmapImage();
                await image.SetSourceAsync(ms);
                img_profilePic.Source = image;
                ms.Dispose();
            }
        }

        public DirectUserControl()
        {
            this.InitializeComponent();
        }

        public DirectUserControl(DirectUser directUser)
        {
            this.InitializeComponent();
            this.directUser = directUser;
        }
    }
}
