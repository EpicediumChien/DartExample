using CommunityToolkit.Mvvm.ComponentModel;
using DDPM.Easy.Common;

namespace DDPM.SA.Plugins.User.EasyArrange
{
    public class ArrangeVM : ObservableObject
    {
        #region Enabled flag

        private bool _isFunctionEnabled = true;

        /// <summary>
        /// Eanble/Disable EasyArrange functions, including Edit, Work,
        /// </summary>
        public bool IsFunctionEnabled
        {
            get => _isFunctionEnabled;
            set => SetProperty(ref _isFunctionEnabled, value);
        }

        #endregion Enabled flag

        #region Option flags

        private bool _isWorkUIEnabled = true;
        private bool _isMoving = false;

        public bool IsWorkUIShowing
        {
            get
            {
                return _isWorkUIEnabled && IsMoving;
            }
        }

        /// <summary>
        /// Manually enable/disable WorkWindow function.
        /// For example, when user is editing custom layout (EditWindow is working),
        /// Set this function to false, so user can move widows and not been arranged.
        /// </summary>
        public bool IsWorkUIEnabled
        {
            get => IsWorkUIEnabled;
            set
            {
                SetProperty(ref _isWorkUIEnabled, value);
                OnPropertyChanged("IsWorkUIShowing");
            }
        }

        /// <summary>
        /// The key flag to show UI, and let the moving (OnLocationChanged handler) to continue
        /// </summary>
        public bool IsMoving
        {
            get => _isMoving;
            set
            {
                SetProperty(ref _isMoving, value);
                OnPropertyChanged("IsWorkUIShowing");
            }
        }

        #endregion Option flags

        #region Cursor position

        /// <summary>
        /// Cursor position (xCursor, yCursor) will be updated by (OnLocationChanged handler).
        /// and then use it to determine if the custor is inside a CellBorder.
        /// </summary>
        //xCursor
        private int _xCursor = 0;

        public int xCursor
        {
            get { return _xCursor; }
            set
            {
                _xCursor = value;
                OnPropertyChanged("xCursor");
            }
        }

        //yCursor
        private int _yCursor = 0;

        public int yCursor
        {
            get { return _yCursor; }
            set
            {
                _yCursor = value;
                OnPropertyChanged("yCursor");
            }
        }

        #endregion Cursor position

        #region Screen Scale

        private double _screenScale = 1.00;

        /// <summary>
        /// Update the screen scale when start moving, will be used to fix the coordinates later
        /// </summary>

        public double ScreenScale
        {
            get => _screenScale;
            set => SetProperty(ref _screenScale, value);
        }

        #endregion Screen Scale

        #region Hovering Cell

        private CellObj? _hoveringCellObj = null;

        /// <summary>
        /// This property will be refreshed by OnLocationChanged handler.
        /// When moving stop, will use it as the target Cell to move the target window into this cell rect.
        /// </summary>
        public CellObj? HoveringCellObj
        {
            get => _hoveringCellObj;
            set => SetProperty(ref _hoveringCellObj, value);
        }

        private string _hoveringCell = "";

        /// <summary>
        /// Property which is used from DataBinding by SplitCtrls
        /// </summary>
        public string HoveringCell
        {
            get { return _hoveringCell; }
            set => SetProperty(ref _hoveringCell, value);
        }

        #endregion Hovering Cell
    }
}