using DDPM.SA.Common;
using Dell.Client.Framework.Common;
using System.Diagnostics;

namespace DDPM.QAM
{
    public static class DdpmCommonHelper
    {
        public static Stopwatch SAUserLaunchTimer = new Stopwatch();
        public static QAMPageViewModel? QAMPageViewModel { get; set; }
        public static bool QAMCameraMenuIsOpen { get; set; } = false;
        public static Guid QAMCameraID { get; set; } = new();
        public static IDeviceManagerSA? DeviceManagerSA { get; set; }
        public static ILog Log { get; set; }
    }
}
