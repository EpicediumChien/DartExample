using DDPM.SA.Common;
using DDPM.SA.Common.Settings;
using DDPM.Win32Lib;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.Common.Annotations;
using Dell.Client.Framework.Common.PluginConditions;
using Dell.Client.Framework.Interfaces;
using Microsoft;
using Newtonsoft.Json;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using Task = System.Threading.Tasks.Task;

namespace DDPM.SA.Plugins.User.ActionsManger
{
    [Plugin(Common.IDs.DDPM_ACTIONS_MANGER_PLUGIN_ID, pluginName, PluginOrderGroupType.Core, Version = pluginVersion)]
    [Descriptor(Description = pluginDescription)]
    [Publisher(Name = publisherCompany, Website = publisherWebsite, Support = publisherSupport)]
    public class ActionsMangerPlugin : BaseAgentPlugin, IDisposableObservable
    {
        #region Private Members

        private const string pluginName = "ActionsMangerPlugin";
        private const string pluginVersion = "1.0.0";
        private const string pluginDescription = "This plugin implements Actions Manger Plugin.";
        private const string publisherCompany = "Wistron";
        private const string publisherWebsite = "https://www.wistron.com";
        private const string publisherSupport = "This plugin implements Actions Manger Plugin.";

        private IAgent _agent;
        public const string PluginLogId = "ActionsManger";

        private IDPeMPlugin _PeripheralsPlugin;
        private readonly object _PluginConditionLock_Peripherals = new object();

        #endregion Private Members

        private void InitializePeripheralsPlugin()
        {
            if (_PeripheralsPlugin != null)
                return;

            _PeripheralsPlugin = _agent.PluginManager.FindPluginByType<IDPeMPlugin>(PluginResolution.Dynamic);

            if (_PeripheralsPlugin is IFrameworkPluginConditionNotification pluginCondition)
            {
                pluginCondition.PluginConditionChangeHandler += OnPeripheralsPluginConditionChangeHandler;
                GetCurrentPeripheralsPluginCondition();
            }
        }

        private void GetCurrentPeripheralsPluginCondition()
        {
            _ = Task.Run(async () =>
            {
                var pluginCondition = await (_PeripheralsPlugin as IFrameworkPluginConditionNotification)?.CurrentConditionAsync();
                lock (_PluginConditionLock_Peripherals)
                {
                    if (pluginCondition is PluginErrorCondition)
                    {
                        writelog($"{nameof(GetCurrentPeripheralsPluginCondition)} - Peripherals Plugin is in an error condition");
                    }
                    else if (pluginCondition is PluginRunningCondition)
                    {
                        writelog($"{nameof(GetCurrentPeripheralsPluginCondition)} - Peripherals Plugin is in a running condition");
                    }
                    else if (pluginCondition is PluginStartedCondition)
                    {
                        writelog($"{nameof(GetCurrentPeripheralsPluginCondition)} - Peripherals Plugin is in a started condition");
                        //var dev = _PeripheralsPlugin.GetDevices().Result;
                        //dev.deviceInfo.ForEach(di =>
                        //{
                        //    writelog("XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXx ... " + di.Name);
                        //});
                        //GetType(dev.deviceInfo[0]);
                    }
                }
            });
        }

        private void OnPeripheralsPluginConditionChangeHandler(object sender, EventArgs e)
        {
            GetCurrentPeripheralsPluginCondition();
        }

        public SWUErrorCode GetType(DeviceInfo devinfo)
        {
            //Necessary to handle the notification is coming from which device and model name.
            //。。。。。。。。。。。。。。。。。。

            var fileFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), @$"Dell\Dell Display and Peripheral Manager\Actions");
            string fileinfo = string.Empty, info = string.Empty;
            DDPMFileSecurity.CheckFold(fileFolder, out fileinfo, out info);
            if (!Directory.Exists(fileFolder))
                Directory.CreateDirectory(fileFolder);

            // Perform Input Validation: check file path
            string FileInfo;
            if (!DDPMFileSecurity.IsFilePathValid(fileFolder, out FileInfo))//Add Security
            {
                writelog($"{nameof(GetType)} - Actions Manager Plugin GetType IsFilePathValid " + FileInfo);
                return SWUErrorCode.FileIsNoSafe;
            }

            //Need to process modele name to file name
            //。。。。。。。。。。。。。。。。。。

