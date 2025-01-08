using DDPM.SA.Common;
using Dell.Client.Framework.Common;

namespace DDPM.QAM
{
    public static class DdpmCommonHelper
    {
        public static QAMPageViewModel? QAMPageViewModel  { get; set; }
        public static IDeviceManagerSA? DeviceManagerSA { get; set; }
        public static ILog Log { get; set; }
    }
}
