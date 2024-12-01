using DDPM.UI.Common;
using UserControl = System.Windows.Controls.UserControl;

namespace DDPM.UI.Plugin.DdpmHomePlugin
{
    /// <summary>
    /// Interaction logic for ConnectionHoverView.xaml
    /// </summary>
    public partial class ConnectionHoverView : UserControl
    {
        //The ViewModel (DataContext) will be the HomeDevice of the attached ListViewItem
        public ConnectionHoverView()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            //Derek 1129 check if DDPM is launched by QAM, then info SA that homepage is ready
            //DdpmCommonHelper.DDPMMesssageBox("UserControl_Loaded", "DDPMMesssageBox");
            if (DdpmCommonHelper.DeviceManagerSA!.GetIsDDPMLaunchByQAM().Result == true)
            {
                DdpmCommonHelper.WriteUILog($"GetIsDDPMLaunchByQAM = true from ConnectionHoverView::UserControl_Loaded");

                DdpmCommonHelper.DeviceManagerSA!.SetIsDDPMHomepageReadyAsync(true);
                DdpmCommonHelper.DeviceManagerSA!.SetIsDDPMLaunchByQAMAsync(false);
            }
        }
    }
}