using System.Collections.Immutable;

namespace DDPM.UI.Common
{
    public class ActionItem
    {
        public ActionCategory? Category;
        public string Caption = "";
        public bool IsForKnM;
        public bool IsForPen;
        public bool IsSuggestedForKeyboard;
        public bool IsSuggestedForMouse;
        public bool IsSuggestedForPen;
        internal int SuggestedOrderK;
        internal int SuggestedOrderM;
        internal int SuggestedOrderP;

        public ActionItem()
        { }

        public ActionItem(ActionCategory category, string caption, bool isForKnM, bool isForPen,
                          bool isSuggestedForKeyboard = false, bool isSuggestedForMouse = false, bool isSuggestedForPen = false,
                          int suggestedOrderK = 0, int suggestedOrderM = 0, int suggestedOrderP = 0)
        {
            Category = category;
            Caption = caption;
            IsForKnM = isForKnM;
            IsForPen = isForPen;
            IsSuggestedForKeyboard = isSuggestedForKeyboard;
            IsSuggestedForMouse = isSuggestedForMouse;
            IsSuggestedForPen = isSuggestedForPen;
            SuggestedOrderK = suggestedOrderK;
            SuggestedOrderM = suggestedOrderM;
            SuggestedOrderP = suggestedOrderP;
        }
    }

