using DDPM.UI.Common;
using DDPM.UI.Plugin.ViewModels;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using UserControl = System.Windows.Controls.UserControl;

namespace DDPM.UI.Module.SpeakerAudioPreset
{
    /// <summary>
    /// Interaction logic for SpeakerAudioPresetRightView.xaml
    /// </summary>
    public partial class SpeakerAudioPresetRightView : UserControl, INotifyPropertyChanged
    {
        private bool isDragging = false;
        private Image? currentNode;
        private Point clickPosition;

        public event PropertyChangedEventHandler PropertyChanged;

        private readonly SoundBarViewModel _vm;

        /// <summary>
        /// HeadsetViewModel data in
        /// </summary>
        /// <param name="vm">ViewModel</param>
        public SpeakerAudioPresetRightView(SoundBarViewModel vm)
        {
            InitializeComponent();
            _vm = vm;
            //InitializeAsync();
            _vm!.SoundbarSettingChanged += SoundbarSettingChanged;
            _vm.CheckSpeakerFunc();
            InitializeAsync();
            //_vm.Invoke_PleaseWaitAsync(_vm.Model, _vm).Wait();
        }
        ~SpeakerAudioPresetRightView()
        {
            if (DdpmCommonHelper.DeviceManagerSA != null)
            {
                _vm!.SoundbarSettingChanged -= SoundbarSettingChanged;
                _vm._log!.Info("[SpeakerAudioPresetRightView] ~SpeakerAudioPresetRightView ~~~~~~~~~~");
            }
        }
        private void SoundbarSettingChanged(object? sender, EventArgs e)
        {
            InitializeAsync();
        }

        private async void InitializeAsync()
        {
            _vm._log!.Info("[SpeakerAudioPresetRightView] Before Invoke_PleaseWaitAsync");
            //await _vm.Invoke_PleaseWaitAsync(_vm.Model, _vm);
            //_vm._log!.Info("[SpeakerAudioPresetRightView] After Invoke_PleaseWaitAsync");
            if (_vm.SpeakerInfoValueDTP.SpeakerProfile == _vm._default)// || _vm.SpeakerInfoValueDTP.SpeakerProfile == "")
            {
                if (_vm.IsDTPReady)
                {
                    _vm.SpeakerInfoValueDTP.SpeakerProfileName = _vm._deviceManager.GetProfileNameAsync(_vm.CurrentDeviceID.ToString()).Result ?? String.Empty;
                    if (_vm.SpeakerInfoValueDTP.SpeakerProfile == _vm._default)
                    {
                        _vm.SpeakerInfoValueDTP.SpeakerBass = _vm._deviceManager.GetBassAsync(_vm.CurrentDeviceID.ToString()).Result;
                        _vm.SpeakerInfoValueDTP.SpeakerMidRange = _vm._deviceManager.GetMidRangeAsync(_vm.CurrentDeviceID.ToString()).Result;
                        _vm.SpeakerInfoValueDTP.SpeakerTreble = _vm._deviceManager.GetTrebleAsync(_vm.CurrentDeviceID.ToString()).Result;
                    }
                    else
                    {
                        _vm.SpeakerInfoValueDTP.SpeakerBass = 0;
                        _vm.SpeakerInfoValueDTP.SpeakerMidRange = 0;
                        _vm.SpeakerInfoValueDTP.SpeakerTreble = 0;
                    }
                }
                else
                {
                    //DTH not support EQ read value
                    _vm.SpeakerInfoValueDTP.SpeakerBass = 0;
                    _vm.SpeakerInfoValueDTP.SpeakerMidRange = 0;
                    _vm.SpeakerInfoValueDTP.SpeakerTreble = 0;
                }

                if (_vm.SpeakerInfoValueDTP.SpeakerBass > 2 || _vm.SpeakerInfoValueDTP.SpeakerBass < -2)
                {
                    SetNodeValue(Node1, 0);
                    _vm._log!.Info($"[SpeakerAudioPresetRightView] SpeakerBass SetNodeValue ... Bass 00000");
                }
                else
                {
                    //_vm.SpeakerInfoValueDTP.SpeakerBass = _vm._deviceManager.GetBassAsync(_vm.CurrentDeviceID.ToString()).Result;
                    SetNodeValue(Node1, _vm.SpeakerInfoValueDTP.SpeakerBass);
                    _vm._log!.Info($"[SpeakerAudioPresetRightView] SpeakerBass SetNodeValue ... Bass {_vm.SpeakerInfoValueDTP!.SpeakerBass.ToString()}");
                }

                if (_vm.SpeakerInfoValueDTP.SpeakerMidRange > 2 || _vm.SpeakerInfoValueDTP.SpeakerMidRange < -2)
                {
                    SetNodeValue(Node2, 0);
                    _vm._log!.Info($"[SpeakerAudioPresetRightView] SpeakerMidRange SetNodeValue ... MidRange 00000");
                }
                else
                {
                    //_vm.SpeakerInfoValueDTP.SpeakerMidRange = _vm._deviceManager.GetMidRangeAsync(_vm.CurrentDeviceID.ToString()).Result;
                    SetNodeValue(Node2, _vm.SpeakerInfoValueDTP.SpeakerMidRange);
                    _vm._log!.Info($"[SpeakerAudioPresetRightView] SpeakerMidRange SetNodeValue ... MidRange {_vm.SpeakerInfoValueDTP!.SpeakerMidRange.ToString()}");
                }

                if (_vm.SpeakerInfoValueDTP.SpeakerTreble > 2 || _vm.SpeakerInfoValueDTP.SpeakerTreble < -2)
                {
                    SetNodeValue(Node3, 0);
                    _vm._log!.Info($"[SpeakerAudioPresetRightView] SpeakerTreble SetNodeValue ... Treble 00000");
                }
                else
                {
                    //_vm.SpeakerInfoValueDTP.SpeakerTreble = _vm._deviceManager.GetTrebleAsync(_vm.CurrentDeviceID.ToString()).Result;
                    SetNodeValue(Node3, _vm.SpeakerInfoValueDTP.SpeakerTreble);
                    _vm._log!.Info($"[SpeakerAudioPresetRightView] SpeakerTreble SetNodeValue ... Treble {_vm.SpeakerInfoValueDTP!.SpeakerTreble.ToString()}");
                }
            }
            _vm.CheckPresetsUI();
            _vm.CheckAudioSettingsUI();
        }

