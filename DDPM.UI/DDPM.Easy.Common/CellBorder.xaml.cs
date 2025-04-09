using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace DDPM.Easy.Common
{
    /// <summary>
    /// Interaction logic for CellBorder.xaml
    /// </summary>
    public partial class CellBorder : UserControl, IDisposable
    {
        private static double _screenScale = -1;
        private bool _isDisposed = false;


        public CellBorder()
        {
            InitializeComponent();

            if (_screenScale < 0)
            {
                var dpiXProperty = typeof(SystemParameters).GetProperty("DpiX", BindingFlags.NonPublic | BindingFlags.Static);
                if (dpiXProperty != null)
                {
                    var varX = (int)dpiXProperty.GetValue(null, null);
                    double dpiX = (double)varX / (double)96;
                    if (dpiX >= 1.0000)
                        _screenScale = dpiX;
                }
            }

            #region Em Use

            AllowDrop = true;
            this.Drop += OnDrop;
            this.Unloaded += OnUnloaded;

            #endregion 
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            this.Drop -= OnDrop;
            this.Unloaded -= OnUnloaded;

            //Derek 2025/03/28
            ReleaseResource();
        }

        private void ReleaseResource()
        {
            SetValue(BorderBrushProperty, null);
            SetValue(BkBrushProperty, null);

            if (_cellAppInfo != null)
            {
                foreach (var item in _cellAppInfo)
                {
                    item.Value.Image = null;
                    item.Value.Cell = null;
                    CellAppData? ca = item.Value as CellAppData;
                    ca = null;
                }

                _cellAppInfo?.Clear();
                _cellAppInfo = null;
            }

            //GC.Collect();
            //GC.WaitForPendingFinalizers();
        }

        private string _cellName = "";
        public string CellName
        {
            get 
            {
                //return (string)GetValue(CellNameProperty);
                return _cellName;
            }
            set 
            {
                SetValue(CellNameProperty, value); 
                _cellName = value;
            }
        }

        // Using a DependencyProperty as the backing store for CellName.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CellNameProperty =
            DependencyProperty.Register("CellName", typeof(string), typeof(CellBorder), new PropertyMetadata(""));


        //Robert_Lin 2025-3-20 fix CS0108 'CellBorder.BorderThicknessProperty' hides inherited member 'Control.BorderThicknessProperty'. Use the new keyword if hiding was intended.
        public new Thickness BorderThickness
        {
            get { return (Thickness)GetValue(BorderThicknessProperty); }
            set { SetValue(BorderThicknessProperty, value); }
        }

        //Robert_Lin 2025-3-20 fix CS0108 'CellBorder.BorderThicknessProperty' hides inherited member 'Control.BorderThicknessProperty'. Use the new keyword if hiding was intended.
        // Using a DependencyProperty as the backing store for BorderThickness.  This enables animation, styling, binding, etc...
        public static new readonly DependencyProperty BorderThicknessProperty =
            DependencyProperty.Register("BorderThickness", typeof(Thickness), typeof(CellBorder), new PropertyMetadata(new Thickness(0)));



        public Brush BorderBrush
        {
            get { return (Brush)GetValue(BorderBrushProperty); }
            set { SetValue(BorderBrushProperty, value); }
        }

        // Using a DependencyProperty as the backing store for BorderBrush.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BorderBrushProperty =
            DependencyProperty.Register("BorderBrush", typeof(Brush), typeof(CellBorder), new PropertyMetadata(new SolidColorBrush(Colors.Transparent)));



        /// <summary>
        /// Unused proprety, do not use and UnitTest
        /// Use Radius instead
        /// </summary>
        public CornerRadius CornerRadius
        {
            get { return (CornerRadius)GetValue(CornerRadiusProperty); }
            set { SetValue(CornerRadiusProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CornerRadius.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.Register("CornerRadius", typeof(CornerRadius), typeof(CellBorder), new PropertyMetadata(new CornerRadius(0)));




        public CornerRadius Radius
        {
            get { return (CornerRadius)GetValue(RadiusProperty); }
            set { SetValue(RadiusProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Radius.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RadiusProperty =
            DependencyProperty.Register("Radius", typeof(CornerRadius), typeof(CellBorder), new PropertyMetadata(new CornerRadius(0)));



        public Brush BkBrush
        {
            get { return (Brush)GetValue(BkBrushProperty); }
            set { SetValue(BkBrushProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Background.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BkBrushProperty =
            DependencyProperty.Register("BkBrush", typeof(Brush), typeof(CellBorder), new PropertyMetadata(new SolidColorBrush(Colors.Transparent)));


        private bool _isHover = false;

        public bool IsHover
        {
            get { return (bool)GetValue(IsHoverProperty); }
            set 
            { 
                SetValue(IsHoverProperty, value); 
                _isHover = value;
            }
        }

        // Using a DependencyProperty as the backing store for IsHover.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsHoverProperty =
            DependencyProperty.Register("IsHover", typeof(bool), typeof(CellBorder), new PropertyMetadata(false));


        public Rect rect { get; set; } = new Rect();
        public Rect rcRatio { get; set; } = new Rect();

        public Border Border { get { return bd; } }

        public void Dispatcher_SetIsHover(bool isHover)
        {
            if (_isHover != isHover)
            {
                //this.Dispatcher.Invoke(() =>
                //{
                //    IsHover = isHover;
                //    //IsEnabled = !isHover;
                //});
                Dispatcher.BeginInvoke(delegate()
                {
                    IsHover = isHover;
                });
            }
        }

        //Unused, do not use and UnitTest
        public void AddChild(UIElement ele)
        {
            childGrid.Children.Add(ele);
        }

        #region Em Use

        public event EventHandler<Dictionary<int, CellAppData>>? DropOccurred;
        private void OnDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var cellBorder = sender as CellBorder;
                if (cellBorder != null)
                {
                    int cellNumber = cellBorder.CellNumber;
                    BitmapImage bitmapImage = new BitmapImage();
                    string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                    string filePath = files[0];
                    string fileName = System.IO.Path.GetFileName(filePath);

                    // MemoryImage
                    System.Drawing.Icon? icon = System.Drawing.Icon.ExtractAssociatedIcon(filePath);
                    if (icon != null)
                    {
                        using (var iconStream = new System.IO.MemoryStream())
                        {
                            icon.Save(iconStream);
                            iconStream.Seek(0, System.IO.SeekOrigin.Begin);
                            bitmapImage.BeginInit();
                            bitmapImage.StreamSource = iconStream;
                            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                            bitmapImage.EndInit();
                            //cellBorder.MemoryImage = bitmapImage;
                        }
                    }

                    //Robert_Lin 2025-3-30 unse new added method to release resources
                    //_cellAppInfo.Clear();
                    ClearCellAppInfos();
                    CellAppData appInfo = new CellAppData();
                    appInfo.Number = cellNumber;
                    appInfo.FileName = fileName;
                    appInfo.FilePath = filePath;
                    appInfo.Image = bitmapImage;
                    appInfo.Cell = cellBorder;
                    _cellAppInfo?.Add(cellNumber, appInfo);
                    if (_cellAppInfo != null)
                        DropOccurred?.Invoke(this, _cellAppInfo);
                }
            }
        }

        public static readonly DependencyProperty IsEmModeProperty = DependencyProperty.Register(
    "IsEmMode", typeof(bool), typeof(CellBorder), new PropertyMetadata(false));

        public bool IsEmMode
        {
            get => (bool)GetValue(IsEmModeProperty);
            set => SetValue(IsEmModeProperty, value);
        }

        public ImageSource MemoryImage
        {
            get => memoryImage.Source;
            set => memoryImage.Source = value;
        }

        public string MemoryText
        {
            get => memoryTB.Text;
            set => memoryTB.Text = value;
        }

        Dictionary<int, CellAppData>? _cellAppInfo = new Dictionary<int, CellAppData>();

        public int CellNumber
        {
            get { return (int)GetValue(CellNumberProperty); }
            set { SetValue(CellNumberProperty, value); }
        }

        public static readonly DependencyProperty CellNumberProperty =
            DependencyProperty.Register("CellNumber", typeof(int), typeof(CellBorder), new PropertyMetadata(0));

        public class CellAppData
        {
            public int Number { get; set; }

            public string FileName { get; set; }

            public string FilePath { get; set; }

            public BitmapImage? Image { get; set; }

            public CellBorder? Cell { get; set; }

            public CellAppData(int number, string fileName, string filePath, BitmapImage image, CellBorder cell)
            {
                Number = number;
                FileName = fileName;
                FilePath = filePath;
                Image = image;
                Cell = cell;
            }

            public CellAppData() { }
        }

        //Robert_Lin 2025-3-30 added to release resources
        private void ClearCellAppInfos()
        {
            if (_cellAppInfo == null)
                return;

            foreach (var item in _cellAppInfo)
            {
                item.Value.Image = null;
                item.Value.Cell = null;
            }
            _cellAppInfo.Clear();
        }
        #endregion

        private void UserControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is bool)
            {
                bool isVisible = (bool)e.NewValue;
                if (!isVisible)
                    return;
            }
            if ((ActualWidth == 0) || (ActualHeight == 0))
                return;

            if (_screenScale < 0)
                return;

            System.Windows.Point ptTopLeft = PointToScreen(new System.Windows.Point(0, 0));
            double w = ActualWidth * _screenScale;
            double h = ActualHeight * _screenScale;
            rect = new Rect(ptTopLeft.X, ptTopLeft.Y, w, h);
        }

        #region Dispose and Destructor
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    // 釋放託管資源
                    ClearCellAppInfos();

                }

                // 釋放非託管資源
                //if (unmanagedResource != IntPtr.Zero)
                //{
                //    // 釋放資源
                //    unmanagedResource = IntPtr.Zero;
                //}

                _isDisposed = true;
            }
        }
        ~CellBorder()
        {
            Dispose(false);
        }
        #endregion

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //Reference to [https://stackoverflow.com/questions/27729881/which-event-fires-after-all-items-are-loaded-and-shown-in-a-listview]
            //To get into RenderingDone() when UI is render done.
        //    Dispatcher.BeginInvoke(new Action(RenderingDone), System.Windows.Threading.DispatcherPriority.ContextIdle, null);
        }
        private void RenderingDone()
        {
            System.Windows.Point ptTopLeft = PointToScreen(new System.Windows.Point(0, 0));
            double w=0, h = 0;
            Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            if (ActualWidth != 0)
            {
                w = ActualWidth * _screenScale;
            }
            else
            {
                w = DesiredSize.Width * _screenScale;
            }

            if (ActualHeight != 0)
            {
                h = ActualHeight * _screenScale;
            }
            else
            {
                h = DesiredSize.Height * _screenScale;
            }
     //       rect = new Rect(ptTopLeft.X, ptTopLeft.Y, w, h);
        }

    }
}
