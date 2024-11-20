using Dell.Client.Framework.UX.WPF.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Button = System.Windows.Controls.Button;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;

namespace DDPM.UI.Common.Method
{
    public static class NarratorModeSupport
    {
        //public static bool Use_NarratorModeSupport = true;
        public static void RecurseUitems(object item)
        {
            //if (Use_NarratorModeSupport != true) return;

            //避免大小寫寫錯問題,一律轉小寫處理 
            string typename = item.GetType().Name.ToLower();
            switch (typename)
            {
                //可自行添加UI項目支援
                case "button":
                    Button bt = (Button)item;
                    bt.Focusable = true;
                    bt.MouseEnter += delegate (object s, MouseEventArgs e) { ((Button)s).Focus(); };
                    break;
                case "textblock":
                    TextBlock tb = (TextBlock)item;
                    tb.Focusable = true;
                    tb.MouseEnter += delegate (object s, MouseEventArgs e) { ((TextBlock)s).Focus(); };
                    break;

                case "uxtextblock":
                    UXTextBlock utb = (UXTextBlock)item;
                    utb.Focusable = true;
                    utb.MouseEnter += delegate (object s, MouseEventArgs e) { ((UXTextBlock)s).Focus(); };
                    break;

                case "uxtoggleswitch":
                    UXToggleSwitch toggle = (UXToggleSwitch)item;
                    toggle.Focusable = true;
                    toggle.MouseEnter += delegate (object s, MouseEventArgs e) { ((UXToggleSwitch)s).Focus(); };
                    break;

                //容器類型UI項目,有可能需要添加類型
                case "grid":
                    {
                        foreach (var _item in ((Grid)item).Children)
                            RecurseUitems(_item);
                    }
                    break;

                case "stackpanel":
                    {
                        foreach (var _item in ((StackPanel)item).Children)
                            RecurseUitems(_item);
                    }
                    break;

                case "border":
                    {
                        var _item = ((Border)item).Child;
                        if (_item != null) RecurseUitems(_item);
                    }
                    break;

                case "dockpanel":
                    {
                        foreach (var _item in ((DockPanel)item).Children)
                            RecurseUitems(_item);
                    }
                    break;

                case "uxscrollviewer":
                    {
                        var _item = ((UXScrollViewer)item).Content;
                        RecurseUitems(_item);
                    }
                    break;

                default:
                    Debug.WriteLine("NarratorMode未處理類型:" + typename);
                    //MessageBox.Show("未處理類型:"+typename);
                    break;

            }
        }

        private static void Utb_MouseEnter(object sender, MouseEventArgs e)
        {
            UXTextBlock utb = (UXTextBlock)sender;
            utb.Focus();
        }

        private static void Bt_MouseEnter(object sender, MouseEventArgs e)
        {
            Button bt = (Button)sender;
            bt.Focus();
        }

        private static void Tb_MouseEnter(object sender, MouseEventArgs e)
        {
            TextBlock tb = (TextBlock)sender;
            tb.Focus();
        }
    }
}
