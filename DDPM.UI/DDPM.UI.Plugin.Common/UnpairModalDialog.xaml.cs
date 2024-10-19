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
        public UnpairModalDialog(eDeviceCategory deviceCat)
        {
            InitializeComponent();

            txtCaption.Text = Strings.Caption;
            switch (deviceCat)
            {
                case eDeviceCategory.Mouse:
                    txtMessage.Text = Strings.MessageMouse;
                    Border1.Height = 312;
                    break;

                case eDeviceCategory.KB:
                    txtMessage.Text = Strings.MessageKeyboard;
                    Border1.Height = 312;
                    break;

                case eDeviceCategory.Pen:
                    txtMessage.Text = Strings.MessagePen;
                    Border1.Height = 312;
                    break;

                case eDeviceCategory.Headset:
                    txtMessage.Text = Strings.MessageHeadset;
                    this.Height = 360;
                    Border1.Height = 360;
                    break;
            }
            txtContinue.Text = Strings.Continue;
            txtCancel.Text = Strings.Cancel;
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