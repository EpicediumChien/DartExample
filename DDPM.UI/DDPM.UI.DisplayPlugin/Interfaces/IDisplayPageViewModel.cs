using DDPM.UI.Common;
using DDPM.UI.Common.Models;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Input;

namespace DDPM.UI.Plugin.DisplayPlugin.Interfaces
{
    internal interface IDisplayPageViewModel
    {
        public void Reset();

        #region Vbar - Unused

        public int VbarSelectedIndex { get; set; }
        public List<VbarItem> VbarItems { get; }
        public ICommand? VbarItemClickCommand { get; set; }

        public int GroupSelIdx { get; set; }
        public int GroupCount { get; }

        #endregion Vbar - Unused

        #region LeftView - Unused

        public UserControl DefaultLeftView { get; }
        public UserControl? LeftView { get; set; }

        #endregion LeftView - Unused

        #region RightView - Unused

        public UserControl? RightView { get; set; }
        public int RightViewHeaderSelectedIndex { get; set; }

        public ObservableCollection<RightViewHeader> RightViewHeaders { get; set; }

        #endregion RightView - Unused

        //Unused
        public List<ModuleGroup> ModuleGroups { get; set; }

        //Unused
        public ModuleGroup? SelectedGroup { get; }

        public List<HomeDevice> HomeDevices { get; set; }
        public HomeDevice? SelectedHomeDevice { get; set; }
        //public MonitorInfo? SelectedMonitorInfo { get; set; }
    }
}