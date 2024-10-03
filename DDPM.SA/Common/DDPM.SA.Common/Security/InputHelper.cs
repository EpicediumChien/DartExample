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
            int len = ProfileName.Length;
            if (len < 1 || len > 30)
            {
                info = $"InputValidation:  ({ProfileName}) length does not match";
                Console.WriteLine(" Fail " + info);
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
                Console.WriteLine("InputValidation_ProfileName: Fail " + ProfileName);
                return false;
            }
            Console.WriteLine("InputValidation_ProfileName: Pass");
            return true;
        }

        public static bool InputValidation_FilePathFileName(string filePathFileName, bool ImportExistFileTrue, out string info)
        {
            info = "Valid";
            PathCheckOption opt = PathCheckOption.None;
            if (ImportExistFileTrue == false)
            {
                opt = PathCheckOption.IgnoreFileExists;
            }

            if (!Settings.DDPMFileSecurity.IsFilePathValid(filePathFileName, opt, out info))
            {
                Console.WriteLine(info);
                return false;
            }

            if (!Settings.DDPMFileSecurity.IsPathSymbolicLinked(filePathFileName, out info))
            {
                Console.WriteLine(info);
                return false;
            }

            return true;
        }

        public static bool InputValidation_WebURL(string strURL, out string info)
        {
            info = "Valid";
            bool bResult = true;
            int len = strURL.Length;
            if (len < 1 || len > 8000)
            {
                info = $"InputValidation:  ({strURL}) length does not match";
                Console.WriteLine(" Fail " + info);
                return false;
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
                Console.WriteLine("InputValidation_ProfileName: Fail " + strURL);
                return false;
            }
            Console.WriteLine("InputValidation_WebURL: Pass");
            return true;
        }

        public static bool InputValidation_WirelessPWD(string strData, out string info)
        {
            info = "Valid";
            bool bResult = true;
            int len = strData.Length;
            if (len < 1 || len > 63)
            {
                info = $"InputValidation:  ({strData}) length does not match";
                Console.WriteLine(" Fail " + info);
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
                Console.WriteLine("InputValidation_ProfileName: Fail " + strData);
                return false;
            }
            Console.WriteLine("InputValidation_WirelessPWD: Pass");
            return true;
        }
    }
}
