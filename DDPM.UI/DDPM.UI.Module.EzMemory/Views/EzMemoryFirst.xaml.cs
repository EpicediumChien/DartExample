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

namespace DDPM.UI.Module.EzMemory.Views
{
    /// <summary>
    /// EzMemoryFirst.xaml 的互動邏輯
    /// </summary>
    public partial class EzMemoryFirst : UserControl
    {
        private EzMemoryViewModel vm
        {
            get
            {
                return (EzMemoryViewModel)DataContext;
            }
        }
        public EzMemoryFirst()
        {
            InitializeComponent();
        }

        private void ArrowButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void NextBtn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void SkipBtn_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