    public static class Actions
    {
        //Robert_Lin, 2024-6-26, fix SAST issues: [Bug] Use an immutable collection or reduce the accessibility of the non-private readonly field.
        //OLD code:
        //  public static readonly Dictionary<int, ActionItem> AllActions = new() {
        //Solusion_1: But it cannot be initialized because compiler translate them into a sequence of calls to the Add() method.
        //  public static ImmutableDictionary<int, ActionItem> AllActions = new() {
        //Solution_2: change it to private
        //Solution_3: remove the unused readonly
        public static readonly Dictionary<int, ActionItem> AllActions = new() {
      { 0, new ActionItem(ActionCategory.None, Strings.None, true, true, true, true, true, 4, 4) },
      { 1, new ActionItem(ActionCategory.WindowsAction, Strings.Copilot, true, false) },
      { 2, new ActionItem(ActionCategory.WindowsAction, Strings.Devices, true, false) },
      { 3, new ActionItem(ActionCategory.WindowsAction, Strings.Lock, true, false) },
      { 4, new ActionItem(ActionCategory.WindowsAction, Strings.NotificationCenter, true, false) },
      { 5, new ActionItem(ActionCategory.WindowsAction, Strings.ScreenSnip, true, false) },
      { 6, new ActionItem(ActionCategory.WindowsAction, Strings.Search, true, false) },
      { 7, new ActionItem(ActionCategory.WindowsAction, Strings.Settings, true, false) },
      { 8, new ActionItem(ActionCategory.WindowsAction, Strings.ShowHideDesktop, true, false) },
      { 9, new ActionItem(ActionCategory.WindowsAction, Strings.Shutdown, true, false) },
      { 10, new ActionItem(ActionCategory.WindowsAction, Strings.SignOut, true, false) },
      { 11, new ActionItem(ActionCategory.WindowsAction, Strings.Sleep, true, false) },
      { 12, new ActionItem(ActionCategory.WindowsAction, Strings.SwitchApplications, true, false) },
      { 13, new ActionItem(ActionCategory.WindowsAction, Strings.TaskView, true, false) },

      { 14, new ActionItem(ActionCategory.ProductivityAction, Strings.AssignKeystroke, true, true, true, true, true, 1, 1) },
      { 15, new ActionItem(ActionCategory.ProductivityAction, Strings.Back, true, true) },
      { 16, new ActionItem(ActionCategory.ProductivityAction, Strings.Calculator, true, false) },
      { 17, new ActionItem(ActionCategory.ProductivityAction, Strings.CloseWindow, true, false) },
      { 18, new ActionItem(ActionCategory.ProductivityAction, Strings.Copy, true, true) },
      { 19, new ActionItem(ActionCategory.ProductivityAction, Strings.Cut, true, false) },
      { 20, new ActionItem(ActionCategory.ProductivityAction, Strings.Documents, true, false) },
      { 21, new ActionItem(ActionCategory.ProductivityAction, Strings.Forward, true, true) },
      { 22, new ActionItem(ActionCategory.ProductivityAction, Strings.MaximizeWindow, true, false) },
      { 23, new ActionItem(ActionCategory.ProductivityAction, Strings.MinimizeWindow, true, false) },
      { 24, new ActionItem(ActionCategory.ProductivityAction, Strings.MyHome, true, false) },
      { 25, new ActionItem(ActionCategory.ProductivityAction, Strings.OpenFile, true, false, true, false, false, 2) },
      { 26, new ActionItem(ActionCategory.ProductivityAction, Strings.OpenFolder, true, false, true, false, false, 3) },
      { 27, new ActionItem(ActionCategory.ProductivityAction, Strings.OpenNewBrowserTab, true, false) },
      { 28, new ActionItem(ActionCategory.ProductivityAction, Strings.OpenWebPage, true, false) },
      { 29, new ActionItem(ActionCategory.ProductivityAction, Strings.Paste, true, true) },
      { 30, new ActionItem(ActionCategory.ProductivityAction, Strings.ZoomIn, true, false) },
      { 31, new ActionItem(ActionCategory.ProductivityAction, Strings.ZoomOut, true, false) },
      { 32, new ActionItem(ActionCategory.ProductivityAction, Strings.ZoomReset, true, false) },

      { 33, new ActionItem(ActionCategory.MultimediaAction, Strings.MediaNextTrack, true, false) },
      { 34, new ActionItem(ActionCategory.MultimediaAction, Strings.MediaPlayPause, true, true, false, true, false, 0, 2) },
      { 35, new ActionItem(ActionCategory.MultimediaAction, Strings.MediaPreviousTrack, true, true) },
      { 36, new ActionItem(ActionCategory.MultimediaAction, Strings.Music, true, false) },
      { 37, new ActionItem(ActionCategory.MultimediaAction, Strings.Pictures, true, false) },
      { 38, new ActionItem(ActionCategory.MultimediaAction, Strings.VolumeDown, true, true) },
      { 39, new ActionItem(ActionCategory.MultimediaAction, Strings.VolumeMute, true, true, false, true, false, 0, 3) },
      { 40, new ActionItem(ActionCategory.MultimediaAction, Strings.VolumeUp, true, true) },

      { 41, new ActionItem(ActionCategory.None, Strings.PrtSc, true, false) },
      { 42, new ActionItem(ActionCategory.None, Strings.ScrollLock, true, false) },
      { 43, new ActionItem(ActionCategory.None, Strings.PauseBreak, true, false) },
      { 44, new ActionItem(ActionCategory.None, Strings.Home, true, false) },
      { 45, new ActionItem(ActionCategory.None, Strings.End, true, false) },
      { 46, new ActionItem(ActionCategory.None, Strings.PageUp, true, false) },
      { 47, new ActionItem(ActionCategory.None, Strings.PageDown, true, false) },

      { 48, new ActionItem(ActionCategory.ProductivityAction, Strings._4thClick, false, true) },
      { 49, new ActionItem(ActionCategory.ProductivityAction, Strings._5thClick, false, true) },
      { 50, new ActionItem(ActionCategory.ProductivityAction, Strings.Erase, false, true) },
      { 51, new ActionItem(ActionCategory.ProductivityAction, Strings.LeftClick, false, true) },
      { 52, new ActionItem(ActionCategory.ProductivityAction, Strings.MiddleClick, false, true) },
      { 53, new ActionItem(ActionCategory.ProductivityAction, Strings.OpenRun, false, true) },
      { 54, new ActionItem(ActionCategory.ProductivityAction, Strings.PenPageDown, false, true) },
      { 55, new ActionItem(ActionCategory.ProductivityAction, Strings.PenPageUp, false, true) },
      { 56, new ActionItem(ActionCategory.ProductivityAction, Strings.RadialMenu, false, true) },
      { 57, new ActionItem(ActionCategory.ProductivityAction, Strings.Redo, false, true) },
      { 58, new ActionItem(ActionCategory.ProductivityAction, Strings.RightClick, false, true) },
      { 59, new ActionItem(ActionCategory.ProductivityAction, Strings.Undo, false, true) },

      { 60, new ActionItem(ActionCategory.WindowsAction, Strings.BarrelButton, false, true) },
      { 61, new ActionItem(ActionCategory.WindowsAction, Strings.EMail, false, true) },
      { 62, new ActionItem(ActionCategory.WindowsAction, Strings.OneNote, false, true) },
      { 63, new ActionItem(ActionCategory.WindowsAction, Strings.OpenNewBrowserTab, false, true) },
      { 64, new ActionItem(ActionCategory.WindowsAction, Strings.SwitchApplication, false, true) },
      { 65, new ActionItem(ActionCategory.WindowsAction, Strings.Widgets, false, true) },
      { 66, new ActionItem(ActionCategory.WindowsAction, Strings.WindowsSearch, false, true) },

      { 67, new ActionItem(ActionCategory.MultimediaAction, Strings.NextTrack, false, true) },
    };

