//Robert_Lin 2025-1-14, to simple swich betwwen Teesting and Release mode, please comment out the following line in Release mode
//#define MULTILINGUAL_TEST
//Robert_Lin, "MULTILINGUAL_TEST" will be removed, to test multilingual, please use registery keys method, read "DdpmCultureMap.cs" for detail.

using System.Globalization;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Navigation;
using Windows.ApplicationModel.Resources.Core;
using Windows.Devices.HumanInterfaceDevice;
using ResourceManager = System.Resources.ResourceManager;
using DDPM.UI.Resources;

namespace DDPM.UI.Common
{
    [Obsolete("please use DDPM.UI.Resources.Helper.LangHelper if static text", false)]//gavin 2024/08/02
    public static class Strings
    {
        private static ResourceManager resManager = Resources.Resources.ResourceManager;
        private static string GetString(string key)
        {
#if MULTILINGUAL_TEST
            //Robert_Lin 2025-1-13 To do multilingual test, please remove below comments
            //
            CultureInfo cultureInfo = CultureInfo.CreateSpecificCulture("zh-TW");
            string str = resManager.GetString(key, cultureInfo) ?? resManager.GetString(key, CultureInfo.InvariantCulture) ?? "";
#else
            //And comment-out below line
            //Robert_Lin 2025-1-22 PIMS-331191 With DDPM installed, observe language in DDPM UI not change for other langauges of Other countries
            //Add a CultureInfoMap to convert (mapped) CultureInfo.CurrentUICulture to the supported cultureInfo of DDPM
            //OLD:
            //string str = resManager.GetString(key, CultureInfo.CurrentUICulture) ?? resManager.GetString(key, CultureInfo.InvariantCulture) ?? "";
            //NEW:
            string str = resManager.GetString(key, DdpmCultureMap.MappedCultureInfo) ?? resManager.GetString(key, CultureInfo.InvariantCulture) ?? "";
#endif

            return System.Text.RegularExpressions.Regex.Unescape(str);
        }
        //private static string GetString(string key, string culture = "en-US")
        //{
        //    CultureInfo cultureInfo = CultureInfo.CreateSpecificCulture(culture);

        //    string str = resManager.GetString(key, cultureInfo) ??
        //                 resManager.GetString(key, CultureInfo.InvariantCulture) ?? "";

        //    return System.Text.RegularExpressions.Regex.Unescape(str);
        //}
        public static readonly string AdaptiveLight = GetString("AdaptiveLight");
        public static readonly string Manual = GetString("Manual");
        public static readonly string On = GetString("On");
        public static readonly string Off = GetString("Off");
        public static readonly string RestoreToDefault = GetString("RestoreToDefault");
        public static readonly string RestoreToDefaultActions = GetString("RestoreToDefaultActions");
        public static readonly string Unpair = GetString("Unpair");
        public static readonly string PollingRateInfoTip1 = GetString("Mouse.13");
        public static readonly string PollingRateInfoTip2 = GetString("Mouse.16");

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
        public static readonly string Slots = GetString("slot");

        //Robert_Lin, 2024-7-25, Vbar Text for DisplayPlugin
        public static readonly string VbarText_DisplaySettings = GetString("DisplaySettings");

        public static readonly string VbarText_InputSource = GetString("InputSource");
        public static readonly string VbarText_EasyArrange = GetString("EasyArrange");
        public static readonly string VbarText_Gaming = GetString("Gaming");
        public static readonly string VbarText_MonitorAudio = GetString("Audio");
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
        public static readonly string DeviceID = GetString("DeviceID"); //"Firmware Version"
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
        public static readonly string MediaNextTrack = GetString("Multimedia.1");
        public static readonly string MediaPlayPause = GetString("Multimedia.2");
        public static readonly string MediaPreviousTrack = GetString("Multimedia.3");
        public static readonly string Music = GetString("Multimedia.4");
        public static readonly string Pictures = GetString("Multimedia.5");
        public static readonly string VolumeDown = GetString("Multimedia.6");
        public static readonly string VolumeMute = GetString("Multimedia.7");
        public static readonly string VolumeUp = GetString("Multimedia.8");

        //pen
        public static readonly string NextTrack = GetString("NextTrack");//Next Track //MediaNextTrack?

