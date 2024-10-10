using DDPM.SA.Common.UpdateProgressPage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using DDPM.SA.Common.UpdateProgressPage;
using DDPM.SA.Common;
using Dell.Client.Framework.Security.Interfaces;
using Dell.Client.Framework.Security;
using PInvoke;
using System.Diagnostics;
using System.Security;

namespace MiniInstaller
{

    internal class LaunchInstaller
    {
        private UpdateProgress _UpdateProgress;
        private SWUpdatePlugins _SWUpdatePlugins;
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
                Process[] processes = Process.GetProcessesByName(processName);
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
                }
            }
            catch (Exception ex)
            {
                LogManage.LogMessage($"{nameof(CloseDDPM)} Error:{ex.Message}");
            }
            LogManage.LogMessage($"{nameof(CloseDDPM)} done");
        }
        private Task CallUpdateProgressUI()
        {
            LogManage.LogMessage($"{nameof(CallUpdateProgressUI)} start");
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
            Thread thread1 = new Thread(() =>
            {
                _UpdateProgress = new UpdateProgress();
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
    }
}
