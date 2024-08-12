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
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using UserControl = System.Windows.Controls.UserControl;

namespace DDPM.UI.Common.UserControls
{
    //Reference:
    // https://stackoverflow.com/questions/36446440/how-to-use-a-contentpresenter-inside-a-usercontrol
    /// <summary>
    /// Interaction logic for SplitItem.xaml
    /// </summary>
    [ContentProperty("InnerContent")]
    public partial class SplitItem : UserControl
    {
        private SplitItemViewModel vm = new SplitItemViewModel();

        #region Init
        public SplitItem()
        {
            InitializeComponent();
            DataContext = vm;
        }
        #endregion

        #region  Content, ISplit
        public object InnerContent
        {
            get { return (object)GetValue(InnerContentProperty); }
            set 
            {
                SetValue(InnerContentProperty, value); 
                if (value is ISplit)
                    vm.Split = (ISplit)value;
            }
        }

        // Using a DependencyProperty as the backing store for InnerContent.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty InnerContentProperty =
            DependencyProperty.Register("InnerContent", typeof(object), typeof(SplitItem));

        public ISplit? ISplit => vm.Split;

        //public Type ISplitType
        //{
        //    //Robert_Lin, 2024-6-26, Fix SAST issue: [Bug] Add a way to break out of this property accessor's recursion
        //    //OLD Code:
        //    /*
        //    get
        //    {
        //        if (ISplitType != null)
        //            return ISplit?.CtrlType;
        //        return typeof(object);
        //    }
        //    */
        //    //NEW Code:
        //    get
        //    {
        //        if (ISplit != null)
        //            return ISplit.CtrlType;
        //        else
        //            return typeof(object);

        //    }
        //}
        #endregion

        #region SplitOwner
        public eSplitOwner SplitOwner
        {
            get
            {
                if (vm != null)
                    return vm.SplitOwner;
                else
                    return eSplitOwner.None;
            }
            set
            {
                vm.SplitOwner = value;
            }
        }
        #endregion

        #region IsSelected state
        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsSelected.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register("IsSelected", typeof(bool), typeof(SplitItem), new PropertyMetadata(false, OnIsSelectedChanged));

        private static void OnIsSelectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (SplitItem)d;
        }
        #endregion

        #region Item ClickCommand
        public ICommand ClickCommand
        {
            get { return (ICommand)GetValue(ClickCommandProperty); }
            set { SetValue(ClickCommandProperty, value); }
        }

        //ContentControl? ISplit.Content => this;

        // Using a DependencyProperty as the backing store for ClickCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ClickCommandProperty =
            DependencyProperty.Register("ClickCommand", typeof(ICommand), typeof(SplitItem));

        private void spItem_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            //if (ClickCommand != null)
            //{
            //    ClickCommand.Execute(this);
            //}
        }

        private void rootGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            if (ClickCommand != null)
                ClickCommand?.Execute(this);
        }
        #endregion

        #region Edit Icon
        public bool IsEditEnabled
        {
            get 
            {
                return (bool)GetValue(IsEditEnabledProperty); 
            }
            set 
            {
                SetValue(IsEditEnabledProperty, value); 
                //vm.IsEditEnabled = value;
            }
        }

        // Using a DependencyProperty as the backing store for IsEditEnabled.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsEditEnabledProperty =
            DependencyProperty.Register("IsEditEnabled", typeof(bool), typeof(SplitItem), new PropertyMetadata(false));



        public ICommand EditClickCommand
        {
            get { return (ICommand)GetValue(EditClickCommandProperty); }
            set { SetValue(EditClickCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for EditClickCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty EditClickCommandProperty =
            DependencyProperty.Register("EditClickCommand", typeof(ICommand), typeof(SplitItem));


        private void pencilIcon_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (EditClickCommand != null)
            {
                e.Handled = true;
                EditClickCommand.Execute(this);
            }
        }

        #endregion

        #region Del Icon


        public bool IsDeleteEnabled
        {
            get { return (bool)GetValue(IsDeleteEnabledProperty); }
            set { SetValue(IsDeleteEnabledProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsDeleteEnabled.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsDeleteEnabledProperty =
            DependencyProperty.Register("IsDeleteEnabled", typeof(bool), typeof(SplitItem), new PropertyMetadata(false));


        #endregion

        #region For Easy Arrange
        public int CustomId;

        #endregion
    }
}
