using DDPM.UI.Common;
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
using System.Windows.Shapes;
using static System.Formats.Asn1.AsnWriter;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

namespace DDPM.UI.Plugin.Common
{
    /// <summary>
    /// LearnMorePage.xaml 的互動邏輯
    /// </summary>
    public partial class LearnMorePage : Window
    {
        readonly string Caption = "";
        readonly string DownloadPage = "Download Dell Audio";

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
