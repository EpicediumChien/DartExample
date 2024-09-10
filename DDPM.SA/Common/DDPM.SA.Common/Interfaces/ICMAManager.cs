using Dell.Client.Framework.Common;
using System;
using System.Threading.Tasks;
using static DDPM.SA.Common.ICLICommandTable;

namespace DDPM.SA.Common
{
    #region Data definitions
    //for ICMAManagerIT, CMA team as caller to input request
    public class CMARequestArgs
    {
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
        Task<CMAResult> PerformCMARequest(CMARequestArgs request);
    }

    public interface ICMAProxy : IFrameworkPlugin
    {
        //no action need
    }
}