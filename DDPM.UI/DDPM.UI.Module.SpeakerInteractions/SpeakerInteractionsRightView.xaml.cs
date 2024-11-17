using DDPM.UI.Plugin.ViewModels;
using System.Windows.Controls;
using System.Windows.Input;

namespace DDPM.UI.Module.SpeakerInteractions
{
    /// <summary>
    /// Interaction logic for SpeakerInteractionsRightView.xaml
    /// </summary>
    public partial class SpeakerInteractionsRightView : UserControl
    {
        public SoundBarViewModel _vm;

        public SpeakerInteractionsRightView(SoundBarViewModel vm)
        {
            InitializeComponent();
            _vm = vm;
        }

        private void MicrosoftTeamsButton_MouseMove(object sender, MouseEventArgs e)
        {
            _vm.ChangeImage(_vm.Model, "MicrosoftTeams");
        }

        private void ZoomButton_MouseMove(object sender, MouseEventArgs e)
        {
            _vm.ChangeImage(_vm.Model, "Zoom");
        }

        private void GoogleMeetButton_MouseMove(object sender, MouseEventArgs e)
        {
            _vm.ChangeImage(_vm.Model, "GoogleMeet");
        }

        private void SkypeforBusinessButton_MouseMove(object sender, MouseEventArgs e)
        {
            _vm.ChangeImage(_vm.Model, "SkypeforBusiness");
        }

        private void MicrosoftTeamsButton_MouseLeave(object sender, MouseEventArgs e)
        {
            _vm.ChangeImageMouseLeave(_vm.Model);
        }
    }
}