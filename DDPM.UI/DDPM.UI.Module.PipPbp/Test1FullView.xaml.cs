using System.Windows.Controls;
using System.Windows.Input;

namespace DDPM.UI.Module.PipPbp
{
    /// <summary>
    /// Interaction logic for Test1FullView.xaml
    /// </summary>
    public partial class Test1FullView : UserControl
    {
        public Test1FullView()
        {
            InitializeComponent();
        }

        public ICommand? closeFullViewClick { get; set; }
        public ICommand? gotoNextClick { get; set; }
    }
}