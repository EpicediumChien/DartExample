using CommunityToolkit.Mvvm.ComponentModel;
using DDPM.UI.Common;
using DDPM.UI.Plugin.DdpmHomePlugin.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Drawing2D;
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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using Window = System.Windows.Window;

namespace DDPM.UI.Plugin.WalkThroughPlugin
{
    /// <summary>
    /// WalkThroughBox.xaml 的互動邏輯
    /// </summary>
    public partial class WalkThroughBox : Window
    {
        private WalkThroughBoxViewModel _viewModel;
        private WalkThroughPageViewModel ViewModel;
        private double screenScalingFactor = 1.0;
        private Window mainWindow;
        internal class WalkThroughBoxViewModel : ObservableObject, INotifyPropertyChanged
        {
            public new event PropertyChangedEventHandler? PropertyChanged;

            private string _strTitle = "";
            private string _strContent = "";

            public string strTitle
            {
                get { return _strTitle; }
                set
                {
                    _strTitle = value;
                    NotifyPropertyChanged(nameof(strTitle));
                }
            }

            public string strContent
            {
                get { return _strContent; }
                set
                {
                    _strContent = value;
                    NotifyPropertyChanged(nameof(strContent));
                }
            }

            private void NotifyPropertyChanged(string info)
            {
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs(info));
                }
            }

