using Dell.Client.Framework.Common;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DDPM.SA.Common
{
    public interface ISWUpdateService : IFrameworkPlugin
    {
        event EventHandler CollCheckUpdate;

        event EventHandler<SWUpdateInfoPackage> CallSaveUpdateInfoPackage;

        event EventHandler<PopupContentPackage> CallPopup;
        event EventHandler<(string, string, bool)> CallOSD;
        /// <summary>
        /// for CLI use
        /// </summary>
        event EventHandler<List<SWUpdateInfo>> DownloadAndInstall_Result_Notify;

        void StartCheckUpdateScheduleTimer();

        Task<SWUpdateInfoPackage> GetSWUpdateInfo(bool isShowNotify, bool isForce, bool isDefer, string currentVersion, bool reScan, bool isUItrigger);

        Task<List<SWUpdateInfo>> DownloadAndInstall(List<SWUpdateInfo> fwUpdateInfos, bool isUITrigger, string installPath);

        void SetDelaySWUpdateInfoPackage(SWUpdateInfoPackage DelayFWUpdateInfoPackage);

        void DelayEvent();

        void UpdateEvent();
        void SetSkipCA(bool isSkipCA);
        void SetSkipSHA(bool isSkipSHA);
    }
}