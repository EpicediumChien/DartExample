using DDPM.UI.Common.Models;
using DDPM.UI.Interfaces;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Media;

namespace DDPM.UI.Common
{
    public class ModuleGroup
    {
        #region Module Group Data

        public string GroupName { get; set; } = "";
        public ImageSource? GroupIcon { get; set; }

        #endregion Module Group Data

        #region Headers

        //Unused
        //private ObservableCollection<RightViewHeader> _groupHeaders = new ObservableCollection<RightViewHeader>();

        #endregion Headers

        #region VbarItem

        public string VbarText { get; set; } = "";
        public ImageSource? VbarIcon { get; set; }
        public ControlTemplate? IconTemplate { get; set; }

        #endregion VbarItem

        #region RightViewHeader

        private ObservableCollection<RightViewHeader> _headers = new ObservableCollection<RightViewHeader>();

        public int HeaderCount { get => _headers.Count; }

        public void AddHeader(string headerText, IDdpmModule module)
        {
            RightViewHeader header = new RightViewHeader(0, headerText, module);
            if (module != null)
            {
                header.ModuleName = module.ModuleName;
            }
            _headers.Add(header);
        }

        public void AddHeader(string headerText, Type moduleType, string moduleName="")
        {
            _headers.Add(new RightViewHeader(0, headerText, null)
            {
                ModuleType = moduleType,
                ModuleName = moduleName
            });
        }

        public ObservableCollection<RightViewHeader> Headers { get => _headers; }
        public int HeaderSelectedIndex { get; set; } = 0;


        #endregion RightViewHeader

        #region Find
        //Robert_Lin, 2204-7-26
        /// <summary>
        /// Find the RightViewHeader by ModuleName
        /// </summary>
        /// <param name="moduleName">The ModuleName to find
        /// One of DDPM.UI.Common.Constants.ModuleName_XXXX
        /// </param>
        /// <returns>the index to Headers, or -1 if not found.</returns>
        public RightViewHeader? FindRightViewHeaderByModuleName(string moduleName)
        {
            if (HeaderCount <= 0) return null;

            for (int idx = 0; idx < HeaderCount; idx++)
            {
                if (Headers[idx].DdpmModule != null)
                {
                    if (Headers[idx].DdpmModule.ModuleName.Equals(moduleName, StringComparison.OrdinalIgnoreCase))
                        return Headers[idx];
                }
                else
                {
                    string moduleType = Headers[idx].ModuleType.ToString();
                    if (moduleType.Contains(moduleName, StringComparison.OrdinalIgnoreCase))
                        return Headers[idx];
                }
            }
            return null;
        }

        /// <summary>
        /// Find the index of ReightViewHeader by ModuleName
        /// </summary>
        /// <param name="moduleName">The ModuleName to find
        /// One of DDPM.UI.Common.Constants.ModuleName_XXXX
        /// </param>
        /// <returns>Return the index, or -1 if not found.</returns>
        public int FindRightViewHeaderIndexByModuleName(string moduleName)
        {
            if (HeaderCount <= 0) return -1;

            for (int idx = 0; idx < HeaderCount; idx++)
            {
                if (Headers[idx].DdpmModule != null)
                {
                    if (Headers[idx].DdpmModule.ModuleName.Equals(moduleName, StringComparison.OrdinalIgnoreCase))
                        return idx;
                }
                else //The DdpmModule did not been created
                {
                    if (Headers[idx].ModuleName.Equals(moduleName, StringComparison.OrdinalIgnoreCase))
                        return idx;
                }
            }
            return -1;
        }

        /// <summary>
        /// Find the index of ReightViewHeader by ModuleType
        /// </summary>
        /// <param name="moduleType">The ModuleType to find
        /// One of DDPM.UI.Common.Constants.ModuleName_XXXX
        /// </param>
        /// <returns>Return the index, or -1 if not found.</returns>
        public int FindRightViewHeaderIndexByModuleType(Type moduleType)
        {
            if (HeaderCount <= 0) return -1;

            for (int idx = 0; idx < HeaderCount; idx++)
            {
                if (Headers[idx].DdpmModule != null)
                {
                    IDdpmModule mod = Headers[idx].DdpmModule;
                    if (mod.GetType().Equals(moduleType))
                        return idx;
                }
                else //The DdpmModule did not been created
                {
                    if (Headers[idx].ModuleType == moduleType)
                        return idx;
                }
            }
            return -1;
        }

        /// <summary>
        /// Set the specific ModuleName as current selected RightViewHeader
        /// </summary>
        /// <param name="moduleName">The ModuleName to find
        /// One of DDPM.UI.Common.Constants.ModuleName_XXXX
        /// </param>
        /// <returns>The index of the new selected RightViewHeader (=HeaderSelectedIndex)
        /// if Fail to find the terget Module, will return -1, but HeaderSelectedIndex will not be updated.</returns>
        public int SetSelectedRightVewHeaderByModuleName(string moduleName)
        {
            int idxModule = FindRightViewHeaderIndexByModuleName(moduleName);
            if (idxModule == -1) return -1;

            HeaderSelectedIndex = idxModule;
            return idxModule;
        }
        #endregion //Find

    }
}