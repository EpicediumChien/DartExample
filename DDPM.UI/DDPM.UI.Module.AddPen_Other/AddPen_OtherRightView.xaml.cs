using DDPM.UI.Common;
using DDPM.UI.Plugin.Common;
using DDPM.UI.Plugin.ViewModels;
using System.Net;
using System.Reflection.Metadata;
using System.Windows;
using System.Windows.Controls;
using static System.Net.Mime.MediaTypeNames;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

namespace DDPM.UI.Module.AddPen_Other
{
    /// <summary>
    /// Interaction logic for AddPen_OtherRightView.xaml
    /// </summary>
    public partial class AddPen_OtherRightView : UserControl
    {
        private readonly AddDeviceViewModel _vm;

        //private readonly string Caption = "Connecting your Pen";
        //private readonly string Step1 = "Touch your pen tip to the screen";
        private readonly string Step1 = UI.Resources.Helper.LangHelper.Instance["AddDevice.Pen.1"];

        public AddPen_OtherRightView(AddDeviceViewModel vm)
        {
            InitializeComponent();
            _vm = vm;

            txtOther.Text = Strings.AddDeviceTypeOther;
            txtCaption.Text = UI.Resources.Helper.LangHelper.Instance["AddDevice.Pen.5"];
            txtStep1.Text = string.Format(Step1, Dns.GetHostName());
            //txtStep2.Text = Step2;
        }

        private void Pairing(object sender, System.Windows.Input.StylusDownEventArgs e)
        {
            MessageModalDialog messageModalDialog;
            Window parentWindow = Window.GetWindow(this);
            if (_vm.IsPandoraPaired)
            {
                messageModalDialog = new(Strings.Error, Strings.PenAlreadyPaired, Strings.Cancel);
                if (parentWindow != null)
                {
                    messageModalDialog.Owner = parentWindow;
                }
                messageModalDialog.ShowDialog();
                return;
            }
            messageModalDialog = new(Strings.PairYourPen, Strings.PairYourPenMessage, Strings.No, Strings.Yes);
            if (parentWindow != null)
            {
                messageModalDialog.Owner = parentWindow;
            }
            if (messageModalDialog.ShowDialog()!.Value)
            {
                _vm.StartPairingPen();
            }
        }
    }
}