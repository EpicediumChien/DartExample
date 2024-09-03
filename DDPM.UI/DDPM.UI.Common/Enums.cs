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
        KB = 1100,
        Mouse = 1200,
        Pen = 1300,
        Headset = 1400,
        Soundbar = 1500,
        Dock = 1600,
        WalkThrough = 1700,
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
        F1 = 0,
        F2 = 1,
        F3 = 2,
        F4 = 3,
        F5 = 4,
        F6 = 5,
        F7 = 6,
        F8 = 7,
        F9 = 8,
        F10 = 9,
        F11 = 10,
        F12 = 11,
        PrtSc,
        ScrollLock,
        PauseBreak,
        Calculator,
        Home,
        End,
        PgUp,
        PgDown
    }

    public enum MouseButtonName
    {
        ScrollWheelClick,
        ScrollTiltLeft,
        ScrollTiltRight,
        SideButtonForward,
        SideButtonBack
    }

    public enum AdvancedAction
    {
        AssignKeystroke,
        OpenFile,
        OpenFolder,
        OpenWebPage
    }
}