        /// <summary>
        /// Set Node Value (from external input)
        /// </summary>
        /// <param name="node">Image</param>
        /// <param name="value">Value between 4 to -6</param>
        public void SetNodeValue(Image node, double value)
        {
            if (value < -2 || value > 2)
            {
                _vm._log!.Info($"[SpeakerAudioPresetRightView] SetNodeValue ...... Value must be between 2 and -2");
                return;
            }

            double minValue = 0;
            double maxValue = 150;
            double minOutput = -2;
            double maxOutput = 2;

            double newY = minValue + (maxValue - minValue) * (maxOutput - value) / (maxOutput - minOutput);

            Canvas.SetTop(node, newY);
            UpdateNodeValuePosition(node);
            UpdateCurve();
        }

        private Point initialPositionNode1;
        private Point initialPositionNode2;
        private Point initialPositionNode3;

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

            // 儲存初始位置
            //initialPositionNode1 = new Point(Canvas.GetLeft(Node1), Canvas.GetTop(Node1));
            //initialPositionNode2 = new Point(Canvas.GetLeft(Node2), Canvas.GetTop(Node2));
            //initialPositionNode3 = new Point(Canvas.GetLeft(Node3), Canvas.GetTop(Node3));

            // 更換選取後的圖片
            currentNode.Source = new BitmapImage(new Uri("pack://application:,,,/DDPM.UI.Common;component/Resources/Headset_whitedot.png"));

