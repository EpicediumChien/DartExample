using DDPM.QAM;
using DDPM.SA.Common;
using System;
using System.Diagnostics;
using System.Threading;
using System.Windows;

namespace DDPM.OSDs
{
    public class OSD_Controler
    {
        private BatteryLowIIWin BatteryLowIIWinx;
        private CapsLockOffWin? CapsLockOffWinx = null;
        private CapsLockOnWin? CapsLockOnWinx = null;
        private DisplayChangedWin DisplayChangedWinx;
        private FingerprintWin FingerprintWinx;
        private HeadsetBatteryLowIWin HeadsetBatteryLowIWinx;
        private KeybordBatteryLowIWin KeybordBatteryLowIWinx;
        private MouseBatteryLowIWin MouseBatteryLowIWinx;
        private StylusBatteryLowIWin StylusBatteryLowIWin;
        private MuteWin MuteWinx;
        private UnMuteWin UnMuteWinx;
        private NumLockOffWin? NumLockOffWinx = null;
        private NumLockOnWin? NumLockOnWinx = null;
        private ScrollLockOffWin? ScrollLockOffWinx = null;
        private ScrollLockOnWin? ScrollLockOnWinx = null;
        private StartRecordingWin StartRecordingWinx;
        private WalkAwayLockWin WalkAwayLockWinx;
        private EasyMemoryWin EasyMemoryWinx;
        private ErrorWin ErrorWin;
        private QAMHotKeyWin QAMHotKeyWin;
        private CollaborationNotAvailableWin CollaborationNotAvailableWinx;
        private KeyAndKeybordBatteryLowWin keyAndKeybordBatteryLowWin;

        public OSD_Controler()
        { }

        private int osdLockMark = 0;
        private object osdLock = new object();
        public bool OSD_ShowStatus(OSDType_Device type_Device)
        {
            lock (osdLock)
            {
                int bit = (int)type_Device;
                if ((osdLockMark >> bit & 1) == 1)
                {
                    return false;
                }
                osdLockMark |= 1 << bit;
                Debug.WriteLine($"osdLockMark show={osdLockMark}");
                return true;
            }

        }

        public void OSD_ShowStatusClose(OSDType_Device type_Device)
        {
            lock (osdLock)
            {
                int bit = (int)type_Device;
                osdLockMark &= ~(1 << bit);
                Debug.WriteLine($"osdLockMark close={osdLockMark}");
            }
        }

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
            HeadsetBatteryLowIWinx.Closed += HeadsetBatteryLow_CloseWindow;

            HeadsetBatteryLowIWinx.Top = Top;
            HeadsetBatteryLowIWinx.Left = Left;
            HeadsetBatteryLowIWinx.ShowWindow();
        }
        public void HeadsetBatteryLow_CloseWindow(object? sender, EventArgs e)
        {
            if (HeadsetBatteryLowIWinx != null)
            {
                HeadsetBatteryLowIWinx.CloseWindow();
                OSD_ShowStatusClose(OSDType_Device.Headset);
            }
        }

        public void KeybordBatteryLow_ShowWindow(string Content, double Top, double Left)
        {
            KeybordBatteryLowIWinx = new KeybordBatteryLowIWin(Content);
            KeybordBatteryLowIWinx.Closed += KeybordBatteryLow_CloseWindow;

            KeybordBatteryLowIWinx.Top = Top;
            KeybordBatteryLowIWinx.Left = Left;
            KeybordBatteryLowIWinx.ShowWindow();
        }

        public void KeybordBatteryLow_CloseWindow(object? sender, EventArgs e)
        {
            if (KeybordBatteryLowIWinx != null)
            {
                KeybordBatteryLowIWinx.CloseWindow();
                OSD_ShowStatusClose(OSDType_Device.Keyboard);
            }
        }

        public void MouseBatteryLow_ShowWindow(string Content, double Top, double Left)
        {
            MouseBatteryLowIWinx = new MouseBatteryLowIWin(Content);
            MouseBatteryLowIWinx.Closed += MouseBatteryLow_CloseWindow;

            MouseBatteryLowIWinx.Top = Top;
            MouseBatteryLowIWinx.Left = Left;
            MouseBatteryLowIWinx.ShowWindow();
        }

