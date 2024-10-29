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

namespace DDPM.Easy.Common
{
    /// <summary>
    /// Interaction logic for CellBorder.xaml
    /// </summary>
    public partial class CellBorder : UserControl
    {
        public CellBorder()
        {
            InitializeComponent();
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



        public Thickness BorderThickness
        {
            get { return (Thickness)GetValue(BorderThicknessProperty); }
            set { SetValue(BorderThicknessProperty, value); }
        }

        // Using a DependencyProperty as the backing store for BorderThickness.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BorderThicknessProperty =
            DependencyProperty.Register("BorderThickness", typeof(Thickness), typeof(CellBorder), new PropertyMetadata(new Thickness(0)));



        public Brush BorderBrush
        {
            get { return (Brush)GetValue(BorderBrushProperty); }
            set { SetValue(BorderBrushProperty, value); }
        }

        // Using a DependencyProperty as the backing store for BorderBrush.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BorderBrushProperty =
            DependencyProperty.Register("BorderBrush", typeof(Brush), typeof(CellBorder), new PropertyMetadata(new SolidColorBrush(Colors.Transparent)));




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
                this.Dispatcher.Invoke(() =>
                {
                    IsHover = isHover;
                    //IsEnabled = !isHover;
                });
            }
        }

        public void AddChild(UIElement ele)
        {
            childGrid.Children.Add(ele);
        }
    }
}
