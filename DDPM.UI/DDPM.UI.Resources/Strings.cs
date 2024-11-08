using System.Globalization;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Navigation;
using Windows.ApplicationModel.Resources.Core;
using Windows.Devices.HumanInterfaceDevice;
using ResourceManager = System.Resources.ResourceManager;

namespace DDPM.UI.Common
{
    [Obsolete("please use DDPM.UI.Resources.Helper.LangHelper if static text", false)]//gavin 2024/08/02
    public static class Strings
    {
        private static ResourceManager resManager = Resources.Resources.ResourceManager;
        private static string GetString(string key)
        {

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
        public static readonly string RestoreToDefaultActions = GetString("RestoreToDefaultActions");
        public static readonly string Unpair = GetString("Unpair");
        public static readonly string PollingRateInfoTip1 = GetString("Mouse.13");
        public static readonly string PollingRateInfoTip2 = GetString("Mouse.14");

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
        public static readonly string KeyCustomizeCaptionCaption = GetString("KeyCustomization.2");
        public static readonly string Customize = GetString("Customize");
        public static readonly string KeyCustomizeMessage = GetString("KeyCustomization.0");
        public static readonly string KeyCustomizeRestoreCaption = GetString("KeyCustomization.1");
        public static readonly string ButtonCustomizeMessage = GetString("ButtonSettings.5");
        public static readonly string ButtonCustomizeRestoreCaption = GetString("ButtonSettings.6");
        public static readonly string PenButtonCustomizeRestoreCaption = GetString("PenButtonSettings.8");
        public static readonly string NullActionTooltip1 = GetString("Mouse.14");
        public static readonly string NullActionTooltip2 = GetString("Mouse.15");
        public static readonly string USBWirelessReceiver = GetString("USBWirelessReceiver");
        public static readonly string ReceiverFirmwareVersion = GetString("ReceiverFirmwareVersion");
        public static readonly string ReadyToBePaired = GetString("ReadyToBePaired");
        public static readonly string DPIMessage = GetString("MouseSettings.0");
        public static readonly string PollingRateMessage = GetString("MouseSettings.1");
        public static readonly string Wired = GetString("Wired");
        public static readonly string Error = GetString("Error");
        public static readonly string MultiDeviceTooltip = GetString("Tooltip.0");
        public static readonly string NoDeviceFound = GetString("Tooltip.1");
        public static readonly string AlreadyPaired = GetString("Tooltip.2");
        public static readonly string NotSupportedDevice = GetString("Tooltip.3");
        public static readonly string DongleSlotFull = GetString("Tooltip.4");
        public static readonly string CopilotTooltip = GetString("Tooltip.5");
        public static readonly string PenSettingsCaption = GetString("Pen.0");
        public static readonly string MouseSettingsCaption = GetString("Mouse.6");
        public static readonly string KeyCustomizationCaption = GetString("Keyboard.2");
        public static readonly string ButtonCustomizationCaption = GetString("Mouse.7");
        public static readonly string CollaborationCaption = GetString("Keyboard.3");
        public static readonly string IlluminationCaption = GetString("Keyboard.4");
        public static readonly string TouchScrollCaption = GetString("Mouse.8");
        public static readonly string TouchScrollInfoTip = GetString("Mouse.9");
        public static readonly string PrimaryButtonCaption = GetString("Mouse.10");
        public static readonly string DPISettingCaption = GetString("Mouse.11");
        public static readonly string PollingRateCaption = GetString("Mouse.12");
        public static readonly string PenButtonClickOnce = GetString("PenButtonSettings.3");
        public static readonly string PenButtonDoubleClick = GetString("PenButtonSettings.4");
        public static readonly string PenButtonPressHold = GetString("PenButtonSettings.5");
        public static readonly string TopButtonCaption = GetString("PenButtonSettings.0");
        public static readonly string TopBarrelButtonCaption = GetString("PenButtonSettings.1");
        public static readonly string BottomBarrelButtonCaption = GetString("PenButtonSettings.2");
        public static readonly string PenButtonCustomizeMessage = GetString("PenButtonSettings.7");
        public static readonly string HoverClick = GetString("PenButtonSettings.6");

        //Robert_Lin, 2024-7-25, Vbar Text for DisplayPlugin
        public static readonly string VbarText_DisplaySettings = GetString("DisplaySettings");

        public static readonly string VbarText_InputSource = GetString("InputSource");
        public static readonly string VbarText_EasyArrange = GetString("EasyArrange");
        public static readonly string VbarText_Gaming = GetString("Gaming");
        public static readonly string VbarText_KVM = GetString("KVM");
        public static readonly string VbarText_DisplayOthers = GetString("VbarText_DisplayOthers");

        //RightViewHeaderText
        public static readonly string RightViewHeader_BrightnessContrast = GetString("BrightnessOrContrast");

        public static readonly string RightViewHeader_Color = GetString("Color");//"Color";
        public static readonly string RightViewHeader_DisplayProperties = GetString("DisplayProperties");
        public static readonly string RightViewHeader_General = GetString("General");
        public static readonly string RightViewHeader_PIPPBP = GetString("PIPorPBP");
        public static readonly string RightViewHeader_Hotkeys = GetString("Hotkeys");
        public static readonly string RightViewHeader_Layout = GetString("Layout");
        public static readonly string RightViewHeader_EasyMemory = GetString("EasyMemory");
        public static readonly string RightViewHeader_Settings = GetString("Settings");
        public static readonly string RightViewHeader_VisionEngine = GetString("VisionEngine");

        public static readonly string Display = GetString("Display"); //"Display"
        public static readonly string FirmwareVersion = GetString("FirmwareVersion"); //"Firmware Version"
        public static readonly string ServiceTag = GetString("ServiceTag"); //"Service Tag"
        public static readonly string ManufactureMonth = GetString("ManufactureMonth"); //"Manufactured"
        //public static readonly string RestoreToDefaultButton = GetString("RestoreToDefaultButton"); //"Restore to default"

        //strings for Action
        public static readonly string None = GetString("None");//None

        //Windows Actions
        public static readonly string Copilot = GetString("Copilot");//Copilot

        public static readonly string Devices = GetString("Devices");//Devices
        public static readonly string Lock = GetString("Lock");//Lock
        public static readonly string NotificationCenter = GetString("NotificationCenter");//Notification Center
        public static readonly string ScreenSnip = GetString("ScreenSnip");//Screen Snip
        public static readonly string Search = GetString("Search");//Search
        public static readonly string Settings = GetString("Settings");//Settings
        public static readonly string ShowHideDesktop = GetString("ShowHideDesktop");//Show/Hide Desktop
        public static readonly string Shutdown = GetString("Shutdown");//Shutdown
        public static readonly string SignOut = GetString("SignOut");//Sign Out
        public static readonly string Sleep = GetString("Sleep");//Sleep
        public static readonly string SwitchApplications = GetString("SwitchApplications");//Switch Applications
        public static readonly string TaskView = GetString("TaskView");//Task View

        //pen
        public static readonly string BarrelButton = GetString("BarrelButton");//Barrel Button

        public static readonly string DefineBySystem = GetString("DefineBySystem");//Define by system
        public static readonly string EMail = GetString("EMail");//E-mail
        public static readonly string OneNote = GetString("OneNote");//One Note
        public static readonly string PenMenu = GetString("PenMenu");//Pen Menu
        public static readonly string QuickNote = GetString("QuickNote");//Quick Note
        public static readonly string ScreenSnipping = GetString("ScreenSnipping");//Screen Snipping //ScreenSnip?
        public static readonly string StickyNotes = GetString("StickyNotes");//Sticky Notes
        public static readonly string SwitchApplication = GetString("SwitchApplication");//Switch Application
        public static readonly string Widgets = GetString("Widgets");//Widgets
        public static readonly string WindowsSearch = GetString("WindowsSearch");//Windows Search //Search?

        //Productivity Actions
        public static readonly string AssignKeystroke = GetString("AssignKeystroke");//Assign Keystroke

        public static readonly string Back = GetString("Back");//Back
        public static readonly string Calculator = GetString("Calculator");//Calculator
        public static readonly string CloseWindow = GetString("CloseWindow");//Close Window
        public static readonly string Copy = GetString("Copy");//Copy
        public static readonly string Cut = GetString("Cut");//Cut
        public static readonly string Documents = GetString("Documents"); //Documents
        public static readonly string Forward = GetString("Forward"); //Forward
        public static readonly string MaximizeWindow = GetString("MaximizeWindow");//Maximize Window
        public static readonly string MinimizeWindow = GetString("MinimizeWindow");//Minimize Window
        public static readonly string MyHome = GetString("MyHome");//My Home
        public static readonly string OpenFile = GetString("OpenFile");//Open File
        public static readonly string OpenFolder = GetString("OpenFolder");//Open Folder
        public static readonly string OpenNewBrowserTab = GetString("OpenNewBrowserTab");//Open New Browser Tab
        public static readonly string OpenWebPage = GetString("OpenWebPage");//Open Web Page
        public static readonly string Paste = GetString("Paste");//Paste
        public static readonly string ZoomIn = GetString("ZoomIn");//Zoom In
        public static readonly string ZoomOut = GetString("ZoomOut");//Zoom Out
        public static readonly string ZoomReset = GetString("ZoomReset");//Zoom Reset

        public static readonly string _4thClick = GetString("4thClick");//"4th Click";
        public static readonly string _5thClick = GetString("5thClick");//"5th Click";
        public static readonly string Erase = GetString("Erase");//"Erase";
        public static readonly string LeftClick = GetString("LeftClick");// "Left Click";
        public static readonly string MiddleClick = GetString("MiddleClick");// "Middle Click";
        public static readonly string OpenRun = GetString("OpenRun");// "Open/Run";
        public static readonly string OpenRun2 = GetString("OpenRun2");// "Open Run";
        public static readonly string PenPageDown = GetString("PenPageDown");// "Page Down";
        public static readonly string PenPageUp = GetString("PenPageUp");// "Page Up";
        public static readonly string RadialMenu = GetString("RadialMenu");//"Radial Menu";
        public static readonly string Redo = GetString("Redo");// "Redo";
        public static readonly string RightClick = GetString("RightClick");//"Right Click";
        public static readonly string Undo = GetString("Undo");//"Undo";

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
        public static readonly string PenSettings = GetString("PenSettings.0");
        public static readonly string TipSensitivity = GetString("PenSettings.1");
        public static readonly string TipTooltip = GetString("PenSettings.2");
        public static readonly string TiltSensitivity = GetString("PenSettings.3");
        public static readonly string TiltTooltip = GetString("PenSettings.4");
        public static readonly string PairWithTile = GetString("PenSettings.5");
        public static readonly string PairTooltip = GetString("PenSettings.6");
        public static readonly string GetStarted2 = GetString("PenSettings.7");
        public static readonly string PairTile1 = "Enable Bluetooth on your device.";
        public static readonly string PairTile2 = "Scan the QR Code to download Tile on to your mobile device.";
        public static readonly string PairTile3 = "Press and hold the two side buttons on the pen for 3 seconds to pair it to Tile.";
        public static readonly string DownloadTile = "Download tile";
        public static readonly string USB_C_DP_14 = "USB-C (DP 1.4)";
        public static readonly string Dual_USB__C_DP_14 = "Dual USB-C (DP 1.4)";
        public static readonly string DUSB__C_TB_4 = "USB-C (TB 4)";
        public static readonly string DUSB__C_TB_5 = "USB-C (TB 5)";

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
        public static readonly string HeadsetDeviceSettings = GetString("DeviceSettings");
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

        //Collaboration
        public static readonly string CollabsCaption = "Collaboration";
        public static readonly string CollaborationToolTip = "Provides quick access to conference controls. Toggling the\nkeys on/off will show or hide them on the keyboard while in\na Microsoft Teams or Zoom call.";
        public static readonly string CollaborationBlinkEffectText = "Enable blink effect when there is a new chat message in conference\ncall";
        public static readonly string CollaborationDoubleTapText = "Activate icons on the keyboard by double tapping instead of single\ntapping";
        public static readonly string LearnMoreCaption = "Learn More";
        public static readonly string LearnMoreText1 = "To use Collaboration Keyboard with Microsoft Teams:\r\n1. Open Teams and go to privacy settings.\n\n2. Select Third-party app API and ensure that Dell Display and Peripheral Manager is not on the blocked list.\n\n3. Pair Microsoft Teams with Dell Display and Peripheral Manager by launching a Microsoft Teams conference call.";
        public static readonly string LearnMoreText2 = "If you are still having trouble, contact your IT department (if applicable) as they may have policies in place that are prohibiting Collaboration Keyboard from connecting to Microsoft Teams.";
        public static readonly string Alert1 = "To use Collaboration Keyboard you need the latest version of Zoom or Microsoft Teams";
        public static readonly string Alert2 = "Connect Collaboration Keyboard with Microsoft Teams by starting a conference call and accepting the connection request";
        public static readonly string Alert3 = "Collaboration Keyboard cannot connect to Microsoft Teams because you blocked the request. To use Microsoft Teams with Collaboration Keyboard, click \"Learn more\" for instructions on how to re-connect.";
        public static readonly string Alert4 = "To use Collaboration Keyboard with Zoom, install Zoom’s latest desktop version";
        public static readonly string Alert5 = "To use Collaboration Keyboard with Microsoft Teams, ensure you are signed into Microsoft Teams, using the latest version, and have the Third-party app API enabled";
        public static readonly string LearnMoreLink = "Learn more";
        public static readonly string VideoCaption = "Video";
        public static readonly string ShareCaption = "Share";
        public static readonly string ChatCaption = "Chat";
        public static readonly string MicCaption = "Mic";
        public static readonly string OKCaption = "OK";

        //Camera
        public static readonly string Preset = "Preset";
        public static readonly string EditPreset = "Edit preset";
        public static readonly string Smooth = "Smooth";
        public static readonly string Vibrant = "Vibrant";
        public static readonly string Warm = "Warm";
        public static readonly string Name = "Name";
        public static readonly string NameIsTaken = "This name is taken";
        public static readonly string DefaultProfileTooltip = "Default Profile Tooltip";
        public static readonly string SmoothProfileTooltip = "Smooth Profile Tooltip";
        public static readonly string VibrantProfileTooltip = "Vibrant Profile Tooltip";
        public static readonly string WarmProfileTooltip = "Warm Profile Tooltip";

        //EazyMemory
        public static readonly string Yes = GetString("Yes");
        public static readonly string No = GetString("No");
        public static readonly string ProfileTitleTextBlockForRightViewUI = GetString("EazyMemory.6");
        public static readonly string AutomaticStartupTextBlockForRightViewUI = GetString("EazyMemory.0");
        public static readonly string LaunchByTimeTextBlockForRightViewUI = GetString("EazyMemory.1");
        public static readonly string AppDocumentTextBlockForRightViewUI = GetString("EazyMemory.2");
        public static readonly string applybtnForRightViewUI = GetString("EazyMemory.3");
        public static readonly string NATextForRightViewUI = GetString("EazyMemory.4");
        public static readonly string msgboxTitle = GetString("EazyMemory.5");
        public static readonly string subTitle = GetString("EazyMemory.8");
        public static readonly string msgboxTitleForFirstPage = GetString("EazyMemory.7");
        public static readonly string subTitleForFirstPage = GetString("EazyMemory.9");
        public static readonly string CustomListTooltipText = GetString("EazyMemory.10");
        public static readonly string ezMemoryStartupErrorTitleStringForLaunchOptionPage = GetString("EazyMemory.5");
        public static readonly string ezMemoryStartupErrorStringForLaunchOptionPage = GetString("EazyMemory.11");
        public static readonly string TitleTBForLaunchOptionPage = GetString("EazyMemory.12");
        public static readonly string StartupCBContentForLaunchOptionPage = GetString("EazyMemory.13");
        public static readonly string ManulRBContentForLaunchOptionPage = GetString("EazyMemory.14");
        public static readonly string AutoRBContentForLaunchOptionPage = GetString("EazyMemory.15");
        public static readonly string FirstPageMainText = GetString("EasyMemory");
        public static readonly string FirstPageSubText = GetString("EazyMemory.36");
        public static readonly string AssignPageMainText = GetString("EazyMemory.34");
        public static readonly string AssignPageSubText = GetString("EazyMemory.37");
        public static readonly string LaunchOptionPageMainText = GetString("EazyMemory.35");
        public static readonly string LaunchOptionPageSubText = GetString("EazyMemory.38");

        //WalkThrough
        public static readonly string NextStr_0 = GetString("Kvm.1");
        public static readonly string WalkThrough_1 = GetString("WalkThrough.1");
        public static readonly string WalkThrough_2 = GetString("WalkThrough.2");
        public static readonly string WalkThroughDDPM_Main0 = GetString("WalkThroughDDPM_Main.0");
        public static readonly string WalkThroughDDPM_Sub0 = GetString("WalkThroughDDPM_Sub.0");
        public static readonly string WalkThroughDDPM_Main1 = GetString("WalkThroughDDPM_Main.1");
        public static readonly string WalkThroughDDPM_Sub1 = GetString("WalkThroughDDPM_Sub.1");
        public static readonly string WalkThroughDDPM_Main2 = GetString("WalkThroughDDPM_Main.2");
        public static readonly string WalkThroughDDPM_Sub2 = GetString("WalkThroughDDPM_Sub.2");
        public static readonly string WalkThroughDDPM_Main3 = GetString("WalkThroughDDPM_Main.3");
        public static readonly string WalkThroughDDPM_Sub3 = GetString("WalkThroughDDPM_Sub.3");
        public static readonly string WalkThroughDDPM_Main4 = GetString("WalkThroughDDPM_Main.4");
        public static readonly string WalkThroughDDPM_Sub4 = GetString("WalkThroughDDPM_Sub.4");
        public static readonly string WalkThroughDDPM_Main5 = GetString("WalkThroughDDPM_Main.5");
        public static readonly string WalkThroughDDPM_Sub5 = GetString("WalkThroughDDPM_Sub.5");
        public static readonly string WalkThroughDisplay_Main0 = GetString("WalkThroughDisplay_Main.0");
        public static readonly string WalkThroughDisplay_Sub0 = GetString("WalkThroughDisplay_Sub.0");
        public static readonly string WalkThroughDisplay_Main1 = GetString("WalkThroughDisplay_Main.1");
        public static readonly string WalkThroughDisplay_Sub1 = GetString("WalkThroughDisplay_Sub.1");
        public static readonly string WalkThroughDisplay_Main2 = GetString("WalkThroughDisplay_Main.2");
        public static readonly string WalkThroughDisplay_Sub2 = GetString("WalkThroughDisplay_Sub.2");
        public static readonly string WalkThroughKB525C_Main1 = GetString("WalkThroughKB525C_Main.1");
        public static readonly string WalkThroughKB525C_Sub1 = GetString("WalkThroughKB525C_Sub.1");
        public static readonly string WalkThroughKB525C_Main0 = GetString("WalkThroughKB525C_Main.0");
        public static readonly string WalkThroughKB525C_Sub0 = GetString("WalkThroughKB525C_Sub.0");
        public static readonly string WalkThroughKB_Main2 = GetString("WalkThroughKB_Main.2");
        public static readonly string WalkThroughKB_Sub2 = GetString("WalkThroughKB_Sub.2");
        public static readonly string WalkThroughMouse_Main0 = GetString("WalkThroughMouse_Main.0");
        public static readonly string WalkThroughMouse_Sub0 = GetString("WalkThroughMouse_Sub.0");
        public static readonly string WalkThroughMouse_Main1 = GetString("WalkThroughMouse_Main.1");
        public static readonly string WalkThroughMouse_Sub1 = GetString("WalkThroughMouse_Sub.1");
        public static readonly string WalkThroughMouseMS355_Main0 = GetString("WalkThroughMouseMS355_Main.0");
        public static readonly string WalkThroughMouseMS355_Sub0 = GetString("WalkThroughMouseMS355_Sub.0");
        public static readonly string WalkThroughMouseMS355_Main1 = GetString("WalkThroughMouseMS355_Main.1");
        public static readonly string WalkThroughMouseMS355_Sub1 = GetString("WalkThroughMouseMS355_Sub.1");
        public static readonly string WalkThroughMouseMS900_Main2 = GetString("WalkThroughMouseMS900_Main.2");
        public static readonly string WalkThroughMouseMS355_Sub2 = GetString("WalkThroughMouseMS355_Sub.2");
        public static readonly string WalkThroughMouseMS900_Main0 = GetString("WalkThroughMouseMS900_Main.0");
        public static readonly string WalkThroughMouseMS900_Sub0 = GetString("WalkThroughMouseMS900_Sub.0");
        public static readonly string WalkThroughMouseMS900_Sub1 = GetString("WalkThroughMouseMS900_Sub.1");
        public static readonly string WalkThroughPen_Main0 = GetString("WalkThroughPen_Main.0");
        public static readonly string WalkThroughPen_Sub0 = GetString("WalkThroughPen_Sub.0");
        public static readonly string WalkThroughPen_Main1 = GetString("WalkThroughPen_Main.1");
        public static readonly string WalkThroughPen_Sub1 = GetString("WalkThroughPen_Sub.1");
        public static readonly string WalkThroughPen_Main2 = GetString("WalkThroughPen_Main.2");
        public static readonly string WalkThroughPen_Sub2 = GetString("WalkThroughPen_Sub.2");
        public static readonly string WalkThroughWebCamWB3023_Main0 = GetString("WalkThroughWebCamWB3023_Main.0");
        public static readonly string WalkThroughWebCamWB3023_Sub0 = GetString("WalkThroughWebCamWB3023_Sub.0");
        public static readonly string WalkThroughWebCamWB3023_Main1 = GetString("WalkThroughWebCamWB3023_Main.1");
        public static readonly string WalkThroughWebCamWB3023_Sub1 = GetString("WalkThroughWebCamWB3023_Sub.1");
        public static readonly string WalkThroughWebCamWB3023_Main2 = GetString("WalkThroughWebCamWB3023_Main.2");
        public static readonly string WalkThroughWebCamWB3023_Sub2 = GetString("WalkThroughWebCamWB3023_Sub.2");
        public static readonly string WalkThroughWebCam_Main2 = GetString("WalkThroughWebCam_Main.2");
        public static readonly string WalkThroughWebCam_Sub2 = GetString("WalkThroughWebCam_Sub.2");
        public static readonly string WalkThroughWebCam_Sub3 = GetString("WalkThroughWebCam_Sub.3");
        public static readonly string WalkThroughWebCam_Main4 = GetString("WalkThroughWebCam_Main.4");
        public static readonly string WalkThroughWebCam_Sub4 = GetString("WalkThroughWebCam_Sub.4");
        public static readonly string WalkThroughHeadsetWH5024_Main1 = GetString("WalkThroughHeadsetWH5024_Main.1");
        public static readonly string WalkThroughHeadsetWH5024_Sub1 = GetString("WalkThroughHeadsetWH5024_Sub.1");
        public static readonly string WalkThroughHeadsetWL7024_Main1 = GetString("WalkThroughHeadsetWL7024_Main.1");
        public static readonly string WalkThroughHeadsetWL5024_Sub1 = GetString("WalkThroughHeadsetWL5024_Sub.1");
        public static readonly string WalkThroughHeadsetWH3024_Sub0 = GetString("WalkThroughHeadsetWH3024_Sub.0");
        public static readonly string WalkThroughHeadsetWL7024_Main2 = GetString("WalkThroughHeadsetWL7024_Main.2");
        public static readonly string WalkThroughHeadsetWL7024_Sub2 = GetString("WalkThroughHeadsetWL7024_Sub.2");
        public static readonly string WalkThroughHeadsetWH5024_Sub0 = GetString("WalkThroughHeadsetWH5024_Sub.0");
        public static readonly string WalkThroughHeadsetWL7024_Main0 = GetString("WalkThroughHeadsetWL7024_Main.0");
        public static readonly string WalkThroughHeadsetWL7024_Sub0 = GetString("WalkThroughHeadsetWL7024_Sub.0");
        public static readonly string WalkThroughHeadsetWL7024_Sub1 = GetString("WalkThroughHeadsetWL7024_Sub.1");
        public static readonly string WalkThroughHeadsetWL7024_Main3 = GetString("WalkThroughHeadsetWL7024_Main.3");
        public static readonly string WalkThroughHeadsetWL7024_Sub3 = GetString("WalkThroughHeadsetWL7024_Sub.3");

        //OpenAction
        public static readonly string AssignKeystrokeDesc = "Enter a key combination to create a shortcut";
        public static readonly string OpenFileDesc = "Click the browse button to select a file";
        public static readonly string OpenFolderDesc = "Click the \"Browse\" button to select a folder";
        public static readonly string OpenWebPageDesc = "Type the URL of the web page in the box below";
        public static readonly string OpenFileWaterMark = "File Name";
        public static readonly string OpenFolderWaterMark = "Folder Name";

        //Headset
        public static readonly string HeadsetAudioSettingsToolTip_1 = GetString("HeadsetAudioSettings.20");
        public static readonly string HeadsetAudioSettingsToolTip_2 = GetString("HeadsetAudioSettings.21");
        public static readonly string HeadsetAudioSettingsToolTip_3 = GetString("HeadsetAudioSettings.22");
        public static readonly string HeadsetAudioSettingsToolTip_4 = GetString("HeadsetAudioSettings.23");
        public static readonly string HeadsetAudioSettingsToolTip_5 = GetString("HeadsetAudioSettings.24");
        public static readonly string HeadsetAudioSettingsToolTip_6 = GetString("HeadsetAudioSettings.25");
        public static readonly string HeadsetAudioSettingsToolTip_7 = GetString("HeadsetAudioSettings.26");
        public static readonly string HeadsetAudioSettingsToolTip_8 = GetString("HeadsetAudioSettings.27");
        public static readonly string HeadsetAudioSettingsToolTip_9 = GetString("HeadsetAudioSettings.28");
        public static readonly string HeadsetAudioSettingsToolTip_10 = GetString("HeadsetAudioSettings.29");
        public static readonly string HeadsetAudioSettingsToolTip_11 = GetString("HeadsetAudioSettings.30");

        public static readonly string HeadsetAutomatedActionsToolTip_1 = GetString("HeadsetAutomatedActions.9");
        public static readonly string HeadsetAutomatedActionsToolTip_2 = GetString("HeadsetAutomatedActions.10");
        public static readonly string HeadsetAutomatedActionsToolTip_3 = GetString("HeadsetAutomatedActions.11");
        public static readonly string HeadsetAutomatedActionsToolTip_4 = GetString("HeadsetAutomatedActions.12");
        public static readonly string HeadsetAutomatedActionsToolTip_5 = GetString("HeadsetAutomatedActions.13");

        public static readonly string HeadsetDeviceSettingsToolTip_1 = GetString("HeadsetDeviceSettings.7");
        public static readonly string HeadsetDeviceSettingsToolTip_2 = GetString("HeadsetDeviceSettings.8");

        //Speaker
        public static readonly string SpeakerToolTip_1 = GetString("SpeakerToolTip.0");
        public static readonly string SpeakerToolTip_2 = GetString("SpeakerToolTip.1");
        public static readonly string SpeakerToolTip_3 = GetString("SpeakerToolTip.2");

        //Import/Export
        public static readonly string ImpExp_Success = GetString("Success");
        public static readonly string ImpExp_Warning = GetString("Warning");
        public static readonly string ImpExp_Continue = GetString("Continue");
        public static readonly string ImpExp_Cancel = GetString("Cancel");
        public static readonly string ImpExp_Restart = GetString("RestartNeeded");
        public static readonly string ImpExp_RestartMsg0 = GetString("ImpExp_RestartMsg.0");
        public static readonly string ImpExp_SuccessMsg0 = GetString("ImpExp_SuccessMsg.0");
        public static readonly string ImpExp_SuccessMsg1 = GetString("ImpExp_SuccessMsg.1");
        public static readonly string ImpExp_WarningMsg0 = GetString("ImpExp_WarningMsg.0");
        public static readonly string ImpExp_WarningMsg1 = GetString("ImpExp_WarningMsg.1");

        // Pair/Unpair
        public static readonly string Caption = "Are you sure?";
        public static readonly string MessageMouse = "Unpairing your mouse can limit your ability to use this computer. Make sure you have an alternative mouse setup before unpairing.";
        public static readonly string MessageKeyboard = "Unpairing your keyboard can limit your ability to use this computer. Make sure you have an alternative keyboard setup before unpairing.";
        public static readonly string MessagePen = "Unpairing your pen can limit your ability to use this computer. Make sure you have an alternative pen setup before unpairing.";
        public static readonly string MessageHeadset = "This will unpair your headset from its USB wireless receiver. You can still pair and use the headset on this system via Bluetooth. If required, you may pair the headset back to the wireless receiver from + icon on top right of the home screen of [NAME] .";
        public static readonly string Continue = "Continue";
        public static readonly string Imcompatible = "This device is not compatible with";
        public static readonly string PenAlreadyPaired = "This device is already paired. Unpair the device first if you would like to pair again.";
        public static readonly string PairYourPen = "Pair your pen";
        public static readonly string PairYourPenMessage = "Would you like to pair your pen with your system?";

        // Display Restore to Default
        public static readonly string RestoreToDefalutText = "Are you sure you want to restore all default settings on your device?";
        public static readonly string DisplayDefault0 = GetString("DisplayDefault.0");
    }
}
