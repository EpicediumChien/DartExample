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
    /// Interaction logic for CameraSetting.xaml
    /// </summary>
    public partial class CameraSetting : Window
    {
        private QAMPageViewModel vm
        {
            get { return (QAMPageViewModel)DataContext; }
        }
        public CameraSetting()
        {
            InitializeComponent();
            DataContext = new QAMPageViewModel();
            vm.RefreshUI();
        }

        private void Presets_Click(object sender, MouseButtonEventArgs e)
        {
            if (vm != null)
            {
                PresetsPage presetsPage = new PresetsPage();
                presetsPage.DataContext = vm;
                vm.QAMPage_Height = "344";
                vm.FullView_Height = "216";
                vm.Settings_Selected(0);
                vm.RefreshUI();
                vm.OpenFullView(presetsPage);
            }
        }

        private void AutoFraming_Click(object sender, MouseButtonEventArgs e)
        {
            if (vm != null)
            {
                AutoFramingPage autoFramingPage = new AutoFramingPage();
                autoFramingPage.DataContext = vm;
                vm.QAMPage_Height = "200";
                vm.FullView_Height = "72";
                vm.Settings_Selected(1);
                vm.RefreshUI();
                vm.OpenFullView(autoFramingPage);
            }
        }

        private void FOV_Click(object sender, MouseButtonEventArgs e)
        {
            if (vm != null)
            {
                FOVPage fOVPage = new FOVPage();
                fOVPage.DataContext = vm;
                vm.QAMPage_Height = "344";
                vm.FullView_Height = "216";
                vm.Settings_Selected(2);
                vm.RefreshUI();
                vm.OpenFullView(fOVPage);
            }
        }

        private void Zoom_Click(object sender, MouseButtonEventArgs e)
        {
            if (vm != null)
            {
                ZoomPage zoomPage = new ZoomPage();
                zoomPage.DataContext = vm;
                vm.QAMPage_Height = "200";
                vm.FullView_Height = "72";
                vm.Settings_Selected(3);
                vm.RefreshUI();
                vm.OpenFullView(zoomPage);
            }
        }
    }
}
