using System.Collections.Immutable;

namespace DDPM.UI.Common
{
    public class ActionItem
    {
        public ActionCategory? Category;
        public string Caption = "";
        public bool IsForTopButton;
        public bool IsForBarrelButton;
        public bool IsSuggestedForKeyboard;
        public bool IsSuggestedForMouse;
        //public bool IsSuggestedForPen;
        internal int SuggestedOrderK;
        internal int SuggestedOrderM;

        public ActionItem()
        { }

        public ActionItem(ActionCategory category, string caption, bool isSuggestedForKeyboard = false, bool isSuggestedForMouse = false, int suggestedOrderK = 0, int suggestedOrderM = 0)
        {
            Category = category;
            Caption = caption;
            IsSuggestedForKeyboard = isSuggestedForKeyboard;
            IsSuggestedForMouse = isSuggestedForMouse;
            SuggestedOrderK = suggestedOrderK;
            SuggestedOrderM = suggestedOrderM;
        }
        public ActionItem(ActionCategory category, string caption, bool isForTopButton, bool isForBarrelButton)
        {
            Category = category;
            Caption = caption;
            IsForTopButton = isForTopButton;
            IsForBarrelButton = isForBarrelButton;
        }
    }

    public static class Actions
    {
        public static readonly Dictionary<int, ActionItem> KnMActions = new() {
          { 0, new ActionItem(ActionCategory.None, Strings.None, true, true, 4, 4) },
          { 1, new ActionItem(ActionCategory.WindowsAction, Strings.Copilot) },
          { 2, new ActionItem(ActionCategory.WindowsAction, Strings.Devices) },
          { 3, new ActionItem(ActionCategory.WindowsAction, Strings.Lock) },
          { 4, new ActionItem(ActionCategory.WindowsAction, Strings.NotificationCenter) },
          { 5, new ActionItem(ActionCategory.WindowsAction, Strings.ScreenSnip) },
          { 6, new ActionItem(ActionCategory.WindowsAction, Strings.Search) },
          { 7, new ActionItem(ActionCategory.WindowsAction, Strings.Settings) },
          { 8, new ActionItem(ActionCategory.WindowsAction, Strings.ShowHideDesktop) },
          { 9, new ActionItem(ActionCategory.WindowsAction, Strings.Shutdown) },
          { 10, new ActionItem(ActionCategory.WindowsAction, Strings.SignOut) },
          { 11, new ActionItem(ActionCategory.WindowsAction, Strings.Sleep) },
          { 12, new ActionItem(ActionCategory.WindowsAction, Strings.SwitchApplications) },
          { 13, new ActionItem(ActionCategory.WindowsAction, Strings.TaskView) },

          { 14, new ActionItem(ActionCategory.ProductivityAction, Strings.AssignKeystroke, true, true, 1, 1) },
          { 15, new ActionItem(ActionCategory.ProductivityAction, Strings.Back) },
          { 16, new ActionItem(ActionCategory.ProductivityAction, Strings.Calculator) },
          { 17, new ActionItem(ActionCategory.ProductivityAction, Strings.CloseWindow) },
          { 18, new ActionItem(ActionCategory.ProductivityAction, Strings.Copy) },
          { 19, new ActionItem(ActionCategory.ProductivityAction, Strings.Cut) },
          { 20, new ActionItem(ActionCategory.ProductivityAction, Strings.Documents) },
          { 21, new ActionItem(ActionCategory.ProductivityAction, Strings.Forward) },
          { 22, new ActionItem(ActionCategory.ProductivityAction, Strings.MaximizeWindow) },
          { 23, new ActionItem(ActionCategory.ProductivityAction, Strings.MinimizeWindow) },
          { 24, new ActionItem(ActionCategory.ProductivityAction, Strings.MyHome) },
          { 25, new ActionItem(ActionCategory.ProductivityAction, Strings.OpenFile, true, false, 2) },
          { 26, new ActionItem(ActionCategory.ProductivityAction, Strings.OpenFolder, true, false, 3) },
          { 27, new ActionItem(ActionCategory.ProductivityAction, Strings.OpenNewBrowserTab) },
          { 28, new ActionItem(ActionCategory.ProductivityAction, Strings.OpenWebPage) },
          { 29, new ActionItem(ActionCategory.ProductivityAction, Strings.Paste) },
          { 30, new ActionItem(ActionCategory.ProductivityAction, Strings.ZoomIn) },
          { 31, new ActionItem(ActionCategory.ProductivityAction, Strings.ZoomOut) },
          { 32, new ActionItem(ActionCategory.ProductivityAction, Strings.ZoomReset) },

          { 33, new ActionItem(ActionCategory.MultimediaAction, Strings.MediaNextTrack) },
          { 34, new ActionItem(ActionCategory.MultimediaAction, Strings.MediaPlayPause, false, true, 0, 2) },
          { 35, new ActionItem(ActionCategory.MultimediaAction, Strings.MediaPreviousTrack) },
          { 36, new ActionItem(ActionCategory.MultimediaAction, Strings.Music) },
          { 37, new ActionItem(ActionCategory.MultimediaAction, Strings.Pictures) },
          { 38, new ActionItem(ActionCategory.MultimediaAction, Strings.VolumeDown) },
          { 39, new ActionItem(ActionCategory.MultimediaAction, Strings.VolumeMute, false, true, 0, 3) },
          { 40, new ActionItem(ActionCategory.MultimediaAction, Strings.VolumeUp) },

          { 41, new ActionItem(ActionCategory.None, Strings.PrtSc) },
          { 42, new ActionItem(ActionCategory.None, Strings.ScrollLock) },
          { 43, new ActionItem(ActionCategory.None, Strings.PauseBreak) },
          { 44, new ActionItem(ActionCategory.None, Strings.Home) },
          { 45, new ActionItem(ActionCategory.None, Strings.End) },
          { 46, new ActionItem(ActionCategory.None, Strings.PageUp) },
          { 47, new ActionItem(ActionCategory.None, Strings.PageDown) },
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

        public static readonly Dictionary<int, ActionItem> PenActions = new() {
          { 0, new ActionItem(ActionCategory.None, Strings.None, true, true) },
          { 1, new ActionItem(ActionCategory.ProductivityAction, Strings.LeftClick, false, true) },
          { 2, new ActionItem(ActionCategory.ProductivityAction, Strings.MiddleClick, false, true) },
          { 3, new ActionItem(ActionCategory.ProductivityAction, Strings.RightClick, false, true) },
          { 8, new ActionItem(ActionCategory.ProductivityAction, Strings.AssignKeystroke, true, true) },
          { 19, new ActionItem(ActionCategory.ProductivityAction, Strings._4thClick, false, true) },
          { 20, new ActionItem(ActionCategory.ProductivityAction, Strings._5thClick, false, true) },
          { 23, new ActionItem(ActionCategory.ProductivityAction, Strings.OpenRun, true, true) },
          { 26, new ActionItem(ActionCategory.ProductivityAction, Strings.Erase, false, true) },
          { 27, new ActionItem(ActionCategory.WindowsAction, Strings.BarrelButton, false, true) },
          { 35, new ActionItem(ActionCategory.ProductivityAction, Strings.Back, false, true) },
          { 36, new ActionItem(ActionCategory.ProductivityAction, Strings.Forward, false, true) },
          { 39, new ActionItem(ActionCategory.WindowsAction, Strings.SwitchApplication, false, true) },
          { 41, new ActionItem(ActionCategory.ProductivityAction, Strings.RadialMenu, false, true) },
          { 64, new ActionItem(ActionCategory.WindowsAction, Strings.DefineBySystem, true, false) },
          { 66, new ActionItem(ActionCategory.ProductivityAction, Strings.Copy, false, true) },
          { 67, new ActionItem(ActionCategory.ProductivityAction, Strings.Paste, false, true) },
          { 68, new ActionItem(ActionCategory.ProductivityAction, Strings.Undo, false, true) },
          { 69, new ActionItem(ActionCategory.ProductivityAction, Strings.Redo, false, true) },
          { 70, new ActionItem(ActionCategory.ProductivityAction, Strings.PenPageUp, true, true) },
          { 71, new ActionItem(ActionCategory.ProductivityAction, Strings.PenPageDown, true, true) },
          { 73, new ActionItem(ActionCategory.WindowsAction, Strings.PenMenu, true, false) },
          { 75, new ActionItem(ActionCategory.WindowsAction, Strings.StickyNotes, true, false) },
          { 79, new ActionItem(ActionCategory.WindowsAction, Strings.WebBrowser, false, true) },
          { 80, new ActionItem(ActionCategory.WindowsAction, Strings.EMail, false, true) },
          { 81, new ActionItem(ActionCategory.MultimediaAction, Strings.MediaPlayPause, true, true) },
          { 82, new ActionItem(ActionCategory.MultimediaAction, Strings.MediaNextTrack, true, true) },
          { 83, new ActionItem(ActionCategory.MultimediaAction, Strings.MediaPreviousTrack, true, true) },
          { 84, new ActionItem(ActionCategory.MultimediaAction, Strings.VolumeUp, true, true) },
          { 85, new ActionItem(ActionCategory.MultimediaAction, Strings.VolumeDown, true, true) },
          { 86, new ActionItem(ActionCategory.MultimediaAction, Strings.VolumeMute, true, true) },
          { 90, new ActionItem(ActionCategory.WindowsAction, Strings.ScreenSnipping, true, false) },
          { 91, new ActionItem(ActionCategory.WindowsAction, Strings.WindowsSearch, true, true) },
          { 114, new ActionItem(ActionCategory.WindowsAction, Strings.Widgets, false, true) },
          //{ 62, new ActionItem(ActionCategory.WindowsAction, Strings.OneNote, false) },
        };

        //public static readonly Dictionary<int, ActionItem> PenTopButtonActions = new() {
        //  { 0, new ActionItem(ActionCategory.None, Strings.None, false, true) },
        //  { 14, new ActionItem(ActionCategory.ProductivityAction, Strings.AssignKeystroke, false, true) },
        //  { 53, new ActionItem(ActionCategory.ProductivityAction, Strings.OpenRun, false, true) },
        //  { 54, new ActionItem(ActionCategory.ProductivityAction, Strings.PenPageDown, false, true) },
        //  { 55, new ActionItem(ActionCategory.ProductivityAction, Strings.PenPageUp, false, true) },
        //  { 91, new ActionItem(ActionCategory.WindowsAction, Strings.DefineBySystem, false, true) },
        //  { 62, new ActionItem(ActionCategory.WindowsAction, Strings.OneNote, false, true) },
        //  { 92, new ActionItem(ActionCategory.WindowsAction, Strings.PenMenu, false, true) },
        //  { 93, new ActionItem(ActionCategory.WindowsAction, Strings.QuickNote, false, true) },
        //  { 94, new ActionItem(ActionCategory.WindowsAction, Strings.ScreenSnipping, false, true) },
        //  { 95, new ActionItem(ActionCategory.WindowsAction, Strings.StickyNotes, false, true) },
        //  { 66, new ActionItem(ActionCategory.WindowsAction, Strings.WindowsSearch, false, true) },
        //  { 34, new ActionItem(ActionCategory.MultimediaAction, Strings.MediaPlayPause, false, true) },
        //  { 35, new ActionItem(ActionCategory.MultimediaAction, Strings.MediaPreviousTrack, false, true) },
        //  { 67, new ActionItem(ActionCategory.MultimediaAction, Strings.NextTrack, false, true) },
        //  { 38, new ActionItem(ActionCategory.MultimediaAction, Strings.VolumeDown, false, true) },
        //  { 39, new ActionItem(ActionCategory.MultimediaAction, Strings.VolumeMute, false, true) },
        //  { 40, new ActionItem(ActionCategory.MultimediaAction, Strings.VolumeUp, false, true) },
        //};

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
          { 0, new ActionItem(ActionCategory.None, Strings.Disabled, false, true) },
          { 8, new ActionItem(ActionCategory.None, Strings.AssignKeystroke, false, true) },
          { 23, new ActionItem(ActionCategory.None, Strings.OpenRun2, false, true) },
          { 35, new ActionItem(ActionCategory.None, Strings.GoBack, false, true) },
          { 36, new ActionItem(ActionCategory.None, Strings.GoForward, false, true) },
          { 39, new ActionItem(ActionCategory.None, Strings.SwitchApplication, false, true) },
          { 66, new ActionItem(ActionCategory.None, Strings.Copy, false, true) },
          { 67, new ActionItem(ActionCategory.None, Strings.Paste, false, true) },
          { 68, new ActionItem(ActionCategory.None, Strings.Undo, false, true) },
          { 69, new ActionItem(ActionCategory.None, Strings.Redo, false, true) },
          { 70, new ActionItem(ActionCategory.None, Strings.PageUp, false, true) },
          { 71, new ActionItem(ActionCategory.None, Strings.PageDown, false, true) },
          //{ 13, new ActionItem(ActionCategory.None, Strings.OneNote, false, true) },
          { 79, new ActionItem(ActionCategory.None, Strings.WebBrowser, false, true) },
          { 80, new ActionItem(ActionCategory.None, Strings.EMail, false, true) },
          { 81, new ActionItem(ActionCategory.None, Strings.PlayPause, false, true) },
          { 82, new ActionItem(ActionCategory.None, Strings.NextTrack, false, true) },
          { 83, new ActionItem(ActionCategory.None, Strings.PreviousTrack, false, true) },
          { 84, new ActionItem(ActionCategory.None, Strings.VolumeUp, false, true) },
          { 85, new ActionItem(ActionCategory.None, Strings.VolumeDown, false, true) },
          { 86, new ActionItem(ActionCategory.None, Strings.Mute, false, true) },
          { 91, new ActionItem(ActionCategory.None, Strings.WindowsSearch, false, true) }
        };

