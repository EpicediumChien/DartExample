using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;

//using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;
using DDPM.UI.Common;
using Dell.Client.Framework.UX.WPF.Controls;
using static System.Collections.Specialized.BitVector32;
using static System.Net.Mime.MediaTypeNames;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

namespace DDPM.UI.Plugin.Common {
  /// <summary>
  /// OpenRunModalDialog.xaml 的互動邏輯
  /// </summary>
  public partial class PairTileModalDialog : Window {

    public PairTileModalDialog(double width, double height) {
      InitializeComponent();
      this.Width = width;
      this.Height = height;
      txtTitleBar.Text = Strings.PairWithTile;
      txtPairTile1.Text = Strings.PairTile1;
      txtPairTile2.Text = Strings.PairTile2;
      txtPairTile3.Text = Strings.PairTile3;
      txtDownloadTile.Text = Strings.DownloadTile;
    }

    private void BacklClick(object sender, MouseButtonEventArgs e) {
      DialogResult = false;
      Close();
    }
  }
}
