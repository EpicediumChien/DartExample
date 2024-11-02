using DDPM.UI.Common;
using DDPM.UI.Plugin.Common;
using DDPM.UI.Plugin.ViewModels;
using Newtonsoft.Json.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Windows.Devices.Geolocation;
using DDPM.SA.Common;

namespace DDPM.UI.Module.HeadsetDeviceSettings
{
    /// <summary>
    /// Interaction logic for HeadsetDeviceSettingsRightView.xaml
    /// </summary>
    public partial class HeadsetDeviceSettingsRightView : UserControl
    {
        private readonly HeadsetViewModel _vm;

        private readonly string regPath = $@"SOFTWARE\Dell\Dell Display And Peripheral Manager\UserSettings\Global\QRCode";
        private readonly string regKey = $"IsFirstTimeWalkThroughDone_com.dell.DPM.Plugin.LogicalDevice.HeadsetQRCode";
        string regKeyForQRCode = $"IsFirstTimeWalkThroughDone_com.dell.DPM.Plugin.LogicalDevice.QRCode";

        public HeadsetDeviceSettingsRightView(HeadsetViewModel vm)
        {
            InitializeComponent();
            _vm = vm;

            object regValue = null ;

            if (DdpmCommonHelper.DeviceManagerSA != null)
            {
                regValue = DdpmCommonHelper.DeviceManagerSA!.ReadRegistryData(DDPM.SA.Common.Settings.RegistryHive.LocalMachine, regPath, regKeyForQRCode);

                if (regValue != null)
                {

                    if (!Convert.ToBoolean(regValue))
                    {
                        _vm.DeviceSettingsDownloadDellAudioPageShow = false;
                    }
                }
            }
        }

        private void CloseDescription(object sender, MouseButtonEventArgs e)
        {
            try
            {
                _vm.DeviceSettingsDownloadDellAudioPageShow = false;

                if (DdpmCommonHelper.DeviceManagerSA != null)
                {
                    DdpmCommonHelper.DeviceManagerSA!.WriteRegistryData(DDPM.SA.Common.Settings.RegistryHive.LocalMachine, regPath, regKey, true);
                }
                _vm._log.Info($"[HeadsetViewModel] CloseDescription ....... success");
            }
            catch (Exception ex)
            {
                _vm._log.Info($"[HeadsetViewModel] CloseDescription ....... {ex.ToString()}");
            }
        }

        private void LearnmoreButton_Click(object sender, RoutedEventArgs e)
        {
            //LoadingScreen

            Window parentWindow = Window.GetWindow(this);
            double windowLeft = 0;
            double windowTop = 0;
            //LoadingScreen loadDialog = new LoadingScreen(parentWindow.ActualWidth, parentWindow.ActualHeight);
            //if (parentWindow != null)
            //{
            //    loadDialog.Owner = parentWindow;
            //    windowLeft = parentWindow.Left + (parentWindow.ActualWidth - loadDialog.Width) / 2;
            //    windowTop = parentWindow.Top + (parentWindow.ActualHeight - loadDialog.Height) / 2;
            //}
            //loadDialog.WindowStartupLocation = WindowStartupLocation.Manual;
            //loadDialog.Left = windowLeft;
            //loadDialog.Top = windowTop;
            //loadDialog.ShowDialog();
            /////////////////////////////////////////////////////////////////////////////////
            var parameter = "";
            AdvancedAction action;

            //Window parentWindow = Window.GetWindow(this);
            //double windowLeft = 0;
            //double windowTop = 0;
            LearnMorePage modalDialog = new(parentWindow.ActualWidth, parentWindow.ActualHeight, parameter);
            if (parentWindow != null)
            {
                modalDialog.Owner = parentWindow;
                windowLeft = parentWindow.Left;
                windowTop = parentWindow.Top;
            }
            modalDialog.WindowStartupLocation = WindowStartupLocation.Manual;
            modalDialog.Left = windowLeft;
            modalDialog.Top = windowTop;
            if (modalDialog.ShowDialog()!.Value)
            {
                parameter = modalDialog.Parameter;
            }
            else
            {
                //Initialize();
                return;
            }
        }
    }
}