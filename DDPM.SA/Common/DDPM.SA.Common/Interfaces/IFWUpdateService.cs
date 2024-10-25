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
        event EventHandler<(string, string, bool)> CallOSD;

        /// <summary>
        /// for CLI use
        /// </summary>
        event EventHandler<List<FWUpdateInfo>> DownloadAndInstall_Result_Notify;

        void StartCheckUpdateScheduleTimer();

        //Task<FWUpdateInfoPackage> GetFWUpdateInfo(UpdateHelper updateHelper, bool isShowNotify, bool isForce, bool isDefer, List<DeviceType> deviceTypeList, bool isUODMode, DisplayUpdateHelper displayUpdateHelper);

        //Task<List<FWUpdateInfo>> CheckUpdate(UpdateHelper updateHelper, bool isShowNotify, List<DeviceType> deviceTypeList, bool isUODMode, DisplayUpdateHelper displayUpdateHelper);

        Task<FWUpdateInfoPackage> GetFWUpdateInfo(UpdateHelper updateHelper, bool isShowNotify, bool isForce, bool isDefer, List<DeviceType>? deviceTypeList, bool isUODMode, DisplayUpdateHelper displayUpdateHelper, bool isOnlyDisplay, bool reScan, bool isUItrigger);


        Task<List<FWUpdateInfo>> DownloadAndInstall(List<FWUpdateInfo> fwUpdateInfos, bool isUITrigger, string installPath);
        Task<FWUErrorCode> Install(string installPath, bool isOnlyDisplay);
        Task<bool> RestartService();

        void SetDeviceinfo(List<DeviceInfo> DeviceInfos);

        void CheckUODFWUInfo(DokcUODUpdateInfoPackage UODFWUInfo, List<DeviceInfo> DeviceInfos);

        void SetDelayFWUpdateInfoPackage(FWUpdateInfoPackage DelayFWUpdateInfoPackage);

        void DelayEvent();

        void UpdateEvent();
        void SetSkipCA(bool isSkipCA);
    }
}