using DDPM.UI.Common.Interfaces;
using DDPM.UI.Common.Models;
using Microsoft.Windows.Themes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
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
    /// Interaction logic for TabHeaders.xaml
    /// </summary>
    public partial class TabHeaders : UserControl
    {
        public TabHeaders()
        {
            InitializeComponent();
        }


        #region ItemsSource
        private int _itemCount = 0;

        public IEnumerable<ITabHeader> ItemsSource
        {
            get { return (IEnumerable<ITabHeader>)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register("ItemsSource", typeof(IEnumerable<ITabHeader>), typeof(TabHeaders),
                new PropertyMetadata(new PropertyChangedCallback(OnItemsSourcePropertyChanged)));

        private static void OnItemsSourcePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var control = sender as TabHeaders;
            if (control != null)
                control.OnItemsSourceChanged((IEnumerable<ITabHeader>)e.OldValue, (IEnumerable<ITabHeader>)e.NewValue);
        }

        private void OnItemsSourceChanged(IEnumerable<ITabHeader> oldValue, IEnumerable<ITabHeader> newValue)
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

            List<ITabHeader> list = newValue.OfType<ITabHeader>().ToList();
            _itemCount = list.Count;

            if (_itemCount >= 3)
            {
                Text3 = list[2].Text;
                Text2 = list[1].Text;
                Text1 = list[0].Text;

                col1.Width = new GridLength((double)1, GridUnitType.Star);
            }
            else if (_itemCount == 2)
            {
                Text3 = list[1].Text;
                Text1 = list[0].Text;

                col1.Width = new GridLength(0, GridUnitType.Pixel);
            }
            else //itemCount == 1
            {

            }
        }
        void newValueINotifyCollectionChanged_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            //Do your stuff here.
            //SetHeaders();
        }
        #endregion



        public string Text1
        {
            get { return (string)GetValue(Text1Property); }
            set { SetValue(Text1Property, value); }
        }

        // Using a DependencyProperty as the backing store for Text1.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty Text1Property =
            DependencyProperty.Register("Text1", typeof(string), typeof(TabHeaders), new PropertyMetadata(String.Empty));




        public string Text2
        {
            get { return (string)GetValue(Text2Property); }
            set { SetValue(Text2Property, value); }
        }

        // Using a DependencyProperty as the backing store for Text2.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty Text2Property =
            DependencyProperty.Register("Text2", typeof(string), typeof(TabHeaders), new PropertyMetadata(String.Empty));




        public string Text3
        {
            get { return (string)GetValue(Text3Property); }
            set { SetValue(Text3Property, value); }
        }

        // Using a DependencyProperty as the backing store for Text3.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty Text3Property =
            DependencyProperty.Register("Text3", typeof(string), typeof(TabHeaders), new PropertyMetadata(String.Empty));





        public int SelectedIndex
        {
            get { return (int)GetValue(SelectedIndexProperty); }
            set 
            {
                SetValue(SelectedIndexProperty, value); 
                if (_itemCount >= 3)
                {
                    HightlightHeaderIndex = SelectedIndex;
                }
                else if (_itemCount == 2)
                {
                    //SelectedIndex         : 0  1  2
                    //HightlightHeaderIndex : 0  2  x
                    if (SelectedIndex == 1)
                        HightlightHeaderIndex = 2;
                    else
                        HightlightHeaderIndex = SelectedIndex;
                }
            }
        }

        // Using a DependencyProperty as the backing store for SelectedIndex.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedIndexProperty =
            DependencyProperty.Register("SelectedIndex", typeof(int), typeof(TabHeaders), new PropertyMetadata(0));




        public int HightlightHeaderIndex
        {
            get { return (int)GetValue(HightlightHeaderIndexProperty); }
            set { SetValue(HightlightHeaderIndexProperty, value); }
        }

        // Using a DependencyProperty as the backing store for HightlightHeaderIndex.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HightlightHeaderIndexProperty =
            DependencyProperty.Register("HightlightHeaderIndex", typeof(int), typeof(TabHeaders), new PropertyMetadata(0));




        public ICommand? ClickCommand
        {
            get { return (ICommand?)GetValue(ClickCommandProperty); }
            set { SetValue(ClickCommandProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ClickCommand.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ClickCommandProperty =
            DependencyProperty.Register("ClickCommand", typeof(ICommand), typeof(TabHeaders));



        private void header1_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            SelectedIndex = 1;
            if (ClickCommand != null)
            {
                ClickCommand.Execute(this);
            }
        }

        private void header0_MouselLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            SelectedIndex = 0;
            if (ClickCommand != null)
            {
                ClickCommand.Execute(this);
            }
        }

        private void header2_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            SelectedIndex = 2;
            if (ClickCommand != null)
            {
                ClickCommand.Execute(this);
            }
        }
    }
}
