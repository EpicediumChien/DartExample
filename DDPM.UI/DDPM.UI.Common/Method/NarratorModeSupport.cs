using Dell.Client.Framework.UX.WPF.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Button = System.Windows.Controls.Button;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;

namespace DDPM.UI.Common.Method
{
    public static class NarratorModeSupport
    {
        public static bool Use_NarratorModeSupport = true;
        public static void RecurseUitemsStackPanel(StackPanel stackpanel)
        {
            foreach (var item in stackpanel.Children)
            {
                string typename = item.GetType().Name.ToLower();
                switch (typename)
                {
                    case "button":
                        Button bt = (Button)item;
                        bt.Focusable = true;
                        bt.MouseEnter += Bt_MouseEnter;
                        break;
                    case "textblock":
                        TextBlock tb = (TextBlock)item;
                        tb.Focusable = true;
                        tb.MouseEnter += Tb_MouseEnter;
                        break;

                    case "uxtextblock":
                        UXTextBlock utb = (UXTextBlock)item;
                        Console.WriteLine("utb:" + utb.Text);
                        utb.Focusable = true;
                        utb.MouseEnter += Utb_MouseEnter;
                        break;

                    case "grid":
                        Grid gd = (Grid)item;
                        RecurseUitemsGrid(gd);
                        break;

                    case "stackpanel":
                        StackPanel sp = (StackPanel)item;
                        RecurseUitemsStackPanel(sp);
                        break;

                    case "border":
                        Border bd = (Border)item;
                        RecurseUitemsBorder(bd);
                        break;

                    case "dockpanel":
                        DockPanel dp = (DockPanel)item;
                        RecurseUitemsDockPanel(dp);
                        break;

                    case "uxscrollviewer":
                        UXScrollViewer sv = (UXScrollViewer)item;
                        RecurseUitemsUXScrollViewer(sv);
                        break;

                    default:
                        Console.WriteLine("未處理類型:" + typename);
                        //MessageBox.Show("未處理類型:"+typename);
                        break;
                }
            }
        }

        public static void RecurseUitemsUXScrollViewer(UXScrollViewer uxscrollviewer)
        {
            var item = uxscrollviewer.Content;
            //foreach (var item in uxscrollviewer.Content )
            //{
            string typename = item.GetType().Name.ToLower();
            switch (typename)
            {
                case "button":
                    Button bt = (Button)item;
                    bt.Focusable = true;
                    bt.MouseEnter += Bt_MouseEnter;
                    break;
                case "textblock":
                    TextBlock tb = (TextBlock)item;
                    tb.Focusable = true;
                    tb.MouseEnter += Tb_MouseEnter;
                    break;

                case "uxtextblock":
                    UXTextBlock utb = (UXTextBlock)item;
                    Console.WriteLine("utb:" + utb.Text);
                    utb.Focusable = true;
                    utb.MouseEnter += Utb_MouseEnter;
                    break;

                case "grid":
                    Grid gd = (Grid)item;
                    RecurseUitemsGrid(gd);
                    break;

                case "stackpanel":
                    StackPanel sp = (StackPanel)item;
                    RecurseUitemsStackPanel(sp);
                    break;

                case "border":
                    Border bd = (Border)item;
                    RecurseUitemsBorder(bd);
                    break;

                case "dockpanel":
                    DockPanel dp = (DockPanel)item;
                    RecurseUitemsDockPanel(dp);
                    break;

                case "uxscrollviewer":
                    UXScrollViewer sv = (UXScrollViewer)item;
                    RecurseUitemsUXScrollViewer(sv);
                    break;

                default:
                    Console.WriteLine("未處理類型:" + typename);
                    //MessageBox.Show("未處理類型:"+typename);
                    break;
            }
            //}
        }

