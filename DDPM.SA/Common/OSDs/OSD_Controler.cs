using DDPM.QAM;
using DDPM.SA.Common;
using Dell.TechHub.Sdk.Common.Utilities.Extensions;
using System;
using System.Diagnostics;
using System.Linq;

namespace DDPM.OSDs
{
    public class OSD_Controler
    {
        //private BatteryLowIIWin BatteryLowIIWinx;
        private CapsLockOffWin? CapsLockOffWinx = null;
        private CapsLockOnWin? CapsLockOnWinx = null;
        private DisplayChangedWin? DisplayChangedWinx = null;
        private FingerprintWin? FingerprintWinx = null;
        private HeadsetBatteryLowIWin? HeadsetBatteryLowIWinx = null;
        private KeybordBatteryLowIWin? KeybordBatteryLowIWinx = null;
        private MouseBatteryLowIWin? MouseBatteryLowIWinx = null;
        private StylusBatteryLowIWin? StylusBatteryLowIWin = null;
        private MuteWin? MuteWinx = null;
        private UnMuteWin? UnMuteWinx = null;
        private NumLockOffWin? NumLockOffWinx = null;
        private NumLockOnWin? NumLockOnWinx = null;
        private ScrollLockOffWin? ScrollLockOffWinx = null;
        private ScrollLockOnWin? ScrollLockOnWinx = null;
        private StartRecordingWin? StartRecordingWinx = null;
        private WalkAwayLockWin? WalkAwayLockWinx = null;
        private EasyMemoryWin? EasyMemoryWinx = null;
        private ErrorWin? ErrorWin = null;
        private QAMHotKeyWin? QAMHotKeyWin = null;
        private CollaborationNotAvailableWin? CollaborationNotAvailableWinx = null;
        private KeyAndKeybordBatteryLowWin? keyAndKeybordBatteryLowWin = null;

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

        // especially for show the OSD with an "close" button
        private OSDMainWin? OSDMainWin = null;
        public void ShowMultipleOSD(string guid, OSDType_Device oSDType_Device, OSDType_Op oSDType_Op, string title, string content)
        {
            lock (osdLock)
            {
                if (OSDMainWin == null)
                {
                    OSDMainWin = new OSDMainWin();
                    OSDMainWin.Closed += CloseMultipleOSD;
                }
                if (!OSDMainWin.OSDWins.Any(x => x.GUID.Equals(guid, StringComparison.InvariantCultureIgnoreCase) && x.OSDType_Op.Equals(oSDType_Op)))
                {
                    OSDMainWin.AddShowOSDWinInfo(new OSDWinInfo()
                    {
                        GUID = guid,
                        OSDType_Device = oSDType_Device,
                        OSDType_Op = oSDType_Op,
                        ShowStringTitle = title,
                        /* ShowStringTitle = title + "(" + guid.Substring(0, 4) + ")",*/
                        ShowStringContent = content
                    });
                }
                Debug.WriteLine($"OSDMainWin!.OSDWins.Count========{OSDMainWin!.OSDWins.Count}");
                OSDMainWin.ShowWindow();
            }
        }

        public bool ExistMultipleOSD()
        {
            lock (osdLock)
            {
                return !(OSDMainWin == null) && OSDMainWin.OSDWins.Count > 0;
            }
        }
        public void CloseMultipleOSDByGuidAndOp(string guid, OSDType_Op oSDType_Op)
        {
            lock (osdLock)
            {
                if (OSDMainWin != null)
                {
                    OSDWinInfo? target = OSDMainWin.OSDWins.FirstOrDefault(x => x.GUID.Equals(guid, StringComparison.InvariantCultureIgnoreCase) && x.OSDType_Op.Equals(oSDType_Op));
                    if (target != null)
                    {
                        target.IsFadeOut = true;
                        OSDMainWin.RemoveShowOSDWinInfo(target);
                        if (OSDMainWin.OSDWins.Count == 0)
                        {
                            OSDMainWin.Closed -= CloseMultipleOSD;
                            OSDMainWin.CloseWindow();
                            OSDMainWin = null;
                        }
                    }

                }
            }

        }
        public void CloseMultipleOSD(object? sender, EventArgs e)
        {
            lock (osdLock)
            {
                if (OSDMainWin != null)
                {
                    OSDMainWin.OSDWins.ForEach(x => x.IsFadeOut = true);
                    OSDMainWin.Closed -= CloseMultipleOSD;
                    OSDMainWin.CloseWindow();
                    OSDMainWin = null;
                }
            }

        }
        public void Mute_ShowWindow(string Content, double Top, double Left)
        {
            MuteWinx = new MuteWin(Content);
            MuteWinx.Closed += Mute_CloseWindow;
            MuteWinx.Top = Top;
            MuteWinx.Left = Left;
            MuteWinx.ShowWindow();
        }

