using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DDPM.UI.Common;

namespace DDPM.UI.Plugin.Common
{
    /// <summary>
    /// OpenRunModalDialog.xaml 的互動邏輯
    /// </summary>
    public partial class PairTileModalDialog : Window
    {

        public PairTileModalDialog(double width, double height)
        {
            try
            {
                InitializeComponent();
                this.Width = width;
                this.Height = height;
                txtTitleBar.Text = Strings.PairWithTile;
                txtPairTile1.Text = Strings.PairTile1;
                txtPairTile2.Text = Strings.PairTile2;
                txtPairTile3.Text = Strings.PairTile3;
                txtDownloadTile.Text = Strings.DownloadTile;
            }
            catch (Exception ex) 
            {
                DdpmCommonHelper.WriteUILog("DDPM.UI.Plugin.Common\\PairTileModalDialog.xaml.cs PairTileModalDialog ex:" + ex.Message);
            }
        }

        private void BacklClick(object sender, MouseButtonEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void DownloadTile(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (sender is Image img)
                {
                    var url = img.Tag.ToString() == "G" ? "https://play.google.com/store/apps/details?id=com.thetileapp.tile&pli=1" : "https://apps.apple.com/us/app/tile-find-lost-keys-phone/id664939913";
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = url,
                        UseShellExecute = true
                    });
                }
            }
            catch (Exception ex) 
            {
                DdpmCommonHelper.WriteUILog("DDPM.UI.Plugin.Common\\PairTileModalDialog.xaml.cs DownloadTile ex:" + ex.Message);
            }

        }
    }
}
