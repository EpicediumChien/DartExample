using Dell.Client.Framework.Common;
using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace DDPM.SA.Common.Settings
{
    internal class SymlinkHelper
    {
        private static Log _log;
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
            //0906 Elsa Add Security
            string FileInfo;
            if (!DDPMFileSecurity.IsFilePathValid(path, out FileInfo))
            {
                _log.Info($"{nameof(GetTargetPath)} {FileInfo}");
                return null;
            }
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

        public static bool RemoveFileSymlink(string path, out string info)
        {
            //0903 Elsa Add Security
            string FileInfo;
            if (!DDPMFileSecurity.IsFilePathValid(path, out FileInfo))
            {
                info = $"{nameof(RemoveFileSymlink)} {FileInfo}";
                _log.Info(info);
                return false;
            }
            while (true)
            {
                try
                {
                    var targetPath = SymlinkHelper.GetTargetPath(path);
                    if (targetPath == null)
                        break;
                    Console.WriteLine($"Symlink target path: {targetPath}, from: {path}");
                    System.IO.File.Delete(targetPath);
                }
                catch (Exception ex)
                {
                    info = ex.Message;
                    return false;
                }
            }
            info = "Success";
            return true;
        }

        public static bool RemoveFolderSymlink(string path, out string info)
        {
            //0906 Elsa Add Security
            string FileInfo;
            if (!DDPMFileSecurity.IsFilePathValid(path, out FileInfo))
            {
                info = $"{nameof(RemoveFolderSymlink)} {FileInfo}";
                _log.Info(info);
                return false;
            }
            while (true)
            {
                try
                {
                    var targetPath = SymlinkHelper.GetTargetPath(path);
                    if (targetPath == null)
                        break;
                    Console.WriteLine($"Symlink target path: {targetPath}, from: {path}");
                    System.IO.Directory.Delete(targetPath);
                }
                catch (Exception ex)
                {
                    info = ex.Message;
                    return false;
                }
            }
            info = "Success";
            return true;
        }

        public static bool IsFileHasSymlink(string path, out string info)
        {
            info = $"File {path} has symlink";
            //0906 Elsa Add Security
            string FileInfo;
            if (!DDPMFileSecurity.IsFilePathValid(path, out FileInfo))
            {
                info = $"{nameof(IsFileHasSymlink)} {FileInfo}";
                _log.Info(info);
                return false;
            }
            try
            {
                FileInfo file = new FileInfo(path);
                if (file.LinkTarget != null)
                    return true;

                info = $"File {path} has no symlink";
            }
            catch (Exception ex)
            {
                info = ex.Message;
            }
            return false;
        }

        public static bool IsFolderHasSymlink(string path, out string info)
        {
            info = $"Folder {path} has symlink";
            //0906 Elsa Add Security
            string FileInfo;
            if (!DDPMFileSecurity.IsFilePathValid(path, out FileInfo))
            {
                info = $"{nameof(IsFolderHasSymlink)} {FileInfo}";
                _log.Info(info);
                return false;
            }
            try
            {
                DirectoryInfo folder = new DirectoryInfo(path);
                if (folder.LinkTarget != null)
                    return true;

                info = $"Folder {path} has no symlink";
            }
            catch (Exception ex)
            {
                info = ex.Message;
            }
            return false;
        }
    }
}