        //Other
        public static readonly string PrtSc = GetString("PrtSc");//Print Screen
        public static readonly string ScrollLock = GetString("ScrollLock");//Scroll Lock
        public static readonly string PauseBreak = GetString("PauseBreak");//Pause Break
        public static readonly string Home = GetString("Home");
        public static readonly string End = GetString("End");
        public static readonly string PageDown = GetString("PageDown");//Page Down
        public static readonly string PageUp = GetString("PageUp");//Page Up
        public static readonly string WebBrowser = GetString("WebBrowser");//Web Browser

        //Word
        public static readonly string Autoscroll = GetString("Word.1");
        public static readonly string Find = GetString("Word.2");
        public static readonly string IncreaseIndent = GetString("Word.3");
        public static readonly string NewComment = GetString("Word.4");
        public static readonly string NextChange = GetString("Word.5");
        public static readonly string NextComment = GetString("Word.6");
        public static readonly string PasteAndKeepSourceFormatting = GetString("Word.7");
        public static readonly string PasteAndKeepTextOnly = GetString("Word.8");
        public static readonly string PasteAndMatchFormatting = GetString("Word.9");
        public static readonly string PasteAndMergeFormatting = GetString("Word.10");
        public static readonly string PreviousChange = GetString("Word.11");
        public static readonly string PreviousComment = GetString("Word.12");
        public static readonly string Print = GetString("Word.13");
        public static readonly string Save = GetString("Word.14");
        public static readonly string Strikethrough = GetString("Word.15");
        public static readonly string TextSizeMinus = GetString("Word.16");
        public static readonly string TextSizePlus = GetString("Word.17");
        public static readonly string TranslateSelectedText = GetString("Word.18");
        public static readonly string ViewOnePage = GetString("Word.19");
        public static readonly string ViewPageWidth = GetString("Word.20");

        //Excel
        public static readonly string AlignCenter = GetString("Excel.1");
        public static readonly string AlignLeft = GetString("Excel.2");
        public static readonly string AlignRight = GetString("Excel.3");
        public static readonly string DecreaseIndent = GetString("Excel.4");
        public static readonly string GotoBottomOfDataRegion = GetString("Excel.5");
        public static readonly string GotoTopOfDataRegion = GetString("Excel.6");
        public static readonly string InsertChart = GetString("Excel.7");
        public static readonly string InsertRowAbove = GetString("Excel.8");
        public static readonly string PanHoldAndMoveMouse = GetString("Excel.9");
        public static readonly string PasteFormatOnly = GetString("Excel.10");
        public static readonly string PasteFormulas = GetString("Excel.11");
        public static readonly string PasteValuesOnly = GetString("Excel.12");
        public static readonly string PreviousSheet = GetString("Excel.13");
        public static readonly string SortAtoZ = GetString("Excel.14");

        //PowerPoint
        public static readonly string ArrangeAlignCenter = GetString("PowerPoint.1");
        public static readonly string ArrangeAlignLeft = GetString("PowerPoint.2");
        public static readonly string ArrangeAlignRight = GetString("PowerPoint.3");
        public static readonly string BringToFront = GetString("PowerPoint.4");
        public static readonly string DecreaseListLevel = GetString("PowerPoint.5");
        public static readonly string DuplicateSelectedSlides = GetString("PowerPoint.6");
        public static readonly string IncreaseListLevel = GetString("PowerPoint.7");
        public static readonly string PlayFromCurrentSlide = GetString("PowerPoint.8");
        public static readonly string PreviousSlide = GetString("PowerPoint.9");
        public static readonly string SendToBack = GetString("PowerPoint.10");

        //Outlook
        public static readonly string AttachFile = GetString("Outlook.1");
        public static readonly string ForwardEmail = GetString("Outlook.2");
        public static readonly string NewEmail = GetString("Outlook.3");
        public static readonly string NewMeeting = GetString("Outlook.4");
        public static readonly string Reply = GetString("Outlook.5");
        public static readonly string ReplyToAll = GetString("Outlook.6");

