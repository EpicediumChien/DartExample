using CommunityToolkit.Mvvm.ComponentModel;
using DDPM.UI.Common.Interfaces;

namespace DDPM.UI.Module.EzArrange
{
    internal class EzArrangeViewModel : ObservableObject
    {
        public IModuleOwner? ModuleOwner { get; set; }
    }
}