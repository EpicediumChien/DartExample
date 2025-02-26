using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDPM.UI.Resources
{
    public class DdpmCultureMap
    {
        //The default mapped CultureInfo which is Mapped from CultureInfo.CurrentUICulture
        private static readonly CultureInfo _currentCultureInfo;


        //The static constructor will not be called until the first reference to the class is made.
        //So add a code in DdpmHomePlugin to get MappedCultureInfo and write to the log file.
        static DdpmCultureMap()
        {
            //Check if Debug flag is specified in Windows Registry
            if (ReadInt(KeyName_IsDebugCultureInfoEnabled) == 1)
            {
                string debugCultureName = ReadString(KeyName_DebugCultureInfo);
                if (!string.IsNullOrWhiteSpace(debugCultureName))
                {
                    try
                    {
                        CultureInfo debugCultureInfo = CultureInfo.CreateSpecificCulture(debugCultureName);
                        _currentCultureInfo = MapCultureInfo(debugCultureInfo);
                        return;
                    }
                    catch (Exception ex1)
                    {

                    }
                }
            }
            _currentCultureInfo = MapCultureInfo(CultureInfo.CurrentUICulture);
        }
        public static CultureInfo MappedCultureInfo => _currentCultureInfo;

        /// <summary>
        /// Input CultureIno and convert to the mapped cultureInfo which is supported by DDPM.
        /// </summary>
        /// <param name="cultureIn"></param>
        public static CultureInfo MapCultureInfo(CultureInfo cultureIn)
        {
            //"ar": All convert to "ar-SA"
            if (cultureIn.TwoLetterISOLanguageName.Equals("ar", StringComparison.OrdinalIgnoreCase))
            {
                return CultureInfo.CreateSpecificCulture("ar-SA");
            }
            //"de": All convert to "de-DE"
            if (cultureIn.TwoLetterISOLanguageName.Equals("de", StringComparison.OrdinalIgnoreCase))
            {
                return CultureInfo.CreateSpecificCulture("de-DE");
            }
            //"es"
            if (cultureIn.TwoLetterISOLanguageName.Equals("es", StringComparison.OrdinalIgnoreCase))
            {
                switch (cultureIn.Name)
                {
                    //es-MX
                    case "es-MX":
                        return CultureInfo.CreateSpecificCulture("es-MX");
                    //es-419 (Latin America)
                    case "es-419":
                    case "es-AR":
                    case "es-BO":
                    case "es-BZ":
                    case "es-CL":
                    case "es-CO":
                    case "es-CR":
                    case "es-CU":
                    case "es-DO":
                    case "es-EC":
                    case "es-GT":
                    case "es-HN":
                    case "es-NI":
                    case "es-PA":
                    case "es-PE":
                    case "es-PR":
                    case "es-PY":
                    case "es-SV":
                    case "es-UY":
                    case "es-VE":
                        return CultureInfo.CreateSpecificCulture("es-419");
                    default:
                        return CultureInfo.CreateSpecificCulture("es-ES");
                } //switch
            }// if "es"
            //"fr"
            if (cultureIn.TwoLetterISOLanguageName.Equals("fr", StringComparison.OrdinalIgnoreCase))
            {
                if (cultureIn.Name.Equals("fr-CA", StringComparison.OrdinalIgnoreCase))
                    return CultureInfo.CreateSpecificCulture("fr-CA");
                else
                    return CultureInfo.CreateSpecificCulture("fr-FR");
            }
            //"it"
            if (cultureIn.TwoLetterISOLanguageName.Equals("it", StringComparison.OrdinalIgnoreCase))
            {
                return CultureInfo.CreateSpecificCulture("it");
            }
            //"ja"
            if (cultureIn.TwoLetterISOLanguageName.Equals("ja", StringComparison.OrdinalIgnoreCase))
            {
                if (cultureIn.Name.Equals("ja-JP", StringComparison.OrdinalIgnoreCase))
                    return CultureInfo.CreateSpecificCulture("ja-JP");
                else
                    return CultureInfo.CreateSpecificCulture("ja");
            }

            //"ko"
            if (cultureIn.TwoLetterISOLanguageName.Equals("ko", StringComparison.OrdinalIgnoreCase))
            {
                if (cultureIn.Name.Equals("ko-KR", StringComparison.OrdinalIgnoreCase))
                    return CultureInfo.CreateSpecificCulture("ko-KR");
                else
                    return CultureInfo.CreateSpecificCulture("ko");
            }
            //"pl"
            if (cultureIn.TwoLetterISOLanguageName.Equals("pl", StringComparison.OrdinalIgnoreCase))
            {
                return CultureInfo.CreateSpecificCulture("pl");
            }
            //"pt"
            if (cultureIn.TwoLetterISOLanguageName.Equals("pt", StringComparison.OrdinalIgnoreCase))
            {
                if (cultureIn.Name.Equals("pt-BR", StringComparison.OrdinalIgnoreCase))
                    return CultureInfo.CreateSpecificCulture("pt-BR");
                else
                    return CultureInfo.CreateSpecificCulture("pt-PT");
            }
            //"ru"
            if (cultureIn.TwoLetterISOLanguageName.Equals("ru", StringComparison.OrdinalIgnoreCase))
            {
                return CultureInfo.CreateSpecificCulture("ru");
            }

            //"tr"
            if (cultureIn.TwoLetterISOLanguageName.Equals("tr", StringComparison.OrdinalIgnoreCase))
            {
                return CultureInfo.CreateSpecificCulture("tr");
            }

            //"uk"
            if (cultureIn.TwoLetterISOLanguageName.Equals("uk", StringComparison.OrdinalIgnoreCase))
            {
                return CultureInfo.CreateSpecificCulture("uk");
            }

            //"zh"
            if (cultureIn.TwoLetterISOLanguageName.Equals("zh", StringComparison.OrdinalIgnoreCase))
            {
                switch (cultureIn.Name)
                {

                    case "zh-Hans-CN":
                    case "zh-CN":
                        return CultureInfo.CreateSpecificCulture("zh-CN");
                    case "zh-TW":
                        return CultureInfo.CreateSpecificCulture("zh-TW");

                    case "zh-Hans":
                    case "zh-SG":
                        return CultureInfo.CreateSpecificCulture("zh-Hans");
                    default:
                        return CultureInfo.CreateSpecificCulture("zh-Hant");
                }
            } //
            //Otherwise, return the input cultureInfo
            return cultureIn;
        }

        #region Programmer Debug Testing Purpose
        //The duplicate function is provied in DDPM.SA.Common/Settings/DevSettings.cs
        //However, the Resources project will need to Add Project Reference to DDPM.SA.Common
        //SO we will add the Registry read function into this class.
        //
        //Used for SW Programmer Testing.
        //Edit Windows Registry keys to force the DDPM UI to display in a specific language.
        //[HKEY_LOCAL_MACHINE\SOFTWARE\Dell\Dell Display And Peripheral Manager\UserSettings]
        //IsDebugCultureInfoEnabled = REG_DWORD:1
        //DebugCultureInfo = REG_SZ:"zh-TW"

        private const string SubKey = @"SOFTWARE\Dell\Dell Display And Peripheral Manager\UserSettings";
        private const string KeyName_IsDebugCultureInfoEnabled = "IsDebugCultureInfoEnabled";
        private const string KeyName_DebugCultureInfo = "DebugCultureInfo";



        //Return value examples:
        // Type          Value    Return value
        // REG_DWORD     1        1
        // REG_QWORD     1        1 
        // REG_SZ        "1"      1
        // REG_SZ        "001"    0 (Causes exception inside ReadInt())
        // REG_SZ        "0x01"   0 (Causes exception inside ReadInt())
        // (keyName is not found) 0
        private static int ReadInt(string keyName, int defaultValue = 0)
        {
            object? o = ReadRegistryKey(RegistryHive.LocalMachine, SubKey, keyName);
            //If the specified keyName is not exist, then return the defaultValue
            if (o == null)
                return defaultValue;

            //Convert object o to int as return value
            try
            {
                int retValue = Convert.ToInt32(o);
                return retValue;
            }
            catch (Exception e1)
            {
                Trace.WriteLine(e1);
            }
            return defaultValue;
        }

        /// <summary>
        /// Read from DevSettings from Windows Registry, return string value.
        /// If the value type is not string, then convert it to string.
        /// </summary>
        /// <param name="keyName"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        private static string ReadString(string keyName, string defaultValue = "")
        {
            object? o = ReadRegistryKey(RegistryHive.LocalMachine, SubKey, keyName);
            //If the specified keyName is not exist, then return the defaultValue
            if (o == null)
                return defaultValue;

            //Convert object o to string as return value
            try
            {
                string retValue = Convert.ToString(o);
                return retValue;
            }
            catch (Exception e1)
            {
                Trace.WriteLine(e1);
            }
            return defaultValue;
        }

        private static object? ReadRegistryKey(RegistryHive hive, string keyPath, string keyName)
        {
            //Validate the input
            if (string.IsNullOrWhiteSpace(keyPath))
                return null;
            if (string.IsNullOrWhiteSpace(keyName))
                return null;

            //Get Full Path
            string fullPath = GetFullPath(hive, keyPath);
            return Registry.GetValue(fullPath, keyName, null);
        }

        private static string GetFullPath(RegistryHive hive, string keyPath)
        {
            return hive switch
            {
                RegistryHive.CurrentUser => $@"HKEY_CURRENT_USER\{keyPath}",
                RegistryHive.LocalMachine => $@"HKEY_LOCAL_MACHINE\{keyPath}",
                _ => throw new ArgumentOutOfRangeException(nameof(hive), hive, null)
            };
        }

        #endregion Programmer Debug Testing Purpose
    }
}