        public void MouseBatteryLow_CloseWindow(object? sender, EventArgs e)
        {
            if (MouseBatteryLowIWinx != null)
            {
                MouseBatteryLowIWinx.CloseWindow();
                OSD_ShowStatusClose(OSDType_Device.Mouse);
            }
        }

        public void StylusBatteryLow_ShowWindow(string Content, double Top, double Left)
        {
            StylusBatteryLowIWin = new StylusBatteryLowIWin(Content);
            StylusBatteryLowIWin.Closed += StylusBatteryLow_CloseWindow;

            StylusBatteryLowIWin.Top = Top;
            StylusBatteryLowIWin.Left = Left;
            StylusBatteryLowIWin.ShowWindow();
        }

        public void StylusBatteryLow_CloseWindow(object? sender, EventArgs e)
        {
            if (StylusBatteryLowIWin != null)
            {
                StylusBatteryLowIWin.CloseWindow();
                OSD_ShowStatusClose(OSDType_Device.Pen);
            }
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
            try
            {
                //Close ScrollLockOff if exist
                ScrollLockOff_CloseWindow();

                if (null != ScrollLockOnWinx)
                {
                    return;
                }

                ScrollLockOnWinx = new ScrollLockOnWin();
                ScrollLockOnWinx.Closed += ScrollLockOnWinx_Closed;

                ScrollLockOnWinx.Top = Top;
                ScrollLockOnWinx.Left = Left;
                ScrollLockOnWinx.ShowWindow();
            }
            catch (Exception ex)
            {
                LogMsg($"Catch exception[{ex.Message}] when ScrollLockOn_ShowWindow");
            }
        }

        private void ScrollLockOnWinx_Closed(object? sender, EventArgs e)
        {
            try
            {
                if (null != ScrollLockOnWinx)
                {
                    ScrollLockOnWinx.Closed -= ScrollLockOnWinx_Closed;
                    ScrollLockOnWinx = null;
                }
            }
            catch (Exception ex)
            {
                LogMsg($"Catch exception[{ex.Message}] when ScrollLockOnWinx_Closed");
            }
        }

        public void ScrollLockOn_CloseWindow()
        {
            try
            {
                if (ScrollLockOnWinx != null)
                {
                    _ = ScrollLockOnWinx.Dispatcher.BeginInvoke(() => ScrollLockOnWinx.Hide());
                    ScrollLockOnWinx.CloseWindow();
                    ScrollLockOnWinx = null;
                }
            }
            catch (Exception ex)
            {
                LogMsg($"Catch exception[{ex.Message}] when ScrollLockOn_CloseWindow");
            }
            
        }

        public void ScrollLockOff_ShowWindow(double Top, double Left)
        {
            try
            {
                //Close ScrollLockOn if exist
                ScrollLockOn_CloseWindow();

                if (null != ScrollLockOffWinx)
                {
                    return;
                }

                ScrollLockOffWinx = new ScrollLockOffWin();
                ScrollLockOffWinx.Closed += ScrollLockOffWinx_Closed;

                ScrollLockOffWinx.Top = Top;
                ScrollLockOffWinx.Left = Left;
                ScrollLockOffWinx.ShowWindow();
            }
            catch (Exception ex)
            {
                LogMsg($"Catch exception[{ex.Message}] when ScrollLockOff_ShowWindow");
            }
            
        }

        private void ScrollLockOffWinx_Closed(object? sender, EventArgs e)
        {
            try
            {
                if (null != ScrollLockOffWinx)
                {
                    ScrollLockOffWinx.Closed -= ScrollLockOffWinx_Closed;
                    ScrollLockOffWinx = null;
                }
            }
            catch (Exception ex)
            {
                LogMsg($"Catch exception[{ex.Message}] when ScrollLockOffWinx_Closed");
            }
        }

        public void ScrollLockOff_CloseWindow()
        {
            try
            {
                if (ScrollLockOffWinx != null)
                {
                    _ = ScrollLockOffWinx.Dispatcher.BeginInvoke(() => ScrollLockOffWinx.Hide());
                    ScrollLockOffWinx.CloseWindow();
                    ScrollLockOffWinx = null;
                }
            }
            catch (Exception ex)
            {
                LogMsg($"Catch exception[{ex.Message}] when ScrollLockOff_CloseWindow");
            }
        }

