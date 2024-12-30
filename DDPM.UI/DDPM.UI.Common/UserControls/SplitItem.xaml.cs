using DDPM.Easy.Common;
using DDPM.SA.Common;
using DDPM.SA.Common.Display;
using DDPM.SA.Common.Settings;
using DDPM.UI.Common.EAEM;
using DDPM.UI.Common.Interfaces;
using DDPM.UI.Common.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
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

        #endregion Init

        #region Content, ISplit

        public object InnerContent
        {
            get { return (object)GetValue(InnerContentProperty); }
            set
            {
                SetValue(InnerContentProperty, value);
                if (value is ISplit)
                    vm.Split = (ISplit)value;
                if (value is ISplitCtrl)
                    vm.SplitCtrl = (ISplitCtrl)value;
            }
        }

        // Using a DependencyProperty as the backing store for InnerContent.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty InnerContentProperty =
            DependencyProperty.Register("InnerContent", typeof(object), typeof(SplitItem));

        public ISplit? ISplit => vm.Split;

        public ISplitCtrl? ISplitCtrl => vm.SplitCtrl;

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

        #endregion Content, ISplit

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

        #endregion SplitOwner

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

        #endregion IsSelected state

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

        #endregion Item ClickCommand

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
            e.Handled = true;
            if (EditClickCommand != null)
            {
                EditClickCommand.Execute(this);
            }
        }

        #endregion Edit Icon

        #region Del Icon

        public bool IsDeleteEnabled
        {
            get { return (bool)GetValue(IsDeleteEnabledProperty); }
            set { SetValue(IsDeleteEnabledProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsDeleteEnabled.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsDeleteEnabledProperty =
            DependencyProperty.Register("IsDeleteEnabled", typeof(bool), typeof(SplitItem), new PropertyMetadata(false));




        public ICommand DeleteCommand
        {
            get { return (ICommand)GetValue(DeleteCommandProperty); }
            set { SetValue(DeleteCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DeleteCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DeleteCommandProperty =
            DependencyProperty.Register("DeleteCommand", typeof(ICommand), typeof(SplitItem));

        private void closeXIcon_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            if (DeleteCommand != null)
            {
                DeleteCommand.Execute(this);
            }
        }

        #endregion Del Icon

        #region For Easy Arrange

        public long CustomId
        {
            get => vm.CustomId;
            set { vm.CustomId = value; }
        }

        public SplitItem? Buddy { get; set; } = null;

        public int CellCount
        {
            get
            {
                if (ISplitCtrl != null)
                {
                    return ISplitCtrl.CellCount;
                }
                return 0;
            }
        }
        public char SplitKey
        {
            get
            {
                if (ISplitCtrl != null)
                {
                    return ISplitCtrl.SplitKey;
                }
                return 'A';
            }
        }
        public List<double> Settings
        {
            get
            {
                if (ISplitCtrl != null)
                {
                    return ISplitCtrl.Settings;
                }
                return new List<double>();
            }
        }
        public string CustomName
        {
            get
            {
                if (ISplitCtrl != null)
                {
                    return ISplitCtrl.FriendlyName;
                }
                return "";
            }
        }

        public SplitJson ToSplitJson
        {
            get
            {
                int eaid = 0;
                List<CellJson> cells = new List<CellJson>();
                if (ISplitCtrl != null)
                {
                    eaid = ISplitCtrl.EAID;
                    //if ((CellCount == 0) && (SplitKey == 'B'))
                    //{
                    //    foreach (CellBorder cellBorder in ISplitCtrl.CellBorders)
                    //    {
                    //        CellJson cj = new CellJson();
                    //        cj.Name = cellBorder.CellName;
                    //        cj.x = cellBorder.rcRatio.Left;
                    //        cj.y = cellBorder.rcRatio.Top;
                    //        cj.w = cellBorder.rcRatio.Width;
                    //        cj.h = cellBorder.rcRatio.Height;
                    //        cells.Add(cj);
                    //    }
                    //}
                    //else
                    //{
                        foreach (CellObj objCell in ISplitCtrl.CellList)
                        {
                            CellJson cj = new CellJson();
                            cj.Name = objCell.Name;
                            cj.x = objCell.rcRatio.Left;
                            cj.y = objCell.rcRatio.Top;
                            cj.w = objCell.rcRatio.Width;
                            cj.h = objCell.rcRatio.Height;
                            cells.Add(cj);
                        }
                    //}
                }
                return new SA.Common.Display.SplitJson()
                {
                    CellCount = CellCount,
                    SplitKey = SplitKey,
                    Settings = Settings,
                    CustomId = CustomId,
                    CustomName = CustomName,
                    EAID = eaid,
                    Cells = cells.ToArray()
                };
            }
        }

        /// <summary>
        /// Compare with other, return true if they are same layout in EasyArrange.
        /// 1 Compare CustomId, if different return false;
        /// 2 Compare (CellCount,SplitKey)
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool IsEquals(SplitItem other)
        {
            //Support EasyArrange content only
            if (ISplitCtrl == null)
                return true;
            if (other.ISplitCtrl == null)
                return true;

            //1 Compare CustomId (0=predefined layout; others=custom layout)
            //  custom layout is d=identified with CustomId
            if (CustomId != other.CustomId)
                return false;
            //Can be a.Both are Predefined layout (CustomId=0) => need to compare with (CellCount,SplitKey)
            //    or b.Both are custom layout and are the same layout
            if (CustomId == 0)
                return (CellCount == other.CellCount) && (SplitKey == other.SplitKey);
            return true;
        }
        #endregion For Easy Arrange

        #region Replace
        public void ReplaceByEAArgs(EAArgs args)
        {
            //Check if it need to change ISplitCtrl
            if ((CellCount != args.SplitJson.CellCount) || (SplitKey !=  args.SplitJson.SplitKey))
            {
                if ((args.SplitJson.CellCount == 0) && (args.SplitJson.SplitKey == 'B'))
                {
                    ISplitCtrl ispNew = new SplitCtrl0B();
                    InnerContent = ispNew.UC;
                }
                else
                {
                    ISplitCtrl? ispNew = ISplitCtrl.Create(args.SplitJson.CellCount, args.SplitJson.SplitKey);
                    if (ispNew == null)
                        return;
                    InnerContent = ispNew.UC;
                }
            }
            if (ISplitCtrl == null)
                return;

            //Copy data
            ISplitCtrl.Settings = new List<double>(args.SplitJson.Settings);
            ISplitCtrl.FriendlyName = args.SplitJson.CustomName;
            ISplitCtrl.EAID = args.SplitJson.EAID;
            //CustomId = args.CustomId;

            vm.NotifyPropertyChanged_TooltipText();
        }
        public void ReplaceWithISplitICtrl(ISplitCtrl ispSource)
        {
            //Copy the another's ISplitCtrl
            ISplitCtrl? isp = ispSource.Clone();
            //Put into /replace with SplitItem's Content
            InnerContent = isp.UC;

            vm.NotifyPropertyChanged_TooltipText();
        }
        #endregion

        #region Add Custom Layout Button
        public bool IsHoverable
        {
            get { return vm.IsHoverable; }
            set { vm.IsHoverable = value; }
        }
        //Robert_Lin, 2024-11-10, to be removed, please use IsOverlapCustomLayout instead
        public bool IsAddedCustomLayout
        {
            get 
            {
                if (ISplitCtrl != null)
                    return ISplitCtrl.IsAddedCustomLayout;
                return false;
            }
        }
        public bool IsOverlapCustomLayout
        {
            get
            {
                if (ISplitCtrl != null)
                    return ISplitCtrl.IsOverlapCustomLayout;
                return false;
            }
        }
        #endregion Add Custom Layout Button

        #region For EzMemory

        private int _profileID;
        public int ProfileID
        {
            get => _profileID;
            set
            {
                _profileID = value;
            }
        }
        private int _layoutID;
        public int LayoutID
        {
            get => _layoutID;
            set
            {
                _layoutID = value;
            }
        }
        #endregion For EzMemory
    }

}