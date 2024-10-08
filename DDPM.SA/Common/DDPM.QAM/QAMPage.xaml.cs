using DDPM.SA.Common;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.UX.WPF.Controls;
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
using System.Windows.Shapes;

namespace DDPM.QAM
{
    /// <summary>
    /// Interaction logic for QAM.xaml
    /// </summary>
    public partial class QAMPage : Window
    {
        CameraSetting CameraSetting;
        private QAMPageViewModel vm
        {
            get { return (QAMPageViewModel)DataContext; }
        }
        public QAMPage(IDeviceManagerSA deviceMangerPlugin)
        {
            InitializeComponent();
            DdpmCommonHelper.DeviceManagerSA = deviceMangerPlugin;
            DdpmCommonHelper.QAMPageViewModel = new QAMPageViewModel();
            DataContext = DdpmCommonHelper.QAMPageViewModel;
        }
        private void Close_Click(object sender, MouseButtonEventArgs e)
        {
            if (CameraSetting != null)
            {
                CameraSetting.Close();
                CameraSetting = null;
            }
            this.Close();
        }

        private void CameraSetting_Click(object sender, MouseButtonEventArgs e)
        {
            if (CameraSetting != null)
            {
                CameraSetting.Close();
                CameraSetting = null;
            }
            else
            {
                CameraSetting = new CameraSetting();
                CameraSetting.Left = this.Left + this.Width;
                CameraSetting.Top = this.Top;
                CameraSetting.Width = 288;
                CameraSetting.Height = 128;
                CameraSetting.Show();
            }
        }

        private void CallDDPM_Click(object sender, MouseButtonEventArgs e)
        {

        }

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
                if (CameraSetting != null)
                {
                    CameraSetting.Left = this.Left + this.Width;
                    CameraSetting.Top = this.Top;
                }
            }
        }
    }
}
