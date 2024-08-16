using DDPM.UI.Plugin.Common;
using DDPM.UI.Plugin.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Windows.Media.SpeechSynthesis;

namespace DDPM.UI.Module.Collaboration
{
    /// <summary>
    /// Interaction logic for CollaborationRightView.xaml
    /// </summary>
    public partial class CollaborationRightView : UserControl
    {
        private readonly KeyboardViewModel _vm;

        private readonly string CollabsCaption1 = "Collaboration Icons";
        private readonly string CollabsCaption2 = "Collaboration";
        private readonly string CollaborationToolTip = "Provides quick access to conference controls. Toggling the\nkeys on/off will show or hide them on the keyboard while in\na Microsoft Teams or Zoom call.";

        //private readonly string CameraInfoTip = "Video icon will be displayed\non Collaboration Keyboard";
        //private readonly string ShareScreenInfoTip = "Screenshare icon will be displayed on\nCollaboration Keyboard";
        //private readonly string ChatInfoTip = "Chat icon will be displayed\non Collaboration Keyboard";
        //private readonly string MicInfoTip = "Mic icon will be displayed\non Collaboration Keyboard";
        private readonly string CollaborationBlinkEffectText = "Enable blink effect when there is a new chat message in conference\ncall";

        private readonly string CollaborationDoubleTapText = "Activate icons on the keyboard by double tapping instead of single\ntapping";
        private readonly string LearnMoreCaption = "Learn More";
        private readonly string LearnMoreText1 = "To use Collaboration Keyboard with Microsoft Teams, open Teams and go to privacy settings.\n\nSelect Third-party app API and ensure that Dell Display and Peripheral Manager is not on the blocked list.\n\nYou’ll then be able to pair Microsoft Teams with Dell Display and Peripheral Manager again by launching a Microsoft Teams conference call.";
        private readonly string LearnMoreText2 = "If you are still having trouble, contact your IT department (if applicable) as they may have policies in place that are prohibiting Collaboration Keyboard from connecting to Microsoft Teams.";
        private readonly string Alert1 = "To use Collaboration Keyboard you need the latest version of Zoom or Microsoft Teams";
        private readonly string Alert2 = "Connect Collaboration Keyboard with Microsoft Teams by starting a conference call and accepting the connection request";
        private readonly string Alert3 = "Collaboration Keyboard cannot connect to Microsoft Teams because you blocked the request. To use Microsoft Teams with Collaboration Keyboard, click \"Learn more\" for instructions on how to re-connect.";
        private readonly string Alert4 = "To use Collaboration Keyboard with Zoom, install Zoom’s latest desktop version";
        private readonly string Alert5 = "To use Collaboration Keyboard with Microsoft Teams, ensure that you are signed into and using the latest version of Microsoft Teams, and that Third-party app API is enabled";
        private readonly string Alert6 = "Connect Collaboration Keyboard with Microsoft Teams by starting a conference call and accepting the connection request";
        private readonly string Alert7 = "";
        private readonly string Alert8 = "";
        private readonly string Alert9 = "";
        private readonly string Alert10 = "";
        private readonly string Alert11 = "";
        private readonly string Alert12 = "";
        private readonly string Alert13 = "";
        private readonly string LearnMoreLink = "Learn more";
        private readonly string VideoCaption = "Video";
        private readonly string ShareCaption = "Share";
        private readonly string ChatCaption = "Chat";
        private readonly string MicCaption = "Mic";
        private readonly string OKCaption = "OK";
        private SpeechSynthesizer _synthesizer;

        public CollaborationRightView(KeyboardViewModel vm)
        {
            InitializeComponent();
            _vm = vm;
            _synthesizer = new SpeechSynthesizer();

            txtCollabsCaption.Text = CollabsCaption1;
            txtCollabsTooltip.Text = CollaborationToolTip;
            cbCollaborationBlinkEffectText.Content = CollaborationBlinkEffectText;
            cbCollaborationDoubleTapText.Content = CollaborationDoubleTapText;

            txtAlert1.Text = Alert5;
            txtLearnMore1.Text = LearnMoreLink;
            txtVideo.Text = VideoCaption;
            txtShare.Text = ShareCaption;
            txtChat.Text = ChatCaption;
            txtMic.Text = MicCaption;
            //_vm.OnPropertyChanged(nameof(_vm.IsCollabShadowVisible));
            //bdrVideoShadow.Visibility = _vm.IsCollabShadowVisible ? Visibility.Visible : Visibility.Collapsed;
        }

        private void CloseDescription(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            CloseAlert();
        }

        private void LearnMore_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ShowLearnMore();
        }

        private void ShowLearnMore()
        {
            string text;
            //Robert_Lin, 2024-6-26, fix SAST issue: [Bug] Correct one of the identical expression of both side of operator '=='
            //OLD Code:
            /*
      if(1 == 1) {
        text = LearnMoreText1;
      }
      else {
        text = LearnMoreText2;
      }
            */
            //NEW Code:
            text = LearnMoreText1;

            MessageModalDialog messageModalDialog = new(LearnMoreCaption, text, "", OKCaption, 600);
            Window parentWindow = Window.GetWindow(this);
            if (parentWindow != null)
            {
                messageModalDialog.Owner = parentWindow;
            }
            messageModalDialog.ShowDialog();
        }

        private void MessageBox_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            txtMessage.Focus();
        }

        private void MessageBox_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            //CloseIcon.Focus();
            txtLearnMore1.Focus();
        }

        private void Border_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                CloseAlert();
        }

        private void CloseAlert()
        {
            bdrAlert.Visibility = Visibility.Collapsed;
            txtCollabsCaption.Text = CollabsCaption2;
        }

        private void txtLearnMore1_KeyDown(object sender, KeyEventArgs e)
        {
            ShowLearnMore();
        }
    }
}