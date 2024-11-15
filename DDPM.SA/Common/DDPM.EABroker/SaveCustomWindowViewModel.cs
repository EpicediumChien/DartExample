using DDPM.SA.Common;
using DDPM.SA.Common.Display;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.Capture.Frames;

namespace DDPM.EABroker
{
    public class SaveCustomWindowViewModel : INotifyPropertyChanged
    {
        #region Private Members
        private readonly IDeviceManagerSA? _deviceManagerSA;
        private ObservableCollection<SplitJson> _customList = new ObservableCollection<SplitJson>();
        private SplitJson _selectedCustomItem;
        private bool _isSaveButtonEnabled = true;
        private string _windowTitle = "";
        private bool _isAdjustTextVisible = false;
        private bool _isOverlapLayout = false;
        #endregion Private Members

        #region INotifyPropertyChanged Members
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string strPropName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(strPropName));
        }
        #endregion INotifyPropertyChanged Members

        #region ctor
        //public SaveCustomWindowViewModel(IDeviceManagerSA? deviceManager = null)
        public SaveCustomWindowViewModel()
        {
            //if (deviceManager != null)
            //    _deviceManagerSA = deviceManager;
        }
        #endregion

        #region ComboBox Items (CustomList)
        public ObservableCollection<SplitJson> CustomList
        {
            get { return _customList; }
            set
            {
                _customList = value;
                OnPropertyChanged("CustomList");
                OnPropertyChanged("IsSaveButtonEnabled");
            }
        }

        public SplitJson SelectedCustomItem
        {
            get => _selectedCustomItem;
            set
            {
                _selectedCustomItem = value;
                OnPropertyChanged("SelectedCustomItem");
                OnPropertyChanged("IsSaveButtonEnabled");
                OnPropertyChanged("IsComboBoxReadOnly");
            }
        }
        #endregion

        #region Element Enable
        public bool IsSaveButtonEnabled
        {
            get
            {
                if (SelectedCustomItem == null)
                    return false;
                if (String.IsNullOrWhiteSpace(SelectedCustomItem.CustomName))
                    return false;
                return true;
            }
        }
        public void RefreshIsSaveButtonEnabled()
        {
            OnPropertyChanged("IsSaveButtonEnabled");
        }

        public bool IsComboBoxReadOnly
        {
            get
            {
                return (SelectedCustomItem == null);
            }
        }

        public bool IsAdjustTextVisible
        {
            get => _isAdjustTextVisible;
            set
            {
                _isAdjustTextVisible = value;
                OnPropertyChanged("IsAdjustTextVisible");
            }
        }
        #endregion

        #region Window Tittle
        public string WindowTitle
        {
            get => _windowTitle;
            set
            {
                _windowTitle = value;
                OnPropertyChanged("WindowTitle");
            }
        }
        #endregion

        #region IsOverlapLayout
        public bool IsOverlapLayout
        {
            get => _isOverlapLayout;
            set
            {
                _isOverlapLayout = value;
                OnPropertyChanged("IsOverlapLayout");
            }
        }
        #endregion
    }


}
