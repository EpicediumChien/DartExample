using DDPM.SA.Common.Settings;
using DDPM.UI.Common;
using Dell.Client.Framework.UX.WPF;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DDPM.UI.Plugin.WalkThroughPlugin
{
    /// <summary>
    /// SettingsPage.xaml 的互動邏輯
    /// </summary>
    public partial class WalkThroughPage : UserControl
    {
        private WalkThroughPageViewModel vm
        {
            get { return (WalkThroughPageViewModel)DataContext; }
        }
        public WalkThroughPage()
        {
            InitializeComponent();
            DataContext = new WalkThroughPageViewModel();
        }

        ~WalkThroughPage()
        {
        }
    }
}