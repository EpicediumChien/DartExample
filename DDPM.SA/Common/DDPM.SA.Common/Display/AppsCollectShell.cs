using DDPM.SA.Common.Settings;
using Microsoft.WindowsAPICodePack.Shell;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Windows;

namespace DDPM.SA.Common
{
    [Serializable]
    public struct AppItemInfo
    {
        public string AppName;

        public string AppExeName;

        public DateTime InstalledDate;

        public string PathArgument;

        public string AppUserModelID;
    }

    public class AutomodeAppInfo
    {
        public int AppId { get; set; }

        public string AppName { get; set; }

        public string AppIcon { get; set; }

        public string AppPth { get; set; }

        public DateTime ModifyDate { get; set; }

        public bool bSelect { get; set; }

        public AutomodeAppInfo(int id, string name, string icon, string pth, DateTime Date, bool bSel)
        {
            AppId = id;
            AppName = name;
            AppIcon = icon;
            AppPth = pth;
            bSelect = bSel;
            ModifyDate = Date;
        }
    }

    public class Serialization
    {
        private static readonly JsonSerializerSettings settings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.All
        };

        public static string Serialize(object obj)
        {
            if (obj == null)
            {
                return "";
            }

            return JsonConvert.SerializeObject(obj, settings);
        }

        public static T Deserialize<T>(string jsonString)
        {
            return JsonConvert.DeserializeObject<T>(jsonString, settings);
        }
    }

    public class AppListDictionary
    {
        private static readonly string storageFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Dell\\Dell Display and Peripheral Manager";

        private static string user = Environment.UserName;// Dean 0626 SAST issue

        private static readonly string fileName = "InstalledAppInfo.json";// Dean 0626 SAST issue

        //////public static string DatafilePath = storageFolder + "\\DellDDM\\" + user + "\\" + fileName;
        private static string DatafilePath = storageFolder + "\\AppLibrary\\" + fileName; // Dean 0626 SAST issue

        private static readonly string iconFolder = storageFolder + "\\AppLibrary\\Icons";// Dean 0626 SAST issue

        private Thread ThthSaveAppDataFile;

        private static AppListDictionary INSTANCE = null;

        public Dictionary<string, InstalledAppInfo> AppInstallsList { get; set; } = new Dictionary<string, InstalledAppInfo>();

        public static AppListDictionary GetInstance()
        {
            if (INSTANCE == null)
            {
                INSTANCE = new AppListDictionary();
                INSTANCE.AppInstallsList = new Dictionary<string, InstalledAppInfo>();
            }

            DatafilePath = storageFolder + "\\" + user + "\\" + fileName;
            return INSTANCE;
        }

        public void SaveInstalledAppInfo_Thread()
        {
            ThthSaveAppDataFile = new Thread(SaveInstalledAppInfo);
            ThthSaveAppDataFile.Start();
        }

        public void SaveInstalledAppInfo()
        {
            try
            {
                string text = Serialization.Serialize(AppInstallsList);
                if (text.Length > 0)
                {
                    File.WriteAllText(DatafilePath, text);
                }
            }
            catch (Exception)
            {
            }
        }

        public void LoadFile()
        {
            try
            {
                if (!File.Exists(DatafilePath))
                {
                    return;
                }

                using StreamReader streamReader = new StreamReader(DatafilePath);
                string text = streamReader.ReadToEnd();
                if (text.Length > 0)
                {
                    AppInstallsList = JsonConvert.DeserializeObject<Dictionary<string, InstalledAppInfo>>(text);
                }
            }
            catch (Exception)
            {
            }
        }
    }

    public class AppsCollectShell
    {
        private static string RootColorPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Dell\\Dell Display and Peripheral Manager\\AppLibrary";

        //private static string IconFolder = RootColorPath + Environment.UserName + "\\Icon\\";
        private static string IconFolder = RootColorPath + "\\Icons\\";

        /* KNOWNFOLDERID: From MSDN https://learn.microsoft.com/en-us/windows/win32/shell/knownfolderid */
        public static readonly Guid FOLDERID_AppsFolder = new Guid("1e87508d-89c2-42f0-8a7e-645a0f50ca58");

        private Dictionary<string, InstalledAppInfo> AppInstallsDic = null;// new Dictionary<string, InstalledAppInfo>();

        private List<AutomodeAppInfo> AutomodeAppInfos = new List<AutomodeAppInfo>();

        private AppListDictionary tmpAppListDictionary = AppListDictionary.GetInstance();

        public AppsCollectShell()
        {
            //use default Icon folder
        }

        public AppsCollectShell(string icon_folder)
        {
            //update icon folder
            if (!string.IsNullOrEmpty(icon_folder))
                IconFolder = icon_folder;
        }

        private string CheckFileNameValid(string filename)
        {
            if (filename.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            {
                filename = filename.Replace("//", "");
                filename = filename.Replace("\\", "");
                filename = filename.Replace(":", "");
                filename = filename.Replace("*", "");
                filename = filename.Replace("?", "");
                filename = filename.Replace("\"", "");
                filename = filename.Replace(">", "");
                filename = filename.Replace("<", "");
                filename = filename.Replace("|", "");
            }
            return filename;
        }

        public Dictionary<string, InstalledAppInfo> FindAppsbyShell()// ref Dictionary<string, InstalledAppInfo> installedApp)
        {
            //logger.SetLogModule("ColorApp");

            Dictionary<string, InstalledAppInfo> installedApp = new Dictionary<string, InstalledAppInfo>();
            //logger.WriteLog($"[ColorApp][FindAppsbyShell] App Icon folder: {IconFolder}");
            string folderInfo = string.Empty, info = string.Empty;
            if (!DDPMFileSecurity.CheckFold(IconFolder, out folderInfo, out info))
                return installedApp;

            if (!System.IO.Directory.Exists(IconFolder))
                System.IO.Directory.CreateDirectory(IconFolder);

            Dictionary<string, List<AppItemInfo>> dictionary = new Dictionary<string, List<AppItemInfo>>();
            IKnownFolder ikf = null;
            try
            {
                ikf = KnownFolderHelper.FromKnownFolderId(FOLDERID_AppsFolder);
            }
            catch (ArgumentException)// ae)
            {
                //logger.WriteLog($"[ColorApp][FindAppsbyShell] try to query [FOLDERID_AppsFolder], exception: {ae.Message}");
                return installedApp;
            }
            if (ikf == null)
            {
                //logger.WriteLog($"[ColorApp][FindAppsbyShell] KnownFolderHelper.FromKnownFolderId got null return");
                return installedApp;
            }

            //logger.WriteLog($"[ColorApp][FindAppsbyShell] Step ShellObject loop, count:{ikf.ToList().Count}");
            foreach (ShellObject item in (IKnownFolder)(ShellObject)ikf)
            {
                string name = string.IsNullOrEmpty(item.Name) ? string.Empty : item.Name;
                string parsingName = string.IsNullOrEmpty(item.ParsingName) ? string.Empty : item.ParsingName;
                string value = string.Empty;// item.Properties.System.Link.TargetParsingPath.Value;
                string value2 = string.Empty;// item.Properties.System.Link.Arguments.Value;
                try
                {
                    value = item.Properties.System.Link.TargetParsingPath.Value;
                    value2 = item.Properties.System.Link.Arguments.Value;
                }
                catch (Exception)// ex)
                {
                }

                //
                // Desktop application parsing
                //
                if (value != null && value.Length > 0)
                {
                    value = value.ToLower();
                    string text = value.Split('\\')[^1].ToLower();
                    if (!text.ToLower().Contains("exe"))
                    {
                        //logger.WriteLog($"[ColorApp][FindAppsbyShell] Desktop:({value}), not end with exe, next loop");
                        continue;
                    }
                    try
                    {
                        System.IO.FileInfo f = new System.IO.FileInfo(value);
                        DateTime lastAccessTime = f.CreationTime;//.LastAccessTime;
                        if (!File.Exists(IconFolder + text + ".png"))
                        {
                            System.Drawing.Icon.ExtractAssociatedIcon(value)!.ToBitmap().Save(IconFolder + text + ".png");
                            //logger.WriteLog($"[ColorApp][FindAppsbyShell] Save icon to [{IconFolder}{text}.png] (Desktop)");
                        }
                        if (!dictionary.ContainsKey(value))
                        {
                            dictionary.Add(value, new List<AppItemInfo>
                                                    {
                                                        new AppItemInfo
                                                        {
                                                            AppName = name,
                                                            AppExeName = text,
                                                            InstalledDate = lastAccessTime,
                                                            PathArgument = value2,
                                                            AppUserModelID = parsingName
                                                        }
                                                    }
                            );
                            //logger.WriteLog($"[ColorApp][FindAppsbyShell] Add installed app AppName[{name}]AppExeName[{text}]Date[{lastAccessTime}]ModelID[{parsingName}]");
                        }
                        else
                        {
                            dictionary[value].Add(new AppItemInfo
                            {
                                AppName = name,
                                AppExeName = text,
                                InstalledDate = lastAccessTime,
                                PathArgument = value2,
                                AppUserModelID = parsingName
                            });
                            //logger.WriteLog($"[ColorApp][FindAppsbyShell] Add installed app Exist[{value}]: AppName[{name}]AppExeName[{text}]Date[{lastAccessTime}]ModelID[{parsingName}]");
                        }
                    }
                    catch (Exception)// ex1)
                    {
                        //logger.WriteLog($"[ColorApp][FindAppsbyShell] Desktop:({ex1.Message})");
                    }
                    continue;
                }
                else
                {
                    //logger.WriteLog($"[ColorApp][FindAppsbyShell] item:({item}), got null [item.Properties.System.Link.TargetParsingPath.Value], not desktop app");
                }
                //
                // UWP application parsing
                //
                Bitmap bitmap = null;
                try
                {
                    string filename = name.ToLower();
                    filename = CheckFileNameValid(filename);
                    string text2 = item.Properties.GetProperty("System.AppUserModel.PackageInstallPath")?.ValueAsObject?.ToString();
                    bool installed_uwp = false;

                    if (!File.GetAttributes(text2).HasFlag(FileAttributes.Directory) || installed_uwp || Directory.GetFiles(text2, "*.exe").Length != 0)
                    {
                        DateTime now = DateTime.Now;
                        //Find uwp app installed date

                        System.Windows.Media.Imaging.BitmapSource bitmapSource = item.Thumbnail.ExtraLargeBitmapSource;
                        bitmap = new Bitmap(bitmapSource.PixelWidth, bitmapSource.PixelHeight, PixelFormat.Format32bppPArgb);
                        BitmapData bitmapData = bitmap.LockBits(new Rectangle(System.Drawing.Point.Empty, bitmap.Size), ImageLockMode.WriteOnly, PixelFormat.Format32bppPArgb);
                        bitmapSource.CopyPixels(Int32Rect.Empty, bitmapData.Scan0, bitmapData.Height * bitmapData.Stride, bitmapData.Stride);
                        bitmap.UnlockBits(bitmapData);
                        if (!File.Exists(IconFolder + filename + ".png"))
                        {
                            bitmap.Save(IconFolder + filename + ".png");
                            //logger.WriteLog($"[ColorApp][FindAppsbyShell] Save icon to [{IconFolder}{filename}.png] (UWP)");
                        }
                        if (!installedApp.ContainsKey(text2))
                        {
                            installedApp.Add(text2, new InstalledAppInfo(name, text2, filename, now, bDesktopApp: false, parsingName));
                            //logger.WriteLog($"[ColorApp][FindAppsbyShell] Add installed app AppName[{name}]AppExeName[{text2}]Date[{now}]ModelID[{parsingName}]");
                        }
                    }
                }
                catch (Exception)// ex2)
                {
                    //logger.WriteLog($"[ColorApp][FindAppsbyShell] UWP:({ex2.Message})");
                }
                finally
                {
                    bitmap?.Dispose();
                }
            }
            try
            {
                foreach (KeyValuePair<string, List<AppItemInfo>> item2 in dictionary)
                {
                    List<AppItemInfo> value3 = item2.Value;
                    if (value3.Count == 1)
                    {
                        if (!installedApp.ContainsKey(item2.Key))
                        {
                            installedApp.Add(item2.Key, new InstalledAppInfo(value3[0].AppName, item2.Key, value3[0].AppExeName, value3[0].InstalledDate, bDesktopApp: true, value3[0].AppUserModelID));
                        }
                    }
                    else
                    {
                        if (value3.Count < 1)
                        {
                            continue;
                        }
                        List<AppItemInfo> list = value3.FindAll((AppItemInfo x) => string.IsNullOrEmpty(x.PathArgument));
                        if (list.Count >= 1 && !installedApp.ContainsKey(item2.Key))
                        {
                            installedApp.Add(item2.Key, new InstalledAppInfo(list[0].AppName, item2.Key, list[0].AppExeName, list[0].InstalledDate, bDesktopApp: true, value3[0].AppUserModelID));
                            continue;
                        }
                        list = value3.FindAll((AppItemInfo x) => !x.PathArgument.Contains("url"));
                        if (list.Count > 0 && !installedApp.ContainsKey(item2.Key))
                        {
                            installedApp.Add(item2.Key, new InstalledAppInfo(list[0].AppName, item2.Key, list[0].AppExeName, list[0].InstalledDate, bDesktopApp: true, value3[0].AppUserModelID));
                        }
                    }
                }
            }
            catch (Exception)// ex3)
            {
                //logger.WriteLog($"[ColorApp][FindAppsbyShell] Merge:({ex3.Message})");
            }
            AppListDictionary.GetInstance().LoadFile();
            tmpAppListDictionary = AppListDictionary.GetInstance();
            foreach (string key in installedApp.Keys)
            {
                if (!tmpAppListDictionary.AppInstallsList.ContainsKey(key))
                {
                    tmpAppListDictionary.AppInstallsList.Add(key, installedApp[key]);
                }
            }
            foreach (string key2 in tmpAppListDictionary.AppInstallsList.Keys)
            {
                if (installedApp.ContainsKey(key2))
                {
                    continue;
                }
                try
                {
                    if (tmpAppListDictionary.AppInstallsList[key2].isDesktopApp)
                    {
                        if (!File.Exists(key2))
                        {
                            tmpAppListDictionary.AppInstallsList.Remove(key2);
                        }
                    }
                    else if (!Directory.Exists(key2))
                    {
                        tmpAppListDictionary.AppInstallsList.Remove(key2);
                    }
                }
                catch
                {
                }
            }
            tmpAppListDictionary.SaveInstalledAppInfo_Thread();

            return installedApp;
        }

        public Dictionary<string, InstalledAppInfo> FindAppsbyShellForEzMemoryFullPathKey()// ref Dictionary<string, InstalledAppInfo> installedApp)
        {
            //logger.SetLogModule("ColorApp");

            Dictionary<string, InstalledAppInfo> installedApp = new Dictionary<string, InstalledAppInfo>();
            //logger.WriteLog($"[ColorApp][FindAppsbyShell] App Icon folder: {IconFolder}");
            string folderInfo = string.Empty, info = string.Empty;
            if (!DDPMFileSecurity.CheckFold(IconFolder, out folderInfo, out info))
                return installedApp;
            if (!System.IO.Directory.Exists(IconFolder))
                System.IO.Directory.CreateDirectory(IconFolder);

            Dictionary<string, List<AppItemInfo>> dictionary = new Dictionary<string, List<AppItemInfo>>();
            IKnownFolder ikf = null;
            try
            {
                ikf = KnownFolderHelper.FromKnownFolderId(FOLDERID_AppsFolder);
            }
            catch (ArgumentException)// ae)
            {
                //logger.WriteLog($"[ColorApp][FindAppsbyShell] try to query [FOLDERID_AppsFolder], exception: {ae.Message}");
                return installedApp;
            }
            if (ikf == null)
            {
                //logger.WriteLog($"[ColorApp][FindAppsbyShell] KnownFolderHelper.FromKnownFolderId got null return");
                return installedApp;
            }

            //logger.WriteLog($"[ColorApp][FindAppsbyShell] Step ShellObject loop, count:{ikf.ToList().Count}");
            foreach (ShellObject item in (IKnownFolder)(ShellObject)ikf)
            {
                string name = string.IsNullOrEmpty(item.Name) ? string.Empty : item.Name;
                string parsingName = string.IsNullOrEmpty(item.ParsingName) ? string.Empty : item.ParsingName;
                string value = string.Empty;// item.Properties.System.Link.TargetParsingPath.Value;
                string value2 = string.Empty;// item.Properties.System.Link.Arguments.Value;
                try
                {
                    value = item.Properties.System.Link.TargetParsingPath.Value;
                    value2 = item.Properties.System.Link.Arguments.Value;
                }
                catch (Exception)// ex)
                {
                }

                //
                // Desktop application parsing
                //
                if (value != null && value.Length > 0)
                {
                    value = value.ToLower();
                    string text = value.Split('\\')[^1].ToLower();
                    if (!text.ToLower().Contains("exe"))
                    {
                        //logger.WriteLog($"[ColorApp][FindAppsbyShell] Desktop:({value}), not end with exe, next loop");
                        continue;
                    }
                    try
                    {
                        System.IO.FileInfo f = new System.IO.FileInfo(value);
                        DateTime lastAccessTime = f.CreationTime;//.LastAccessTime;
                        if (!File.Exists(IconFolder + text + ".png"))
                        {
                            System.Drawing.Icon.ExtractAssociatedIcon(value)!.ToBitmap().Save(IconFolder + text + ".png");
                            //logger.WriteLog($"[ColorApp][FindAppsbyShell] Save icon to [{IconFolder}{text}.png] (Desktop)");
                        }
                        if (!dictionary.ContainsKey(value))
                        {
                            dictionary.Add(value, new List<AppItemInfo>
                                                    {
                                                        new AppItemInfo
                                                        {
                                                            AppName = name,
                                                            AppExeName = text,
                                                            InstalledDate = lastAccessTime,
                                                            PathArgument = value,
                                                            AppUserModelID = parsingName
                                                        }
                                                    }
                            );
                            //logger.WriteLog($"[ColorApp][FindAppsbyShell] Add installed app AppName[{name}]AppExeName[{text}]Date[{lastAccessTime}]ModelID[{parsingName}]");
                        }
                        else
                        {
                            dictionary[value].Add(new AppItemInfo
                            {
                                AppName = name,
                                AppExeName = text,
                                InstalledDate = lastAccessTime,
                                PathArgument = value,
                                AppUserModelID = parsingName
                            });
                            //logger.WriteLog($"[ColorApp][FindAppsbyShell] Add installed app Exist[{value}]: AppName[{name}]AppExeName[{text}]Date[{lastAccessTime}]ModelID[{parsingName}]");
                        }
                    }
                    catch (Exception)// ex1)
                    {
                        //logger.WriteLog($"[ColorApp][FindAppsbyShell] Desktop:({ex1.Message})");
                    }
                    continue;
                }
                else
                {
                    //logger.WriteLog($"[ColorApp][FindAppsbyShell] item:({item}), got null [item.Properties.System.Link.TargetParsingPath.Value], not desktop app");
                }
                //
                // UWP application parsing
                //
                Bitmap bitmap = null;
                try
                {
                    string filename = name.ToLower();
                    filename = CheckFileNameValid(filename);
                    string text2 = item.Properties.GetProperty("System.AppUserModel.PackageInstallPath")?.ValueAsObject?.ToString();
                    bool installed_uwp = false;

                    if (!File.GetAttributes(text2).HasFlag(FileAttributes.Directory) || installed_uwp || Directory.GetFiles(text2, "*.exe").Length != 0)
                    {
                        DateTime now = DateTime.Now;
                        //Find uwp app installed date

                        System.Windows.Media.Imaging.BitmapSource bitmapSource = item.Thumbnail.ExtraLargeBitmapSource;
                        bitmap = new Bitmap(bitmapSource.PixelWidth, bitmapSource.PixelHeight, PixelFormat.Format32bppPArgb);
                        BitmapData bitmapData = bitmap.LockBits(new Rectangle(System.Drawing.Point.Empty, bitmap.Size), ImageLockMode.WriteOnly, PixelFormat.Format32bppPArgb);
                        bitmapSource.CopyPixels(Int32Rect.Empty, bitmapData.Scan0, bitmapData.Height * bitmapData.Stride, bitmapData.Stride);
                        bitmap.UnlockBits(bitmapData);
                        if (!File.Exists(IconFolder + filename + ".png"))
                        {
                            bitmap.Save(IconFolder + filename + ".png");
                            //logger.WriteLog($"[ColorApp][FindAppsbyShell] Save icon to [{IconFolder}{filename}.png] (UWP)");
                        }
                        if (!installedApp.ContainsKey(text2))
                        {
                            installedApp.Add(text2, new InstalledAppInfo(name, text2, filename, now, bDesktopApp: false, parsingName));
                            //logger.WriteLog($"[ColorApp][FindAppsbyShell] Add installed app AppName[{name}]AppExeName[{text2}]Date[{now}]ModelID[{parsingName}]");
                        }
                    }
                }
                catch (Exception)// ex2)
                {
                    //logger.WriteLog($"[ColorApp][FindAppsbyShell] UWP:({ex2.Message})");
                }
                finally
                {
                    bitmap?.Dispose();
                }
            }
            try
            {
                foreach (KeyValuePair<string, List<AppItemInfo>> item2 in dictionary)
                {
                    List<AppItemInfo> value3 = item2.Value;
                    if (value3.Count == 1)
                    {
                        if (!installedApp.ContainsKey(item2.Key))
                        {
                            installedApp.Add(item2.Key, new InstalledAppInfo(value3[0].AppName, item2.Key, value3[0].AppExeName, value3[0].InstalledDate, bDesktopApp: true, value3[0].AppUserModelID));
                        }
                    }
                    else
                    {
                        if (value3.Count < 1)
                        {
                            continue;
                        }
                        List<AppItemInfo> list = value3.FindAll((AppItemInfo x) => string.IsNullOrEmpty(x.PathArgument));
                        if (list.Count >= 1 && !installedApp.ContainsKey(item2.Key))
                        {
                            installedApp.Add(item2.Key, new InstalledAppInfo(list[0].AppName, item2.Key, list[0].AppExeName, list[0].InstalledDate, bDesktopApp: true, value3[0].AppUserModelID));
                            continue;
                        }
                        list = value3.FindAll((AppItemInfo x) => !x.PathArgument.Contains("url"));
                        if (list.Count > 0 && !installedApp.ContainsKey(item2.Key))
                        {
                            installedApp.Add(item2.Key, new InstalledAppInfo(list[0].AppName, item2.Key, list[0].AppExeName, list[0].InstalledDate, bDesktopApp: true, value3[0].AppUserModelID));
                        }
                    }
                }
            }
            catch (Exception)// ex3)
            {
                //logger.WriteLog($"[ColorApp][FindAppsbyShell] Merge:({ex3.Message})");
            }
            AppListDictionary.GetInstance().LoadFile();
            tmpAppListDictionary = AppListDictionary.GetInstance();
            foreach (string key in installedApp.Keys)
            {
                if (!tmpAppListDictionary.AppInstallsList.ContainsKey(key))
                {
                    tmpAppListDictionary.AppInstallsList.Add(key, installedApp[key]);
                }
            }
            foreach (string key2 in tmpAppListDictionary.AppInstallsList.Keys)
            {
                if (installedApp.ContainsKey(key2))
                {
                    continue;
                }
                try
                {
                    if (tmpAppListDictionary.AppInstallsList[key2].isDesktopApp)
                    {
                        if (!File.Exists(key2))
                        {
                            tmpAppListDictionary.AppInstallsList.Remove(key2);
                        }
                    }
                    else if (!Directory.Exists(key2))
                    {
                        tmpAppListDictionary.AppInstallsList.Remove(key2);
                    }
                }
                catch
                {
                }
            }
            tmpAppListDictionary.SaveInstalledAppInfo_Thread();

            return installedApp;
        }
    }
}