using Dell.Client.Framework.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

namespace DDPM.UI.Plugin.SettingsPlugin
{
    /// <summary>
    /// Interaction logic for Settings_About.xaml
    /// </summary>
    public partial class Settings_About : UserControl
    {
        private ILog? _log;
        public Settings_About()
        {
            _log = SettingsPlugin.PluginIoc?.GetService<ILog>();
            _log?.Info("Settings_About initialize start");
            InitializeComponent();
            _log?.Info("Settings_About initialize done");
        }

        private void ThirdPartyLicenses_Click(object sender, RoutedEventArgs e)
        {
            ThirdPartyLicenses thirdPartyLicenses = new ThirdPartyLicenses();
            thirdPartyLicenses.Show();
        }

        private void LearnMore_Click(object sender, RoutedEventArgs e)
        {
            string url = "https://www.dell.com/ddpm";
            try
            {
                /*Process.Start(new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });*/
                DDPM.SA.Common.Settings.DDPMFileSecurity.StartProcessSafely(
                    null,
                    new ProcessStartInfo
                    {
                        FileName = url,
                        UseShellExecute = true
                    });
            }
            catch
            {
            }
        }
    }
}
