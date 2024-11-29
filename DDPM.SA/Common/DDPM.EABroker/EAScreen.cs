using DDPM.SA.Common;
using Dell.Client.Framework.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using VcpCore.Common;
using static VcpCore.Common.User32;

namespace DDPM.EABroker
{
    /// <summary>
    /// Extend Forms.Screen for EA used. Add real resolution, scale, and attached MonitorInfos.
    /// </summary>
    public class EAScreen
    {
        #region Private members
        private readonly Screen _screen; //Forms.Screen
        private readonly DEVMODE _devMode; //Win32 DEVMODE of the _screen
        private double _logicalScale = 1.00; //The logical ScreenScale from primary screen
        private double _realScale = 1.000; //The real ScreenScale of this _screen.
        private List<MonitorInfo>? _attachedMonitors = null; //Attatched supported (Dell) monitors
        #endregion

        #region ctor
        
        private EAScreen(Screen scr)
        {
           
            _screen = scr;
            _devMode = GetDevMode(_screen.DeviceName);
            _logicalScale = GetLogicScale();
            RefreshRealScale();
        }
        #endregion ctor

        #region Functions for init/refresh
        /// <summary>
        /// Return the DEVMODE from the specified Screen.DeviceName
        /// </summary>
        /// <param name="deviceName"></param>
        /// <returns></returns>
        private static DEVMODE GetDevMode(string deviceName)
        {
            DEVMODE devMode = new DEVMODE();
            devMode.dmSize = (short)Marshal.SizeOf(typeof(DEVMODE));
            bool isOK = _EnumDisplaySettings(deviceName, ENUM_CURRENT_SETTINGS, ref devMode);
            return devMode;
        }

        /// <summary>
        /// Return the logical screen scale of the primary screen
        /// </summary>
        /// <returns></returns>
        private static double GetLogicScale()
        {
            var dpiXProperty = typeof(SystemParameters).GetProperty("DpiX", BindingFlags.NonPublic | BindingFlags.Static);
            if (dpiXProperty != null)
            {
                var varX = (int)dpiXProperty.GetValue(null, null);
                double dpiX = (double)varX / (double)96;
                return dpiX;
            }
            return 1.000;
        }

        /// <summary>
        /// Input _devMode, _screen and _logicalScale to calculate the _realScale; 
        /// </summary>
        private void RefreshRealScale()
        {
            _realScale = _logicalScale * Decimal.ToDouble(Math.Round(Decimal.Divide(_devMode.dmPelsWidth, _screen.Bounds.Width), 2));

        }
        #endregion Functions for init/refresh

        #region Properties
        public int RealWidth => _devMode.dmPelsWidth;
        public int RealHeight => _devMode.dmPelsHeight;
        public int ScreenFrequency => _devMode.dmDisplayFrequency;
        public double RealScale => _realScale;
        public Rectangle Bounds => _screen.Bounds;
        public Rectangle WorkingArea => _screen.WorkingArea;
        public string ScreenDeviceName => _screen.DeviceName;
        public bool IsPrimary => _screen.Primary;
        public bool HasAttachedMonitor
        {
            get 
            {
                if (_attachedMonitors != null)
                {
                    return _attachedMonitors.Count > 0;
                }
                return false;
            }
        }
        public Screen FormsScreen => _screen;
        #endregion

        #region GetEAScreens
        /// <summary>
        /// Convert monitor list to EAScreen list.
        /// Each EAScreen is a Forms.Screen with the Monitors that attached on it.
        /// </summary>
        /// <param name="monitors"></param>
        /// <returns></returns>
        public static List<EAScreen> GetEAScreens(List<MonitorInfo>? monitors)
        {
            List<EAScreen> listOut = new List<EAScreen>();
            foreach (Screen scr in Screen.AllScreens)
            {
                EAScreen eaScreen = new EAScreen(scr);

                if (monitors != null)
                {
                    //Find all monitors which have the same DeviceName (DisplayName)
                    eaScreen._attachedMonitors = monitors.FindAll(x => x.DisplayName.Equals(eaScreen.ScreenDeviceName, StringComparison.OrdinalIgnoreCase));

                }
                listOut.Add(eaScreen);
            }
            return listOut;
        }
        #endregion GetEAScreens

        #region Attached Monitors
        public List<MonitorInfo>? AttachedMonitors => _attachedMonitors;
        #endregion