        public static readonly Dictionary<int, ActionItem> OfficeActions = new() {
      { 101, new ActionItem(ActionCategory.WordAction, Strings.Autoscroll, false, false) },
      { 102, new ActionItem(ActionCategory.WordAction, Strings.Find, false, false) },
      { 103, new ActionItem(ActionCategory.WordAction, Strings.IncreaseIndent, false, false) },
      { 104, new ActionItem(ActionCategory.WordAction, Strings.NewComment, false, false) },
      { 105, new ActionItem(ActionCategory.WordAction, Strings.NextChange, false, false) },
      { 106, new ActionItem(ActionCategory.WordAction, Strings.NextComment, false, false) },
      { 107, new ActionItem(ActionCategory.WordAction, Strings.PasteAndKeepSourceFormatting, false, false) },
      { 108, new ActionItem(ActionCategory.WordAction, Strings.PasteAndKeepTextOnly, false, false) },
      { 109, new ActionItem(ActionCategory.WordAction, Strings.PasteAndMatchFormatting, false, false) },
      { 110, new ActionItem(ActionCategory.WordAction, Strings.PasteAndMergeFormatting, false, false) },
      { 111, new ActionItem(ActionCategory.WordAction, Strings.PreviousChange, false, false) },
      { 112, new ActionItem(ActionCategory.WordAction, Strings.PreviousComment, false, false) },
      { 113, new ActionItem(ActionCategory.WordAction, Strings.Print, false, false) },
      { 114, new ActionItem(ActionCategory.WordAction, Strings.Save, false, false) },
      { 115, new ActionItem(ActionCategory.WordAction, Strings.Strikethrough, false, false) },
      { 116, new ActionItem(ActionCategory.WordAction, Strings.TextSizeMinus, false, false) },
      { 117, new ActionItem(ActionCategory.WordAction, Strings.TextSizePlus, false, false) },
      { 118, new ActionItem(ActionCategory.WordAction, Strings.TranslateSelectedText, false, false) },
      { 119, new ActionItem(ActionCategory.WordAction, Strings.ViewOnePage, false, false) },
      { 120, new ActionItem(ActionCategory.WordAction, Strings.ViewPageWidth, false, false) },
      { 121, new ActionItem(ActionCategory.WordAction, Strings.ZoomIn, false, false) },
      { 122, new ActionItem(ActionCategory.WordAction, Strings.ZoomOut, false, false) },
      { 201, new ActionItem(ActionCategory.ExcelAction, Strings.AlignCenter, false, false) },
      { 202, new ActionItem(ActionCategory.ExcelAction, Strings.AlignLeft, false, false) },
      { 203, new ActionItem(ActionCategory.ExcelAction, Strings.AlignRight, false, false) },
      { 204, new ActionItem(ActionCategory.ExcelAction, Strings.DecreaseIndent, false, false) },
      { 205, new ActionItem(ActionCategory.ExcelAction, Strings.GotoBottomOfDataRegion, false, false) },
      { 206, new ActionItem(ActionCategory.ExcelAction, Strings.GotoTopOfDataRegion, false, false) },
      { 207, new ActionItem(ActionCategory.ExcelAction, Strings.IncreaseIndent, false, false) },
      { 208, new ActionItem(ActionCategory.ExcelAction, Strings.InsertChart, false, false) },
      { 209, new ActionItem(ActionCategory.ExcelAction, Strings.InsertRowAbove, false, false) },
      { 210, new ActionItem(ActionCategory.ExcelAction, Strings.NewComment, false, false) },
      { 211, new ActionItem(ActionCategory.ExcelAction, Strings.NextComment, false, false) },
      { 212, new ActionItem(ActionCategory.ExcelAction, Strings.PanHoldAndMoveMouse, false, false) },
      { 213, new ActionItem(ActionCategory.ExcelAction, Strings.PasteFormatOnly, false, false) },
      { 214, new ActionItem(ActionCategory.ExcelAction, Strings.PasteFormulas, false, false) },
      { 215, new ActionItem(ActionCategory.ExcelAction, Strings.PasteValueOnly, false, false) },
      { 216, new ActionItem(ActionCategory.ExcelAction, Strings.PreviousComment, false, false) },
      { 217, new ActionItem(ActionCategory.ExcelAction, Strings.PreviousSheet, false, false) },
      { 218, new ActionItem(ActionCategory.ExcelAction, Strings.Save, false, false) },
      { 219, new ActionItem(ActionCategory.ExcelAction, Strings.SortAtoZ, false, false) },
      { 220, new ActionItem(ActionCategory.ExcelAction, Strings.ZoomIn, false, false) },
      { 221, new ActionItem(ActionCategory.ExcelAction, Strings.ZoomOut, false, false) },
      { 301, new ActionItem(ActionCategory.PowerPointAction, Strings.ArrangeAlignCenter, false, false) },
      { 302, new ActionItem(ActionCategory.PowerPointAction, Strings.ArrangeAlignLeft, false, false) },
      { 303, new ActionItem(ActionCategory.PowerPointAction, Strings.ArrangeAlignRight, false, false) },
      { 304, new ActionItem(ActionCategory.PowerPointAction, Strings.BringToFront, false, false) },
      { 305, new ActionItem(ActionCategory.PowerPointAction, Strings.DecreaseListLevel, false, false) },
      { 306, new ActionItem(ActionCategory.PowerPointAction, Strings.DuplicateSelectedSlides, false, false) },
      { 307, new ActionItem(ActionCategory.PowerPointAction, Strings.IncreaseListLevel, false, false) },
      { 308, new ActionItem(ActionCategory.PowerPointAction, Strings.NewComment, false, false) },
      { 309, new ActionItem(ActionCategory.PowerPointAction, Strings.NextComment, false, false) },
      { 310, new ActionItem(ActionCategory.PowerPointAction, Strings.PanHoldAndMoveMouse, false, false) },
      { 311, new ActionItem(ActionCategory.PowerPointAction, Strings.PlayFromCurrentSlide, false, false) },
      { 312, new ActionItem(ActionCategory.PowerPointAction, Strings.PreviousComment, false, false) },
      { 313, new ActionItem(ActionCategory.PowerPointAction, Strings.PreviousSlide, false, false) },
      { 314, new ActionItem(ActionCategory.PowerPointAction, Strings.Save, false, false) },
      { 315, new ActionItem(ActionCategory.PowerPointAction, Strings.SendToBack, false, false) },
      { 316, new ActionItem(ActionCategory.PowerPointAction, Strings.Strikethrough, false, false) },
      { 317, new ActionItem(ActionCategory.PowerPointAction, Strings.TextSizeMinus, false, false) },
      { 318, new ActionItem(ActionCategory.PowerPointAction, Strings.TextSizePlus, false, false) },
      { 319, new ActionItem(ActionCategory.PowerPointAction, Strings.ZoomIn, false, false) },
      { 320, new ActionItem(ActionCategory.PowerPointAction, Strings.ZoomOut, false, false) },
      { 401, new ActionItem(ActionCategory.OutlookAction, Strings.AttachFile, false, false) },
      { 402, new ActionItem(ActionCategory.OutlookAction, Strings.DecreaseIndent, false, false) },
      { 403, new ActionItem(ActionCategory.OutlookAction, Strings.ForwardEmail, false, false) },
      { 404, new ActionItem(ActionCategory.OutlookAction, Strings.IncreaseIndent, false, false) },
      { 405, new ActionItem(ActionCategory.OutlookAction, Strings.NewEmail, false, false) },
      { 406, new ActionItem(ActionCategory.OutlookAction, Strings.NewMeeting, false, false) },
      { 407, new ActionItem(ActionCategory.OutlookAction, Strings.Reply, false, false) },
      { 408, new ActionItem(ActionCategory.OutlookAction, Strings.ReplyToAll, false, false) },
    };

