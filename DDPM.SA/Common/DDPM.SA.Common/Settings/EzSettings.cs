using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDPM.SA.Common.Settings
{
    /// <summary>
    /// The settings of Display/Easy Arrange/Settings page.
    /// It will be a member of DDPMUserSettings class
    /// </summary>
    public class EzSettings
    {
        #region Default Values
        public const bool Default_IsWidthoutGap = true;
        public const bool Default_IsOnlyAllowWhenShiftKeyPressed = false;
        public const bool Default_IsSpanAcrossMultiMonitors = false;
        public const bool Default_IsAwsEnabled = false;

        #endregion Default Values
        /// <summary>
        /// The setting of "Allow app to split side by side without gap" in Easy Arrange / Settings page.
        /// The defualt value is True.
        /// </summary>
        public bool IsWidthoutGap { get; set; } = Default_IsWidthoutGap;

        /// <summary>
        /// The setting of "Only allow zone positioning when SHIFT is pressed" in Easy Arrange / Settings page.
        /// The defualt value is False.
        /// </summary>
        public bool IsOnlyAllowWhenShiftKeyPressed { get; set; } = Default_IsOnlyAllowWhenShiftKeyPressed;

        /// <summary>
        /// The setting of "Span across multiple monitors" in Easy Arrange / Settings page.
        /// The defualt value is False.
        /// </summary>
        public bool IsSpanAcrossMultiMonitors { get; set; } = Default_IsSpanAcrossMultiMonitors;

        /// <summary>
        /// The settings of "Application Window Snap" in Easy Arrange / Settings page.
        /// </summary>
        public bool IsAwsEnabled { get; set; } = Default_IsAwsEnabled;
    }
}