            private int _currentAnimationPage;
            public int CurrentAnimationPage
            {
                get => _currentAnimationPage;
                set => SetProperty(ref _currentAnimationPage, value);
            }
            private Visibility _skipButtonVisibility = Visibility.Visible;
            public Visibility SkipButtonVisibility
            {
                get => _skipButtonVisibility;
                set => SetProperty(ref _skipButtonVisibility, value);
            }
            private double _progressValue = 1;//first round set 1
            public double ProgressValue
            {
                get
                {
                    return _progressValue;
                }
                set
                {
                    _progressValue = value;
                    OnPropertyChanged(nameof(ProgressValue));
                }
            }
        }

        private int _currentPage = 1;
        private int _totalPages = 5;

        public WalkThroughBox(WalkThroughPageViewModel viewModel, Window owner, double factor = 1.0)
        {
            InitializeComponent();
            ViewModel = viewModel;
            var devicePages = WalkThroughData.WalkThroughData.GetDevicePages((int)DdpmCommonHelper.PreviousOsTheme);
            _viewModel = new WalkThroughBoxViewModel
            {
                strTitle = devicePages["DDPM"][_currentPage].MainText,
                strContent = devicePages["DDPM"][_currentPage].SubText,
                SkipButtonVisibility = _currentPage < _totalPages ? Visibility.Visible : Visibility.Collapsed
            };

            _viewModel.CurrentAnimationPage = devicePages["DDPM"].Count - 2;
            //_totalPages = devicePages["DDPM"].Count - 1;

            DataContext = _viewModel;
            ViewModel.IsOtherVisibility = true;
            ViewModel.Img3Source = DdpmCommonHelper.GetImageSourceFromCommonResource(devicePages["DDPM"][_currentPage].MainImageSource, "DDPM.UI.WalkThroughData");
            //_currentPage++;
            //UpdatePage(devicePages["DDPM"][1].MainImageSource);
            mainWindow = owner;
            base.Owner = owner;
            screenScalingFactor = factor;
            UpdatePosition("Top_Right");
        }
        private void NextBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPage < _totalPages)
            {
                _currentPage++;
                UpdateText(_currentPage);
                UpdateProgressBar(true);
                //_currentPage++;
                if (_currentPage != _totalPages)
                    UpdatePosition("Left");
                else
                    UpdatePosition("Top_Right");
            }
            else
            {
                EndProgress();
            }
        }

        private void ArrowButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPage > 1)
            {
                _currentPage--;
                UpdateText(_currentPage);
                UpdateProgressBar(false);
                if (_currentPage == 1)
                    UpdatePosition("Top_Right");
                else
                    UpdatePosition("Left");
            }
            else
            {
                ViewModel.IsDDPMVisibility = true;
                this.Close();
            }
        }

        private void UpdateProgressBar(bool isForward)
        {
            double newProgressValue;

            if (isForward)
            {
                // forward
                newProgressValue = Math.Min(_viewModel.ProgressValue + 1, _totalPages);
            }
            else
            {
                // backward
                newProgressValue = Math.Max(_viewModel.ProgressValue - 1, 1);
            }

            DoubleAnimation progressAnimation = new DoubleAnimation
            {
                From = _viewModel.ProgressValue,
                To = newProgressValue,
                Duration = new Duration(TimeSpan.FromSeconds(0.5)),
                FillBehavior = FillBehavior.HoldEnd
            };

            WalkThroughProgressbar.BeginAnimation(System.Windows.Controls.ProgressBar.ValueProperty, progressAnimation);

            _viewModel.ProgressValue = newProgressValue;
        }

        private void UpdateText(int page)
        {
            var devicePages = WalkThroughData.WalkThroughData.GetDevicePages((int)DdpmCommonHelper.PreviousOsTheme);

            _viewModel.strTitle = devicePages["DDPM"][page].MainText;
            _viewModel.strContent = devicePages["DDPM"][page].SubText;

            UpdatePage(devicePages["DDPM"][page].MainImageSource);
        }

        private void UpdatePage(string page)
        {
            //ViewModel.IsPeripheralVisible = false;
            //ViewModel.IsDDPMVisibility = false;
            //ViewModel.IsOtherVisibility = true;
            //ViewModel.Img3Source = DdpmCommonHelper.GetImageSourceFromCommonResource($"WalkThrough/DDPM/DDPM{page}.png", "DDPM.UI.WalkThroughData");
            ViewModel.Img3Source = DdpmCommonHelper.GetImageSourceFromCommonResource(page, "DDPM.UI.WalkThroughData");
        }

        private void UpdatePosition(string position, double factor = 1.0)
        {
            switch (position.ToLower())
            {
                case "top_right":
                    if (mainWindow != null) {
                        if (mainWindow.WindowState == WindowState.Normal)
                        {
                            this.Left = this.Owner.Left + base.Owner.Width - this.Width - 122;
                            this.Top = this.Owner.Top + 64;
                        }
                        else
                        {
                            var screen = Screen.FromHandle(new System.Windows.Interop.WindowInteropHelper(mainWindow).Handle); // Default to primary screen
                            this.Left = (screen.WorkingArea.Left + screen.WorkingArea.Width) / screenScalingFactor - this.Width - 122;
                            this.Top = screen.WorkingArea.Top / screenScalingFactor + 64 ;
                        }
                    }
                    else
                    {
                        DdpmCommonHelper.WriteUILog("[UpdatePosition] AppWalkThroughBox can not detect MainWindow!!");
                    }
                    break;
                case "left":
                    if (mainWindow != null)
                    {
                        if (mainWindow.WindowState == WindowState.Normal)
                        {
                            this.Left = this.Owner.Left + 164; // Align with the left edge of the owner
                            this.Top = this.Owner.Top + (base.Owner.Height - this.Height) / 2; // Center vertically
                        }
                        else
                        {
                            var screen = Screen.FromHandle(new System.Windows.Interop.WindowInteropHelper(mainWindow).Handle); // Default to primary screen
                            this.Left = screen.WorkingArea.Left / screenScalingFactor + 164; // Align with the left edge of the owner
                            this.Top = screen.WorkingArea.Top / screenScalingFactor + (screen.WorkingArea.Height / screenScalingFactor - this.Height) / 2; // Center vertically
                        }
                    }
                    else
                    {
                        DdpmCommonHelper.WriteUILog("[UpdatePosition] AppWalkThroughBox can not detect MainWindow!!");
                    }
                    break;
            }
        }

        private void EndProgress()
        {
            if (DdpmHomePlugin.DdpmHomePlugin.WalkThroughQueue.Count != 0) // Error handling
            {
                ViewModel.WriteWalkThroughReg("DDPM");
                DdpmHomePlugin.DdpmHomePlugin.WalkThroughEndList.Add(new WalkThroughInfo("DDPM", "DDPM", null)); // Add DDPM to the end of the queue
                DdpmHomePlugin.DdpmHomePlugin.WalkThroughQueue.RemoveAll(item => item.ModelName == "DDPM"); // Remove all DDPM from the queue
            }
            if (DdpmHomePlugin.DdpmHomePlugin.WalkThroughQueue.Count == 0)
            {
                DdpmHomePlugin.DdpmHomePlugin.ShowPluginById = false;
                ViewModel.EndWalkThrough();
            }
            else
            {
                if (!DdpmHomePlugin.DdpmHomePlugin.WalkThroughQueue.Exists(info => info.ModelName == "DDPM"))
                {
                    ViewModel.IsPeripheralVisible = true;
                    ViewModel.IsDDPMVisibility = false;
                    ViewModel.InitializeDeviceFromQueue();
                    ViewModel.UpdateButtonVisibility();
                }
            }

            this.Close();
        }

        private void SkipBtn_Click(object sender, RoutedEventArgs e)
        {
            EndProgress();
        }

        public void RefreshWalkThroughBoxPosition()
        {
            if (_currentPage == _totalPages || _currentPage == 1)
                UpdatePosition("Top_Right");
            else
                UpdatePosition("Left");
        }
    }
}
