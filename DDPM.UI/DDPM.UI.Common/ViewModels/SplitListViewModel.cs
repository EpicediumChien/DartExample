using CommunityToolkit.Mvvm.ComponentModel;
using DDPM.UI.Common.EAEM;
using DDPM.UI.Common.UserControls;
using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace DDPM.UI.Common.ViewModels
{
    public class SplitListViewModel : ObservableObject
    {
        #region SplitOwner

        //The owner must set this value before adding Split items into the ItemsSource
        public eSplitOwner SplitOwner { get; set; } = eSplitOwner.None;

        #endregion SplitOwner

        #region ItemsSource

        private ObservableCollection<SplitItem> _splitList = new ObservableCollection<SplitItem>();

        public ObservableCollection<SplitItem> SplitList
        {
            get => _splitList;
            set => SetProperty(ref _splitList, value);
        }

        public int ItemCount { get => SplitList.Count; }

        public void AddSplitItemToList(SplitItem spItem)
        {
            SplitList.Add(spItem);
            //OnPropertyChanged("SplitList");
            RefreshDisplayItems();
        }

        public void ClearList()
        {
            SplitList = new ObservableCollection<SplitItem>();
        }

        #endregion ItemsSource

        #region Index

        private int _indexToItem0 = 0;

        //The index in SplitList, the index of Item0
        public int IndexToItem0
        {
            get => _indexToItem0;
            set => SetProperty(ref _indexToItem0, value);
        }

        //Check if specified index is a valid index value
        public bool IsIndexValid(int idx)
        {
            if (idx < 0)
                return false;
            if (idx >= ItemCount)
                return false;
            return true;
        }

        #endregion Index

        #region Split Items

        public ContentControl? SplitItem0
        {
            get
            {
                if (IsIndexValid(IndexToItem0))
                    return SplitList[IndexToItem0];
                else
                    return null;
            }
        }

        public ContentControl? SplitItem1
        {
            get
            {
                if (IsIndexValid(IndexToItem0 + 1))
                    return SplitList[IndexToItem0 + 1];
                else
                    return null;
            }
        }

        public ContentControl? SplitItem2
        {
            get
            {
                if (IsIndexValid(IndexToItem0 + 2))
                    return SplitList[IndexToItem0 + 2];
                else
                    return null;
            }
        }

        public ContentControl? SplitItem3
        {
            get
            {
                if (IsIndexValid(IndexToItem0 + 3))
                    return SplitList[IndexToItem0 + 3];
                else
                    return null;
            }
        }

        public ContentControl? SplitItem4
        {
            get
            {
                if (IsIndexValid(IndexToItem0 + 4))
                    return SplitList[IndexToItem0 + 4];
                else
                    return null;
            }
        }

        public void RefreshDisplayItems()
        {
            OnPropertyChanged("SplitItem0");
            OnPropertyChanged("SplitItem1");
            OnPropertyChanged("SplitItem2");
            OnPropertyChanged("SplitItem3");
            OnPropertyChanged("SplitItem4");
            RefreshPrevNextButtons();
        }

        #endregion Split Items

        #region Page Navigation

        private int _itemsPerPage = 5;

        public int ItemsPerPage
        {
            get => _itemsPerPage;
            set
            {
                _itemsPerPage = value;
                OnPropertyChanged("ItemsPerPage");
                RefreshDisplayItems();
                //RefreshPrevNextButtons();
            }
        }

        public bool IsPrevButtonEnabled
        {
            get
            {
                if (ItemCount <= 0)
                    return false;
                if (IndexToItem0 <= 0)
                    return false;
                return true;
            }
        }

        public bool IsNextButtonEnabled
        {
            get
            {
                if (ItemCount <= 0)
                    return false;
                if (IsIndexValid(IndexToItem0 + ItemsPerPage))
                    return true;
                else
                    return false;
            }
        }

        public void RefreshPrevNextButtons()
        {
            OnPropertyChanged("IsPrevButtonEnabled");
            OnPropertyChanged("IsNextButtonEnabled");
        }

        public bool GoToNextPage()
        {
            if (IsIndexValid(IndexToItem0 + ItemsPerPage))
            {
                IndexToItem0 += ItemsPerPage;
                RefreshDisplayItems();
                RefreshPrevNextButtons();
                return true;
            }
            return false;
        }

        public bool GoToPrevPage()
        {
            if (IsIndexValid(IndexToItem0 - ItemsPerPage))
            {
                IndexToItem0 -= ItemsPerPage;
                RefreshDisplayItems();
                RefreshPrevNextButtons();
                return true;
            }
            return false;
        }

        #endregion Page Navigation

        //private ICommand? _itemEditCommand;

        //public ICommand? ItemEditCommand
        //{
        //    get => _itemEditCommand;
        //    set => SetProperty(ref _itemEditCommand, value);
        //}
    }
}