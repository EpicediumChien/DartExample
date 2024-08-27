using DDPM.UI.Common.Interfaces;
using DDPM.UI.Common.Models;
using DDPM.UI.Interfaces;
using DDPM.UI.Plugin.ViewModels;
using System.Diagnostics;
using System.Windows.Controls;

namespace DDPM.UI.Module.AddPen_Other
{
    public class AddPen_OtherModule : IDdpmModule
    {
        private readonly UserControl? _leftView;
        private readonly UserControl? _rightView;

        private bool isSelectChanged = false;
        public bool IsModuleActive { get; set; } = false;

        public AddPen_OtherModule()
        { }

        public AddPen_OtherModule(AddDeviceViewModel vm)
        {
            _rightView = new AddPen_OtherRightView(vm);
        }

        public string ModuleName { get => "AddPen_OtherModule"; }

        public UserControl? GetLeftView()
        {
            return _leftView;
        }

        public UserControl GetRightView()
        {
            return _rightView!;
        }

        public HomeDevice? SelectedHomeDevice { get; set; }
        public IModuleOwner? ModuleOwner { get; set; }

        #region Event Handlers

        public void OnSelectedHomeDeviceChanged()
        {
            Trace.WriteLine("AddPen_OtherModule.OnSelectedHomeDeviceChanged");
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
            Trace.WriteLine("AddPen_OtherModule.OnActivated");
            if (isSelectChanged)
            {
                isSelectChanged = false;
                InitNewViewModel();
            }
        }

        public void OnDeactivated()
        {
            Trace.WriteLine("AddPen_OtherModule.OnDeactivated");
        }

        #endregion Event Handlers
    }
}