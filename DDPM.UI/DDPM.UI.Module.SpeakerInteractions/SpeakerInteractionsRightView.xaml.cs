using DDPM.SA.Common;
using DDPM.UI.Common;
using DDPM.UI.Plugin.ViewModels;
using Microsoft.Win32;
using System.Windows.Controls;
using System.Windows.Input;

namespace DDPM.UI.Module.SpeakerInteractions
{
    /// <summary>
    /// Interaction logic for SpeakerInteractionsRightView.xaml
    /// </summary>
    public partial class SpeakerInteractionsRightView : UserControl
    {
        public SoundBarViewModel _vm;

        bool isTeamsInstalled = IsProgramInstalled("Teams");

        bool isZoomInstalled = IsProgramInstalled("Zoom");

        bool isMeetInstalled = IsProgramInstalled("Google Meet");// Awalys need
        public SpeakerInteractionsRightView(SoundBarViewModel vm)
        {
            InitializeComponent();
            _vm = vm;

            if (DdpmCommonHelper.DeviceManagerSA != null)
            {
                Dictionary<string, InstalledAppInfo> data = DdpmCommonHelper.DeviceManagerSA.GetAllAppList().Result;
                var matchingTeams = data.Values.Where(app => app.AppName.ToLower().Contains("teams", StringComparison.OrdinalIgnoreCase));
                var matchingZoom = data.Values.Where(app => app.AppName.ToLower().Contains("zoom", StringComparison.OrdinalIgnoreCase));
                var matchingMeet = data.Values.Where(app => app.AppName.ToLower().Contains("meet", StringComparison.OrdinalIgnoreCase));
                if (!matchingTeams.Any())//!isTeamsInstalled)
                {
                    MicrosoftTeamsButton.Visibility = System.Windows.Visibility.Collapsed;
                    DdpmCommonHelper.WriteUILog($"[SpeakerInteractionsRightView] MicrosoftTeamsButton Collapsed");
                }
                else
                {
                    DdpmCommonHelper.WriteUILog($"[SpeakerInteractionsRightView] MicrosoftTeamsButton Visibility");
                }
                if (!matchingZoom.Any())//!isZoomInstalled)
                { 
                    ZoomButton.Visibility = System.Windows.Visibility.Collapsed;
                    DdpmCommonHelper.WriteUILog($"[SpeakerInteractionsRightView] ZoomButton Collapsed");
                }
                else
                {
                    DdpmCommonHelper.WriteUILog($"[SpeakerInteractionsRightView] ZoomButton Visibility");
                }
                if (!matchingMeet.Any())//!isMeetInstalled)// Awalys need
                { 
                    GoogleMeetButton.Visibility = System.Windows.Visibility.Visible;
                    DdpmCommonHelper.WriteUILog($"[SpeakerInteractionsRightView] GoogleMeetButton Collapsed");
                }
                else
                {
                    GoogleMeetButton.Visibility = System.Windows.Visibility.Visible;
                    //DdpmCommonHelper.WriteUILog($"[SpeakerInteractionsRightView] GoogleMeetButton Visibility");
                }
            }
            else
            {
                MicrosoftTeamsButton.Visibility = System.Windows.Visibility.Visible;
                ZoomButton.Visibility = System.Windows.Visibility.Visible;
                GoogleMeetButton.Visibility = System.Windows.Visibility.Visible;
                DdpmCommonHelper.WriteUILog($"[SpeakerInteractionsRightView] DdpmCommonHelper.DeviceManagerSA  null, Button All Visibility");
            }
        }

        private void MicrosoftTeamsButton_MouseMove(object sender, MouseEventArgs e)
        {
            _vm.ChangeImage(_vm.Model, "MicrosoftTeams");
        }

        private void ZoomButton_MouseMove(object sender, MouseEventArgs e)
        {
            _vm.ChangeImage(_vm.Model, "Zoom");
        }

        private void GoogleMeetButton_MouseMove(object sender, MouseEventArgs e)
        {
            _vm.ChangeImage(_vm.Model, "GoogleMeet");
        }

        private void SkypeforBusinessButton_MouseMove(object sender, MouseEventArgs e)
        {
            _vm.ChangeImage(_vm.Model, "SkypeforBusiness");
        }

        private void MicrosoftTeamsButton_MouseLeave(object sender, MouseEventArgs e)
        {
            _vm.ChangeImageMouseLeave(_vm.Model);
        }

        public static bool IsProgramInstalled(string programName)
        {
            // 檢查 HKLM\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall
            if (CheckUninstallKey(RegistryHive.LocalMachine, programName))
                return true;

            // 檢查 HKCU\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall
            if (CheckUninstallKey(RegistryHive.CurrentUser, programName))
                return true;

            return false;
        }

        private static bool CheckUninstallKey(RegistryHive hive, string programName)
        {
            using (var baseKey = RegistryKey.OpenBaseKey(hive, RegistryView.Registry64))
            using (var key = baseKey.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall"))
            {
                if (key == null) return false;

                foreach (var subKeyName in key.GetSubKeyNames())
                {
                    using (var subKey = key.OpenSubKey(subKeyName))
                    {
                        var displayName = subKey?.GetValue("DisplayName") as string;
                        if (!string.IsNullOrEmpty(displayName) && displayName.IndexOf(programName, StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
    }
}