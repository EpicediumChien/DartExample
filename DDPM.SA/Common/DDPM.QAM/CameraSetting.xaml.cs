using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;

namespace DDPM.QAM
{
    /// <summary>
    /// Interaction logic for CameraSetting.xaml
    /// </summary>
    public partial class CameraSetting : Window
    {
        QAMPage QAMPage;

        //Derek 2025/01/06 to fix bug that QAM UI will set to default when click Presets button again
        //private bool bFirstClick = true;

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
            if (DataContext is QAMPageViewModel vm && vm.CurrentDeviceInfo != null)
            {
                //condition from UI Derek 1225
                //CurrentDeviceInfo!.IsPropertyAutoFramingSensitivitySupported || CurrentDeviceInfo.IsPropertyAutoFramingSizeSupported || CurrentDeviceInfo.IsPropertyAutoFramingTransitionSupported
                //if (vm.CurrentDeviceInfo.IsPropertyAutoFramingSupported)
                if (vm.IsAutoFramingVisable())
                {
                    btnRes0.Width = 72;
                    btnRes1.Width = 72;
                    btnRes2.Width = 72;
                    btnRes3.Width = 72;
                }
                else
                {
                    btnRes0.Width = 96;
                    //btnRes1.Width = 96;
                    btnRes2.Width = 96;
                    btnRes3.Width = 96;
                }
                vm.RefreshUI();
            }
        }

        public void OpenPresetsFullView()
        {
            Presets_Click(this, null);
        }

        private void Presets_Click(object sender, MouseButtonEventArgs e)
        {
            //DdpmCommonHelper.DeviceManagerSA!.WriteLog($"Presets_Click, bFirstClick = {bFirstClick}");

            if (DataContext is QAMPageViewModel vm)
            {
                PresetsPage presetsPage = new PresetsPage();
                presetsPage.DataContext = vm;
                double newHeight = 145 + presetsPage.Height; //origin value is 128
                this.Height = newHeight;
                vm.FullView_Height = presetsPage.Height.ToString();
                vm.Settings_Selected(0);
                vm.RefreshUI();
                vm.OpenFullView(presetsPage);

                //select profile for first load
                //if (bFirstClick)
                //{
                //    bFirstClick = false;
                //    vm.SetProfile(); 
                //}
            }
        }

        private void AutoFraming_Click(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is QAMPageViewModel vm)
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
            QAMPageViewModel? vm = DataContext as QAMPageViewModel;
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
            if (DataContext is QAMPageViewModel vm)
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
                QAMPageViewModel? vm = DataContext as QAMPageViewModel;
                if (vm != null && vm.CurrentDeviceInfo != null)
                {
                    vm.IsDragging = true;
                }

                this.DragMove();
                if (QAMPage != null)
                {
                    //Make sure CameraSetting & QAMPage in same screen
                    var scalingRatio = Screen.PrimaryScreen.Bounds.Width / SystemParameters.PrimaryScreenWidth;
                    System.Drawing.Point cursorPosition = System.Windows.Forms.Cursor.Position;
                    Screen screen = Screen.FromPoint(cursorPosition);

                    if (this.Left + this.Width > screen.Bounds.Right / scalingRatio)
                    {
                        this.Left = screen.Bounds.Right / scalingRatio - this.Width;
                    }

                    if (this.Left * scalingRatio - screen.Bounds.Left < QAMPage.Width)
                    {
                        this.Left = screen.Bounds.Left / scalingRatio + QAMPage.Width;
                        QAMPage.Left = screen.Bounds.Left / scalingRatio;
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

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {

        }
    }
}
