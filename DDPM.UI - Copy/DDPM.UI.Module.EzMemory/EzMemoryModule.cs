using DDPM.UI.Common.Interfaces;
using DDPM.UI.Common.Models;
using DDPM.UI.Interfaces;
using System.Diagnostics;
using System.Windows.Controls;

namespace DDPM.UI.Module.EzMemory
{
    public class EzMemoryModule : IDdpmModule
    {
        private UserControl? _leftView = null;
        private UserControl _rightView;
        private EzMemoryViewModel vm = new EzMemoryViewModel();

        private bool isSelectChanged = false;
        public bool IsModuleActive { get; set; } = false;

        public EzMemoryModule(IModuleOwner moduleOwner = null)
        {
            _rightView = new EzMemoryRightView(vm);
            //_rightView.DataContext = vm;
        }

        public string ModuleName { get => "EzMemoryModule"; }

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
            Trace.WriteLine("EzMemoryModule.OnActivated");
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