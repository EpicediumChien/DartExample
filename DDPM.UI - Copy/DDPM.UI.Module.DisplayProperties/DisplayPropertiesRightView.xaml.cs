using DDPM.UI.Common;
using System.Windows;
using System.Windows.Controls;

namespace DDPM.UI.Module.DisplayProperties
{
    /// <summary>
    /// Interaction logic for DisplayPropertiesRightView.xaml
    /// </summary>
    public partial class DisplayPropertiesRightView : UserControl
    {
        //0604 Bruce 將change select item和swich click事件拿掉，改用ViemModel的set去做設定功能
        public DisplayPropertiesRightView()
        {
            InitializeComponent();
            //DisplayPropertiesViewModel vm = (DisplayPropertiesViewModel)DataContext;
        }

        private void CallWindowsSettings_Click(object sender, RoutedEventArgs e)
        {
            DdpmCommonHelper.DeviceManagerSA.CallWindowsDisplaySetting();
            //Refresh();
        }

        private void RefreshUI()
        {
            DisplayPropertiesViewModel vm = (DisplayPropertiesViewModel)DataContext;
            vm.RefreshUI();
        }
    }
}