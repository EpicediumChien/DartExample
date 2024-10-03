using Dell.Client.Framework.Common;
using System;
using System.Threading.Tasks;
using VcpCore.Common;

namespace DDPM.SA.Common
{
    public interface ISchedulerManager : IFrameworkPlugin
    {
        Task StartSchedulerManger(int millisecond);

        Task StopSchedulerManger();

        Task ReceiveScheduleInfo(scheduleInfo info);

        event EventHandler<ReadWriteRequest> ServiceRequest;
    }
}