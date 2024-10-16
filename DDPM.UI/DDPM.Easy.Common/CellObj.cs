using System.Windows;
using System.Windows.Controls;

namespace DDPM.Easy.Common
{
    /// <summary>
    /// A taget arrange rectangle in Easy Arrange function.
    /// For exmaple, SplitCtrl2A have 2 cells, Left (cell_2a1) and Right (cell_2a2),
    /// each cell on UI (XAML) is represent with a CellObj in code behind.
    /// </summary>
    public class CellObj
    {
        #region Native data members

        public string Name { get; set; } = ""; //Cell Name
        public Rect rc { get; set; } //Rect of the Cell, used by UI for detect hovering
        public Border bd { get; set; } //Attached to the UI Element (Border)
        public Rect rcRatio { get; set; } //Rect if Border is in a (x,y)=(0,0)1x1 screen

        #endregion Native data members

        #region Ctor

        public CellObj(string name, Border border)
        {
            Name = name;
            bd = border;
            rc = new Rect();
        }

        #endregion Ctor
    }
}