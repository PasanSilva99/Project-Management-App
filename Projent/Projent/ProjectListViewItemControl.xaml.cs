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
            set { _projectName = value; }
        }
        public string ProjectDescription
        {
            get { return _projectDescription; }
            set { _projectDescription = value; }
        }
        public DateTime ProjectDate
        {
            get { return _projectDate; }
            set { _projectDate = value; }
        }
        public string Manager
        {
            get { return _manager; }
            set { _manager = value; }
        }
        public List<string> Asignees
        {
            get { return _asignees; }
            set { _asignees = value; }
        }
        public ProjectListViewItemControl()
        {
            this.InitializeComponent();
        }
    }
}
