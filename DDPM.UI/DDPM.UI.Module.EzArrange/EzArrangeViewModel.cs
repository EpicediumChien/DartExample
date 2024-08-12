using CommunityToolkit.Mvvm.ComponentModel;
using DDPM.UI.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DDPM.UI.Module.EzArrange
{
    internal class EzArrangeViewModel : ObservableObject
    {
        public IModuleOwner? ModuleOwner { get; set; }
    }
}
