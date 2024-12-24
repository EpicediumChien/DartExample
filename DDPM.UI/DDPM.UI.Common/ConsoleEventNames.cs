using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDPM.UI.Common
{
    /// <summary>
    /// The constant strings of EventName for IConsole.RaiseEvent()
    /// Example: Register an event handler
    ///   see DDPM.UI.Plugin.DdpmHomePlugin/DdpmHomePlugin.cs, AddIconsToMasthead( )
    /// Example: Raise an event
    ///   see DDPM.UI.Plugin.DdpmHomePlugin/DdpmHomePlugin.cs, OnGearIconClicked( )
    /// </summary>
    public class ConsoleEventNames
    {
        #region MainWindow
        /// <summary>
        /// Bring the MainWindow to Z-order buttom with SetWindowPos Win32 API. Regietered by MainWindow.
        /// </summary>
        public const string MainWindow_SetToBottomWindow = "MainWindow.SetToBottomWindow";

        /// <summary>
        /// Activate MainWindow with Window.Activate(). Registered by MainWindow and Webcamera plugin
        /// </summary>
        public const string MainWindow_Activate = "MainWindow.Activate";

        /// <summary>
        /// DeActivate MainWindow with Window.DeActivate(). Registered by Webcamera plugin. 
        /// </summary>
        public const string MainWindow_DeActivate = "MainWindow.DeActivate";

        /// <summary>
        /// Trigger from MainWindow, when DDPM move to a new position (position is changed)
        /// </summary>
        public const string MainWindow_MoveToNewPosition = "MainWindow.MoveToNewPosition";
        #endregion MainWindow

        #region DisplayPlugin
        /// <summary>
        /// When user changed selected Monitor from the ComboBox of Display LandingPage. 
        /// Registered by ModuleOwner (DeviceBasePageViewModel)
        /// </summary>
        public const string Display_SelectedHomeDeviceChanged = "Display.SelectedHomeDeviceChanged";
        #endregion DisplayPlugin

        #region Masthead Icons : AddDevice and GlobalSettings (gear icon)
        /// <summary>
        /// Request HomePlugin to load/show AddDevicePlugin. It will let AddDevice icon hide and GlobalSettings icon show.
        /// </summary>
        public const string Masthead_ShowAddDevicePlugin = "ShowAddDevicePlugin";

        /// <summary>
        /// Request HomePlugin to load/show SettingsPlugin. It will let GlobalSettings icon hide and AddDevice icon show.
        /// </summary>
        public const string Masthead_ShowSettingsPlugin = "ShowSettingsPlugin";

        /// <summary>
        /// Request the gear icon (GlobalSettings) starts the Glow effect.
        /// </summary>
        public const string Masthead_StartGlowEffectOnGearIcon = "StartGlowEffectOnGearIcon";

        /// <summary>
        /// Request the gear icon (GlobalSettings) stops the Glow effect.
        /// </summary>
        public const string Masthead_StopGlowEffectOnGearIcon = "StopGlowEffectOnGearIcon";

        /// <summary>
        /// Request to show/hide the AddDevice icon.
        /// var args = new EventManagerArgs();
        /// args.Tag = (bool) isShow; //true=Show, false=Hide
        /// (IConsole) _console.RaiseEvent(ConsoleEventNames.Masthead_ShowAddDeviceIcon, this, args);
        /// </summary>
        public const string Masthead_ShowAddDeviceIcon = "Masthead_ShowAddDeviceIcon";

        /// <summary>
        /// Request to show/hide the Settings icon.
        /// var args = new EventManagerArgs();
        /// args.Tag = (bool) isShow; //true=Show, false=Hide
        /// (IConsole) _console.RaiseEvent(ConsoleEventNames.Masthead_HideAddDeviceIcon, this, args);
        /// </summary>
        public const string Masthead_ShowSettingsIcon = "Masthead_ShowSettingsIcon";

        #endregion Masthead Icons : AddDevice and GlobalSettings (gear icon)

    }
}
