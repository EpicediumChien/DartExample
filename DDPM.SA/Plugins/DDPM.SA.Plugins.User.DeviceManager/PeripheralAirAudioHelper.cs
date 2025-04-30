//#define SUPPORT_210

using DDPM.SA.Common;
using Dell.Client.Framework.Common;
using DPeMPublic.Common.Enums;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDPM.SA.Plugins.User.DeviceManager
{
    public class PeripheralAirAudioHelper
    {
        //private static IDeviceManagerSA devManagerSA = null;
        private static IDTPProxyPlugin DTPService = null;
        //private static IDPeMPlugin DTHService = null;

        private static ILog _log = null;

        public PeripheralAirAudioHelper(ILog Log)
        {
            _log = Log;
        }

        private void writelog(string text,
            [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
            [System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "",
            [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0)
        {
            if (string.IsNullOrEmpty(text))
                text = "";

            string className = this.GetType().Name;
            text = $"{DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss.fff")}[PeripheralAirAudioHelper] {text}, Class:{className}, Caller Name:{memberName}, Source Line {sourceLineNumber}";
            Console.WriteLine(text);
            if (_log != null)
            {
                _log.Info(text);
            }
        }

        //public void UpdateDDPMPluginInstances(IDeviceManagerSA devMgr = null, IDTPProxyPlugin dtpService = null, IDPeMPlugin dthService = null)
        public void UpdateDDPMPluginInstances(IDTPProxyPlugin dtpService = null)
        {
            //if (devMgr != null)
            //    devManagerSA = devMgr;
            if (dtpService != null)
                DTPService = dtpService;
            //if (dthService != null)
            //    DTHService = dthService;

            //writelog($"[UpdateDDPMPluginInstances] devManagerSA is {(devMgr == null ? "NULL" : "NOTNULL")}, dtpService is {(dtpService == null ? "NULL" : "NOTNULL")}, dthService is {(dthService == null ? "NULL" : "NOTNULL")}");
            writelog($"[UpdateDDPMPluginInstances] devManagerSA is dtpService is {(dtpService == null ? "NULL" : "NOTNULL")}");
        }

        public async Task<HeadsetConnectionType> GetAirAudioConnectionTypeAsync(string Guid)
        {
            try
            {
                //It's enum HeadsetConnectionType
                var result = await DTPService.GetAirAudioConnectionTypeAsync(Guid);
                writelog($"GetAirAudioConnectionTypeAsync succeeded, value is {result.ToString()}");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"GetAirAudioConnectionTypeAsync failed for {Guid} - Exception: {ex.Message}");
                return HeadsetConnectionType.HeadsetConnectionTypeUnknown;
            }
        }

        public async Task<JArray> GetAirAudioDeviceItemsAsync()
        {
            try
            {
                var result = await DTPService.GetAirAudioDeviceItemsAsync();
                if (result != null)
                {
                    writelog($"GetAirAudioDeviceItemsAsync Success");
                    return result;
                }
                else
                {
                    writelog($"GetAirAudioDeviceItemsAsync value is null");
                    return null;
                }
            }
            catch (Exception ex)
            {
                writelog($"GetAirAudioDeviceItemsAsync failed - Exception: {ex.Message}");
                return null;
            }
        }

        public async Task<string> GetAirAudioSerialNumberAsync(string Guid)
        {
            try
            {
                var result = await DTPService.GetAirAudioSerialNumberAsync(Guid);
                if (result != null)
                {
                    writelog($"GetAirAudioSerialNumberAsync Success");
                    return result;
                }
                else
                {
                    writelog($"GetAirAudioSerialNumberAsync value is null");
                    return null;
                }
            }
            catch (Exception ex)
            {
                writelog($"GetAirAudioSerialNumberAsync failed for {Guid} - Exception: {ex.Message}");
                return null;
            }
        }

        public async Task<string> GetAirAudioDeviceBatteryStatusAsync(string Guid)
        {
            try
            {
                var result = await DTPService.GetAirAudioDeviceBatteryStatusAsync(Guid);
                if (result != null)
                {
                    writelog($"GetAirAudioDeviceBatteryStatusAsync Success");
                    return result;
                }
                else
                {
                    writelog($"GetAirAudioSerialNumberAsync value is null");
                    return null;
                }
            }
            catch (Exception ex)
            {
                writelog($"GetAirAudioSerialNumberAsync failed for {Guid} - Exception: {ex.Message}");
                return null;
            }
        }

        public async Task<string> GetAirAudioPairingHostName1Async(string Guid)
        {
            try
            {
                var result = await DTPService.GetAirAudioPairingHostName1Async(Guid);
                if (result != null)
                {
                    writelog($"GetAirAudioPairingHostName1Async Success");
                    return result;
                }
                else
                {
                    writelog($"GetAirAudioPairingHostName1Async value is null");
                    return null;
                }
            }
            catch (Exception ex)
            {
                writelog($"GetAirAudioPairingHostName1Async failed for {Guid} - Exception: {ex.Message}");
                return null;
            }
        }

        public async Task<string> GetAirAudioPairingHostName2Async(string Guid)
        {
            try
            {
                var result = await DTPService.GetAirAudioPairingHostName2Async(Guid);
                if (result != null)
                {
                    writelog($"GetAirAudioPairingHostName2Async Success");
                    return result;
                }
                else
                {
                    writelog($"GetAirAudioPairingHostName2Async value is null");
                    return null;
                }
            }
            catch (Exception ex)
            {
                writelog($"GetAirAudioPairingHostName2Async failed for {Guid} - Exception: {ex.Message}");
                return null;
            }
        }

        public async Task<string> GetAirAudioPairingHostName3Async(string Guid)
        {
            try
            {
                var result = await DTPService.GetAirAudioPairingHostName3Async(Guid);
                if (result != null)
                {
                    writelog($"GetAirAudioPairingHostName3Async Success");
                    return result;
                }
                else
                {
                    writelog($"GetAirAudioPairingHostName3Async value is null");
                    return null;
                }
            }
            catch (Exception ex)
            {
                writelog($"GetAirAudioPairingHostName3Async failed for {Guid} - Exception: {ex.Message}");
                return null;
            }
        }

        public async Task<string> GetAirAudioPairingStatusNameAsync(string Guid)
        {
            try
            {
                var result = await DTPService.GetAirAudioPairingStatusNameAsync(Guid);
                if (result != null)
                {
                    writelog($"GetAirAudioPairingStatusNameAsync Success");
                    return result;
                }
                else
                {
                    writelog($"GetAirAudioPairingStatusNameAsync value is null");
                    return null;
                }
            }
            catch (Exception ex)
            {
                writelog($"GetAirAudioPairingStatusNameAsync failed for {Guid} - Exception: {ex.Message}");
                return null;
            }
        }

        public async Task<string> GetAirAudioParentDeviceTypeAsync(string Guid)
        {
            try
            {
                var result = await DTPService.GetAirAudioParentDeviceTypeAsync(Guid);
                if (result != null)
                {
                    writelog($"GetAirAudioParentDeviceTypeAsync Success");
                    return result;
                }
                else
                {
                    writelog($"GetAirAudioParentDeviceTypeAsync value is null");
                    return null;
                }
            }
            catch (Exception ex)
            {
                writelog($"GetAirAudioParentDeviceTypeAsync failed for {Guid} - Exception: {ex.Message}");
                return null;
            }
        }

        public async Task<string> GetAirAudioModelNumberAsync(string Guid)
        {
            try
            {
                var result = await DTPService.GetAirAudioModelNumberAsync(Guid);
                if (result != null)
                {
                    writelog($"GetAirAudioModelNumberAsync Success");
                    return result;
                }
                else
                {
                    writelog($"GetAirAudioModelNumberAsync value is null");
                    return null;
                }
            }
            catch (Exception ex)
            {
                writelog($"GetAirAudioModelNumberAsync failed for {Guid} - Exception: {ex.Message}");
                return null;
            }
        }

        public async Task<string> GetAirAudioDeviceTypeAsync(string Guid)
        {
            try
            {
                var result = await DTPService.GetAirAudioDeviceTypeAsync(Guid);
                if (result != null)
                {
                    writelog($"GetAirAudioDeviceTypeAsync Success");
                    return result;
                }
                else
                {
                    writelog($"GetAirAudioDeviceTypeAsync value is null");
                    return null;
                }
            }
            catch (Exception ex)
            {
                writelog($"GetAirAudioDeviceTypeAsync failed for {Guid} - Exception: {ex.Message}");
                return null;
            }
        }

        public async Task<string> GetAirAudioFirmwareVersionAsync(string Guid)
        {
            try
            {
                var result = await DTPService.GetAirAudioFirmwareVersionAsync(Guid);
                if (result != null)
                {
                    writelog($"GetAirAudioFirmwareVersionAsync Success");
                    return result;
                }
                else
                {
                    writelog($"GetAirAudioFirmwareVersionAsync value is null");
                    return null;
                }
            }
            catch (Exception ex)
            {
                writelog($"GetAirAudioFirmwareVersionAsync failed for {Guid} - Exception: {ex.Message}");
                return null;
            }
        }

        public async Task<string> GetAirAudioPluginIdAsync(string Guid)
        {
            try
            {
                var result = await DTPService.GetAirAudioPluginIdAsync(Guid);
                if (result != null)
                {
                    writelog($"GetAirAudioPluginIdAsync Success");
                    return result;
                }
                else
                {
                    writelog($"GetAirAudioPluginIdAsync value is null");
                    return null;
                }
            }
            catch (Exception ex)
            {
                writelog($"GetAirAudioPluginIdAsync failed for {Guid} - Exception: {ex.Message}");
                return null;
            }
        }

        public async Task<string> GetAirAudioDeviceIdAsync(string Guid)
        {
            try
            {
                var result = await DTPService.GetAirAudioDeviceIdAsync(Guid);
                if (result != null)
                {
                    writelog($"GetAirAudioDeviceIdAsync Success");
                    return result;
                }
                else
                {
                    writelog($"GetAirAudioDeviceIdAsync value is null");
                    return null;
                }
            }
            catch (Exception ex)
            {
                writelog($"GetAirAudioDeviceIdAsync failed for {Guid} - Exception: {ex.Message}");
                return null;
            }
        }

        public async Task<string> GetAirAudioDeviceNameAsync(string Guid)
        {
            try
            {
                var result = await DTPService.GetAirAudioDeviceNameAsync(Guid);
                if (result != null)
                {
                    writelog($"GetAirAudioDeviceNameAsync Success");
                    return result;
                }
                else
                {
                    writelog($"GetAirAudioDeviceNameAsync value is null");
                    return null;
                }
            }
            catch (Exception ex)
            {
                writelog($"GetAirAudioDeviceNameAsync failed for {Guid} - Exception: {ex.Message}");
                return null;
            }
        }

        public async Task<DeviceInterfaceType> GetAirAudioDeviceInterfaceTypeAsync(string Guid)
        {
            try
            {
                var result = await DTPService.GetAirAudioDeviceInterfaceTypeAsync(Guid);
                writelog($"GetAirAudioDeviceInterfaceTypeAsync succeeded, value is {result.ToString()}");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"GetAirAudioDeviceInterfaceTypeAsync failed for {Guid} - Exception: {ex.Message}");
                return default(DeviceInterfaceType);
            }
        }

        //public async Task<bool> GetAirAudioIsWearDetectionAsync(string Guid)
        //{
        //    try
        //    {
        //        var result = await DTPService.GetAirAudioIsWearDetectionAsync(Guid);
        //        if (result)
        //            writelog($"GetAirAudioIsWearDetectionAsync Success");
        //        else
        //            writelog($"GetAirAudioIsWearDetectionAsync Fail");
        //        return result;
        //    }
        //    catch (Exception ex)
        //    {
        //        writelog($"GetAirAudioIsWearDetectionAsync failed for {Guid} - Exception: {ex.Message}");
        //        return false;
        //    }
        //}

        public async Task<bool> GetAirAudioMuteStatusAsync(string Guid)
        {
            try
            {
                var result = await DTPService.GetAirAudioMuteStatusAsync(Guid);
                if (result)
                    writelog($"GetAirAudioMuteStatusAsync Success");
                else
                    writelog($"GetAirAudioMuteStatusAsync Fail");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"GetAirAudioMuteStatusAsync failed for {Guid} - Exception: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> GetAirAudioBoomMicAsync(string Guid)
        {
            try
            {
                var result = await DTPService.GetAirAudioBoomMicAsync(Guid);
                if (result)
                    writelog($"GetAirAudioBoomMicAsync Success");
                else
                    writelog($"GetAirAudioBoomMicAsync Fail");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"GetAirAudioBoomMicAsync failed for {Guid} - Exception: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> GetAirAudioIsBoomMicSupportedAsync(string Guid)
        {
            try
            {
                var result = await DTPService.GetAirAudioIsBoomMicSupportedAsync(Guid);
                if (result)
                    writelog($"GetAirAudioIsBoomMicSupportedAsync Success");
                else
                    writelog($"GetAirAudioIsBoomMicSupportedAsync Fail");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"GetAirAudioIsBoomMicSupportedAsync failed for {Guid} - Exception: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> GetAirAudioWearDetectionAsync(string Guid)
        {
            try
            {
                var result = await DTPService.GetAirAudioWearDetectionAsync(Guid);
                if (result)
                    writelog($"GetAirAudioWearDetectionAsync Success");
                else
                    writelog($"GetAirAudioWearDetectionAsync Fail");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"GetAirAudioWearDetectionAsync failed for {Guid} - Exception: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> GetAirAudioVoiceGuidanceAsync(string Guid)
        {
            try
            {
                var result = await DTPService.GetAirAudioVoiceGuidanceAsync(Guid);
                if (result)
                    writelog($"GetAirAudioVoiceGuidanceAsync Success");
                else
                    writelog($"GetAirAudioVoiceGuidanceAsync Fail");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"GetAirAudioVoiceGuidanceAsync failed for {Guid} - Exception: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> GetAirAudioBusyLightAsync(string Guid)
        {
            try
            {
                var result = await DTPService.GetAirAudioBusyLightAsync(Guid);
                if (result)
                    writelog($"GetAirAudioBusyLightAsync Success");
                else
                    writelog($"GetAirAudioBusyLightAsync Fail");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"GetAirAudioBusyLightAsync failed for {Guid} - Exception: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> GetAirAudioSidetoneAsync(string Guid)
        {
            try
            {
                var result = await DTPService.GetAirAudioSidetoneAsync(Guid);
                if (result)
                    writelog($"GetAirAudioSidetoneAsync Success");
                else
                    writelog($"GetAirAudioSidetoneAsync Fail");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"GetAirAudioSidetoneAsync failed for {Guid} - Exception: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> GetAirAudioMicNCIncomingAsync(string Guid)
        {
            try
            {
                var result = await DTPService.GetAirAudioMicNCIncomingAsync(Guid);
                if (result)
                    writelog($"GetAirAudioMicNCIncomingAsync Success");
                else
                    writelog($"GetAirAudioMicNCIncomingAsync Fail");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"GetAirAudioMicNCIncomingAsync failed for {Guid} - Exception: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> GetAirAudioIsMicNCIncomingSupportedAsync(string Guid)
        {
            try
            {
                var result = await DTPService.GetAirAudioIsMicNCIncomingSupportedAsync(Guid);
                if (result)
                    writelog($"GetAirAudioIsMicNCIncomingSupportedAsync Success");
                else
                    writelog($"GetAirAudioIsMicNCIncomingSupportedAsync Fail");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"GetAirAudioIsMicNCIncomingSupportedAsync failed for {Guid} - Exception: {ex.Message}");
                return false;
            }
        }

        //public async Task<bool> GetAirAudioIsWearDetectionQuickPauseSupportedAsync(string Guid)
        //{
        //    try
        //    {
        //        var result = await DTPService.GetAirAudioMicNoiseCancellationAsync(Guid);
        //        if (result)
        //            writelog($"GetAirAudioIsMicNoiseCancellationAsync Success");
        //        else
        //            writelog($"GetAirAudioIsMicNoiseCancellationAsync Fail");
        //        return result;
        //    }
        //    catch (Exception ex)
        //    {
        //        writelog($"GetAirAudioIsMicNoiseCancellationAsync failed for {Guid} - Exception: {ex.Message}");
        //        return false;
        //    }
        //}

        public async Task<bool> GetAirAudioIsWearDetectionMuteMicSupportedAsync(string Guid)
        {
            try
            {
                var result = await DTPService.GetAirAudioIsWearDetectionMuteMicSupportedAsync(Guid);
                if (result)
                    writelog($"GetAirAudioIsWearDetectionMuteMicSupportedAsync Success");
                else
                    writelog($"GetAirAudioIsWearDetectionMuteMicSupportedAsync Fail");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"GetAirAudioIsWearDetectionMuteMicSupportedAsync failed for {Guid} - Exception: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> GetAirAudioIsWearDetectionPauseMusicSupportedAsync(string Guid)
        {
            try
            {
                var result = await DTPService.GetAirAudioIsWearDetectionPauseMusicSupportedAsync(Guid);
                if (result)
                    writelog($"GetAirAudioIsWearDetectionPauseMusicSupportedAsync Success");
                else
                    writelog($"GetAirAudioIsWearDetectionPauseMusicSupportedAsync Fail");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"GetAirAudioIsWearDetectionPauseMusicSupportedAsync failed for {Guid} - Exception: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> GetAirAudioIsWearDetectionSensitivitySupportedAsync(string Guid)
        {
            try
            {
                var result = await DTPService.GetAirAudioIsWearDetectionSensitivitySupportedAsync(Guid);
                if (result)
                    writelog($"GetAirAudioIsWearDetectionSensitivitySupportedAsync Success");
                else
                    writelog($"GetAirAudioIsWearDetectionSensitivitySupportedAsync Fail");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"GetAirAudioIsWearDetectionSensitivitySupportedAsync failed for {Guid} - Exception: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> GetAirAudioIsWearDetectionSupportedAsync(string Guid)
        {
            try
            {
                var result = await DTPService.GetAirAudioIsWearDetectionSupportedAsync(Guid);
                if (result)
                    writelog($"GetAirAudioIsWearDetectionSupportedAsync Success");
                else
                    writelog($"GetAirAudioIsWearDetectionSupportedAsync Fail");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"GetAirAudioIsWearDetectionSupportedAsync failed for {Guid} - Exception: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> GetAirAudioIsANCSupportedAsync(string Guid)
        {
            try
            {
                var result = await DTPService.GetAirAudioIsANCSupportedAsync(Guid);
                if (result)
                    writelog($"GetAirAudioIsANCSupportedAsync Success");
                else
                    writelog($"GetAirAudioIsANCSupportedAsync Fail");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"GetAirAudioIsANCSupportedAsync failed for {Guid} - Exception: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> GetAirAudioIsEqualizerSupportedAsync(string Guid)
        {
            try
            {
                var result = await DTPService.GetAirAudioIsEqualizerSupportedAsync(Guid);
                if (result)
                    writelog($"GetAirAudioIsEqualizerSupportedAsync Success");
                else
                    writelog($"GetAirAudioIsEqualizerSupportedAsync Fail");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"GetAirAudioIsEqualizerSupportedAsync failed for {Guid} - Exception: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> GetAirAudioIsPresetsSupportedAsync(string Guid)
        {
            try
            {
                var result = await DTPService.GetAirAudioIsPresetsSupportedAsync(Guid);
                if (result)
                    writelog($"GetAirAudioIsPresetsSupportedAsync Success");
                else
                    writelog($"GetAirAudioIsPresetsSupportedAsync Fail");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"GetAirAudioIsPresetsSupportedAsync failed for {Guid} - Exception: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> GetAirAudioIsVoiceGuidanceSupportedAsync(string Guid)
        {
            try
            {
                var result = await DTPService.GetAirAudioIsVoiceGuidanceSupportedAsync(Guid);
                if (result)
                    writelog($"GetAirAudioIsVoiceGuidanceSupportedAsync Success");
                else
                    writelog($"GetAirAudioIsVoiceGuidanceSupportedAsync Fail");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"GetAirAudioIsVoiceGuidanceSupportedAsync failed for {Guid} - Exception: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> GetAirAudioIsBusyLightSupportedAsync(string Guid)
        {
            try
            {
                var result = await DTPService.GetAirAudioIsBusyLightSupportedAsync(Guid);
                if (result)
                    writelog($"GetAirAudioIsBusyLightSupportedAsync Success");
                else
                    writelog($"GetAirAudioIsBusyLightSupportedAsync Fail");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"GetAirAudioIsBusyLightSupportedAsync failed for {Guid} - Exception: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> GetAirAudioIsSidetoneSupportedAsync(string Guid)
        {
            try
            {
                var result = await DTPService.GetAirAudioIsSidetoneSupportedAsync(Guid);
                if (result)
                    writelog($"GetAirAudioIsSidetoneSupportedAsync Success");
                else
                    writelog($"GetAirAudioIsSidetoneSupportedAsync Fail");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"GetAirAudioIsSidetoneSupportedAsync failed for {Guid} - Exception: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> GetAirAudioIsMicNoiseCancellationSupportedAsync(string Guid)
        {
            try
            {
                var result = await DTPService.GetAirAudioIsMicNoiseCancellationSupportedAsync(Guid);
                if (result)
                    writelog($"GetAirAudioIsMicNoiseCancellationSupportedAsync Success");
                else
                    writelog($"GetAirAudioIsMicNoiseCancellationSupportedAsync Fail");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"GetAirAudioIsMicNoiseCancellationSupportedAsync failed for {Guid} - Exception: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> GetAirAudioIsDirtyAsync(string Guid)
        {
            try
            {
                var result = await DTPService.GetAirAudioIsDirtyAsync(Guid);
                if (result)
                    writelog($"GetAirAudioIsDirtyAsync Success");
                else
                    writelog($"GetAirAudioIsDirtyAsync Fail");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"GetAirAudioIsDirtyAsync failed for {Guid} - Exception: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> GetAirAudioIsReadyAsync(string Guid)
        {
            try
            {
                var result = await DTPService.GetAirAudioIsReadyAsync(Guid);
                if (result)
                    writelog($"GetAirAudioIsReadyAsync Success");
                else
                    writelog($"GetAirAudioIsReadyAsync Fail");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"GetAirAudioIsReadyAsync failed for {Guid} - Exception: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> GetAirAudioIsWearDetectionPauseMusicEnabledAsync(string Guid)
        {
            try
            {
                var result = await DTPService.GetAirAudioIsWearDetectionPauseMusicEnabledAsync(Guid);
                if (result)
                    writelog($"GetAirAudioIsWearDetectionPauseMusicEnabledAsync Success");
                else
                    writelog($"GetAirAudioIsWearDetectionPauseMusicEnabledAsync Fail");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"GetAirAudioIsWearDetectionPauseMusicEnabledAsync failed for {Guid} - Exception: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> GetAirAudioIsWearDetectionMuteMicEnabledAsync(string Guid)
        {
            try
            {
                var result = await DTPService.GetAirAudioIsWearDetectionMuteMicEnabledAsync(Guid);
                if (result)
                    writelog($"GetAirAudioIsWearDetectionMuteMicEnabledAsync Success");
                else
                    writelog($"GetAirAudioIsWearDetectionMuteMicEnabledAsync Fail");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"GetAirAudioIsWearDetectionMuteMicEnabledAsync failed for {Guid} - Exception: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> GetAirAudioIsBatteryLevelSupportedAsync(string Guid)
        {
            try
            {
                var result = await DTPService.GetAirAudioIsBatteryLevelSupportedAsync(Guid);
                if (result)
                    writelog($"GetAirAudioIsBatteryLevelSupportedAsync Success");
                else
                    writelog($"GetAirAudioIsBatteryLevelSupportedAsync Fail");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"GetAirAudioIsBatteryLevelSupportedAsync failed for {Guid} - Exception: {ex.Message}");
                return false;
            }
        }

        public async Task<int> GetAirAudioWearDetectionSensitivityAsync(string Guid)
        {
            try
            {
                var result = await DTPService.GetAirAudioWearDetectionSensitivityAsync(Guid);
                if (result != -1)
                    writelog($"GetAirAudioWearDetectionSensitivityAsync Success");
                else
                    writelog($"GetAirAudioWearDetectionSensitivityAsync Fail");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"GetAirAudioWearDetectionSensitivityAsync failed for {Guid} - Exception: {ex.Message}");
                return -1;
            }
        }

        public async Task<int> GetAirAudioIsWearDetectionQuickPauseAsync(string Guid)
        {
            try
            {
                var result = await DTPService.GetAirAudioWearDetectionQuickPauseAsync(Guid);
                if (result != -1)
                    writelog($"GetAirAudioIsWearDetectionQuickPauseAsync Success");
                else
                    writelog($"GetAirAudioIsWearDetectionQuickPauseAsync Fail");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"GetAirAudioIsWearDetectionQuickPauseAsync failed for {Guid} - Exception: {ex.Message}");
                return -1;
            }
        }

        public async Task<int> GetAirAudioAncGainAsync(string Guid)
        {
            try
            {
                var result = await DTPService.GetAirAudioAncGainAsync(Guid);
                if (result != -1)
                    writelog($"GetAirAudioAncGainAsync Success");
                else
                    writelog($"GetAirAudioAncGainAsync Fail");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"GetAirAudioAncGainAsync failed for {Guid} - Exception: {ex.Message}");
                return -1;
            }
        }

        public async Task<int> GetAirAudioAncModeAsync(string Guid)
        {
            try
            {
                var result = await DTPService.GetAirAudioAncModeAsync(Guid);
                if (result != -1)
                    writelog($"GetAirAudioAncModeAsync Success");
                else
                    writelog($"GetAirAudioAncModeAsync Fail");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"GetAirAudioAncModeAsync failed for {Guid} - Exception: {ex.Message}");
                return -1;
            }
        }

        public async Task<int> GetAirAudioBand1GainAsync(string Guid)
        {
            try
            {
                var result = await DTPService.GetAirAudioBand1GainAsync(Guid);
                if (result != -1)
                    writelog($"GetAirAudioBand1GainAsync Success");
                else
                    writelog($"GetAirAudioBand1GainAsync Fail");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"GetAirAudioBand1GainAsync failed for {Guid} - Exception: {ex.Message}");
                return -1;
            }
        }

        public async Task<int> GetAirAudioBand2GainAsync(string Guid)
        {
            try
            {
                var result = await DTPService.GetAirAudioBand2GainAsync(Guid);
                if (result != -1)
                    writelog($"GetAirAudioBand2GainAsync Success");
                else
                    writelog($"GetAirAudioBand2GainAsync Fail");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"GetAirAudioBand2GainAsync failed for {Guid} - Exception: {ex.Message}");
                return -1;
            }
        }

        public async Task<int> GetAirAudioBand3GainAsync(string Guid)
        {
            try
            {
                var result = await DTPService.GetAirAudioBand3GainAsync(Guid);
                if (result != -1)
                    writelog($"GetAirAudioBand3GainAsync Success");
                else
                    writelog($"GetAirAudioBand3GainAsync Fail");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"GetAirAudioBand3GainAsync failed for {Guid} - Exception: {ex.Message}");
                return -1;
            }
        }

        public async Task<int> GetAirAudioBand4GainAsync(string Guid)
        {
            try
            {
                var result = await DTPService.GetAirAudioBand4GainAsync(Guid);
                if (result != -1)
                    writelog($"GetAirAudioBand4GainAsync Success");
                else
                    writelog($"GetAirAudioBand4GainAsync Fail");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"GetAirAudioBand4GainAsync failed for {Guid} - Exception: {ex.Message}");
                return -1;
            }
        }

        public async Task<int> GetAirAudioBand5GainAsync(string Guid)
        {
            try
            {
                var result = await DTPService.GetAirAudioBand5GainAsync(Guid);
                if (result != -1)
                    writelog($"GetAirAudioBand5GainAsync Success");
                else
                    writelog($"GetAirAudioBand5GainAsync Fail");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"GetAirAudioBand5GainAsync failed for {Guid} - Exception: {ex.Message}");
                return -1;
            }
        }

        public async Task<int> GetAirAudioSidetoneLevelAsync(string Guid)
        {
            try
            {
                var result = await DTPService.GetAirAudioSidetoneLevelAsync(Guid);
                if (result != -1)
                    writelog($"GetAirAudioSidetoneLevelAsync Success");
                else
                    writelog($"GetAirAudioSidetoneLevelAsync Fail");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"GetAirAudioSidetoneLevelAsync failed for {Guid} - Exception: {ex.Message}");
                return -1;
            }
        }

        public async Task<int> GetAirAudioSelectedPresetAsync(string Guid)
        {
            try
            {
                var result = await DTPService.GetAirAudioSelectedPresetAsync(Guid);
                if (result != -1)
                    writelog($"GetAirAudioSelectedPresetAsync Success");
                else
                    writelog($"GetAirAudioSelectedPresetAsync Fail");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"GetAirAudioSelectedPresetAsync failed for {Guid} - Exception: {ex.Message}");
                return -1;
            }
        }

        public async Task<int> GetAirAudioBatteryLevelAsync(string Guid)
        {
            try
            {
                var result = await DTPService.GetAirAudioBatteryLevelAsync(Guid);
                if (result != -1)
                    writelog($"GetAirAudioBatteryLevelAsync Success");
                else
                    writelog($"GetAirAudioBatteryLevelAsync Fail");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"GetAirAudioBatteryLevelAsync failed for {Guid} - Exception: {ex.Message}");
                return -1;
            }
        }

        public async Task<int> GetAirAudioPairedDeviceCountAsync(string Guid)
        {
            try
            {
                var result = await DTPService.GetAirAudioPairedDeviceCountAsync(Guid);
                if (result != -1)
                    writelog($"GetAirAudioPairedDeviceCountAsync Success");
                else
                    writelog($"GetAirAudioPairedDeviceCountAsync Fail");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"GetAirAudioPairedDeviceCountAsync failed for {Guid} - Exception: {ex.Message}");
                return -1;
            }
        }

        public async Task<int> GetAirAudioMaxPairingSlotsAsync(string Guid)
        {
            try
            {
                var result = await DTPService.GetAirAudioMaxPairingSlotsAsync(Guid);
                if (result != -1)
                    writelog($"GetAirAudioMaxPairingSlotsAsync Success");
                else
                    writelog($"GetAirAudioMaxPairingSlotsAsync Fail");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"GetAirAudioMaxPairingSlotsAsync failed for {Guid} - Exception: {ex.Message}");
                return -1;
            }
        }

        public async Task<int> GetAirAudioTotalNumberOfPairedHostNameAsync(string Guid)
        {
            try
            {
                var result = await DTPService.GetAirAudioTotalNumberOfPairedHostNameAsync(Guid);
                if (result != -1)
                    writelog($"GetAirAudioTotalNumberOfPairedHostNameAsync Success");
                else
                    writelog($"GetAirAudioTotalNumberOfPairedHostNameAsync Fail");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"GetAirAudioTotalNumberOfPairedHostNameAsync failed for {Guid} - Exception: {ex.Message}");
                return -1;
            }
        }

        public async Task<int> GetAirAudioInstanceIdAsync(string Guid)
        {
            try
            {
                var result = await DTPService.GetAirAudioInstanceIdAsync(Guid);
                if (result != -1)
                    writelog($"GetAirAudioInstanceIdAsync Success");
                else
                    writelog($"GetAirAudioInstanceIdAsync Fail");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"GetAirAudioInstanceIdAsync failed for {Guid} - Exception: {ex.Message}");
                return -1;
            }
        }

        public async Task<int> GetAirAudioInstanceNumberAsync(string Guid)
        {
            try
            {
                var result = await DTPService.GetAirAudioInstanceNumberAsync(Guid);
                if (result != -1)
                    writelog($"GetAirAudioInstanceNumberAsync Success");
                else
                    writelog($"GetAirAudioInstanceNumberAsync Fail");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"GetAirAudioInstanceNumberAsync failed for {Guid} - Exception: {ex.Message}");
                return -1;
            }
        }

        public async Task<int> GetAirAudioODMIdAsync(string Guid)
        {
            try
            {
                var result = await DTPService.GetAirAudioODMIdAsync(Guid);
                if (result != -1)
                    writelog($"GetAirAudioODMIdAsync Success");
                else
                    writelog($"GetAirAudioODMIdAsync Fail");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"GetAirAudioODMIdAsync failed for {Guid} - Exception: {ex.Message}");
                return -1;
            }
        }

#if SUPPORT_210
        public async Task<bool> GetAirAudioIsConnectedAsync(string Guid)
        {
            try
            {
                var result = await DTPService.GetAirAudioIsConnectedAsync(Guid);
                if (result)
                    writelog($"GetAirAudioIsConnectedAsync Success");
                else
                    writelog($"GetAirAudioIsConnectedAsync Fail");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"GetAirAudioIsConnectedAsync failed for {Guid} - Exception: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> GetAirAudioIsConnectedLeftAsync(string Guid)
        {
            try
            {
                var result = await DTPService.GetAirAudioIsConnectedLeftAsync(Guid);
                if (result)
                    writelog($"GetAirAudioIsConnectedLeftAsync Success");
                else
                    writelog($"GetAirAudioIsConnectedLeftAsync Fail");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"GetAirAudioIsConnectedLeftAsync failed for {Guid} - Exception: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> GetAirAudioIsConnectedRightAsync(string Guid)
        {
            try
            {
                var result = await DTPService.GetAirAudioIsConnectedRightAsync(Guid);
                if (result)
                    writelog($"GetAirAudioIsConnectedRightAsync Success");
                else
                    writelog($"GetAirAudioIsConnectedRightAsync Fail");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"GetAirAudioIsConnectedRightAsync failed for {Guid} - Exception: {ex.Message}");
                return false;
            }
        }
#endif
        public async Task<bool> SetAirAudioMicNoiseCancellationAsync(string Guid, bool newValue)
        {
            try
            {
                bool result = await DTPService.SetAirAudioMicNoiseCancellationAsync(Guid, newValue);
                if (result)
                    writelog($"SetAirAudioMicNoiseCancellationAsync Success");
                else
                    writelog($"SetAirAudioMicNoiseCancellationAsync Fail");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"SetAirAudioMicNoiseCancellationAsync failed for GUID: {Guid}, Error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SetAirAudioSidetoneAsync(string Guid, bool newValue)
        {
            try
            {
                bool result = await DTPService.SetAirAudioSidetoneAsync(Guid, newValue);
                if (result)
                    writelog($"SetAirAudioSidetoneAsync Success");
                else
                    writelog($"SetAirAudioSidetoneAsync Fail");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"SetAirAudioSidetoneAsync failed for GUID: {Guid}, Error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SetAirAudioBusyLightAsync(string Guid, bool newValue)
        {
            try
            {
                bool result = await DTPService.SetAirAudioBusyLightAsync(Guid, newValue);
                if (result)
                    writelog($"SetAirAudioBusyLightAsync Success");
                else
                    writelog($"SetAirAudioBusyLightAsync Fail");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"SetAirAudioBusyLightAsync failed for GUID: {Guid}, Error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SetAirAudioVoiceGuidanceAsync(string Guid, bool newValue)
        {
            try
            {
                bool result = await DTPService.SetAirAudioVoiceGuidanceAsync(Guid, newValue);
                if (result)
                    writelog($"SetAirAudioVoiceGuidanceAsync Success");
                else
                    writelog($"SetAirAudioVoiceGuidanceAsync Fail");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"SetAirAudioVoiceGuidanceAsync failed for GUID: {Guid}, Error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SetAirAudioSelectedPresetAsync(string Guid, int newValue)
        {
            try
            {
                bool result = await DTPService.SetAirAudioSelectedPresetAsync(Guid, newValue);
                if (result)
                    writelog($"SetAirAudioSelectedPresetAsync Success");
                else
                    writelog($"SetAirAudioSelectedPresetAsync Fail");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"SetAirAudioSelectedPresetAsync failed for GUID: {Guid}, Error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SetAirAudioSidetoneLevelAsync(string Guid, int newValue)
        {
            try
            {
                bool result = await DTPService.SetAirAudioSidetoneLevelAsync(Guid, newValue);
                if (result)
                    writelog($"SetAirAudioSidetoneLevelAsync Success");
                else
                    writelog($"SetAirAudioSidetoneLevelAsync Fail");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"SetAirAudioSidetoneLevelAsync failed for GUID: {Guid}, Error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SetAirAudioBandsGainAsync(string Guid, byte[] newValue)
        {
            try
            {
                bool result = await DTPService.SetAirAudioBandsGainAsync(Guid, newValue);
                if (result)
                    writelog($"SetAirAudioBandsGainAsync Success");
                else
                    writelog($"SetAirAudioBandsGainAsync Fail");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"SetAirAudioBandsGainAsync failed for GUID: {Guid}, Error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SetAirAudioBand1GainAsync(string Guid, int newValue)
        {
            try
            {
                bool result = await DTPService.SetAirAudioBand1GainAsync(Guid, newValue);
                if (result)
                    writelog($"SetAirAudioBand1GainAsync Success");
                else
                    writelog($"SetAirAudioBand1GainAsync Fail");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"SetAirAudioBand1GainAsync failed for GUID: {Guid}, Error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SetAirAudioBand2GainAsync(string Guid, int newValue)
        {
            try
            {
                bool result = await DTPService.SetAirAudioBand2GainAsync(Guid, newValue);
                if (result)
                    writelog($"SetAirAudioBand2GainAsync Success");
                else
                    writelog($"SetAirAudioBand2GainAsync Fail");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"SetAirAudioBand2GainAsync failed for GUID: {Guid}, Error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SetAirAudioBand3GainAsync(string Guid, int newValue)
        {
            try
            {
                bool result = await DTPService.SetAirAudioBand3GainAsync(Guid, newValue);
                if (result)
                    writelog($"SetAirAudioBand3GainAsync Success");
                else
                    writelog($"SetAirAudioBand3GainAsync Fail");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"SetAirAudioBand3GainAsync failed for GUID: {Guid}, Error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SetAirAudioBand4GainAsync(string Guid, int newValue)
        {
            try
            {
                bool result = await DTPService.SetAirAudioBand4GainAsync(Guid, newValue);
                if (result)
                    writelog($"SetAirAudioBand4GainAsync Success");
                else
                    writelog($"SetAirAudioBand4GainAsync Fail");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"SetAirAudioBand5GainAsync failed for GUID: {Guid}, Error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SetAirAudioBand5GainAsync(string Guid, int newValue)
        {
            try
            {
                bool result = await DTPService.SetAirAudioBand5GainAsync(Guid, newValue);
                if (result)
                    writelog($"SetAirAudioBand5GainAsync Success");
                else
                    writelog($"SetAirAudioBand5GainAsync Fail");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"SetAirAudioBand5GainAsync failed for GUID: {Guid}, Error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SetAirAudioAncModeAsync(string Guid, int newValue)
        {
            try
            {
                bool result = await DTPService.SetAirAudioAncModeAsync(Guid, newValue);
                if (result)
                    writelog($"SetAirAudioAncModeAsync Success");
                else
                    writelog($"SetAirAudioAncGainAsync Fail");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"SetAirAudioAncGainAsync failed for GUID: {Guid}, Error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SetAirAudioAncGainAsync(string Guid, int newValue)
        {
            try
            {
                bool result = await DTPService.SetAirAudioAncGainAsync(Guid, newValue);
                if (result)
                    writelog($"SetAirAudioAncGainAsync Success");
                else
                    writelog($"SetAirAudioAncGainAsync Fail");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"SetAirAudioAncGainAsync failed for GUID: {Guid}, Error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SetAirAudioWearDetectionAsync(string Guid, bool newValue)
        {
            try
            {
                bool result = await DTPService.SetAirAudioWearDetectionAsync(Guid, newValue);
                if (result)
                    writelog($"SetAirAudioFactoryResetAsync Success");
                else
                    writelog($"SetAirAudioFactoryResetAsync Fail");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"SetAirAudioFactoryResetAsync failed for GUID: {Guid}, Error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SetAirAudioFactoryResetAsync(string Guid, bool newValue)
        {
            try
            {
                bool result = await DTPService.SetAirAudioFactoryResetAsync(Guid, newValue);
                if (result)
                    writelog($"SetAirAudioFactoryResetAsync Success");
                else
                    writelog($"SetAirAudioFactoryResetAsync Fail");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"SetAirAudioFactoryResetAsync failed for GUID: {Guid}, Error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SetAirAudioIsBoomMicSupportedAsync(string Guid, bool newValue)
        {
            try
            {
                bool result = await DTPService.SetAirAudioIsBoomMicSupportedAsync(Guid, newValue);
                if (result)
                    writelog($"SetAirAudioIsBoomMicSupportedAsync Success");
                else
                    writelog($"SetAirAudioIsBoomMicSupportedAsync Fail");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"SetAirAudioIsBoomMicSupportedAsync failed for GUID: {Guid}, Error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SetAirAudioWearDetectionQuickPauseAsync(string Guid, int newValue)
        {
            try
            {
                bool result = await DTPService.SetAirAudioWearDetectionQuickPauseAsync(Guid, newValue);
                if (result)
                    writelog($"SetAirAudioWearDetectionQuickPauseAsync Success");
                else
                    writelog($"SetAirAudioWearDetectionQuickPauseAsync Fail");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"SetAirAudioWearDetectionQuickPauseAsync failed for GUID: {Guid}, Error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SetAirAudioWearDetectionSensitivityAsync(string Guid, int newValue)
        {
            try
            {
                bool result = await DTPService.SetAirAudioWearDetectionSensitivityAsync(Guid, newValue);
                if (result)
                    writelog($"SetAirAudioWearDetectionSensitivityAsync Success");
                else
                    writelog($"SetAirAudioWearDetectionSensitivityAsync Fail");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"SetAirAudioWearDetectionSensitivityAsync failed for GUID: {Guid}, Error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SetAirAudioMicNCIncomingAsync(string Guid, bool newValue)
        {
            try
            {
                bool result = await DTPService.SetAirAudioMicNCIncomingAsync(Guid, newValue);
                if (result)
                    writelog($"SetAirAudioMicNCIncomingAsync Success");
                else
                    writelog($"SetAirAudioMicNCIncomingAsync Fail");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"SetAirAudioMicNCIncomingAsync failed for GUID: {Guid}, Error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SetAirAudioUnPairAsync(string Guid, bool newValue)
        {
            try
            {
                bool result = await DTPService.SetAirAudioUnPairAsync(Guid, newValue);
                if (result)
                    writelog($"SetAirAudioUnPairAsync Success");
                else
                    writelog($"SetAirAudioUnPairAsync Fail");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"SetAirAudioUnPairAsync failed for GUID: {Guid}, Error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SetAirAudioIsWearDetectionPauseMusicEnabledAsync(string Guid, bool newValue)
        {
            try
            {
                bool result = await DTPService.SetAirAudioIsWearDetectionPauseMusicEnabledAsync(Guid, newValue);
                if (result)
                    writelog($"SetAirAudioIsWearDetectionPauseMusicEnabledAsync Success");
                else
                    writelog($"SetAirAudioIsWearDetectionPauseMusicEnabledAsync Fail");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"SetAirAudioIsWearDetectionPauseMusicEnabledAsync failed for GUID: {Guid}, Error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SetAirAudioIsWearDetectionMuteMicEnabledAsync(string Guid, bool newValue)
        {
            try
            {
                bool result = await DTPService.SetAirAudioIsWearDetectionMuteMicEnabledAsync(Guid, newValue);
                if (result)
                    writelog($"SetAirAudioIsWearDetectionMuteMicEnabledAsync Success");
                else
                    writelog($"SetAirAudioIsWearDetectionMuteMicEnabledAsync Fail");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"SetAirAudioIsWearDetectionMuteMicEnabledAsync failed for GUID: {Guid}, Error: {ex.Message}");
                return false;
            }
        }

        //public async Task<bool> GetDTPProxyPluginReady()
        //{
        //    return DTPService.GetDTPProxyPluginReady().Result;
        //}

        public async Task<string> GetAirAudioSerialNumberCaseAsync(string Guid)
        {
            try
            {
                var result = await DTPService.GetAirAudioSerialNumberCaseAsync(Guid);
                if (result != null)
                {
                    writelog($"GetAirAudioSerialNumberCaseAsync Success");
                    return result;
                }
                else
                {
                    writelog($"GetAirAudioSerialNumberCaseAsync value is null");
                    return null;
                }
            }
            catch (Exception ex)
            {
                writelog($"GetAirAudioSerialNumberCaseAsync failed for {Guid} - Exception: {ex.Message}");
                return null;
            }
        }

        public async Task<string> GetAirAudioBatteryStatusLeftAsync(string Guid)
        {
            try
            {
                var result = await DTPService.GetAirAudioBatteryStatusLeftAsync(Guid);
                if (result != null)
                {
                    writelog($"GetAirAudioBatteryStatusLeftAsync Success");
                    return result;
                }
                else
                {
                    writelog($"GetAirAudioBatteryStatusLeftAsync value is null");
                    return null;
                }
            }
            catch (Exception ex)
            {
                writelog($"GetAirAudioBatteryStatusLeftAsync failed for {Guid} - Exception: {ex.Message}");
                return null;
            }
        }

        public async Task<string> GetAirAudioBatteryStatusRightAsync(string Guid)
        {
            try
            {
                var result = await DTPService.GetAirAudioBatteryStatusRightAsync(Guid);
                if (result != null)
                {
                    writelog($"GetAirAudioBatteryStatusRightAsync Success");
                    return result;
                }
                else
                {
                    writelog($"GetAirAudioBatteryStatusRightAsync value is null");
                    return null;
                }
            }
            catch (Exception ex)
            {
                writelog($"GetAirAudioBatteryStatusRightAsync failed for {Guid} - Exception: {ex.Message}");
                return null;
            }
        }

        public async Task<string> GetAirAudioBatteryStatusCaseAsync(string Guid)
        {
            try
            {
                var result = await DTPService.GetAirAudioBatteryStatusCaseAsync(Guid);
                if (result != null)
                {
                    writelog($"GetAirAudioBatteryStatusCaseAsync Success");
                    return result;
                }
                else
                {
                    writelog($"GetAirAudioBatteryStatusCaseAsync value is null");
                    return null;
                }
            }
            catch (Exception ex)
            {
                writelog($"GetAirAudioBatteryStatusCaseAsync failed for {Guid} - Exception: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> GetAirAudioIsAutoPowerOffEnabledAsync(string Guid)
        {
            try
            {
                var result = await DTPService.GetAirAudioIsAutoPowerOffEnabledAsync(Guid);
                if (result)
                    writelog($"GetAirAudioIsAutoPowerOffEnabledAsync Success");
                else
                    writelog($"GetAirAudioIsAutoPowerOffEnabledAsync Fail");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"GetAirAudioIsAutoPowerOffEnabledAsync failed for {Guid} - Exception: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> GetAirAudioMicNoiseCancellationAsync(string Guid)
        {
            try
            {
                var result = await DTPService.GetAirAudioMicNoiseCancellationAsync(Guid);
                if (result)
                    writelog($"GetAirAudioIsMicNoiseCancellationAsync Success");
                else
                    writelog($"GetAirAudioIsMicNoiseCancellationAsync Fail");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"GetAirAudioIsMicNoiseCancellationAsync failed for {Guid} - Exception: {ex.Message}");
                return false;
            }
        }

        public async Task<int> GetAirAudioWearDetectionQuickPauseAsync(string Guid)
        {
            try
            {
                var result = await DTPService.GetAirAudioWearDetectionQuickPauseAsync(Guid);
                if (result != -1)
                    writelog($"GetAirAudioWearDetectionQuickPauseAsync Success");
                else
                    writelog($"GetAirAudioWearDetectionQuickPauseAsync Fail");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"GetAirAudioWearDetectionQuickPauseAsync failed for {Guid} - Exception: {ex.Message}");
                return -1;
            }
        }

        public async Task<bool> GetAirAudioIsWearDetectionAnswerCallsEnabledAsync(string Guid)
        {
            try
            {
                var result = await DTPService.GetAirAudioIsWearDetectionAnswerCallsEnabledAsync(Guid);
                if (result)
                    writelog($"GetAirAudioIsWearDetectionAnswerCallsEnabledAsync Success");
                else
                    writelog($"GetAirAudioIsWearDetectionAnswerCallsEnabledAsync Fail");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"GetAirAudioIsWearDetectionAnswerCallsEnabledAsync failed for {Guid} - Exception: {ex.Message}");
                return false;
            }
        }

        public async Task<int> GetAirAudioAutoPowerOffIntervalAsync(string Guid)
        {
            try
            {
                var result = await DTPService.GetAirAudioAutoPowerOffIntervalAsync(Guid);
                if (result != -1)
                    writelog($"GetAirAudioAutoPowerOffIntervalAsync Success");
                else
                    writelog($"GetAirAudioAutoPowerOffIntervalAsync Fail");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"GetAirAudioAutoPowerOffIntervalAsync failed for {Guid} - Exception: {ex.Message}");
                return -1;
            }
        }

        public async Task<int> GetAirAudioBatteryLevelLeftAsync(string Guid)
        {
            try
            {
                var result = await DTPService.GetAirAudioBatteryLevelLeftAsync(Guid);
                if (result != -1)
                    writelog($"GetAirAudioBatteryLevelLeftAsync Success");
                else
                    writelog($"GetAirAudioBatteryLevelLeftAsync Fail");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"GetAirAudioBatteryLevelLeftAsync failed for {Guid} - Exception: {ex.Message}");
                return -1;
            }
        }

        public async Task<int> GetAirAudioBatteryLevelRightAsync(string Guid)
        {
            try
            {
                var result = await DTPService.GetAirAudioBatteryLevelRightAsync(Guid);
                if (result != -1)
                    writelog($"GetAirAudioBatteryLevelRightAsync Success");
                else
                    writelog($"GetAirAudioBatteryLevelRightAsync Fail");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"GetAirAudioBatteryLevelRightAsync failed for {Guid} - Exception: {ex.Message}");
                return -1;
            }
        }

        public async Task<int> GetAirAudioBatteryLevelCaseAsync(string Guid)
        {
            try
            {
                var result = await DTPService.GetAirAudioBatteryLevelCaseAsync(Guid);
                if (result != -1)
                    writelog($"GetAirAudioBatteryLevelCaseAsync Success");
                else
                    writelog($"GetAirAudioBatteryLevelCaseAsync Fail");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"GetAirAudioBatteryLevelCaseAsync failed for {Guid} - Exception: {ex.Message}");
                return -1;
            }
        }
        public async Task<int> GetAirAudioMaxAllowedPariedHost(string Guid)
        {
            try
            {
                var result = await DTPService.GetAirAudioMaxAllowedPariedHost(Guid);
                if (result != -1)
                    writelog($"GetAirAudioMaxAllowedPariedHost Success");
                else
                    writelog($"GetAirAudioMaxAllowedPariedHost Fail");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"GetAirAudioMaxAllowedPariedHost failed for {Guid} - Exception: {ex.Message}");
                return -1;
            }
        }
        public async Task<bool> SetFactoryResetAsyncValueForAirAudioAsync(string Guid, bool newValue)
        {
            try
            {
                bool result = await DTPService.SetFactoryResetAsyncValueForAirAudioAsync(Guid, newValue);
                if (result)
                    writelog($"SetFactoryResetAsyncValueForAirAudioAsync Success");
                else
                    writelog($"SetFactoryResetAsyncValueForAirAudioAsync Fail");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"SetFactoryResetAsyncValueForAirAudioAsync failed for GUID: {Guid}, Error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SetAirAudioIsAutoPowerOffEnabledAsync(string Guid, bool newValue)
        {
            try
            {
                bool result = await DTPService.SetAirAudioIsAutoPowerOffEnabledAsync(Guid, newValue);
                if (result)
                    writelog($"SetAirAudioIsAutoPowerOffEnabledAsync Success");
                else
                    writelog($"SetAirAudioIsAutoPowerOffEnabledAsync Fail");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"SetAirAudioIsAutoPowerOffEnabledAsync failed for GUID: {Guid}, Error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SetAirAudioIsWearDetectionAnswerCallsEnabledAsync(string Guid, bool newValue)
        {
            try
            {
                bool result = await DTPService.SetAirAudioIsWearDetectionAnswerCallsEnabledAsync(Guid, newValue);
                if (result)
                    writelog($"SetAirAudioIsWearDetectionAnswerCallsEnabledAsync Success");
                else
                    writelog($"SetAirAudioIsWearDetectionAnswerCallsEnabledAsync Fail");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"SetAirAudioIsWearDetectionAnswerCallsEnabledAsync failed for GUID: {Guid}, Error: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SetAirAudioAutoPowerOffIntervalAsync(string Guid, int newValue)
        {
            try
            {
                bool result = await DTPService.SetAirAudioAutoPowerOffIntervalAsync(Guid, newValue);
                if (result)
                    writelog($"SetAirAudioAutoPowerOffIntervalAsync Success");
                else
                    writelog($"SetAirAudioAutoPowerOffIntervalAsync Fail");
                return result;
            }
            catch (Exception ex)
            {
                writelog($"SetAirAudioAutoPowerOffIntervalAsync failed for GUID: {Guid}, Error: {ex.Message}");
                return false;
            }
        }
    }
}
