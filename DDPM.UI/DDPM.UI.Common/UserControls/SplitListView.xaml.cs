using CommunityToolkit.Mvvm.Input;
using DDPM.Easy.Common;
using DDPM.SA.Common.Display;
using DDPM.UI.Common.EAEM;
using DDPM.UI.Common.Interfaces;
using DDPM.UI.Common.ViewModels;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using UserControl = System.Windows.Controls.UserControl;

namespace DDPM.UI.Common.UserControls
{
    /// <summary>
    /// Interaction logic for SplitListView.xaml
    /// </summary>
    public partial class SplitListView : UserControl, IDisposable
    {
        private SplitListViewModel vm = new SplitListViewModel();
        private ICommand? _splitItemClickCommand;
        private SplitItem spItem;

        #region ctor
        public SplitListView()
        {
            InitializeComponent();
            DataContext = vm;

            addButton.ClickCommand = new RelayCommand<AddCustomLayoutButton>(HandleAddButtonClickCommand);
        }
        #endregion ctor

        #region Exit
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                addButton.ClickCommand = null;
                spItem.Dispose();

                ClearList();
                BindingOperations.ClearAllBindings(this);
            }
        }

        ~SplitListView()
        {
            Dispose(false);
        }
        #endregion Exit

        #region SplitOwner
        //Owner must assign this value before calling to AddSplitToList()
        public eSplitOwner SplitOwner
        {
            get => vm.SplitOwner;
            set => vm.SplitOwner = value;
        }
        #endregion SplitOwner

        #region SplitList
        public ObservableCollection<SplitItem> SplitList
        {
            get { return vm.SplitList; }
        }

        public int ItemCount { get { return vm.ItemCount; } }

        public SplitItem? GetAt(int index)
        {
            if (index < 0 || index >= ItemCount) return null;
            return vm.SplitList[index];
        }

        public void ClearList()
        {
            DataContext = null;
            vm.ClearList();
            DataContext = vm;
            vm.RefreshDisplayItems();
            vm.RefreshPrevNextButtons();
        }
        #endregion SplitList

        #region Add Item
        public SplitItem AddSplitToList(ISplit split)
        {
            spItem = new SplitItem();
            spItem.InnerContent = split;
            spItem.SplitOwner = vm.SplitOwner;
            spItem.ClickCommand = new RelayCommand<SplitItem>(HandleSplitItemClickCommand);
            vm.AddSplitItemToList(spItem);
            return spItem;
        }

        public SplitItem AddItemToList(ContentControl contentControl)
        {
            spItem = new SplitItem();
            spItem.InnerContent = contentControl;
            spItem.SplitOwner = vm.SplitOwner;
            spItem.ClickCommand = new RelayCommand<SplitItem>(HandleSplitItemClickCommand);
            spItem.EditClickCommand = new RelayCommand<SplitItem>(HandleSplitItemEditClickCommand);
            spItem.DeleteCommand = new RelayCommand<SplitItem>(HandleSplitItemDeleteCommand);

            if (vm.SplitOwner == eSplitOwner.EaCustom)
            {
                spItem.IsDeleteEnabled = true;
                spItem.IsEditEnabled = !spItem.IsAddedCustomLayout;

            }
            else if (vm.SplitOwner == eSplitOwner.EaWin)
            {
                spItem.IsEditEnabled = true;
            }

            if (spItem.ISplitCtrl != null)
                spItem.ISplitCtrl.IsVertical = IsVertical;

            vm.AddSplitItemToList(spItem);
            return spItem;
        }

        public SplitItem InsertSplitCtrlToList(ISplitCtrl isp, int index)
        {
            //Validate index
            if (index < 0)
                index = 0; //Insert to the first
            else if (index > ItemCount)
                index = ItemCount; //Insert to the end

            spItem = new SplitItem();
            spItem.InnerContent = isp.UC;
            spItem.SplitOwner = vm.SplitOwner;
            spItem.ClickCommand = new RelayCommand<SplitItem>(HandleSplitItemClickCommand);
            spItem.EditClickCommand = new RelayCommand<SplitItem>(HandleSplitItemEditClickCommand);
            spItem.DeleteCommand = new RelayCommand<SplitItem>(HandleSplitItemDeleteCommand);

            if (vm.SplitOwner == eSplitOwner.EaCustom)
            {
                spItem.IsDeleteEnabled = true;
                spItem.IsEditEnabled = !spItem.IsAddedCustomLayout;
            }
            else if (vm.SplitOwner == eSplitOwner.EaWin)
            {
                spItem.IsEditEnabled = true;
            }

            if (spItem.ISplitCtrl != null)
                spItem.ISplitCtrl.IsVertical = IsVertical;

            DataContext = null;
            vm.SplitList.Insert(index, spItem);
            DataContext = vm;
            vm.RefreshDisplayItems();
            return spItem;
        }

        #endregion Add Item

        #region Split List Operations



        /// <summary>
        /// Return a list of FriendlyNames for Custom List before starting edit.
        /// If CustomList is empty, then return the first FriendlyName
        /// </summary>
        /// <returns></returns>
        public List<string> GetCustomFriendlyNames()
        {
            return vm.GetCustomFriendlyNameList();
        }
        #endregion Split List Operations

        #region Click and Item Selection

        public ICommand ItemClickCommand
        {
            get { return (ICommand)GetValue(ItemClickCommandProperty); }
            set { SetValue(ItemClickCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ClickCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemClickCommandProperty =
            DependencyProperty.Register("ItemClickCommand", typeof(ICommand), typeof(SplitListView));

        //public ICommand? SplitItemClickCommand
        //{
        //    get => _splitItemClickCommand;
        //    set => _splitItemClickCommand = value;
        //}
        private void HandleSplitItemClickCommand(SplitItem spItem)
        {
            //Robert_Lin, bug, it will be null. so the ListView owner will not be notified
            if (ItemClickCommand != null)
                ItemClickCommand.Execute(spItem);
            //The ICommand binding to SplitListView will be failed. for example:
            // in XAML:
            // <SplitListView Name="splitLV1" ItemClickCommand={Binding LV1ItemClickCommand}"/>
            // in ViewModel:
            //  private ICommand? _lv1ClickCommand;
            //  private void OnLv1ItemClicked(SplitItem clickedItem)
            //  {
            //  }
            // in ctor of ViewModel:
            //     _lv1ClickCommand = new RelayCommand<SplitItem>(OnLv1ItemClicked);
            //Issue: OnLv1ItemClicked() never be called.
            //Root cause: in above code: ItemClickCommand will always null.
            //   if (ItemClickCommand != null)
            //       ItemClickCommand.Execute(spItem);
            //Workaround:
            // Do not uning data binding to ItemClickCommand, use property set instead.
            // For example:
            // in View's C# code:
            //    splitLV1.ItemClickCommand = new RelayCommand<SplitItem>(_viewModel.OnLv1ItemClicked);
            //Optional:
            // Remove teh data binding in XAML to avoid confuse:
            //    <SplitListView Name="splitLV1" />   (remove ItemClickCommand binding)
        }

        #endregion Click and Item Selection

        public event RoutedEventHandler? SelectionChanged;

        #region Page Navigation
        //Unused
        //private void splitArrowPrev_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        //{
        //     vm.GoToPrevPage();
        //}
        //Unused
        //private void splitArrowNext_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        //{
        //    vm.GoToNextPage();
        //}
        private void prevBtn_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            vm.GoToPrevPage();
        }

        private void nextBtn_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            vm.GoToNextPage();
        }

        public bool GotoFirstSelectedItemPage()
        {
            return vm.GotoFirstSelectedItemPage();
        }
        private void prevButton_Click(object sender, RoutedEventArgs e)
        {
            vm.GoToPrevPage();
        }
        private void nextButton_Click(object sender, RoutedEventArgs e)
        {
            vm.GoToNextPage();
        }

        #endregion Page Navigation

        #region SplitItem Edit Command
        /// <summary>
        /// Handle the event from SplitItem's pencil item clicking
        /// </summary>
        /// <param name="spItem"></param>
        private void HandleSplitItemEditClickCommand(SplitItem spItem)
        {
            if (ItemEditCommand != null)
                ItemEditCommand.Execute(spItem);
        }

        public ICommand ItemEditCommand
        {
            get { return (ICommand)GetValue(ItemEditCommandProperty); }
            set { SetValue(ItemEditCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ItemEditCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemEditCommandProperty =
            DependencyProperty.Register("ItemEditCommand", typeof(ICommand), typeof(SplitListView));

        #endregion SplitItem Edit Command

        #region SplitItem Delete Command
        /// <summary>
        /// Handle the event from SplitItem's pencil item clicking
        /// </summary>
        /// <param name="spItem"></param>
        private void HandleSplitItemDeleteCommand(SplitItem spItem)
        {
            if (ItemDeleteCommand != null)
                ItemDeleteCommand.Execute(spItem);
        }

        public ICommand ItemDeleteCommand
        {
            get { return (ICommand)GetValue(ItemDeleteCommandProperty); }
            set { SetValue(ItemDeleteCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ItemDeleteCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemDeleteCommandProperty =
            DependencyProperty.Register("ItemDeleteCommand", typeof(ICommand), typeof(SplitListView));


        #endregion SplitItem Delete Command

        #region Find
        /// <summary>
        /// Find SplitItem by (cellCount, splitKey)
        /// </summary>
        /// <param name="cellCount"></param>
        /// <param name="splitKey"></param>
        /// <returns></returns>
        public SplitItem? FindSplitItem(int cellCount, char splitKey)
        {
            return vm.FindSplitCtrl(cellCount, splitKey);
        }

        /// <summary>
        /// Return the index of the first IsSlected=true item.
        /// </summary>
        /// <returns></returns>
        public int FindIndexOfSelectedItem()
        {
            int idx = 0;
            foreach (SplitItem spItem in vm.SplitList)
            {
                if (spItem.IsSelected)
                    return idx;
                idx++;
            }
            return -1;
        }

        public SplitItem? FindItemByFriendlyName(string firendlyName)
        {
            return vm.FindSplitCtrlByFriendlyName(firendlyName);
        }
        public SplitItem? FindItemByCustomId(long customId)
        {
            return vm.FindSplitItemByCustomId(customId);
        }

        public SplitItem? FindItemBySplitJson(SplitJson spj)
        {
            return vm.FindItemBySplitJson(spj);
        }
        public SplitItem? FindItemByEAID(int EAID)
        {
            return vm.FindItemByEAID(EAID);
        }

        public SplitItem? GetLatestItem()
        {
            return vm.GetLatestItem();
        }

        /// <summary>
        /// Return the first item which Buddy is null
        /// </summary>
        /// <returns></returns>
        public SplitItem? FindFirstNoBuddyItem()
        {
            return vm.FindFirstNoBuddyItem();
        }

        public SplitItem? FindItemByProfileId(int profileId)
        {
            return vm.FindItemByProfileId(profileId);
        }

        //public SplitItem? ReplaceByFriendlyName(string friendlyName, )
        //{
        //    int idx = 0;
        //    foreach (SplitItem spItem in vm.SplitList)
        //    {
        //        if (spItem.ISplitCtrl != null)
        //        {
        //            if (spItem.ISplitCtrl.FriendlyName.Equals(friendlyName))
        //            {
        //            }
        //        }
        //        if (spItem.IsSelected)
        //            return idx;
        //        idx++;
        //    }
        //}
        #endregion Find

        #region Recent List 
        public void MoveSelectedItemToSecondPosition()
        {
            int idxSelected = FindIndexOfSelectedItem();
            if (idxSelected <= 0)
                return;

            //Unbinding
            DataContext = null;
            //Move the new recent item [idx] to [2]
            vm.SplitList.Move(idxSelected, 1);
            //Restore binding
            DataContext = vm;

            //Move to first page
            vm.GotoFirstPage();
        }

        public SplitItem AddSplitCtrlTo2ndPosition(ISplitCtrl isp)
        {
            //Robert_Lin 2025-4-10, The new SplitItem will return to caller
            //SO itcannot be created inside an using
            //NEW:
            spItem = new SplitItem();
            //OLD:
            //using (SplitItem spItem = new SplitItem())
            {
                spItem.InnerContent = isp.UC;
                spItem.SplitOwner = vm.SplitOwner;
                spItem.ClickCommand = new RelayCommand<SplitItem>(HandleSplitItemClickCommand);
                spItem.EditClickCommand = new RelayCommand<SplitItem>(HandleSplitItemEditClickCommand);
                spItem.DeleteCommand = new RelayCommand<SplitItem>(HandleSplitItemDeleteCommand);

                if (vm.SplitOwner == eSplitOwner.EaCustom)
                {
                    spItem.IsDeleteEnabled = true;
                    spItem.IsEditEnabled = true;
                }
                else if (vm.SplitOwner == eSplitOwner.EaWin)
                {
                    spItem.IsEditEnabled = true;
                }

                if (spItem.ISplitCtrl != null)
                    spItem.ISplitCtrl.IsVertical = IsVertical;

                DataContext = null;
                vm.SplitList.Insert(1, spItem);
                DataContext = vm;
                vm.RefreshDisplayItems();
                return spItem;
            }            
        } 

        #endregion Recent List 

        #region Delete an item
        public bool DeleteSplitItem(SplitItem spItem)
        {
            DataContext = null;
            bool res = vm.SplitList.Remove(spItem);
            DataContext = vm;
            vm.RefreshDisplayItems();
            vm.RefreshPrevNextButtons();
            return res;
        }
        #endregion Delete an item

        #region Screen Orientation

        public bool IsVertical
        {
            get { return vm.IsVertical; }
            set 
            {
                vm.IsVertical = value; 
                foreach(SplitItem spItem in vm.SplitList)
                {
                    if (spItem.ISplitCtrl != null)
                        spItem.ISplitCtrl.IsVertical = vm.IsVertical;
                }
            }
        }

        #endregion Screen Orientation

        #region AddCustomLayoutButton
        public bool HasAddButton
        {
            get { return vm.HasAddButton; }
            set { vm.HasAddButton = value; }
        }



        public ICommand AddButtonClickCommand
        {
            get { return (ICommand)GetValue(AddButtonClickCommandProperty); }
            set { SetValue(AddButtonClickCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for AddButtonClickCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AddButtonClickCommandProperty =
            DependencyProperty.Register("AddButtonClickCommand", typeof(ICommand), typeof(SplitListView));

        private void HandleAddButtonClickCommand(AddCustomLayoutButton addButton)
        {
            if (AddButtonClickCommand != null)
                AddButtonClickCommand.Execute(this);
        }

        #endregion

        #region Custom List
        public void RefreshCustomEAID()
        {
            if (SplitOwner == eSplitOwner.EaCustom)
            {
                int idx = 0;
                foreach(SplitItem spIem in SplitList)
                {
                    if (spIem.ISplitCtrl != null)
                    {
                        spIem.ISplitCtrl.EAID = EAEMConstants.EAID_FirstCustom + idx;
                        idx++;
                    }
                }
            }
        }
        #endregion Custom List

    }
}