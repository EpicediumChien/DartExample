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

namespace DDPM.UI.Plugin.Common {
  /// <summary>
  /// RestoreModalDialog.xaml 的互動邏輯
  /// </summary>
  public partial class RestoreModalDialog : Window {
    private readonly string Caption = "Restore to default";
    private readonly string Message = "Are you sure you want to restore all default settings on your device?";
    private readonly string Yes = "Yes";
    private readonly string No = "No";

    public RestoreModalDialog() {
      InitializeComponent();

      txtCaption.Text = Caption;
      txtMessage.Text = Message;
      txtYes.Text = Yes;
      txtNo.Text = No;
    }

    private void Yes_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
      DialogResult = true;
      Close();
    }

    private void No_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
      DialogResult = false;
      Close();
    }
  }
}
