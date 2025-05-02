using DDPM.SA.Common.Settings;
using DDPM.UI.Common;
using DDPM.UI.Plugin.ViewModels;
using Dell.Client.Framework.UX.WPF.Controls;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using UserControl = System.Windows.Controls.UserControl;

namespace DDPM.UI.Module.HeadsetAudioForSB725Settings
{
    /// <summary>
    /// Interaction logic for HeadsetAudioForSB725SettingsRightView.xaml
    /// </summary>
    public partial class HeadsetAudioForSB725SettingsRightView : UserControl, INotifyPropertyChanged
    {
        private bool isDragging = false;
        private Image currentNode;
        private Point clickPosition;

        public event PropertyChangedEventHandler PropertyChanged;

        private readonly AirAudioViewModel _vm;

        /// <summary>
        /// AirAudioViewModel data in
        /// </summary>
        /// <param name="vm">ViewModel</param>
        public HeadsetAudioForSB725SettingsRightView(AirAudioViewModel vm)
        {
            InitializeComponent();
            //var converter = (CollaborationCheckedToVisibilityConverter)Resources["CollaborationCheckedToVisibilityConverter"];
            _vm = vm;
            //converter.ViewModel = _vm;

            InitializeAsync();
            _vm!.AirAudioChanged += AirAudioChanged;
        }

        private void AirAudioChanged(object? sender, string e)
        {
            switch (e) 
            {
                case "RestoreToDefault":
                    _vm.IsDefaultChecked = true;
                    SetNodeValue(Node1, 0);
                    SetNodeValue(Node2, 0);
                    SetNodeValue(Node3, 0);
                    SetNodeValue(Node4, 0);
                    SetNodeValue(Node5, 0);
                    _vm._deviceManager.SetAirAudioBand1GainAsync(_vm.CurrentDeviceInfo!.ID.ToString(), 0).Wait();
                    _vm._deviceManager.SetAirAudioBand2GainAsync(_vm.CurrentDeviceInfo!.ID.ToString(), 0).Wait();
                    _vm._deviceManager.SetAirAudioBand3GainAsync(_vm.CurrentDeviceInfo!.ID.ToString(), 0).Wait();
                    _vm._deviceManager.SetAirAudioBand4GainAsync(_vm.CurrentDeviceInfo!.ID.ToString(), 0).Wait();
                    _vm._deviceManager.SetAirAudioBand5GainAsync(_vm.CurrentDeviceInfo!.ID.ToString(), 0).Wait();
                    break;
            }
        }

