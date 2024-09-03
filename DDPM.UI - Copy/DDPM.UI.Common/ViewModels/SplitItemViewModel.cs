using CommunityToolkit.Mvvm.ComponentModel;
using DDPM.Easy.Common;
using DDPM.UI.Common.EAEM;
using DDPM.UI.Common.Interfaces;

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

        #endregion ISplit

        #region SplitOwner

        private eSplitOwner _splitOwner = eSplitOwner.None;

        public eSplitOwner SplitOwner
        {
            get => _splitOwner;
            set => SetProperty(ref _splitOwner, value);
        }

        #endregion SplitOwner

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

        #endregion Edit and Delete Icon

        #region ISplitCtrl (EasyArrange)
        private ISplitCtrl? _splitCtrl;
        public ISplitCtrl? SplitCtrl
        {
            get => _splitCtrl;
            set
            {
                SetProperty(ref _splitCtrl, value);
                OnPropertyChanged("TooltipText");
            }
        }
        #endregion ISplitCtrl (EasyArrange)
    }
}