            // 顯示數值
            ShowNodeValue(currentNode, true);
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
            _vm._isRestoreEnable = false;
            isDragging = false;
            if (currentNode != null)
            {
                switch (currentNode.Name)
                {
                    case "Node1":
                        _vm._log!.Info($"[SpeakerAudioPresetRightView] SetBassAsync ...... {Node1Text.Text}");
                        _vm._deviceManager.SetBassAsync(_vm.CurrentDeviceInfo!.ID.ToString(), int.Parse(Node1Text.Text));
                        _vm.SpeakerInfoValueDTP.SpeakerBass = int.Parse(Node1Text.Text);
                        break;

                    case "Node2":
                        _vm._log!.Info($"[SpeakerAudioPresetRightView] SetMidRangeAsync ...... {Node2Text.Text}");
                        _vm._deviceManager.SetMidRangeAsync(_vm.CurrentDeviceInfo!.ID.ToString(), int.Parse(Node2Text.Text));
                        _vm.SpeakerInfoValueDTP.SpeakerMidRange = int.Parse(Node2Text.Text);
                        break;

                    case "Node3":
                        _vm._log!.Info($"[SpeakerAudioPresetRightView] SetTrebleAsync ...... {Node3Text.Text}");
                        _vm._deviceManager.SetTrebleAsync(_vm.CurrentDeviceInfo!.ID.ToString(), int.Parse(Node3Text.Text));
                        _vm.SpeakerInfoValueDTP.SpeakerTreble = int.Parse(Node3Text.Text);
                        break;
                }

                currentNode.ReleaseMouseCapture();

                // 更換未選取的圖片
                currentNode.Source = new BitmapImage(new Uri("pack://application:,,,/DDPM.UI.Common;component/Resources/Headset_Dot.png"));

                // 隱藏數值
                ShowNodeValue(currentNode, false);

                currentNode = null;
            }
        }

        /// <summary>
        /// Show Node Value
        /// </summary>
        /// <param name="node">Image</param>
        /// <param name="show">True or False</param>
        private void ShowNodeValue(Image node, bool show)
        {
            Border? textBackground = null;
            if (node == Node1)
                textBackground = Node1TextBackground;
            else if (node == Node2)
                textBackground = Node2TextBackground;
            else if (node == Node3)
                textBackground = Node3TextBackground;

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

            if (textBackground != null && textBlock != null)
            {
                Canvas.SetLeft(textBackground, Canvas.GetLeft(node) - (textBackground.Width - node.Width) / 2);
                Canvas.SetTop(textBackground, Canvas.GetTop(node) - textBackground.Height);

                // 計算並顯示介於 2 到 -2 之間的值
                double minValue = 0;
                double maxValue = 150;
                double minOutput = -2;
                double maxOutput = 3;

                double normalizedValue = maxOutput - ((Canvas.GetTop(node) - minValue) / (maxValue - minValue) * (maxOutput - minOutput));
                normalizedValue = Convert.ToInt16(Math.Floor(normalizedValue));
                if (normalizedValue >= 3)
                    normalizedValue = 2;
                textBlock.Text = normalizedValue.ToString("0");
                //Debug.WriteLine("SetNodeValue normalizedValue : " + normalizedValue.ToString());
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

            CurvePathFigure.StartPoint = node1Position;

            //Segment1.Point1 = new Point((initialPositionNode1.X + node2Position.X) / 2, node1Position.Y);
            //Segment1.Point2 = new Point((initialPositionNode1.X + node2Position.X) / 2, node2Position.Y);
            //Segment1.Point3 = node2Position;

            Segment1.Point1 = new Point((node1Position.X + node2Position.X) / 2, node1Position.Y);
            Segment1.Point2 = new Point((node1Position.X + node2Position.X) / 2, node2Position.Y);
            Segment1.Point3 = node2Position;

            //Segment2.Point1 = new Point((node2Position.X + node3Position.X) / 2, node2Position.Y);
            //Segment2.Point2 = new Point((node2Position.X + node3Position.X) / 2, node3Position.Y);
            //Segment2.Point3 = node3Position;

            Segment2.Point1 = new Point((node2Position.X + node3Position.X) / 2, node2Position.Y);
            Segment2.Point2 = new Point((node2Position.X + node3Position.X) / 2, node3Position.Y);
            Segment2.Point3 = node3Position;
        }
    }
}