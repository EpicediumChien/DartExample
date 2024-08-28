using DDPM.UI.Common.Interfaces;
using DDPM.UI.Common.Models;
using DDPM.UI.Interfaces;
using DDPM.UI.Plugin.ViewModels;
using System.Diagnostics;
using System.Windows.Controls;

namespace DDPM.UI.Module.AddPen_BL
{
    public class AddPen_BLModule : IDdpmModule
    {
        private readonly UserControl? _leftView = null;
        private readonly UserControl _rightView;

        private bool isSelectChanged = false;
        public bool IsModuleActive { get; set; } = false;

        public AddPen_BLModule(AddDeviceViewModel vm)
        {
            _rightView = new AddPen_BLRightView(vm);
        }

        public string ModuleName { get => "AddPen_BLModule"; }

        public UserControl? GetLeftView()
        {
            return _leftView;
        }

        public UserControl GetRightView()
        {
            return _rightView;
        }

        public HomeDevice? SelectedHomeDevice { get; set; }
        public IModuleOwner? ModuleOwner { get; set; }

        #region Event Handlers

        public void OnSelectedHomeDeviceChanged()
        {
            Trace.WriteLine("AddPen_BLModule.OnSelectedHomeDeviceChanged");
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
            Trace.WriteLine("AddPen_BLModule.OnActivated");
            if (isSelectChanged)
            {
                isSelectChanged = false;
                InitNewViewModel();
            }
        }

        public void OnDeactivated()
        {
            Trace.WriteLine("AddPen_BLModule.OnDeactivated");
        }

        #endregion Event Handlers
    }
}