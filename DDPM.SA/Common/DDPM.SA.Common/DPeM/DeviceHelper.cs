using DPeMPublic.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace DDPM.SA.Common
{
    [Serializable]
    public class DeviceHelper
    {
        public List<DeviceInfo> deviceInfo { get; set; }
        public string DPeMSDKVersion { get; set; }
        public string DCFVersion { get; set; }
        public string DPeMSubAgentVersion { get; set; }
        public string IsdDriverVersion { get; set; } = "";

    public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            if (deviceInfo.Where(x => x.IsConnected).Count() > 0)
            {
                Console.WriteLine("Select Device Serial Number");
                sb.AppendLine("*******  Connected Device List");
            }
            else
            {
                sb.AppendLine("*******  No Device Connected");
            }
            int index = 1;
            foreach (var item in deviceInfo.Where(x => x.IsConnected))
            {
                PrintDeciveName(sb, item, index++);
            }
            return sb.ToString();
        }

        public string PrintDeviceInfo(int index)
        {
            StringBuilder sb = new StringBuilder();

            if (index > 0)
            {
                List<DeviceInfo> items = deviceInfo.Where(x => x.IsConnected).ToList();
                index--;
                AppendDeviceLine(sb, items[index]);
                if (items[index].PhysicalDeviceType == DeviceType.PhysicalDongle)
                {
                    sb.AppendLine($"Unpair(set)           :set Unpair");
                }
            }

            return sb.ToString();
        }

        public string PrintVersionInfo()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine($"Version Info----------");
            stringBuilder.AppendLine($"DPeM SDK: {DPeMSDKVersion}");
            stringBuilder.AppendLine($"Dell Client Framework: {DCFVersion}");
            stringBuilder.AppendLine($"DPM SubAgent: {DPeMSubAgentVersion}");
            stringBuilder.AppendLine("----------");
            return stringBuilder.ToString();
        }

        private static void PrintDeciveName(StringBuilder sb, DeviceInfo item, int index)
        {
            sb.AppendLine($"{index}.----------");
            sb.AppendLine($"{nameof(item.ID)}                         : {item.ID}");
            sb.AppendLine($"{nameof(item.Name)}                       : {item.Name}");
            sb.AppendLine("----------");
        }

        private static void AppendDeviceLine(StringBuilder sb, DeviceInfo item)
        {
            if (item.LogicalDeviceType == DeviceType.LogicalKeyboard.ToString())
            {
                AppendKeyboardDeviceLine(sb, item);
            }
            else if (item.LogicalDeviceType == DeviceType.LogicalMouse.ToString())
            {
                AppendMouseDeviceLine(sb, item);
            }
            else if (item.LogicalDeviceType == DeviceType.LogicalWiredAudio.ToString())
            {
                AppendWiredAudioDeviceLine(sb, item);
            }
            //Dean 0626 fix SAST issue
            //else if (item.LogicalDeviceType == DeviceType.LogicalHeadset.ToString())
            //{
            //    AppendWiredAudioDeviceLine(sb, item);
            //}
            else if (item.LogicalDeviceType == DeviceType.LogicalHeadset.ToString())
            {
                AppendHeadsetDeviceLine(sb, item);
            }
            else if (item.LogicalDeviceType == DeviceType.LogicalWebcam.ToString())
            {
                AppendWebcamDeviceLine(sb, item);
            }
            else if (item.LogicalDeviceType == DeviceType.LogicalPen.ToString())
            {
                AppendPenDeviceLine(sb, item);
            }
            else if (item.LogicalDeviceType == DeviceType.LogicalDock.ToString())
            {
                AppendDockDeviceLine(sb, item);
            }
            else
            {
                AppendCommonDeviceLine(sb, item);
            }
        }

        private static void AppendCommonDeviceLine(StringBuilder sb, DeviceInfo item)
        {
            sb.AppendLine($"{nameof(item.Name)} (get)                                     : {item.Name.ToString()}");
            sb.AppendLine($"{nameof(item.InterfaceType)} (get)                            : {item.InterfaceType.ToString()}");
            sb.AppendLine($"{nameof(item.PhysicalDeviceType)} (get)                       : {item.PhysicalDeviceType}");
            sb.AppendLine($"{nameof(item.Type)} (get)                                     : {item.Type.ToString()}");
            sb.AppendLine($"{nameof(item.ID)} (get)                                       : {item.ID}");
            sb.AppendLine($"{nameof(item.ModelNumber)} (get)                              : {item.ModelNumber}");
            sb.AppendLine($"{nameof(item.PluginId)} (get)                                 : {item.PluginId}");
            sb.AppendLine($"{nameof(item.OdmId)} (get)                                    : {item.OdmId}");
            sb.AppendLine($"{nameof(item.InstanceNumber)} (get)                           : {item.InstanceNumber}");
            sb.AppendLine($"{nameof(item.InstanceId)} (get)                               : {item.InstanceId.ToString("X")}");
            sb.AppendLine($"{nameof(item.ColorCode)} (get)                                : {item.ColorCode}");
            sb.AppendLine($"{nameof(item.ThumbnailImageRawData)}  (get)                   : {item.ThumbnailImageRawData}");
            sb.AppendLine($"{nameof(item.FirmwareVersion)} (get)                          : {Regex.Replace(item.FirmwareVersion, ".{1}", "$0.").Substring(0, (item.FirmwareVersion.Length * 2) - 1)}");
        }

        private static void AppendKeyboardDeviceLine(StringBuilder sb, DeviceInfo item)
        {
            AppendCommonDeviceLine(sb, item);

            sb.AppendLine($"{nameof(item.IsBatteryLevelSupported)} (get)                  : {item.IsBatteryLevelSupported}");
            if (item.IsBatteryLevelSupported)
            {
                sb.AppendLine($"{nameof(item.BatteryLevel)} (get)                         : {item.BatteryLevel}%");
                sb.AppendLine($"{nameof(item.BatteryStatus)} (get)                        : {item.BatteryStatus}");
            }
            sb.AppendLine($"{nameof(item.TotalNumberOfPairedHostName)} (get)              : {item.TotalNumberOfPairedHostName}");

            for (int i = 0; i < item.TotalNumberOfPairedHostName; i++)
            {
                sb.AppendLine($"PairHostName{i + 1}(get)                                     : {item.PairedHostNames[0]}");
            }

            //if (item.TotalNumberOfPairedHostName > 0)
            //    sb.AppendLine($"{nameof(item.PairedHostName1)}(get)                         : {item.PairedHostName1}");
            //if (item.TotalNumberOfPairedHostName > 1)
            //    sb.AppendLine($"{nameof(item.PairedHostName2)} (get)                        : {item.PairedHostName2}");
            //if (item.TotalNumberOfPairedHostName > 2)
            //    sb.AppendLine($"{nameof(item.PairedHostName3)} (get)                        : {item.PairedHostName3}");

            sb.AppendLine($"{nameof(item.IsCollabsKeysSupported)} (get)                   : {item.IsCollabsKeysSupported}");

            if (item.IsCollabsKeysSupported)
            {
                sb.AppendLine($"{nameof(item.IsCollaborationKeyEnable)} (get,set)              : {item.IsCollaborationKeyEnable}");
                sb.AppendLine($"{nameof(item.IsCollaborationCameraEnable)} (get,set)           : {item.IsCollaborationCameraEnable}");
                sb.AppendLine($"{nameof(item.IsCollaborationScreenShareEnable)} (get,set)      : {item.IsCollaborationScreenShareEnable}");
                sb.AppendLine($"{nameof(item.IsCollaborationChatEnable)} (get,set)             : {item.IsCollaborationChatEnable}");
                sb.AppendLine($"{nameof(item.IsCollaborationMicEnable)} (get,set)              : {item.IsCollaborationMicEnable}");
                sb.AppendLine($"{nameof(item.IsCollaborationBlinkEffectEnable)} (get,set)      : {item.IsCollaborationBlinkEffectEnable}");
                sb.AppendLine($"{nameof(item.IsCollaborationDoubleTapEnable)} (get,set)        : {item.IsCollaborationDoubleTapEnable}");
            }
            sb.AppendLine($"{nameof(item.IsIlluminationSupported)}  (get)                 : {item.IsIlluminationSupported}");
            if (item.IsIlluminationSupported)
            {
                sb.AppendLine($"{nameof(item.BackLightingControls)} (get,set)                     : {item.BackLightingControls}");
                sb.AppendLine($"{nameof(item.BackLightingLevel)} (get,set)                        : {item.BackLightingLevel}");
            }
            sb.AppendLine("----------");
        }

        private static void AppendMouseDeviceLine(StringBuilder sb, DeviceInfo item)
        {
            AppendCommonDeviceLine(sb, item);

            sb.AppendLine($"{nameof(item.IsBatteryLevelSupported)} (get)                  : {item.IsBatteryLevelSupported}");
            if (item.IsBatteryLevelSupported)
            {
                sb.AppendLine($"{nameof(item.BatteryLevel)} (get)                           : {item.BatteryLevel}%");
                sb.AppendLine($"{nameof(item.BatteryStatus)} (get)                          : {item.BatteryStatus}");
            }
            sb.AppendLine($"{nameof(item.TotalNumberOfPairedHostName)} (get)              : {item.TotalNumberOfPairedHostName}");
            for (int i = 0; i < item.TotalNumberOfPairedHostName; i++)
            {
                sb.AppendLine($"PairHostName{i + 1}(get)                                     : {item.PairedHostNames[0]}");
            }

            if (item.TotalNumberOfPairedHostName > 0)
                sb.AppendLine($"{nameof(item.PairedHostName1)}(get)                         : {item.PairedHostName1}");
            if (item.TotalNumberOfPairedHostName > 1)
                sb.AppendLine($"{nameof(item.PairedHostName2)} (get)                        : {item.PairedHostName2}");
            if (item.TotalNumberOfPairedHostName > 2)
                sb.AppendLine($"{nameof(item.PairedHostName3)} (get)                          : {item.PairedHostName3}");
            sb.AppendLine($"{nameof(item.MousePrimaryButton)} (get,set)                         : {item.MousePrimaryButton}");

            sb.AppendLine($"{nameof(item.IsDPILevelSupported)} (get)                      : {item.IsDPILevelSupported}");

            if (item.IsDPILevelSupported)
            {
                sb.AppendLine($"{nameof(item.DpiLevel)}  (get,set)                          : {item.DpiLevel}");
                sb.AppendLine($"{nameof(item.DpiLevelValues)} (get)                         : {String.Join(",", item.DpiLevelValues)}");
            }

            sb.AppendLine($"{nameof(item.IsDPIValueSupported)} (get)                      : {item.IsDPIValueSupported}");
            if (item.IsDPIValueSupported)
            {
                sb.AppendLine($"{nameof(item.DpiValue)} (get,set)                           : {item.DpiValue}");
                sb.AppendLine($"{nameof(item.DpiMin)}  (get)                                : {item.DpiMin}");
                sb.AppendLine($"{nameof(item.DpiMax)}  (get)                                : {item.DpiMax}");
                sb.AppendLine($"{nameof(item.DpiDelta)} (get)                               : {item.DpiDelta}");
            }
            sb.AppendLine($"{nameof(item.IsTouchScrollSensitivitySupported)}(get)         : {item.IsTouchScrollSensitivitySupported}");
            if (item.IsTouchScrollSensitivitySupported)
            {
                sb.AppendLine($"{nameof(item.TouchScrollSensitivityLevel)}  (get,set)     : {item.TouchScrollSensitivityLevel}");
                sb.AppendLine($"{nameof(item.TouchSensitivityLevelValue)} (get)       : {item.TouchSensitivityLevelValue}");
            }
            sb.AppendLine("----------");
        }

        private static void AppendWiredAudioDeviceLine(StringBuilder sb, DeviceInfo item)
        {
            AppendCommonDeviceLine(sb, item);

            sb.AppendLine($"{nameof(item.MuteStatus)} (get)                                 : {item.MuteStatus}");
            sb.AppendLine($"{nameof(item.IsWiredAudioIMicNSEnable)} (get, set)                  : {item.IsWiredAudioIMicNSEnable}");
            sb.AppendLine($"{nameof(item.IsWiredAudioMicMuteSoundEnable)} (get, set)            : {item.IsWiredAudioMicMuteSoundEnable}");
            sb.AppendLine($"{nameof(item.WiredAudioVolumeAdjustmentTone)} (get, set)    : {item.WiredAudioVolumeAdjustmentTone}");
            sb.AppendLine("----------");
        }

        private static void AppendWebcamDeviceLine(StringBuilder sb, DeviceInfo item)
        {
            AppendCommonDeviceLine(sb, item);

            sb.AppendLine($"{nameof(item.DeviceSymbolicLink)} (get)                       : {item.DeviceSymbolicLink}");
            sb.AppendLine($"{nameof(item.ParentDevInstanceId)} (get)                      : {item.ParentDevInstanceId}");
            sb.AppendLine($"{nameof(item.IsESISupported)} (get)                           : {item.IsESISupported}");
            sb.AppendLine($"{nameof(item.SupportedProperties)} (get)                      : {string.Join("," + Environment.NewLine + string.Empty.PadLeft(6, '\t') + " ", item.SupportedProperties.Select(i => i.Replace("'", "''")))}");
            sb.AppendLine($"{nameof(item.FOVValues)} (get)                                : {string.Join("," + Environment.NewLine + string.Empty.PadLeft(6, '\t') + " ", item.FOVValues.Select(i => i.Replace("'", "''")))}");
            sb.AppendLine($"{nameof(item.SupportedResolutions)} (get)                     : {string.Join("," + Environment.NewLine + string.Empty.PadLeft(6, '\t') + " ", item.SupportedResolutions.Select(i => i.Replace("'", "''")))}");
            sb.AppendLine($"{nameof(item.SupportedFeatures)} (get)                        : {item.SupportedFeatures}");
            sb.AppendLine($"{nameof(item.CurrentFeatures)} (get)                          : {item.CurrentFeatures}");
            sb.AppendLine($"{nameof(item.IsMicEnumerationSupported)} (get)                : {item.IsMicEnumerationSupported}");
            sb.AppendLine($"{nameof(item.IsMicEnumerationOn)} (get,set)                   : {item.IsMicEnumerationOn}");
            sb.AppendLine($"{nameof(item.IsWindowsHelloSupported)} (get)                  : {item.IsWindowsHelloSupported}");
            sb.AppendLine($"{nameof(item.HasWindowsHelloPowerConstraint)} (get)           : {item.HasWindowsHelloPowerConstraint}");
            sb.AppendLine("----------");
        }

        private static void AppendHeadsetDeviceLine(StringBuilder sb, DeviceInfo item)
        {
            AppendCommonDeviceLine(sb, item);

            sb.AppendLine($"{nameof(item.IsReady)} (get)                                : {item.IsReady}");
            sb.AppendLine($"{nameof(item.IsDirty)} (get)                                : {item.IsDirty}");
            sb.AppendLine($"{nameof(item.IsMicNoiseCancellationSupported)} (get)        : {item.IsMicNoiseCancellationSupported}");
            sb.AppendLine($"{nameof(item.IsSidetoneSupported)} (get)                    : {item.IsSidetoneSupported}");
            sb.AppendLine($"{nameof(item.IsBusyLightSupported)} (get)                   : {item.IsBusyLightSupported}");
            sb.AppendLine($"{nameof(item.IsVoiceGuidanceSupported)} (get)               : {item.IsVoiceGuidanceSupported}");
            sb.AppendLine($"{nameof(item.IsPresetsSupported)} (get)                     : {item.IsPresetsSupported}");
            sb.AppendLine($"{nameof(item.IsEqualizerSupported)} (get)                   : {item.IsEqualizerSupported}");
            sb.AppendLine($"{nameof(item.ConnectionType)} (get)                         : {item.ConnectionType}");
            sb.AppendLine($"{nameof(item.IsANCSupported)} (get)                         : {item.IsANCSupported}");
            sb.AppendLine($"{nameof(item.AncMode)} (get, set)                           : {item.AncMode}");
            sb.AppendLine($"{nameof(item.AncGain)} (get, set)                           : {item.AncGain}");
            sb.AppendLine($"{nameof(item.WearDetection)} (get, set)                     : {item.WearDetection}");
            sb.AppendLine($"{nameof(item.IsMicNCIncomingSupported)} (get)               : {item.IsMicNCIncomingSupported}");
            sb.AppendLine($"{nameof(item.IsWearDetectionSupported)} (get)               : {item.IsWearDetectionSupported}");
            sb.AppendLine($"{nameof(item.IsWearDetectionSensitivitySupported)} (get)    : {item.IsWearDetectionSensitivitySupported}");
            sb.AppendLine($"{nameof(item.IsWearDetectionPauseMusicSupported)} (get)     : {item.IsWearDetectionPauseMusicSupported}");
            sb.AppendLine($"{nameof(item.IsWearDetectionMuteMicSupported)} (get)        : {item.IsWearDetectionMuteMicSupported}");
            sb.AppendLine($"{nameof(item.IsWearDetectionQuickPauseSupported)} (get)     : {item.IsWearDetectionQuickPauseSupported}");
            sb.AppendLine($"{nameof(item.IsWearDetectionChecked)} (get)                 : {item.IsWearDetectionChecked}");
            sb.AppendLine($"{nameof(item.IsPauseMusicChecked)} (get)                    : {item.IsPauseMusicChecked}");
            sb.AppendLine($"{nameof(item.IsMuteMicrophoneChecked)} (get)                : {item.IsMuteMicrophoneChecked}");
            sb.AppendLine($"{nameof(item.IsQuickPauseChecked)} (get)                    : {item.IsQuickPauseChecked}");
            sb.AppendLine($"{nameof(item.MicNoiseCancellation)} (get, set)              : {item.MicNoiseCancellation}");
            sb.AppendLine($"{nameof(item.MicNCIncoming)} (get)                          : {item.MicNCIncoming}");
            sb.AppendLine($"{nameof(item.Sidetone)} (get, set)                          : {item.Sidetone}");
            sb.AppendLine($"{nameof(item.BusyLight)} (get, set)                         : {item.BusyLight}");
            sb.AppendLine($"{nameof(item.VoiceGuidance)} (get, set)                     : {item.VoiceGuidance}");
            sb.AppendLine($"{nameof(item.SelectedPreset)} (get, set)                    : {item.SelectedPreset}");
            sb.AppendLine($"{nameof(item.SidetoneLevel)} (get)                          : {item.SidetoneLevel}");
            sb.AppendLine($"{nameof(item.MuteStatus)} (get)                             : {item.MuteStatus}");
            sb.AppendLine($"{nameof(item.Band1Gain)} (get, set)                         : {item.Band1Gain}");
            sb.AppendLine($"{nameof(item.Band2Gain)} (get, set)                         : {item.Band2Gain}");
            sb.AppendLine($"{nameof(item.Band3Gain)} (get, set)                         : {item.Band3Gain}");
            sb.AppendLine($"{nameof(item.Band4Gain)} (get, set)                         : {item.Band4Gain}");
            sb.AppendLine($"{nameof(item.Band5Gain)} (get, set)                         : {item.Band5Gain}");
            sb.AppendLine("----------");
        }

        private static void AppendPenDeviceLine(StringBuilder sb, DeviceInfo item)
        {
            AppendCommonDeviceLine(sb, item);
            sb.AppendLine($"{nameof(item.IsBatteryLevelSupported)} (get)                  : {item.IsBatteryLevelSupported}");
            if (item.IsBatteryLevelSupported)
            {
                sb.AppendLine($"{nameof(item.BatteryLevel)} (get)                           : {item.BatteryLevel}%");
                sb.AppendLine($"{nameof(item.BatteryStatus)} (get)                          : {item.BatteryStatus}");
            }
            sb.AppendLine($"{nameof(item.IsBLE)} (get)                                    : {item.IsBLE}");
            sb.AppendLine($"{nameof(item.IsdDriverVersion)} (get)                         : {item.IsdDriverVersion}");
            sb.AppendLine($"{nameof(item.IsdServiceVersion)} (get)                        : {item.IsdServiceVersion}");
            sb.AppendLine("----------");
        }

        private static void AppendDockDeviceLine(StringBuilder sb, DeviceInfo item)
        {
            AppendCommonDeviceLine(sb, item);

            sb.AppendLine($"{nameof(item.MonitorCount)} (get)                            : {item.MonitorCount}");

            sb.AppendLine($"{nameof(item.DockData)} (get)                                : {item.DockData}");

            sb.AppendLine($"{nameof(item.DockInfo)} (get)                                : {item.DockInfo}");

            sb.AppendLine($"{nameof(item.DockType)} (get)                                : {item.DockType}");

            sb.AppendLine($"{nameof(item.DockServiceTag)} (get)                             : {item.DockServiceTag}");

            sb.AppendLine($"{nameof(item.DockPackageFwVersion)} (get)                    : {item.DockPackageFwVersion}");

            sb.AppendLine($"{nameof(item.DockFwUpdateStatus)} (get)                      : {item.DockFwUpdateStatus}");

            sb.AppendLine($"{nameof(item.DockTBTConnectionStatus)} (get)                 : {item.DockTBTConnectionStatus}");

            sb.AppendLine("----------");
        }
    }
}