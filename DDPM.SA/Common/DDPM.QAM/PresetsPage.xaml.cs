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

namespace DDPM.QAM
{
    /// <summary>
    /// Interaction logic for PresetsPage.xaml
    /// </summary>
    public partial class PresetsPage : UserControl
    {
        public PresetsPage()
        {
            InitializeComponent();
            DataContext = DdpmCommonHelper.QAMPageViewModel;
        }

        private void InitializeFOV()
        {
            QAMPageViewModel vm = DataContext as QAMPageViewModel;
            if (vm != null && vm.CurrentDeviceInfo != null)
            {
                var FOV = vm.CurrentDeviceInfo!.FOVValues;
                btnFOV0.Visibility = Visibility.Collapsed;
                btnFOV1.Visibility = Visibility.Collapsed;
                btnFOV2.Visibility = Visibility.Collapsed;
                switch (FOV.Length)
                {
                    case 2:
                        vm.FullView_Height = "172";
                        txtFOV0.Text = $"{FOV[0]}°";
                        txtFOV1.Text = $"{FOV[1]}°";
                        btnFOV0.Visibility = Visibility.Visible;
                        btnFOV1.Visibility = Visibility.Visible;
                        break;
                    case 3:
                        vm.FullView_Height = "216";
                        txtFOV0.Text = $"{FOV[0]}°";
                        txtFOV1.Text = $"{FOV[1]}°";
                        txtFOV2.Text = $"{FOV[2]}°";
                        btnFOV0.Visibility = Visibility.Visible;
                        btnFOV1.Visibility = Visibility.Visible;
                        btnFOV2.Visibility = Visibility.Visible;
                        break;
                }
            }
        }
        private void FOV_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border bdr)
            {
                QAMPageViewModel vm = DataContext as QAMPageViewModel;
                var index = int.Parse(bdr.Tag.ToString()!);
                var val = vm.FOVs[index];
                if (val == vm.FieldOfView)
                { return; }
                vm.FOV_Selected(index);
                vm.FieldOfView = val;
            }
        }
    }
}
