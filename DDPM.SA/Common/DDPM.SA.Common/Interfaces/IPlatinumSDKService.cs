using Dell.Client.Framework.Common;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DDPM.SA.Common
{
    public interface IPlatinumSDKService : IFrameworkPlugin
    {

        Task<bool> UpdateEventValue(string Event, string EventValue);
    }
}