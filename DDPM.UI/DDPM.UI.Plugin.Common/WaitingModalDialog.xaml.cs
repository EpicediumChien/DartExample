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
using System.Windows.Threading;
using DDPM.UI.Plugin.ViewModels;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

namespace DDPM.UI.Plugin.Common {

  /// <summary>
  /// WaitingModalDialog.xaml 的互動邏輯
  /// </summary>
  public partial class WaitingModalDialog : Window {
   public WaitingModalDialog(string caption, string message, string alert) {
      InitializeComponent();

      txtCaption.Text = caption;
      txtMessage.Text = message;
      txtAlert.Text = alert;

      var timer = new DispatcherTimer {
        Interval = TimeSpan.FromSeconds(2.5)
      };
      timer.Tick += Timer_Tick;
      timer.Start();
    }

    private void Timer_Tick(object? sender, EventArgs e) {
      this.Close();
    }
  }
}
