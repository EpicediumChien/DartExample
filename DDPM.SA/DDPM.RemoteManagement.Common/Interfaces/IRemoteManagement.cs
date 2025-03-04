using Dell.Client.Framework.Common;

namespace DDPM.RemoteManagement.Common.Interfaces
{
    #region Data definitions
    public class NotifyArgs : EventArgs
    {
        public string eventType { get; set; } = string.Empty;
        public string notification { get; set; } = string.Empty;
    }

    public class RemoteRequestArgs
    {
        //Json serialized string as request
        public string remote_request { get; set; } = string.Empty;
    }

    //for CMAProxy to write back result and CMAManager bypass/re-create result to CMA team
    public class RemoteManagementResult
    {
        public string output_result { get; set; } = string.Empty;
        public Guid cma_request_id;
        public string message { get; set; } = string.Empty;
    }
    #endregion

    public interface IRemoteManagement : IFrameworkPlugin
    {
        Task<RemoteManagementResult> Info(RemoteRequestArgs request);
        event EventHandler<NotifyArgs> Notify;

        event EventHandler<NotifyArgs> DisplayConnected;
        event EventHandler<NotifyArgs> DisplayDisconnected;

    }

}
