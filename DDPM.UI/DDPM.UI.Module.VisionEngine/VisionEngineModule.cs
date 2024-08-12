
using DDPM.UI.Common.Interfaces;
using DDPM.UI.Common.Models;
using DDPM.UI.Interfaces;
using System.Diagnostics;
using System.Windows.Controls;
//using DDPM.UI.Plugin.Common.ViewModels;

namespace DDPM.UI.Module.VisionEngine
{
    public class VisionEngineModule : IDdpmModule
    {
        private VisionEngineRightView _rightView;
        //private DisplayViewModel _vmDisplay;
        public string ModuleName { get => "VisionEngineModule"; }

        public VisionEngineModule(IModuleOwner? moduleOwner = null)
        {
            //_vmDisplay = vmDisplay;
        }

        public UserControl? GetLeftView()
        {
            return null;
        }

        public UserControl GetRightView()
        {
            if (_rightView == null)
            {
                _rightView = new VisionEngineRightView();
            }
            return _rightView;
        }
        public HomeDevice? SelectedHomeDevice { get; set; }

        #region ModuleOwner
        public IModuleOwner? ModuleOwner
        {
            get;
            set;
        }
        #endregion

        #region Event Handlers
        public void OnSelectedHomeDeviceChanged()
        //    public void OnSelectedHomeDeviceChanged(HomeDevice homeDevice)
        {
            Trace.WriteLine("PipPbpModule.OnSelectedHomeDeviceChanged");
        }
        public void OnActivated()
        {
            Trace.WriteLine("PipPbpModule.OnActivated");
        }
        public void OnDeactivated()
        {
            Trace.WriteLine("PipPbpModule.OnDeactivated");
        }
        #endregion
    }

}
