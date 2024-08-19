using DDPM.UI.Common;
using DDPM.UI.Plugin.Common;
using DDPM.UI.Plugin.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Windows.Media.SpeechSynthesis;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

namespace DDPM.UI.Module.Collaboration {
  /// <summary>
  /// Interaction logic for CollaborationRightView.xaml
  /// </summary>
  public partial class CollaborationRightView : UserControl {
    private readonly KeyboardViewModel _vm;

    private bool HasCTKMessage = true;


    public CollaborationRightView(KeyboardViewModel vm) {
      InitializeComponent();
      _vm = vm;

      txtCollabsCaption.Text = Strings.CollabsCaption;
      txtCollabsTooltip.Text = Strings.CollaborationToolTip;
      cbCollaborationBlinkEffectText.Content = Strings.CollaborationBlinkEffectText;
      cbCollaborationDoubleTapText.Content = Strings.CollaborationDoubleTapText;

      txtVideo.Text = Strings.VideoCaption;
      txtShare.Text = Strings.ShareCaption;
      txtChat.Text = Strings.ChatCaption;
      txtMic.Text = Strings.MicCaption;
      //_vm.OnPropertyChanged(nameof(_vm.IsCollabShadowVisible));
      //bdrVideoShadow.Visibility = _vm.IsCollabShadowVisible ? Visibility.Visible : Visibility.Collapsed;

      CheckCTKMessage();
    }

    void CheckCTKMessage() {
      if(!_vm.CTKMessageHelper.IsZoomClientInstalled && _vm.CTKMessageHelper.TeamsSDKState == "SDK_STATE_NOT_INSTALLED") { // Scenario 1
        txtAlert1.Text = Strings.Alert5;
        txtLearnMore1.Text = Strings.LearnMoreLink;
        _vm.IsCollaborationKeyEnable = false;
        tsCollaboration.IsEnabled = false;
      }
      else {
       HasCTKMessage = true;
      }

      txtAlert1.Text = Strings.Alert5;
      txtLearnMore1.Text = Strings.LearnMoreLink;

      bdrAlert.Visibility = HasCTKMessage && _vm.IsCollaborationKeyEnable ? Visibility.Visible : Visibility.Collapsed; // Scenario 13
    }

    private void CloseDescription(object sender, System.Windows.Input.MouseButtonEventArgs e) {
      CloseAlert();
    }

    private void LearnMore_Click(object sender, System.Windows.Input.MouseButtonEventArgs e) {
      ShowLearnMore();
    }

    private void ShowLearnMore() {
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
      text = Strings.LearnMoreText1;

      MessageModalDialog messageModalDialog = new(Strings.LearnMoreCaption, text, "", Strings.OKCaption, 600);
      Window parentWindow = Window.GetWindow(this);
      if(parentWindow != null) {
        messageModalDialog.Owner = parentWindow;
      }
      messageModalDialog.ShowDialog();
    }

    private void MessageBox_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e) {
      txtMessage.Focus();
    }

    private void MessageBox_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e) {
      //CloseIcon.Focus();
      txtLearnMore1.Focus();
    }

    private void Border_KeyDown(object sender, System.Windows.Input.KeyEventArgs e) {
      if(e.Key == Key.Enter)
        CloseAlert();
    }

    private void CloseAlert() {
      bdrAlert.Visibility = Visibility.Collapsed;
    }

    private void txtLearnMore1_KeyDown(object sender, KeyEventArgs e) {
      ShowLearnMore();
    }

    private void Collaboration_Checked(object sender, RoutedEventArgs e) {
      if(HasCTKMessage)
        bdrAlert.Visibility = Visibility.Visible;
    }

    private void Collaboration_Unchecked(object sender, RoutedEventArgs e) {
      bdrAlert.Visibility = Visibility.Collapsed;
    }
  }
}