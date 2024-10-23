using Dell.Client.Framework.Common;

namespace DDPM.RemoteManagement.Common.Interfaces
{
    #region Data definitions
    public class NotifyArgs : EventArgs
    {
        public string eventType;
        public string notification;
    }

    public class RemoteRequestArgs
    {
        //Json serialized string as request
        public string remote_request;
    }

    //for CMAProxy to write back result and CMAManager bypass/re-create result to CMA team
    public class RemoteManagementResult
    {
        public string output_result;
        public Guid cma_request_id;
        public string message;
    }
    #endregion

    public interface IRemoteManagement : IFrameworkPlugin
    {
        Task<RemoteManagementResult> Info(RemoteRequestArgs request);
        event EventHandler<NotifyArgs> Notify;

    }
}
