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
using Button = System.Windows.Controls.Button;

namespace DDPM.UI.Common.UserControls
{
    /// <summary>
    /// Follow steps 1a or 1b and then 2 to use this custom control in a XAML file.
    ///
    /// Step 1a) Using this custom control in a XAML file that exists in the current project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:DDPM.UI.Common.UserControls"
    ///
    ///
    /// Step 1b) Using this custom control in a XAML file that exists in a different project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:DDPM.UI.Common.UserControls;assembly=DDPM.UI.Common.UserControls"
    ///
    /// You will also need to add a project reference from the project where the XAML file lives
    /// to this project and Rebuild to avoid compilation errors:
    ///
    ///     Right click on the target project in the Solution Explorer and
    ///     "Add Reference"->"Projects"->[Browse to and select this project]
    ///
    ///
    /// Step 2)
    /// Go ahead and use your control in the XAML file.
    ///
    ///     <MyNamespace:GearButton/>
    ///
    /// </summary>
    public class GearButton : Button
    {
        static GearButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(GearButton), new FrameworkPropertyMetadata(typeof(GearButton)));
        }

        public GearButton()
        {
            this.Loaded += GearButton_Loaded;

        }
        private void GearButton_Loaded(object sender, RoutedEventArgs e)
        {
            _indication = GetTemplateChild("indication") as Border;
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            //SetOrangeDotVisible(true);
            //GlowEffect_Trigger();
        }
        #region Glow and Breathe effect

        private Storyboard? _storyboardGlow = null;
        private Border? _indication = null;
        

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
                if (_storyboardGlow != null && 
                    _indication != null)
                {
                    _indication.Visibility = Visibility.Visible;
                    _storyboardGlow.Begin(_indication);
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
                if (_indication == null)
                {
                   _indication = GetTemplateChild("indication") as Border;
                }

                if (_indication == null)
                    return;

                if (isVisible)
                {
                    _indication.Visibility = Visibility.Visible;
                }
                else
                {
                    _indication.Visibility = Visibility.Collapsed;
                }
            });
        }
        #endregion Glow and Breathe effect

    }
}