        private async Task InitializeAsync()
        {
            _vm._log!.Info("[HeadsetAudioSettingsRightView] Before Invoke_PleaseWaitAsync");
            
            await _vm.Invoke_PleaseWaitAsync(_vm.Model, _vm);
            
            _vm._log!.Info("[HeadsetAudioSettingsRightView] After Invoke_PleaseWaitAsync");
            if (_vm.deviceInfoDTP!.IsPresetsSupported)
            {
                if (_vm.deviceInfoDTP!.Band1Gain > 4 || _vm.deviceInfoDTP!.Band1Gain < -6)
                {
                    SetNodeValue(Node1, 0);
                    _vm._log!.Info($"[HeadsetAudioSettingsRightView] HeadsetAudioSettingsRightView SetNodeValue ... Band1Gain 00000");
                }
                else
                {
                    SetNodeValue(Node1, _vm.deviceInfoDTP!.Band1Gain);
                    _vm._log!.Info($"[HeadsetAudioSettingsRightView] HeadsetAudioSettingsRightView SetNodeValue ... Band1Gain {_vm.deviceInfoDTP!.Band1Gain.ToString()}");
                }

                if (_vm.deviceInfoDTP!.Band2Gain > 4 || _vm.deviceInfoDTP!.Band2Gain < -6)
                {
                    SetNodeValue(Node2, 0);
                    _vm._log!.Info($"[HeadsetAudioSettingsRightView] HeadsetAudioSettingsRightView SetNodeValue ... Band2Gain 00000");
                }
                else
                {
                    SetNodeValue(Node2, _vm.deviceInfoDTP!.Band2Gain);
                    _vm._log!.Info($"[HeadsetAudioSettingsRightView] HeadsetAudioSettingsRightView SetNodeValue ... Band2Gain {_vm.deviceInfoDTP!.Band2Gain.ToString()}");
                }

                if (_vm.deviceInfoDTP!.Band3Gain > 4 || _vm.deviceInfoDTP!.Band3Gain < -6)
                {
                    SetNodeValue(Node3, 0);
                    _vm._log!.Info($"[HeadsetAudioSettingsRightView] HeadsetAudioSettingsRightView SetNodeValue ... Band3Gain 00000");
                }
                else
                {
                    SetNodeValue(Node3, _vm.deviceInfoDTP!.Band3Gain);
                    _vm._log!.Info($"[HeadsetAudioSettingsRightView] HeadsetAudioSettingsRightView SetNodeValue ... Band3Gain {_vm.deviceInfoDTP!.Band3Gain.ToString()}");
                }

                if (_vm.deviceInfoDTP!.Band4Gain > 4 || _vm.deviceInfoDTP!.Band4Gain < -6)
                {
                    SetNodeValue(Node4, 0);
                    _vm._log!.Info($"[HeadsetAudioSettingsRightView] HeadsetAudioSettingsRightView SetNodeValue ... Band4Gain 00000");
                }
                else
                {
                    SetNodeValue(Node4, _vm.deviceInfoDTP!.Band4Gain);
                    _vm._log!.Info($"[HeadsetAudioSettingsRightView] HeadsetAudioSettingsRightView SetNodeValue ... Band4Gain {_vm.deviceInfoDTP!.Band4Gain.ToString()}");
                }

                if (_vm.deviceInfoDTP!.Band5Gain > 4 || _vm.deviceInfoDTP!.Band5Gain < -6)
                {
                    SetNodeValue(Node5, 0);
                    _vm._log!.Info($"[HeadsetAudioSettingsRightView] HeadsetAudioSettingsRightView SetNodeValue ... Band5Gain 00000");
                }
                else
                {
                    SetNodeValue(Node5, _vm.deviceInfoDTP!.Band5Gain);
                    _vm._log!.Info($"[HeadsetAudioSettingsRightView] HeadsetAudioSettingsRightView SetNodeValue ... Band5Gain {_vm.deviceInfoDTP!.Band5Gain.ToString()}");
                }
            }
            //_vm.Invoke_PleaseWait(_vm.Model);

            //lock/unlock init, 9/23 add
            if (DdpmCommonHelper.DeviceManagerSA != null)
            {
                DdpmCommonHelper.DeviceManagerSA.ITSettingsActionEvent += DeviceManagerSA_ITSettingsActionEvent;

                DDPMSettings data = DdpmCommonHelper.DeviceManagerSA.ReloadAppConfigData().Result;
                if (data != null)
                {
                    if (data.LockSettings.Lock_Audio_ancMode)
                    {
                        _vm.isAncModeLocked = Visibility.Visible;
                        _vm.isAncEnabled = false;
                    }
                    else
                    {
                        _vm.isAncModeLocked = Visibility.Collapsed;
                        _vm.isAncEnabled = true;
                    }

                    if (data.LockSettings.Lock_Audio_micNoiseCancellation)
                    {
                        _vm.isMicCancelLocked = Visibility.Visible;
                        _vm.isMicTabStopped = false;
                    }
                    else
                    {
                        _vm.isMicCancelLocked = Visibility.Collapsed;
                        _vm.isMicTabStopped = true;
                    }
                }
            }
            SystemParameters_StaticPropertyChanged(null, null);
            SystemParameters.StaticPropertyChanged += SystemParameters_StaticPropertyChanged;
        }
        ~HeadsetAudioForSB725SettingsRightView()
        {
            if (DdpmCommonHelper.DeviceManagerSA != null)
            {
                DdpmCommonHelper.DeviceManagerSA.ITSettingsActionEvent -= DeviceManagerSA_ITSettingsActionEvent;
                SystemParameters.StaticPropertyChanged -= SystemParameters_StaticPropertyChanged;
            }
        }

        private void SystemParameters_StaticPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (DdpmCommonHelper.PreviousOsTheme == OSThemeEnum.Dark)
            {
                _vm.IsDarkTheme = true;
            }
            else
            {
                _vm.IsDarkTheme = false;
            }
        }

