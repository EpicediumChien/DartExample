using DDPM.SA.Common.Settings;
using DDPM.UI.Common;
using DDPM.UI.Plugin.Common;
using DDPM.UI.Plugin.ViewModels;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Windows.Media.SpeechSynthesis;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

namespace DDPM.UI.Module.Collaboration
{
    /// <summary>
    /// Interaction logic for CollaborationRightView.xaml
    /// </summary>
    public partial class CollaborationRightView : UserControl
    {
        private readonly KeyboardViewModel _vm;

        private string LearnMoreText = "";

        public CollaborationRightView(KeyboardViewModel vm)
        {
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

            CheckCTKMessage(_vm.CTKMessageHelper.CollaborationMsg);

            //lock/unlock
            if (DdpmCommonHelper.DeviceManagerSA != null)
            {
                DdpmCommonHelper.DeviceManagerSA.ITSettingsActionEvent += DeviceManagerSA_ITSettingsActionEvent;
                DdpmCommonHelper.DeviceManagerSA.DeviceChanged += DeviceManagerSA_DeviceChanged;

                DDPMSettings data = DdpmCommonHelper.DeviceManagerSA.ReloadAppConfigData().Result;
                if (data != null &&
                    data.LockSettings.Lock_Keyboard_CollabScreenShare)
                {
                    bdrShare.Opacity = 0.5;
                    imgLock.Visibility = Visibility.Visible;
                    tsShare.IsEnabled = false;                    
                }
            }
        }

        private void DeviceManagerSA_DeviceChanged(object? sender, SA.Common.DeviceChangedEventArgs e)
        {
            if (e.changedProperty == "CollaborationMsgChanged")
            {
                Dispatcher.Invoke(new Action(() =>
                {
                    CheckCTKMessage(e.device_peripherals.Message);
                }));
            }
        }

        ~CollaborationRightView()
        {
            if (DdpmCommonHelper.DeviceManagerSA != null)
            {
                DdpmCommonHelper.DeviceManagerSA.ITSettingsActionEvent -= DeviceManagerSA_ITSettingsActionEvent;
                DdpmCommonHelper.DeviceManagerSA.DeviceChanged -= DeviceManagerSA_DeviceChanged;
            }
        }

        private void DeviceManagerSA_ITSettingsActionEvent(object? sender, SA.Common.ITSettingEventArgs e)
        {
            bool? isLocked = DdpmCommonHelper.GetUINotifyPropertyValue_Boolean("Lock_Keyboard_CollabScreenShare", e);
            if (isLocked != null)
            {
                Dispatcher.Invoke(new Action(() =>
                {
                    //KeyboardViewModel vm = (KeyboardViewModel)this.DataContext;
                    //if (vm != null)
                    if (_vm != null)
                    {
                        //vm.LockMaskVisible = (bool)isLocked ? Visibility.Visible : Visibility.Collapsed;
                        Trace.WriteLine($"[SettingsPage] Apply Keyboard CollabScreenShare(Lock) : {isLocked}");
                        if (isLocked.Value)
                        {
                            bdrShare.Opacity = 0.5;
                            imgLock.Visibility = Visibility.Visible;
                            tsShare.IsEnabled = false;
                        }
                        else
                        {
                            bdrShare.Opacity = 1;
                            imgLock.Visibility = Visibility.Collapsed;
                            tsShare.IsEnabled = true;
                        }
                    }
                }));
            }
        }

