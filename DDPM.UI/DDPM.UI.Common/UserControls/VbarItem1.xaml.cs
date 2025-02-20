using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using UserControl = System.Windows.Controls.UserControl;

namespace DDPM.UI.Common.UserControls
{
    /// <summary>
    /// Interaction logic for VbarItem1.xaml
    /// </summary>
    public partial class VbarItem1 : UserControl
    {
        #region Ctor

        public VbarItem1()
        {
            InitializeComponent();
            //DataContext = this;
        }

        #endregion Ctor

        #region UserControl Content - Text and IconTemplate

        public int Index
        {
            get { return (int)GetValue(IndexProperty); }
            set { SetValue(IndexProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Index.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IndexProperty =
            DependencyProperty.Register("Index", typeof(int), typeof(VbarItem1), new PropertyMetadata(0));

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Text.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(VbarItem1), new PropertyMetadata(""));

        public ControlTemplate IconTemplate
        {
            get { return (ControlTemplate)GetValue(IconTemplateProperty); }
            set { SetValue(IconTemplateProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IconTemplate.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IconTemplateProperty =
            DependencyProperty.Register("IconTemplate", typeof(ControlTemplate), typeof(VbarItem1));

        public ImageSource? IconImage
        {
            get { return (ImageSource)GetValue(IconImageProperty); }
            set { SetValue(IconImageProperty, value); }
        }

        public bool NoCanvasIcon
        {
            get { return (bool)GetValue(NoCanvasIconProperty); }
            set { SetValue(NoCanvasIconProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsLocked.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NoCanvasIconProperty =
            DependencyProperty.Register("NoCanvasIcon", typeof(bool), typeof(VbarItem1), new PropertyMetadata(true));

        private Canvas? _canvas;
        public Canvas? IconCanvas {
            get { return _canvas; } 
            set
            {
                _canvas = value;
                IconCanvasContent.Content = value;
                if (value != null)
                    NoCanvasIcon = false;
            }
        }

        // Using a DependencyProperty as the backing store for IconImage.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IconImageProperty =
            DependencyProperty.Register("IconImage", typeof(ImageSource), typeof(VbarItem1), new PropertyMetadata(null));

        #endregion UserControl Content - Text and IconTemplate

        #region Flags and States

        /// <summary>
        /// Default is true, need owner set this flag to false to leave the Landing mode.
        /// </summary>
        public bool IsLandingMode
        {
            get { return (bool)GetValue(IsLandingModeProperty); }
            set
            {
                SetValue(IsLandingModeProperty, value);
                if (value)
                {
                    bdOuter.Width = 212;
                }
                else
                {
                    bdOuter.Width = 64;
                }
            }
        }

        // Using a DependencyProperty as the backing store for IsLandingMode.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsLandingModeProperty =
            DependencyProperty.Register("IsLandingMode", typeof(bool), typeof(VbarItem1), new PropertyMetadata(true));

        /// <summary>
        /// Default is false, need owner to set this flag to enter Selected state.
        /// Owner may need to lisent ClickCommand and determine if the VbarItem will change to Selected state.
        /// </summary>
        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            set
            {
                SetValue(IsSelectedProperty, value);
            }
        }

        // Using a DependencyProperty as the backing store for IsSelected.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register("IsSelected", typeof(bool), typeof(VbarItem1), new PropertyMetadata(false));

        #region RWD by Derek
        public void AutoReverse()
        {
            Hover.Storyboard.AutoReverse = true; //自动展开又回去
        }
        public void CompleteStory()
        {
            bdOuter.Width = 212;
        }

        public void ResetStory()
        {
            bdOuter.Width = 64;
        }
        #endregion

        //Robert_Lin, 2024-9-25 Locked state
        public bool IsLocked
        {
            get { return (bool)GetValue(IsLockedProperty); }
            set { SetValue(IsLockedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsLocked.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsLockedProperty =
            DependencyProperty.Register("IsLocked", typeof(bool), typeof(VbarItem1), new PropertyMetadata(false));


        #endregion Flags and States

        #region Events / Commands

        /// <summary>
        /// Binding or add ReplayCommand to handle the click command.
        /// </summary>
        public ICommand? ClickCommand
        {
            get { return (ICommand)GetValue(ClickCommandProperty); }
            set { SetValue(ClickCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ClickCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ClickCommandProperty =
            DependencyProperty.Register("ClickCommand", typeof(ICommand), typeof(VbarItem1));

        #endregion Events / Commands

        #region Internal UI event handlers

        /// <summary>
        /// Entering hover state with conditions:
        /// 1 In Disabled state => ignore this event, do nothing
        /// 2 In Landing mode => Change to "LandingHover" state which will show a hover border.
        /// 3 In Default mode => Change to "Hover" state which the bdOuter.Width will become Expand mode.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void rootGrid_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (!IsEnabled)
            {
                return;
            }
            if (IsLandingMode)
            {
                if (IsLocked)
                {
                    VisualStateManager.GoToState(this, "LandingLockHover", false);
                }
                else
                {
                    VisualStateManager.GoToState(this, "LandingHover", false);
                }
            }
            else
            {
                if (IsLocked)
                {
                    VisualStateManager.GoToState(this, "LockHover", false);
                }
                else
                {
                    VisualStateManager.GoToState(this, "Hover", false);
                }
            }
        }

        /// <summary>
        /// Return back to non-hover state.
        /// 1 Disabled => do nothing.
        /// 2 Landing mode => Rmove the hovering border (LandingDefault state)
        /// 3 Default mode => Remove the hovering border (Default state)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void rootGrid_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (!IsEnabled)
                return;

            if (IsLandingMode)
            {
                if (IsLocked)
                {
                    VisualStateManager.GoToState(this, "LandingLock", false);
                }
                else
                {
                    VisualStateManager.GoToState(this, "LandingDefault", false);

                }
            }
            else
            {
                VisualStateManager.GoToState(this, "Default", false);
            }
        }

        /// <summary>
        /// When user clicking on the VbarItem, will notify owner through ClickCommand.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void rootGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!IsEnabled)
            {
                return;
            }
            if (IsLocked)
            {
                return;
            }
            if (ClickCommand != null)
            {
                ClickCommand.Execute(this);
            }
        }

        #endregion Internal UI event handlers

        #region Test

        //Workarround method to fix issue
        // When cursor is hovering on a VBbarItem, the bdOuter.Width=212
        // At this moment, the DDC/CI Off event is received, and DispayPage will
        // Set the VbarItem1 to Disabled.
        // The VbarItem1 change to Disabled state however its bdOuter.Width does not
        // change to 64, it is because we are still in "Hover" state
        // This workaround method let owner call us to leave from Hover state
        public void LeaveHoverState()
        {
            VisualStateManager.GoToState(this, "Default", false);
        }

        #endregion Test

        private void UserControl_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (!IsEnabled)
                {
                    return;
                }
                if (ClickCommand != null)
                {
                    ClickCommand.Execute(this);
                }
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            // Set the input method to English for the entire UserControl
            InputMethod.SetPreferredImeState(this, InputMethodState.Off);

        }
    }
}