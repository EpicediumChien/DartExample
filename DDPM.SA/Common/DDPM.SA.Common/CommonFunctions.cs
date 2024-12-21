using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DDPM.SA.Common
{
    //Robert_Lin, 2024-12-6
    /// <summary>
    /// The common static functions/methods for easy management and maintainace
    /// </summary>
    public static class CommonFunctions
    {
        /// <summary>
        /// Get the screen scale of primary screen when your app startup.
        /// The scale will not be updated when the screen scale changed at runtime.
        /// You need this scale to calculate the correct position/size if you are using
        /// System.Window.Forms.Screen to get size/position.
        /// </summary>
        /// <returns>Screen scale (>= 1.0), if fail to get, it return 1.</returns>
        public static double GetDpiX()
        {
            try
            {
                var dpiXProperty = typeof(SystemParameters).GetProperty("DpiX", BindingFlags.NonPublic | BindingFlags.Static);
                if (dpiXProperty != null)
                {
                    var varX = (int)dpiXProperty.GetValue(null, null);
                    double dpiX = (double)varX / (double)96;
                    return dpiX;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("GetDpiX causes an exception: " + ex.Message);
            }
            return 1.000;
        }

     }
}
