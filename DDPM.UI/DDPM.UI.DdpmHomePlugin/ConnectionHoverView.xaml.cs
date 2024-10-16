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
    }
}