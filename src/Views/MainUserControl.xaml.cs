using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FileFinder.View
{
    /// <summary>
    /// Interaction logic for MainUserControl.xaml
    /// </summary>
    public partial class MainUserControl : UserControl
    {
        public MainUserControl()
        {
            InitializeComponent();
        }

        private void CopyClick(object sender, RoutedEventArgs e)
        {
            if (ResultListBox.SelectedItem == null)
            {
                return;
            }

            Clipboard.SetText(ResultListBox.SelectedItem.ToString());
            MessageTb.Text = "Copied";
        }

        private void OpenInFolderClick(object sender, RoutedEventArgs e)
        {
            if (ResultListBox.SelectedItem == null)
            {
                return;
            }

            var v = ResultListBox.SelectedItem.ToString();
            string argument = "/select, \"" + v + "\"";
            Process.Start("explorer.exe", argument);
            MessageTb.Text = "Opened the file location";
        }
    }
}