        public static void RecurseUitemsDockPanel(DockPanel dockpanel)
        {
            foreach (var item in dockpanel.Children)
            {
                string typename = item.GetType().Name.ToLower();
                switch (typename)
                {
                    case "button":
                        Button bt = (Button)item;
                        bt.Focusable = true;
                        bt.MouseEnter += Bt_MouseEnter;
                        break;
                    case "textblock":
                        TextBlock tb = (TextBlock)item;
                        tb.Focusable = true;
                        tb.MouseEnter += Tb_MouseEnter;
                        break;

                    case "uxtextblock":
                        UXTextBlock utb = (UXTextBlock)item;
                        Console.WriteLine("utb:" + utb.Text);
                        utb.Focusable = true;
                        utb.MouseEnter += Utb_MouseEnter;
                        break;

                    case "grid":
                        Grid gd = (Grid)item;
                        RecurseUitemsGrid(gd);
                        break;

                    case "stackpanel":
                        StackPanel sp = (StackPanel)item;
                        RecurseUitemsStackPanel(sp);
                        break;

                    case "border":
                        Border bd = (Border)item;
                        RecurseUitemsBorder(bd);
                        break;

                    case "dockpanel":
                        DockPanel dp = (DockPanel)item;
                        RecurseUitemsDockPanel(dp);
                        break;

                    case "uxscrollviewer":
                        UXScrollViewer sv = (UXScrollViewer)item;
                        RecurseUitemsUXScrollViewer(sv);
                        break;

                    default:
                        Console.WriteLine("未處理類型:" + typename);
                        //MessageBox.Show("未處理類型:"+typename);
                        break;
                }
            }
        }

        public static void RecurseUitemsBorder(Border border)
        {
            var item = border.Child;
            if (item == null) return;
            string typename = item.GetType().Name.ToLower();
            switch (typename)
            {
                case "button":
                    Button bt = (Button)item;
                    bt.Focusable = true;
                    bt.MouseEnter += Bt_MouseEnter;
                    break;
                case "textblock":
                    TextBlock tb = (TextBlock)item;
                    tb.Focusable = true;
                    tb.MouseEnter += Tb_MouseEnter;
                    break;

                case "uxtextblock":
                    UXTextBlock utb = (UXTextBlock)item;
                    Console.WriteLine("utb:" + utb.Text);
                    utb.Focusable = true;
                    utb.MouseEnter += Utb_MouseEnter;
                    break;

                case "grid":
                    Grid gd = (Grid)item;
                    RecurseUitemsGrid(gd);
                    break;

                case "stackpanel":
                    StackPanel sp = (StackPanel)item;
                    RecurseUitemsStackPanel(sp);
                    break;

                case "border":
                    Border bd = (Border)item;
                    RecurseUitemsBorder(bd);
                    break;

                case "dockpanel":
                    DockPanel dp = (DockPanel)item;
                    RecurseUitemsDockPanel(dp);
                    break;

                case "uxscrollviewer":
                    UXScrollViewer sv = (UXScrollViewer)item;
                    RecurseUitemsUXScrollViewer(sv);
                    break;

                default:
                    Console.WriteLine("未處理類型:" + typename);
                    //MessageBox.Show("未處理類型:"+typename);
                    break;

            }
        }

        public static void RecurseUitemsGrid(Grid grid)
        {
            if (Use_NarratorModeSupport != true) return;

            foreach (var item in grid.Children)
            {
                string typename = item.GetType().Name.ToLower();
                switch (typename)
                {
                    case "button":
                        Button bt = (Button)item;
                        bt.Focusable = true;
                        bt.MouseEnter += Bt_MouseEnter;
                        break;
                    case "textblock":
                        TextBlock tb = (TextBlock)item;
                        tb.Focusable = true;
                        tb.MouseEnter += Tb_MouseEnter;
                        break;

                    case "uxtextblock":
                        UXTextBlock utb = (UXTextBlock)item;
                        Console.WriteLine("utb:" + utb.Text);
                        utb.Focusable = true;
                        utb.MouseEnter += Utb_MouseEnter;
                        break;

                    case "grid":
                        Grid gd = (Grid)item;
                        RecurseUitemsGrid(gd);
                        break;

                    case "stackpanel":
                        StackPanel sp = (StackPanel)item;
                        RecurseUitemsStackPanel(sp);
                        break;

                    case "border":
                        Border bd = (Border)item;
                        RecurseUitemsBorder(bd);
                        break;

                    case "dockpanel":
                        DockPanel dp = (DockPanel)item;
                        RecurseUitemsDockPanel(dp);
                        break;

                    case "uxscrollviewer":
                        UXScrollViewer sv = (UXScrollViewer)item;
                        RecurseUitemsUXScrollViewer(sv);
                        break;

                    default:
                        Console.WriteLine("未處理類型:" + typename);
                        //MessageBox.Show("未處理類型:"+typename);
                        break;

                }
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
