using CommunityToolkit.Mvvm.ComponentModel;
using DDPM.UI.Common.Models;
using System.Windows;

namespace DDPM.UI.Common
{
    internal class RightViewHeaderCtrlViewModel : ObservableObject
    {
        private string _text1 = "";
        private string _text2 = "";
        private string _text3 = "";

        private int _externalIndex1 = -1;
        private int _externalIndex2 = -1;
        private int _externalIndex3 = -1;

        private int _itemCount = 0;
        private int _shownCount = 0;

        public string Text1
        {
            get => _text1;
            set => SetProperty(ref _text1, value);
        }

        public string Text2
        {
            get => _text2;
            set => SetProperty(ref _text2, value);
        }

        public string Text3
        {
            get => _text3;
            set => SetProperty(ref _text3, value);
        }

        public int ItemCount
        {
            get => _itemCount;
            private set => SetProperty(ref _itemCount, value);
        }

        #region Init

        public RightViewHeaderCtrlViewModel()
        {
        }

        public void SetHeaders(RightViewHeader[] headers)
        {
            _itemCount = headers.Length;
            if (_itemCount >= 3) _itemCount = 3;
            if (_itemCount <= 1) _itemCount = 0;

            _shownCount = 0;
            for (int i = 0; i < _itemCount; i++)
            {
                if (headers[i].IsShown)
                    _shownCount++;
            }

            if (_itemCount == 3)
            {
                if (_shownCount == 3) //Case#1
                {
                    //ColumnDefinitions[1]="1*"
                    Col1Width = new GridLength((double)1, GridUnitType.Star);
                    CtrlVisibility = Visibility.Visible;

                    Text1 = headers[0].Text;
                    Text2 = headers[1].Text;
                    Text3 = headers[2].Text;
                    _externalIndex1 = 0;
                    _externalIndex2 = 1;
                    _externalIndex3 = 2;
                    return;
                }
                if (_shownCount == 2)
                {
                    //_itemCount = 2;

                    //ColumnDefinitions[1]="0"
                    Col1Width = new GridLength(0, GridUnitType.Pixel);
                    CtrlVisibility = Visibility.Visible;

                    if (headers[0].IsShown) //Show A at Col[0]
                    {
                        Text1 = headers[0].Text;
                        _externalIndex1 = 0;

                        if (headers[1].IsShown) //Case#2
                        {
                            Text3 = headers[1].Text;
                            _externalIndex3 = 1;
                            _externalIndex2 = -1;
                            return;
                        }
                        //Else is Case#3
                        Text3 = headers[2].Text;
                        _externalIndex3 = 2;
                        _externalIndex2 = -1;
                        return;
                    }
                    //Else is Case#4, Both headers[1] and headers[2] IsShown must be true
                    Text1 = headers[1].Text;
                    _externalIndex1 = 1;
                    Text3 = headers[2].Text;
                    _externalIndex3 = 2;
                    _externalIndex1 = -1;
                    return;
                }
                //Else _showCount<=1 => Hide the RightViewHeader
                _itemCount = 0;
                CtrlVisibility = Visibility.Collapsed;
                return;
            } //if (_itemCount == 3)

            if (_itemCount == 2)
            {
                if (_shownCount == 2) //Case#8
                {
                    //ColumnDefinitions[1]="0"
                    Col1Width = new GridLength(0, GridUnitType.Pixel);
                    CtrlVisibility = Visibility.Visible;

                    Text1 = headers[0].Text;
                    _externalIndex1 = 0;
                    Text3 = headers[1].Text;
                    _externalIndex3 = 1;
                    return;
                }
                //Else _showCount<=1 => Hide the RightViewHeader
                //_itemCount = 0;
                //CtrlVisibility = Visibility.Collapsed;
                //return;
            }
            //Else _showCount<=1 => Hide the RightViewHeader
            _itemCount = 0;
            CtrlVisibility = Visibility.Collapsed;
        }

        #endregion Init

        #region Columnn1 Width

        /// <summary>
        /// The ColumnDefinition.Width of column 1, default is "1*"
        /// </summary>
        private GridLength _col1Width = new GridLength(1, GridUnitType.Star);

        public GridLength Col1Width
        {
            get => _col1Width;
            set => SetProperty(ref _col1Width, value);
        }

        #endregion Columnn1 Width

        #region CtrlVisibility

        private Visibility _ctrlVisibility = Visibility.Visible;

        public Visibility CtrlVisibility
        {
            get => _ctrlVisibility;
            set => SetProperty(ref _ctrlVisibility, value);
        }

        #endregion CtrlVisibility

        #region Selection

        private int _internalSelectedIndex = 0;

        public int InternalSelectedIndex
        {
            get => (int)_internalSelectedIndex;
            set => SetProperty(ref _internalSelectedIndex, value);
        }

        /// <summary>
        /// Set from user side (DisplayPageView)
        /// </summary>
        public int SelectedIndex
        {
            get
            {
                if (ItemCount == 3)
                {
                    if (_shownCount == 2)
                    {

                    }
                }
                if (ItemCount == 2)
                    if (_internalSelectedIndex == 2)
                    {
                        //_selectedIndex = 1;
                        return 1;
                    }
                return _internalSelectedIndex;
            }
            set
            {
                if (ItemCount == 2)
                {
                    if (value >= 1)
                    {
                        InternalSelectedIndex = 2;
                        return;
                    }
                }
                InternalSelectedIndex = value;
            }
        }

        #endregion Selection

    }
}