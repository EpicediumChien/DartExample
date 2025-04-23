using DDPM.SA.Common.Display;
using DDPM.UI.Resources.Helper;
using Dell.Client.Framework.UX.WPF.Controls;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Input;
using Windows.System;

namespace DDPM.UI.Common
{
    public static class BlockKeys
    {
        private static List<VirtualKey> blockKeysList;

        static BlockKeys()
        {
            blockKeysList = new List<VirtualKey>();
            //blockKeysList.Add(VirtualKey.Escape);
            blockKeysList.Add(VirtualKey.Tab);
            blockKeysList.Add(VirtualKey.CapitalLock);
            blockKeysList.Add(VirtualKey.Enter);
            blockKeysList.Add(VirtualKey.LeftWindows);
            blockKeysList.Add(VirtualKey.RightWindows);
            blockKeysList.Add(VirtualKey.NumberKeyLock);
            //blockKeysList.Add(VirtualKey.Back);
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
            if (hotkeyInfo == null)
            {
                DdpmCommonHelper.WriteUILog($"hotkeyInfo == null when run hotKeyConflictsCheck");

                return false;
            }

            bool result = false;

            try
            {
                //Derek 2025/01/03 to check ALT+Z hotkey conflict for QAM
                if (hotkeyInfo.Alt && hotkeyInfo.Hotkey.Contains(VirtualKey.Z) && hotkeyInfo.Hotkey.Count == 2)
                {
                    //Robert_Lin 2025-4-22 Due to DDPMMsgBox changed, need to fix  below code or the HeaderText and SubHeaderText will be overlapped.
                    //OLD:
                    //Thickness headMargin = new Thickness(24, 30, 45, 24);
                    //Thickness subMargin = new Thickness(24, -66, 24, 8);

                    //DdpmCommonHelper.DDPMEzMesssageBox(LangHelper.Instance["hotkey.7"], LangHelper.Instance["Hotkey.10"], true, null,
                    //    420, 240, headMargin, subMargin);

                    //NEW:
                    Window mainWindow = System.Windows.Application.Current.MainWindow;
                    DdpmCommonHelper.DDPMPureMesssageBox(LangHelper.Instance["hotkey.7"], LangHelper.Instance["Hotkey.10"], true, mainWindow);

                    return false;
                }

                HotkeyWarning hotkeyWarning = DdpmCommonHelper.DeviceManagerSA!.GetHotkeyConflicts(hotkeyInfo).Result;
                
                switch (hotkeyWarning)
                {
                    case HotkeyWarning.None:
                        result = true;
                        break;

                    case HotkeyWarning.SingleKey:
                        result = DdpmCommonHelper.DDPMMesssageBox(LangHelper.Instance["hotkey.7"], LangHelper.Instance["hotkey.8"]);
                        break;

                    case HotkeyWarning.ConflictInbox:
                        result = DdpmCommonHelper.DDPMMesssageBox(LangHelper.Instance["hotkey.7"], LangHelper.Instance["hotkey.9"]);
                        break;
                }
            }
            catch (Exception e)
            {
                result = false;
                DdpmCommonHelper.WriteUILog($"Catch exception[{e.Message}] when run hotKeyConflictsCheck");
            }

            return result;
        }
        public static HotkeyInfo getUXTextBoxHotkeyInfo(object sender, System.Windows.Input.KeyEventArgs e, ref List<VirtualKey> newKeys, HotkeyType hotkeyType)
        {
            VirtualKey thisVirtualKey;
            VirtualKey thisVirtualKey_system = (VirtualKey)KeyInterop.VirtualKeyFromKey(e.SystemKey);
            if (thisVirtualKey_system != VirtualKey.None)
            {
                thisVirtualKey = thisVirtualKey_system;
            }
            else
            {
                thisVirtualKey = (VirtualKey)KeyInterop.VirtualKeyFromKey(e.Key);
            }
            if (thisVirtualKey == VirtualKey.LeftControl || thisVirtualKey == VirtualKey.RightControl)
            {
                thisVirtualKey = VirtualKey.Control;
            }
            else if (thisVirtualKey == VirtualKey.LeftShift || thisVirtualKey == VirtualKey.RightShift)
            {
                thisVirtualKey = VirtualKey.Shift;
            }
            else if (thisVirtualKey == VirtualKey.LeftMenu || thisVirtualKey == VirtualKey.RightMenu)
            {
                thisVirtualKey = VirtualKey.Menu;
            }
            if (newKeys.Contains(thisVirtualKey)) newKeys.Remove(thisVirtualKey);
            if (newKeys.Count == 0)
            {
                var texbox = sender as UXTextBox;
                if (texbox != null)
                {
                    string keyText = texbox.Text;
                    string[] strings = keyText.Split("+");
                    List<VirtualKey> keyList = new List<VirtualKey>();
                    foreach (string s in strings)
                    {
                        string keyStr = string.Empty;
                        switch (s.Trim())
                        {
                            case "Ctrl":
                                keyStr = "Control";
                                break;
                            case "Alt":
                                keyStr = "Menu";
                                break;
                            case "Shift":
                                keyStr = "Shift";
                                break;
                            case "0":
                                keyStr = "Number0";
                                break;
                            case "1":
                                keyStr = "Number1";
                                break;
                            case "2":
                                keyStr = "Number2";
                                break;
                            case "3":
                                keyStr = "Number3";
                                break;
                            case "4":
                                keyStr = "Number4";
                                break;
                            case "5":
                                keyStr = "Number5";
                                break;
                            case "6":
                                keyStr = "Number6";
                                break;
                            case "7":
                                keyStr = "Number7";
                                break;
                            case "8":
                                keyStr = "Number8";
                                break;
                            case "9":
                                keyStr = "Number9";
                                break;
                            default:
                                keyStr = s;
                                break;
                        }

                        if (!string.IsNullOrEmpty(keyStr))
                        {
                            if (keyStr.Trim().Equals(LangHelper.Instance["None"].Trim(), StringComparison.OrdinalIgnoreCase))
                            {
                                keyList.Add((VirtualKey)Enum.Parse(typeof(VirtualKey), "None"));
                            }
                            else
                            {
                                keyList.Add((VirtualKey)Enum.Parse(typeof(VirtualKey), keyStr));
                            }
                        }
                    }
                    HotkeyInfo hotkeyInfo = new HotkeyInfo();
                    hotkeyInfo.Job = hotkeyType;
                    hotkeyInfo.Description = hotkeyType.ToString();

                    hotkeyInfo.Hotkey = keyList;
                    return hotkeyInfo;
                }
            }
            return new HotkeyInfo();
        }

