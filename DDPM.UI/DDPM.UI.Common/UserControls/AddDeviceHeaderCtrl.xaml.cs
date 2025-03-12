using DDPM.SA.Common;
using DDPM.UI.Common.Models;
using Dell.Client.Framework.Common;
using Dell.Client.Framework.UX.WPF.Controls;
using System;
using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
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
            Canvas1.Visibility = headers.Length == 2 ? Visibility.Collapsed : Visibility.Visible;
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
                //SelectionChanged!(this, new RoutedEventArgs());
                if (value >= 0)
                    ((Border)FindName($"header{value}Border")).Background = (SolidColorBrush)FindResource("SubNav_Selected_BkBrush_Default");
            }
        }

        // Using a DependencyProperty as the backing store for SelectedIndex.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedIndexProperty =
            DependencyProperty.Register("SelectedIndex", typeof(int), typeof(AddDeviceHeaderCtrl), new PropertyMetadata(0));

        private void header0_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (vm.SelectedIndex == 0)
                return;

            header0Border.Background = (SolidColorBrush)FindResource("SubNav_Selected_BkBrush_Hover");
            header1Border.Background = new SolidColorBrush(Colors.Transparent);
            header2Border.Background = new SolidColorBrush(Colors.Transparent);
            bool isSelectionChanged = (vm.InternalSelectedIndex != 0);
            vm.InternalSelectedIndex = 0;
            if (isSelectionChanged && (SelectionChanged != null))
                SelectionChanged(this, new RoutedEventArgs());

        }

        private void header1_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (vm.SelectedIndex == 1)
                return;

            header0Border.Background = new SolidColorBrush(Colors.Transparent);
            header1Border.Background = (SolidColorBrush)FindResource("SubNav_Selected_BkBrush_Hover");
            header2Border.Background = new SolidColorBrush(Colors.Transparent);
            bool isSelectionChanged = (vm.InternalSelectedIndex != 1);
            vm.InternalSelectedIndex = 1;
            if (isSelectionChanged && (SelectionChanged != null))
                SelectionChanged(this, new RoutedEventArgs());
        }

        private void header2_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (vm.SelectedIndex == 2)
                return;

            header0Border.Background = new SolidColorBrush(Colors.Transparent);
            header1Border.Background = new SolidColorBrush(Colors.Transparent);
            header2Border.Background = (SolidColorBrush)FindResource("SubNav_Selected_BkBrush_Hover");
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

        private void header0_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                bool isSelectionChanged = (vm.InternalSelectedIndex != 0);
                vm.InternalSelectedIndex = 0;
                if (isSelectionChanged && (SelectionChanged != null))
                    SelectionChanged(this, new RoutedEventArgs());
            }
        }

        private void header1_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                bool isSelectionChanged = (vm.InternalSelectedIndex != 1);
                vm.InternalSelectedIndex = 1;
                if (isSelectionChanged && (SelectionChanged != null))
                    SelectionChanged(this, new RoutedEventArgs());
            }
        }

        private void header2_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                bool isSelectionChanged = (vm.InternalSelectedIndex != 2);
                vm.InternalSelectedIndex = 2;
                if (isSelectionChanged && (SelectionChanged != null))
                    SelectionChanged(this, new RoutedEventArgs());
            }
        }

        private void HeaderBorder_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (sender is Border hd)
            {
                if (int.TryParse(hd.Tag.ToString(), out int index))
                {
                    Border bd = (Border)FindName($"header{index}Border");
                    if (bd == null)
                        return;

                    if (index == vm.SelectedIndex)
                        bd.Background = (SolidColorBrush)FindResource("SubNav_Selected_BkBrush_Hover");
                    else
                        bd.Background = DdpmCommonHelper.isDarkMode() ? (SolidColorBrush)FindResource("SubNav_BkBrush_Hover_Dark") : (SolidColorBrush)FindResource("SubNav_BkBrush_Hover_Light");

                    vm.HoverSelectedIndex = index;
                }
            }
        }

        private void HeaderBorder_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (sender is Border hd)
            {
                if (int.TryParse(hd.Tag.ToString(), out int index))
                {
                    vm.HoverSelectedIndex = -1;
                    if (index == vm.SelectedIndex)
                        ((Border)FindName($"header{index}Border")).Background = (SolidColorBrush)FindResource("SubNav_Selected_BkBrush_Default");
                    else
                        ((Border)FindName($"header{index}Border")).Background = new SolidColorBrush(Colors.Transparent);
                }
            }
        }
    }
}