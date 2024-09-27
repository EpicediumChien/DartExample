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
            SWUErrorCode ret = SWUErrorCode.NoError;
            List<SWUpdateInfo> swUpdate = _SWUpdatePlugins.CheckUpdate().Result;
            if (swUpdate != null && swUpdate.Count > 0)
            {
                CallUpdateProgressUI().Wait();
                List<SWUpdateInfo> retSWUpdate = _SWUpdatePlugins.DownloadAndInstall(swUpdate, "").Result;
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
            return Task.FromResult(ret);
        }
        private Task CallUpdateProgressUI()
        {
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
            var sessionId = Kernel32.WTSGetActiveConsoleSessionId();
            if (sessionId is Advapi32.InvalidSessionId) throw new InvalidOperationException($"Cannot get session id");
            IntPtr token = UserImpersonator.GetTokenFromSession(sessionId, systemUser: false);
            VerifierOption myVerifierOptions = VerifierOption.FailOnNoErrorsAndSelfSignedCert;
            SubjectPublicKeyInfoHashes hashes = new SubjectPublicKeyInfoHashes(HashType.Sha256);
            var constraints = new LeafCertConstraints(hashes)
            {
                RequireAllCerts = false
            };
            PeAuthenticodeVerifier verifier = new PeAuthenticodeVerifier(myVerifierOptions, omitDefaultOptions: true)
            {
                Constraints = constraints
            };
            UserImpersonator.RunAsUser(token, () =>
            {
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
            });
            return tcs.Task;
        }
    }
}
