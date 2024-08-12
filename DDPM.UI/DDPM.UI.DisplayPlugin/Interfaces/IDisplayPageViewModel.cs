using DDPM.UI.Common.Models;
using DDPM.UI.Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Controls;
using VcpCore.Common;
using DDPM.UI.Plugin.DdpmHomePlugin.Model;

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

        #endregion

        #region LeftView - Unused
        public UserControl DefaultLeftView { get; }
        public UserControl? LeftView { get; set; }
        #endregion

        #region RightView - Unused
        public UserControl? RightView { get; set; }
        public int RightViewHeaderSelectedIndex { get; set; }

        public ObservableCollection<RightViewHeader> RightViewHeaders { get; set; }
        #endregion

        //Unused
        public List<ModuleGroup> ModuleGroups { get; set; }

        //Unused
        public ModuleGroup? SelectedGroup { get; }


        public List<HomeDevice> HomeDevices { get; set; }
        public HomeDevice? SelectedHomeDevice { get; set; }
        //public MonitorInfo? SelectedMonitorInfo { get; set; }
    }
}