        public static List<int> OpenRunActionsList => OpenRunActions.Select(x => x.Key).ToList();
        public static List<int> RadialMenuActionsList => RadialMenuActions.Select(x => x.Key).ToList();

        public static List<int> AllActionsKnM
        {
            get
            {
                return KnMActions.OrderBy(x => x.Value.Caption).Select(x => x.Key).ToList();
            }
        }

        public static List<int> AllActionsPenBarrelButton
        {
            get
            {
                return PenActions.Where(x => x.Value.IsForBarrelButton).OrderBy(x => x.Value.Caption).Select(x => x.Key).ToList();
            }
        }

        public static List<int> AllActionsPenTopButton
        {
            get
            {
                return PenActions.Where(x => x.Value.IsForTopButton).OrderBy(x => x.Value.Caption).Select(x => x.Key).ToList();
            }
        }

        public static List<int> WindowsActionsKnM
        {
            get
            {
                return KnMActions.Where(x => x.Value.Category == ActionCategory.WindowsAction).OrderBy(x => x.Value.Caption).Select(x => x.Key).ToList();
            }
        }

        public static List<int> WindowsActionsPenBarrelButton
        {
            get
            {
                return PenActions.Where(x => x.Value.Category == ActionCategory.WindowsAction && x.Value.IsForBarrelButton).OrderBy(x => x.Value.Caption).Select(x => x.Key).ToList();
            }
        }

