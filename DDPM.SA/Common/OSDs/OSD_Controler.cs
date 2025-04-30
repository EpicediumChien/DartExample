using DDPM.QAM;
using DDPM.SA.Common;
using Dell.TechHub.Sdk.Common.Utilities.Extensions;
using System;
using System.Collections.Generic;
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
        public void ShowMultipleOSD(string guid, OSDType_Device oSDType_Device, OSDType_Op oSDType_Op, string title, string content, (double, double, double, double) args)
        {
            lock (osdLock)
            {
                if (OSDMainWin == null)
                {
                    OSDMainWin = new OSDMainWin();
                    OSDMainWin.Closed += CloseMultipleOSD;
                }
                switch (oSDType_Device)
                {
                    case OSDType_Device.CapsLockOn:
                    case OSDType_Device.CapsLockOff:
                        CloseMultipleOSDByType(OSDType_Device.CapsLockOff);
                        CloseMultipleOSDByType(OSDType_Device.CapsLockOn);
                        break;
                    case OSDType_Device.NumLockOn:
                    case OSDType_Device.NumLockOff:
                        CloseMultipleOSDByType(OSDType_Device.NumLockOn);
                        CloseMultipleOSDByType(OSDType_Device.NumLockOff);
                        break;
                    case OSDType_Device.ScrollLockOn:
                    case OSDType_Device.ScrollLockOff:
                        CloseMultipleOSDByType(OSDType_Device.ScrollLockOn);
                        CloseMultipleOSDByType(OSDType_Device.ScrollLockOff);
                        break;
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
                        showStringContent = content
                    });
                }
                OSDMainWin.adjustOSDWin(args);
                //Debug.WriteLine($"OSDMainWin!.OSDWins.Count========{OSDMainWin!.OSDWins.Count}");
                OSDMainWin.ShowWindow();
            }
        }
        public void CloseMultipleOSDByType(OSDType_Device oSDType_Device)
        {
            lock (osdLock)
            {
                if (OSDMainWin != null)
                {
                    List<OSDWinInfo> oSDWinInfos = OSDMainWin.OSDWins.Where(x => x.OSDType_Device.Equals(oSDType_Device)).ToList();
                    foreach (var item in oSDWinInfos)
                    {
                        item.IsFadeOut = true;
                        OSDMainWin.RemoveShowOSDWinInfo(item);
                    }
                    /*if (target != null)
                    {
                        target.IsFadeOut = true;
                        OSDMainWin.RemoveShowOSDWinInfo(target);
                    }*/
                }
            }
        }
        public void UpdateOsdContentByGuid(string guid, string content)
        {

            if (OSDMainWin != null)
            {
                OSDWinInfo? target = OSDMainWin.OSDWins.FirstOrDefault(x => x.GUID.Equals(guid, StringComparison.InvariantCultureIgnoreCase));
                if (target != null)
                {
                    target.ShowStringContent = content;
                }
            }
        }
        public bool ExistMultipleOSD()
        {
            return !(OSDMainWin == null) && OSDMainWin.OSDWins.Count > 0;
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
        public void Mute_ShowWindow(string Content, (double, double, double, double) args)
        {
            MuteWinx = new MuteWin(Content);
            MuteWinx.Closed += Mute_CloseWindow;
            MuteWinx.Top = args.Item1 + 1;
            MuteWinx.Left = args.Item2 + 1;
            MuteWinx.Width = args.Item3 - 2;
            MuteWinx.Height = args.Item4 - 2;
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

        public void UnMute_ShowWindow(string Content, (double, double, double, double) args)
        {
            UnMuteWinx = new UnMuteWin(Content);
            UnMuteWinx.Closed += UnMute_CloseWindow;
            UnMuteWinx.Top = args.Item1 + 1;
            UnMuteWinx.Left = args.Item2 + 1;
            UnMuteWinx.Width = args.Item3 - 2;
            UnMuteWinx.Height = args.Item4 - 2;
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

        public void HeadsetBatteryLow_ShowWindow(string Content, (double, double, double, double) args)
        {
            HeadsetBatteryLowIWinx = new HeadsetBatteryLowIWin(Content);
            HeadsetBatteryLowIWinx.Closed += HeadsetBatteryLow_CloseWindow;

            HeadsetBatteryLowIWinx.Top = args.Item1 + 1;
            HeadsetBatteryLowIWinx.Left = args.Item2 + 1;
            HeadsetBatteryLowIWinx.Width = args.Item3 - 2;
            HeadsetBatteryLowIWinx.Height = args.Item4 - 2;
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

        public void KeybordBatteryLow_ShowWindow(string Content, (double, double, double, double) args)
        {
            KeybordBatteryLowIWinx = new KeybordBatteryLowIWin(Content);
            KeybordBatteryLowIWinx.Closed += KeybordBatteryLow_CloseWindow;

            KeybordBatteryLowIWinx.Top = args.Item1 + 1;
            KeybordBatteryLowIWinx.Left = args.Item2 + 1;
            KeybordBatteryLowIWinx.Width = args.Item3 - 2;
            KeybordBatteryLowIWinx.Height = args.Item4 - 2;
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

        public void MouseBatteryLow_ShowWindow(string Content, (double, double, double, double) args)
        {
            MouseBatteryLowIWinx = new MouseBatteryLowIWin(Content);
            MouseBatteryLowIWinx.Closed += MouseBatteryLow_CloseWindow;

            MouseBatteryLowIWinx.Top = args.Item1 + 1;
            MouseBatteryLowIWinx.Left = args.Item2 - 1;
            MouseBatteryLowIWinx.Width = args.Item3 - 2;
            MouseBatteryLowIWinx.Height = args.Item4 - 2;
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

        public void StylusBatteryLow_ShowWindow(string Content, (double, double, double, double) args)
        {
            StylusBatteryLowIWin = new StylusBatteryLowIWin(Content);
            StylusBatteryLowIWin.Closed += StylusBatteryLow_CloseWindow;

            StylusBatteryLowIWin.Top = args.Item1 + 1;
            StylusBatteryLowIWin.Left = args.Item2 + 1;
            StylusBatteryLowIWin.Width = args.Item3 - 2;
            StylusBatteryLowIWin.Height = args.Item4 - 2;
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

        public void StartRecording_ShowWindow(string Content, (double, double, double, double) args)
        {
            StartRecordingWinx = new StartRecordingWin(Content);
            StartRecordingWinx.Closed += StartRecording_CloseWindow;
            StartRecordingWinx.Top = args.Item1 + 1;
            StartRecordingWinx.Left = args.Item2 + 1;
            StartRecordingWinx.Width = args.Item3 - 2;
            StartRecordingWinx.Height = args.Item4 - 2;
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

        public void DisplayChanged_ShowWindow(string Content, (double, double, double, double) args)
        {
            DisplayChangedWinx = new DisplayChangedWin(Content);
            DisplayChangedWinx.Closed += DisplayChanged_CloseWindow;
            DisplayChangedWinx.Top = args.Item1 + 1;
            DisplayChangedWinx.Left = args.Item2 + 1;
            DisplayChangedWinx.Width = args.Item3 - 2;
            DisplayChangedWinx.Height = args.Item4 - 2;

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

        public void WalkAwayLock_ShowWindow(string Content, (double, double, double, double) args)
        {
            WalkAwayLockWinx = new WalkAwayLockWin(Content);
            WalkAwayLockWinx.Closed += WalkAwayLock_CloseWindow;
            WalkAwayLockWinx.Top = args.Item1 + 1;
            WalkAwayLockWinx.Left = args.Item2 + 1;
            WalkAwayLockWinx.Width = args.Item3 - 2;
            WalkAwayLockWinx.Height = args.Item4 - 2;
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

        public void ScrollLockOn_ShowWindow((double, double, double, double) args)
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

                ScrollLockOnWinx.Top = args.Item1 + 1;
                ScrollLockOnWinx.Left = args.Item2 + 1;
                ScrollLockOnWinx.Width = args.Item3 - 2;
                ScrollLockOnWinx.Height = args.Item4 - 2;
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

        public void ScrollLockOff_ShowWindow((double, double, double, double) args)
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

                ScrollLockOffWinx.Top = args.Item1 + 1;
                ScrollLockOffWinx.Left = args.Item2 + 1;
                ScrollLockOffWinx.Width = args.Item3 - 2;
                ScrollLockOffWinx.Height = args.Item4 - 2;
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

        public void NumLockOn_ShowWindow((double, double, double, double) args)
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

                NumLockOnWinx.Top = args.Item1 + 1;
                NumLockOnWinx.Left = args.Item2 + 1;
                NumLockOnWinx.Width = args.Item3 - 2;
                NumLockOnWinx.Height = args.Item4 - 2;
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

        public void NumLockOff_ShowWindow((double, double, double, double) args)
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

                NumLockOffWinx.Top = args.Item1 + 1;
                NumLockOffWinx.Left = args.Item2 + 1;
                NumLockOffWinx.Width = args.Item3 - 2;
                NumLockOffWinx.Height = args.Item4 - 2;
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


        public void CapsLockOn_ShowWindow((double, double, double, double) args)
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

                CapsLockOnWinx.Top = args.Item1 + 1;
                CapsLockOnWinx.Left = args.Item2 + 1;
                CapsLockOnWinx.Width = args.Item3 - 2;
                CapsLockOnWinx.Height = args.Item4 - 2;
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

        public void CapsLockOff_ShowWindow((double, double, double, double) args)
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

                CapsLockOffWinx.Top = args.Item1 + 1;
                CapsLockOffWinx.Left = args.Item2 + 1;
                CapsLockOffWinx.Width = args.Item3 - 2;
                CapsLockOffWinx.Height = args.Item4 - 2;
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

        public void Fingerprint_ShowWindow((double, double, double, double) args)
        {
            FingerprintWinx = new FingerprintWin();
            FingerprintWinx.Closed += Fingerprint_CloseWindow;
            FingerprintWinx.Top = args.Item1 + 1;
            FingerprintWinx.Left = args.Item2 + 1;
            FingerprintWinx.Width = args.Item3 - 2;
            FingerprintWinx.Height = args.Item4 - 2;
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

        public void EasyMemory_ShowWindow((double, double, double, double) args)
        {
            EasyMemoryWinx = new EasyMemoryWin();
            EasyMemoryWinx.Closed += EasyMemory_CloseWindow;
            EasyMemoryWinx.Top = args.Item1 + 1;
            EasyMemoryWinx.Left = args.Item2 + 1;
            EasyMemoryWinx.Width = args.Item3 - 2;
            EasyMemoryWinx.Height = args.Item4 - 2;
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

        public void Error_ShowWindow(string title, string Content, bool stayOpen, (double, double, double, double) args)
        {
            ErrorWin = new ErrorWin(title, Content, stayOpen);
            ErrorWin.Closed += Error_CloseWindow;
            ErrorWin.Top = args.Item1 + 1;
            ErrorWin.Left = args.Item2 + 1;
            ErrorWin.Width = args.Item3 - 2;
            ErrorWin.Height = args.Item4 - 2;
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
        public void QAMHotKeyWin_ShowWindow((double, double, double, double) args)
        {
            QAMHotKeyWin = new QAMHotKeyWin();
            QAMHotKeyWin.Closed += QAMHotKeyWin_CloseWindow;
            QAMHotKeyWin.Top = args.Item1 + 1;
            QAMHotKeyWin.Left = args.Item2 + 1;
            QAMHotKeyWin.Width = args.Item3 - 2;
            QAMHotKeyWin.Height = args.Item4 - 2;
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
        public void CollaborationNotAvailableWin_ShowWindow(string Content, (double, double, double, double) args)
        {
            CollaborationNotAvailableWinx = new CollaborationNotAvailableWin(Content);
            CollaborationNotAvailableWinx.Closed += CollaborationNotAvailableWin_CloseWindow;
            CollaborationNotAvailableWinx.Top = args.Item1 + 1;
            CollaborationNotAvailableWinx.Left = args.Item2 + 1;
            CollaborationNotAvailableWinx.Width = args.Item3 - 2;
            CollaborationNotAvailableWinx.Height = args.Item4 - 2;
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

        public void KeyAndKeybordBatteryLowWin_ShowWindow(string Content, (double, double, double, double) args, OSDType type, OSDType_Device device, bool state)
        {
            keyAndKeybordBatteryLowWin = new KeyAndKeybordBatteryLowWin(Content, type, device, state);
            keyAndKeybordBatteryLowWin.Closed += KeyAndKeybordBatteryLowWin_CloseWindow;
            keyAndKeybordBatteryLowWin.Top = args.Item1 + 1;
            keyAndKeybordBatteryLowWin.Left = args.Item2 + 1;
            keyAndKeybordBatteryLowWin.Width = args.Item3 - 2;
            keyAndKeybordBatteryLowWin.Height = args.Item4 - 2;
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