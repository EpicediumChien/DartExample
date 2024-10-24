using Dell.Client.Framework.Security.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DDPM.SA.Common.Security
{
    public class InputHelper
    {
        static string ReplaceSensitiveContent(string input, string[] sensitiveWords)
        {
            foreach (var word in sensitiveWords)
            {
                string pattern = Regex.Escape(word);
                string replacement = new string('*', word.Length); // replace keyword by '*'
                input = Regex.Replace(input, pattern, replacement, RegexOptions.IgnoreCase);
            }
            return input;
        }

        public static bool InputValidation_ProfileName(string ProfileName, out string info)
        {
            info = "Valid";
            bool bResult = true;
            if (string.IsNullOrEmpty(ProfileName))
            {
                info = $"InputValidation: ProfileName NULL";
                return false;
            }
            
            int len = ProfileName.Length;
            if (len < 1 || len > 30)
            {
                info = $"InputValidation:  ({ProfileName}) length does not match";
#if DEBUG
                Console.WriteLine(" Fail " + info);
#endif
                return false;
            }

            for (int idx = 0; idx < len; idx++)
            {
                if (ProfileName[idx] >= 'a' && ProfileName[idx] <= 'z')
                    continue;
                else if (ProfileName[idx] >= 'A' && ProfileName[idx] <= 'Z')
                    continue;
                else if (ProfileName[idx] >= '0' && ProfileName[idx] <= '9')
                    continue;
                else if (ProfileName[idx] == '@' || ProfileName[idx] == ' ' || ProfileName[idx] == '-')
                    continue;
                else
                {
                    bResult = false;
                    break;
                }
            }

            if (!bResult)
            {
                info = $"InputValidation:  ({ProfileName}) does not match";
#if DEBUG
                Console.WriteLine("InputValidation_ProfileName: Fail " + ProfileName);
#endif
                return false;
            }
#if DEBUG
            Console.WriteLine("InputValidation_ProfileName: Pass");
#endif
            return true;
        }

        public static bool InputValidation_FilePathFileName(string filePathFileName, bool ImportExistFileTrue, out string info)
        {
			
			// add start @ 20241022 stephen: pass check
            if ((@$"x:\config.json").ToLower().Equals(filePathFileName.ToLower()))
            {
                info = $"x:\\config.json is a special case to pass checking.";
                return true;
            }
            // add end
			
            info = "Valid";
            PathCheckOption opt = PathCheckOption.None;
            if (ImportExistFileTrue == false)
            {
                opt = PathCheckOption.IgnoreFileExists;
            }

            if (string.IsNullOrEmpty(filePathFileName))
            {
                info = $"InputValidation: filePathFileName NULL";
                return false;
            }

            if (!Settings.DDPMFileSecurity.IsFilePathValid(filePathFileName, opt, out info))
            {
#if DEBUG 
                Console.WriteLine(info);
#endif
                return false;
            }

            if (Settings.DDPMFileSecurity.IsPathSymbolicLinked(filePathFileName, out info))
            {
#if DEBUG
                Console.WriteLine(info);
#endif
                return false;
            }

            string filename = System.IO.Path.GetFileNameWithoutExtension(filePathFileName);
            if ( string.IsNullOrEmpty(filename))
            {
                info = "File name - Invalid.";
                return false;
            }
            if (filename.IndexOfAny(System.IO.Path.GetInvalidFileNameChars()) >= 0)
            {
                info = "File name - Invalid File Name Char.";
                return false;
            }

            if (filename.Equals("CON") || filename.Equals("PRN") || filename.Equals("AUX") || filename.Equals("NUL") ||
                filename.Equals("COM0") || filename.Equals("COM1") || filename.Equals("COM2") || filename.Equals("COM3") ||
                filename.Equals("COM4") || filename.Equals("COM5") || filename.Equals("COM6") || filename.Equals("COM7") ||
                filename.Equals("COM8") || filename.Equals("COM9") || filename.Equals("LPT0") || filename.Equals("LPT1") ||
                filename.Equals("LPT2") || filename.Equals("LPT3") || filename.Equals("LPT4") || filename.Equals("LPT5") ||
                filename.Equals("LPT6") || filename.Equals("LPT7") || filename.Equals("LPT8") || filename.Equals("LPT9"))
            {
                info = $"File name - ({filename}) reserved character.";
                return false;
            }

            return true;
        }

        public static bool InputValidation_WebURL(string strURL, out string info)
        {
            info = "Valid";
            bool bResult = true;

            if (string.IsNullOrEmpty(strURL))
            {
                info = $"InputValidation: URL NULL";
                return false;
            }

            int len = strURL.Length;
            if (len < 1 || len > 8000)
            {
                info = $"InputValidation:  ({strURL}) length does not match";
#if DEBUG
                Console.WriteLine(" Fail " + info);
#endif
                return false;
            }
            Uri absUri;
            if (!Uri.TryCreate(strURL, UriKind.Absolute, out absUri))
            {
                info = $"Only Absolute URL allowed.";
                return false;
#if DEBUG
                Console.WriteLine("fail " + absUri);
#endif
            }
            else
            {
                if(!((absUri.Scheme == Uri.UriSchemeHttp) || (absUri.Scheme == Uri.UriSchemeHttps )) )
                {
                    info = $"Only HTTP HTTPs URL allowed.";
                    return false;
                }
            }

            for (int idx = 0; idx < len; idx++)
            {
                if (strURL[idx] >= 'a' && strURL[idx] <= 'z')
                    continue;
                else if (strURL[idx] >= 'A' && strURL[idx] <= 'Z')
                    continue;
                else if (strURL[idx] >= '0' && strURL[idx] <= '9')
                    continue;
                else if (strURL[idx] == '@' || strURL[idx] == ' ' || strURL[idx] == '-' || strURL[idx] == '_')
                    continue;
                else if (strURL[idx] == '.' || strURL[idx] == '~' || strURL[idx] == ':' || strURL[idx] == '/')
                    continue;
                else if (strURL[idx] == '?' || strURL[idx] == '&' || strURL[idx] == '=' || strURL[idx] == '#')
                    continue;
                else if (strURL[idx] == '%' || strURL[idx] == '+' || strURL[idx] == '[' || strURL[idx] == ']')
                    continue;
                else if (strURL[idx] == '!' || strURL[idx] == '$' || strURL[idx] == '(' || strURL[idx] == ')')
                    continue;
                else if (strURL[idx] == '*' || strURL[idx] == ':')
                    continue;
                else
                {
                    bResult = false;
                    break;
                }
            }

            if (!bResult)
            {
                info = $"InputValidation:  ({strURL}) does not match";
#if DEBUG
                Console.WriteLine("InputValidation_ProfileName: Fail " + strURL);
#endif
                return false;
            }
#if DEBUG
            Console.WriteLine("InputValidation_WebURL: Pass");
#endif
            return true;
        }

        public static bool InputValidation_WirelessPWD(string strData, out string info)
        {
            info = "Valid";
            bool bResult = true;

            if (string.IsNullOrEmpty(strData))
            {
                info = $"InputValidation: WLAN PWD NULL";
                return false;
            }

            int len = strData.Length;
            if (len < 1 || len > 63)
            {
                info = $"InputValidation:  ({strData}) length does not match";
#if DEBUG
                Console.WriteLine(" Fail " + info);
#endif
                return false;
            }

            for (int idx = 0; idx < len; idx++)
            {
                if (strData[idx] >= 'a' && strData[idx] <= 'z')
                    continue;
                else if (strData[idx] >= 'A' && strData[idx] <= 'Z')
                    continue;
                else if (strData[idx] >= '0' && strData[idx] <= '9')
                    continue;
                else if (strData[idx] == '!' || strData[idx] == '"' || strData[idx] == '#' || strData[idx] == '$')
                    continue;
                else if (strData[idx] == '%' || strData[idx] == '&' || strData[idx] == '\'' || strData[idx] == '(')
                    continue;
                else if (strData[idx] == ')' || strData[idx] == '*' || strData[idx] == '+' || strData[idx] == ' ')
                    continue;
                else if (strData[idx] == '-' || strData[idx] == '.' || strData[idx] == '/' || strData[idx] == ':')
                    continue;
                else if (strData[idx] == ';' || strData[idx] == '<' || strData[idx] == '>' || strData[idx] == '?')
                    continue;
                else if (strData[idx] == '@' || strData[idx] == '[' || strData[idx] == ']' || strData[idx] == '|')
                    continue;
                else if (strData[idx] == '~' || strData[idx] == '{' || strData[idx] == '}')
                    continue;
                else
                {
                    bResult = false;
                    break;
                }
            }

            if (!bResult)
            {
                info = $"InputValidation:  ({strData}) does not match";
#if DEBUG
                Console.WriteLine("InputValidation_ProfileName: Fail " + strData);
#endif
                return false;
            }
#if DEBUG
            Console.WriteLine("InputValidation_WirelessPWD: Pass");
#endif
            return true;
        }
    }
}
