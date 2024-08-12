using DDPM.UI.Common.Models;
using DDPM.UI.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;

namespace DDPM.UI.Common
{
    public class ModuleGroup
    {
        #region Module Group Data
        public string GroupName { get; set; } = "";
        public ImageSource? GroupIcon { get; set; }
        #endregion

        #region Headers
        //Unused
        //private ObservableCollection<RightViewHeader> _groupHeaders = new ObservableCollection<RightViewHeader>();

        #endregion

        #region VbarItem
        public string VbarText { get; set; } = "";
        public ImageSource? VbarIcon { get; set; }
        public ControlTemplate? IconTemplate { get; set; }
        #endregion

        #region ReightViewHeader

        private ObservableCollection<RightViewHeader> _headers = new ObservableCollection<RightViewHeader>();

        public int HeaderCount { get => _headers.Count; }
        public void AddHeader(string headerText, IDdpmModule module)
        {
            _headers.Add(new RightViewHeader(0, headerText, module));
        }
        public void AddHeader(string headerText, Type moduleType)
        {
            _headers.Add(new RightViewHeader(0, headerText, null)
            { ModuleType = moduleType });
        }

        public ObservableCollection<RightViewHeader> Headers { get => _headers; }
        public int HeaderSelectedIndex { get; set; } = 0;


        //Robert_Lin, 2204-7-26
        /// <summary>
        /// Return the index of the RightViewHeader by its ModeuleType
        /// </summary>
        /// <param name="moduleType"></param>
        /// <returns>the index to Headers, or -1 if not found.</returns>
        public RightViewHeader? FindRightViewHeaderByModuleName(string moduleName)
        {
            if (HeaderCount <= 0) return null;

           for (int idx=0; idx<HeaderCount; idx++)
            {
                if (Headers[idx].DdpmModule != null)
                {
                    if (Headers[idx].DdpmModule.ModuleName.Equals(moduleName, StringComparison.OrdinalIgnoreCase))
                        return Headers[idx];
                }
                else
                {
                    string moduleType = Headers[idx].ModuleType.ToString();
                    if (moduleType.Contains(moduleName,StringComparison.OrdinalIgnoreCase))
                        return Headers[idx];
                }
            }
            return null;
        }
        #endregion


    }
}