        public static bool onlyContainModifyKeys(List<VirtualKey> keys)
        {
            List<VirtualKey> tmp = new List<VirtualKey>();
            tmp.AddRange(keys);
            if (tmp.Contains(VirtualKey.Control)) tmp.Remove(VirtualKey.Control);
            if (tmp.Contains(VirtualKey.Menu)) tmp.Remove(VirtualKey.Menu);
            if (tmp.Contains(VirtualKey.Shift)) tmp.Remove(VirtualKey.Shift);
            return !(tmp.Count > 0);
        }


        public static void setUXTextBoxPreviewKey(object sender, System.Windows.Input.KeyEventArgs e, ref List<VirtualKey> newKeys, ref List<VirtualKey> BundleNewKeys, ref bool alphabetKey)
        {
            //bypass
            if (!DdpmCommonHelper.isHotkeyBypass)
            {
                DdpmCommonHelper.isHotkeyBypass = DdpmCommonHelper.DeviceManagerSA!.ByPassHotkey(true).Result;
                bool v = DdpmCommonHelper.DeviceManagerSA.UnRegistAllHotkey().Result;
            }
            e.Handled = true;
            if (e.IsRepeat) return;
            newKeys = newKeys.Distinct().ToList();
            BundleNewKeys = BundleNewKeys.Distinct().ToList();
            if (newKeys.Count >= 4) return;
            var texBox = (sender as UXTextBox);
            if (texBox == null) return;
            var texBoxName = texBox?.Name;
            if (string.IsNullOrEmpty(texBoxName)) return;
            Debug.WriteLine($"{texBoxName}_PreviewKeyDown---Key---{e.Key}");
            Debug.WriteLine($"{texBoxName}_PreviewKeyDown---SystemKey---{e.SystemKey}");
            VirtualKey thisVirtualKey;
            VirtualKey thisVirtualKey_system = (VirtualKey)KeyInterop.VirtualKeyFromKey(e.SystemKey);
            if (thisVirtualKey_system != VirtualKey.None)
            {
                thisVirtualKey = thisVirtualKey_system;
            }
            else
            {
                thisVirtualKey = (VirtualKey)KeyInterop.VirtualKeyFromKey(e.Key);
            }
            //if the key will be processed by an Input Method Editor (IME), then return ?
            //Object v;
            // Enum.TryParse(typeof(VirtualKey), e.Key.ToString(), out v);
            bool r = Enum.IsDefined(typeof(VirtualKey), thisVirtualKey);
            if (!r) return;
            if (BlockKeys.isBlocked(thisVirtualKey)) return;
            string swHortcutText = string.Empty;
            //Esc and Backspace clear hotkey setting
            if (BundleNewKeys.Count == 1 && BundleNewKeys[0] == VirtualKey.None) BundleNewKeys.Clear();
            if ((thisVirtualKey == VirtualKey.Escape) || (thisVirtualKey == VirtualKey.Back))
            {
                newKeys.Clear();
                BundleNewKeys.Clear();
                BundleNewKeys.Add(VirtualKey.None);
                alphabetKey = false;
                swHortcutText = string.Empty;
                KeysHelper.ReSetHotKeyText(ref swHortcutText, ref BundleNewKeys);
                texBox.Text = swHortcutText;
                texBox.Select(swHortcutText.Length, 1);
                return;
            }

            if (newKeys.Count > 0 && !newKeys.Any(x => (x == VirtualKey.Control) || (x == VirtualKey.Shift) || (x == VirtualKey.Menu)))
            {
                int i = newKeys.FindIndex(x => x >= VirtualKey.A && x <= VirtualKey.Z);
                //remove exist key and then update new alphabetKey
                if (i >= 0)
                    newKeys.RemoveAt(i);
                else
                    return;
            }
            if (newKeys.Count == 2 && newKeys.Any(x => (x == VirtualKey.Menu)) && !newKeys.Any(x => (x == VirtualKey.Control) || (x == VirtualKey.Shift)))
            {
                //second Alt+ (key)
                return;
            }
            if (thisVirtualKey == VirtualKey.LeftControl || thisVirtualKey == VirtualKey.RightControl)
            {
                thisVirtualKey = VirtualKey.Control;
            }
            else if (thisVirtualKey == VirtualKey.LeftShift || thisVirtualKey == VirtualKey.RightShift)
            {
                thisVirtualKey = VirtualKey.Shift;
            }
            else if (thisVirtualKey == VirtualKey.LeftMenu || thisVirtualKey == VirtualKey.RightMenu)
            {
                thisVirtualKey = VirtualKey.Menu;
            }

            //only on alphabet Key
            if (alphabetKey)
            {
                int i = newKeys.FindIndex(x => x >= VirtualKey.A && x <= VirtualKey.Z);
                //remove exist key and then update new alphabetKey
                if (i >= 0)
                    newKeys.RemoveAt(i);
                //return;
            }
            if (thisVirtualKey >= VirtualKey.A && thisVirtualKey <= VirtualKey.Z)
            {
                alphabetKey = true;
            }

            /*  if (!alphabetKey)
              {
                  newKeys.Clear();
                  alphabetKey = true;
                  newKeys.Add(thisVirtualKey);
              }*/

            Debug.WriteLine($"{texBoxName}_PreviewKeyDown-NewKeys-----{string.Join(",", newKeys)}");
            if (!newKeys.Contains(thisVirtualKey))
            {
                //newKeys.Add(thisVirtualKey);
                if ((thisVirtualKey == VirtualKey.Menu) ||
                    (thisVirtualKey == VirtualKey.Control) ||
                    (thisVirtualKey == VirtualKey.Shift) ||
                    (thisVirtualKey >= VirtualKey.A && thisVirtualKey <= VirtualKey.Z) ||
                    (thisVirtualKey >= VirtualKey.Number0 && thisVirtualKey <= VirtualKey.Number9))
                {
                    newKeys.Add(thisVirtualKey);
                }

                /* if ((thisVirtualKey >= VirtualKey.Number0 && thisVirtualKey <= VirtualKey.Number9) ||
                     (thisVirtualKey >= VirtualKey.A && thisVirtualKey <= VirtualKey.Z) ||
                     (thisVirtualKey >= VirtualKey.F1 && thisVirtualKey <= VirtualKey.F24) ||
                     (thisVirtualKey >= VirtualKey.NumberPad0 && thisVirtualKey <= VirtualKey.Divide))
                 {
                     newKeys.Add(thisVirtualKey);
                 }*/
            }
            //string swHortcutText = string.Empty;
            if (newKeys.Count == 0) return;
            KeysHelper.ReSetHotKeyText(ref swHortcutText, ref newKeys);
            Debug.WriteLine($"shotcutText:{swHortcutText}: length: {swHortcutText.Length}");
            if (swHortcutText.Length > 30)
            {
                //if lenght gt 30,reset to default
                return;
            }
            texBox.Text = swHortcutText;
            texBox.Select(swHortcutText.Length, 1);
            BundleNewKeys.AddRange(newKeys);

        }
        public static void ReSetHotKeyText(ref string strShortCutText, ref List<VirtualKey> newHotKeys)
        {
            string strTmpKey = string.Empty;

            //tbGlobalShortCut.Text
            strShortCutText = strTmpKey;
            if (newHotKeys.Count == 0)
            {
                strShortCutText = LangHelper.Instance["None"];
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
            if ("None".Equals(strTmpKey.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                strTmpKey = LangHelper.Instance["None"];
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