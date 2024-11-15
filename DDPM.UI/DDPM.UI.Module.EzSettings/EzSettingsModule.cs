using DDPM.UI.Common;
using DDPM.UI.Common.Interfaces;
using DDPM.UI.Common.Models;
using DDPM.UI.Interfaces;
using Dell.Client.Framework.Common;
using System.Diagnostics;
using System.Windows.Controls;

namespace DDPM.UI.Module.EzSettings
{
    public class EzSettingsModule : IDdpmModule
    {
        private UserControl? _leftView = null;
        private UserControl _rightView = null;
        private EzSettingsViewModel vm;

        private bool isSelectChanged = false;
        public bool IsModuleActive { get; set; } = false;

        public EzSettingsModule(IModuleOwner moduleOwner = null)
        {
            vm = new EzSettingsViewModel(moduleOwner);
            if (DdpmCommonHelper.MyConsole != null)
            {
                vm.Log = (ILog?)DdpmCommonHelper.MyConsole.CreateLog("EzSettings");
            }
            _rightView = new EzSettingsRightView(vm);
        }

        public string ModuleName { get => Constants.ModuleName_EzSettings; } //"EzSettingsModule"

        public UserControl? GetLeftView()
        {
            return (UserControl?)_leftView;
        }

        public UserControl GetRightView()
        {
            return _rightView;
        }

        public HomeDevice? SelectedHomeDevice { get; set; }

        #region ModuleOwner

        public IModuleOwner? ModuleOwner
        {
            get => vm.ModuleOwner;
            set => vm.ModuleOwner = value;
        }

        #endregion ModuleOwner

        #region Event Handlers

        public void OnSelectedHomeDeviceChanged()
        {
            isSelectChanged = true;
            if (IsModuleActive)
            {
                isSelectChanged = false;
                InitNewViewModel();
            }
        }

        //Handle new device coming
        private void InitNewViewModel()
        {
        }

        public void OnActivated()
        {
            Trace.WriteLine("EzSettingsModule.OnActivated");
            if (isSelectChanged)
            {
                isSelectChanged = false;
                InitNewViewModel();
            }
        }

        public void OnDeactivated()
        {
        }

        #endregion Event Handlers
    }
}