        public static readonly Dictionary<int, ActionItem> PenTopButtonActions = new() {
      { 0, new ActionItem(ActionCategory.None, Strings.None, false, true) },
      { 14, new ActionItem(ActionCategory.ProductivityAction, Strings.AssignKeystroke, false, true) },
      { 53, new ActionItem(ActionCategory.ProductivityAction, Strings.OpenRun, false, true) },
      { 54, new ActionItem(ActionCategory.ProductivityAction, Strings.PenPageDown, false, true) },
      { 55, new ActionItem(ActionCategory.ProductivityAction, Strings.PenPageUp, false, true) },
      { 91, new ActionItem(ActionCategory.WindowsAction, Strings.DefineBySystem, false, true) },
      { 62, new ActionItem(ActionCategory.WindowsAction, Strings.OneNote, false, true) },
      { 92, new ActionItem(ActionCategory.WindowsAction, Strings.PenMenu, false, true) },
      { 93, new ActionItem(ActionCategory.WindowsAction, Strings.QuickNote, false, true) },
      { 94, new ActionItem(ActionCategory.WindowsAction, Strings.ScreenSnipping, false, true) },
      { 95, new ActionItem(ActionCategory.WindowsAction, Strings.StickyNotes, false, true) },
      { 66, new ActionItem(ActionCategory.WindowsAction, Strings.WindowsSearch, false, true) },
      { 34, new ActionItem(ActionCategory.MultimediaAction, Strings.MediaPlayPause, false, true) },
      { 35, new ActionItem(ActionCategory.MultimediaAction, Strings.MediaPreviousTrack, false, true) },
      { 67, new ActionItem(ActionCategory.MultimediaAction, Strings.NextTrack, false, true) },
      { 38, new ActionItem(ActionCategory.MultimediaAction, Strings.VolumeDown, false, true) },
      { 39, new ActionItem(ActionCategory.MultimediaAction, Strings.VolumeMute, false, true) },
      { 40, new ActionItem(ActionCategory.MultimediaAction, Strings.VolumeUp, false, true) },
    };

