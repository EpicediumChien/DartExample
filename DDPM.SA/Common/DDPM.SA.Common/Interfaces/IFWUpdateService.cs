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

        Task<FWUpdateInfoPackage> GetFWUpdateInfo(UpdateHelper updateHelper, List<DeviceInfo> deviceInfos, bool isShowNotify, bool isForce, bool isDefer, List<DeviceType>? deviceTypeList, bool isUODMode, DisplayUpdateHelper displayUpdateHelper, bool isOnlyDisplay, bool reScan, bool isUItrigger, List<string> giuds, List<string> serviceTags, List<string> models, string minVersion);


        Task<List<FWUpdateInfo>> DownloadAndInstall(List<FWUpdateInfo> fwUpdateInfos, bool isUITrigger, string installPath);
        Task<FWUErrorCode> Install(string installPath, bool isOnlyDisplay, DeviceType deviceType);
        Task<bool> RestartService();

        void SetDeviceinfo(List<DeviceInfo> DeviceInfos, int DongleCount);

        void CheckUODFWUInfo(DokcUODUpdateInfoPackage UODFWUInfo, List<DeviceInfo> DeviceInfos);

        void SetDelayFWUpdateInfoPackage(FWUpdateInfoPackage DelayFWUpdateInfoPackage);

        void DelayEvent();

        Task<List<FWUpdateInfo>> UpdateEvent();
        void SetSkipCA(bool isSkipCA);
    }
}