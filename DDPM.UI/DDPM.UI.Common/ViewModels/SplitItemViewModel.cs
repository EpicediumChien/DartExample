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
                else if (SplitCtrl != null)
                {
                    //Release version:
                    return $"{SplitCtrl.FriendlyName}";
                    //Debug version
                    //return $"[{CustomId}]{SplitCtrl.FriendlyName}";
                }
                return "";
            }
        }

        public void NotifyPropertyChanged_TooltipText()
        {
            OnPropertyChanged("ToolTipText");
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

        #region CustomId
        private long _customId = 0;
        public long CustomId
        {
            get => _customId;
            set => SetProperty(ref _customId, value);
        }
        #endregion
<<<<<<< HEAD
=======

        #region Screen Orientation

        private bool isVertical = false;

        public bool IsVertical
        {
            get { return isVertical; }
            set { isVertical = value; OnPropertyChanged("IsVertical"); }
        }

        #endregion Screen Orientation

>>>>>>> ac397592151171fdead1fff9c20b6409c75006e3
    }
}