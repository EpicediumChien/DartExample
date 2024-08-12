using DDPM.SA.Common;
using DDPM.UI.Common;
using DDPM.UI.Plugin.Common;
using Dell.Client.Framework.UX.WPF;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DDPM.UI.Plugin.DisplayPlugin.Views
{
    /// <summary>
    /// Interaction logic for DisplayDefaultLeftView.xaml
    /// </summary>
    public partial class DisplayDefaultLeftView : UserControl
    {
        private readonly string Restore = Strings.RestoreToDefault;// "Restore to default";

        public DisplayDefaultLeftView()
        {
            InitializeComponent();
            txtRestore.Text = Restore;
        }

        // 20240617  jim add
        private void Restore_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            RestoreModalDialog restoreModalDialog = new();
            Window parentWindow = Window.GetWindow(this);
            if (parentWindow != null)
            {
                restoreModalDialog.Owner = parentWindow;
            }

            bool? dialogResult = restoreModalDialog.ShowDialog();
            if ((dialogResult == true) && (DdpmCommonHelper.DeviceManagerSA !=null) && 
                (DdpmCommonHelper.ModuleOwner !=null) && (DdpmCommonHelper.ModuleOwner.SelectedHomeDevice != null))
            {
                bool r;
                
                // 20240627 jim modify
                r = DdpmCommonHelper.DeviceManagerSA.SetVCPCapability(DdpmCommonHelper.ModuleOwner.SelectedHomeDevice.MonitorInfo, 0x04, 1).Result;

                // 20240627 jim add
                //Return to DdpmHomePage
                IConsole? console = DisplayPlugin.PluginIoc.GetService<IConsole>();
                console?.ShowPluginById(DDPM.UI.Common.Constants.DdpmHomePluginId);

                //MessageBox.Show("OK button was clicked");
            }
        }

    }
}