        void CheckCTKMessage(string CTKMessage)
        {
            bdrAlert1.Visibility = Visibility.Collapsed;
            bdrAlert2.Visibility = Visibility.Collapsed;

            if (CTKMessage == "ZoomNotInstalled, TeamsNotInstalled")
            { // Scenario 1
                txtAlert1.Text = Strings.Alert1;
                bdrAlert1.Visibility = Visibility.Visible;
                tsCollaboration.IsEnabled = false;
                _vm.IsCollaborationKeyEnable = false;
                spAlert.Visibility = Visibility.Visible;
                return;
            }
            else if (CTKMessage == "ZoomNotInstalled, ApplicationUnpaired")
            { // Scenario 2
                txtAlert1.Text = Strings.Alert2;
                bdrAlert1.Visibility = Visibility.Visible;
            }
            else if (CTKMessage == "ZoomNotInstalled, ApplicationBlocked")
            { // Scenario 3
                txtAlert2.Text = Strings.Alert3;
                bdrAlert2.Visibility = Visibility.Visible;
                txtLearnMore2.Text = Strings.LearnMoreLink;
                LearnMoreText = Strings.LearnMoreText1;
            }
            else if (CTKMessage == "ZoomIncompatibleVersion, TeamsNotInstalled")
            { // Scenario 4
                txtAlert1.Text = Strings.Alert4;
                bdrAlert1.Visibility = Visibility.Visible;
            }
            else if (CTKMessage == "ZoomNotInstalled, TeamsNotRunning")
            { // Scenario 5
                txtAlert2.Text = Strings.Alert5;
                bdrAlert2.Visibility = Visibility.Visible;
                txtLearnMore2.Text = Strings.LearnMoreLink;
                LearnMoreText = Strings.LearnMoreText2;
            }
            else if (CTKMessage == "ZoomIncompatibleVersion, ApplicationUnpaired")
            { // Scenario 6
                txtAlert1.Text = Strings.Alert2;
                bdrAlert1.Visibility = Visibility.Visible;
                txtAlert2.Text = Strings.Alert4;
                bdrAlert2.Visibility = Visibility.Visible;
                txtLearnMore2.Text = "";
            }
            else if (CTKMessage == "ZoomIncompatibleVersion, ApplicationBlocked")
            { // Scenario 7
                txtAlert1.Text = Strings.Alert4;
                bdrAlert1.Visibility = Visibility.Visible;
                txtAlert2.Text = Strings.Alert3;
                bdrAlert2.Visibility = Visibility.Visible;
                txtLearnMore2.Text = Strings.LearnMoreLink;
                LearnMoreText = Strings.LearnMoreText1;
            }
            else if (CTKMessage == "ZoomIncompatibleVersion, TeamsNotRunning")
            { // Scenario 8
                txtAlert1.Text = Strings.Alert4;
                bdrAlert1.Visibility = Visibility.Visible;
                txtAlert2.Text = Strings.Alert5;
                bdrAlert2.Visibility = Visibility.Visible;
                txtLearnMore2.Text = Strings.LearnMoreLink;
                LearnMoreText = Strings.LearnMoreText2;
            }
            else if (CTKMessage == "ZoomUpToDate, TeamsNotRunning")
            { // Scenario 9
                txtAlert2.Text = Strings.Alert5;
                bdrAlert2.Visibility = Visibility.Visible;
                txtLearnMore2.Text = Strings.LearnMoreLink;
                LearnMoreText = Strings.LearnMoreText2;
            }
            else if (CTKMessage == "ZoomUpToDate, ApplicationBlocked")
            { // Scenario 10
                txtAlert2.Text = Strings.Alert3;
                bdrAlert2.Visibility = Visibility.Visible;
                txtLearnMore2.Text = Strings.LearnMoreLink;
                LearnMoreText = Strings.LearnMoreText1;
            }
            else if (CTKMessage == "ZoomUpToDate, ApplicationUnpaired")
            { // Scenario 11
                txtAlert1.Text = Strings.Alert2;
                bdrAlert1.Visibility = Visibility.Visible;
            }
            else if (CTKMessage == "ZoomIncompatibleVersion, ApplicationPaired")
            { // Scenario 12
                txtAlert1.Text = Strings.Alert4;
                bdrAlert1.Visibility = Visibility.Visible;
            }
            else
            {
                //bdrAlert1.Visibility = Visibility.Collapsed;
                //bdrAlert2.Visibility = Visibility.Collapsed;
            }

            spAlert.Visibility = _vm.IsCollaborationKeyEnable ? Visibility.Visible : Visibility.Collapsed; // Scenario 13
        }

        private void CloseDescription1(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            CloseAlert1();
        }
        private void CloseDescription2(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            CloseAlert2();
        }

        private void LearnMore_Click(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ShowLearnMore();
        }

        private void ShowLearnMore()
        {
            MessageModalDialog messageModalDialog = new(Strings.LearnMoreCaption, LearnMoreText, "", "", "", 600);
            Window mainWindow = System.Windows.Application.Current.MainWindow;
            if (mainWindow != null)
            {
                messageModalDialog.Owner = mainWindow;
                messageModalDialog.Left = mainWindow.Left + (mainWindow!.ActualWidth - 600) / 2;
                messageModalDialog.Top = mainWindow.Top + 300;
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
            //txtLearnMore1.Focus();
        }

        private void Border1_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                CloseAlert1();
        }
        private void Border2_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                CloseAlert2();
        }

        private void CloseAlert1()
        {
            bdrAlert1.Visibility = Visibility.Collapsed;
        }
        private void CloseAlert2()
        {
            bdrAlert2.Visibility = Visibility.Collapsed;
        }

        private void txtLearnMore1_KeyDown(object sender, KeyEventArgs e)
        {
            ShowLearnMore();
        }

        private void Collaboration_Checked(object sender, RoutedEventArgs e)
        {
            spAlert.Visibility = Visibility.Visible;
        }

        private void Collaboration_Unchecked(object sender, RoutedEventArgs e)
        {
            spAlert.Visibility = Visibility.Collapsed;
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            if (DdpmCommonHelper.DeviceManagerSA != null)
            {
                DdpmCommonHelper.DeviceManagerSA.ITSettingsActionEvent -= DeviceManagerSA_ITSettingsActionEvent;
                DdpmCommonHelper.DeviceManagerSA.DeviceChanged -= DeviceManagerSA_DeviceChanged;
            }
        }
    }
}