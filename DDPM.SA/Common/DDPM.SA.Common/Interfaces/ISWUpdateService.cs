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

        void StartCheckUpdateScheduleTimer();

        Task<SWUpdateInfoPackage> GetSWUpdateInfo(bool isShowNotify, bool isForce, bool isDefer, string currentVersion);

        Task<List<SWUpdateInfo>> CheckUpdate(bool isShowNotify, string currentVersion);

        Task<List<SWUpdateInfo>> DownloadAndInstall(List<SWUpdateInfo> fwUpdateInfos, string installPath);

        void SetDelaySWUpdateInfoPackage(SWUpdateInfoPackage DelayFWUpdateInfoPackage);

        void DelayEvent(object e);

        void UpdateEvent(object e);
    }
}