using CommunityToolkit.Mvvm.Input;
using DDPM.UI.Common.EAEM;
using DDPM.UI.Common.Interfaces;
using DDPM.UI.Common.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using UserControl = System.Windows.Controls.UserControl;

namespace DDPM.UI.Common.UserControls
{
    /// <summary>
    /// Interaction logic for SplitListView.xaml
    /// </summary>
    public partial class SplitListView : UserControl
    {
        private SplitListViewModel vm = new SplitListViewModel();
        private ICommand? _splitItemClickCommand;

        public SplitListView()
        {
            InitializeComponent();
            DataContext = vm;
        }

        //Owner must assign this value before calling to AddSplitToList()
        public eSplitOwner SplitOwner
        {
            get => vm.SplitOwner;
            set => vm.SplitOwner = value;
        }

        public SplitItem AddSplitToList(ISplit split)
        {
            SplitItem spItem = new SplitItem();
            spItem.InnerContent = split;
            spItem.SplitOwner = vm.SplitOwner;
            spItem.ClickCommand = new RelayCommand<SplitItem>(HandleSplitItemClickCommand);
            vm.AddSplitItemToList(spItem);
            return spItem;
        }

        public SplitItem AddItemToList(ContentControl contentControl) 
        {
            SplitItem spItem = new SplitItem();
            spItem.InnerContent = contentControl;
            spItem.SplitOwner = vm.SplitOwner;
            spItem.ClickCommand = new RelayCommand<SplitItem>(HandleSplitItemClickCommand);
            spItem.EditClickCommand = new RelayCommand<SplitItem>(HandleSplitItemEditClickCommand);
            vm.AddSplitItemToList(spItem);
            return spItem;
        }

        public void ClearList()
        {
            vm.ClearList();
        }

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

         #endregion

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
        #endregion

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



        #endregion
    }
}
