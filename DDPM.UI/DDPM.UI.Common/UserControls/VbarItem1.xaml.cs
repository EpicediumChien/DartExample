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
        #endregion

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

        // Using a DependencyProperty as the backing store for IconImage.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IconImageProperty =
            DependencyProperty.Register("IconImage", typeof(ImageSource), typeof(VbarItem1), new PropertyMetadata(null));

        #endregion

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
        #endregion

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

        #endregion

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
                VisualStateManager.GoToState(this, "LandingHover", false);
            }
            else
            {
                VisualStateManager.GoToState(this, "Hover", false);
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
                VisualStateManager.GoToState(this, "LandingDefault", false);
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
            if (ClickCommand != null)
            {
                ClickCommand.Execute(this);
            }
        }
        #endregion

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
        #endregion

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
    }
}