            string jsonFilePath = Path.Combine(fileFolder, "KB555.json");//test KB555
            using (StreamReader sr = new StreamReader(jsonFilePath))
            using (JsonReader reader = new JsonTextReader(sr))
            {
                var serializer = new JsonSerializer();
                var jsonData = serializer.Deserialize<KeyboardActionsSA>(reader);

                // 解析後的數據
                writelog(jsonData.KeyActions.Count().ToString());

                string notify = "F12";// If notification from F12

                if (Enum.TryParse<KeyNameSA>(notify, out KeyNameSA keyToFind))
                {
                    if (jsonData.KeyActions.TryGetValue(keyToFind, out SelectedActionSA action))
                    {
                        // Find F12 的 SelectedAction
                        int assignedActionId = action.AssignedAction.ID;
                        //OpenApplication("calc.exe");
                        //WindowsAction_DoEMail();
                        WindowsAction_DoOpenNewBrowserTab();
                    }
                    else
                    {
                        // No F12 的 SelectedAction
                        writelog($"{notify} action not found.");
                    }
                }
                else
                {
                    writelog($"{notify} is not a valid KeyName.");
                }
            }
            return SWUErrorCode.NoError;
        }

        private void DoKeyboardAction(int id)//AllActions
        {
            switch (id)
            {
                case 0:
                    //No action
                    break;

                case 1:
                    WindowsAction_DoCopilot();
                    break;

                case 2:
                    WindowsAction_DoDevices();
                    break;

                case 3:
                    WindowsAction_DoLock();
                    break;

                case 4:
                    WindowsAction_DoNotificationCenter();
                    break;

                case 5:
                    WindowsAction_DoScreenSnip();
                    break;

                case 6:
                    WindowsAction_DoSearch();
                    break;

                case 7:
                    WindowsAction_DoSettings();
                    break;

                case 8:
                    WindowsAction_DoShowHideDesktop();
                    break;

                case 9:
                    WindowsAction_DoShutdown();
                    break;

                case 10:
                    WindowsAction_DoSignOut();
                    break;

                case 11:
                    WindowsAction_DoSleep();
                    break;

                case 12:
                    WindowsAction_DoSwitchApplications();
                    break;

                case 13:
                    WindowsAction_DoTaskView();
                    break;

                case 14:
                    ProductivityAction_AssignKeystroke();
                    break;

                case 15:
                    ProductivityAction_DoBack();
                    break;

                case 16:
                    ProductivityAction_DoCalculator();
                    break;

                case 17:
                    ProductivityAction_DoCloseWindow();
                    break;

                case 18:
                    ProductivityAction_DoCopy();
                    break;

                case 19:
                    ProductivityAction_DoCut();
                    break;

                case 20:
                    ProductivityAction_DoDocuments();
                    break;

                case 21:
                    ProductivityAction_DoForward();
                    break;

                case 22:
                    ProductivityAction_DoMaximizeWindow();
                    break;

                case 23:
                    ProductivityAction_DoMinimizeWindow();
                    break;

                case 24:
                    ProductivityAction_DoMyHome();
                    break;

                case 25:
                    ProductivityAction_DoOpenFile();
                    break;

                case 26:
                    ProductivityAction_DoOpenFolder();
                    break;

                case 27:
                    ProductivityAction_DoOpenNewBrowserTab();
                    break;

                case 28:
                    ProductivityAction_DoOpenWebPage();
                    break;

                case 29:
                    ProductivityAction_DoPaste();
                    break;

                case 30:
                    ProductivityAction_DoZoomIn();
                    break;

                case 31:
                    ProductivityAction_DoZoomOut();
                    break;

                case 32:
                    ProductivityAction_DoZoomReset();
                    break;

                case 33:
                    MultimediaAction_MediaNextTrack();
                    break;

                case 34:
                    MultimediaAction_MediaPlayPause();
                    break;

                case 35:
                    MultimediaAction_MediaPreviousTrack();
                    break;

                case 36:
                    MultimediaAction_DoMusic();
                    break;

                case 37:
                    MultimediaAction_DoPictures();
                    break;

                case 38:
                    MultimediaAction_VolumeDown();
                    break;

                case 39:
                    MultimediaAction_VolumeMute();
                    break;

                case 40:
                    MultimediaAction_VolumeUp();
                    break;

                case 41:
                    NoneAction_DoPrtSc();
                    break;

                case 42:
                    NoneAction_DoScrollLock();
                    break;

                case 43:
                    NoneAction_DoPauseBreak();
                    break;

                case 44:
                    NoneAction_DoHome();
                    break;

                case 45:
                    NoneAction_DoEnd();
                    break;

                case 46:
                    NoneAction_DoPageUp();
                    break;

                case 47:
                    NoneAction_DoPageDown();
                    break;

                case 48:
                    ProductivityAction_Do4thClick();
                    break;

                case 49:
                    ProductivityAction_Do5thClick();
                    break;

                case 50:
                    ProductivityAction_DoErase();
                    break;

                case 51:
                    ProductivityAction_DoLeftClick();
                    break;

                case 52:
                    ProductivityAction_DoMiddleClick();
                    break;

                case 53:
                    ProductivityAction_DoOpenRun();
                    break;

                case 54:
                    ProductivityAction_DoPenPageDown();
                    break;

                case 55:
                    ProductivityAction_DoPenPageUp();
                    break;

                case 56:
                    ProductivityAction_DoRadialMenu();
                    break;

                case 57:
                    ProductivityAction_DoRedo();
                    break;

                case 58:
                    ProductivityAction_DoRightClick();
                    break;

                case 59:
                    ProductivityAction_DoUndo();
                    break;

                case 60:
                    WindowsAction_DoBarrelButton();
                    break;

                case 61:
                    WindowsAction_DoEMail();
                    break;

                case 62:
                    WindowsAction_DoOneNote();
                    break;

                case 63:
                    WindowsAction_DoOpenNewBrowserTab();
                    break;

                case 64:
                    WindowsAction_DoSwitchApplication();
                    break;

                case 65:
                    WindowsAction_DoWidgets();
                    break;

                case 66:
                    WindowsAction_DoWindowsSearch();
                    break;

                case 67:
                    MultimediaAction_NextTrack();
                    break;

                default:
                    // Handle any cases that aren't explicitly covered
                    break;
            }
        }

        private void DoOfficeActions(int id)
        {
            switch (id)
            {
                // Word Actions (101 - 122)
                case 101:
                    WordAction_Autoscroll();
                    break;

                case 102:
                    WordAction_Find();
                    break;

                case 103:
                    WordAction_IncreaseIndent();
                    break;

                case 104:
                    WordAction_NewComment();
                    break;

                case 105:
                    WordAction_NextChange();
                    break;

                case 106:
                    WordAction_NextComment();
                    break;

                case 107:
                    WordAction_PasteAndKeepSourceFormatting();
                    break;

                case 108:
                    WordAction_PasteAndKeepTextOnly();
                    break;

                case 109:
                    WordAction_PasteAndMatchFormatting();
                    break;

                case 110:
                    WordAction_PasteAndMergeFormatting();
                    break;

                case 111:
                    WordAction_PreviousChange();
                    break;

                case 112:
                    WordAction_PreviousComment();
                    break;

                case 113:
                    WordAction_Print();
                    break;

                case 114:
                    WordAction_Save();
                    break;

                case 115:
                    WordAction_Strikethrough();
                    break;

                case 116:
                    WordAction_TextSizeMinus();
                    break;

                case 117:
                    WordAction_TextSizePlus();
                    break;

                case 118:
                    WordAction_TranslateSelectedText();
                    break;

                case 119:
                    WordAction_ViewOnePage();
                    break;

                case 120:
                    WordAction_ViewPageWidth();
                    break;

                case 121:
                    WordAction_ZoomIn();
                    break;

                case 122:
                    WordAction_ZoomOut();
                    break;
                // Excel Actions (201 - 221)
                case 201:
                    ExcelAction_AlignCenter();
                    break;

                case 202:
                    ExcelAction_AlignLeft();
                    break;

                case 203:
                    ExcelAction_AlignRight();
                    break;

                case 204:
                    ExcelAction_DecreaseIndent();
                    break;

                case 205:
                    ExcelAction_GotoBottomOfDataRegion();
                    break;

                case 206:
                    ExcelAction_GotoTopOfDataRegion();
                    break;

                case 207:
                    ExcelAction_IncreaseIndent();
                    break;

                case 208:
                    ExcelAction_InsertChart();
                    break;

                case 209:
                    ExcelAction_InsertRowAbove();
                    break;

                case 210:
                    ExcelAction_NewComment();
                    break;

                case 211:
                    ExcelAction_NextComment();
                    break;

                case 212:
                    ExcelAction_PanHoldAndMoveMouse();
                    break;

                case 213:
                    ExcelAction_PasteFormatOnly();
                    break;

                case 214:
                    ExcelAction_PasteFormulas();
                    break;

                case 215:
                    ExcelAction_PasteValueOnly();
                    break;

                case 216:
                    ExcelAction_PreviousComment();
                    break;

                case 217:
                    ExcelAction_PreviousSheet();
                    break;

                case 218:
                    ExcelAction_Save();
                    break;

                case 219:
                    ExcelAction_SortAtoZ();
                    break;

                case 220:
                    ExcelAction_ZoomIn();
                    break;

                case 221:
                    ExcelAction_ZoomOut();
                    break;
                // PowerPoint Actions (301 - 320)
                case 301:
                    PowerPointAction_ArrangeAlignCenter();
                    break;

                case 302:
                    PowerPointAction_ArrangeAlignLeft();
                    break;

                case 303:
                    PowerPointAction_ArrangeAlignRight();
                    break;

                case 304:
                    PowerPointAction_BringToFront();
                    break;

                case 305:
                    PowerPointAction_DecreaseListLevel();
                    break;

                case 306:
                    PowerPointAction_DuplicateSelectedSlides();
                    break;

                case 307:
                    PowerPointAction_IncreaseListLevel();
                    break;

                case 308:
                    PowerPointAction_NewComment();
                    break;

                case 309:
                    PowerPointAction_NextComment();
                    break;

                case 310:
                    PowerPointAction_PanHoldAndMoveMouse();
                    break;

                case 311:
                    PowerPointAction_PlayFromCurrentSlide();
                    break;

                case 312:
                    PowerPointAction_PreviousComment();
                    break;

                case 313:
                    PowerPointAction_PreviousSlide();
                    break;

                case 314:
                    PowerPointAction_Save();
                    break;

                case 315:
                    PowerPointAction_SendToBack();
                    break;

                case 316:
                    PowerPointAction_Strikethrough();
                    break;

                case 317:
                    PowerPointAction_TextSizeMinus();
                    break;

                case 318:
                    PowerPointAction_TextSizePlus();
                    break;

                case 319:
                    PowerPointAction_ZoomIn();
                    break;

                case 320:
                    PowerPointAction_ZoomOut();
                    break;
                // Outlook Actions (401 - 408)
                case 401:
                    OutlookAction_AttachFile();
                    break;

                case 402:
                    OutlookAction_DecreaseIndent();
                    break;

                case 403:
                    OutlookAction_ForwardEmail();
                    break;

                case 404:
                    OutlookAction_IncreaseIndent();
                    break;

                case 405:
                    OutlookAction_NewEmail();
                    break;

                case 406:
                    OutlookAction_NewMeeting();
                    break;

                case 407:
                    OutlookAction_Reply();
                    break;

                case 408:
                    OutlookAction_ReplyToAll();
                    break;

                default:
                    // Handle any cases that aren't explicitly covered
                    break;
            }
        }

        // Main method for handling pen top button actions
        private void PenTopButtonActions(int id)
        {
            switch (id)
            {
                case 0:
                    // No action
                    break;

                case 14:
                    ProductivityAction_AssignKeystroke();
                    break;

                case 53:
                    ProductivityAction_DoOpenRun();
                    break;

                case 54:
                    ProductivityAction_DoPenPageDown();
                    break;

                case 55:
                    ProductivityAction_DoPenPageUp();
                    break;

                case 91:
                    WindowsAction_DefineBySystem();
                    break;

                case 62:
                    WindowsAction_DoOneNote();
                    break;

                case 92:
                    WindowsAction_DoPenMenu();
                    break;

                case 93:
                    WindowsAction_DoQuickNote();
                    break;

                case 94:
                    WindowsAction_DoScreenSnip();
                    break;

                case 95:
                    WindowsAction_DoStickyNotes();
                    break;

                case 66:
                    WindowsAction_DoWindowsSearch();
                    break;

                case 34:
                    MultimediaAction_MediaPlayPause();
                    break;

                case 35:
                    MultimediaAction_MediaPreviousTrack();
                    break;

                case 67:
                    MultimediaAction_NextTrack();
                    break;

                case 38:
                    MultimediaAction_VolumeDown();
                    break;

                case 39:
                    MultimediaAction_VolumeMute();
                    break;

                case 40:
                    MultimediaAction_VolumeUp();
                    break;

                default:
                    // Handle any cases that aren't explicitly covered
                    break;
            }
        }

        private void OpenRunActions(int id)
        {
            switch (id)
            {
                case 1:
                    OpenApplication("explorer.exe");
                    break;

                case 2:
                    OpenApplication("calc.exe");
                    break;

                case 3:
                    OpenApplication("outlookcal:");
                    break;

                case 4:
                    OpenApplication("microsoft.windows.camera:");
                    break;

                case 5:
                    OpenApplication("ms-clock:");
                    break;

                case 6:
                    OpenApplication("ms-cortana:");
                    break;

                case 7:
                    OpenApplication("DellCommandUpdate.exe");
                    break;

                case 8:
                    OpenApplication("DellDigitalDelivery.exe");
                    break;

                case 9:
                    OpenApplication("DellOptimizer.exe");
                    break;

                case 10:
                    OpenApplication("ms-family:");
                    break;

                case 11:
                    OpenApplication("feedback-hub:");
                    break;

                case 12:
                    OpenApplication("ms-gamingoverlay:");
                    break;

                case 13:
                    OpenApplication("ms-contact-support:");
                    break;

                case 14:
                    OpenApplication("ms-get-started:");
                    break;

                case 15:
                    OpenApplication("imssvc.exe");
                    break;

                case 16:
                    OpenApplication("igfxCUIService.exe");
                    break;

                case 17:
                    OpenApplication("IntelOptaneMemory.exe");
                    break;

                case 18:
                    OpenApplication("ms-mail:");
                    break;

                case 19:
                    OpenApplication("ms-drive-to:");
                    break;

                case 20:
                    OpenApplication("mswindowsmusic:");
                    break;

                case 21:
                    OpenApplication("ms-officehub:");
                    break;

                case 22:
                    OpenApplication("clipchamp:");
                    break;

                case 23:
                    OpenApplication("windowsdefender:");
                    break;

                case 24:
                    OpenApplication("ms-windows-store:");
                    break;

                case 25:
                    OpenApplication("msteams:");
                    break;

                case 26:
                    OpenApplication("msteams://");
                    break;

                case 27:
                    OpenApplication("todo:");
                    break;

                case 28:
                    OpenApplication("mswindowsvideo:");
                    break;

                case 29:
                    OpenApplication("bingnews:");
                    break;

                case 30:
                    OpenApplication("notepad.exe");
                    break;

                case 31:
                    OpenApplication("mspaint.exe");
                    break;

                case 32:
                    OpenApplication("ms-phone:");
                    break;

                case 33:
                    OpenApplication("ms-photos:");
                    break;

                case 34:
                    OpenApplication("ms-powerautomate:");
                    break;

                case 35:
                    OpenApplication("ms-settings:");
                    break;

                case 36:
                    OpenApplication("SnippingTool.exe");
                    break;

                case 37:
                    OpenApplication("xboxliveapp-1297287741://");
                    break;

                case 38:
                    OpenApplication("ms-soundrecorder:");
                    break;

                case 39:
                    OpenApplication("spotify:");
                    break;

                case 40:
                    OpenApplication("ms-stickynotes:");
                    break;

                case 41:
                    OpenApplication("SupportAssist.exe");
                    break;

                case 42:
                    OpenApplication("wt.exe");
                    break;

                case 43:
                    OpenApplication("ms-get-started:");
                    break;

                case 44:
                    OpenApplication("msnweather:");
                    break;

                case 45:
                    OpenApplication("ms-settings:windowsbackup");
                    break;

                case 46:
                    OpenApplication("windowsdefender:");
                    break;

                case 47:
                    OpenApplication("xbox:");
                    break;

                default:
                    MessageBox.Show("Invalid ID. Please provide a valid action ID.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
            }
        }

        private void OpenApplication(string command)
        {
            try
            {
                Process.Start(command);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to open application: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RadialMenuActions(int id)
        {
            switch (id)
            {
                case 1:
                    // Disabled, no action
                    break;

                case 2:
                    ProductivityAction_AssignKeystroke();
                    break;

                case 3:
                    OpenRunActions(3); // Assuming OpenRun2 is equivalent to OpenRun with ID 3
                    break;

                case 4:
                    ProductivityAction_DoBack();
                    break;

                case 5:
                    ProductivityAction_DoForward();
                    break;

                case 6:
                    WindowsAction_DoSwitchApplication();
                    break;

                case 7:
                    ProductivityAction_DoCopy();
                    break;

                case 8:
                    ProductivityAction_DoPaste();
                    break;

                case 9:
                    ProductivityAction_DoUndo();
                    break;

                case 10:
                    ProductivityAction_DoRedo();
                    break;

                case 11:
                    NoneAction_DoPageUp();
                    break;

                case 12:
                    NoneAction_DoPageDown();
                    break;

                case 13:
                    WindowsAction_DoOneNote();
                    break;

                case 14:
                    ProductivityAction_OpenWebBrowser();
                    break;

                case 15:
                    WindowsAction_DoEMail();
                    break;

                case 16:
                    MultimediaAction_MediaPlayPause();
                    break;

                case 17:
                    MultimediaAction_MediaNextTrack();
                    break;

                case 18:
                    MultimediaAction_MediaPreviousTrack();
                    break;

                case 19:
                    MultimediaAction_VolumeUp();
                    break;

                case 20:
                    MultimediaAction_VolumeDown();
                    break;

                case 21:
                    MultimediaAction_VolumeMute();
                    break;

                case 22:
                    WindowsAction_DoWindowsSearch();
                    break;

                default:
                    MessageBox.Show("Invalid ID. Please provide a valid action ID.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    break;
            }
        }

        private void ProductivityAction_OpenWebBrowser()
        {
            // Simulate opening the default web browser
            Process.Start("http://www.example.com");
        }

        #region AllActions Event

        private static void WindowsAction_DoCopilot()
        {
            // Send Win + C (Open Copilot)
            Win32._keybd_event(Win32.VK_LWIN, 0, Win32.KEYEVENTF_KEYDOWN, UIntPtr.Zero);
            SendKeys.SendWait("c");
            Win32._keybd_event(Win32.VK_LWIN, 0, Win32.KEYEVENTF_KEYUP, UIntPtr.Zero);
        }

        private static void WindowsAction_DoDevices()
        {
            // Open "Devices" settings page (Open settings with Win + I and select Devices)
            Win32._keybd_event(Win32.VK_LWIN, 0, Win32.KEYEVENTF_KEYDOWN, UIntPtr.Zero);
            SendKeys.SendWait("i");
            Win32._keybd_event(Win32.VK_LWIN, 0, Win32.KEYEVENTF_KEYUP, UIntPtr.Zero);
            // Assuming Devices is the default option, press Enter
            SendKeys.SendWait("{ENTER}");
        }

        private static void WindowsAction_DoLock()
        {
            // Send Win + L (Lock screen)
            Win32._keybd_event(Win32.VK_LWIN, 0, Win32.KEYEVENTF_KEYDOWN, UIntPtr.Zero);
            SendKeys.SendWait("l");
            Win32._keybd_event(Win32.VK_LWIN, 0, Win32.KEYEVENTF_KEYUP, UIntPtr.Zero);
        }

        private static void WindowsAction_DoNotificationCenter()
        {
            // Send Win + A (Open Notification Center)
            Win32._keybd_event(Win32.VK_LWIN, 0, Win32.KEYEVENTF_KEYDOWN, UIntPtr.Zero);
            SendKeys.SendWait("a");
            Win32._keybd_event(Win32.VK_LWIN, 0, Win32.KEYEVENTF_KEYUP, UIntPtr.Zero);
        }

        private static void WindowsAction_DoScreenSnip()
        {
            // Send Win + Shift + S (Start Screen Snip tool)
            Win32._keybd_event(Win32.VK_LWIN, 0, Win32.KEYEVENTF_KEYDOWN, UIntPtr.Zero);
            Win32._keybd_event(Win32.VK_SHIFT, 0, Win32.KEYEVENTF_KEYDOWN, UIntPtr.Zero);
            Win32._keybd_event(Win32.VK_S, 0, Win32.KEYEVENTF_KEYDOWN, UIntPtr.Zero);
            Win32._keybd_event(Win32.VK_S, 0, Win32.KEYEVENTF_KEYUP, UIntPtr.Zero);
            Win32._keybd_event(Win32.VK_SHIFT, 0, Win32.KEYEVENTF_KEYUP, UIntPtr.Zero);
            Win32._keybd_event(Win32.VK_LWIN, 0, Win32.KEYEVENTF_KEYUP, UIntPtr.Zero);
        }

        private static void WindowsAction_DoSearch()
        {
            // Send Win + S (Open Search)
            Win32._keybd_event(Win32.VK_LWIN, 0, Win32.KEYEVENTF_KEYDOWN, UIntPtr.Zero);
            SendKeys.SendWait("s");
            Win32._keybd_event(Win32.VK_LWIN, 0, Win32.KEYEVENTF_KEYUP, UIntPtr.Zero);
        }

        private static void WindowsAction_DoSettings()
        {
            // Send Win + I (Open Settings)
            Win32._keybd_event(Win32.VK_LWIN, 0, Win32.KEYEVENTF_KEYDOWN, UIntPtr.Zero);
            SendKeys.SendWait("i");
            Win32._keybd_event(Win32.VK_LWIN, 0, Win32.KEYEVENTF_KEYUP, UIntPtr.Zero);
        }

        private static void WindowsAction_DoShowHideDesktop()
        {
            // Send Win + D (Show/Hide Desktop)
            Win32._keybd_event(Win32.VK_LWIN, 0, Win32.KEYEVENTF_KEYDOWN, UIntPtr.Zero);
            SendKeys.SendWait("d");
            Win32._keybd_event(Win32.VK_LWIN, 0, Win32.KEYEVENTF_KEYUP, UIntPtr.Zero);
        }

        private static void WindowsAction_DoShutdown()
        {
            // Send Alt + F4 and confirm shutdown
            SendKeys.SendWait("%{F4}");
            SendKeys.SendWait("{ENTER}");
        }

        private static void WindowsAction_DoSignOut()
        {
            // Send Ctrl + Alt + Delete and select Sign out (Depends on system configuration)
            SendKeys.SendWait("^{%}{DELETE}");
            // Assuming Sign out is the default option, press Enter
            SendKeys.SendWait("{ENTER}");
        }

        private static void WindowsAction_DoSleep()
        {
            // Send Win + X and select Sleep mode (Assuming it's an option in the dropdown menu)
            Win32._keybd_event(Win32.VK_LWIN, 0, Win32.KEYEVENTF_KEYDOWN, UIntPtr.Zero);
            SendKeys.SendWait("x");
            Win32._keybd_event(Win32.VK_LWIN, 0, Win32.KEYEVENTF_KEYUP, UIntPtr.Zero);
            // Navigate to "Sleep" option (Might need to press arrow keys several times)
            SendKeys.SendWait("{DOWN 4}"); // Adjust number as needed
            SendKeys.SendWait("{ENTER}");
        }

        private static void WindowsAction_DoSwitchApplications()
        {
            // Send Alt + Tab (Switch applications)
            SendKeys.SendWait("%{TAB}");
        }

        private static void WindowsAction_DoTaskView()
        {
            // Send Win + Tab (Open Task View)
            Win32._keybd_event(Win32.VK_LWIN, 0, Win32.KEYEVENTF_KEYDOWN, UIntPtr.Zero);
            SendKeys.SendWait("{TAB}");
            Win32._keybd_event(Win32.VK_LWIN, 0, Win32.KEYEVENTF_KEYUP, UIntPtr.Zero);
        }

        private static void ProductivityAction_AssignKeystroke()
        {
            // Implementation depends on specific use case
        }

        private static void ProductivityAction_DoBack()
        {
            // Send Alt + Left (Go back)
            SendKeys.SendWait("%{LEFT}");
        }

        private static void ProductivityAction_DoCalculator()
        {
            // Send Win + R and run calc.exe
            Win32._keybd_event(Win32.VK_LWIN, 0, Win32.KEYEVENTF_KEYDOWN, UIntPtr.Zero);
            SendKeys.SendWait("r");
            Win32._keybd_event(Win32.VK_LWIN, 0, Win32.KEYEVENTF_KEYUP, UIntPtr.Zero);
            SendKeys.SendWait("calc{ENTER}");
        }

        private static void ProductivityAction_DoCloseWindow()
        {
            // Send Alt + F4 (Close window)
            SendKeys.SendWait("%{F4}");
        }

        private static void ProductivityAction_DoCopy()
        {
            // Send Ctrl + C (Copy)
            SendKeys.SendWait("^{C}");
        }

        private static void ProductivityAction_DoCut()
        {
            // Send Ctrl + X (Cut)
            SendKeys.SendWait("^{X}");
        }

        private static void ProductivityAction_DoDocuments()
        {
            // Send Win + E and open Documents folder
            Win32._keybd_event(Win32.VK_LWIN, 0, Win32.KEYEVENTF_KEYDOWN, UIntPtr.Zero);
            SendKeys.SendWait("e");
            Win32._keybd_event(Win32.VK_LWIN, 0, Win32.KEYEVENTF_KEYUP, UIntPtr.Zero);
            SendKeys.SendWait("%d{ENTER}");
        }

        private static void ProductivityAction_DoForward()
        {
            // Send Alt + Right (Go forward)
            SendKeys.SendWait("%{RIGHT}");
        }

        private static void ProductivityAction_DoMaximizeWindow()
        {
            // Send Win + Up Arrow (Maximize window)
            Win32._keybd_event(Win32.VK_LWIN, 0, Win32.KEYEVENTF_KEYDOWN, UIntPtr.Zero);
            SendKeys.SendWait("{UP}");
            Win32._keybd_event(Win32.VK_LWIN, 0, Win32.KEYEVENTF_KEYUP, UIntPtr.Zero);
        }

        private static void ProductivityAction_DoMinimizeWindow()
        {
            // Send Win + Down Arrow (Minimize window)
            Win32._keybd_event(Win32.VK_LWIN, 0, Win32.KEYEVENTF_KEYDOWN, UIntPtr.Zero);
            SendKeys.SendWait("{DOWN}");
            Win32._keybd_event(Win32.VK_LWIN, 0, Win32.KEYEVENTF_KEYUP, UIntPtr.Zero);
        }

        private static void ProductivityAction_DoMyHome()
        {
            // Send Win + E (Open File Explorer)
            Win32._keybd_event(Win32.VK_LWIN, 0, Win32.KEYEVENTF_KEYDOWN, UIntPtr.Zero);
            SendKeys.SendWait("e");
            Win32._keybd_event(Win32.VK_LWIN, 0, Win32.KEYEVENTF_KEYUP, UIntPtr.Zero);
        }

        private static void ProductivityAction_DoOpenFile()
        {
            // Send Ctrl + O (Open file)
            SendKeys.SendWait("^{O}");
        }

        private static void ProductivityAction_DoOpenFolder()
        {
            // Send Ctrl + Shift + O (Open folder)
            SendKeys.SendWait("^{+O}");
        }

        private static void ProductivityAction_DoOpenNewBrowserTab()
        {
            // Send Ctrl + T (Open new browser tab)
            SendKeys.SendWait("^{T}");
        }

        private static void ProductivityAction_DoOpenWebPage()
        {
            // Send Alt + D and enter a URL (Open web page)
            SendKeys.SendWait("%d");
            SendKeys.SendWait("https://www.example.com{ENTER}");
        }

        private static void ProductivityAction_DoPaste()
        {
            // Send Ctrl + V (Paste)
            SendKeys.SendWait("^{V}");
        }

        private static void ProductivityAction_DoZoomIn()
        {
            // Send Ctrl + Plus (Zoom in)
            SendKeys.SendWait("^{+}");
        }

        private static void ProductivityAction_DoZoomOut()
        {
            // Send Ctrl + Minus (Zoom out)
            SendKeys.SendWait("^{-}");
        }

        private static void ProductivityAction_DoZoomReset()
        {
            // Send Ctrl + 0 (Reset zoom)
            SendKeys.SendWait("^{0}");
        }

        private static void MultimediaAction_MediaNextTrack()
        {
            // Send Media Next Track key
            SendKeys.SendWait("{MEDIA_NEXT_TRACK}");
        }

        private static void MultimediaAction_MediaPlayPause()
        {
            // Send Media Play/Pause key
            SendKeys.SendWait("{MEDIA_PLAY_PAUSE}");
        }

        private static void MultimediaAction_MediaPreviousTrack()
        {
            // Send Media Previous Track key
            SendKeys.SendWait("{MEDIA_PREV_TRACK}");
        }

        private static void MultimediaAction_DoMusic()
        {
            // Open default music player
            SendKeys.SendWait("^{+M}");
        }

        private static void MultimediaAction_DoPictures()
        {
            // Open default pictures folder
            Win32._keybd_event(Win32.VK_LWIN, 0, Win32.KEYEVENTF_KEYDOWN, UIntPtr.Zero);
            SendKeys.SendWait("e");
            Win32._keybd_event(Win32.VK_LWIN, 0, Win32.KEYEVENTF_KEYUP, UIntPtr.Zero);
            SendKeys.SendWait("^{P}");
        }

        private static void MultimediaAction_VolumeDown()
        {
            // Send Volume Down key
            SendKeys.SendWait("{VOLUME_DOWN}");
        }

        private static void MultimediaAction_VolumeMute()
        {
            // Send Volume Mute key
            SendKeys.SendWait("{VOLUME_MUTE}");
        }

        private static void MultimediaAction_VolumeUp()
        {
            // Send Volume Up key
            SendKeys.SendWait("{VOLUME_UP}");
        }

        private static void NoneAction_DoPrtSc()
        {
            // Send Print Screen key
            SendKeys.SendWait("{PRTSC}");
        }

        private static void NoneAction_DoScrollLock()
        {
            // Send Scroll Lock key
            SendKeys.SendWait("{SCROLLLOCK}");
        }

        private static void NoneAction_DoPauseBreak()
        {
            // Send Pause/Break key
            SendKeys.SendWait("{BREAK}");
        }

        private static void NoneAction_DoHome()
        {
            // Send Home key
            SendKeys.SendWait("{HOME}");
        }

        private static void NoneAction_DoEnd()
        {
            // Send End key
            SendKeys.SendWait("{END}");
        }

        private static void NoneAction_DoPageUp()
        {
            // Send Page Up key
            SendKeys.SendWait("{PGUP}");
        }

        private static void NoneAction_DoPageDown()
        {
            // Send Page Down key
            SendKeys.SendWait("{PGDN}");
        }

        private static void ProductivityAction_Do4thClick()
        {
            // Send 4th mouse button click
            SendKeys.SendWait("{XBUTTON1}");
        }

        private static void ProductivityAction_Do5thClick()
        {
            // Send 5th mouse button click
            SendKeys.SendWait("{XBUTTON2}");
        }

        private static void ProductivityAction_DoErase()
        {
            // Simulate erase action (Specific implementation may vary)
            SendKeys.SendWait("{ERASE}");
        }

        private static void ProductivityAction_DoLeftClick()
        {
            // Send left mouse button click
            SendKeys.SendWait("{LEFTCLICK}");
        }

        private static void ProductivityAction_DoMiddleClick()
        {
            // Send middle mouse button click
            SendKeys.SendWait("{MIDDLECLICK}");
        }

        private static void ProductivityAction_DoOpenRun()
        {
            // Send Win + R (Open Run dialog)
            Win32._keybd_event(Win32.VK_LWIN, 0, Win32.KEYEVENTF_KEYDOWN, UIntPtr.Zero);
            SendKeys.SendWait("r");
            Win32._keybd_event(Win32.VK_LWIN, 0, Win32.KEYEVENTF_KEYUP, UIntPtr.Zero);
        }

        private static void ProductivityAction_DoPenPageDown()
        {
            // Simulate pen page down (Specific implementation may vary)
            SendKeys.SendWait("{PENDOWN}");
        }

        private static void ProductivityAction_DoPenPageUp()
        {
            // Simulate pen page up (Specific implementation may vary)
            SendKeys.SendWait("{PENUP}");
        }

        private static void ProductivityAction_DoRadialMenu()
        {
            // Simulate opening radial menu (Specific implementation may vary)
            SendKeys.SendWait("{RADIALMENU}");
        }

        private static void ProductivityAction_DoRedo()
        {
            // Send Ctrl + Y (Redo)
            SendKeys.SendWait("^{Y}");
        }

        private static void ProductivityAction_DoRightClick()
        {
            // Send right mouse button click
            SendKeys.SendWait("{RIGHTCLICK}");
        }

        private static void ProductivityAction_DoUndo()
        {
            // Send Ctrl + Z (Undo)
            SendKeys.SendWait("^{Z}");
        }

        private static void WindowsAction_DoBarrelButton()
        {
            // Simulate barrel button action (Specific implementation may vary)
            SendKeys.SendWait("{BARRELBUTTON}");
        }

        private static void WindowsAction_DoEMail()
        {
            // Send Win + E (Open email client)
            Win32._keybd_event(Win32.VK_LWIN, 0, Win32.KEYEVENTF_KEYDOWN, UIntPtr.Zero);
            SendKeys.SendWait("e");
            Win32._keybd_event(Win32.VK_LWIN, 0, Win32.KEYEVENTF_KEYUP, UIntPtr.Zero);
            // Navigate to email client
            SendKeys.SendWait("%m");
        }

        private static void WindowsAction_DoOneNote()
        {
            // Send Win + N (Open OneNote)
            Win32._keybd_event(Win32.VK_LWIN, 0, Win32.KEYEVENTF_KEYDOWN, UIntPtr.Zero);
            SendKeys.SendWait("n");
            Win32._keybd_event(Win32.VK_LWIN, 0, Win32.KEYEVENTF_KEYUP, UIntPtr.Zero);
        }

        private static void WindowsAction_DoOpenNewBrowserTab()
        {
            // Send Ctrl + T (Open new browser tab)
            SendKeys.SendWait("^{T}");
        }

        private static void WindowsAction_DoSwitchApplication()
        {
            // Send Alt + Tab (Switch application)
            SendKeys.SendWait("%{TAB}");
        }

        private static void WindowsAction_DoWidgets()
        {
            // Send Win + W (Open Widgets)
            Win32._keybd_event(Win32.VK_LWIN, 0, Win32.KEYEVENTF_KEYDOWN, UIntPtr.Zero);
            SendKeys.SendWait("w");
            Win32._keybd_event(Win32.VK_LWIN, 0, Win32.KEYEVENTF_KEYUP, UIntPtr.Zero);
        }

        private static void WindowsAction_DoWindowsSearch()
        {
            // Send Win + S (Open Windows Search)
            Win32._keybd_event(Win32.VK_LWIN, 0, Win32.KEYEVENTF_KEYDOWN, UIntPtr.Zero);
            SendKeys.SendWait("s");
            Win32._keybd_event(Win32.VK_LWIN, 0, Win32.KEYEVENTF_KEYUP, UIntPtr.Zero);
        }

        private static void MultimediaAction_NextTrack()
        {
            // Send Media Next Track key
            SendKeys.SendWait("{MEDIA_NEXT_TRACK}");
        }

        #endregion AllActions Event

        #region Word Actions

        private void WordAction_Autoscroll()
        {
            // Autoscroll: Ctrl + Alt + Middle mouse button
            Win32._keybd_event(Win32.VK_CONTROL, 0, Win32.KEYEVENTF_KEYDOWN, UIntPtr.Zero);
            Win32._keybd_event(Win32.VK_ALT, 0, Win32.KEYEVENTF_KEYDOWN, UIntPtr.Zero);
            // Simulate middle mouse button click
            MouseClickMiddleButton();
            Win32._keybd_event(Win32.VK_ALT, 0, Win32.KEYEVENTF_KEYUP, UIntPtr.Zero);
            Win32._keybd_event(Win32.VK_CONTROL, 0, Win32.KEYEVENTF_KEYUP, UIntPtr.Zero);
        }

        private void WordAction_Find()
        {
            // Find: Ctrl + F
            SendKeys.SendWait("^{F}");
        }

        private void WordAction_IncreaseIndent()
        {
            // Increase indent: Ctrl + M
            SendKeys.SendWait("^{M}");
        }

        private void WordAction_NewComment()
        {
            // New comment: Ctrl + Alt + M
            SendKeys.SendWait("^%{M}");
        }

        private void WordAction_NextChange()
        {
            // Next change: Alt + Shift + N
            SendKeys.SendWait("%+{N}");
        }

        private void WordAction_NextComment()
        {
            // Next comment: Ctrl + Alt + Home
            SendKeys.SendWait("^%{HOME}");
        }

        private void WordAction_PasteAndKeepSourceFormatting()
        {
            // Paste and keep source formatting: Ctrl + Alt + V, then select keep source formatting
            SendKeys.SendWait("^%{V}");
            // Further navigation needed depending on context
        }

        private void WordAction_PasteAndKeepTextOnly()
        {
            // Paste as text only: Ctrl + Alt + V, then select text only
            SendKeys.SendWait("^%{V}");
            // Navigate to text only option and press enter
        }

        private void WordAction_PasteAndMatchFormatting()
        {
            // Paste and match formatting: Ctrl + Alt + V, then select match formatting
            SendKeys.SendWait("^%{V}");
            // Navigate to match formatting option and press enter
        }

        private void WordAction_PasteAndMergeFormatting()
        {
            // Paste and merge formatting: Ctrl + Alt + V, then select merge formatting
            SendKeys.SendWait("^%{V}");
            // Navigate to merge formatting option and press enter
        }

        private void WordAction_PreviousChange()
        {
            // Previous change: Alt + Shift + P
            SendKeys.SendWait("%+{P}");
        }

        private void WordAction_PreviousComment()
        {
            // Previous comment: Ctrl + Alt + PageUp
            SendKeys.SendWait("^%{PGUP}");
        }

        private void WordAction_Print()
        {
            // Print: Ctrl + P
            SendKeys.SendWait("^{P}");
        }

        private void WordAction_Save()
        {
            // Save: Ctrl + S
            SendKeys.SendWait("^{S}");
        }

        private void WordAction_Strikethrough()
        {
            // Strikethrough: Ctrl + D, then Alt + K
            SendKeys.SendWait("^{D}");
            SendKeys.SendWait("%{K}");
            SendKeys.SendWait("{ENTER}");
        }

        private void WordAction_TextSizeMinus()
        {
            // Decrease text size: Ctrl + Shift + <
            SendKeys.SendWait("^{+}{<}");
        }

        private void WordAction_TextSizePlus()
        {
            // Increase text size: Ctrl + Shift + >
            SendKeys.SendWait("^{+}{>}");
        }

        private void WordAction_TranslateSelectedText()
        {
            // Translate selected text: Ctrl + Alt + T
            SendKeys.SendWait("^%{T}");
        }

        private void WordAction_ViewOnePage()
        {
            // View one page: Alt + W, 1
            SendKeys.SendWait("%w1");
        }

        private void WordAction_ViewPageWidth()
        {
            // View page width: Alt + W, P
            SendKeys.SendWait("%w{P}");
        }

        private void WordAction_ZoomIn()
        {
            // Zoom in: Alt + W, Q, then enter a larger percentage
            SendKeys.SendWait("%wq");
            SendKeys.SendWait("120{ENTER}");
        }

        private void WordAction_ZoomOut()
        {
            // Zoom out: Alt + W, Q, then enter a smaller percentage
            SendKeys.SendWait("%wq");
            SendKeys.SendWait("80{ENTER}");
        }

        #endregion Word Actions

        #region Excel Actions

        private void ExcelAction_AlignCenter()
        {
            // Align center: Alt + H, A, C
            SendKeys.SendWait("%h");
            SendKeys.SendWait("ac");
        }

        private void ExcelAction_AlignLeft()
        {
            // Align left: Alt + H, A, L
            SendKeys.SendWait("%h");
            SendKeys.SendWait("al");
        }

        private void ExcelAction_AlignRight()
        {
            // Align right: Alt + H, A, R
            SendKeys.SendWait("%h");
            SendKeys.SendWait("ar");
        }

        private void ExcelAction_DecreaseIndent()
        {
            // Decrease indent: Alt + H, 5
            SendKeys.SendWait("%h");
            SendKeys.SendWait("5");
        }

        private void ExcelAction_GotoBottomOfDataRegion()
        {
            // Go to bottom of data region: Ctrl + Down Arrow
            SendKeys.SendWait("^{DOWN}");
        }

        private void ExcelAction_GotoTopOfDataRegion()
        {
            // Go to top of data region: Ctrl + Up Arrow
            SendKeys.SendWait("^{UP}");
        }

        private void ExcelAction_IncreaseIndent()
        {
            // Increase indent: Alt + H, 6
            SendKeys.SendWait("%h");
            SendKeys.SendWait("6");
        }

        private void ExcelAction_InsertChart()
        {
            // Insert chart: Alt + N, R
            SendKeys.SendWait("%n");
            SendKeys.SendWait("r");
        }

        private void ExcelAction_InsertRowAbove()
        {
            // Insert row above: Ctrl + Shift + +
            SendKeys.SendWait("^{+}");
        }

        private void ExcelAction_NewComment()
        {
            // New comment: Shift + F2
            SendKeys.SendWait("+{F2}");
        }

        private void ExcelAction_NextComment()
        {
            // Next comment: Ctrl + Alt + PageDown
            SendKeys.SendWait("^%{PGDN}");
        }

        private void ExcelAction_PanHoldAndMoveMouse()
        {
            // Pan: Hold middle mouse button and move the mouse
            MouseClickAndHoldMiddleButton();
            // Implement mouse movement logic
            MouseReleaseMiddleButton();
        }

        private void ExcelAction_PasteFormatOnly()
        {
            // Paste format only: Ctrl + Alt + V, then T
            SendKeys.SendWait("^%{V}");
            SendKeys.SendWait("t");
            SendKeys.SendWait("{ENTER}");
        }

        private void ExcelAction_PasteFormulas()
        {
            // Paste formulas: Ctrl + Alt + V, then F
            SendKeys.SendWait("^%{V}");
            SendKeys.SendWait("f");
            SendKeys.SendWait("{ENTER}");
        }

        private void ExcelAction_PasteValueOnly()
        {
            // Paste values only: Ctrl + Alt + V, then V
            SendKeys.SendWait("^%{V}");
            SendKeys.SendWait("v");
            SendKeys.SendWait("{ENTER}");
        }

        private void ExcelAction_PreviousComment()
        {
            // Previous comment: Ctrl + Alt + PageUp
            SendKeys.SendWait("^%{PGUP}");
        }

        private void ExcelAction_PreviousSheet()
        {
            // Previous sheet: Ctrl + PageUp
            SendKeys.SendWait("^{PGUP}");
        }

        private void ExcelAction_Save()
        {
            // Save: Ctrl + S
            SendKeys.SendWait("^{S}");
        }

        private void ExcelAction_SortAtoZ()
        {
            // Sort A to Z: Alt + H, S, S
            SendKeys.SendWait("%h");
            SendKeys.SendWait("ss");
        }

        private void ExcelAction_ZoomIn()
        {
            // Zoom in: Ctrl + Mouse Wheel Up
            CtrlMouseWheelUp();
        }

        private void ExcelAction_ZoomOut()
        {
            // Zoom out: Ctrl + Mouse Wheel Down
            CtrlMouseWheelDown();
        }

        #endregion Excel Actions

        #region PowerPoint Actions

        private void PowerPointAction_ArrangeAlignCenter()
        {
            // Align center: Alt + H, G, A, C
            SendKeys.SendWait("%h");
            SendKeys.SendWait("gac");
        }

        private void PowerPointAction_ArrangeAlignLeft()
        {
            // Align left: Alt + H, G, A, L
            SendKeys.SendWait("%h");
            SendKeys.SendWait("gal");
        }

        private void PowerPointAction_ArrangeAlignRight()
        {
            // Align right: Alt + H, G, A, R
            SendKeys.SendWait("%h");
            SendKeys.SendWait("gar");
        }

        private void PowerPointAction_BringToFront()
        {
            // Bring to front: Ctrl + Shift + ]
            SendKeys.SendWait("^{+}]");
        }

        private void PowerPointAction_DecreaseListLevel()
        {
            // Decrease list level: Alt + Shift + Left Arrow
            SendKeys.SendWait("%+{LEFT}");
        }

        private void PowerPointAction_DuplicateSelectedSlides()
        {
            // Duplicate selected slides: Ctrl + D
            SendKeys.SendWait("^{D}");
        }

        private void PowerPointAction_IncreaseListLevel()
        {
            // Increase list level: Alt + Shift + Right Arrow
            SendKeys.SendWait("%+{RIGHT}");
        }

        private void PowerPointAction_NewComment()
        {
            // New comment: Ctrl + Alt + M
            SendKeys.SendWait("^%{M}");
        }

        private void PowerPointAction_NextComment()
        {
            // Next comment: Alt + Shift + N
            SendKeys.SendWait("%+{N}");
        }

        private void PowerPointAction_PanHoldAndMoveMouse()
        {
            // Pan: Hold space bar and drag the mouse
            SendKeys.SendWait(" ");
            // Implement mouse movement logic
            SendKeys.SendWait(" ");
        }

        private void PowerPointAction_PlayFromCurrentSlide()
        {
            // Play from current slide: Shift + F5
            SendKeys.SendWait("+{F5}");
        }

        private void PowerPointAction_PreviousComment()
        {
            // Previous comment: Alt + Shift + P
            SendKeys.SendWait("%+{P}");
        }

        private void PowerPointAction_PreviousSlide()
        {
            // Previous slide: PageUp
            SendKeys.SendWait("{PGUP}");
        }

        private void PowerPointAction_Save()
        {
            // Save: Ctrl + S
            SendKeys.SendWait("^{S}");
        }

        private void PowerPointAction_SendToBack()
        {
            // Send to back: Ctrl + Shift + [
            SendKeys.SendWait("^{+}[");
        }

        private void PowerPointAction_Strikethrough()
        {
            // Strikethrough: Ctrl + T, then Alt + K
            SendKeys.SendWait("^{T}");
            SendKeys.SendWait("%{K}");
            SendKeys.SendWait("{ENTER}");
        }

        private void PowerPointAction_TextSizeMinus()
        {
            // Decrease text size: Ctrl + Shift + <
            SendKeys.SendWait("^{+}{<}");
        }

        private void PowerPointAction_TextSizePlus()
        {
            // Increase text size: Ctrl + Shift + >
            SendKeys.SendWait("^{+}{>}");
        }

        private void PowerPointAction_ZoomIn()
        {
            // Zoom in: Alt + W, Q, then enter a larger percentage
            SendKeys.SendWait("%wq");
            SendKeys.SendWait("120{ENTER}");
        }

        private void PowerPointAction_ZoomOut()
        {
            // Zoom out: Alt + W, Q, then enter a smaller percentage
            SendKeys.SendWait("%wq");
            SendKeys.SendWait("80{ENTER}");
        }

        #endregion PowerPoint Actions

        #region Outlook Actions

        private void OutlookAction_AttachFile()
        {
            // Attach file: Alt + N, A, F
            SendKeys.SendWait("%n");
            SendKeys.SendWait("af");
        }

        private void OutlookAction_DecreaseIndent()
        {
            // Decrease indent: Ctrl + Shift + M
            SendKeys.SendWait("^{+}{M}");
        }

        private void OutlookAction_ForwardEmail()
        {
            // Forward email: Ctrl + F
            SendKeys.SendWait("^{F}");
        }

        private void OutlookAction_IncreaseIndent()
        {
            // Increase indent: Ctrl + M
            SendKeys.SendWait("^{M}");
        }

        private void OutlookAction_NewEmail()
        {
            // New email: Ctrl + Shift + M
            SendKeys.SendWait("^{+}{M}");
        }

        private void OutlookAction_NewMeeting()
        {
            // New meeting: Ctrl + Shift + Q
            SendKeys.SendWait("^{+}{Q}");
        }

        private void OutlookAction_Reply()
        {
            // Reply: Ctrl + R
            SendKeys.SendWait("^{R}");
        }

        private void OutlookAction_ReplyToAll()
        {
            // Reply to all: Ctrl + Shift + R
            SendKeys.SendWait("^{+}{R}");
        }

        #endregion Outlook Actions

        #region Helper Methods

        private void MouseClickMiddleButton()
        {
            // Simulate middle mouse button down
            Win32._mouse_event(Win32.MOUSEEVENTF_MIDDLEDOWN, 0, 0, 0, UIntPtr.Zero);

            // Simulate middle mouse button up
            Win32._mouse_event(Win32.MOUSEEVENTF_MIDDLEUP, 0, 0, 0, UIntPtr.Zero);
        }

        private void MouseClickAndHoldMiddleButton()
        {
            // Implement logic to click and hold the middle mouse button
            // Simulate middle mouse button down (click and hold)
            Win32._mouse_event(Win32.MOUSEEVENTF_MIDDLEDOWN, 0, 0, 0, UIntPtr.Zero);
        }

        private void MouseReleaseMiddleButton()
        {
            // Implement logic to release the middle mouse button
            // Simulate middle mouse button up (release)
            Win32._mouse_event(Win32.MOUSEEVENTF_MIDDLEUP, 0, 0, 0, UIntPtr.Zero);
        }

        private void CtrlMouseWheelUp()
        {
            // Implement logic for Ctrl + Mouse Wheel Up
            // Simulate pressing the Ctrl key
            Win32._keybd_event(Win32.VK_CONTROL, 0, Win32.KEYEVENTF_KEYDOWN, UIntPtr.Zero);

            // Simulate mouse wheel scroll up
            Win32._mouse_event(Win32.MOUSEEVENTF_WHEEL, 0, 0, Win32.WHEEL_DELTA, UIntPtr.Zero);

            // Simulate releasing the Ctrl key
            Win32._keybd_event(Win32.VK_CONTROL, 0, Win32.KEYEVENTF_KEYUP, UIntPtr.Zero);
        }

        private void CtrlMouseWheelDown()
        {
            // Implement logic for Ctrl + Mouse Wheel Down
            // Simulate pressing the Ctrl key
            Win32._keybd_event(Win32.VK_CONTROL, 0, Win32.KEYEVENTF_KEYDOWN, UIntPtr.Zero);

            // Simulate mouse wheel scroll down (negative delta)
            Win32._mouse_event(Win32.MOUSEEVENTF_WHEEL, 0, 0, unchecked((uint)-Win32.WHEEL_DELTA), UIntPtr.Zero);

            // Simulate releasing the Ctrl key
            Win32._keybd_event(Win32.VK_CONTROL, 0, Win32.KEYEVENTF_KEYUP, UIntPtr.Zero);
        }

        #endregion Helper Methods

        #region Productivity Actions

        //private void ProductivityAction_AssignKeystroke()
        //{
        //    // Example: Simulate Ctrl + Shift + S (Save As)
        //    keybd_event(VK_CONTROL, 0, KEYEVENTF_KEYDOWN, UIntPtr.Zero);
        //    keybd_event(VK_SHIFT, 0, KEYEVENTF_KEYDOWN, UIntPtr.Zero);
        //    SendKeys.SendWait("S");
        //    keybd_event(VK_SHIFT, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
        //    keybd_event(VK_CONTROL, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
        //}

        //private void ProductivityAction_DoOpenRun()
        //{
        //    // Simulate Win + R (Open Run dialog)
        //    keybd_event(VK_LWIN, 0, KEYEVENTF_KEYDOWN, UIntPtr.Zero);
        //    SendKeys.SendWait("r");
        //    keybd_event(VK_LWIN, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
        //}

        //private void ProductivityAction_DoPenPageDown()
        //{
        //    // Simulate Page Down key press
        //    SendKeys.SendWait("{PGDN}");
        //}

        //private void ProductivityAction_DoPenPageUp()
        //{
        //    // Simulate Page Up key press
        //    SendKeys.SendWait("{PGUP}");
        //}

        #endregion Productivity Actions

        #region Windows Actions

        private void WindowsAction_DefineBySystem()
        {
            // Placeholder for system-defined action
            // Implementation depends on system-specific behavior
        }

        //private void WindowsAction_DoOneNote()
        //{
        //    // Simulate Win + N (Open OneNote)
        //    keybd_event(VK_LWIN, 0, KEYEVENTF_KEYDOWN, UIntPtr.Zero);
        //    SendKeys.SendWait("n");
        //    keybd_event(VK_LWIN, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
        //}

        private void WindowsAction_DoPenMenu()
        {
            // Simulate opening the Pen menu (this could be specific to certain devices)
            // Implementation may vary depending on the system and application
        }

        private void WindowsAction_DoQuickNote()
        {
            // Simulate opening a Quick Note (e.g., Win + Alt + N in OneNote)
            Win32._keybd_event(Win32.VK_LWIN, 0, Win32.KEYEVENTF_KEYDOWN, UIntPtr.Zero);
            Win32._keybd_event(Win32.VK_ALT, 0, Win32.KEYEVENTF_KEYDOWN, UIntPtr.Zero);
            SendKeys.SendWait("n");
            Win32._keybd_event(Win32.VK_ALT, 0, Win32.KEYEVENTF_KEYUP, UIntPtr.Zero);
            Win32._keybd_event(Win32.VK_LWIN, 0, Win32.KEYEVENTF_KEYUP, UIntPtr.Zero);
        }

        //private void WindowsAction_DoScreenSnip()
        //{
        //    // Simulate Win + Shift + S (Start Screen Snip tool)
        //    keybd_event(VK_LWIN, 0, KEYEVENTF_KEYDOWN, UIntPtr.Zero);
        //    keybd_event(VK_SHIFT, 0, KEYEVENTF_KEYDOWN, UIntPtr.Zero);
        //    SendKeys.SendWait("s");
        //    keybd_event(VK_SHIFT, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
        //    keybd_event(VK_LWIN, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
        //}

        private void WindowsAction_DoStickyNotes()
        {
            // Simulate opening Sticky Notes (e.g., Win + S, type "Sticky Notes" and Enter)
            Win32._keybd_event(Win32.VK_LWIN, 0, Win32.KEYEVENTF_KEYDOWN, UIntPtr.Zero);
            SendKeys.SendWait("s");
            Win32._keybd_event(Win32.VK_LWIN, 0, Win32.KEYEVENTF_KEYUP, UIntPtr.Zero);
            SendKeys.SendWait("Sticky Notes{ENTER}");
        }

        //private void WindowsAction_DoWindowsSearch()
        //{
        //    // Simulate Win + S (Open Windows Search)
        //    keybd_event(VK_LWIN, 0, KEYEVENTF_KEYDOWN, UIntPtr.Zero);
        //    SendKeys.SendWait("s");
        //    keybd_event(VK_LWIN, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
        //}

        #endregion Windows Actions

        #region Multimedia Actions

        //private void MultimediaAction_MediaPlayPause()
        //{
        //    // Simulate Media Play/Pause key press
        //    SendKeys.SendWait("{MEDIA_PLAY_PAUSE}");
        //}

        //private void MultimediaAction_MediaPreviousTrack()
        //{
        //    // Simulate Media Previous Track key press
        //    SendKeys.SendWait("{MEDIA_PREV_TRACK}");
        //}

        //private void MultimediaAction_NextTrack()
        //{
        //    // Simulate Media Next Track key press
        //    SendKeys.SendWait("{MEDIA_NEXT_TRACK}");
        //}

        //private void MultimediaAction_VolumeDown()
        //{
        //    // Simulate Volume Down key press
        //    SendKeys.SendWait("{VOLUME_DOWN}");
        //}

        //private void MultimediaAction_VolumeMute()
        //{
        //    // Simulate Volume Mute key press
        //    SendKeys.SendWait("{VOLUME_MUTE}");
        //}

        //private void MultimediaAction_VolumeUp()
        //{
        //    // Simulate Volume Up key press
        //    SendKeys.SendWait("{VOLUME_UP}");
        //}

        #endregion Multimedia Actions

        private enum log_type
        {
            info = 0,
            error
        }

        #region Constructor

        public ActionsMangerPlugin(IAgent agent) : base(agent, PluginLogId)
        {
            _agent = agent;
            writelog("ActionsMangerPlugin constructor ...");
        }

        #endregion Constructor

        #region IDisposableObservable

        /// <summary>
        /// unhook
        /// </summary>
        public bool IsDisposed { get; private set; }

        protected override void Dispose(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing)
                {
                    _agent.PluginManager.PluginsStarted -= PluginManagerOnPluginsStarted;
                    _agent = null;
                }

                IsDisposed = true;
            }
            base.Dispose(disposing);
        }

        #endregion IDisposableObservable

        #region Private Methods

        /// <summary>
        /// //
        /// </summary>
        /// <param name="text"></param>
        /// <param name="log_type">0 means info, others means error</param>
        private void writelog(string text, log_type log_type = log_type.info)
        {
            if (string.IsNullOrEmpty(text))
                text = "";

            text = "[ActionsMangerPlugin] " + text;
            Console.WriteLine(text);
            if (log_type == log_type.info)
                Log.Info(text);
            else
                Log.Error(text);
        }

        #endregion Private Methods

        #region Overriding methods

        protected override void OnPluginStarting()
        {
            _agent.PluginManager.PluginsStarted += PluginManagerOnPluginsStarted;
            InitializePeripheralsPlugin();
            PluginCondition = new PluginStartedCondition();
            writelog("Actions Manger plugin started");
        }

        #endregion Overriding methods

        #region Event Handler

        private void PluginManagerOnPluginsStarted(object sender, PluginsStartedEventArgs e)
        {
            if (e == null)
                return;
            if (e.ChangedPlugins == null)
                return;
            if (e.ChangedPlugins.Any() == false)
                return;
            if (e.ChangedPlugins.OfType<IDPeMPlugin>().Any())
                InitializePeripheralsPlugin();
        }

        #endregion Event Handler
    }
}