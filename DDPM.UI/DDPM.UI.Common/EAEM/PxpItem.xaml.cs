using System.Windows;
using System.Windows.Markup;

namespace DDPM.UI.Common.EAEM
{
    /// <summary>
    /// Interaction logic for PxpItem.xaml
    /// </summary>
    [ContentProperty("InnerContent")]
    public partial class PxpItem : System.Windows.Controls.UserControl
    {
        public PxpItem()
        {
            InitializeComponent();
        }

        public object InnerContent
        {
            get { return (object)GetValue(InnerContentProperty); }
            set { SetValue(InnerContentProperty, value); }
        }

        // Using a DependencyProperty as the backing store for InnerContent.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty InnerContentProperty =
            DependencyProperty.Register("InnerContent", typeof(object), typeof(PxpItem));
    }
}