        //Open/Run
        public static readonly string Calendar = GetString("Calendar");//"Calendar";
        public static readonly string Camera = GetString("Camera");//"Camera";
        public static readonly string Clock = GetString("Clock");
        public static readonly string Cortana = GetString("Cortana");
        public static readonly string DellCommandUpdate = GetString("DellCommandUpdate");//Dell Command Update
        public static readonly string DellDigitalDelivery = GetString("DellDigitalDelivery");//Dell Digital Delivery
        public static readonly string DellOptimizer = GetString("DellOptimizer");//Dell Optimizer
        public static readonly string Family = GetString("Family");
        public static readonly string FeedbackHub = GetString("FeedbackHub");//Feedback Hub
        public static readonly string GameBar = GetString("GameBar");//Game Bar
        public static readonly string GetHelp = GetString("GetHelp");//Get Help
        public static readonly string GetStarted = GetString("GetStarted");//Get Started
        public static readonly string IntelManagementAndSecurityStatus = GetString("IntelManagementAndSecurityStatus");//Intel (R) Management and Security Status
        public static readonly string IntelGraphicsCommandCenter = GetString("IntelGraphicsCommandCenter");//Intel® Graphics Command Center
        public static readonly string IntelOptaneMemoryAndStorageManagement = GetString("IntelOptaneMemoryAndStorageManagement");//Intel® Optane™ Memory and Storage Management
        public static readonly string Mail = GetString("Mail");//"Mail";
        public static readonly string Maps = GetString("Maps");//"Maps";
        public static readonly string MediaPlayer = GetString("MediaPlayer");//Media Player
        public static readonly string Microsoft365Office = GetString("Microsoft365Office");//Microsoft 365 (Office)
        public static readonly string MicrosoftClipchamp = GetString("MicrosoftClipchamp");//Microsoft Clipchamp
        public static readonly string MicrosoftDefender = GetString("MicrosoftDefender");//Microsoft Defender
        public static readonly string MicrosoftStore = GetString("MicrosoftStore");//Microsoft Store
        public static readonly string MicrosoftTeams = GetString("MicrosoftTeams");//
        public static readonly string MicrosoftTeamsWorkSchool = GetString("MicrosoftTeamsWorkSchool");
        public static readonly string MicrosoftToDo = GetString("MicrosoftToDo");
        public static readonly string MoviesTV = GetString("MoviesTV");
        public static readonly string News = GetString("News");
        public static readonly string Notepad = GetString("Notepad");
        public static readonly string Paint = GetString("Paint");
        public static readonly string PhoneLink = GetString("PhoneLink");
        public static readonly string Photos = GetString("Photos");
        public static readonly string PowerAutomate = GetString("PowerAutomate");
        public static readonly string SnippingTool = GetString("SnippingTool");
        public static readonly string SolitaireCasualGames = GetString("SolitaireCasualGames");
        public static readonly string SoundRecorder = GetString("SoundRecorder");
        public static readonly string Spotify = GetString("Spotify");
        public static readonly string SupportAssist = GetString("SupportAssist");
        public static readonly string Terminal = GetString("Terminal");
        public static readonly string Tips = GetString("Tips");
        public static readonly string Weather = GetString("Weather");
        public static readonly string WindowsBackup = GetString("WindowsBackup");
        public static readonly string WindowsSecurity = GetString("WindowsSecurity");//"Windows Security";
        public static readonly string Xbox = GetString("Xbox");//"Xbox";

        //Radial Menu
        public static readonly string Disabled = GetString("RadialMenu.1");
        public static readonly string GoBack = GetString("RadialMenu.2");
        public static readonly string GoForward = GetString("RadialMenu.3");
        public static readonly string PlayPause = GetString("RadialMenu.4");
        public static readonly string PreviousTrack = GetString("RadialMenu.5");
        public static readonly string Mute = GetString("RadialMenu.6");
        public static readonly string FunctionForSelectedRadial = GetString("RadialMenu.7");
        public static readonly string LabelForSelectedRadial = GetString("RadialMenu.8");
        public static readonly string UseCenterForEmulatingRightClick = GetString("RadialMenu.9");

        //Common
        public static readonly string Browse = GetString("Common.0");
        public static readonly string Clear = GetString("Common.1");
        public static readonly string Cancel = GetString("Common.2");

        //Dialog
        public static readonly string OpenRunDesc = GetString("Common.3");
        public static readonly string SelectAFile = GetString("Common.4");
        public static readonly string SelectedFile = GetString("Common.5");