        public static readonly Dictionary<int, string> OpenRunActions = new() {
      { 1, Strings.Browse },
      { 2, Strings.Calculator },
      { 3, Strings.Calendar },
      { 4, Strings.Camera },
      { 5, Strings.Clock },
      { 6, Strings.Cortana },
      { 7, Strings.DellCommandUpdate },
      { 8, Strings.DellDigitalDelivery },
      { 9, Strings.DellOptimizer },
      { 10, Strings.Family },
      { 11, Strings.FeedbackHub },
      { 12, Strings.GameBar },
      { 13, Strings.GetHelp },
      { 14, Strings.GetStarted },
      { 15, Strings.IntelManagementAndSecurityStatus },
      { 16, Strings.IntelGraphicsCommandCenter },
      { 17, Strings.IntelOptaneMemoryAndStorageManagement },
      { 18, Strings.Mail },
      { 19, Strings.Maps },
      { 20, Strings.MediaPlayer },
      { 21, Strings.Microsoft365Office },
      { 22, Strings.MicrosoftClipchamp },
      { 23, Strings.MicrosoftDefender },
      { 24, Strings.MicrosoftStore },
      { 25, Strings.MicrosoftTeams },
      { 26, Strings.MicrosoftTeamsWorkSchool },
      { 27, Strings.MicrosoftToDo },
      { 28, Strings.MoviesTV },
      { 29, Strings.News },
      { 30, Strings.Notepad },
      { 31, Strings.Paint },
      { 32, Strings.PhoneLink },
      { 33, Strings.Photos },
      { 34, Strings.PowerAutomate },
      { 35, Strings.Settings },
      { 36, Strings.SnippingTool },
      { 37, Strings.SolitaireCasualGames },
      { 38, Strings.SoundRecorder },
      { 39, Strings.Spotify },
      { 40, Strings.StickyNotes },
      { 41, Strings.SupportAssist },
      { 42, Strings.Terminal },
      { 43, Strings.Tips },
      { 44, Strings.Weather },
      { 45, Strings.WindowsBackup },
      { 46, Strings.WindowsSecurity },
      { 47, Strings.Xbox }
    };

