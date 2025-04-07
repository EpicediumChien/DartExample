using DDPM.UI.Common;
using DDPM.UI.Plugin.Common;
using DDPM.UI.Plugin.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DDPM.UI.Module.HeadsetDeviceSettings
{
    /// <summary>
    /// Interaction logic for HeadsetDeviceSettingsRightView.xaml
    /// </summary>
    public partial class HeadsetDeviceSettingsRightView : UserControl
    {
        private readonly HeadsetViewModel _vm;
        private LearnMorePage modalDialog;
        public HeadsetDeviceSettingsRightView(HeadsetViewModel vm)
        {
            InitializeComponent();
            _vm = vm;
        }

        private void CloseDescription(object sender, MouseButtonEventArgs e)
        {
            try
            {
                _vm.DeviceSettingsDownloadDellAudioPageShow = false;

                //if (DdpmCommonHelper.DeviceManagerSA != null)
                //{
                //    DdpmCommonHelper.DeviceManagerSA!.WriteRegistryData(DDPM.SA.Common.Settings.RegistryHive.LocalMachine, _vm.RegPath, _vm.RegKeyForQRCode, true);
                //}
                DdpmCommonHelper.WriteRegistryData(DDPM.SA.Common.Settings.RegistryHive.LocalMachine, _vm.RegPath, _vm.RegKeyForQRCode, true);
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
            var parameter = _vm.Name;
            AdvancedAction action;

            //Window parentWindow = Window.GetWindow(this);
            //double windowLeft = 0;
            //double windowTop = 0;
            modalDialog = new(parentWindow.ActualWidth, parentWindow.ActualHeight, parameter, _vm.Model, _vm.FirmwareVersion2);
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
        private void Leave(object sender, RoutedEventArgs e)
        {
            if(modalDialog != null)
                modalDialog.Close();
        }
        private double GetScreenScaleX()
        {
            var source = PresentationSource.FromVisual(this);
            if (source?.CompositionTarget != null)
            {
                return source.CompositionTarget.TransformToDevice.M11;
            }
            return 1;
        }

        private void toolTip_Opened(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.ToolTip? target = sender as System.Windows.Controls.ToolTip;
            if (target == null)
                return;
            double screenScaleX = GetScreenScaleX();
            Window mainWindow = System.Windows.Application.Current.MainWindow;
            double winRightX = (mainWindow.Left + mainWindow!.ActualWidth) * screenScaleX;
            double mousepositionX = System.Windows.Forms.Cursor.Position.X;
            double mouseaddtooltip = mousepositionX + target.ActualWidth * screenScaleX;
            if (winRightX > mouseaddtooltip)
            {
                target.HorizontalOffset = 2;
            }
            else
            {
                target.HorizontalOffset = -1 * target.ActualWidth + 14;
            }
        }
    }
}