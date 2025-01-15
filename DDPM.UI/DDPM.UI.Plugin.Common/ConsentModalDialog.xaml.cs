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
    /// <summary>
    /// ConsentModalDialog.xaml 的互動邏輯
    /// </summary>
    [Obsolete("This method is obsolete. Moved to WalkThroughPage.xaml before app walk through.")]
    public partial class ConsentModalDialog : Window
    {
        private string PrivacyUrl = "https://www.dell.com/learn/us/en/uscorp1/policies-privacy-country-specific-privacy-policy";

        public ConsentModalDialog(double width, double height)
        {
            InitializeComponent();
            this.Width = width;
            this.Height = height;
            txtYes.Content = LangHelper.Instance["ConsentYes"];
            txtNo.Content = LangHelper.Instance["ConsentNo"];
            txtCaption.Text = LangHelper.Instance["Consent.1"];
            txtCaption2.Text = LangHelper.Instance["AppName"];
            txt1.Text = LangHelper.Instance["Consent.2"];
            txt2.Text = LangHelper.Instance["Analytics.2"];
        }

        private void No_MouseLeftButtonDown(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void Yes_MouseLeftButtonDown(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void OpenPrivacy(object sender, MouseButtonEventArgs e)
        {
            DDPM.SA.Common.Settings.DDPMFileSecurity.StartProcessSafely(
                null,
                new ProcessStartInfo
                {
                    FileName = PrivacyUrl,
                    UseShellExecute = true
                });
        }
    }
}
