using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using DDPM.UI.Resources.Helper;

namespace DDPM.UI.Plugin.Common
{
    public partial class ImportModalDialog : Window
    {
        public bool isChecked { get; set; } = false;
        public ImportModalDialog(string model, double width, double height)
        {
            InitializeComponent();
            this.Width = width;
            this.Height = height;
            txtYes.Content = LangHelper.Instance["Yes"];
            txtNo.Content = LangHelper.Instance["No"];
            string title = LangHelper.Instance["ImpExp_Title.0"].Replace("%1", model);
            txtCaption.Text = title;
            txtMessage.Text = LangHelper.Instance["ImpExp_Message.1"];
            //chkIgnore.Content = LangHelper.Instance["ImpExp_CheckBox.0"];
        }
        /*
        private void No_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DialogResult = false;
            isChecked = chkIgnore.IsChecked == null? false : (bool)chkIgnore.IsChecked;
            Close();
        }

        private void Yes_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DialogResult = true;
            isChecked = chkIgnore.IsChecked == null ? false : (bool)chkIgnore.IsChecked;
            Close();
        }*/

        private void No_MouseLeftButtonDown(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            isChecked = chkIgnore.IsChecked == null ? false : (bool)chkIgnore.IsChecked;
            Close();
        }

        private void Yes_MouseLeftButtonDown(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            isChecked = chkIgnore.IsChecked == null ? false : (bool)chkIgnore.IsChecked;
            Close();
        }
    }
}
