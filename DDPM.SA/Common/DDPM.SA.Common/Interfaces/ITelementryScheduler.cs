using Dell.Client.Framework.Common;
using System.Threading.Tasks;

namespace DDPM.SA.Common
{
    public interface ITelementryScheduler : IFrameworkPlugin
    {
        Task StartTelemetrySchedulerManger(bool IsStart);

        Task<bool> ReceiveTelemetryInfo(string EventTag, string EventValue, Telementry_Frequency Frequency);

        Task<bool> ReceiveTelemetryInfo(string EventTag, Telementry_Frequency Frequency);

        Task GetGlobalsetting_IsTelemetryConsentOn(bool value);
    }
}