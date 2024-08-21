using System.Globalization;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Navigation;
using Windows.ApplicationModel.Resources.Core;
using Windows.Devices.HumanInterfaceDevice;
using ResourceManager=System.Resources.ResourceManager;

namespace DDPM.UI.Common {
    [Obsolete("please use DDPM.UI.Resources.Helper.LangHelper if static text",false)]//gavin 2024/08/02
    public static class Strings {
    private static ResourceManager resManager = Resources.Resources.ResourceManager;
    private static string GetString(string key) {

            //CultureInfo cultureInfo = CultureInfo.CreateSpecificCulture("pl-PL");
           // string str = resManager.GetString(key, cultureInfo) ?? resManager.GetString(key, CultureInfo.InvariantCulture) ?? "";
            string str = resManager.GetString(key, CultureInfo.InstalledUICulture) ?? resManager.GetString(key, CultureInfo.InvariantCulture) ?? "";
            return System.Text.RegularExpressions.Regex.Unescape(str);
    }

        public static readonly string AdaptiveLight = GetString("AdaptiveLight");
        public static readonly string Manual = GetString("Manual");
        public static readonly string On = GetString("On");
        public static readonly string Off = GetString("Off");
        public static readonly string RestoreToDefault = GetString("RestoreToDefault");
        public static readonly string Unpair = GetString("Unpair");
        public static readonly string PollingRateInfoTip1 = "Number of times per second your mouse’s position is reported to system";
        public static readonly string PollingRateInfoTip2 = "Number of times per second mouse’s position reports to system. Connect via USB for higher polling rate.";

        public static readonly string ButtonCustomizeCaption = GetString("ButtonSettings.13");
        public static readonly string ScrollWheelCaption = GetString("ButtonSettings.0");
        public static readonly string ScrollTiltLCaption = GetString("ButtonSettings.1");
        public static readonly string ScrollTiltRCaption = GetString("ButtonSettings.2");
        public static readonly string SideButtonFCaption = GetString("ButtonSettings.3");
        public static readonly string SideButtonBCaption = GetString("ButtonSettings.4");
        public static readonly string SuggestedActionsCaption = GetString("ButtonSettings.7");
        public static readonly string ProductivityActionsCaption = GetString("ButtonSettings.8");
        public static readonly string WindowsActionsCaption = GetString("ButtonSettings.9");
        public static readonly string MultimediaActionsCaption = GetString("ButtonSettings.10");
        public static readonly string AdvancedActionsCaption = GetString("ButtonSettings.11");
        public static readonly string Edit = GetString("Edit");
        public static readonly string Remove = GetString("Remove");
        public static readonly string SearchResultsCaption = GetString("ButtonSettings.12");
        public static readonly string KeyCustomizeCaptionCaption = "Key Customization";
        public static readonly string Customize = GetString("Customize");
        public static readonly string KeyCustomizeMessage = "To customize a key, click one of the outlined keys on the image to the left";
        public static readonly string KeyCustomizeRestoreCaption = "Restore all actions to default";
        public static readonly string ButtonCustomizeMessage = GetString("ButtonSettings.5");
        public static readonly string ButtonCustomizeRestoreCaption = GetString("ButtonSettings.6");
        public static readonly string PenButtonCustomizeRestoreCaption = "Restore to default actions";
        public static readonly string NullActionTooltip1 = "Click to Assign an action";
        public static readonly string NullActionTooltip2 = "Select an action from\nthe list on the right";
        public static readonly string USBWirelessReceiver = "USB Wireless Receiver";
        public static readonly string ReceiverFirmwareVersion = "Receiver Firmware Version";
        public static readonly string ReadyToBePaired = "Ready to be paired";
        public static readonly string DPIMessage = "Move your mouse to complete the change to the DPI value";
        public static readonly string PollingRateMessage = "Increasing polling rate may affect mouse’s battery life.";
        //public static readonly string EOLMessage = "There are no advanced features on this device";
        public static readonly string Wired = "Wired";
        public static readonly string Error = GetString("Error");
        public static readonly string MultiDeviceTooltip = "You have connected multiple devices of the same model. Actions will be duplicated on each instance of the device";
        public static readonly string NoDeviceFound = "No device found. Make sure your wireless device is charged and turned ON.";
        public static readonly string AlreadyPaired = "This device is already paired. Unpair the device first if you would like to pair again.";
        public static readonly string NotSupportedDevice = "This device you are pairing is not supported with the connected USB wireless receiver. Use the receiver that came with the device.";
        public static readonly string DongleSlotFull = "The available slots on this USB wireless receiver are full. To use, unpair a device from it by clicking on the \"Unpair\" button in the device settings page.";
        public static readonly string PenSettingsCaption = GetString("Pen.0");
        public static readonly string MouseSettingsCaption = GetString("Mouse.6");
        public static readonly string KeyCustomizationCaption = GetString("Keyboard.2");
        public static readonly string ButtonCustomizationCaption = GetString("Mouse.7");
        public static readonly string CollaborationCaption = GetString("Keyboard.3");
        public static readonly string IlluminationCaption = GetString("Keyboard.4");
        public static readonly string TouchScrollCaption = "Touch Scroll Sensitivity";
        public static readonly string TouchScrollInfoTip = "Adjust the Scroll Speed";
        public static readonly string PrimaryButtonCaption = "Primary Mouse Button";
        public static readonly string DPISettingCaption = "DPI Setting";
        public static readonly string PollingRateCaption = "Polling Rate";
        public static readonly string PenButtonClickOnce = "Click Once";
        public static readonly string PenButtonDoubleClick = "Double Click";
        public static readonly string PenButtonPressHold = "Press and Hold";
        public static readonly string TopButtonCaption = "Top Button";
        public static readonly string TopBarrelButtonCaption = "Top Barrel Button";
        public static readonly string BottomBarrelButtonCaption = "Bottom Barrel Button";
        public static readonly string PenButtonCustomizeMessage = "To customize a button, click an outlined button on the image to the left";
        public static readonly string HoverClick = "Hover Click";