        public void Mute_CloseWindow(object? sender, EventArgs e)
        {
            if (MuteWinx != null)
            {
                MuteWinx.Closed -= Mute_CloseWindow;
                MuteWinx.CloseWindow();
                MuteWinx = null;
            }
        }

        public void UnMute_ShowWindow(string Content, double Top, double Left)
        {
            UnMuteWinx = new UnMuteWin(Content);
            UnMuteWinx.Closed += UnMute_CloseWindow;
            UnMuteWinx.Top = Top;
            UnMuteWinx.Left = Left;
            UnMuteWinx.ShowWindow();
        }

        public void UnMute_CloseWindow(object? sender, EventArgs e)
        {
            if (UnMuteWinx != null)
            {
                UnMuteWinx.Closed -= UnMute_CloseWindow;
                UnMuteWinx.CloseWindow();
                UnMuteWinx = null;
            }
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
                HeadsetBatteryLowIWinx.Closed -= HeadsetBatteryLow_CloseWindow;
                HeadsetBatteryLowIWinx.CloseWindow();
                OSD_ShowStatusClose(OSDType_Device.Headset);
                HeadsetBatteryLowIWinx = null;
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
                KeybordBatteryLowIWinx.Closed -= KeybordBatteryLow_CloseWindow;
                KeybordBatteryLowIWinx.CloseWindow();
                OSD_ShowStatusClose(OSDType_Device.Keyboard);
                KeybordBatteryLowIWinx = null;
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
                MouseBatteryLowIWinx.Closed -= MouseBatteryLow_CloseWindow;
                MouseBatteryLowIWinx.CloseWindow();
                OSD_ShowStatusClose(OSDType_Device.Mouse);
                MouseBatteryLowIWinx = null;
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
                StylusBatteryLowIWin.Closed -= StylusBatteryLow_CloseWindow;
                StylusBatteryLowIWin.CloseWindow();
                OSD_ShowStatusClose(OSDType_Device.Pen);
                StylusBatteryLowIWin = null;
            }
        }

        public void StartRecording_ShowWindow(string Content, double Top, double Left)
        {
            StartRecordingWinx = new StartRecordingWin(Content);
            StartRecordingWinx.Closed += StartRecording_CloseWindow;
            StartRecordingWinx.Top = Top;
            StartRecordingWinx.Left = Left;
            StartRecordingWinx.ShowWindow();
        }

        public void StartRecording_CloseWindow(object? sender, EventArgs e)
        {
            if (StartRecordingWinx != null)
            {
                StartRecordingWinx.Closed -= StartRecording_CloseWindow;
                StartRecordingWinx.CloseWindow();
                StartRecordingWinx = null;
            }
        }

        public void DisplayChanged_ShowWindow(string Content, double Top, double Left)
        {
            DisplayChangedWinx = new DisplayChangedWin(Content);
            DisplayChangedWinx.Closed += DisplayChanged_CloseWindow;
            DisplayChangedWinx.Top = Top;
            DisplayChangedWinx.Left = Left;
            DisplayChangedWinx.ShowWindow();
        }

        public void DisplayChanged_CloseWindow(object? sender, EventArgs e)
        {
            if (DisplayChangedWinx != null)
            {
                DisplayChangedWinx.Closed -= DisplayChanged_CloseWindow;
                DisplayChangedWinx.CloseWindow();
                DisplayChangedWinx = null;
            }
        }

        public void WalkAwayLock_ShowWindow(string Content, double Top, double Left)
        {
            WalkAwayLockWinx = new WalkAwayLockWin(Content);
            WalkAwayLockWinx.Closed += WalkAwayLock_CloseWindow;
            WalkAwayLockWinx.Top = Top;
            WalkAwayLockWinx.Left = Left;
            WalkAwayLockWinx.ShowWindow();
        }

        public void WalkAwayLock_CloseWindow(object? sender, EventArgs e)
        {
            if (WalkAwayLockWinx != null)
            {
                WalkAwayLockWinx.Closed -= WalkAwayLock_CloseWindow;
                WalkAwayLockWinx.CloseWindow();
                WalkAwayLockWinx = null;
            }
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
            FingerprintWinx.Closed += Fingerprint_CloseWindow;
            FingerprintWinx.Top = Top;
            FingerprintWinx.Left = Left;
            FingerprintWinx.ShowWindow();
        }

