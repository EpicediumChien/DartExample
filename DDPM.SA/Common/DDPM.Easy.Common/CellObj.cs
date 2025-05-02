using System.CodeDom;
using System.Windows;
using System.Windows.Controls;

namespace DDPM.Easy.Common
{
    /// <summary>
    /// A taget arrange rectangle in Easy Arrange function.
    /// For exmaple, SplitCtrl2A have 2 cells, Left (cell_2a1) and Right (cell_2a2),
    /// each cell on UI (XAML) is represent with a CellObj in code behind.
    /// </summary>
    public class CellObj: IDisposable
    {
        #region Private members
        private bool _isDisposed = false;
        #endregion

        #region Native data members

        public string Name { get; set; } = ""; //Cell Name
        public Rect rc { get; set; } //Rect of the Cell, used by UI for detect hovering
        public Border bd { get; set; } //Attached to the UI Element (Border) -- Unused, use CellBd instead
        public Rect rcRatio { get; set; } //Rect if Border is in a (x,y)=(0,0)1x1 screen

        public CellBorder CellBd { get; set; }

        #endregion Native data members

        #region Ctor

        public CellObj(string name, Border border)
        {
            Name = name;
            bd = border;
            rc = new Rect();
        }

        //For SplitCtrl0B
        public CellObj(string name)
        {
            Name = name;
            rc = new Rect();
        }

        public CellObj(string name, CellBorder cellBd)
        {
            Name = name;
            CellBd = cellBd;
        }
        #endregion Ctor

        #region Dispose and Destructor
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    // ??????
                    //Most of CellBd is a reference to UI Element, so no need to dispose it.
                }

                // ???????
                //if (unmanagedResource != IntPtr.Zero)
                //{
                //    // ????
                //    unmanagedResource = IntPtr.Zero;
                //}

                _isDisposed = true;
            }
        }
        ~CellObj()
        {
            Dispose(false);
        }
        #endregion
    }
}