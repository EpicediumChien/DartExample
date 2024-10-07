using Dell.Client.Framework.Common;
using DPeMPublic.Common.Enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DDPM.SA.Common
{
    public interface IFWUpdateService : IFrameworkPlugin
    {
        event EventHandler CollCheckUpdate;

        event EventHandler<UpdateProgressInfo> ProgressUpdate_Notify;

        event EventHandler<FWUpdateInfoPackage> CallSaveUpdateInfoPackage;

        event EventHandler CallGetDeviceInfos;

        event EventHandler<DokcUODUpdateInfoPackage> CallSaveUODFWDeviceInfos;

        event EventHandler CallCheckUODFWInfos;

        event EventHandler<PopupContentPackage> CallPopup;

        /// <summary>
        /// for CLI use
        /// </summary>
        event EventHandler<List<FWUpdateInfo>> DownloadAndInstall_Result_Notify;

        void StartCheckUpdateScheduleTimer();

        Task<FWUpdateInfoPackage> GetFWUpdateInfo(UpdateHelper updateHelper, bool isShowNotify, bool isForce, bool isDefer, List<DeviceType> deviceTypeList, bool isUODMode, DisplayUpdateHelper displayUpdateHelper);

        Task<List<FWUpdateInfo>> CheckUpdate(UpdateHelper updateHelper, bool isShowNotify, List<DeviceType> deviceTypeList, bool isUODMode, DisplayUpdateHelper displayUpdateHelper);

        Task<List<FWUpdateInfo>> DownloadAndInstall(List<FWUpdateInfo> fwUpdateInfos, string installPath);
        Task<FWUErrorCode> Install(string installPath);

        void SetDeviceinfo(List<DeviceInfo> DeviceInfos);

        void CheckUODFWUInfo(DokcUODUpdateInfoPackage UODFWUInfo, List<DeviceInfo> DeviceInfos);

        void SetDelayFWUpdateInfoPackage(FWUpdateInfoPackage DelayFWUpdateInfoPackage);

        void DelayEvent(object e);

        void UpdateEvent(object e);
        void SetSkipCA(bool isSkipCA);
    }
}