        public static List<int> WindowsActionsPenTopButton
        {
            get
            {
                return PenActions.Where(x => x.Value.Category == ActionCategory.WindowsAction && x.Value.IsForTopButton).OrderBy(x => x.Value.Caption).Select(x => x.Key).ToList();
            }
        }

        public static List<int> ProductivityActionsKnM
        {
            get
            {
                return KnMActions.Where(x => x.Value.Category == ActionCategory.ProductivityAction).OrderBy(x => x.Value.Caption).Select(x => x.Key).ToList();
            }
        }

        public static List<int> ProductivityActionsPenBarrelButton
        {
            get
            {
                return PenActions.Where(x => x.Value.Category == ActionCategory.ProductivityAction && x.Value.IsForBarrelButton).OrderBy(x => x.Value.Caption).Select(x => x.Key).ToList();
            }
        }

        public static List<int> ProductivityActionsPenTopButton
        {
            get
            {
                return PenActions.Where(x => x.Value.Category == ActionCategory.ProductivityAction && x.Value.IsForTopButton).OrderBy(x => x.Value.Caption).Select(x => x.Key).ToList();
            }
        }

        public static List<int> MultimediaActionsKnM
        {
            get
            {
                return KnMActions.Where(x => x.Value.Category == ActionCategory.MultimediaAction).OrderBy(x => x.Value.Caption).Select(x => x.Key).ToList();
            }
        }

