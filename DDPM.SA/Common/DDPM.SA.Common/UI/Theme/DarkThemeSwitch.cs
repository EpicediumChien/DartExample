using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace DDPM.SA.Common.UI
{
    public static partial class SACommonHelper
    {
        public static void SwitchToDarkMode() 
        {
            UpdateFreezable<SolidColorBrush>("Default_SA_OSD_Border_BkColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#141D28"));
            UpdateFreezable<SolidColorBrush>("Default_SA_OSD_PathColor", (ref SolidColorBrush brush) => brush.Color = (Color)ColorConverter.ConvertFromString("#FFFFFF"));
        }
    }
}
