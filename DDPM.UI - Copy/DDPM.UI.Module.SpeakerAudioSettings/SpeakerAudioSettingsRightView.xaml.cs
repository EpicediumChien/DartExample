using DDPM.UI.Plugin.ViewModels;
using UserControl = System.Windows.Controls.UserControl;

namespace DDPM.UI.Module.SpeakerAudioSettings
{
    /// <summary>
    /// Interaction logic for SpeakerAudioSettingsRightView.xaml
    /// </summary>
    public partial class SpeakerAudioSettingsRightView : UserControl
    {
        private readonly SoundBarViewModel _vm;

        public SpeakerAudioSettingsRightView(SoundBarViewModel vm)
        {
            InitializeComponent();
            _vm = vm;
        }
    }
}