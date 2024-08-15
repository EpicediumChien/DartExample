using Dell.Client.Framework.Common;
using System.Threading.Tasks;

namespace DDPM.SA.Common
{
    public interface ISchedulerManager : IFrameworkPlugin
    {
        Task StartSchedulerManger(int millisecond);

        Task StopSchedulerManger();
    }
}