using System;
using System.Runtime.InteropServices;
using System.Windows.Input;

namespace DDPM.SA.Common
{
    public class HotKeyWinApi
    {
        public const int WmHotKey = 0x0312;

        [DllImport("user32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, ModifierKeys fsModifiers, int vk);
        public static bool _RegisterHotKey(IntPtr hWnd, int id, ModifierKeys fsModifiers, int vk)
        {
            return RegisterHotKey(hWnd, id, fsModifiers, vk);
        }

        [DllImport("user32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
        public static bool _UnregisterHotKey(IntPtr hWnd, int id)
        {
            return UnregisterHotKey(hWnd, id);
        }

        //[DllImport("Kernel32.dll", SetLastError = true)]
        //public static extern ulong GetLastError();


        /*       [DllImport("kernel32.dll")]
                private static extern IntPtr GetModuleHandle(string lpFileName);

                [DllImport("user32.dll")]
                [return: MarshalAs(UnmanagedType.Bool)]
                public static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

                [DllImport("user32.dll")]
                [return: MarshalAs(UnmanagedType.Bool)]
                public static extern bool UnregisterHotKey(IntPtr hWnd, int id);*/

       /* [DllImport("kernel32.dll")]
        public static extern ushort GlobalAddAtom(string lpString);

        [DllImport("kernel32.dll")]
        public static extern ushort GlobalDeleteAtom(ushort nAtom);

        [DllImport("kernel32.dll")]
        public static extern IntPtr GetModuleHandle(string lpFileName);

        [DllImport("kernel32.dll")]
        public static extern IntPtr GetCurrentThread();

        [DllImport("Kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.SysInt)]
        public static extern IntPtr OpenThread(uint dwDesiredAccess, [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle, uint dwThreadId);*/
    }
}
