using Dell.Client.Framework.Common;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DDPM.SA.Common
{
    public interface ISWUpdateService : IFrameworkPlugin
    {
        event EventHandler<PopupContentPackage> CallPopup;
        event EventHandler<(string, string, bool)> CallOSD;
        /// <summary>
        /// for CLI use
        /// </summary>
        event EventHandler<List<SWUpdateInfo>> DownloadAndInstall_Result_Notify;

        Task<SWUpdateInfoPackage> GetSWUpdateInfo(bool isShowNotify, string currentVersion, bool reScan);

        Task<List<SWUpdateInfo>> DownloadAndInstall(List<SWUpdateInfo> swUpdateInfos, bool isUITrigger, string installPath);
        void SetSkipCA(bool isSkipCA);
        void SetSkipSHA(bool isSkipSHA);
    }
}