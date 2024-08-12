using Dell.Client.Framework.Common;
using DPeMPublic.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDPM.SA.Common
{
    public interface IFWUpdateService : IFrameworkPlugin
    {
        event EventHandler CollCheckUpdate;
        event EventHandler<FWUpdateInfo> ProgressUpdate_Notify;
        event EventHandler<FWUpdateInfoPackage> CallSaveUpdateInfoPackage;
        event EventHandler CallGetDeviceInfos;
        event EventHandler<DokcUODUpdateInfoPackage> CallSaveUODFWDeviceInfos;
        event EventHandler CallCheckUODFWInfos;
        event EventHandler<PopupContentPackage> CallPopup;
        /// <summary>
        /// 供CLI使用
        /// </summary>
        event EventHandler<List<FWUpdateInfo>> DownloadAndInstall_Result_Notify;
        void StartCheckUpdateScheduleTimer();
        //0612 Bruce 將傳入值FWUpdateInfoPackage移除因已不需使用，不會影響UI和CLI
        Task<FWUpdateInfoPackage> GetFWUpdateInfo(UpdateHelper updateHelper, bool isShowNotify, bool isForce, bool isDefer, List<DeviceType> deviceTypeList, bool isUODMode);
        Task<List<FWUpdateInfo>> CheckUpdate(UpdateHelper updateHelper, bool isShowNotify, List<DeviceType> deviceTypeList, bool isUODMode);
        Task<List<FWUpdateInfo>> DownloadAndInstall(List<FWUpdateInfo> fwUpdateInfos, string installPath);
        void SetDeviceinfo(List<DeviceInfo> DeviceInfos);
        void CheckUODFWUInfo(DokcUODUpdateInfoPackage UODFWUInfo, List<DeviceInfo> DeviceInfos);
        void SetDelayFWUpdateInfoPackage(FWUpdateInfoPackage DelayFWUpdateInfoPackage);
        void DelayEvent(object e);
        void UpdateEvent(object e);
    }
}