        //Robert_Lin, 2024-7-25, Vbar Text for DisplayPlugin
        public static readonly string VbarText_DisplaySettings = GetString("DisplaySettings");

        public static readonly string VbarText_InputSource = GetString("InputSource");
        public static readonly string VbarText_EasyArrange = GetString("EasyArrange");
        public static readonly string VbarText_Gaming = GetString("Gaming");
        public static readonly string VbarText_KVM = GetString("KVM");
        public static readonly string VbarText_DisplayOthers = GetString("VbarText_DisplayOthers");

        //RightViewHeaderText
        public static readonly string RightViewHeader_BrightnessContrast = "Brightness/Contrast";

        public static readonly string RightViewHeader_Color = "Color";
        public static readonly string RightViewHeader_DisplayProperties = "Display Properties";
        public static readonly string RightViewHeader_General = "General";
        public static readonly string RightViewHeader_PIPPBP = "PIP/PBP";
        public static readonly string RightViewHeader_Hotkeys = "Hotkeys";
        public static readonly string RightViewHeader_Layout = "Layout";
        public static readonly string RightViewHeader_EasyMemory = "Easy Memory";
        public static readonly string RightViewHeader_Settings = "Settings";
        public static readonly string RightViewHeader_VisionEngine = "Vision Engine";

        public static readonly string Display = GetString("Display"); //"Display"
        public static readonly string FirmwareVersion = GetString("FirmwareVersion"); //"Firmware Version"
        public static readonly string ServiceTag = GetString("ServiceTag"); //"Service Tag"
        public static readonly string ManufactureMonth = GetString("ManufactureMonth"); //"Manufactured"
        public static readonly string RestoreToDefaultButton = GetString("RestoreToDefaultButton"); //"Restore to default"

        //strings for Action
        public static readonly string None = "None";

        //Windows Actions
        public static readonly string Copilot = "Copilot";

        public static readonly string Devices = "Devices";
        public static readonly string Lock = "Lock";
        public static readonly string NotificationCenter = "Notification Center";
        public static readonly string ScreenSnip = "Screen Snip";
        public static readonly string Search = "Search";
        public static readonly string Settings = "Settings";
        public static readonly string ShowHideDesktop = "Show/Hide Desktop";
        public static readonly string Shutdown = "Shutdown";
        public static readonly string SignOut = "Sign Out";
        public static readonly string Sleep = "Sleep";
        public static readonly string SwitchApplications = "Switch Applications";
        public static readonly string TaskView = "Task View";

        //pen
        public static readonly string BarrelButton = "Barrel Button";

        public static readonly string DefineBySystem = "Define by system";
        public static readonly string EMail = "E-mail";
        public static readonly string OneNote = "One Note";
        public static readonly string PenMenu = "Pen Menu";
        public static readonly string QuickNote = "Quick Note";
        public static readonly string ScreenSnipping = "Screen Snipping"; //ScreenSnip?
        public static readonly string StickyNotes = "Sticky Notes";
        public static readonly string SwitchApplication = "Switch Application";
        public static readonly string Widgets = "Widgets";
        public static readonly string WindowsSearch = "Windows Search"; //Search?

