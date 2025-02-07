using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;

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
            e.Handled = true;
            if (ClickCommand != null)
            {
                ClickCommand.Execute(this);
            }
        }

        #region Glow and Breathe effect

        private Storyboard? _storyboardGlow = null;

        //Unused
        public void GlowEffect_Start()
        {
            //To make it running on UI thread
            //Dispatcher.Invoke(() =>
            //{
            //    if (_storyboardGlow == null)
            //    {
            //        object oSB = TryFindResource("glowAnimation");
            //        if (oSB == null)
            //            return;

            //        _storyboardGlow = oSB as Storyboard;
            //    }
            //    if (_storyboardGlow != null)
            //    {
            //        _storyboardGlow.Begin(indication);
            //        indication.Visibility = Visibility.Visible;
            //    }
            //});
        }

        //Unused
        public void GlowEffect_Stop()
        {
            //To make it running on UI thread
            //Dispatcher.Invoke(() =>
            //{
            //    if (_storyboardGlow != null)
            //    {
            //        indication.Visibility = Visibility.Collapsed;
            //        _storyboardGlow.Stop();
            //    }
            //});
        }

        /// <summary>
        /// Call this method to blinking for 2 sec when detect a new update added.
        /// After triggered, the OrangeDot will keep in "On" state.
        /// </summary>
        public void GlowEffect_Trigger()
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

        /// <summary>
        /// Call with (isVisible=false) when available updates count is changed from 1 => 0
        /// </summary>
        /// <param name="isVisible"></param>
        public void SetOrangeDotVisible(bool isVisible)
        {
            //To make it running on UI thread
            Dispatcher.Invoke(() =>
            {
                if (isVisible)
                {
                    indication.Visibility = Visibility.Visible;
                }
                else
                {
                    indication.Visibility = Visibility.Collapsed;
                }
            });
        }
        #endregion Glow and Breathe effect

        //Robert_Lin, 2025-1-6 Narrator, handling Enter key down
        private void UserControl_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (!IsEnabled)
                {
                    return;
                }
                e.Handled = true;
                if (ClickCommand != null)
                {
                    ClickCommand.Execute(this);
                }
            }
        }

        private void UserControl_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {

        }
    }
}
