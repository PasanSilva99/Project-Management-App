using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Newtonsoft.Json;
using Windows.UI.Xaml.Media.Imaging;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace PCClient
{
    public sealed partial class ReceiveMessageControl : UserControl
    {
        private string _MessageContent = "";
        private bool _isSticker = false;
        private string _sender = "";
        private ImageSource _ProfileImage = null;
        private DateTime _Time = DateTime.Now;
        private List<String> _MentionedUsers = null;

        public string MessageContent
        {
            get
            {
                return _MessageContent;
            }
            set
            {
                _MessageContent = value;
                UpdateView();
            }
        }
        public bool isSticker
        {
            get
            {
                return _isSticker;
            }
            set
            {
                _isSticker = value;
                UpdateView();
            }
        }
        public string sender
        {
            get
            {
                return _sender;
            }
            set
            {
                _sender = value;
                UpdateView();
            }
        }
        public ImageSource ProfileImage
        {
            get
            {
                return _ProfileImage;
            }
            set
            {
                _ProfileImage = value;
                UpdateView();
            }
        }
        public DateTime Time
        {
            get
            {
                return _Time;
            }
            set
            {
                _Time = value;
                UpdateView();
            }
        }

        public List<String> MentionedUsers
        {
            get
            {
                return _MentionedUsers;
            }
            set
            {
                _MentionedUsers=value;
            }
        }

        private void UpdateView()
        {
            if (!isSticker)
            {
                message_body.Text = _MessageContent;
                img_prifilePic.Source = _ProfileImage;
                lbl_username.Text = _sender;
                lbl_time.Text = _Time.ToString("t");
            }
        }

        public ReceiveMessageControl()
        {
            this.InitializeComponent();
        }

        private void lbl_username_Tapped(object sender, TappedRoutedEventArgs e)
        {

        }

        private void lbl_time_PointerEntered(object sender, PointerRoutedEventArgs e)
        {

        }

        private void lbl_time_PointerExited(object sender, PointerRoutedEventArgs e)
        {

        }
    }
}
