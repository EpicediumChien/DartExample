using System.Threading.Tasks;

namespace DDPM.SA.Common
{
    public interface ICMA
    {
        Task<CommandOutput_DeviceConnection> queryConnectedDeviceInfo(CommandInput_notifyDeviceConnection input);
    }
}