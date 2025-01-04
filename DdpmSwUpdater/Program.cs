using DDPM.SA.Common;
using DDPM.SA.Common.Method;
using DDPM.SA.Common.Settings;
using DDPM.SA.Resources.Helper;
using DdpmSwUpdater;
using Dell.Client.Framework.Common;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows.Media.Animation;
using Windows.Devices.Geolocation;
internal class Program
{
    private static void Main(string[] args)
    {
        LogManage.SetPath();
        if (ProcessSecurityHelperWrapper.IsCurrentProcessRunningElevated())
        {
            if (args.Length > 0)
            {
                LogManage.LogMessage($"args.Length > 0 go to preset process");
                if (args[0].ToLower().Equals("/fromddpm") || args[0].ToLower().Equals("/fromddm"))
                {
                    if (args[0].ToLower().Equals("/fromddpm"))
                    {
                        LogManage.fromDDPM = true;
                        LogManage.LogMessage($"Is DDPM call");
                    }
                    else
                    {
                        LogManage.fromDDPM = false;
                        LogManage.LogMessage($"Is DDM call");
                    }
                    string registryKey = @"SOFTWARE\Dell\Dell Display and Peripheral Manager";
                    ///Bruce added Test///
                    string registryKey_test = @"SOFTWARE\Dell Display and Peripheral Manager";
                    try
                    {
                        string folderName = FindParentFolderName();
                        if (!string.IsNullOrEmpty(folderName))
                        {
                            DDPMRegistryHelper.WriteRegistryKey(RegistryHive.LocalMachine, registryKey, "DdpmSwUpdater", folderName);
                            DDPMRegistryHelper.WriteRegistryKey(RegistryHive.LocalMachine, registryKey_test, "DdpmSwUpdater", folderName);
                        }
                    }
                    catch (Exception ex) { LogManage.LogMessage($"DDPMRegistryHelper.WriteRegistryKey error : {ex.Message}"); }
                    LaunchInstaller launchInstaller = new LaunchInstaller();
                    SWUErrorCode ret = launchInstaller.LaunchUpdate().Result;
                    LogManage.LogMessage($"DdpmSwUpdater End Resule:{ret.ToString()}");
                    try
                    {
                        object o = DDPMRegistryHelper.ReadRegistryKey(RegistryHive.LocalMachine, registryKey, "SW_Available_date");
                        LogManage.LogMessage($"DdpmSwUpdater ,ReadRegistryData SW_Available_date o = {o}.");
                        if (o != null && o is string && !string.IsNullOrEmpty(o.ToString()))
                        {
                            string UpdateVersion = LogManage.Version;
                            string Results = ret == SWUErrorCode.NoError ? "success" : "failed";
                            string FailureMessage = ret.ToString();
                            string SW_Update_date = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
                            string ErrorCode = ((int)ret).ToString();
                            LogManage.LogMessage($"WriteRegistryKey UpdateVersion : {UpdateVersion}");
                            LogManage.LogMessage($"WriteRegistryKey Results : {Results}");
                            LogManage.LogMessage($"WriteRegistryKey FailureMessage : {FailureMessage}");
                            LogManage.LogMessage($"WriteRegistryKey SW_Update_date : {SW_Update_date}");
                            LogManage.LogMessage($"WriteRegistryKey ErrorCode : {ErrorCode}");
                            DDPMRegistryHelper.WriteRegistryKey(RegistryHive.LocalMachine, registryKey, nameof(UpdateVersion), UpdateVersion);
                            DDPMRegistryHelper.WriteRegistryKey(RegistryHive.LocalMachine, registryKey, nameof(Results), Results);
                            DDPMRegistryHelper.WriteRegistryKey(RegistryHive.LocalMachine, registryKey, nameof(FailureMessage), FailureMessage);
                            DDPMRegistryHelper.WriteRegistryKey(RegistryHive.LocalMachine, registryKey, nameof(SW_Update_date), SW_Update_date);
                            DDPMRegistryHelper.WriteRegistryKey(RegistryHive.LocalMachine, registryKey, nameof(ErrorCode), ErrorCode);
                            ///Bruce added Test///
                            DDPMRegistryHelper.WriteRegistryKey(RegistryHive.LocalMachine, registryKey_test, nameof(UpdateVersion), UpdateVersion);
                            DDPMRegistryHelper.WriteRegistryKey(RegistryHive.LocalMachine, registryKey_test, nameof(Results), Results);
                            DDPMRegistryHelper.WriteRegistryKey(RegistryHive.LocalMachine, registryKey_test, nameof(FailureMessage), FailureMessage);
                            DDPMRegistryHelper.WriteRegistryKey(RegistryHive.LocalMachine, registryKey_test, nameof(SW_Update_date), SW_Update_date);
                            DDPMRegistryHelper.WriteRegistryKey(RegistryHive.LocalMachine, registryKey_test, nameof(ErrorCode), ErrorCode);
                            ///Bruce added Test///
                        }
                    }
                    catch (Exception ex) { LogManage.LogMessage($"DDPMRegistryHelper.WriteRegistryKey error : {ex.Message}"); }
                }
                else
                {
                    LogManage.LogMessage($"args is no Equals. args is : {args[0]}");
                }
            }
            else
            {
                LogManage.fromDDPM = false;
                LogManage.LogMessage($"args.Length <= 0 go to copy");
                SWUpdatePlugins swUpdatePlugins = new SWUpdatePlugins();
                swUpdatePlugins.DownloadAndExecutionSwUpdater();
                //string exeFilePath = CopyToProgram();
                //if (!string.IsNullOrEmpty(exeFilePath))
                //{
                //    ProcessStartInfo startInfo = new ProcessStartInfo()
                //    {
                //        UseShellExecute = false,
                //        FileName = exeFilePath,//fileFullPath,
                //        Arguments = "/fromddm"
                //    };
                //    Process clientProcess = new Process();
                //    clientProcess.StartInfo = startInfo;
                //    clientProcess.Start();
                //}
            }
        }
        else
        {
            LogManage.LogMessage($"Current process is not run elevated");
        }
    }
    static string CopyToProgram()
    {
        string path_programdata = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
        string saveFolderName = Guid.NewGuid().ToString();
        string savePath = string.Empty;
        DDPMFileSecurity DDPMFileSecurity = new DDPMFileSecurity();
        if (!string.IsNullOrEmpty(path_programdata))
        {
            savePath = path_programdata + "\\Dell" + "\\" + saveFolderName + "\\";
            if (!Directory.Exists(savePath))
            {
                Directory.CreateDirectory(savePath);
            }
            if (DDPMFileSecurity.CheckFold(savePath, out string FolderInfo, out string PathSymbolicLinInfo))
            {
                // 取得目前執行檔案的所在資料夾
                string? currentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                if (!string.IsNullOrEmpty(currentDirectory))
                {
                    Method method = new Method(LogManage.logs);
                    method.CopyLogFolder(currentDirectory, savePath);
                    method.Dispose();
                    string? exePath = Process.GetCurrentProcess().MainModule?.FileName;
                    if (!string.IsNullOrEmpty(exePath))
                    {
                        string exeFileName = Path.GetFileName(exePath);
                        return savePath + "\\" + exeFileName;
                    }
                }
            }
            else
            {
                LogManage.LogMessage($"{nameof(CopyToProgram)} savePath FolderIsNotSafe : {FolderInfo}--or--{PathSymbolicLinInfo}");
            }
        }
        else
        {
            LogManage.LogMessage($"{nameof(CopyToProgram)} path_programdata get error");
        }
        return "";
    }
    static string FindParentFolderName()
    {
        string folderName = string.Empty;
        try
        {
            // 取得執行檔所在的資料夾
            string exePath = AppDomain.CurrentDomain.BaseDirectory;
            // 取得執行檔的名稱（不包含副檔名）
            string exeName = Path.GetFileNameWithoutExtension(System.Reflection.Assembly.GetExecutingAssembly().Location);
            string currentFolder = exePath;
            while (true)
            {
                currentFolder = Path.TrimEndingDirectorySeparator(currentFolder);
                // 取得目前資料夾的名稱
                folderName = Path.GetFileName(currentFolder);
                LogManage.LogMessage($"currentFolder folder name {folderName}");
                // 如果資料夾名稱與執行檔名稱不同，則輸出此資料夾名稱並結束
                if (!string.IsNullOrEmpty(folderName) && folderName != exeName)
                {
                    LogManage.LogMessage($"finded folder name {folderName}");
                    break;
                }
                string? parentFolder = Directory.GetParent(currentFolder)?.FullName;
                // 如果到達最源頭則結束搜尋
                if (string.IsNullOrEmpty(parentFolder) || parentFolder == currentFolder)
                {
                    LogManage.LogMessage("FindParentFolderName no matching sources found");
                    break;
                }
                // 更新為父資料夾，繼續向上搜尋
                currentFolder = parentFolder;
            }
        }
        catch(Exception ex)
        {
            LogManage.LogMessage($"FindParentFolderName Error : {ex.Message}");
        }
        return folderName;
    }
}