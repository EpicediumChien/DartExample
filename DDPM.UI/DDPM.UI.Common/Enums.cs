namespace DDPM.UI.Common
{
    /// <summary>
    /// Robert_Lin, 2024-7-9, change the values, for the Webcam and Monitor grouping.
    /// Robert_Lin, 2024/5/27 Change the order to meet the requirement of custom
    /// The displaying order on HomePage should be
    /// 1.Display, 2.Webcam, 3.Keyboard, 4.Mouse, 5.Pen/Stylus, 6.Audio headset
    /// 7.Soundbar/Speakerphone, 8.Dock
    /// </summary>
    public enum eDeviceCategory
    {
        Unknown = -1,
        Display = 0,
        Webcam = 1000,
        Bootloader = 1100,
        KB = 1200,
        Mouse = 1300,
        Pen = 1400,
        Headset = 1500,
        Soundbar = 1600,
        Dock = 1700,
        WalkThrough = 1800,
        AirAudio = 1900,
        RtkHub = 2000,
    }

    public enum PenButtonName
    {
        TopButton,
        TopBarrelButton,
        BottomBarrelButton
    }

    public enum ButtonBehavior
    {
        ClickOnce = 0,
        DoubleClick,
        PressAndHold
    }

    public enum ActionCategory
    {
        None = 0,
        WindowsAction,
        ProductivityAction,
        MultimediaAction,
        WordAction,
        ExcelAction,
        PowerPointAction,
        OutlookAction
    }

    public enum KeyName
    {
        F1 = 1,
        F2,
        F3,
        F4,
        F5,
        F6,
        F7,
        F8,
        F9,
        F10,
        F11,
        F12,
        PrtSc = 13,
        Home,
        End,
        PgUp,
        PgDown,
        Calculator,
        ScrollLock,
        PauseBreak,
    }

    public enum MouseButtonName
    {
        SideButtonForward = 65,
        SideButtonBack,
        ScrollWheelClick,
        ScrollTiltLeft,
        ScrollTiltRight
    }

    public enum AdvancedAction
    {
        AssignKeystroke,
        OpenFile,
        OpenFolder,
        OpenWebPage
    }

    public enum VbarIcon
    {
        DisplaySettings,
        DisplayInputSource,
        DisplayEA,
        DisplayGaming,
        DisplayMonitorAudio,
        DisplayKVM,
        DisplayOthers,
        KeyboardKeyCustom,
        KeyboardCollaboration,
        KeyboardIllumination,
        MouseSettings,
        MouseButton,
        PenSettings,
        PenButton,
        SpeakerPhonePreset,
        AudioSettings,
        SpeakerPhoneInteractions,
        HeadsetAutoActions,
        HeadsetSettings,
        WebcamControl,
        WebcamColorImg,
        WebcamDetection,
        WebcamCapture,
        WebcamMicrophone,
        RtkHubPortInfo
    }
}