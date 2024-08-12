using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
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
