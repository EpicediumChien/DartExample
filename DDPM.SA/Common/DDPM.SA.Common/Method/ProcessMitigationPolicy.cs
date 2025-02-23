using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DDPM.SA.Common.Method
{
    public class ProcessMitigationPolicy
    {
        // Define the PROCESS_MITIGATION_REDIRECTION_TRUST_POLICY structure
        [StructLayout(LayoutKind.Sequential)]
        private struct PROCESS_MITIGATION_REDIRECTION_TRUST_POLICY
        {
            public UInt32 Flags;
        }

        // Define the SetProcessMitigationPolicy function from kernel32.dll
        [DllImport("kernel32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern bool SetProcessMitigationPolicy(
            uint MitigationPolicy,
            ref PROCESS_MITIGATION_REDIRECTION_TRUST_POLICY lpBuffer,
            uint dwSize);

        private static bool _SetProcessMitigationPolicy(
            uint MitigationPolicy,
            ref PROCESS_MITIGATION_REDIRECTION_TRUST_POLICY lpBuffer,
            uint dwSize)
        {
            bool rst = SetProcessMitigationPolicy(MitigationPolicy, ref lpBuffer, dwSize);

            if (!rst) 
            {
#if DEBUG
                Console.WriteLine("[ProcessMitigationPolicy] SetProcessMitigationPolicy Failed");
#endif
            }

            return rst;
        }

        // Constant for the ProcessRedirectionTrustPolicy Mitigation Policy
        private const uint ProcessRedirectionTrustPolicy = 0x10;
        private const uint EnforceRedirectionTrust = 0x01;

        public static void ApplySystemProcessPolicy()
        {
            PROCESS_MITIGATION_REDIRECTION_TRUST_POLICY signature = new PROCESS_MITIGATION_REDIRECTION_TRUST_POLICY
            {
                Flags = EnforceRedirectionTrust
            };

            // Call SetProcessMitigationPolicy with the structure
            int size = Marshal.SizeOf(typeof(PROCESS_MITIGATION_REDIRECTION_TRUST_POLICY));
            if (!_SetProcessMitigationPolicy(ProcessRedirectionTrustPolicy, ref signature, (uint)size))
            {
                int errorCode = Marshal.GetLastWin32Error();
#if DEBUG
                Console.WriteLine("Failed to set process mitigation policy :" + errorCode);
#endif
                return;
            }
        }
    }
}
