using Dell.Client.Framework.Common;
using System;
using System.Threading.Tasks;
using static DDPM.SA.Common.ICLICommandTable;

namespace DDPM.SA.Common
{
    #region Data definitions
    //for ICMAManagerIT, CMA team as caller to input request
    public class NotifyArgs : EventArgs
    {
        public string eventtype;
        public string notification;
    }

/*    public class CMAAgentArgs : EventArgs
    {
        public string eventtype;
        public string notification;

        public string command_guid_string;
        public string serialize_Json_response;
        public int ExitCode;
        public DateTime ticket;
        public string response;
    }*/

    public class CMARequestArgs
    {
        // serialize string as json
        public string cma_request;
        //public Guid cma_request_id;
    }

    //for ICMAManagerSA that CMAProxy register CMAManager to get request over "CMARequestEvent" and feedback result by call "WriteResult"
    public class CMAEventArgs
    {
        public string input_param;
        public Guid cma_request_id;
    }

    //for CMAProxy to write back result and CMAManager bypass/re-create result to CMA team
    public class CMAResult
    {
        public string output_result;
        public Guid cma_request_id;
        public string message;
    }
    #endregion

    public interface ICMAManagerSA : IFrameworkPlugin
    {
        Task WriteResult(CMAResult result);
        event EventHandler<CMAEventArgs> CMARequestEvent;
    }

    public interface ICMAManagerIT : IFrameworkPlugin
    {
        // Task<CMAResult> PerformCMARequest(CMARequestArgs request);
        Task<CMAResult> Info(CMARequestArgs request);
        //public event EventHandler<CMAAgentArgs> CMAAgentEvent;
        event EventHandler<NotifyArgs> Notify;

    }

    public interface ICMAProxy : IFrameworkPlugin
    {
        //no action need
    }

    
}