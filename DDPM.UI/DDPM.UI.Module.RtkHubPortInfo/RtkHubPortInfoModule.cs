using DDPM.UI.Common.Interfaces;
using DDPM.UI.Common.Models;
using DDPM.UI.Interfaces;
using DDPM.UI.Plugin.ViewModels;
using System.Diagnostics;
using System.Windows.Controls;

namespace DDPM.UI.Module.RtkHubPortInfo
{
    public class RtkHubPortInfoModule : IDdpmModule
    {
        private UserControl? _leftView = null;
        private UserControl _rightView;

        private bool isSelectChanged = false;
        public bool IsModuleActive { get; set; } = false;

        public RtkHubPortInfoModule(RtkHubViewModel vm)
        {
            _rightView = new RtkHubPortInfoRightView(vm);
        }

        public string ModuleName { get => "RtkHubPortInfoModule"; }

        public UserControl? GetLeftView()
        {
            return _leftView;
        }

        public UserControl GetRightView()
        {
            return _rightView;
        }

        public HomeDevice SelectedHomeDevice { get; set; }
        public IModuleOwner? ModuleOwner { get; set; }

        #region Event Handlers

        public void OnSelectedHomeDeviceChanged()
        {
            Trace.WriteLine("RtkHubPortInfoModule.OnSelectedHomeDeviceChanged");
            isSelectChanged = true;
            if (IsModuleActive)
            {
                isSelectChanged = false;
            }
        }

        public void OnActivated()
        {
            Trace.WriteLine("RtkHubPortInfoModule.OnActivated");
            if (isSelectChanged)
            {
                isSelectChanged = false;
            }
        }

        public void OnDeactivated()
        {
            Trace.WriteLine("RtkHubPortInfoModule.OnDeactivated");
        }

        #endregion Event Handlers
    }
}