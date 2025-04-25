namespace DDPM.SA.Common
{
    public enum OSDType
    {
        BatteryLow,
        CapsLock,
        ScrollLock,
        NumLock,
        DisplayChanged,
        Fingerprint,
        Mute,
        WalkAwayLock,
        StartRecording,
        EasyMemory,
        Error,
        QAM,
        CollaborationNotAvailable
    }

    public enum OSDType_Device
    {
        Unknown,
        Keyboard,
        Mouse,
        Headset,
        Pen,
        QAM,
        CapsLockOn,
        CapsLockOff,
        ScrollLockOn,
        ScrollLockOff,
        NumLockOn,
        NumLockOff,
        FW,
        Mute,
        UnMute,
        FingerPrint,
        WalkAwayLock,
        StartRecording,
        EasyMemory,
        FWAutoClose
    }

    public enum OSDType_Op
    {
        None,
        Plugin,
        Unplug,
        AutoClose,
        CloseAll
    }
}