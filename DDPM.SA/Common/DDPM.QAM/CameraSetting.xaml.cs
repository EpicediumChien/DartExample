using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
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
        QAMPage QAMPage;

        public CameraSetting(QAMPage qam)
        {
            InitializeComponent();
            if (DdpmCommonHelper.QAMPageViewModel != null)
            {
                DataContext = DdpmCommonHelper.QAMPageViewModel;
            }
            InitializeSettings();

            QAMPage = qam;
        }
        private void InitializeSettings()
        {
            QAMPageViewModel vm = DataContext as QAMPageViewModel;
            if (vm != null && vm.CurrentDeviceInfo != null)
            {
                if (vm.CurrentDeviceInfo.IsPropertyAutoFramingSupported)
                {
                    btnRes0.Width = 72;
                    btnRes1.Width = 72;
                    btnRes2.Width = 72;
                    btnRes3.Width = 72;
                }
                else
                {
                    btnRes0.Width = 96;
                    btnRes1.Width = 96;
                    btnRes2.Width = 96;
                    btnRes3.Width = 96;
                }
                vm.RefreshUI();
            }
        }
        private void Presets_Click(object sender, MouseButtonEventArgs e)
        {
            QAMPageViewModel vm = DataContext as QAMPageViewModel;
            if (vm != null)
            {
                PresetsPage presetsPage = new PresetsPage();
                presetsPage.DataContext = vm;
                double newHeight = 145 + presetsPage.Height; //origin value is 128 
                this.Height = newHeight;
                vm.FullView_Height = presetsPage.Height.ToString();
                vm.Settings_Selected(0);
                vm.RefreshUI();
                vm.OpenFullView(presetsPage);
            }
        }

        private void AutoFraming_Click(object sender, MouseButtonEventArgs e)
        {
            QAMPageViewModel vm = DataContext as QAMPageViewModel;
            if (vm != null)
            {
                AutoFramingPage autoFramingPage = new AutoFramingPage();
                autoFramingPage.DataContext = vm;
                double newHeight = 128 + autoFramingPage.Height;
                this.Height = newHeight;
                vm.FullView_Height = autoFramingPage.Height.ToString();
                vm.Settings_Selected(1);
                vm.RefreshUI();
                vm.OpenFullView(autoFramingPage);
            }
        }

        private void FOV_Click(object sender, MouseButtonEventArgs e)
        {
            QAMPageViewModel vm = DataContext as QAMPageViewModel;
            if (vm != null)
            {
                FOVPage fOVPage = new FOVPage();
                fOVPage.DataContext = vm;
                double newHeight = 128 + fOVPage.Height;
                this.Height = newHeight;
                vm.FullView_Height = fOVPage.Height.ToString();
                vm.Settings_Selected(2);
                vm.RefreshUI();
                vm.OpenFullView(fOVPage);
            }
        }

        private void Zoom_Click(object sender, MouseButtonEventArgs e)
        {
            QAMPageViewModel vm = DataContext as QAMPageViewModel;
            if (vm != null)
            {
                ZoomPage zoomPage = new ZoomPage();
                zoomPage.DataContext = vm;
                double newHeight = 128 + zoomPage.Height;
                this.Height = newHeight;
                vm.FullView_Height = zoomPage.Height.ToString();
                vm.Settings_Selected(3);
                vm.RefreshUI();
                vm.OpenFullView(zoomPage);
            }
        }

        private void Header_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                QAMPageViewModel vm = DataContext as QAMPageViewModel;
                if (vm != null && vm.CurrentDeviceInfo != null)
                {
                    vm.IsDragging = true;
                }

                this.DragMove();
                if (QAMPage != null)
                {
                            //Make sure CameraSetting & QAMPage in same screen
        System.Drawing.Point cursorPosition = System.Windows.Forms.Cursor.Position;
        Screen screen = Screen.FromPoint(cursorPosition);
        
                            if (this.Left + this.Width > screen.Bounds.Right)
                                {
            this.Left = screen.Bounds.Right - this.Width;
                                }
        
                            if (this.Left - screen.Bounds.Left < QAMPage.Width)
                                {
            this.Left = screen.Bounds.Left + QAMPage.Width;
            QAMPage.Left = screen.Bounds.Left;
                                }
                            else
                                {
            QAMPage.Left = this.Left - QAMPage.Width;
                                }
        QAMPage.Top = this.Top;
                        }

                if (vm != null && vm.CurrentDeviceInfo != null)
                {
        vm.IsDragging = false;
                        }
            }
        }
    }
}
