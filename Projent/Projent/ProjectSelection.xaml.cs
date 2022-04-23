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

namespace Projent
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ProjectSelection : Page
    {
        public ProjectSelection()
        {
            this.InitializeComponent();

            ProjectListViewItemControl projectListViewItemControl = new ProjectListViewItemControl();
            projectListViewItemControl.ProjectName = "Test Project";
            projectListViewItemControl.ProjectDescription = "Test Descrption";
            projectListViewItemControl.ProjectDate = DateTime.Now;
            projectListViewItemControl.Manager = "Amoeher";
            projectListViewItemControl.Asignees = new List<string> { "SandaruDev", "LilyKi"};

            ListViewItem listViewItem = new ListViewItem();
            listViewItem.Style = Resources["ProjectListItem"] as Style;
            listViewItem.Content = projectListViewItemControl;
            listViewItem.CornerRadius = new CornerRadius(10.0, 10.0, 10.0, 10.0);

            list_projects.Items.Add(listViewItem);
        }
    }
}
