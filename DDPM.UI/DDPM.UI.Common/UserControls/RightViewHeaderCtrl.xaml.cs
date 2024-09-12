using DDPM.SA.Common.Settings;
using DDPM.UI.Common.Models;
using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
using UserControl = System.Windows.Controls.UserControl;

namespace DDPM.UI.Common
{
    /// <summary>
    /// Interaction logic for RightViewHeaderCtrl.xaml
    /// </summary>
    public partial class RightViewHeaderCtrl : UserControl
    {
        private RightViewHeaderCtrlViewModel vm = new RightViewHeaderCtrlViewModel();

        public RightViewHeaderCtrl()
        {
            InitializeComponent();
            DataContext = vm;

            if (DdpmCommonHelper.DeviceManagerSA != null)
            {
                DdpmCommonHelper.DeviceManagerSA.ITSettingsActionEvent += DeviceManagerSA_ITSettingsActionEvent;
                DDPMSettings data = DdpmCommonHelper.DeviceManagerSA.ReloadAppConfigData().Result;
                Dispatcher.Invoke(new Action(() =>
                {
                    RightViewHeaderCtrlViewModel vm = (RightViewHeaderCtrlViewModel)this.DataContext;
                    if (vm != null)
                    {
                        if(vm.Text1.Equals(Strings.RightViewHeader_BrightnessContrast))
                        {
                            vm.Locker1 = data.LockSettings.Lock_Settings_TelemetryConsent ? Visibility.Visible : Visibility.Collapsed;
                            Trace.WriteLine($"[SettingsPage] Apply Brightness (lock) : {data.LockSettings.Lock_Settings_TelemetryConsent}");
                        }
                    }
                }));
            }
        }

        ~RightViewHeaderCtrl()
        {
            if (DdpmCommonHelper.DeviceManagerSA != null)
            {
                DdpmCommonHelper.DeviceManagerSA.ITSettingsActionEvent -= DeviceManagerSA_ITSettingsActionEvent;
            }
        }

        private void DeviceManagerSA_ITSettingsActionEvent(object? sender, SA.Common.ITSettingEventArgs e)
        {
            if (e == null || e.IT_Feature_TriggerList == null || e.target_object == null)
            {
                Trace.WriteLine("Got [SettingsPage][DeviceManagerSA_ITSettingsActionEvent] event but its argument is empty!");
                return;
            }
            int idx = e.IT_Feature_TriggerList.FindIndex(x => x.Trim().Equals("Lock_Settings_TelemetryConsent"));
            if (idx >= 0)
            {
                string feature = e.IT_Feature_TriggerList[idx];
                PropertyInfo propertyInfo = e.target_object.GetType().GetProperty(feature);
                Trace.WriteLine($"Got [SettingsPage][IT settings event] {feature} : {propertyInfo.GetValue(e.target_object)}");
                Dispatcher.Invoke(new Action(() =>
                {
                    RightViewHeaderCtrlViewModel vm = (RightViewHeaderCtrlViewModel)this.DataContext;
                    if (vm != null)
                    {
                        if (vm.Text1.Equals(Strings.RightViewHeader_BrightnessContrast))
                        {
                            vm.Locker1 = (bool)propertyInfo.GetValue(e.target_object) ? Visibility.Visible : Visibility.Collapsed;
                            Trace.WriteLine($"[SettingsPage] Apply TelemetryConsent(Lock) : {propertyInfo.GetValue(e.target_object)}");
                        }
                    }
                }));
            }
        }

        #region Items

        public void SetHeaders(RightViewHeader[] headers)
        {
            vm.SetHeaders(headers);
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
            DependencyProperty.Register("ItemsSource", typeof(IEnumerable), typeof(RightViewHeaderCtrl),
                 new PropertyMetadata(new PropertyChangedCallback(OnItemsSourcePropertyChanged)));

        private static void OnItemsSourcePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var control = sender as RightViewHeaderCtrl;
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
            }
        }

        // Using a DependencyProperty as the backing store for SelectedIndex.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedIndexProperty =
            DependencyProperty.Register("SelectedIndex", typeof(int), typeof(RightViewHeaderCtrl), new PropertyMetadata(0));

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
    }
}