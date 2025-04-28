using DDPM.SA.Common.Settings;
using DDPM.UI.Common;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DDPM.UI.Module.MonitorAudio
{
    /// <summary>
    /// Interaction logic for DisplayPropertiesRightView.xaml
    /// </summary>
    public partial class MonitorAudioRightView : UserControl
    {
        //0604 Bruce 將change select item和swich click事件拿掉，改用ViemModel的set去做設定功能
        public MonitorAudioRightView()
        {
            InitializeComponent();
            //DisplayPropertiesViewModel vm = (DisplayPropertiesViewModel)DataContext;

            if (DdpmCommonHelper.DeviceManagerSA != null)
            {
                
            }
            // Set the input method to English for the entire UserControl
            InputMethod.SetPreferredImeState(this, InputMethodState.Off);

        }
        ~MonitorAudioRightView()
        {
            if (DdpmCommonHelper.DeviceManagerSA != null)
            {
                
            }
        }

        private void CallWindowsSettings_Click(object sender, RoutedEventArgs e)
        {
            DdpmCommonHelper.DeviceManagerSA.CallWindowsDisplaySetting();
            //Refresh();
        }

        private void RefreshUI()
        {
            MonitorAudioViewModel vm = (MonitorAudioViewModel)DataContext;
            vm.RefreshUI();
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

        private void NotMute_MouseDown(object sender, MouseButtonEventArgs e)
        {
            MonitorAudioViewModel vm = (MonitorAudioViewModel)this.DataContext;
            if (vm != null)
            {
                vm.SetMute(false);
            }
        }

        private void Mute_MouseDown(object sender, MouseButtonEventArgs e)
        {
            MonitorAudioViewModel vm = (MonitorAudioViewModel)this.DataContext;
            if (vm != null)
            {
                vm.SetMute(true);
            }
        }
    }
}