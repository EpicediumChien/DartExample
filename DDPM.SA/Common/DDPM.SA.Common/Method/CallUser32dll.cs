using System;
using System.Collections.Generic;
using System.Linq;
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
            return ShowWindow(hWnd, nCmdShow);
        }

        [DllImport("user32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        public static IntPtr _FindWindow(string lpClassName, string lpWindowName)
        {
            return FindWindow(lpClassName, lpWindowName);
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
            return GetSystemMetrics(nIndex);
        }

        [DllImport("user32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern bool SystemParametersInfo(uint uiAction, uint uiParam, bool pvParam, uint fWinIni);
        private static bool _SystemParametersInfo(uint uiAction, uint uiParam, bool pvParam, uint fWinIni)
        {
            return SystemParametersInfo(uiAction, uiParam, pvParam, fWinIni);
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
