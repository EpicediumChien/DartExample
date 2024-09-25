using DDPM.SA.Common.UpdateProgressPage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using DDPM.SA.Common.UpdateProgressPage;
using DDPM.SA.Common;

namespace MiniInstaller
{

    internal class LaunchInstaller
    {
        private UpdateProgress _UpdateProgress;
        private SWUpdatePlugins _SWUpdatePlugins;
        public Task<SWUErrorCode> DownloadAndInstall(List<FWUpdateInfo> fwUpdateInfos, string installPath = "")
        {
            SWUErrorCode ret = SWUErrorCode.NoError;
            _UpdateProgress = null;
            _SWUpdatePlugins.DownloadAndInstall();
            CallUpdateProgressUI().Wait();
            if (_UpdateProgress != null)
            {
                _SWUpdatePlugins.ProgressUpdate_Notify -= _UpdateProgress._FWUpdatePlugin_ProgressUpdate;
                _UpdateProgress.CloseWindow();
                _UpdateProgress = null;
            }
            return Task.FromResult(ret);
        }
        private Task CallUpdateProgressUI()
        {
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
            Thread thread1 = new Thread(() =>
            {
                _UpdateProgress = new UpdateProgress();
                _UpdateProgress.Width = 800;
                _UpdateProgress.Height = 440;
                _UpdateProgress.Topmost = true;
                _UpdateProgress.Closed += (sender2, e2) =>
                {
                    _UpdateProgress.Dispatcher.InvokeShutdown();
                };
                _UpdateProgress.Show();
                _SWUpdatePlugins.ProgressUpdate_Notify += _UpdateProgress._FWUpdatePlugin_ProgressUpdate;
                tcs.SetResult(true);
                Dispatcher.Run();
            });
            thread1.SetApartmentState(ApartmentState.STA);
            thread1.Start();
            return tcs.Task;
        }
    }
}