        //Pen settings
        public static readonly string PenSettings = GetString("PenSettings.0");
        public static readonly string TipSensitivity = GetString("PenSettings.1");
        public static readonly string TipTooltip = GetString("PenSettings.2");
        public static readonly string TiltSensitivity = GetString("PenSettings.3");
        public static readonly string TiltTooltip = GetString("PenSettings.4");
        public static readonly string PairWithTile = GetString("PenSettings.5");
        public static readonly string PairTooltip = GetString("PenSettings.6");
        public static readonly string GetStarted2 = GetString("PenSettings.7");
        public static readonly string PairTile1 = GetString("PenSettings.8");
        public static readonly string PairTile2 = GetString("PenSettings.9");
        public static readonly string PairTile3 = GetString("PenSettings.10");
        public static readonly string DownloadTile = GetString("PenSettings.11");

        //Dock
        public static readonly string USB_C_DP_14 = GetString("Dock.2");
        public static readonly string Dual_USB__C_DP_14 = GetString("Dock.3");
        public static readonly string DUSB__C_TB_4 = GetString("Dock.4");
        public static readonly string DUSB__C_TB_5 = GetString("Dock.5");

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
        public static readonly string CollabsCaption = GetString("Collab.1");
        public static readonly string CollaborationToolTip = GetString("Collab.2");
        public static readonly string CollaborationBlinkEffectText = GetString("Collab.3");
        public static readonly string CollaborationDoubleTapText = GetString("Collab.4");
        public static readonly string LearnMoreCaption = GetString("Collab.5");
        public static readonly string LearnMoreText1 = GetString("Collab.6");
        public static readonly string LearnMoreText2 = GetString("Collab.7");
        public static readonly string Alert1 = GetString("Collab.8");
        public static readonly string Alert2 = GetString("Collab.9");
        public static readonly string Alert3 = GetString("Collab.10");
        public static readonly string Alert4 = GetString("Collab.11");
        public static readonly string Alert5 = GetString("Collab.12");
        public static readonly string LearnMoreLink = GetString("Collab.5");
        public static readonly string VideoCaption = GetString("Collab.13");
        public static readonly string ShareCaption = GetString("Collab.14");
        public static readonly string ChatCaption = GetString("Collab.15");
        public static readonly string MicCaption = GetString("Collab.16");
        public static readonly string OKCaption = GetString("Collab.17");

        //Camera
        public static readonly string Preset = GetString("Camera.14");
        public static readonly string EditPreset = GetString("Camera.15");
        public static readonly string Smooth = GetString("Camera.16");
        public static readonly string Vibrant = GetString("Camera.17");
        public static readonly string Warm = GetString("Camera.18");
        public static readonly string Name = GetString("Camera.19");
        public static readonly string NameIsTaken = GetString("Camera.20");
        public static readonly string DefaultProfileTooltip = GetString("Camera.21");
        public static readonly string SmoothProfileTooltip = GetString("Camera.22");
        public static readonly string VibrantProfileTooltip = GetString("Camera.23");
        public static readonly string WarmProfileTooltip = GetString("Camera.24");

        //EazyMemory
        public static readonly string Am = GetString("AM");
        public static readonly string Pm = GetString("PM");
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
        public static readonly string ezMemoryAutoLaunchErrorStringForLaunchOptionPage = GetString("EazyMemory.41");
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
        public static readonly string EASYMEMORYSETUP = GetString("EazyMemory.39");
        public static readonly string BlankProfileSubText = GetString("EazyMemory.40");

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
        public static readonly string WalkThroughPen_Sub1_1 = GetString("WalkThroughPen_Sub.1_1");
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
        public static readonly string AssignKeystrokeDesc = GetString("Action.1");
        public static readonly string OpenFileDesc = GetString("Action.2");
        public static readonly string OpenFolderDesc = GetString("Action.3");
        public static readonly string OpenWebPageDesc = GetString("Action.4");
        public static readonly string OpenFileWaterMark = GetString("Action.5");
        public static readonly string OpenFolderWaterMark = GetString("Action.6");

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
        public static readonly string ImpExp_Tooltip1 = GetString("DisplayOthers.7");
        public static readonly string ImpExp_Tooltip2 = GetString("DisplayOthers.8");

