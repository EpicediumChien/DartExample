using Dell.Client.Framework.Common;

namespace DDPM.SA.Common
{
    public interface ICLIBase : IFrameworkPlugin
    {
        //
        // return the exitcode
        //
        CLIEventResult SetCommandArgs(CLIEventArgs input, IDeviceManagerSA devMgr);
    }
}