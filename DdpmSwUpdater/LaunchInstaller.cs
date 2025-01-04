using DDPM.SA.Common.UpdateProgressPage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using DDPM.SA.Common;
using Dell.Client.Framework.Security.Interfaces;
using Dell.Client.Framework.Security;
using PInvoke;
using System.Diagnostics;
using System.Security;
using Newtonsoft.Json.Linq;

namespace DdpmSwUpdater
{

    internal class LaunchInstaller
    {
        private UpdateProgress _UpdateProgress;
        private SWUpdatePlugins _SWUpdatePlugins;
        Thread _CheckDDPMThread;
        CancellationTokenSource _CancellationTokenSource;
        public LaunchInstaller()
        {
            _SWUpdatePlugins = new SWUpdatePlugins();
        }
        public Task<SWUErrorCode> LaunchUpdate()
        {
            LogManage.LogMessage($"{nameof(LaunchUpdate)} start");
            SWUErrorCode ret = SWUErrorCode.NoError;
            CloseDDPM();
            {
                CallUpdateProgressUI().Wait();
                List<SWUpdateInfo> retSWUpdate = _SWUpdatePlugins.DownloadAndInstall("").Result;
                if (_UpdateProgress != null)
                {
                    _SWUpdatePlugins.ProgressUpdate_Notify -= _UpdateProgress._FWUpdatePlugin_ProgressUpdate;
                    _UpdateProgress.CloseWindow();
                    _UpdateProgress = null;
                }
                if (_CheckDDPMThread != null && _CancellationTokenSource != null)
                {
                    _CancellationTokenSource.Cancel();
                }
                foreach (SWUpdateInfo swUErrorCode in retSWUpdate)
                {
                    if (swUErrorCode.SWUErrorCode != SWUErrorCode.NoError)
                    {
                        ret = swUErrorCode.SWUErrorCode;
                    }
                }
            }
            LogManage.LogMessage($"{nameof(LaunchUpdate)} done");
            return Task.FromResult(ret);
        }
        private void CloseDDPM()
        {
            try
            {
                LogManage.LogMessage($"{nameof(CloseDDPM)} start");
                string processName = "DDPM";
                Process[] processes;
                do
                {
                    processes = Process.GetProcessesByName(processName);
                    LogManage.LogMessage($"{nameof(CloseDDPM)} processes.Length {processes.Length}");
                    if (processes.Length > 0)
                    {
                        foreach (Process process in processes)
                        {
                            // Close process by sending a close message to its main window.
                            process.CloseMainWindow();
                            // Free resources associated with process.
                            process.Close();
                        }
                        Thread.Sleep(1000);
                    }
                } while (processes.Length > 0);
            }
            catch (Exception ex)
            {
                LogManage.LogMessage($"{nameof(CloseDDPM)} Error:{ex.Message}");
            }
            _CancellationTokenSource = new CancellationTokenSource();
            CancellationToken token = _CancellationTokenSource.Token;
            _CheckDDPMThread = new Thread(() => CheckDDPM(token));
            _CheckDDPMThread.Start();
            LogManage.LogMessage($"{nameof(CloseDDPM)} done");
        }
        private Task CallUpdateProgressUI()
        {
            LogManage.LogMessage($"{nameof(CallUpdateProgressUI)} start");
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
            Thread thread1 = new Thread(() =>
            {
                _UpdateProgress = new UpdateProgress(LogManage.logs);
                _UpdateProgress.Closed += (sender2, e2) =>
                {
                    _UpdateProgress.Dispatcher.InvokeShutdown();
                };
                _UpdateProgress.Dispatcher.Invoke(() => _UpdateProgress.Show());
                _SWUpdatePlugins.ProgressUpdate_Notify += _UpdateProgress._FWUpdatePlugin_ProgressUpdate;
                tcs.SetResult(true);
                Dispatcher.Run();
            });
            thread1.SetApartmentState(ApartmentState.STA);
            thread1.Start();
            LogManage.LogMessage($"{nameof(CallUpdateProgressUI)} done");
            return tcs.Task;
        }
        void CheckDDPM(CancellationToken token)
        {
            LogManage.LogMessage($"{nameof(CheckDDPM)} start");
            string processName = "DDPM";
            Process[] processes;
            do
            {
                processes = Process.GetProcessesByName(processName);
                Thread.Sleep(1000);
            }
            while (processes.Length <= 0 && !token.IsCancellationRequested);
            if (_UpdateProgress != null)
            {
                _UpdateProgress.HideWindow();
            }
            LogManage.LogMessage($"{nameof(CheckDDPM)} done");
        }
    }
}
