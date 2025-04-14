using DDPM.UI.Common;
using Dell.Client.Framework.Common;
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

namespace DDPM.UI.Plugin.ExitAppPlugin.Views
{
    /// <summary>
    /// Interaction logic for ExitAppView.xaml
    /// </summary>
    public partial class ExitAppView : UserControl
    {
        public ExitAppView()
        {
            InitializeComponent();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown(0);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            IConsole console = ExitAppPlugin.PluginIoc.GetService<IConsole>();
            if (console != null)
            {
                console?.RaiseEvent(ConsoleEventNames.Masthead_ShowAddDeviceIcon, this, new EventManagerArgs() { Tag = new List<bool> { false, false } });
                console?.RaiseEvent(ConsoleEventNames.Masthead_ShowSettingsIcon, this, new EventManagerArgs() { Tag = new List<bool> { false, false } });
            }

        }
    }
}
