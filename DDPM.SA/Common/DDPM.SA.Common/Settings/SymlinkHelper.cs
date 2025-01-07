using Dell.Client.Framework.Security.Interfaces;
using Dell.Client.Framework.Security;
using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Diagnostics;

namespace DDPM.SA.Common.Settings
{
    internal class SymlinkHelper
    {
        //private static Log _log;
        private static readonly IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);
        private const uint FILE_READ_EA = 0x0008;
        private const uint FILE_FLAG_BACKUP_SEMANTICS = 0x2000000;

        [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern uint GetFinalPathNameByHandle(IntPtr hFile, StringBuilder lpszFilePath, uint cchFilePath, uint dwFlags);

        private static uint _GetFinalPathNameByHandle(IntPtr hFile, StringBuilder lpszFilePath, uint cchFilePath, uint dwFlags)
        {
            return GetFinalPathNameByHandle(hFile, lpszFilePath, cchFilePath, dwFlags);
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern bool CloseHandle(IntPtr hObject);

        private static bool _CloseHandle(IntPtr hObject)
        {
            return CloseHandle(hObject);
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern IntPtr CreateFile(
            [MarshalAs(UnmanagedType.LPTStr)] string filename,
            [MarshalAs(UnmanagedType.U4)] uint access,
            [MarshalAs(UnmanagedType.U4)] FileShare share,
            IntPtr securityAttributes,
            FileMode creationDisposition,
            uint flagsAndAttributes,
            IntPtr templateFile);

        private static IntPtr _CreateFile(
            string filename,
            uint access,
            FileShare share,
            IntPtr securityAttributes,
            FileMode creationDisposition,
            uint flagsAndAttributes,
            IntPtr templateFile)
        {
            return CreateFile(filename, access, share, securityAttributes, creationDisposition, flagsAndAttributes, templateFile);
        }

        public static string GetTargetPath(string path)
        {
            //Dean 0911: basic function, no function mix with other file checking
            //Elsa Add Security
            //string FileInfo;
            //if (!DDPMFileSecurity.IsFilePathValid(path, out FileInfo))
            //{
            //    _log.Info($"{nameof(GetTargetPath)} {FileInfo}");
            //    return null;
            //}
            var handle = _CreateFile(path, FILE_READ_EA, FileShare.ReadWrite | FileShare.Delete, IntPtr.Zero, FileMode.Open, FILE_FLAG_BACKUP_SEMANTICS, IntPtr.Zero);
            if (handle == INVALID_HANDLE_VALUE)
                throw new Win32Exception();

            try
            {
                var sb = new StringBuilder(1024);
                var result = _GetFinalPathNameByHandle(handle, sb, 1024, 0);
                if (result == 0)
                    throw new Win32Exception();

                return sb.ToString();
            }
            catch
            {
            }
            finally
            {
                _CloseHandle(handle);
            }
            return null;
        }

        /*public static bool RemoveFolderSymlink2(string path, out string info)
        {
            try
            {
                var targetPath = SymlinkHelper.GetTargetPath(path);
                if (targetPath != null)
                {
#if DEBUG
                    Console.WriteLine($"Symlink target path: {targetPath}, from: {path}");
#endif
                    System.IO.Directory.Delete(path);
                }
            }
            catch (Exception ex)
            {
                info = ex.Message;
                return false;
            }

            info = "Success";
            return true;
        }*/

        public static bool IsFilePathHasSymlink(string path, out string info, PathCheckOption option = PathCheckOption.None)
        {
            info = string.Empty;// $"Folder {path} has symlink";

            try
            {
                // Check our path string for invalid characters, null value, empty value, etc.
                if (PathHelper.ValidateFilePath(path, option) != PathCheckErrorCodes.SUCCESS)
                {
                    info = $"Invalid file path string - {path}";
                    Debug.WriteLine(info);
                    return true;
                }

                //Check for path redirection (symlink, mountpoint, hardlink, etc.) at the path AND along the path
                if (PathHelper.CheckPathRedirection(path) != PathRedirectionReturn.PathIsNormal)
                {
                    info = $"Redirection detected along file path - {path}";
                    return true;
                }

                info = $"File {path} has no symlink";
            }
            catch (Exception ex)
            {
                info = "Verify path failed. " + ex.Message;
                return true; //recognize the exception as abnormal condition
            }
            return false;
        }

        public static bool IsFolderHasSymlink(string path, out string info)
        {
            info = string.Empty;// $"Folder {path} has symlink";

            try
            {
                // Check our path string for invalid characters, null value, empty value, etc.
                if (PathHelper.ValidateDirectoryPath(path, PathCheckOption.None) != PathCheckErrorCodes.SUCCESS)
                {
                    info = $"Invalid folder path string - {path}";
                    return true;
                }

                //Check for path redirection (symlink, mountpoint, hardlink, etc.) at the path AND along the path
                if (PathHelper.CheckPathRedirection(path) != PathRedirectionReturn.PathIsNormal)
                {
                    info = $"Redirection detected along folder path - {path}";
                    return true;
                }

                info = $"Folder {path} has no symlink";
            }
            catch (Exception ex)
            {
                info = "Verify folder failed. " + ex.Message;
                return true; //recognize the exception as abnormal condition
            }
            return false;
        }
    }
}
