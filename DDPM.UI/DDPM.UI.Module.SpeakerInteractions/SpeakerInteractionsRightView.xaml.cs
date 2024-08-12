using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using DDPM.UI.Common;
using DDPM.UI.Plugin.Common;
using DDPM.UI.Plugin.ViewModels;
using Dell.Client.Framework.UX.WPF.Controls;
using DDPM.UI.Common.Interfaces;

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
    }
}
