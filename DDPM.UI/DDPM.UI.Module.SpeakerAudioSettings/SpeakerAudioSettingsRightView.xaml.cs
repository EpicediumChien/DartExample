using System.Windows.Media;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using DDPM.UI.Plugin.ViewModels;
using Dell.Client.Framework.UX.WPF.Controls;
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
