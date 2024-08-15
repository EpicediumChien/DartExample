using DDPM.UI.Common;
using System.Windows;
using System.Windows.Input;

namespace DDPM.UI.Plugin.Common
{
    /// <summary>
    /// UnpairModalDialog.xaml 的互動邏輯
    /// </summary>
    public partial class UnpairModalDialog : Window
    {
        private readonly string Caption = "Are you sure?";
        private readonly string MessageMouse = "Unpairing your mouse can limit your ability to use this computer. Make sure you have an alternative mouse setup before unpairing.";
        private readonly string MessageKeyboard = "Unpairing your keyboard can limit your ability to use this computer. Make sure you have an alternative keyboard setup before unpairing.";
        private readonly string MessagePen = "Unpairing your pen can limit your ability to use this computer. Please make sure you have an alternative pen setup before unpairing.";
        private readonly string MessageHeadset = "This will unpair your headset from its USB wireless receiver. You can still pair and use the headset on this system via Bluetooth. If required, you may pair the headset back to the wireless receiver from + icon on top right of the home screen of [NAME] .";
        private readonly string Continue = "Continue";
        private readonly string Cancel = "Cancel";

        public UnpairModalDialog(eDeviceCategory deviceCat)
        {
            InitializeComponent();

            txtCaption.Text = Caption;
            switch (deviceCat)
            {
                case eDeviceCategory.Mouse:
                    txtMessage.Text = MessageMouse;
                    Border1.Height = 312;
                    break;

                case eDeviceCategory.KB:
                    txtMessage.Text = MessageKeyboard;
                    Border1.Height = 312;
                    break;

                case eDeviceCategory.Pen:
                    txtMessage.Text = MessagePen;
                    Border1.Height = 312;
                    break;

                case eDeviceCategory.Headset:
                    txtMessage.Text = MessageHeadset;
                    this.Height = 360;
                    Border1.Height = 360;
                    break;
            }
            txtContinue.Text = Continue;
            txtCancel.Text = Cancel;
        }

        private void Continue_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void Cancel_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}