        //Productivity Actions
        public static readonly string AssignKeystroke = "Assign Keystroke";

        public static readonly string Back = "Back";
        public static readonly string Calculator = "Calculator";
        public static readonly string CloseWindow = "Close Window";
        public static readonly string Copy = "Copy";
        public static readonly string Cut = "Cut";
        public static readonly string Documents = "Documents";
        public static readonly string Forward = "Forward";
        public static readonly string MaximizeWindow = "Maximize Window";
        public static readonly string MinimizeWindow = "Minimize Window";
        public static readonly string MyHome = "My Home";
        public static readonly string OpenFile = "Open File";
        public static readonly string OpenFolder = "Open Folder";
        public static readonly string OpenNewBrowserTab = "Open New Browser Tab";
        public static readonly string OpenWebPage = "Open Web Page";
        public static readonly string Paste = "Paste";
        public static readonly string ZoomIn = "Zoom In";
        public static readonly string ZoomOut = "Zoom Out";
        public static readonly string ZoomReset = "Zoom Reset";

        public static readonly string _4thClick = "4th Click";
        public static readonly string _5thClick = "5th Click";
        public static readonly string Erase = "Erase";
        public static readonly string LeftClick = "Left Click";
        public static readonly string MiddleClick = "Middle Click";
        public static readonly string OpenRun = "Open/Run";
        public static readonly string OpenRun2 = "Open Run";
        public static readonly string PenPageDown = "Page Down";
        public static readonly string PenPageUp = "Page Up";
        public static readonly string RadialMenu = "Radial Menu";
        public static readonly string Redo = "Redo";
        public static readonly string RightClick = "Right Click";
        public static readonly string Undo = "Undo";

        //Multimedia Actions
        public static readonly string MediaNextTrack = "Media Next Track";

        public static readonly string MediaPlayPause = "Media Play/Pause";
        public static readonly string MediaPreviousTrack = "Media Previous Track";
        public static readonly string Music = "Music";
        public static readonly string Pictures = "Pictures";
        public static readonly string VolumeDown = "Volume Down";
        public static readonly string VolumeMute = "Volume Mute";
        public static readonly string VolumeUp = "Volume Up";

        //pen
        public static readonly string NextTrack = "Next Track"; //MediaNextTrack?

        //Other
        public static readonly string PrtSc = "Print Screen";

        public static readonly string ScrollLock = "Scroll Lock";
        public static readonly string PauseBreak = "Pause Break";
        public static readonly string Home = "Home";
        public static readonly string End = "End";
        public static readonly string PageDown = "Page Down";
        public static readonly string PageUp = "Page Up";
        public static readonly string WebBrowser = "Web Browser";

        //Word
        public static readonly string Autoscroll = "Autoscroll";

        public static readonly string Find = "Find";
        public static readonly string IncreaseIndent = "Increase Indent";
        public static readonly string NewComment = "New Comment";
        public static readonly string NextChange = "Next Change";
        public static readonly string NextComment = "Next Comment";
        public static readonly string PasteAndKeepSourceFormatting = "Paste and Keep Source Formatting";
        public static readonly string PasteAndKeepTextOnly = "Paste and Keep Text Only";
        public static readonly string PasteAndMatchFormatting = "Paste and Merge Formatting";
        public static readonly string PasteAndMergeFormatting = "Paste and Match Formatting";
        public static readonly string PreviousChange = "Previous Change";
        public static readonly string PreviousComment = "Previous Comment";
        public static readonly string Print = "Print";
        public static readonly string Save = "Save";
        public static readonly string Strikethrough = "Strikethrough";
        public static readonly string TextSizeMinus = "Text Size -";
        public static readonly string TextSizePlus = "Text Size +";
        public static readonly string TranslateSelectedText = "Translate Selected Text";
        public static readonly string ViewOnePage = "View One Page";
        public static readonly string ViewPageWidth = "View Page Width";

        //Excel
        public static readonly string AlignCenter = "Align Center";

        public static readonly string AlignLeft = "Align Left";
        public static readonly string AlignRight = "Align Right";
        public static readonly string DecreaseIndent = "Decrease Indent";
        public static readonly string GotoBottomOfDataRegion = "Go to Bottom of Data Region";
        public static readonly string GotoTopOfDataRegion = "Go to Top of Data Region";
        public static readonly string InsertChart = "Insert Chart";
        public static readonly string InsertRowAbove = "Insert Row Above";
        public static readonly string PanHoldAndMoveMouse = "Pan (Hold and Move Mouse)";
        public static readonly string PasteFormatOnly = "Paste Format Only";
        public static readonly string PasteFormulas = "Paste Formulas";
        public static readonly string PasteValueOnly = "Paste Value Only";
        public static readonly string PreviousSheet = "Previous Sheet";
        public static readonly string SortAtoZ = "Sort A to Z";