        public void NumLockOn_ShowWindow(double Top, double Left)
        {
            try
            {
                //Close NumLockOff if exist
                NumLockOff_CloseWindow();

                if (null != NumLockOnWinx)
                {
                    return;
                }

                NumLockOnWinx = new NumLockOnWin();
                NumLockOnWinx.Closed += NumLockOnWinx_Closed;

                NumLockOnWinx.Top = Top;
                NumLockOnWinx.Left = Left;
                NumLockOnWinx.ShowWindow();
            }
            catch (Exception ex)
            {
                LogMsg($"Catch exception[{ex.Message}] when NumLockOn_ShowWindow");
            }
            
        }

        private void NumLockOnWinx_Closed(object? sender, EventArgs e)
        {
            try
            {
                if (null != NumLockOnWinx)
                {
                    NumLockOnWinx.Closed -= NumLockOnWinx_Closed;
                    NumLockOnWinx = null;
                }
            }
            catch (Exception ex)
            {
                LogMsg($"Catch exception[{ex.Message}] when NumLockOnWinx_Closed");
            }
        }

        public void NumLockOn_CloseWindow()
        {
            try
            {
                if (NumLockOnWinx != null)
                {
                    //NumLockOnWinx.Dispatcher.InvokeShutdown();
                    _ = NumLockOnWinx.Dispatcher.BeginInvoke(() => NumLockOnWinx.Hide());
                    NumLockOnWinx.CloseWindow();
                    NumLockOnWinx = null;
                }
            }
            catch (Exception ex)
            {
                LogMsg($"Catch exception[{ex.Message}] when NumLockOn_CloseWindow");
            }
        }

        public void NumLockOff_ShowWindow(double Top, double Left)
        {
            try
            {
                //Close NumLockOn if exist
                NumLockOn_CloseWindow();

                if (null != NumLockOffWinx)
                {
                    return;
                }

                NumLockOffWinx = new NumLockOffWin();
                NumLockOffWinx.Closed += NumLockOffWinx_Closed;

                NumLockOffWinx.Top = Top;
                NumLockOffWinx.Left = Left;
                NumLockOffWinx.ShowWindow();
            }
            catch (Exception ex)
            {
                LogMsg($"Catch exception[{ex.Message}] when NumLockOff_ShowWindow");
            }
        }

        public void NumLockOff_CloseWindow()
        {
            try
            {
                if (NumLockOffWinx != null)
                {
                    //MessageBox.Show("NumLockOff_CloseWindow");
                    _ = NumLockOffWinx.Dispatcher.BeginInvoke(() => NumLockOffWinx.Hide());
                    NumLockOffWinx.CloseWindow();
                    NumLockOffWinx = null;
                }
            }
            catch (Exception ex)
            {
                LogMsg($"Catch exception[{ex.Message}] when NumLockOff_CloseWindow");
            }            
        }

        private void NumLockOffWinx_Closed(object? sender, EventArgs e)
        {
            try
            {
                if (null != NumLockOffWinx)
                {
                    NumLockOffWinx.Closed -= NumLockOffWinx_Closed;
                    NumLockOffWinx = null;
                }
            }
            catch (Exception ex)
            {
                LogMsg($"Catch exception[{ex.Message}] when NumLockOffWinx_Closed");
            }
        }

        private void LogMsg(string msg)
        {
            if (null != DdpmCommonHelper.DeviceManagerSA)
                _ = DdpmCommonHelper.DeviceManagerSA.WriteLog(msg);
        }


        public void CapsLockOn_ShowWindow(double Top, double Left)
        {
            try
            {
                //Close CapsLockOff if exist
                CapsLockOff_CloseWindow();

                if (null != CapsLockOnWinx)
                {
                    return;
                }

                CapsLockOnWinx = new CapsLockOnWin();
                CapsLockOnWinx.Closed += CapsLockOnWinx_Closed;

                CapsLockOnWinx.Top = Top;
                CapsLockOnWinx.Left = Left;
                CapsLockOnWinx.ShowWindow();
            }
            catch (Exception ex)
            {
                LogMsg($"Catch exception[{ex.Message}] when CapsLockOn_ShowWindow");
            } 
        }

