using DDPM.SA.Obfuscation;
using Dell.Client.Framework.Security;
using Dell.Client.Framework.Security.Interfaces;
using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace DDPM.SA.Common.Settings
{
    public class ICC_SupportDeviceName
    {
        public string File { get; set; } = string.Empty;
        public string ColorPreset { get; set; } = string.Empty;
        public string SHA256 { get; set; } = string.Empty;
    }

    public class IIC_Metadata
    {
        public string Signature { get; set; } = string.Empty;
        public Dictionary<string, List<ICC_SupportDeviceName>> _support_ICC_DeviceName = new Dictionary<string, List<ICC_SupportDeviceName>>() { };
    }

    public class DDPMFileSecurity
    {
        //private static Log _log;
        /// <summary>
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
                Console.WriteLine("Data was not encrypted. An error occurred.");
                Console.WriteLine(e.ToString());
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
                Console.WriteLine("Data was not decrypted. An error occurred.");
                Console.WriteLine(e.ToString());
                return null;
            }
        }

        //need system privilege to query this string
        //public static string AppAccessInfo { get; } = SettingsAccess.AppAccessInfo;

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
        public static bool SetJsonContentFromSerializedString(string accessInfo, string serialized_string, string target_file, out string info, bool isEncrypt = false)
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
            try
            {
                //byte[] decrypted_data = Encoding.UTF8.GetBytes(serialized_string);
                //1. Calculate the HASH
                //byte[] hash_sign = GetSHA512(decrypted_data, 0, decrypted_data.Length);
                //signature = Encoding.UTF8.GetString(hash_sign);

                //0905 apply DDPM private key rule
                if (accessInfo == null || accessInfo.Length < 32)
                {
                    info = "DDPM AccessInfo value is abnormal";
                    return false;
                }
                //signature = SettingsAccess.GenerateAccessString(Encoding.UTF8.GetBytes(accessInfo), serialized_string);
                signature = SettingsAccess.ComputeAccessInfo2(Encoding.UTF8.GetBytes(accessInfo), serialized_string);
            }
            catch (Exception e)
            {
                info = "Calculate signature failed." + e.Message;
                return false;
            }
            if (string.IsNullOrEmpty(signature))
            {
                info = "Got null signature";
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
                    obj.Add("Signature", signature);
                    temp = obj.ToString();
                }
                else if (token.Type == JTokenType.Array)
                {
                    JArray array = (JArray)token;
                    // Handle array
                    //JObject newObject = new JObject();
                    //newObject["Signature"] = signature;
                    //array.Add(newObject);
                    array.Add("Signature : " + signature);
                    temp = array.ToString();
                }
                if(string.IsNullOrEmpty(temp))
                {
                    info = "Add sign to json object failed";
                    return false;
                }
                //2. Add signature
                //jObject.Add("Signature", signature);
                string modifiedJson = temp;// jObject.ToString();
                //convert whole content and protect it as bytes array
                byte[] body_array = Encoding.UTF8.GetBytes(modifiedJson);
                //3. Data protect if need
                if (isEncrypt)
                {
                    byte[] encrypted_data = DataProtect(body_array);
                    //From bytes array to base64 string
                    write_string = Convert.ToBase64String(encrypted_data);
                }
                else
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
            File.WriteAllText(target_file, write_string);
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
        public static string GetSerializedJsonString(string accessInfo, string filePath, out string info, bool isEncrypt = false)
        {
            info = "Success";
            if (!IsFilePathValid(filePath, out info))
            {
                info = $"[GetSerializedJsonString] {info}";
                return string.Empty;
            }
            if (accessInfo == null || accessInfo.Length < 32)
            {
                info = "DDPM AccessInfo value is abnormal";
                return string.Empty;
            }
            //1. Read json content
            string json_content = File.ReadAllText(filePath);
            if (string.IsNullOrEmpty(json_content))
            {
                info = "Null content of json file";
#if DEBUG 
                Console.WriteLine(info);
#endif 
                return string.Empty;
            }
            string serialized;
            try
            {
                //From base64 string to byte array
                if (isEncrypt)
                {
                    byte[] read_data = Convert.FromBase64String(json_content);
                    //2. assume input data already be encrypted, so decrypt it
                    byte[] decrypted_data = DataUnprotect(read_data);
                    //Serialized string with signature
                    serialized = Encoding.UTF8.GetString(decrypted_data);
                }
                else
                    //Serialized string with signature
                    serialized = json_content;
            }
            catch (Exception e)
            {
                info = e.Message;
                return string.Empty;
            }
            if (string.IsNullOrEmpty(serialized))
            {
                info = "Retrieve content of json file failed";
#if DEBUG 
                Console.WriteLine(info);
#endif
                return string.Empty;
            }
            // Parse the JSON string into a JObject
            string modifiedJson = string.Empty;
            string signature = string.Empty;
            JObject jObject;
            try
            {
                //jObject = JObject.Parse(serialized);
                JToken token = JToken.Parse(serialized);
                string temp = string.Empty;
                if (token.Type == JTokenType.Object)
                {
                    JObject obj = (JObject)token;
                    // Handle object
                    signature = (string)obj["Signature"];
                    // Remove the "signature" property for hash generating
                    if (!obj.Remove("Signature"))
                    {
                        info = "Remove signature field of json failed";
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
                    var signatureStrings = array.Where(token => token.Type == JTokenType.String && token.ToString().StartsWith("Signature"));
                    foreach (var sign in signatureStrings)
                    {
                        if (sign != null && !string.IsNullOrEmpty(sign.ToString()))
                        {
                            signature = sign.ToString().Replace("Signature", "").Trim();
                            if(signature.StartsWith(":"))
                            {
                                signature = signature.Substring(1).Trim();
                            }
                            array.Remove(sign);
                            break;
                        }
                    }

                    modifiedJson = array.ToString();
                }
                if (string.IsNullOrEmpty(signature))
                {
                    info = "No signature in json file";
#if DEBUG 
                    Console.WriteLine(info);
#endif
                    return string.Empty;
                }
                //jObject = (JObject)JsonConvert.SerializeObject(serialized, Formatting.Indented);
                //3. retrieve signature for comparison
                //signature = (string)jObject["Signature"];

                // Remove the "signature" property for hash generating
                //if (!jObject.Remove("Signature"))
                //{
                //    info = "Remove signature field of json failed";
                //    Console.WriteLine(info);
                //    return string.Empty;
                //}
            }
            catch (Exception ex)
            {
                info = "Get Signature from json fail.\nReason: " + ex.ToString();
#if DEBUG 
                Console.WriteLine(info);
#endif
                return string.Empty;
            }
            //if (jObject == null)// || jObject.Count == 0)
            //{
            //    info = "Convert from json content got no object";
            //    Console.WriteLine(info);
            //    return string.Empty;
            //}
            // Convert the modified JObject back to a JSON string
            string cal_sign;
            try
            {
                //0905 apply DDPM private key rule
            //    modifiedJson = jObject.ToString();
                //byte[] body_array = Encoding.UTF8.GetBytes(modifiedJson);
                //byte[] sign = GetSHA512(body_array, 0, body_array.Length);
                //cal_sign = Encoding.UTF8.GetString(sign);//target for comparison

                //cal_sign = SettingsAccess.GenerateAccessString(Encoding.UTF8.GetBytes(accessInfo), modifiedJson);
                cal_sign = SettingsAccess.ComputeAccessInfo2(Encoding.UTF8.GetBytes(accessInfo), modifiedJson);
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
            //4. Check if signature valid
            if (cal_sign.ToLower().Equals(signature.ToLower()))
                //5. return serialized string
                return modifiedJson;
            else
            {
                info = "Signature comparison result is FALSE";
                return string.Empty;
            }
        }

        public static bool Json_ExportSettingsToFileWithCheckSum(string path, DisplaySettings settings, out string info)
        {
            info = "Success";
            try
            {
                byte[] bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(settings));
                uint value2 = GetCheckSum(bytes, bytes.Length);

                using (FileStream fileStream = File.Create(path))
                {
                    fileStream.Write(bytes, 0, bytes.Length);
                    fileStream.Write(BitConverter.GetBytes(value2), 0, 4);
                    fileStream.Flush();
                    fileStream.Close();
                }
                //Elsa Add Security
                string FileInfo;
                if (!DDPMFileSecurity.IsFilePathValid(path, out FileInfo))
                {
                    info = $"[Json_ExportSettingsToFileWithCheckSum] {FileInfo}";
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                info = ex.Message;
            }
            return false;
        }

        public static bool Json_ExportSettingsToFileWithSha512(string path, DisplaySettings settings, out string info)
        {
            info = "Success";
            try
            {
                //DisplaySettings value = new DisplaySettings();
                byte[] bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(settings));
                byte[] sha = GetSHA512(bytes, 0, bytes.Length);

                using (FileStream fileStream = File.Create(path))
                {
                    fileStream.Write(bytes, 0, bytes.Length);
                    fileStream.Write(sha, 0, sha.Length);//normally 64bytes
                    fileStream.Flush();
                    fileStream.Close();
                }
                //Elsa Add Security
                string FileInfo;
                if (!DDPMFileSecurity.IsFilePathValid(path, out FileInfo))
                {
                    info = $"[Json_ExportSettingsToFileWithCheckSum] {FileInfo}";
                    return false;
                }
                
                return true;
            }
            catch (Exception ex)
            {
                info = ex.Message;
            }
            return false;
        }

        /// <summary>
        /// Output and do not specify the input data object
        /// </summary>
        /// <param name="path"></param>
        /// <param name="settings"></param>
        /// <param name="info"></param>
        /// <returns></returns>
        public static bool Json_ExportSettingsToFileWithSha512(string path, object settings, out string info)
        {
            info = "Success";
            try
            {
                //DisplaySettings value = new DisplaySettings();
                byte[] bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(settings));
                byte[] sha = GetSHA512(bytes, 0, bytes.Length);

                using (FileStream fileStream = File.Create(path))
                {
                    fileStream.Write(bytes, 0, bytes.Length);
                    fileStream.Write(sha, 0, sha.Length);//normally 64bytes
                    fileStream.Flush();
                    fileStream.Close();
                }
                //Elsa Add Security
                string FileInfo;
                if (!DDPMFileSecurity.IsFilePathValid(path, out FileInfo))
                {
                    info = $"[Json_ExportSettingsToFileWithSha512] {FileInfo}";
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                info = ex.Message;
            }
            return false;
        }

        public static bool Json_ExportSettingsToFileWithoutSignature(string path, DisplaySettings settings, out string info)
        {
            info = "Success";
            try
            {
                byte[] bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(settings));

                using (FileStream fileStream = File.Create(path))
                {
                    fileStream.Write(bytes, 0, bytes.Length);
                    //fileStream.Write(BitConverter.GetBytes(value2), 0, 4);
                    fileStream.Flush();
                    fileStream.Close();
                }
                //Elsa Add Security
                string FileInfo;
                if (!DDPMFileSecurity.IsFilePathValid(path, out FileInfo))
                {
                    info = $"[Json_ExportSettingsToFileWithoutSignature] {FileInfo}";
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                info = ex.Message;
            }
            return false;
        }

        public static DisplaySettings Json_ImportSettingsWithoutSignature(string path, out string info)
        {
            info = "Success";
            DisplaySettings dDMSettings = null;
            try
            {
                if (File.Exists(path))
                {
                    //Elsa Add Security
                    string FileInfo;
                    if (!DDPMFileSecurity.IsFilePathValid(path, out FileInfo))
                    {
                        info = $"[Json_ImportSettingsWithoutSignature] {FileInfo}";
                        return null;
                    }
                    int num = (int)new FileInfo(path).Length;
                    if (num > 0)
                    {
                        byte[] array = File.ReadAllBytes(path);
                        dDMSettings = JsonConvert.DeserializeObject<DisplaySettings>(Encoding.UTF8.GetString(array, 0, array.Length));
                        return dDMSettings;
                    }
                    else
                    {
                        info = "File content is abnormal";
                        return null;
                    }
                }
                else
                {
                    info = "File isn't exist";
                    return null;
                }
            }
            catch (Exception ex)
            {
                info = ex.Message;
            }
            return null;
        }

        public static DisplaySettings Json_ImportSettingsAndCheckCheckSum(string path, out string info)
        {
            info = "Success";
            DisplaySettings dDMSettings = null;
            try
            {
                if (File.Exists(path))
                {
                    //Elsa Add Security
                    string FileInfo;
                    if (!DDPMFileSecurity.IsFilePathValid(path, out FileInfo))
                    {
                        info = $"[Json_ImportSettingsAndCheckCheckSum] {FileInfo}";
                        return null;
                    }

                    int num = (int)new FileInfo(path).Length;
                    if (num > 4)
                    {
                        byte[] array = File.ReadAllBytes(path);
                        if (GetCheckSum(array, num - 4).Equals(BitConverter.ToUInt32(array, num - 4)))
                        {
                            dDMSettings = JsonConvert.DeserializeObject<DisplaySettings>(Encoding.UTF8.GetString(array, 0, array.Length - 4));
                            return dDMSettings;
                        }
                        else
                        {
                            info = "Checksum is different";
                            return null;
                        }
                    }
                    else
                    {
                        info = "File content is abnormal";
                        return null;
                    }
                }
                else
                {
                    info = "File isn't exist";
                    return null;
                }
            }
            catch (Exception ex)
            {
                info = ex.Message;
            }
            return null;
        }

        public static DisplaySettings Json_ImportSettingsAndCheckSha512(string path, out string info)
        {
            info = "Success";
            DisplaySettings dDMSettings = null;
            try
            {
                if (File.Exists(path))
                {
                    //Elsa Add Security
                    string FileInfo;
                    if (!DDPMFileSecurity.IsFilePathValid(path, out FileInfo))
                    {
                        info = $"[Json_ImportSettingsAndCheckSha512] {FileInfo}";
                        return null;
                    }
                    int num = (int)new FileInfo(path).Length;
                    if (num > 64)
                    {
                        byte[] array = File.ReadAllBytes(path);
                        byte[] arrByte2 = new byte[64];
                        Buffer.BlockCopy(array, num - 64, arrByte2, 0, 64);
                        if (CompareByteArrays(GetSHA512(array, 0, num - 64), arrByte2))
                        {
                            dDMSettings = JsonConvert.DeserializeObject<DisplaySettings>(Encoding.UTF8.GetString(array, 0, array.Length - 64));
                            return dDMSettings;
                        }
                        else
                        {
                            info = "SHA512 is different";
                            return null;
                        }
                    }
                    else
                    {
                        info = "File content is abnormal";
                        return null;
                    }
                }
                else
                {
                    info = "File isn't exist";
                    return null;
                }
            }
            catch (Exception ex)
            {
                info = ex.Message;
            }
            return null;
        }

        public static bool LoadFileToVerifyJson(string jsonfilepath, string publickeyfilepath, out string strJson)
        {
            //1.Load public key from file (public_key.txt) --> verify signature with input json file via public key.
            //2.Load public key from file (public_key.cer, it could be DER or PEM format) --> verify signature with input json file via public key.
            string json_file = jsonfilepath;
            string public_key = publickeyfilepath;
            string info = string.Empty;
            strJson = string.Empty;

            if (!DDPMFileSecurity.CheckFileACL(json_file, out info, true))
            {
#if DEBUG 
                Console.WriteLine($"File: {json_file}\nFail with [{info}]");
#endif
                return false;
            }
            if (!File.Exists(public_key))
            {
#if DEBUG 
                Console.WriteLine($"Please check if public keys exists");
#endif
                return false;
            }
            //Read json content
            string json_content = File.ReadAllText(json_file);

            // Parse the JSON string into a JObject
            JObject jObject = JObject.Parse(json_content);
            string modifiedJson;
            string signature;
            try
            {
                signature = (string)jObject["Signature"];
                // Remove the "age" property
                jObject.Remove("Signature");
                // Convert the modified JObject back to a JSON string
                modifiedJson = jObject.ToString();
                strJson = modifiedJson;
            }
            catch (Exception ex)
            {
#if DEBUG 
                Console.WriteLine("Try to get Signature from json fail.\nReason: " + ex.ToString());
#endif
                return false;
            }

            //use signature to verify json
            if (!DDPMFileSecurity.IsJsonContentValid(modifiedJson, signature, public_key, HashAlgorithmName.SHA512, out info))
            {
#if DEBUG
                Console.WriteLine($"Validate json content with signature failed\nReason: {info}");
#endif
                return false;
            }
#if DEBUG
            Console.WriteLine("Operation completed");
#endif
            return true;
        }

        public static uint GetCheckSum(byte[] content, int count)
        {
            uint num = 0u;
            for (int i = 0; i < count; i++)
            {
                num += content[i];
            }
            return num;
        }

        public static byte[] GetSHA256(byte[] message, int offset, int count)
        {
            using SHA256 sHA = SHA256.Create();
            return sHA.ComputeHash(message, offset, count);
        }

        /// <summary>
        /// This function is used to provide hash as file checksum or json content signature
        /// </summary>
        /// <param name="message"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public static byte[] GetSHA512(byte[] message, int offset, int count)//output 64bytes=512bits
        {
            using SHA512 sHA = SHA512.Create();
            return sHA.ComputeHash(message, offset, count);
        }

        private static bool CompareByteArrays(byte[] array1, byte[] array2)
        {
            return array1.SequenceEqual(array2);
        }
        /*
        /// <summary>
        /// Encrypt bytes content with RSA key
        /// </summary>
        /// <param name="dataToEncrypt"></param>
        /// <param name="outputFilePath">if null or empty then do not write to file</param>
        /// <param name="publicKey"></param>
        /// <returns></returns>
        public static byte[] RsaEncryptByteArrayOverRsa(byte[] dataToEncrypt, string outputFilePath, string publicKey)
        {
            //byte[] dataToEncrypt = File.ReadAllBytes(inputFilePath);
            try
            {
                using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(4096))
                {
                    rsa.FromXmlString(publicKey);
                    byte[] encryptedData = rsa.Encrypt(dataToEncrypt, true);
                    if (!string.IsNullOrEmpty(outputFilePath))
                        File.WriteAllBytes(outputFilePath, encryptedData);

                    return encryptedData;
                }
            }
            catch (Exception)// ex)
            {
            }
            return null;
        }*/
        /*
        /// <summary>
        /// Decrypt bytes content with RSA key
        /// </summary>
        /// <param name="dataToDecrypt"></param>
        /// <param name="outputFilePath">if null or empty then do not write to file</param>
        /// <param name="privateKey"></param>
        /// <returns></returns>
        public static byte[] RsaDecryptByteArrayOverRsa(byte[] dataToDecrypt, string outputFilePath, string privateKey)
        {
            //byte[] dataToDecrypt = File.ReadAllBytes(inputFilePath);
            try
            {
                using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(4096))
                {
                    rsa.FromXmlString(privateKey);
                    byte[] decryptedData = rsa.Decrypt(dataToDecrypt, true);
                    if (!string.IsNullOrEmpty(outputFilePath))
                        File.WriteAllBytes(outputFilePath, dataToDecrypt);

                    return decryptedData;
                }
            }
            catch (Exception)// ex)
            {
            }
            return null;
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
                return false;
            }
            return true;
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

            // Get file's security content
            //FileSecurity fileSecurity = fileInfo.GetAccessControl();
            // Create rules for setting file
            //var usersReadRule = new FileSystemAccessRule(new SecurityIdentifier(WellKnownSidType.BuiltinUsersSid, null), FileSystemRights.Read, AccessControlType.Allow);
            //var usersWriteRule = new FileSystemAccessRule(new SecurityIdentifier(WellKnownSidType.BuiltinUsersSid, null), FileSystemRights.Write, AccessControlType.Allow);
            //var usersRule = new FileSystemAccessRule(new SecurityIdentifier(WellKnownSidType.BuiltinUsersSid, null), FileSystemRights.FullControl, AccessControlType.Allow);
            //var systemRule = new FileSystemAccessRule(new SecurityIdentifier(WellKnownSidType.LocalSystemSid, null), FileSystemRights.FullControl, AccessControlType.Allow);
            //var adminRule = new FileSystemAccessRule(new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, null), FileSystemRights.Read | FileSystemRights.Write, AccessControlType.Allow);

            FileSecurity fileSecurity = new FileSecurity();
            fileSecurity.SetAccessRuleProtection(isProtected: true, preserveInheritance: false);
            fileSecurity.AddAccessRule(new FileSystemAccessRule(LocalAccounts.Users.LocalSystemSid, FileSystemRights.FullControl, InheritanceFlags.None, PropagationFlags.None, AccessControlType.Allow));
            fileSecurity.AddAccessRule(new FileSystemAccessRule(LocalAccounts.Groups.BuiltinAdminsSid, FileSystemRights.FullControl, InheritanceFlags.None, PropagationFlags.None, AccessControlType.Allow));
            fileSecurity.AddAccessRule(new FileSystemAccessRule(LocalAccounts.Groups.BuiltinUsersSid, FileSystemRights.Read, InheritanceFlags.None, PropagationFlags.None, AccessControlType.Allow));
            fileSecurity.AddAccessRule(new FileSystemAccessRule(LocalAccounts.Groups.BuiltinUsersSid, FileSystemRights.Write, InheritanceFlags.None, PropagationFlags.None, AccessControlType.Allow));
            fileSecurity.SetOwner(LocalAccounts.Groups.BuiltinAdminsSid);

            try
            {
                // check if can apply rules
                /*SetAccessRuleIfNotExists(ref fileSecurity, systemRule);
                if (!isDebug)
                {
                    //for release build please use this rule for normal user
                    SetAccessRuleIfNotExists(ref fileSecurity, usersReadRule);
                    SetAccessRuleIfNotExists(ref fileSecurity, usersWriteRule);
                }
                else
                    //debug purpose that apply all right for user
                    SetAccessRuleIfNotExists(ref fileSecurity, usersRule);

                SetAccessRuleIfNotExists(ref fileSecurity, adminRule);*/

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

        /*public static bool CheckIfFileCanBeExecuted_Secure(string executablePath, bool NeedElevated = false)
        {
            if (string.IsNullOrEmpty(executablePath))
            {
                throw new ArgumentException("Empty file path.");
            }
            try
            {
                string filePath = executablePath.Trim();

                // Check if file path is valid
                if (string.IsNullOrWhiteSpace(filePath) || !Path.IsPathRooted(filePath) || filePath.IndexOfAny(Path.GetInvalidPathChars()) != -1)
                {
                    _log.Info($"{nameof(CheckIfFileCanBeExecuted_Secure)} -- Invalid file path.");
                    throw new ArgumentException("Invalid file path.");
                }

                // Check if file exist
                if (!File.Exists(filePath))
                {
                    _log.Info($"{nameof(CheckIfFileCanBeExecuted_Secure)} -- File isn't exist.");
                    throw new ArgumentException("File isn't exist. ");
                }

                // Perform Input Validation: check file path
                if (PathHelper.ValidateFilePath(filePath, PathCheckOption.None) != PathCheckErrorCodes.SUCCESS)
                {
                    _log.Info($"{nameof(CheckIfFileCanBeExecuted_Secure)} -- Invalid file path string - {filePath}");
                    throw new ArgumentException($"Invalid file path string - {filePath}");
                }

                // Prevent Path Traversal: check redirection
                if (PathHelper.CheckPathRedirection(filePath) != PathRedirectionReturn.PathIsNormal)
                {
                    _log.Info($"{nameof(CheckIfFileCanBeExecuted_Secure)} -- Redirection detected along file path - {filePath}");
                    throw new PathCheckRedirectionException($"Redirection detected along file path - {filePath}");
                }

                if (NeedElevated)
                {
                    // Check if file is located in an elevated location
                    var permissionSet = new PermissionSet(PermissionState.None);
                    permissionSet.AddPermission(new FileIOPermission(FileIOPermissionAccess.Read, filePath));
                    if (!permissionSet.IsSubsetOf(AppDomain.CurrentDomain.PermissionSet))
                    {
                        throw new SecurityException("File is not located in an elevated location.");
                    }
                }

                // Check if the executable has a valid certificate
                X509Certificate2 cert = GetCertificate(filePath);
                if (cert == null)
                {
                    throw new SecurityException("The executable does not have a valid certificate.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                //throw new SecurityException(ex.Message);
                return false;
            }
            return true;
        }*/

        private static X509Certificate2 GetCertificate(string filePath)
        {
            X509Certificate2? cert = null;

            try
            {
                //Dean 0911: only load file's cert, basic function
                //Elsa Add Security
                //string FileInfo;
                //if (!IsFilePathValid(filePath, out FileInfo))
                //{
                //    _log.Info($"{nameof(GetCertificate)} {FileInfo}");
                //    return cert;
                //}

                // Load the executable into a byte array
                //byte[] fileBytes = File.ReadAllBytes(filePath);

                // Load the executable as an X509Certificate2 object
                cert = new X509Certificate2(filePath); //fileBytes);

                // Validate the certificate
                if (!cert.Verify())
                {
                    cert = null;
                }
            }
            catch
            {
                cert = null;
            }

            return cert;
        }

        //Hard code for test
        //private static string _Sha256SubjectPublicKeyInfoHash = "1d58d1d2bbebc4f3c8169c17c75086b38348e1bcfe0210b21518d32e1301d763";

        /*public static bool CheckIsValidFile_Secure(string JsonPath)//, string rsa_key_public)
        {
            if (string.IsNullOrEmpty(JsonPath))
            {
                throw new ArgumentException("Empty file path.");
            }
            try
            {
                string filePath = JsonPath.Trim();

                //
                //   STEP 1: Create our Authenticode signature verifier
                //
                //   SDL Checklist: Follow Best Practices for Crypto and Security Protocols, Ensure Proper Authentication
                //
                VerifierOption myVerifierOptions = VerifierOption.FailOnNoErrorsAndSelfSignedCert;     // fails validation on all errors or if the signing certificate was self signed

                SubjectPublicKeyInfoHashes hashes = new SubjectPublicKeyInfoHashes(HashType.Sha256);     // object for storing our SHA256 subject public key info hash

                //hashes.Add(_Sha256SubjectPublicKeyInfoHash);     // adding our pre-computed sha256 hash to the collection

                var constraints = new LeafCertConstraints(hashes)     // create our LeafCertConstraint using our hash "collection" object
                {
                    RequireAllCerts = false     // we only expect one signing certificate (file should not be multi-signed)
                };

                PeAuthenticodeVerifier verifier = new PeAuthenticodeVerifier(myVerifierOptions, omitDefaultOptions: true)     // create our verifier
                {
                    Constraints = constraints     // pass in our LeafCertConstraints that contains our pre-computed sha256 subject public key info hash
                };

                //
                //   STEP 2: Check our path string for invalid characters, null value, empty value, etc.
                //
                //   SDL Checklist: Perform Input Validation
                //
                if (PathHelper.ValidateFilePath(filePath, PathCheckOption.None) != PathCheckErrorCodes.SUCCESS)
                {
                    _log.Info($"{nameof(CheckIsValidFile_Secure)} -- Invalid file path string - {filePath}");
                    throw new ArgumentException($"Invalid file path string - {filePath}");
                }

                //
                //   STEP 3: Check for path redirection (symlink, mountpoint, hardlink, etc.) at the path AND along the path
                //
                //   SDL Checklist: Prevent Path Traversal
                //
                if (PathHelper.CheckPathRedirection(filePath) != PathRedirectionReturn.PathIsNormal)
                {
                    _log.Info($"{nameof(CheckIsValidFile_Secure)} -- Redirection detected along file path - {filePath}");
                    throw new PathCheckRedirectionException($"Redirection detected along file path - {filePath}");
                }

                //
                //   STEP 4: Lock the file using Security Library FileLock class
                //
                //   SDL Checklist: Ensure Authorization and Access Controls (takes care of TOCTOU), Protect Against Brute Force Attacks
                //
                using (FileLock fileLock = new FileLock(filePath, PathCheckOption.None, lockNow: true))     // file lock protects us from TOCTOU attacks
                {
                    //
                    //   STEP 5: Verify file ACLs
                    //
                    //   SDL Checklist: Ensure Authorization and Access Controls
                    //
                    AclChecker aclChecker = new AclChecker();
                    if (aclChecker.ContainsUnprivilegedWriteAccess(fileLock))
                    {
                        throw new SecurityException($"File ACLs for {filePath} contained unprivileged write access for one or more identity");
                    }

                    //
                    //   STEP 6: Verify signature of signing certificate
                    //
                    //   SDL Checklist: Follow Best Practices for Crypto and Security Protocols, Ensure Proper Authentication
                    //
                    var result = verifier.Verify(fileLock);
                    if (result != Win32ErrorCodes.ERROR_SUCCESS)
                    {
                        throw new SecurityException($"Signature validation failed for {filePath}! Received the following return code {result}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw new SecurityException(ex.Message);
            }

            return true;
        }*/

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
            byte[] data = File.ReadAllBytes(filePath);
            if (data == null)
            {
                info = $"Read data from file path - {filePath}, failed";
                return null;
            }
            //Bruce 0909 modify
            //byte[] result = CryptoHelper.GenerateHashBytes(data, HashType.Sha256);
            string result = CalculateFileSHA256(filePath);
            info = "Complete";
            if (string.IsNullOrEmpty(result))
            {
                info = "Calculate fail.";
            }
            return result;
        }
        static string CalculateFileSHA256(string filePath)
        {
            string ret = string.Empty;
            using (FileStream fileStream = File.OpenRead(filePath))
            {
                using (SHA256 sha256 = SHA256.Create())
                {
                    byte[] hashBytes = sha256.ComputeHash(fileStream);

                    // 將計算的雜湊值轉換為十六進制字符串
                    ret = BitConverter.ToString(hashBytes).Replace("-", "");
                }
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
            byte[] data = File.ReadAllBytes(filePath);
            if (data == null)
            {
                info = $"Read data from file path - {filePath}, failed";
                return null;
            }
            //Bruce 0909 modify
            //byte[] result = CryptoHelper.GenerateHashBytes(data, HashType.Sha512);
            string result = CalculateFileSHA512(filePath);
            info = "Complete";
            if (string.IsNullOrEmpty(result))
            {
                info = "Calculate fail.";
            }
            return result;
        }
        static string CalculateFileSHA512(string filePath)
        {
            string ret = string.Empty;
            using (FileStream fileStream = File.OpenRead(filePath))
            {
                using (SHA512 sha512 = SHA512.Create())
                {
                    byte[] hashBytes = sha512.ComputeHash(fileStream);

                    // 將計算的雜湊值轉換為十六進制字符串
                    ret = BitConverter.ToString(hashBytes).Replace("-", "");
                }
            }
            return ret;
        }

        /*public static bool IsContainValidDigitalSignature(string filePath, out string info)
        {
            info = "";
            // Check our path string for invalid characters, null value, empty value, etc.
            if (PathHelper.ValidateFilePath(filePath, PathCheckOption.None) != PathCheckErrorCodes.SUCCESS)
            {
                info = $"Invalid file path string - {filePath}";
                _log.Info(info);
                return false;
            }

            //Check for path redirection (symlink, mountpoint, hardlink, etc.) at the path AND along the path
            if (PathHelper.CheckPathRedirection(filePath) != PathRedirectionReturn.PathIsNormal)
            {
                info = $"Redirection detected along file path - {filePath}";
                _log.Info(info);
                return false;
            }

            // Check if the executable(dll/exe) has a valid certificate
            X509Certificate2 cert = GetCertificate(filePath);
            if (cert == null)
            {
                info = "The executable does not have a valid certificate.";
                return false;
            }
            return true;
        }*/

        //using private key and source json file to create signature output file
        /*public static bool CreateJsonSignature(string json_content, string private_key_file, string target_sign_file, HashAlgorithmName algorithm, out string info)
        {
            info = "unknow error";

            try
            {
                // Read the base64-encoded private key from a text file
                string privateKeyBase64 = File.ReadAllText(private_key_file);

                // Load the private key (from base64)
                byte[] privateKeyBytes = Convert.FromBase64String(privateKeyBase64);
                using (var rsa = RSA.Create())
                {
                    rsa.ImportRSAPrivateKey(privateKeyBytes, out _);

                    // Sign the data
                    byte[] dataBytes = Encoding.UTF8.GetBytes(json_content);
                    byte[] signature = rsa.SignData(dataBytes, algorithm, RSASignaturePadding.Pkcs1);

                    // Encode the signature in base64
                    string base64Signature = Convert.ToBase64String(signature);

                    Console.WriteLine("Signature (base64):");
                    Console.WriteLine(base64Signature);

                    File.WriteAllText(target_sign_file, base64Signature);
                    info = "Completed";
                    return true;
                }
            }
            catch (Exception ex)
            {
                info = ex.Message;
                return false;
            }
        }*/

        //Using public key and pre-generated signature to validate json file
        public static bool IsJsonContentValid(string json_content, string base64_signature, string public_key_file, HashAlgorithmName algorithm, out string info)
        {
            info = "unknow error";

            // Read the base64-encoded public key from a text file
            string publicKeyBase64 = File.ReadAllText(public_key_file);

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

        //for test purpose to generate public and private key pair, method 1
        /*public static bool GenerateNewRSAKeyPair(string publicName, string privateName, out string privateKey, out string publicKey)
        {
            using (var rsa = RSA.Create(4096))
            {
                privateKey = Convert.ToBase64String(rsa.ExportRSAPrivateKey());
                publicKey = Convert.ToBase64String(rsa.ExportRSAPublicKey());

                if (File.Exists(publicName))
                {
                    File.Delete(publicName);
                    Console.WriteLine($"File {publicName} exist, delete it.");
                }
                if (File.Exists(privateName))
                {
                    File.Delete(privateName);
                    Console.WriteLine($"File {privateName} exist, delete it.");
                }
                // save keys
                File.WriteAllText(publicName, publicKey);// "public_key.xml", publicKey);

                if (File.Exists(publicName))
                {
                    Console.WriteLine($"Public key File {publicName}.");
                }
                else
                {
                    Console.WriteLine($"Save public key file failed.");
                    return false;
                }

                File.WriteAllText(privateName, privateKey);// "private_key.xml", privateKey);

                if (File.Exists(privateName))
                {
                    Console.WriteLine($"Private key File {privateName}.");
                }
                else
                {
                    Console.WriteLine($"Save private key file failed.");
                    return false;
                }
                return true;
            }
        }*/

        //for test purpose to generate public and private key pair, method 2
        /*private static bool GenerateNewRSAKeyPair(string publicName, string privateName)
        {
            if (File.Exists(publicName))
            {
                File.Delete(publicName);
                Console.WriteLine($"File {publicName} exist, delete it.");
            }
            if (File.Exists(privateName))
            {
                File.Delete(privateName);
                Console.WriteLine($"File {privateName} exist, delete it.");
            }
            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(4096))
            {
                string publicKey = rsa.ToXmlString(false);
                string privateKey = rsa.ToXmlString(true);

                Console.WriteLine("RSA keys generated.");
                // save keys
                File.WriteAllText(publicName, publicKey);// "public_key.xml", publicKey);

                if (File.Exists(publicName))
                {
                    Console.WriteLine($"Public key File {publicName}.");
                }
                else
                {
                    Console.WriteLine($"Save public key file failed.");
                    return false;
                }

                File.WriteAllText(privateName, privateKey);// "private_key.xml", privateKey);

                if (File.Exists(privateName))
                {
                    Console.WriteLine($"Private key File {privateName}.");
                }
                else
                {
                    Console.WriteLine($"Save private key file failed.");
                    return false;
                }
            }
            return true;
        }*/

        /*public static void DirectoryLockTest(string folderPath)
        {
            try
            {
                // Create a fileSteam and keep it open
                using (var fileStream = new FileStream(Path.Combine(folderPath, "lockfile.lock"), FileMode.Create, FileAccess.ReadWrite, FileShare.None))
                {
                    // any action here
                    Console.WriteLine("Folder locked.");
                    //Console.ReadLine();
                }

                // Delete lockfile.lock to unlock folder
                File.Delete(Path.Combine(folderPath, "lockfile.lock"));

                Console.WriteLine("Folder unlock");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error：{ex.Message}");
            }
        }*/
        /*
        //public static X509Certificate2 LoadCertificate(string filePath)
        //{
        //    byte[] certBytes = File.ReadAllBytes(filePath);
        //    return new X509Certificate2(certBytes);
        //}*/

        private static byte[] ConvertThumbprintToByteArray(string thumbprint)
        {
            return Enumerable.Range(0, thumbprint.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(thumbprint.Substring(x, 2), 16))
                             .ToArray();
        }

        public static bool VerifyFileCertWithThumbprint(string filePath, out string info)
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
                X509Certificate2 cert = new X509Certificate2(filePath);
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
            //NTAccount f = new NTAccount(accountName);
            //writelog($"GetUserSid: final using: {accountName}");
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
            if (DDPMFileSecurity.IsPathSymbolicLinked(filePath, out info))  // filePath contain symbolic
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
            if (IsPathSymbolicLinked(filePath, out info))  // filePath contain symbolic
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
                    Directory.Delete(folderPath, true);
                    Directory.CreateDirectory(folderPath);
                    count++;
                }
            } while (!folderValid && count < 2);
            return folderValid;
        }

    }
}
