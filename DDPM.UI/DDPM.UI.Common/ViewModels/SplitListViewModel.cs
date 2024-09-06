using CommunityToolkit.Mvvm.ComponentModel;
using DDPM.SA.Common.Display;
using DDPM.UI.Common.EAEM;
using DDPM.UI.Common.Interfaces;
using DDPM.UI.Common.UserControls;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Forms;

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

        #region Find
        public SplitItem? FindSplitCtrl(int cellCount, char splitKey)
        {
            if (_splitList == null) return null;
            if (_splitList.Count == 0) return null;

            foreach (SplitItem spItem in _splitList)
            {
                if (spItem.ISplitCtrl != null)
                {
                    if ((spItem.ISplitCtrl.CellCount == cellCount) &&
                        (spItem.ISplitCtrl.SplitKey == splitKey))
                        return spItem;
                }
            }
            return null;
        }

        public SplitItem? FindSplitCtrlByFriendlyName(string friendlyName)
        {
            if (_splitList == null) return null;
            if (_splitList.Count == 0) return null;

            foreach (SplitItem spItem in _splitList)
            {
                if (spItem.ISplitCtrl != null)
                {
                    if (spItem.ISplitCtrl.FriendlyName.Equals(friendlyName, StringComparison.OrdinalIgnoreCase))
                        return spItem;
                }
            }
            return null;
        }

        public SplitItem? FindSplitItemByCustomId(long customId)
        {
            if (_splitList == null) return null;
            if (_splitList.Count == 0) return null;

            foreach (SplitItem spItem in _splitList)
            {
                if (spItem.CustomId == customId)
                {
                    return spItem;
                }
            }
            return null;

        }

        public List<string> GetCustomFriendlyNameList()
        {
            List<string> listOut = new List<string>();
            foreach (SplitItem spItem in _splitList)
            {
                if (spItem.ISplitCtrl != null)
                {
                    if (!String.IsNullOrWhiteSpace(spItem.ISplitCtrl.FriendlyName))
                        listOut.Add(spItem.ISplitCtrl.FriendlyName);
                }
            }
            return listOut;

        }

        public SplitItem? FindItemBySplitJson(SplitJson spj)
        {
            if (_splitList == null) return null;
            if (_splitList.Count == 0) return null;

            foreach (SplitItem spItem in _splitList)
            {
                //Compare if it's equal between SplitItem and SplitJson
                //  1 CustomId must be the same
                //  2 If CustomId==0, the compare (CellCount, SplitKey)
                //
                //1 CustomId must be the same
                if (spItem.CustomId != spj.CustomId)
                    continue;

                //2 If CustomId==0, the compare (CellCount, SplitKey)
                if (spItem.CustomId == 0)
                {
                    if ((spItem.CellCount == spj.CellCount) && (spItem.SplitKey == spj.SplitKey))
                        return spItem;
                }
            }
            return null;
        }
        #endregion Find

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

        public void GotoFirstPage()
        {
            IndexToItem0 = 0;
            RefreshDisplayItems();
            RefreshPrevNextButtons();
        }

        /// <summary>
        /// Navigate to the page which contains the first IsSelected item
        /// Return: true if a IsSelected item found and page navigated
        /// </summary>
        public bool GotoFirstSelectedItemPage()
        {
            //Find the index of SplitItem which is IsSelected
            int idx = 0;
            foreach (SplitItem spItem in SplitList)
            {
                if (spItem.IsSelected)
                {
                    //Calculate the IndexToItem0
                    // idx      IndexToItem0   (ItemsPerPage=5)
                    // 0~4      0         = (idx/ItemsPerPage)*ItemsPerPage
                    // 5~9      5
                    // 10~14    10
                    // 15~19    15
                    int pageNo = (idx / ItemsPerPage);
                    IndexToItem0 = pageNo * ItemsPerPage;
                    RefreshDisplayItems();
                    RefreshPrevNextButtons();
                    break;
                }
                idx++;
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