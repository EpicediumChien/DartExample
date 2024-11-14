using DDPM.UI.Common.Models;
using Dell.Client.Framework.Common;
using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using UserControl = System.Windows.Controls.UserControl;

namespace DDPM.UI.Common
{
    /// <summary>
    /// Interaction logic for AddDeviceHeaderCtrl.xaml
    /// </summary>
    public partial class AddDeviceHeaderCtrl : UserControl
    {
        private RightViewHeaderCtrlViewModel vm = new();
        //private ILog _log;

        public AddDeviceHeaderCtrl()
        {
            InitializeComponent();
            DataContext = vm;

            //if (DdpmCommonHelper.MyConsole != null)
            //{
                //_log = DdpmCommonHelper.MyConsole.CreateLog("AddDeviceHeaderCtrl");
                //_log.Info("AddDeviceHeaderCtrl ctor");

                //_log.Info($"header1Text.Width = {header1Text.ActualWidth}");
                //_log.Info($"header1SP.Width = {header1SP.ActualWidth}");
                //_log.Info($"header1Border.Width = {header1Border.ActualWidth}");
            //}
        }

        #region Items

        public void SetHeaders(RightViewHeader[] headers)
        {
            vm.SetHeaders(headers);
            if (headers.Length > 1)
            {
                Icon1.Visibility = Visibility.Collapsed;
                if (headers[0].ImageFile != "")
                {
                    Icon1.Source = DdpmCommonHelper.GetImageSourceFromCommonResource($"Resources/Images/{headers[0].ImageFile}", "DDPM.UI.Resources");
                    Icon1.Visibility = Visibility.Visible;
                }
                Icon2.Visibility = Visibility.Collapsed;
                if (headers[1].ImageFile != "")
                {
                    Icon2.Source = DdpmCommonHelper.GetImageSourceFromCommonResource($"Resources/Images/{headers[1].ImageFile}", "DDPM.UI.Resources");
                    Icon2.Visibility = Visibility.Visible;
                }
            }
            Icon3.Visibility = Visibility.Collapsed;
            if (headers.Length > 2 && headers[2].ImageFile != "")
            {
                Icon3.Source = DdpmCommonHelper.GetImageSourceFromCommonResource($"Resources/Images/{headers[2].ImageFile}", "DDPM.UI.Resources");
                Icon3.Visibility = Visibility.Visible;
            }
        }

        public IEnumerable ItemsSource
        {
            get { return (IEnumerable)GetValue(ItemsSourceProperty); }
            set
            {
                SetValue(ItemsSourceProperty, value);

                RightViewHeader[] headers = (RightViewHeader[])value;
                SetHeaders(headers);
            }
        }

        // Using a DependencyProperty as the backing store for ItemsSource.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register("ItemsSource", typeof(IEnumerable), typeof(AddDeviceHeaderCtrl),
                 new PropertyMetadata(new PropertyChangedCallback(OnItemsSourcePropertyChanged)));

        private static void OnItemsSourcePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var control = sender as AddDeviceHeaderCtrl;
            if (control != null)
                control.OnItemsSourceChanged((IEnumerable)e.OldValue, (IEnumerable)e.NewValue);
        }

        private void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
        {
            // Remove handler for oldValue.CollectionChanged
            var oldValueINotifyCollectionChanged = oldValue as INotifyCollectionChanged;

            if (null != oldValueINotifyCollectionChanged)
            {
                oldValueINotifyCollectionChanged.CollectionChanged -= new NotifyCollectionChangedEventHandler(newValueINotifyCollectionChanged_CollectionChanged);
            }
            // Add handler for newValue.CollectionChanged (if possible)
            var newValueINotifyCollectionChanged = newValue as INotifyCollectionChanged;
            if (null != newValueINotifyCollectionChanged)
            {
                newValueINotifyCollectionChanged.CollectionChanged += new NotifyCollectionChangedEventHandler(newValueINotifyCollectionChanged_CollectionChanged);
            }
        }

        private void newValueINotifyCollectionChanged_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            //Do your stuff here.
            //SetHeaders();
        }

        #endregion Items

        #region Selection Changed

        public event RoutedEventHandler? SelectionChanged;

        public int SelectedIndex
        {
            get
            {
                return vm.SelectedIndex;
                //return (int)GetValue(SelectedIndexProperty);
            }
            set
            {
                vm.SelectedIndex = value;
                SetValue(SelectedIndexProperty, vm.SelectedIndex);
                SelectionChanged!(this, new RoutedEventArgs());
            }
        }

        // Using a DependencyProperty as the backing store for SelectedIndex.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedIndexProperty =
            DependencyProperty.Register("SelectedIndex", typeof(int), typeof(AddDeviceHeaderCtrl), new PropertyMetadata(0));

        private void header0_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            bool isSelectionChanged = (vm.InternalSelectedIndex != 0);
            vm.InternalSelectedIndex = 0;
            if (isSelectionChanged && (SelectionChanged != null))
                SelectionChanged(this, new RoutedEventArgs());
        }

        private void header1_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            bool isSelectionChanged = (vm.InternalSelectedIndex != 1);
            vm.InternalSelectedIndex = 1;
            if (isSelectionChanged && (SelectionChanged != null))
                SelectionChanged(this, new RoutedEventArgs());
        }

        private void header2_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            bool isSelectionChanged = (vm.InternalSelectedIndex != 2);
            vm.InternalSelectedIndex = 2;
            if (isSelectionChanged && (SelectionChanged != null))
                SelectionChanged(this, new RoutedEventArgs());
        }

        #endregion Selection Changed

        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //_log.Info($"header1Text.Width = {header1Text.ActualWidth}");
            //_log.Info($"header1SP.Width = {header1SP.ActualWidth}");
            //_log.Info($"header1Border.Width = {header1Border.ActualWidth}");

            //remove due to main window's min width change to 1050
            //if (header1Text.ActualWidth <= 150)
            //    header1Text.Width = header1Border.ActualWidth - 40;
        }
    }
}