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
            _vm.DetectPageShow(_vm.Model);
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
            initialPositionNode1 = new Point(Canvas.GetLeft(Node1), Canvas.GetTop(Node1));
            initialPositionNode2 = new Point(Canvas.GetLeft(Node2), Canvas.GetTop(Node2));
            initialPositionNode3 = new Point(Canvas.GetLeft(Node3), Canvas.GetTop(Node3));

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
            isDragging = false;
            if (currentNode != null)
            {
                switch (currentNode.Name)
                {
                    case "Node1":
                        //_vm._deviceManager.SetBandsGain(int.Parse(Node1Text.Text), _vm.CurrentDeviceInfo!.ID, "band1gain").Wait();
                        break;

                    case "Node2":
                        //_vm._deviceManager.SetBandsGain(int.Parse(Node2Text.Text), _vm.CurrentDeviceInfo!.ID, "band2gain").Wait();
                        break;

                    case "Node3":
                        //_vm._deviceManager.SetBandsGain(int.Parse(Node3Text.Text), _vm.CurrentDeviceInfo!.ID, "band3gain").Wait();
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

            Segment1.Point1 = new Point((initialPositionNode1.X + node2Position.X) / 2, node1Position.Y);
            Segment1.Point2 = new Point((initialPositionNode1.X + node2Position.X) / 2, node2Position.Y);
            Segment1.Point3 = node2Position;

            Segment2.Point1 = new Point((node2Position.X + node3Position.X) / 2, node2Position.Y);
            Segment2.Point2 = new Point((node2Position.X + node3Position.X) / 2, node3Position.Y);
            Segment2.Point3 = node3Position;
        }
    }
}