        private void DeviceManagerSA_ITSettingsActionEvent(object? sender, SA.Common.ITSettingEventArgs e)
        {
            bool? rst = DdpmCommonHelper.GetUINotifyPropertyValue_Boolean("Lock_Audio_ancMode", e);
            Dispatcher.Invoke(new Action(() =>
            {
                if (_vm != null)
                {
                    bool locked = false;
                    if (rst != null && rst == true)
                        locked = true;

                    _vm.isAncModeLocked = locked ? Visibility.Visible : Visibility.Collapsed;
                    _vm.isAncEnabled = !locked;
                }
            }));
            rst = DdpmCommonHelper.GetUINotifyPropertyValue_Boolean("Lock_Audio_micNoiseCancellation", e);
            Dispatcher.Invoke(new Action(() =>
            {
                if (_vm != null)
                {
                    bool locked = false;
                    if (rst != null && rst == true)
                        locked = true;

                    _vm.isMicCancelLocked = locked ? Visibility.Visible : Visibility.Collapsed;
                    _vm.isMicTabStopped = !locked;
                }
            }));
        }

        private void Slider_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            _vm.IsSliderDragging = true;
        }

        private void TipSlider_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            _vm.IsSliderDragging = false;
            //_vm.SetDPIValue();
        }

        private void TiltSlider_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            _vm.IsSliderDragging = false;
            //_vm.SetTouchScrollSensitivityLevel();
        }

        /// <summary>
        /// Node move, Mouse Left Button Down event
        /// </summary>
        /// <param name="sender">object type</param>
        /// <param name="e">EventArgs</param>
        private void Node_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            isDragging = true;
            currentNode = sender as Image;
            clickPosition = e.GetPosition(EqualizerCanvas);
            currentNode.CaptureMouse();

            // 更換選取後的圖片
            currentNode.Source = new BitmapImage(new Uri("pack://application:,,,/DDPM.UI.Common;component/Resources/Headset_whitedot.png"));

            // 顯示數值
            ShowNodeValue(currentNode, true);

            // 開始拖動時更新陰影效果
            //UpdateShadowVisibility();
        }

        /// <summary>
        /// Node move, Mouse Move event
        /// </summary>
        /// <param name="sender">object type</param>
        /// <param name="e">EventArgs</param>
        private void Node_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging && currentNode != null)
            {
                Point currentPosition = e.GetPosition(EqualizerCanvas);
                double offsetY = currentPosition.Y - clickPosition.Y;

                double newY = Canvas.GetTop(currentNode) + offsetY;

                // 限制上下移動範圍
                double minY = 0; // 頂端邊界
                double maxY = 150; // 底端邊界

                if (newY >= minY && newY <= maxY)
                {
                    Canvas.SetTop(currentNode, newY);
                    clickPosition = currentPosition;
                    UpdateCurve();
                    // 更新數值位置
                    UpdateNodeValuePosition(currentNode);

                    // 更新陰影效果
                    //UpdateShadowVisibility();
                }
            }
        }

        /// <summary>
        /// Node move, Mouse Left Button Up event
        /// </summary>
        /// <param name="sender">object type</param>
        /// <param name="e">EventArgs</param>
        private void Node_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            isDragging = false;
            if (currentNode != null)
            {
                switch (currentNode.Name)
                {
                    case "Node1":
                        _vm._deviceManager.SetAirAudioBand1GainAsync(_vm.CurrentDeviceInfo!.ID.ToString(), int.Parse(Node1Text.Text)).Wait();
                        break;

                    case "Node2":
                         _vm._deviceManager.SetAirAudioBand2GainAsync(_vm.CurrentDeviceInfo!.ID.ToString(), int.Parse(Node2Text.Text)).Wait();
                        break;

                    case "Node3":
                         _vm._deviceManager.SetAirAudioBand3GainAsync(_vm.CurrentDeviceInfo!.ID.ToString(), int.Parse(Node3Text.Text)).Wait();
                        break;

                    case "Node4":
                        _vm._deviceManager.SetAirAudioBand4GainAsync(_vm.CurrentDeviceInfo!.ID.ToString(), int.Parse(Node4Text.Text)).Wait();
                        break;

                    case "Node5":
                        _vm._deviceManager.SetAirAudioBand5GainAsync(_vm.CurrentDeviceInfo!.ID.ToString(), int.Parse(Node5Text.Text)).Wait();
                        break;
                }

                currentNode.ReleaseMouseCapture();

                // 更換未選取的圖片
                currentNode.Source = new BitmapImage(new Uri("pack://application:,,,/DDPM.UI.Common;component/Resources/Headset_Dot.png"));

                // 隱藏數值
                ShowNodeValue(currentNode, false);

                currentNode = null;

                // 更新陰影效果
                //UpdateShadowVisibility();
            }
        }

        /// <summary>
        /// Show Node Value
        /// </summary>
        /// <param name="node">Image</param>
        /// <param name="show">True or False</param>
        private void ShowNodeValue(Image node, bool show)
        {
            Border textBackground = null;
            if (node == Node1)
                textBackground = Node1TextBackground;
            else if (node == Node2)
                textBackground = Node2TextBackground;
            else if (node == Node3)
                textBackground = Node3TextBackground;
            else if (node == Node4)
                textBackground = Node4TextBackground;
            else if (node == Node5)
                textBackground = Node5TextBackground;

            if (textBackground != null)
            {
                textBackground.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
                if (show)
                {
                    UpdateNodeValuePosition(node);
                }
            }
        }

        /// <summary>
        /// Update Node Value Position
        /// </summary>
        /// <param name="node">Image</param>
        private void UpdateNodeValuePosition(Image node)
        {
            Border textBackground = null;
            TextBlock textBlock = null;
            if (node == Node1)
            {
                textBackground = Node1TextBackground;
                textBlock = Node1Text;
            }
            else if (node == Node2)
            {
                textBackground = Node2TextBackground;
                textBlock = Node2Text;
            }
            else if (node == Node3)
            {
                textBackground = Node3TextBackground;
                textBlock = Node3Text;
            }
            else if (node == Node4)
            {
                textBackground = Node4TextBackground;
                textBlock = Node4Text;
            }
            else if (node == Node5)
            {
                textBackground = Node5TextBackground;
                textBlock = Node5Text;
            }

            if (textBackground != null && textBlock != null)
            {
                Canvas.SetLeft(textBackground, Canvas.GetLeft(node) - (textBackground.Width - node.Width) / 2);
                Canvas.SetTop(textBackground, Canvas.GetTop(node) - textBackground.Height);

                // 計算並顯示介於 4 到 -6 之間的值
                double minValue = 0;
                double maxValue = 150;
                double minOutput = -6;
                double maxOutput = 5;

                double normalizedValue = maxOutput - ((Canvas.GetTop(node) - minValue) / (maxValue - minValue) * (maxOutput - minOutput));
                normalizedValue = Convert.ToInt16(Math.Floor(normalizedValue));
                if (normalizedValue >= 5)
                    normalizedValue = 4;
                textBlock.Text = normalizedValue.ToString("0");
            }
        }

        /// <summary>
        /// Calculate point position to draw curve
        /// </summary>
        private void UpdateCurve()
        {
            Point node1Position = new Point(Canvas.GetLeft(Node1) + Node1.Width / 2, Canvas.GetTop(Node1) + Node1.Height / 2);
            Point node2Position = new Point(Canvas.GetLeft(Node2) + Node2.Width / 2, Canvas.GetTop(Node2) + Node2.Height / 2);
            Point node3Position = new Point(Canvas.GetLeft(Node3) + Node3.Width / 2, Canvas.GetTop(Node3) + Node3.Height / 2);
            Point node4Position = new Point(Canvas.GetLeft(Node4) + Node4.Width / 2, Canvas.GetTop(Node4) + Node4.Height / 2);
            Point node5Position = new Point(Canvas.GetLeft(Node5) + Node5.Width / 2, Canvas.GetTop(Node5) + Node5.Height / 2);

            CurvePathFigure.StartPoint = node1Position;
            //ShadowPathFigure.StartPoint = node1Position;

            Segment1.Point1 = new Point((node1Position.X + node2Position.X) / 2, node1Position.Y);
            Segment1.Point2 = new Point((node1Position.X + node2Position.X) / 2, node2Position.Y);
            Segment1.Point3 = node2Position;

            //ShadowSegment1.Point1 = Segment1.Point1;
            //ShadowSegment1.Point2 = Segment1.Point2;
            //ShadowSegment1.Point3 = Segment1.Point3;

            Segment2.Point1 = new Point((node2Position.X + node3Position.X) / 2, node2Position.Y);
            Segment2.Point2 = new Point((node2Position.X + node3Position.X) / 2, node3Position.Y);
            Segment2.Point3 = node3Position;

            //ShadowSegment2.Point1 = Segment2.Point1;
            //ShadowSegment2.Point2 = Segment2.Point2;
            //ShadowSegment2.Point3 = Segment2.Point3;

            Segment3.Point1 = new Point((node3Position.X + node4Position.X) / 2, node3Position.Y);
            Segment3.Point2 = new Point((node3Position.X + node4Position.X) / 2, node4Position.Y);
            Segment3.Point3 = node4Position;

            //ShadowSegment3.Point1 = Segment3.Point1;
            //ShadowSegment3.Point2 = Segment3.Point2;
            //ShadowSegment3.Point3 = Segment3.Point3;

            Segment4.Point1 = new Point((node4Position.X + node5Position.X) / 2, node4Position.Y);
            Segment4.Point2 = new Point((node4Position.X + node5Position.X) / 2, node5Position.Y);
            Segment4.Point3 = node5Position;

            //ShadowSegment4.Point1 = Segment4.Point1;
            //ShadowSegment4.Point2 = Segment4.Point2;
            //ShadowSegment4.Point3 = Segment4.Point3;
        }

        private void UpdateShadowVisibility()
        {
            //double y1 = Canvas.GetTop(Node1);
            //double y2 = Canvas.GetTop(Node2);
            //double y3 = Canvas.GetTop(Node3);
            //double y4 = Canvas.GetTop(Node4);
            //double y5 = Canvas.GetTop(Node5);

            //if (y1 == y2 && y2 == y3 && y3 == y4 && y4 == y5)
            //{
            //    ShadowPath.Visibility = Visibility.Collapsed;
            //}
            //else
            //{
            //    ShadowPath.Visibility = Visibility.Visible;
            //    UpdateShadowPath();
            //}
        }

        private void UpdateShadowPath()
        {
            //Point node1Position = new Point(Canvas.GetLeft(Node1) + Node1.Width / 2, Canvas.GetTop(Node1) + Node1.Height / 2);
            //Point node2Position = new Point(Canvas.GetLeft(Node2) + Node2.Width / 2, Canvas.GetTop(Node2) + Node2.Height / 2);
            //Point node3Position = new Point(Canvas.GetLeft(Node3) + Node3.Width / 2, Canvas.GetTop(Node3) + Node3.Height / 2);
            //Point node4Position = new Point(Canvas.GetLeft(Node4) + Node4.Width / 2, Canvas.GetTop(Node4) + Node4.Height / 2);
            //Point node5Position = new Point(Canvas.GetLeft(Node5) + Node5.Width / 2, Canvas.GetTop(Node5) + Node5.Height / 2);

            //ShadowPathFigure.StartPoint = node1Position;

            //ShadowSegment1.Point1 = new Point((node1Position.X + node2Position.X) / 2, node1Position.Y);
            //ShadowSegment1.Point2 = new Point((node1Position.X + node2Position.X) / 2, node2Position.Y);
            //ShadowSegment1.Point3 = node2Position;

            //ShadowSegment2.Point1 = new Point((node2Position.X + node3Position.X) / 2, node2Position.Y);
            //ShadowSegment2.Point2 = new Point((node2Position.X + node3Position.X) / 2, node3Position.Y);
            //ShadowSegment2.Point3 = node3Position;

            //ShadowSegment3.Point1 = new Point((node3Position.X + node4Position.X) / 2, node3Position.Y);
            //ShadowSegment3.Point2 = new Point((node3Position.X + node4Position.X) / 2, node4Position.Y);
            //ShadowSegment3.Point3 = node4Position;

            //ShadowSegment4.Point1 = new Point((node4Position.X + node5Position.X) / 2, node4Position.Y);
            //ShadowSegment4.Point2 = new Point((node4Position.X + node5Position.X) / 2, node5Position.Y);
            //ShadowSegment4.Point3 = node5Position;

            //LineSegment shadowLine1 = new LineSegment(new Point(node5Position.X, 300), true);
            //LineSegment shadowLine2 = new LineSegment(new Point(node1Position.X, 300), true);

            //PathFigure shadowFigure = new PathFigure
            //{
            //    StartPoint = node1Position,
            //    Segments = new PathSegmentCollection { ShadowSegment1, ShadowSegment2, ShadowSegment3, ShadowSegment4, shadowLine1, shadowLine2 }
            //};

            //ShadowPathGeometry.Figures.Clear();
            //ShadowPathGeometry.Figures.Add(shadowFigure);
        }

        private void InitializeNodePositions()
        {
            double initialTop = 60; // 初始位置
            Canvas.SetTop(Node1, initialTop);
            Canvas.SetTop(Node2, initialTop);
            Canvas.SetTop(Node3, initialTop);
            Canvas.SetTop(Node4, initialTop);
            Canvas.SetTop(Node5, initialTop);

            UpdateCurve();
            UpdateShadowVisibility();
        }

        /// <summary>
        /// Set Node Value (from external input)
        /// </summary>
        /// <param name="node">Image</param>
        /// <param name="value">Value between 4 to -6</param>
        public void SetNodeValue(Image node, double value)
        {
            if (value < -6 || value > 4)
            {
                return;//throw new ArgumentOutOfRangeException(nameof(value), "Value must be between 4 and -6");
            }

            double minValue = 0;
            double maxValue = 150;
            double minOutput = -6;
            double maxOutput = 4;

            double newY = minValue + (maxValue - minValue) * (maxOutput - value) / (maxOutput - minOutput);

            Canvas.SetTop(node, newY);
            UpdateNodeValuePosition(node);
            UpdateCurve();
            //UpdateShadowVisibility();
        }


        //Elsa add for tooltip issue fix
        private double GetScreenScaleX()
        {
            var source = PresentationSource.FromVisual(this);
            if (source?.CompositionTarget != null)
            {
                return source.CompositionTarget.TransformToDevice.M11;
            }
            return 1;
        }

        //Elsa add for tooltip issue fix
        private void toolTip_Opened(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.ToolTip? target = sender as System.Windows.Controls.ToolTip;
            if (target == null)
                return;
            double screenScaleX = GetScreenScaleX();
            Window mainWindow = System.Windows.Application.Current.MainWindow;
            double winRightX = (mainWindow.Left + mainWindow!.ActualWidth) * screenScaleX;
            double mousepositionX = System.Windows.Forms.Cursor.Position.X;
            double mouseaddtooltip = mousepositionX + target.ActualWidth * screenScaleX;
            if (winRightX > mouseaddtooltip)
            {
                target.HorizontalOffset = 2;
            }
            else
            {
                target.HorizontalOffset = -1 * target.ActualWidth + 14;
            }
        }
    }

    public class CollaborationCheckedToVisibilityConverter : IValueConverter
    {
        public HeadsetViewModel ViewModel { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isChecked)
            {
                //For Debug Test
                //if (isChecked && ViewModel.modelTest == "WL7024")
                //{
                //    return parameter?.ToString() == "MicNoiseCancellationPageShow" ? Visibility.Visible : Visibility.Collapsed;
                //}
                //else if (isChecked && ViewModel.modelTest != "WL7024")
                //{
                //    return parameter?.ToString() == "MicNoiseCancellationFewPageShow" ? Visibility.Visible : Visibility.Collapsed;
                //}
                if (isChecked && ViewModel.Model == "WL7024")
                {
                    return parameter?.ToString() == "MicNoiseCancellationPageShow" ? Visibility.Visible : Visibility.Collapsed;
                }
                else if (isChecked && ViewModel.Model != "WL7024")
                {
                    return parameter?.ToString() == "MicNoiseCancellationFewPageShow" ? Visibility.Visible : Visibility.Collapsed;
                }
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ValueToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return Visibility.Collapsed;

            int sliderValue = (int)value;
            int targetValue = int.Parse((string)parameter);

            return sliderValue == targetValue ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class InverseBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolean)
            {
                return !boolean;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolean)
            {
                return !boolean;
            }
            return false;
        }
    }

    public class BooleanToInverseForegroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool booleanValue)
            {
                // 如果為 true，則回傳灰色，否則回傳白色
                return booleanValue ? Brushes.Gray : Brushes.White;
            }
            return Brushes.White;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BooleanToForegroundConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Any())
            {
                if (bool.Parse(values[0].ToString()) == true)
                {
                    return Brushes.Gray;
                }
                else
                {
                    if (values[1] == DependencyProperty.UnsetValue)
                        return Brushes.Gray;

                    if (bool.Parse(values[1].ToString()) == true)
                        return Brushes.White;
                    else
                        return Brushes.Black;
                }
            }
            else return Brushes.Gray;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}