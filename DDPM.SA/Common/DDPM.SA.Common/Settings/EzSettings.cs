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
        /// <summary>
        /// The setting of "Allow app to split side by side without gap" in Easy Arrange / Settings page.
        /// The defualt value is True.
        /// </summary>
        public bool IsWidthoutGap { get; set; } = true;

        /// <summary>
        /// The setting of "Only allow zone positioning when SHIFT is pressed" in Easy Arrange / Settings page.
        /// The defualt value is False.
        /// </summary>
        public bool IsOnlyAllowWhenShiftKeyPressed { get; set; } = false;

        /// <summary>
        /// The setting of "Span across multiple monitors" in Easy Arrange / Settings page.
        /// The defualt value is False.
        /// </summary>
        public bool IsSpanAcrossMultiMonitors { get; set; } = false;

        /// <summary>
        /// The settings of "Application Window Snap" in Easy Arrange / Settings page.
        /// </summary>
        public bool IsAwsEnabled { get; set; } = false;
    }
}