        private void CapsLockOnWinx_Closed(object? sender, EventArgs e)
        {
            try
            {
                if (CapsLockOnWinx != null)
                {
                    CapsLockOnWinx.Closed -= CapsLockOnWinx_Closed;
                    CapsLockOnWinx = null;
                }
            }
            catch (Exception ex)
            {
                LogMsg($"Catch exception[{ex.Message}] when CapsLockOnWinx_Closed");
            }
        }

        public void CapsLockOn_CloseWindow()
        {
            try
            {
                if (CapsLockOnWinx != null)
                {
                    _ = CapsLockOnWinx.Dispatcher.BeginInvoke(() => CapsLockOnWinx.Hide());
                    CapsLockOnWinx.CloseWindow();
                    CapsLockOnWinx = null;
                }
            }
            catch (Exception ex)
            {
                LogMsg($"Catch exception[{ex.Message}] when CapsLockOn_CloseWindow");
            }
        }

        public void CapsLockOff_ShowWindow(double Top, double Left)
        {
            try
            {
                //Close CapsLockOnWinx if exist
                CapsLockOn_CloseWindow();

                if (null != CapsLockOffWinx)
                {
                    return;
                }

                CapsLockOffWinx = new CapsLockOffWin();
                CapsLockOffWinx.Closed += CapsLockOffWinx_Closed;

                CapsLockOffWinx.Top = Top;
                CapsLockOffWinx.Left = Left;
                CapsLockOffWinx.ShowWindow();
            }
            catch (Exception ex)
            {
                LogMsg($"Catch exception[{ex.Message}] when CapsLockOff_CloseWindow");
            }
        }

        private void CapsLockOffWinx_Closed(object? sender, EventArgs e)
        {
            try
            {
                if (CapsLockOffWinx != null)
                {
                    CapsLockOffWinx.Closed -= CapsLockOffWinx_Closed;
                    CapsLockOffWinx = null;
                }
            }
            catch (Exception ex)
            {
                LogMsg($"Catch exception[{ex.Message}] when CapsLockOffWinx_Closed");
            }
        }

        public void CapsLockOff_CloseWindow()
        {
            try
            {
                if (CapsLockOffWinx != null)
                {
                    _ = CapsLockOffWinx.Dispatcher.BeginInvoke(() => CapsLockOffWinx.Hide());
                    CapsLockOffWinx.CloseWindow();
                    CapsLockOffWinx = null;
                }
            }
            catch (Exception ex)
            {
                LogMsg($"Catch exception[{ex.Message}] when CapsLockOff_CloseWindow");
            }
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
        public void CollaborationNotAvailableWin_ShowWindow(string Content, double Top, double Left)
        {
            CollaborationNotAvailableWinx = new CollaborationNotAvailableWin(Content);

            CollaborationNotAvailableWinx.Top = Top;
            CollaborationNotAvailableWinx.Left = Left;
            CollaborationNotAvailableWinx.ShowWindow();
        }
        public void CollaborationNotAvailableWin_CloseWindow()
        {
            if (CollaborationNotAvailableWinx != null)
                CollaborationNotAvailableWinx.CloseWindow();
        }

        public void KeyAndKeybordBatteryLowWin_ShowWindow(string Content, double Top, double Left)
        {
            keyAndKeybordBatteryLowWin = new KeyAndKeybordBatteryLowWin(Content);

            keyAndKeybordBatteryLowWin.Top = Top;
            keyAndKeybordBatteryLowWin.Left = Left;
            keyAndKeybordBatteryLowWin.ShowWindow();
        }

        public void KeyAndKeybordBatteryLowWin_ShowWindow(string Content, double Top, double Left, OSDType type, OSDType_Device device, bool state)
        {
            keyAndKeybordBatteryLowWin = new KeyAndKeybordBatteryLowWin(Content, type, device, state);

            keyAndKeybordBatteryLowWin.Top = Top;
            keyAndKeybordBatteryLowWin.Left = Left;
            keyAndKeybordBatteryLowWin.ShowWindow();
        }

        public void KeyAndKeybordBatteryLowWin_CloseWindow()
        {
            if (keyAndKeybordBatteryLowWin != null)
                keyAndKeybordBatteryLowWin.CloseWindow();
        }
    }
}