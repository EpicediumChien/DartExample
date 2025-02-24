using Dell.Client.Framework.Common;
using Dell.TechHub.Common;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DDPM.SA.Common
{
    public interface IPlatinumSDKService : IFrameworkPlugin
    {
        Task<bool> UpdateEventValue(string EventTag, string EventValue, DataClassificationId _dcid = DataClassificationId.Restricted);

        Task<bool> UpdateEventValueforPeripheral(string EventTag, IEnumerable<KeyValuePair<string, string>> EventValue, DataClassificationId _dcid = DataClassificationId.Restricted);

        Task SetGlobalsetting_IsTelemetryConsentOn(bool value);
    }
}