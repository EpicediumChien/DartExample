using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace DDPM.SA.Common.Settings
{
    internal class SymlinkHelper
    {
        private static readonly IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);
        private const uint FILE_READ_EA = 0x0008;
        private const uint FILE_FLAG_BACKUP_SEMANTICS = 0x2000000;

        [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern uint GetFinalPathNameByHandle(IntPtr hFile, StringBuilder lpszFilePath, uint cchFilePath, uint dwFlags);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr CreateFile(
            [MarshalAs(UnmanagedType.LPTStr)] string filename,
            [MarshalAs(UnmanagedType.U4)] uint access,
            [MarshalAs(UnmanagedType.U4)] FileShare share,
            IntPtr securityAttributes,
            FileMode creationDisposition,
            uint flagsAndAttributes,
            IntPtr templateFile);

        public static string GetTargetPath(string path)
        {
            var handle = CreateFile(path, FILE_READ_EA, FileShare.ReadWrite | FileShare.Delete, IntPtr.Zero, FileMode.Open, FILE_FLAG_BACKUP_SEMANTICS, IntPtr.Zero);
            if (handle == INVALID_HANDLE_VALUE)
                throw new Win32Exception();

            try
            {
                var sb = new StringBuilder(1024);
                var result = GetFinalPathNameByHandle(handle, sb, 1024, 0);
                if (result == 0)
                    throw new Win32Exception();

                return sb.ToString();
            }
            catch
            {
            }
            finally
            {
                CloseHandle(handle);
            }
            return null;
        }

        public static bool RemoveFileSymlink(string path, out string info)
        {
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
            try
            {
                //var attributes = System.IO.File.GetAttributes(path);
                //if ((attributes & FileAttributes.ReparsePoint) != 0)
                //{
                //    return true;
                //}
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