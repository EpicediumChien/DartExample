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

    private string LearnMoreText = "";

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
        txtAlert1.Text = Strings.Alert1;
        bdrAlert1.Visibility = Visibility.Visible;
      }
      else if(!_vm.CTKMessageHelper.IsZoomClientInstalled && _vm.CTKMessageHelper.TeamsSDKState == "SDK_STATE_CLIENT_UNPAIRED") { // Scenario 2
        txtAlert1.Text = Strings.Alert2;
        bdrAlert1.Visibility = Visibility.Visible;
      }
      else if(!_vm.CTKMessageHelper.IsZoomClientInstalled && _vm.CTKMessageHelper.TeamsSDKState == "SDK_STATE_CLIENT_BLOCKED") { // Scenario 3
        txtAlert2.Text = Strings.Alert3;
        bdrAlert2.Visibility = Visibility.Visible;
        txtLearnMore2.Text = Strings.LearnMoreLink;
        LearnMoreText = Strings.LearnMoreText1;
      }

      //txtAlert1.Text = Strings.Alert5;
      //txtLearnMore1.Text = Strings.LearnMoreLink;

      spAlert.Visibility = _vm.IsCollaborationKeyEnable ? Visibility.Visible : Visibility.Collapsed; // Scenario 13
    }

    private void CloseDescription1(object sender, System.Windows.Input.MouseButtonEventArgs e) {
      CloseAlert1();
    }
    private void CloseDescription2(object sender, System.Windows.Input.MouseButtonEventArgs e) {
      CloseAlert2();
    }

    private void LearnMore_Click(object sender, System.Windows.Input.MouseButtonEventArgs e) {
      ShowLearnMore();
    }

    private void ShowLearnMore() {
      MessageModalDialog messageModalDialog = new(Strings.LearnMoreCaption, LearnMoreText, "", "", 600);
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
      //txtLearnMore1.Focus();
    }

    private void Border1_KeyDown(object sender, System.Windows.Input.KeyEventArgs e) {
      if(e.Key == Key.Enter)
        CloseAlert1();
    }
    private void Border2_KeyDown(object sender, System.Windows.Input.KeyEventArgs e) {
      if(e.Key == Key.Enter)
        CloseAlert2();
    }

    private void CloseAlert1() {
      bdrAlert1.Visibility = Visibility.Collapsed;
    }
    private void CloseAlert2() {
      bdrAlert2.Visibility = Visibility.Collapsed;
    }

    private void txtLearnMore1_KeyDown(object sender, KeyEventArgs e) {
      ShowLearnMore();
    }

    private void Collaboration_Checked(object sender, RoutedEventArgs e) {
      spAlert.Visibility = Visibility.Visible;
    }

    private void Collaboration_Unchecked(object sender, RoutedEventArgs e) {
      spAlert.Visibility = Visibility.Collapsed;
    }
  }
}