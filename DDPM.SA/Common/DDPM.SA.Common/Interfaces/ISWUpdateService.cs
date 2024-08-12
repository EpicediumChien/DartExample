using Dell.Client.Framework.Common;
using DPeMPublic.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDPM.SA.Common
{
    public interface ISWUpdateService : IFrameworkPlugin
    {
        event EventHandler CollCheckUpdate;
        event EventHandler<SWUpdateInfoPackage> CallSaveUpdateInfoPackage;
        event EventHandler<PopupContentPackage> CallPopup;
        void StartCheckUpdateScheduleTimer();
        Task<SWUpdateInfoPackage> GetSWUpdateInfo(bool isShowNotify, bool isForce, bool isDefer);
        Task<List<SWUpdateInfo>> CheckUpdate(bool isShowNotify);
        Task<List<SWUpdateInfo>> DownloadAndInstall(List<SWUpdateInfo> fwUpdateInfos, string installPath);
        void SetDelaySWUpdateInfoPackage(SWUpdateInfoPackage DelayFWUpdateInfoPackage);
        void DelayEvent(object e);
        void UpdateEvent(object e);
    }
}
