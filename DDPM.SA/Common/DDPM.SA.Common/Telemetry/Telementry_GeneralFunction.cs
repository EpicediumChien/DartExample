using DdmLibrary;
using System;
using System.Collections.Generic;
using System.Management;
using System.Text;
using Windows.System;

namespace DDPM.SA.Common
{
    public class Telementry_GeneralFunction
    {
        public string GetMonitorAdapter()
        {
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_VideoController");
            foreach (ManagementObject mo in searcher.Get())
            {
                PropertyData currentBitsPerPixel = mo.Properties["CurrentBitsPerPixel"];
                PropertyData description = mo.Properties["Description"];
                if (currentBitsPerPixel != null && description != null)
                {
                    if (currentBitsPerPixel.Value != null)
                        return (description.Value).ToString();
                }
            }
            return string.Empty;
        }
        public static class HotkeyTelementryHelper
        {
            public static string getHotkeyNoStr(List<VirtualKey> hotKeys)
            {
                List<string> strList = new List<string>(4) { "1Key", " 2Keys", "3Keys", "4Keys" };
                string rt = string.Empty;
                if (hotKeys != null)
                {
                    if (hotKeys.Count > 0 && hotKeys.Count <= 4)
                    {
                        rt = strList[hotKeys.Count - 1];
                    }
                }
                return rt;
            }
            public static string toHotKeyText(List<VirtualKey> newHotKeys)
            {
                string strTmpKey = string.Empty;
                if (newHotKeys.Count == 0)
                {
                    return "None";
                }
                int ControlIndex = newHotKeys.IndexOf(VirtualKey.Control);
                int ControlIndexLeft = newHotKeys.IndexOf(VirtualKey.LeftControl);

                int MenuIndex = newHotKeys.IndexOf(VirtualKey.Menu);
                int MenuIndexLeft = newHotKeys.IndexOf(VirtualKey.LeftMenu);

                int ShiftIndex = newHotKeys.IndexOf(VirtualKey.Shift);
                int ShiftIndexLeft = newHotKeys.IndexOf(VirtualKey.LeftShift);

                if ((ControlIndex >= 0) || (ControlIndexLeft >= 0))
                    strTmpKey += "Ctrl +";

                if ((MenuIndex >= 0) || (MenuIndexLeft >= 0))
                    strTmpKey += "Alt +";

                if ((ShiftIndex >= 0) || (ShiftIndexLeft >= 0))
                    strTmpKey += "Shift +";
                foreach (var item in newHotKeys)
                {
                    if (item == VirtualKey.Control || item == VirtualKey.Menu || item == VirtualKey.Shift ||
                        item == VirtualKey.LeftControl || item == VirtualKey.LeftMenu || item == VirtualKey.LeftShift)
                    {
                        continue;
                    }
                    if (strTmpKey != "" && !strTmpKey.EndsWith("+"))
                    {
                        strTmpKey += "+";
                    }
                    strTmpKey = strTmpKey + " " + VirtualKeyToString(item);
                    break;
                }

                return strTmpKey;
            }
            private static string VirtualKeyToString(VirtualKey vk)
            {
                string strRetKey = string.Empty;

                if (vk >= VirtualKey.Number0 && vk <= VirtualKey.Number9)
                {
                    switch (vk)
                    {
                        case VirtualKey.Number0:
                            strRetKey = "0";
                            break;

                        case VirtualKey.Number1:
                            strRetKey = "1";
                            break;

                        case VirtualKey.Number2:
                            strRetKey = "2";
                            break;

                        case VirtualKey.Number3:
                            strRetKey = "3";
                            break;

                        case VirtualKey.Number4:
                            strRetKey = "4";
                            break;

                        case VirtualKey.Number5:
                            strRetKey = "5";
                            break;

                        case VirtualKey.Number6:
                            strRetKey = "6";
                            break;

                        case VirtualKey.Number7:
                            strRetKey = "7";
                            break;

                        case VirtualKey.Number8:
                            strRetKey = "8";
                            break;

                        case VirtualKey.Number9:
                            strRetKey = "9";
                            break;
                    }
                }
                else
                {
                    strRetKey = vk.ToString();
                    if (FullWidthCharactersHandler.isHalfWidthString(strRetKey))
                    {
                        strRetKey = FullWidthCharactersHandler.convertFullWidthToHalfWidth(strRetKey);
                    }
                }

                return strRetKey;
            }
            public static class FullWidthCharactersHandler
            {
                private static Dictionary<char, char> fullWidth2halfWidthDic;

                static FullWidthCharactersHandler()
                {
                    fullWidth2halfWidthDic = new Dictionary<char, char>();
                    string fullWidthChars = "０１２３４５６７８９ＡＢＣＤＥＦＧＨＩＪＫＬＭＮＯＰＱＲＳＴＵＶＷＸＹＺａｂｃｄｅｆｇｈｉｊｋｌｍｎｏｐｑｒｓｔｕｖｗｘｙｚ";
                    string halfWidthChars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
                    for (int i = 0; i < fullWidthChars.Length; i++)
                    {
                        fullWidth2halfWidthDic.Add(fullWidthChars[i], halfWidthChars[i]);
                    }
                }

                public static bool isHalfWidthString(string toTestString)
                {
                    bool isHalfWidth = true;
                    foreach (char ch in toTestString)
                    {
                        if (fullWidth2halfWidthDic.ContainsKey(ch))
                        {
                            isHalfWidth = false;
                            break;
                        }
                    }
                    return isHalfWidth;
                }

                public static string convertFullWidthToHalfWidth(string theString)
                {
                    StringBuilder sbResult = new StringBuilder(theString);
                    for (int i = 0; i < theString.Length; i++)
                    {
                        if (fullWidth2halfWidthDic.ContainsKey(theString[i]))
                        {
                            sbResult[i] = fullWidth2halfWidthDic[theString[i]];
                        }
                    }
                    return sbResult.ToString();
                }
            }

        }
    }
}