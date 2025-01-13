using System.Windows;
using Button = System.Windows.Controls.Button;

namespace DDPM.UI.Plugin.DdpmHomePlugin
{
    /// <summary>
    /// Follow steps 1a or 1b and then 2 to use this custom control in a XAML file.
    ///
    /// Step 1a) Using this custom control in a XAML file that exists in the current project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:DDPM.UI.Plugin.DdpmHomePlugin"
    ///
    ///
    /// Step 1b) Using this custom control in a XAML file that exists in a different project.
    /// Add this XmlNamespace attribute to the root element of the markup file where it is 
    /// to be used:
    ///
    ///     xmlns:MyNamespace="clr-namespace:DDPM.UI.Plugin.DdpmHomePlugin;assembly=DDPM.UI.Plugin.DdpmHomePlugin"
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
    ///     <MyNamespace:AddDeviceButton/>
    ///
    /// </summary>
    public class AddDeviceButton : Button
    {
        static AddDeviceButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AddDeviceButton), new FrameworkPropertyMetadata(typeof(AddDeviceButton)));
        }
    }
}
