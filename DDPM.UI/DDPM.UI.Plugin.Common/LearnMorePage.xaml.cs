using System.Windows;
using System.Windows.Input;

namespace DDPM.UI.Plugin.Common
{
    /// <summary>
    /// LearnMorePage.xaml 的互動邏輯
    /// </summary>
    public partial class LearnMorePage : Window
    {
        private readonly string Caption = "";
        private readonly string DownloadPage = "Download Dell Audio";

        public string Parameter { get; private set; } = "";

        public LearnMorePage(double width, double height, string parameter = "")
        {
            InitializeComponent();
            this.Width = width;
            this.Height = height;
            Caption = DownloadPage;
            txtTitleBar.Text = Caption;
            txtCaption.Text = Caption;
        }

        private void CancelClick(object sender, MouseButtonEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}