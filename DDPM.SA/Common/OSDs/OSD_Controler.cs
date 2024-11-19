namespace DDPM.OSDs
{
    public class OSD_Controler
    {
        private BatteryLowIIWin BatteryLowIIWinx;
        private CapsLockOffWin CapsLockOffWinx;
        private CapsLockOnWin CapsLockOnWinx;
        private DisplayChangedWin DisplayChangedWinx;
        private FingerprintWin FingerprintWinx;
        private HeadsetBatteryLowIWin HeadsetBatteryLowIWinx;
        private KeybordBatteryLowIWin KeybordBatteryLowIWinx;
        private MouseBatteryLowIWin MouseBatteryLowIWinx;
        private StylusBatteryLowIWin StylusBatteryLowIWin;
        private MuteWin MuteWinx;
        private UnMuteWin UnMuteWinx;
        private NumLockOffWin NumLockOffWinx;
        private NumLockOnWin NumLockOnWinx;
        private ScrollLockOffWin ScrollLockOffWinx;
        private ScrollLockOnWin ScrollLockOnWinx;
        private StartRecordingWin StartRecordingWinx;
        private WalkAwayLockWin WalkAwayLockWinx;
        private EasyMemoryWin EasyMemoryWinx;
        private ErrorWin ErrorWin;
        private QAMHotKeyWin QAMHotKeyWin;

        public OSD_Controler()
        { }

        public void Mute_ShowWindow(string Content, double Top, double Left)
        {
            MuteWinx = new MuteWin(Content);

            MuteWinx.Top = Top;
            MuteWinx.Left = Left;
            MuteWinx.ShowWindow();
        }

        public void Mute_CloseWindow()
        {
            if (MuteWinx != null)
                MuteWinx.CloseWindow();
        }

        public void UnMute_ShowWindow(string Content, double Top, double Left)
        {
            UnMuteWinx = new UnMuteWin(Content);

            UnMuteWinx.Top = Top;
            UnMuteWinx.Left = Left;
            UnMuteWinx.ShowWindow();
        }

        public void UnMute_CloseWindow()
        {
            if (UnMuteWinx != null)
                UnMuteWinx.CloseWindow();
        }

        public void HeadsetBatteryLow_ShowWindow(string Content, double Top, double Left)
        {
            HeadsetBatteryLowIWinx = new HeadsetBatteryLowIWin(Content);

            HeadsetBatteryLowIWinx.Top = Top;
            HeadsetBatteryLowIWinx.Left = Left;
            HeadsetBatteryLowIWinx.ShowWindow();
        }

        public void HeadsetBatteryLow_CloseWindow()
        {
            if (HeadsetBatteryLowIWinx != null)
                HeadsetBatteryLowIWinx.CloseWindow();
        }

        public void KeybordBatteryLow_ShowWindow(string Content, double Top, double Left)
        {
            KeybordBatteryLowIWinx = new KeybordBatteryLowIWin(Content);

            KeybordBatteryLowIWinx.Top = Top;
            KeybordBatteryLowIWinx.Left = Left;
            KeybordBatteryLowIWinx.ShowWindow();
        }

        public void KeybordBatteryLow_CloseWindow()
        {
            if (KeybordBatteryLowIWinx != null)
                KeybordBatteryLowIWinx.CloseWindow();
        }

        public void MouseBatteryLow_ShowWindow(string Content, double Top, double Left)
        {
            MouseBatteryLowIWinx = new MouseBatteryLowIWin(Content);

            MouseBatteryLowIWinx.Top = Top;
            MouseBatteryLowIWinx.Left = Left;
            MouseBatteryLowIWinx.ShowWindow();
        }

        public void MouseBatteryLow_CloseWindow()
        {
            if (MouseBatteryLowIWinx != null)
                MouseBatteryLowIWinx.CloseWindow();
        }

        public void StylusBatteryLow_ShowWindow(string Content, double Top, double Left)
        {
            StylusBatteryLowIWin = new StylusBatteryLowIWin(Content);

            StylusBatteryLowIWin.Top = Top;
            StylusBatteryLowIWin.Left = Left;
            StylusBatteryLowIWin.ShowWindow();
        }

        public void StylusBatteryLow_CloseWindow()
        {
            if (StylusBatteryLowIWin != null)
                StylusBatteryLowIWin.CloseWindow();
        }

        public void StartRecording_ShowWindow(string Content, double Top, double Left)
        {
            StartRecordingWinx = new StartRecordingWin(Content);

            StartRecordingWinx.Top = Top;
            StartRecordingWinx.Left = Left;
            StartRecordingWinx.ShowWindow();
        }

        public void StartRecording_CloseWindow()
        {
            if (StartRecordingWinx != null)
                StartRecordingWinx.CloseWindow();
        }

        public void DisplayChanged_ShowWindow(string Content, double Top, double Left)
        {
            DisplayChangedWinx = new DisplayChangedWin(Content);

            DisplayChangedWinx.Top = Top;
            DisplayChangedWinx.Left = Left;
            DisplayChangedWinx.ShowWindow();
        }

        public void DisplayChanged_CloseWindow()
        {
            if (DisplayChangedWinx != null)
                DisplayChangedWinx.CloseWindow();
        }

        public void WalkAwayLock_ShowWindow(string Content, double Top, double Left)
        {
            WalkAwayLockWinx = new WalkAwayLockWin(Content);

            WalkAwayLockWinx.Top = Top;
            WalkAwayLockWinx.Left = Left;
            WalkAwayLockWinx.ShowWindow();
        }

        public void WalkAwayLock_CloseWindow()
        {
            if (WalkAwayLockWinx != null)
                WalkAwayLockWinx.CloseWindow();
        }

        public void ScrollLockOn_ShowWindow(double Top, double Left)
        {
            ScrollLockOnWinx = new ScrollLockOnWin();

            ScrollLockOnWinx.Top = Top;
            ScrollLockOnWinx.Left = Left;
            ScrollLockOnWinx.ShowWindow();
        }

        public void ScrollLockOn_CloseWindow()
        {
            if (ScrollLockOnWinx != null)
                ScrollLockOnWinx.CloseWindow();
        }

        public void ScrollLockOff_ShowWindow(double Top, double Left)
        {
            ScrollLockOffWinx = new ScrollLockOffWin();

            ScrollLockOffWinx.Top = Top;
            ScrollLockOffWinx.Left = Left;
            ScrollLockOffWinx.ShowWindow();
        }

        public void ScrollLockOff_CloseWindow()
        {
            if (ScrollLockOffWinx != null)
                ScrollLockOffWinx.CloseWindow();
        }

        public void NumLockOn_ShowWindow(double Top, double Left)
        {
            NumLockOnWinx = new NumLockOnWin();

            NumLockOnWinx.Top = Top;
            NumLockOnWinx.Left = Left;
            NumLockOnWinx.ShowWindow();
        }

        public void NumLockOn_CloseWindow()
        {
            if (NumLockOnWinx != null)
                NumLockOnWinx.CloseWindow();
        }

        public void NumLockOff_ShowWindow(double Top, double Left)
        {
            NumLockOffWinx = new NumLockOffWin();

            NumLockOffWinx.Top = Top;
            NumLockOffWinx.Left = Left;
            NumLockOffWinx.ShowWindow();
        }

        public void NumLockOff_CloseWindow()
        {
            if (NumLockOffWinx != null)
                NumLockOffWinx.CloseWindow();
        }

        public void CapsLockOn_ShowWindow(double Top, double Left)
        {
            CapsLockOnWinx = new CapsLockOnWin();

            CapsLockOnWinx.Top = Top;
            CapsLockOnWinx.Left = Left;
            CapsLockOnWinx.ShowWindow();
        }

        public void CapsLockOn_CloseWindow()
        {
            if (CapsLockOnWinx != null)
                CapsLockOnWinx.CloseWindow();
        }

        public void CapsLockOff_ShowWindow(double Top, double Left)
        {
            CapsLockOffWinx = new CapsLockOffWin();

            CapsLockOffWinx.Top = Top;
            CapsLockOffWinx.Left = Left;
            CapsLockOffWinx.ShowWindow();
        }

        public void CapsLockOff_CloseWindow()
        {
            if (CapsLockOffWinx != null)
                CapsLockOffWinx.CloseWindow();
        }

        public void Fingerprint_ShowWindow(double Top, double Left)
        {
            FingerprintWinx = new FingerprintWin();

            FingerprintWinx.Top = Top;
            FingerprintWinx.Left = Left;
            FingerprintWinx.ShowWindow();
        }

        public void Fingerprint_CloseWindow()
        {
            if (FingerprintWinx != null)
                FingerprintWinx.CloseWindow();
        }

        public void EasyMemory_ShowWindow(double Top, double Left)
        {
            EasyMemoryWinx = new EasyMemoryWin();

            EasyMemoryWinx.Top = Top;
            EasyMemoryWinx.Left = Left;
            EasyMemoryWinx.ShowWindow();
        }

        public void EasyMemory_CloseWindow()
        {
            if (EasyMemoryWinx != null)
                EasyMemoryWinx.CloseWindow();
        }

        public void Error_ShowWindow(string title, string Content, bool stayOpen, double Top, double Left)
        {
            ErrorWin = new ErrorWin(title, Content, stayOpen);

            ErrorWin.Top = Top;
            ErrorWin.Left = Left;
            ErrorWin.ShowWindow();
        }

        public void Error_CloseWindow()
        {
            if (ErrorWin != null)
                ErrorWin.CloseWindow();
        }
        public void QAMHotKeyWin_ShowWindow(double Top, double Left)
        {
            QAMHotKeyWin = new QAMHotKeyWin();

            QAMHotKeyWin.Top = Top;
            QAMHotKeyWin.Left = Left;
            QAMHotKeyWin.ShowWindow();
        }
        public void QAMHotKeyWin_CloseWindow()
        {
            if (QAMHotKeyWin != null)
                QAMHotKeyWin.CloseWindow();
        }
    }
}