        public static List<int> MultimediaActionsPenBarrelButton
        {
            get
            {
                return PenActions.Where(x => x.Value.Category == ActionCategory.MultimediaAction && x.Value.IsForBarrelButton).OrderBy(x => x.Value.Caption).Select(x => x.Key).ToList();
            }
        }

        public static List<int> MultimediaActionsPenTopButton
        {
            get
            {
                return PenActions.Where(x => x.Value.Category == ActionCategory.MultimediaAction && x.Value.IsForTopButton).OrderBy(x => x.Value.Caption).Select(x => x.Key).ToList();
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
                return KnMActions.Where(x => x.Value.IsSuggestedForKeyboard).OrderBy(x => x.Value.SuggestedOrderK).Select(x => x.Key).ToList();
            }
        }

        public static List<int> SuggestedActionsM
        {
            get
            {
                return KnMActions.Where(x => x.Value.IsSuggestedForMouse).OrderBy(x => x.Value.SuggestedOrderM).Select(x => x.Key).ToList();
            }
        }

        public static List<int> SuggestedActionsPenTopButton
        {
            get
            {
                return new() { 8, 0, 23, 73 };
            }
        }

        public static List<int> SuggestedActionsPenBarrelButton
        {
            get
            {
                return new() { 8, 0, 81, 41 };
            }
        }

