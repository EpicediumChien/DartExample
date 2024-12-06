using DDPM.SA.Common.Settings;
using DDPM.SA.Common;
using Dell.Client.Framework.UX.WPF;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows;
using Dell.Client.Framework.Common;
using static DDPM.QAM.QAMPage;

namespace DDPM.QAM
{
    public static class DdpmCommonHelper
    {
        public static QAMPageViewModel? QAMPageViewModel  { get; set; }
        public static IDeviceManagerSA? DeviceManagerSA { get; set; }
    }
}
