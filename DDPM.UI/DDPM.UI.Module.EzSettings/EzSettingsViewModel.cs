using CommunityToolkit.Mvvm.ComponentModel;
using DDPM.UI.Common.Interfaces;

namespace DDPM.UI.Module.EzSettings
{
    internal class EzSettingsViewModel : ObservableObject
    {
        public IModuleOwner? ModuleOwner { get; set; }
    }
}