        public void Fingerprint_CloseWindow(object? sender, EventArgs e)
        {
            if (FingerprintWinx != null)
            {
                FingerprintWinx.Closed -= Fingerprint_CloseWindow;
                FingerprintWinx.CloseWindow();
                FingerprintWinx = null;
            }
        }

        public void EasyMemory_ShowWindow(double Top, double Left)
        {
            EasyMemoryWinx = new EasyMemoryWin();
            EasyMemoryWinx.Closed += EasyMemory_CloseWindow;
            EasyMemoryWinx.Top = Top;
            EasyMemoryWinx.Left = Left;
            EasyMemoryWinx.ShowWindow();
        }

        public void EasyMemory_CloseWindow(object? sender, EventArgs e)
        {
            if (EasyMemoryWinx != null)
            {
                EasyMemoryWinx.Closed -= EasyMemory_CloseWindow;
                EasyMemoryWinx.CloseWindow();
                EasyMemoryWinx = null;
            }
        }

        public void Error_ShowWindow(string title, string Content, bool stayOpen, double Top, double Left)
        {
            ErrorWin = new ErrorWin(title, Content, stayOpen);
            ErrorWin.Closed += Error_CloseWindow;
            ErrorWin.Top = Top;
            ErrorWin.Left = Left;
            ErrorWin.ShowWindow();
        }

        public void Error_CloseWindow(object? sender, EventArgs e)
        {
            if (ErrorWin != null)
            {
                ErrorWin.Closed -= Error_CloseWindow;
                ErrorWin.CloseWindow();
                ErrorWin = null;
            }
        }
        public void QAMHotKeyWin_ShowWindow(double Top, double Left)
        {
            QAMHotKeyWin = new QAMHotKeyWin();
            QAMHotKeyWin.Closed += QAMHotKeyWin_CloseWindow;
            QAMHotKeyWin.Top = Top;
            QAMHotKeyWin.Left = Left;
            QAMHotKeyWin.ShowWindow();
        }
        public void QAMHotKeyWin_CloseWindow(object? sender, EventArgs e)
        {
            if (QAMHotKeyWin != null)
            {
                QAMHotKeyWin.Closed -= QAMHotKeyWin_CloseWindow;
                QAMHotKeyWin.CloseWindow();
                QAMHotKeyWin = null;
            }
        }
        public void CollaborationNotAvailableWin_ShowWindow(string Content, double Top, double Left)
        {
            CollaborationNotAvailableWinx = new CollaborationNotAvailableWin(Content);
            CollaborationNotAvailableWinx.Closed += CollaborationNotAvailableWin_CloseWindow;
            CollaborationNotAvailableWinx.Top = Top;
            CollaborationNotAvailableWinx.Left = Left;
            CollaborationNotAvailableWinx.ShowWindow();
        }
        public void CollaborationNotAvailableWin_CloseWindow(object? sender, EventArgs e)
        {
            if (CollaborationNotAvailableWinx != null)
            {
                CollaborationNotAvailableWinx.Closed -= CollaborationNotAvailableWin_CloseWindow;
                CollaborationNotAvailableWinx.CloseWindow();
                CollaborationNotAvailableWinx = null;
            }
        }

        /*        public void KeyAndKeybordBatteryLowWin_ShowWindow(string Content, double Top, double Left)
                {
                    keyAndKeybordBatteryLowWin = new KeyAndKeybordBatteryLowWin(Content);

                    keyAndKeybordBatteryLowWin.Top = Top;
                    keyAndKeybordBatteryLowWin.Left = Left;
                    keyAndKeybordBatteryLowWin.ShowWindow();
                }*/

        public void KeyAndKeybordBatteryLowWin_ShowWindow(string Content, double Top, double Left, OSDType type, OSDType_Device device, bool state)
        {
            keyAndKeybordBatteryLowWin = new KeyAndKeybordBatteryLowWin(Content, type, device, state);
            keyAndKeybordBatteryLowWin.Closed += KeyAndKeybordBatteryLowWin_CloseWindow;
            keyAndKeybordBatteryLowWin.Top = Top;
            keyAndKeybordBatteryLowWin.Left = Left;
            keyAndKeybordBatteryLowWin.ShowWindow();
        }

        public void KeyAndKeybordBatteryLowWin_CloseWindow(object? sender, EventArgs e)
        {
            if (keyAndKeybordBatteryLowWin != null)
            {
                keyAndKeybordBatteryLowWin.Closed -= KeyAndKeybordBatteryLowWin_CloseWindow;
                keyAndKeybordBatteryLowWin.CloseWindow();
                keyAndKeybordBatteryLowWin = null;
            }
        }
    }
}