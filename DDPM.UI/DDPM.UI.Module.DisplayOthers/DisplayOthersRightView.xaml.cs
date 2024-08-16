using System.Windows;
using System.Windows.Controls;

namespace DDPM.UI.Module.DisplayOthers
{
    /// <summary>
    /// Interaction logic for DisplayOthersRightView.xaml
    /// </summary>
    public partial class DisplayOthersRightView : UserControl
    {
        private DisplayOthersViewModel vm
        {
            get => (DisplayOthersViewModel)DataContext;
        }

        public DisplayOthersRightView(DisplayOthersViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
        }

        private void tbOpenScreensaverSettings_Click(object sender, RoutedEventArgs e)
        {
            var psi = new System.Diagnostics.ProcessStartInfo();

            psi.FileName = @"C:\Windows\System32\rundll32.exe";
            psi.Arguments = "shell32.dll,Control_RunDLL desk.cpl,,1";
            psi.UseShellExecute = true;

            System.Diagnostics.Process.Start(psi);
        }

        private void import_Click(object sender, RoutedEventArgs e)
        {
        }

        private void export_Click(object sender, RoutedEventArgs e)
        {
            vm.ExportSettings();
        }
    }
}