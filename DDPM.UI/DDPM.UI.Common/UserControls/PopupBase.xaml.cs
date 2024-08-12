using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using UserControl = System.Windows.Controls.UserControl;

namespace DDPM.UI.Common.UserControls
{
    /// <summary>
    /// PopupBase.xaml 的互動邏輯
    /// </summary>
    //0614 Bruce 新增dock多個dock更新顯示通知，目前icon標題會是白色，推測跟整個應用程式是亮模式或是暗模式有關
    public partial class PopupBase : UserControl
    {
        public static bool isClose = false;
        public PopupBase()
        {
            InitializeComponent();
        }
        public PopupBase(bool isSubHeaderVisible, bool isButtonsVisible, string HeaderText, string SubHeaderText)
        {
            InitializeComponent();
            this.Header.Text = HeaderText;
            this.SubHeader.Text = SubHeaderText;
            SubHeader.Visibility = isSubHeaderVisible ? Visibility.Visible : Visibility.Collapsed;
            ButtonPanel.Visibility = isButtonsVisible ? Visibility.Visible : Visibility.Collapsed;
            LeftButton.Content = "";
            RightButton.Content = "";
        }

        private void UserControl_ToolTipClosing(object sender, ToolTipEventArgs e)
        {
            isClose = true;
        }
    }
}
