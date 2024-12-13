using DDPM.UI.Common;
using Dell.Client.Framework.UX.WPF.Controls;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace DDPM.UI.Plugin.Common
{
    /// <summary>
    /// LearnMorePage.xaml 的互動邏輯
    /// </summary>
    public partial class LearnMorePage : Window
    {
        private readonly string Caption = "";
        private readonly string DownloadPage = "Download Dell Audio";
        private readonly string Model = "";
        private readonly string FirmwareVersion2 = "";

        public string Parameter { get; private set; } = "";

        public LearnMorePage(double width, double height, string parameter, string model, string fwv)
        {
            InitializeComponent();
            this.Width = width;
            this.Height = height;
            Caption = parameter;
            txtTitleBar.Text = Caption;
            txtCaption.Text = Caption;
            TxBlockModel.Text = model;
            TxBlockToolTip.Text = fwv;

            DdpmCommonHelper.BitmapImageUpdated += ImageUpdate;
        }

        private void ImageUpdate(OSThemeEnum oSThemeEnum)
        {
            ArrowLeft.Source = null;
            ArrowLeft.Source = (BitmapImage)Application.Current.Resources["Arrow_Left"];
        }

        private void CancelClick(object sender, MouseButtonEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void AppleQR_Click(object sender, MouseButtonEventArgs e)
        {
            Process.Start(new ProcessStartInfo("https://apps.apple.com/us/app/dell-audio/id6472411862") { UseShellExecute = true });
        }

        private void AndroidQR_Click(object sender, MouseButtonEventArgs e)
        {
            Process.Start(new ProcessStartInfo("https://play.google.com/store/apps/details?id=com.dell.dellaudio&pli=1") { UseShellExecute = true });
        }
    }
}