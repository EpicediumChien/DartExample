using CommunityToolkit.Mvvm.ComponentModel;
using DDPM.UI.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDPM.UI.Module.EzSettings
{
    internal class EzSettingsViewModel : ObservableObject
    {
        public IModuleOwner? ModuleOwner { get; set; }
    }
}
