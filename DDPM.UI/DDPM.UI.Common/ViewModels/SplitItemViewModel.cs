using CommunityToolkit.Mvvm.ComponentModel;
using DDPM.UI.Common.EAEM;
using DDPM.UI.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DDPM.UI.Common.ViewModels
{

    public class SplitItemViewModel : ObservableObject
    {
        #region ISplit
        private ISplit? _split;
        public ISplit? Split
        {
            get => _split;
            set
            {
                SetProperty(ref _split, value);
                OnPropertyChanged("TooltipText");
            }
        }
        #endregion

        #region SplitOwner
        private eSplitOwner _splitOwner = eSplitOwner.None;
        public eSplitOwner SplitOwner 
        {
            get => _splitOwner;
            set => SetProperty(ref _splitOwner, value);
        }
        #endregion

        /*
        private ICommand? _clickCommand;
        public ICommand? ClickCommand
        {
            get => _clickCommand;
            set => SetProperty(ref _clickCommand, value);
        }
        */

        public string ToolTipText
        {
            get
            {
                if (_split != null)
                    return _split.Description;
                else 
                    return "";
            }
        }

        #region Edit and Delete Icon
        //private bool _isEditEnabled = false;

        //public bool IsEditEnabled
        //{
        //    get => _isEditEnabled;
        //    set => SetProperty(ref _isEditEnabled, value);
        //}

        #endregion

    }
}
