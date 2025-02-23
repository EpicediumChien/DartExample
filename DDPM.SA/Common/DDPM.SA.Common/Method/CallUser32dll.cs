using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace DDPM.SA.Common.Method
{
    public class CallUser32dll
    {
        public enum WindowState
        {
            SW_SHOWMINIMIZED = 2,
            SW_SHOWMAXIMIZED = 3,
            SW_MAXIMIZE = 3,
            SW_MINIMIZE = 6,
            SW_RESTORE = 9
        }
        [DllImport("user32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        public static bool _ShowWindow(IntPtr hWnd, int nCmdShow)
        {
            bool rst = ShowWindow(hWnd, nCmdShow);

            if(!rst)
            {
#if DEBUG
                Console.WriteLine("[CallUser32dll] ShowWindow: Window was hidden before.");
#endif
            }

            return rst;
        }

        [DllImport("user32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        public static IntPtr _FindWindow(string lpClassName, string lpWindowName)
        {
            IntPtr rst = FindWindow(lpClassName, lpWindowName);

            if (rst == IntPtr.Zero)
            {
#if DEBUG
                Console.WriteLine("[CallUser32dll] FindWindow failed.");
#endif
            }

            return rst;
        }

        // << 250102 added by Hess to change MousePrimaryButton
        private const uint SPI_SETMOUSEBUTTONSWAP = 0x0021;
        private const uint SPIF_UPDATEINIFILE = 0x0001;
        private const uint SPIF_SENDCHANGE = 0x0002;
        private const int SM_SWAPBUTTON = 0x0017;
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern int GetSystemMetrics(int nIndex);
        private static int _GetSystemMetrics(int nIndex)
        {
            int rst = GetSystemMetrics(nIndex);

            if (rst == 0)
            {
#if DEBUG
                Console.WriteLine("[CallUser32dll] GetSystemMetrics failed.");
#endif
            }

            return rst;
        }

        [DllImport("user32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern bool SystemParametersInfo(uint uiAction, uint uiParam, bool pvParam, uint fWinIni);
        private static bool _SystemParametersInfo(uint uiAction, uint uiParam, bool pvParam, uint fWinIni)
        {
            bool rst = SystemParametersInfo(uiAction, uiParam, pvParam, fWinIni);

            if (!rst)
            {
#if DEBUG
                Console.WriteLine("[CallUser32dll] SystemParametersInfo failed.");
#endif
            }

            return rst;
        }

        //DDPM.UI reference this function
        public static bool IsPrimaryButtonLeft()
        {
            var value = _GetSystemMetrics(SM_SWAPBUTTON);
            return value == 0;
        }
        public static void SetPrimaryButtonToLeft(bool isLeftPrimary)
        {
            _SystemParametersInfo(SPI_SETMOUSEBUTTONSWAP, (uint)(isLeftPrimary ? 0 : 1), false, SPIF_UPDATEINIFILE | SPIF_SENDCHANGE);
        }
        // >>
    }
}
