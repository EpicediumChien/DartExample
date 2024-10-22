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
          //{ 109, new ActionItem(ActionCategory.WordAction, Strings.PasteAndMatchFormatting, false, false) }, //IL not support
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
          { 200, new ActionItem(ActionCategory.ExcelAction, Strings.Autoscroll, false, false) },
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
          //{ 212, new ActionItem(ActionCategory.ExcelAction, Strings.PanHoldAndMoveMouse, false, false) },  //IL not support
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
          //{ 310, new ActionItem(ActionCategory.PowerPointAction, Strings.PanHoldAndMoveMouse, false, false) },
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

        public static readonly Dictionary<int, string> ActionIdToGuid = new()
        {
            { 0, "{1BA1FA13-1D52-4B31-8303-ACBC4B49B41D}" },
            { 1, "{025325C0-50CD-404A-B8EF-1412C02112C5}" },
            { 2, "{FBE45760-F8D5-45B2-BA51-967621D19850}" },
            { 3, "{95159634-4D8D-47A4-9631-1734D60EEDB6}" },
            { 4, "{35529178-47BB-4858-8881-B0EA7D54B0A5}" },
            { 5, "{C84CBAFD-BEB0-430E-839A-B0E25B2DF251}" },
            { 6, "{06568D8C-E720-40D5-8EDB-C979AEDFF153}" },
            { 7, "{AC14A070-E472-4FF2-8401-9A2BB4A1292C}" },
            { 8, "{36D97440-7BC5-4F52-9DB6-A15F62E0DD9D}" },
            { 9, "{E4D7F3B0-B28C-4F67-9C0A-DA21230FCE4C}" },
            { 10, "{AD58B546-D7BA-4CC7-B542-9F6E9749891E}" },
            { 11, "{8EA6E624-2142-416D-8718-CF2FC299F4E6}" },
            { 12, "{DA046FF0-36F1-4BDC-9FCD-DEBA537DD3D6}" },
            { 13, "{5E845D14-6893-484B-80F5-7B1888DAF433}" },
            { 14, "{972768F6-0B38-4086-AA36-6136EC1AE3F5}" },
            { 15, "{A156B2AE-706A-46A8-8105-12EC278AAC85}" },
            { 16, "{F4DD6D77-AEB4-4732-BCAF-15C7F45A36F0}" },
            { 17, "{59C2D7E4-D546-4804-85F5-B0C76FA7CB43}" },
            { 18, "{7960DA9C-5502-4165-8250-4165F12FE58A}" },
            { 19, "{03D40DB1-6D3D-4150-9F16-A98A2AFE343C}" },
            { 20, "{B976DFA9-4F0A-4876-845C-A13D52FEC792}" },
            { 21, "{820BFCA2-201E-4AD7-A531-D8353E0FB534}" },
            { 22, "{D4CCAFAC-5AD5-42ED-BF8C-4E96C9BA524D}" },
            { 23, "{C8AD0782-6BEE-415D-BD9F-88A10C89E53D}" },
            { 24, "{AE9FA2F1-9D63-43C8-9E6C-8E84C4592825}" },
            { 25, "{E6305556-7F78-4274-9DC7-14FA69278574}" },
            { 26, "{BE925104-C9AD-4AF7-AA66-DBC8D9694E79}" },
            { 27, "{C18E855B-7AC9-4A69-BF2D-989A9E9E1E0D}" },
            { 28, "{D8642FB9-1B1F-45D5-8D70-B0C54B651098}" },
            { 29, "{1ADBF5D9-00E0-4CCB-B859-B0B4EBB0643F}" },
            { 30, "{966D97AD-2548-4C46-B62F-1025E434E821}" },
            { 31, "{804F3C4C-4157-4B13-A1C6-2817A49D988D}" },
            { 32, "{9D034453-81D2-4971-A4F8-79A7437DDDB1}" },
            { 33, "{A825068D-A738-4208-8FD0-EE2BF4455B6F}" },
            { 34, "{CEBC44E6-8AEA-4B8A-B3B6-6D0052752FC8}" },
            { 35, "{B3892026-CA01-45CB-8F56-A78AEE732C05}" },
            { 36, "{9ACB1DF8-4A45-4F10-B2D0-235BAB88C16F}" },
            { 37, "{59689227-69D6-43AB-ABC4-E9BA291BFB64}" },
            { 38, "{C944AAFD-6668-4674-AF7D-1212293ED150}" },
            { 39, "{6A093339-849A-439E-9A9F-D2FF9D0F6610}" },
            { 40, "{2F3F87AE-19F3-48BC-A054-3C1F6EDF8BA0}" },
            { 41, "{53DC555C-E0AF-4F89-AF1E-04E13F821CE8}" },
            { 42, "{0C3D7789-650C-4005-B683-7AE960B8F1A4}" },
            { 43, "{F15E185A-0C79-484E-B9EA-84ABFC2B8EAD}" },
            { 44, "{A3C2C2E8-02BB-453A-A32F-57864FF770D6}" },
            { 45, "{FBEDA3CC-AEAF-4A44-8AEF-7B027DC127D4}" },
            { 46, "{7DC923FB-9805-41EF-B2C5-D8059CCA35FD}" },
            { 47, "{CC0A31FB-8125-4037-9B87-CF4971D41D6A}" },
            { 101, "{D8CB0A73-982F-406F-A633-83F5641BFCE8}" },
            { 102, "{5063DE79-E735-44F9-812A-19FFFE1E0BAA}" },
            { 103, "{748A22A0-B998-4861-AB01-8A571850A28A}" },
            { 104, "{5F1B89E2-3477-4CF2-A554-70EDE7D6B19E}" },
            { 105, "{6D6C055C-446A-4B5F-8111-E35C2A44D3D9}" },
            { 106, "{B75B82E9-E603-4F8B-9991-47F1D01B540A}" },
            { 107, "{0C9CCDA9-6FE5-480E-A8E5-F0AB639DEDB6}" },
            { 108, "{104A56A9-A577-4AE1-BB7F-8A41124C7180}" },
            { 110, "{DFF4AFA4-DC5F-4266-9169-64586140B50A}" },
            { 111, "{0EA21171-77D5-4E7A-BF0B-91C766BC4811}" },
            { 112, "{1B1545DD-F996-41E4-AF04-2FA2213CA719}" },
            { 113, "{6DDD00F6-BE0B-4EA6-A019-7F847CFD23E3}" },
            { 114, "{9C00978F-B489-49FE-B4EC-AEAAA94FC4C9}" },
            { 115, "{86923B16-6E73-4F25-AFA5-AAE923E262E6}" },
            { 116, "{83B5767F-242D-4814-BB47-FC2BAB2BA1D0}" },
            { 117, "{9D356DEA-1E6C-4F7A-87C4-55E17F3433B8}" },
            { 118, "{C37144DE-CCB4-4164-A217-E52C17AD3EF7}" },
            { 119, "{0FC9D020-BB3E-44E3-A7D5-3F5D1C4B75DF}" },
            { 120, "{5AB084FE-97E0-4A64-9824-D95DBDEE811E}" },
            { 121, "{966D97AD-2548-4C46-B62F-1025E434E821}" },
            { 122, "{804F3C4C-4157-4B13-A1C6-2817A49D988D}" },
            { 200, "{D8CB0A73-982F-406F-A633-83F5641BFCE8}" },
            { 201, "{905710B7-904D-4E86-BFD4-AB9F6C2FA651}" },
            { 202, "{25249AF8-B06A-4932-8E50-C7D39CD5FF72}" },
            { 203, "{6A81D7D2-F814-4080-A49C-37630F6A15CB}" },
            { 204, "{08F91980-4E4E-40C5-97F1-69D4BBDF5697}" },
            { 205, "{3D410981-8B77-4EAC-8FA9-115D3732BBD0}" },
            { 206, "{7E8B5439-A2E9-4094-B609-590866A79975}" },
            { 207, "{372AFA19-1604-48CA-BD65-503725B11936}" },
            { 208, "{958AECA0-D49D-45AF-A492-747FCACE2FE8}" },
            { 209, "{9F3CC307-A773-4EB4-8867-E42313975C23}" },
            { 210, "{5F1B89E2-3477-4CF2-A554-70EDE7D6B19E}" },
            { 211, "{B75B82E9-E603-4F8B-9991-47F1D01B540A}" },
            { 213, "{AE2A7A7B-5F26-4B79-9EB3-B6C2296E803D}" },
            { 214, "{ABAA5983-65D3-4F73-A8CF-63EE43FE2A3F}" },
            { 215, "{59E4124C-08AC-4470-9B6A-05BB27A55E26}" },
            { 216, "{1B1545DD-F996-41E4-AF04-2FA2213CA719}" },
            { 217, "{0F9A5F8B-15AE-40FE-8C4A-1EE93017FFE7}" },
            { 218, "{9C00978F-B489-49FE-B4EC-AEAAA94FC4C9}" },
            { 219, "{4FF621F8-81B3-4A5F-9D5C-83CC1FEB938C}" },
            { 220, "{966D97AD-2548-4C46-B62F-1025E434E821}" },
            { 221, "{804F3C4C-4157-4B13-A1C6-2817A49D988D}" },
            { 301, "{43E71254-6501-4240-B608-B593E21E726D}" },
            { 302, "{74A30D08-94F4-431A-9F27-048BC842D063}" },
            { 303, "{1487A3C2-3306-4E76-8C04-379088185776}" },
            { 304, "{1E710FB5-4DAB-45ED-AA02-6BA9D5BBB968}" },
            { 305, "{A422D0EA-0781-4BF6-ADD6-C8A8E714E785}" },
            { 306, "{F858E304-477D-401B-9D87-110FABEB66A4}" },
            { 307, "{4073FE50-F05D-426F-BF5C-4826950CBC84}" },
            { 308, "{5F1B89E2-3477-4CF2-A554-70EDE7D6B19E}" },
            { 309, "{B75B82E9-E603-4F8B-9991-47F1D01B540A}" },
            { 311, "{246ABB06-0404-4D8C-9C44-2613D7C691EB}" },
            { 312, "{1B1545DD-F996-41E4-AF04-2FA2213CA719}" },
            { 313, "{FB8BC07F-818F-4840-AA03-9196004361D6}" },
            { 314, "{9C00978F-B489-49FE-B4EC-AEAAA94FC4C9}" },
            { 315, "{A1E2F2BD-0676-491C-BD74-7FC4BD623B67}" },
            { 316, "{86923B16-6E73-4F25-AFA5-AAE923E262E6}" },
            { 317, "{83B5767F-242D-4814-BB47-FC2BAB2BA1D0}" },
            { 318, "{9D356DEA-1E6C-4F7A-87C4-55E17F3433B8}" },
            { 319, "{966D97AD-2548-4C46-B62F-1025E434E821}" },
            { 320, "{804F3C4C-4157-4B13-A1C6-2817A49D988D}" },
            { 401, "{E661D079-9FE7-4D9B-BEE0-721C78A31DBC}" },
            { 402, "{D9E165A4-48B3-459C-8023-CF4DC7051619}" },
            { 403, "{9BDF2A38-535D-4762-953B-ACB3462DEF03}" },
            { 404, "{748A22A0-B998-4861-AB01-8A571850A28A}" },
            { 405, "{F9EF5872-C1D0-4BCB-8A75-2DDF46E222CC}" },
            { 406, "{914F36BF-A28A-4BFB-803E-07E29BC66303}" },
            { 407, "{6B798768-376C-4295-9FF8-EE992B113675}" },
            { 408, "{3DCE84ED-081D-4ACB-A8A7-ED49FBF8024C}" },
        };
    }
}