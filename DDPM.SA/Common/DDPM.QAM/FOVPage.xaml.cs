using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DDPM.QAM
{
    /// <summary>
    /// Interaction logic for FOVPage.xaml
    /// </summary>
    public partial class FOVPage : UserControl
    {
        public FOVPage()
        {
            InitializeComponent();
            if (DdpmCommonHelper.QAMPageViewModel != null)
            {
                DataContext = DdpmCommonHelper.QAMPageViewModel;
            }
            InitializeFOV();
        }
        private void InitializeFOV()
        {
            if (DataContext is QAMPageViewModel vm && vm.CurrentDeviceInfo != null)
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
                        this.Height = this.Height - 51;
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
            if (sender is Border bdr && 
                DataContext is QAMPageViewModel vm)
            {
                var index = int.Parse(bdr.Tag.ToString()!);
                var val = vm.FOVs[index];
                if (val == vm.FieldOfView)
                { return; }
                vm.FOV_Selected(index);
                vm.isStatusChagneByDDPM = false;
                vm.FieldOfView = val;
            }
        }
    }
}
