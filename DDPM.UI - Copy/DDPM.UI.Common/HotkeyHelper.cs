using DDPM.SA.Common.Display;
using System.Text;
using Windows.System;

namespace DDPM.UI.Common
{
    public static class BlockKeys
    {
        private static List<VirtualKey> blockKeysList;

        static BlockKeys()
        {
            blockKeysList = new List<VirtualKey>();
            blockKeysList.Add(VirtualKey.Escape);
            blockKeysList.Add(VirtualKey.Tab);
            blockKeysList.Add(VirtualKey.CapitalLock);
            blockKeysList.Add(VirtualKey.Enter);
            blockKeysList.Add(VirtualKey.LeftWindows);
            blockKeysList.Add(VirtualKey.RightWindows);
            blockKeysList.Add(VirtualKey.NumberKeyLock);
            blockKeysList.Add(VirtualKey.Back);
            blockKeysList.Add(VirtualKey.Scroll);
            blockKeysList.Add(VirtualKey.Pause);
            blockKeysList.Add(VirtualKey.Snapshot);
            //todo
            // Fn key,copilot key, video key
        }

        public static bool isBlocked(VirtualKey key)
        {
            return blockKeysList.Contains(key);
        }
    }

    public static class KeysHelper
    {
        public static bool ContainsKeyIgnoreLeftRight(List<VirtualKey> list, VirtualKey key)
        {
            if (key == VirtualKey.LeftShift || key == VirtualKey.RightShift)
            {
            }
            return true;
        }

        public static string hotkeyTypeToStr(HotkeyType hotkeyType)
        {
            switch (hotkeyType)
            {
                case HotkeyType.BrightnessReduce:
                    return "Brightness-";

                case HotkeyType.BrightnessIncrease:
                    return "Brightness+";

                case HotkeyType.ContrastReduce:
                    return "Contrast-";

                case HotkeyType.ContrastIncrease:
                    return "Contrast+";

                case HotkeyType.LuminanceReduce:
                    return "Luminance-";

                case HotkeyType.LuminanceIncrease:
                    return "Luminance+";

                case HotkeyType.ToggleInputSource:
                    return "ToggleInputSource";

                case HotkeyType.FavoriteInputSource:
                    return "FavoriteInputSource";

                case HotkeyType.SwitchInputSource:
                    return "SwitchInputSource";

                case HotkeyType.SwapIputPIPPBP:
                    return "SwapIputPIPPBP";

                case HotkeyType.ChangePIPPosition:
                    return "ChangePIPPosition";
            }
            return "";
        }

        public static bool hotKeyConflictsCheck(HotkeyInfo hotkeyInfo)
        {
            HotkeyWarning hotkeyWarning = DdpmCommonHelper.DeviceManagerSA.GetHotkeyConflicts(hotkeyInfo).Result;
            bool result = false;
            switch (hotkeyWarning)
            {
                case HotkeyWarning.None:
                    result = true;
                    break;

                case HotkeyWarning.SingleKey:
                    result = DdpmCommonHelper.DDPMMesssageBox("Hotkey Warning", "The hotkey you configured is a single key.It may interfere with how you intend that key to work in other applications.Are you sure you want to proceed?");
                    break;

                case HotkeyWarning.ConflictInbox:
                    result = DdpmCommonHelper.DDPMMesssageBox("Hotkey Warning", "The hotkey conflicts with a hotkey configured in another application.Use a different hotkey combination.");
                    break;
            }
            return result;
        }

        public static void ReSetHotKeyText(ref string strShortCutText, ref List<VirtualKey> newHotKeys)
        {
            string strTmpKey = string.Empty;

            //tbGlobalShortCut.Text
            strShortCutText = strTmpKey;
            if (newHotKeys.Count == 0)
            {
                strShortCutText = "None";
                return;
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

            //tbGlobalShortCut.Text
            strShortCutText = strTmpKey;

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

            strShortCutText = strTmpKey;
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