        //PowerPoint
        public static readonly string ArrangeAlignCenter = "Arrange Align Center";

        public static readonly string ArrangeAlignLeft = "Arrange Align Left";
        public static readonly string ArrangeAlignRight = "Arrange Align Right";
        public static readonly string BringToFront = "Bring to Front";
        public static readonly string DecreaseListLevel = "Decrease List Level";
        public static readonly string DuplicateSelectedSlides = "Duplicate Selected Slides";
        public static readonly string IncreaseListLevel = "Increase List Level";
        public static readonly string PlayFromCurrentSlide = "Play from Current Slide";
        public static readonly string PreviousSlide = "Previous Slide";
        public static readonly string SendToBack = "Send to Back";

        //Outlook
        public static readonly string AttachFile = "Attach File";

        public static readonly string ForwardEmail = "Forward Email";
        public static readonly string NewEmail = "New Email";
        public static readonly string NewMeeting = "New Meeting";
        public static readonly string Reply = "Reply";
        public static readonly string ReplyToAll = "Reply to All";

        //Open/Run
        public static readonly string Calendar = "Calendar";

        public static readonly string Camera = "Camera";
        public static readonly string Clock = "Clock";
        public static readonly string Cortana = "Cortana";
        public static readonly string DellCommandUpdate = "Dell Command Update";
        public static readonly string DellDigitalDelivery = "Dell Digital Delivery";
        public static readonly string DellOptimizer = "Dell Optimizer";
        public static readonly string Family = "Family";
        public static readonly string FeedbackHub = "Feedback Hub";
        public static readonly string GameBar = "Game Bar";
        public static readonly string GetHelp = "Get Help";
        public static readonly string GetStarted = "Get Started";
        public static readonly string IntelManagementAndSecurityStatus = "Intel (R) Management and Security Status";
        public static readonly string IntelGraphicsCommandCenter = "Intel® Graphics Command Center";
        public static readonly string IntelOptaneMemoryAndStorageManagement = "Intel® Optane™ Memory and Storage Management";
        public static readonly string Mail = "Mail";
        public static readonly string Maps = "Maps";
        public static readonly string MediaPlayer = "Media Player";
        public static readonly string Microsoft365Office = "Microsoft 365 (Office)";
        public static readonly string MicrosoftClipchamp = "Microsoft Clipchamp";
        public static readonly string MicrosoftDefender = "Microsoft Defender";
        public static readonly string MicrosoftStore = "Microsoft Store";
        public static readonly string MicrosoftTeams = "Microsoft Teams";
        public static readonly string MicrosoftTeamsWorkSchool = "Microsoft Teams (work or school)";
        public static readonly string MicrosoftToDo = "Microsoft To Do";
        public static readonly string MoviesTV = "Movies & TV";
        public static readonly string News = "News";
        public static readonly string Notepad = "Notepad";
        public static readonly string Paint = "Paint";
        public static readonly string PhoneLink = "Phone Link";
        public static readonly string Photos = "Photos";
        public static readonly string PowerAutomate = "Power Automate";
        public static readonly string SnippingTool = "Snipping Tool";
        public static readonly string SolitaireCasualGames = "Solitaire & Casual Games";
        public static readonly string SoundRecorder = "Sound Recorder";
        public static readonly string Spotify = "Spotify";
        public static readonly string SupportAssist = "SupportAssist";
        public static readonly string Terminal = "Terminal";
        public static readonly string Tips = "Tips";
        public static readonly string Weather = "Weather";
        public static readonly string WindowsBackup = "Windows backup";
        public static readonly string WindowsSecurity = "Windows Security";
        public static readonly string Xbox = "Xbox";

        //Radial Menu
        public static readonly string Disabled = "Disabled";

        public static readonly string GoBack = "Go Back";
        public static readonly string GoForward = "Go Forward";
        public static readonly string PlayPause = "Play/Pause";
        public static readonly string PreviousTrack = "Previous Track";
        public static readonly string Mute = "Mute";
        public static readonly string FunctionForSelectedRadial = "Function for selected radial";
        public static readonly string LabelForSelectedRadial = "Label for selected radial";
        public static readonly string UseCenterForEmulatingRightClick = "Use center for emulating right click";

