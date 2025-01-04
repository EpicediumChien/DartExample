using System.Windows;
using System.Windows.Controls;
using UserControl = System.Windows.Controls.UserControl;

namespace DDPM.UI.Common.UserControls
{
    /// <summary>
    /// PopupBase.xaml 的互動邏輯
    /// </summary>
    //0614 Bruce 新增dock多個dock更新顯示通知，目前icon標題會是白色，推測跟整個應用程式是亮模式或是暗模式有關
    public partial class PopupBase : UserControl
    {
        private static bool isClose = false;

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

        public static bool IsClose { get => isClose; set => isClose = value; }

        private void UserControl_ToolTipClosing(object sender, ToolTipEventArgs e)
        {
            IsClose = true;
        }
    }
}