        public static readonly Dictionary<int, ActionItem> RadialMenuActions = new() {
      { 1, new ActionItem(ActionCategory.None, Strings.Disabled, false, true) },
      { 2, new ActionItem(ActionCategory.None, Strings.AssignKeystroke, false, true) },
      { 3, new ActionItem(ActionCategory.None, Strings.OpenRun2, false, true) },
      { 4, new ActionItem(ActionCategory.None, Strings.GoBack, false, true) },
      { 5, new ActionItem(ActionCategory.None, Strings.GoForward, false, true) },
      { 6, new ActionItem(ActionCategory.None, Strings.SwitchApplication, false, true) },
      { 7, new ActionItem(ActionCategory.None, Strings.Copy, false, true) },
      { 8, new ActionItem(ActionCategory.None, Strings.Paste, false, true) },
      { 9, new ActionItem(ActionCategory.None, Strings.Undo, false, true) },
      { 10, new ActionItem(ActionCategory.None, Strings.Redo, false, true) },
      { 11, new ActionItem(ActionCategory.None, Strings.PageUp, false, true) },
      { 12, new ActionItem(ActionCategory.None, Strings.PageDown, false, true) },
      { 13, new ActionItem(ActionCategory.None, Strings.OneNote, false, true) },
      { 14, new ActionItem(ActionCategory.None, Strings.WebBrowser, false, true) },
      { 15, new ActionItem(ActionCategory.None, Strings.EMail, false, true) },
      { 16, new ActionItem(ActionCategory.None, Strings.PlayPause, false, true) },
      { 17, new ActionItem(ActionCategory.None, Strings.NextTrack, false, true) },
      { 18, new ActionItem(ActionCategory.None, Strings.PreviousTrack, false, true) },
      { 19, new ActionItem(ActionCategory.None, Strings.VolumeUp, false, true) },
      { 20, new ActionItem(ActionCategory.None, Strings.VolumeDown, false, true) },
      { 21, new ActionItem(ActionCategory.None, Strings.Mute, false, true) },
      { 22, new ActionItem(ActionCategory.None, Strings.WindowsSearch, false, true) }
    };

        public static List<int> OpenRunActionsList => OpenRunActions.Select(x => x.Key).ToList();
        public static List<int> RadialMenuActionsList => RadialMenuActions.Select(x => x.Key).ToList();

        public static List<int> AllActionsKnM
        {
            get
            {
                return AllActions.Where(x => x.Value.Category == ActionCategory.None || x.Value.IsForKnM).OrderBy(x => x.Value.Caption).Select(x => x.Key).ToList();
            }
        }

        public static List<int> AllActionsPenBarrelButton
        {
            get
            {
                return AllActions.Where(x => x.Value.Category == ActionCategory.None || x.Value.IsForPen).OrderBy(x => x.Value.Caption).Select(x => x.Key).ToList();
            }
        }

        public static List<int> AllActionsPenTopButton
        {
            get
            {
                return PenTopButtonActions.OrderBy(x => x.Value.Caption).Select(x => x.Key).ToList();
            }
        }

        public static List<int> WindowsActionsKnM
        {
            get
            {
                return AllActions.Where(x => x.Value.Category == ActionCategory.WindowsAction && x.Value.IsForKnM).OrderBy(x => x.Value.Caption).Select(x => x.Key).ToList();
            }
        }

        public static List<int> WindowsActionsPenBarrelButton
        {
            get
            {
                return AllActions.Where(x => x.Value.Category == ActionCategory.WindowsAction && x.Value.IsForPen).OrderBy(x => x.Value.Caption).Select(x => x.Key).ToList();
            }
        }

        public static List<int> WindowsActionsPenTopButton
        {
            get
            {
                return PenTopButtonActions.Where(x => x.Value.Category == ActionCategory.WindowsAction).OrderBy(x => x.Value.Caption).Select(x => x.Key).ToList();
            }
        }