        #region For SpanScreen.DetectSpanScreens()
        /// <summary>
        /// Check if another EAScreen can be spaned with me?
        /// </summary>
        /// <param name="another"></param>
        /// <param name="log">
        /// Optional, to write log for engineer debug</param>
        /// <returns></returns>
        public bool CanItSpanWithMe(EAScreen another, ILog? log=null)
        {
            //Conditions to return true:
            //1 another is not me
            //2 Both have same real resolution and scale
            //3 We are adjacent

            //1 another is not me : Compare with DeviceName
            if (ScreenDeviceName.Equals(another.ScreenDeviceName, StringComparison.OrdinalIgnoreCase))
                return false;

            //2 Both have same real resolution and scale
            if (RealWidth != another.RealWidth) return false;
            if (RealHeight != another.RealHeight) return false;
            if (RealScale != another.RealScale) return false;

            //3 We are adjacent
            //  When two screen (Rectangle) are fully adacent:
            //  Area of union = area of this + area of another
            Rectangle rcUnion = Rectangle.Union(this.Bounds, another.Bounds);
            int areaUnion = rcUnion.Width * rcUnion.Height;
            int areaThis = Bounds.Width * Bounds.Height;
            int areaAnother = another.Bounds.Width * another.Bounds.Height;
            int areaSum = areaThis + areaAnother;

            //Robert_Lin@wistron.com 2024-11-17
            //
            //under ideal conditions: areaUnion == areaSum
            //But when I testing with DDM v2.x it accepts a tolerance 
            //In real cases when you adjacent two differnet model monitors (same size)
            //They did have a litter of gaps, below is an exmaple:
            //   Screen1                 Screen 2
            // (-6400,-288)             (-3200,-278)
            //   +---------------------++---------------------+    What you see
            //   |                     ||                     |
            //   |                     ||                     |
            //   |     3200x1800       ||    3200x1800        |
            //   |                     ||                     |
            //   |                     ||                     |
            //   +---------------------++---------------------+
            //              (-3200,1512)              (0,1522)
            //
            //   Top:                                But the actual data in program
            //          -288 ----------+
            //                         |+------ -278
            //                      gap = 10 pixels
            //   Bottom:
            //                         ||
            //          1512 ----------+|
            //                          +------ 1522
            //                      gap = 10 pixels
            //
            //After some study with DDM2, we decide to set below crteria 
            //   IF (the delta of area(Union) - area(Sum)) < criteria ) will allow them to Span.
            //   Currently set the criteria to 40 * 3200;
            //
            int accepableDelta = 40 * 3200;
            int delta = Math.Abs(areaSum - areaUnion);
            bool returnValue = (delta <= accepableDelta);

            if (log != null)
            {
                log?.Info($"@CanItSpanWithMe() return {returnValue} == Delta({delta}) <= Criteria({accepableDelta}); ");
                log?.Info($"@CanItSpanWithMe() rects, This:({Bounds.Left},{Bounds.Top})-({Bounds.Right},{Bounds.Bottom}){Bounds.Width}x{Bounds.Height}, Area_This={areaThis}; Another:({another.Bounds.Left},{another.Bounds.Top})-({another.Bounds.Right},{another.Bounds.Bottom}){another.Bounds.Width}x{another.Bounds.Height}, Area_Another={areaAnother}");
                log?.Info($"@CanItSpanWithMe() calculate, Union:({rcUnion.Left},{rcUnion.Top})-({rcUnion.Right},{rcUnion.Bottom}){rcUnion.Width}x{rcUnion.Height}, Area_Union={areaUnion}; Area_Sum=Area_this+Area_Another={areaSum},Delta=ABS(Area_Union - Area_Sum)={delta})");
            }
            return returnValue;
        }
        #endregion For SpanScreen.DetectSpanScreens()

        #region Debug
        //Output format: "({x},{y}){w}x{h}"
        private string FormatRectangle4(Rectangle rect)
        {
            return $"({rect.Left},{rect.Top}){rect.Width}x{rect.Height}";
        }
        //Output format:
        // { "deviceName" (Primary): Bounds:(x,y)wxh, WorkingArea:(x,y)wxh, Resolution:wxh, Scale:1.25,
        //   Monitors:[{"model","serviceTag"},{"model"},{"serviceTag"}, ...]
        // } 
        // Bounds[x,y,wxh]
        //
        //
        public override string ToString()
        {
            string strOut = $"{{ \"{ScreenDeviceName}\" ";
            if (IsPrimary)
                strOut += "(Primary) ";
            strOut += $"Bounds:{FormatRectangle4(Bounds)}, WorkingArea:{FormatRectangle4(WorkingArea)}, ";
            strOut += $"Resolution:{RealWidth}x{RealHeight}, Scale:{RealScale}, ";

            if (HasAttachedMonitor)
            {
                strOut += "Monitors:[";
                foreach(MonitorInfo mi in AttachedMonitors)
                {
                    strOut += $" {{\"{mi.modelName}\",\"{mi.edid.ServiceTag}\"}},";
                }
                strOut = strOut.TrimEnd(',');
                strOut += "]";
            }
            else
            {
                strOut += "Monitors:[]";
            }
            strOut += "}";
            return strOut;
        }

        //Output format:
        //  EAScreens:[ {EAScreen1}, {EAScreen2], ...]
        public static string EAScreensToString(List<EAScreen> eaScreens)
        {
            string strOut = "EAScreens:[";
            foreach(EAScreen screen in eaScreens)
            {
                strOut += " ";
                strOut += screen.ToString();
                strOut += ",";
            }
            strOut = strOut.TrimEnd(',');
            strOut += " ]";
            return strOut;
        }
        //Output format:
        /*
        EAScreen : {
            Screen: { "DeviceName":"{deviceName}", "Bounds":"x,y,w,h", "WorkingArea":"x,y,w,h","Resolution":"w,h", "Scale":"{scale}" },
            Monitors: [  { "Model":"model", "ServiceTag": "serviceTag"}, { }, ... ]
        }
        */
        //public string ToJsonString()
        //{
        //    string strOut = "EAScreen:"
        //    return $""
        //}
        #endregion Debug
    }
}
