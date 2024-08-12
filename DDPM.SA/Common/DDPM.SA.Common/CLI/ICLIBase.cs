using System;
using System.Threading.Tasks;
using DDPM.SA.Common;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.Interfaces;
using static DDPM.SA.Common.ICLICommandTable;

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