        public static List<int> ProductivityActionsKnM
        {
            get
            {
                return AllActions.Where(x => x.Value.Category == ActionCategory.ProductivityAction && x.Value.IsForKnM).OrderBy(x => x.Value.Caption).Select(x => x.Key).ToList();
            }
        }

        public static List<int> ProductivityActionsPenBarrelButton
        {
            get
            {
                return AllActions.Where(x => x.Value.Category == ActionCategory.ProductivityAction && x.Value.IsForPen).OrderBy(x => x.Value.Caption).Select(x => x.Key).ToList();
            }
        }

        public static List<int> ProductivityActionsPenTopButton
        {
            get
            {
                return PenTopButtonActions.Where(x => x.Value.Category == ActionCategory.ProductivityAction).OrderBy(x => x.Value.Caption).Select(x => x.Key).ToList();
            }
        }

        public static List<int> MultimediaActionsKnM
        {
            get
            {
                return AllActions.Where(x => x.Value.Category == ActionCategory.MultimediaAction && x.Value.IsForKnM).OrderBy(x => x.Value.Caption).Select(x => x.Key).ToList();
            }
        }

        public static List<int> MultimediaActionsPenBarrelButton
        {
            get
            {
                return AllActions.Where(x => x.Value.Category == ActionCategory.MultimediaAction && x.Value.IsForPen).OrderBy(x => x.Value.Caption).Select(x => x.Key).ToList();
            }
        }

        public static List<int> MultimediaActionsPenTopButton
        {
            get
            {
                return PenTopButtonActions.Where(x => x.Value.Category == ActionCategory.MultimediaAction).OrderBy(x => x.Value.Caption).Select(x => x.Key).ToList();
            }
        }

        public static List<int> WordActions
        {
            get
            {
                return OfficeActions.Where(x => x.Value.Category == ActionCategory.WordAction).OrderBy(x => x.Value.Caption).Select(x => x.Key).ToList();
            }
        }

        public static List<int> ExcelActions
        {
            get
            {
                return OfficeActions.Where(x => x.Value.Category == ActionCategory.ExcelAction).OrderBy(x => x.Value.Caption).Select(x => x.Key).ToList();
            }
        }

        public static List<int> PowerPointActions
        {
            get
            {
                return OfficeActions.Where(x => x.Value.Category == ActionCategory.PowerPointAction).OrderBy(x => x.Value.Caption).Select(x => x.Key).ToList();
            }
        }

        public static List<int> OutlookActions
        {
            get
            {
                return OfficeActions.Where(x => x.Value.Category == ActionCategory.OutlookAction).OrderBy(x => x.Value.Caption).Select(x => x.Key).ToList();
            }
        }

        public static List<int> SuggestedActionsK
        {
            get
            {
                return AllActions.Where(x => x.Value.IsSuggestedForKeyboard).OrderBy(x => x.Value.SuggestedOrderK).Select(x => x.Key).ToList();
            }
        }

        public static List<int> SuggestedActionsM
        {
            get
            {
                return AllActions.Where(x => x.Value.IsSuggestedForMouse).OrderBy(x => x.Value.SuggestedOrderM).Select(x => x.Key).ToList();
            }
        }

        public static List<int> SuggestedActionsPenTopButton
        {
            get
            {
                return new() { 14, 0, 53, 92 };
            }
        }

        public static List<int> SuggestedActionsPenBarrelButton
        {
            get
            {
                return new() { 14, 0, 34, 56 };
            }
        }

        //Robert_Lin, 2024-6-26, fix SAST issue: [Bug] Use an immutable collection or reduce the accessibiity of the non-private readonly field.
        //The same issue with AllActions, use solution_1, use a ImmutableList instead
        //OLD Code:
        //public readonly static List<int> AdvenceActions = [14, 25, 26, 28];
        //NEW Code:
        public static readonly ImmutableList<int> AdvancedActions = ImmutableList.Create(new int[] { 14, 25, 26, 28 });

        public static readonly ImmutableList<int> AdvancedActionsPen = ImmutableList.Create(new int[] { 14, 53, 56 });
    }
}