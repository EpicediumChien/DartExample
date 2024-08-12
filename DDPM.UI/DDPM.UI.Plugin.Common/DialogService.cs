using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DDPM.UI.Plugin.Common;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

namespace DDPM.UI.Plugin.Common {

  public interface IDialogService {
    void ShowDialog(string caption, string message, string alert);
  }

  public class DialogService : IDialogService {
    public void ShowDialog(string caption, string message, string alert) {
      var dialog = new WaitingModalDialog(caption, message, alert);
      dialog.Show();
    }
  }
}
