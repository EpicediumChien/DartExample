using DDPM.UI.Common;
using System.Windows.Controls;
using Dell.Client.Framework.UX.WPF;
using Dell.Client.Framework.Common;
using DDPM.UI.Module.EzMemory.Views;
using System.Windows;

namespace DDPM.UI.Module.EzMemory
{
    /// <summary>
    /// Interaction logic for EzMemoryRightView.xaml
    /// </summary>
    public partial class EzMemoryRightView : UserControl
    {
        private EzMemoryViewModel? vm;

        public EzMemoryRightView(EzMemoryViewModel? vm)
        {
            InitializeComponent();
            DataContext = vm;
        }

        private void EzMemoryStart_Click(object sender, RoutedEventArgs e)
        {
            EzMemoryFirst ezFirst = new EzMemoryFirst();
            ezFirst.DataContext = vm;
            DdpmCommonHelper.ModuleOwner?.OpenFullView(ezFirst);
        }

        //private void CheckBox_Click_1(object sender, System.Windows.RoutedEventArgs e)
        //{
        //    IConsole? console = DdpmCommonHelper.MyConsole;
        //    if (console != null)
        //    {
        //        if  (ck.IsChecked != null)
        //        {
        //            var args = new EventManagerArgs();
        //            args.Tag = (bool)ck.IsChecked; //true=Show, false=Hide
        //            console.RaiseEvent(ConsoleEventNames.Masthead_ShowAddDeviceIcon, this, args);
        //        }
        //    }
        //}

        //private void ckSettings_Click(object sender, System.Windows.RoutedEventArgs e)
        //{
        //    IConsole? console = DdpmCommonHelper.MyConsole;
        //    if (console != null)
        //    {
        //        if (ckSettings.IsChecked != null)
        //        {
        //            var args = new EventManagerArgs();
        //            args.Tag = (bool)ckSettings.IsChecked; //true=Show, false=Hide
        //            console.RaiseEvent(ConsoleEventNames.Masthead_ShowSettingsIcon, this, args);
        //        }
        //    }
        //}
    }
}