        //Common
        public static readonly string Browse = "Browse";

        public static readonly string Clear = "Clear";
        public static readonly string Cancel = "Cancel";

    //Dialog
    public static readonly string OpenRunDesc = "Choose the app from a list of apps";
    public static readonly string SelectAFile = "Select a file";
    public static readonly string SelectedFile = "Selected file";

    //Pen settings
    public static readonly string PenSettings = "Pen Settings";
    public static readonly string TipSensitivity = "Tip Sensitivity";
    public static readonly string TipTooltip = "Moving the slider to the right will gradually\ndecrease the sensitivity to pressure and you\nneed to apply firmer pen pressure";
    public static readonly string TiltSensitivity = "Tilt Sensitivity";
    public static readonly string TiltTooltip = "Moving the slider to the right will gradually\nincrease the tilting effect, and you need to\napply less tilting angle";
    public static readonly string PairWithTile = "Pair with Tile";
    public static readonly string PairTooltip = "Pair your pen to your mobile device using\nthe Tile app";
    public static readonly string GetStarted2 = "Get started";
    public static readonly string PairTile1 = "Enable Bluetooth on your device.";
    public static readonly string PairTile2 = "Scan the QR Code to download Tile on to your mobile device.";
    public static readonly string PairTile3 = "Press and hold the two side buttons on the pen for 3 seconds to pair it to Tile.";
    public static readonly string DownloadTile = "Download tile";

    #region [2]
    //please do not insert any code to region [2]
    // add device 
    public static readonly string AddDevice = GetString("AddDevice");
    public static readonly string AddDevice_Display = GetString("AddDevice.Display");
    public static readonly string AddDevice_Webcam = GetString("AddDevice.Webcam");
    public static readonly string AddDevice_KnM = GetString("AddDevice.KnM");
    public static readonly string AddDevice_Pen = GetString("AddDevice.Pen");
    public static readonly string AddDevice_Headset = GetString("AddDevice.Headset");
    public static readonly string AddDevice_Speaker = GetString("AddDevice.Speaker");
    public static readonly string AddDevice_Dock = GetString("AddDevice.Dock");
    public static readonly string AddDeviceTypeBluetooth = GetString("AddDevice.Type.Bluetooth");
    public static readonly string AddDeviceTypeWireless = GetString("AddDevice.Type.Wireless");
    public static readonly string AddDeviceTypeWired = GetString("AddDevice.Type.Wired");
    public static readonly string AddDeviceTypeOther = GetString("AddDevice.Type.Other");
    public static readonly string AddDeviceMsgCancelBtn = GetString("AddDevice.Msg.CancelBtn");
    public static readonly string AddDeviceMsgWaitingCap = GetString("AddDevice.Msg.WaitingCap");
    public static readonly string AddDeviceMsgWaitingMsg = GetString("AddDevice.Msg.WaitingMsg");
    public static readonly string AddDeviceMsgWaitingAlert = GetString("AddDevice.Msg.WaitingAlert");
    public static readonly string AddDeviceKnMmultiDongleAlert = GetString("AddDevice.KnM.10");
    public static readonly string AddDeviceKnMnoDongleAlertKnM = GetString("AddDevice.KnM.11");
    public static readonly string AddDeviceKnMnoDongleAlertHeadset = GetString("AddDevice.KnM.12");
    public static readonly string DockDongle0 = GetString("Dock.0");
    public static readonly string DockDongle1 = GetString("Dock.1");
    public static readonly string HeadsetAudioSettings = GetString("AudioSettings");
    public static readonly string HeadsetAutomatedActions = GetString("AutomatedActions");
    public static readonly string HeadsetDeviceSettings = GetString("Headset.2");
    public static readonly string SoundBarAudioPreset = GetString("Soundbar.0");
    public static readonly string SoundBarInteractions = GetString("Soundbar.1");
    public static readonly string SoundBarAudioSettings = GetString("AudioSettings");
    public static readonly string CameraControl = GetString("Camera.0");
    public static readonly string ColorandImage = GetString("Camera.1");
    public static readonly string PresenceDetection = GetString("Camera.2");
    public static readonly string Capture = GetString("Camera.3");
    public static readonly string Microphone = GetString("Camera.4");
    public static readonly string BrightnessErrorMsg0 = GetString("Brightness.0");
    public static readonly string OK = GetString("OK");
    public static readonly string LearnMore = GetString("LearnMore");
    public static readonly string Collaboration = GetString("Collaboration");

    #endregion
    }
}
