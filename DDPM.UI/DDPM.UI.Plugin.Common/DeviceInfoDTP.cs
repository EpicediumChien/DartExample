using DDPM.SA.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDPM.UI.Plugin.Common
{
    /// <summary>
    /// DeviceInfo + DTP
    /// </summary>
    #region DeviceInfo  DTP

    public class DeviceInfoDTP : DeviceInfo
    {
        private bool _isAnswerCallSupported;
        private bool _isAnswerCall;

        public bool AnswerCall
        {
            get => _isAnswerCall;
            set
            {
                _isAnswerCall = value;
                OnPropertyChanged();
            }
        }

        public bool IsAnswerCallSupported
        {
            get => _isAnswerCallSupported;
            set
            {
                _isAnswerCallSupported = value;
                OnPropertyChanged();
            }
        }

        private bool _wearDetectionFromDTP;
        private bool _isWearDetectionMuteMicEnabledFromDTP;
        private bool _isWearDetectionPauseMusicEnableFromDTP;
        private int _wearDetectionQuickPauseAsyncFromDTP;
        private int _wearDetectionSensitivityFromDTP;

        public bool WearDetectionFromDTP
        {
            get => _wearDetectionFromDTP;
            set
            {
                _wearDetectionFromDTP = value;
                OnPropertyChanged();
            }
        }

        public bool IsWearDetectionMuteMicEnabledFromDTP
        {
            get => _isWearDetectionMuteMicEnabledFromDTP;
            set
            {
                _isWearDetectionMuteMicEnabledFromDTP = value;
                OnPropertyChanged();
            }
        }

        public bool IsWearDetectionPauseMusicEnableFromDTP
        {
            get => _isWearDetectionPauseMusicEnableFromDTP;
            set
            {
                _isWearDetectionPauseMusicEnableFromDTP = value;
                OnPropertyChanged();
            }
        }

        public int WearDetectionQuickPauseAsyncFromDTP
        {
            get => _wearDetectionQuickPauseAsyncFromDTP;
            set
            {
                _wearDetectionQuickPauseAsyncFromDTP = value;
                OnPropertyChanged();
            }
        }

        public int WearDetectionSensitivityFromDTP
        {
            get => _wearDetectionSensitivityFromDTP;
            set
            {
                _wearDetectionSensitivityFromDTP = value;
                OnPropertyChanged();
            }
        }
    }
    public class HeadsetDeviceDefaultSettings
    {
        public int AncMode { get; set; } = 0;
        public int AncGain { get; set; } = 0;
        public bool BusyLight { get; set; } = false;
        public bool MicNoiseCancellation { get; set; } = false;
        public bool Sidetone { get; set; } = false;
        public bool VoiceGuidance { get; set; } = false;
        public int SelectedPreset { get; set; } = 1;
        public int Band1Gain { get; set; } = 0;
        public int Band2Gain { get; set; } = 0;
        public int Band3Gain { get; set; } = 0;
        public int Band4Gain { get; set; } = 0;
        public int Band5Gain { get; set; } = 0;
        public bool MicNCIncoming { get; set; } = false;
        public bool WearDetectionFromDTP { get; set; } = false;
        public bool IsWearDetectionPauseMusicEnableFromDTP { get; set; } = false;
        public bool IsWearDetectionMuteMicEnabledFromDTP { get; set; } = false;
        public int WearDetectionQuickPauseAsyncFromDTP { get; set; } = 0;
        public int WearDetectionSensitivityFromDTP { get; set; } = 0;
        public bool AnswerCall { get; set; } = false;
    }
    public class AirAudioDeviceDefaultSettings : HeadsetDeviceDefaultSettings 
    {
    
    }
    //***********************************************************************
    //DeviceInfoDTP.AncMode.....................= 2
    //DeviceInfoDTP.AncGain.....................= 3
    //DeviceInfoDTP.BusyLight...................= True
    //DeviceInfoDTP.MicNoiseCancellation.............= True
    //DeviceInfoDTP.Sidetone...................= False
    //DeviceInfoDTP.VoiceGuidance..............= True
    //DeviceInfoDTP.SelectedPreset.............= 1
    //DeviceInfoDTP.Band1Gain..................= 0
    //DeviceInfoDTP.Band2Gain..................= 0
    //DeviceInfoDTP.Band3Gain..................= 0
    //DeviceInfoDTP.Band4Gain..................= 0
    //DeviceInfoDTP.Band5Gain..................= 0
    //DeviceInfoDTP.MicNCIncoming.............= False
    //DeviceInfoDTP.GetIsWearDetectionSupportedAsync.............= True
    //DeviceInfoDTP.WearDetectionFromDTP.............= True
    //DeviceInfoDTP.IsWearDetectionPauseMusicEnableFromDTP.............= True
    //DeviceInfoDTP.IsWearDetectionMuteMicEnabledFromDTP.............= True
    //DeviceInfoDTP.WearDetectionQuickPauseAsyncFromDTP.............= True
    //DeviceInfoDTP.WearDetectionSensitivityFromDTP.............= True
    //GetIsBoomMicSupportedAsync.............NO
    //DetectPageShow...WL7024


    //GetIsANCSupportedAsync..............................NO
    //DeviceInfoDTP.BusyLight...................= True
    //DeviceInfoDTP.MicNoiseCancellation.............= True
    //DeviceInfoDTP.Sidetone...................= True
    //GetIsVoiceGuidanceSupportedAsync....................NO
    //DeviceInfoDTP.SelectedPreset.............= 1
    //DeviceInfoDTP.Band1Gain..................= 0
    //DeviceInfoDTP.Band2Gain..................= 0
    //DeviceInfoDTP.Band3Gain..................= 0
    //DeviceInfoDTP.Band4Gain..................= 0
    //DeviceInfoDTP.Band5Gain..................= 0
    //GetIsMicNCIncomingSupportedAsync.............NO
    //GetIsWearDetectionSupportedAsync.............NO
    //DeviceInfoDTP.AnswerCall.............= False
    //DetectPageShow...WH3024

    //GetIsANCSupportedAsync..............................NO
    //DeviceInfoDTP.BusyLight...................= True
    //DeviceInfoDTP.MicNoiseCancellation.............= True
    //DeviceInfoDTP.Sidetone...................= True
    //DeviceInfoDTP.VoiceGuidance..............= True
    //DeviceInfoDTP.SelectedPreset.............= 1
    //DeviceInfoDTP.Band1Gain..................= 0
    //DeviceInfoDTP.Band2Gain..................= 0
    //DeviceInfoDTP.Band3Gain..................= 0
    //DeviceInfoDTP.Band4Gain..................= 0
    //DeviceInfoDTP.Band5Gain..................= 0
    //GetIsMicNCIncomingSupportedAsync.............NO
    //GetIsWearDetectionSupportedAsync.............NO
    //DeviceInfoDTP.AnswerCall.............= False
    //DetectPageShow...WL3024


    //DeviceInfoDTP.AncMode.....................= 0
    //DeviceInfoDTP.AncGain.....................= 3
    //DeviceInfoDTP.BusyLight...................= True
    //DeviceInfoDTP.MicNoiseCancellation.............= True
    //DeviceInfoDTP.Sidetone...................= True
    //DeviceInfoDTP.VoiceGuidance..............= True
    //DeviceInfoDTP.SelectedPreset.............= 1
    //DeviceInfoDTP.Band1Gain..................= 0
    //DeviceInfoDTP.Band2Gain..................= 0
    //DeviceInfoDTP.Band3Gain..................= 0
    //DeviceInfoDTP.Band4Gain..................= 0
    //DeviceInfoDTP.Band5Gain..................= 0
    //GetIsMicNCIncomingSupportedAsync.............NO
    //DeviceInfoDTP.GetIsWearDetectionSupportedAsync.............= True
    //DeviceInfoDTP.WearDetectionFromDTP.............= True
    //DeviceInfoDTP.IsWearDetectionPauseMusicEnableFromDTP.............= True
    //DeviceInfoDTP.IsWearDetectionMuteMicEnabledFromDTP.............= True
    //DeviceInfoDTP.WearDetectionQuickPauseAsyncFromDTP.............= True
    //DeviceInfoDTP.WearDetectionSensitivityFromDTP.............= True
    //DeviceInfoDTP.AnswerCall.............= False
    //DetectPageShow...WL5024



    //DeviceInfoDTP.AncMode.....................= 1
    //DeviceInfoDTP.AncGain.....................= 3
    //DeviceInfoDTP.BusyLight...................= True
    //DeviceInfoDTP.MicNoiseCancellation.............= True
    //DeviceInfoDTP.Sidetone...................= True
    //DeviceInfoDTP.VoiceGuidance..............= True
    //DeviceInfoDTP.SelectedPreset.............= 1
    //DeviceInfoDTP.Band1Gain..................= 0
    //DeviceInfoDTP.Band2Gain..................= 0
    //DeviceInfoDTP.Band3Gain..................= 0
    //DeviceInfoDTP.Band4Gain..................= 0
    //DeviceInfoDTP.Band5Gain..................= 0
    //GetIsMicNCIncomingSupportedAsync.............NO
    //GetIsWearDetectionSupportedAsync.............NO
    //DeviceInfoDTP.AnswerCall.............= False
    //DetectPageShow...WH5024
    #endregion
}