        // Pair/Unpair
        public static readonly string Caption = GetString("PairedInfo.1");
        public static readonly string MessageMouse = GetString("PairedInfo.2");
        public static readonly string MessageKeyboard = GetString("PairedInfo.3");
        public static readonly string MessagePen = GetString("PairedInfo.4");
        public static readonly string MessageHeadset = GetString("PairedInfo.5");
        public static readonly string Continue = GetString("PairedInfo.6");
        public static readonly string Imcompatible = GetString("PairedInfo.7");
        public static readonly string PenAlreadyPaired = GetString("PairedInfo.8");
        public static readonly string PairYourPen = GetString("PairedInfo.9");
        public static readonly string PairYourPenMessage = GetString("PairedInfo.10");
        public static readonly string Paired_Info = GetString("PairedInfo.0");

        // Display Restore to Default
        public static readonly string RestoreToDefalutText = GetString("DisplayDefault.1");
        public static readonly string DisplayDefault0 = GetString("DisplayDefault.0");

        //Brightness right view messagebox
        public static readonly string BrightnessPageWarning = GetString("Warning");
        public static readonly string BrightnessPageNotice0 = GetString("Brightness.19");
        public static readonly string BrightnessPageNotice1 = GetString("Brightness.20");
        public static readonly string ALSRangeLevelLow = GetString("AutoBrightnessRangeLevel.0");
        public static readonly string ALSRangeLevelMid = GetString("AutoBrightnessRangeLevel.1");
        public static readonly string ALSRangeLevelHigh = GetString("AutoBrightnessRangeLevel.2");

        //InputSource
        public static readonly string InputTitle0 = GetString("InputSource.1");
        public static readonly string InputTitle1 = GetString("InputSource.5");


        public static readonly string DTPUnavailable = GetString("DTPUnavailable");//"DTP service is unavailable!";

        public static readonly string Auto_Color_Temperature_MSG = GetString("Color.10");

        //For DDPM 2.1.x.x
        public static readonly string USB_C = GetString("USBC");
        public static readonly string UpdateFirmware = GetString("UpdateFirmware");
        public static readonly string RtkHub01 = GetString("RtkHub.01");
        public static readonly string RtkHub02 = GetString("RtkHub.02");
        public static readonly string RtkHub03 = GetString("RtkHub.03");
        public static readonly string RtkHub04 = GetString("RtkHub.04");
        public static readonly string RtkHub05 = GetString("RtkHub.05");
        public static readonly string RtkHub06 = GetString("RtkHub.06");
        public static readonly string RtkHub07 = GetString("RtkHub.07");
        public static readonly string RtkHub08 = GetString("RtkHub.08");
        public static readonly string RtkHub09 = GetString("RtkHub.09");
        public static readonly string RtkHub10 = GetString("RtkHub.10");
        public static readonly string RtkHub11 = GetString("RtkHub.11");
        public static readonly string RtkHub12 = GetString("RtkHub.12");
        public static readonly string RtkHub13 = GetString("RtkHub.13");
        public static readonly string RtkHub14 = GetString("RtkHub.14");
        public static readonly string RtkHub15 = GetString("RtkHub.15");
        public static readonly string RtkHub16 = GetString("RtkHub.16");
        public static readonly string RtkHub17 = GetString("RtkHub.17");
        public static readonly string RtkHub18 = GetString("RtkHub.18");
        public static readonly string RtkHub19 = GetString("RtkHub.19");
        public static readonly string RtkHub20 = GetString("RtkHub.20");
        public static readonly string RtkHub21 = GetString("RtkHub.21");
        public static readonly string RtkHub22 = GetString("RtkHub.22");
        public static readonly string RtkHub23 = GetString("RtkHub.23");
        public static readonly string RtkHub24 = GetString("RtkHub.24");
        public static readonly string RtkHub25 = GetString("RtkHub.25");
        public static readonly string RtkHub26 = GetString("RtkHub.26");
        public static readonly string RtkHub27 = GetString("RtkHub.27");
        public static readonly string RtkHub28 = GetString("RtkHub.28");
        public static readonly string RtkHub29 = GetString("RtkHub.29");
    }
}
