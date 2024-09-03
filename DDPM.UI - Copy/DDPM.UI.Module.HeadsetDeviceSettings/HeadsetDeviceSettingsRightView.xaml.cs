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

        public HeadsetDeviceSettingsRightView(HeadsetViewModel vm)
        {
            InitializeComponent();
            _vm = vm;
        }

        private void CloseDescription(object sender, MouseButtonEventArgs e)
        {
            _vm.DeviceSettingsDownloadDellAudioPageShow = false;
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