        //Robert_Lin, 2024-6-26, fix SAST issue: [Bug] Use an immutable collection or reduce the accessibiity of the non-private readonly field.
        //The same issue with KnMActions, use solution_1, use a ImmutableList instead
        //OLD Code:
        //public readonly static List<int> AdvenceActions = [14, 25, 26, 28];
        //NEW Code:
        public static readonly ImmutableList<int> AdvancedActions = ImmutableList.Create(new int[] { 14, 25, 26, 28 });

        public static readonly ImmutableList<int> AdvancedActionsPen = ImmutableList.Create(new int[] { 8, 23, 41 });

        public static readonly Dictionary<int, IndilogicAction> ActionIdMAP = new Dictionary<int, IndilogicAction>() {
            { 21 , new IndilogicAction("{820BFCA2-201E-4AD7-A531-D8353E0FB534}","Forward") },
            { 15 , new IndilogicAction("{A156B2AE-706A-46A8-8105-12EC278AAC85}","Back") },
            { 22 , new IndilogicAction("{D4CCAFAC-5AD5-42ED-BF8C-4E96C9BA524D}","Maximize Window") },
            { 17 , new IndilogicAction("{59C2D7E4-D546-4804-85F5-B0C76FA7CB43}","Close Window") },
            { 30 , new IndilogicAction("{966D97AD-2548-4C46-B62F-1025E434E821}","Zoom In") },
            { 31 , new IndilogicAction("{804F3C4C-4157-4B13-A1C6-2817A49D988D}","Zoom Out") },
            { 32 , new IndilogicAction("{9D034453-81D2-4971-A4F8-79A7437DDDB1}","Zoom Reset") },
            { 14 , new IndilogicAction("{972768F6-0B38-4086-AA36-6136EC1AE3F5}","") },
            { 19 , new IndilogicAction("{03D40DB1-6D3D-4150-9F16-A98A2AFE343C}","Cut") },
            { 18 , new IndilogicAction("{7960DA9C-5502-4165-8250-4165F12FE58A}","Copy") },
            { 29 , new IndilogicAction("{1ADBF5D9-00E0-4CCB-B859-B0B4EBB0643F}","Paste") },
            { 16 , new IndilogicAction("{F4DD6D77-AEB4-4732-BCAF-15C7F45A36F0}","Calculator") },
            { 4 , new IndilogicAction("{35529178-47BB-4858-8881-B0EA7D54B0A5}","Notification Center") },
            { 40 , new IndilogicAction("{2F3F87AE-19F3-48BC-A054-3C1F6EDF8BA0}","Volume Up") },
            { 38 , new IndilogicAction("{C944AAFD-6668-4674-AF7D-1212293ED150}","Volume Down") },
            { 11 , new IndilogicAction("{8EA6E624-2142-416D-8718-CF2FC299F4E6}","Sleep") },
            { 9 , new IndilogicAction("{E4D7F3B0-B28C-4F67-9C0A-DA21230FCE4C}","Shut Down") },
            { 10 , new IndilogicAction("{AD58B546-D7BA-4CC7-B542-9F6E9749891E}","Sign Out") },
            { 20 , new IndilogicAction("{B976DFA9-4F0A-4876-845C-A13D52FEC792}","Documents") },
            { 37 , new IndilogicAction("{59689227-69D6-43AB-ABC4-E9BA291BFB64}","Pictures") },
            { 36 , new IndilogicAction("{9ACB1DF8-4A45-4F10-B2D0-235BAB88C16F}","Music") },
            { 8 , new IndilogicAction("{36D97440-7BC5-4F52-9DB6-A15F62E0DD9D}","Show/Hide Desktop") },
            { 2 , new IndilogicAction("{FBE45760-F8D5-45B2-BA51-967621D19850}","Devices") },
            { 7 , new IndilogicAction("{AC14A070-E472-4FF2-8401-9A2BB4A1292C}","Settings") },
            { 25 , new IndilogicAction("{E6305556-7F78-4274-9DC7-14FA69278574}","Open File") },
            { 26 , new IndilogicAction("{BE925104-C9AD-4AF7-AA66-DBC8D9694E79}","Open Folder") },
            { 0 , new IndilogicAction("{1BA1FA13-1D52-4B31-8303-ACBC4B49B41D}","None") },
            { 39 , new IndilogicAction("{6A093339-849A-439E-9A9F-D2FF9D0F6610}","Volume Mute") },
            { 35 , new IndilogicAction("{B3892026-CA01-45CB-8F56-A78AEE732C05}","Media Previous Track") },
            { 33 , new IndilogicAction("{A825068D-A738-4208-8FD0-EE2BF4455B6F}","Media Next Track") },
            { 34 , new IndilogicAction("{CEBC44E6-8AEA-4B8A-B3B6-6D0052752FC8}","Media Play/Pause") },
            { 6 , new IndilogicAction("{06568D8C-E720-40D5-8EDB-C979AEDFF153}","Search") },
            { 24 , new IndilogicAction("{AE9FA2F1-9D63-43C8-9E6C-8E84C4592825}","My Home") },
            { 3 , new IndilogicAction("{95159634-4D8D-47A4-9631-1734D60EEDB6}","Lock") },
            { 5 , new IndilogicAction("{C84CBAFD-BEB0-430E-839A-B0E25B2DF251}","Screen Snip") },
            { 41 , new IndilogicAction("{53DC555C-E0AF-4F89-AF1E-04E13F821CE8}","Print Screen") },
            { 44 , new IndilogicAction("{A3C2C2E8-02BB-453A-A32F-57864FF770D6}","Home") },
            { 45 , new IndilogicAction("{FBEDA3CC-AEAF-4A44-8AEF-7B027DC127D4}","End") },
            { 13 , new IndilogicAction("{5E845D14-6893-484B-80F5-7B1888DAF433}","Task View") },
            { 23 , new IndilogicAction("{C8AD0782-6BEE-415D-BD9F-88A10C89E53D}","Minimize Window") },
            { 27 , new IndilogicAction("{C18E855B-7AC9-4A69-BF2D-989A9E9E1E0D}","Open New Browser Tab") },
            { 12 , new IndilogicAction("{DA046FF0-36F1-4BDC-9FCD-DEBA537DD3D6}","Switch Applications") },
            { 28 , new IndilogicAction("{D8642FB9-1B1F-45D5-8D70-B0C54B651098}","Open Web Page") },
            { 42 , new IndilogicAction("{0C3D7789-650C-4005-B683-7AE960B8F1A4}","Scroll Lock") },
            { 43 , new IndilogicAction("{F15E185A-0C79-484E-B9EA-84ABFC2B8EAD}","Pause Break") },
            { 46 , new IndilogicAction("{7DC923FB-9805-41EF-B2C5-D8059CCA35FD}","Page Up") },
            { 47 , new IndilogicAction("{CC0A31FB-8125-4037-9B87-CF4971D41D6A}","Page Down") },
            { 1 , new IndilogicAction("{025325C0-50CD-404A-B8EF-1412C02112C5}","Copilot") }//ok
        };
    }
    public class IndilogicAction
    {
        public string ActionComment { get; set; }
        public string ActionGuid { get; set; }

        public IndilogicAction(string guid, string comment)
        {
            ActionComment = comment;
            ActionGuid = guid;
        }
    }

}