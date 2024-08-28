using System.Collections.Generic;

namespace DDPM.SA.Common
{
    public class PenActionsSA
    {
        public PenActionsSA()
        {
        }
    }

    public class KeyboardActionsSA
    {
        public Dictionary<KeyNameSA, SelectedActionSA> KeyActions = new();

        public KeyboardActionsSA()
        { }
    }

    public class MouseActionsSA
    {
        public Dictionary<MouseButtonNameSA, SelectedMouseActionSA> ButtonActions = new();

        public MouseActionsSA()
        { }
    }

    public class SelectedActionSA
    {
        public int DefaultActionID = -1;
        public AssignedActionSA AssignedAction = new();

        public SelectedActionSA()
        { }

        public SelectedActionSA(int defaultActionID, AssignedActionSA assignedAction)
        {
            DefaultActionID = defaultActionID;
            AssignedAction = assignedAction;
        }
    }

    public class SelectedMouseActionSA
    {
        public int DefaultActionID = -1;
        public AssignedActionSA AssignedAction = new();
        public Dictionary<string, int> OfficeActions = new() { { "Word", -1 }, { "Excel", -1 }, { "PowerPoint", -1 }, { "Outlook", -1 } };

        public SelectedMouseActionSA()
        { }

        public SelectedMouseActionSA(int defaultActionID, AssignedActionSA assignedAction)
        {
            DefaultActionID = defaultActionID;
            AssignedAction = assignedAction;
        }
    }

    public class AssignedActionSA
    {
        public int ID = -1;
        public string Parameter = "";

        public AssignedActionSA()
        { }

        public AssignedActionSA(int id, string parameter = "")
        {
            ID = id;
            Parameter = parameter;
        }
    }

    public enum eDeviceCategorySA
    {
        Unknown = -1,
        Display = 0,
        Webcam = 1000,
        KB = 1100,
        Mouse = 1200,
        Pen = 1300,
        Headset = 1400,
        Soundbar = 1500,
        Dock = 1600
    }

    public enum PenButtonNameSA
    {
        TopButton,
        TopBarrelButton,
        BottomBarrelButton
    }

    public enum ButtonBehaviorSA
    {
        ClickOnce = 0,
        DoubleClick,
        PressAndHold
    }

    public enum ActionCategorySA
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

    public enum KeyNameSA
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

    public enum MouseButtonNameSA
    {
        ScrollWheelClick,
        ScrollTiltLeft,
        ScrollTiltRight,
        SideButtonForward,
        SideButtonBack
    }

    public enum AdvancedActionSA
    {
        AssignKeystroke,
        OpenFile,
        OpenFolder,
        OpenWebPage
    }
}