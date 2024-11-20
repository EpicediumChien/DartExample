using DDPM.SA.Common.Method;
using DDPM.SA.Obfuscation;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.Security;
using Dell.Client.Framework.Security.Interfaces;
using Dell.TechHub.Sdk.Common;
using DPeMPublic.Common;
using Microsoft.Win32;
using MS.WindowsAPICodePack.Internal;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http.Json;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.Text;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace DDPM.SA.Common.Settings
{
    public class DDPMFileSecurity
    {
        private static void WriteLog(ILog log, string message, bool isError = false)
        {
#if DEBUG
            Console.WriteLine(message);
#endif
            if (log == null)
                return;
            if (!isError)
                log.Info(message);
            else
                log.Error(message);
        }

        /*/// <summary>
        /// Using DPAPI to protect data
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static byte[] DataProtect(byte[] data)
        {
            try
            {
                return ProtectedData.Protect(data, null, DataProtectionScope.LocalMachine);
            }
            catch (CryptographicException e)
            {
#if DEBUG 
                Console.WriteLine("Data was not encrypted. An error occurred.");
                Console.WriteLine(e.ToString());
#endif
                return null;
            }
        }

        /// <summary>
        /// Using DPAPI to protect data
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static byte[] DataUnprotect(byte[] data)
        {
            try
            {
                return ProtectedData.Unprotect(data, null, DataProtectionScope.LocalMachine);
            }
            catch (CryptographicException e)
            {
#if DEBUG 
                Console.WriteLine("Data was not decrypted. An error occurred.");
                Console.WriteLine(e.ToString());
#endif
                return null;
            }
        }*/

        /// <summary>
        /// Apply DDPM data security [Write settings]
        /// 1. Calculate hash of serialized string
        /// 2. Add signature
        /// 3. Data protect if need
        /// 4. Write to file
        /// </summary>
        /// <param name="serialized_string">Please using JObject.ToString() as input</param>
        /// <param name="target_file">Describe your target file to save</param>
        /// <param name="info">Read this param for detail info if return false</param>
        /// <returns>true or false as result</returns>
        public static bool SetJsonContentFromSerializedString(string serialized_string, string target_file, out string info)//, bool isEncrypt = false)
        {
            info = "Success";
            if (string.IsNullOrEmpty(serialized_string))
            {
                info = "Null json content as input";
                return false;
            }

            //Elsa Add Security
            //Dean 0913 if file not exist, this check will cause the fail and never init
            if (File.Exists(target_file))
            {
                string FileInfo;
                if (!IsFilePathValid(target_file, out FileInfo))
                {
                    info = $"[SetJsonContentFromSerializedString] {FileInfo}";
                    return false;
                }
            }
            string signature;
            string strTicketToFile;
            string strRandom;
            try
            {
                DateTimeOffset utcNow = DateTimeOffset.UtcNow;
                string strTicket = SettingsAccess.GenerateReferenceTicket(utcNow);
                strRandom = SettingsAccess.GenerateReferenceInfo();                
                strTicketToFile = utcNow.ToString("M/d/yyyy h:mm:ss tt zzz", CultureInfo.InvariantCulture);
                signature = SettingsAccess.GenerateSignature(strTicket, strRandom, serialized_string);
                //signature = strRandom + ";;" + strTicketToFile + ";;" + signature; //combine as single key
            }
            catch (Exception e)
            {
                info = "Calculate signature failed." + e.Message;
                return false;
            }
            if (string.IsNullOrEmpty(signature) || string.IsNullOrEmpty(strTicketToFile) || string.IsNullOrEmpty(strRandom))
            {
                info = $"Got null DDPM object, (1){signature},(2){strRandom},(3){strTicketToFile}";
                return false;
            }
            string write_string;
            try
            {
                //JObject jObject = JObject.Parse(serialized_string);
                JToken token = JToken.Parse(serialized_string);
                string temp = string.Empty;
                if (token.Type == JTokenType.Object)
                {
                    JObject obj = (JObject)token;
                    // Handle object
                    obj.Add("DDPM.Ticket", strTicketToFile);
                    obj.Add("DDPM.Info", strRandom);
                    obj.Add("DDPM.Signature", signature);
                    temp = obj.ToString();
                }
                else if (token.Type == JTokenType.Array)
                {
                    JArray array = (JArray)token;
                    // Handle array
                    //JObject newObject = new JObject();
                    //newObject["Signature"] = signature;
                    //array.Add(newObject);
                    array.Add("DDPM.Ticket : " + strTicketToFile);
                    array.Add("DDPM.Info : " + strRandom);
                    array.Add("DDPM.Signature : " + signature);
                    temp = array.ToString();
                }
                if (string.IsNullOrEmpty(temp))
                {
                    info = "Add sign to json object failed";
                    return false;
                }
                //2. Add signature
                //jObject.Add("Signature", signature);
                string modifiedJson = temp;// jObject.ToString();
                //convert whole content and protect it as bytes array
                byte[] body_array = Encoding.UTF8.GetBytes(modifiedJson);
                //3. Data protect if need (11/11 drop this action)
                
                write_string = modifiedJson;
            }
            catch (Exception ex)
            {
                info = "Add sign to json and protect it failed. " + ex.Message;
                return false;
            }

            if (string.IsNullOrEmpty(write_string))
            {
                info = "Convert protect content to base64 string got null result";
                return false;
            }
            //4. Write to target file
            try
            {
                File.WriteAllText(target_file, write_string);
            }
            catch (Exception ex2)
            {
                info = $"Write serialized string to file failed. ({ex2.Message})";
                return false;
            }
            return true;
        }

        /// <summary>
        /// Apply DDPM data security [Read settings]
        /// 1. Read based64 string from file
        /// 2. Data Unprotect if need
        /// 3. Retrieve signature
        /// 4. Check signature
        /// 5. return serialized string
        /// </summary>
        /// <param name="filePath">Source file for reading content</param>
        /// <returns>Empty string returned if any error occur</returns>
        public static string GetSerializedJsonString(string filePath, out string info)
        {
            info = "Success";
            if (!IsFilePathValid(filePath, out info))
            {
                info = $"[GetSerializedJsonString] {info}";
                return string.Empty;
            }

            string json_content = string.Empty;
            try
            {
                using (FileLock fileLock = new FileLock(filePath, PathCheckOption.None, lockNow: true))
                {
                    //1. Read json content
                    json_content = File.ReadAllText(filePath);
                }
            }
            catch (Exception ex)
            {
                info = "FileLock/ReadFile fail: " + ex.Message;
                return string.Empty;
            }

            if (string.IsNullOrEmpty(json_content))
            {
                info = "Null content of json file";
#if DEBUG 
                Console.WriteLine(info);
#endif 
                return string.Empty;
            }
            string serialized = json_content;
            // Parse the JSON string into a JObject
            string modifiedJson = string.Empty;
            string signature = string.Empty;
            string sInfo = string.Empty;
            string ticket = string.Empty;
            //3. retrieve signature for comparison
            try
            {
                JToken token = JToken.Parse(serialized);
                string temp = string.Empty;
                if (token.Type == JTokenType.Object)
                {
                    JObject obj = (JObject)token;
                    // Handle object
                    signature = (string)obj["DDPM.Signature"];
                    // Remove the "signature" property for hash generating
                    if (!obj.Remove("DDPM.Signature"))
                    {
                        info = "Remove signature field of json failed";
#if DEBUG 
                        Console.WriteLine(info);
#endif
                        return string.Empty;
                    }
                    sInfo = (string)obj["DDPM.Info"];
                    // Remove the "Info" property for hash generating
                    if (!obj.Remove("DDPM.Info"))
                    {
                        info = "Remove Info field of json failed";
#if DEBUG 
                        Console.WriteLine(info);
#endif
                        return string.Empty;
                    }
                    ticket = (string)obj["DDPM.Ticket"];
                    // Remove the "Ticket" property for hash generating
                    if (!obj.Remove("DDPM.Ticket"))
                    {
                        info = "Remove Ticket field of json failed";
#if DEBUG 
                        Console.WriteLine(info);
#endif
                        return string.Empty;
                    }
                    modifiedJson = obj.ToString();
                }
                else if (token.Type == JTokenType.Array)
                {
                    JArray array = (JArray)token;
                    // Handle array
                    var signatureStrings = array.Where(token => token.Type == JTokenType.String && token.ToString().StartsWith("DDPM.Signature"));
                    foreach (var sign in signatureStrings)
                    {
                        if (sign != null && !string.IsNullOrEmpty(sign.ToString()))
                        {
                            signature = sign.ToString().Replace("DDPM.Signature", "").Trim();
                            if (signature.StartsWith(":"))
                            {
                                signature = signature.Substring(1).Trim();
                            }
                            array.Remove(sign);
                            break;
                        }
                    }
                    signatureStrings = array.Where(token => token.Type == JTokenType.String && token.ToString().StartsWith("DDPM.Info"));
                    foreach (var sign in signatureStrings)
                    {
                        if (sign != null && !string.IsNullOrEmpty(sign.ToString()))
                        {
                            sInfo = sign.ToString().Replace("DDPM.Info", "").Trim();
                            if (sInfo.StartsWith(":"))
                            {
                                sInfo = sInfo.Substring(1).Trim();
                            }
                            array.Remove(sign);
                            break;
                        }
                    }
                    signatureStrings = array.Where(token => token.Type == JTokenType.String && token.ToString().StartsWith("DDPM.Ticket"));
                    foreach (var sign in signatureStrings)
                    {
                        if (sign != null && !string.IsNullOrEmpty(sign.ToString()))
                        {
                            ticket = sign.ToString().Replace("DDPM.Ticket", "").Trim();
                            if (ticket.StartsWith(":"))
                            {
                                ticket = ticket.Substring(1).Trim();
                            }
                            array.Remove(sign);
                            break;
                        }
                    }
                    modifiedJson = array.ToString();
                }
                if (string.IsNullOrEmpty(signature) || string.IsNullOrEmpty(sInfo) || string.IsNullOrEmpty(ticket))
                {
                    info = $"a part of key is null (1){signature},(2){sInfo},(3){ticket}";
#if DEBUG 
                    Console.WriteLine(info);
#endif
                    return string.Empty;
                }
            }
            catch (Exception ex)
            {
                info = "Get Signature from json fail.\nReason: " + ex.ToString();
#if DEBUG 
                Console.WriteLine(info);
#endif
                return string.Empty;
            }

            // Convert the modified JObject back to a JSON string
            string cal_sign;
            try
            {
                //cal_sign = SettingsAccess.ComputeAccessInfo2(Encoding.UTF8.GetBytes(accessInfo), modifiedJson);
                cal_sign = SettingsAccess.GenerateSignature(ticket, sInfo, modifiedJson);
                if (string.IsNullOrEmpty(cal_sign))
                {
                    info = "Null signature from hash calculation";
                    return string.Empty;
                }
            }
            catch (Exception ex)
            {
                info = "Calculate signature for verify failed. " + ex.Message;
                return string.Empty;
            }

            try
            {
                //4. Check if signature valid
                //if (cal_sign.ToLower().Equals(signature.ToLower()))
                if (SettingsAccess.VerifySignature(ticket, sInfo, modifiedJson, signature))
                    //5. return serialized string
                    return modifiedJson;
                else
                {
                    info = "Signature comparison result is FALSE";
                    return string.Empty;
                }
            }
            catch (Exception ex)
            {
                info = "Verify Signature exception: " + ex.Message;
                return string.Empty;
            }
        }

        public static bool LoadFileToVerifyJson_2(ILog log, string json_content, List<string> InfoPkey, out string strJson)
        {
            //1.Load public key from file (public_key.txt) --> verify signature with input json file via public key.
            //2.Load public key from file (public_key.cer, it could be DER or PEM format) --> verify signature with input json file via public key.
            //string json_file = jsonfilepath;
            string info = string.Empty;
            strJson = string.Empty;

            if (InfoPkey.Count <= 0)
            {
                WriteLog(log, $"Please check if Info keys exists", true);
                return false;
            }
            //Read json content
            //string json_content = File.ReadAllText(json_file);

            // Parse the JSON string into a JObject
            JObject jObject = JObject.Parse(json_content);
            string signature;
            try
            {
                signature = (string)jObject["Signature"];
                // Remove the "age" property
                jObject.Remove("Signature");
                // Convert the modified JObject back to a JSON string
                strJson = jObject.ToString();
            }
            catch (Exception ex)
            {
                WriteLog(log, "Try to get Signature from json fail.\nReason: " + ex.ToString(), true);
                strJson = string.Empty;
                return false;
            }

            if (string.IsNullOrEmpty(signature))
            {
                WriteLog(log, "Null signature in json content", true);
                strJson = string.Empty;
                return false;// No signature so fail
            }

            foreach (string key in InfoPkey)
            {
                //use signature to verify json
                if (DDPMFileSecurity.IsJsonContentValid_2(strJson, signature, key, HashAlgorithmName.SHA512, out info))
                {
                    WriteLog(log, "operation complete");
                    return true;
                }
            }
            WriteLog(log, "Json content got no info matched to signature", true);
            strJson = string.Empty;
            return false;
        }

        /*public static uint GetCheckSum(byte[] content, int count)
        {
            uint num = 0u;
            for (int i = 0; i < count; i++)
            {
                num += content[i];
            }
            return num;
        }*/

        /*public static byte[] GetSHA256(byte[] message, int offset, int count)
        {
            using SHA256 sHA = SHA256.Create();
            return sHA.ComputeHash(message, offset, count);
        }*/

        /// <summary>
        /// This function is used to provide hash as file checksum or json content signature
        /// </summary>
        /// <param name="message"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        /*public static byte[] GetSHA512(byte[] message, int offset, int count)//output 64bytes=512bits
        {
            using SHA512 sHA = SHA512.Create();
            return sHA.ComputeHash(message, offset, count);
        }*/

        /*private static bool CompareByteArrays(byte[] array1, byte[] array2)
        {
            return array1.SequenceEqual(array2);
        }*/

        public static bool IsFilePathValid(string filePath, out string info)
        {
            info = "Valid";
            //check return code with Enum PathCheckErrorCodes
            PathCheckErrorCodes result = PathHelper.ValidateFilePath(filePath);
            if (result != PathCheckErrorCodes.SUCCESS)
            {
                info = $"IsFilePathValid: {nameof(result)}";
                return false;
            }
            return true;
        }

        public static bool IsFolderPathValid(string folderPath, out string info)
        {
            info = "Valid";
            //check return code with Enum PathCheckErrorCodes
            PathCheckErrorCodes result = PathHelper.ValidateDirectoryPath(folderPath);
            if (result != PathCheckErrorCodes.SUCCESS)
            {
                info = $"IsFolderPathValid: {nameof(result)}";
                return false;
            }
            return true;
        }

        public static bool IsFilePathValid(string filePath, PathCheckOption option, out string info)
        {
            info = "Valid";
            //check return code with Enum PathCheckErrorCodes
            PathCheckErrorCodes result = PathHelper.ValidateFilePath(filePath, option);
            if (result != PathCheckErrorCodes.SUCCESS)
            {
                info = $"IsFilePathValid: {nameof(result)}";
                return false;
            }
            return true;
        }

        /// <summary>
        /// Check if the path redirected/junction/Mountpoint
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="info"></param>
        /// <returns></returns>
        public static bool IsPathSymbolicLinked(string Path, out string info)
        {
            info = "Valid";
            //check return code with Enum PathCheckErrorCodes
            PathRedirectionReturn result = PathHelper.CheckPathRedirection(Path);
            if (result != PathRedirectionReturn.PathIsNormal)
            {
                info = $"IsPathSymboliced: {nameof(result)}";
                return true;
            }
            return false;
        }

        /// <summary>
        /// Normal user only can read but admin has full right
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="info"></param>
        /// <param name="isDebug">if this true then all user can read/write</param>
        /// <returns></returns>
        public static bool ApplyFileACLUserReadOnly(string fileName, out string info, bool isDebug = false)
        {
            info = "Unknow failure";
            if (fileName == null || string.IsNullOrEmpty(fileName))
            {
                info = "[ApplyFileACLUserReadOnly] null or empty file path";
                return false;
            }

            if (File.Exists(fileName) == false)
            {
                info = $"[ApplyDDPMACLtoSettingFile] file ({fileName}) not exist";
                return false;
            }

            //Elsa Add Security
            string FileInfo;
            if (!DDPMFileSecurity.IsFilePathValid(fileName, out FileInfo))
            {
                info = $"[ApplyFileACLUserReadOnly] {FileInfo}";
                return false;
            }
            FileInfo fileInfo = new FileInfo(fileName);

            // Get file's security content
            FileSecurity fileSecurity = fileInfo.GetAccessControl();

            // Create rules for setting file
            var usersReadRule = new FileSystemAccessRule(new SecurityIdentifier(WellKnownSidType.BuiltinUsersSid, null), FileSystemRights.Read, AccessControlType.Allow);
            var usersWriteRule = new FileSystemAccessRule(new SecurityIdentifier(WellKnownSidType.BuiltinUsersSid, null), FileSystemRights.Write, AccessControlType.Deny);
            var usersRule = new FileSystemAccessRule(new SecurityIdentifier(WellKnownSidType.BuiltinUsersSid, null), FileSystemRights.FullControl, AccessControlType.Allow);
            var systemRule = new FileSystemAccessRule(new SecurityIdentifier(WellKnownSidType.LocalSystemSid, null), FileSystemRights.FullControl, AccessControlType.Allow);
            var adminRule = new FileSystemAccessRule(new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, null), FileSystemRights.Read | FileSystemRights.Write, AccessControlType.Allow);

            try
            {
                // check if can apply rules
                SetAccessRuleIfNotExists(ref fileSecurity, systemRule);
                if (!isDebug)
                {
                    //for release build please use this rule for normal user
                    SetAccessRuleIfNotExists(ref fileSecurity, usersReadRule);
                    SetAccessRuleIfNotExists(ref fileSecurity, usersWriteRule);
                }
                else
                    //debug purpose that apply all right for user
                    SetAccessRuleIfNotExists(ref fileSecurity, usersRule);

                SetAccessRuleIfNotExists(ref fileSecurity, adminRule);

                // In dotnet core, FileSystemAclExtensions.SetAccessControl method is the major function used to update file access right
                fileInfo.SetAccessControl(fileSecurity);
            }
            catch (Exception ex)
            {
                info = ex.Message;
                return false;
            }

            info = "Success";
            return true;
        }

        public static bool ApplyFileACLNormalUser(string fileName, out string info, bool isDebug = false)
        {
            info = "Unknow failure";
            if (fileName == null || string.IsNullOrEmpty(fileName))
            {
                info = "[ApplyFileACLNormalUser] null or empty file path";
                return false;
            }

            if (File.Exists(fileName) == false)
            {
                info = $"[ApplyFileACLNormalUser] file ({fileName}) not exist";
                return false;
            }

            //Elsa Add Security
            string FileInfo;
            if (!DDPMFileSecurity.IsFilePathValid(fileName, out FileInfo))
            {
                info = $"[ApplyFileACLNormalUser] {FileInfo}";
                return false;
            }

            FileInfo fileInfo = new FileInfo(fileName);

            FileSecurity fileSecurity = new FileSecurity();
            fileSecurity.SetAccessRuleProtection(isProtected: true, preserveInheritance: false);
            fileSecurity.AddAccessRule(new FileSystemAccessRule(LocalAccounts.Users.LocalSystemSid, FileSystemRights.FullControl, InheritanceFlags.None, PropagationFlags.None, AccessControlType.Allow));
            fileSecurity.AddAccessRule(new FileSystemAccessRule(LocalAccounts.Groups.BuiltinAdminsSid, FileSystemRights.FullControl, InheritanceFlags.None, PropagationFlags.None, AccessControlType.Allow));
            fileSecurity.AddAccessRule(new FileSystemAccessRule(LocalAccounts.Groups.BuiltinUsersSid, FileSystemRights.Read, InheritanceFlags.None, PropagationFlags.None, AccessControlType.Allow));
            fileSecurity.AddAccessRule(new FileSystemAccessRule(LocalAccounts.Groups.BuiltinUsersSid, FileSystemRights.Write, InheritanceFlags.None, PropagationFlags.None, AccessControlType.Allow));
            fileSecurity.SetOwner(LocalAccounts.Groups.BuiltinAdminsSid);

            try
            {
                // In dotnet core, FileSystemAclExtensions.SetAccessControl method is the major function used to update file access right
                fileInfo.SetAccessControl(fileSecurity);
            }
            catch (Exception ex)
            {
                info = ex.Message;
                return false;
            }

            info = "Success";
            return true;
        }

        private static void SetAccessRuleIfNotExists(ref FileSecurity fileSecurity, FileSystemAccessRule rule)
        {
            var rules = fileSecurity.GetAccessRules(true, true, typeof(SecurityIdentifier));
            if (!rules.Cast<FileSystemAccessRule>().Any(r => r.IdentityReference == rule.IdentityReference &&
                                                             r.FileSystemRights == rule.FileSystemRights &&
                                                             r.AccessControlType == rule.AccessControlType))
            {
                fileSecurity.AddAccessRule(rule);
            }
        }

        /// <summary>
        /// Check if the input file has restrict privilege
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="info"></param>
        /// <param name="isApplyACL">if the file is not restricted, set true to apply ACL</param>
        /// <returns></returns>
        public static bool CheckFileACL(string filePath, out string info, bool isApplyACL = false)
        {
            if (!File.Exists(filePath))
            {
                info = $"CheckFileACL: {filePath} isn't exist.";
                return false;
            }
            //Elsa Add Security
            string FileInfo;
            if (!IsFilePathValid(filePath, out FileInfo))
            {
                info = $"[CheckFileACL] {FileInfo}";
                return false;
            }
            FileInfo fileInfo = new FileInfo(filePath);
            try
            {
                // verify the ACLs using the ACLChecker class
                var aclChecker = new AclChecker();
                if (aclChecker.ContainsUnprivilegedWriteAccess(fileInfo))
                {
                    // fileInfo contains unprivileged write access. Not good!
                    if (isApplyACL)
                    {
                        // set the pre-canned restricted ACLs for files from FileSystemAcls
                        var fileSystemAcls = new FileSystemAcls();
                        fileSystemAcls.SetRestrictedAclsForFile(filePath);
                        if (aclChecker.ContainsUnprivilegedWriteAccess(fileInfo))
                        {
                            info = $"CheckFileACL: File {filePath} apply ACLs failed";
                            return false;
                        }
                        else
                        {
                            info = "File ACLed w/ ACLs applied";
                            return true;
                        }
                    }
                    else
                    {
                        info = $"CheckFileACL: File {filePath} isn't good with ACLs";
                        return false;
                    }
                }
                else
                {
                    info = "File ACLed";
                    return true;
                }
            }
            catch (Exception e)
            {
                info = e.Message;
                return false;
            }
        }

        /// <summary>
        /// Check if the input file has restrict privilege
        /// </summary>
        /// <param name="folderPath"></param>
        /// <param name="info"></param>
        /// <param name="isApplyACL">if the file is not restricted, set true to apply ACL</param>
        /// <returns></returns>
        public static bool CheckFolderACL(string folderPath, out string info, bool isApplyACL = false)
        {
            if (!Directory.Exists(folderPath))
            {
                info = $"CheckFolderACL: {folderPath} isn't exist.";
                return false;
            }

            //Elsa Add Security
            string FileInfo;
            if (!IsFolderPathValid(folderPath, out FileInfo))
            {
                info = $"[CheckFolderACL] {FileInfo}";
                return false;
            }
            DirectoryInfo folderInfo = new DirectoryInfo(folderPath);
            // verify the ACLs using the ACLChecker class
            try
            {
                var aclChecker = new AclChecker();
                if (aclChecker.ContainsUnprivilegedWriteAccess(folderInfo))
                {
                    // folderInfo contains unprivileged write access. Not good!
                    if (isApplyACL)
                    {
                        // set the pre-canned restricted ACLs for files from FileSystemAcls
                        var folderSystemAcls = new FileSystemAcls();
                        folderSystemAcls.SetRestrictedAclsForDirectory(folderPath);
                        if (aclChecker.ContainsUnprivilegedWriteAccess(folderInfo))
                        {
                            info = $"CheckFolderACL: Folder {folderPath} apply ACLs failed";
                            return false;
                        }
                        else
                        {
                            info = "Folder ACLed w/ ACLs applied";
                            return true;
                        }
                    }
                    else
                    {
                        info = $"CheckFolderACL: Folder {folderPath} isn't good with ACLs";
                        return false;
                    }
                }
                else
                {
                    info = "Folder ACLed";
                    return true;
                }
            }
            catch (Exception e)
            {
                info = e.Message;
                return false;
            }
        }

        //
        //The caller should use try-catch to catch exception and avoid crash
        public static void SetFolderPermissions_UserReadAndExecute(string folderPath)
        {
            // Elsa Add Security
            string FileInfo;
            if (!IsFolderPathValid(folderPath, out FileInfo))
            {
                //_log.Info($"{nameof(SetFolderPermissions_UserReadAndExecute)} {FileInfo}");
                throw new SecurityException($"{FileInfo}");
            }
            DirectoryInfo directoryInfo = new DirectoryInfo(folderPath);
            DirectorySecurity directorySecurity = directoryInfo.GetAccessControl();

            // Admin - full control
            SecurityIdentifier adminSid = new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, null);
            FileSystemAccessRule adminRule = new FileSystemAccessRule(adminSid, FileSystemRights.FullControl, InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit, PropagationFlags.None, AccessControlType.Allow);
            directorySecurity.AddAccessRule(adminRule);

            // System - full control
            SecurityIdentifier systemSid = new SecurityIdentifier(WellKnownSidType.LocalSystemSid, null);
            FileSystemAccessRule systemRule = new FileSystemAccessRule(systemSid, FileSystemRights.FullControl, InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit, PropagationFlags.None, AccessControlType.Allow);
            directorySecurity.AddAccessRule(systemRule);

            // normal user - read and execute (w/o write)
            SecurityIdentifier usersSid = new SecurityIdentifier(WellKnownSidType.BuiltinUsersSid, null);
            FileSystemAccessRule usersRule = new FileSystemAccessRule(usersSid, FileSystemRights.ReadAndExecute, InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit, PropagationFlags.None, AccessControlType.Allow);
            directorySecurity.AddAccessRule(usersRule);

            // normal user - read write deny
            FileSystemAccessRule denyWriteRule = new FileSystemAccessRule(usersSid, FileSystemRights.Write, InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit, PropagationFlags.None, AccessControlType.Deny);
            directorySecurity.AddAccessRule(denyWriteRule);

            // apply change
            directoryInfo.SetAccessControl(directorySecurity);
        }

        public static string GetFileSHA_256(string filePath, out string info)
        {
            // Check our path string for invalid characters, null value, empty value, etc.
            if (PathHelper.ValidateFilePath(filePath, PathCheckOption.None) != PathCheckErrorCodes.SUCCESS)
            {
                info = $"Invalid file path string - {filePath}";
                //_log.Info(info);
                return null;
            }

            //Check for path redirection (symlink, mountpoint, hardlink, etc.) at the path AND along the path
            if (PathHelper.CheckPathRedirection(filePath) != PathRedirectionReturn.PathIsNormal)
            {
                info = $"Redirection detected along file path - {filePath}";
                //_log.Info(info);
                return null;
            }

            byte[] data = null;

            try
            {
                data = File.ReadAllBytes(filePath);
            }
            catch
            {
                info = $"Read data from file path - {filePath}, exception";
                return null;
            }

            if (data == null)
            {
                info = $"Read data from file path - {filePath}, failed";
                return null;
            }
            //Bruce 0909 modify
            //byte[] result = CryptoHelper.GenerateHashBytes(data, HashType.Sha256);
            string result = CalculateFileSHA256(filePath, out info);
            info = "Complete";
            if (string.IsNullOrEmpty(result))
            {
                info = "Calculate fail. " + info;
            }
            return result;
        }
        static string CalculateFileSHA256(string filePath, out string info)
        {
            info = string.Empty;
            string ret = string.Empty;
            try
            {
                using (FileStream fileStream = File.OpenRead(filePath))
                {
                    using (SHA256 sha256 = SHA256.Create())
                    {
                        byte[] hashBytes = sha256.ComputeHash(fileStream);

                        // 將計算的雜湊值轉換為十六進制字符串
                        ret = BitConverter.ToString(hashBytes).Replace("-", "");
                    }
                }
            }
            catch (Exception ex)
            {
                info = "[CalculateFileSHA256] exception, message: " + ex.Message;
            }
            return ret;
        }
        public static string GetFileSHA_512(string filePath, out string info)
        {
            // Check our path string for invalid characters, null value, empty value, etc.
            if (PathHelper.ValidateFilePath(filePath, PathCheckOption.None) != PathCheckErrorCodes.SUCCESS)
            {
                info = $"Invalid file path string - {filePath}";
                // _log.Info(info);
                return null;
            }

            //Check for path redirection (symlink, mountpoint, hardlink, etc.) at the path AND along the path
            if (PathHelper.CheckPathRedirection(filePath) != PathRedirectionReturn.PathIsNormal)
            {
                info = $"Redirection detected along file path - {filePath}";
                // _log.Info(info);
                return null;
            }

            byte[] data = null;
            try
            {
                data = File.ReadAllBytes(filePath);
            }
            catch
            {
                info = $"Read data from file path - {filePath}, exception";
                return null;
            }

            if (data == null)
            {
                info = $"Read data from file path - {filePath}, failed";
                return null;
            }
            //Bruce 0909 modify
            //byte[] result = CryptoHelper.GenerateHashBytes(data, HashType.Sha512);
            string result = CalculateFileSHA512(filePath, out info);
            info = "Complete";
            if (string.IsNullOrEmpty(result))
            {
                info = "Calculate fail.";
            }
            return result;
        }
        static string CalculateFileSHA512(string filePath, out string info)
        {
            info = string.Empty;
            string ret = string.Empty;
            try
            {
                using (FileStream fileStream = File.OpenRead(filePath))
                {
                    using (SHA512 sha512 = SHA512.Create())
                    {
                        byte[] hashBytes = sha512.ComputeHash(fileStream);

                        // 將計算的雜湊值轉換為十六進制字符串
                        ret = BitConverter.ToString(hashBytes).Replace("-", "");
                    }
                }
            }
            catch(Exception ex)
            {
                info = "[CalculateFileSHA256] exception, message: " + ex.Message;
            }
            return ret;
        }

        //Using public key and pre-generated signature to validate json file
        public static bool IsJsonContentValid(string json_content, string base64_signature, string public_key_file, HashAlgorithmName algorithm, out string info)
        {
            info = "unknow error";

            string publicKeyBase64 = string.Empty;
            try
            {
                using (FileLock fileLock = new FileLock(public_key_file, PathCheckOption.None, lockNow: true))
                {
                    // Read the base64-encoded public key from a text file
                    publicKeyBase64 = File.ReadAllText(public_key_file);
                }
            }
            catch (Exception ex)
            {
                info = "FileLock/ReadFile fail: " + ex.Message;
                return false;
            }


            // Convert the base64 string to bytes
            byte[] publicKeyBytes = Convert.FromBase64String(publicKeyBase64);

            try
            {
                // Create an RSACryptoServiceProvider from the public key
                using (var rsa = new RSACryptoServiceProvider())
                {
                    rsa.ImportRSAPublicKey(publicKeyBytes, out _);

                    // Now you can use 'rsa' for verification
                    // For example, verify a JSON file's content
                    //string jsonContent = File.ReadAllText("data.json");
                    //byte[] dataBytes = System.Text.Encoding.UTF8.GetBytes(jsonContent);
                    byte[] dataBytes = System.Text.Encoding.UTF8.GetBytes(json_content);

                    // Assume you have the signature (base64-encoded) in 'signatureBase64'
                    byte[] signature = Convert.FromBase64String(base64_signature);

                    //algorithm could be SHA256 or SHA512
                    bool isSignatureValid = rsa.VerifyData(dataBytes, signature, algorithm, RSASignaturePadding.Pkcs1);
#if DEBUG
                    Console.WriteLine($"Signature is valid: {isSignatureValid}");
#endif
                    info = $"The signature validated result: {isSignatureValid}";
                    return isSignatureValid;
                }
            }
            catch (Exception ex)
            {
                info = ex.Message;
                return false;
            }
        }

        //Using public key and pre-generated signature to validate json file
        public static bool IsJsonContentValid_2(string json_content, string base64_signature, string PInfoKey, HashAlgorithmName algorithm, out string info)
        {
            info = "unknow error";

            // Convert the base64 string to bytes
            byte[] publicKeyBytes = Convert.FromBase64String(PInfoKey);

            try
            {
                // Create an RSACryptoServiceProvider from the public key
                using (var rsa = new RSACryptoServiceProvider())
                {
                    rsa.ImportSubjectPublicKeyInfo(publicKeyBytes, out _);

                    // Now you can use 'rsa' for verification
                    // For example, verify a JSON file's content
                    byte[] dataBytes = System.Text.Encoding.UTF8.GetBytes(json_content);

                    // Assume you have the signature (base64-encoded) in 'signatureBase64'
                    byte[] signature = Convert.FromBase64String(base64_signature);

                    //algorithm could be SHA256 or SHA512
                    bool isSignatureValid = rsa.VerifyData(dataBytes, signature, algorithm, RSASignaturePadding.Pkcs1);
#if DEBUG
                    Console.WriteLine($"Signature is valid: {isSignatureValid}");
#endif
                    info = $"The signature validated result: {isSignatureValid}";
                    return isSignatureValid;
                }
            }
            catch (Exception ex)
            {
                info = ex.Message;
                return false;
            }
        }

        //Using DCF security library to check if certification valid. (WinVerifyTrust)
        public static bool VerifyExecutableFileSignature(string filePath, out string info)
        {
            info = string.Empty;
            try
            {
                //Method 1
                /*var verifier = new PeAuthenticodeVerifier();

                using (var fileLock = new FileLock(filePath, PathCheckOption.IgnoreAll, lockNow: true))
                {
                    var result = verifier.Verify(fileLock);

                    if (result == Win32ErrorCodes.ERROR_SUCCESS)
                    {
                        info = $"[VerifyExecutableFileSignature] File has valid signature";
                        return true;
                    }
                    else
                    {
                        info = $"[VerifyExecutableFileSignature] File has invalid signature, last error: {result}";
                        return false;
                    }
                }*/
                //Method 2 - with verify option
                VerifierOption myVerifierOptions = VerifierOption.UseOfflineRevocationCheck;
                var verifier = new PeAuthenticodeVerifier(myVerifierOptions);

                using (var fileLock = new FileLock(filePath, PathCheckOption.IgnoreAll, lockNow: true))
                {
                    var result = verifier.Verify(fileLock);

                    if (result == Win32ErrorCodes.ERROR_SUCCESS)
                    {
                        info = $"[VerifyExecutableFileSignature] File has valid signature";
                        return true;
                    }
                    else
                    {
                        info = $"[VerifyExecutableFileSignature] File has invalid signature, last error: {result}";
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                info = "[VerifyExecutableFileSignature] " + ex.Message;
                return false;
            }
        }

        /*private static bool CheckCertificateIsVaild(X509Certificate2 cert, ref string info)
        {                        
            bool result = false;
            try
            {
                X509Chain x509Chain = new X509Chain();
                x509Chain.ChainPolicy.RevocationFlag = X509RevocationFlag.EntireChain;
                x509Chain.ChainPolicy.RevocationMode = X509RevocationMode.Online;
                x509Chain.ChainPolicy.UrlRetrievalTimeout = new TimeSpan(0, 1, 0); // 1 minute timeout
                x509Chain.ChainPolicy.VerificationFlags = X509VerificationFlags.NoFlag;
                result = x509Chain.Build(cert);

                info = "Certificate is vaild";
            }
            catch (Exception ex)
            {
                info = "[CheckCertificateIsVaild] error: " + ex.Message;
            }

            return result;
        }*/

        private static byte[] ConvertThumbprintToByteArray(string thumbprint)
        {
            return Enumerable.Range(0, thumbprint.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(thumbprint.Substring(x, 2), 16))
                             .ToArray();
        }

        public static bool VerifyFileCertWithInboxThumbprint(string filePath, out string info)
        {
            info = "success";
            if (!IsFilePathValid(filePath, out info))
            {
#if DEBUG
                Console.WriteLine(info);
#endif
                return false;
            }
            try
            {
                X509Certificate2 cert = LoadFileCertificate(filePath);
                if (cert == null)
                {
                    info = "Can't retrieve cert from file.";
                    return false;
                }

                //compare thumbprint
                //source array DDPM.SA.Obfuscation.ThumbprintHash.certificateHash
                //Target cert.Thumbprint
                bool contains = DDPM.SA.Obfuscation.ThumbprintHash.certificateHash.Any(arr => arr.SequenceEqual(ConvertThumbprintToByteArray(cert.Thumbprint)));
                if (!contains)
                {
                    info = $"No matched cert. thumbprint in file is {cert.Thumbprint}";
                    return false;
                }
                if (!VerifyExecutableFileSignature(filePath, out info))
                {
#if DEBUG
                    Console.WriteLine(info);
#endif
                    return false;
                }
            }
            catch (Exception ex)
            {
                info = ex.Message;
                return false;
            }
            return true;
        }

        public static bool VerifyFileCertWithoutThumbprint(string filePath, out string info)
        {
            info = "success";
            if (!IsFilePathValid(filePath, out info))
            {
#if DEBUG
                Console.WriteLine(info);
#endif
                return false;
            }
            try
            {
                if (!VerifyExecutableFileSignature(filePath, out info))
                {
#if DEBUG
                    Console.WriteLine(info);
#endif
                    return false;
                }
            }
            catch (Exception ex)
            {
                info = ex.Message;
                return false;
            }
            return true;
        }

        public static bool VerifyFileCertWithThumbprint(string filePath, string targetThumbprint, out string info)
        {
            info = "success";
            if (!IsFilePathValid(filePath, out info))
            {
#if DEBUG
                Console.WriteLine(info);
#endif
                return false;
            }
            if (string.IsNullOrEmpty(targetThumbprint))
            {
                info = "Abnormal thumbprint as input";
                return false;
            }
            try
            {
                if (!VerifyExecutableFileSignature(filePath, out info))
                {
#if DEBUG
                    Console.WriteLine(info);
#endif
                    return false;
                }

                X509Certificate2 cert = new X509Certificate2(filePath);
                if (cert == null)
                {
                    info = "Can't retrieve cert from file.";
                    return false;
                }

                //compare thumbprint from input
                //Target cert.Thumbprint{
                bool contains = targetThumbprint.ToUpper().Trim().Equals(cert.Thumbprint.ToUpper().Trim());
                if (!contains)
                {
                    info = $"No matched cert. thumbprint in file is {cert.Thumbprint}";
                    return false;
                }
            }
            catch (Exception e)
            {
                info = e.Message;
                return false;

            }
            return true;

        }

        #region Bruce 0814 Move this method to DDPM.SA.Common

        private enum WTS_INFO_CLASS
        {
            WTSUserName = 5,
            WTSDomainName = 7,
        }

        [DllImport("Kernel32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern int WTSGetActiveConsoleSessionId();

        private int WTSGetActiveConsoleSessionId_Public()
        {
            return WTSGetActiveConsoleSessionId();
        }

        [DllImport("Wtsapi32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern bool WTSQuerySessionInformation(IntPtr hServer, int sessionId, WTS_INFO_CLASS wtsInfoClass, out IntPtr ppBuffer, out int pBytesReturned);

        private bool WTSQuerySessionInformation_Public(IntPtr hServer, int sessionId, WTS_INFO_CLASS wtsInfoClass, out IntPtr ppBuffer, out int pBytesReturned)
        {
            return WTSQuerySessionInformation(hServer, sessionId, wtsInfoClass, out ppBuffer, out pBytesReturned);
        }

        [DllImport("Wtsapi32.dll", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern void WTSFreeMemory(IntPtr pointer);

        private void WTSFreeMemory_Public(IntPtr pointer)
        {
            WTSFreeMemory(pointer);
        }

        public string GetActiveUserLocalAppDataPath()
        {
            IntPtr buffer;
            int bytesReturned = 0;
            int sessionId = WTSGetActiveConsoleSessionId_Public(); // This gets the session ID of the user logged into the console
#if DEBUG
            Console.WriteLine($"WTSGetActiveConsoleSessionId: {sessionId}");
#endif
            if (WTSQuerySessionInformation_Public(IntPtr.Zero, sessionId, WTS_INFO_CLASS.WTSUserName, out buffer, out bytesReturned))
            {
                string userName = Marshal.PtrToStringAnsi(buffer);
                WTSFreeMemory_Public(buffer);
#if DEBUG
                Console.WriteLine($"WTSQuerySessionInformation: user name ({userName})");
#endif

                if (!string.IsNullOrEmpty(userName))
                {
                    string userSid = GetUserSid(userName);
                    if (!string.IsNullOrEmpty(userSid))
                    {
                        string regKey = $@"HKEY_USERS\{userSid}\Software\Microsoft\Windows\CurrentVersion\Explorer\Shell Folders";
                        string localAppDataPath = (string)Registry.GetValue(regKey, "Local AppData", null);
#if DEBUG
                        Console.WriteLine($"Local app data from registry: {localAppDataPath}");
#endif
                        return localAppDataPath;
                    }
                }
                else
                {
#if DEBUG
                    Console.WriteLine("Got null user name");
#endif
                }
            }
            else
            {
#if DEBUG
                Console.WriteLine("WTSQuerySessionInformation: return false");
#endif
            }
            return null;
        }

        private string GetUserSid(string userName)
        {
            NTAccount f_normal, f_domain = null;
            string accountName = $"{Environment.MachineName}\\{userName}";
            f_normal = new NTAccount(accountName);
#if DEBUG
            Console.WriteLine($"GetUserSid: Machine name: {Environment.MachineName}, User name:{userName}");
#endif
            if (!string.IsNullOrEmpty(Environment.UserDomainName))
            {
                accountName = $"{Environment.UserDomainName}\\{userName}";
#if DEBUG 
                Console.WriteLine($"GetUserSid: find domain name: {Environment.UserDomainName}, User name:{userName}");
#endif
                f_domain = new NTAccount(Environment.UserDomainName, userName);
            }

            string sidString;
            try
            {
                SecurityIdentifier s = (SecurityIdentifier)f_normal.Translate(typeof(SecurityIdentifier));
                sidString = s.ToString();
#if DEBUG
                Console.WriteLine($"GetUserSid(normal user): SID: {sidString}");
#endif
            }
            catch (Exception ex)
            {
                sidString = null;
#if DEBUG
                Console.WriteLine($"GetUserSid(normal user): try translate fail: {ex.Message}");
#endif

                //0724 add code that translate normal user and do translate domain user if fail.
                if (f_domain != null)
                {
                    try
                    {
                        SecurityIdentifier s = (SecurityIdentifier)f_domain.Translate(typeof(SecurityIdentifier));
                        sidString = s.ToString();
#if DEBUG
                        Console.WriteLine($"GetUserSid(domain user): SID: {sidString}");
#endif
                    }
                    catch (Exception e)
                    {
                        sidString = null;
#if DEBUG
                        Console.WriteLine($"GetUserSid(domain user): try translate fail: {e.Message}");
#endif
                    }
                }
            }
            return sidString;
        }

        #endregion Bruce 0814 Move this method to DDPM.SA.Common
        public static bool SRemoveSymbolicFile(string filePath, out string info)
        {
            info = "pass";
            if (!DDPMFileSecurity.IsPathSymbolicLinked(filePath, out info))  // filePath contain symbolic
            {
                return true;
            }

            FileAttributes attr = File.GetAttributes(filePath);
            if (!attr.HasFlag(FileAttributes.Directory))
            {   // File
                if (SymlinkHelper.IsFileHasSymlink(filePath, out info)) // is the current file symbolic ?
                {
                    if (!SymlinkHelper.RemoveFileSymlink2(filePath, out info))
                    {
#if DEBUG
                        Console.WriteLine($"Delete File failed. ({info})");
#endif
                        return false;
                    }
                }
                else
                {
                    info = "The File is not a Symbolic";    // need to check Symbolic in Path folder
                    return false;
                }
            }
            return true;
        }

        public static bool SRemoveSymbolicFolder(string filePath, out string info)
        {
            info = "pass";
            if (!IsPathSymbolicLinked(filePath, out info))  // filePath contain symbolic
            {
                return true;
            }

            FileAttributes attr = File.GetAttributes(filePath);
            if (attr.HasFlag(FileAttributes.Directory))
            {   // Directory
                if (SymlinkHelper.IsFolderHasSymlink(filePath, out info)) // is current folder symbolic ? 
                {
                    if (!SymlinkHelper.RemoveFolderSymlink2(filePath, out info)) // Remove current symbolic folder
                    {
#if DEBUG
                        Console.WriteLine($"Delete Folder failed. ({info})");
#endif
                        return false;
                    }
                    else // check parent folder for symbolic
                    {
                        string tmpParentPath = string.Empty;
                        tmpParentPath = Path.GetDirectoryName(filePath);
                        if (tmpParentPath != null && SRemoveSymbolicFolder(tmpParentPath, out info))
                        {
                            Directory.CreateDirectory(filePath);
                        }
                        return true;
                    }
                }
                else
                {
                    //info = "The Folder is not a Symbolic";
                    string tmpParentPath = string.Empty;
                    tmpParentPath = Path.GetDirectoryName(filePath); // to check parent 
                    if (tmpParentPath != null && SRemoveSymbolicFolder(tmpParentPath, out info))
                    {
                        Directory.CreateDirectory(filePath);
                        return true;
                    }
                    else
                    {
                        Directory.CreateDirectory(filePath);
                        return false;
                    }
                }
            }
            return true;
        }
        public static bool CheckFold(string folderPath, out string folderInfo, out string pathSymbolicLinInfo)    // Move from Bruce code
        {
            folderInfo = "Error";
            pathSymbolicLinInfo = "Error";
            int count = 0;
            bool folderValid = false;

            if (string.IsNullOrEmpty(folderPath))
            {
                folderInfo = "CheckFold - folder path NULL";
                return folderValid;
            }

            do
            {
                folderInfo = string.Empty;
                pathSymbolicLinInfo = string.Empty;
                folderValid = false;
                folderValid = DDPMFileSecurity.SRemoveSymbolicFolder(folderPath, out pathSymbolicLinInfo);  //0924 Bruce Add Security
                if (!folderValid)
                {
                    count++;
                }
                folderValid = DDPMFileSecurity.IsFolderPathValid(folderPath, out folderInfo) && folderValid;
                if (!folderValid)
                {
                    Directory.CreateDirectory(folderPath);
                    count++;
                }
            } while (!folderValid && count < 2);
            return folderValid;
        }

        public static bool VerifyDDPMMetadata(ILog log, string filePath, List<string> InfoPkey, out string inline_info, out string strJson)
        {
            string msg = string.Empty;
            inline_info = string.Empty;
            strJson = string.Empty;
            string json_read = string.Empty;
            try
            {
                if (!IsFilePathValid(filePath, out msg))
                {
                    WriteLog(log, msg, true);
                    return false;
                }
                using (FileLock fileLock = new FileLock(filePath, PathCheckOption.None, lockNow: true))
                {
                    json_read = File.ReadAllText(filePath);
                    if (string.IsNullOrEmpty(json_read))
                    {
                        WriteLog(log, "Read file without any content", true);
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                WriteLog(log, $"Read file failed: ({ex.Message})", true);
                return false;
            }
            strJson = VerifyDDPMMetadata(log, json_read, InfoPkey, out inline_info);
            return !string.IsNullOrEmpty(strJson);
        }

        //check json content to remove signature and info then output for caller
        public static string VerifyDDPMMetadata(ILog log, string fileContent, List<string> InfoPkey, out string inline_info)
        {
            bool ret = false;
            inline_info = string.Empty;
            string strJson = string.Empty;
            //Pass json metadata to security check and try to output serialized json string
            // the output json string will remove signature
            ret = LoadFileToVerifyJson_2(log, fileContent, InfoPkey, out strJson);

            // Handle "Info" section
            if (ret && !string.IsNullOrEmpty(strJson))
            {
                JObject jObject = JObject.Parse(strJson);
                string szInfo;
                try
                {
                    szInfo = (string)jObject["Info"];
                    if (!string.IsNullOrEmpty(szInfo))
                    {
                        jObject.Remove("Info");
                        inline_info = szInfo;
                    }
                }
                catch (Exception ex)
                {
                    WriteLog(log, "Try to get info key from json fail.\nReason: " + ex.ToString(), true);
                }
                // Convert the modified JObject back to a JSON string                
                strJson = jObject.ToString();
            }
            else
            {
                WriteLog(log, "[Metadata check] metadata is invalid", true);
            }

            return strJson;
        }

        private static bool IsAbsolutePath(string path)
        {
            return Path.IsPathRooted(path) && !path.StartsWith(".") && !path.StartsWith(@"..");
        }

        private static bool IsRelativePath(string path)
        {
            return !IsAbsolutePath(path) && (path.StartsWith(".") || path.StartsWith(@"..") || Path.GetDirectoryName(path) != null);
        }

        private static bool IsFileNameOnly(string path)
        {
            return !IsAbsolutePath(path) && !IsRelativePath(path);
        }

        private static bool IsUrl(string path)
        {
            Uri uriResult;
            bool result = Uri.TryCreate(path, UriKind.Absolute, out uriResult)
                          && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
            return result;
        }

        //Make sure "needCheckThumbprintInbox" and "givenThumbprintCheck" do not active at the same time
        private static bool IsProcessInfoValid(
            ILog log, string filePath,
            string fileHash = "",
            string hashType = "SHA512",
            bool needCheckThumbprintInbox = false,
            string givenThumbprintCheck = "")
        {
            string info = string.Empty;
            if (IsFileNameOnly(filePath))
            {
                if (log != null)
                    log.Error($"[IsProcessInfoValid] just file name [{filePath}] only");
            }
            else if (IsUrl(filePath))
            {
                if (log != null)
                    log.Error($"[IsProcessInfoValid] just URL [{filePath}] only");
                return true;
            }
            else
            {
                FileInfo fi = new FileInfo(filePath);
                if (fi == null)
                {
                    if (log != null)
                        log.Error("[IsProcessInfoValid] create FileInfo from path got null object");
                    return false;
                }
                if (!IsFilePathValid(filePath, out info))
                {
                    if (log != null)
                        log.Info($"[IsProcessInfoValid] IsFilePathValid: {info}");
                    return false;
                }
            }
            if (!string.IsNullOrEmpty(fileHash) && fileHash.Length > 0)
            {
                bool ret = false;
                if (hashType.ToLower().Equals("sha512"))
                    ret = GetFileSHA_512(filePath, out info).ToLower().Equals(fileHash.ToLower());
                else if (hashType.ToLower().Equals("sha256"))
                    ret = GetFileSHA_256(filePath, out info).ToLower().Equals(fileHash.ToLower());
                else
                    info = $"un-support file hash type: {hashType}";

                if (!ret)
                {
                    if (log != null)
                        log.Error($"[IsProcessInfoValid] file hash check failed: {info}");
                    return false;
                }
            }
            if (needCheckThumbprintInbox)
            {
                //if (!VerifyFileCertWithoutThumbprint(filePath, out info))
                if(!VerifyFileCertWithInboxThumbprint(filePath, out info))
                {
                    if (log != null)
                        log.Error($"[IsProcessInfoValid] VerifyFileCertWithThumbprint: {info}");
                    return false;
                }
            }
            if (!string.IsNullOrEmpty(givenThumbprintCheck) && givenThumbprintCheck.Length > 0)
            {
                if (!VerifyFileCertWithThumbprint(filePath, givenThumbprintCheck, out info))
                {
                    if (log != null)
                        log.Error($"[IsProcessInfoValid] VerifyFileCertWithThumbprint: {info}");
                    return false;
                }
            }
            return true;
        }

        //Make sure that startInfo and filePath should not exist at the same time
        private static bool StartProcessByOptions(ILog log,
            ProcessStartInfo startInfo = null,
            string filePath = "",
            string arguments = "",
            bool isLockNeeded = false,
            bool isWaitExitCode = false)
        {
            if (startInfo == null && string.IsNullOrEmpty(filePath))
            {
                if (log != null)
                    log.Error("[StartProcessByOptions] startInfo and filePath are empty at the same time");
                return false;
            }
            if (startInfo != null && !string.IsNullOrEmpty(filePath) && filePath.Length > 0)
            {
                if (log != null)
                    log.Error("[StartProcessByOptions] startInfo and filePath should not exist at the same time");
                return false;
            }
            if (startInfo == null)
            {
                startInfo = new ProcessStartInfo(filePath, arguments);
            }
            else
            {
                filePath = startInfo.FileName;
            }
            bool result = true;
            if (isLockNeeded)
            {
                log.Info("[StartProcessSafely] Lock file");
                using (FileLock fileLock = new FileLock(filePath, PathCheckOption.None, lockNow: true))
                {
                    // start process
                    using (Process process = Process.Start(startInfo))
                    {
                        if (isWaitExitCode)
                        {
                            log.Info($"[StartProcessSafely] Start process [{process.ProcessName}]");
                            string output = process.StandardOutput.ReadToEnd();
                            string error = process.StandardError.ReadToEnd();

                            // Wait result
                            process.WaitForExit();

                            // Get result
                            if (process.ExitCode == 0)
                            {
                                if (log != null)
                                    log.Info("[StartProcessSafely] Events have been exported successfully.");
                                result = true;
                            }
                            else
                            {
                                if (log != null)
                                    log.Error($"[StartProcessSafely] Error exporting events: {error}");
                                result = false;
                            }
                        }
                        else
                        {
                            if (log != null)
                            {
                                if (!IsUrl(filePath))
                                    log.Info($"[StartProcessSafely] Start process [{process.ProcessName}] and do not wait.");
                                else
                                    log.Info($"[StartProcessSafely] Start URL and do not wait.");
                            }
                        }
                    }
                }
                return result;
            }
            else
            {
                // start process
                using (Process process = Process.Start(startInfo))
                {
                    if (isWaitExitCode)
                    {
                        log.Info($"[StartProcessSafely] Start process [{process.ProcessName}]");
                        string output = process.StandardOutput.ReadToEnd();
                        string error = process.StandardError.ReadToEnd();

                        // Wait result
                        process.WaitForExit();

                        // Get result
                        if (process.ExitCode == 0)
                        {
                            if (log != null)
                                log.Info("[StartProcessSafely] Events have been exported successfully.");
                            result = true;
                        }
                        else
                        {
                            if (log != null)
                                log.Info($"[StartProcessSafely] Error exporting events: {error}");
                            result = false;
                        }
                    }
                    else
                    {
                        if (log != null)
                        {
                            if (!IsUrl(filePath))
                                log.Info($"[StartProcessSafely] Start process [{process.ProcessName}] and do not wait.");
                            else
                                log.Info($"[StartProcessSafely] Start URL and do not wait.");
                        }
                    }
                }
                return result;
            }
        }

        //hashType: SHA256 / SHA512
        public static bool StartProcessSafely(
            ILog log, string filePath,
            string arguments = "",
            bool needCheckThumbprintInbox = false,
            string fileHash = "",
            string hashType = "SHA512",
            bool isLockNeeded = false)
        {
            string info = string.Empty;
            if (!IsProcessInfoValid(log, filePath, fileHash, hashType, needCheckThumbprintInbox))
                return false;

            StartProcessByOptions(log, null, filePath, arguments, isLockNeeded);
            return true;
        }

        //hashType: SHA256 / SHA512
        public static bool StartProcessSafely(
            ILog log, string filePath,
            string arguments = "",
            string fileHash = "",
            string hashType = "SHA512",
            bool isLockNeeded = false,
            string givenThumbprintCheck = "")
        {
            string info = string.Empty;
            if (!IsProcessInfoValid(log, filePath, fileHash, hashType, false, givenThumbprintCheck))
                return false;

            StartProcessByOptions(log, null, filePath, arguments, isLockNeeded);
            return true;
        }

        //Start process without any criteria
        public static bool StartProcessSafely(
            ILog log, string filePath,
            string arguments = "")
        {
            string info = string.Empty;
            if (!IsProcessInfoValid(log, filePath))
                return false;

            StartProcessByOptions(log, null, filePath, arguments);
            return true;
        }

        //Start process without any criteria
        public static bool StartProcessSafely(
            ILog log, ProcessStartInfo startInfo)
        {
            StartProcessByOptions(log, startInfo);
            return true;
        }

        //hashType: SHA256 / SHA512
        public static bool StartProcessSafely(
            ILog log,
            ProcessStartInfo startInfo,
            bool needCheckThumbprintInbox = false,
            string fileHash = "",
            string hashType = "SHA512",
            bool isWaitExitCode = false,
            bool isLockNeeded = false)
        {
            string info = string.Empty;
            if (startInfo == null)
            {
                if (log != null)
                    log.Error("[StartProcessSafely] null process StartInfo");
                return false;
            }

            string filePath = startInfo.FileName;

            if (!IsProcessInfoValid(log, filePath, fileHash, hashType, needCheckThumbprintInbox))
                return false;

            return StartProcessByOptions(log, startInfo, "", "", isLockNeeded, isWaitExitCode);
        }

        //hashType: SHA256 / SHA512
        public static bool StartProcessSafely(
            ILog log,
            ProcessStartInfo startInfo,
            string fileHash = "",
            string hashType = "SHA512",
            bool isWaitExitCode = false,
            bool isLockNeeded = false,
            string givenThumbprintCheck = "")
        {
            string info = string.Empty;
            if (startInfo == null)
            {
                if (log != null)
                    log.Error("[StartProcessSafely] null process StartInfo");
                return false;
            }

            string filePath = startInfo.FileName;

            if (!IsProcessInfoValid(log, filePath, fileHash, hashType, false, givenThumbprintCheck))
                return false;

            return StartProcessByOptions(log, startInfo, "", "", isLockNeeded, isWaitExitCode);
        }

        public static X509Certificate2 LoadFileCertificate(string strFilePath)
        {
            X509Certificate2 certificate = new X509Certificate2(strFilePath);
            return certificate;
        }
    }
}
