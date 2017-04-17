using System;
using System.Collections.Generic;
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
using files;
using SharepointOnlineFileTransfer.CustomControls;

namespace SharepointOnlineFileTransfer
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.txtSourcePath.GotFocus += TxtSourcePath_GotFocus;
        }

        private void TxtSourcePath_GotFocus(object sender, RoutedEventArgs e)
        {
            string path = FilePicker.GetSourcePath();
            if (!string.IsNullOrEmpty(path))
            {
                txtSourcePath.Text = path;
            }
        }

        private void btnProcess_Click(object sender, RoutedEventArgs e)
        {
            //todo: do all process starts here

        }
    }
}
