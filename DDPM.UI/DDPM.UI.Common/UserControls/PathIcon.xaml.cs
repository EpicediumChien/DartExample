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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DDPM.UI.Common.UserControls
{
    /// <summary>
    /// Interaction logic for PathIcon.xaml
    /// </summary>
    public partial class PathIcon : System.Windows.Controls.UserControl
    {
        public PathIcon()
        {
            InitializeComponent();
        }



        public string PathData
        {
            get { return (string)GetValue(PathDataProperty); }
            set { SetValue(PathDataProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PathData.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PathDataProperty =
            DependencyProperty.Register("PathData", typeof(string), typeof(PathIcon), new PropertyMetadata(String.Empty));





        public System.Windows.Media.Brush PathFill
        {
            get { return (System.Windows.Media.Brush)GetValue(PathFillProperty); }
            set { SetValue(PathFillProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PathFill.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PathFillProperty =
            DependencyProperty.Register("PathFill", typeof(System.Windows.Media.Brush), typeof(PathIcon), new PropertyMetadata(System.Windows.Media.Brushes.White));




        public ICommand? ClickCommand
        {
            get { return (ICommand)GetValue(ClickCommandProperty); }
            set { SetValue(ClickCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ClickCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ClickCommandProperty =
            DependencyProperty.Register("ClickCommand", typeof(ICommand), typeof(PathIcon));




        public string TooltipText
        {
            get { return (string)GetValue(TooltipTextProperty); }
            set { SetValue(TooltipTextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TooltipText.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TooltipTextProperty =
            DependencyProperty.Register("TooltipText", typeof(string), typeof(PathIcon), new PropertyMetadata(String.Empty));



        private void Path_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (ClickCommand != null)
            {
                ClickCommand.Execute(this);
            }
        }

        #region Glow and Breathe effect
        Storyboard? _storyboardGlow = null;
        public void GlowEffect_Start()
        {
            //To make it running on UI thread
            Dispatcher.Invoke(() =>
            {
                if (_storyboardGlow == null)
                {
                    object oSB = TryFindResource("glowAnimation");
                    if (oSB == null)
                        return;

                    _storyboardGlow = oSB as Storyboard;
                }
                if (_storyboardGlow != null)
                {
                    _storyboardGlow.Begin(indication);
                    indication.Visibility = Visibility.Visible;
                }
            });
        }
        public void GlowEffect_Stop()
        {
            //To make it running on UI thread
            Dispatcher.Invoke(() =>
            {
                if (_storyboardGlow != null)
                {
                    indication.Visibility = Visibility.Collapsed;
                    _storyboardGlow.Stop();
                }
            });
        }
        #endregion
    }
}
