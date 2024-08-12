using DDPM.UI.Common.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDPM.UI.Plugin.DdpmHomePlugin
{
    public class HomeDeviceSorter : IComparer
    {
        /// <summary>
        /// HomePage device list sort rules:
        /// 1 Sort by HomeDevice.DeviceCategory
        /// Order: 1.Display < 2.Webcam < 3.Keyboard < 4.Mouse < 5.Pen/Stylus < 
        ///        6.Audio headset < 7.sound-bar /Speakerphone/< 8.Dock
        /// 2 Grouping Webcam
        ///   If a Webcam is integrated with a Display, then it should be grouped with that Display.
        ///   That is, it should be next with that display.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public int Compare(object? x, object? y)
        {
            if (x == null && y == null) 
                return 0;

            HomeDevice homex = (HomeDevice)x;
            HomeDevice homey = (HomeDevice)y;
            return homex.SortOrder.CompareTo(homey.SortOrder);

            /* Robert_Lin, 2024-7-10 remarked
            HomeDevice homex = (HomeDevice)x;
            HomeDevice homey = (HomeDevice)y;

            int ret = 0;
            //If both are the same Category
            if (homex.DeviceCategory == homey.DeviceCategory)
            {
                //Sort by DisplayName
                ret = homex.DisplayName.CompareTo(homey.DisplayName);
            }

            //Check if both are grouped
            if (homex.MonitorIndexOfIntegratedPeripheral >= 0)
            {
                if ((homey.DeviceCategory == Common.eDeviceCategory.Display) &&
                    (homey.MonitorInfo.Index == homex.MonitorIndexOfIntegratedPeripheral)
            }
            int xValue = (int)homex.DeviceCategory;
            int yValue = (int)homey.DeviceCategory;

            //If they are not the same Category, then sort by Category
            if (xValue != yValue) 
            {
                ret = homex.DeviceCategory.CompareTo(homey.DeviceCategory);
            }
            else //The same Catrgory
            {
                //Sort by DisplayName
                ret = homex.DisplayName.CompareTo(homey.DisplayName);
            }
            return ret;
            */
        }
    }
}
