using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDPM.UI.Common
{
    /// <summary>
    /// The constant strings of EventName for IConsole.RaiseEvent()
    /// </summary>
    public class ConsoleEventNames
    {
        #region MainWindow
        /// <summary>
        /// Bring the MainWindow to Z-order buttom with SetWindowPos Win32 API. Regietered by MainWindow.
        /// </summary>
        public const string MainWindow_SetToBottomWindow = "MainWindow.SetToBottomWindow";

        /// <summary>
        /// Activate MainWindow with Window.Activate(). Registered by MainWindow. 
        /// </summary>
        public const string MainWindow_Activate = "MainWindow.Activate";
        #endregion

        #region DisplayPlugin
        /// <summary>
        /// When user changed selected Monitor from the ComboBox of Display LandingPage. 
        /// Registered by ModuleOwner (DeviceBasePageViewModel)
        /// </summary>
        public const string Display_SelectedHomeDeviceChanged = "Display.SelectedHomeDeviceChanged";
        #endregion
    }
}
